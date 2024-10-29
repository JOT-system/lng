Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 請求先・勘定科目別・計上店別営業収入計上一覧帳票作成クラス
''' </summary>

Public Class LNT0012_BILLINGACCOUNTBYDEPARTMENTLIST_DIODOC

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable

    '行高さ
    Private Const COMMON_ROW_HEIGHT As Double = 27.5
    Private Const FINANCE_SUM_ROW_HEIGHT As Double = 5
    Private Const FOOTER_ROW_HEIGHT As Double = 16.5

    '引数
    Private Year As String
    Private Type As Integer
    Private STYMD As String
    Private ENDYMD As String

    '定数
    Private CONST_BILLING_ACCOUNT_LIST As String = "請求先・勘定科目・計上部店別営業収入計上一覧表"
    Private CONST_INVOICECODE As String = "ZZZZZZZZZZ"

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
    Public Sub New(ByVal mapId As String,
                   ByVal excelFileName As String,
                   ByVal printDataClass As DataTable,
                   ByVal fiscalyear As String,
                   ByVal reportType As Integer,
                   ByVal yearMonthFrom As String,
                   ByVal yearMonthTo As String)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            '抽出データのフォーマットを実際のExcelの出力状態に変換
            Me.PrintData = OutPutExcelFormat(printDataClass)
            Me.Year = fiscalyear
            Me.Type = reportType
            Me.STYMD = yearMonthFrom
            Me.ENDYMD = yearMonthTo
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
                If WW_Workbook.Worksheets(i).Name = "請求先・勘定科目・計上店別営業収入計上一覧" Then
                    WW_SheetNo = i
                ElseIf WW_Workbook.Worksheets(i).Name = "temp" Then
                    WW_tmpSheetNo = i
                End If
            Next
        Catch ex As Exception

        End Try

    End Sub


    ''' <summary>
    ''' 抽出データのフォーマットを実際のExcelの出力状態に変換
    ''' </summary>
    ''' <returns>Excelフォーマットの出力データ</returns>
    Public Function OutPutExcelFormat(ByVal dt As DataTable) As DataTable

        Dim ExcelFormat As DataTable = New DataTable
        Dim ExcelFormat2 As DataTable = New DataTable
        Dim SelectRow As DataRow()
        Dim CreateRow As DataRow

        'テーブルのフィールド名を設定
        ExcelFormat.Columns.Add("請求先コード")
        ExcelFormat.Columns.Add("請求先名")
        ExcelFormat.Columns.Add("勘定科目コード")
        ExcelFormat.Columns.Add("勘定科目名")
        ExcelFormat.Columns.Add("セグメントコード")
        ExcelFormat.Columns.Add("セグメント名")
        ExcelFormat.Columns.Add("組織コード")
        ExcelFormat.Columns.Add("組織名")
        ExcelFormat.Columns.Add("レコードタイプ")    '1.リース、元請輸送、追加明細,2.ファイナンスリース（自動生成）
        ExcelFormat.Columns.Add("レコード集計")      '1.明細行,2.合計行
        ExcelFormat.Columns.Add("コンテナ部", GetType(Integer))
        ExcelFormat.Columns.Add("北海道支店", GetType(Integer))
        ExcelFormat.Columns.Add("東北支店", GetType(Integer))
        ExcelFormat.Columns.Add("関東支店", GetType(Integer))
        ExcelFormat.Columns.Add("中部支店", GetType(Integer))
        ExcelFormat.Columns.Add("関西支店", GetType(Integer))
        ExcelFormat.Columns.Add("九州支店", GetType(Integer))
        ExcelFormat.Columns.Add("経理部", GetType(Integer))
        ExcelFormat.Columns.Add("本社", GetType(Integer))
        ExcelFormat.Columns.Add("合計", GetType(Integer))

        'Excel出力フォーマットにデータを設定
        For Each row As DataRow In dt.Rows
            SelectRow = ExcelFormat.Select("請求先コード = '" & row("INVOICECODE").ToString & "' AND 勘定科目コード = '" & row("ACCOUNTCODE").ToString & "' AND セグメントコード = '" & row("SEGMENTCODE").ToString & "' AND レコードタイプ = '" & row("RECORDTYPE").ToString & "' AND レコード集計 = '" & row("RECORDCOUNT").ToString & "'")
            If SelectRow.Length = 0 Then
                'データが存在しない場合
                CreateRow = ExcelFormat.NewRow
                CreateRow("請求先コード") = row("INVOICECODE")
                CreateRow("請求先名") = row("INVOICENAME")
                CreateRow("勘定科目コード") = row("ACCOUNTCODE")
                CreateRow("勘定科目名") = row("ACCOUNTNAME")
                CreateRow("セグメントコード") = row("SEGMENTCODE")
                CreateRow("セグメント名") = row("SEGMENTNAMERYAKU")
                CreateRow("組織コード") = row("ORGCODE")
                CreateRow("組織名") = row("ORGNAME")
                CreateRow("レコードタイプ") = row("RECORDTYPE")
                CreateRow("レコード集計") = row("RECORDCOUNT")
                'コンテナ部
                If row("ORGCODE").ToString = "011312" Then
                    CreateRow("コンテナ部") = row("AMOUNT")
                    CreateRow("合計") = row("AMOUNT")
                Else
                    CreateRow("コンテナ部") = 0
                End If
                '北海道支店
                If row("ORGCODE").ToString = "010102" Then
                    CreateRow("北海道支店") = row("AMOUNT")
                    CreateRow("合計") = row("AMOUNT")
                Else
                    CreateRow("北海道支店") = 0
                End If
                '東北支店
                If row("ORGCODE").ToString = "010401" Then
                    CreateRow("東北支店") = row("AMOUNT")
                    CreateRow("合計") = row("AMOUNT")
                Else
                    CreateRow("東北支店") = 0
                End If
                '関東支店と新潟支店
                If row("ORGCODE").ToString = "011402" OrElse row("ORGCODE").ToString = "011501" Then
                    CreateRow("関東支店") = row("AMOUNT")
                    CreateRow("合計") = row("AMOUNT")
                Else
                    CreateRow("関東支店") = 0
                End If
                '中部支店
                If row("ORGCODE").ToString = "012401" Then
                    CreateRow("中部支店") = row("AMOUNT")
                    CreateRow("合計") = row("AMOUNT")
                Else
                    CreateRow("中部支店") = 0
                End If
                '関西支店
                If row("ORGCODE").ToString = "012701" Then
                    CreateRow("関西支店") = row("AMOUNT")
                    CreateRow("合計") = row("AMOUNT")
                Else
                    CreateRow("関西支店") = 0
                End If
                '九州支店
                If row("ORGCODE").ToString = "014001" Then
                    CreateRow("九州支店") = row("AMOUNT")
                    CreateRow("合計") = row("AMOUNT")
                Else
                    CreateRow("九州支店") = 0
                End If
                '経理部
                If row("ORGCODE").ToString = "011307" Then
                    CreateRow("経理部") = row("AMOUNT")
                    CreateRow("合計") = row("AMOUNT")
                Else
                    CreateRow("経理部") = 0
                End If
                '本社
                If row("ORGCODE").ToString = "011301" Then
                    CreateRow("本社") = row("AMOUNT")
                    CreateRow("合計") = row("AMOUNT")
                Else
                    CreateRow("本社") = 0
                End If

                ExcelFormat.Rows.Add(CreateRow)
            Else
                '分割計算
                'Dim test = CType(SelectRow(0).Item("コンテナ部").ToString, Integer)
                'test += CInt(row("AMOUNT"))
                'SelectRow(0).Item("コンテナ部") = test.ToString
                'データが存在する場合
                Select Case row("ORGCODE").ToString
                    Case "011312"
                        'コンテナ部
                        SelectRow(0).Item("コンテナ部") = (CType(SelectRow(0).Item("コンテナ部").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                        SelectRow(0).Item("合計") = (CType(SelectRow(0).Item("合計").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                    Case "010102"
                        '北海道支店
                        SelectRow(0).Item("北海道支店") = (CType(SelectRow(0).Item("北海道支店").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                        SelectRow(0).Item("合計") = (CType(SelectRow(0).Item("合計").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                    Case "010401"
                        '東北支店
                        SelectRow(0).Item("東北支店") = (CType(SelectRow(0).Item("東北支店").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                        SelectRow(0).Item("合計") = (CType(SelectRow(0).Item("合計").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                    Case "011402",
                         "011501"
                        '関東支店と新潟支店
                        SelectRow(0).Item("関東支店") = (CType(SelectRow(0).Item("関東支店").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                        SelectRow(0).Item("合計") = (CType(SelectRow(0).Item("合計").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                    Case "012401"
                        '中部支店
                        SelectRow(0).Item("中部支店") = (CType(SelectRow(0).Item("中部支店").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                        SelectRow(0).Item("合計") = (CType(SelectRow(0).Item("合計").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                    Case "012701"
                        '関西支店
                        SelectRow(0).Item("関西支店") = (CType(SelectRow(0).Item("関西支店").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                        SelectRow(0).Item("合計") = (CType(SelectRow(0).Item("合計").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                    Case "014001"
                        '九州支店
                        SelectRow(0).Item("九州支店") = (CType(SelectRow(0).Item("九州支店").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                        SelectRow(0).Item("合計") = (CType(SelectRow(0).Item("合計").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                    Case "011307"
                        '経理部
                        SelectRow(0).Item("経理部") = (CType(SelectRow(0).Item("経理部").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                        SelectRow(0).Item("合計") = (CType(SelectRow(0).Item("合計").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                    Case "011301"
                        '本社
                        SelectRow(0).Item("本社") = (CType(SelectRow(0).Item("本社").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                        SelectRow(0).Item("合計") = (CType(SelectRow(0).Item("合計").ToString, Integer) + CInt(row("AMOUNT"))).ToString
                End Select
            End If
        Next

        'Excelのフォーマット形に出力したデータの並び替え
        Dim dv As DataView = New DataView(ExcelFormat)
        dv.Sort = "請求先コード, レコードタイプ, レコード集計, 勘定科目コード, セグメントコード"
        ExcelFormat = dv.ToTable()

        Return ExcelFormat

    End Function


    ''' <summary>
    ''' 帳票作成
    ''' 
    ''' ※帳票ヘッダ部への出力内容等があるなら引数として渡す
    ''' </summary>
    ''' <returns>ダウンロードURL</returns>
    Public Function CreateExcelPrintData() As String

        Dim tmpFileName As String = ""
        Select Case Me.Type.ToString
            Case "1"
                '任意期間
                tmpFileName &= "請求先・勘定科目・計上店別営業収入計上一覧表（" & CDate(Me.STYMD).ToString("yyyy年MM月dd日") & "～" & CDate(Me.ENDYMD).ToString("yyyy年MM月dd日") & "）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "2"
                '１Ｑ
                tmpFileName &= "請求先・勘定科目・計上店別営業収入計上一覧表（" & Year & "年１Ｑ）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "3"
                '２Ｑ
                tmpFileName &= "請求先・勘定科目・計上店別営業収入計上一覧表（" & Year & "年２Ｑ）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "4"
                '３Ｑ
                tmpFileName &= "請求先・勘定科目・計上店別営業収入計上一覧表（" & Year & "年３Ｑ）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "5"
                '４Ｑ
                tmpFileName &= "請求先・勘定科目・計上店別営業収入計上一覧表（" & Year & "年４Ｑ）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "6"
                '上半期
                tmpFileName &= "請求先・勘定科目・計上店別営業収入計上一覧表（" & Year & "年上期）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "7"
                '下半期
                tmpFileName &= "請求先・勘定科目・計上店別営業収入計上一覧表（" & Year & "年下期）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
            Case "8"
                '年度
                tmpFileName &= "請求先・勘定科目・計上店別営業収入計上一覧表（" & Year & "年度）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        End Select
        Dim tmpFilePath As String = IO.Path.Combine(UploadRootPath, tmpFileName)

        Try
            Dim srcRange As IRange = Nothing   '参考元ソース
            Dim destRange As IRange = Nothing  '出力先ソース
            Dim lRow As DataRow = Nothing      '一つ前のデータ
            Dim ReportCount As Integer = 0     '出力個数
            Dim rowCnt As Integer = 1          '出力行


            '勘定科目別・計上店別営業収入計上一覧表明細作成処理
            For Each row As DataRow In PrintData.Rows

                If ReportCount = 0 Then
                    '請求先・勘定科目・計上店別営業収入計上一覧表ヘッダ作成処理
                    EditHeader(row, rowCnt)
                    lRow = row
                    ReportCount += 1
                Else
                    ReportCount += 1
                End If

                Select Case row("レコードタイプ").ToString
                    Case "1"
                        If row("レコード集計").ToString = "1" Then
                            '明細編集
                            EditDetail(row, rowCnt, "1", lRow, ReportCount)
                            lRow = row
                        ElseIf row("レコード集計").ToString = "2" Then
                            '小計編集
                            EditSegmentSum(row, rowCnt, "1")
                        End If
                    Case "2"
                        If row("レコード集計").ToString = "1" Then
                            '明細編集
                            EditDetail(row, rowCnt, "2", lRow, ReportCount)
                            lRow = row
                        ElseIf row("レコード集計").ToString = "2" Then
                            '小計編集
                            EditSegmentSum(row, rowCnt, "2")
                        End If
                End Select
            Next

            '出力シートのみ残す 
            WW_Workbook.Worksheets(WW_tmpSheetNo).Delete() '雛形シート削除

            '出力シートの名称変更
            WW_Workbook.Worksheets(0).Name = CONST_BILLING_ACCOUNT_LIST

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
            'タイトル
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("I" + rowCnt.ToString())
            Select Case Me.Type.ToString
                Case "1"
                    '任意期間
                    srcRange.Value = srcRange.Value.ToString + "　（" & CDate(Me.STYMD).ToString("yyyy年MM月dd日") & "～" & CDate(Me.ENDYMD).ToString("yyyy年MM月dd日") & "）"
                Case "2"
                    '１Ｑ
                    srcRange.Value = srcRange.Value.ToString + "　（" & Year & "年１Ｑ）"
                Case "3"
                    '２Ｑ
                    srcRange.Value = srcRange.Value.ToString + "　（" & Year & "年２Ｑ）"
                Case "4"
                    '３Ｑ
                    srcRange.Value = srcRange.Value.ToString + "　（" & Year & "年３Ｑ）"
                Case "5"
                    '４Ｑ
                    srcRange.Value = srcRange.Value.ToString + "　（" & Year & "年４Ｑ）"
                Case "6"
                    '上半期
                    srcRange.Value = srcRange.Value.ToString + "　（" & Year & "年上期）"
                Case "7"
                    '下半期
                    srcRange.Value = srcRange.Value.ToString + "　（" & Year & "年下期）"
                Case "8"
                    '年度
                    srcRange.Value = srcRange.Value.ToString + "　（" & Year & "年度）"
            End Select

            rowCnt += 3

            '明細部ヘッダーフォーマットコピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A4:BX4")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))

            rowCnt += 1

        Catch ex As Exception
            Throw '呼出し元にThrow
        End Try

    End Sub

    ''' <summary>
    ''' 明細部編集
    ''' </summary>
    Private Sub EditDetail(ByVal row As DataRow,
                           ByRef rowCnt As Integer,
                           ByVal Type As String,
                           ByVal lrow As DataRow,
                           ByVal ReportCount As Integer)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        '明細行フォーマットコピー
        If Type = "1" Then
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A6:BX6")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            destRange.RowHeight = COMMON_ROW_HEIGHT
        ElseIf Type = "2" Then
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A10:BX10")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            destRange.RowHeight = COMMON_ROW_HEIGHT
        End If

        '請求先コード
        If row("請求先コード").ToString <> CONST_INVOICECODE Then
            If Type = "1" AndAlso lrow("請求先コード").ToString <> row("請求先コード").ToString Then
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                srcRange.Value = row("請求先コード")
            ElseIf Type = "1" AndAlso ReportCount = 1 Then
                srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
                srcRange.Value = row("請求先コード")
            End If
        End If

        '請求先名
        If Type = "2" OrElse lrow("請求先コード").ToString <> row("請求先コード").ToString OrElse ReportCount = 1 Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("D" + rowCnt.ToString())
            srcRange.Value = row("請求先名")
        End If

        '勘定科目コード
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("V" + rowCnt.ToString())
        srcRange.Value = row("勘定科目コード")

        '勘定科目名
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("Y" + rowCnt.ToString())
        srcRange.Value = row("勘定科目名")

        'セグメントコード
        If Not IsDBNull(row("セグメントコード")) Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AE" + rowCnt.ToString())
            srcRange.Value = row("セグメントコード")
        End If

        'セグメント名
        If Not IsDBNull(row("セグメント名")) Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AG" + rowCnt.ToString())
            srcRange.Value = row("セグメント名")
        End If

        '化成品一部
        If Not IsDBNull(row("コンテナ部")) Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AK" + rowCnt.ToString())
            srcRange.Value = row("コンテナ部")
        Else
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AK" + rowCnt.ToString())
            srcRange.Value = 0
        End If

        '北海道支店
        If Not IsDBNull(row("北海道支店")) Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AO" + rowCnt.ToString())
            srcRange.Value = row("北海道支店")
        Else
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AO" + rowCnt.ToString())
            srcRange.Value = 0
        End If

        '東北支店
        If Not IsDBNull(row("東北支店")) Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AS" + rowCnt.ToString())
            srcRange.Value = row("東北支店")
        Else
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AS" + rowCnt.ToString())
            srcRange.Value = 0
        End If

        '関東支店
        If Not IsDBNull(row("関東支店")) Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AW" + rowCnt.ToString())
            srcRange.Value = row("関東支店")
        Else
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AW" + rowCnt.ToString())
            srcRange.Value = 0
        End If

        '中部支店
        If Not IsDBNull(row("中部支店")) Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BA" + rowCnt.ToString())
            srcRange.Value = row("中部支店")
        Else
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BA" + rowCnt.ToString())
            srcRange.Value = 0
        End If

        '関西支店
        If Not IsDBNull(row("関西支店")) Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BE" + rowCnt.ToString())
            srcRange.Value = row("関西支店")
        Else
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BE" + rowCnt.ToString())
            srcRange.Value = 0
        End If

        '九州支店
        If Not IsDBNull(row("九州支店")) Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BI" + rowCnt.ToString())
            srcRange.Value = row("九州支店")
        Else
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BI" + rowCnt.ToString())
            srcRange.Value = 0
        End If

        '経理部
        If Not IsDBNull(row("経理部")) Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BM" + rowCnt.ToString())
            srcRange.Value = row("経理部")
        Else
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BM" + rowCnt.ToString())
            srcRange.Value = 0
        End If

        '本社
        If Not IsDBNull(row("本社")) Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BQ" + rowCnt.ToString())
            srcRange.Value = row("本社")
        Else
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BQ" + rowCnt.ToString())
            srcRange.Value = 0
        End If

        '合計金額
        If Not IsDBNull(row("合計")) Then
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BU" + rowCnt.ToString())
            srcRange.Value = row("合計")
        Else
            srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BU" + rowCnt.ToString())
            srcRange.Value = 0
        End If

        '行番号加算
        rowCnt += 1

    End Sub

    ''' <summary>
    ''' 合計行編集
    ''' </summary>
    Private Sub EditSegmentSum(ByVal row As DataRow,
                           ByRef rowCnt As Integer,
                               ByVal Type As String)

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        '合計金額行フォーマットコピー
        If Type = "1" Then
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A8:BX8")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            destRange.RowHeight = COMMON_ROW_HEIGHT
        ElseIf Type = "2" Then
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A12:BX12")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + rowCnt.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", rowCnt))
            destRange.RowHeight = FINANCE_SUM_ROW_HEIGHT
            '行番号加算
            rowCnt += 1
            Exit Sub
        End If

        'コンテナ部合計金額
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AK" + rowCnt.ToString())
        srcRange.Value = CInt(srcRange.Value) + CInt(row("コンテナ部"))

        '北海道支店合計金額
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AO" + rowCnt.ToString())
        srcRange.Value = CInt(srcRange.Value) + CInt(row("北海道支店"))

        '東北支店合計金額
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AS" + rowCnt.ToString())
        srcRange.Value = CInt(srcRange.Value) + CInt(row("東北支店"))

        '関東支店合計金額
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("AW" + rowCnt.ToString())
        srcRange.Value = CInt(srcRange.Value) + CInt(row("関東支店"))

        '中部支店合計金額
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BA" + rowCnt.ToString())
        srcRange.Value = CInt(srcRange.Value) + CInt(row("中部支店"))

        '関西支店合計金額
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BE" + rowCnt.ToString())
        srcRange.Value = CInt(srcRange.Value) + CInt(row("関西支店"))

        '九州支店合計金額
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BI" + rowCnt.ToString())
        srcRange.Value = CInt(srcRange.Value) + CInt(row("九州支店"))

        '経理部合計金額
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BM" + rowCnt.ToString())
        srcRange.Value = CInt(srcRange.Value) + CInt(row("経理部"))

        '本社合計金額
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BQ" + rowCnt.ToString())
        srcRange.Value = CInt(srcRange.Value) + CInt(row("本社"))

        '全支店合計金額
        srcRange = WW_Workbook.Worksheets(WW_SheetNo).Range("BU" + rowCnt.ToString())
        srcRange.Value = CInt(srcRange.Value) + CInt(row("合計"))

        '行番号加算
        rowCnt += 1

    End Sub

End Class

