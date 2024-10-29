'Option Strict On
Imports System.Runtime.InteropServices
Imports MySQL.Data.MySqlClient
Imports GrapeCity.Documents.Excel
Imports System.Drawing

Public Class LNT0022_CustomReport : Inherits CmnCustomReport

    ''' <summary>
    ''' テンプレートファイル名称
    ''' </summary>
    Private Const TEMP_XLS_FILE_NAME As String = "LNT0022C.xlsx"

    ''' <summary>
    ''' MAPID
    ''' </summary>
    Private Const MAPID As String = "LNT0022C"

    'コンテナ一覧
    Public ContainerTbl As DataTable = Nothing

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    Public Sub New(containerDt As DataTable)
        MyBase.New(TEMP_XLS_FILE_NAME, MAPID)
        Me.ContainerTbl = containerDt
    End Sub

    ''' <summary>
    ''' 帳票作成
    ''' </summary>
    ''' <returns>ダウンロードURL</returns>
    Public Function CreatePrintData() As String
        '出力ファイル名＆ファイルパス生成
        Dim now As DateTime = DateTime.Now
        Dim tmpFileName As String = MAPID & now.ToString("_yyyyMMddHHmmss") &
                                    String.Format("{0:000}.xlsx", now.Millisecond)
        Dim tmpFilePath As String = IO.Path.Combine(UploadRootPath, tmpFileName)
        '帳票作成
        Try
            Dim ridx As Integer = 5
            Dim range As IRange = Nothing
            Dim sheet As IWorksheet = WW_Workbook.Worksheets(WW_SheetNo)

            For Each dr As DataRow In ContainerTbl.Rows
                Dim overFlg As Boolean = "1".Equals(dr("YEAR16OVERFLG").ToString)

                '記号
                range = sheet.Range("A" & CStr(ridx))
                range.Value = dr("CTNTYPE")
                If overFlg Then range.Interior.Color = Color.FromArgb(255, 255, 102)

                '番号
                range = sheet.Range("B" & CStr(ridx))
                range.Value = dr("CTNNO")
                If overFlg Then range.Interior.Color = Color.FromArgb(255, 255, 102)

                'コンテナ番号
                range = sheet.Range("C" & CStr(ridx))
                range.Value = dr("CONTNUM")
                If overFlg Then range.Interior.Color = Color.FromArgb(255, 255, 102)

                '製造年月
                range = sheet.Range("D" & CStr(ridx))
                range.Value = CDate(dr("CONTRUCTIONYM").ToString).ToString("yyyy年MM月")
                If overFlg Then range.Interior.Color = Color.FromArgb(255, 255, 102)

                '現在駅
                range = sheet.Range("E" & CStr(ridx))
                range.Value = dr("ARRSTATIONNAME")
                If overFlg Then range.Interior.Color = Color.FromArgb(255, 255, 102)

                '交番検査 - 前回実施日
                range = sheet.Range("F" & CStr(ridx))
                range.Value = dr("TRAINSBEFORERUNYMD")

                '交番検査 - 次回実施日
                range = sheet.Range("G" & CStr(ridx))
                range.Value = dr("TRAINSNEXTRUNYMD")

                '４年検査 - 背景色設定
                Dim cautionFlg As Boolean = "1".Equals(dr("YEAR4FLG").ToString)
                Dim warningFlg As Boolean = "1".Equals(dr("YEAR4NOREGFLG").ToString)
                Dim registedFlg As Boolean = Not String.IsNullOrEmpty(dr("YEAR4_INSPECTYMD").ToString)

                range = sheet.Range(String.Format("I{0}:M{0}", ridx))
                If cautionFlg Then
                    range.Interior.Color = Color.FromArgb(255, 255, 102)
                ElseIf warningFlg Then
                    range.Interior.Color = Color.FromArgb(255, 51, 51)
                    range.Font.Color = Color.White
                ElseIf registedFlg Then
                    range.Interior.Color = Color.FromArgb(252, 228, 214)
                End If

                '４年検査 - 検査年
                range = sheet.Range("H" & CStr(ridx))
                range.Value = dr("YEAR4_AFTER")

                '４年検査 - 検査実施日
                range = sheet.Range("I" & CStr(ridx))
                range.Value = dr("YEAR4_INSPECTYMD")

                '４年検査 - 種別
                range = sheet.Range("J" & CStr(ridx))
                range.Value = dr("YEAR4_INSPECTCODE")

                '４年検査 - 種別名
                range = sheet.Range("K" & CStr(ridx))
                range.Value = dr("YEAR4_INSPECTNAME")

                '４年検査 - 実施場所
                range = sheet.Range("L" & CStr(ridx))
                range.Value = dr("YEAR4_ENFORCEPLACE")

                '４年検査 - 点検修理者
                range = sheet.Range("M" & CStr(ridx))
                range.Value = If(String.IsNullOrEmpty(dr("YEAR4_INSPECTVENDOR").ToString),
                                 "", dr("YEAR4_INSPECTVENDOR").ToString & ":" & dr("YEAR4_INSPECTVENDORNAME").ToString)

                '８年検査 - 背景色設定
                cautionFlg = "1".Equals(dr("YEAR8FLG").ToString)
                warningFlg = "1".Equals(dr("YEAR8NOREGFLG").ToString)
                registedFlg = Not String.IsNullOrEmpty(dr("YEAR8_INSPECTYMD").ToString)

                range = sheet.Range(String.Format("O{0}:S{0}", ridx))
                If cautionFlg Then
                    range.Interior.Color = Color.FromArgb(255, 255, 102)
                ElseIf warningFlg Then
                    range.Interior.Color = Color.FromArgb(255, 51, 51)
                    range.Font.Color = Color.White
                ElseIf registedFlg Then
                    range.Interior.Color = Color.FromArgb(252, 228, 214)
                End If

                '８年検査 - 検査年
                range = sheet.Range("N" & CStr(ridx))
                range.Value = dr("YEAR8_AFTER")

                '８年検査 - 検査実施日
                range = sheet.Range("O" & CStr(ridx))
                range.Value = dr("YEAR8_INSPECTYMD")

                '８年検査 - 種別
                range = sheet.Range("P" & CStr(ridx))
                range.Value = dr("YEAR8_INSPECTCODE")

                '８年検査 - 種別名
                range = sheet.Range("Q" & CStr(ridx))
                range.Value = dr("YEAR8_INSPECTNAME")

                '８年検査 - 実施場所
                range = sheet.Range("R" & CStr(ridx))
                range.Value = dr("YEAR8_ENFORCEPLACE")

                '８年検査 - 点検修理者
                range = sheet.Range("S" & CStr(ridx))
                range.Value = If(String.IsNullOrEmpty(dr("YEAR8_INSPECTVENDOR").ToString),
                                 "", dr("YEAR8_INSPECTVENDOR").ToString & ":" & dr("YEAR8_INSPECTVENDORNAME").ToString)

                '１２年検査 - 背景色設定
                cautionFlg = "1".Equals(dr("YEAR12FLG").ToString)
                warningFlg = "1".Equals(dr("YEAR12NOREGFLG").ToString)
                registedFlg = Not String.IsNullOrEmpty(dr("YEAR12_INSPECTYMD").ToString)

                range = sheet.Range(String.Format("U{0}:Y{0}", ridx))
                If cautionFlg Then
                    range.Interior.Color = Color.FromArgb(255, 255, 102)
                ElseIf warningFlg Then
                    range.Interior.Color = Color.FromArgb(255, 51, 51)
                    range.Font.Color = Color.White
                ElseIf registedFlg Then
                    range.Interior.Color = Color.FromArgb(252, 228, 214)
                End If

                '１２年検査 - 検査年
                range = sheet.Range("T" & CStr(ridx))
                range.Value = dr("YEAR12_AFTER")

                '１２年検査 - 検査実施日
                range = sheet.Range("U" & CStr(ridx))
                range.Value = dr("YEAR12_INSPECTYMD")

                '１２年検査 - 種別
                range = sheet.Range("V" & CStr(ridx))
                range.Value = dr("YEAR12_INSPECTCODE")

                '１２年検査 - 種別名
                range = sheet.Range("W" & CStr(ridx))
                range.Value = dr("YEAR12_INSPECTNAME")

                '１２年検査 - 実施場所
                range = sheet.Range("X" & CStr(ridx))
                range.Value = dr("YEAR12_ENFORCEPLACE")

                '１２年検査 - 点検修理者
                range = sheet.Range("Y" & CStr(ridx))
                range.Value = If(String.IsNullOrEmpty(dr("YEAR12_INSPECTVENDOR").ToString),
                                 "", dr("YEAR12_INSPECTVENDOR").ToString & ":" & dr("YEAR12_INSPECTVENDORNAME").ToString)

                'Ｎ年検査
                If dr("YEARN_SEQ") <> 0 Then
                    '背景色
                    range = sheet.Range(String.Format("AB{0}:AF{0}", ridx))
                    range.Interior.Color = Color.FromArgb(252, 228, 214)

                    'Ｎ年
                    range = sheet.Range("Z" & CStr(ridx))
                    range.Value = dr("YEARN_SEQ")
                    range.Interior.Color = Color.FromArgb(252, 228, 214)

                    '検査年
                    range = sheet.Range("AA" & CStr(ridx))
                    range.Value = dr("YEARN_YEAR")

                    '検査実施日
                    range = sheet.Range("AB" & CStr(ridx))
                    range.Value = dr("YEARN_INSPECTYMD")

                    '種別
                    range = sheet.Range("AC" & CStr(ridx))
                    range.Value = dr("YEARN_INSPECTCODE")

                    '種別名
                    range = sheet.Range("AD" & CStr(ridx))
                    range.Value = dr("YEARN_INSPECTNAME")

                    '実施場所
                    range = sheet.Range("AE" & CStr(ridx))
                    range.Value = dr("YEARN_ENFORCEPLACE")

                    '点検修理者
                    range = sheet.Range("AF" & CStr(ridx))
                    range.Value = If(String.IsNullOrEmpty(dr("YEARN_INSPECTVENDOR").ToString),
                                     "", dr("YEARN_INSPECTVENDOR").ToString & ":" & dr("YEARN_INSPECTVENDORNAME").ToString)
                End If

                '追加検査
                If Not String.IsNullOrEmpty(dr("ADD_YEAR").ToString) Then
                    '背景色
                    range = sheet.Range(String.Format("AG{0}:AM{0}", ridx))
                    range.Interior.Color = Color.FromArgb(252, 228, 214)

                    'SEQ
                    range = sheet.Range("AG" & CStr(ridx))
                    range.Value = dr("ADD_SEQ")

                    '検査年
                    range = sheet.Range("AH" & CStr(ridx))
                    range.Value = CInt(dr("ADD_YEAR"))

                    '検査実施日
                    range = sheet.Range("AI" & CStr(ridx))
                    range.Value = dr("ADD_INSPECTYMD")

                    '種別
                    range = sheet.Range("AJ" & CStr(ridx))
                    range.Value = dr("ADD_INSPECTCODE")

                    '種別名
                    range = sheet.Range("AK" & CStr(ridx))
                    range.Value = dr("ADD_INSPECTNAME")

                    '実施場所
                    range = sheet.Range("AL" & CStr(ridx))
                    range.Value = dr("ADD_ENFORCEPLACE")

                    '点検修理者
                    range = sheet.Range("AM" & CStr(ridx))
                    range.Value = If(String.IsNullOrEmpty(dr("ADD_INSPECTVENDOR").ToString),
                                     "", dr("ADD_INSPECTVENDOR").ToString & ":" & dr("ADD_INSPECTVENDORNAME").ToString)
                End If

                ridx += 1
            Next

            'EXCEL帳票保存
            ExcelSaveAs(tmpFilePath)

            Return UrlRoot & tmpFileName

        Catch ex As Exception
            Throw ex
        End Try

    End Function

End Class
