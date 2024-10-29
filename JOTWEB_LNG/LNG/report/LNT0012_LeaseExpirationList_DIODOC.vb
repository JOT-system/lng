Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient

''' <summary>
''' リース満了一覧表帳票作成クラス
''' </summary>
Public Class LNT0012_LeaseExpirationList_DIODOC

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable

    '行数
    Private Const HEADER_ROW_COUNT As Integer = 2
    Private Const DETAIL_ROW_COUNT As Integer = 58
    Private Const FOOTER_ROW_COUNT As Integer = 1

    '行高さ
    Private Const HEADER_ROW_HEIGHT As Double = 15.0
    Private Const DETAIL_ROW_HEIGHT As Double = 15.0
    Private Const FOOTER_ROW_HEIGHT As Double = 10.75

    '出力年月
    Private YearMonth As Date
    Private FormatType As Integer

    Private WW_Workbook As New Workbook
    Private WW_SheetNo As Integer = 0
    Private WW_tmpSheetNo As Integer = 0
    Private WW_InsDate As Date
    Private WW_CampCode As String = ""
    Private WW_KeyYMD As String = ""

    Private CS0050SESSION As New CS0050SESSION              'セッション情報操作処理

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    ''' <param name="mapId">帳票格納先のMAPID</param>
    ''' <param name="excelFileName">Excelファイル名（フルパスではない)</param>
    ''' <remarks>テンプレートファイルを読み取りモードとして開く</remarks>
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable, YearMonth As Date, FormatType As Integer)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.YearMonth = YearMonth
            Me.FormatType = FormatType
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
            WW_Workbook.Open(Me.ExcelTemplatePath)

            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If WW_Workbook.Worksheets(i).Name = "リース満了一覧表" Then
                    WW_SheetNo = i
                ElseIf WW_Workbook.Worksheets(i).Name = "temp" Then
                    WW_tmpSheetNo = i
                End If
            Next
        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' 帳票作成
    ''' 
    ''' ※帳票ヘッダ部への出力内容等があるなら引数として渡す
    ''' </summary>
    ''' <returns>ダウンロードURL</returns>
    Public Function CreateExcelPrintData() As String

        Dim ReportName As String = "リース満了一覧_"
        Dim tmpFileName As String = ReportName & "(" & Me.YearMonth.ToString("yyyy年MM月・") & If(Me.FormatType = 1, "請求部店", "計上部店") & ")_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try

            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim idxStr As String = ""
            Dim rowCnt As Integer = 1
            Dim pageBreakFlg As Boolean = False
            Dim lRow As DataRow = Nothing

            '明細出力
            For Each row As DataRow In Me.PrintData.Rows

                If rowCnt = 1 Then
                    '---------
                    ' 初回出力
                    '---------
                    'ヘッダー編集
                    EditHeader(row, rowCnt)
                    '明細編集
                    EditDetail(row, rowCnt, lRow)
                Else
                    '部店別出力のあとは部店名称表示(現在レコードが全支店計の場合はなし)
                    If pageBreakFlg AndAlso CInt(row("RECORDTYPE").ToString) <> 5 Then
                        EditPageBreak(row, rowCnt)
                        pageBreakFlg = False
                        lRow = Nothing
                    End If

                    Select Case CInt(row("RECORDTYPE").ToString)
                        Case 1
                            '明細編集
                            EditDetail(row, rowCnt, lRow)
                        Case 2
                            '小分類計編集
                            EditAggregated(row, rowCnt, 1)
                        Case 3
                            '請求先計編集
                            EditAggregated(row, rowCnt, 2)
                        Case 4
                            '部店計編集
                            EditAggregated(row, rowCnt, 3)
                            pageBreakFlg = True
                        Case 5
                            '全支店計編集
                            EditAggregated(row, rowCnt, 4)
                    End Select
                End If

                lRow = row
            Next

            '印刷範囲指定
            WW_Workbook.Worksheets(WW_SheetNo).PageSetup.PrintArea = "$A$1:$EB$" + (rowCnt - 1).ToString

            '出力シートのみ残す
            WW_Workbook.Worksheets(WW_tmpSheetNo).Delete() '雛形シート削除

            '保存処理実行
            Dim saveExcelLock As New Object
            SyncLock saveExcelLock '複数Excel起動で同時セーブすると落ちるので抑止
                WW_Workbook.Save(tmpFilePath, SaveFileFormat.Xlsx)
            End SyncLock

            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try

    End Function

    ''' <summary>
    ''' ヘッダー部編集
    ''' </summary>
    Private Sub EditHeader(ByVal row As DataRow, ByRef rowCnt As Integer)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            '行高さ
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = HEADER_ROW_HEIGHT

            '出力年月
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AL" + rowCnt.ToString)
            srcRange.Value = Me.YearMonth.ToString("yyyy年MM月")

            '出力フォーマット
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AX" + rowCnt.ToString)
            srcRange.Value = If(Me.FormatType = 1, "請求部店別", "収入計上部店別")

            rowCnt += 1

            '行高さ
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = HEADER_ROW_HEIGHT

            '出力対象タイトル
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString)
            srcRange.Value = If(Me.FormatType = 1, "請求部店", "収入計上部店")

            '出力対象名
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString)
            srcRange.Value = If(Me.FormatType = 1, row("BILLOUTPUTORGNAME"), row("POSTOFFICENAME"))

            rowCnt += 1

            '行高さ
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = HEADER_ROW_HEIGHT

            rowCnt += 1

            '行高さ
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = HEADER_ROW_HEIGHT

            'ヘッダー列１行目
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("DJ" + rowCnt.ToString)
            srcRange.Value = If(Me.FormatType = 1, "収入", "請求書")

            rowCnt += 1

            '行高さ
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = HEADER_ROW_HEIGHT

            'ヘッダー列２行目
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("DJ" + rowCnt.ToString)
            srcRange.Value = If(Me.FormatType = 1, "計上部店", "発行部店")

            rowCnt += 1

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 明細部編集
    ''' </summary>
    Private Sub EditDetail(ByVal row As DataRow,
                           ByRef rowCnt As Integer,
                           ByVal lRow As DataRow)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try

            '行テンプレートコピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B8:EA8")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString())
            srcRange.Copy(destRange)

            '行高さ
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = DETAIL_ROW_HEIGHT

            '請求先コード
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString())
            srcRange.Value = ""
            If IsNothing(lRow) OrElse
                Not lRow("INVOICECODE").ToString.Equals(row("INVOICECODE").ToString) OrElse
                rowCnt = 5 Then
                srcRange.Value = row("INVOICECODE")
            End If

            '請求先会社名
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
            srcRange.Value = ""
            If IsNothing(lRow) OrElse
                Not lRow("INVOICECODE").ToString.Equals(row("INVOICECODE").ToString) OrElse
                rowCnt = 5 Then
                srcRange.Value = row("INVOICECOMNAME")
            End If

            '請求先部門名
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("Y" + rowCnt.ToString())
            srcRange.Value = ""
            If IsNothing(lRow) OrElse
                Not lRow("INVOICECODE").ToString.Equals(row("INVOICECODE").ToString) OrElse
                rowCnt = 5 Then
                srcRange.Value = row("INVOICEDIVNAME")
            End If

            'コンテナ種別名
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AO" + rowCnt.ToString())
            srcRange.Value = row("BIGCTNNAME")

            '契約種別
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AY" + rowCnt.ToString())
            srcRange.Value = row("CONTRACTNAME")

            '記号
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BI" + rowCnt.ToString())
            srcRange.Value = row("KIGOU")

            '番号
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BN" + rowCnt.ToString())
            srcRange.Value = row("BANGOU")

            '屯数
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BS" + rowCnt.ToString())
            srcRange.Value = Decimal.Parse(row("MARKTON").ToString)

            '開始日
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BX" + rowCnt.ToString())
            srcRange.Value = CDate(row("STYMD").ToString)

            '終了日
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("CB" + rowCnt.ToString())
            srcRange.Value = CDate(row("ENDYMD").ToString)

            '日数
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("CF" + rowCnt.ToString())
            srcRange.Value = row("DAYCOUNT")

            '単価
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("CI" + rowCnt.ToString())
            srcRange.Value = row("UNITPRICE")

            '金額
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("CO" + rowCnt.ToString())
            srcRange.Value = row("LEASEAMOUNT")

            '税率
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("CX" + rowCnt.ToString())
            srcRange.Value = row("TAXTYPENAME")

            '税額
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("DA" + rowCnt.ToString())
            srcRange.Value = row("TAXPRICE")

            '請求書発行部店 OR 収入計上部店
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("DJ" + rowCnt.ToString())
            srcRange.Value = If(Me.FormatType = 1, row("POSTOFFICENAME"), row("BILLOUTPUTORGNAME"))

            '契約状態
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("DP" + rowCnt.ToString())
            If row("EXPIRATIONTYPE").ToString = "1" Then
                srcRange.Value = "満了　(契約終了)"
            ElseIf row("EXPIRATIONTYPE").ToString = "2" Then
                srcRange.Value = "途中解約"
            ElseIf row("EXPIRATIONTYPE").ToString = "3" Then
                srcRange.Value = "満了　(自動更新予定)"
            End If

            '行番号加算
            rowCnt += 1

        Catch ex As Exception
            Throw
        Finally

        End Try

    End Sub


    ''' <summary>
    ''' 集計部編集
    ''' </summary>
    Private Sub EditAggregated(ByVal row As DataRow,
                               ByRef rowCnt As Integer,
                               ByVal type As Integer)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim whereStr As String = ""

        Try
            '行テンプレートコピー
            If type = 1 Then srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B10:EA11")
            If type = 2 Then srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B13:EA14")
            If type = 3 Then srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B16:EA17")
            If type = 4 Then srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B19:EA20")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString())
            srcRange.Copy(destRange)

            '行高さ
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = DETAIL_ROW_HEIGHT
            '行高さ
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt + 1))
            srcRange.RowHeight = DETAIL_ROW_HEIGHT

            '項目編集＆集計条件設定
            If type = 1 Then
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString() + ":" +
                                                                    "AN" + (rowCnt + 1).ToString())
                '値をクリア
                srcRange.ClearContents()
                'セルを結合
                srcRange.Merge()
                'コンテナ種別名
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AO" + rowCnt.ToString())
                srcRange.Value = row("BIGCTNNAME").ToString & "計"
            ElseIf type = 2 Then
                '請求先コード
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString())
                srcRange.Value = row("INVOICECODE")
                '請求先会社名
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                srcRange.Value = row("INVOICECOMNAME")
                '請求先会社部門名
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("Y" + rowCnt.ToString())
                srcRange.Value = row("INVOICEDIVNAME")
            ElseIf type = 3 Then
                '集計部店
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString())
                srcRange.Value = If(Me.FormatType = 1, "請求部店名", " 収入計上部店名")
                '部店名
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + (rowCnt + 1).ToString())
                If Me.FormatType = 1 Then
                    srcRange.Value = row("BILLOUTPUTORGCODE").ToString & " " & row("BILLOUTPUTORGNAME").ToString
                Else
                    srcRange.Value = row("POSTOFFICECODE").ToString & " " & row("POSTOFFICENAME").ToString
                End If
            End If

            '件数
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BN" + rowCnt.ToString())
            srcRange.Value = String.Format("{0:#,0} 件", row("CONTAINERCOUNT"))

            '屯数
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BS" + rowCnt.ToString())
            srcRange.Value = row("MARKTON")

            '金額
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("CO" + rowCnt.ToString())
            srcRange.Value = row("LEASEAMOUNT")

            '消費税額
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("DA" + rowCnt.ToString())
            srcRange.Value = row("TAXPRICE")

            '行番号加算
            rowCnt += 2


        Catch ex As Exception
            Throw
        Finally

        End Try

    End Sub

    ''' <summary>
    ''' 改頁編集
    ''' </summary>
    Private Sub EditPageBreak(ByVal nRow As DataRow, ByRef rowCnt As Integer)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        'フッター行高さ設定
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
        srcRange.RowHeight = FOOTER_ROW_HEIGHT
        rowCnt += 1
        'ヘッダー行テンプレートコピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("B22:EA25")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString())
        srcRange.Copy(destRange)

        Try
            '行高さ
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = HEADER_ROW_HEIGHT

            '出力対象タイトル
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString)
            srcRange.Value = If(Me.FormatType = 1, "請求部店", "収入計上部店")

            '出力対象名
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString)
            srcRange.Value = If(Me.FormatType = 1, nRow("BILLOUTPUTORGNAME"), nRow("POSTOFFICENAME"))

            rowCnt += 1

            '行高さ
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = HEADER_ROW_HEIGHT

            rowCnt += 1

            '行高さ
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = HEADER_ROW_HEIGHT

            'ヘッダー列１行目
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("DJ" + rowCnt.ToString)
            srcRange.Value = If(Me.FormatType = 1, "収入", "請求書")

            rowCnt += 1

            '行高さ
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = HEADER_ROW_HEIGHT

            'ヘッダー列２行目
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("DJ" + rowCnt.ToString)
            srcRange.Value = If(Me.FormatType = 1, "計上部店", "発行部店")

            rowCnt += 1

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

End Class
