Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient

''' <summary>
''' レンタルコンテナ回送費明細(コンテナ別)帳票作成クラス
''' </summary>
Public Class LNT0012_RentalCTNListReport_DIODOC

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable

    '行高さ
    Private Const HEADER_ROW_HEIGHT As Double = 30.0
    Private Const DETAIL_HEADER_ROW_HEIGHT1 As Double = 23.0
    Private Const DETAIL_HEADER_ROW_HEIGHT2 As Double = 20.25
    Private Const DETAIL_ROW_HEIGHT As Double = 19.5
    Private Const DETAIL_ROW_HEIGHT2 As Double = 30.5
    Private Const FOOTER_ROW_HEIGHT As Double = 10.75

    '出力年月
    Private STYMD As Date
    Private ENDYMD As Date

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
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable, STYMD As Date, ENDYMD As Date)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
            Me.STYMD = STYMD
            Me.ENDYMD = ENDYMD
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
                If WW_Workbook.Worksheets(i).Name = "レンタルコンテナ回送費明細(コンテナ別)" Then
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
        Dim ReportName As String = "レンタルコンテナ回送費明細(コンテナ別)_"
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

                    '部店ヘッダー編集
                    EditORGHeader(row, rowCnt, pageRowCnt)

                    '明細ヘッダー編集
                    EditDetailHeader(row, rowCnt, pageRowCnt)
                    '明細編集
                    EditDetail(row, rowCnt, pageRowCnt, lRow)
                Else
                    If row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "1" Then
                        If row("PAYFILINGBRANCH").ToString <> lRow("PAYFILINGBRANCH").ToString Then
                            '部店ヘッダー編集
                            EditORGHeader(row, rowCnt, pageRowCnt)
                            '明細ヘッダー編集
                            EditDetailHeader(row, rowCnt, pageRowCnt)
                        ElseIf row("TORICODE").ToString <> lRow("TORICODE").ToString Then
                            '明細ヘッダー編集
                            EditDetailHeader(row, rowCnt, pageRowCnt)
                        End If
                        '明細編集
                        EditDetail(row, rowCnt, pageRowCnt, lRow)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "2" Then
                        '種別計編集
                        EditAggregated(row, rowCnt, "1", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "3" Then
                        '着駅計編集
                        EditAggregated(row, rowCnt, "2", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "4" Then
                        '種別計（修繕）編集
                        EditAggregated(row, rowCnt, "5", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "5" Then
                        '着駅計（修繕）編集
                        EditAggregated(row, rowCnt, "6", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "6" Then
                        '種別計（除却）編集
                        EditAggregated(row, rowCnt, "7", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "7" Then
                        '着駅計（除却）編集
                        EditAggregated(row, rowCnt, "8", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "8" Then
                        '種別計（売却）編集
                        EditAggregated(row, rowCnt, "9", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "9" Then
                        '着駅計（売却）編集
                        EditAggregated(row, rowCnt, "15", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "15" Then
                        '種別計（その他）編集
                        EditAggregated(row, rowCnt, "15", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "16" Then
                        '着駅計（その他）編集
                        EditAggregated(row, rowCnt, "16", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "10" Then
                        '発駅別空回計編集
                        EditAggregated(row, rowCnt, "11", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "11" Then
                        '発駅別空（修繕）計編集
                        EditAggregated(row, rowCnt, "12", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "12" Then
                        '発駅別空（除却）計編集
                        EditAggregated(row, rowCnt, "13", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "13" Then
                        '発駅別空（売却）計編集
                        EditAggregated(row, rowCnt, "14", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "17" Then
                        '発駅別空（その他）計編集
                        EditAggregated(row, rowCnt, "17", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "1" AndAlso row("RECORDTYPE").ToString = "14" Then
                        '発駅計編集
                        EditAggregated(row, rowCnt, "3", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "2" AndAlso row("RECORDTYPE").ToString = "1" Then
                        If row("PAYFILINGBRANCH").ToString <> lRow("PAYFILINGBRANCH").ToString Then
                            '部店ヘッダー編集
                            EditORGHeader(row, rowCnt, pageRowCnt)
                            '明細ヘッダー編集
                            EditDetailHeader(row, rowCnt, pageRowCnt)
                        ElseIf row("TORICODE").ToString <> lRow("TORICODE").ToString Then
                            '明細ヘッダー編集
                            EditDetailHeader(row, rowCnt, pageRowCnt)
                        End If
                        '明細編集
                        EditDetail(row, rowCnt, pageRowCnt, lRow)
                    ElseIf row("RECORDCLASS").ToString = "2" AndAlso row("RECORDTYPE").ToString = "2" Then
                        '加減額計編集
                        EditAggregated(row, rowCnt, "4", pageRowCnt)
                    ElseIf row("RECORDCLASS").ToString = "3" AndAlso row("RECORDTYPE").ToString = "1" Then
                        '支店別空回計編集
                        EditTotalBranches(row, rowCnt, pageRowCnt, lRow, "1")
                    ElseIf row("RECORDCLASS").ToString = "3" AndAlso row("RECORDTYPE").ToString = "2" Then
                        '支店別（修繕）計編集
                        EditTotalBranches(row, rowCnt, pageRowCnt, lRow, "2")
                    ElseIf row("RECORDCLASS").ToString = "3" AndAlso row("RECORDTYPE").ToString = "3" Then
                        '支店別（除却）計編集
                        EditTotalBranches(row, rowCnt, pageRowCnt, lRow, "3")
                    ElseIf row("RECORDCLASS").ToString = "3" AndAlso row("RECORDTYPE").ToString = "4" Then
                        '支店別（売却）計編集
                        EditTotalBranches(row, rowCnt, pageRowCnt, lRow, "4")
                    ElseIf row("RECORDCLASS").ToString = "3" AndAlso row("RECORDTYPE").ToString = "5" Then
                        '支店別（その他）計編集
                        EditTotalBranches(row, rowCnt, pageRowCnt, lRow, "5")
                    ElseIf row("RECORDCLASS").ToString = "3" AndAlso row("RECORDTYPE").ToString = "6" Then
                        '支店計編集
                        EditTotalBranches(row, rowCnt, pageRowCnt, lRow, "6")


                    End If
                End If

                lRow = row
            Next

            '印刷範囲指定
            WW_Workbook.Worksheets(WW_SheetNo).PageSetup.PrintArea = "$A$1:$P$" + (rowCnt - 3).ToString

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

            'ヘッダー行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A1:P1")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)

            'ヘッダー日付設定
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
            srcRange.Value = Me.STYMD.ToString("yyyy年M月分") + "（" + Me.STYMD.ToString("d日～") + Me.ENDYMD.ToString("d日）")

            'ヘッダー日付設定
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
            srcRange.Value = Date.Now.ToString("yyyy/M/d hh:mm")

            rowCnt += 1
            pageRowCnt = 1

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' ヘッダー部編集
    ''' </summary>
    Private Sub EditORGHeader(ByVal row As DataRow, ByRef rowCnt As Integer, ByRef pageRowCnt As Integer)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            'ヘッダー行高さ設定
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = HEADER_ROW_HEIGHT

            '明細ヘッダー部コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A3:P3")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)

            '請求支店
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("P" + rowCnt.ToString())
            srcRange.Value = row("PAYFILINGNAME").ToString

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
            'ヘッダー行高さ設定
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = DETAIL_HEADER_ROW_HEIGHT1

            '明細ヘッダー部コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A5:P8")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)

            '支払先
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString())
            srcRange.Value = row("TORINAME")

            '銀行
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
            srcRange.Value = row("BANKCODE")
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
            srcRange.Value = row("BANKNAME")

            '種別
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
            srcRange.Value = row("ACCOUNTTYPENM")

            'インボイス番号
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
            srcRange.Value = row("INVOICENO")

            '改行
            rowCnt += 1

            '支払先部店
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString())
            srcRange.Value = row("TORIDIVNAME")

            '支店
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
            srcRange.Value = row("BANKBRANCHCODE")
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
            srcRange.Value = row("BANKBRANCHNAME")

            '口座番号
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
            srcRange.Value = row("ACCOUNTNUMBER")

            '名義
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
            srcRange.Value = row("ACCOUNTNAME")

            rowCnt += 2

            'ヘッダー行高さ設定
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = DETAIL_HEADER_ROW_HEIGHT2

            rowCnt += 1

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub


    ''' <summary>
    ''' 明細部編集
    ''' </summary>
    Private Sub EditDetail(ByVal row As DataRow, ByRef rowCnt As Integer, ByRef pageRowCnt As Integer, ByVal lrow As DataRow)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            'ヘッダー行高さ設定
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            srcRange.RowHeight = DETAIL_ROW_HEIGHT

            '明細部コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A10:P10")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)

            '発駅
            If IsNothing(lrow) OrElse row("DEPSTATIONNM").ToString <> lrow("DEPSTATIONNM").ToString Then
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                srcRange.Value = row("DEPSTATIONNM")
            End If

            '着駅
            If IsNothing(lrow) OrElse row("ARRSTATIONNM").ToString <> lrow("ARRSTATIONNM").ToString Then
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("B" + rowCnt.ToString())
                srcRange.Value = row("ARRSTATIONNM")
            End If

            '種別
            If IsNothing(lrow) OrElse row("BIGCTNNM").ToString <> lrow("BIGCTNNM").ToString OrElse row("ACCOUNTSTATUSKBNNM").ToString <> lrow("ACCOUNTSTATUSKBNNM").ToString Then
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("C" + rowCnt.ToString())
                srcRange.Value = row("BIGCTNNM")
            End If

            '回送
            If IsNothing(lrow) OrElse row("ACCOUNTSTATUSKBNNM").ToString <> lrow("ACCOUNTSTATUSKBNNM").ToString Then
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("D" + rowCnt.ToString())
                If row("ACCOUNTSTATUSKBN").ToString <> "4" AndAlso
                   row("ACCOUNTSTATUSKBN").ToString <> "5" AndAlso
                   row("ACCOUNTSTATUSKBN").ToString <> "9" AndAlso
                   row("ACCOUNTSTATUSKBN").ToString <> "98" Then
                    srcRange.Value = "空回"
                Else
                    srcRange.Value = row("ACCOUNTSTATUSKBNNM")
                End If
            End If

            '発送年月
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("E" + rowCnt.ToString())
            srcRange.Value = Left(row("SHIPYMD").ToString, 10)

            'コンテナ番号
            If row("CTNNUMBER").ToString <> "" Then
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("F" + rowCnt.ToString())
                srcRange.Value = row("CTNNUMBER")
            End If

            '所定運賃
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
            srcRange.Value = row("TOTALFIXEDFARE")

            '割引
            If row("OWNDISCOUNTFEE").ToString <> "0" Then
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
            End If

            '適用運賃
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
            srcRange.Value = row("APPLICABLEFARE")

            '発送料
            If row("SHIPFEE").ToString <> "0" Then
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                srcRange.Value = row("SHIPFEE")
            End If

            '手数料
            If row("COMMISSIONFEE").ToString <> "0" Then
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                srcRange.Value = row("COMMISSIONFEE")
            End If

            '適用運賃
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
            srcRange.Value = row("OTHER1FEE")

            '小計
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
            srcRange.Value = row("TOTALPAYMENT")

            rowCnt += 1


        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 明細部編集
    ''' </summary>
    Private Sub EditAggregated(ByVal row As DataRow, ByRef rowCnt As Integer, ByVal type As String, ByRef pageRowCnt As Integer)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            Select Case type
                Case "1"
                    '種別計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A12:P12")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "2"
                    '着駅計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A14:P14")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "3"
                    '発駅計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A16:P16")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "4"
                    '加減額計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A18:P18")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "5"
                    '種別計（修繕）
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A30:P30")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "6"
                    '着駅（修繕）計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A32:P32")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "7"
                    '種別計（除却）
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A34:P34")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "8"
                    '着駅（除却）計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A36:P36")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "9"
                    '種別計（売却）
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A38:P38")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "10"
                    '着駅（売却）計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A40:P40")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "11"
                    '発駅別空回総計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A42:P42")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "12"
                    '発駅計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A44:P44")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "13"
                    '発駅計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A46:P46")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "14"
                    '発駅計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A48:P48")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "15"
                    '種別計（その他）
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A50:P50")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "16"
                    '着駅（その他）計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A52:P52")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                Case "17"
                    '発駅計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A54:P54")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

            End Select

            rowCnt += 1

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 支払先合計編集
    ''' </summary>
    Private Sub EditTotalBranches(ByVal row As DataRow, ByRef rowCnt As Integer, ByRef pageRowCnt As Integer, ByVal lrow As DataRow, ByVal type As String)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            Select Case type
                Case "1"
                    '支払先別空回計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A22:P22")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                    '消費税
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("O" + rowCnt.ToString())
                    srcRange.Value = row("CONSUMPTIONTAX")

                    '合計(税込)
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("P" + rowCnt.ToString())
                    srcRange.Value = row("TOTALAMOUNT")

                    rowCnt += 1

                Case "2"
                    '支払先別空（修繕）計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A24:P24")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                    '消費税
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("O" + rowCnt.ToString())
                    srcRange.Value = row("CONSUMPTIONTAX")

                    '合計(税込)
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("P" + rowCnt.ToString())
                    srcRange.Value = row("TOTALAMOUNT")

                    rowCnt += 1

                Case "3"
                    '支払先別空（除却）計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A26:P26")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                    '消費税
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("O" + rowCnt.ToString())
                    srcRange.Value = row("CONSUMPTIONTAX")

                    '合計(税込)
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("P" + rowCnt.ToString())
                    srcRange.Value = row("TOTALAMOUNT")

                    rowCnt += 1

                Case "4"
                    '支払先別空（売却）計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A28:P28")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                    '消費税
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("O" + rowCnt.ToString())
                    srcRange.Value = row("CONSUMPTIONTAX")

                    '合計(税込)
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("P" + rowCnt.ToString())
                    srcRange.Value = row("TOTALAMOUNT")

                    rowCnt += 1

                Case "6"
                    '支払先計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A20:P20")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                    '消費税
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("O" + rowCnt.ToString())
                    srcRange.Value = row("CONSUMPTIONTAX")

                    '合計(税込)
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("P" + rowCnt.ToString())
                    srcRange.Value = row("TOTALAMOUNT")

                    rowCnt += 3

                Case "5"
                    '支払先別空(その他)計
                    'ヘッダー行高さ設定
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
                    srcRange.RowHeight = DETAIL_ROW_HEIGHT2

                    '明細部コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A56:P56")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                    srcRange.Copy(destRange)

                    '個数
                    If row("NUMBER").ToString <> "" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("G" + rowCnt.ToString())
                        srcRange.Value = row("NUMBER").ToString & "個"
                    End If

                    '所定運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("H" + rowCnt.ToString())
                    srcRange.Value = row("TOTALFIXEDFARE")

                    '割引
                    If row("OWNDISCOUNTFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
                        srcRange.Value = CInt(row("OWNDISCOUNTFEE")) - (CInt(row("OWNDISCOUNTFEE").ToString) * 2)
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("J" + rowCnt.ToString())
                    srcRange.Value = row("APPLICABLEFARE")

                    '発送料
                    If row("SHIPFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("K" + rowCnt.ToString())
                        srcRange.Value = row("SHIPFEE")
                    End If

                    '手数料
                    If row("COMMISSIONFEE").ToString <> "0" Then
                        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("L" + rowCnt.ToString())
                        srcRange.Value = row("COMMISSIONFEE")
                    End If

                    '適用運賃
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("M" + rowCnt.ToString())
                    srcRange.Value = row("OTHER1FEE")

                    '小計
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("N" + rowCnt.ToString())
                    srcRange.Value = row("TOTALPAYMENT")

                    '消費税
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("O" + rowCnt.ToString())
                    srcRange.Value = row("CONSUMPTIONTAX")

                    '合計(税込)
                    srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("P" + rowCnt.ToString())
                    srcRange.Value = row("TOTALAMOUNT")

                    rowCnt += 1

            End Select

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub


End Class
