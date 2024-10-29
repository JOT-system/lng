Imports System.IO
Imports System.Runtime.InteropServices
Imports GrapeCity.Documents.Excel

''' <summary>
''' XLSアップロード
''' </summary>
''' <remarks></remarks>
Public Structure CS0023XLSUPLOAD

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value>会社コード</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE As String

    ''' <summary>
    ''' プロファイルID
    ''' </summary>
    ''' <value></value>
    ''' <returns>プロファイルID</returns>
    ''' <remarks></remarks>
    Public Property PROFID As String

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID As String

    ''' <summary>
    ''' 帳票ID
    ''' </summary>
    ''' <value></value>
    ''' <returns>帳票ID</returns>
    ''' <remarks></remarks>
    Public Property REPORTID As String

    ''' <summary>
    ''' 入力対象シート名
    ''' </summary>
    ''' <value></value>
    ''' <returns>入力対象シート名</returns>
    ''' <remarks></remarks>
    Public Property INPUTSHEET As String

    ''' <summary>
    ''' 判定対象項目名
    ''' </summary>
    ''' <value></value>
    ''' <returns>判定対象項目名</returns>
    ''' <remarks></remarks>
    Public Property KEYITEMNAME As String

    ''' <summary>
    ''' 結果tabledata
    ''' </summary>
    ''' <value></value>
    ''' <returns>結果tabledata</returns>
    ''' <remarks></remarks>
    Public Property TBLDATA As DataTable

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value></value>
    ''' <returns>エラーコード</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR As String

    Private Const METHOD_NAME = "CS0023XLSUPLOAD"
    Private Const CONST_SHEET_ALL = "ALL"

    '■共通宣言
    Private CS0011LOGWRITE As CS0011LOGWrite                'LogOutput DirString Get
    Private CS0021PROFXLS As CS0021PROFXLS                  'プロファイル(帳票)取得
    '縦横表なし（STRUCTテーブルなし） 2024/10/18
    'Private CS0028STRUCT As CS0028STRUCT                    '構造取得

    ''' <summary>
    ''' XLSアップロード
    ''' </summary>
    ''' <param name="I_REPORTID">帳票ID</param>
    ''' <param name="I_PROFID">PROFID</param>
    ''' <param name="I_SHEETFLG">全シート対応有無（ALL:全シート対象）</param>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Sub CS0023XLSUPLOAD(Optional ByVal I_REPORTID As String = "", Optional ByVal I_PROFID As String = "", Optional ByVal I_SHEETFLG As String = "")

        Dim CS0050SESSION As New CS0050SESSION                  'セッション情報操作処理

        'DioDocs
        Dim WW_Workbook As New Workbook
        Dim intSheetNo As Integer = 0                           'シート番号（初期値）

        '縦横表Struct定義情報
        Dim WW_Structtbl As New DataTable
        Dim WW_Structrow As DataRow

        Dim WW_ActiveSheetIndex As Integer = 0

        '■InPARAMチェック

        'CAMPCODE
        If IsNothing(CAMPCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CAMPCODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
        End If

        'MAPID
        If IsNothing(MAPID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "MAPID"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If

        '■初期処理

        '〇ExcelFile取得　＆　ExcelOpen
        Dim WW_FILEnm As String = ""
        Try
            'アップロードFILEディレクトリおよびファイル名を取得
            For Each tempFile As String In Directory.GetFiles(CS0050SESSION.UPLOAD_PATH & "\UPLOAD_TMP\" & CS0050SESSION.USERID, "*.*")
                WW_FILEnm = tempFile
                Exit For
            Next

            If WW_FILEnm = "" Then
                ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                                    'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "Excel_Open"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
                CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR
                CS0011LOGWRITE.CS0011LOGWrite()                                             'ログ出力

                'Workbook終了＆リリース
                WW_Workbook = Nothing
                Exit Sub
            End If

            'ExcelOpen
            'オプション：開いた後に再計算しません
            Dim WW_Options As XlsxOpenOptions = New XlsxOpenOptions
            WW_Options.DoNotRecalculateAfterOpened = True
            WW_Workbook.Open(WW_FILEnm, WW_Options)

        Catch ex As Exception
            ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Open"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                             'ログ出力

            'Workbook終了＆リリース
            WW_Workbook = Nothing
            Exit Sub
        End Try

        '〇ExcelよりプロファイルID、レポートID取得
        REPORTID = Nothing
        PROFID = Nothing
        WW_ActiveSheetIndex = 0
        Dim SCOLON As Integer = 0

        For sheetCNT As Integer = 0 To WW_Workbook.Worksheets.Count - 1

            If INPUTSHEET = "" Then
                If WW_Workbook.Worksheets(sheetCNT).Name = "入力" Or WW_Workbook.Worksheets(sheetCNT).Name = "入出力" Then
                    WW_ActiveSheetIndex = sheetCNT
                    Exit For
                End If
            Else
                If WW_Workbook.Worksheets(sheetCNT).Name = INPUTSHEET Then
                    WW_ActiveSheetIndex = sheetCNT
                    Exit For
                End If
            End If
        Next

        For i As Integer = 0 To 50
            For j As Integer = 0 To 100
                If Not WW_Workbook.Worksheets(WW_ActiveSheetIndex).Cells(i, j).Value = Nothing Then
                    Dim WW_str As String = WW_Workbook.Worksheets(WW_ActiveSheetIndex).Cells(i, j).Value.ToString
                    If InStr(WW_str, "ID:") > 0 Then
                        '　レポートIDとプロファイル格納CELLを発見の場合、REPORTID、PROFID取出し
                        REPORTID = Trim(WW_str).Replace("ID:", "")
                        If InStr(REPORTID, ";") > 0 Then
                            SCOLON = InStr(REPORTID, ";")
                            PROFID = Mid(REPORTID, SCOLON + 1, Len(REPORTID))
                            REPORTID = Mid(REPORTID, 1, SCOLON - 1)
                        End If
                        Exit For
                    End If
                End If
            Next
            If SCOLON > 0 Then Exit For
        Next

        'REPORTID取得できない場合はデフォルトIDを設定
        If String.IsNullOrEmpty(REPORTID) Then
            REPORTID = I_REPORTID
            PROFID = I_PROFID
        End If
        'PROFID取得できない場合はデフォルトIDを設定
        If String.IsNullOrEmpty(PROFID) Then
            PROFID = I_PROFID
        End If

        If REPORTID = Nothing Then
            ERR = C_MESSAGE_NO.REPORT_ID_NOT_EXISTS

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel ID not findE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = WW_FILEnm
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.REPORT_ID_NOT_EXISTS
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            'Excel終了＆リリース
            WW_Workbook = Nothing
            Exit Sub
        End If


        '〇出力レイアウト取得
        CS0021PROFXLS.CAMPCODE = CAMPCODE
        CS0021PROFXLS.PROFID = PROFID
        CS0021PROFXLS.MAPID = MAPID
        CS0021PROFXLS.REPORTID = REPORTID
        CS0021PROFXLS.TARGETDATE = ""
        CS0021PROFXLS.CS0021PROFXLS()

        If Not isNormal(CS0021PROFXLS.ERR) Then
            '帳票ID未存在エラー
            ERR = C_MESSAGE_NO.REPORT_ID_NOT_EXISTS

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CS0021PROFXLS call"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWRITE.TEXT = "帳票IDが存在しません。"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.REPORT_ID_NOT_EXISTS
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            'Workbook終了＆リリース
            WW_Workbook = Nothing
            Exit Sub
        End If

        '　定義未設定の補完処理
        If CS0021PROFXLS.POSISTART = 0 Then
            CS0021PROFXLS.POSISTART = 1
        End If

        If CS0021PROFXLS.POSI_T_X_MAX = 0 Then
            CS0021PROFXLS.POSI_T_X_MAX = 1
        End If

        If CS0021PROFXLS.POSI_T_Y_MAX = 0 Then
            CS0021PROFXLS.POSI_T_Y_MAX = 1
        End If

        If CS0021PROFXLS.POSI_I_X_MAX = 0 Then
            CS0021PROFXLS.POSI_I_X_MAX = 1
        End If

        If CS0021PROFXLS.POSI_I_Y_MAX = 0 Then
            CS0021PROFXLS.POSI_I_Y_MAX = 1
        End If

        '〇縦横表用の列見出し条件情報を取得
        '　縦横Excelの繰返し情報格納準備
        '　　　WW_Structtbl説明
        '　　　　　WW_Structtblレコードは、縦横Excelの指定繰返し列条件を示す(項目CNTは繰返し順番を示す)。
        '　　　　　繰返し条件は、複数指定可。WW_Structrowの項目に、条件値をもたせる。

        WW_Structtbl.Clear()
        WW_Structtbl.Columns.Add("CNT", GetType(Integer))                    '縦横Excelの列繰返数

        '縦横表なし（STRUCTテーブルなし） 2024/10/18
        '　繰返し情報格納準備
        'For i As Integer = 0 To CS0021PROFXLS.STRUCT.Count - 1
        '    If CS0021PROFXLS.TITLEKBN(i) = "I_DataKey" And CS0021PROFXLS.STRUCT(i) <> "" And CS0021PROFXLS.EFFECT(i) = "Y" Then

        '        Try
        '            WW_Structtbl.Columns.Add(CS0021PROFXLS.FIELD(i), GetType(String))
        '        Catch ex As Exception
        '            'Column重複無視
        '        End Try

        '        'TBL(STRUCT)取得
        '        CS0028STRUCT.CAMPCODE = CAMPCODE
        '        CS0028STRUCT.STRUCT = CS0021PROFXLS.STRUCT(i)
        '        CS0028STRUCT.CS0028STRUCT()
        '        If isNormal(CS0028STRUCT.ERR) Then

        '            Try
        '                If WW_Structtbl.Rows.Count = 0 Then
        '                    For j As Integer = 0 To CS0028STRUCT.CODE.Count - 1
        '                        WW_Structrow = WW_Structtbl.NewRow
        '                        WW_Structrow("CNT") = j
        '                        WW_Structrow(CS0021PROFXLS.FIELD(i)) = CS0028STRUCT.CODE(j)
        '                        WW_Structtbl.Rows.Add(WW_Structrow)
        '                    Next
        '                Else
        '                    For j As Integer = 0 To CS0028STRUCT.CODE.Count - 1
        '                        WW_Structtbl.Rows(j)(CS0021PROFXLS.FIELD(i)) = CS0028STRUCT.CODE(j)
        '                    Next
        '                End If

        '            Catch ex As Exception
        '                ERR = C_MESSAGE_NO.EXCEL_COLUMNS_FORMAT_ERROR

        '                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                'SUBクラス名
        '                CS0011LOGWRITE.INFPOSI = "CS0021PROFXLS"
        '                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
        '                CS0011LOGWRITE.TEXT = "Excel書式(列構造定義)不良"
        '                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
        '                CS0011LOGWRITE.CS0011LOGWrite()                         'ログ出力

        '                'Excel終了＆リリース
        '                WW_Structtbl.Clear()
        '                WW_Structtbl.Dispose()
        '                WW_Structtbl = Nothing
        '                WW_Structrow = Nothing
        '                'Workbook終了＆リリース
        '                WW_Workbook = Nothing

        '                Exit Sub
        '            End Try

        '        Else
        '            ERR = C_MESSAGE_NO.EXCEL_COLUMNS_FORMAT_ERROR

        '            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
        '            CS0011LOGWRITE.INFPOSI = "CS0021PROFXLS"
        '            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ERR
        '            CS0011LOGWRITE.TEXT = "Excel書式(列構造定義)不良"
        '            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
        '            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

        '            'Excel終了＆リリース
        '            WW_Structtbl.Clear()
        '            WW_Structtbl.Dispose()
        '            WW_Structtbl = Nothing
        '            WW_Structrow = Nothing
        '            'Workbook終了＆リリース
        '            WW_Workbook = Nothing

        '            Exit Sub
        '        End If
        '    End If
        'Next

        '〇Excel(明細)データ格納準備

        TBLDATA = New DataTable
        TBLDATA.Clear()

        '出力DATATABLEに列(項目)追加
        For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
            If CS0021PROFXLS.EFFECT(i) = "Y" Then
                Select Case CS0021PROFXLS.TITLEKBN(i)
                    Case "T"
                        If CS0021PROFXLS.POSIY(i) > 0 And CS0021PROFXLS.POSIX(i) > 0 Then
                            If CS0021PROFXLS.FIELD(i) <> "EXCELTITOL" And CS0021PROFXLS.FIELD(i) <> "REPORTID" Then
                                TBLDATA.Columns.Add(CS0021PROFXLS.FIELD(i), GetType(String))
                            End If
                        End If
                    Case "I"
                        If CS0021PROFXLS.POSIY(i) > 0 And CS0021PROFXLS.POSIX(i) > 0 Then
                            TBLDATA.Columns.Add(CS0021PROFXLS.FIELD(i), GetType(String))
                        End If
                    Case "I_Data"
                        If CS0021PROFXLS.POSIY(i) > 0 And CS0021PROFXLS.POSIX(i) > 0 Then
                            TBLDATA.Columns.Add(CS0021PROFXLS.FIELD(i), GetType(String))
                        End If
                    Case "I_DataKey"
                        TBLDATA.Columns.Add(CS0021PROFXLS.FIELD(i), GetType(String))
                End Select

            End If
        Next

        '■メイン処理

        Try
            '〇指定シートのExcelデータ取得　＆　データ格納
            If I_SHEETFLG = CONST_SHEET_ALL Then
                '全シート対象の場合

                'シート決定　&　データ取得
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1

                    WW_ActiveSheetIndex = i
                    GetXLSDATA(WW_Workbook, WW_Structtbl, WW_ActiveSheetIndex)

                    If Not ERR = C_MESSAGE_NO.NORMAL Then

                        'Excel終了＆リリース
                        WW_Structtbl.Clear()
                        WW_Structtbl.Dispose()
                        WW_Structtbl = Nothing
                        WW_Structrow = Nothing
                        'Workbook終了＆リリース
                        WW_Workbook = Nothing

                        Exit Sub
                    End If

                Next

            Else
                '1シートのみ対象の場合

                'シート決定                                  優先：INPUTSHEET指定＞入力シート("入力" or "入出力")＞先頭シート
                WW_ActiveSheetIndex = 0
                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If INPUTSHEET = "" Then
                        If WW_Workbook.Worksheets(i).Name = "入力" Or WW_Workbook.Worksheets(i).Name = "入出力" Then
                            WW_ActiveSheetIndex = i
                            Exit For
                        End If
                    Else
                        If WW_Workbook.Worksheets(i).Name = INPUTSHEET Then
                            WW_ActiveSheetIndex = i
                            Exit For
                        End If
                    End If
                Next

                'データ取得
                GetXLSDATA(WW_Workbook, WW_Structtbl, WW_ActiveSheetIndex)

                If Not ERR = C_MESSAGE_NO.NORMAL Then

                    'Excel終了＆リリース
                    WW_Structtbl.Clear()
                    WW_Structtbl.Dispose()
                    WW_Structtbl = Nothing
                    WW_Structrow = Nothing
                    'Workbook終了＆リリース
                    WW_Workbook = Nothing

                    Exit Sub
                End If

            End If

        Catch ex As Exception

            'EXCEL OPENエラー
            ERR = C_MESSAGE_NO.EXCEL_OPEN_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Open"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = ERR
            CS0011LOGWRITE.CS0011LOGWrite()                     'ログ出力

            'Excel終了＆リリース
            WW_Structtbl.Clear()
            WW_Structtbl.Dispose()
            WW_Structtbl = Nothing
            WW_Structrow = Nothing
            'Workbook終了＆リリース
            WW_Workbook = Nothing

            Exit Sub

        End Try

    End Sub

    ''' <summary>
    ''' XLSデータ取得
    ''' </summary>
    ''' <param name="I_Workbook">ExcelインスタンスID</param>
    ''' <param name="I_CS30tbl">構造テーブル</param>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Sub GetXLSDATA(ByRef I_Workbook As Workbook, ByRef I_CS30tbl As DataTable, ByRef I_ActiveSheetIndex As Integer)

        'ワークDataRow
        Dim WW_Row_template As DataRow
        Dim WW_Row As DataRow = TBLDATA.NewRow()

        Dim WW_RecCNT As Integer = 0
        Dim WW_CellsDataCNT As Integer = 0
        Dim WW_X As Integer = 0
        Dim WW_Y As Integer = 0

        'カラムカウント補正
        'If I_Workbook.Worksheets(I_ActiveSheetIndex).Columns.Count < CS0021PROFXLS.POSI_I_X_MAX + I_CS30tbl.Rows.Count * CS0021PROFXLS.POSI_R_X_MAX Then
        '    I_Workbook.Worksheets(I_ActiveSheetIndex).Columns.Count = CS0021PROFXLS.POSI_I_X_MAX + I_CS30tbl.Rows.Count * CS0021PROFXLS.POSI_R_X_MAX
        'End If

        '■Excelデータ取得
        '******************************************************************
        '*  タイトル(T)処理                                               *
        '******************************************************************

        Try
            'ヘッダ情報をWW_Row_templateへセット
            WW_Row_template = TBLDATA.NewRow()

            'タイトルデータ取得
            For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1

                If CS0021PROFXLS.TITLEKBN(i) = "T" And CS0021PROFXLS.EFFECT(i) = "Y" And CS0021PROFXLS.POSIY(i) > 0 And CS0021PROFXLS.POSIX(i) > 0 And
                   CS0021PROFXLS.FIELD(i) <> "EXCELTITOL" And CS0021PROFXLS.FIELD(i) <> "REPORTID" Then
                    '縦位置：明細縦位置
                    WW_Y = CS0021PROFXLS.POSIY(i) - 1
                    '横位置：明細横位置
                    WW_X = CS0021PROFXLS.POSIX(i) - 1
                    If Not I_Workbook.Worksheets(I_ActiveSheetIndex).Cells(WW_Y, WW_X).Value = Nothing Then
                        WW_Row_template(CS0021PROFXLS.FIELD(i)) = I_Workbook.Worksheets(I_ActiveSheetIndex).Cells(WW_Y, WW_X).Value.ToString
                    End If
                End If

            Next

        Catch ex As Exception
            '他Excel処理完了待ち
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_TITOL_Exception"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            Exit Sub

        End Try

        '******************************************************************
        '*  縦表Excel明細(I)処理                                          *
        '******************************************************************

        If I_CS30tbl.Rows.Count = 0 Then
            Do
                Try

                    WW_CellsDataCNT = 0

                    'Do終了判定
                    If I_Workbook.Worksheets(I_ActiveSheetIndex).Rows.Count < CS0021PROFXLS.POSISTART + (WW_RecCNT * CS0021PROFXLS.POSI_I_Y_MAX) + CS0021PROFXLS.POSI_I_Y_MAX Then
                        Exit Do
                    End If

                    'ヘッダ情報(WW_Row_template)をRowへセット
                    WW_Row = TBLDATA.NewRow()
                    WW_Row.ItemArray = WW_Row_template.ItemArray

                    '明細データ取得
                    For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1

                        If CS0021PROFXLS.TITLEKBN(i) = "I" And CS0021PROFXLS.EFFECT(i) = "Y" And CS0021PROFXLS.POSIY(i) > 0 And CS0021PROFXLS.POSIX(i) > 0 Then
                            '縦位置：書出し位置　＋　列見出し高さ　＋　処理対象レコード数　×　明細縦MAX行　＋　明細縦位置　　　※複数段明細を勘案する事。PROFXLSは1〜nで設定。
                            WW_Y = (CS0021PROFXLS.POSISTART - 1) + CS0021PROFXLS.POSI_I_Y_MAX + WW_RecCNT * CS0021PROFXLS.POSI_I_Y_MAX + (CS0021PROFXLS.POSIY(i) - 1)
                            '横位置：明細位置
                            WW_X = (CS0021PROFXLS.POSIX(i) - 1)

                            '2022/10/24 現在、はこぶわの場合でカード番号（車番）が空白の場合その行を無視する
                            If CS0021PROFXLS.FIELD(i) = KEYITEMNAME Then
                                If I_Workbook.Worksheets(I_ActiveSheetIndex).Cells(WW_Y, WW_X).Value = Nothing Then
                                    '存在したらループ終了
                                    WW_CellsDataCNT = 0
                                    i = CS0021PROFXLS.TITLEKBN.Count
                                    Continue For
                                End If
                            End If

                            If Not I_Workbook.Worksheets(I_ActiveSheetIndex).Cells(WW_Y, WW_X).Value = Nothing Then
                                WW_CellsDataCNT = WW_CellsDataCNT + 1
                                WW_Row(CS0021PROFXLS.FIELD(i)) = I_Workbook.Worksheets(I_ActiveSheetIndex).Cells(WW_Y, WW_X).Value.ToString
                            End If
                        End If

                    Next

                    If WW_CellsDataCNT = 0 Then
                        Exit Do
                    Else
                        TBLDATA.Rows.Add(WW_Row)
                        WW_RecCNT = WW_RecCNT + 1
                    End If

                Catch ex As Exception
                    '他Excel処理完了待ち
                    ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "Excel_Detail_Exception"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
                    CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

                    Exit Sub
                End Try

            Loop Until WW_CellsDataCNT = 0

        End If

        '******************************************************************
        '*  縦横表明細(I,I_Data,I_DataKey)処理                            *
        '******************************************************************
        If I_CS30tbl.Rows.Count <> 0 Then

            Do
                Try

                    WW_CellsDataCNT = 0

                    'Do終了判定
                    If I_Workbook.Worksheets(I_ActiveSheetIndex).Rows.Count < CS0021PROFXLS.POSISTART + (WW_RecCNT - 1) * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX) + CS0021PROFXLS.POSI_R_Y_MAX Then
                        Exit Do
                    End If

                    'ヘッダ情報(WW_Row_template)をRowへセット
                    WW_Row = TBLDATA.NewRow()
                    WW_Row.ItemArray = WW_Row_template.ItemArray

                    '縦横表明細(I)データ取得
                    For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1

                        If CS0021PROFXLS.TITLEKBN(i) = "I" And CS0021PROFXLS.EFFECT(i) = "Y" And CS0021PROFXLS.POSIY(i) > 0 And CS0021PROFXLS.POSIX(i) > 0 Then
                            '縦位置：書出し位置　＋　処理対象レコード数　×　MAX(明細縦MAX行,繰返し縦MAX行)　＋　明細縦位置
                            WW_Y = (CS0021PROFXLS.POSISTART - 1) + WW_RecCNT * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX) + (CS0021PROFXLS.POSIY(i) - 1)
                            '横位置：明細位置
                            WW_X = (CS0021PROFXLS.POSIX(i) - 1)
                            If Not I_Workbook.Worksheets(I_ActiveSheetIndex).Cells(WW_Y, WW_X).Value = Nothing Then
                                WW_CellsDataCNT = WW_CellsDataCNT + 1
                                WW_Row(CS0021PROFXLS.FIELD(i)) = I_Workbook.Worksheets(I_ActiveSheetIndex).Cells(WW_Y, WW_X).Value.ToString
                            End If
                        End If

                    Next

                    '○縦横表明細(I_Data,I_DataKey)データ取得
                    For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
                        If CS0021PROFXLS.TITLEKBN(i) = "I_Data" And CS0021PROFXLS.EFFECT(i) = "Y" And CS0021PROFXLS.POSIY(i) > 0 And CS0021PROFXLS.POSIX(i) > 0 Then

                            For j As Integer = 0 To I_CS30tbl.Rows.Count - 1

                                '縦位置：書出し位置　＋　処理対象レコード数　×　明細縦MAX行　＋　明細縦位置
                                WW_Y = (CS0021PROFXLS.POSISTART - 1) + WW_RecCNT * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX) + (CS0021PROFXLS.POSIY(i) - 1)
                                '横位置：明細(I)Max列　＋　処理列数　×　明細横MAX行(R)　＋　明細位置
                                WW_X = CS0021PROFXLS.POSI_I_X_MAX + j * CS0021PROFXLS.POSI_R_X_MAX + (CS0021PROFXLS.POSIX(i) - 1)

                                If Not I_Workbook.Worksheets(I_ActiveSheetIndex).Cells(WW_Y, WW_X).Value = Nothing Then

                                    '○縦横表明細(I_Data)データ設定
                                    WW_CellsDataCNT = WW_CellsDataCNT + 1
                                    WW_Row(CS0021PROFXLS.FIELD(i)) = I_Workbook.Worksheets(I_ActiveSheetIndex).Cells(WW_Y, WW_X).Value.ToString

                                    '○縦横表明細(I_DataKey)データ設定
                                    For k As Integer = 0 To I_CS30tbl.Columns.Count - 1
                                        If I_CS30tbl.Columns(k).ColumnName <> "CNT" Then
                                            WW_Row(I_CS30tbl.Columns(k).ColumnName) = I_CS30tbl.Rows(j)(I_CS30tbl.Columns(k).ColumnName)
                                        End If
                                    Next

                                End If

                            Next
                        End If
                    Next

                    If WW_CellsDataCNT = 0 Then
                        Exit Do
                    Else
                        TBLDATA.Rows.Add(WW_Row)
                        WW_RecCNT = WW_RecCNT + 1
                    End If

                Catch ex As Exception
                    '他Excel処理完了待ち
                    ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

                    CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
                    CS0011LOGWRITE.INFPOSI = "Excel_Detail_Exception"
                    CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWRITE.TEXT = ex.ToString()
                    CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
                    CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

                    Exit Sub
                End Try

            Loop Until WW_CellsDataCNT = 0

        End If

        ERR = C_MESSAGE_NO.NORMAL


    End Sub

End Structure
