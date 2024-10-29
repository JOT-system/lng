''************************************************************
' 経理連携データExcel出力処理(DioDoc)
' 作成日 2023/06/23
' 作成者 名取
' 更新日 
' 更新者 
'
' 修正履歴:
''************************************************************
Option Strict On
Imports System.Runtime.InteropServices
Imports MySQL.Data.MySqlClient
Imports GrapeCity.Documents.Excel
Imports GrapeCity.Documents.Drawing
Imports System.Drawing
''' <summary>
''' 経理連携帳票作成クラス
''' </summary>
Public Class CPT0019_AccountingData_DIODOC

    ''' <summary>
    ''' 雛形ファイルパス
    ''' </summary>
    Private ExcelTemplatePath As String = ""
    Private UploadRootPath As String = ""
    Private UrlRoot As String = ""
    Private tmpFileName As String = ""
    Private PrintData As DataTable

    Private WW_Workbook As New Workbook
    Private WW_SheetNo As Integer = 0
    Private WW_tmpSheetNo As Integer = 0
    Private WW_InsDate As Date
    Private WW_CampCode As String = ""
    Private WW_KeyYMD As String = ""

    Private Master As New LNGMasterPage          'マスタページ情報
    Private CS0011LOGWrite As New CS0011LOGWrite 'ログ出力
    Private CS0050SESSION As New CS0050SESSION   'セッション情報操作処理

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
            'URLのルートを表示
            Me.UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

            'ファイルOPEN
            WW_Workbook.Open(Me.ExcelTemplatePath)

            For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                If WW_Workbook.Worksheets(i).Name = "経理仕訳データ" Then
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

        Dim tmpFileName As String = "経理仕訳データ_" & DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString & ".xlsx"
        Dim tmpFilePath As String = IO.Path.Combine(Me.UploadRootPath, tmpFileName)
        Dim retByte() As Byte
        Dim Excelrow As Integer = 1

        Try

            '○ csvデータ読込
            For Each row As DataRow In PrintData.Rows()

                Excelrow += 1

                '○ 担当者名称
                WW_Workbook.Worksheets(WW_SheetNo).Range("A" + Excelrow.ToString()).Value = row("データ基準")
                WW_Workbook.Worksheets(WW_SheetNo).Range("B" + Excelrow.ToString()).Value = row("仕訳形式入力")
                WW_Workbook.Worksheets(WW_SheetNo).Range("C" + Excelrow.ToString()).Value = row("入力画面番号")
                WW_Workbook.Worksheets(WW_SheetNo).Range("D" + Excelrow.ToString()).Value = row("伝票日付").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("E" + Excelrow.ToString()).Value = row("決算月区分")
                WW_Workbook.Worksheets(WW_SheetNo).Range("F" + Excelrow.ToString()).Value = row("証憑番号").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("G" + Excelrow.ToString()).Value = row("伝票番号").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("H" + Excelrow.ToString()).Value = row("伝票No")
                WW_Workbook.Worksheets(WW_SheetNo).Range("I" + Excelrow.ToString()).Value = row("明細行番号").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("J" + Excelrow.ToString()).Value = row("借方科目")
                WW_Workbook.Worksheets(WW_SheetNo).Range("K" + Excelrow.ToString()).Value = row("借方部門").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("L" + Excelrow.ToString()).Value = row("借方銀行").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("M" + Excelrow.ToString()).Value = row("借方取引先").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("N" + Excelrow.ToString()).Value = row("借方汎用補助1").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("O" + Excelrow.ToString()).Value = row("借方セグメント1").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("P" + Excelrow.ToString()).Value = row("借方セグメント2").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("Q" + Excelrow.ToString()).Value = row("借方セグメント3").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("R" + Excelrow.ToString()).Value = row("借方番号1")
                WW_Workbook.Worksheets(WW_SheetNo).Range("S" + Excelrow.ToString()).Value = row("借方番号2")
                WW_Workbook.Worksheets(WW_SheetNo).Range("T" + Excelrow.ToString()).Value = row("借方消費税区分")
                WW_Workbook.Worksheets(WW_SheetNo).Range("U" + Excelrow.ToString()).Value = row("借方消費税コード")
                WW_Workbook.Worksheets(WW_SheetNo).Range("V" + Excelrow.ToString()).Value = row("借方消費税率区分")
                WW_Workbook.Worksheets(WW_SheetNo).Range("W" + Excelrow.ToString()).Value = row("借方外税同時入力区分")
                WW_Workbook.Worksheets(WW_SheetNo).Range("X" + Excelrow.ToString()).Value = CInt(row("借方金額"))
                WW_Workbook.Worksheets(WW_SheetNo).Range("Y" + Excelrow.ToString()).Value = CInt(row("借方消費税額"))
                WW_Workbook.Worksheets(WW_SheetNo).Range("Z" + Excelrow.ToString()).Value = row("借方外貨金額")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AA" + Excelrow.ToString()).Value = row("借方外貨レート")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AB" + Excelrow.ToString()).Value = row("借方外貨取引区分")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AC" + Excelrow.ToString()).Value = row("貸方科目")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AD" + Excelrow.ToString()).Value = row("貸方部門").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("AE" + Excelrow.ToString()).Value = row("貸方銀行").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("AF" + Excelrow.ToString()).Value = row("貸方取引先").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("AG" + Excelrow.ToString()).Value = row("貸方汎用補助1").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("AH" + Excelrow.ToString()).Value = row("貸方セグメント1").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("AI" + Excelrow.ToString()).Value = row("貸方セグメント2").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("AJ" + Excelrow.ToString()).Value = row("貸方セグメント3").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("AK" + Excelrow.ToString()).Value = row("貸方番号1")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AL" + Excelrow.ToString()).Value = row("貸方番号2")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AM" + Excelrow.ToString()).Value = row("貸方消費税区分")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AN" + Excelrow.ToString()).Value = row("貸方消費税コード")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AO" + Excelrow.ToString()).Value = row("貸方消費税率区分")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AP" + Excelrow.ToString()).Value = row("貸方外税同時入力区分")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AQ" + Excelrow.ToString()).Value = CInt(row("貸方金額"))
                WW_Workbook.Worksheets(WW_SheetNo).Range("AR" + Excelrow.ToString()).Value = CInt(row("貸方消費税額"))
                WW_Workbook.Worksheets(WW_SheetNo).Range("AS" + Excelrow.ToString()).Value = row("貸方外貨金額")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AT" + Excelrow.ToString()).Value = row("貸方外貨レート")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AU" + Excelrow.ToString()).Value = row("貸方外貨取引区分")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AV" + Excelrow.ToString()).Value = row("期日").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("AW" + Excelrow.ToString()).Value = row("摘要").ToString
                WW_Workbook.Worksheets(WW_SheetNo).Range("AX" + Excelrow.ToString()).Value = row("摘要コード1")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AY" + Excelrow.ToString()).Value = row("作成日")
                WW_Workbook.Worksheets(WW_SheetNo).Range("AZ" + Excelrow.ToString()).Value = row("作成時間")
                WW_Workbook.Worksheets(WW_SheetNo).Range("BA" + Excelrow.ToString()).Value = row("作成者").ToString

            Next

            WW_Workbook.Worksheets(WW_SheetNo).Range("A2:BA" + Excelrow.ToString()).Borders.Color = Color.LightSlateGray
            WW_Workbook.Worksheets(WW_SheetNo).Range("A2:BA" + Excelrow.ToString()).Borders.LineStyle = BorderLineStyle.Thin

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
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "CPT0010C LeaseReport", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "CPT0010C LeaseReport"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力
            Throw '呼出し元にThrow
        Finally
        End Try


    End Function

    ''' <summary>
    ''' 画面表示用のURLを取得する
    ''' </summary>
    ''' <returns>画面表示用URL</returns>
    ''' <remarks>作成メソッド、パブリックスコープはここに収める</remarks>
    Public Function CreateDisPlayURL() As String

        '○ 画面表示用urlに変更
        Me.UrlRoot = String.Format("{0}://{1}", HttpContext.Current.Request.Url.Scheme, System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) & "\Downloads\")

        Return UrlRoot & tmpFileName

    End Function
End Class
