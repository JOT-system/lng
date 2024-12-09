''************************************************************
' 営業日報(全社別)帳票作成処理
' 作成日 2022/08/25
' 作成者 牧野
' 更新日 2024/08/07
' 更新者 名取
'
' 修正履歴 : 2022/08/25 牧野 新規作成
'          : 2022/12/05 牧野 VIEWのID変更
'          : 2022/12/16 名取 営業日報出力条件の変更(ヘッダー情報が取得出来ない場合、処理を中止)
'          : 2022/02/20 伊藤 カレンダーマスタ使用変更対応
'          : 2022/02/20 名取 帳票の年月日に出力対象の日付を表示させるように変更
'          : 2023/03/13 名取 予算対応
'          : 2023/03/16 名取 品目分類変更対応
'          : 2023/04/06 名取 組織変更対応
'          : 2023/04/13 名取 差引収支明細処理をコメントアウト
'          : 2023/06/07 名取 品目分類変更対応(再修正)
'          : 2023/07/04 名取 前年の個数が0の場合の比率計算不具合対応
'          : 2023/07/20 名取 冷蔵運用個数追加
'          : 2024/05/16 名取 冷蔵運用個数表項目追加対応
'          : 2024/06/06 名取 新潟内訳追加対応
'          : 2024/08/07 名取 予算計算修正対応
''************************************************************
Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 営業日報(全社別)帳票作成クラス
''' </summary>
Public Class LNT0010_SelesReport_ALL_DIODOC

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""

    Private HeaderDtl As DataTable                          'ヘッダー
    Private KeishikiDataDtl As DataTable                    '(形式別)当日・前年実績予算対比明細
    Private KeishikiMonthYOSANDataDtl As DataTable          '(形式別)当月予算明細
    Private HinmkDateDtl As DataTable                       '(品目支店別)当月・前年実績対比明細
    Private SshkShushiDataDtl As DataTable                  '差引収支明細
    Private ReizouKosuDataDtl As DataTable                  '冷蔵運用個数明細

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
                If WW_Workbook.Worksheets(i).Name = "営業日報(全社用)" Then
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
        Dim tmpFileName As String = "営業日報（全社）_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Me.WW_CampCode = I_CAMPCD
        Me.WW_KeyYMD = I_KEYYMD.Replace("/", "")

        Try
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                'ヘッダー取得処理
                HeaderDtl = GetHeaderDataTbl(SQLcon)

                '(形式別)当日・前年実績予算対比明細取得処理
                KeishikiDataDtl = GetKeishikiDataTbl(SQLcon)
                If KeishikiDataDtl.Rows.Count = 0 Then
                    Return ""
                End If

                '(形式別)当日・前年実績予算対比明細取得処理
                KeishikiMonthYOSANDataDtl = KeishikiMonthYOSANDataTbl(SQLcon, I_KEYYMD)

                '冷蔵明細取得処理
                HinmkDateDtl = GetHinmkDataTbl(SQLcon)

                '差引収支明細取得処理
                'SshkShushiDataDtl = GetSshkShushiDataTbl(SQLcon)

                '冷蔵運用個数明細取得処理
                ReizouKosuDataDtl = GetReizouUnyoKosuDataTbl(SQLcon)

            End Using

            '***** TODO処理 ここから *****
            '◯ヘッダーの設定
            EditHeaderArea(I_KEYYMD)

            '◯明細の設定
            '(形式別)当日・前年実績予算対比明細
            EditDetailShunyu()

            '(形式別)当月予算明細
            EditDetailYOSANShunyu()

            '(品目支店別)当月・前年実績対比明細
            EditDetailHinmk()

            '差引収支明細
            'EditDetailSshkShushi()

            '冷蔵運用個数明細
            EditDetailReizouUnyoKosu()
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
            WW_Workbook.Worksheets(WW_SheetNo).Cells(1, 2).Value = Format(CDate(I_KEYYMD), "yyyy年MM月dd日（ddd）")
            '◯ 稼働日数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(1, 17).Value = "稼働日数：" & HeaderDtl.Rows(0)("TOU_WORKINGDAY").ToString & " / " & HeaderDtl.Rows(0)("TOU_WORKINGSUM").ToString &
                                                                    " 日目（前年稼働日数：" & HeaderDtl.Rows(0)("ZEN_WORKINGDAY").ToString & " / " & HeaderDtl.Rows(0)("ZEN_WORKINGSUM").ToString & "日目）"
        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定((形式別)当日・前年実績予算対比明細)
    ''' </summary>
    Private Sub EditDetailShunyu()

        Try
            Dim ShunyuRow As Integer = 0
            Dim KaisouRow As Integer = 0
            For Each rowData As DataRow In KeishikiDataDtl.Rows

                Select Case rowData("KEISHIKIGROUPCD").ToString
                    Case "1"        '冷蔵
                        ShunyuRow = 6
                        KaisouRow = 20
                    Case "2"        'S-UR
                        ShunyuRow = 8
                        KaisouRow = 22
                    Case "6"        'L10t
                        ShunyuRow = 10
                        KaisouRow = 24
                    Case "9"        '計
                        ShunyuRow = 14
                        KaisouRow = 28
                    Case Else       'その他
                        ShunyuRow = 12
                        KaisouRow = 26
                End Select

                '明細設定処理
                '【収入】
                '◯ 当日実績
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 2).Value = CInt(rowData("SN_TOJ_KOSU").ToString)                '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 3).Value = CInt(rowData("SN_TOJ_SHUNYU").ToString)              '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 4).Value = CInt(rowData("SN_TOJ_TANKA").ToString)               '単価

                '◯ 当日予算
                ' 2024/08/07 名取 CHG START
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 5).Value = CDec(rowData("SN_TOY_KOSU").ToString)                '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 6).Value = CDec(rowData("SN_TOY_SHUNYU").ToString)              '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 7).Value = CDec(rowData("SN_TOY_TANKA").ToString)               '単価
                ' 2024/08/07 名取 CHG END

                '◯ 当日予算対比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 8).Value = CDec(rowData("SN_TORTO_KOSU").ToString)              '個数 2024/08/07 名取 CHG
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 8).Value = CDec(rowData("SN_TORTO_KOSURATIO").ToString)     '個数比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 9).Value = CDec(rowData("SN_TORTO_SHUNYU").ToString)            '収入 2024/08/07 名取 CHG
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 9).Value = CDec(rowData("SN_TORTO_SHUNYURATIO").ToString)   '収入比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 10).Value = CDec(rowData("SN_TORTO_TANKA").ToString)            '単価 2024/08/07 名取 CHG
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 10).Value = CDec(rowData("SN_TORTO_TANKARATIO").ToString)   '単価比率

                '◯ 当月累計実績
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 11).Value = CInt(rowData("SN_TRJ_KOSU").ToString)               '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 12).Value = CInt(rowData("SN_TRJ_SHUNYU").ToString)             '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 13).Value = CInt(rowData("SN_TRJ_TANKA").ToString)              '単価

                '◯ 当月累計予算
                ' 2024/08/07 名取 CHG START
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 14).Value = CDec(rowData("SN_TRY_KOSU").ToString)               '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 15).Value = CDec(rowData("SN_TRY_SHUNYU").ToString)             '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 16).Value = CDec(rowData("SN_TRY_TANKA").ToString)              '単価
                ' 2024/08/07 名取 CHG END

                '◯ 当月予算対比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 17).Value = CDec(rowData("SN_TGRTO_KOSU").ToString)             '個数 2024/08/07 名取 CHG
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 17).Value = CDec(rowData("SN_TGRTO_KOSURATIO").ToString)    '個数比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 18).Value = CDec(rowData("SN_TGRTO_SHUNYU").ToString)           '収入 2024/08/07 名取 CHG
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 18).Value = CDec(rowData("SN_TGRTO_SHUNYURATIO").ToString)  '収入比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 19).Value = CDec(rowData("SN_TGRTO_TANKA").ToString)            '単価 2024/08/07 名取 CHG
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 19).Value = CDec(rowData("SN_TGRTO_TANKARATIO").ToString)   '単価比率

                '◯ 前年単日実績
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 20).Value = CInt(rowData("SN_ZNJ_KOSU").ToString)               '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 21).Value = CInt(rowData("SN_ZNJ_SHUNYU").ToString)             '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 22).Value = CInt(rowData("SN_ZNJ_TANKA").ToString)              '単価

                '◯ 前年単日実績
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 20).Value = CInt(rowData("SN_ZNJ_KOSU").ToString)               '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 21).Value = CInt(rowData("SN_ZNJ_SHUNYU").ToString)             '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 22).Value = CInt(rowData("SN_ZNJ_TANKA").ToString)              '単価

                '◯ 前年単日対比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 23).Value = CInt(rowData("SN_ZNRTO_KOSU").ToString)             '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 23).Value = CDec(rowData("SN_ZNRTO_KOSURATIO").ToString)    '個数比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 24).Value = CInt(rowData("SN_ZNRTO_SHUNYU").ToString)           '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 24).Value = CDec(rowData("SN_ZNRTO_SHUNYURATIO").ToString)  '収入比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 25).Value = CInt(rowData("SN_ZNRTO_TANKA").ToString)            '単価
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 25).Value = CDec(rowData("SN_ZNRTO_TANKARATIO").ToString)   '単価比率

                '◯ 前年累計実績
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 26).Value = CInt(rowData("SN_ZRJ_KOSU").ToString)               '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 27).Value = CInt(rowData("SN_ZRJ_SHUNYU").ToString)             '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 28).Value = CInt(rowData("SN_ZRJ_TANKA").ToString)              '単価
                '◯ 前年月間累計実績
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 26).Value = CInt(rowData("SN_MON_ZRJ_KOSU").ToString)       '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 27).Value = CInt(rowData("SN_MON_ZRJ_SHUNYU").ToString)     '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 28).Value = CInt(rowData("SN_MON_ZRJ_TANKA").ToString)      '単価

                '◯ 前年単日対比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 29).Value = CInt(rowData("SN_ZRRTO_KOSU").ToString)             '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 29).Value = CDec(rowData("SN_ZRRTO_KOSURATIO").ToString)    '個数比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 30).Value = CInt(rowData("SN_ZRRTO_SHUNYU").ToString)           '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 30).Value = CDec(rowData("SN_ZRRTO_SHUNYURATIO").ToString)  '収入比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 31).Value = CInt(rowData("SN_ZRRTO_TANKA").ToString)            '単価
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow + 1, 31).Value = CDec(rowData("SN_ZRRTO_TANKARATIO").ToString)   '単価比率

                '【回送運賃】
                '◯ 当日実績
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 2).Value = CInt(rowData("KS_TOJ_KOSU").ToString)                '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 3).Value = CInt(rowData("KS_TOJ_SHUNYU").ToString)              '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 4).Value = CInt(rowData("KS_TOJ_TANKA").ToString)               '単価

                '◯ 当日予算
                ' 2024/08/07 名取 CHG START
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 5).Value = CDec(rowData("KS_TOY_KOSU").ToString)                '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 6).Value = CDec(rowData("KS_TOY_SHUNYU").ToString)              '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 7).Value = CDec(rowData("KS_TOY_TANKA").ToString)               '単価
                ' 2024/08/07 名取 CHG END

                '◯ 当日予算対比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 8).Value = CDec(rowData("KS_TORTO_KOSU").ToString)              '個数 2024/08/07 名取 CHG
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 8).Value = CDec(rowData("KS_TORTO_KOSURATIO").ToString)     '個数比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 9).Value = CDec(rowData("KS_TORTO_SHUNYU").ToString)            '収入 2024/08/07 名取 CHG
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 9).Value = CDec(rowData("KS_TORTO_SHUNYURATIO").ToString)   '収入比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 10).Value = CDec(rowData("KS_TORTO_TANKA").ToString)            '単価 2024/08/07 名取 CHG
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 10).Value = CDec(rowData("KS_TORTO_TANKARATIO").ToString)   '単価比率

                '◯ 当月累計実績
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 11).Value = CInt(rowData("KS_TRJ_KOSU").ToString)               '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 12).Value = CInt(rowData("KS_TRJ_SHUNYU").ToString)             '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 13).Value = CInt(rowData("KS_TRJ_TANKA").ToString)              '単価

                '◯ 当月累計予算
                ' 2024/08/07 名取 CHG START
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 14).Value = CDec(rowData("KS_TRY_KOSU").ToString)               '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 15).Value = CDec(rowData("KS_TRY_SHUNYU").ToString)             '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 16).Value = CDec(rowData("KS_TRY_TANKA").ToString)              '単価
                ' 2024/08/07 名取 CHG END

                '◯ 当月予算対比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 17).Value = CDec(rowData("KS_TGRTO_KOSU").ToString)             '個数 2024/08/07 名取 CHG
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 17).Value = CDec(rowData("KS_TGRTO_KOSURATIO").ToString)    '個数比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 18).Value = CDec(rowData("KS_TGRTO_SHUNYU").ToString)           '収入 2024/08/07 名取 CHG
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 18).Value = CDec(rowData("KS_TGRTO_SHUNYURATIO").ToString)  '収入比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 19).Value = CDec(rowData("KS_TGRTO_TANKA").ToString)            '単価 2024/08/07 名取 CHG
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 19).Value = CDec(rowData("KS_TGRTO_TANKARATIO").ToString)   '単価比率

                '◯ 前年単日実績
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 20).Value = CInt(rowData("KS_ZNJ_KOSU").ToString)               '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 21).Value = CInt(rowData("KS_ZNJ_SHUNYU").ToString)             '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 22).Value = CInt(rowData("KS_ZNJ_TANKA").ToString)              '単価

                '◯ 前年単日実績
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 20).Value = CInt(rowData("KS_ZNJ_KOSU").ToString)               '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 21).Value = CInt(rowData("KS_ZNJ_SHUNYU").ToString)             '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 22).Value = CInt(rowData("KS_ZNJ_TANKA").ToString)              '単価

                '◯ 前年単日対比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 23).Value = CInt(rowData("KS_ZNRTO_KOSU").ToString)             '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 23).Value = CDec(rowData("KS_ZNRTO_KOSURATIO").ToString)    '個数比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 24).Value = CInt(rowData("KS_ZNRTO_SHUNYU").ToString)           '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 24).Value = CDec(rowData("KS_ZNRTO_SHUNYURATIO").ToString)  '収入比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 25).Value = CInt(rowData("KS_ZNRTO_TANKA").ToString)            '単価
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 25).Value = CDec(rowData("KS_ZNRTO_TANKARATIO").ToString)   '単価比率

                '◯ 前年累計実績
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 26).Value = CInt(rowData("KS_ZRJ_KOSU").ToString)               '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 27).Value = CInt(rowData("KS_ZRJ_SHUNYU").ToString)             '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 28).Value = CInt(rowData("KS_ZRJ_TANKA").ToString)              '単価
                '◯ 前年月間累計実績
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 26).Value = CInt(rowData("KS_MON_ZRJ_KOSU").ToString)       '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 27).Value = CInt(rowData("KS_MON_ZRJ_SHUNYU").ToString)     '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 28).Value = CInt(rowData("KS_MON_ZRJ_TANKA").ToString)      '単価

                '◯ 前年単日対比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 29).Value = CInt(rowData("KS_ZRRTO_KOSU").ToString)             '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 29).Value = CDec(rowData("KS_ZRRTO_KOSURATIO").ToString)    '個数比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 30).Value = CInt(rowData("KS_ZRRTO_SHUNYU").ToString)           '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 30).Value = CDec(rowData("KS_ZRRTO_SHUNYURATIO").ToString)  '収入比率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 31).Value = CInt(rowData("KS_ZRRTO_TANKA").ToString)            '単価
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow + 1, 31).Value = CDec(rowData("KS_ZRRTO_TANKARATIO").ToString)   '単価比率
            Next

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 帳票の明細設定((形式別)当月予算明細)
    ''' </summary>
    Private Sub EditDetailYOSANShunyu()

        Try
            Dim ShunyuRow As Integer = 0
            Dim KaisouRow As Integer = 0
            For Each rowData As DataRow In KeishikiMonthYOSANDataDtl.Rows

                Select Case rowData("BIGCTNTYPECODE").ToString
                    Case "1"        '冷蔵
                        ShunyuRow = 7
                        KaisouRow = 21
                    Case "2"        'S-UR
                        ShunyuRow = 9
                        KaisouRow = 23
                    Case "6"        'L10t
                        ShunyuRow = 11
                        KaisouRow = 25
                    Case "9"       'その他
                        ShunyuRow = 13
                        KaisouRow = 27
                    Case Else       '計
                        ShunyuRow = 15
                        KaisouRow = 29
                End Select

                '明細設定処理
                '【収入】
                '◯ 当月予算
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 14).Value = CInt(rowData("SN_QUANTITY").ToString)  '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 15).Value = CInt(rowData("SN_USEFEE").ToString)    '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ShunyuRow, 16).Value = CInt(rowData("SN_UNITPRICE").ToString) '単価

                '【回送運賃】
                '◯ 当月予算
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 14).Value = CInt(rowData("KS_QUANTITY").ToString)  '個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 15).Value = CInt(rowData("KS_USEFEE").ToString)    '収入
                WW_Workbook.Worksheets(WW_SheetNo).Cells(KaisouRow, 16).Value = CInt(rowData("KS_UNITPRICE").ToString) '単価

            Next

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 帳票の明細設定((品目支店別)当月・前年実績対比明細)
    ''' </summary>
    Private Sub EditDetailHinmk()

        Try
            Dim ReizouRow As Integer = 0
            Dim SuperurRow As Integer = 0

            '冷蔵集計用変数
            Dim WK_RZ_HKD_KOSU_TGT As Integer = 0
            Dim WK_RZ_HKD_KOSU_ZNN As Integer = 0
            Dim WK_RZ_HKD_KOSU_TIH As Integer = 0
            Dim WK_RZ_THK_KOSU_TGT As Integer = 0
            Dim WK_RZ_THK_KOSU_ZNN As Integer = 0
            Dim WK_RZ_THK_KOSU_TIH As Integer = 0
            Dim WK_RZ_KNT_KOSU_TGT As Integer = 0
            Dim WK_RZ_KNT_KOSU_ZNN As Integer = 0
            Dim WK_RZ_KNT_KOSU_TIH As Integer = 0
            Dim WK_RZ_TYB_KOSU_TGT As Integer = 0
            Dim WK_RZ_TYB_KOSU_ZNN As Integer = 0
            Dim WK_RZ_TYB_KOSU_TIH As Integer = 0
            Dim WK_RZ_KNS_KOSU_TGT As Integer = 0
            Dim WK_RZ_KNS_KOSU_ZNN As Integer = 0
            Dim WK_RZ_KNS_KOSU_TIH As Integer = 0
            Dim WK_RZ_KSY_KOSU_TGT As Integer = 0
            Dim WK_RZ_KSY_KOSU_ZNN As Integer = 0
            Dim WK_RZ_KSY_KOSU_TIH As Integer = 0
            Dim WK_RZ_TTL_KOSU_TGT As Integer = 0
            Dim WK_RZ_TTL_KOSU_ZNN As Integer = 0
            Dim WK_RZ_TTL_KOSU_TIH As Integer = 0

            'S-UR集計用変数
            Dim WK_SR_HKD_KOSU_TGT As Integer = 0
            Dim WK_SR_HKD_KOSU_ZNN As Integer = 0
            Dim WK_SR_HKD_KOSU_TIH As Integer = 0
            Dim WK_SR_THK_KOSU_TGT As Integer = 0
            Dim WK_SR_THK_KOSU_ZNN As Integer = 0
            Dim WK_SR_THK_KOSU_TIH As Integer = 0
            Dim WK_SR_KNT_KOSU_TGT As Integer = 0
            Dim WK_SR_KNT_KOSU_ZNN As Integer = 0
            Dim WK_SR_KNT_KOSU_TIH As Integer = 0
            Dim WK_SR_TYB_KOSU_TGT As Integer = 0
            Dim WK_SR_TYB_KOSU_ZNN As Integer = 0
            Dim WK_SR_TYB_KOSU_TIH As Integer = 0
            Dim WK_SR_KNS_KOSU_TGT As Integer = 0
            Dim WK_SR_KNS_KOSU_ZNN As Integer = 0
            Dim WK_SR_KNS_KOSU_TIH As Integer = 0
            Dim WK_SR_KSY_KOSU_TGT As Integer = 0
            Dim WK_SR_KSY_KOSU_ZNN As Integer = 0
            Dim WK_SR_KSY_KOSU_TIH As Integer = 0
            Dim WK_SR_TTL_KOSU_TGT As Integer = 0
            Dim WK_SR_TTL_KOSU_ZNN As Integer = 0
            Dim WK_SR_TTL_KOSU_TIH As Integer = 0

            For Each rowData As DataRow In HinmkDateDtl.Rows

                Select Case rowData("HINMOKUGROUPCD").ToString
                    Case "1"        '食料工業品
                        ReizouRow = 34
                        SuperurRow = 61
                    Case "2"        '農産品・青果物
                        ReizouRow = 36
                        SuperurRow = 63
                    Case "3"        '化学工業品
                        ReizouRow = 38
                        SuperurRow = 65
                    Case "4"        '化学薬品
                        ReizouRow = 40
                        SuperurRow = 67
                    Case "5"        '家電・情報機器
                        ReizouRow = 42
                        SuperurRow = 69
                    Case "6"        '紙・パルプ
                        ReizouRow = 44
                        SuperurRow = 71
                    Case "7"        '自動車部品
                        ReizouRow = 46
                        SuperurRow = 73
                    Case "8"        '積合せ貨物
                        ReizouRow = 48
                        SuperurRow = 75
                    Case "9"        '他工業品
                        ReizouRow = 50
                        SuperurRow = 77
                    Case "10"       'その他
                        ReizouRow = 52
                        SuperurRow = 79
                    Case Else
                        Continue For
                End Select

                '〇 明細設定処理
                '【冷蔵】****************************************************************************************************
                '◯ 北海道
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 2).Value = CInt(rowData("RZ_HKD_KOSU_TGT").ToString)                 '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 3).Value = CInt(rowData("RZ_HKD_KOSU_ZNN").ToString)                 '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 4).Value = CInt(rowData("RZ_HKD_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 4).Value = CDec(CInt(rowData("RZ_HKD_KOSU_RTO").ToString) / 100) '対比_比率

                '◯ 東北
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 5).Value = CInt(rowData("RZ_THK_KOSU_TGT").ToString)                 '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 6).Value = CInt(rowData("RZ_THK_KOSU_ZNN").ToString)                 '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 7).Value = CInt(rowData("RZ_THK_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 7).Value = CDec(CInt(rowData("RZ_THK_KOSU_RTO").ToString) / 100) '対比_比率

                '◯ 関東
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 8).Value = CInt(rowData("RZ_KNT_KOSU_TGT").ToString)                  '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 9).Value = CInt(rowData("RZ_KNT_KOSU_ZNN").ToString)                  '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 10).Value = CInt(rowData("RZ_KNT_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 10).Value = CDec(CInt(rowData("RZ_KNT_KOSU_RTO").ToString) / 100) '対比_比率

                '◯ 中部
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 11).Value = CInt(rowData("RZ_TYB_KOSU_TGT").ToString)                 '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 12).Value = CInt(rowData("RZ_TYB_KOSU_ZNN").ToString)                 '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 13).Value = CInt(rowData("RZ_TYB_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 13).Value = CDec(CInt(rowData("RZ_TYB_KOSU_RTO").ToString) / 100) '対比_比率

                '◯ 関西
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 14).Value = CInt(rowData("RZ_KNS_KOSU_TGT").ToString)                 '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 15).Value = CInt(rowData("RZ_KNS_KOSU_ZNN").ToString)                 '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 16).Value = CInt(rowData("RZ_KNS_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 16).Value = CDec(CInt(rowData("RZ_KNS_KOSU_RTO").ToString) / 100) '対比_比率

                '◯ 九州
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 17).Value = CInt(rowData("RZ_KSY_KOSU_TGT").ToString)                 '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 18).Value = CInt(rowData("RZ_KSY_KOSU_ZNN").ToString)                 '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 19).Value = CInt(rowData("RZ_KSY_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 19).Value = CDec(CInt(rowData("RZ_KSY_KOSU_RTO").ToString) / 100) '対比_比率

                '◯ 全国計
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 20).Value = CInt(rowData("RZ_TTL_KOSU_TGT").ToString)                 '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 21).Value = CInt(rowData("RZ_TTL_KOSU_ZNN").ToString)                 '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 22).Value = CInt(rowData("RZ_TTL_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 22).Value = CDec(CInt(rowData("RZ_TTL_KOSU_RTO").ToString) / 100) '対比_比率

                '【S-UR】****************************************************************************************************
                '◯ 北海道
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 2).Value = CInt(rowData("SR_HKD_KOSU_TGT").ToString)                 '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 3).Value = CInt(rowData("SR_HKD_KOSU_ZNN").ToString)                 '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 4).Value = CInt(rowData("SR_HKD_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 4).Value = CDec(CInt(rowData("SR_HKD_KOSU_RTO").ToString) / 100) '対比_比率

                '◯ 東北
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 5).Value = CInt(rowData("SR_THK_KOSU_TGT").ToString)                 '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 6).Value = CInt(rowData("SR_THK_KOSU_ZNN").ToString)                 '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 7).Value = CInt(rowData("SR_THK_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 7).Value = CDec(CInt(rowData("SR_THK_KOSU_RTO").ToString) / 100) '対比_比率

                '◯ 関東
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 8).Value = CInt(rowData("SR_KNT_KOSU_TGT").ToString)                  '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 9).Value = CInt(rowData("SR_KNT_KOSU_ZNN").ToString)                  '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 10).Value = CInt(rowData("SR_KNT_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 10).Value = CDec(CInt(rowData("SR_KNT_KOSU_RTO").ToString) / 100) '対比_比率

                '◯ 中部
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 11).Value = CInt(rowData("SR_TYB_KOSU_TGT").ToString)                 '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 12).Value = CInt(rowData("SR_TYB_KOSU_ZNN").ToString)                 '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 13).Value = CInt(rowData("SR_TYB_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 13).Value = CDec(CInt(rowData("SR_TYB_KOSU_RTO").ToString) / 100) '対比_比率

                '◯ 関西
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 14).Value = CInt(rowData("SR_KNS_KOSU_TGT").ToString)                 '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 15).Value = CInt(rowData("SR_KNS_KOSU_ZNN").ToString)                 '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 16).Value = CInt(rowData("SR_KNS_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 16).Value = CDec(CInt(rowData("SR_KNS_KOSU_RTO").ToString) / 100) '対比_比率

                '◯ 九州
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 17).Value = CInt(rowData("SR_KSY_KOSU_TGT").ToString)                 '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 18).Value = CInt(rowData("SR_KSY_KOSU_ZNN").ToString)                 '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 19).Value = CInt(rowData("SR_KSY_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 19).Value = CDec(CInt(rowData("SR_KSY_KOSU_RTO").ToString) / 100) '対比_比率

                '◯ 全国計
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 20).Value = CInt(rowData("SR_TTL_KOSU_TGT").ToString)                 '当月_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 21).Value = CInt(rowData("SR_TTL_KOSU_ZNN").ToString)                 '前年_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 22).Value = CInt(rowData("SR_TTL_KOSU_TIH").ToString)                 '対比_個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 22).Value = CDec(CInt(rowData("SR_TTL_KOSU_RTO").ToString) / 100) '対比_比率

                '冷蔵集計用変数に加算 ****************************************************************************************************
                WK_RZ_HKD_KOSU_TGT = WK_RZ_HKD_KOSU_TGT + CInt(rowData("RZ_HKD_KOSU_TGT").ToString)
                WK_RZ_HKD_KOSU_ZNN = WK_RZ_HKD_KOSU_ZNN + CInt(rowData("RZ_HKD_KOSU_ZNN").ToString)
                WK_RZ_HKD_KOSU_TIH = WK_RZ_HKD_KOSU_TIH + CInt(rowData("RZ_HKD_KOSU_TIH").ToString)
                WK_RZ_THK_KOSU_TGT = WK_RZ_THK_KOSU_TGT + CInt(rowData("RZ_THK_KOSU_TGT").ToString)
                WK_RZ_THK_KOSU_ZNN = WK_RZ_THK_KOSU_ZNN + CInt(rowData("RZ_THK_KOSU_ZNN").ToString)
                WK_RZ_THK_KOSU_TIH = WK_RZ_THK_KOSU_TIH + CInt(rowData("RZ_THK_KOSU_TIH").ToString)
                WK_RZ_KNT_KOSU_TGT = WK_RZ_KNT_KOSU_TGT + CInt(rowData("RZ_KNT_KOSU_TGT").ToString)
                WK_RZ_KNT_KOSU_ZNN = WK_RZ_KNT_KOSU_ZNN + CInt(rowData("RZ_KNT_KOSU_ZNN").ToString)
                WK_RZ_KNT_KOSU_TIH = WK_RZ_KNT_KOSU_TIH + CInt(rowData("RZ_KNT_KOSU_TIH").ToString)
                WK_RZ_TYB_KOSU_TGT = WK_RZ_TYB_KOSU_TGT + CInt(rowData("RZ_TYB_KOSU_TGT").ToString)
                WK_RZ_TYB_KOSU_ZNN = WK_RZ_TYB_KOSU_ZNN + CInt(rowData("RZ_TYB_KOSU_ZNN").ToString)
                WK_RZ_TYB_KOSU_TIH = WK_RZ_TYB_KOSU_TIH + CInt(rowData("RZ_TYB_KOSU_TIH").ToString)
                WK_RZ_KNS_KOSU_TGT = WK_RZ_KNS_KOSU_TGT + CInt(rowData("RZ_KNS_KOSU_TGT").ToString)
                WK_RZ_KNS_KOSU_ZNN = WK_RZ_KNS_KOSU_ZNN + CInt(rowData("RZ_KNS_KOSU_ZNN").ToString)
                WK_RZ_KNS_KOSU_TIH = WK_RZ_KNS_KOSU_TIH + CInt(rowData("RZ_KNS_KOSU_TIH").ToString)
                WK_RZ_KSY_KOSU_TGT = WK_RZ_KSY_KOSU_TGT + CInt(rowData("RZ_KSY_KOSU_TGT").ToString)
                WK_RZ_KSY_KOSU_ZNN = WK_RZ_KSY_KOSU_ZNN + CInt(rowData("RZ_KSY_KOSU_ZNN").ToString)
                WK_RZ_KSY_KOSU_TIH = WK_RZ_KSY_KOSU_TIH + CInt(rowData("RZ_KSY_KOSU_TIH").ToString)
                WK_RZ_TTL_KOSU_TGT = WK_RZ_TTL_KOSU_TGT + CInt(rowData("RZ_TTL_KOSU_TGT").ToString)
                WK_RZ_TTL_KOSU_ZNN = WK_RZ_TTL_KOSU_ZNN + CInt(rowData("RZ_TTL_KOSU_ZNN").ToString)
                WK_RZ_TTL_KOSU_TIH = WK_RZ_TTL_KOSU_TIH + CInt(rowData("RZ_TTL_KOSU_TIH").ToString)

                'S-UR集計用変数に加算 ****************************************************************************************************
                WK_SR_HKD_KOSU_TGT = WK_SR_HKD_KOSU_TGT + CInt(rowData("SR_HKD_KOSU_TGT").ToString)
                WK_SR_HKD_KOSU_ZNN = WK_SR_HKD_KOSU_ZNN + CInt(rowData("SR_HKD_KOSU_ZNN").ToString)
                WK_SR_HKD_KOSU_TIH = WK_SR_HKD_KOSU_TIH + CInt(rowData("SR_HKD_KOSU_TIH").ToString)
                WK_SR_THK_KOSU_TGT = WK_SR_THK_KOSU_TGT + CInt(rowData("SR_THK_KOSU_TGT").ToString)
                WK_SR_THK_KOSU_ZNN = WK_SR_THK_KOSU_ZNN + CInt(rowData("SR_THK_KOSU_ZNN").ToString)
                WK_SR_THK_KOSU_TIH = WK_SR_THK_KOSU_TIH + CInt(rowData("SR_THK_KOSU_TIH").ToString)
                WK_SR_KNT_KOSU_TGT = WK_SR_KNT_KOSU_TGT + CInt(rowData("SR_KNT_KOSU_TGT").ToString)
                WK_SR_KNT_KOSU_ZNN = WK_SR_KNT_KOSU_ZNN + CInt(rowData("SR_KNT_KOSU_ZNN").ToString)
                WK_SR_KNT_KOSU_TIH = WK_SR_KNT_KOSU_TIH + CInt(rowData("SR_KNT_KOSU_TIH").ToString)
                WK_SR_TYB_KOSU_TGT = WK_SR_TYB_KOSU_TGT + CInt(rowData("SR_TYB_KOSU_TGT").ToString)
                WK_SR_TYB_KOSU_ZNN = WK_SR_TYB_KOSU_ZNN + CInt(rowData("SR_TYB_KOSU_ZNN").ToString)
                WK_SR_TYB_KOSU_TIH = WK_SR_TYB_KOSU_TIH + CInt(rowData("SR_TYB_KOSU_TIH").ToString)
                WK_SR_KNS_KOSU_TGT = WK_SR_KNS_KOSU_TGT + CInt(rowData("SR_KNS_KOSU_TGT").ToString)
                WK_SR_KNS_KOSU_ZNN = WK_SR_KNS_KOSU_ZNN + CInt(rowData("SR_KNS_KOSU_ZNN").ToString)
                WK_SR_KNS_KOSU_TIH = WK_SR_KNS_KOSU_TIH + CInt(rowData("SR_KNS_KOSU_TIH").ToString)
                WK_SR_KSY_KOSU_TGT = WK_SR_KSY_KOSU_TGT + CInt(rowData("SR_KSY_KOSU_TGT").ToString)
                WK_SR_KSY_KOSU_ZNN = WK_SR_KSY_KOSU_ZNN + CInt(rowData("SR_KSY_KOSU_ZNN").ToString)
                WK_SR_KSY_KOSU_TIH = WK_SR_KSY_KOSU_TIH + CInt(rowData("SR_KSY_KOSU_TIH").ToString)
                WK_SR_TTL_KOSU_TGT = WK_SR_TTL_KOSU_TGT + CInt(rowData("SR_TTL_KOSU_TGT").ToString)
                WK_SR_TTL_KOSU_ZNN = WK_SR_TTL_KOSU_ZNN + CInt(rowData("SR_TTL_KOSU_ZNN").ToString)
                WK_SR_TTL_KOSU_TIH = WK_SR_TTL_KOSU_TIH + CInt(rowData("SR_TTL_KOSU_TIH").ToString)
            Next

            ReizouRow = 54
            SuperurRow = 81

            '〇 明細設定処理
            '【冷蔵】****************************************************************************************************
            '◯ 北海道
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 2).Value = CInt(WK_RZ_HKD_KOSU_TGT.ToString)                                               '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 3).Value = CInt(WK_RZ_HKD_KOSU_ZNN.ToString)                                               '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 4).Value = CInt(WK_RZ_HKD_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_RZ_HKD_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_RZ_HKD_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 4).Value = CDec(CInt(WK_RZ_HKD_KOSU_TGT.ToString) / CInt(WK_RZ_HKD_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 4).Value = 0
            End If

            '◯ 東北
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 5).Value = CInt(WK_RZ_THK_KOSU_TGT.ToString)                                               '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 6).Value = CInt(WK_RZ_THK_KOSU_ZNN.ToString)                                               '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 7).Value = CInt(WK_RZ_THK_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_RZ_THK_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_RZ_THK_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 7).Value = CDec(CInt(WK_RZ_THK_KOSU_TGT.ToString) / CInt(WK_RZ_THK_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 7).Value = 0
            End If

            '◯ 関東
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 8).Value = CInt(WK_RZ_KNT_KOSU_TGT.ToString)                                                '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 9).Value = CInt(WK_RZ_KNT_KOSU_ZNN.ToString)                                                '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 10).Value = CInt(WK_RZ_KNT_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_RZ_KNT_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_RZ_KNT_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 10).Value = CDec(CInt(WK_RZ_KNT_KOSU_TGT.ToString) / CInt(WK_RZ_KNT_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 10).Value = 0
            End If

            '◯ 中部
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 11).Value = CInt(WK_RZ_TYB_KOSU_TGT.ToString)                                               '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 12).Value = CInt(WK_RZ_TYB_KOSU_ZNN.ToString)                                               '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 13).Value = CInt(WK_RZ_TYB_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_RZ_TYB_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_RZ_TYB_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 13).Value = CDec(CInt(WK_RZ_TYB_KOSU_TGT.ToString) / CInt(WK_RZ_TYB_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 13).Value = 0
            End If

            '◯ 関西
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 14).Value = CInt(WK_RZ_KNS_KOSU_TGT.ToString)                                               '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 15).Value = CInt(WK_RZ_KNS_KOSU_ZNN.ToString)                                               '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 16).Value = CInt(WK_RZ_KNS_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_RZ_KNS_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_RZ_KNS_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 16).Value = CDec(CInt(WK_RZ_KNS_KOSU_TGT.ToString) / CInt(WK_RZ_KNS_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 16).Value = 0
            End If

            '◯ 九州
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 17).Value = CInt(WK_RZ_KSY_KOSU_TGT.ToString)                                               '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 18).Value = CInt(WK_RZ_KSY_KOSU_ZNN.ToString)                                               '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 19).Value = CInt(WK_RZ_KSY_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_RZ_KSY_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_RZ_KSY_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 19).Value = CDec(CInt(WK_RZ_KSY_KOSU_TGT.ToString) / CInt(WK_RZ_KSY_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 19).Value = 0
            End If

            '◯ 全国計
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 20).Value = CInt(WK_RZ_TTL_KOSU_TGT.ToString)                                               '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 21).Value = CInt(WK_RZ_TTL_KOSU_ZNN.ToString)                                               '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow, 22).Value = CInt(WK_RZ_TTL_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_RZ_TTL_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_RZ_TTL_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 22).Value = CDec(CInt(WK_RZ_TTL_KOSU_TGT.ToString) / CInt(WK_RZ_TTL_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(ReizouRow + 1, 22).Value = 0
            End If

            '【S-UR】****************************************************************************************************
            '◯ 北海道
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 2).Value = CInt(WK_SR_HKD_KOSU_TGT.ToString)                                               '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 3).Value = CInt(WK_SR_HKD_KOSU_ZNN.ToString)                                               '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 4).Value = CInt(WK_SR_HKD_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_SR_HKD_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_SR_HKD_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 4).Value = CDec(CInt(WK_SR_HKD_KOSU_TGT.ToString) / CInt(WK_SR_HKD_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 4).Value = 0
            End If

            '◯ 東北
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 5).Value = CInt(WK_SR_THK_KOSU_TGT.ToString)                                               '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 6).Value = CInt(WK_SR_THK_KOSU_ZNN.ToString)                                               '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 7).Value = CInt(WK_SR_THK_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_SR_THK_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_SR_THK_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 7).Value = CDec(CInt(WK_SR_THK_KOSU_TGT.ToString) / CInt(WK_SR_THK_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 7).Value = 0
            End If

            '◯ 関東
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 8).Value = CInt(WK_SR_KNT_KOSU_TGT.ToString)                                                '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 9).Value = CInt(WK_SR_KNT_KOSU_ZNN.ToString)                                                '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 10).Value = CInt(WK_SR_KNT_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_SR_KNT_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_SR_KNT_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 10).Value = CDec(CInt(WK_SR_KNT_KOSU_TGT.ToString) / CInt(WK_SR_KNT_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 10).Value = 0
            End If

            '◯ 中部
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 11).Value = CInt(WK_SR_TYB_KOSU_TGT.ToString)                                               '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 12).Value = CInt(WK_SR_TYB_KOSU_ZNN.ToString)                                               '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 13).Value = CInt(WK_SR_TYB_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_SR_TYB_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_SR_TYB_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 13).Value = CDec(CInt(WK_SR_TYB_KOSU_TGT.ToString) / CInt(WK_SR_TYB_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 13).Value = 0
            End If

            '◯ 関西
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 14).Value = CInt(WK_SR_KNS_KOSU_TGT.ToString)                                               '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 15).Value = CInt(WK_SR_KNS_KOSU_ZNN.ToString)                                               '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 16).Value = CInt(WK_SR_KNS_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_SR_KNS_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_SR_KNS_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 16).Value = CDec(CInt(WK_SR_KNS_KOSU_TGT.ToString) / CInt(WK_SR_KNS_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 16).Value = 0
            End If

            '◯ 九州
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 17).Value = CInt(WK_SR_KSY_KOSU_TGT.ToString)                                               '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 18).Value = CInt(WK_SR_KSY_KOSU_ZNN.ToString)                                               '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 19).Value = CInt(WK_SR_KSY_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_SR_KSY_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_SR_KSY_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 19).Value = CDec(CInt(WK_SR_KSY_KOSU_TGT.ToString) / CInt(WK_SR_KSY_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 19).Value = 0
            End If

            '◯ 全国計
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 20).Value = CInt(WK_SR_TTL_KOSU_TGT.ToString)                                               '当月_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 21).Value = CInt(WK_SR_TTL_KOSU_ZNN.ToString)                                               '前年_個数
            WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow, 22).Value = CInt(WK_SR_TTL_KOSU_TIH.ToString)                                               '対比_個数
            If CInt(WK_SR_TTL_KOSU_TGT.ToString) <> 0 AndAlso CInt(WK_SR_TTL_KOSU_ZNN.ToString) <> 0 Then
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 22).Value = CDec(CInt(WK_SR_TTL_KOSU_TGT.ToString) / CInt(WK_SR_TTL_KOSU_ZNN.ToString)) - 1 '対比_比率
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Cells(SuperurRow + 1, 22).Value = 0
            End If
        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 帳票の明細設定(差引収支明細)
    ''' </summary>
    Private Sub EditDetailSshkShushi()

        Try
            Dim RowNumber As Integer = 0
            For Each rowData As DataRow In SshkShushiDataDtl.Rows

                Select Case rowData("KEISHIKIGROUPCD").ToString
                    Case "1"        '冷蔵
                        RowNumber = 34
                    Case "2"        'S-UR
                        RowNumber = 36
                    Case "6"        'L10t
                        RowNumber = 38
                    Case "9"        '計
                        RowNumber = 42
                    Case Else       'その他
                        RowNumber = 40
                End Select

                '明細設定処理
                WW_Workbook.Worksheets(WW_SheetNo).Cells(RowNumber, 26).Value = rowData("SHUSHI_KINGAKU").ToString & " / " & rowData("SHUSHI_RATIO").ToString & "％"
            Next

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' 帳票の明細設定(冷蔵運用個数明細)
    ''' </summary>
    Private Sub EditDetailReizouUnyoKosu()

        Try
            Dim TotalCurrentRowNumber As Integer = 0 ' 2024/05/16 CHG
            Dim AnchorageRowNumber As Integer = 0    ' 2024/05/16 ADD
            Dim NiigataFlg As Integer = 0            ' 2024/06/06 ADD
            For Each rowData As DataRow In ReizouKosuDataDtl.Rows

                Select Case rowData("ORGCODE").ToString
                    Case "010102"      '北海道
                        TotalCurrentRowNumber = 36 ' 2024/05/16 CHG
                        AnchorageRowNumber = 61    ' 2024/05/16 ADD
                    Case "010401"      '東北
                        TotalCurrentRowNumber = 38 ' 2024/05/16 CHG
                        AnchorageRowNumber = 63    ' 2024/05/16 ADD
                    Case "011402"      '関東
                        TotalCurrentRowNumber = 40 ' 2024/05/16 CHG
                        AnchorageRowNumber = 65    ' 2024/05/16 ADD
                    Case "011501"      '内：新潟
                        TotalCurrentRowNumber = 42 ' 2024/05/16 CHG
                        AnchorageRowNumber = 67    ' 2024/05/16 ADD
                        NiigataFlg = 1             ' 2024/06/06 ADD
                    Case "012401"      '中部
                        TotalCurrentRowNumber = 44 ' 2024/05/16 CHG
                        AnchorageRowNumber = 69    ' 2024/05/16 ADD
                    Case "012701"      '関西
                        TotalCurrentRowNumber = 46 ' 2024/05/16 CHG
                        AnchorageRowNumber = 71    ' 2024/05/16 ADD
                    Case "014001"      '九州
                        TotalCurrentRowNumber = 48 ' 2024/05/16 CHG
                        AnchorageRowNumber = 73    ' 2024/05/16 ADD
                    Case "ALL"         '全国計
                        TotalCurrentRowNumber = 50 ' 2024/05/16 CHG
                        AnchorageRowNumber = 75    ' 2024/05/16 ADD
                    Case Else                      ' 2024/06/06 ADD
                        Continue For               ' 2024/06/06 ADD
                End Select

                '明細設定処理
                '【冷蔵運用個数　総現個数】
                ' 総現個数 適正個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(TotalCurrentRowNumber, 25).Value = CInt(rowData("PROPER_NUM").ToString) ' 2024/05/16 CHG
                ' 総現個数 当日個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(TotalCurrentRowNumber, 26).Value = CInt(rowData("TOJ_NUM").ToString)    ' 2024/05/16 CHG
                ' 総現個数 対比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(TotalCurrentRowNumber, 27).Value = CDec(rowData("TOJ_RATE").ToString)   ' 2024/05/16 CHG
                ' 総現個数 前日個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(TotalCurrentRowNumber, 28).Value = CInt(rowData("ZNJ_NUM").ToString)    ' 2024/05/16 CHG
                ' 総現個数 前日比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(TotalCurrentRowNumber, 29).Value = CDec(rowData("ZNJ_RATE").ToString)   ' 2024/05/16 CHG
                ' 総現個数 前年個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(TotalCurrentRowNumber, 30).Value = CInt(rowData("ZNN_NUM").ToString)    ' 2024/05/16 CHG
                ' 総現個数 前年同月比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(TotalCurrentRowNumber, 31).Value = CDec(rowData("ZNN_RATE").ToString)   ' 2024/05/16 CHG

                '【冷蔵運用個数　停泊個数】
                ' 停泊個数 停泊個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(AnchorageRowNumber, 25).Value = CInt(rowData("ANCHORAGE_NUM").ToString)            ' 2024/05/16 CHG
                ' 停泊個数 内：修理
                WW_Workbook.Worksheets(WW_SheetNo).Cells(AnchorageRowNumber, 26).Value = CInt(rowData("ANCHORAGE_REPAIRNUM").ToString)      ' 2024/05/16 ADD
                ' 停泊個数 修理除く
                WW_Workbook.Worksheets(WW_SheetNo).Cells(AnchorageRowNumber, 27).Value = CInt(rowData("ANCHORAGE_NORMALONLYNUM").ToString)  ' 2024/05/16 ADD
                ' 停泊個数 停泊10以上
                WW_Workbook.Worksheets(WW_SheetNo).Cells(AnchorageRowNumber, 28).Value = CInt(rowData("ANCHORAGE_TENDAYSNUM").ToString)     ' 2024/05/16 ADD
                ' 停泊個数 在庫日数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(AnchorageRowNumber, 29).Value = CDec(rowData("ANCHORAGE_DAY").ToString)            ' 2024/05/16 CHG
                ' 停泊個数 長停率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(AnchorageRowNumber, 30).Value = CDec(rowData("ANCHORAGE_RATE").ToString)           ' 2024/05/16 CHG
                ' 停泊個数 特留
                WW_Workbook.Worksheets(WW_SheetNo).Cells(AnchorageRowNumber, 31).Value = CInt(rowData("ANCHORAGE_SPDETENTIONNUM").ToString) ' 2024/05/16 ADD
                ' 運用効率 当日
                WW_Workbook.Worksheets(WW_SheetNo).Cells(AnchorageRowNumber, 32).Value = CDec(rowData("OPE_EFF_TOJ_RATE").ToString)         ' 2024/05/16 CHG
                ' 運用効率 累計                                                                                                             
                WW_Workbook.Worksheets(WW_SheetNo).Cells(AnchorageRowNumber, 33).Value = CDec(rowData("OPE_EFF_TOTAL_RATE").ToString)       ' 2024/05/16 CHG
            Next

            ' 2024/06/06 ADD START
            If NiigataFlg = 0 Then

                '明細設定処理
                '【冷蔵運用個数　総現個数】
                ' 総現個数 適正個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(42, 25).Value = 0
                ' 総現個数 当日個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(42, 26).Value = 0
                ' 総現個数 対比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(42, 27).Value = 0
                ' 総現個数 前日個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(42, 28).Value = 0
                ' 総現個数 前日比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(42, 29).Value = 0
                ' 総現個数 前年個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(42, 30).Value = 0
                ' 総現個数 前年同月比
                WW_Workbook.Worksheets(WW_SheetNo).Cells(42, 31).Value = 0

                '【冷蔵運用個数　停泊個数】
                ' 停泊個数 停泊個数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(67, 25).Value = 0
                ' 停泊個数 内：修理
                WW_Workbook.Worksheets(WW_SheetNo).Cells(67, 26).Value = 0
                ' 停泊個数 修理除く
                WW_Workbook.Worksheets(WW_SheetNo).Cells(67, 27).Value = 0
                ' 停泊個数 停泊10以上
                WW_Workbook.Worksheets(WW_SheetNo).Cells(67, 28).Value = 0
                ' 停泊個数 在庫日数
                WW_Workbook.Worksheets(WW_SheetNo).Cells(67, 29).Value = 0
                ' 停泊個数 長停率
                WW_Workbook.Worksheets(WW_SheetNo).Cells(67, 30).Value = 0
                ' 停泊個数 特留
                WW_Workbook.Worksheets(WW_SheetNo).Cells(67, 31).Value = 0
                ' 運用効率 当日
                WW_Workbook.Worksheets(WW_SheetNo).Cells(67, 32).Value = 0
                ' 運用効率 累計
                WW_Workbook.Worksheets(WW_SheetNo).Cells(67, 33).Value = 0
            End If
            ' 2024/06/06 ADD END

        Catch ex As Exception
            Throw
        Finally
        End Try

    End Sub

    ''' <summary>
    ''' ヘッダー取得処理
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetHeaderDataTbl(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt = New DataTable
        Dim WW_DATENOW As Date = Date.Now

        Dim SQLBldr As New StringBuilder
        '稼働・非稼働日数取得
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    SUM(A01.TOU_WORKINGDAY) AS TOU_WORKINGDAY")                                     '当月_稼働日数
        SQLBldr.AppendLine("    , SUM(A01.ZEN_WORKINGDAY) AS ZEN_WORKINGDAY")                                   '前年_稼働日数
        SQLBldr.AppendLine("    , SUM(A01.TOU_WORKINGSUM) AS TOU_WORKINGSUM")                                   '当月_稼働総日数
        SQLBldr.AppendLine("    , SUM(A01.ZEN_WORKINGSUM) AS ZEN_WORKINGSUM")                                   '前年_稼働総日数
        SQLBldr.AppendLine("FROM")
        '当日分の稼働日数
        SQLBldr.AppendLine("    (")
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            SUM(CASE WORKINGKBN WHEN '0' THEN 1 ELSE 0 END) AS TOU_WORKINGDAY")     '当月_稼働日数
        SQLBldr.AppendLine("            , 0 AS ZEN_WORKINGDAY")                                                 '前年_稼働日数
        SQLBldr.AppendLine("            , 0 AS TOU_WORKINGSUM")                                                 '当月_稼働総日数
        SQLBldr.AppendLine("            , 0 AS ZEN_WORKINGSUM")                                                 '前年_稼働総日数
        SQLBldr.AppendLine("        FROM")
        'メイン カレンダーマスタ
        SQLBldr.AppendLine("            com.LNS0021_CALENDAR")
        '抽出条件
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            CAMPCODE = @P02")
        SQLBldr.AppendLine("            AND WORKINGYMD <= '" & WW_KeyYMD & "'")
        SQLBldr.AppendLine("            AND WORKINGYMD >= FORMAT(CONVERT(DATE, '" & WW_KeyYMD & "'), 'yyyy/MM/01')")
        SQLBldr.AppendLine("            AND WORKINGKBN = '0'")
        SQLBldr.AppendLine("            AND CALENDARKBN = '00'")
        SQLBldr.AppendLine("            AND DELFLG = @P01")
        SQLBldr.AppendLine("        UNION ALL")
        '前年分の稼働日数
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            0 AS TOU_WORKINGDAY")                                                   '当月_稼働日数
        SQLBldr.AppendLine("            , SUM(CASE WORKINGKBN WHEN '0' THEN 1 ELSE 0 END) AS ZEN_WORKINGDAY")   '前年_稼働日数
        SQLBldr.AppendLine("            , 0 AS TOU_WORKINGSUM")                                                 '当月_稼働総日数
        SQLBldr.AppendLine("            , 0 AS ZEN_WORKINGSUM")                                                 '前年_稼働総日数
        SQLBldr.AppendLine("        FROM")
        'メイン カレンダーマスタ
        SQLBldr.AppendLine("            com.LNS0021_CALENDAR")
        '抽出条件
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            CAMPCODE = @P02")
        SQLBldr.AppendLine("            AND WORKINGYMD <= FORMAT(DATEADD(YEAR,-1,CONVERT(DATE, '" & WW_KeyYMD & "')), 'yyyy/MM/dd')")
        SQLBldr.AppendLine("            AND WORKINGYMD >= FORMAT(DATEADD(YEAR,-1,CONVERT(DATE, '" & WW_KeyYMD & "')), 'yyyy/MM/01')")
        SQLBldr.AppendLine("            AND WORKINGKBN = '0'")
        SQLBldr.AppendLine("            AND CALENDARKBN = '00'")
        SQLBldr.AppendLine("            AND DELFLG = @P01")
        SQLBldr.AppendLine("        UNION ALL")
        '当月分の稼働総日数
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            0 AS TOU_WORKINGDAY")                                                   '当月_稼働日数
        SQLBldr.AppendLine("            , 0 AS ZEN_WORKINGDAY")                                                 '前年_稼働日数
        SQLBldr.AppendLine("            , COUNT(WORKINGYMD) AS TOU_WORKINGSUM")                                 '当月_稼働総日数
        SQLBldr.AppendLine("            , 0 AS ZEN_WORKINGSUM")                                                 '前年_稼働総日数
        SQLBldr.AppendLine("        FROM")
        'メイン カレンダーマスタ
        SQLBldr.AppendLine("            com.LNS0021_CALENDAR")
        '抽出条件
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            CAMPCODE = @P02")
        SQLBldr.AppendLine("            AND WORKINGYMD >= FORMAT(CONVERT(DATE, '" & WW_KeyYMD & "'), 'yyyy/MM/01')")
        SQLBldr.AppendLine("            AND WORKINGYMD <= DATEADD(DAY,-1,CONVERT(DATE, FORMAT(DATEADD(MONTH,1,CONVERT(DATE, '" & WW_KeyYMD & "')), 'yyyy/MM/01')))")
        SQLBldr.AppendLine("            AND WORKINGKBN = '0'")
        SQLBldr.AppendLine("            AND CALENDARKBN = '00'")
        SQLBldr.AppendLine("            AND DELFLG = @P01")
        SQLBldr.AppendLine("        UNION ALL")
        '前月分の稼働総日数
        SQLBldr.AppendLine("        SELECT")
        SQLBldr.AppendLine("            0 AS TOU_WORKINGDAY")                                                   '当月_稼働日数
        SQLBldr.AppendLine("            , 0 AS ZEN_WORKINGDAY")                                                 '前年_稼働日数
        SQLBldr.AppendLine("            , 0 AS TOU_WORKINGSUM")                                                 '当月_稼働総日数
        SQLBldr.AppendLine("            , COUNT(WORKINGYMD) AS ZEN_WORKINGSUM")                                 '前年_稼働総日数
        SQLBldr.AppendLine("        FROM")
        'メイン カレンダーマスタ
        SQLBldr.AppendLine("            com.LNS0021_CALENDAR")
        '抽出条件
        SQLBldr.AppendLine("        WHERE")
        SQLBldr.AppendLine("            CAMPCODE = @P02")
        SQLBldr.AppendLine("            AND WORKINGYMD >= FORMAT(DATEADD(YEAR,-1,CONVERT(DATE, '" & WW_KeyYMD & "')), 'yyyy/MM/01')")
        SQLBldr.AppendLine("            AND WORKINGYMD <= DATEADD(DAY,-1,CONVERT(DATE, FORMAT(DATEADD(MONTH,1,DATEADD(YEAR,-1,CONVERT(DATE, '" & WW_KeyYMD & "'))), 'yyyy/MM/01')))")
        SQLBldr.AppendLine("            AND WORKINGKBN = '0'")
        SQLBldr.AppendLine("            AND CALENDARKBN = '00'")
        SQLBldr.AppendLine("            AND DELFLG = @P01")
        SQLBldr.AppendLine("    ) A01")
        Try
            Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)  '削除フラグ
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)  '会社コード

                PARA01.Value = C_DELETE_FLG.ALIVE
                PARA02.Value = WW_CampCode

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
            Dim a As String = ex.ToString()
            Throw
        End Try

        Return dt

    End Function

    ''' <summary>
    ''' (形式別)当日・前年実績予算対比明細取得処理
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetKeishikiDataTbl(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt = New DataTable
        Dim WW_DATENOW As Date = Date.Now

        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    D01.KEISHIKIGROUPCD")                                                                                               '形式グループコード
        '収入データ
        SQLBldr.AppendLine("    , D01.SN_TOJ_KOSU AS SN_TOJ_KOSU")                                                                                      '[収入]個別(当日実績)
        SQLBldr.AppendLine("    , D01.SN_TOJ_SHUNYU AS SN_TOJ_SHUNYU")                                                                                  '[収入]収入(当日実績)
        SQLBldr.AppendLine("    , D01.SN_TOJ_TANKA AS SN_TOJ_TANKA")                                                                                    '[収入]単価(当日実績)
        SQLBldr.AppendLine("    , D01.SN_TOY_KOSU AS SN_TOY_KOSU")                                                                                      '[収入]個数(当日予算)
        SQLBldr.AppendLine("    , D01.SN_TOY_SHUNYU AS SN_TOY_SHUNYU")                                                                                  '[収入]収入(当日予算)
        SQLBldr.AppendLine("    , D01.SN_TOY_TANKA AS SN_TOY_TANKA")                                                                                    '[収入]単価(当日予算)
        SQLBldr.AppendLine("    , D01.SN_TOJ_KOSU - D01.SN_TOY_KOSU AS SN_TORTO_KOSU")                                                                  '[収入]個数(当日予算対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.SN_TOY_KOSU <> 0 THEN D01.SN_TOJ_KOSU / D01.SN_TOY_KOSU - 1 ELSE 0 END AS SN_TORTO_KOSURATIO")          '[収入]個数比率(当日予算対比)
        SQLBldr.AppendLine("    , D01.SN_TOJ_SHUNYU - D01.SN_TOY_SHUNYU AS SN_TORTO_SHUNYU")                                                            '[収入]収入(当日予算対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.SN_TOY_SHUNYU <> 0 THEN D01.SN_TOJ_SHUNYU / D01.SN_TOY_SHUNYU - 1 ELSE 0 END AS SN_TORTO_SHUNYURATIO")  '[収入]収入比率(当日予算対比)
        SQLBldr.AppendLine("    , D01.SN_TOJ_TANKA - D01.SN_TOY_TANKA AS SN_TORTO_TANKA")                                                               '[収入]単価(当時予算対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.SN_TOY_TANKA <> 0 THEN D01.SN_TOJ_TANKA / D01.SN_TOY_TANKA - 1 ELSE 0 END AS SN_TORTO_TANKARATIO")      '[収入]単価比率(当日予算対比)
        SQLBldr.AppendLine("    , D01.SN_TRJ_KOSU AS SN_TRJ_KOSU")                                                                                      '[収入]個数(当月累計実績)
        SQLBldr.AppendLine("    , D01.SN_TRJ_SHUNYU AS SN_TRJ_SHUNYU")                                                                                  '[収入]収入(当月累計実績)
        SQLBldr.AppendLine("    , D01.SN_TRJ_TANKA AS SN_TRJ_TANKA")                                                                                    '[収入]単価(当月累計実績)
        SQLBldr.AppendLine("    , D01.SN_TRY_KOSU AS SN_TRY_KOSU")                                                                                      '[収入]個数(当月累計予算)
        SQLBldr.AppendLine("    , D01.SN_TRY_SHUNYU AS SN_TRY_SHUNYU")                                                                                  '[収入]収入(当月累計予算)
        SQLBldr.AppendLine("    , D01.SN_TRY_TANKA AS SN_TRY_TANKA")                                                                                    '[収入]単価(当月累計予算)
        SQLBldr.AppendLine("    , D01.SN_TRJ_KOSU - D01.SN_TRY_KOSU AS SN_TGRTO_KOSU")                                                                  '[収入]個数(当月予算対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.SN_TRY_KOSU <> 0 THEN D01.SN_TRJ_KOSU / D01.SN_TRY_KOSU - 1 ELSE 0 END AS SN_TGRTO_KOSURATIO")          '[収入]個数比率(当月予算対比)
        SQLBldr.AppendLine("    , D01.SN_TRJ_SHUNYU - D01.SN_TRY_SHUNYU AS SN_TGRTO_SHUNYU")                                                            '[収入]収入(当月予算対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.SN_TRY_SHUNYU <> 0 THEN D01.SN_TRJ_SHUNYU / D01.SN_TRY_SHUNYU - 1 ELSE 0 END AS SN_TGRTO_SHUNYURATIO")  '[収入]収入比率(当月予算対比)
        SQLBldr.AppendLine("    , D01.SN_TRJ_TANKA - D01.SN_TRY_TANKA AS SN_TGRTO_TANKA")                                                               '[収入]単価(当月予算対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.SN_TRY_TANKA <> 0 THEN D01.SN_TRJ_TANKA / D01.SN_TRY_TANKA - 1 ELSE 0 END AS SN_TGRTO_TANKARATIO")      '[収入]単価比率(当月予算対比)
        SQLBldr.AppendLine("    , D01.SN_ZNJ_KOSU AS SN_ZNJ_KOSU")                                                                                      '[収入]個数(前年単日実績)
        SQLBldr.AppendLine("    , D01.SN_ZNJ_SHUNYU AS SN_ZNJ_SHUNYU")                                                                                  '[収入]収入(前年単日実績)
        SQLBldr.AppendLine("    , D01.SN_ZNJ_TANKA AS SN_ZNJ_TANKA")                                                                                    '[収入]単価(前年単日実績)
        SQLBldr.AppendLine("    , D01.SN_TOJ_KOSU - D01.SN_ZNJ_KOSU AS SN_ZNRTO_KOSU")                                                                  '[収入]個数(前年単日対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.SN_ZNJ_KOSU <> 0 THEN D01.SN_TOJ_KOSU / D01.SN_ZNJ_KOSU - 1 ELSE 0 END AS SN_ZNRTO_KOSURATIO")          '[収入]個数比率(前年単日対比)
        SQLBldr.AppendLine("    , D01.SN_TOJ_SHUNYU - D01.SN_ZNJ_SHUNYU AS SN_ZNRTO_SHUNYU")                                                            '[収入]収入(前年単日対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.SN_ZNJ_SHUNYU <> 0 THEN D01.SN_TOJ_SHUNYU / D01.SN_ZNJ_SHUNYU - 1 ELSE 0 END AS SN_ZNRTO_SHUNYURATIO")  '[収入]収入比率(前年単日対比)
        SQLBldr.AppendLine("    , D01.SN_TOJ_TANKA - D01.SN_ZNJ_TANKA AS SN_ZNRTO_TANKA")                                                               '[収入]単価(前年単日対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.SN_ZNJ_TANKA <> 0 THEN D01.SN_TOJ_TANKA / D01.SN_ZNJ_TANKA - 1 ELSE 0 END AS SN_ZNRTO_TANKARATIO")      '[収入]単価比率(前年単日対比)
        SQLBldr.AppendLine("    , D01.SN_ZRJ_KOSU AS SN_ZRJ_KOSU")                                                                                      '[収入]個数(前年累計実績)
        SQLBldr.AppendLine("    , D01.SN_ZRJ_SHUNYU AS SN_ZRJ_SHUNYU")                                                                                  '[収入]収入(前年累計実績)
        SQLBldr.AppendLine("    , D01.SN_ZRJ_TANKA AS SN_ZRJ_TANKA")                                                                                    '[収入]単価(前年累計実績)
        SQLBldr.AppendLine("    , D01.SN_MON_ZRJ_KOSU AS SN_MON_ZRJ_KOSU")                                                                              '[収入]個数(前年月間累計実績)
        SQLBldr.AppendLine("    , D01.SN_MON_ZRJ_SHUNYU AS SN_MON_ZRJ_SHUNYU")                                                                          '[収入]収入(前年月間累計実績)
        SQLBldr.AppendLine("    , D01.SN_MON_ZRJ_TANKA AS SN_MON_ZRJ_TANKA")                                                                            '[収入]単価(前年月間累計実績)
        SQLBldr.AppendLine("    , D01.SN_TRJ_KOSU - D01.SN_ZRJ_KOSU AS SN_ZRRTO_KOSU")                                                                  '[収入]個数(前年累計対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.SN_ZRJ_KOSU <> 0 THEN D01.SN_TRJ_KOSU / D01.SN_ZRJ_KOSU - 1 ELSE 0 END AS SN_ZRRTO_KOSURATIO")          '[収入]個数比率(前年累計対比)
        SQLBldr.AppendLine("    , D01.SN_TRJ_SHUNYU - D01.SN_ZRJ_SHUNYU AS SN_ZRRTO_SHUNYU")                                                            '[収入]収入(前年累計対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.SN_ZRJ_SHUNYU <> 0 THEN D01.SN_TRJ_SHUNYU / D01.SN_ZRJ_SHUNYU - 1 ELSE 0 END AS SN_ZRRTO_SHUNYURATIO")  '[収入]収入比率(前年累計対比)
        SQLBldr.AppendLine("    , D01.SN_TRJ_TANKA - D01.SN_ZRJ_TANKA AS SN_ZRRTO_TANKA")                                                               '[収入]単価(前年累計対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.SN_ZRJ_TANKA <> 0 THEN D01.SN_TRJ_TANKA / D01.SN_ZRJ_TANKA - 1 ELSE 0 END AS SN_ZRRTO_TANKARATIO")      '[収入]単価比率(前年累計対比)
        '回送運賃データ
        SQLBldr.AppendLine("    , D01.KS_TOJ_KOSU AS KS_TOJ_KOSU")                                                                                      '[回送運賃]個別(当日実績)
        SQLBldr.AppendLine("    , D01.KS_TOJ_SHUNYU AS KS_TOJ_SHUNYU")                                                                                  '[回送運賃]収入(当日実績)
        SQLBldr.AppendLine("    , D01.KS_TOJ_TANKA AS KS_TOJ_TANKA")                                                                                    '[回送運賃]単価(当日実績)
        SQLBldr.AppendLine("    , D01.KS_TOY_KOSU AS KS_TOY_KOSU")                                                                                      '[回送運賃]個数(当日予算)
        SQLBldr.AppendLine("    , D01.KS_TOY_SHUNYU AS KS_TOY_SHUNYU")                                                                                  '[回送運賃]収入(当日予算)
        SQLBldr.AppendLine("    , D01.KS_TOY_TANKA AS KS_TOY_TANKA")                                                                                    '[回送運賃]単価(当日予算)
        SQLBldr.AppendLine("    , D01.KS_TOJ_KOSU - D01.KS_TOY_KOSU AS KS_TORTO_KOSU")                                                                  '[回送運賃]個数(当日予算対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_TOY_KOSU <> 0 THEN D01.KS_TOJ_KOSU / D01.KS_TOY_KOSU - 1 ELSE 0 END AS KS_TORTO_KOSURATIO")          '[回送運賃]個数比率(当日予算対比)
        SQLBldr.AppendLine("    , D01.KS_TOJ_SHUNYU - D01.KS_TOY_SHUNYU AS KS_TORTO_SHUNYU")                                                            '[回送運賃]収入(当日予算対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_TOY_SHUNYU <> 0 THEN D01.KS_TOJ_SHUNYU / D01.KS_TOY_SHUNYU - 1 ELSE 0 END AS KS_TORTO_SHUNYURATIO")  '[回送運賃]収入比率(当日予算対比)
        SQLBldr.AppendLine("    , D01.KS_TOJ_TANKA - D01.KS_TOY_TANKA AS KS_TORTO_TANKA")                                                               '[回送運賃]単価(当時予算対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_TOY_TANKA <> 0 THEN D01.KS_TOJ_TANKA / D01.KS_TOY_TANKA - 1 ELSE 0 END AS KS_TORTO_TANKARATIO")      '[回送運賃]単価比率(当日予算対比)
        SQLBldr.AppendLine("    , D01.KS_TRJ_KOSU AS KS_TRJ_KOSU")                                                                                      '[回送運賃]個数(当月累計実績)
        SQLBldr.AppendLine("    , D01.KS_TRJ_SHUNYU AS KS_TRJ_SHUNYU")                                                                                  '[回送運賃]収入(当月累計実績)
        SQLBldr.AppendLine("    , D01.KS_TRJ_TANKA AS KS_TRJ_TANKA")                                                                                    '[回送運賃]単価(当月累計実績)
        SQLBldr.AppendLine("    , D01.KS_TRY_KOSU AS KS_TRY_KOSU")                                                                                      '[回送運賃]個数(当月累計予算)
        SQLBldr.AppendLine("    , D01.KS_TRY_SHUNYU AS KS_TRY_SHUNYU")                                                                                  '[回送運賃]収入(当月累計予算)
        SQLBldr.AppendLine("    , D01.KS_TRY_TANKA AS KS_TRY_TANKA")                                                                                    '[回送運賃]単価(当月累計予算)
        SQLBldr.AppendLine("    , D01.KS_TRJ_KOSU - D01.KS_TRY_KOSU AS KS_TGRTO_KOSU")                                                                  '[回送運賃]個数(当月予算対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_TRY_KOSU <> 0 THEN D01.KS_TRJ_KOSU / D01.KS_TRY_KOSU - 1 ELSE 0 END AS KS_TGRTO_KOSURATIO")          '[回送運賃]個数比率(当月予算対比)
        SQLBldr.AppendLine("    , D01.KS_TRJ_SHUNYU - D01.KS_TRY_SHUNYU AS KS_TGRTO_SHUNYU")                                                            '[回送運賃]収入(当月予算対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_TRY_SHUNYU <> 0 THEN D01.KS_TRJ_SHUNYU / D01.KS_TRY_SHUNYU - 1 ELSE 0 END AS KS_TGRTO_SHUNYURATIO")  '[回送運賃]収入比率(当月予算対比)
        SQLBldr.AppendLine("    , D01.KS_TRJ_TANKA - D01.KS_TRY_TANKA AS KS_TGRTO_TANKA")                                                               '[回送運賃]単価(当月予算対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_TRY_TANKA <> 0 THEN D01.KS_TRJ_TANKA / D01.KS_TRY_TANKA - 1 ELSE 0 END AS KS_TGRTO_TANKARATIO")      '[回送運賃]単価比率(当月予算対比)
        SQLBldr.AppendLine("    , D01.KS_ZNJ_KOSU AS KS_ZNJ_KOSU")                                                                                      '[回送運賃]個数(前年単日実績)
        SQLBldr.AppendLine("    , D01.KS_ZNJ_SHUNYU AS KS_ZNJ_SHUNYU")                                                                                  '[回送運賃]収入(前年単日実績)
        SQLBldr.AppendLine("    , D01.KS_ZNJ_TANKA AS KS_ZNJ_TANKA")                                                                                    '[回送運賃]単価(前年単日実績)
        SQLBldr.AppendLine("    , D01.KS_TOJ_KOSU - D01.KS_ZNJ_KOSU AS KS_ZNRTO_KOSU")                                                                  '[回送運賃]個数(前年単日対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_ZNJ_KOSU <> 0 THEN D01.KS_TOJ_KOSU / D01.KS_ZNJ_KOSU - 1 ELSE 0 END AS KS_ZNRTO_KOSURATIO")          '[回送運賃]個数比率(前年単日対比)
        SQLBldr.AppendLine("    , D01.KS_TOJ_SHUNYU - D01.KS_ZNJ_SHUNYU AS KS_ZNRTO_SHUNYU")                                                            '[回送運賃]収入(前年単日対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_ZNJ_SHUNYU <> 0 THEN D01.KS_TOJ_SHUNYU / D01.KS_ZNJ_SHUNYU - 1 ELSE 0 END AS KS_ZNRTO_SHUNYURATIO")  '[回送運賃]収入比率(前年単日対比)
        SQLBldr.AppendLine("    , D01.KS_TOJ_TANKA - D01.KS_ZNJ_TANKA AS KS_ZNRTO_TANKA")                                                               '[回送運賃]単価(前年単日対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_ZNJ_TANKA <> 0 THEN D01.KS_TOJ_TANKA / D01.KS_ZNJ_TANKA - 1 ELSE 0 END AS KS_ZNRTO_TANKARATIO")      '[回送運賃]単価比率(前年単日対比)
        SQLBldr.AppendLine("    , D01.KS_ZRJ_KOSU AS KS_ZRJ_KOSU")                                                                                      '[回送運賃]個数(前年累計実績)
        SQLBldr.AppendLine("    , D01.KS_ZRJ_SHUNYU AS KS_ZRJ_SHUNYU")                                                                                  '[回送運賃]収入(前年累計実績)
        SQLBldr.AppendLine("    , D01.KS_ZRJ_TANKA AS KS_ZRJ_TANKA")                                                                                    '[回送運賃]単価(前年累計実績)
        SQLBldr.AppendLine("    , D01.KS_MON_ZRJ_KOSU AS KS_MON_ZRJ_KOSU")                                                                              '[回送運賃]個数(前年月間累計実績)
        SQLBldr.AppendLine("    , D01.KS_MON_ZRJ_SHUNYU AS KS_MON_ZRJ_SHUNYU")                                                                          '[回送運賃]収入(前年月間累計実績)
        SQLBldr.AppendLine("    , D01.KS_MON_ZRJ_TANKA AS KS_MON_ZRJ_TANKA")                                                                            '[回送運賃]単価(前年月間累計実績)
        SQLBldr.AppendLine("    , D01.KS_TRJ_KOSU - D01.KS_ZRJ_KOSU AS KS_ZRRTO_KOSU")                                                                  '[回送運賃]個数(前年累計対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_ZRJ_KOSU <> 0 THEN D01.KS_TRJ_KOSU / D01.KS_ZRJ_KOSU - 1 ELSE 0 END AS KS_ZRRTO_KOSURATIO")          '[回送運賃]個数比率(前年累計対比)
        SQLBldr.AppendLine("    , D01.KS_TRJ_SHUNYU - D01.KS_ZRJ_SHUNYU AS KS_ZRRTO_SHUNYU")                                                            '[回送運賃]収入(前年累計対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_ZRJ_SHUNYU <> 0 THEN D01.KS_TRJ_SHUNYU / D01.KS_ZRJ_SHUNYU - 1 ELSE 0 END AS KS_ZRRTO_SHUNYURATIO")  '[回送運賃]収入比率(前年累計対比)
        SQLBldr.AppendLine("    , D01.KS_TRJ_TANKA - D01.KS_ZRJ_TANKA AS KS_ZRRTO_TANKA")                                                               '[回送運賃]単価(前年累計対比)
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_ZRJ_TANKA <> 0 THEN D01.KS_TRJ_TANKA / D01.KS_ZRJ_TANKA - 1 ELSE 0 END AS KS_ZRRTO_TANKARATIO")      '[回送運賃]単価比率(前年累計対比)
        SQLBldr.AppendLine("FROM")
        'メイン 営業日報(全社)用VIEW (形式別)当日・前年実績予算対比
        SQLBldr.AppendLine("    lng.VIW0005_BIZDAILYRP_ALL_1 D01")
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

    ''' <summary>
    ''' (形式別)当月予算明細取得処理
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function KeishikiMonthYOSANDataTbl(ByVal SQLcon As MySqlConnection, ByVal I_KEYYMD As String) As DataTable

        Dim dt = New DataTable

        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("Select                                   ")
        SQLBldr.AppendLine("    Case                                 ")
        SQLBldr.AppendLine("        WHEN D01.BIGCTNCD = '10' THEN '1'")
        SQLBldr.AppendLine("        WHEN D01.BIGCTNCD = '11' THEN '2'")
        SQLBldr.AppendLine("        WHEN D01.BIGCTNCD = '20' THEN '6'")
        SQLBldr.AppendLine("    	ELSE '9'                         ")
        SQLBldr.AppendLine("    END AS BIGCTNTYPECODE                ")                                                                       '形式グループコード
        '収入データ
        SQLBldr.AppendLine("   ,SUM(coalesce(D01.QUANTITY, 0))                                        AS SN_QUANTITY ")                         '[収入]個数(当月予算)
        SQLBldr.AppendLine("   ,ROUND(SUM(coalesce(D01.USEFEE, 0)) / 1000, 0)                         AS SN_USEFEE   ")                         '[収入]収入(当月予算)
        SQLBldr.AppendLine("   ,CASE")
        SQLBldr.AppendLine("        WHEN SUM(coalesce(D01.USEFEE, 0)) = 0 THEN 0")
        SQLBldr.AppendLine("        ELSE")
        SQLBldr.AppendLine("        ROUND(SUM(coalesce(D01.USEFEE, 0)) / SUM(coalesce(D01.QUANTITY, 0)), 0)")
        SQLBldr.AppendLine("    END AS SN_UNITPRICE ")                                                                                        '[収入]単価(当月予算)
        '回送運賃データ
        SQLBldr.AppendLine("   ,SUM(coalesce(D02.FREEPIPEQUANTITY, 0) + coalesce(D02.FREESURGERYQUANTITY, 0))                AS KS_QUANTITY  ")   '[回送運賃]個数(当月予算)
        SQLBldr.AppendLine("   ,ROUND(SUM(coalesce(D02.FREEPIPEKAISOHI, 0) + coalesce(D02.FREESURGERYKAISOHI, 0)) / 1000, 0) AS KS_USEFEE    ")   '[回送運賃]収入(当月予算)
        SQLBldr.AppendLine("   ,CASE")
        SQLBldr.AppendLine("        WHEN SUM(coalesce(D02.FREEPIPEKAISOHI, 0) + coalesce(D02.FREESURGERYKAISOHI, 0)) = 0 THEN 0")
        SQLBldr.AppendLine("        ELSE")
        SQLBldr.AppendLine("        ROUND(SUM(coalesce(D02.FREEPIPEKAISOHI, 0) + coalesce(D02.FREESURGERYKAISOHI, 0)) /  ")
        SQLBldr.AppendLine("             (SUM(coalesce(D02.FREEPIPEQUANTITY, 0) + coalesce(D02.FREESURGERYQUANTITY, 0))), 0)")
        SQLBldr.AppendLine("    END AS KS_UNITPRICE ")                                                                                        '[回送運賃]単価(当月予算)
        SQLBldr.AppendLine("FROM")
        'メイン 予算使用料月別マスタ
        SQLBldr.AppendLine("    lng.LNM0030_BUDGETMONTHFEE     D01")
        SQLBldr.AppendLine("LEFT JOIN")
        SQLBldr.AppendLine("    lng.LNM0032_BUDGETMONTHKAISOHI D02")
        SQLBldr.AppendLine("ON")
        SQLBldr.AppendLine("    D01.BUDGETYEAR  = D02.BUDGETYEAR  ")
        SQLBldr.AppendLine("AND D01.BUDGETMONTH = D02.BUDGETMONTH ")
        SQLBldr.AppendLine("AND D01.BRANCHCODE  = D02.BRANCHCODE  ")
        SQLBldr.AppendLine("AND D01.BIGCTNCD    = D02.BIGCTNCD    ")
        SQLBldr.AppendLine("AND D01.MIDDLECTNCD = D02.MIDDLECTNCD ")
        SQLBldr.AppendLine("AND D02.DELFLG      = @P01            ")
        SQLBldr.AppendLine("INNER JOIN")
        SQLBldr.AppendLine("    com.LNS0014_ORG D03")
        SQLBldr.AppendLine("ON")
        SQLBldr.AppendLine("    D03.CTNFLG     = '1'")
        SQLBldr.AppendLine("AND D03.CLASS01    = '1'")
        SQLBldr.AppendLine("AND D01.BRANCHCODE = D03.ORGCODE")
        SQLBldr.AppendLine("AND D03.DELFLG     = @P01")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("    D01.BUDGETYEAR  = @P02               ")
        SQLBldr.AppendLine("AND D01.BUDGETMONTH = @P03               ")
        SQLBldr.AppendLine("AND D01.DELFLG      = @P01               ")
        SQLBldr.AppendLine("GROUP BY                                 ")
        SQLBldr.AppendLine("    CASE                                 ")
        SQLBldr.AppendLine("        WHEN D01.BIGCTNCD = '10' THEN '1'")
        SQLBldr.AppendLine("        WHEN D01.BIGCTNCD = '11' THEN '2'")
        SQLBldr.AppendLine("        WHEN D01.BIGCTNCD = '20' THEN '6'")
        SQLBldr.AppendLine("    	ELSE '9'                         ")
        SQLBldr.AppendLine("    END                                  ")
        '抽出条件
        SQLBldr.AppendLine("UNION ALL                                ")
        SQLBldr.AppendLine("SELECT                                   ")
        SQLBldr.AppendLine("    '10' AS BIGCTNTYPECODE               ")                                                                       '形式グループコード
        '収入データ
        SQLBldr.AppendLine("   ,SUM(coalesce(D01.QUANTITY, 0))                                        AS SN_QUANTITY ")                         '[収入]個数(当月予算)
        SQLBldr.AppendLine("   ,ROUND(SUM(coalesce(D01.USEFEE, 0)) / 1000, 0)                         AS SN_USEFEE   ")                         '[収入]収入(当月予算)
        SQLBldr.AppendLine("   ,CASE")
        SQLBldr.AppendLine("        WHEN SUM(coalesce(D01.USEFEE, 0)) = 0 THEN 0")
        SQLBldr.AppendLine("        ELSE")
        SQLBldr.AppendLine("        ROUND(SUM(coalesce(D01.USEFEE, 0)) / SUM(coalesce(D01.QUANTITY, 0)), 0)")
        SQLBldr.AppendLine("    END AS SN_UNITPRICE ")                                                                                        '[収入]単価(当月予算)
        '回送運賃データ
        SQLBldr.AppendLine("   ,SUM(coalesce(D02.FREEPIPEQUANTITY, 0) + coalesce(D02.FREESURGERYQUANTITY, 0))                AS KS_QUANTITY  ")   '[回送運賃]個数(当月予算)
        SQLBldr.AppendLine("   ,ROUND(SUM(coalesce(D02.FREEPIPEKAISOHI, 0) + coalesce(D02.FREESURGERYKAISOHI, 0)) / 1000, 0) AS KS_USEFEE    ")   '[回送運賃]収入(当月予算)
        SQLBldr.AppendLine("   ,CASE")
        SQLBldr.AppendLine("        WHEN SUM(coalesce(D02.FREEPIPEKAISOHI, 0) + coalesce(D02.FREESURGERYKAISOHI, 0)) = 0 THEN 0")
        SQLBldr.AppendLine("        ELSE")
        SQLBldr.AppendLine("        ROUND(SUM(coalesce(D02.FREEPIPEKAISOHI, 0) + coalesce(D02.FREESURGERYKAISOHI, 0)) /  ")
        SQLBldr.AppendLine("             (SUM(coalesce(D02.FREEPIPEQUANTITY, 0) + coalesce(D02.FREESURGERYQUANTITY, 0))), 0)")
        SQLBldr.AppendLine("    END AS KS_UNITPRICE ")                                                                                        '[回送運賃]単価(当月予算)
        SQLBldr.AppendLine("FROM")
        'メイン 予算使用料月別マスタ
        SQLBldr.AppendLine("    lng.LNM0030_BUDGETMONTHFEE     D01")
        SQLBldr.AppendLine("LEFT JOIN")
        SQLBldr.AppendLine("    lng.LNM0032_BUDGETMONTHKAISOHI D02")
        SQLBldr.AppendLine("ON")
        SQLBldr.AppendLine("    D01.BUDGETYEAR  = D02.BUDGETYEAR  ")
        SQLBldr.AppendLine("AND D01.BUDGETMONTH = D02.BUDGETMONTH ")
        SQLBldr.AppendLine("AND D01.BRANCHCODE  = D02.BRANCHCODE  ")
        SQLBldr.AppendLine("AND D01.BIGCTNCD    = D02.BIGCTNCD    ")
        SQLBldr.AppendLine("AND D01.MIDDLECTNCD = D02.MIDDLECTNCD ")
        SQLBldr.AppendLine("AND D02.DELFLG      = @P01            ")
        SQLBldr.AppendLine("INNER JOIN")
        SQLBldr.AppendLine("    com.LNS0014_ORG D03")
        SQLBldr.AppendLine("ON")
        SQLBldr.AppendLine("    D03.CTNFLG     = '1'")
        SQLBldr.AppendLine("AND D03.CLASS01    = '1'")
        SQLBldr.AppendLine("AND D01.BRANCHCODE = D03.ORGCODE")
        SQLBldr.AppendLine("AND D03.DELFLG     = @P01")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("    D01.BUDGETYEAR  = @P02            ")
        SQLBldr.AppendLine("AND D01.BUDGETMONTH = @P03            ")
        SQLBldr.AppendLine("AND D01.DELFLG      = @P01            ")

        Try
            Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)  '削除フラグ
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)  '年
                Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar)  '月

                PARA01.Value = C_DELETE_FLG.ALIVE
                PARA02.Value = Format(CDate(I_KEYYMD), "yyyy").ToString
                PARA03.Value = Format(CDate(I_KEYYMD), "MM").ToString

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

    ''' <summary>
    ''' (品目支店別)当月・前年実績対比明細取得処理
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetHinmkDataTbl(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt = New DataTable
        Dim WW_DATENOW As Date = Date.Now

        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    D01.HINMOKUGROUPCD")                            '品目グループコード
        '冷蔵データ
        SQLBldr.AppendLine("    , D01.RZ_HKD_KOSU_TGT")                         '[冷蔵]当月_個数(北海道)
        SQLBldr.AppendLine("    , D01.RZ_HKD_KOSU_ZNN")                         '[冷蔵]前年_個数(北海道)
        SQLBldr.AppendLine("    , D01.RZ_HKD_KOSU_TIH")                         '[冷蔵]対比_個数(北海道)
        SQLBldr.AppendLine("    , D01.RZ_HKD_KOSU_RTO")                         '[冷蔵]対比_比率(北海道)
        SQLBldr.AppendLine("    , D01.RZ_THK_KOSU_TGT")                         '[冷蔵]当月_個数(東北)
        SQLBldr.AppendLine("    , D01.RZ_THK_KOSU_ZNN")                         '[冷蔵]前年_個数(東北)
        SQLBldr.AppendLine("    , D01.RZ_THK_KOSU_TIH")                         '[冷蔵]対比_個数(東北)
        SQLBldr.AppendLine("    , D01.RZ_THK_KOSU_RTO")                         '[冷蔵]対比_比率(東北)
        SQLBldr.AppendLine("    , D01.RZ_KNT_KOSU_TGT")                         '[冷蔵]当月_個数(関東)
        SQLBldr.AppendLine("    , D01.RZ_KNT_KOSU_ZNN")                         '[冷蔵]前年_個数(関東)
        SQLBldr.AppendLine("    , D01.RZ_KNT_KOSU_TIH")                         '[冷蔵]対比_個数(関東)
        SQLBldr.AppendLine("    , D01.RZ_KNT_KOSU_RTO")                         '[冷蔵]対比_比率(関東)
        SQLBldr.AppendLine("    , D01.RZ_TYB_KOSU_TGT")                         '[冷蔵]当月_個数(中部)
        SQLBldr.AppendLine("    , D01.RZ_TYB_KOSU_ZNN")                         '[冷蔵]前年_個数(中部)
        SQLBldr.AppendLine("    , D01.RZ_TYB_KOSU_TIH")                         '[冷蔵]対比_個数(中部)
        SQLBldr.AppendLine("    , D01.RZ_TYB_KOSU_RTO")                         '[冷蔵]対比_比率(中部)
        SQLBldr.AppendLine("    , D01.RZ_KNS_KOSU_TGT")                         '[冷蔵]当月_個数(関西)
        SQLBldr.AppendLine("    , D01.RZ_KNS_KOSU_ZNN")                         '[冷蔵]前年_個数(関西)
        SQLBldr.AppendLine("    , D01.RZ_KNS_KOSU_TIH")                         '[冷蔵]対比_個数(関西)
        SQLBldr.AppendLine("    , D01.RZ_KNS_KOSU_RTO")                         '[冷蔵]対比_比率(関西)
        SQLBldr.AppendLine("    , D01.RZ_KSY_KOSU_TGT")                         '[冷蔵]当月_個数(九州)
        SQLBldr.AppendLine("    , D01.RZ_KSY_KOSU_ZNN")                         '[冷蔵]前年_個数(九州)
        SQLBldr.AppendLine("    , D01.RZ_KSY_KOSU_TIH")                         '[冷蔵]対比_個数(九州)
        SQLBldr.AppendLine("    , D01.RZ_KSY_KOSU_RTO")                         '[冷蔵]対比_比率(九州)
        SQLBldr.AppendLine("    , D01.RZ_TTL_KOSU_TGT")                         '[冷蔵]当月_個数(全国計)
        SQLBldr.AppendLine("    , D01.RZ_TTL_KOSU_ZNN")                         '[冷蔵]前年_個数(全国計)
        SQLBldr.AppendLine("    , D01.RZ_TTL_KOSU_TIH")                         '[冷蔵]対比_個数(全国計)
        SQLBldr.AppendLine("    , D01.RZ_TTL_KOSU_RTO")                         '[冷蔵]対比_比率(全国計)
        'S-URデータ
        SQLBldr.AppendLine("    , D01.SR_HKD_KOSU_TGT")                         '[S-UR]当月_個数(北海道)
        SQLBldr.AppendLine("    , D01.SR_HKD_KOSU_ZNN")                         '[S-UR]前年_個数(北海道)
        SQLBldr.AppendLine("    , D01.SR_HKD_KOSU_TIH")                         '[S-UR]対比_個数(北海道)
        SQLBldr.AppendLine("    , D01.SR_HKD_KOSU_RTO")                         '[S-UR]対比_比率(北海道)
        SQLBldr.AppendLine("    , D01.SR_THK_KOSU_TGT")                         '[S-UR]当月_個数(東北)
        SQLBldr.AppendLine("    , D01.SR_THK_KOSU_ZNN")                         '[S-UR]前年_個数(東北)
        SQLBldr.AppendLine("    , D01.SR_THK_KOSU_TIH")                         '[S-UR]対比_個数(東北)
        SQLBldr.AppendLine("    , D01.SR_THK_KOSU_RTO")                         '[S-UR]対比_比率(東北)
        SQLBldr.AppendLine("    , D01.SR_KNT_KOSU_TGT")                         '[S-UR]当月_個数(関東)
        SQLBldr.AppendLine("    , D01.SR_KNT_KOSU_ZNN")                         '[S-UR]前年_個数(関東)
        SQLBldr.AppendLine("    , D01.SR_KNT_KOSU_TIH")                         '[S-UR]対比_個数(関東)
        SQLBldr.AppendLine("    , D01.SR_KNT_KOSU_RTO")                         '[S-UR]対比_比率(関東)
        SQLBldr.AppendLine("    , D01.SR_TYB_KOSU_TGT")                         '[S-UR]当月_個数(中部)
        SQLBldr.AppendLine("    , D01.SR_TYB_KOSU_ZNN")                         '[S-UR]前年_個数(中部)
        SQLBldr.AppendLine("    , D01.SR_TYB_KOSU_TIH")                         '[S-UR]対比_個数(中部)
        SQLBldr.AppendLine("    , D01.SR_TYB_KOSU_RTO")                         '[S-UR]対比_比率(中部)
        SQLBldr.AppendLine("    , D01.SR_KNS_KOSU_TGT")                         '[S-UR]当月_個数(関西)
        SQLBldr.AppendLine("    , D01.SR_KNS_KOSU_ZNN")                         '[S-UR]前年_個数(関西)
        SQLBldr.AppendLine("    , D01.SR_KNS_KOSU_TIH")                         '[S-UR]対比_個数(関西)
        SQLBldr.AppendLine("    , D01.SR_KNS_KOSU_RTO")                         '[S-UR]対比_比率(関西)
        SQLBldr.AppendLine("    , D01.SR_KSY_KOSU_TGT")                         '[S-UR]当月_個数(九州)
        SQLBldr.AppendLine("    , D01.SR_KSY_KOSU_ZNN")                         '[S-UR]前年_個数(九州)
        SQLBldr.AppendLine("    , D01.SR_KSY_KOSU_TIH")                         '[S-UR]対比_個数(九州)
        SQLBldr.AppendLine("    , D01.SR_KSY_KOSU_RTO")                         '[S-UR]対比_比率(九州)
        SQLBldr.AppendLine("    , D01.SR_TTL_KOSU_TGT")                         '[S-UR]当月_個数(全国計)
        SQLBldr.AppendLine("    , D01.SR_TTL_KOSU_ZNN")                         '[S-UR]前年_個数(全国計)
        SQLBldr.AppendLine("    , D01.SR_TTL_KOSU_TIH")                         '[S-UR]対比_個数(全国計)
        SQLBldr.AppendLine("    , D01.SR_TTL_KOSU_RTO")                         '[S-UR]対比_比率(全国計)
        SQLBldr.AppendLine("FROM")
        'メイン (品目支店別)当月・前年実績対比
        SQLBldr.AppendLine("    lng.VIW0006_BIZDAILYRP_ALL_2 D01")
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

    ''' <summary>
    ''' 差引収支明細取得処理
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetSshkShushiDataTbl(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt = New DataTable
        Dim WW_DATENOW As Date = Date.Now

        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT")
        SQLBldr.AppendLine("    D01.KEISHIKIGROUPCD")                                                                                                   '形式グループコード
        '収支データ
        SQLBldr.AppendLine("    , D01.SN_TOJ_SHUNYU - D01.KS_TOJ_SHUNYU AS SHUSHI_KINGAKU")                                                             '収支金額
        SQLBldr.AppendLine("    , CASE WHEN D01.KS_TOJ_SHUNYU <> 0 THEN FLOOR(D01.SN_TOJ_SHUNYU / D01.KS_TOJ_SHUNYU * 100) ELSE 0 END AS SHUSHI_RATIO") '収支比率
        SQLBldr.AppendLine("FROM")
        'メイン 営業日報(全社)用VIEW (形式別)当日・前年実績予算対比
        SQLBldr.AppendLine("    lng.VIW0005_BIZDAILYRP_ALL_1 D01")
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

    ''' <summary>
    ''' 冷蔵運用個数明細取得処理
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetReizouUnyoKosuDataTbl(ByVal SQLcon As MySqlConnection) As DataTable

        Dim dt = New DataTable
        Dim WW_DATENOW As Date = Date.Now

        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine("SELECT                        ")
        SQLBldr.AppendLine("     ORGCODE                  ")
        SQLBldr.AppendLine("    ,PROPER_NUM               ")
        SQLBldr.AppendLine("    ,TOJ_NUM                  ")
        SQLBldr.AppendLine("    ,TOJ_RATE                 ")
        SQLBldr.AppendLine("    ,ZNJ_NUM                  ")
        SQLBldr.AppendLine("    ,ZNJ_RATE                 ")
        SQLBldr.AppendLine("    ,ZNN_NUM                  ")
        SQLBldr.AppendLine("    ,ZNN_RATE                 ")
        SQLBldr.AppendLine("    ,ANCHORAGE_NUM            ")
        SQLBldr.AppendLine("    ,ANCHORAGE_DAY            ")
        SQLBldr.AppendLine("    ,ANCHORAGE_RATE           ")
        ' 2024/05/16 ADD START
        SQLBldr.AppendLine("    ,ANCHORAGE_TENDAYSNUM     ")
        SQLBldr.AppendLine("    ,ANCHORAGE_REPAIRNUM      ")
        SQLBldr.AppendLine("    ,ANCHORAGE_SPDETENTIONNUM ")
        SQLBldr.AppendLine("    ,ANCHORAGE_NORMALONLYNUM  ")
        ' 2024/05/16 ADD END
        SQLBldr.AppendLine("    ,OPE_EFF_TOJ_RATE         ")
        SQLBldr.AppendLine("    ,OPE_EFF_TOTAL_RATE       ")
        SQLBldr.AppendLine("FROM")
        'メイン 冷蔵運用個数テーブル
        SQLBldr.AppendLine("    LNG.LNT0111_REIZOUUNYOKOSU")
        '抽出条件
        SQLBldr.AppendLine("WHERE")
        SQLBldr.AppendLine("    DATADATE = '" & WW_KeyYMD & "'")
        SQLBldr.AppendLine("ORDER BY")
        SQLBldr.AppendLine("    ORGCODE")

        Try
            Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)
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
