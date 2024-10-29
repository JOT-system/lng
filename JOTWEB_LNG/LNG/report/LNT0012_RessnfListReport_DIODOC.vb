Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' コンテナ回送費明細(発駅・受託人別)帳票作成クラス
''' </summary>

Public Class LNT0012_RessnfListReport_DIODOC

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable

    '行高さ
    Private Const HEADER_ROW_HEIGHT As Double = 30.0
    Private Const DETAIL_HEADER_ROW_HEIGHT1 As Double = 18.75
    Private Const DETAIL_HEADER_ROW_HEIGHT2 As Double = 43.75
    Private Const DETAIL_ROW_HEIGHT As Double = 23.75
    Private Const FOOTER_ROW_HEIGHT As Double = 10.75

    '出力年月
    Private YearMonth As Date

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
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable, yearMonth As Date)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.YearMonth = yearMonth
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
                If WW_Workbook.Worksheets(i).Name = "コンテナ回送費明細(発駅・受託人別)" Then
                    WW_SheetNo = i
                ElseIf WW_Workbook.Worksheets(i).Name = "temp" Then
                    WW_tmpSheetNo = i
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
        Dim ReportName As String = "コンテナ回送費明細(発駅・受託人別)_"
        Dim tmpFileName As String = ReportName & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        Try
            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim idxStr As String = ""
            Dim rowCnt As Integer = 1
            Dim pageRowCnt As Integer = 0
            Dim pageBreakFlg As Boolean = False
            Dim lRow As DataRow = Nothing

            '明細出力
            For Each row As DataRow In Me.PrintData.Rows

                If rowCnt = 1 Then
                    '---------
                    ' 初回出力
                    '---------
                    'ヘッダー編集
                    EditHeader(row, rowCnt, pageRowCnt)
                    '明細ヘッダー編集
                    EditDetailHeader(row, rowCnt, pageRowCnt)
                    '明細編集
                    EditDetail(row, rowCnt, pageRowCnt, lRow)
                Else
                    If row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "1" Then
                        If row("PAYFILINGNAME").ToString <> lRow("PAYFILINGNAME").ToString Then
                            '明細ヘッダー編集
                            EditDetailHeader(row, rowCnt, pageRowCnt)
                        End If
                        '明細編集
                        EditDetail(row, rowCnt, pageRowCnt, lRow)
                        ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "2" Then
                            '種別計編集
                            EditAggregated(row, rowCnt, 1, pageRowCnt)
                        ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "3" Then
                            '発受託人計編集
                            EditAggregated(row, rowCnt, 2, pageRowCnt)
                        ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "4" Then
                            '発駅計編集
                            EditAggregated(row, rowCnt, 3, pageRowCnt)
                        ElseIf row("RECORDCLASS").ToString = "2" Then
                            '支店計編集
                            EditTotalBranches(row, rowCnt, pageRowCnt, lRow)

                    End If
                End If

                lRow = row
            Next

            '印刷範囲指定
            WW_Workbook.Worksheets(WW_SheetNo).PageSetup.PrintArea = "$A$1:$M$" + (rowCnt - 1).ToString

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
        Finally
        End Try

    End Function

    ''' <summary>
    ''' ヘッダー部編集
    ''' </summary>
    Private Sub EditHeader(ByVal row As DataRow, ByRef rowCnt As Integer, ByRef pageRowCnt As Integer)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            'ヘッダー行高さ設定
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = HEADER_ROW_HEIGHT

            '出力年月
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A1:A1")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)
            destRange.Value = Me.YearMonth.ToString("yyyy年M月") + "実績"

            'タイトル
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("C1:C1")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("C" + rowCnt.ToString())
            srcRange.Copy(destRange)

            rowCnt += 1
            pageRowCnt = 1

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 明細ヘッダー部編集
    ''' </summary>
    Private Sub EditDetailHeader(ByVal row As DataRow, ByRef rowCnt As Integer, ByRef pageRowCnt As Integer)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            '明細ヘッダー行高さ設定(対象支店)
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = DETAIL_HEADER_ROW_HEIGHT1

            '対象支店
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A2:A2")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)
            destRange.Value = "【" + row("PAYFILINGNAME").ToString + "】"

            rowCnt += 1

            '明細ヘッダー行高さ設定(明細ヘッダータイトル)
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = DETAIL_HEADER_ROW_HEIGHT2

            '明細ヘッダータイトルコピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A3:M3")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)

            rowCnt += 1
            pageRowCnt += 2


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
                           ByRef pageRowCnt As Integer,
                           ByVal lRow As DataRow)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            '明細行高さ設定
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = DETAIL_ROW_HEIGHT

            '明細行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A5:M5")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)

            '発駅
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Value = ""
            If IsNothing(lRow) OrElse
                Not lRow("DEPSTATIONCD").ToString.Equals(row("DEPSTATIONCD").ToString) Then
                srcRange.Value = row("DEPSTATIONNM")
            End If

            '発受託人
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString())
            srcRange.Value = ""
            If IsNothing(lRow) OrElse
                Not lRow("DEPTRUSTEECD").ToString.Equals(row("DEPTRUSTEECD").ToString) Then
                srcRange.Value = row("DEPTRUSTEENM")
            End If

            '種別
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("C" + rowCnt.ToString())
            srcRange.Value = row("BIGCTNNM")

            '着駅
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("D" + rowCnt.ToString())
            srcRange.Value = row("ARRSTATIONNM")

            '着受託人
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("E" + rowCnt.ToString())
            srcRange.Value = row("ARRTRUSTEENM")

            '運賃単価
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("F" + rowCnt.ToString())
            srcRange.Value = row("JRFIXEDFARE")

            '個数
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
            srcRange.Value = row("NUMBER")

            '所定運賃
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
            srcRange.Value = row("TOTALFIXEDFARE")

            '協定
            If row("TOTALFIXEDFARE").ToString <> row("FREESENDFEE").ToString Then
                If row("ARRSTATION").ToString = "999999" OrElse row("NUMBER").ToString = "0" Then
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                    srcRange.Value = ""
                Else
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                    srcRange.Value = "★"
                End If
            End If

            '私有割引
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
            srcRange.Value = row("OWNDISCOUNTFEE")

            '支払運賃
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
            srcRange.Value = row("FREESENDFEE")

            '発送料
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
            srcRange.Value = row("SHIPFEE")

            '支払合計
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
            srcRange.Value = row("TOTALPAYMENT")

            '行番号加算
            rowCnt += 1
            pageRowCnt += 1

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
                               ByVal type As Integer,
                               ByRef pageRowCnt As Integer)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            '明細行高さ設定
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = DETAIL_ROW_HEIGHT

            '行テンプレートコピー
            '種別計
            If type = 1 Then srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A7:M7")
            '発受託人計
            If type = 2 Then srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A9:M9")
            '発駅計
            If type = 3 Then srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A11:M11")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)

            '種別計コード
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("E" + rowCnt.ToString())
            srcRange.Value = row("ARRTRUSTEENM")

            '個数
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
            srcRange.Value = row("NUMBER")

            '所定運賃
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
            srcRange.Value = row("TOTALFIXEDFARE")

            '協定
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
            srcRange.Value = row("PACT")

            '私有割引
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
            srcRange.Value = row("OWNDISCOUNTFEE")

            '支払運賃
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
            srcRange.Value = row("FREESENDFEE")

            '発送料
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
            srcRange.Value = row("SHIPFEE")

            '支払合計
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
            srcRange.Value = row("TOTALPAYMENT")

            '行番号加算
            rowCnt += 1
            pageRowCnt += 1

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 支店集計部編集
    ''' </summary>
    Private Sub EditTotalBranches(ByVal row As DataRow,
                               ByRef rowCnt As Integer,
                               ByRef pageRowCnt As Integer,
                               ByVal lRow As DataRow)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            '明細行高さ設定
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = DETAIL_ROW_HEIGHT

            '明細行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A13:M13")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)

            '発駅
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Value = ""
            If IsNothing(lRow) OrElse
                Not lRow("PAYFILINGNAME").ToString.Equals(row("PAYFILINGNAME").ToString) Then
                srcRange.Value = row("PAYFILINGNAME")
            End If

            '種別
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("C" + rowCnt.ToString())
            srcRange.Value = ""
            If IsNothing(lRow) OrElse
                Not lRow("BIGCTNCD").ToString.Equals(row("BIGCTNCD").ToString) Then
                srcRange.Value = row("BIGCTNNM")
            End If

            '管内、管外、計
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("D" + rowCnt.ToString())
            srcRange.Value = row("ARRSTATIONNM")

            '運賃単価
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("F" + rowCnt.ToString())
            srcRange.Value = row("JRFIXEDFARE")

            '個数
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
            srcRange.Value = row("NUMBER")

            '所定運賃
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
            srcRange.Value = row("TOTALFIXEDFARE")

            '協定
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
            srcRange.Value = row("PACT")

            '私有割引
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
            srcRange.Value = row("OWNDISCOUNTFEE")

            '支払運賃
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
            srcRange.Value = row("FREESENDFEE")

            '発送料
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
            srcRange.Value = row("SHIPFEE")

            '支払合計
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
            srcRange.Value = row("TOTALPAYMENT")

            '行番号加算
            rowCnt += 1
            pageRowCnt += 1

        Catch ex As Exception
            Throw
        Finally
        End Try


    End Sub



End Class
