Option Explicit On
Imports MySql.Data.MySqlClient
Imports System.Data.SqlClient
Imports System.IO
Imports ClosedXML.Excel
Public Class CmnCheck
    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0023XLSUPLOAD As New CS0023XLSUPLOAD                  'XLSアップロード
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CMNPTS As New CmnParts                                  '共通関数

    Private TaishoYm As String = ""
    Private ToriList As DropDownList

    ''' <summary>
    ''' コンストラクタ
    ''' </summary>
    Public Sub New(ByVal I_taishoYm As String, ByVal I_toriList As DropDownList)
        TaishoYm = I_taishoYm
        ToriList = I_toriList
    End Sub

    ''' <summary>
    ''' (帳票)項目チェック処理(ENEOS)
    ''' </summary>
    ''' <param name="reportCode">輸送費明細(部署コード)</param>
    ''' <param name="LNT0001tbl">実績データ</param>
    ''' <param name="LNT0001Tanktbl">単価マスタ</param>
    ''' <param name="LNT0001Koteihi">固定費マスタ</param>
    ''' <param name="LNT0001TogouSprate">特別料金マスタ</param>
    ''' <param name="LNT0001Calendar">カレンダーマスタ</param>
    ''' <param name="LNT0001HolidayRate">休日割増単価マスタ</param>
    ''' <remarks></remarks>
    Public Sub WW_ReportCheckEneos(ByVal reportName As String, ByVal reportCode As String,
                                   ByRef LNT0001tbl As DataTable, ByRef LNT0001Tanktbl As DataTable, ByRef LNT0001Koteihi As DataTable,
                                   ByRef LNT0001TogouSprate As DataTable, ByRef LNT0001Calendar As DataTable, ByRef LNT0001HolidayRate As DataTable,
                                   Optional ByRef LNT0001HachinoheSprate As DataTable = Nothing,
                                   Optional ByRef LNT0001EneosComfee As DataTable = Nothing)
        Dim dtEneosTank As New DataTable
        Dim dtEneosTodoke As New DataTable
        Dim eneosTankClass As String = ""
        Dim eneosTodokeClass As String = ""
        Dim arrToriCode As String() = {"", "", ""}
        Dim fuzumiLimit As Decimal = 1.7                                    '--★不積(しきい値)

        Select Case reportCode
            '"ENEOS_八戸　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_020202
                eneosTankClass = "ENEOS_HACHINOHE_TANK"
                eneosTodokeClass = "AVOCADO_TODOKE_MAS"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0005700000
                arrToriCode(1) = BaseDllConst.CONST_ORDERORGCODE_020202

            '"ENEOS_水島　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_023301
                eneosTankClass = "ENEOS_MIZUSHIMA_TANK"
                eneosTodokeClass = "MIZUSHIMA_TODOKE_MAS"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0005700000
                arrToriCode(1) = BaseDllConst.CONST_ORDERORGCODE_023301

            '"DAIGAS_姫路　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_022801
                eneosTankClass = "DAIGAS_HIMEGI_TANK"
                eneosTodokeClass = "HIMEGI_TODOKE_MAS"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0051200000
                arrToriCode(1) = BaseDllConst.CONST_ORDERORGCODE_022801
                arrToriCode(2) = Nothing

            Case Else
                Exit Sub
        End Select

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            CMNPTS.SelectCONVERTMaster(SQLcon, eneosTankClass, dtEneosTank)
            CMNPTS.SelectCONVERTMaster(SQLcon, eneosTodokeClass, dtEneosTodoke)
            CMNPTS.SelectNEWTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", eneosTodokeClass, LNT0001Tanktbl)
            'CMNPTS.SelectTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", eneosTodokeClass, LNT0001Tanktbl)
            CMNPTS.SelectFIXEDMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm.Replace("/", ""), LNT0001Koteihi, I_CLASS:=eneosTankClass)
            'CMNPTS.SelectKOTEIHIMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", LNT0001Koteihi, I_CLASS:=eneosTankClass)
            CMNPTS.SelectHACHINOHESPRATEMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", LNT0001HachinoheSprate)
            CMNPTS.SelectENEOSCOMFEEMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", LNT0001EneosComfee)
            CMNPTS.SelectIntegrationSprateFEEMaster(SQLcon, arrToriCode(0), TaishoYm.Replace("/", ""), LNT0001TogouSprate, I_ORGCODE:=arrToriCode(1))
            CMNPTS.SelectCALENDARMaster(SQLcon, arrToriCode(0), TaishoYm + "/01", LNT0001Calendar)
            CMNPTS.SelectHOLIDAYRATEMaster(SQLcon, arrToriCode(0), LNT0001HolidayRate, I_dtTODOKEMas:=dtEneosTodoke, I_ORDERORGCODE:=arrToriCode(1), I_SHUKABASHO:=arrToriCode(2), I_CLASS:=eneosTodokeClass)
        End Using

        '〇(帳票)使用項目の設定
        WW_ReportMeisaiAdd(LNT0001tbl)

        '〇陸事番号マスタ設定
        For Each dtEneosTankrow As DataRow In dtEneosTank.Rows
            Dim condition As String = String.Format("TANKNUMBER='{0}'", dtEneosTankrow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                '★届日より日を取得(セル(行数)の設定のため)
                Dim setDay As String = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("dd")
                Dim lastMonth As Boolean = False
                If Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("yyyy/MM") = Date.Parse(TaishoYm + "/01").AddMonths(-1).ToString("yyyy/MM") Then
                    setDay = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
                    lastMonth = True
                End If
                Dim iLine As Integer = Integer.Parse(setDay) - 1
                iLine = (iLine * Integer.Parse(dtEneosTankrow("VALUE06"))) + Integer.Parse(dtEneosTankrow("VALUE05"))
                '★トリップより位置を取得
                Dim iTrip As Integer = Integer.Parse(LNT0001tblrow("TRIP_REP").ToString()) - 1
                iTrip += iLine
                LNT0001tblrow("ROWSORTNO") = dtEneosTankrow("VALUE01")
                LNT0001tblrow("SETCELL01") = dtEneosTankrow("VALUE02") + iTrip.ToString()
                LNT0001tblrow("SETCELL02") = dtEneosTankrow("VALUE03") + iTrip.ToString()
                LNT0001tblrow("SETCELL03") = dtEneosTankrow("VALUE04") + iTrip.ToString()
                LNT0001tblrow("SETLINE") = iTrip.ToString()

                '# 不積の判断 ----------------------------------------------------------------------------
                Dim todokeCode As String = LNT0001tblrow("TODOKECODE").ToString()
                Dim decFuzumi As Decimal = Decimal.Parse(LNT0001tblrow("SYABARA").ToString()) - fuzumiLimit
                Dim decZisseki As Decimal = Decimal.Parse(LNT0001tblrow("ZISSEKI").ToString())
                LNT0001tblrow("ZISSEKI_FUZUMI") = decFuzumi
                LNT0001tblrow("FUZUMI_REFVALUE") = decFuzumi - decZisseki
                If Decimal.Parse(LNT0001tblrow("FUZUMI_REFVALUE").ToString()) >= 0 Then
                    LNT0001tblrow("ZISSEKI_FUZUMIFLG") = "TRUE"
                Else
                    LNT0001tblrow("ZISSEKI_FUZUMIFLG") = "FALSE"
                End If
                ' ---------------------------------------------------------------------------------------/

                '★表示セルフラグ(1:表示)
                If dtEneosTankrow("VALUE07").ToString() = "1" Then
                    LNT0001tblrow("DISPLAYCELL_START") = dtEneosTankrow("VALUE02").ToString()
                    LNT0001tblrow("DISPLAYCELL_END") = dtEneosTankrow("VALUE04").ToString()
                    LNT0001tblrow("DISPLAYCELL_KOTEICHI") = dtEneosTankrow("VALUE08").ToString()
                Else
                    LNT0001tblrow("DISPLAYCELL_START") = ""
                    LNT0001tblrow("DISPLAYCELL_END") = ""
                    LNT0001tblrow("DISPLAYCELL_KOTEICHI") = ""
                End If

                '★備考設定用(出荷日と届日が不一致の場合)
                If LNT0001tblrow("SHUKADATE").ToString() <> LNT0001tblrow("TODOKEDATE").ToString() Then
                    If lastMonth = True Then
                        LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("M/d") + "積"
                    Else
                        LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("M/d") + "届"
                    End If
                End If
            Next
        Next

        '〇(ENEOS)届先出荷場所車庫マスタ設定
        For Each dtEneosTodokerow As DataRow In dtEneosTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}'", dtEneosTodokerow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("SHEETSORTNO_REP") = dtEneosTodokerow("KEYCODE03")
                LNT0001tblrow("TODOKENAME_REP") = dtEneosTodokerow("VALUE01")
                LNT0001tblrow("SHEETNAME_REP") = dtEneosTodokerow("VALUE06")
#Region "コメント"
                ''〇八戸営業所(東部瓦斯)独自仕様
                'If LNT0001tblrow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_005487 Then
                '    '★[３台目]に納入
                '    If LNT0001tblrow("TODOKEDATE_ORDER").ToString() = "3" Then
                '        LNT0001tblrow("TODOKENAME_REP") = dtEneosTodokerow("VALUE01") + LNT0001tblrow("TODOKEDATE_ORDER").ToString()
                '    End If
                'End If
                ''〇水島営業所 ----------------------------------------------------------------------------
                ''■コカ・コーラボトラーズジャパン株式会社(独自仕様)
                'If LNT0001tblrow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_005509 Then
                '    '--コカ・コーラ　ボトラーズジャパン(12.3t, 12.5t, 13.2t, 14t, 不積)
                '    Dim arrFuriwake005509 As String() = {"②", "③", "④", "不積"}

                '    '-- 不積判定の設定
                '    If LNT0001tblrow("ZISSEKI_FUZUMIFLG").ToString() = "TRUE" Then
                '        LNT0001tblrow("TODOKENAME_REP") = dtEneosTodokerow("VALUE01") + arrFuriwake005509(3)
                '    End If
                'End If
                '' ---------------------------------------------------------------------------------------/
#End Region
                '〇届先が追加された場合
                If dtEneosTodokerow("VALUE02").ToString() = "1" Then
                    LNT0001tblrow("TODOKECELL_REP") = dtEneosTodokerow("VALUE03")
                    LNT0001tblrow("MASTERCELL_REP") = dtEneosTodokerow("VALUE04")
                    LNT0001tblrow("SHEETDISPLAY_REP") = dtEneosTodokerow("VALUE05")
                Else
                    LNT0001tblrow("TODOKECELL_REP") = ""
                    LNT0001tblrow("MASTERCELL_REP") = ""
                    LNT0001tblrow("SHEETDISPLAY_REP") = ""
                End If
            Next
        Next

        ''○各シート(届先名)抽出処理
        'Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
        '    SQLcon.Open()  ' DataBase接続
        '    '〇実績WORK作成
        '    WW_InsertHachinoheMoment(SQLcon, reportName)
        'End Using

    End Sub

    ''' <summary>
    ''' (帳票)項目チェック処理(DAIGAS)
    ''' </summary>
    ''' <param name="reportCode">輸送費明細(部署コード)</param>
    ''' <param name="LNT0001tbl">実績データ</param>
    ''' <param name="LNT0001Tanktbl">単価マスタ</param>
    ''' <param name="LNT0001Koteihi">固定費マスタ</param>
    ''' <param name="LNT0001TogouSprate">特別料金マスタ</param>
    ''' <param name="LNT0001Calendar">カレンダーマスタ</param>
    ''' <param name="LNT0001HolidayRate">休日割増単価マスタ</param>
    ''' <remarks></remarks>
    Public Sub WW_ReportCheckDaigas(ByVal reportName As String, ByVal reportCode As String,
                                    ByRef LNT0001tbl As DataTable, ByRef LNT0001Tanktbl As DataTable, ByRef LNT0001Koteihi As DataTable,
                                    ByRef LNT0001TogouSprate As DataTable, ByRef LNT0001Calendar As DataTable, ByRef LNT0001HolidayRate As DataTable)

        Dim dtDaigasTank As New DataTable
        Dim dtDaigasTodoke As New DataTable
        Dim daigasTankClass As String = ""
        Dim daigasTodokeClass As String = ""
        Dim arrToriCode As String() = {"", "", ""}

        Select Case reportCode
            '"DAIGAS_泉北　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_022702
                daigasTankClass = "DAIGAS_SENBOKU_TANK"
                daigasTodokeClass = "SENBOKU_TODOKE_MAS"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0051200000
                arrToriCode(1) = BaseDllConst.CONST_ORDERORGCODE_022702
                '"DAIGAS_新宮　輸送費請求書"を指定した場合
                If ToriList.SelectedValue = "02270202" Then
                    daigasTodokeClass = "NIIMIYA_TODOKE_MAS"
                    arrToriCode(2) = BaseDllConst.CONST_TODOKECODE_001640
                ElseIf ToriList.SelectedValue = "02270203" Then
                    '"エスケイ産業　輸送費請求書"を指定した場合
                    daigasTankClass = "DAIGAS_ESUKEI_TANK"
                    daigasTodokeClass = "ESUKEI_TODOKE_MAS"
                    arrToriCode(0) = BaseDllConst.CONST_TORICODE_0045200000
                    arrToriCode(2) = BaseDllConst.CONST_TODOKECODE_004559
                Else
                    arrToriCode(2) = Nothing
                End If

            Case Else
                Exit Sub
        End Select

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            CMNPTS.SelectCONVERTMaster(SQLcon, daigasTankClass, dtDaigasTank)
            CMNPTS.SelectCONVERTMaster(SQLcon, daigasTodokeClass, dtDaigasTodoke)
            CMNPTS.SelectNEWTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", daigasTodokeClass, LNT0001Tanktbl, I_TODOKECODE:=arrToriCode(2))
            'CMNPTS.SelectTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", daigasTodokeClass, LNT0001Tanktbl, I_TODOKECODE:=arrToriCode(2))
            CMNPTS.SelectFIXEDMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm.Replace("/", ""), LNT0001Koteihi, I_CLASS:=daigasTankClass)
            'CMNPTS.SelectKOTEIHIMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", LNT0001Koteihi, I_CLASS:=daigasTankClass)
            CMNPTS.SelectCALENDARMaster(SQLcon, arrToriCode(0), TaishoYm + "/01", LNT0001Calendar)
            CMNPTS.SelectHOLIDAYRATEMaster(SQLcon, arrToriCode(0), LNT0001HolidayRate, I_dtTODOKEMas:=dtDaigasTodoke, I_ORDERORGCODE:=arrToriCode(1), I_SHUKABASHO:=arrToriCode(2), I_CLASS:=daigasTodokeClass)
        End Using

        '〇(帳票)使用項目の設定
        WW_ReportMeisaiAdd(LNT0001tbl)

        '〇陸事番号マスタ設定
        For Each dtDaigasTankrow As DataRow In dtDaigasTank.Rows
            Dim condition As String = String.Format("TANKNUMBER='{0}'", dtDaigasTankrow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                '★届日より日を取得(セル(行数)の設定のため)
                Dim setDay As String = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("dd")
                Dim lastMonth As Boolean = False
                If Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("yyyy/MM") = Date.Parse(TaishoYm + "/01").AddMonths(-1).ToString("yyyy/MM") Then
                    setDay = "1"
                    lastMonth = True
                End If
                Dim iLine As Integer = Integer.Parse(setDay) - 1
                iLine = (iLine * Integer.Parse(dtDaigasTankrow("VALUE06"))) + Integer.Parse(dtDaigasTankrow("VALUE05"))
                ''★トリップより位置を取得
                'Dim iTrip As Integer = Integer.Parse(LNT0001tblrow("TRIP_REP").ToString()) - 1
                'iTrip += iLine

                LNT0001tblrow("ROWSORTNO") = dtDaigasTankrow("VALUE01")
                If LNT0001tblrow("TODOKEDATE_ROWNUM") = "1" Then
                    LNT0001tblrow("SETCELL01") = dtDaigasTankrow("VALUE02") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtDaigasTankrow("VALUE03") + iLine.ToString()
                ElseIf LNT0001tblrow("TODOKEDATE_ROWNUM") = "2" Then
                    LNT0001tblrow("SETCELL01") = dtDaigasTankrow("VALUE04") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtDaigasTankrow("VALUE09") + iLine.ToString()
                ElseIf LNT0001tblrow("TODOKEDATE_ROWNUM") = "3" Then
                    LNT0001tblrow("SETCELL01") = dtDaigasTankrow("VALUE10") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtDaigasTankrow("VALUE11") + iLine.ToString()
                ElseIf LNT0001tblrow("TODOKEDATE_ROWNUM") = "4" Then
                    LNT0001tblrow("SETCELL01") = dtDaigasTankrow("VALUE12") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtDaigasTankrow("VALUE13") + iLine.ToString()
                ElseIf LNT0001tblrow("TODOKEDATE_ROWNUM") = "5" Then
                    '★単車の枠が４つしかないが５つあった場合はSKIP
                    If LNT0001tblrow("SYAGATA") = "単車" Then Continue For
                    LNT0001tblrow("SETCELL01") = dtDaigasTankrow("VALUE14") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtDaigasTankrow("VALUE15") + iLine.ToString()
                ElseIf LNT0001tblrow("TODOKEDATE_ROWNUM") = "6" Then
                    '★単車の枠が４つしかないが６つあった場合はSKIP
                    If LNT0001tblrow("SYAGATA") = "単車" Then Continue For
                    LNT0001tblrow("SETCELL01") = dtDaigasTankrow("VALUE16") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtDaigasTankrow("VALUE17") + iLine.ToString()
                Else
                    '★枠が７つ以上の場合はSKIP
                    Continue For
                End If
                LNT0001tblrow("SETLINE") = iLine

                '★表示セルフラグ(1:表示)
                If dtDaigasTankrow("VALUE07").ToString() = "1" Then
                    LNT0001tblrow("DISPLAYCELL_START") = dtDaigasTankrow("VALUE02").ToString()
                    LNT0001tblrow("DISPLAYCELL_END") = dtDaigasTankrow("VALUE04").ToString()
                    LNT0001tblrow("DISPLAYCELL_KOTEICHI") = dtDaigasTankrow("VALUE08").ToString()
                Else
                    LNT0001tblrow("DISPLAYCELL_START") = ""
                    LNT0001tblrow("DISPLAYCELL_END") = ""
                    LNT0001tblrow("DISPLAYCELL_KOTEICHI") = ""
                End If

                '★備考設定用(出荷日と届日が不一致の場合)
                If LNT0001tblrow("SHUKADATE").ToString() <> LNT0001tblrow("TODOKEDATE").ToString() Then
                    If lastMonth = True Then
                        LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("M/d") + "積"
                    Else
                        LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("M/d") + "届"
                    End If
                End If
            Next
        Next

        '〇(DAIGAS)届先出荷場所車庫マスタ設定
        For Each dtDaigasTodokerow As DataRow In dtDaigasTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}'", dtDaigasTodokerow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("SHEETSORTNO_REP") = dtDaigasTodokerow("KEYCODE03")

                '★ＤＧＥ(泉北)の場合([昭和産業㈱]独自対応)
                If LNT0001tblrow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_005866 Then
                    '１運行目は"1"(枝番)、２運行目は"2"(枝番)を後ろにつけて設定
                    Dim sheetName = dtDaigasTodokerow("VALUE01").ToString().Replace("1", "").Replace("2", "")
                    Dim blanchCode = LNT0001tblrow("BRANCHCODE").ToString()
                    LNT0001tblrow("TODOKENAME_REP") = sheetName + blanchCode
                    LNT0001tblrow("SHEETNAME_REP") = sheetName + blanchCode

                ElseIf LNT0001tblrow("BRANCHCODE").ToString() = "2" Then
                    '★ＤＧＥ(泉北)の場合([ハルナプロデュース]独自対応)
                    If LNT0001tblrow("TODOKECODE").ToString() = BaseDllConst.CONST_TODOKECODE_007304 Then
                        Dim sheetName = dtDaigasTodokerow("VALUE01").ToString().Replace("1", "").Replace("2", "")
                        Dim blanchCode = LNT0001tblrow("BRANCHCODE").ToString()
                        LNT0001tblrow("TODOKENAME_REP") = sheetName + blanchCode
                        'LNT0001tblrow("SHEETNAME_REP") = sheetName + blanchCode
                    End If

                Else
                    LNT0001tblrow("TODOKENAME_REP") = dtDaigasTodokerow("VALUE01")
                    LNT0001tblrow("SHEETNAME_REP") = dtDaigasTodokerow("VALUE06")
                End If

                '〇届先が追加された場合
                If dtDaigasTodokerow("VALUE02").ToString() = "1" Then
                    LNT0001tblrow("TODOKECELL_REP") = dtDaigasTodokerow("VALUE03")
                    LNT0001tblrow("MASTERCELL_REP") = dtDaigasTodokerow("VALUE04")
                    LNT0001tblrow("SHEETDISPLAY_REP") = dtDaigasTodokerow("VALUE05")
                Else
                    LNT0001tblrow("TODOKECELL_REP") = ""
                    LNT0001tblrow("MASTERCELL_REP") = ""
                    LNT0001tblrow("SHEETDISPLAY_REP") = ""
                End If
            Next
        Next

    End Sub

#Region "石油資源開発(本州分)"
    ''' <summary>
    ''' (帳票)項目チェック処理(石油資源開発(本州分))
    ''' </summary>
    ''' <param name="reportCode">輸送費明細(部署コード)</param>
    ''' <param name="dcNigataList">届先取得用(新潟支店車庫)</param>
    ''' <param name="dcSyonaiList">届先取得用(庄内営業所)</param>
    ''' <param name="dcTouhokuList">届先取得用(EX 東北支店車庫)</param>
    ''' <param name="dcIbarakiList">届先取得用(茨城営業所)</param>
    ''' <param name="LNT0001tbl">実績データ</param>
    ''' <param name="LNT0001Tanktbl">単価マスタ</param>
    ''' <param name="LNT0001Koteihi">固定費マスタ</param>
    ''' <param name="LNT0001SKKoteichi">SK固定値マスタ</param>
    ''' <param name="LNT0001TogouSprate">特別料金マスタ</param>
    ''' <param name="LNT0001Calendar">カレンダーマスタ</param>
    ''' <param name="LNT0001HolidayRate">休日割増単価マスタ</param>
    ''' <remarks></remarks>
    Public Sub WW_ReportCheckSekiyuSigen(ByVal reportName As String, ByVal reportCode As String,
                                         ByRef dcNigataList As Dictionary(Of String, String), ByRef dcSyonaiList As Dictionary(Of String, String),
                                         ByRef dcTouhokuList As Dictionary(Of String, String), ByRef dcIbarakiList As Dictionary(Of String, String),
                                         ByRef LNT0001tbl As DataTable, ByRef LNT0001Tanktbl As DataTable, ByRef LNT0001Koteihi As DataTable, ByRef LNT0001SKSurcharge As DataTable, ByRef LNT0001SKKoteichi As DataTable,
                                         ByRef LNT0001TogouSprate As DataTable, ByRef LNT0001Calendar As DataTable, ByRef LNT0001HolidayRate As DataTable)

        Dim dtSekiyuSigenTank As New DataTable
        Dim dtSekiyuSigenTankSub As New DataTable
        Dim dtSekiyuSigenTodoke As New DataTable
        Dim sekiyuSigenTankClass As String = ""
        Dim sekiyuSigenTankSubClass As String = ""
        Dim sekiyuSigenTodokeClass As String = ""
        Dim sekiyuSigenSGKoteihiClass As String = ""
        Dim arrToriCode As String() = {"", "", ""}
        Dim listOrderOrgCode As New List(Of String)
        Dim commaOrderOrgCode As String = ""
        Dim fuzumiLimit As Decimal = 1.7                    '--★不積(しきい値)
        Dim arrFuzumi002022_302 As String() = {"T", "U"}    '--（ＳＫ）本田金属　喜多方サテライト(302号車(11.4t車)不積)
        Dim arrFuzumi002019_333 As String() = {"T", "U"}    '--（ＳＫ）テーブルマーク　塩沢      (333号車(14.0t車)不積)
        Dim arrFuzumi002019_334 As String() = {"Z", "AA"}   '--（ＳＫ）テーブルマーク　塩沢      (334号車(15.7t車)不積)

        Dim arrOPFCycle_002025_326 As String() = {"Z", "AA"}    ' （ＳＫ）若松ガス　玉川(326号車(若松1.5回転))

        Select Case reportCode
            '"石油資源開発　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_021502
                sekiyuSigenTankClass = "SEKIYUSIGEN_TANK"
                sekiyuSigenTankSubClass = "SEKIYUSIGEN_TANK_OTR"
                sekiyuSigenTodokeClass = "SEKIYUSIG_TODOKE_MAS"
                sekiyuSigenSGKoteihiClass = "SEKIYUSIGEN_KOTEIHI"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0132800000
                arrToriCode(1) = Nothing
                arrToriCode(2) = Nothing
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_021502)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_020601)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_020402)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_020804)
                commaOrderOrgCode = String.Join(",", listOrderOrgCode)
            Case Else
                Exit Sub
        End Select

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            CMNPTS.SelectCONVERTMaster(SQLcon, sekiyuSigenTankClass, dtSekiyuSigenTank)
            CMNPTS.SelectCONVERTMaster(SQLcon, sekiyuSigenTankSubClass, dtSekiyuSigenTankSub)
            CMNPTS.SelectCONVERTMaster(SQLcon, sekiyuSigenTodokeClass, dtSekiyuSigenTodoke)
            CMNPTS.SelectNEWTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", sekiyuSigenTodokeClass, LNT0001Tanktbl, I_SEKIYU_HONSHU_FLG:=True)
            'CMNPTS.SelectTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", sekiyuSigenTodokeClass, LNT0001Tanktbl)
            CMNPTS.SelectSKFIXEDMaster(SQLcon, arrToriCode(0), commaOrderOrgCode, TaishoYm.Replace("/", ""), LNT0001Koteihi, I_CLASS:=sekiyuSigenSGKoteihiClass)
            'CMNPTS.SelectSKKOTEIHIMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", LNT0001Koteihi, I_CLASS:=sekiyuSigenSGKoteihiClass)
            CMNPTS.SelectSKFuelSurchargeMaster(SQLcon, arrToriCode(0), BaseDllConst.CONST_ORDERORGCODE_020804, TaishoYm.Replace("/", ""), LNT0001SKSurcharge)
            CMNPTS.SelectCALENDARMaster(SQLcon, arrToriCode(0), TaishoYm + "/01", LNT0001Calendar)
            CMNPTS.SelectSKKOTEICHIMaster(SQLcon, LNT0001Tanktbl, LNT0001SKKoteichi)
            CMNPTS.SelectIntegrationSprateFEEMaster(SQLcon, arrToriCode(0), TaishoYm.Replace("/", ""), LNT0001TogouSprate, I_ORGCODE:=commaOrderOrgCode, I_CLASS:=sekiyuSigenTodokeClass)
            CMNPTS.SelectHOLIDAYRATEMaster(SQLcon, arrToriCode(0), LNT0001HolidayRate, I_dtTODOKEMas:=dtSekiyuSigenTodoke, I_ORDERORGCODE:=commaOrderOrgCode, I_SHUKABASHO:=arrToriCode(2), I_CLASS:=sekiyuSigenTodokeClass)
        End Using

        '〇(帳票)使用項目の設定
        WW_ReportMeisaiAdd(LNT0001tbl)

        '〇陸事番号マスタ設定
        For Each dtSekiyuSigenTankrow As DataRow In dtSekiyuSigenTank.Rows
            Dim condition As String = String.Format("TANKNUMBER='{0}'", dtSekiyuSigenTankrow("KEYCODE01"))
            If Mid(ToriList.SelectedValue, 1, 6) = BaseDllConst.CONST_ORDERORGCODE_021502 Then
                condition &= String.Format(" AND SHUKABASHO='{0}'", dtSekiyuSigenTankrow("KEYCODE05"))
            End If
            '★届先(個別設定)は除く
            condition &= String.Format(" AND TODOKECODE NOT IN ('{0}', '{1}')",
                                       BaseDllConst.CONST_TODOKECODE_004012,
                                       BaseDllConst.CONST_TODOKECODE_005890)
            '届先(明細)セル値設定
            WW_SekiyuSigenRikugiMas(dtSekiyuSigenTankrow, condition, fuzumiLimit, LNT0001tbl)

        Next
        '〇陸事番号マスタ設定(※個別設定用)
        Dim todokeMerge = CMNPTS.filterItem(dtSekiyuSigenTankSub, "KEYCODE07", "KEYCODE08")
        For Each dtSekiyuSigenTankrow As DataRow In dtSekiyuSigenTankSub.Rows
            Dim condition As String = String.Format("TANKNUMBER='{0}'", dtSekiyuSigenTankrow("KEYCODE01"))
            If Mid(ToriList.SelectedValue, 1, 6) = BaseDllConst.CONST_ORDERORGCODE_021502 Then
                condition &= String.Format(" AND SHUKABASHO='{0}'", dtSekiyuSigenTankrow("KEYCODE05"))
            End If
            '★届先(個別設定)のみ
            condition &= String.Format(" AND TODOKECODE IN ({0})", todokeMerge)
            '届先(明細)セル値設定
            WW_SekiyuSigenRikugiMas(dtSekiyuSigenTankrow, condition, fuzumiLimit, LNT0001tbl)
        Next

        '〇石油資源開発(不積判定の設定) ----------------------------------------------------------
        '■若松ｶﾞｽ(喜多方) 
        '  --302号車(11.4t車)不積
        WW_SetSekiyuSigenFuzumi(BaseDllConst.CONST_TODOKECODE_002022, arrFuzumi002022_302, LNT0001tbl, gyomuNo:="302")
        '■ﾃｰﾌﾞﾙﾏｰｸ新潟魚沼工場
        '  --333号車(14.0t車)不積 
        WW_SetSekiyuSigenFuzumi(BaseDllConst.CONST_TODOKECODE_002019, arrFuzumi002019_333, LNT0001tbl, gyomuNo:="333", tyoseiFlg:=True)
        '  --334号車(15.7t車)不積
        WW_SetSekiyuSigenFuzumi(BaseDllConst.CONST_TODOKECODE_002019, arrFuzumi002019_334, LNT0001tbl, gyomuNo:="334", tyoseiFlg:=True)
        ' ---------------------------------------------------------------------------------------/

        '〇石油資源開発(1.5回転の設定) -----------------------------------------------------------
        '■若松ｶﾞｽ(玉川)
        '  --326号車(若松1.5回転)
        WW_SetSekiyuSigenOnePointFiveCycle(BaseDllConst.CONST_TODOKECODE_002025, "積込", "積置", "326", arrOPFCycle_002025_326, LNT0001tbl, judgeDate:="SHUKADATE")
        WW_SetSekiyuSigenOnePointFiveCycle(BaseDllConst.CONST_TODOKECODE_002025, "荷卸", "積配", "326", arrOPFCycle_002025_326, LNT0001tbl, tyoseiFlg:=True)
        ' ---------------------------------------------------------------------------------------/

        '〇(石油資源開発)届先出荷場所車庫マスタ設定
        For Each SekiyuSigenTodokerow As DataRow In dtSekiyuSigenTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}'", SekiyuSigenTodokerow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("SHEETSORTNO_REP") = SekiyuSigenTodokerow("KEYCODE03")
                LNT0001tblrow("TODOKENAME_REP") = SekiyuSigenTodokerow("VALUE01")
                LNT0001tblrow("SHEETNAME_REP") = SekiyuSigenTodokerow("VALUE06")
                'LNT0001tblrow("GROUPNO_REP") = SekiyuSigenTodokerow("KEYCODE08")

                '〇届先が追加された場合
                If SekiyuSigenTodokerow("VALUE02").ToString() = "1" Then
                    LNT0001tblrow("TODOKECELL_REP") = SekiyuSigenTodokerow("VALUE03")
                    LNT0001tblrow("MASTERCELL_REP") = SekiyuSigenTodokerow("VALUE04")
                    LNT0001tblrow("SHEETDISPLAY_REP") = SekiyuSigenTodokerow("VALUE05")
                Else
                    LNT0001tblrow("TODOKECELL_REP") = ""
                    LNT0001tblrow("MASTERCELL_REP") = ""
                    LNT0001tblrow("SHEETDISPLAY_REP") = ""
                End If
            Next
        Next

        '〇各部署ごとの情報取得
        For Each SekiyuSigenTodokerow As DataRow In dtSekiyuSigenTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}' AND SHUKABASHO='{1}'", SekiyuSigenTodokerow("KEYCODE01"), SekiyuSigenTodokerow("KEYCODE06"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("GROUPNO_REP") = SekiyuSigenTodokerow("KEYCODE08")
            Next
        Next
        For Each SekiyuSigenTodokerow As DataRow In dtSekiyuSigenTodoke.Select("", "KEYCODE08, KEYCODE01")
            Select Case SekiyuSigenTodokerow("KEYCODE08").ToString()
                Case "1"
                    Try
                        dcNigataList.Add(SekiyuSigenTodokerow("KEYCODE01"), SekiyuSigenTodokerow("KEYCODE09"))
                    Catch ex As Exception
                    End Try
                Case "2"
                    dcSyonaiList.Add(SekiyuSigenTodokerow("KEYCODE01"), SekiyuSigenTodokerow("KEYCODE09"))
                Case "3"
                    dcTouhokuList.Add(SekiyuSigenTodokerow("KEYCODE01"), SekiyuSigenTodokerow("KEYCODE09"))
                Case "4"
                    dcIbarakiList.Add(SekiyuSigenTodokerow("KEYCODE01"), SekiyuSigenTodokerow("KEYCODE09"))
            End Select
        Next

    End Sub

    ''' <summary>
    ''' 石油資源開発(届先(明細)セル値設定)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_SekiyuSigenRikugiMas(ByVal dtSekiyuSigenTankrow As DataRow, ByVal condition As String, ByVal fuzumiLimit As Decimal, ByRef LNT0001tbl As DataTable)
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
            '★届日より日を取得(セル(行数)の設定のため)
            Dim setDay As String = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
            Dim lastMonth As Boolean = False
            If Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("yyyy/MM") = Date.Parse(TaishoYm + "/01").AddMonths(-1).ToString("yyyy/MM") Then
                setDay = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
                lastMonth = True
            End If
            Dim iLine As Integer = Integer.Parse(setDay) - 1
            iLine = (iLine * Integer.Parse(dtSekiyuSigenTankrow("VALUE06"))) + Integer.Parse(dtSekiyuSigenTankrow("VALUE05"))
            ''★トリップより位置を取得
            'Dim iTrip As Integer = Integer.Parse(LNT0001tblrow("TRIP_REP").ToString()) - 1
            'iTrip += iLine
            LNT0001tblrow("ROWSORTNO") = dtSekiyuSigenTankrow("VALUE01")
            LNT0001tblrow("SETCELL01") = dtSekiyuSigenTankrow("VALUE02") + iLine.ToString()
            LNT0001tblrow("SETCELL02") = dtSekiyuSigenTankrow("VALUE03") + iLine.ToString()
            'LNT0001tblrow("SETCELL03") = dtSekiyuSigenTankrow("VALUE04") + iLine.ToString()
            LNT0001tblrow("SETLINE") = iLine
            LNT0001tblrow("ORDERORGCODE_REP") = dtSekiyuSigenTankrow("KEYCODE05")
            LNT0001tblrow("GYOMUTANKNUM_REP") = dtSekiyuSigenTankrow("KEYCODE04")

            '# 不積の判断
            Dim todokeCode As String = LNT0001tblrow("TODOKECODE").ToString()
            Dim decFuzumi As Decimal = Decimal.Parse(LNT0001tblrow("SYABARA").ToString()) - fuzumiLimit
            Dim decZisseki As Decimal = Decimal.Parse(LNT0001tblrow("ZISSEKI").ToString())
            LNT0001tblrow("ZISSEKI_FUZUMI") = decFuzumi
            LNT0001tblrow("FUZUMI_REFVALUE") = decFuzumi - decZisseki
            If Decimal.Parse(LNT0001tblrow("FUZUMI_REFVALUE").ToString()) >= 0 Then
                LNT0001tblrow("ZISSEKI_FUZUMIFLG") = "TRUE"
            Else
                LNT0001tblrow("ZISSEKI_FUZUMIFLG") = "FALSE"
            End If

            '★表示セルフラグ(1:表示)
            If dtSekiyuSigenTankrow("VALUE07").ToString() = "1" Then
                LNT0001tblrow("DISPLAYCELL_START") = dtSekiyuSigenTankrow("VALUE02").ToString()
                LNT0001tblrow("DISPLAYCELL_END") = dtSekiyuSigenTankrow("VALUE03").ToString()
                'LNT0001tblrow("DISPLAYCELL_KOTEICHI") = dtSekiyuSigenTankrow("VALUE08").ToString()
            Else
                LNT0001tblrow("DISPLAYCELL_START") = ""
                LNT0001tblrow("DISPLAYCELL_END") = ""
                'LNT0001tblrow("DISPLAYCELL_KOTEICHI") = ""
            End If

            '★備考設定用(出荷日と届日が不一致の場合)
            If LNT0001tblrow("SHUKADATE").ToString() <> LNT0001tblrow("TODOKEDATE").ToString() Then
                If lastMonth = True Then
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("M/d") + "積"
                Else
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("M/d") + "届"
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' 石油資源開発(不積判定の設定)
    ''' </summary>
    Protected Sub WW_SetSekiyuSigenFuzumi(ByVal todokeCode As String, cellSet As String(),
                                          ByRef LNT0001tbl As DataTable,
                                          Optional ByVal gyomuNo As String = Nothing,
                                          Optional ByVal tyoseiFlg As Boolean = False)
        Dim condition As String = ""
        condition &= String.Format("TODOKECODE='{0}' ", todokeCode)
        If tyoseiFlg = False Then
            condition &= "AND ZISSEKI_FUZUMIFLG='TRUE' "
        Else
            '★単価調整にて"2"(不積単価)設定
            condition &= "AND (ZISSEKI_FUZUMIFLG='TRUE' OR BRANCHCODE=2) "
        End If
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
            If Not IsNothing(gyomuNo) AndAlso LNT0001tblrow("GYOMUTANKNUM_REP").ToString() <> gyomuNo Then
                Continue For
            End If
            LNT0001tblrow("SETCELL01") = cellSet(0) + LNT0001tblrow("SETLINE").ToString()
            LNT0001tblrow("SETCELL02") = cellSet(1) + LNT0001tblrow("SETLINE").ToString()
        Next

    End Sub

    ''' <summary>
    ''' 石油資源開発(1.5回転の設定)
    ''' </summary>
    ''' <param name="todokeCode">届先コード</param>
    ''' <param name="loadUnloType">積込荷卸区分</param>
    ''' <param name="stackingType">積置区分</param>
    Protected Sub WW_SetSekiyuSigenOnePointFiveCycle(ByVal todokeCode As String, ByVal loadUnloType As String, ByVal stackingType As String, ByVal gyomuNo As String, cellSet As String(),
                                                     ByRef LNT0001tbl As DataTable,
                                                     Optional ByVal judgeDate As String = "TODOKEDATE",
                                                     Optional ByVal tyoseiFlg As Boolean = False)
        Dim condition As String = ""
        condition &= String.Format("TODOKECODE='{0}' ", todokeCode)             '-- 届先
        condition &= String.Format("AND LOADUNLOTYPE='{0}' ", loadUnloType)     '-- 積込荷卸区分
        condition &= String.Format("AND STACKINGTYPE='{0}' ", stackingType)     '-- 積置区分
        condition &= String.Format("AND GYOMUTANKNUM_REP='{0}' ", gyomuNo)      '-- 業務車番

        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
            Dim conditionSub As String = ""
            conditionSub &= String.Format("TODOKECODE='{0}' ", LNT0001tblrow("TODOKECODE").ToString())
            conditionSub &= String.Format("AND SHUKADATE='{0}' ", LNT0001tblrow(judgeDate).ToString())
            conditionSub &= String.Format("AND TODOKEDATE='{0}' ", LNT0001tblrow(judgeDate).ToString())
            conditionSub &= String.Format("AND STAFFCODE='{0}' ", LNT0001tblrow("STAFFCODE").ToString())
            conditionSub &= String.Format("AND GYOMUTANKNUM_REP='{0}' ", LNT0001tblrow("GYOMUTANKNUM_REP").ToString())

            For Each LNT0001tblSubrow As DataRow In LNT0001tbl.Select(conditionSub)
                LNT0001tblrow("SETCELL01") = cellSet(0) + LNT0001tblrow("SETLINE").ToString()
                LNT0001tblrow("SETCELL02") = cellSet(1) + LNT0001tblrow("SETLINE").ToString()
            Next
        Next

        '〇単価調整フラグFALSEの場合は終了
        If tyoseiFlg = False Then Exit Sub

        '★単価調整にて"2"(1.5回転)設定
        condition &= " AND BRANCHCODE=2 "
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
            LNT0001tblrow("SETCELL01") = cellSet(0) + LNT0001tblrow("SETLINE").ToString()
            LNT0001tblrow("SETCELL02") = cellSet(1) + LNT0001tblrow("SETLINE").ToString()
        Next

    End Sub
#End Region

#Region "石油資源開発(北海道)"
    ''' <summary>
    ''' (帳票)項目チェック処理(石油資源開発(北海道))
    ''' </summary>
    ''' <param name="reportCode">輸送費明細(部署コード)</param>
    ''' <param name="dcIshikariList">届先取得用(石狩営業所)</param>
    ''' <param name="LNT0001tbl">実績データ</param>
    ''' <param name="LNT0001Tanktbl">単価マスタ</param>
    ''' <param name="LNT0001SKSprate">SK特別料金マスタ</param>
    ''' <param name="LNT0001SKSurcharge">SK燃料サーチャージマスタ</param>
    ''' <param name="LNT0001TogouSprate">特別料金マスタ</param>
    ''' <param name="LNT0001Calendar">カレンダーマスタ</param>
    ''' <param name="LNT0001HolidayRate">休日割増単価マスタ</param>
    ''' <remarks></remarks>
    Public Sub WW_ReportCheckSekiyuSigenHokaido(ByVal reportName As String, ByVal reportCode As String, ByRef dcIshikariList As Dictionary(Of String, String),
                                                   ByRef LNT0001tbl As DataTable, ByRef LNT0001Tanktbl As DataTable,
                                                   ByRef LNT0001SKSprate As DataTable, ByRef LNT0001SKSurcharge As DataTable,
                                                   ByRef LNT0001TogouSprate As DataTable, ByRef LNT0001Calendar As DataTable, ByRef LNT0001HolidayRate As DataTable)

        Dim dtSekiyuSigenHKDTank As New DataTable
        Dim dtSekiyuSigenHKDTodoke As New DataTable
        Dim sekiyuSigenTankHKDClass As String = ""
        Dim sekiyuSigenTodokeHKDClass As String = ""
        Dim sekiyuSigenKoteihiHKDClass As String = ""
        Dim arrToriCode As String() = {"", "", ""}
        Dim fuzumiLimit As Decimal = 1.7                    '--★不積(しきい値)

        Select Case reportCode
            '"石油資源開発(北海道)　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_020104
                sekiyuSigenTankHKDClass = "SEKIYUSIGEN_HKD_TANK"
                sekiyuSigenTodokeHKDClass = "SEKIYUSIG_HKD_TODOKE"
                sekiyuSigenKoteihiHKDClass = "SEKIYU_HKD_KOTEIHI"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0132800000
                arrToriCode(1) = BaseDllConst.CONST_ORDERORGCODE_020104
                arrToriCode(2) = Nothing
        End Select

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            CMNPTS.SelectCONVERTMaster(SQLcon, sekiyuSigenTankHKDClass, dtSekiyuSigenHKDTank)
            CMNPTS.SelectCONVERTMaster(SQLcon, sekiyuSigenTodokeHKDClass, dtSekiyuSigenHKDTodoke)
            CMNPTS.SelectNEWTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", sekiyuSigenTodokeHKDClass, LNT0001Tanktbl)
            'CMNPTS.SelectTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", sekiyuSigenTodokeHKDClass, LNT0001Tanktbl)
            CMNPTS.SelectSKSpecialFEEMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", LNT0001SKSprate, I_CLASS:=sekiyuSigenKoteihiHKDClass)
            CMNPTS.SelectSKFuelSurchargeMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm.Replace("/", ""), LNT0001SKSurcharge)
            CMNPTS.SelectIntegrationSprateFEEMaster(SQLcon, arrToriCode(0), TaishoYm.Replace("/", ""), LNT0001TogouSprate, I_ORGCODE:=arrToriCode(1), I_CLASS:=sekiyuSigenKoteihiHKDClass)
            CMNPTS.SelectCALENDARMaster(SQLcon, arrToriCode(0), TaishoYm + "/01", LNT0001Calendar)
            CMNPTS.SelectHOLIDAYRATEMaster(SQLcon, arrToriCode(0), LNT0001HolidayRate, I_dtTODOKEMas:=LNT0001SKSprate, I_ORDERORGCODE:=arrToriCode(1), I_SHUKABASHO:=arrToriCode(2), I_CLASS:=sekiyuSigenKoteihiHKDClass)
        End Using

        '〇(帳票)使用項目の設定
        WW_ReportMeisaiAdd(LNT0001tbl)

        '〇陸事番号マスタ設定
        For Each dtSekiyuSigenHKDTankrow As DataRow In dtSekiyuSigenHKDTank.Rows
            Dim condition As String = String.Format("TANKNUMBER='{0}'", dtSekiyuSigenHKDTankrow("KEYCODE01"))
            If Mid(ToriList.SelectedValue, 1, 6) = BaseDllConst.CONST_ORDERORGCODE_020104 Then
                condition &= String.Format(" AND TODOKECODE='{0}'", dtSekiyuSigenHKDTankrow("KEYCODE05"))
            End If
            '届先(明細)セル値設定
            WW_SekiyuSigenHKDRikugiMas(dtSekiyuSigenHKDTankrow, condition, fuzumiLimit, LNT0001tbl)
        Next

        '〇(石油資源開発)届先出荷場所車庫マスタ設定
        For Each SekiyuSigenTodokerow As DataRow In dtSekiyuSigenHKDTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}'", SekiyuSigenTodokerow("KEYCODE01"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("SHEETSORTNO_REP") = SekiyuSigenTodokerow("KEYCODE03")
                LNT0001tblrow("TODOKENAME_REP") = SekiyuSigenTodokerow("VALUE01")

                '〇届先が追加された場合
                If SekiyuSigenTodokerow("VALUE02").ToString() = "1" Then
                    LNT0001tblrow("TODOKECELL_REP") = SekiyuSigenTodokerow("VALUE03")
                    LNT0001tblrow("MASTERCELL_REP") = SekiyuSigenTodokerow("VALUE04")
                    LNT0001tblrow("SHEETDISPLAY_REP") = SekiyuSigenTodokerow("VALUE05")
                Else
                    LNT0001tblrow("TODOKECELL_REP") = ""
                    LNT0001tblrow("MASTERCELL_REP") = ""
                    LNT0001tblrow("SHEETDISPLAY_REP") = ""
                End If
            Next
        Next
        '〇各種別ごとの情報取得
        For Each SekiyuSigenTodokerow As DataRow In dtSekiyuSigenHKDTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}' AND SETCELL03='{1}'", SekiyuSigenTodokerow("KEYCODE01"), SekiyuSigenTodokerow("KEYCODE10"))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("SHEETNAME_REP") = SekiyuSigenTodokerow("VALUE06")
                LNT0001tblrow("GROUPNO_REP") = SekiyuSigenTodokerow("KEYCODE08")
            Next
        Next
        'シート名取得用
        For Each SekiyuSigenTodokerow As DataRow In dtSekiyuSigenHKDTodoke.Select("", "KEYCODE01")
            If SekiyuSigenTodokerow("KEYCODE01").ToString() = "" Then Continue For
            Dim subNo As String = SekiyuSigenTodokerow("KEYCODE08") + SekiyuSigenTodokerow("KEYCODE03")
            Try
                dcIshikariList.Add(SekiyuSigenTodokerow("KEYCODE01") + subNo, SekiyuSigenTodokerow("KEYCODE09"))
            Catch ex As Exception
            End Try
        Next

    End Sub

    ''' <summary>
    ''' 石油資源開発(北海道(届先(明細)セル値設定))
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_SekiyuSigenHKDRikugiMas(ByVal dtSekiyuSigenTankrow As DataRow, ByVal condition As String, ByVal fuzumiLimit As Decimal, ByRef LNT0001tbl As DataTable)
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
            '★届日より日を取得(セル(行数)の設定のため)
            Dim setDay As String = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
            Dim lastMonth As Boolean = False
            If Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("yyyy/MM") = Date.Parse(TaishoYm + "/01").AddMonths(-1).ToString("yyyy/MM") Then
                setDay = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
                lastMonth = True
            End If
            Dim iLine As Integer = Integer.Parse(setDay) - 1
            iLine = (iLine * Integer.Parse(dtSekiyuSigenTankrow("VALUE06"))) + Integer.Parse(dtSekiyuSigenTankrow("VALUE05"))
            ''★トリップより位置を取得
            'Dim iTrip As Integer = Integer.Parse(LNT0001tblrow("TRIP_REP").ToString()) - 1
            'iTrip += iLine
            LNT0001tblrow("ROWSORTNO") = dtSekiyuSigenTankrow("VALUE01")
            If LNT0001tblrow("TRIP") = "1" Then
                LNT0001tblrow("SETCELL01") = dtSekiyuSigenTankrow("VALUE02") + iLine.ToString()
                LNT0001tblrow("SETCELL02") = dtSekiyuSigenTankrow("VALUE03") + iLine.ToString()
            ElseIf LNT0001tblrow("TRIP") = "2" Then
                LNT0001tblrow("SETCELL01") = dtSekiyuSigenTankrow("VALUE04") + iLine.ToString()
                LNT0001tblrow("SETCELL02") = dtSekiyuSigenTankrow("VALUE09") + iLine.ToString()
                If dtSekiyuSigenTankrow("VALUE04").ToString() = "" Then
                    LNT0001tblrow("SETCELL01") = dtSekiyuSigenTankrow("VALUE02") + iLine.ToString()
                    LNT0001tblrow("SETCELL02") = dtSekiyuSigenTankrow("VALUE03") + iLine.ToString()
                End If
            End If
            LNT0001tblrow("SETCELL03") = dtSekiyuSigenTankrow("KEYCODE02")
            LNT0001tblrow("SETLINE") = iLine
            LNT0001tblrow("ORDERORGCODE_REP") = dtSekiyuSigenTankrow("KEYCODE05")
            LNT0001tblrow("GYOMUTANKNUM_REP") = dtSekiyuSigenTankrow("KEYCODE04")
            LNT0001tblrow("ROLLY_CONTAINER") = dtSekiyuSigenTankrow("KEYCODE03")

            '★表示セルフラグ(1:表示)
            If dtSekiyuSigenTankrow("VALUE07").ToString() = "1" Then
                LNT0001tblrow("DISPLAYCELL_START") = dtSekiyuSigenTankrow("VALUE02").ToString()
                If dtSekiyuSigenTankrow("KEYCODE05") = BaseDllConst.CONST_TODOKECODE_006915 _
                    OrElse dtSekiyuSigenTankrow("KEYCODE05") = BaseDllConst.CONST_TODOKECODE_005834 Then
                    LNT0001tblrow("DISPLAYCELL_END") = dtSekiyuSigenTankrow("VALUE09").ToString()
                Else
                    LNT0001tblrow("DISPLAYCELL_END") = dtSekiyuSigenTankrow("VALUE03").ToString()
                End If
                'LNT0001tblrow("DISPLAYCELL_KOTEICHI") = dtSekiyuSigenTankrow("VALUE08").ToString()
            Else
                LNT0001tblrow("DISPLAYCELL_START") = ""
                LNT0001tblrow("DISPLAYCELL_END") = ""
                'LNT0001tblrow("DISPLAYCELL_KOTEICHI") = ""
            End If

            '★備考設定用(出荷日と届日が不一致の場合)
            If LNT0001tblrow("SHUKADATE").ToString() <> LNT0001tblrow("TODOKEDATE").ToString() Then
                If lastMonth = True Then
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("M/d") + "積"
                Else
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("M/d") + "届"
                End If
            End If

        Next
    End Sub
#End Region

#Region "シーエナジー・エルネス"
    ''' <summary>
    ''' (帳票)項目チェック処理(シーエナジー・エルネス)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ReportCheckCenergyElNess(ByVal reportName As String, ByVal reportCode As String,
                                           ByRef dcCenergyList As Dictionary(Of String, String), ByRef dcElNessList As Dictionary(Of String, String),
                                           ByRef LNT0001tbl As DataTable, ByRef LNT0001Tanktbl As DataTable, ByRef LNT0001Koteihi As DataTable,
                                           ByRef LNT0001TogouSprate As DataTable, ByRef LNT0001Calendar As DataTable, ByRef LNT0001HolidayRate As DataTable)

        Dim dtCenergyElNessTank As New DataTable
        Dim dtCenergyTodoke As New DataTable
        Dim dtElNessTodoke As New DataTable
        Dim cenergyElNessTankClass As String = ""
        Dim cenergyTodokeClass As String = ""
        Dim elNessTodokeClass As String = ""
        'Dim cenergyElNessKoteihiClass As String = ""
        Dim arrToriCode As String() = {"", "", ""}
        Dim fuzumiLimit As Decimal = 1.7                    '--★不積(しきい値)
        Dim listOrderOrgCode As New List(Of String)
        Dim commaOrderOrgCode As String = ""

        Select Case reportCode
            '"シーエナジー・エルネス　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_022302
                cenergyElNessTankClass = "CENERGY_TANK"
                cenergyTodokeClass = "CENERGY_TODOKE"
                elNessTodokeClass = "ELNESS_TODOKE"
                'cenergyElNessKoteihiClass = ""
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0110600000
                arrToriCode(1) = Nothing
                arrToriCode(2) = Nothing
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_021502)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_022302)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_021504)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_021601)
                listOrderOrgCode.Add(BaseDllConst.CONST_ORDERORGCODE_022401)
                commaOrderOrgCode = String.Join(",", listOrderOrgCode)
        End Select

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            CMNPTS.SelectCONVERTMaster(SQLcon, cenergyElNessTankClass, dtCenergyElNessTank)
            CMNPTS.SelectCONVERTMaster(SQLcon, cenergyTodokeClass, dtCenergyTodoke)
            CMNPTS.SelectCONVERTMaster(SQLcon, elNessTodokeClass, dtElNessTodoke)
            CMNPTS.SelectCALENDARMaster(SQLcon, arrToriCode(0), TaishoYm + "/01", LNT0001Calendar)
            CMNPTS.SelectNEWTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", cenergyTodokeClass, LNT0001Tanktbl)
            CMNPTS.SelectFIXEDMaster(SQLcon, arrToriCode(0), commaOrderOrgCode, TaishoYm.Replace("/", ""), LNT0001Koteihi, I_CLASS:=cenergyElNessTankClass)
            CMNPTS.SelectIntegrationSprateFEEMaster(SQLcon, arrToriCode(0), TaishoYm.Replace("/", ""), LNT0001TogouSprate)
            CMNPTS.SelectHOLIDAYRATEMaster(SQLcon, arrToriCode(0), LNT0001HolidayRate, I_dtTODOKEMas:=dtCenergyElNessTank, I_ORDERORGCODE:=arrToriCode(1), I_SHUKABASHO:=arrToriCode(2), I_CLASS:=cenergyElNessTankClass)
        End Using

        '〇(帳票)使用項目の設定
        WW_ReportMeisaiAdd(LNT0001tbl)

        '〇陸事番号マスタ設定
        For Each dtCenergyElNessTankrow As DataRow In dtCenergyElNessTank.Rows
            '届先(明細)セル値設定
            Dim condition As String = String.Format("GYOMUTANKNUM='{0}'", dtCenergyElNessTankrow("KEYCODE04"))
            WW_CenergyElnessRikugiMas(dtCenergyElNessTankrow, condition, fuzumiLimit, LNT0001tbl)
        Next

        '〇業務車番(3XX)取得用
        For Each CenergyElNessTankrow As DataRow In dtCenergyElNessTank.Select("KEYCODE04<>''", "KEYCODE04")
            If CenergyElNessTankrow("KEYCODE04").ToString().Substring(0, 1) <> "3" Then Continue For
            Try
                dcCenergyList.Add(CenergyElNessTankrow("KEYCODE04"), CenergyElNessTankrow("KEYCODE01"))
            Catch ex As Exception
            End Try
        Next
        '〇(シーエナジー)届先出荷場所車庫マスタ設定(3XX)
        For Each CenergyTodokerow As DataRow In dtCenergyTodoke.Select("KEYCODE01<>''", "KEYCODE01")
            If CenergyTodokerow("KEYCODE01").ToString().Substring(0, 3) = "TMP" Then Continue For
            Dim condition As String = String.Format("TODOKECODE='{0}'", CenergyTodokerow("KEYCODE01").ToString().Replace(" ", ""))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                If LNT0001tblrow("GYOMUTANKNUM").ToString().Substring(0, 1) <> "3" Then Continue For
                Try
                    LNT0001tblrow("CENERGYELNESS_SHUKACODE") = CenergyTodokerow("KEYCODE07").ToString()
                    LNT0001tblrow("CENERGYELNESS_SHUKANAME") = CenergyTodokerow("KEYCODE08").ToString()
                    LNT0001tblrow("CENERGYELNESS_TODOKECODE") = CenergyTodokerow("KEYCODE03").ToString()
                    LNT0001tblrow("CENERGYELNESS_TODOKENAME") = CenergyTodokerow("KEYCODE04").ToString()
                Catch ex As Exception
                End Try
            Next
        Next
        '〇(シーエナジー)統合版単価マスタ設定(出荷場所)
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(String.Format("TORICODE='{0}'", BaseDllConst.CONST_TORICODE_0110600000), "TODOKECODE")
            If LNT0001tblrow("GYOMUTANKNUM").ToString().Substring(0, 1) <> "3" Then Continue For
            Dim condition As String = ""
            condition &= String.Format(" KASANORGCODE='{0}' ", LNT0001tblrow("KASANCODEORDERORG").ToString())
            condition &= String.Format(" AND AVOCADOSHUKABASHO='{0}' ", LNT0001tblrow("SHUKABASHO").ToString())
            condition &= String.Format(" AND TODOKECODE='{0}' ", LNT0001tblrow("TODOKECODE").ToString())
            condition &= String.Format(" AND SYAGOU='{0}' ", LNT0001tblrow("GYOMUTANKNUM").ToString())
            For Each LNT0001Tanktblrow As DataRow In LNT0001Tanktbl.Select(condition)
                Try
                    LNT0001tblrow("CENERGYELNESS_SHUKACODE") = LNT0001Tanktblrow("SHUKABASHO").ToString()
                    LNT0001tblrow("CENERGYELNESS_SHUKANAME") = LNT0001Tanktblrow("SHUKANAME").ToString()
                Catch ex As Exception
                End Try
            Next
        Next


        '〇業務車番(6XX)取得用
        For Each CenergyElNessTankrow As DataRow In dtCenergyElNessTank.Select("KEYCODE04<>''", "KEYCODE04")
            If CenergyElNessTankrow("KEYCODE04").ToString().Substring(0, 1) <> "6" Then Continue For
            Try
                dcElNessList.Add(CenergyElNessTankrow("KEYCODE04"), CenergyElNessTankrow("KEYCODE01"))
            Catch ex As Exception
            End Try
        Next
        '〇(エルネス)届先出荷場所車庫マスタ設定(6XX)
        For Each ElNessTodokerow As DataRow In dtElNessTodoke.Select("KEYCODE01<>''", "KEYCODE01")
            If ElNessTodokerow("KEYCODE01").ToString().Substring(0, 3) = "TMP" Then Continue For
            Dim condition As String = String.Format("TODOKECODE='{0}'", ElNessTodokerow("KEYCODE01").ToString().Replace(" ", ""))
            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                If LNT0001tblrow("GYOMUTANKNUM").ToString().Substring(0, 1) <> "6" Then Continue For
                Try
                    LNT0001tblrow("CENERGYELNESS_SHUKACODE") = ElNessTodokerow("KEYCODE07").ToString()
                    LNT0001tblrow("CENERGYELNESS_SHUKANAME") = ElNessTodokerow("KEYCODE08").ToString()
                    LNT0001tblrow("CENERGYELNESS_TODOKECODE") = ElNessTodokerow("KEYCODE03").ToString()
                    LNT0001tblrow("CENERGYELNESS_TODOKENAME") = ElNessTodokerow("KEYCODE04").ToString()
                Catch ex As Exception
                End Try
            Next
        Next
        '〇(エルネス)統合版単価マスタ設定(出荷場所)
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(String.Format("TORICODE='{0}'", BaseDllConst.CONST_TORICODE_0238900000), "TODOKECODE")
            If LNT0001tblrow("GYOMUTANKNUM").ToString().Substring(0, 1) <> "6" Then Continue For
            Dim condition As String = ""
            condition &= String.Format(" KASANORGCODE='{0}' ", LNT0001tblrow("KASANCODEORDERORG").ToString())
            condition &= String.Format(" AND AVOCADOSHUKABASHO='{0}' ", LNT0001tblrow("SHUKABASHO").ToString())
            condition &= String.Format(" AND TODOKECODE='{0}' ", LNT0001tblrow("TODOKECODE").ToString())
            condition &= String.Format(" AND SYAGOU='{0}' ", LNT0001tblrow("GYOMUTANKNUM").ToString())
            For Each LNT0001Tanktblrow As DataRow In LNT0001Tanktbl.Select(condition)
                Try
                    LNT0001tblrow("CENERGYELNESS_SHUKACODE") = LNT0001Tanktblrow("SHUKABASHO").ToString()
                    LNT0001tblrow("CENERGYELNESS_SHUKANAME") = LNT0001Tanktblrow("SHUKANAME").ToString()
                Catch ex As Exception
                End Try
            Next
        Next

    End Sub

    ''' <summary>
    ''' シーエナジー・エルネス(届先(明細)セル値設定)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_CenergyElnessRikugiMas(ByVal dtCenergyElnessTankrow As DataRow, ByVal condition As String, ByVal fuzumiLimit As Decimal, ByRef LNT0001tbl As DataTable)
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
            '★届日より日を取得(セル(行数)の設定のため)
            Dim setDay As String = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
            Dim lastMonth As Boolean = False
            If Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("yyyy/MM") = Date.Parse(TaishoYm + "/01").AddMonths(-1).ToString("yyyy/MM") Then
                setDay = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
                lastMonth = True
            End If
            Dim iLine As Integer = Integer.Parse(setDay) - 1
            iLine = (iLine * Integer.Parse(dtCenergyElnessTankrow("VALUE06"))) + Integer.Parse(dtCenergyElnessTankrow("VALUE05"))
            ''★トリップより位置を取得
            'Dim iTrip As Integer = Integer.Parse(LNT0001tblrow("TRIP_REP").ToString()) - 1
            'iTrip += iLine
            LNT0001tblrow("ROWSORTNO") = dtCenergyElnessTankrow("VALUE01")
            LNT0001tblrow("SETCELL01") = dtCenergyElnessTankrow("VALUE02")
            LNT0001tblrow("SETCELL02") = dtCenergyElnessTankrow("VALUE03")
            LNT0001tblrow("SETCELL03") = dtCenergyElnessTankrow("VALUE04")
            LNT0001tblrow("SETCELL04") = dtCenergyElnessTankrow("VALUE09")
            LNT0001tblrow("SETCELL05") = dtCenergyElnessTankrow("VALUE10")
            'LNT0001tblrow("SETCELL01") = dtCenergyElnessTankrow("VALUE02") + iLine.ToString()
            'LNT0001tblrow("SETCELL02") = dtCenergyElnessTankrow("VALUE03") + iLine.ToString()
            'LNT0001tblrow("SETCELL03") = dtCenergyElnessTankrow("VALUE04") + iLine.ToString()
            'LNT0001tblrow("SETCELL04") = dtCenergyElnessTankrow("VALUE09") + iLine.ToString()
            'LNT0001tblrow("SETCELL05") = dtCenergyElnessTankrow("VALUE10") + iLine.ToString()
            LNT0001tblrow("SETSTARTLINE") = dtCenergyElnessTankrow("VALUE05")
            LNT0001tblrow("SETLINE") = iLine
            LNT0001tblrow("ORDERORGCODE_REP") = dtCenergyElnessTankrow("KEYCODE05")
            LNT0001tblrow("GYOMUTANKNUM_REP") = dtCenergyElnessTankrow("KEYCODE04")
            LNT0001tblrow("ROLLY_CONTAINER") = dtCenergyElnessTankrow("KEYCODE03")

            '★表示セルフラグ(1:表示)
            If dtCenergyElnessTankrow("VALUE07").ToString() = "1" Then
                LNT0001tblrow("DISPLAYCELL_START") = dtCenergyElnessTankrow("VALUE02").ToString()
                LNT0001tblrow("DISPLAYCELL_END") = dtCenergyElnessTankrow("VALUE10").ToString()
                LNT0001tblrow("DISPLAYCELL_KOTEICHI") = dtCenergyElnessTankrow("VALUE08").ToString()
            Else
                LNT0001tblrow("DISPLAYCELL_START") = ""
                LNT0001tblrow("DISPLAYCELL_END") = ""
                LNT0001tblrow("DISPLAYCELL_KOTEICHI") = ""
            End If

            '★備考設定用(出荷日と届日が不一致の場合)
            If LNT0001tblrow("SHUKADATE").ToString() <> LNT0001tblrow("TODOKEDATE").ToString() Then
                If lastMonth = True Then
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("M/d") + "積"
                Else
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("M/d") + "届"
                End If
            End If

        Next
    End Sub
#End Region

#Region "北海道LNG"
    ''' <summary>
    ''' (帳票)項目チェック処理(北海道LNG)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_ReportCheckHokaidoLNG(ByVal reportName As String, ByVal reportCode As String,
                                        ByRef dcHokaidoLNGList As Dictionary(Of String, String),
                                        ByRef LNT0001tbl As DataTable, ByRef LNT0001Tanktbl As DataTable, ByRef LNT0001Koteihi As DataTable, ByRef LNT0001KihonFeeA As DataTable,
                                        ByRef LNT0001TogouSprate As DataTable, ByRef LNT0001Calendar As DataTable, ByRef LNT0001HolidayRate As DataTable, ByRef LNT0001HolidayRateNum As DataTable)

        Dim dtHokkaidoLNGTank As New DataTable
        Dim dtHokkaidoLNGTodoke As New DataTable
        Dim sekiyuHokkaidoTankLNGClass As String = ""
        Dim sekiyuHokkaidoTodokeLNGClass As String = ""
        Dim sekiyuHokkaidoKoteihiLNGClass As String = ""
        Dim arrToriCode As String() = {"", "", ""}
        Dim fuzumiLimit As Decimal = 1.7                    '--★不積(しきい値)

        Select Case reportCode
            '"北海道LNG　輸送費請求書"
            Case BaseDllConst.CONST_ORDERORGCODE_020104
                sekiyuHokkaidoTankLNGClass = "HOKKAIDO_LNG_TANK"
                sekiyuHokkaidoTodokeLNGClass = "HOKKAIDO_LNG_TODOKE"
                sekiyuHokkaidoKoteihiLNGClass = "HOKKAIDO_LNG_KOTEIHI"
                arrToriCode(0) = BaseDllConst.CONST_TORICODE_0239900000
                arrToriCode(1) = BaseDllConst.CONST_ORDERORGCODE_020104
                arrToriCode(2) = Nothing
        End Select

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            CMNPTS.SelectCONVERTMaster(SQLcon, sekiyuHokkaidoTankLNGClass, dtHokkaidoLNGTank)
            CMNPTS.SelectCONVERTMaster(SQLcon, sekiyuHokkaidoTodokeLNGClass, dtHokkaidoLNGTodoke)
            CMNPTS.SelectNEWTANKAMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm + "/01", sekiyuHokkaidoTodokeLNGClass, LNT0001Tanktbl)
            CMNPTS.SelectFIXEDMaster(SQLcon, arrToriCode(0), arrToriCode(1), TaishoYm.Replace("/", ""), LNT0001Koteihi, I_CLASS:=sekiyuHokkaidoTankLNGClass)
            CMNPTS.SelectIntegrationSprateFEEMaster(SQLcon, arrToriCode(0), TaishoYm.Replace("/", ""), LNT0001TogouSprate, I_ORGCODE:=arrToriCode(1), I_CLASS:=sekiyuHokkaidoKoteihiLNGClass)
            CMNPTS.SelectHokkaidoLNG_YusouhiKihonFeeA(sekiyuHokkaidoKoteihiLNGClass, arrToriCode(0), arrToriCode(1), TaishoYm.Replace("/", ""), LNT0001KihonFeeA)

            CMNPTS.SelectCALENDARMaster(SQLcon, arrToriCode(0), TaishoYm + "/01", LNT0001Calendar)
            CMNPTS.SelectHOLIDAYRATEMaster(SQLcon, arrToriCode(0), LNT0001HolidayRate, I_dtTODOKEMas:=dtHokkaidoLNGTodoke, I_CLASS:=sekiyuHokkaidoKoteihiLNGClass)
            CMNPTS.SelectHokkaidoLNG_YusouhiHolidayRate(arrToriCode(0), TaishoYm + "/01", LNT0001HolidayRateNum)
        End Using

        '〇(帳票)使用項目の設定
        WW_ReportMeisaiAdd(LNT0001tbl)

        '〇陸事番号マスタ設定
        For Each dtHokkaidoLNGTankrow As DataRow In dtHokkaidoLNGTank.Rows
            '★届日より日を取得(セル(行数)の設定のため)
            Dim condition As String = String.Format("GYOMUTANKNUM='{0}'", dtHokkaidoLNGTankrow("KEYCODE04"))
            WW_HokkaidoLNGRikugiMas(dtHokkaidoLNGTankrow, condition, fuzumiLimit, LNT0001tbl)
        Next

        '〇(北海道LNG)届先出荷場所車庫マスタ設定
        For Each dtHokkaidoLNGTodokerow As DataRow In dtHokkaidoLNGTodoke.Rows
            Dim condition As String = String.Format("TODOKECODE='{0}'", dtHokkaidoLNGTodokerow("KEYCODE01"))
            '★特殊届先(条件)チェック([雪印メグ中標津][大塚製薬工場])
            If dtHokkaidoLNGTodokerow("KEYCODE01") = BaseDllConst.CONST_TODOKECODE_003630 _
                    OrElse dtHokkaidoLNGTodokerow("KEYCODE01") = BaseDllConst.CONST_TODOKECODE_007279 Then
                condition &= String.Format(" AND SHUKABASHO = '{0}' ", dtHokkaidoLNGTodokerow("KEYCODE04"))
            End If

            For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)
                LNT0001tblrow("SHEETSORTNO_REP") = dtHokkaidoLNGTodokerow("KEYCODE03")
                LNT0001tblrow("TODOKENAME_REP") = dtHokkaidoLNGTodokerow("VALUE01")
                LNT0001tblrow("SHEETNAME_REP") = dtHokkaidoLNGTodokerow("VALUE06")

                '〇届先が追加された場合
                If dtHokkaidoLNGTodokerow("VALUE02").ToString() = "1" Then
                    LNT0001tblrow("TODOKECELL_REP") = dtHokkaidoLNGTodokerow("VALUE03")
                    LNT0001tblrow("MASTERCELL_REP") = dtHokkaidoLNGTodokerow("VALUE04")
                    LNT0001tblrow("SHEETDISPLAY_REP") = dtHokkaidoLNGTodokerow("VALUE05")
                Else
                    LNT0001tblrow("TODOKECELL_REP") = ""
                    LNT0001tblrow("MASTERCELL_REP") = ""
                    LNT0001tblrow("SHEETDISPLAY_REP") = ""
                End If
            Next
        Next

        'シート名取得用
        For Each dtHokkaidoLNGTodokerow As DataRow In dtHokkaidoLNGTodoke.Select("", "KEYCODE01")
            If dtHokkaidoLNGTodokerow("KEYCODE01").ToString() = "" Then Continue For
            Try
                dcHokaidoLNGList.Add(dtHokkaidoLNGTodokerow("KEYCODE01"), dtHokkaidoLNGTodokerow("VALUE06"))
            Catch ex As Exception
                '★特殊届先チェック([雪印メグ中標津][大塚製薬工場])
                If dtHokkaidoLNGTodokerow("KEYCODE01") = BaseDllConst.CONST_TODOKECODE_003630 _
                    OrElse dtHokkaidoLNGTodokerow("KEYCODE01") = BaseDllConst.CONST_TODOKECODE_007279 Then
                    dcHokaidoLNGList.Add(dtHokkaidoLNGTodokerow("KEYCODE01") + "02", dtHokkaidoLNGTodokerow("VALUE06"))
                End If
            End Try
        Next

    End Sub

    ''' <summary>
    ''' 北海道LNG(届先(明細)セル値設定)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_HokkaidoLNGRikugiMas(ByVal dtHokkaidoLNGTankrow As DataRow, ByVal condition As String, ByVal fuzumiLimit As Decimal, ByRef LNT0001tbl As DataTable)
        For Each LNT0001tblrow As DataRow In LNT0001tbl.Select(condition)

            '★釧路配属車の場合(出荷場所が[釧路エルエヌジー](006826))チェック
            If dtHokkaidoLNGTankrow("KEYCODE07").ToString() = "3" _
                AndAlso dtHokkaidoLNGTankrow("KEYCODE04").ToString() = "3308" Then
                If LNT0001tblrow("SHUKABASHO").ToString() = "006826" Then
                    '### 釧路配属車の場合は、下記処理を実施
                Else
                    Continue For
                End If
            End If

            '★届日より日を取得(セル(行数)の設定のため)
            Dim setDay As String = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
            Dim lastMonth As Boolean = False
            If Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("yyyy/MM") = Date.Parse(TaishoYm + "/01").AddMonths(-1).ToString("yyyy/MM") Then
                setDay = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("dd")
                lastMonth = True
            End If
            Dim iLine As Integer = Integer.Parse(setDay) - 1
            iLine = (iLine * Integer.Parse(dtHokkaidoLNGTankrow("VALUE06"))) + Integer.Parse(dtHokkaidoLNGTankrow("VALUE05"))
            ''★トリップより位置を取得
            'Dim iTrip As Integer = Integer.Parse(LNT0001tblrow("TRIP_REP").ToString()) - 1
            'iTrip += iLine
            LNT0001tblrow("ROWSORTNO") = dtHokkaidoLNGTankrow("VALUE01")
            '★室蘭ガスの場合(専用セルに設定)
            If LNT0001tblrow("TODOKECODE") = BaseDllConst.CONST_TODOKECODE_004831 Then
                '※ただし、ナンバーが室蘭の場合のみ(専用セル)
                If LNT0001tblrow("TANKNUMBER").ToString().Substring(0, 2) = "室蘭" Then
                    LNT0001tblrow("SETCELL") = dtHokkaidoLNGTankrow("VALUE03")
                    LNT0001tblrow("SETCELL01") = dtHokkaidoLNGTankrow("VALUE03") + iLine.ToString()
                Else
                    LNT0001tblrow("SETCELL") = dtHokkaidoLNGTankrow("VALUE02")
                    LNT0001tblrow("SETCELL01") = dtHokkaidoLNGTankrow("VALUE02") + iLine.ToString()
                End If
            Else
                LNT0001tblrow("SETCELL") = dtHokkaidoLNGTankrow("VALUE02")
                LNT0001tblrow("SETCELL01") = dtHokkaidoLNGTankrow("VALUE02") + iLine.ToString()
            End If
            'LNT0001tblrow("SETCELL02") = dtHokkaidoLNGTankrow("VALUE03") + iLine.ToString()
            'LNT0001tblrow("SETCELL03") = dtHokkaidoLNGTankrow("VALUE04") + iLine.ToString()
            LNT0001tblrow("SETLINE") = iLine.ToString()

            '★表示セルフラグ(1:表示)
            LNT0001tblrow("DISPLAYCELL_START") = dtHokkaidoLNGTankrow("VALUE02").ToString()
            LNT0001tblrow("DISPLAYCELL_END") = dtHokkaidoLNGTankrow("VALUE02").ToString()
            'LNT0001tblrow("DISPLAYCELL_KOTEICHI") = dtHokkaidoLNGTankrow("VALUE08").ToString()

            '★備考設定用(出荷日と届日が不一致の場合)
            If LNT0001tblrow("SHUKADATE").ToString() <> LNT0001tblrow("TODOKEDATE").ToString() Then
                If lastMonth = True Then
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("SHUKADATE").ToString()).ToString("M/d") + "積"
                Else
                    LNT0001tblrow("REMARK_REP") = Date.Parse(LNT0001tblrow("TODOKEDATE").ToString()).ToString("M/d") + "届"
                End If
            End If
        Next
    End Sub

#End Region

    Protected Sub WW_ReportMeisaiAdd(ByRef LNT0001tbl As DataTable)
        '〇(帳票)使用項目の設定
        LNT0001tbl.Columns.Add("ROWSORTNO", Type.GetType("System.Int32"))               '// 【入力用】EXCEL用ソート番号
        LNT0001tbl.Columns.Add("SETCELL", Type.GetType("System.String"))                '// 【入力用】EXCEL用セル
        LNT0001tbl.Columns.Add("SETCELL01", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(届先名)
        LNT0001tbl.Columns.Add("SETCELL02", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(実績数量)
        LNT0001tbl.Columns.Add("SETCELL03", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(備考)
        LNT0001tbl.Columns.Add("SETCELL04", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(予備)
        LNT0001tbl.Columns.Add("SETCELL05", Type.GetType("System.String"))              '// 【入力用】EXCEL用セル(予備)
        LNT0001tbl.Columns.Add("SETSTARTLINE", Type.GetType("System.Int32"))            '// 【入力用】EXCEL用(開始行)
        LNT0001tbl.Columns.Add("SETLINE", Type.GetType("System.Int32"))                 '// 【入力用】EXCEL用(行数)
        LNT0001tbl.Columns.Add("TODOKENAME_REP", Type.GetType("System.String"))         '// 【入力用】EXCEL用(届先名)
        LNT0001tbl.Columns.Add("REMARK_REP", Type.GetType("System.String"))             '// 【入力用】EXCEL用(備考)
        LNT0001tbl.Columns.Add("DISPLAYCELL_START", Type.GetType("System.String"))      '// 【入力用】EXCEL用(陸事番号)設定用
        LNT0001tbl.Columns.Add("DISPLAYCELL_END", Type.GetType("System.String"))        '// 【入力用】EXCEL用(受注数量)設定用
        LNT0001tbl.Columns.Add("DISPLAYCELL_KOTEICHI", Type.GetType("System.String"))   '// 【固定費】EXCEL用(陸事番号)表示用
        LNT0001tbl.Columns.Add("TODOKECELL_REP", Type.GetType("System.String"))         '// 【届先毎】EXCEL用(届先名)表示用
        LNT0001tbl.Columns.Add("MASTERCELL_REP", Type.GetType("System.String"))         '// 【マスタ】EXCEL用(届先名)表示用
        LNT0001tbl.Columns.Add("ORDERORGCODE_REP", Type.GetType("System.String"))       '// EXCELシート(受注受付部署コード)設定用
        LNT0001tbl.Columns.Add("GYOMUTANKNUM_REP", Type.GetType("System.String"))       '// EXCELシート(業務車番)設定用
        LNT0001tbl.Columns.Add("SHEETDISPLAY_REP", Type.GetType("System.String"))       '// EXCELシート(届先名)表示用
        LNT0001tbl.Columns.Add("SHEETSORTNO_REP", Type.GetType("System.Int32"))         '// EXCELシート(届先名)ソート用
        LNT0001tbl.Columns.Add("SHEETNAME_REP", Type.GetType("System.String"))          '// EXCELシート(届先名)設定用
        LNT0001tbl.Columns.Add("GROUPNO_REP", Type.GetType("System.String"))            '// EXCELシート(届先GRP)設定用
        LNT0001tbl.Columns.Add("ZISSEKI_FUZUMI", Type.GetType("System.Decimal"))        '// EXCELシート①(車腹 - 不積(しきい値))設定用
        LNT0001tbl.Columns.Add("FUZUMI_REFVALUE", Type.GetType("System.Decimal"))       '// EXCELシート②(① - 実績数量)設定用
        LNT0001tbl.Columns.Add("ZISSEKI_FUZUMIFLG", Type.GetType("System.String"))      '// EXCELシート(不積フラグ)設定用
        LNT0001tbl.Columns.Add("ROLLY_CONTAINER", Type.GetType("System.String"))        '// EXCELシート(ローリー・コンテナ)設定用
        LNT0001tbl.Columns.Add("CENERGYELNESS_SHUKACODE", Type.GetType("System.Int32"))      '// EXCELシート(シーエナジー・エルネス)出荷コード設定用
        'LNT0001tbl.Columns.Add("CENERGYELNESS_SHUKACODE", Type.GetType("System.String"))      '// EXCELシート(シーエナジー・エルネス)出荷コード設定用
        LNT0001tbl.Columns.Add("CENERGYELNESS_SHUKANAME", Type.GetType("System.String"))      '// EXCELシート(シーエナジー・エルネス)出荷名　称設定用
        LNT0001tbl.Columns.Add("CENERGYELNESS_TODOKECODE", Type.GetType("System.Int32"))     '// EXCELシート(シーエナジー・エルネス)届先コード設定用
        'LNT0001tbl.Columns.Add("CENERGYELNESS_TODOKECODE", Type.GetType("System.String"))     '// EXCELシート(シーエナジー・エルネス)届先コード設定用
        LNT0001tbl.Columns.Add("CENERGYELNESS_TODOKENAME", Type.GetType("System.String"))     '// EXCELシート(シーエナジー・エルネス)届先名　称設定用
    End Sub

End Class
