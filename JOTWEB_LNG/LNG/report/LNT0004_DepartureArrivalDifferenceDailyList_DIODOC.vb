Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 発着差実績表(日別)帳票作成クラス
''' </summary>
Public Class LNT0004_DepartureArrivalDifferenceDailyList_DIODOC

    ''' <summary>
    ''' 選択種類
    ''' </summary>
    Public Enum SelectCategory
        ALL '全選択
        STATIONCODEINPUT '駅コード入力
        OTHER 'その他
    End Enum

    ''' <summary>
    ''' 抽出タイプ
    ''' </summary>
    Public Enum ExtractionType
        SingleMode　'単体
        AllMode '全体
    End Enum

#Region "帳票行設定"
    ''' <summary>
    ''' 開始行(表題)
    ''' </summary>
    Public Enum RowStPosTitle
        Name = 2
    End Enum

    ''' <summary>
    ''' 開始行(1日～15日)
    ''' </summary>
    Public Enum RowStPos01To15
        HeadDaysName = 3  'ヘッダ(日)
        HeadDetailName    'ヘッダ(明細名)
        StackDep          '積-地区外行(発送)
        StackArv          '積-地区外発(到着)
        StackDepArvDiff   '積-発着差
        FreeDep           '空-地区外行(発送)
        FreeArv           '空-地区外発(到着)
        FreeDepArvDiff    '空-発着差
        TotalDepArvDiff   '計-発着差計
        TotalPresentTrain '計-列車現在
        TotalAnchorageNum '計-停泊個数
        TotalAfter10Date  '計-(内１０日以上)
        TotalPresentNum   '計-総現在
    End Enum

    ''' <summary>
    ''' 開始行(16日～31日)
    ''' </summary>
    Public Enum RowStPos16To31
        HeadDaysName = 16 'ヘッダ(日)
        HeadDetailName    'ヘッダ(明細名)
        StackDep          '積-地区外行(発送)
        StackArv          '積-地区外発(到着)
        StackDepArvDiff   '積-発着差
        FreeDep           '空-地区外行(発送)
        FreeArv           '空-地区外発(到着)
        FreeDepArvDiff    '空-発着差
        TotalDepArvDiff   '計-発着差計
        TotalPresentTrain '計-列車現在
        TotalAnchorageNum '計-停泊個数
        TotalAfter10Date  '計-(内１０日以上)
        TotalPresentNum   '計-総現在
    End Enum
#End Region

#Region "帳票列設定"
    ''' <summary>
    ''' 開始列(表題)
    ''' </summary>
    Public Enum ColStPosTitle
        OrgStationName = 1 '支店(駅名)
        TargetYM = 9       '対象年月
        Type = 30          '種別
        OutputTime = 50　　'出力日時
    End Enum

    ''' <summary>
    ''' 開始列(前月末、1日～15日)
    ''' </summary>
    Public Enum ColStPos01To15
        '前年末
        OTHER_BEFLASTDAY = 3  'その他
        LSHAP_BEFLASTDAY      'L字
        DOUBLE_BEFLASTDAY     '両開き
        '1日
        OTHER_DAY_01
        LSHAP_DAY_01
        DOUBLE_DAY_01

        OTHER_DAY_02
        LSHAP_DAY_02
        DOUBLE_DAY_02

        OTHER_DAY_03
        LSHAP_DAY_03
        DOUBLE_DAY_03

        OTHER_DAY_04
        LSHAP_DAY_04
        DOUBLE_DAY_04

        OTHER_DAY_05
        LSHAP_DAY_05
        DOUBLE_DAY_05

        OTHER_DAY_06
        LSHAP_DAY_06
        DOUBLE_DAY_06

        OTHER_DAY_07
        LSHAP_DAY_07
        DOUBLE_DAY_07

        OTHER_DAY_08
        LSHAP_DAY_08
        DOUBLE_DAY_08

        OTHER_DAY_09
        LSHAP_DAY_09
        DOUBLE_DAY_09

        OTHER_DAY_10
        LSHAP_DAY_10
        DOUBLE_DAY_10

        OTHER_DAY_11
        LSHAP_DAY_11
        DOUBLE_DAY_11

        OTHER_DAY_12
        LSHAP_DAY_12
        DOUBLE_DAY_12

        OTHER_DAY_13
        LSHAP_DAY_13
        DOUBLE_DAY_13

        OTHER_DAY_14
        LSHAP_DAY_14
        DOUBLE_DAY_14

        OTHER_DAY_15
        LSHAP_DAY_15
        DOUBLE_DAY_15
    End Enum

    ''' <summary>
    ''' 開始列(16日～31日、月間計)
    ''' </summary>
    Public Enum ColStPos16To31
        '16日
        OTHER_DAY_16 = 3 'その他
        LSHAP_DAY_16     'L字
        DOUBLE_DAY_16    '両開き

        OTHER_DAY_17
        LSHAP_DAY_17
        DOUBLE_DAY_17

        OTHER_DAY_18
        LSHAP_DAY_18
        DOUBLE_DAY_18

        OTHER_DAY_19
        LSHAP_DAY_19
        DOUBLE_DAY_19

        OTHER_DAY_20
        LSHAP_DAY_20
        DOUBLE_DAY_20

        OTHER_DAY_21
        LSHAP_DAY_21
        DOUBLE_DAY_21

        OTHER_DAY_22
        LSHAP_DAY_22
        DOUBLE_DAY_22

        OTHER_DAY_23
        LSHAP_DAY_23
        DOUBLE_DAY_23

        OTHER_DAY_24
        LSHAP_DAY_24
        DOUBLE_DAY_24

        OTHER_DAY_25
        LSHAP_DAY_25
        DOUBLE_DAY_25

        OTHER_DAY_26
        LSHAP_DAY_26
        DOUBLE_DAY_26

        OTHER_DAY_27
        LSHAP_DAY_27
        DOUBLE_DAY_27

        OTHER_DAY_28
        LSHAP_DAY_28
        DOUBLE_DAY_28

        OTHER_DAY_29
        LSHAP_DAY_29
        DOUBLE_DAY_29

        OTHER_DAY_30
        LSHAP_DAY_30
        DOUBLE_DAY_30

        OTHER_DAY_31
        LSHAP_DAY_31
        DOUBLE_DAY_31

        '月間計
        OTHER_MonthTotalDAY
        LSHAP_MonthTotalDAY
        DOUBLE_MonthTotalDAY
    End Enum

#End Region

    Private Const CONST_TABLE_SPACE_LINE As Integer = 3 '表毎の空白行数
    Private Const CONST_SEL_MAX_BIGCTNCNT As Integer = 8 '種別全選択数

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

        Catch ex As Exception

        End Try

    End Sub

    ''' <summary>
    ''' 帳票作成
    ''' 
    ''' ※明細の出力条件を引数として渡す
    ''' </summary>
    ''' <param name="WW_CATEGORY">帳票の出力種類(全支店出力、駅コード指定出力、支店、駅指定出力など)</param>
    ''' <param name="WW_ORGCODE">画面選択支店コード</param>
    ''' <param name="WW_STATIONCODE">画面入力駅コード</param>
    ''' <param name="WW_STATIONLIST">画面選択駅コード</param>
    ''' <param name="WW_BIGCTNCDLIST">画面選択コンテナ種別コード</param>
    ''' <param name="WW_DATESTART">画面選択開始日</param>
    ''' <param name="WW_DATEEND">画面選択終了日</param>
    ''' <param name="WW_ORGNAME">画面選択支店名</param>
    ''' <param name="WW_STATIONHT">画面駅名</param>
    ''' <param name="WW_BIGCTNCDHT">画面選択コンテナ種別名</param>
    ''' <param name="WW_STATIONNAME">画面入力駅名</param>
    ''' <returns>ダウンロードURL</returns>
    Public Function CreateExcelPrintData(
                                         ByVal WW_CATEGORY As Integer,
                                         ByVal WW_ORGCODE As String,
                                         ByVal WW_STATIONCODE As String,
                                         ByVal WW_STATIONLIST As ArrayList,
                                         ByVal WW_BIGCTNCDLIST As ArrayList,
                                         ByVal WW_DATESTART As String,
                                         ByVal WW_DATEEND As String,
                                         ByVal WW_ORGNAME As String,
                                         ByVal WW_STATIONHT As Hashtable,
                                         ByVal WW_BIGCTNCDHT As Hashtable,
                                         ByVal WW_STATIONNAME As String
                                         ) As String

        Dim tmpFileName As String = "帳票_発着差実績表(日別)_" &
            Replace(WW_DATESTART, "/", "") & "-" & Replace(WW_DATEEND, "/", "") & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)

        'セルの貼り付け方法を設定
        Dim pasteDefault As PasteOption = New PasteOption()
        Dim pasteRowHeights As PasteOption = New PasteOption()
        pasteDefault.PasteType = PasteType.Default
        pasteRowHeights.PasteType = PasteType.RowHeights

        Try
            '■対象データ取得
            '前月末取得
            'Dim WW_ENDLASTYMD As String = CDate(WW_DATESTART.Substring(0, 8) + "01").AddDays(-1).ToString("yyyy/MM/dd")
            Dim WW_ENDLASTYMD As String = GetLastYMD(Left(Replace(WW_DATESTART, "/", ""), 6))

            Dim dt As New DataTable '日付毎抽出(支店、駅)
            Dim dtALL As New DataTable '日付毎抽出(全体合計)

            '前月末と開始日～終了日の条件に当てはまるデータをまとめて抽出(月、支店毎の絞り込みは抽出後Datatableにて行う)
            dt = Me.GetDepArvDiff(WW_CATEGORY,
                                    WW_ORGCODE,
                                    WW_STATIONCODE,
                                    WW_STATIONLIST,
                                    WW_BIGCTNCDLIST,
                                    WW_DATESTART,
                                    WW_DATEEND,
                                    WW_ENDLASTYMD,
                                    ExtractionType.SingleMode)

            dtALL = Me.GetDepArvDiff(WW_CATEGORY,
                                    WW_ORGCODE,
                                    WW_STATIONCODE,
                                    WW_STATIONLIST,
                                    WW_BIGCTNCDLIST,
                                    WW_DATESTART,
                                    WW_DATEEND,
                                    WW_ENDLASTYMD,
                                    ExtractionType.AllMode)

            '■帳票共通部分設定
            '種別名
            Dim WW_REPORT_BIGCTNNAME As String = ""
            For Each key As String In WW_BIGCTNCDHT.Keys
                If WW_REPORT_BIGCTNNAME = "" Then
                    WW_REPORT_BIGCTNNAME = WW_BIGCTNCDHT(key)
                Else
                    WW_REPORT_BIGCTNNAME = WW_REPORT_BIGCTNNAME + "・" + WW_BIGCTNCDHT(key)
                End If
            Next

            '出力日時
            Dim WW_REPORT_OUTPUTTIME As String = DateTime.Now.ToString("yyyy/MM/dd HH:mm")

            'テンプレートシートに共通部分の種別名と出力日時を書き込む
            Dim WW_TemplateSheet As IWorksheet = WW_Workbook.Worksheets(0)
            WW_TemplateSheet.Range(RowStPosTitle.Name, ColStPosTitle.Type).Value = WW_REPORT_BIGCTNNAME '種別名
            WW_TemplateSheet.Range(RowStPosTitle.Name, ColStPosTitle.OutputTime).Value = WW_REPORT_OUTPUTTIME '出力日時

            '1つの表に使う行数を取得
            Dim WW_TableStepRow As Integer = (RowStPos16To31.TotalPresentNum - RowStPosTitle.Name) + 1 + CONST_TABLE_SPACE_LINE

            '■条件分岐部分設定
            Dim WW_TargetYM As Integer = Replace(WW_DATESTART.Substring(0, 7), "/", "")
            Dim WW_EndYM As Integer = Replace(WW_DATEEND.Substring(0, 7), "/", "")

            Dim WW_ORGHT As New Hashtable
            WW_ORGHT.Add("010102", "北海道支店")
            WW_ORGHT.Add("010401", "東北支店")
            WW_ORGHT.Add("011402", "関東支店")
            WW_ORGHT.Add("012401", "中部支店")
            WW_ORGHT.Add("012701", "関西支店")
            WW_ORGHT.Add("014001", "九州支店")

            Dim WW_ORGCODELIST As New ArrayList(WW_ORGHT.Keys)
            WW_ORGCODELIST.Sort()

            While WW_TargetYM <= WW_EndYM
                Dim MonthRowALL As DataRow()

                '月全体の一覧を取得
                MonthRowALL = dtALL.Select("FILTERYM = '" + WW_TargetYM.ToString + "'")

                'データを抽出できた場合
                If Not MonthRowALL.Count = 0 Then
                    Dim WW_TargetYMNAME As String = Left(WW_TargetYM.ToString, 4) + "年" + Right(WW_TargetYM.ToString, 2) + "月"

                    'テンプレートの日付を対象月用に更新
                    SetHeaderDay(WW_TemplateSheet, WW_TargetYM)
                    'テンプレートに対象年月を入力
                    WW_TemplateSheet.Range(RowStPosTitle.Name, ColStPosTitle.TargetYM).Value = WW_TargetYMNAME

                    'テンプレートシートを複製してデータ出力用のシートを作成
                    Dim WW_CopySheet = WW_TemplateSheet.Copy()
                    WW_CopySheet.Activate()
                    WW_CopySheet.Name = WW_TargetYMNAME

                    '複製したシートに全体の支店(駅名)入力
                    Select Case WW_CATEGORY
                        Case SelectCategory.STATIONCODEINPUT '駅コード入力の場合
                            WW_CopySheet.Range(RowStPosTitle.Name, ColStPosTitle.OrgStationName).Value = WW_STATIONNAME + "駅選択（合計）"
                        Case SelectCategory.ALL '全支店検索の場合
                            WW_CopySheet.Range(RowStPosTitle.Name, ColStPosTitle.OrgStationName).Value = "全支店"
                        Case Else 'その他の場合 駅毎のデータを取得(支店は固定)
                            WW_CopySheet.Range(RowStPosTitle.Name, ColStPosTitle.OrgStationName).Value = WW_ORGNAME + "（全体）"
                    End Select

                    Dim WW_OffsetRow As Integer = 0  '行のオフセット

                    '前年度末(全体)入力
                    Dim WW_LastYMD As String = GetLastYMD(WW_TargetYM)
                    Dim LastYMDRow As DataRow() = dtALL.Select("DATADATE = '" + WW_LastYMD + "'")
                    '取得できた場合のみ出力
                    If Not LastYMDRow.Count = 0 Then
                        SetLastYMD(WW_CopySheet, LastYMDRow(0), WW_OffsetRow)
                    End If

                    '実績表の全体出力
                    SetMonthData(WW_CopySheet, MonthRowALL, WW_OffsetRow)

                    '全体以外のデータ出力
                    Dim MonthRowSub As DataRow()
                    Dim LastYMDRowSub As DataRow()
                    Select Case WW_CATEGORY
                        Case SelectCategory.STATIONCODEINPUT '駅コード入力の場合
                        Case SelectCategory.ALL '全支店検索の場合
                            'For Each CODE As String In WW_ORGHT.Keys
                            For Each CODE As String In WW_ORGCODELIST
                                MonthRowSub = dt.Select("FILTERYM = '" + WW_TargetYM.ToString + "' and ORGCODE = '" + CODE + "'")
                                'データを抽出できた場合
                                If Not MonthRowSub.Count = 0 Then
                                    WW_OffsetRow += WW_TableStepRow
                                    'テンプレートシートから表をコピー貼り付け
                                    WW_TemplateSheet.Range("A3:BB30").Copy(WW_CopySheet.Range(CInt(RowStPosTitle.Name) + WW_OffsetRow, 0), pasteDefault)
                                    WW_TemplateSheet.Range("A3:BB30").Copy(WW_CopySheet.Range(RowStPosTitle.Name + WW_OffsetRow, 0), pasteRowHeights)
                                    '支店名
                                    WW_CopySheet.Range(RowStPosTitle.Name + WW_OffsetRow, ColStPosTitle.OrgStationName).Value = WW_ORGHT(CODE)
                                    '前年度末入力
                                    LastYMDRowSub = dt.Select("DATADATE = '" + WW_LastYMD + "' and ORGCODE = '" + CODE + "'")
                                    '取得できた場合のみ出力
                                    If Not LastYMDRowSub.Count = 0 Then
                                        SetLastYMD(WW_CopySheet, LastYMDRowSub(0), WW_OffsetRow)
                                    End If
                                    '実績表出力
                                    SetMonthData(WW_CopySheet, MonthRowSub, WW_OffsetRow)
                                End If
                            Next
                        Case Else 'その他の場合 
                            For Each CODE As String In WW_STATIONLIST
                                MonthRowSub = dt.Select("FILTERYM = '" + WW_TargetYM.ToString + "' and STATIONCODE = '" + CODE + "'")
                                'データを抽出できた場合
                                If Not MonthRowSub.Count = 0 Then
                                    WW_OffsetRow += WW_TableStepRow
                                    'テンプレートシートから表をコピー貼り付け
                                    WW_TemplateSheet.Range("A3:BB30").Copy(WW_CopySheet.Range(CInt(RowStPosTitle.Name) + WW_OffsetRow, 0), pasteDefault)
                                    WW_TemplateSheet.Range("A3:BB30").Copy(WW_CopySheet.Range(RowStPosTitle.Name + WW_OffsetRow, 0), pasteRowHeights)
                                    '駅名
                                    WW_CopySheet.Range(RowStPosTitle.Name + WW_OffsetRow, ColStPosTitle.OrgStationName).Value = WW_ORGNAME + "（" + WW_STATIONHT(CODE) + "）"
                                    '前年度末入力
                                    LastYMDRowSub = dt.Select("DATADATE = '" + WW_LastYMD + "' and STATIONCODE = '" + CODE + "'")
                                    '取得できた場合のみ出力
                                    If Not LastYMDRowSub.Count = 0 Then
                                        SetLastYMD(WW_CopySheet, LastYMDRowSub(0), WW_OffsetRow)
                                    End If
                                    '実績表出力
                                    SetMonthData(WW_CopySheet, MonthRowSub, WW_OffsetRow)
                                End If
                            Next
                    End Select
                End If
                '月加算
                AddMonth(WW_TargetYM)
            End While

            'テンプレートシート削除
            Try
                WW_TemplateSheet.Delete()
            Catch ex As Exception
            End Try

            '全シートA1選択
            For Each sheet As IWorksheet In WW_Workbook.Worksheets
                sheet.Range(0, 0).Select()
            Next

            '先頭シート選択
            WW_Workbook.Worksheets(0).Activate()

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
    ''' 月加算
    ''' </summary>
    Private Sub AddMonth(ByRef WW_YM As Integer)
        Dim WW_Date As Date = CDate(Left(WW_YM.ToString, 4) + "/" + Right(WW_YM.ToString, 2) + "/01")
        WW_Date = WW_Date.AddMonths(1)
        WW_YM = CInt(Replace(WW_Date.ToString.Substring(0, 7), "/", ""))
    End Sub

    ''' <summary>
    ''' 前月末取得
    ''' </summary>
    Private Function GetLastYMD(ByVal WW_YM As Integer) As String
        GetLastYMD = CDate(Left(WW_YM.ToString, 4) + "/" + Right(WW_YM.ToString, 2) + "/01").AddDays(-1).ToString("yyyy/MM/dd")
    End Function

    ''' <summary>
    ''' ヘッダに日付を割当て
    ''' </summary>
    Private Sub SetHeaderDay(ByVal sheet As IWorksheet, ByVal WW_YM As Integer)
        Dim weekday As New Hashtable
        weekday.Add("1", "(日)")
        weekday.Add("2", "(月)")
        weekday.Add("3", "(火)")
        weekday.Add("4", "(水)")
        weekday.Add("5", "(木)")
        weekday.Add("6", "(金)")
        weekday.Add("7", "(土)")

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()

            Dim SQLStr = New StringBuilder
            SQLStr.AppendLine(" SELECT ")
            SQLStr.AppendLine("   DATEPART(weekday, '" + WW_YM.ToString + "01') AS DAY01 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "02') AS DAY02 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "03') AS DAY03 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "04') AS DAY04 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "05') AS DAY05 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "06') AS DAY06 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "07') AS DAY07 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "08') AS DAY08 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "09') AS DAY09 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "10') AS DAY10 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "11') AS DAY11 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "12') AS DAY12 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "13') AS DAY13 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "14') AS DAY14 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "15') AS DAY15 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "16') AS DAY16 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "17') AS DAY17 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "18') AS DAY18 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "19') AS DAY19 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "20') AS DAY20 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "21') AS DAY21 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "22') AS DAY22 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "23') AS DAY23 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "24') AS DAY24 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "25') AS DAY25 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "26') AS DAY26 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "27') AS DAY27 ")
            SQLStr.AppendLine("  ,DATEPART(weekday, '" + WW_YM.ToString + "28') AS DAY28 ")
            SQLStr.AppendLine(" ,CASE ISDATE('" + WW_YM.ToString + "29') ")
            SQLStr.AppendLine("   WHEN '1' THEN  DATEPART(weekday, '" + WW_YM.ToString + "29') ")
            SQLStr.AppendLine("   ELSE '0' ")
            SQLStr.AppendLine(" END AS DAY29 ")
            SQLStr.AppendLine(" ,CASE ISDATE('" + WW_YM.ToString + "30') ")
            SQLStr.AppendLine("   WHEN '1' THEN  DATEPART(weekday, '" + WW_YM.ToString + "30') ")
            SQLStr.AppendLine("   ELSE '0' ")
            SQLStr.AppendLine(" END AS DAY30 ")
            SQLStr.AppendLine(" ,CASE ISDATE('" + WW_YM.ToString + "31') ")
            SQLStr.AppendLine("   WHEN '1' THEN  DATEPART(weekday, '" + WW_YM.ToString + "31') ")
            SQLStr.AppendLine("   ELSE '0' ")
            SQLStr.AppendLine(" END AS DAY31 ")

            Try
                Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
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
            End Try
        End Using

        If dt.Rows.Count > 0 Then
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_01).Value = "1" + weekday(dt(0)("DAY01").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_02).Value = "2" + weekday(dt(0)("DAY02").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_03).Value = "3" + weekday(dt(0)("DAY03").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_04).Value = "4" + weekday(dt(0)("DAY04").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_05).Value = "5" + weekday(dt(0)("DAY05").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_06).Value = "6" + weekday(dt(0)("DAY06").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_07).Value = "7" + weekday(dt(0)("DAY07").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_08).Value = "8" + weekday(dt(0)("DAY08").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_09).Value = "9" + weekday(dt(0)("DAY09").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_10).Value = "10" + weekday(dt(0)("DAY10").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_11).Value = "11" + weekday(dt(0)("DAY11").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_12).Value = "12" + weekday(dt(0)("DAY12").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_13).Value = "13" + weekday(dt(0)("DAY13").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_14).Value = "14" + weekday(dt(0)("DAY14").ToString)
            sheet.Range(RowStPos01To15.HeadDaysName, ColStPos01To15.OTHER_DAY_15).Value = "15" + weekday(dt(0)("DAY15").ToString)

            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_16).Value = "16" + weekday(dt(0)("DAY16").ToString)
            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_17).Value = "17" + weekday(dt(0)("DAY17").ToString)
            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_18).Value = "18" + weekday(dt(0)("DAY18").ToString)
            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_19).Value = "19" + weekday(dt(0)("DAY19").ToString)
            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_20).Value = "20" + weekday(dt(0)("DAY20").ToString)
            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_21).Value = "21" + weekday(dt(0)("DAY21").ToString)
            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_22).Value = "22" + weekday(dt(0)("DAY22").ToString)
            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_23).Value = "23" + weekday(dt(0)("DAY23").ToString)
            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_24).Value = "24" + weekday(dt(0)("DAY24").ToString)
            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_25).Value = "25" + weekday(dt(0)("DAY25").ToString)
            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_26).Value = "26" + weekday(dt(0)("DAY26").ToString)
            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_27).Value = "27" + weekday(dt(0)("DAY27").ToString)
            sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_28).Value = "28" + weekday(dt(0)("DAY28").ToString)
            If Not dt(0)("DAY29").ToString = "0" Then
                sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_29).Value = "29" + weekday(dt(0)("DAY29").ToString)
            Else
                sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_29).Value = ""
            End If
            If Not dt(0)("DAY30").ToString = "0" Then
                sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_30).Value = "30" + weekday(dt(0)("DAY30").ToString)
            Else
                sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_30).Value = ""
            End If
            If Not dt(0)("DAY31").ToString = "0" Then
                sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_31).Value = "31" + weekday(dt(0)("DAY31").ToString)
            Else
                sheet.Range(RowStPos16To31.HeadDaysName, ColStPos16To31.OTHER_DAY_31).Value = ""
            End If
        End If
    End Sub

    ''' <summary>
    ''' 前年度末入力
    ''' </summary>
    Private Sub SetLastYMD(ByVal sheet As IWorksheet, ByVal WW_Row As DataRow, ByVal WW_OffsetRow As Integer)
        Dim WW_StackDepArvDiff_OTHER As Integer '積-発着差 その他
        Dim WW_StackDepArvDiff_LSHAP As Integer '積-発着差 L字
        Dim WW_StackDepArvDiff_DOUBLE As Integer '積-発着差 両開き

        Dim WW_FreeDepArvDiff_OTHER As Integer '空-発着差 その他
        Dim WW_FreeDepArvDiff_LSHAP As Integer '空-発着差 L字
        Dim WW_FreeDepArvDiff_DOUBLE As Integer '空-発着差 両開き

        Dim WW_TotalDepArvDiff_OTHER As Integer '計-発着差計 その他
        Dim WW_TotalDepArvDiff_LSHAP As Integer '計-発着差計 L字
        Dim WW_TotalDepArvDiff_DOUBLE As Integer '計-発着差計 両開き

        Dim WW_TotalPresentTrain As Integer '計-列車現在
        Dim WW_TotalAnchorageNum As Integer '計-停泊個数
        Dim WW_TotalAfter10Date As Integer '計-(内１０日以上)
        Dim WW_TotalPresentNum As Integer '計-総現在

        '初期化
        WW_StackDepArvDiff_OTHER = 0 '積-発着差 その他
        WW_StackDepArvDiff_LSHAP = 0 '積-発着差 L字
        WW_StackDepArvDiff_DOUBLE = 0 '積-発着差 両開き

        WW_FreeDepArvDiff_OTHER = 0 '空-発着差 その他
        WW_FreeDepArvDiff_LSHAP = 0 '空-発着差 L字
        WW_FreeDepArvDiff_DOUBLE = 0 '空-発着差 両開き

        WW_TotalDepArvDiff_OTHER = 0 '計-発着差計 その他
        WW_TotalDepArvDiff_LSHAP = 0 '計-発着差計 L字
        WW_TotalDepArvDiff_DOUBLE = 0 '計-発着差計 両開き

        WW_TotalPresentTrain = 0 '計-列車現在
        WW_TotalAnchorageNum = 0 '計-停泊個数
        WW_TotalAfter10Date = 0 '計-(内１０日以上)
        WW_TotalPresentNum = 0 '計-総現在

        '計算
        WW_StackDepArvDiff_OTHER = CInt(WW_Row("STACK_ARV_OTHER")) - CInt(WW_Row("STACK_DEP_OTHER")) '積-発着差 その他
        WW_StackDepArvDiff_LSHAP = CInt(WW_Row("STACK_ARV_LSHAP")) - CInt(WW_Row("STACK_DEP_LSHAP")) '積-発着差 L字
        WW_StackDepArvDiff_DOUBLE = CInt(WW_Row("STACK_ARV_DOUBLE")) - CInt(WW_Row("STACK_DEP_DOUBLE")) '積-発着差 両開き

        WW_FreeDepArvDiff_OTHER = CInt(WW_Row("FREE_ARV_OTHER")) - CInt(WW_Row("FREE_DEP_OTHER")) '空-発着差 その他
        WW_FreeDepArvDiff_LSHAP = CInt(WW_Row("FREE_ARV_LSHAP")) - CInt(WW_Row("FREE_DEP_LSHAP")) '空-発着差 L字
        WW_FreeDepArvDiff_DOUBLE = CInt(WW_Row("FREE_ARV_DOUBLE")) - CInt(WW_Row("FREE_DEP_DOUBLE")) '空-発着差 両開き

        WW_TotalDepArvDiff_OTHER = WW_StackDepArvDiff_OTHER - WW_FreeDepArvDiff_OTHER '計-発着差計 その他
        WW_TotalDepArvDiff_LSHAP = WW_StackDepArvDiff_LSHAP - WW_FreeDepArvDiff_LSHAP '計-発着差計 L字
        WW_TotalDepArvDiff_DOUBLE = WW_StackDepArvDiff_DOUBLE - WW_FreeDepArvDiff_DOUBLE '計-発着差計 両開き

        WW_TotalPresentTrain = CInt(WW_Row("STACK_PRESENTTRAIN")) + CInt(WW_Row("FREE_PRESENTTRAIN")) '計-列車現在
        WW_TotalAnchorageNum = CInt(WW_Row("STACK_ANCHORAGENUM")) + CInt(WW_Row("FREE_ANCHORAGENUM")) '計-停泊個数
        WW_TotalAfter10Date = CInt(WW_Row("STACK_AFTER10DATE")) + CInt(WW_Row("FREE_AFTER10DATE")) '計-停泊個数

        'WW_TotalPresentNum = WW_TotalPresentTrain + WW_TotalAnchorageNum
        WW_TotalPresentNum = CInt(WW_Row("TOTAL_PRESENT_NUM"))

        '積-地区外行(発送)
        SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_BEFLASTDAY, WW_Row("STACK_DEP_OTHER").ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_BEFLASTDAY, WW_Row("STACK_DEP_LSHAP").ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_BEFLASTDAY, WW_Row("STACK_DEP_DOUBLE").ToString) '両開き

        '積-地区外発(到着)
        SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_BEFLASTDAY, WW_Row("STACK_ARV_OTHER").ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_BEFLASTDAY, WW_Row("STACK_ARV_LSHAP").ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_BEFLASTDAY, WW_Row("STACK_ARV_DOUBLE").ToString) '両開き

        '積-発着差
        SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_BEFLASTDAY, WW_StackDepArvDiff_OTHER.ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_BEFLASTDAY, WW_StackDepArvDiff_LSHAP.ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_BEFLASTDAY, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

        '空-地区外行(発送)
        SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_BEFLASTDAY, WW_Row("FREE_DEP_OTHER").ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_BEFLASTDAY, WW_Row("FREE_DEP_LSHAP").ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_BEFLASTDAY, WW_Row("FREE_DEP_DOUBLE").ToString) '両開き

        '空-地区外発(到着)
        SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_BEFLASTDAY, WW_Row("FREE_ARV_OTHER").ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_BEFLASTDAY, WW_Row("FREE_ARV_LSHAP").ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_BEFLASTDAY, WW_Row("FREE_ARV_DOUBLE").ToString) '両開き

        '空-発着差
        SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_BEFLASTDAY, WW_FreeDepArvDiff_OTHER.ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_BEFLASTDAY, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_BEFLASTDAY, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

        '計-発着差計
        SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_BEFLASTDAY, WW_TotalDepArvDiff_OTHER.ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_BEFLASTDAY, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_BEFLASTDAY, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

        '計-列車現在
        SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_BEFLASTDAY, WW_TotalPresentTrain.ToString)
        '計-停泊個数
        SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_BEFLASTDAY, WW_TotalAnchorageNum.ToString)
        '計-(内１０日以上)
        SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_BEFLASTDAY, WW_TotalAfter10Date.ToString)
        '計-総現在
        SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_BEFLASTDAY, WW_TotalPresentNum.ToString)

    End Sub

    ''' <summary>
    ''' 対象月のデータ入力
    ''' </summary>
    Private Sub SetMonthData(ByVal sheet As IWorksheet, ByVal WW_Row As DataRow(), ByVal WW_OffsetRow As Integer)
        Dim WW_StackDep_OTHER As Integer '積-地区外行(発送) その他
        Dim WW_StackDep_LSHAP As Integer '積-地区外行(発送) L字
        Dim WW_StackDep_DOUBLE As Integer '積-地区外行(発送) 両開き

        Dim WW_StackArv_OTHER As Integer '積-地区外発(到着) その他
        Dim WW_StackArv_LSHAP As Integer '積-地区外発(到着) L字
        Dim WW_StackArv_DOUBLE As Integer '積-地区外発(到着) 両開き

        Dim WW_StackDepArvDiff_OTHER As Integer '積-発着差 その他
        Dim WW_StackDepArvDiff_LSHAP As Integer '積-発着差 L字
        Dim WW_StackDepArvDiff_DOUBLE As Integer '積-発着差 両開き

        Dim WW_FreeDep_OTHER As Integer '空-地区外行(発送) その他
        Dim WW_FreeDep_LSHAP As Integer '空-地区外行(発送) L字
        Dim WW_FreeDep_DOUBLE As Integer '空-地区外行(発送) 両開き

        Dim WW_FreeArv_OTHER As Integer '空-地区外発(到着) その他
        Dim WW_FreeArv_LSHAP As Integer '空-地区外発(到着) L字
        Dim WW_FreeArv_DOUBLE As Integer '空-地区外発(到着) 両開き

        Dim WW_FreeDepArvDiff_OTHER As Integer '空-発着差 その他
        Dim WW_FreeDepArvDiff_LSHAP As Integer '空-発着差 L字
        Dim WW_FreeDepArvDiff_DOUBLE As Integer '空-発着差 両開き

        Dim WW_TotalDepArvDiff_OTHER As Integer '計-発着差計 その他
        Dim WW_TotalDepArvDiff_LSHAP As Integer '計-発着差計 L字
        Dim WW_TotalDepArvDiff_DOUBLE As Integer '計-発着差計 両開き

        Dim WW_TotalPresentTrain As Integer '計-列車現在
        Dim WW_TotalAnchorageNum As Integer '計-停泊個数
        Dim WW_TotalAfter10Date As Integer '計-(内１０日以上)
        Dim WW_TotalPresentNum As Integer '計-総現在

        Dim SUM_StackDep_OTHER As Integer = 0 '【合計】積-地区外行(発送) その他
        Dim SUM_StackDep_LSHAP As Integer = 0 '【合計】積-地区外行(発送) L字
        Dim SUM_StackDep_DOUBLE As Integer = 0 '【合計】積-地区外行(発送) 両開き

        Dim SUM_StackArv_OTHER As Integer = 0 '【合計】積-地区外発(到着) その他
        Dim SUM_StackArv_LSHAP As Integer = 0 '【合計】積-地区外発(到着) L字
        Dim SUM_StackArv_DOUBLE As Integer = 0 '【合計】積-地区外発(到着) 両開き

        Dim SUM_StackDepArvDiff_OTHER As Integer = 0 '【合計】積-発着差 その他
        Dim SUM_StackDepArvDiff_LSHAP As Integer = 0 '【合計】積-発着差 L字
        Dim SUM_StackDepArvDiff_DOUBLE As Integer = 0 '【合計】積-発着差 両開き

        Dim SUM_FreeDep_OTHER As Integer = 0 '【合計】空-地区外行(発送) その他
        Dim SUM_FreeDep_LSHAP As Integer = 0 '【合計】空-地区外行(発送) L字
        Dim SUM_FreeDep_DOUBLE As Integer = 0 '【合計】空-地区外行(発送) 両開き

        Dim SUM_FreeArv_OTHER As Integer = 0 '【合計】空-地区外発(到着) その他
        Dim SUM_FreeArv_LSHAP As Integer = 0 '【合計】空-地区外発(到着) L字
        Dim SUM_FreeArv_DOUBLE As Integer = 0 '【合計】空-地区外発(到着) 両開き

        Dim SUM_FreeDepArvDiff_OTHER As Integer = 0 '【合計】空-発着差 その他
        Dim SUM_FreeDepArvDiff_LSHAP As Integer = 0 '【合計】空-発着差 L字
        Dim SUM_FreeDepArvDiff_DOUBLE As Integer = 0 '【合計】空-発着差 両開き

        Dim SUM_TotalDepArvDiff_OTHER As Integer = 0 '【合計】計-発着差計 その他
        Dim SUM_TotalDepArvDiff_LSHAP As Integer = 0 '【合計】計-発着差計 L字
        Dim SUM_TotalDepArvDiff_DOUBLE As Integer = 0 '【合計】計-発着差計 両開き

        Dim SUM_TotalPresentTrain As Integer = 0 '【合計】計-列車現在
        Dim SUM_TotalAnchorageNum As Integer = 0 '【合計】計-停泊個数
        Dim SUM_TotalAfter10Date As Integer = 0 '【合計】計-(内１０日以上)
        Dim SUM_TotalPresentNum As Integer = 0 '【合計】計-総現在

        For Each Row As DataRow In WW_Row
            WW_StackDep_OTHER = CInt(Row("STACK_DEP_OTHER")) '積-地区外行(発送) その他
            WW_StackDep_LSHAP = CInt(Row("STACK_DEP_LSHAP")) '積-地区外行(発送) L字
            WW_StackDep_DOUBLE = CInt(Row("STACK_DEP_DOUBLE")) '積-地区外行(発送) 両開き

            WW_StackArv_OTHER = CInt(Row("STACK_ARV_OTHER")) '積-地区外発(到着) その他
            WW_StackArv_LSHAP = CInt(Row("STACK_ARV_LSHAP")) '積-地区外発(到着) L字
            WW_StackArv_DOUBLE = CInt(Row("STACK_ARV_DOUBLE")) '積-地区外発(到着) 両開き

            WW_FreeDep_OTHER = CInt(Row("FREE_DEP_OTHER")) '空-地区外行(発送) その他
            WW_FreeDep_LSHAP = CInt(Row("FREE_DEP_LSHAP")) '空-地区外行(発送) L字
            WW_FreeDep_DOUBLE = CInt(Row("FREE_DEP_DOUBLE")) '空-地区外行(発送) 両開き

            WW_FreeArv_OTHER = CInt(Row("FREE_ARV_OTHER")) '空-地区外発(到着) その他
            WW_FreeArv_LSHAP = CInt(Row("FREE_ARV_LSHAP")) '空-地区外発(到着) L字
            WW_FreeArv_DOUBLE = CInt(Row("FREE_ARV_DOUBLE")) '空-地区外発(到着) 両開き

            '計算
            WW_StackDepArvDiff_OTHER = WW_StackArv_OTHER - WW_StackDep_OTHER　'積-発着差 その他
            WW_StackDepArvDiff_LSHAP = WW_StackArv_LSHAP - WW_StackDep_LSHAP　'積-発着差 L字
            WW_StackDepArvDiff_DOUBLE = WW_StackArv_DOUBLE - WW_StackDep_DOUBLE　'積-発着差 両開き

            WW_FreeDepArvDiff_OTHER = WW_FreeArv_OTHER - WW_FreeDep_OTHER '空-発着差 その他
            WW_FreeDepArvDiff_LSHAP = WW_FreeArv_LSHAP - WW_FreeDep_LSHAP '空-発着差 L字
            WW_FreeDepArvDiff_DOUBLE = WW_FreeArv_DOUBLE - WW_FreeDep_DOUBLE '空-発着差 両開き

            WW_TotalDepArvDiff_OTHER = WW_StackDepArvDiff_OTHER - WW_FreeDepArvDiff_OTHER '計-発着差計 その他
            WW_TotalDepArvDiff_LSHAP = WW_StackDepArvDiff_LSHAP - WW_FreeDepArvDiff_LSHAP '計-発着差計 L字
            WW_TotalDepArvDiff_DOUBLE = WW_StackDepArvDiff_DOUBLE - WW_FreeDepArvDiff_DOUBLE '計-発着差計 両開き

            WW_TotalPresentTrain = CInt(Row("STACK_PRESENTTRAIN")) + CInt(Row("FREE_PRESENTTRAIN")) '計-列車現在
            WW_TotalAnchorageNum = CInt(Row("STACK_ANCHORAGENUM")) + CInt(Row("FREE_ANCHORAGENUM")) '計-停泊個数
            WW_TotalAfter10Date = CInt(Row("STACK_AFTER10DATE")) + CInt(Row("FREE_AFTER10DATE")) '計-停泊個数

            'WW_TotalPresentNum = WW_TotalPresentTrain + WW_TotalAnchorageNum '計-総現在
            WW_TotalPresentNum = CInt(Row("TOTAL_PRESENT_NUM")) '計-総現在


            '月合計
            SUM_StackDep_OTHER += WW_StackDep_OTHER '【合計】積-地区外行(発送) その他
            SUM_StackDep_LSHAP += WW_StackDep_LSHAP '【合計】積-地区外行(発送) L字
            SUM_StackDep_DOUBLE += WW_StackDep_DOUBLE '【合計】積-地区外行(発送) 両開き

            SUM_StackArv_OTHER += WW_StackArv_OTHER '【合計】積-地区外発(到着) その他
            SUM_StackArv_LSHAP += WW_StackArv_LSHAP '【合計】積-地区外発(到着) L字
            SUM_StackArv_DOUBLE += WW_StackArv_DOUBLE '【合計】積-地区外発(到着) 両開き

            SUM_StackDepArvDiff_OTHER += WW_StackDepArvDiff_OTHER '【合計】積-発着差 その他
            SUM_StackDepArvDiff_LSHAP += WW_StackDepArvDiff_LSHAP '【合計】積-発着差 L字
            SUM_StackDepArvDiff_DOUBLE += WW_StackDepArvDiff_DOUBLE '【合計】積-発着差 両開き

            SUM_FreeDep_OTHER += WW_FreeDep_OTHER '【合計】空-地区外行(発送) その他
            SUM_FreeDep_LSHAP += WW_FreeDep_LSHAP '【合計】空-地区外行(発送) L字
            SUM_FreeDep_DOUBLE += WW_FreeDep_DOUBLE '【合計】空-地区外行(発送) 両開き

            SUM_FreeArv_OTHER += WW_FreeArv_OTHER '【合計】空-地区外発(到着) その他
            SUM_FreeArv_LSHAP += WW_FreeArv_LSHAP '【合計】空-地区外発(到着) L字
            SUM_FreeArv_DOUBLE += WW_FreeArv_DOUBLE '【合計】空-地区外発(到着) 両開き

            SUM_FreeDepArvDiff_OTHER += WW_FreeDepArvDiff_OTHER '【合計】空-発着差 その他
            SUM_FreeDepArvDiff_LSHAP += WW_FreeDepArvDiff_LSHAP '【合計】空-発着差 L字
            SUM_FreeDepArvDiff_DOUBLE += WW_FreeDepArvDiff_DOUBLE '【合計】空-発着差 両開き

            SUM_TotalDepArvDiff_OTHER += WW_TotalDepArvDiff_OTHER '【合計】計-発着差計 その他
            SUM_TotalDepArvDiff_LSHAP += WW_TotalDepArvDiff_LSHAP '【合計】計-発着差計 L字
            SUM_TotalDepArvDiff_DOUBLE += WW_TotalDepArvDiff_DOUBLE '【合計】計-発着差計 両開き

            SUM_TotalPresentTrain += WW_TotalPresentTrain '【合計】計-列車現在
            SUM_TotalAnchorageNum += WW_TotalAnchorageNum '【合計】計-停泊個数
            SUM_TotalAfter10Date += WW_TotalAfter10Date '【合計】計-(内１０日以上)
            SUM_TotalPresentNum += WW_TotalPresentNum '【合計】計-総現在

            Select Case Row("TARGETDAY").ToString
                Case "01"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_01, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_01, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_01, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_01, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_01, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_01, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_01, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_01, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_01, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_01, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_01, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_01, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_01, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_01, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_01, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_01, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_01, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_01, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_01, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_01, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_01, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_01, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_01, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_01, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_01, WW_TotalPresentNum.ToString)
                Case "02"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_02, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_02, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_02, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_02, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_02, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_02, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_02, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_02, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_02, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_02, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_02, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_02, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_02, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_02, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_02, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_02, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_02, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_02, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_02, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_02, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_02, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_02, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_02, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_02, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_02, WW_TotalPresentNum.ToString)
                Case "03"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_03, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_03, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_03, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_03, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_03, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_03, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_03, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_03, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_03, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_03, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_03, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_03, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_03, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_03, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_03, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_03, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_03, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_03, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_03, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_03, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_03, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_03, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_03, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_03, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_03, WW_TotalPresentNum.ToString)
                Case "04"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_04, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_04, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_04, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_04, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_04, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_04, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_04, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_04, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_04, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_04, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_04, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_04, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_04, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_04, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_04, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_04, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_04, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_04, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_04, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_04, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_04, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_04, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_04, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_04, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_04, WW_TotalPresentNum.ToString)
                Case "05"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_05, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_05, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_05, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_05, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_05, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_05, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_05, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_05, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_05, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_05, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_05, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_05, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_05, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_05, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_05, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_05, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_05, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_05, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_05, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_05, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_05, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_05, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_05, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_05, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_05, WW_TotalPresentNum.ToString)
                Case "06"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_06, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_06, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_06, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_06, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_06, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_06, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_06, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_06, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_06, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_06, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_06, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_06, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_06, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_06, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_06, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_06, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_06, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_06, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_06, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_06, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_06, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_06, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_06, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_06, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_06, WW_TotalPresentNum.ToString)
                Case "07"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_07, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_07, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_07, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_07, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_07, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_07, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_07, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_07, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_07, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_07, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_07, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_07, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_07, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_07, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_07, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_07, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_07, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_07, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_07, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_07, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_07, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_07, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_07, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_07, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_07, WW_TotalPresentNum.ToString)
                Case "08"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_08, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_08, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_08, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_08, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_08, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_08, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_08, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_08, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_08, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_08, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_08, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_08, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_08, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_08, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_08, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_08, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_08, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_08, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_08, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_08, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_08, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_08, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_08, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_08, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_08, WW_TotalPresentNum.ToString)
                Case "09"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_09, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_09, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_09, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_09, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_09, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_09, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_09, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_09, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_09, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_09, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_09, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_09, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_09, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_09, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_09, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_09, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_09, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_09, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_09, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_09, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_09, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_09, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_09, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_09, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_09, WW_TotalPresentNum.ToString)
                Case "10"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_10, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_10, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_10, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_10, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_10, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_10, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_10, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_10, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_10, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_10, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_10, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_10, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_10, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_10, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_10, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_10, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_10, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_10, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_10, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_10, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_10, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_10, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_10, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_10, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_10, WW_TotalPresentNum.ToString)
                Case "11"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_11, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_11, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_11, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_11, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_11, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_11, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_11, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_11, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_11, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_11, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_11, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_11, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_11, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_11, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_11, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_11, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_11, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_11, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_11, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_11, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_11, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_11, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_11, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_11, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_11, WW_TotalPresentNum.ToString)
                Case "12"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_12, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_12, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_12, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_12, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_12, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_12, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_12, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_12, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_12, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_12, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_12, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_12, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_12, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_12, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_12, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_12, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_12, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_12, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_12, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_12, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_12, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_12, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_12, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_12, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_12, WW_TotalPresentNum.ToString)
                Case "13"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_13, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_13, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_13, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_13, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_13, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_13, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_13, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_13, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_13, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_13, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_13, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_13, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_13, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_13, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_13, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_13, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_13, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_13, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_13, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_13, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_13, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_13, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_13, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_13, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_13, WW_TotalPresentNum.ToString)
                Case "14"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_14, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_14, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_14, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_14, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_14, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_14, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_14, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_14, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_14, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_14, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_14, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_14, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_14, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_14, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_14, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_14, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_14, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_14, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_14, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_14, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_14, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_14, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_14, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_14, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_14, WW_TotalPresentNum.ToString)
                Case "15"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.OTHER_DAY_15, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.LSHAP_DAY_15, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDep, ColStPos01To15.DOUBLE_DAY_15, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.OTHER_DAY_15, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.LSHAP_DAY_15, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackArv, ColStPos01To15.DOUBLE_DAY_15, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.OTHER_DAY_15, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.LSHAP_DAY_15, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.StackDepArvDiff, ColStPos01To15.DOUBLE_DAY_15, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.OTHER_DAY_15, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.LSHAP_DAY_15, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDep, ColStPos01To15.DOUBLE_DAY_15, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.OTHER_DAY_15, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.LSHAP_DAY_15, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeArv, ColStPos01To15.DOUBLE_DAY_15, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.OTHER_DAY_15, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.LSHAP_DAY_15, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.FreeDepArvDiff, ColStPos01To15.DOUBLE_DAY_15, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.OTHER_DAY_15, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.LSHAP_DAY_15, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalDepArvDiff, ColStPos01To15.DOUBLE_DAY_15, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentTrain, ColStPos01To15.OTHER_DAY_15, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAnchorageNum, ColStPos01To15.OTHER_DAY_15, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalAfter10Date, ColStPos01To15.OTHER_DAY_15, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos01To15.TotalPresentNum, ColStPos01To15.OTHER_DAY_15, WW_TotalPresentNum.ToString)
                Case "16"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_16, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_16, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_16, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_16, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_16, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_16, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_16, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_16, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_16, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_16, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_16, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_16, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_16, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_16, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_16, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_16, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_16, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_16, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_16, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_16, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_16, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_16, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_16, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_16, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_16, WW_TotalPresentNum.ToString)
                Case "17"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_17, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_17, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_17, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_17, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_17, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_17, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_17, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_17, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_17, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_17, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_17, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_17, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_17, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_17, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_17, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_17, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_17, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_17, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_17, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_17, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_17, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_17, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_17, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_17, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_17, WW_TotalPresentNum.ToString)
                Case "18"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_18, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_18, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_18, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_18, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_18, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_18, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_18, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_18, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_18, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_18, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_18, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_18, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_18, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_18, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_18, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_18, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_18, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_18, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_18, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_18, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_18, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_18, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_18, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_18, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_18, WW_TotalPresentNum.ToString)
                Case "19"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_19, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_19, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_19, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_19, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_19, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_19, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_19, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_19, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_19, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_19, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_19, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_19, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_19, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_19, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_19, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_19, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_19, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_19, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_19, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_19, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_19, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_19, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_19, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_19, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_19, WW_TotalPresentNum.ToString)
                Case "20"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_20, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_20, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_20, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_20, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_20, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_20, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_20, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_20, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_20, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_20, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_20, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_20, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_20, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_20, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_20, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_20, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_20, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_20, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_20, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_20, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_20, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_20, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_20, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_20, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_20, WW_TotalPresentNum.ToString)
                Case "21"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_21, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_21, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_21, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_21, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_21, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_21, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_21, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_21, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_21, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_21, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_21, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_21, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_21, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_21, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_21, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_21, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_21, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_21, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_21, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_21, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_21, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_21, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_21, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_21, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_21, WW_TotalPresentNum.ToString)
                Case "22"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_22, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_22, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_22, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_22, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_22, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_22, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_22, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_22, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_22, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_22, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_22, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_22, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_22, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_22, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_22, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_22, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_22, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_22, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_22, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_22, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_22, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_22, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_22, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_22, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_22, WW_TotalPresentNum.ToString)
                Case "23"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_23, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_23, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_23, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_23, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_23, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_23, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_23, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_23, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_23, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_23, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_23, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_23, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_23, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_23, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_23, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_23, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_23, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_23, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_23, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_23, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_23, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_23, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_23, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_23, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_23, WW_TotalPresentNum.ToString)
                Case "24"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_24, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_24, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_24, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_24, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_24, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_24, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_24, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_24, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_24, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_24, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_24, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_24, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_24, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_24, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_24, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_24, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_24, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_24, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_24, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_24, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_24, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_24, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_24, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_24, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_24, WW_TotalPresentNum.ToString)
                Case "25"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_25, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_25, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_25, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_25, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_25, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_25, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_25, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_25, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_25, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_25, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_25, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_25, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_25, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_25, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_25, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_25, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_25, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_25, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_25, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_25, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_25, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_25, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_25, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_25, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_25, WW_TotalPresentNum.ToString)
                Case "26"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_26, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_26, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_26, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_26, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_26, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_26, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_26, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_26, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_26, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_26, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_26, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_26, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_26, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_26, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_26, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_26, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_26, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_26, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_26, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_26, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_26, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_26, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_26, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_26, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_26, WW_TotalPresentNum.ToString)
                Case "27"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_27, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_27, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_27, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_27, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_27, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_27, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_27, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_27, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_27, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_27, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_27, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_27, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_27, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_27, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_27, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_27, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_27, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_27, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_27, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_27, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_27, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_27, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_27, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_27, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_27, WW_TotalPresentNum.ToString)
                Case "28"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_28, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_28, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_28, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_28, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_28, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_28, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_28, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_28, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_28, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_28, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_28, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_28, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_28, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_28, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_28, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_28, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_28, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_28, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_28, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_28, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_28, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_28, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_28, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_28, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_28, WW_TotalPresentNum.ToString)
                Case "29"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_29, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_29, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_29, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_29, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_29, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_29, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_29, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_29, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_29, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_29, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_29, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_29, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_29, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_29, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_29, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_29, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_29, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_29, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_29, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_29, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_29, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_29, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_29, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_29, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_29, WW_TotalPresentNum.ToString)
                Case "30"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_30, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_30, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_30, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_30, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_30, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_30, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_30, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_30, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_30, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_30, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_30, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_30, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_30, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_30, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_30, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_30, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_30, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_30, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_30, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_30, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_30, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_30, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_30, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_30, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_30, WW_TotalPresentNum.ToString)
                Case "31"
                    '積-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_DAY_31, WW_StackDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_DAY_31, WW_StackDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_DAY_31, WW_StackDep_DOUBLE.ToString) '両開き

                    '積-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_DAY_31, WW_StackArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_DAY_31, WW_StackArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_DAY_31, WW_StackArv_DOUBLE.ToString) '両開き

                    '積-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_DAY_31, WW_StackDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_DAY_31, WW_StackDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_DAY_31, WW_StackDepArvDiff_DOUBLE.ToString) '両開き

                    '空-地区外行(発送)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_DAY_31, WW_FreeDep_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_DAY_31, WW_FreeDep_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_DAY_31, WW_FreeDep_DOUBLE.ToString) '両開き

                    '空-地区外発(到着)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_DAY_31, WW_FreeArv_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_DAY_31, WW_FreeArv_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_DAY_31, WW_FreeArv_DOUBLE.ToString) '両開き

                    '空-発着差
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_DAY_31, WW_FreeDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_DAY_31, WW_FreeDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_DAY_31, WW_FreeDepArvDiff_DOUBLE.ToString) '両開き

                    '計-発着差計
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_DAY_31, WW_TotalDepArvDiff_OTHER.ToString) 'その他
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_DAY_31, WW_TotalDepArvDiff_LSHAP.ToString) 'L字
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_DAY_31, WW_TotalDepArvDiff_DOUBLE.ToString) '両開き

                    '計-列車現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_DAY_31, WW_TotalPresentTrain.ToString)
                    '計-停泊個数
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_DAY_31, WW_TotalAnchorageNum.ToString)
                    '計-(内１０日以上)
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_DAY_31, WW_TotalAfter10Date.ToString)
                    '計-総現在
                    SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_DAY_31, WW_TotalPresentNum.ToString)
            End Select
        Next

        '月間計
        '積-地区外行(発送)
        SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.OTHER_MonthTotalDAY, SUM_StackDep_OTHER.ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.LSHAP_MonthTotalDAY, SUM_StackDep_LSHAP.ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDep, ColStPos16To31.DOUBLE_MonthTotalDAY, SUM_StackDep_DOUBLE.ToString) '両開き

        '積-地区外発(到着)
        SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.OTHER_MonthTotalDAY, SUM_StackArv_OTHER.ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.LSHAP_MonthTotalDAY, SUM_StackArv_LSHAP.ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos16To31.StackArv, ColStPos16To31.DOUBLE_MonthTotalDAY, SUM_StackArv_DOUBLE.ToString) '両開き

        '積-発着差
        SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.OTHER_MonthTotalDAY, SUM_StackDepArvDiff_OTHER.ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.LSHAP_MonthTotalDAY, SUM_StackDepArvDiff_LSHAP.ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos16To31.StackDepArvDiff, ColStPos16To31.DOUBLE_MonthTotalDAY, SUM_StackDepArvDiff_DOUBLE.ToString) '両開き

        '空-地区外行(発送)
        SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.OTHER_MonthTotalDAY, SUM_FreeDep_OTHER.ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.LSHAP_MonthTotalDAY, SUM_FreeDep_LSHAP.ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDep, ColStPos16To31.DOUBLE_MonthTotalDAY, SUM_FreeDep_DOUBLE.ToString) '両開き

        '空-地区外発(到着)
        SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.OTHER_MonthTotalDAY, SUM_FreeArv_OTHER.ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.LSHAP_MonthTotalDAY, SUM_FreeArv_LSHAP.ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeArv, ColStPos16To31.DOUBLE_MonthTotalDAY, SUM_FreeArv_DOUBLE.ToString) '両開き

        '空-発着差
        SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.OTHER_MonthTotalDAY, SUM_FreeDepArvDiff_OTHER.ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.LSHAP_MonthTotalDAY, SUM_FreeDepArvDiff_LSHAP.ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos16To31.FreeDepArvDiff, ColStPos16To31.DOUBLE_MonthTotalDAY, SUM_FreeDepArvDiff_DOUBLE.ToString) '両開き

        '計-発着差計
        SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.OTHER_MonthTotalDAY, SUM_TotalDepArvDiff_OTHER.ToString) 'その他
        SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.LSHAP_MonthTotalDAY, SUM_TotalDepArvDiff_LSHAP.ToString) 'L字
        SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalDepArvDiff, ColStPos16To31.DOUBLE_MonthTotalDAY, SUM_TotalDepArvDiff_DOUBLE.ToString) '両開き

        '計-列車現在
        SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentTrain, ColStPos16To31.OTHER_MonthTotalDAY, SUM_TotalPresentTrain.ToString)
        '計-停泊個数
        SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAnchorageNum, ColStPos16To31.OTHER_MonthTotalDAY, SUM_TotalAnchorageNum.ToString)
        '計-(内１０日以上)
        SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalAfter10Date, ColStPos16To31.OTHER_MonthTotalDAY, SUM_TotalAfter10Date.ToString)
        '計-総現在
        SetData(sheet, WW_OffsetRow, RowStPos16To31.TotalPresentNum, ColStPos16To31.OTHER_MonthTotalDAY, SUM_TotalPresentNum.ToString)

    End Sub

    ''' <summary>
    ''' 帳票にデータをセット
    ''' </summary>
    Private Sub SetData(ByVal sheet As IWorksheet, ByVal WW_OFFSETROW As Integer, ByVal WW_ROW As Integer, ByVal WW_COL As Integer, ByVal WW_DATA As String)
        If Not WW_DATA = "0" Then
            sheet.Range(WW_ROW + WW_OFFSETROW, WW_COL).Value = CInt(WW_DATA)
        End If
    End Sub

    ''' <summary>
    ''' 発着差実績データ取得
    ''' </summary>
    ''' <param name="WW_CATEGORY">帳票の出力種類(全支店出力、駅コード指定出力、支店、駅指定出力など)</param>
    ''' <param name="WW_ORGCODE">画面選択支店コード</param>
    ''' <param name="WW_STATIONCODE">画面入力駅コード</param>
    ''' <param name="WW_STATIONLIST">画面選択駅コード</param>
    ''' <param name="WW_BIGCTNCDLIST">画面選択コンテナ種別コード</param>
    ''' <param name="WW_DATESTART">画面選択開始日</param>
    ''' <param name="WW_DATEEND">画面選択終了日</param>
    ''' <param name="WW_ENDLASTYMD">前月末</param>
    ''' <param name="WW_EXTRACTIONTYPE">抽出タイプ(全体または単体)</param>
    ''' <returns>DataTable</returns>
    Private Function GetDepArvDiff(ByVal WW_CATEGORY As Integer,
                                         ByVal WW_ORGCODE As String,
                                         ByVal WW_STATIONCODE As String,
                                         ByVal WW_STATIONLIST As ArrayList,
                                         ByVal WW_BIGCTNCDLIST As ArrayList,
                                         ByVal WW_DATESTART As String,
                                         ByVal WW_DATEEND As String,
                                         ByVal WW_ENDLASTYMD As String,
                                         ByVal WW_EXTRACTIONTYPE As Integer
                                         ) As DataTable

        Dim dt As DataTable = New DataTable()
        dt.Clear()

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()

            Dim SQLStr = New StringBuilder
            SQLStr.AppendLine(" SELECT ")
            SQLStr.AppendLine("     FORMAT(DATADATE, 'yyyyMM') AS FILTERYM ")
            SQLStr.AppendLine("     , FORMAT(DATADATE, 'dd') AS TARGETDAY ")
            SQLStr.AppendLine("     , DATADATE AS DATADATE ")
            Select Case WW_EXTRACTIONTYPE
                Case ExtractionType.AllMode '全体抽出の場合
                    If Not WW_CATEGORY = SelectCategory.ALL Then
                        SQLStr.AppendLine("     , ORGCODE AS ORGCODE ")
                    End If
                Case ExtractionType.SingleMode '単体抽出の場合
                    SQLStr.AppendLine("     , ORGCODE AS ORGCODE ")
                    If Not WW_CATEGORY = LNT0004_DepartureArrivalDifferenceDailyList_DIODOC.SelectCategory.ALL Then '全支店検索の場合駅は不要
                        SQLStr.AppendLine("     , STATIONCODE AS STATIONCODE ")
                    End If
            End Select
            SQLStr.AppendLine("     , CONVERT(INT, SUM(STACK_DEP_LSHAP)) AS STACK_DEP_LSHAP ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(STACK_DEP_DOUBLE)) AS STACK_DEP_DOUBLE ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(STACK_DEP_OTHER)) AS STACK_DEP_OTHER ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(FREE_DEP_LSHAP)) AS FREE_DEP_LSHAP ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(FREE_DEP_DOUBLE)) AS FREE_DEP_DOUBLE ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(FREE_DEP_OTHER)) AS FREE_DEP_OTHER ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(STACK_ARV_LSHAP)) AS STACK_ARV_LSHAP ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(STACK_ARV_DOUBLE)) AS STACK_ARV_DOUBLE ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(STACK_ARV_OTHER)) AS STACK_ARV_OTHER ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(FREE_ARV_LSHAP)) AS FREE_ARV_LSHAP ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(FREE_ARV_DOUBLE)) AS FREE_ARV_DOUBLE ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(FREE_ARV_OTHER)) AS FREE_ARV_OTHER ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(STACK_PRESENTTRAIN)) AS STACK_PRESENTTRAIN ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(FREE_PRESENTTRAIN)) AS FREE_PRESENTTRAIN ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(STACK_ANCHORAGENUM)) AS STACK_ANCHORAGENUM ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(FREE_ANCHORAGENUM)) AS FREE_ANCHORAGENUM ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(STACK_AFTER10DATE)) AS STACK_AFTER10DATE ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(FREE_AFTER10DATE)) AS FREE_AFTER10DATE  ")
            SQLStr.AppendLine("     , CONVERT(INT, SUM(TOTAL_PRESENT_NUM)) AS TOTAL_PRESENT_NUM  ")
            SQLStr.AppendLine(" FROM ")
            SQLStr.AppendLine("     LNG.VIW0015_DEPARVDIFF_OPESITUATION  ")
            SQLStr.AppendLine(" WHERE ")
            '■共通条件
            'データ年月日FROM、TO 、前月末
            SQLStr.AppendLine("     (  ")
            SQLStr.AppendLine("         DATADATE BETWEEN CONVERT(DATE, '" + WW_DATESTART + "') AND CONVERT(DATE, '" + WW_DATEEND + "')  ")
            SQLStr.AppendLine("         OR DATADATE = CONVERT(DATE, '" + WW_ENDLASTYMD + "') ")
            SQLStr.AppendLine("     )  ")
            '種別(大分類コード)
            Select Case True
                Case WW_BIGCTNCDLIST.Count = CONST_SEL_MAX_BIGCTNCNT '全選択の場合抽出条件に含めない
                Case WW_BIGCTNCDLIST.Count = 1
                    SQLStr.AppendLine(" AND BIGCTNCD = '" + WW_BIGCTNCDLIST(0) + "'")
                Case WW_BIGCTNCDLIST.Count = 2
                    SQLStr.AppendLine(" AND BIGCTNCD IN ('" + WW_BIGCTNCDLIST(0) + "'")
                    SQLStr.AppendLine("                 ,'" + WW_BIGCTNCDLIST(1) + "')")
                Case WW_BIGCTNCDLIST.Count > 2
                    SQLStr.AppendLine(" AND BIGCTNCD IN ('" + WW_BIGCTNCDLIST(0) + "'")
                    For i As Integer = 1 To WW_BIGCTNCDLIST.Count - 2 '先頭と末尾以外
                        SQLStr.AppendLine("    ,'" + WW_BIGCTNCDLIST(i) + "'")
                    Next
                    SQLStr.AppendLine("    ,'" + WW_BIGCTNCDLIST(WW_BIGCTNCDLIST.Count - 1) + "')")
                Case Else
            End Select
            '■画面選択状態
            Select Case WW_CATEGORY
                Case LNT0004_DepartureArrivalDifferenceDailyList_DIODOC.SelectCategory.STATIONCODEINPUT '駅コード入力の場合 対象駅のデータを取得
                    SQLStr.AppendLine(" AND STATIONCODE = '" + WW_STATIONCODE + "'")
                Case LNT0004_DepartureArrivalDifferenceDailyList_DIODOC.SelectCategory.ALL '全支店検索の場合条件追加無し
                Case Else 'その他の場合 駅毎のデータを取得(支店は固定)
                    SQLStr.AppendLine(" AND ORGCODE = '" + WW_ORGCODE + "'")

                    Select Case True
                        Case WW_STATIONLIST.Count = 1
                            SQLStr.AppendLine(" AND STATIONCODE = '" + WW_STATIONLIST(0) + "'")
                        Case WW_STATIONLIST.Count = 2
                            SQLStr.AppendLine(" AND STATIONCODE IN ('" + WW_STATIONLIST(0) + "'")
                            SQLStr.AppendLine("                    ,'" + WW_STATIONLIST(1) + "')")
                        Case WW_STATIONLIST.Count > 2
                            SQLStr.AppendLine(" AND STATIONCODE IN ('" + WW_STATIONLIST(0) + "'")
                            For i As Integer = 1 To WW_STATIONLIST.Count - 2 '先頭と末尾以外
                                SQLStr.AppendLine("    ,'" + WW_STATIONLIST(i) + "'")
                            Next
                            SQLStr.AppendLine("    ,'" + WW_STATIONLIST(WW_STATIONLIST.Count - 1) + "')")
                        Case Else
                    End Select
            End Select

            SQLStr.AppendLine(" GROUP BY ")
            SQLStr.AppendLine("     DATADATE ")
            Select Case WW_EXTRACTIONTYPE
                Case ExtractionType.AllMode '全体抽出の場合
                    If Not WW_CATEGORY = SelectCategory.ALL Then
                        SQLStr.AppendLine("     , ORGCODE ")
                    End If
                Case ExtractionType.SingleMode　'単体抽出の場合
                    SQLStr.AppendLine("     , ORGCODE ")
                    If Not WW_CATEGORY = LNT0004_DepartureArrivalDifferenceDailyList_DIODOC.SelectCategory.ALL Then '全支店検索の場合駅は不要
                        SQLStr.AppendLine("     , STATIONCODE  ")
                    End If
            End Select

            SQLStr.AppendLine(" ORDER BY ")
            SQLStr.AppendLine("     DATADATE ")
            Select Case WW_EXTRACTIONTYPE
                Case ExtractionType.AllMode '全体抽出の場合
                    If Not WW_CATEGORY = SelectCategory.ALL Then
                        SQLStr.AppendLine("     , ORGCODE ")
                    End If
                Case ExtractionType.SingleMode　'単体抽出の場合
                    SQLStr.AppendLine("     , ORGCODE ")
                    If Not WW_CATEGORY = LNT0004_DepartureArrivalDifferenceDailyList_DIODOC.SelectCategory.ALL Then '全支店検索の場合駅は不要
                        SQLStr.AppendLine("     , STATIONCODE  ")
                    End If
            End Select

            Try
                Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
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
            End Try
        End Using
        Return dt
    End Function

End Class
