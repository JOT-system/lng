''************************************************************
' 画面名称   ：発送日報
' 作成日     ：2022/11/24
' 作成者     ：伊藤
' 最終更新日 ：2024/09/20
' 最終更新者 ：星
' バージョン ：ver2
' 
' 修正履歴：2024/09/20 ver2 星 メニュー出力日付別シート分け
''************************************************************
Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 発送日報帳票作成クラス
''' </summary>
Public Class LNT0012_DailyShipmentReport_DIODOC

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private PrintData As DataTable

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
    Public Sub New(mapId As String, excelFileName As String, printDataClass As DataTable)
        Try
            Dim CS0050SESSION As New CS0050SESSION
            Me.PrintData = printDataClass
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
                If WW_Workbook.Worksheets(i).Name = "発送日報" Then
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
    Public Function CreateExcelPrintData(OfficeCode As String, type As String, menu As String) As String ' 2024/09/20 ver2 星 CHG
        Dim ReportName As String = ""
        If type = "A" Then
            ReportName = "発送日報（A）_"
        Else
            ReportName = "発送日報（B）_"
        End If
        Dim tmpFileName As String = ReportName & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte
        Dim ReportType As String = type
        ' 2024/09/20 ver2 星 ADD START
        Dim FROMYMD As String = ""
        Dim LastFROMYMD As String = ""
        Dim PrintaddsheetFlg As Boolean = False
        ' 2024/09/20 ver2 星 ADD END

        Try
            Dim lastRow As DataRow = Nothing
            Dim lastlastRow As DataRow = Nothing ' 2024/09/20 ver2 星 ADD
            Dim idx As Int32 = 1
            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim PageNum As Int32 = 1
            Dim row_cnt As Int32 = 0
            Dim Mode As Integer = 0
            Dim Ndeptrusteecd_1 As String = ""
            Dim Odeptrusteecd_1 As String = ""
            Dim TotalKBN As Integer = 0
            Dim TrusteeKBN As Integer = 0
            Dim BigCtncdKBN As Integer = 0
            Dim Quantity1(6, 6, 9) As Long
            Dim Quantity2(6, 6, 9) As Long
            Dim UseFee1(6, 6, 9) As Long
            Dim Quantity3(6, 6, 9) As Long
            Dim FreesendFee1(6, 6, 9) As Long
            Dim Quantity4(6, 6, 9) As Long
            Dim FreesendFee2(6, 6, 9) As Long
            Dim FixedFare(6, 6, 9) As Long
            Dim OwnDiscountFee(6, 6, 9) As Long
            Dim UseFee2(6, 6, 9) As Long
            Dim FreesendFee3(6, 6, 9) As Long
            Dim NituuFreesend(6, 6, 9) As Long
            Dim FreesendFee4(6, 6, 9) As Long
            Dim ShipFee(6, 6, 9) As Long
            Dim ShipBurdenFee(6, 6, 9) As Long  '荷主負担運賃
            Dim PickupFee(6, 6, 9) As Long      '集荷料
            Dim StackFreeKBN As Integer = 0

            '配列初期化
            For A As Integer = 1 To 6
                For B As Integer = 1 To 6
                    For C As Integer = 1 To 9
                        Quantity1(A, B, C) = 0
                        Quantity2(A, B, C) = 0
                        UseFee1(A, B, C) = 0
                        Quantity3(A, B, C) = 0
                        FreesendFee1(A, B, C) = 0
                        Quantity4(A, B, C) = 0
                        FreesendFee2(A, B, C) = 0
                        FixedFare(A, B, C) = 0
                        OwnDiscountFee(A, B, C) = 0
                        UseFee2(A, B, C) = 0
                        FreesendFee3(A, B, C) = 0
                        NituuFreesend(A, B, C) = 0
                        FreesendFee4(A, B, C) = 0
                        ShipFee(A, B, C) = 0
                        ShipBurdenFee(A, B, C) = 0
                        PickupFee(A, B, C) = 0
                    Next
                Next
            Next

            For Each row As DataRow In PrintData.Rows

                ' 2024/09/20 ver2 星 ADD START
                If menu = "1" Then

                    If type = "A" Then
                        FROMYMD = row("FROMYMD").ToString
                    ElseIf type = "B" Then
                        FROMYMD = row("FROMDAY").ToString
                    End If

                    If LastFROMYMD <> "" AndAlso
                       LastFROMYMD <> FROMYMD Then
                        PrintaddsheetFlg = True
                        If lastlastRow Is Nothing Then
                            '〇全合計
                            EditTotalArea(idx, row, lastRow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                                      Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                                      FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFreeKBN, ReportType, OfficeCode, 1, 1)
                        Else
                            '〇全合計
                            EditTotalArea(idx, lastRow, lastlastRow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                                      Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                                      FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFreeKBN, ReportType, OfficeCode, 1, 1)
                        End If
                    ElseIf LastFROMYMD = "" Then
                        PrintaddsheetFlg = True
                    Else
                        PrintaddsheetFlg = False
                    End If

                    If type = "A" Then
                        LastFROMYMD = row("FROMYMD").ToString
                        'シート追加
                        If PrintaddsheetFlg = True Then
                            Dim seetname As String = CDate(row("FROMYMD")).ToString("yyyyMMdd")
                            TrySetExcelWorkSheet(idx, seetname, PageNum, "発送日報")
                            Me.WW_Workbook.Worksheets(WW_SheetNo).Name = seetname
                            'シートが切り替わり、ページ数リセット
                            PageNum = 1
                            row_cnt = 0
                            idx = 1

                            '配列初期化
                            For A As Integer = 1 To 6
                                For B As Integer = 1 To 6
                                    For C As Integer = 1 To 9
                                        Quantity1(A, B, C) = 0
                                        Quantity2(A, B, C) = 0
                                        UseFee1(A, B, C) = 0
                                        Quantity3(A, B, C) = 0
                                        FreesendFee1(A, B, C) = 0
                                        Quantity4(A, B, C) = 0
                                        FreesendFee2(A, B, C) = 0
                                        FixedFare(A, B, C) = 0
                                        OwnDiscountFee(A, B, C) = 0
                                        UseFee2(A, B, C) = 0
                                        FreesendFee3(A, B, C) = 0
                                        NituuFreesend(A, B, C) = 0
                                        FreesendFee4(A, B, C) = 0
                                        ShipFee(A, B, C) = 0
                                        ShipBurdenFee(A, B, C) = 0
                                        PickupFee(A, B, C) = 0
                                    Next
                                Next
                            Next
                        End If
                    ElseIf type = "B" Then
                        LastFROMYMD = row("FROMDAY").ToString
                        'シート追加
                        If PrintaddsheetFlg = True Then
                            Dim seetname As String = CDate(row("FROMDAY")).ToString("yyyyMMdd")
                            TrySetExcelWorkSheet(idx, seetname, PageNum, "発送日報")
                            Me.WW_Workbook.Worksheets(WW_SheetNo).Name = seetname
                            'シートが切り替わり、ページ数リセット
                            PageNum = 1
                            row_cnt = 0
                            idx = 1

                            '配列初期化
                            For A As Integer = 1 To 6
                                For B As Integer = 1 To 6
                                    For C As Integer = 1 To 9
                                        Quantity1(A, B, C) = 0
                                        Quantity2(A, B, C) = 0
                                        UseFee1(A, B, C) = 0
                                        Quantity3(A, B, C) = 0
                                        FreesendFee1(A, B, C) = 0
                                        Quantity4(A, B, C) = 0
                                        FreesendFee2(A, B, C) = 0
                                        FixedFare(A, B, C) = 0
                                        OwnDiscountFee(A, B, C) = 0
                                        UseFee2(A, B, C) = 0
                                        FreesendFee3(A, B, C) = 0
                                        NituuFreesend(A, B, C) = 0
                                        FreesendFee4(A, B, C) = 0
                                        ShipFee(A, B, C) = 0
                                        ShipBurdenFee(A, B, C) = 0
                                        PickupFee(A, B, C) = 0
                                    Next
                                Next
                            Next
                        End If
                    End If

                End If
                ' 2024/09/20 ver2 星 ADD END

                row_cnt += 1

                '1行目
                If lastRow Is Nothing OrElse
                   PrintaddsheetFlg = True Then ' 2024/09/20 ver2 星 ADD
                    '〇ヘッダー情報セット
                    EditHeaderArea(idx, CDate(row("SHIPYMD")), row("JOTDEPBRANCHNM").ToString, PageNum, ReportType, row("OFFICECODE").ToString)

                Else '2行目以降
                    Ndeptrusteecd_1 = Left(row("DEPTRUSTEECD").ToString(), 1)
                    Odeptrusteecd_1 = Left(lastRow("DEPTRUSTEECD").ToString(), 1)
                    '前行と発送年月日、支店、発駅、発受託人、発受託人サブ、積空区分が一致する場合
                    If lastRow("SHIPYMD").ToString() = row("SHIPYMD").ToString() AndAlso
                    lastRow("JOTDEPBRANCHCD").ToString() = row("JOTDEPBRANCHCD").ToString() AndAlso
                    lastRow("DEPSTATIONCD").ToString() = row("DEPSTATIONCD").ToString() AndAlso
                    Odeptrusteecd_1 = Ndeptrusteecd_1 AndAlso
                    lastRow("DEPTRUSTEECD").ToString() = row("DEPTRUSTEECD").ToString() AndAlso
                    lastRow("DEPTRUSTEESUBCD").ToString() = row("DEPTRUSTEESUBCD").ToString() AndAlso
                    lastRow("STACKFREEKBN").ToString() = row("STACKFREEKBN").ToString() Then
                        Mode = 1
                    Else
                        '発送年月日が不一致の場合
                        If lastRow("SHIPYMD").ToString() <> row("SHIPYMD").ToString() Then
                            '〇全合計
                            EditTotalArea(idx, row, lastRow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                      Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                      FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFreeKBN, ReportType, OfficeCode, 0, 0)
                        Else
                            '支店が不一致の場合
                            If lastRow("JOTDEPBRANCHCD").ToString() <> row("JOTDEPBRANCHCD").ToString() Then
                                '〇支店計
                                EditBranchTotalArea(idx, row, lastRow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                                    Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                                    FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFreeKBN, ReportType, OfficeCode, 0, 0)
                            Else
                                '発駅が不一致の場合
                                If lastRow("DEPSTATIONCD").ToString() <> row("DEPSTATIONCD").ToString() Then
                                    '〇発駅計
                                    EditStationTotalArea(idx, lastRow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                                     Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                                     FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFreeKBN, ReportType)
                                Else
                                    '発受託人(前1桁)が不一致の場合
                                    If Odeptrusteecd_1 <> Ndeptrusteecd_1 Then
                                        '発受託人計
                                        EditTrusteeTotalArea(idx, lastRow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                                         Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                                         FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFreeKBN, ReportType)
                                    Else
                                        '発受託人が不一致の場合
                                        If lastRow("DEPTRUSTEECD").ToString() <> row("DEPTRUSTEECD").ToString() OrElse lastRow("DEPTRUSTEESUBCD").ToString() <> row("DEPTRUSTEESUBCD").ToString() Then
                                            '発受託人計
                                            EditTrusteeSubTotalArea(idx, lastRow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                                                Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                                                FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFreeKBN, ReportType)
                                        Else
                                            '積空区分が不一致の場合
                                            If lastRow("STACKFREEKBN").ToString() <> row("STACKFREEKBN").ToString() Then
                                                '積空区分別計
                                                EditStackFreeTotalArea(idx, lastRow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                                                   Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                                                   FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFreeKBN, ReportType)
                                            End If
                                        End If
                                    End If
                                End If
                            End If
                        End If
                    End If
                End If
                '明細セット
                EditDetailArea(idx, row, lastRow, PageNum, Mode, ReportType)

                '数量、料金加算
                '〇発受託人区分
                TrusteeKBN = 0
                If row("DEPTRUSTEECD") IsNot DBNull.Value Then
                    Dim DepTrustee As Integer = CType(row("DEPTRUSTEECD"), Integer)
                    If DepTrustee > 59999 And DepTrustee < 70000 Then
                        If DepTrustee <> 67368 Then
                            TrusteeKBN = 1
                        End If
                    End If
                    If DepTrustee > 69999 And DepTrustee < 80000 Then
                        TrusteeKBN = 2
                    End If
                    If DepTrustee > 89999 And DepTrustee < 100000 Then
                        TrusteeKBN = 3
                    End If
                    If DepTrustee > 79999 And DepTrustee < 90000 Then
                        If DepTrustee <> 82080 Then
                            TrusteeKBN = 4
                        End If
                    End If
                    If TrusteeKBN = 0 Then
                        TrusteeKBN = 5
                    End If
                Else
                    TrusteeKBN = 5
                End If

                '〇大分類コード区分
                Dim BigCtncd As String = row("BIGCTNCD").ToString
                If BigCtncd = "05" Then
                    BigCtncdKBN = 1
                End If
                If BigCtncd = "10" Then
                    BigCtncdKBN = 2
                End If
                If BigCtncd = "11" Then
                    BigCtncdKBN = 3
                End If
                If BigCtncd = "15" Then
                    BigCtncdKBN = 4
                End If
                If BigCtncd = "20" Then
                    BigCtncdKBN = 5
                End If
                If BigCtncd = "35" Then
                    BigCtncdKBN = 6
                End If
                If BigCtncd = "25" Then
                    BigCtncdKBN = 7
                End If
                If BigCtncd = "0" Then
                    BigCtncdKBN = 8
                End If
                If BigCtncdKBN = 0 Then
                    BigCtncdKBN = 8
                End If
                If row("FILETYPE").ToString() = "2" OrElse row("FILETYPE").ToString() = "3" Then
                    BigCtncdKBN = 8
                End If

                '〇
                If row("STACKFREEKBN") IsNot DBNull.Value Then
                    StackFreeKBN = CType(row("STACKFREEKBN"), Integer)
                Else
                    StackFreeKBN = 0
                End If
                Dim SetTrusteeKBN As Integer = TrusteeKBN
                Dim SetBigCtncdKBN As Integer = BigCtncdKBN
                For i As Integer = 0 To 3
                    If i = 1 Then
                        SetTrusteeKBN = TrusteeKBN
                        SetBigCtncdKBN = 9
                    End If
                    If i = 2 Then
                        SetTrusteeKBN = 6
                        SetBigCtncdKBN = BigCtncdKBN
                    End If
                    If i = 3 Then
                        SetBigCtncdKBN = 9
                        SetTrusteeKBN = 6
                    End If
                    If StackFreeKBN = 1 Then
                        Quantity1(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("QUANTITY"), Long)
                        Quantity2(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("QUANTITY"), Long)
                        UseFee1(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("USEFEE"), Long)
                        FixedFare(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("JRFIXEDFARE"), Long)
                        UseFee2(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("USEFEE"), Long)
                        NituuFreesend(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("NITTSUFREESEND"), Long)
                        OwnDiscountFee(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("OWNDISCOUNTFEE"), Long)
                        ShipFee(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("SHIPFEE"), Long)
                        ShipBurdenFee(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("SHIPBURDENFEE"), Long)
                        PickupFee(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("PICKUPFEE"), Long)
                    ElseIf StackFreeKBN = 0 Then
                        If row("FILETYPE").ToString() = "2" Then
                            Quantity1(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("QUANTITY"), Long)
                            Quantity2(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("QUANTITY"), Long)
                            UseFee1(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("USEFEE"), Long)
                            UseFee2(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("USEFEE"), Long)
                        End If
                    Else
                        Quantity1(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("QUANTITY"), Long)
                        ShipFee(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("SHIPFEE"), Long)
                        ShipBurdenFee(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("SHIPBURDENFEE"), Long)
                        PickupFee(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("PICKUPFEE"), Long)
                        If row("INOUTSIDEKBN").ToString = "1" Then
                            Quantity3(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("QUANTITY"), Long)
                            FreesendFee1(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("FREESENDFEE"), Long)
                        ElseIf row("INOUTSIDEKBN").ToString = "2" Then
                            Quantity4(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("QUANTITY"), Long)
                            FreesendFee2(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("FREESENDFEE"), Long)
                        Else
                            If row("JOTDEPBRANCHCD").ToString = row("JOTARRBRANCHCD").ToString OrElse
                               row("JOTARRBRANCHCD").ToString = "" Then
                                Quantity3(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("QUANTITY"), Long)
                                FreesendFee1(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("FREESENDFEE"), Long)
                            ElseIf row("JOTDEPBRANCHCD").ToString <> row("JOTARRBRANCHCD").ToString AndAlso
                                   row("JOTARRBRANCHCD").ToString <> "" Then
                                Quantity4(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("QUANTITY"), Long)
                                FreesendFee2(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("FREESENDFEE"), Long)
                            End If
                        End If
                        If CType(row("JRFIXEDFARE"), Long) = CType(row("FREESENDFEE"), Long) Then
                            FreesendFee3(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("FREESENDFEE"), Long)
                        Else
                            FreesendFee4(1, SetTrusteeKBN, SetBigCtncdKBN) += CType(row("FREESENDFEE"), Long)
                        End If
                    End If
                Next

                '最後に出力した行を保存
                lastlastRow = lastRow ' 2024/09/20 ver2 星 ADD
                lastRow = row

                If menu = "0" Then ' 2024/09/20 ver2 星 ADD
                    '最終レコードの場合
                    If row_cnt = PrintData.Rows.Count Then
                        '〇全合計
                        EditTotalArea(idx, row, lastRow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                          Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                          FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFreeKBN, ReportType, OfficeCode, 1, 1)
                        Exit For
                    End If
                End If ' 2024/09/20 ver2 星 ADD

            Next

            ' 2024/09/20 ver2 星 ADD START
            If menu = "1" Then
                If lastlastRow Is Nothing Then
                    '〇全合計
                    EditTotalArea(idx, lastRow, lastRow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                                      Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                                      FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFreeKBN, ReportType, OfficeCode, 1, 1)
                Else
                    '〇全合計
                    EditTotalArea(idx, lastRow, lastlastRow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                                      Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                                      FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFreeKBN, ReportType, OfficeCode, 1, 1)
                End If
            End If
            ' 2024/09/20 ver2 星 ADD END

            'テンプレート削除
            WW_Workbook.Worksheets(WW_tmpSheetNo).Delete()
            ' 2024/09/20 ver2 星 ADD START
            If menu = "1" Then
                WW_Workbook.Worksheets(0).Delete()
            End If
            ' 2024/09/20 ver2 星 ADD END

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
    Private Sub EditHeaderArea(
        ByRef idx As Integer,
        ByVal ShipYMD As Date,
        ByVal jotdepbranchnm As String,
        ByVal pageNum As Integer,
        ByVal ReportType As String,
        ByVal jotdepbranchcd As String
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try

            '営業所選択時名称取得
            Select Case jotdepbranchcd
                Case "010104"
                    jotdepbranchnm = "帯広営業所"
                Case "011501"
                    jotdepbranchnm = "新潟営業所"
                Case "011316"
                    jotdepbranchnm = "隅田川営業所"
                Case "011317"
                    jotdepbranchnm = "大井営業所"
                Case "012801"
                    jotdepbranchnm = "水島営業所"
                Case "013501"
                    jotdepbranchnm = "徳山営業所"
            End Select

            'ヘッダー行コピー
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A2:V5")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            '〇機能
            WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = "LNT0012"
            '◯ 出力日
            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = ShipYMD
            '〇タイトル
            If ReportType = "B" Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = "コンテナ　発送日報明細（Ｂ）"
            End If
            '頁数
            WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = pageNum
            '〇処理日
            WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = Now
            '支店名
            WW_Workbook.Worksheets(WW_SheetNo).Range("R" + (idx + 1).ToString()).Value = jotdepbranchnm
            'ヘッダーFLG
            WW_Workbook.Worksheets(WW_SheetNo).Range("W" + (idx + 3).ToString()).Value = "0"

            If idx > 58 Then
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:V{0}", idx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            idx += 4

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ' 2024/09/20 ver2 星 ADD START
    ''' <summary>
    ''' Excel作業シート設定
    ''' </summary>
    ''' <param name="sheetName"></param>
    Protected Function TrySetExcelWorkSheet(ByRef idx As Integer, ByVal sheetName As String, ByRef PageNum As Integer, Optional ByVal templateSheetName As String = Nothing) As Boolean
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
                idx = 1
            End If

        Catch ex As Exception
            WW_Workbook = Nothing
            Throw
        End Try
        Return result
    End Function
    ' 2024/09/20 ver2 星 ADD END

    ''' <summary>
    ''' 帳票の明細設定
    ''' </summary>
    Private Sub EditDetailArea(
         ByRef idx As Integer,
         ByVal row As DataRow,
         ByVal lastrow As DataRow,
         ByRef PageNum As Integer,
         ByVal Mode As Integer,
         ByVal ReportType As String
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim StackFreeKBN As Integer = 0
        Dim Modcnt As Integer = 0
        Dim DetailArea As IRange = Nothing
        Dim TotalRowFLG As String = WW_Workbook.Worksheets(WW_SheetNo).Range("W" + (idx - 1).ToString()).Text
        Dim BEFDEPTRUSTEECD As Integer = 0
        Dim BEFDEPTRUSTEESUBCD As Integer = 0
        Dim BEFDEPSHIPPERCD As Integer = 0
        Dim BEFARRTRUSTEECD As Integer = 0
        Dim BEFARRTRUSTEESUBCD As Integer = 0
        Dim BEFJRITEMCD As Integer = 0
        Dim BEFSTACKFREEKBN As Integer = 0
        Dim BEFFLG As String = "0"

        '積空判断
        If row("STACKFREEKBN") IsNot DBNull.Value Then
            StackFreeKBN = CType(row("STACKFREEKBN"), Integer)
        End If

        '明細行コピー
        If row("FILETYPE").ToString() = "1" OrElse row("FILETYPE").ToString() = "3" Then
            If StackFreeKBN = 1 Then
                If CInt(row("JRFIXEDFARE")) = 0 And
                   CInt(row("OWNDISCOUNTFEE")) = 0 And
                   CInt(row("USEFEE")) = 0 Then
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A27:V27")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                    srcRange.Copy(destRange)
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                Else
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A8:V8")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                    srcRange.Copy(destRange)
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                End If
            ElseIf StackFreeKBN = 2 Then
                If CInt(row("FREESENDFEE")) = 0 And
                   CInt(row("SHIPFEE")) = 0 And
                   CInt(row("SHIPBURDENFEE")) = 0 And
                   CInt(row("PICKUPFEE")) = 0 Then
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A27:V27")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                    srcRange.Copy(destRange)
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                Else
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A8:V8")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                    srcRange.Copy(destRange)
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                End If
            End If
        ElseIf row("FILETYPE").ToString() = "2" Then
            srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A25:V25")
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
        End If

        '変換前項目
        If row("BEFDEPTRUSTEECD") IsNot DBNull.Value Then
            BEFDEPTRUSTEECD = CType(row("BEFDEPTRUSTEECD"), Integer)
            If BEFDEPTRUSTEECD <> 0 Then
                BEFFLG = "1"
            End If
        End If
        If row("BEFDEPTRUSTEESUBCD") IsNot DBNull.Value Then
            BEFDEPTRUSTEESUBCD = CType(row("BEFDEPTRUSTEESUBCD"), Integer)
            If BEFDEPTRUSTEESUBCD <> 0 Then
                BEFFLG = "1"
            End If
        End If
        If row("BEFDEPSHIPPERCD") IsNot DBNull.Value Then
            BEFDEPSHIPPERCD = CType(row("BEFDEPSHIPPERCD"), Integer)
            If BEFDEPSHIPPERCD <> 0 Then
                BEFFLG = "1"
            End If
        End If
        If row("BEFARRTRUSTEECD") IsNot DBNull.Value Then
            BEFARRTRUSTEECD = CType(row("BEFARRTRUSTEECD"), Integer)
            If BEFARRTRUSTEECD <> 0 Then
                BEFFLG = "1"
            End If
        End If
        If row("BEFARRTRUSTEESUBCD") IsNot DBNull.Value Then
            BEFARRTRUSTEESUBCD = CType(row("BEFARRTRUSTEESUBCD"), Integer)
            If BEFARRTRUSTEESUBCD <> 0 Then
                BEFFLG = "1"
            End If
        End If
        If row("BEFJRITEMCD") IsNot DBNull.Value Then
            BEFJRITEMCD = CType(row("BEFJRITEMCD"), Integer)
            If BEFJRITEMCD <> 0 Then
                BEFFLG = "1"
            End If
        End If
        If row("BEFSTACKFREEKBN") IsNot DBNull.Value Then
            BEFSTACKFREEKBN = CType(row("BEFSTACKFREEKBN"), Integer)
            If BEFSTACKFREEKBN <> 0 Then
                BEFFLG = "1"
            End If
        End If

        'セット
        If row("FILETYPE").ToString() = "1" OrElse row("FILETYPE").ToString() = "3" Then
            '発駅名称、発受託人名称
            WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = row("DEPSTATIONNM")
            If BEFFLG = "1" Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = "M" & row("DEPTRUSTEENM").ToString
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = row("DEPTRUSTEENM")
            End If

            If TotalRowFLG <> "0" And lastrow IsNot Nothing Then
                If row("DEPSTATIONNM").ToString = lastrow("DEPSTATIONNM").ToString Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = ""
                    If row("DEPTRUSTEENM").ToString = lastrow("DEPTRUSTEENM").ToString AndAlso row("DEPTRUSTEECD").ToString = lastrow("DEPTRUSTEECD").ToString Then
                        If BEFFLG = "1" Then
                            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = "M"
                        Else
                            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = ""
                        End If

                    End If
                End If
            End If
            'コンテナ記号
            If row("BIGCTNCD").ToString <> "05" Then
                If row("CTNTYPE").ToString = "KAGEN" Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = ""
                Else
                    If row("BIGCTNCD").ToString = "15" Then
                        If Not row("CTNTYPE").ToString = "" Then
                            WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = "*" & row("CTNTYPE").ToString
                        End If
                    Else
                        If row("ADDITEM2NAME").ToString <> "　　　" Then
                            WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = row("ADDITEM2NAME").ToString & row("CTNTYPE").ToString
                        Else
                            If row("ADDITEM10NAME").ToString <> "　　　" Then
                                WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = row("ADDITEM10NAME").ToString & row("CTNTYPE").ToString
                            Else
                                WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = row("CTNTYPE").ToString
                            End If
                        End If
                    End If
                End If
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = ""
            End If
            'コンテナ番号
            If row("CTNNO").ToString = "0" Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = ""
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = row("CTNNO")
            End If
            '変更後着駅名称
            If row("AFTERARRSTATIONNM") IsNot DBNull.Value Then
                If row("DEPSTATIONNM").ToString <> row("AFTERARRSTATIONNM").ToString Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = row("AFTERARRSTATIONNM")
                End If
            End If
            '発荷主名称
            WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = row("DEPSHIPPERNM")
            'JR荷送人名
            WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = row("JRSHIPPERNM")
            '品名
            WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = row("ITEMNAME")
            '通運記載品目名
            WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = row("JRITEMNM")

            '列車番号(発)
            WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = row("DEPTRAINNO")
            '着駅名称
            WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = row("ARRSTATIONNM")
            If Not row("ARRPLANMM").ToString = "" AndAlso
               Not row("ARRPLANDD").ToString = "" Then
                '到着年月
                WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = row("ARRPLANMM").ToString & "." & row("ARRPLANDD").ToString
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = ""
            End If
            '列車番号(着)
            WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = row("ARRTRAINNO")
            '着受託人名称
            WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = row("ARRTRUSTEENM")
            '契約コード
            WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = row("CONTRACTCD")
            '私有割引相当額
            WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = row("OWNDISCOUNTFEE")
            If StackFreeKBN = 1 Then
                'ＪＲ所定運賃
                WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = row("JRFIXEDFARE")
                '使用料金額
                WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = row("USEFEE")
                '通運負担回送運賃
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = row("NITTSUFREESEND")
                '運行管理
                WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = row("MANAGEFEE")
                '荷主負担運賃
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = row("SHIPBURDENFEE")
                '集荷料
                WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = row("PICKUPFEE")
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = 0
                WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = 0
                '回送運賃
                If CType(row("JRFIXEDFARE"), Long) = CType(row("FREESENDFEE"), Long) Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).NumberFormat = "(" & "    " & "#,##0" & ")"
                Else
                    WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).NumberFormat = "<" & "    " & "#,##0" & ">"
                    If row("CONTRACTCD").ToString = "N6999" OrElse row("CONTRACTCD").ToString = "N7999" OrElse
                        row("CONTRACTCD").ToString = "N8999" OrElse row("CONTRACTCD").ToString = "N9999" Then
                        '契約コード
                        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = row("CONTRACTCD").ToString & "*"
                    End If
                End If
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = row("FREESENDFEE")
                '発送料
                WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).NumberFormat = "(" & "    " & "#,##0" & ")"
                WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = row("SHIPFEE")
                '荷主負担運賃
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = 0
                '集荷料
                WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = 0
            End If
        ElseIf row("FILETYPE").ToString() = "2" Then
            '発駅名称、発受託人名称
            WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = row("DEPSTATIONNM")
            If BEFFLG = "1" Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = "M" & row("DEPTRUSTEENM").ToString
            Else
                WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = row("DEPTRUSTEENM")
            End If

            If TotalRowFLG <> "0" And lastrow IsNot Nothing Then
                If row("DEPSTATIONNM").ToString = lastrow("DEPSTATIONNM").ToString Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = ""
                    If row("DEPTRUSTEENM").ToString = lastrow("DEPTRUSTEENM").ToString AndAlso row("DEPTRUSTEECD").ToString = lastrow("DEPTRUSTEECD").ToString Then
                        If BEFFLG = "1" Then
                            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = "M"
                        Else
                            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = ""
                        End If

                    End If
                End If
            End If
            If row("FILETYPE").ToString() = "2" Then
                '加減額表示
                WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = "請求加減額"
                'ＪＲ所定運賃
                WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = row("JRFIXEDFARE")
                '私有割引相当額
                WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = row("OWNDISCOUNTFEE")
                '使用料金額
                WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = row("USEFEE")
                '通運負担回送運賃
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = row("NITTSUFREESEND")
                '運行管理
                WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = row("MANAGEFEE")
                '荷主負担運賃
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = row("SHIPBURDENFEE")
                '集荷料
                WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = row("PICKUPFEE")
                'Else
                '    '加減額表示
                '    WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = "支払加減額"
                '    WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = 0
                '    '私有割引相当額
                '    WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = 0
                '    WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = 0
                '    '回送運賃
                '    WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).NumberFormat = "(" & "    " & "#,##0" & ")"
                '    WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = row("FREESENDFEE")
                '    '発送料
                '    WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).NumberFormat = "(" & "    " & "#,##0" & ")"
                '    WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = row("SHIPFEE")
                '    '荷主負担運賃
                '    WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = 0
                '    '集荷料
                '    WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = 0
            End If

        End If

        '罫線設定
        DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "V" + idx.ToString())
        If TotalRowFLG = "1" Then
            DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
        End If
        idx += 1

        '改頁判断
        Modcnt = idx Mod 59
        If Modcnt = 0 Then
            DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, CDate(row("SHIPYMD")), row("JOTDEPBRANCHNM").ToString, PageNum, ReportType, row("OFFICECODE").ToString)
        End If

    End Sub

    ''' <summary>
    ''' 全合計
    ''' </summary>
    Private Sub EditTotalArea(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByVal lastrow As DataRow,
        ByRef PageNum As Integer,
        ByRef Quantity1(,,) As Long,
        ByRef Quantity2(,,) As Long,
        ByRef UseFee1(,,) As Long,
        ByRef Quantity3(,,) As Long,
        ByRef FreesendFee1(,,) As Long,
        ByRef Quantity4(,,) As Long,
        ByRef FreesendFee2(,,) As Long,
        ByRef FixedFare(,,) As Long,
        ByRef OwnDiscountFee(,,) As Long,
        ByRef UseFee2(,,) As Long,
        ByRef FreesendFee3(,,) As Long,
        ByRef NituuFreesend(,,) As Long,
        ByRef FreesendFee4(,,) As Long,
        ByRef ShipFee(,,) As Long,
        ByRef ShipBurdenFee(,,) As Long,
        ByRef PickupFee(,,) As Long,
        ByVal StackFree As Integer,
        ByVal ReportType As String,
        ByVal Officecode As String,
        ByVal LastFlg As Integer,
        ByVal LastPageFlg As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim BigCtncdKBN As Integer = 1
        Dim COPYFLG As String = "0"
        Dim NAMEFLG As String = "0"
        Dim PAGEFLG As String = "0"
        Dim Modcnt As Integer = 0

        '〇支店計
        EditBranchTotalArea(idx, row, lastrow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                            Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                            FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFree, ReportType, Officecode, 1, LastPageFlg)

        If Officecode = "" Then
            '〇算出
            For TrusteeKBN As Integer = 1 To 6
                While BigCtncdKBN < 10
                    If Quantity1(6, TrusteeKBN, BigCtncdKBN) = 0 AndAlso
                        UseFee1(6, TrusteeKBN, BigCtncdKBN) = 0 AndAlso
                        FreesendFee1(6, TrusteeKBN, BigCtncdKBN) = 0 AndAlso
                        FreesendFee2(6, TrusteeKBN, BigCtncdKBN) = 0 Then
                        BigCtncdKBN += 1
                    Else
                        '合計行コピー
                        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A22:V22")
                        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                        srcRange.Copy(destRange)
                        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                        If COPYFLG = "0" Then
                            WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = "【合計】"
                        Else
                            WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = ""
                        End If
                        '受託人名称セット
                        If NAMEFLG = "0" Then
                            Dim TrusteeName As String = ""
                            If TrusteeKBN = 1 Then
                                TrusteeName = "日本通運"
                            End If
                            If TrusteeKBN = 2 Then
                                TrusteeName = "全国通運"
                            End If
                            If TrusteeKBN = 3 Then
                                TrusteeName = "日本フレート"
                            End If
                            If TrusteeKBN = 4 Then
                                TrusteeName = "地区通運"
                            End If
                            If TrusteeKBN = 5 Then
                                TrusteeName = "その外"
                            End If
                            If TrusteeKBN = 6 Then
                                TrusteeName = "合計"
                            End If
                            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = TrusteeName
                            WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = Quantity1(6, TrusteeKBN, 9)
                        Else
                            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = ""
                        End If
                        '大分類判断

                        '大分類名称セット
                        Dim BigCtncdName As String = ""
                        If BigCtncdKBN = 1 Then
                            BigCtncdName = "UV"
                        End If
                        If BigCtncdKBN = 2 Then
                            BigCtncdName = "UR"
                        End If
                        If BigCtncdKBN = 3 Then
                            BigCtncdName = "SUR"
                        End If
                        If BigCtncdKBN = 4 Then
                            BigCtncdName = "UF"
                        End If
                        If BigCtncdKBN = 5 Then
                            BigCtncdName = "L10"
                        End If
                        If BigCtncdKBN = 6 Then
                            BigCtncdName = "UM"
                        End If
                        If BigCtncdKBN = 7 Then
                            BigCtncdName = "ｳｲﾝ"
                        End If
                        If BigCtncdKBN = 8 Then
                            BigCtncdName = "ｿﾉﾀ"
                        End If
                        If BigCtncdKBN = 9 Then
                            BigCtncdName = "*ｹｲ"
                        End If
                        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = "(" & BigCtncdName & "積"
                        '数量、金額セット
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = Quantity2(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = UseFee1(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Quantity3(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = FreesendFee1(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = Quantity4(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = FreesendFee2(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = FixedFare(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = OwnDiscountFee(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = UseFee2(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = NituuFreesend(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = ShipFee(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = ShipBurdenFee(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = PickupFee(6, TrusteeKBN, BigCtncdKBN)
                        idx += 1
                        '改頁判断
                        Modcnt = 0
                        Modcnt = idx Mod 59
                        If Modcnt = 0 Then
                            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "V" + idx.ToString())
                            DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
                            idx += 1
                            PageNum += 1
                            EditHeaderArea(idx, CDate(lastrow("SHIPYMD")), lastrow("JOTDEPBRANCHNM").ToString, PageNum, ReportType, row("OFFICECODE").ToString)
                        End If
                        '合計行コピー
                        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A23:V23")
                        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                        srcRange.Copy(destRange)
                        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = FreesendFee4(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = FreesendFee3(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = ShipFee(6, TrusteeKBN, BigCtncdKBN)
                        WW_Workbook.Worksheets(WW_SheetNo).Range("W" + idx.ToString()).Value = "1"
                        idx += 1
                        '改頁判断
                        Modcnt = 0
                        Modcnt = idx Mod 59
                        If Modcnt = 0 Then
                            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "V" + idx.ToString())
                            DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
                            If LastFlg = 0 Then
                                idx += 1
                                PageNum += 1
                                EditHeaderArea(idx, CDate(lastrow("SHIPYMD")), lastrow("JOTDEPBRANCHNM").ToString, PageNum, ReportType, row("OFFICECODE").ToString)
                            End If
                            If TrusteeKBN = 6 And BigCtncdKBN = 9 Then
                                PAGEFLG = "1"
                            End If
                        End If
                        If BigCtncdKBN < 10 Then
                            COPYFLG = "1"
                            NAMEFLG = "1"
                            BigCtncdKBN += 1
                        End If
                    End If
                End While
                NAMEFLG = "0"
                BigCtncdKBN = 1
            Next

            '改頁
            If PAGEFLG = "0" Then
                Modcnt = 0
                While 0 = 0
                    Modcnt = idx Mod 59
                    If Modcnt = 0 Then
                        If LastFlg = 0 Then
                            PageNum += 1
                            EditHeaderArea(idx, CDate(row("SHIPYMD")), row("JOTDEPBRANCHNM").ToString, PageNum, ReportType, row("OFFICECODE").ToString)
                        Else
                            Dim pagebreak As IRange = Nothing
                            pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:V{0}", idx))
                            WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
                        End If
                        Exit While
                    Else
                        idx += 1
                    End If
                End While
            End If

            '数量移行
            QuantitySet(Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                        Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                        FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, 6)
        End If

    End Sub

    ''' <summary>
    ''' 支店計
    ''' </summary>
    Private Sub EditBranchTotalArea(
        ByRef idx As Integer,
        ByVal row As DataRow,
        ByVal lastrow As DataRow,
        ByRef PageNum As Integer,
        ByRef Quantity1(,,) As Long,
        ByRef Quantity2(,,) As Long,
        ByRef UseFee1(,,) As Long,
        ByRef Quantity3(,,) As Long,
        ByRef FreesendFee1(,,) As Long,
        ByRef Quantity4(,,) As Long,
        ByRef FreesendFee2(,,) As Long,
        ByRef FixedFare(,,) As Long,
        ByRef OwnDiscountFee(,,) As Long,
        ByRef UseFee2(,,) As Long,
        ByRef FreesendFee3(,,) As Long,
        ByRef NituuFreesend(,,) As Long,
        ByRef FreesendFee4(,,) As Long,
        ByRef ShipFee(,,) As Long,
        ByRef ShipBurdenFee(,,) As Long,
        ByRef PickupFee(,,) As Long,
        ByVal StackFree As Integer,
        ByVal ReportType As String,
        ByVal Officecode As String,
        ByVal LastFlg As Integer,
        ByVal LastPageFlg As Integer
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim BigCtncdKBN As Integer = 1
        Dim COPYFLG As String = "0"
        Dim NAMEFLG As String = "0"
        Dim PAGEFLG As String = "0"
        Dim Modcnt As Integer = 0

        '〇発駅計
        EditStationTotalArea(idx, lastrow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                             Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                             FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFree, ReportType)

        '〇算出
        For TrusteeKBN As Integer = 1 To 6
            While BigCtncdKBN < 10
                If Quantity1(5, TrusteeKBN, BigCtncdKBN) = 0 AndAlso
                    UseFee1(5, TrusteeKBN, BigCtncdKBN) = 0 AndAlso
                    FreesendFee1(5, TrusteeKBN, BigCtncdKBN) = 0 AndAlso
                    FreesendFee2(5, TrusteeKBN, BigCtncdKBN) = 0 Then
                    BigCtncdKBN += 1
                Else
                    '合計行コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A22:V22")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                    srcRange.Copy(destRange)
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                    If COPYFLG = "0" Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = "【合計】"
                    Else
                        WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = ""
                    End If
                    '受託人名称セット
                    If NAMEFLG = "0" Then
                        Dim TrusteeName As String = ""
                        If TrusteeKBN = 1 Then
                            TrusteeName = "日本通運"
                        End If
                        If TrusteeKBN = 2 Then
                            TrusteeName = "全国通運"
                        End If
                        If TrusteeKBN = 3 Then
                            TrusteeName = "日本フレート"
                        End If
                        If TrusteeKBN = 4 Then
                            TrusteeName = "地区通運"
                        End If
                        If TrusteeKBN = 5 Then
                            TrusteeName = "その外"
                        End If
                        If TrusteeKBN = 6 Then
                            TrusteeName = "合計"
                        End If
                        WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = TrusteeName
                        WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = Quantity1(5, TrusteeKBN, 9)
                    Else
                        WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = ""
                    End If
                    '大分類判断

                    '大分類名称セット
                    Dim BigCtncdName As String = ""
                    If BigCtncdKBN = 1 Then
                        BigCtncdName = "UV"
                    End If
                    If BigCtncdKBN = 2 Then
                        BigCtncdName = "UR"
                    End If
                    If BigCtncdKBN = 3 Then
                        BigCtncdName = "SUR"
                    End If
                    If BigCtncdKBN = 4 Then
                        BigCtncdName = "UF"
                    End If
                    If BigCtncdKBN = 5 Then
                        BigCtncdName = "L10"
                    End If
                    If BigCtncdKBN = 6 Then
                        BigCtncdName = "UM"
                    End If
                    If BigCtncdKBN = 7 Then
                        BigCtncdName = "ｳｲﾝ"
                    End If
                    If BigCtncdKBN = 8 Then
                        BigCtncdName = "ｿﾉﾀ"
                    End If
                    If BigCtncdKBN = 9 Then
                        BigCtncdName = "*ｹｲ"
                    End If
                    WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = "(" & BigCtncdName & "積"
                    '数量、金額セット
                    WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = Quantity2(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = UseFee1(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Quantity3(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = FreesendFee1(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = Quantity4(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = FreesendFee2(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = FixedFare(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = OwnDiscountFee(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = UseFee2(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = NituuFreesend(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = ShipFee(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = ShipBurdenFee(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = PickupFee(5, TrusteeKBN, BigCtncdKBN)
                    idx += 1
                    '改頁判断
                    Modcnt = 0
                    Modcnt = idx Mod 59
                    If Modcnt = 0 Then
                        DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "V" + idx.ToString())
                        DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
                        idx += 1
                        PageNum += 1
                        EditHeaderArea(idx, CDate(lastrow("SHIPYMD")), lastrow("JOTDEPBRANCHNM").ToString, PageNum, ReportType, row("OFFICECODE").ToString)
                    End If
                    '合計行コピー
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A23:V23")
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                    srcRange.Copy(destRange)
                    destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                    WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = FreesendFee4(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = FreesendFee3(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = ShipFee(5, TrusteeKBN, BigCtncdKBN)
                    WW_Workbook.Worksheets(WW_SheetNo).Range("W" + idx.ToString()).Value = "1"
                    idx += 1
                    '改頁判断
                    Modcnt = 0
                    Modcnt = idx Mod 59
                    If Modcnt = 0 Then
                        DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "V" + idx.ToString())
                        DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
                        If LastFlg = 0 Then
                            idx += 1
                            PageNum += 1
                            EditHeaderArea(idx, CDate(lastrow("SHIPYMD")), lastrow("JOTDEPBRANCHNM").ToString, PageNum, ReportType, row("OFFICECODE").ToString)
                        Else
                            If Officecode = "" Then
                                idx += 1
                                PageNum += 1
                                EditHeaderArea(idx, CDate(lastrow("SHIPYMD")), lastrow("JOTDEPBRANCHNM").ToString, PageNum, ReportType, row("OFFICECODE").ToString)
                            End If
                        End If
                        If TrusteeKBN = 6 And BigCtncdKBN = 9 Then
                            PAGEFLG = "1"
                        End If
                    End If
                    If BigCtncdKBN < 10 Then
                        COPYFLG = "1"
                        NAMEFLG = "1"
                        BigCtncdKBN += 1
                    End If
                End If
            End While
            NAMEFLG = "0"
            BigCtncdKBN = 1
        Next

        '改頁
        If LastFlg = 0 Then
            If PAGEFLG = "0" Then
                Modcnt = 0
                While 0 = 0
                    Modcnt = idx Mod 59
                    If Modcnt = 0 Then
                        PageNum += 1
                        EditHeaderArea(idx, CDate(row("SHIPYMD")), row("JOTDEPBRANCHNM").ToString, PageNum, ReportType, row("OFFICECODE").ToString)
                        Exit While
                    Else
                        idx += 1
                    End If
                End While
            End If
        Else
            If Not Officecode = "" Then
                If PAGEFLG = "0" Then
                    Modcnt = 0
                    While 0 = 0
                        Modcnt = idx Mod 59
                        If Modcnt = 0 Then
                            If LastPageFlg = 0 Then
                                PageNum += 1
                                EditHeaderArea(idx, CDate(row("SHIPYMD")), row("JOTDEPBRANCHNM").ToString, PageNum, ReportType, row("OFFICECODE").ToString)
                            Else
                                Dim pagebreak As IRange = Nothing
                                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:V{0}", idx))
                                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
                            End If
                            Exit While
                        Else
                            idx += 1
                        End If
                    End While
                End If
            End If
        End If

        '数量移行
        QuantitySet(Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                    Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                    FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, 5)

    End Sub

    ''' <summary>
    ''' 発駅計
    ''' </summary>
    Private Sub EditStationTotalArea(
        ByRef idx As Integer,
        ByVal lastrow As DataRow,
        ByRef PageNum As Integer,
        ByRef Quantity1(,,) As Long,
        ByRef Quantity2(,,) As Long,
        ByRef UseFee1(,,) As Long,
        ByRef Quantity3(,,) As Long,
        ByRef FreesendFee1(,,) As Long,
        ByRef Quantity4(,,) As Long,
        ByRef FreesendFee2(,,) As Long,
        ByRef FixedFare(,,) As Long,
        ByRef OwnDiscountFee(,,) As Long,
        ByRef UseFee2(,,) As Long,
        ByRef FreesendFee3(,,) As Long,
        ByRef NituuFreesend(,,) As Long,
        ByRef FreesendFee4(,,) As Long,
        ByRef ShipFee(,,) As Long,
        ByRef ShipBurdenFee(,,) As Long,
        ByRef PickupFee(,,) As Long,
        ByVal StackFree As Integer,
        ByVal ReportType As String
        )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim BigCtncdKBN As Integer = 1
        Dim COPYFLG As String = "0"
        Dim Modcnt As Integer = 0

        '〇発受託人計
        EditTrusteeTotalArea(idx, lastrow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                             Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                             FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFree, ReportType)

        '〇算出
        While BigCtncdKBN < 10
            If Quantity1(4, 6, BigCtncdKBN) = 0 AndAlso
                UseFee1(4, 6, BigCtncdKBN) = 0 AndAlso
                FreesendFee1(4, 6, BigCtncdKBN) = 0 AndAlso
                FreesendFee2(4, 6, BigCtncdKBN) = 0 Then
                BigCtncdKBN += 1
            Else
                '合計行コピー
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A19:V19")
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                srcRange.Copy(destRange)
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                If COPYFLG = "0" Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = "【発駅計】"
                    WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = Quantity1(4, 6, 9)
                Else
                    WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = ""
                End If
                '大分類名称セット
                Dim BigCtncdName As String = ""
                If BigCtncdKBN = 1 Then
                    BigCtncdName = "UV"
                End If
                If BigCtncdKBN = 2 Then
                    BigCtncdName = "UR"
                End If
                If BigCtncdKBN = 3 Then
                    BigCtncdName = "SUR"
                End If
                If BigCtncdKBN = 4 Then
                    BigCtncdName = "UF"
                End If
                If BigCtncdKBN = 5 Then
                    BigCtncdName = "L10"
                End If
                If BigCtncdKBN = 6 Then
                    BigCtncdName = "UM"
                End If
                If BigCtncdKBN = 7 Then
                    BigCtncdName = "ｳｲﾝ"
                End If
                If BigCtncdKBN = 8 Then
                    BigCtncdName = "ｿﾉﾀ"
                End If
                If BigCtncdKBN = 9 Then
                    BigCtncdName = "*ｹｲ"
                End If
                WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = "(" & BigCtncdName & "積"
                '数量、金額セット
                WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = Quantity2(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = UseFee1(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Quantity3(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = FreesendFee1(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = Quantity4(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = FreesendFee2(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = FixedFare(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = OwnDiscountFee(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = UseFee2(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = NituuFreesend(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = ShipFee(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = ShipBurdenFee(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = PickupFee(4, 6, BigCtncdKBN)
                idx += 1
                '改頁判断
                Modcnt = 0
                Modcnt = idx Mod 59
                If Modcnt = 0 Then
                    DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "V" + idx.ToString())
                    DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
                    idx += 1
                    PageNum += 1
                    EditHeaderArea(idx, CDate(lastrow("SHIPYMD")), lastrow("JOTDEPBRANCHNM").ToString, PageNum, ReportType, lastrow("OFFICECODE").ToString)
                End If
                '合計行コピー
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A20:V20")
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                srcRange.Copy(destRange)
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = FreesendFee4(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = FreesendFee3(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = ShipFee(4, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("W" + idx.ToString()).Value = "1"
                idx += 1
                '改頁判断
                Modcnt = 0
                Modcnt = idx Mod 59
                If Modcnt = 0 Then
                    DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "V" + idx.ToString())
                    DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
                    idx += 1
                    PageNum += 1
                    EditHeaderArea(idx, CDate(lastrow("SHIPYMD")), lastrow("JOTDEPBRANCHNM").ToString, PageNum, ReportType, lastrow("OFFICECODE").ToString)
                End If
                If BigCtncdKBN < 10 Then
                    COPYFLG = "1"
                    BigCtncdKBN += 1
                End If
            End If
        End While

        '数量移行
        QuantitySet(Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                    Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                    FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, 4)

    End Sub

    ''' <summary>
    ''' 発受託人計
    ''' </summary>
    Private Sub EditTrusteeTotalArea(
        ByRef idx As Integer,
        ByVal lastrow As DataRow,
        ByRef PageNum As Integer,
        ByRef Quantity1(,,) As Long,
        ByRef Quantity2(,,) As Long,
        ByRef UseFee1(,,) As Long,
        ByRef Quantity3(,,) As Long,
        ByRef FreesendFee1(,,) As Long,
        ByRef Quantity4(,,) As Long,
        ByRef FreesendFee2(,,) As Long,
        ByRef FixedFare(,,) As Long,
        ByRef OwnDiscountFee(,,) As Long,
        ByRef UseFee2(,,) As Long,
        ByRef FreesendFee3(,,) As Long,
        ByRef NituuFreesend(,,) As Long,
        ByRef FreesendFee4(,,) As Long,
        ByRef ShipFee(,,) As Long,
        ByRef ShipBurdenFee(,,) As Long,
        ByRef PickupFee(,,) As Long,
        ByVal StackFree As Integer,
        ByVal ReportType As String
        )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim BigCtncdKBN As Integer = 1
        Dim COPYFLG As String = "0"
        Dim Modcnt As Integer = 0

        '〇発受託人サブ計
        EditTrusteeSubTotalArea(idx, lastrow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFree, ReportType)

        '〇算出
        While BigCtncdKBN < 10
            If Quantity1(3, 6, BigCtncdKBN) = 0 AndAlso
                UseFee1(3, 6, BigCtncdKBN) = 0 AndAlso
                FreesendFee1(3, 6, BigCtncdKBN) = 0 AndAlso
                FreesendFee2(3, 6, BigCtncdKBN) = 0 Then
                BigCtncdKBN += 1
            Else
                '改頁判断

                '合計行コピー
                If COPYFLG = "0" Then
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A15:V15")
                ElseIf COPYFLG = "1" Then
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A16:V16")
                ElseIf COPYFLG = "2" Then
                    srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A17:V17")
                End If
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                srcRange.Copy(destRange)
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
                If COPYFLG = "0" Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = Quantity1(3, 6, 9)
                Else
                    DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                    DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.None
                End If

                '大分類名称セット
                Dim BigCtncdName As String = ""
                If BigCtncdKBN = 1 Then
                    BigCtncdName = "UV"
                End If
                If BigCtncdKBN = 2 Then
                    BigCtncdName = "UR"
                End If
                If BigCtncdKBN = 3 Then
                    BigCtncdName = "SUR"
                End If
                If BigCtncdKBN = 4 Then
                    BigCtncdName = "UF"
                End If
                If BigCtncdKBN = 5 Then
                    BigCtncdName = "L10"
                End If
                If BigCtncdKBN = 6 Then
                    BigCtncdName = "UM"
                End If
                If BigCtncdKBN = 7 Then
                    BigCtncdName = "ｳｲﾝ"
                End If
                If BigCtncdKBN = 8 Then
                    BigCtncdName = "ｿﾉﾀ"
                End If
                If BigCtncdKBN = 9 Then
                    BigCtncdName = "*ｹｲ"
                End If
                WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = "(" & BigCtncdName & "積"
                '数量、金額セット
                WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = Quantity2(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = UseFee1(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Quantity3(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = FreesendFee1(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = Quantity4(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = FreesendFee2(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = FixedFare(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = OwnDiscountFee(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = UseFee2(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = NituuFreesend(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = ShipFee(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = ShipBurdenFee(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = PickupFee(3, 6, BigCtncdKBN)
                WW_Workbook.Worksheets(WW_SheetNo).Range("W" + idx.ToString()).Value = "1"
                idx += 1
                '改頁判断
                Modcnt = 0
                Modcnt = idx Mod 59
                If Modcnt = 0 Then
                    DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "V" + idx.ToString())
                    DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
                    idx += 1
                    PageNum += 1
                    EditHeaderArea(idx, CDate(lastrow("SHIPYMD")), lastrow("JOTDEPBRANCHNM").ToString, PageNum, ReportType, lastrow("OFFICECODE").ToString)
                End If
                If BigCtncdKBN < 10 Then
                    If COPYFLG = "0" Then
                        COPYFLG = "1"
                    ElseIf COPYFLG = "1" Then
                        COPYFLG = "2"
                    End If
                    BigCtncdKBN += 1
                End If
            End If
        End While

        '数量移行
        QuantitySet(Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                    Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                    FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, 3)

    End Sub

    ''' <summary>
    ''' 発受託人サブ計
    ''' </summary>
    Private Sub EditTrusteeSubTotalArea(
        ByRef idx As Integer,
        ByVal lastrow As DataRow,
        ByRef PageNum As Integer,
        ByRef Quantity1(,,) As Long,
        ByRef Quantity2(,,) As Long,
        ByRef UseFee1(,,) As Long,
        ByRef Quantity3(,,) As Long,
        ByRef FreesendFee1(,,) As Long,
        ByRef Quantity4(,,) As Long,
        ByRef FreesendFee2(,,) As Long,
        ByRef FixedFare(,,) As Long,
        ByRef OwnDiscountFee(,,) As Long,
        ByRef UseFee2(,,) As Long,
        ByRef FreesendFee3(,,) As Long,
        ByRef NituuFreesend(,,) As Long,
        ByRef FreesendFee4(,,) As Long,
        ByRef ShipFee(,,) As Long,
        ByRef ShipBurdenFee(,,) As Long,
        ByRef PickupFee(,,) As Long,
        ByVal StackFree As Integer,
        ByVal ReportType As String
        )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0

        '〇積空区分別計
        EditStackFreeTotalArea(idx, lastrow, PageNum, Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                                   Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                                   FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, StackFree, ReportType)
        '〇算出
        '合計行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A13:V13")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
        WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = Quantity1(2, 6, 9)
        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = FixedFare(2, 6, 9)
        WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = OwnDiscountFee(2, 6, 9)
        WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = UseFee2(2, 6, 9)
        WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = NituuFreesend(2, 6, 9)
        WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = ShipFee(2, 6, 9)
        WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = ShipBurdenFee(2, 6, 9)
        WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = PickupFee(2, 6, 9)
        WW_Workbook.Worksheets(WW_SheetNo).Range("W" + idx.ToString()).Value = "1"
        idx += 1
        '改頁判断
        Modcnt = 0
        Modcnt = idx Mod 59
        If Modcnt = 0 Then
            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "V" + idx.ToString())
            DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, CDate(lastrow("SHIPYMD")), lastrow("JOTDEPBRANCHNM").ToString, PageNum, ReportType, lastrow("OFFICECODE").ToString)
        End If

        '数量移行
        QuantitySet(Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                    Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                    FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, 2)


    End Sub

    ''' <summary>
    ''' 積空区分別計
    ''' </summary>
    Private Sub EditStackFreeTotalArea(
        ByRef idx As Integer,
        ByVal lastrow As DataRow,
        ByRef PageNum As Integer,
        ByRef Quantity1(,,) As Long,
        ByRef Quantity2(,,) As Long,
        ByRef UseFee1(,,) As Long,
        ByRef Quantity3(,,) As Long,
        ByRef FreesendFee1(,,) As Long,
        ByRef Quantity4(,,) As Long,
        ByRef FreesendFee2(,,) As Long,
        ByRef FixedFare(,,) As Long,
        ByRef OwnDiscountFee(,,) As Long,
        ByRef UseFee2(,,) As Long,
        ByRef FreesendFee3(,,) As Long,
        ByRef NituuFreesend(,,) As Long,
        ByRef FreesendFee4(,,) As Long,
        ByRef ShipFee(,,) As Long,
        ByRef ShipBurdenFee(,,) As Long,
        ByRef PickupFee(,,) As Long,
        ByVal StackFree As Integer,
        ByVal ReportType As String
        )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim DetailArea As IRange = Nothing
        Dim Modcnt As Integer = 0


        If lastrow("FILETYPE").ToString() = "1" Then
            '〇算出
            '合計行コピー
            If StackFree = 1 Then
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A10:V10")
            Else
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A11:V11")
            End If
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
            srcRange.Copy(destRange)
            destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = Quantity1(1, 6, 9)
            WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = FixedFare(1, 6, 9)
            WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + idx.ToString()).Value = OwnDiscountFee(1, 6, 9)
            WW_Workbook.Worksheets(WW_SheetNo).Range("R" + idx.ToString()).Value = UseFee2(1, 6, 9)
            WW_Workbook.Worksheets(WW_SheetNo).Range("S" + idx.ToString()).Value = NituuFreesend(1, 6, 9)
            WW_Workbook.Worksheets(WW_SheetNo).Range("T" + idx.ToString()).Value = ShipFee(1, 6, 9)
            WW_Workbook.Worksheets(WW_SheetNo).Range("U" + idx.ToString()).Value = ShipBurdenFee(1, 6, 9)
            WW_Workbook.Worksheets(WW_SheetNo).Range("V" + idx.ToString()).Value = PickupFee(1, 6, 9)
            WW_Workbook.Worksheets(WW_SheetNo).Range("W" + idx.ToString()).Value = "1"
            idx += 1
            '改頁判断
            Modcnt = 0
            Modcnt = idx Mod 59
            If Modcnt = 0 Then
                DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString() & ":" & "V" + idx.ToString())
                DetailArea.Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
                idx += 1
                PageNum += 1
                EditHeaderArea(idx, CDate(lastrow("SHIPYMD")), lastrow("JOTDEPBRANCHNM").ToString, PageNum, ReportType, lastrow("OFFICECODE").ToString)
            End If
        End If

        '数量移行
        QuantitySet(Quantity1, Quantity2, UseFee1, Quantity3, FreesendFee1,
                    Quantity4, FreesendFee2, FixedFare, OwnDiscountFee, UseFee2,
                    FreesendFee3, NituuFreesend, FreesendFee4, ShipFee, ShipBurdenFee, PickupFee, 1)

    End Sub

    ''' <summary>
    ''' 数量移行
    ''' </summary>
    Private Sub QuantitySet(
        ByRef Quantity1(,,) As Long,
        ByRef Quantity2(,,) As Long,
        ByRef UseFee1(,,) As Long,
        ByRef Quantity3(,,) As Long,
        ByRef FreesendFee1(,,) As Long,
        ByRef Quantity4(,,) As Long,
        ByRef FreesendFee2(,,) As Long,
        ByRef FixedFare(,,) As Long,
        ByRef OwnDiscountFee(,,) As Long,
        ByRef UseFee2(,,) As Long,
        ByRef FreesendFee3(,,) As Long,
        ByRef NituuFreesend(,,) As Long,
        ByRef FreesendFee4(,,) As Long,
        ByRef ShipFee(,,) As Long,
        ByRef ShipBurdenFee(,,) As Long,
        ByRef PickupFee(,,) As Long,
        ByVal Mode As Integer
        )

        Dim NextMode As Integer = 0

        If Mode = 6 Then
            '配列初期化
            For A As Integer = 1 To 6
                For B As Integer = 1 To 6
                    For C As Integer = 1 To 9
                        Quantity1(A, B, C) = 0
                        Quantity2(A, B, C) = 0
                        UseFee1(A, B, C) = 0
                        Quantity3(A, B, C) = 0
                        FreesendFee1(A, B, C) = 0
                        Quantity4(A, B, C) = 0
                        FreesendFee2(A, B, C) = 0
                        FixedFare(A, B, C) = 0
                        OwnDiscountFee(A, B, C) = 0
                        UseFee2(A, B, C) = 0
                        FreesendFee3(A, B, C) = 0
                        NituuFreesend(A, B, C) = 0
                        FreesendFee4(A, B, C) = 0
                        ShipFee(A, B, C) = 0
                        ShipBurdenFee(A, B, C) = 0
                        PickupFee(A, B, C) = 0
                    Next
                Next
            Next
        Else
            NextMode = Mode + 1
            For TrusteeKBN As Integer = 1 To 6
                For BigCtncdKBN As Integer = 1 To 9
                    'セット
                    Quantity1(NextMode, TrusteeKBN, BigCtncdKBN) += Quantity1(Mode, TrusteeKBN, BigCtncdKBN)
                    Quantity2(NextMode, TrusteeKBN, BigCtncdKBN) += Quantity2(Mode, TrusteeKBN, BigCtncdKBN)
                    UseFee1(NextMode, TrusteeKBN, BigCtncdKBN) += UseFee1(Mode, TrusteeKBN, BigCtncdKBN)
                    Quantity3(NextMode, TrusteeKBN, BigCtncdKBN) += Quantity3(Mode, TrusteeKBN, BigCtncdKBN)
                    FreesendFee1(NextMode, TrusteeKBN, BigCtncdKBN) += FreesendFee1(Mode, TrusteeKBN, BigCtncdKBN)
                    Quantity4(NextMode, TrusteeKBN, BigCtncdKBN) += Quantity4(Mode, TrusteeKBN, BigCtncdKBN)
                    FreesendFee2(NextMode, TrusteeKBN, BigCtncdKBN) += FreesendFee2(Mode, TrusteeKBN, BigCtncdKBN)
                    FixedFare(NextMode, TrusteeKBN, BigCtncdKBN) += FixedFare(Mode, TrusteeKBN, BigCtncdKBN)
                    OwnDiscountFee(NextMode, TrusteeKBN, BigCtncdKBN) += OwnDiscountFee(Mode, TrusteeKBN, BigCtncdKBN)
                    UseFee2(NextMode, TrusteeKBN, BigCtncdKBN) += UseFee2(Mode, TrusteeKBN, BigCtncdKBN)
                    FreesendFee3(NextMode, TrusteeKBN, BigCtncdKBN) += FreesendFee3(Mode, TrusteeKBN, BigCtncdKBN)
                    NituuFreesend(NextMode, TrusteeKBN, BigCtncdKBN) += NituuFreesend(Mode, TrusteeKBN, BigCtncdKBN)
                    FreesendFee4(NextMode, TrusteeKBN, BigCtncdKBN) += FreesendFee4(Mode, TrusteeKBN, BigCtncdKBN)
                    ShipFee(NextMode, TrusteeKBN, BigCtncdKBN) += ShipFee(Mode, TrusteeKBN, BigCtncdKBN)
                    ShipBurdenFee(NextMode, TrusteeKBN, BigCtncdKBN) += ShipBurdenFee(Mode, TrusteeKBN, BigCtncdKBN)
                    PickupFee(NextMode, TrusteeKBN, BigCtncdKBN) += PickupFee(Mode, TrusteeKBN, BigCtncdKBN)
                    'クリア
                    Quantity1(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    Quantity2(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    UseFee1(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    Quantity3(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    FreesendFee1(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    Quantity4(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    FreesendFee2(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    FixedFare(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    OwnDiscountFee(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    UseFee2(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    FreesendFee3(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    NituuFreesend(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    FreesendFee4(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    ShipFee(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    ShipBurdenFee(Mode, TrusteeKBN, BigCtncdKBN) = 0
                    PickupFee(Mode, TrusteeKBN, BigCtncdKBN) = 0
                Next
            Next
        End If

    End Sub
End Class
