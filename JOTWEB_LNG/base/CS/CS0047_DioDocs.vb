Imports System.Web
Imports System.IO
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel


''' <summary>
''' 帳票マージ
''' </summary>
''' <remarks></remarks>
Public Structure CS0047XLSMERGE

    ''' <summary>
    ''' Excelディレクトリ
    ''' </summary>
    ''' <value>Excelディレクトリ</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DIR() As String

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value></value>
    ''' <returns>エラーコード</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String

    ''' <summary>
    ''' 出力Dir＋ファイル名
    ''' </summary>
    ''' <value></value>
    ''' <returns>出力Dir＋ファイル名</returns>
    ''' <remarks></remarks>
    Public Property FILEpath() As String

    ''' <summary>
    ''' 全出力フラグ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property REMOVALLENGTH() As Integer

    ''' <summary>
    ''' 出力URL＋ファイル名
    ''' </summary>
    ''' <value></value>
    ''' <returns>出力URL＋ファイル名</returns>
    ''' <remarks></remarks>
    Public Property URL() As String

    ''' <summary>
    ''' 指定フォルダー内の複数Excelを出力Excel内複数Sheetへ格納
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub CS0047XLSMERGE()

        Dim CS0011LOGWRITE As New CS0011LOGWrite        'ログ出力
        Dim CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

        'DioDocs
        Dim WW_InWorkbook As New Workbook
        Dim WW_OutWorkbook As New Workbook

        Dim W_ExcelLIST As New List(Of String)
        Dim WW_datetime As String = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString
        Dim W_SheetName As String = ""

        '●In PARAMチェック
        '○ 入力フォルダ存在確認＆Excelファイル名抽出 (C:\apple\files\TEXTWORK)
        If Directory.Exists(DIR) Then
            'ファイル格納フォルダ内不要ファイル削除(すべて削除)
            For Each tempFile As String In Directory.GetFiles(DIR, "*.*")
                If InStr(tempFile, ".XLS") > 0 Or InStr(tempFile, ".xls") > 0 Then
                    W_ExcelLIST.Add(tempFile)
                End If
            Next
        Else
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = "CS0047XLSMERGE"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "InParamチェック"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "Excel処理に失敗しました"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        '○ 入力フォルダー内のExcelファイルが存在しない場合はエラー
        If W_ExcelLIST.Count = 0 Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = "CS0047XLSMERGE"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "InParamチェック"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "Excel処理に失敗しました"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End If

        '■Excelデータ処理
        Try
            '計算エンジンの無効化します
            WW_InWorkbook.EnableCalculation = False

            'オプション：開いた後に再計算しません
            Dim WW_Options As XlsxOpenOptions = New XlsxOpenOptions
            WW_Options.DoNotRecalculateAfterOpened = True

            For i As Integer = 0 To W_ExcelLIST.Count - 1
                Try
                    '○入力Excelファイルを開く
                    WW_InWorkbook.Open(W_ExcelLIST(i).ToString, WW_Options)

                    '計算エンジンの無効化します
                    WW_InWorkbook.EnableCalculation = False

                    '○Sheet名指定
                    W_SheetName = Mid(W_ExcelLIST(i).ToString, 1, InStr(W_ExcelLIST(i).ToString, ".") - 1)
                    Do Until InStr(W_SheetName, "\") = 0
                        W_SheetName = Mid(W_SheetName, InStrRev(W_SheetName, "\") + 1, 100)
                    Loop

                    '〇ソート用の名前を削除（先頭から指定された長さまでカットするしてシート名とする）
                    If Not IsNothing(REMOVALLENGTH) AndAlso REMOVALLENGTH <> 0 Then
                        If W_SheetName.Length > REMOVALLENGTH Then
                            W_SheetName = W_SheetName.Remove(0, REMOVALLENGTH)
                        End If
                    End If

                    WW_InWorkbook.ActiveSheet.Name = W_SheetName

                    '○Sheetコピー
                    '出力先の最終Sheetを設定
                    WW_InWorkbook.ActiveSheet.Copy(WW_OutWorkbook)

                Catch ex As Exception
                    ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

                    CS0011LOGWRITE.INFSUBCLASS = "CS0047XLSMERGE"               'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "Excel_Merge"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
                    CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                    'Excel終了＆リリース
                    WW_InWorkbook = Nothing
                    WW_OutWorkbook = Nothing
                    Exit Sub
                End Try
            Next

            'Sheet1（空白シート）ができるので削除
            For i As Integer = WW_OutWorkbook.Worksheets.Count - 1 To 0 Step -1
                If WW_OutWorkbook.Worksheets(i).Name = "Sheet1" Then
                    WW_OutWorkbook.Worksheets(i).Delete()
                End If
            Next

            '○Excelファイル保存準備
            Try
                Dim WW_Dir As String

                ' 印刷用フォルダ作成
                WW_Dir = CS0050SESSION.UPLOAD_PATH & "\" & "PRINTWORK"
                ' 格納フォルダ存在確認＆作成(...\PRINTWORK)
                If Directory.Exists(WW_Dir) Then
                Else
                    Directory.CreateDirectory(WW_Dir)
                End If

                ' 格納フォルダ存在確認＆作成(...\PRINTWORK\ユーザーID)
                WW_Dir = CS0050SESSION.UPLOAD_PATH & "\" & "PRINTWORK" & "\" & CS0050SESSION.USERID
                If Directory.Exists(WW_Dir) Then
                Else
                    Directory.CreateDirectory(WW_Dir)
                End If

            Catch ex As Exception
                ERR = C_MESSAGE_NO.FILE_IO_ERROR

                CS0011LOGWRITE.INFSUBCLASS = "CS0047XLSMERGE"              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "Excel_Folder"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                'Excel終了＆リリース
                WW_InWorkbook = Nothing
                WW_OutWorkbook = Nothing
                Exit Sub
            End Try

            '○Excelファイル保存

            '計算エンジンの有効化します
            WW_OutWorkbook.EnableCalculation = True

            Try
                FILEpath = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID & "\" & WW_datetime & ".XLSX"
                URL = HttpContext.Current.Request.Url.Scheme & "://" & HttpContext.Current.Request.Url.Host & "/" & CS0050SESSION.PRINT_ROOT_URL_NAME & "/" & CS0050SESSION.USERID & "/" & WW_datetime & ".XLSX"
                'Workbook.Saveメソッドを呼び出すと、数式セルの結果値が計算され、Excelファイルに保存されます
                WW_OutWorkbook.Save(FILEpath, SaveFileFormat.Xlsx)

            Catch ex As Exception
                ERR = C_MESSAGE_NO.FILE_IO_ERROR

                CS0011LOGWRITE.INFSUBCLASS = "CS0047XLSMERGE"              'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "Excel_Save"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

                'Excel終了＆リリース
                WW_InWorkbook = Nothing
                WW_OutWorkbook = Nothing
                Exit Sub
            End Try

        Catch ex As Exception
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = "CS0047XLSMERGE"              'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Open"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            'Excel終了＆リリース
            WW_InWorkbook = Nothing
            WW_OutWorkbook = Nothing
            Exit Sub
        End Try

        'Excel終了＆リリース
        WW_InWorkbook = Nothing
        WW_OutWorkbook = Nothing

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

End Structure
