''************************************************************
' 営業日報(全社別)帳票作成処理
' 作成日 2022/09/07
' 作成者 牧野
' 更新日 2024/08/07
' 更新者 名取
'
' 修正履歴 : 2022/12/05 牧野 VIEWのID変更
'          : 2024/08/07 名取 予算計算修正対応
''************************************************************
Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 営業日報(支店別)帳票作成クラス
''' </summary>
Public Class LNT0010_SelesReport_SHITEN_DIODOC

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""

    Private DetailDataDtl As DataTable

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
    Public Sub New(mapId As String, excelFileName As String)
        Try
            Dim CS0050SESSION As New CS0050SESSION
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
                If WW_Workbook.Worksheets(i).Name = "営業日報(支店別)" Then
                    WW_SheetNo = i
                End If
            Next
        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' テンプレートを元に帳票を作成しダウンロードURLを生成する
    ''' </summary>
    ''' <param name="I_CAMPCD">会社CD(検索条件用)</param>
    ''' <param name="I_KEYYMD">年月日(検索条件用)</param>
    ''' <returns>ダウンロード先URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateExcelPrintData(ByVal I_CAMPCD As String, ByVal I_KEYYMD As String) As String
        Dim tmpFileName As String = "営業日報（支店別）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Me.WW_CampCode = I_CAMPCD
        Me.WW_KeyYMD = I_KEYYMD.Replace("/", "")

        Try
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                '明細取得処理
                DetailDataDtl = GetSyunyuDataTbl(SQLcon)
                If DetailDataDtl.Rows.Count = 0 Then
                    Return ""
                End If

            End Using

            '***** TODO処理 ここから *****
            '◯ヘッダーの設定
            EditHeaderArea(I_KEYYMD)

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
    ''' 帳票のヘッダー設定
    ''' </summary>
    Private Sub EditHeaderArea(ByVal I_KEYYMD As String)
        Try
            '◯ 出力日
            WW_Workbook.Worksheets(WW_SheetNo).Cells(1, 3).Value = Format(CDate(I_KEYYMD), "yyyy年MM月dd日（ddd）")

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
            Dim shitenRow As Integer = 0
            Dim rowNumber As Integer = 0
            For Each rowData As DataRow In DetailDataDtl.Rows

                '支店
                Select Case rowData("SHITENGROUPCD").ToString
                    Case "2"       '北海道
                        shitenRow = 6
                    Case "3"       '東北
                        shitenRow = 16
                    Case "4"       '関東
                        shitenRow = 26
                    Case "5"       '中部
                        shitenRow = 36
                    Case "6"       '関西
                        shitenRow = 46
                    Case "7"       '九州
                        shitenRow = 56
                End Select

                Select Case rowData("KEISHIKIGROUPCD").ToString
                    Case "1"       '冷蔵
                        rowNumber = shitenRow
                    Case "2"       'S-UR
                        rowNumber = shitenRow + 2
                    Case "6"       'L10t
                        rowNumber = shitenRow + 4
                    Case "9"       '計
                        rowNumber = shitenRow + 8
                    Case Else       'その他
                        rowNumber = shitenRow + 6
                End Select

                '明細設定処理
                '◯ 当日実績
                '当日[実績]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 3).Value = CInt(rowData("TTJ_KOSU").ToString)        '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 4).Value = CInt(rowData("TTJ_SHUNYU").ToString)      '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 5).Value = CInt(rowData("TTJ_TANKA").ToString)       '単価
                '累計[実績]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 3).Value = CInt(rowData("TRJ_KOSU").ToString)    '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 4).Value = CInt(rowData("TRJ_SHUNYU").ToString)  '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 5).Value = CInt(rowData("TRJ_TANKA").ToString)   '単価

                ' 2024/08/07 名取 CHG START
                '当日[予算]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 6).Value = CDec(rowData("TTY_KOSU").ToString)        '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 7).Value = CDec(rowData("TTY_SHUNYU").ToString)      '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 8).Value = CDec(rowData("TTY_TANKA").ToString)       '単価
                '累計[予算]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 6).Value = CDec(rowData("TRY_KOSU").ToString)    '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 7).Value = CDec(rowData("TRY_SHUNYU").ToString)  '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 8).Value = CDec(rowData("TRY_TANKA").ToString)   '単価

                '当日[対比]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 9).Value = CDec(rowData("TTT_KOSU").ToString)        '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 10).Value = CDec(rowData("TTT_SHUNYU").ToString)     '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 11).Value = CDec(rowData("TTT_TANKA").ToString)      '単価
                '累計[対比]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 9).Value = CDec(rowData("TTR_KOSU").ToString)    '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 10).Value = CDec(rowData("TTR_SHUNYU").ToString) '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 11).Value = CDec(rowData("TTR_TANKA").ToString)  '単価
                ' 2024/08/07 名取 CHG END

                '当日[対比％]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 12).Value = CDec(rowData("TRT_KOSU").ToString)       '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 13).Value = CDec(rowData("TRT_SHUNYU").ToString)     '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 14).Value = CDec(rowData("TRT_TANKA").ToString)      '単価
                '累計[対比％]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 12).Value = CDec(rowData("TRR_KOSU").ToString)   '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 13).Value = CDec(rowData("TRR_SHUNYU").ToString) '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 14).Value = CDec(rowData("TRR_TANKA").ToString)  '単価

                '◯ 前年実績
                '当日[実績]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 15).Value = CInt(rowData("ZTJ_KOSU").ToString)       '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 16).Value = CInt(rowData("ZTJ_SHUNYU").ToString)     '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 17).Value = CInt(rowData("ZTJ_TANKA").ToString)      '単価
                '累計[実績]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 15).Value = CInt(rowData("ZRJ_KOSU").ToString)   '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 16).Value = CInt(rowData("ZRJ_SHUNYU").ToString) '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 17).Value = CInt(rowData("ZRJ_TANKA").ToString)  '単価

                '当日[対比]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 18).Value = CInt(rowData("ZTT_KOSU").ToString)       '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 19).Value = CInt(rowData("ZTT_SHUNYU").ToString)     '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 20).Value = CInt(rowData("ZTT_TANKA").ToString)      '単価
                '累計[対比]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 18).Value = CInt(rowData("ZTR_KOSU").ToString)   '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 19).Value = CInt(rowData("ZTR_SHUNYU").ToString) '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 20).Value = CInt(rowData("ZTR_TANKA").ToString)  '単価

                '当日[対比％]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 21).Value = CDec(rowData("ZRT_KOSU").ToString)       '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 22).Value = CDec(rowData("ZRT_SHUNYU").ToString)     '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 23).Value = CDec(rowData("ZRT_TANKA").ToString)      '単価
                '累計[対比％]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 21).Value = CDec(rowData("ZRR_KOSU").ToString)   '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 22).Value = CDec(rowData("ZRR_SHUNYU").ToString) '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 23).Value = CDec(rowData("ZRR_TANKA").ToString)  '単価

                '◯ 回送累計実績
                '当日[実績]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 24).Value = CInt(rowData("KTJ_KNNI").ToString)       '管内
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 25).Value = CInt(rowData("KTJ_KNGI").ToString)       '管外
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 26).Value = CInt(rowData("KTJ_TOTAL").ToString)      '合計
                '累計[実績]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 24).Value = CInt(rowData("KRJ_KNNI").ToString)   '管内
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 25).Value = CInt(rowData("KRJ_KNGI").ToString)   '管外
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 26).Value = CInt(rowData("KRJ_TOTAL").ToString)  '合計

                ' 2024/08/07 名取 CHG START
                '当日[予算]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 27).Value = CDec(rowData("KTY_KNNI").ToString)       '管内
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 28).Value = CDec(rowData("KTY_KNGI").ToString)       '管外
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 29).Value = CDec(rowData("KTY_TOTAL").ToString)      '合計
                '累計[予算]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 27).Value = CDec(rowData("KRY_KNNI").ToString)   '管内
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 28).Value = CDec(rowData("KRY_KNGI").ToString)   '管外
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 29).Value = CDec(rowData("KRY_TOTAL").ToString)  '合計

                '当日[対比]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 30).Value = CDec(rowData("KTT_KNNI").ToString)       '管内
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 31).Value = CDec(rowData("KTT_KNGI").ToString)       '管外
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 32).Value = CDec(rowData("KTT_TOTAL").ToString)      '合計
                '累計[対比]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 30).Value = CDec(rowData("KTR_KNNI").ToString)   '管内
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 31).Value = CDec(rowData("KTR_KNGI").ToString)   '管外
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 32).Value = CDec(rowData("KTR_TOTAL").ToString)  '合計
                ' 2024/08/07 名取 CHG END

                '当日[対比％]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 33).Value = CDec(rowData("KRT_KNNI").ToString)       '管内
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 34).Value = CDec(rowData("KRT_KNGI").ToString)       '管外
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber, 35).Value = CDec(rowData("KRT_TOTAL").ToString)      '合計
                '累計[対比％]
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 33).Value = CDec(rowData("KRR_KNNI").ToString)   '管内
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 34).Value = CDec(rowData("KRR_KNGI").ToString)   '管外
                WW_Workbook.Worksheets(WW_SheetNo).Cells(rowNumber + 1, 35).Value = CDec(rowData("KRR_TOTAL").ToString)  '合計
            Next

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 収入明細取得処理
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetSyunyuDataTbl(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt = New DataTable
        Dim WW_DATENOW As Date = Date.Now

        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    D01.SHITENGROUPCD")                                                                                                     '支店グループコード
        SQLBldr.AppendLine("    , D01.KEISHIKIGROUPCD")                                                                                                 '形式グループコード
        SQLBldr.AppendLine("    , D01.TTJ_KOSU")                                                                                                        '当日_個数(当日実績)
        SQLBldr.AppendLine("    , D01.TTJ_SHUNYU")                                                                                                      '当日_収入(当日実績)
        SQLBldr.AppendLine("    , D01.TTJ_TANKA")                                                                                                       '当日_単価(当日実績)
        SQLBldr.AppendLine("    , D01.TRJ_KOSU")                                                                                                        '累計_個数(当日実績)
        SQLBldr.AppendLine("    , D01.TRJ_SHUNYU")                                                                                                      '累計_収入(当日予算)
        SQLBldr.AppendLine("    , D01.TRJ_TANKA")                                                                                                       '累計_単価(当日実績)
        SQLBldr.AppendLine("    , D01.TTY_KOSU")                                                                                                        '当日_個数(当日予算)
        SQLBldr.AppendLine("    , D01.TTY_SHUNYU")                                                                                                      '当日_収入(当日予算)
        SQLBldr.AppendLine("    , D01.TTY_TANKA")                                                                                                       '当日_単価(当日予算)
        SQLBldr.AppendLine("    , D01.TRY_KOSU")                                                                                                        '累計_個数(当日予算)
        SQLBldr.AppendLine("    , D01.TRY_SHUNYU")                                                                                                      '累計_収入(当日予算)
        SQLBldr.AppendLine("    , D01.TRY_TANKA")                                                                                                       '累計_単価(当日予算)
        SQLBldr.AppendLine("    , D01.TTJ_KOSU - D01.TTY_KOSU AS TTT_KOSU")                                                                             '当日_個数(当日対比)
        SQLBldr.AppendLine("    , D01.TTJ_SHUNYU - D01.TTY_SHUNYU AS TTT_SHUNYU")                                                                       '当日_収入(当日対比)
        SQLBldr.AppendLine("    , D01.TTJ_TANKA - D01.TTY_TANKA AS TTT_TANKA")                                                                          '当日_単価(当日対比)
        SQLBldr.AppendLine("    , D01.TRJ_KOSU - D01.TRY_KOSU AS TTR_KOSU")                                                                             '累計_個数(当日対比)
        SQLBldr.AppendLine("    , D01.TRJ_SHUNYU - D01.TRY_SHUNYU AS TTR_SHUNYU")                                                                       '累計_収入(当日対比)
        SQLBldr.AppendLine("    , D01.TRJ_TANKA - D01.TRY_TANKA AS TTR_TANKA")                                                                          '累計_単価(当日対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.TTJ_KOSU <> 0 AND TTY_KOSU <> 0 THEN D01.TTJ_KOSU / D01.TTY_KOSU - 1 ELSE 0 END AS TRT_KOSU")           '当日_個数(当日対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.TTJ_SHUNYU <> 0 AND TTY_SHUNYU <> 0 THEN D01.TTJ_SHUNYU / D01.TTY_SHUNYU - 1 ELSE 0 END AS TRT_SHUNYU") '当日_収入(当日対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.TTJ_TANKA <> 0 AND TTY_TANKA <> 0 THEN D01.TTJ_TANKA / D01.TTY_TANKA - 1 ELSE 0 END AS TRT_TANKA")      '当日_単価(当日対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.TRJ_KOSU <> 0 AND TRY_KOSU <> 0 THEN D01.TRJ_KOSU / D01.TRY_KOSU - 1 ELSE 0 END AS TRR_KOSU")           '累計_個数(当日対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.TRJ_SHUNYU <> 0 AND TRY_SHUNYU <> 0 THEN D01.TRJ_SHUNYU / D01.TRY_SHUNYU - 1 ELSE 0 END AS TRR_SHUNYU") '累計_収入(当日対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.TRJ_TANKA <> 0 AND TRY_TANKA <> 0 THEN D01.TRJ_TANKA / D01.TRY_TANKA - 1 ELSE 0 END AS TRR_TANKA")      '累計_単価(当日対比比率)
        SQLBldr.AppendLine("    , D01.ZTJ_KOSU")                                                                                                        '当日_個数(前年実績)
        SQLBldr.AppendLine("    , D01.ZTJ_SHUNYU")                                                                                                      '当日_収入(前年実績)
        SQLBldr.AppendLine("    , D01.ZTJ_TANKA")                                                                                                       '当日_単価(前年実績)
        SQLBldr.AppendLine("    , D01.ZRJ_KOSU")                                                                                                        '累計_個数(前年実績)
        SQLBldr.AppendLine("    , D01.ZRJ_SHUNYU")                                                                                                      '累計_収入(前年実績)
        SQLBldr.AppendLine("    , D01.ZRJ_TANKA")                                                                                                       '累計_単価(前年実績)
        SQLBldr.AppendLine("    , D01.ZTY_KOSU")                                                                                                        '当日_個数(前年予算)
        SQLBldr.AppendLine("    , D01.ZTY_SHUNYU")                                                                                                      '当日_収入(前年予算)
        SQLBldr.AppendLine("    , D01.ZTY_TANKA")                                                                                                       '当日_単価(前年予算)
        SQLBldr.AppendLine("    , D01.ZRY_KOSU")                                                                                                        '累計_個数(前年予算)
        SQLBldr.AppendLine("    , D01.ZRY_SHUNYU")                                                                                                      '累計_収入(前年予算)
        SQLBldr.AppendLine("    , D01.ZRY_TANKA")                                                                                                       '累計_単価(前年予算)
        SQLBldr.AppendLine("    , D01.TTJ_KOSU - D01.ZTJ_KOSU AS ZTT_KOSU")                                                                             '当日_個数(前年対比)
        SQLBldr.AppendLine("    , D01.TTJ_SHUNYU - D01.ZTJ_SHUNYU AS ZTT_SHUNYU")                                                                       '当日_収入(前年対比)
        SQLBldr.AppendLine("    , D01.TTJ_TANKA - D01.ZTJ_TANKA AS ZTT_TANKA")                                                                          '当日_単価(前年対比)
        SQLBldr.AppendLine("    , D01.TRJ_KOSU - D01.ZRJ_KOSU AS ZTR_KOSU")                                                                             '累計_個数(前年対比)
        SQLBldr.AppendLine("    , D01.TRJ_SHUNYU - D01.ZRJ_SHUNYU AS ZTR_SHUNYU")                                                                       '累計_収入(前年対比)
        SQLBldr.AppendLine("    , D01.TRJ_TANKA - D01.ZRJ_TANKA AS ZTR_TANKA")                                                                          '累計_単価(前年対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.TTJ_KOSU <> 0 AND ZTJ_KOSU <> 0 THEN D01.TTJ_KOSU / D01.ZTJ_KOSU - 1 ELSE 0 END AS ZRT_KOSU")           '当日_個数(前年対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.TTJ_SHUNYU <> 0 AND ZTJ_SHUNYU <> 0 THEN D01.TTJ_SHUNYU / D01.ZTJ_SHUNYU - 1 ELSE 0 END AS ZRT_SHUNYU") '当日_収入(前年対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.TTJ_TANKA <> 0 AND ZTJ_TANKA <> 0 THEN D01.TTJ_TANKA / D01.ZTJ_TANKA - 1 ELSE 0 END AS ZRT_TANKA")      '当日_単価(前年対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.TRJ_KOSU <> 0 AND ZRJ_KOSU <> 0 THEN D01.TRJ_KOSU / D01.ZRJ_KOSU - 1 ELSE 0 END AS ZRR_KOSU")           '累計_個数(前年対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.TRJ_SHUNYU <> 0 AND ZRJ_SHUNYU <> 0 THEN D01.TRJ_SHUNYU / D01.ZRJ_SHUNYU - 1 ELSE 0 END AS ZRR_SHUNYU") '累計_収入(前年累計対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.TRJ_TANKA <> 0 AND ZRJ_TANKA <> 0 THEN D01.TRJ_TANKA / D01.ZRJ_TANKA - 1 ELSE 0 END AS ZRR_TANKA")      '累計_単価(前年累計対比)
        SQLBldr.AppendLine("    , D01.KTJ_KNNI")                                                                                                        '当日_管内(回送累計実績)
        SQLBldr.AppendLine("    , D01.KTJ_KNGI")                                                                                                        '当日_管外(回送累計実績)
        SQLBldr.AppendLine("    , D01.KTJ_TOTAL")                                                                                                       '当日_合計(回送累計実績)
        SQLBldr.AppendLine("    , D01.KRJ_KNNI")                                                                                                        '累計_管内(回送累計実績)
        SQLBldr.AppendLine("    , D01.KRJ_KNGI")                                                                                                        '累計_管外(回送累計実績)
        SQLBldr.AppendLine("    , D01.KRJ_TOTAL")                                                                                                       '累計_合計(回送累計実績)
        SQLBldr.AppendLine("    , D01.KTY_KNNI")                                                                                                        '当日_管内(回送累計予算)
        SQLBldr.AppendLine("    , D01.KTY_KNGI")                                                                                                        '当日_管外(回送累計予算)
        SQLBldr.AppendLine("    , D01.KTY_TOTAL")                                                                                                       '当日_合計(回送累計予算)
        SQLBldr.AppendLine("    , D01.KRY_KNNI")                                                                                                        '累計_管内(回送累計予算)
        SQLBldr.AppendLine("    , D01.KRY_KNGI")                                                                                                        '累計_管外(回送累計予算)
        SQLBldr.AppendLine("    , D01.KRY_TOTAL")                                                                                                       '累計_合計(回送累計予算)
        SQLBldr.AppendLine("    , D01.KTJ_KNNI - D01.KTY_KNNI AS KTT_KNNI")                                                                             '当日_管内(回送累計対比)
        SQLBldr.AppendLine("    , D01.KTJ_KNGI - D01.KTY_KNGI AS KTT_KNGI")                                                                             '当日_管外(回送累計対比)
        SQLBldr.AppendLine("    , D01.KTJ_TOTAL - D01.KTY_TOTAL AS KTT_TOTAL")                                                                          '当日_合計(回送累計対比)
        SQLBldr.AppendLine("    , D01.KRJ_KNNI - D01.KRY_KNNI AS KTR_KNNI")                                                                             '累計_管内(回送累計対比)
        SQLBldr.AppendLine("    , D01.KRJ_KNGI - D01.KRY_KNGI AS KTR_KNGI")                                                                             '累計_管外(回送累計対比)
        SQLBldr.AppendLine("    , D01.KRJ_TOTAL - D01.KRY_TOTAL AS KTR_TOTAL")                                                                          '累計_合計(回送累計対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KTJ_KNNI <> 0 AND KTY_KNNI <> 0 THEN D01.KTJ_KNNI / D01.KTY_KNNI - 1 ELSE 0 END AS KRT_KNNI")           '当日_管内(回送累計対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.KTJ_KNGI <> 0 AND KTY_KNGI <> 0 THEN D01.KTJ_KNGI / D01.KTY_KNGI - 1 ELSE 0 END AS KRT_KNGI")           '当日_管外(回送累計対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.KTJ_TOTAL <> 0 AND KTY_TOTAL <> 0 THEN D01.KTJ_TOTAL / D01.KTY_TOTAL - 1 ELSE 0 END AS KRT_TOTAL")      '当日_合計(回送累計対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.KRJ_KNNI <> 0 AND KRY_KNNI <> 0 THEN D01.KRJ_KNNI / D01.KRY_KNNI - 1 ELSE 0 END AS KRR_KNNI")           '累計_管内(回送累計対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.KRJ_KNGI <> 0 AND KRY_KNGI <> 0 THEN D01.KRJ_KNGI / D01.KRY_KNGI - 1 ELSE 0 END AS KRR_KNGI")           '累計_管外(回送累計対比比率)
        SQLBldr.AppendLine("    , CASE WHEN D01.KRJ_TOTAL <> 0 AND KRY_TOTAL <> 0 THEN D01.KRJ_TOTAL / D01.KRY_TOTAL - 1 ELSE 0 END AS KRR_TOTAL")      '累計_合計(回送累計対比比率)
        SQLBldr.AppendLine("FROM")
        'メイン コンテナ営業日報データ 表Ａ
        SQLBldr.AppendLine("    lng.VIW0008_BIZDAILYRP_SHITEN D01")
        '抽出条件
        SQLBldr.AppendLine("    WHERE")
        SQLBldr.AppendLine("        D01.DATADATE = '" & WW_KeyYMD & "'")

        Try
            Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)  '削除フラグ

                PARA01.Value = C_DELETE_FLG.ALIVE

                'SQL実行
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
            Throw
        End Try

        Return dt

    End Function

End Class
