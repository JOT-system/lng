Imports System.Web
Imports System.IO
Imports GrapeCity.Documents.Excel
Imports System.Drawing

''' <summary>
''' 帳票出力
''' </summary>
''' <remarks></remarks>
Public Structure CS0030REPORT

    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String

    ''' <summary>
    ''' プロファイルID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PROFID() As String

    ''' <summary>
    ''' 画面ID
    ''' </summary>
    ''' <value>画面ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAPID() As String

    ''' <summary>
    ''' 帳票ID
    ''' </summary>
    ''' <value>帳票ID</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property REPORTID() As String

    ''' <summary>
    ''' 出力ファイル形式
    ''' </summary>
    ''' <value>出力ファイル形式</value>
    ''' <returns></returns>
    ''' <remarks>pdf, csv, xlsx, xlsm</remarks>
    Public Property FILEtyp() As String

    ''' <summary>
    ''' データ参照tabledata
    ''' </summary>
    ''' <value>データ参照tabledata</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TBLDATA() As DataTable

    ''' <summary>
    ''' 出力Dir＋ファイル名
    ''' </summary>
    ''' <value></value>
    ''' <returns>出力Dir＋ファイル名</returns>
    ''' <remarks></remarks>
    Public Property FILEpath() As String

    ''' <summary>
    ''' 出力URL＋ファイル名
    ''' </summary>
    ''' <value></value>
    ''' <returns>出力URL＋ファイル名</returns>
    ''' <remarks></remarks>
    Public Property URL() As String

    ''' <summary>
    ''' 対象日付
    ''' </summary>
    ''' <value>対象日付</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TARGETDATE() As String

    ''' <summary>
    ''' チェックテーブル（6/9現在、LNT0001Dのためのみ）
    ''' </summary>
    ''' <value>チェックテーブル</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CHKTBL() As DataTable

    ''' <summary>
    ''' 出力ファイル名
    ''' </summary>
    ''' <value>出力ファイル名</value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property FILENAME() As string

    ''' <summary>
    ''' エラーコード
    ''' </summary>
    ''' <value></value>
    ''' <returns>0;正常、それ以外：エラー</returns>
    ''' <remarks>OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)</remarks>
    Public Property ERR() As String

    Private Const METHOD_NAME = "CS0030REPORT"

    Public Sub CS0030REPORT()

        '■共通宣言
        Dim CS0011LOGWRITE As New CS0011LOGWrite                'LogOutput DirString Get
        Dim CS0021PROFXLS As New CS0021PROFXLS                  'プロファイル（XLS）取得
        '縦横表なし（STRUCTテーブルなし） 2024/10/18
        'Dim CS0028STRUCT As New CS0028STRUCT                    '構造取得
        Dim CS0050SESSION As New CS0050SESSION                  'セッション情報操作処理
        Dim intSheetNo As Integer = 0                           'シート番号（初期値）

        Dim PROFCODE As String = String.Empty

        '縦横表Struct定義情報
        Dim WW_Structtbl As New DataTable
        Dim WW_Structrow As DataRow

        'DioDocs
        Dim WW_Workbook As New Workbook
        Dim WW_ColumnCount As Integer = 0
        Dim WW_RowCount As Integer = 0

        '●In PARAMチェック
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

        'REPORTID
        If IsNothing(REPORTID) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "REPORTID"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If
        'REPORTID のコード
        Dim GS0032 As New GS0032FIXVALUElst
        GS0032.CAMPCODE = CAMPCODE
        GS0032.CLAS = "CO0004_RPRTPROFID"
        GS0032.STDATE = Date.Now
        GS0032.ENDDATE = Date.Now
        GS0032.GS0032FIXVALUElst()
        If Not isNormal(GS0032.ERR) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "REPORTID-CODE NOT EXIST"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If
        PROFCODE = If(IsNothing(GS0032.VALUE1.Items.FindByText(PROFID)), C_DEFAULT_DATAKEY, GS0032.VALUE1.Items.FindByText(PROFID).Value)
        If String.IsNullOrEmpty(PROFCODE) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "REPORTID-CODE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
        End If
        'FILEtyp
        If IsNothing(FILEtyp) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "FILEtyp"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If

        'TBLDATA
        If IsNothing(TBLDATA) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "TBLDATA"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If

        'ユーザーID
        If String.IsNullOrEmpty(CS0050SESSION.USERID) Then
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "APSRVname ERR"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = "APSRVname ERR"
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
            Exit Sub
        End If


        '■初期処理
        '〇対象日付
        If IsNothing(TARGETDATE) OrElse TARGETDATE = "" Then
            TARGETDATE = Date.Now.ToString("yyyy/MM/dd")
        End If

        '〇出力レイアウト取得
        CS0021PROFXLS.CAMPCODE = CAMPCODE
        CS0021PROFXLS.PROFID = PROFID
        CS0021PROFXLS.MAPID = MAPID
        CS0021PROFXLS.REPORTID = REPORTID
        CS0021PROFXLS.TARGETDATE = TARGETDATE
        CS0021PROFXLS.CS0021PROFXLS()
        If Not isNormal(CS0021PROFXLS.ERR) Then
            ERR = C_MESSAGE_NO.DLL_IF_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                        'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "CS0021PROFXLS"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = C_MESSAGE_TEXT.IN_PARAM_ERROR_TEXT
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DLL_IF_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                                 'ログ出力
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

        '〇明細データソート

        'ソート用Cell追加
        TBLDATA.Columns.Add("ROWKEY", GetType(String))                      '行SORT・マッチング用Key

        'データソート準備
        For i As Integer = 0 To TBLDATA.Rows.Count - 1

            '行SORT Key編集(ソート対象項目内容をコンカチしソートKEYを設定)
            Dim WW_RowKey As String = ""
            For j As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
                If CS0021PROFXLS.TITLEKBN(j) = "I" AndAlso CS0021PROFXLS.EFFECT(j) = "Y" AndAlso CS0021PROFXLS.POSIY(j) > 0 AndAlso CS0021PROFXLS.POSIX(j) > 0 AndAlso CS0021PROFXLS.SORT(j) <> 0 Then
                    If WW_RowKey = "" Then
                        WW_RowKey = TBLDATA.Rows(i).Item(CS0021PROFXLS.FIELD(j))
                    Else
                        WW_RowKey = WW_RowKey & "_" & TBLDATA.Rows(i).Item(CS0021PROFXLS.FIELD(j))
                    End If
                End If
            Next

            TBLDATA.Rows(i).Item("ROWKEY") = WW_RowKey

        Next

        'ソート
        Dim WW_TBLDATA_View As DataView = New DataView(TBLDATA)
        Dim WW_TBLDATA_SORTstr As String = "ROWKEY"
        WW_TBLDATA_View.Sort = WW_TBLDATA_SORTstr
        TBLDATA = WW_TBLDATA_View.ToTable

        WW_TBLDATA_View.Dispose()
        WW_TBLDATA_View = Nothing


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
        '    'If CS0021PROFXLS.TITLEKBN(i) = "I_DataKey" And CS0021PROFXLS.STRUCT(i) <> "" And CS0021PROFXLS.EFFECT(i) = "Y" Then
        '    If CS0021PROFXLS.TITLEKBN(i) = "I_DataKey" And CS0021PROFXLS.STRUCT(i) <> "" Then

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

        '                WW_Structtbl.Clear()
        '                WW_Structtbl.Dispose()
        '                WW_Structtbl = Nothing
        '                WW_Structrow = Nothing

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

        '            WW_Structtbl.Clear()
        '            WW_Structtbl.Dispose()
        '            WW_Structtbl = Nothing
        '            WW_Structrow = Nothing

        '            Exit Sub
        '        End If
        '    End If
        'Next

        '〇縦横Excelの横列決定

        '繰返位置番号用Cell追加
        TBLDATA.Columns.Add("CELLNO", GetType(Integer))

        Dim WW_TBLDATArow As Object
        '繰返し位置決定
        If WW_Structtbl.Rows.Count <> 0 Then
            Dim WW_Column As Integer = Nothing
            For i As Integer = 0 To TBLDATA.Rows.Count - 1

                WW_TBLDATArow = TBLDATA.Rows(i)

                For j As Integer = 0 To WW_Structtbl.Rows.Count - 1
                    '複数列ループ

                    WW_Structrow = WW_Structtbl.Rows(j)

                    Dim WW_HIT_CNT As Integer = 0
                    For k As Integer = 0 To WW_Structtbl.Columns.Count - 1
                        '複数条件ループ

                        If WW_Structtbl.Columns(k).ColumnName = "CNT" Then
                            Continue For
                        Else
                            If WW_TBLDATArow(WW_Structtbl.Columns(k).ColumnName).ToString = WW_Structrow(k).ToString Then
                                '条件一致
                                WW_HIT_CNT = WW_HIT_CNT + 1
                            End If
                        End If

                    Next

                    If WW_HIT_CNT = (WW_Structtbl.Columns.Count - 1) Then
                        '全件HIT(CNT除く)の場合
                        WW_TBLDATArow("CELLNO") = j + 1
                        Exit For
                    Else
                        '初期値
                        WW_TBLDATArow("CELLNO") = WW_Structtbl.Rows.Count + 1
                        'WW_TBLDATArow("CELLNO") = WW_Structtbl.Rows.Count
                    End If

                Next

            Next
        End If

        '■メイン処理

        '〇既存ExcelのOpen　＆　アクティブSheet準備
        Dim WW_ExcelExist As String = ""

        Try
            If CS0021PROFXLS.EXCELFILE = "" OrElse Not File.Exists(CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & PROFCODE & "\" & MAPID & "\" & CS0021PROFXLS.EXCELFILE) Then
                'PRINTFORMAT(部署定義Excelファイル)が存在しない場合

                If File.Exists(CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & C_DEFAULT_DATAKEY & "\" & MAPID & "\" & CS0021PROFXLS.EXCELFILE) Then
                    'PRINTFORMAT(Default定義Excelファイル)が存在する場合

                    '既存ExcelのOpen
                    'オプション：開いた後に再計算しません
                    Dim WW_Options As XlsxOpenOptions = New XlsxOpenOptions
                    WW_Options.DoNotRecalculateAfterOpened = True
                    WW_Workbook.Open(CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & C_DEFAULT_DATAKEY & "\" & MAPID & "\" & CS0021PROFXLS.EXCELFILE, WW_Options)

                    '出力シート決定
                    intSheetNo = 0
                    For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                        If WW_Workbook.Worksheets(i).Name = "出力" Or WW_Workbook.Worksheets(i).Name = "入出力" Then
                            intSheetNo = i
                            Exit For
                        End If
                    Next
                    WW_ExcelExist = "ON"
                Else

                    '既存ExcelのOpen
                    'WW_Workbook.Open(CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & C_DEFAULT_DATAKEY & "\COMMON\書式無.xlsx")
                    WW_ExcelExist = ""
                End If
            Else
                'PRINTFORMAT(部署定義Excelファイル)が存在する場合

                '既存ExcelのOpen
                'オプション：開いた後に再計算しません
                Dim WW_Options As XlsxOpenOptions = New XlsxOpenOptions
                WW_Options.DoNotRecalculateAfterOpened = True
                WW_Workbook.Open(CS0050SESSION.UPLOAD_PATH & "\PRINTFORMAT\" & PROFCODE & "\" & MAPID & "\" & CS0021PROFXLS.EXCELFILE, WW_Options)

                '出力シート決定
                intSheetNo = 0

                For i As Integer = 0 To WW_Workbook.Worksheets.Count - 1
                    If WW_Workbook.Worksheets(i).Name = "出力" Or WW_Workbook.Worksheets(i).Name = "入出力" Then
                        intSheetNo = i
                        Exit For
                    End If
                Next
                WW_ExcelExist = "ON"
            End If

            '計算エンジンの有効化します
            WW_Workbook.EnableCalculation = False

        Catch ex As Exception
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Open"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            'Excel終了＆リリース
            WW_Structtbl.Clear()
            WW_Structtbl.Dispose()
            WW_Structtbl = Nothing
            WW_Structrow = Nothing
            WW_Workbook = Nothing
            Exit Sub
        End Try

        '〇Sheetサイズ決定             

        If WW_Structtbl.Rows.Count = 0 Then
            '縦表(ベタ帳票)の場合

            'Sheet横サイズ
            If WW_Workbook.Worksheets(intSheetNo).ColumnCount = 0 Then
                'Sheet横サイズ：明細横MAX行
                WW_ColumnCount = CS0021PROFXLS.POSI_I_X_MAX
            Else
                'Sheet横サイズ：元のEXCELを優先
                If WW_Workbook.Worksheets(intSheetNo).ColumnCount > CS0021PROFXLS.POSI_I_X_MAX - 1 Then
                    WW_ColumnCount = WW_Workbook.Worksheets(intSheetNo).ColumnCount
                Else
                    WW_ColumnCount = CS0021PROFXLS.POSI_I_X_MAX
                End If
            End If

            'Sheet縦サイズ：書出し位置　＋　列見出し高さ　＋　処理対象レコード数　×　明細縦MAX行
            WW_RowCount = (CS0021PROFXLS.POSISTART - 1) + CS0021PROFXLS.POSI_I_Y_MAX + TBLDATA.Rows.Count * CS0021PROFXLS.POSI_I_Y_MAX

        Else
            '縦横表の場合

            'Sheet横サイズ
            If WW_Workbook.Worksheets(intSheetNo).ColumnCount = 0 Then
                'Sheet横サイズ：明細横MAX行　＋　繰返し列数(CNT除く)　×　繰返しMAX列数　
                WW_ColumnCount = CS0021PROFXLS.POSI_I_X_MAX + (WW_Structtbl.Rows.Count - 1) * CS0021PROFXLS.POSI_R_X_MAX
            Else
                'Sheet横サイズ：元のEXCELを優先
                If (WW_Workbook.Worksheets(intSheetNo).ColumnCount) > (CS0021PROFXLS.POSI_I_X_MAX + (WW_Structtbl.Rows.Count - 1) * CS0021PROFXLS.POSI_R_X_MAX) Then
                    WW_ColumnCount = WW_Workbook.Worksheets(intSheetNo).ColumnCount
                Else
                    'Sheet横サイズ：明細横MAX行　＋　繰返し列数(CNT除く)　×　繰返しMAX列数　
                    WW_ColumnCount = CS0021PROFXLS.POSI_I_X_MAX + (WW_Structtbl.Rows.Count - 1) * CS0021PROFXLS.POSI_R_X_MAX
                End If
            End If

            'Sheet縦サイズ：書出し位置　＋　処理対象レコード数　×　Max(明細縦MAX高さ,明細列MAX高さ)
            WW_RowCount = (CS0021PROFXLS.POSISTART - 1) + TBLDATA.Rows.Count * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX)

        End If

        WW_Workbook.Worksheets(intSheetNo).ColumnCount = WW_ColumnCount
        WW_Workbook.Worksheets(intSheetNo).RowCount = WW_RowCount

        ' ******************************************************
        ' *    タイトル(T)                                     *
        ' ******************************************************

        Try
            Dim WW_X As Integer = 0
            Dim WW_Y As Integer = 0
            For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
                If CS0021PROFXLS.TITLEKBN(i) = "T" And CS0021PROFXLS.EFFECT(i) = "Y" And CS0021PROFXLS.POSIX(i) > 0 And CS0021PROFXLS.POSIY(i) > 0 Then
                    '縦位置：明細縦位置
                    WW_Y = CS0021PROFXLS.POSIY(i) - 1
                    '横位置：明細横位置
                    WW_X = CS0021PROFXLS.POSIX(i) - 1
                    Select Case CS0021PROFXLS.FIELD(i)
                        Case "EXCELTITOL"                'CS0021UPROFXLSパラメータ(FIELDNAME)をセット
                            WW_Workbook.Worksheets(intSheetNo).Cells(WW_Y, WW_X).Value = CS0021PROFXLS.FIELDNAME(i)
                        Case "REPORTID"                  'CS0021UPROFXLSパラメータ(REPORTID)をセット
                            WW_Workbook.Worksheets(intSheetNo).Cells(WW_Y, WW_X).Value = "ID:" & CS0021PROFXLS.REPORTID & ";" & CS0021PROFXLS.PROFID
                        Case Else                        'Tableの1行目の該当項目値をセット
                            If TBLDATA.Rows.Count <> 0 Then
                                '項目名が無い場合、無視
                                If columnCheck(TBLDATA, CS0021PROFXLS.FIELD(i)) Then
                                    WW_Workbook.Worksheets(intSheetNo).Cells(WW_Y, WW_X).Value = TBLDATA.Rows(0).Item(CS0021PROFXLS.FIELD(i))
                                End If
                            End If
                    End Select
                End If
            Next

        Catch ex As Exception
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_TITLE"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            'Excel終了＆リリース
            WW_Structtbl.Clear()
            WW_Structtbl.Dispose()
            WW_Structtbl = Nothing
            WW_Structrow = Nothing
            WW_Workbook = Nothing
            Exit Sub
        End Try

        ' ******************************************************
        ' *    列見出し(I)                                     *
        ' ******************************************************

        Try
            Dim WW_X As Integer = 0
            Dim WW_Y As Integer = 0
            If WW_Structtbl.Rows.Count = 0 Then
                '縦表(ベタ帳票)の場合

                For i As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
                    If CS0021PROFXLS.TITLEKBN(i) = "I" And CS0021PROFXLS.EFFECT(i) = "Y" And CS0021PROFXLS.POSIX(i) > 0 And CS0021PROFXLS.POSIY(i) > 0 Then
                        '縦位置：書出し位置　＋　見出し縦位置
                        WW_Y = (CS0021PROFXLS.POSISTART - 1) + (CS0021PROFXLS.POSIY(i) - 1)
                        '横位置：見出し横位置
                        WW_X = (CS0021PROFXLS.POSIX(i) - 1)
                        WW_Workbook.Worksheets(intSheetNo).Cells(WW_Y, WW_X).Value = CS0021PROFXLS.FIELDNAME(i).ToString
                    End If
                Next

            Else
                '縦表(ベタ帳票)の場合、列見出しは出力しない

            End If

        Catch ex As Exception
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_DetailHeader"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            'Excel終了＆リリース
            WW_Structtbl.Clear()
            WW_Structtbl.Dispose()
            WW_Structtbl = Nothing
            WW_Structrow = Nothing
            WW_Workbook = Nothing
            Exit Sub
        End Try

        ' ******************************************************　STRUCT(REPORT)の設定コードのみタイトルとしてセット
        ' *   縦横表の列見出し                                 *　(I_Dataの定義値は使用しない)
        ' ******************************************************

        If WW_Structtbl.Rows.Count = 0 Then
            '　見出し出力しない

        End If

        ' ******************************************************
        ' *    明細(I)                                         *
        ' ******************************************************
        If WW_Structtbl.Rows.Count = 0 Then

            Try
                Dim WW_X As Integer = 0
                Dim WW_Y As Integer = 0
                Dim WW_listY As List(Of Integer) = New List(Of Integer)

                For i As Integer = 0 To TBLDATA.Rows.Count - 1

                    '〇明細範囲へ値設定
                    For j As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
                        If CS0021PROFXLS.TITLEKBN(j) = "I" And CS0021PROFXLS.EFFECT(j) = "Y" And CS0021PROFXLS.POSIX(j) > 0 And CS0021PROFXLS.POSIY(j) > 0 Then
                            '縦位置：書出し位置　＋　列見出し高さ　＋　処理対象レコード数　×　明細縦MAX行　＋　明細縦位置　　　※複数段明細を勘案する事。PROFXLSは1~nで設定。
                            WW_Y = (CS0021PROFXLS.POSISTART - 1) + CS0021PROFXLS.POSI_I_Y_MAX + i * CS0021PROFXLS.POSI_I_Y_MAX + (CS0021PROFXLS.POSIY(j) - 1)
                            '横位置：明細位置
                            WW_X = (CS0021PROFXLS.POSIX(j) - 1)

                            '初めての行の場合、その行をクリアする（その行の対象列全てクリア）
                            If Not WW_listY.Contains(WW_Y) Then
                                WW_listY.Add(WW_Y)
                                WW_Workbook.Worksheets(intSheetNo).Range(WW_Y, 0, 1, CS0021PROFXLS.POSI_I_X_MAX - 1).Value = ""
                            End If

                            '項目名が無い場合、無視
                            If columnCheck(TBLDATA, CS0021PROFXLS.FIELD(j)) Then
                                WW_Workbook.Worksheets(intSheetNo).Cells(WW_Y, WW_X).Value = TBLDATA.Rows(i)(CS0021PROFXLS.FIELD(j)).ToString

                                '---------------------------------------------
                                '実績取込（実績不良データ出力）のみ
                                '---------------------------------------------
                                If MAPID = "LNT0001D" AndAlso String.IsNullOrEmpty(TBLDATA.Rows(i)(CS0021PROFXLS.FIELD(j)).ToString) Then
                                    '荷主別にエラー項目が異なるための処理
                                    For Each ChkRow As DataRow In CHKTBL.Select("KEYCODE ='" & TBLDATA.Rows(i)("TORICODE") & "'")
                                        For idx As Integer = 1 To 20
                                            'VALUNE1～20に値が存在する分、処理する
                                            Dim ValueStr As String = "VALUE" & idx
                                            If String.IsNullOrEmpty(ChkRow(ValueStr)) Then
                                                Exit For
                                            End If
                                            If CS0021PROFXLS.FIELDNAME(j) = ChkRow(ValueStr) Then
                                                If String.IsNullOrEmpty(TBLDATA.Rows(i)(CS0021PROFXLS.FIELD(j))) Then
                                                    'チェック項目がNGの場合、実績不良テーブルへの黄色い網掛指示
                                                    WW_Workbook.Worksheets(intSheetNo).Cells(WW_Y, WW_X).Interior.Color = Color.Yellow
                                                End If
                                            End If
                                        Next
                                    Next
                                End If
                            End If
                        End If
                    Next

                Next
            Catch ex As Exception
                ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "Excel_Detail_Range"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
                CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

                'Excel終了＆リリース
                WW_Structtbl.Clear()
                WW_Structtbl.Dispose()
                WW_Structtbl = Nothing
                WW_Structrow = Nothing
                WW_Workbook = Nothing
                Exit Sub
            End Try
        End If

        ' ******************************************************
        ' *    縦横表の明細(I、I_Data、I_DataKey)              *
        ' ******************************************************
        If WW_Structtbl.Rows.Count <> 0 Then
            Try
                TBLDATA.Columns.Add("ROWCNT", GetType(Integer))            '(I_DataKey+列番号)内順番

                'ソート
                WW_TBLDATA_View = New DataView(TBLDATA)
                WW_TBLDATA_SORTstr = "ROWKEY , CELLNO"
                WW_TBLDATA_View.Sort = WW_TBLDATA_SORTstr
                TBLDATA = WW_TBLDATA_View.ToTable

                WW_TBLDATA_View.Dispose()
                WW_TBLDATA_View = Nothing

                'ROWCNT設定
                Dim WW_BreakKey As String = ""
                Dim WW_ROWCNT As Integer = 0
                For i As Integer = 0 To TBLDATA.Rows.Count - 1
                    If TBLDATA.Rows(i).Item("ROWKEY").ToString & TBLDATA.Rows(i).Item("CELLNO").ToString = WW_BreakKey Then
                        WW_ROWCNT = WW_ROWCNT + 1
                    Else
                        WW_BreakKey = TBLDATA.Rows(i).Item("ROWKEY").ToString & TBLDATA.Rows(i).Item("CELLNO").ToString
                        WW_ROWCNT = 0
                    End If

                    TBLDATA.Rows(i).Item("ROWCNT") = WW_ROWCNT
                Next

                Dim WW_View As DataView = New DataView(TBLDATA)
                Dim WW_SORTstr As String = "ROWKEY , ROWCNT"
                WW_View.Sort = WW_SORTstr
                TBLDATA = WW_View.ToTable

                WW_View.Dispose()
                WW_View = Nothing

                Dim WW_LineCNT As Integer = 0
                Dim WW_LineKEY As String = ""

                Dim WW_X As Integer = 0
                Dim WW_Y As Integer = 0
                For i As Integer = 0 To TBLDATA.Rows.Count - 1

                    If WW_LineKEY = "" Then
                        WW_LineKEY = TBLDATA.Rows(i)("ROWKEY").ToString & "_" & TBLDATA.Rows(i)("ROWCNT").ToString
                    End If

                    If WW_LineKEY <> (TBLDATA.Rows(i)("ROWKEY").ToString & "_" & TBLDATA.Rows(i)("ROWCNT").ToString) Then
                        WW_LineKEY = TBLDATA.Rows(i)("ROWKEY").ToString & "_" & TBLDATA.Rows(i)("ROWCNT").ToString
                        WW_LineCNT = WW_LineCNT + 1
                    End If

                    '〇明細範囲へ値設定(I)
                    For j As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
                        If CS0021PROFXLS.TITLEKBN(j) = "I" And CS0021PROFXLS.EFFECT(j) = "Y" And CS0021PROFXLS.POSIX(j) > 0 And CS0021PROFXLS.POSIY(j) > 0 Then
                            '縦位置：書出し位置　＋　処理対象レコード数　×　MAX(明細縦MAX行,繰返し縦MAX行)　＋　明細縦位置
                            'WW_Y = (CS0021PROFXLS.POSISTART - 1) + i * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX) + (CS0021PROFXLS.POSIY(j) - 1)
                            WW_Y = (CS0021PROFXLS.POSISTART) + WW_LineCNT * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX) + (CS0021PROFXLS.POSIY(j) - 1)
                            '横位置：明細位置
                            WW_X = (CS0021PROFXLS.POSIX(j) - 1)
                            '項目名が無い場合、無視
                            If columnCheck(TBLDATA, CS0021PROFXLS.FIELD(j)) Then
                                WW_Workbook.Worksheets(intSheetNo).Cells(WW_Y, WW_X).Value = TBLDATA.Rows(i)(CS0021PROFXLS.FIELD(j)).ToString
                            End If
                        End If
                    Next

                    '〇明細範囲へ値設定

                    '繰返データセット
                    If TBLDATA.Rows(i)("CELLNO") > (WW_Structtbl.Rows.Count) Then
                        '列位置決め用KeyにHitしない場合、メッセージをセット
                        'WW_HENSYUrange(WW_LineCNT * CS0021UPROFXLS.POSI_I_Y_MAX, CS0021UPROFXLS.POSI_I_X_MAX + WW_CELL_KEY.Count * CS0021UPROFXLS.POSI_R_X_MAX) = "★表示出来ないデータ有(該当列無)"
                        'WW_HENSYUrange(WW_LineCNT * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX), CS0021PROFXLS.POSI_I_X_MAX + WW_CELL_KEY.Count * CS0021PROFXLS.POSI_R_X_MAX) = "★表示出来ないデータ有(該当列無)"

                        '縦位置：書出し位置　＋　処理対象レコード数　×　明細縦MAX行　＋　明細縦位置
                        WW_Y = (CS0021PROFXLS.POSISTART) + WW_LineCNT * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX)
                        '横位置：明細(I)Max列　＋　処理列数　×　明細横MAX行(R)　＋　明細位置
                        WW_X = CS0021PROFXLS.POSI_I_X_MAX + WW_Structtbl.Rows.Count * CS0021PROFXLS.POSI_R_X_MAX

                        WW_Workbook.Worksheets(intSheetNo).Cells(WW_Y, WW_X).Value = "★表示出来ないデータ有(該当列無)"
                        'WW_HENSYUrange(WW_LineCNT * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX), CS0021PROFXLS.POSI_I_X_MAX + WW_Structtbl.Rows.Count * CS0021PROFXLS.POSI_R_X_MAX) = "★表示出来ないデータ有(該当列無)"


                    Else

                        For j As Integer = 0 To CS0021PROFXLS.TITLEKBN.Count - 1
                            If CS0021PROFXLS.TITLEKBN(j) = "I_Data" And CS0021PROFXLS.EFFECT(j) = "Y" And CS0021PROFXLS.POSIX(j) > 0 And CS0021PROFXLS.POSIY(j) > 0 And TBLDATA.Rows(i)("CELLNO") > 0 Then
                                '縦位置：書出し位置　＋　処理対象レコード数　×　明細縦MAX行　＋　明細縦位置
                                'WW_Y = (CS0021PROFXLS.POSISTART - 1) + i * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX) + (CS0021PROFXLS.POSIY(j) - 1)
                                WW_Y = (CS0021PROFXLS.POSISTART) + WW_LineCNT * Math.Max(CS0021PROFXLS.POSI_I_Y_MAX, CS0021PROFXLS.POSI_R_Y_MAX) + (CS0021PROFXLS.POSIY(j) - 1)
                                '横位置：明細(I)Max列　＋　処理列数　×　明細横MAX行(R)　＋　明細位置
                                WW_X = CS0021PROFXLS.POSI_I_X_MAX + (TBLDATA.Rows(i)("CELLNO") - 1) * CS0021PROFXLS.POSI_R_X_MAX + (CS0021PROFXLS.POSIX(j) - 1)
                                If columnCheck(TBLDATA, CS0021PROFXLS.FIELD(j)) Then
                                    WW_Workbook.Worksheets(intSheetNo).Cells(WW_Y, WW_X).Value = TBLDATA.Rows(i)(CS0021PROFXLS.FIELD(j)).ToString
                                End If
                            End If
                        Next

                    End If

                Next
            Catch ex As Exception
                ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

                CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                            'SUBクラス名
                CS0011LOGWRITE.INFPOSI = "Excel_Detail_Range"
                CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWRITE.TEXT = ex.ToString()
                CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
                CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

                'Excel終了＆リリース
                WW_Structtbl.Clear()
                WW_Structtbl.Dispose()
                WW_Structtbl = Nothing
                WW_Structrow = Nothing
                WW_Workbook = Nothing
                Exit Sub
            End Try
        End If


        ' ******************************************************
        ' *    Excel書式設定                                   *
        ' ******************************************************
        Dim WW_Activesheet As IWorksheet = WW_Workbook.Worksheets(intSheetNo)
        Try
            If WW_ExcelExist = "ON" Then
                'Excel書式ありの場合、書式設定しない
            Else
                With WW_Activesheet.PageSetup
                    .Orientation = PageOrientation.Landscape
                    .TopMargin = 20
                    .BottomMargin = 20
                    .LeftMargin = 20
                    .RightMargin = 20
                    .IsPercentScale = False
                    .FitToPagesWide = 1     '横を1ページに収める
                    .FitToPagesTall = 1000  '横(縦)[1]×縦(横)[n]の場合、十分なページ数を設定して幅優先にします
                    .PrintTitleRows = "$1:$" & (CS0021PROFXLS.POSISTART + CS0021PROFXLS.POSI_I_Y_MAX - 1).ToString    'ページタイトル固定
                End With
            End If

        Catch ex As Exception
            ERR = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB

            CS0011LOGWRITE.INFSUBCLASS = 'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_OverLay"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.WAIT_OTHER_EXCEL_JOB
            CS0011LOGWRITE.CS0011LOGWrite()                                     'ログ出力

            'Excel終了＆リリース
            WW_Structtbl.Clear()
            WW_Structtbl.Dispose()
            WW_Structtbl = Nothing
            WW_Structrow = Nothing
            WW_Workbook = Nothing
            Exit Sub
        Finally
        End Try

        ' ******************************************************
        ' *    Excel保存                                       *
        ' ******************************************************

        '○EXCEL保存
        'Dim WW_Dir As String = ""

        Try
            Dim WW_Dir As String = ""
            '　印刷用フォルダ作成
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK"
            '　格納フォルダ存在確認＆作成(...\PRINTWORK)
            If Directory.Exists(WW_Dir) Then
            Else
                Directory.CreateDirectory(WW_Dir)
            End If

            '　格納フォルダ存在確認＆作成(...\PRINTWORK\ユーザーID)
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID
            If Directory.Exists(WW_Dir) Then
            Else
                Directory.CreateDirectory(WW_Dir)
            End If

            '　印刷用フォルダ内不要ファイル削除(当日以外のファイルは削除)
            WW_Dir = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID
            For Each FileName As String In Directory.GetFiles(WW_Dir, "*.*")
                ' ファイルパスからファイル名を取得
                Do
                    FileName = Mid(FileName, InStr(FileName, "\") + 1, 100)
                Loop Until InStr(FileName, "\") = 0

                If FileName = "" Then
                Else
                    If IsNumeric(Mid(FileName, 1, 8)) And Mid(FileName, 1, 8) = Date.Now.ToString("yyyyMMdd") Then
                    Else
                        For Each tempFile As String In Directory.GetFiles(WW_Dir)
                            File.Delete(tempFile)
                        Next
                        Exit For
                    End If
                End If
            Next
        Catch ex As Exception
            ERR = C_MESSAGE_NO.FILE_IO_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Folder"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            'Excel終了＆リリース
            WW_Structtbl.Clear()
            WW_Structtbl.Dispose()
            WW_Structtbl = Nothing
            WW_Structrow = Nothing
            WW_Workbook = Nothing
            Exit Sub
        End Try

        '計算エンジンの有効化します
        Try
            WW_Workbook.EnableCalculation = True
        Catch ex As Exception
        End Try

        '○ファイル(PDF,CSV)保存
        Dim WW_datetime As String = ""
        If String.IsNullOrEmpty(FILENAME) Then
            WW_datetime = DateTime.Now.ToString("yyyyMMddHHmmss") & DateTime.Now.Millisecond.ToString
        Else
            WW_datetime = FILENAME
        End If
        FILEtyp = FILEtyp.ToLower()
        Dim WW_SaveState As String = Nothing

        Try
            Select Case FILEtyp
                Case "pdf"
                    FILEpath = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID & "\" & WW_datetime & ".pdf"
                    URL = CS0050SESSION.HTTPS_GET & "://" & HttpContext.Current.Request.Url.Host & "/" & CS0050SESSION.PRINT_ROOT_URL_NAME & "/" & CS0050SESSION.USERID & "/" & WW_datetime & ".pdf"
                    'アクティブシートのみ出力（横1ページに収める）
                    WW_Activesheet.Save(FILEpath, SaveFileFormat.Pdf)
                Case "csv"
                    FILEpath = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID & "\" & WW_datetime & ".CSV"
                    URL = CS0050SESSION.HTTPS_GET & "://" & HttpContext.Current.Request.Url.Host & "/" & CS0050SESSION.PRINT_ROOT_URL_NAME & "/" & CS0050SESSION.USERID & "/" & WW_datetime & ".CSV"
                    WW_Activesheet.Save(FILEpath, SaveFileFormat.Csv)

                Case "xls"
                    FILEpath = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID & "\" & WW_datetime & ".XLS"
                    URL = CS0050SESSION.HTTPS_GET & "://" & HttpContext.Current.Request.Url.Host & "/" & CS0050SESSION.PRINT_ROOT_URL_NAME & "/" & CS0050SESSION.USERID & "/" & WW_datetime & ".XLS"
                    'DioDocsでは、xls形式のExcelファイルを読み込んだり、また、作成した内容をxls形式のExcelファイルに保存することはできません。 from GreapeCityナレッジベース
                    WW_Workbook.Save(FILEpath, SaveFileFormat.Xlsx)
                Case "xlsx"
                    FILEpath = CS0050SESSION.UPLOAD_PATH & "\PRINTWORK\" & CS0050SESSION.USERID & "\" & WW_datetime & ".XLSX"
                    URL = CS0050SESSION.HTTPS_GET & "://" & HttpContext.Current.Request.Url.Host & "/" & CS0050SESSION.PRINT_ROOT_URL_NAME & "/" & CS0050SESSION.USERID & "/" & WW_datetime & ".XLSX"
                    'Workbook.Saveメソッドを呼び出すと、数式セルの結果値が計算され、Excelファイルに保存されます
                    WW_Workbook.Save(FILEpath, SaveFileFormat.Xlsx)
            End Select

        Catch ex As Exception
            ERR = C_MESSAGE_NO.FILE_IO_ERROR

            CS0011LOGWRITE.INFSUBCLASS = METHOD_NAME                    'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "Excel_Save"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力

            'Excel終了＆リリース
            WW_Structtbl.Clear()
            WW_Structtbl.Dispose()
            WW_Structtbl = Nothing
            WW_Structrow = Nothing
            WW_Workbook = Nothing
            Exit Sub
        End Try

        '■終了

        '○1秒間表示して終了処理へ
        System.Threading.Thread.Sleep(1000)

        '○Excel終了＆リリース
        WW_Structtbl.Clear()
        WW_Structtbl.Dispose()
        WW_Structtbl = Nothing
        WW_Structrow = Nothing
        WW_Workbook = Nothing

        ERR = C_MESSAGE_NO.NORMAL

    End Sub

    Public Function columnCheck(ByVal iTbl As DataTable, ByVal iField As String) As Boolean

        For i As Integer = 0 To iTbl.Columns.Count - 1
            'If iTbl.Columns(i).ColumnName = iField Then
            '    Return True
            'End If
            If iTbl.Columns(i).ColumnName.ToUpper = iField.ToUpper Then
                Return True
            End If
        Next

        Return False
    End Function



End Structure
