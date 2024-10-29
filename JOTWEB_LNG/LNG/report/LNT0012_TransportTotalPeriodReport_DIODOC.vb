Option Strict On
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
''' <summary>
''' 発駅・通運別合計表(期間)帳票作成クラス
''' </summary>
Public Class LNT0012_TransportTotalPeriodReport_DIODOC

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
                If WW_Workbook.Worksheets(i).Name = "発駅・通運別合計表(期間)" Then
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
    Public Function CreateExcelPrintData(Sort As String, addsub As String) As String
        Dim ReportName As String = "発駅・通運別合計表(期間)_"
        Dim tmpFileName As String = ReportName & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte

        Try
            Dim idx As Int32 = 1
            Dim lastRow As DataRow = Nothing
            Dim srcRange As IRange = Nothing
            Dim destRange As IRange = Nothing
            Dim PageNum As Int32 = 1
            Dim row_cnt As Int32 = 0
            Dim fstflg As String = "0"
            Dim Mode As Integer = 0
            Dim Table1(5, 32, 3) As Long
            Dim Table2(5, 32, 3) As Long
            Dim Table3(5, 32, 3) As Long
            Dim Table4(5, 32, 3) As Long
            Dim Table5(5, 32, 3) As Long
            Dim Table6(5, 32, 3) As Long
            Dim Key_Orgcode As String = ""
            Dim Key_Orgcode_Total As String = ""
            Dim Key_Toricode As String = ""
            Dim Key_Toricode_Total As String = ""
            Dim Key_Trustee As Integer = 0
            Dim Key_TrusteeSub As Integer = 0
            Dim Key_Station As Integer = 0
            Dim WW_OrgName As String = ""
            Dim WW_OrgName_Total As String = ""
            Dim WW_ToriName As String = ""
            Dim WW_ToriName_Total As String = ""
            Dim WW_TrusteeName As String = ""
            Dim WW_TrusteeSubName As String = ""
            Dim WW_StationName As String = ""
            Dim WW_PartnerCamp As String = ""
            Dim WB_OrgName As String = ""
            Dim WB_ToriName As String = ""
            Dim WB_TrusteeName As String = ""
            Dim WB_TrusteeSubName As String = ""
            Dim WB_StationName As String = ""
            Dim WB_InvKeijoBranch As String = ""
            Dim WB_InvFilingDepT As String = ""
            Dim WB_Quantity As Long = 0
            Dim WB_UseFee As Long = 0
            Dim WB_NittuFreesend As Long = 0
            Dim WB_ShipBurdenFee As Long = 0
            Dim WB_ManageFee As Long = 0
            Dim SkipFLG As String = "0"

            For i As Integer = 1 To 5
                For j As Integer = 1 To 32
                    For k As Integer = 1 To 3
                        Table1(i, j, k) = 0
                        Table2(i, j, k) = 0
                        Table3(i, j, k) = 0
                        Table4(i, j, k) = 0
                        Table5(i, j, k) = 0
                        Table6(i, j, k) = 0
                    Next
                Next
            Next

            For Each row As DataRow In PrintData.Rows

                row_cnt += 1

                '最終レコードの場合
                If row_cnt = PrintData.Rows.Count Then
                    'EditAddArea(row, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee)
                    'EditCalcArea(row, Table1, Table2, Table3, Table4, Table5, Table6, addsub)
                    lastRow = row
                    'Exit For
                End If

                'LT00.

                '1行目
                If fstflg = "0" Then
                    '〇ヘッダー情報セット
                    EditHeaderArea(idx, row, PageNum, Sort)
                    Key_Orgcode = row("ORGCODE").ToString
                    Key_Orgcode_Total = row("ORGCODE").ToString
                    Key_Toricode = row("TORICODE").ToString
                    Key_Toricode_Total = row("TORICODE").ToString
                    Key_Trustee = CType(row("DEPTRUSTEECD"), Integer)
                    Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer)
                    Key_Station = CType(row("DEPSTATIONCD"), Integer)
                    WW_OrgName = row("ORGNAME").ToString
                    WW_OrgName_Total = row("ORGNAME").ToString
                    WW_ToriName = row("TORINAME").ToString
                    WW_ToriName_Total = row("TORINAME").ToString
                    WW_TrusteeName = row("DEPTRUSTEENM").ToString
                    WW_TrusteeSubName = row("DEPTRUSTEESUBNM").ToString
                    WW_StationName = row("DEPSTATIONNM").ToString
                    WW_PartnerCamp = row("PARTNERCAMPCD").ToString
                    WB_OrgName = row("ORGNAME").ToString
                    WB_ToriName = row("TORINAME").ToString
                    WB_TrusteeName = row("DEPTRUSTEENM").ToString
                    WB_TrusteeSubName = row("DEPTRUSTEESUBNM").ToString
                    WB_StationName = row("DEPSTATIONNM").ToString
                    WB_InvKeijoBranch = row("INVKEIJYOBRANCHCD").ToString
                    WB_InvFilingDepT = row("INVFILINGDEPT").ToString
                    '明細ヘッダー情報(数量加算)
                    EditAddArea(row, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee, addsub)
                    '明細行表示内容計算(テーブル加算)
                    EditCalcArea(row, Table1, Table2, Table3, Table4, Table5, Table6, addsub)
                    fstflg = "1"
                    lastRow = row
                    Continue For
                End If

                If Sort <> "2" Then
                    'LT11.
                    SkipFLG = "0"

                    '取引先名称が一つ前のレコードと別の場合
                    If row("TORINAME").ToString <> WB_ToriName Then
                        SkipFLG = "1"
                    End If

                    If SkipFLG = "0" Then
                        '　一つ前のレコードと発受託人コード、発受託人サブコード、発駅コードが一致するかつ、追加明細以外の場合
                        If Key_Trustee = CType(row("DEPTRUSTEECD"), Integer) AndAlso Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer) AndAlso Key_Station = CType(row("DEPSTATIONCD"), Integer) AndAlso CType(row("RECODETYPE"), Integer) <> 2 Then
                            '明細ヘッダー情報(数量加算)
                            EditAddArea(row, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee, addsub)
                            '明細行表示内容計算(テーブル加算)
                            EditCalcArea(row, Table1, Table2, Table3, Table4, Table5, Table6, addsub)
                            lastRow = row
                            Continue For
                        ElseIf Key_Trustee = CType(row("DEPTRUSTEECD"), Integer) AndAlso Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer) AndAlso Key_Station = CType(row("DEPSTATIONCD"), Integer) AndAlso CType(row("RECODETYPE"), Integer) = 2 Then
                            '一つ前のレコードと発受託人コード、発受託人サブコード、発駅コードが一致するかつ　追加明細の場合
                            '一つ前のレコードと処理中のレコードのレコードが一致しない場合、これまでの明細と合計を出力
                            If lastRow("RECODETYPE").ToString <> row("RECODETYPE").ToString Then
                                '明細ヘッダーセット
                                EditDetailArea(idx, row, PageNum, Sort, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WB_OrgName, WB_ToriName, WB_TrusteeName, WB_TrusteeSubName, WB_StationName,
                                           WB_InvKeijoBranch, WB_InvFilingDepT, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee)
                                '明細行出力
                                EditTotalAreaA(idx, row, lastRow, PageNum, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_TrusteeName, WW_TrusteeSubName, WW_StationName,
                                               Table1, Table2, Table3, Table4, Table5, Table6, Sort, "1", "1")
                            End If
                            '追加明細の中で請求年月の変更がある場合
                            If lastRow("KEIJOYM").ToString <> row("KEIJOYM").ToString Then
                                '明細行出力
                                EditTotalAreaA(idx, row, lastRow, PageNum, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_TrusteeName, WW_TrusteeSubName, WW_StationName,
                                               Table1, Table2, Table3, Table4, Table5, Table6, Sort, "1", "2")
                            End If
                            '明細ヘッダー情報(数量加算)
                            EditAddArea(row, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee, addsub)
                            '明細行表示内容計算(テーブル加算)
                            EditCalcArea(row, Table1, Table2, Table3, Table4, Table5, Table6, addsub)
                            lastRow = row
                            Continue For
                        End If
                        '一つ前のレコードと発受託人コード、発受託人サブコード、発駅コードのいずれかが一致しない場合
                        '一つ前のレコードが追加明細以外の場合に明細ヘッダー出力処理を行う
                        If lastRow("RECODETYPE").ToString <> "2" Then
                            '明細ヘッダーセット
                            EditDetailArea(idx, row, PageNum, Sort, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WB_OrgName, WB_ToriName, WB_TrusteeName, WB_TrusteeSubName, WB_StationName,
                                           WB_InvKeijoBranch, WB_InvFilingDepT, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee)
                        Else
                            'トリガーキーの更新
                            Key_Orgcode = row("ORGCODE").ToString
                            Key_Toricode = row("TORICODE").ToString
                            Key_Trustee = CType(row("DEPTRUSTEECD"), Integer)
                            Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer)
                            Key_Station = CType(row("DEPSTATIONCD"), Integer)
                            WB_OrgName = row("ORGNAME").ToString
                            WB_ToriName = row("TORINAME").ToString
                            WB_TrusteeName = row("DEPTRUSTEENM").ToString
                            WB_TrusteeSubName = row("DEPTRUSTEESUBNM").ToString
                            WB_StationName = row("DEPSTATIONNM").ToString
                            WB_InvKeijoBranch = row("INVKEIJYOBRANCHCD").ToString
                            WB_InvFilingDepT = row("INVFILINGDEPT").ToString
                        End If
                        '明細セット
                        EditTotalAreaA(idx, row, lastRow, PageNum, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_TrusteeName, WW_TrusteeSubName, WW_StationName,
                                       Table1, Table2, Table3, Table4, Table5, Table6, Sort, "1", "3")
                        '明細ヘッダー情報(数量加算)
                        EditAddArea(row, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee, addsub)
                        '明細行表示内容計算(テーブル加算)
                        EditCalcArea(row, Table1, Table2, Table3, Table4, Table5, Table6, addsub)
                        lastRow = row
                        Continue For
                    End If

                    '追加明細を出力している場合は、明細ヘッダー行が出力済みの為、skip

                    If lastRow("RECODETYPE").ToString <> "2" Then
                        '明細セット
                        EditDetailArea(idx, row, PageNum, Sort, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WB_OrgName, WB_ToriName, WB_TrusteeName, WB_TrusteeSubName, WB_StationName,
                                           WB_InvKeijoBranch, WB_InvFilingDepT, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee)
                    Else
                        'トリガーキーの更新
                        Key_Orgcode = row("ORGCODE").ToString
                        Key_Toricode = row("TORICODE").ToString
                        Key_Trustee = CType(row("DEPTRUSTEECD"), Integer)
                        Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer)
                        Key_Station = CType(row("DEPSTATIONCD"), Integer)
                        WB_OrgName = row("ORGNAME").ToString
                        WB_ToriName = row("TORINAME").ToString
                        WB_TrusteeName = row("DEPTRUSTEENM").ToString
                        WB_TrusteeSubName = row("DEPTRUSTEESUBNM").ToString
                        WB_StationName = row("DEPSTATIONNM").ToString
                        WB_InvKeijoBranch = row("INVKEIJYOBRANCHCD").ToString
                        WB_InvFilingDepT = row("INVFILINGDEPT").ToString
                    End If
                    If Table2(1, 31, 1) <> Table2(2, 31, 1) OrElse Table2(1, 31, 1) <> Table2(3, 31, 1) Then
                        '合計A
                        EditTotalAreaA(idx, row, lastRow, PageNum, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_TrusteeName, WW_TrusteeSubName, WW_StationName,
                                       Table1, Table2, Table3, Table4, Table5, Table6, Sort, "0", "3")
                    Else
                        For j As Integer = 1 To 32
                            For k As Integer = 1 To 3
                                Table1(1, j, k) = 0
                                Table2(1, j, k) = 0
                                Table3(1, j, k) = 0
                                Table4(1, j, k) = 0
                                Table5(1, j, k) = 0
                                Table6(1, j, k) = 0
                            Next
                        Next
                    End If
                    '発受託人の合計出力
                    EditTotalAreaB(idx, row, PageNum, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_OrgName, WW_ToriName, WW_TrusteeName, WW_TrusteeSubName, WW_StationName, WW_PartnerCamp,
                                   Table1, Table2, Table3, Table4, Table5, Table6, Sort)
                    If Key_Toricode_Total <> row("TORICODE").ToString Then
                        EditTotalAreaE(idx, row, PageNum, Key_Orgcode, Key_Toricode, Key_Toricode_Total, Key_Trustee, Key_TrusteeSub, Key_Station, WW_OrgName, WW_ToriName, WW_ToriName_Total, WW_TrusteeName, WW_TrusteeSubName, WW_StationName, WW_PartnerCamp,
                                           Table1, Table2, Table3, Table4, Table5, Table6, Sort)
                    End If
                    If Key_Orgcode_Total <> row("ORGCODE").ToString Then
                        EditTotalAreaD(idx, row, PageNum, Key_Orgcode, Key_Orgcode_Total, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_OrgName, WW_OrgName_Total, WW_ToriName, WW_TrusteeName, WW_TrusteeSubName, WW_StationName, WW_PartnerCamp,
                                           Table1, Table2, Table3, Table4, Table5, Table6, Sort)
                    End If
                    '明細ヘッダー情報(数量加算)
                    EditAddArea(row, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee, addsub)
                    '明細行表示内容計算(テーブル加算)
                    EditCalcArea(row, Table1, Table2, Table3, Table4, Table5, Table6, addsub)
                    lastRow = row
                    Continue For

                ElseIf Sort = "2" Then      '発駅順、発受託人
                    '一つ前のレコードと発駅が違う場合
                    If CType(row("DEPSTATIONCD"), Integer) <> Key_Station Then
                        '追加明細を出力している場合は、明細ヘッダー行が出力済みの為、skip
                        If lastRow("RECODETYPE").ToString <> "2" Then
                            '明細ヘッダーセット
                            EditDetailArea(idx, row, PageNum, Sort, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WB_OrgName, WB_ToriName, WB_TrusteeName, WB_TrusteeSubName, WB_StationName,
                                               WB_InvKeijoBranch, WB_InvFilingDepT, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee)
                        Else
                            'トリガーキーの更新
                            Key_Orgcode = row("ORGCODE").ToString
                            Key_Toricode = row("TORICODE").ToString
                            Key_Trustee = CType(row("DEPTRUSTEECD"), Integer)
                            Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer)
                            Key_Station = CType(row("DEPSTATIONCD"), Integer)
                            WB_OrgName = row("ORGNAME").ToString
                            WB_ToriName = row("TORINAME").ToString
                            WB_TrusteeName = row("DEPTRUSTEENM").ToString
                            WB_TrusteeSubName = row("DEPTRUSTEESUBNM").ToString
                            WB_StationName = row("DEPSTATIONNM").ToString
                            WB_InvKeijoBranch = row("INVKEIJYOBRANCHCD").ToString
                            WB_InvFilingDepT = row("INVFILINGDEPT").ToString
                        End If
                        '2件以上の受託人の処理が発生した場合
                        If Table2(1, 31, 1) <> Table2(2, 31, 1) OrElse Table2(1, 31, 1) <> Table2(3, 31, 1) Then
                            '明細行出力
                            EditTotalAreaA(idx, row, lastRow, PageNum, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_TrusteeName, WW_TrusteeSubName, WW_StationName,
                                       Table1, Table2, Table3, Table4, Table5, Table6, Sort, "0", "3")
                        Else
                            'データの初期化
                            For j As Integer = 1 To 32
                                For k As Integer = 1 To 3
                                    Table1(1, j, k) = 0
                                    Table2(1, j, k) = 0
                                    Table3(1, j, k) = 0
                                    Table4(1, j, k) = 0
                                    Table5(1, j, k) = 0
                                    Table6(1, j, k) = 0
                                Next
                            Next
                        End If
                        '発受託人の合計出力
                        EditTotalAreaB(idx, row, PageNum, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_OrgName, WW_ToriName, WW_TrusteeName, WW_TrusteeSubName, WW_StationName, WW_PartnerCamp,
                                       Table1, Table2, Table3, Table4, Table5, Table6, Sort)
                        If Key_Toricode_Total <> row("TORICODE").ToString Then
                            EditTotalAreaE(idx, row, PageNum, Key_Orgcode, Key_Toricode, Key_Toricode_Total, Key_Trustee, Key_TrusteeSub, Key_Station, WW_OrgName, WW_ToriName, WW_ToriName_Total, WW_TrusteeName, WW_TrusteeSubName, WW_StationName, WW_PartnerCamp,
                                           Table1, Table2, Table3, Table4, Table5, Table6, Sort)
                        End If
                        If Key_Orgcode_Total <> row("ORGCODE").ToString Then
                            EditTotalAreaD(idx, row, PageNum, Key_Orgcode, Key_Orgcode_Total, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_OrgName, WW_OrgName_Total, WW_ToriName, WW_TrusteeName, WW_TrusteeSubName, WW_StationName, WW_PartnerCamp,
                                           Table1, Table2, Table3, Table4, Table5, Table6, Sort)
                        End If
                        '明細ヘッダー情報(数量加算)
                        EditAddArea(row, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee, addsub)
                        '明細行表示内容計算(テーブル加算)
                        EditCalcArea(row, Table1, Table2, Table3, Table4, Table5, Table6, addsub)
                        lastRow = row
                        Continue For
                    End If

                    '一つ前のレコードと発駅が一致する場合
                    '一つ前のレコードと発受託人コード、発受託人サブコードが一致するかつ　追加明細以外の場合
                    If CType(row("DEPTRUSTEECD"), Integer) = Key_Trustee AndAlso CType(row("DEPTRUSTEESUBCD"), Integer) = Key_TrusteeSub AndAlso CType(row("RECODETYPE"), Integer) <> 2 Then
                        '明細ヘッダー情報(数量加算)
                        EditAddArea(row, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee, addsub)
                        '明細行表示内容計算(テーブル加算)
                        EditCalcArea(row, Table1, Table2, Table3, Table4, Table5, Table6, addsub)
                        lastRow = row
                        Continue For
                    ElseIf Key_Trustee = CType(row("DEPTRUSTEECD"), Integer) AndAlso Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer) AndAlso CType(row("RECODETYPE"), Integer) = 2 Then
                        '一つ前のレコードと発受託人コード、発受託人サブコード、発駅コードが一致するかつ　追加明細の場合
                        '一つ前のレコードと処理中のレコードのレコードが一致しない場合、これまでの明細と合計を出力
                        If lastRow("RECODETYPE").ToString <> row("RECODETYPE").ToString Then
                            '明細ヘッダーセット
                            EditDetailArea(idx, row, PageNum, Sort, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WB_OrgName, WB_ToriName, WB_TrusteeName, WB_TrusteeSubName, WB_StationName,
                                       WB_InvKeijoBranch, WB_InvFilingDepT, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee)
                            '明細行出力
                            EditTotalAreaA(idx, row, lastRow, PageNum, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_TrusteeName, WW_TrusteeSubName, WW_StationName,
                                           Table1, Table2, Table3, Table4, Table5, Table6, Sort, "1", "1")
                        End If
                        '追加明細の中で請求年月の変更がある場合
                        If lastRow("KEIJOYM").ToString <> row("KEIJOYM").ToString Then
                            '明細行出力
                            EditTotalAreaA(idx, row, lastRow, PageNum, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_TrusteeName, WW_TrusteeSubName, WW_StationName,
                                           Table1, Table2, Table3, Table4, Table5, Table6, Sort, "1", "2")
                        End If
                        '明細ヘッダー情報(数量加算)
                        EditAddArea(row, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee, addsub)
                        '明細行表示内容計算(テーブル加算)
                        EditCalcArea(row, Table1, Table2, Table3, Table4, Table5, Table6, addsub)
                        lastRow = row
                        Continue For
                    End If
                    '一つ前のレコードと発受託人コード、発受託人サブコード、発駅コードのいずれかが一致しない場合
                    '一つ前のレコードが追加明細以外の場合に明細ヘッダー出力処理を行う
                    If lastRow("RECODETYPE").ToString <> "2" Then
                        '明細セット
                        EditDetailArea(idx, row, PageNum, Sort, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WB_OrgName, WB_ToriName, WB_TrusteeName, WB_TrusteeSubName, WB_StationName,
                                       WB_InvKeijoBranch, WB_InvFilingDepT, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee)
                    Else
                        'トリガーキーの更新
                        Key_Orgcode = row("ORGCODE").ToString
                        Key_Toricode = row("TORICODE").ToString
                        Key_Trustee = CType(row("DEPTRUSTEECD"), Integer)
                        Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer)
                        Key_Station = CType(row("DEPSTATIONCD"), Integer)
                        WB_OrgName = row("ORGNAME").ToString
                        WB_ToriName = row("TORINAME").ToString
                        WB_TrusteeName = row("DEPTRUSTEENM").ToString
                        WB_TrusteeSubName = row("DEPTRUSTEESUBNM").ToString
                        WB_StationName = row("DEPSTATIONNM").ToString
                        WB_InvKeijoBranch = row("INVKEIJYOBRANCHCD").ToString
                        WB_InvFilingDepT = row("INVFILINGDEPT").ToString
                    End If

                    '明細行出力
                    EditTotalAreaA(idx, row, lastRow, PageNum, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_TrusteeName, WW_TrusteeSubName, WW_StationName,
                                           Table1, Table2, Table3, Table4, Table5, Table6, Sort, "1", "3")
                    '明細ヘッダー情報(数量加算)
                    EditAddArea(row, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee, addsub)
                    '明細行表示内容計算(テーブル加算)
                    EditCalcArea(row, Table1, Table2, Table3, Table4, Table5, Table6, addsub)
                    lastRow = row
                    Continue For
                End If
            Next

            '一つ前のレコードが追加明細以外の場合に明細ヘッダー出力処理を行う
            If lastRow("RECODETYPE").ToString <> "2" Then
                '明細セット
                EditDetailArea(idx, lastRow, PageNum, Sort, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WB_OrgName, WB_ToriName, WB_TrusteeName, WB_TrusteeSubName, WB_StationName,
                                               WB_InvKeijoBranch, WB_InvFilingDepT, WB_Quantity, WB_UseFee, WB_NittuFreesend, WB_ShipBurdenFee, WB_ManageFee)
            End If
            If Table2(1, 31, 1) <> Table2(2, 31, 1) OrElse Table2(1, 31, 1) <> Table2(3, 31, 1) Then
                '明細行出力
                EditTotalAreaA(idx, lastRow, lastRow, PageNum, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_TrusteeName, WW_TrusteeSubName, WW_StationName,
                            Table1, Table2, Table3, Table4, Table5, Table6, Sort, "0", "3")
            Else
                'データの初期化
                For j As Integer = 1 To 32
                    For k As Integer = 1 To 3
                        Table1(1, j, k) = 0
                        Table2(1, j, k) = 0
                        Table3(1, j, k) = 0
                        Table4(1, j, k) = 0
                        Table5(1, j, k) = 0
                        Table6(1, j, k) = 0
                    Next
                Next
            End If
            '合計B
            EditTotalAreaB(idx, lastRow, PageNum, Key_Orgcode, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_OrgName, WW_ToriName, WW_TrusteeName, WW_TrusteeSubName, WW_StationName, WW_PartnerCamp,
                                       Table1, Table2, Table3, Table4, Table5, Table6, Sort)
            EditTotalAreaE(idx, lastRow, PageNum, Key_Orgcode, Key_Toricode, Key_Toricode_Total, Key_Trustee, Key_TrusteeSub, Key_Station, WW_OrgName, WW_ToriName, WW_ToriName_Total, WW_TrusteeName, WW_TrusteeSubName, WW_StationName, WW_PartnerCamp,
                                       Table1, Table2, Table3, Table4, Table5, Table6, Sort)
            EditTotalAreaD(idx, lastRow, PageNum, Key_Orgcode, Key_Orgcode_Total, Key_Toricode, Key_Trustee, Key_TrusteeSub, Key_Station, WW_OrgName, WW_OrgName_Total, WW_ToriName, WW_TrusteeName, WW_TrusteeSubName, WW_StationName, WW_PartnerCamp,
                                       Table1, Table2, Table3, Table4, Table5, Table6, Sort)
            '合計C
            EditTotalAreaC(idx, lastRow, PageNum, Table1, Table2, Table3, Table4, Table5, Table6, Sort)


            'テンプレート削除
            WW_Workbook.Worksheets(WW_tmpSheetNo).Delete()

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
        ByVal row As DataRow,
        ByVal pageNum As Integer,
        ByVal sort As String
     )
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing

        Try
            'ヘッダー行コピー
            If sort = "1" Then
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A2:P5")
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                srcRange.Copy(destRange)
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            ElseIf sort = "2" Then
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A8:P11")
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                srcRange.Copy(destRange)
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))
            End If
            '〇機能
            WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = "LNT0012"

            '〇請求年月(FROM)
            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = row("FROMYMD")
            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).NumberFormat = "yyyy年M月d日～"
            '〇請求年月(TO)
            WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = row("TOYMD")
            WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).NumberFormat = "yyyy年M月d日"
            '〇頁数
            WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = pageNum
            '◯処理日
            WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = DateTime.Now.ToShortDateString
            '◯処理時刻
            WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = DateTime.Now.ToShortTimeString
            '〇締め日
            WW_Workbook.Worksheets(WW_SheetNo).Range("D" + (idx + 1).ToString()).Value = row("HEADER_A")
            '〇ベース
            WW_Workbook.Worksheets(WW_SheetNo).Range("K" + (idx + 1).ToString()).Value = row("HEADER_B")
            '〇加減額
            WW_Workbook.Worksheets(WW_SheetNo).Range("N" + (idx + 1).ToString()).Value = row("HEADER_C")
            '〇支店
            If row("BRANCHNM") IsNot DBNull.Value Then
                WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx + 2).ToString()).Value = row("BRANCHNM")
            End If
            '〇ヘッダーFLG
            WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + (idx + 3).ToString()).Value = "0"

            If idx > 59 Then
                Dim pagebreak As IRange = Nothing
                pagebreak = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("A{0}:P{0}", idx))
                WW_Workbook.Worksheets(WW_SheetNo).HPageBreaks.Add(pagebreak)
            End If

            idx += 4

        Catch ex As Exception
            Throw
        Finally
        End Try
    End Sub

    ''' <summary>
    ''' 帳票の明細設定
    ''' </summary>
    Private Sub EditDetailArea(
         ByRef idx As Integer,
         ByVal row As DataRow,
         ByRef PageNum As Integer,
         ByVal Sort As String,
         ByRef Key_Orgcode As String,
         ByRef Key_Toricode As String,
         ByRef Key_Trustee As Integer,
         ByRef Key_TrusteeSub As Integer,
         ByRef Key_Station As Integer,
         ByRef WB_OrgName As String,
         ByRef WB_ToriName As String,
         ByRef WB_TrusteeName As String,
         ByRef WB_TrusteeSubName As String,
         ByRef WB_StationName As String,
         ByRef WB_InvKeijoBranch As String,
         ByRef WB_InvFilingDepT As String,
         ByRef WB_Quantity As Long,
         ByRef WB_UseFee As Long,
         ByRef WB_NittuFreesend As Long,
         ByRef WB_ShipBurdenFee As Long,
         ByRef WB_ManageFee As Long
     )

        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0

        Dim DetailArea As IRange = Nothing

        '改頁判断
        Modcnt = idx Mod 59
        If Modcnt = 0 Then
            DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx - 1).ToString() & ":" & "P" + (idx - 1).ToString())
            DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
            idx += 1
            PageNum += 1
            EditHeaderArea(idx, row, PageNum, Sort)
        End If

        '明細行コピー
        srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A14:P14")
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
        srcRange.Copy(destRange)
        destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

        '〇セット
        If Sort <> "2" Then
            '請求支店
            WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = WB_OrgName
            '請求先
            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = WB_ToriName
            '発受託人
            WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = WB_TrusteeName
            '部門
            WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = WB_TrusteeSubName
            '発駅
            WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = WB_StationName
            '請求支店
            WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = Key_Orgcode
            '請求先コード
            WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = Key_Toricode
            '発受託人コード
            WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = Key_Trustee
            '発受託人サブコード
            WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = Key_TrusteeSub
            '発駅コード
            WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = Key_Station
        ElseIf Sort = "2" Then
            '請求支店
            WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = WB_OrgName
            '請求先
            WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = WB_ToriName
            '発駅
            WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = WB_StationName
            '発受託人
            WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = WB_TrusteeName
            '部門
            WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = WB_TrusteeSubName
            '請求支店
            WW_Workbook.Worksheets(WW_SheetNo).Range("L" + idx.ToString()).Value = Key_Orgcode
            '請求先コード
            WW_Workbook.Worksheets(WW_SheetNo).Range("M" + idx.ToString()).Value = Key_Toricode
            '発駅コード
            WW_Workbook.Worksheets(WW_SheetNo).Range("N" + idx.ToString()).Value = Key_Station
            '発受託人コード
            WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = Key_Trustee
            '発受託人サブコード
            WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = Key_TrusteeSub
        End If
        '店 収請
        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = WB_InvKeijoBranch & " " & WB_InvFilingDepT
        '個数
        WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = WB_Quantity
        '使用料
        WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = WB_UseFee
        'その他収入
        WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = WB_ManageFee
        '通運負担
        WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = WB_NittuFreesend
        '荷主負担
        WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = WB_ShipBurdenFee

        Key_Orgcode = row("ORGCODE").ToString
        Key_Toricode = row("TORICODE").ToString
        Key_Trustee = CType(row("DEPTRUSTEECD"), Integer)
        Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer)
        Key_Station = CType(row("DEPSTATIONCD"), Integer)
        WB_OrgName = row("ORGNAME").ToString
        WB_ToriName = row("TORINAME").ToString
        WB_TrusteeName = row("DEPTRUSTEENM").ToString
        WB_TrusteeSubName = row("DEPTRUSTEESUBNM").ToString
        WB_StationName = row("DEPSTATIONNM").ToString
        WB_InvKeijoBranch = row("INVKEIJYOBRANCHCD").ToString
        WB_InvFilingDepT = row("INVFILINGDEPT").ToString
        WB_Quantity = 0
        WB_UseFee = 0
        WB_NittuFreesend = 0
        WB_ShipBurdenFee = 0
        WB_ManageFee = 0

        idx += 1

    End Sub

    ''' <summary>
    ''' 合計A
    ''' </summary>
    Private Sub EditTotalAreaA(
         ByRef idx As Integer,
         ByVal row As DataRow,
         ByVal lastrow As DataRow,
         ByRef PageNum As Integer,
         ByRef Key_orgcode As String,
         ByRef Key_Toricode As String,
         ByRef Key_Trustee As Integer,
         ByRef Key_TrusteeSub As Integer,
         ByRef Key_Station As Integer,
         ByRef WW_TrusteeName As String,
         ByRef WW_TrusteeSubName As String,
         ByRef WW_StationName As String,
         ByRef Table1(,,) As Long,
         ByRef Table2(,,) As Long,
         ByRef Table3(,,) As Long,
         ByRef Table4(,,) As Long,
         ByRef Table5(,,) As Long,
         ByRef Table6(,,) As Long,
         ByVal Sort As String,
         ByVal mode As String,
         ByVal Type As String)      '1.作成済み部分(全体未出力)、2.追加明細部分(全体未出力)、3.作成済み部分(全体出力)

        Dim AA As Integer = 0
        Dim BB As Integer = 1
        Dim RepeatFLG As String = "0"
        Dim SkipFLG As String = "0"
        Dim EndFLG As String = "0"
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0

        Dim DetailArea As IRange = Nothing

        While 0 = 0
            If RepeatFLG = "0" Then
                'KAP00.
                AA = 1
            End If

            'KAP01.
            SkipFLG = "0"
            If Table2(1, AA, BB) = 0 AndAlso Table3(1, AA, BB) = 0 Then
                SkipFLG = "1"
            End If

            If SkipFLG = "0" Then

                '改頁判断
                Modcnt = idx Mod 59
                If Modcnt = 0 Then
                    DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx - 1).ToString() & ":" & "P" + (idx - 1).ToString())
                    DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
                    idx += 1
                    PageNum += 1
                    EditHeaderArea(idx, row, PageNum, Sort)
                End If

                '合計行Aコピー
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A17:P17")
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                srcRange.Copy(destRange)
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

                '〇セット
                If AA < 32 Then
                    If BB = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = "加減額"
                    ElseIf BB = 3 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = Left(lastrow("KEIJOYM").ToString, 4) & "年" & Right(lastrow("KEIJOYM").ToString, 2) & "月計上"
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = "追加明細金額"
                    End If
                End If
                EndFLG = "0"

                If Type = "1" Then
                    If AA = 32 Then
                        EndFLG = "1"
                    End If
                ElseIf Type = "2" Then
                    If AA = 32 Then
                        EndFLG = "1"
                    End If
                ElseIf Type = "3" Then
                    If AA = 32 Then
                        If Sort = "1" Then
                            WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = WW_StationName
                        ElseIf Sort = "2" Then
                            WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = WW_TrusteeName
                            WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = WW_TrusteeSubName
                        End If
                    End If
                End If

                If AA = 31 Then
                    If Table6(1, 31, 1) = Table6(1, 32, 3) Then
                        EndFLG = "1"
                    End If
                End If

                If EndFLG = "0" Then
                    '見出し
                    WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = EditMidashiArea(AA, BB)
                    '個数
                    WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = Table1(1, AA, BB)
                    '使用料
                    WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = Table2(1, AA, BB)
                    'その他収入
                    WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Table3(1, AA, BB)
                    '通運負担
                    WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = Table4(1, AA, BB)
                    '荷主負担
                    WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = Table5(1, AA, BB)
                    If AA = 32 Then
                        '請求額
                        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = "請求額"
                        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).NumberFormat = "#,##0"
                        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = Table6(1, AA, BB)
                    End If
                    idx += 1
                End If
            End If

            'KAP15.
            If AA < 32 Then
                AA += 1
                RepeatFLG = "1"
                Continue While
            End If
            If BB < 3 Then
                BB += 1
                RepeatFLG = "0"
                Continue While
            End If
            Exit While
        End While

        If mode = "1" Then
            WW_TrusteeName = row("DEPTRUSTEENM").ToString
            WW_TrusteeSubName = row("DEPTRUSTEESUBNM").ToString
            WW_StationName = row("DEPSTATIONNM").ToString
        End If
        Key_orgcode = row("ORGCODE").ToString
        Key_Toricode = row("TORICODE").ToString
        Key_Trustee = CType(row("DEPTRUSTEECD"), Integer)
        Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer)
        Key_Station = CType(row("DEPSTATIONCD"), Integer)

        '合計行未出力の場合は、合計欄を初期化しない
        If Type = "1" OrElse Type = "2" Then
            For j As Integer = 1 To 31
                For k As Integer = 1 To 3
                    Table1(1, j, k) = 0
                    Table2(1, j, k) = 0
                    Table3(1, j, k) = 0
                    Table4(1, j, k) = 0
                    Table5(1, j, k) = 0
                    Table6(1, j, k) = 0
                Next
            Next
        ElseIf Type = "3" Then
            '合計行を出力済みの場合は、全てを初期化する
            For j As Integer = 1 To 32
                For k As Integer = 1 To 3
                    Table1(1, j, k) = 0
                    Table2(1, j, k) = 0
                    Table3(1, j, k) = 0
                    Table4(1, j, k) = 0
                    Table5(1, j, k) = 0
                    Table6(1, j, k) = 0
                Next
            Next
        End If

    End Sub

    ''' <summary>
    ''' 合計B
    ''' </summary>
    Private Sub EditTotalAreaB(
         ByRef idx As Integer,
         ByVal row As DataRow,
         ByRef PageNum As Integer,
         ByRef Key_orgcode As String,
         ByRef Key_Toricode As String,
         ByRef Key_Trustee As Integer,
         ByRef Key_TrusteeSub As Integer,
         ByRef Key_Station As Integer,
         ByRef WW_OrgName As String,
         ByRef WW_ToriName As String,
         ByRef WW_TrusteeName As String,
         ByRef WW_TrusteeSubName As String,
         ByRef WW_StationName As String,
         ByRef WW_PartnerCamp As String,
         ByRef Table1(,,) As Long,
         ByRef Table2(,,) As Long,
         ByRef Table3(,,) As Long,
         ByRef Table4(,,) As Long,
         ByRef Table5(,,) As Long,
         ByRef Table6(,,) As Long,
         ByVal Sort As String)

        Dim AA As Integer = 0
        Dim BB As Integer = 1
        Dim RepeatFLG As String = "0"
        Dim SkipFLG As String = "0"
        Dim EndFLG As String = "0"
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0

        Dim DetailArea As IRange = Nothing

        While 0 = 0
            If RepeatFLG = "0" Then
                'KAP00.
                AA = 1
            End If

            'KAP01.
            SkipFLG = "0"
            If Table2(2, AA, BB) = 0 AndAlso Table3(2, AA, BB) = 0 Then
                SkipFLG = "1"
            End If

            If SkipFLG = "0" Then
                '改頁判断
                Modcnt = idx Mod 59
                If Modcnt = 0 Then
                    DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx - 1).ToString() & ":" & "P" + (idx - 1).ToString())
                    DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
                    idx += 1
                    PageNum += 1
                    EditHeaderArea(idx, row, PageNum, Sort)
                End If

                '合計行Bコピー
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A20:P20")
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                srcRange.Copy(destRange)
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

                '〇セット
                If AA < 32 Then
                    If BB = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = "加減額"
                    ElseIf BB = 3 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = "追加明細金額"
                    End If
                End If
                If AA = 31 OrElse AA = 32 Then
                    If Sort = "1" Then
                        'WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = WW_OrgName
                        'WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = WW_ToriName
                        WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = WW_TrusteeName
                        WW_Workbook.Worksheets(WW_SheetNo).Range("D" + idx.ToString()).Value = WW_TrusteeSubName
                    ElseIf Sort = "2" Then
                        'WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = WW_OrgName
                        WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = WW_ToriName
                        WW_Workbook.Worksheets(WW_SheetNo).Range("C" + idx.ToString()).Value = WW_StationName
                    End If
                End If

                EndFLG = "0"
                If AA = 31 Then
                    If Table6(2, 31, 1) = Table6(2, 32, 3) Then
                        EndFLG = "1"
                    End If
                End If

                If EndFLG = "0" Then
                    '見出し
                    WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = EditMidashiArea(AA, BB)
                    '個数
                    WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = Table1(2, AA, BB)
                    '使用料
                    WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = Table2(2, AA, BB)
                    'その他収入
                    WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Table3(2, AA, BB)
                    '通運負担
                    WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = Table4(2, AA, BB)
                    '荷主負担
                    WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = Table5(2, AA, BB)
                    If AA = 32 Then
                        '請求額
                        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = "請求額"
                        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).NumberFormat = "#,##0"
                        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = Table6(2, AA, BB)
                    End If
                    idx += 1
                End If
            End If

            'KAP15.
            If AA < 32 Then
                AA += 1
                RepeatFLG = "1"
                Continue While
            End If
            If BB < 3 Then
                BB += 1
                RepeatFLG = "0"
                Continue While
            End If
            Exit While
        End While

        WW_OrgName = row("ORGNAME").ToString
        WW_ToriName = row("TORINAME").ToString
        WW_TrusteeName = row("DEPTRUSTEENM").ToString
        WW_TrusteeSubName = row("DEPTRUSTEESUBNM").ToString
        WW_StationName = row("DEPSTATIONNM").ToString
        WW_PartnerCamp = row("PARTNERCAMPCD").ToString
        Key_orgcode = row("ORGCODE").ToString
        Key_Toricode = row("TORICODE").ToString
        Key_Trustee = CType(row("DEPTRUSTEECD"), Integer)
        Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer)
        Key_Station = CType(row("DEPSTATIONCD"), Integer)
        For j As Integer = 1 To 32
            For k As Integer = 1 To 3
                Table1(2, j, k) = 0
                Table2(2, j, k) = 0
                Table3(2, j, k) = 0
                Table4(2, j, k) = 0
                Table5(2, j, k) = 0
                Table6(2, j, k) = 0
            Next
        Next

    End Sub

    ''' <summary>
    ''' 合計C
    ''' </summary>
    Private Sub EditTotalAreaC(
         ByRef idx As Integer,
         ByVal row As DataRow,
         ByRef PageNum As Integer,
         ByRef Table1(,,) As Long,
         ByRef Table2(,,) As Long,
         ByRef Table3(,,) As Long,
         ByRef Table4(,,) As Long,
         ByRef Table5(,,) As Long,
         ByRef Table6(,,) As Long,
         ByVal Sort As String)

        Dim AA As Integer = 0
        Dim BB As Integer = 1
        Dim RepeatFLG As String = "0"
        Dim SkipFLG As String = "0"
        Dim EndFLG As String = "0"
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0

        Dim DetailArea As IRange = Nothing

        While 0 = 0
            If RepeatFLG = "0" Then
                'KAP00.
                AA = 1
            End If

            'KAP01.
            SkipFLG = "0"
            If Table2(3, AA, BB) = 0 AndAlso Table3(3, AA, BB) = 0 Then
                SkipFLG = "1"
            End If

            If SkipFLG = "0" Then
                '改頁判断
                Modcnt = idx Mod 59
                If Modcnt = 0 Then
                    DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx - 1).ToString() & ":" & "P" + (idx - 1).ToString())
                    DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
                    idx += 1
                    PageNum += 1
                    EditHeaderArea(idx, row, PageNum, Sort)
                End If

                '合計行Cコピー
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A23:P23")
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                srcRange.Copy(destRange)
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

                '〇セット
                If AA < 32 Then
                    If BB = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = "加減額"
                    ElseIf BB = 3 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = "追加明細金額"
                    End If
                End If
                If AA = 31 Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = "〔合　計〕"
                End If
                If AA = 32 Then
                    WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = "〔総合計〕"
                End If

                EndFLG = "0"
                If AA = 31 Then
                    If Table6(3, 31, 1) = Table6(3, 32, 3) Then
                        EndFLG = "1"
                    End If
                End If

                If EndFLG = "0" Then
                    '見出し
                    WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = EditMidashiArea(AA, BB)
                    '個数
                    WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = Table1(3, AA, BB)
                    '使用料
                    WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = Table2(3, AA, BB)
                    'その他収入
                    WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Table3(3, AA, BB)
                    '通運負担
                    WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = Table4(3, AA, BB)
                    '荷主負担
                    WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = Table5(3, AA, BB)
                    If AA = 32 Then
                        '請求額
                        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = "請求額"
                        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).NumberFormat = "#,##0"
                        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = Table6(3, AA, BB)
                    End If
                    idx += 1
                End If
            End If

            'KAP15.
            If AA < 32 Then
                AA += 1
                RepeatFLG = "1"
                Continue While
            End If
            If BB < 3 Then
                BB += 1
                RepeatFLG = "0"
                Continue While
            End If
            Exit While
        End While

        DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx - 1).ToString() & ":" & "L" + (idx - 1).ToString())
        DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin

    End Sub

    ''' <summary>
    ''' 合計D
    ''' </summary>
    Private Sub EditTotalAreaD(
         ByRef idx As Integer,
         ByVal row As DataRow,
         ByRef PageNum As Integer,
         ByRef Key_orgcode As String,
         ByRef Key_orgcode_Total As String,
         ByRef Key_Toricode As String,
         ByRef Key_Trustee As Integer,
         ByRef Key_TrusteeSub As Integer,
         ByRef Key_Station As Integer,
         ByRef WW_OrgName As String,
         ByRef WW_OrgName_Total As String,
         ByRef WW_ToriName As String,
         ByRef WW_TrusteeName As String,
         ByRef WW_TrusteeSubName As String,
         ByRef WW_StationName As String,
         ByRef WW_PartnerCamp As String,
         ByRef Table1(,,) As Long,
         ByRef Table2(,,) As Long,
         ByRef Table3(,,) As Long,
         ByRef Table4(,,) As Long,
         ByRef Table5(,,) As Long,
         ByRef Table6(,,) As Long,
         ByVal Sort As String)

        Dim AA As Integer = 0
        Dim BB As Integer = 1
        Dim RepeatFLG As String = "0"
        Dim SkipFLG As String = "0"
        Dim EndFLG As String = "0"
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0

        Dim DetailArea As IRange = Nothing

        While 0 = 0
            If RepeatFLG = "0" Then
                'KAP00.
                AA = 1
            End If

            'KAP01.
            SkipFLG = "0"
            If Table2(4, AA, BB) = 0 AndAlso Table3(4, AA, BB) = 0 Then
                SkipFLG = "1"
            End If

            If SkipFLG = "0" Then
                '改頁判断
                Modcnt = idx Mod 59
                If Modcnt = 0 Then
                    DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx - 1).ToString() & ":" & "P" + (idx - 1).ToString())
                    DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
                    idx += 1
                    PageNum += 1
                    EditHeaderArea(idx, row, PageNum, Sort)
                End If

                '合計行Dコピー
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A26:P26")
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                srcRange.Copy(destRange)
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

                '〇セット
                If AA < 32 Then
                    If BB = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = "加減額"
                    ElseIf BB = 3 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = "追加明細金額"
                    End If
                End If
                If AA = 31 OrElse AA = 32 Then
                    If Sort = "1" Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = WW_OrgName_Total
                    ElseIf Sort = "2" Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString()).Value = WW_OrgName_Total
                    End If
                End If

                EndFLG = "0"
                If AA = 31 Then
                    If Table6(4, 31, 1) = Table6(4, 32, 3) Then
                        EndFLG = "1"
                    End If
                End If

                If EndFLG = "0" Then
                    If AA = 32 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = "【支店計】"
                    Else
                        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = EditMidashiArea(AA, BB)
                    End If
                    '個数
                    WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = Table1(4, AA, BB)
                    '使用料
                    WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = Table2(4, AA, BB)
                    'その他収入
                    WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Table3(4, AA, BB)
                    '通運負担
                    WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = Table4(4, AA, BB)
                    '荷主負担
                    WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = Table5(4, AA, BB)
                    If AA = 32 Then
                        '請求額
                        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = "請求額"
                        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).NumberFormat = "#,##0"
                        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = Table6(4, AA, BB)
                    End If
                    idx += 1
                End If
            End If

            'KAP15.
            If AA < 32 Then
                AA += 1
                RepeatFLG = "1"
                Continue While
            End If
            If BB < 3 Then
                BB += 1
                RepeatFLG = "0"
                Continue While
            End If
            Exit While
        End While

        WW_OrgName = row("ORGNAME").ToString
        WW_OrgName_Total = row("ORGNAME").ToString
        WW_ToriName = row("TORINAME").ToString
        WW_TrusteeName = row("DEPTRUSTEENM").ToString
        WW_TrusteeSubName = row("DEPTRUSTEESUBNM").ToString
        WW_StationName = row("DEPSTATIONNM").ToString
        WW_PartnerCamp = row("PARTNERCAMPCD").ToString
        Key_orgcode = row("ORGCODE").ToString
        Key_orgcode_Total = row("ORGCODE").ToString
        Key_Toricode = row("TORICODE").ToString
        Key_Trustee = CType(row("DEPTRUSTEECD"), Integer)
        Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer)
        Key_Station = CType(row("DEPSTATIONCD"), Integer)
        For j As Integer = 1 To 32
            For k As Integer = 1 To 3
                Table1(4, j, k) = 0
                Table2(4, j, k) = 0
                Table3(4, j, k) = 0
                Table4(4, j, k) = 0
                Table5(4, j, k) = 0
                Table6(4, j, k) = 0
            Next
        Next

    End Sub

    ''' <summary>
    ''' 合計E
    ''' </summary>
    Private Sub EditTotalAreaE(
         ByRef idx As Integer,
         ByVal row As DataRow,
         ByRef PageNum As Integer,
         ByRef Key_orgcode As String,
         ByRef Key_Toricode As String,
         ByRef Key_Toricode_Total As String,
         ByRef Key_Trustee As Integer,
         ByRef Key_TrusteeSub As Integer,
         ByRef Key_Station As Integer,
         ByRef WW_OrgName As String,
         ByRef WW_ToriName As String,
         ByRef WW_ToriName_Total As String,
         ByRef WW_TrusteeName As String,
         ByRef WW_TrusteeSubName As String,
         ByRef WW_StationName As String,
         ByRef WW_PartnerCamp As String,
         ByRef Table1(,,) As Long,
         ByRef Table2(,,) As Long,
         ByRef Table3(,,) As Long,
         ByRef Table4(,,) As Long,
         ByRef Table5(,,) As Long,
         ByRef Table6(,,) As Long,
         ByVal Sort As String)

        Dim AA As Integer = 0
        Dim BB As Integer = 1
        Dim RepeatFLG As String = "0"
        Dim SkipFLG As String = "0"
        Dim EndFLG As String = "0"
        Dim srcRange As IRange = Nothing
        Dim destRange As IRange = Nothing
        Dim Modcnt As Integer = 0

        Dim DetailArea As IRange = Nothing

        While 0 = 0
            If RepeatFLG = "0" Then
                'KAP00.
                AA = 1
            End If

            'KAP01.
            SkipFLG = "0"
            If Table2(5, AA, BB) = 0 AndAlso Table3(5, AA, BB) = 0 Then
                SkipFLG = "1"
            End If

            If SkipFLG = "0" Then
                '改頁判断
                Modcnt = idx Mod 59
                If Modcnt = 0 Then
                    DetailArea = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + (idx - 1).ToString() & ":" & "P" + (idx - 1).ToString())
                    DetailArea.Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
                    idx += 1
                    PageNum += 1
                    EditHeaderArea(idx, row, PageNum, Sort)
                End If

                '合計行Eコピー
                srcRange = WW_Workbook.Worksheets(WW_tmpSheetNo).Range("A29:P29")
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range("A" + idx.ToString())
                srcRange.Copy(destRange)
                destRange = WW_Workbook.Worksheets(WW_SheetNo).Range(String.Format("{0}:{0}", idx))

                '〇セット
                If AA < 32 Then
                    If BB = 2 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = "加減額"
                    ElseIf BB = 3 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("E" + idx.ToString()).Value = "追加明細金額"
                    End If
                End If
                If AA = 31 OrElse AA = 32 Then
                    If Sort = "1" Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = WW_ToriName_Total
                    ElseIf Sort = "2" Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("B" + idx.ToString()).Value = WW_ToriName_Total
                    End If
                End If

                EndFLG = "0"
                If AA = 31 Then
                    If Table6(5, 31, 1) = Table6(5, 32, 3) Then
                        EndFLG = "1"
                    End If
                End If

                If EndFLG = "0" Then
                    If AA = 32 Then
                        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = "【取引先計】"
                    Else
                        WW_Workbook.Worksheets(WW_SheetNo).Range("F" + idx.ToString()).Value = EditMidashiArea(AA, BB)
                    End If
                    '個数
                    WW_Workbook.Worksheets(WW_SheetNo).Range("G" + idx.ToString()).Value = Table1(5, AA, BB)
                    '使用料
                    WW_Workbook.Worksheets(WW_SheetNo).Range("H" + idx.ToString()).Value = Table2(5, AA, BB)
                    'その他収入
                    WW_Workbook.Worksheets(WW_SheetNo).Range("I" + idx.ToString()).Value = Table3(5, AA, BB)
                    '通運負担
                    WW_Workbook.Worksheets(WW_SheetNo).Range("J" + idx.ToString()).Value = Table4(5, AA, BB)
                    '荷主負担
                    WW_Workbook.Worksheets(WW_SheetNo).Range("K" + idx.ToString()).Value = Table5(5, AA, BB)
                    If AA = 32 Then
                        '請求額
                        WW_Workbook.Worksheets(WW_SheetNo).Range("O" + idx.ToString()).Value = "請求額"
                        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).NumberFormat = "#,##0"
                        WW_Workbook.Worksheets(WW_SheetNo).Range("P" + idx.ToString()).Value = Table6(5, AA, BB)
                    End If
                    idx += 1
                End If
            End If

            'KAP15.
            If AA < 32 Then
                AA += 1
                RepeatFLG = "1"
                Continue While
            End If
            If BB < 3 Then
                BB += 1
                RepeatFLG = "0"
                Continue While
            End If
            Exit While
        End While

        WW_OrgName = row("ORGNAME").ToString
        WW_ToriName = row("TORINAME").ToString
        WW_ToriName_Total = row("TORINAME").ToString
        WW_TrusteeName = row("DEPTRUSTEENM").ToString
        WW_TrusteeSubName = row("DEPTRUSTEESUBNM").ToString
        WW_StationName = row("DEPSTATIONNM").ToString
        WW_PartnerCamp = row("PARTNERCAMPCD").ToString
        Key_orgcode = row("ORGCODE").ToString
        Key_Toricode = row("TORICODE").ToString
        Key_Toricode_Total = row("TORICODE").ToString
        Key_Trustee = CType(row("DEPTRUSTEECD"), Integer)
        Key_TrusteeSub = CType(row("DEPTRUSTEESUBCD"), Integer)
        Key_Station = CType(row("DEPSTATIONCD"), Integer)
        For j As Integer = 1 To 32
            For k As Integer = 1 To 3
                Table1(5, j, k) = 0
                Table2(5, j, k) = 0
                Table3(5, j, k) = 0
                Table4(5, j, k) = 0
                Table5(5, j, k) = 0
                Table6(5, j, k) = 0
            Next
        Next

    End Sub

    ''' <summary>
    ''' 数量加算
    ''' </summary>
    Private Sub EditAddArea(
         ByVal row As DataRow,
         ByRef Quantity As Long,
         ByRef UseFee As Long,
         ByRef NittuFreesend As Long,
         ByRef ShipBurdenFee As Long,
         ByRef ManageFee As Long,
         ByVal addsub As String
     )

        '〇加算
        '○加減額、追加明細金額の除外
        If addsub = "1" Then
            If row("FLG").ToString <> "9" AndAlso row("FLG").ToString <> "8" Then
                Quantity += CType(row("QUANTITY"), Long)
                UseFee += CType(row("USEFEE"), Long)
                NittuFreesend += CType(row("NITTSUFREESEND"), Long)
                ShipBurdenFee += CType(row("SHIPBURDENFEE"), Long)
                ManageFee += CType(row("TOTAL"), Long)
            End If
        Else
            If row("BIGCTNCD").ToString <> "99" Then
                Quantity += CType(row("QUANTITY"), Long)
                UseFee += CType(row("USEFEE"), Long)
                NittuFreesend += CType(row("NITTSUFREESEND"), Long)
                ShipBurdenFee += CType(row("SHIPBURDENFEE"), Long)
                ManageFee += CType(row("TOTAL"), Long)
            End If
        End If



    End Sub
    ''' <summary>
    ''' テーブル加算
    ''' </summary>
    Private Sub EditCalcArea(
         ByVal row As DataRow,
         ByRef Table1(,,) As Long,
         ByRef Table2(,,) As Long,
         ByRef Table3(,,) As Long,
         ByRef Table4(,,) As Long,
         ByRef Table5(,,) As Long,
         ByRef Table6(,,) As Long,
         ByVal addsub As String
     )

        Dim AA As Integer = 1
        Dim BB As Integer = 28
        Dim CC As Integer = 1

        If row("BIGCTNCD").ToString = "05" Then
            If row("MIDDLECTNCD").ToString = "12" Then
                BB = 1
            End If
        ElseIf row("BIGCTNCD").ToString = "10" Then
            If row("MIDDLECTNCD").ToString = "12" Then
                If row("SPRFITKBN").ToString = "1" Then
                    BB = 2
                ElseIf row("SPRFITKBN").ToString = "2" OrElse row("SPRFITKBN").ToString = "0" Then
                    BB = 3
                End If
            End If
        ElseIf row("BIGCTNCD").ToString = "11" Then
            If row("MIDDLECTNCD").ToString = "12" Then
                BB = 5
            End If
        ElseIf row("BIGCTNCD").ToString = "15" Then
            If row("MIDDLECTNCD").ToString = "12" Then
                BB = 6
            ElseIf row("MIDDLECTNCD").ToString = "20" Then
                BB = 7
            ElseIf row("MIDDLECTNCD").ToString = "30" Then
                BB = 8
            End If
        ElseIf row("BIGCTNCD").ToString = "20" Then
            If row("MIDDLECTNCD").ToString = "30" Then
                BB = 10
            End If
        ElseIf row("BIGCTNCD").ToString = "25" Then
            If row("MIDDLECTNCD").ToString = "30" Then
                BB = 11
            End If
        ElseIf row("BIGCTNCD").ToString = "30" Then
            If row("MIDDLECTNCD").ToString = "12" Then
                BB = 12
            ElseIf row("MIDDLECTNCD").ToString = "20" Then
                BB = 13
            ElseIf row("MIDDLECTNCD").ToString = "24" Then
                BB = 14
            ElseIf row("MIDDLECTNCD").ToString = "31" Then
                BB = 15
            End If
        ElseIf row("BIGCTNCD").ToString = "35" Then
            If row("MIDDLECTNCD").ToString = "12" Then
                BB = 16
            ElseIf row("MIDDLECTNCD").ToString = "20" Then
                BB = 17
            ElseIf row("MIDDLECTNCD").ToString = "30" Then
                BB = 18
            End If
        ElseIf row("BIGCTNCD").ToString = "99" Then
            If row("MIDDLECTNCD").ToString = "91" Then
                'ボリュームインセンティブ
                BB = 20
            ElseIf row("MIDDLECTNCD").ToString = "92" Then
                '使用料加減額,リース料加減額
                BB = 21
            ElseIf row("MIDDLECTNCD").ToString = "93" Then
                '延滞料金
                BB = 22
            ElseIf row("MIDDLECTNCD").ToString = "94" Then
                'サブスク調整額
                BB = 23
            ElseIf row("MIDDLECTNCD").ToString = "95" Then
                '雑費 
                BB = 24
            ElseIf row("MIDDLECTNCD").ToString = "96" Then
                '手書き
                BB = 25
            ElseIf row("MIDDLECTNCD").ToString = "97" Then
                'スポットリース
                BB = 26
            ElseIf row("MIDDLECTNCD").ToString = "98" Then
                '固定料金
                BB = 27
            End If
        End If

        If addsub = "1" Then
            If row("FLG").ToString = "9" Then
                CC = 2
            ElseIf row("FLG").ToString = "8" Then
                CC = 3
            End If
        End If

        'LT03.
        While 0 = 0
            Table1(AA, BB, CC) += CType(row("QUANTITY"), Long)
            Table2(AA, BB, CC) += CType(row("USEFEE"), Long)
            Table3(AA, BB, CC) += CType(row("TOTAL"), Long)
            Table4(AA, BB, CC) += CType(row("NITTSUFREESEND"), Long)
            Table5(AA, BB, CC) += CType(row("SHIPBURDENFEE"), Long)
            If AA < 5 Then
                AA += 1
                Continue While
            End If
            If BB = 2 OrElse BB = 3 Then
                AA = 1
                BB = 4
                Continue While
            End If
            If BB = 6 OrElse BB = 7 OrElse BB = 8 Then
                AA = 1
                BB = 9
                Continue While
            End If
            If BB = 12 OrElse BB = 13 OrElse BB = 14 OrElse BB = 15 Then
                AA = 1
                BB = 30
                Continue While
            End If
            If BB = 16 OrElse BB = 17 OrElse BB = 18 Then
                AA = 1
                BB = 19
                Continue While
            End If
            Exit While
        End While

        'LT05.
        AA = 1

        'LT07.
        While 0 = 0
            Table1(AA, 31, CC) += CType(row("QUANTITY"), Long)
            Table2(AA, 31, CC) += CType(row("USEFEE"), Long)
            Table3(AA, 31, CC) += CType(row("TOTAL"), Long)
            Table4(AA, 31, CC) += CType(row("NITTSUFREESEND"), Long)
            Table5(AA, 31, CC) += CType(row("SHIPBURDENFEE"), Long)

            Table6(AA, 31, CC) += CType(row("USEFEE"), Long)
            Table6(AA, 31, CC) += CType(row("NITTSUFREESEND"), Long)
            Table6(AA, 31, CC) += CType(row("SHIPBURDENFEE"), Long)
            Table6(AA, 31, CC) += CType(row("TOTAL"), Long)

            Table1(AA, 32, 3) += CType(row("QUANTITY"), Long)
            Table2(AA, 32, 3) += CType(row("USEFEE"), Long)
            Table3(AA, 32, 3) += CType(row("TOTAL"), Long)
            Table4(AA, 32, 3) += CType(row("NITTSUFREESEND"), Long)
            Table5(AA, 32, 3) += CType(row("SHIPBURDENFEE"), Long)

            Table6(AA, 32, 3) += CType(row("USEFEE"), Long)
            Table6(AA, 32, 3) += CType(row("NITTSUFREESEND"), Long)
            Table6(AA, 32, 3) += CType(row("SHIPBURDENFEE"), Long)
            Table6(AA, 32, 3) += CType(row("TOTAL"), Long)

            If AA < 5 Then
                AA += 1
                Continue While
            End If
            Exit While
        End While

    End Sub

    ''' <summary>
    ''' 見出し設定
    ''' </summary>
    Private Function EditMidashiArea(AA As Integer, BB As Integer) As String
        Dim Midashi As String = ""

        If AA = 1 Then
            Midashi = "通風"
        ElseIf AA = 2 Then
            Midashi = "冷蔵（適"
        ElseIf AA = 3 Then
            Midashi = "冷蔵（一"
        ElseIf AA = 4 Then
            Midashi = "冷蔵（計"
        ElseIf AA = 5 Then
            Midashi = "Ｓ－ＵＲ"
        ElseIf AA = 6 Then
            Midashi = "冷凍１２"
        ElseIf AA = 7 Then
            Midashi = "冷凍２０"
        ElseIf AA = 8 Then
            Midashi = "冷凍３０"
        ElseIf AA = 9 Then
            Midashi = "冷凍（計"
        ElseIf AA = 10 Then
            Midashi = "Ｌ１０屯"
        ElseIf AA = 11 Then
            Midashi = "ウイング"
        ElseIf AA = 12 Then
            Midashi = "有蓋"
        ElseIf AA = 13 Then
            Midashi = "有蓋２０"
        ElseIf AA = 14 Then
            Midashi = "有蓋２４"
        ElseIf AA = 15 Then
            Midashi = "有蓋３１"
        ElseIf AA = 16 Then
            Midashi = "無蓋１２"
        ElseIf AA = 17 Then
            Midashi = "無蓋２０"
        ElseIf AA = 18 Then
            Midashi = "無蓋３０"
        ElseIf AA = 19 Then
            Midashi = "無蓋計"
        ElseIf AA = 20 Then
            Midashi = "インセンティブ"
        ElseIf AA = 21 Then
            Midashi = "加減額"
        ElseIf AA = 22 Then
            Midashi = "延滞料金"
        ElseIf AA = 23 Then
            Midashi = "サブスク調整額"
        ElseIf AA = 24 Then
            Midashi = "雑費"
        ElseIf AA = 25 Then
            Midashi = "手書き"
        ElseIf AA = 26 Then
            Midashi = "スポットリース"
        ElseIf AA = 27 Then
            Midashi = "固定料金"
        ElseIf AA = 28 Then
            Midashi = "その他"
        ElseIf AA = 29 Then
            Midashi = "加減額"
        ElseIf AA = 30 Then
            Midashi = "有蓋計"
        ElseIf AA = 31 Then
            If BB = 1 Then
                Midashi = "（合計）"
            ElseIf BB = 2 Then
                Midashi = "加減額計"
            Else
                Midashi = "追加明細金額計"
            End If
        ElseIf AA = 32 Then
            Midashi = "【合計】"
        End If

        Return Midashi
    End Function

End Class
