''************************************************************
' 支店別締め処理画面
' 作成日     ：2023/03/31
' 作成者     ：星
' 最終更新日 ：2024/09/04
' 最終更新者 ：名取
' バージョン ：ver2
' 
' 修正履歴：2024/09/04 ver2 名取 経理連携不可理由の画面表示対応
''************************************************************

Option Strict On
Option Explicit On

Imports MySQL.Data.MySqlClient
Imports System.IO
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0019ClosingBranch
    Inherits System.Web.UI.Page

#Region "定数・変数"
    '○ 検索結果格納Table
    Private LNT0019tbl As DataTable                                 '一覧格納用テーブル

    '〇変数
    Dim GBL_MISYO_FLG As Boolean = True                             '未承認存在フラグ（回送費を除き未承認存在の場合ダウンロードボタン押下不可）
    Dim GBL_PAYMENTMISYO_FLG As Boolean = True                      '回送費未承認存在フラグ（回送費を含め未承認存在の場合確定ボタン押下不可）
    Dim GBL_TOTAL As Integer = 0                                    '合計（0の場合のみダウンロードボタン押下可）
    Dim GBL_KAKUTEI_FLG As Integer = 0                              '確定ボタン押下後ダウンロード不可
    Dim GBL_SEACH_FLG As String = "0"                               '検索ボタン押下フラグ

    '色
    Private COLOR_DETAIL As System.Drawing.Color = System.Drawing.Color.White
    Private COLOR_SUBTTL As System.Drawing.Color = System.Drawing.Color.FromArgb(221, 235, 247)
    Private COLOR_TOTAL As System.Drawing.Color = System.Drawing.Color.FromArgb(149, 179, 215)

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RTN_SW As String = ""
    Private WW_DUMMY As String = ""

    ' 2024/09/04 ver2 名取 ADD START
    ' 経理連携不可メッセージ
    Private WW_RENTAL_MISHONIN_MES As String = "レンタル使用料"
    Private WW_LEASE_MISHONIN_MES As String = "リース料"
    Private WW_WRITE_MISHONIN_MES As String = "手書き請求書"
    Private WW_CTNSALE_MISHONIN_MES As String = "コンテナ売却"
    Private WW_MISHONIN_MES As String = "に未承認が残っています。"
    Private WW_CTNSALECALCULATION_MES As String = "コンテナ売却に原価未計算が残っています。"
    ' 2024/09/04 ver2 名取 ADD END
#End Region

#Region "メイン処理（初期化処理）"

    ''' <summary>
    ''' ページロード処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            ' 2024/09/04 ver2 名取 ADD START
            WF_SHONIN_LABEL.Text = ""
            WF_GENKA_LABEL.Text = ""
            ' 2024/09/04 ver2 名取 ADD END

            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then

                    '◯ フラグ初期化
                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonEND"                     '戻るボタン押下
                            Me.WF_ButtonEND_Click()
                        Case "txtDownloadMonth"                 '年月変更
                            Me.WF_Seach_CTN()
                        Case "WF_CSV_DL"                        'ダウンロードボタン押下
                            Me.WF_CsvDownload()
                        Case "WF_CLOSTA"                        '確定・未確定の押下
                            '確定・未確定ダイアログ
                            Master.Output(C_MESSAGE_NO.CTN_CONFIRM_CHK, C_MESSAGE_TYPE.QUES, I_PARA01:="", I_PARA02:="Q",
                                          needsPopUp:=True, messageBoxTitle:="確認", IsConfirm:=True, YesButtonId:="btnConfirmOK",
                                          needsConfirmNgToPostBack:=True, NoButtonId:="btnConfirmNG")
                        Case "btnConfirmOK"                     'ダイアログはい押下
                            Me.Save_SearchItemOK()
                        Case "btnConfirmNG"                     'ダイアログいいえ押下
                            Me.Save_SearchItemNG()
                    End Select
                End If
            Else
                '○ 初期化処理
                Me.Initialize()
            End If

        Finally
            '○ 格納Table Close
            If Not IsNothing(LNT0019tbl) Then
                LNT0019tbl.Clear()
                LNT0019tbl.Dispose()
                LNT0019tbl = Nothing
            End If

#Region "ボタン制御"
            'ダウンロードできるか判断
            If Not GBL_MISYO_FLG Then
                WF_CSVDLDisabledFlg.Value = "1"
                If Not GBL_PAYMENTMISYO_FLG Then
                    Me.WF_CLOSTA.SelectEnabledItem("0", False)
                    Me.WF_CLOSTA.SelectEnabledItem("1", False)
                End If
            ElseIf GBL_MISYO_FLG Then
                WF_CSVDLDisabledFlg.Value = "0"
                If Not GBL_PAYMENTMISYO_FLG Then
                    Me.WF_CLOSTA.SelectEnabledItem("0", False)
                    Me.WF_CLOSTA.SelectEnabledItem("1", False)
                Else
                    Me.WF_CLOSTA.SelectEnabledItem("0", True)
                    Me.WF_CLOSTA.SelectEnabledItem("1", True)
                End If
            End If
            '確定ボタン押下後ダウンロード不可
            If GBL_KAKUTEI_FLG = 1 OrElse
                WF_CSVDLDisabledFlg.Value = "1" Then
                WF_CSVDLDisabledFlg.Value = "1"
            ElseIf GBL_KAKUTEI_FLG = 0 Then
                WF_CSVDLDisabledFlg.Value = "0"
            End If
            If GBL_SEACH_FLG = "1" Then
                '0件の場合ダウンロード押下不可
                If GBL_TOTAL = 0 Then
                    WF_CSVDLDisabledFlg.Value = "1"
                    Me.WF_CLOSTA.SelectEnabledItem("0", False)
                    Me.WF_CLOSTA.SelectEnabledItem("1", False)
                End If
            End If
#End Region

            ' 2024/09/04 ver2 名取 ADD START
#Region "経理連携不可理由メッセージ設定"
            If WF_Hokkaido0.Value = "0" OrElse WF_Touhoku0.Value = "0" OrElse WF_Kantou0.Value = "0" OrElse
                WF_Tyubu0.Value = "0" OrElse WF_Kansai0.Value = "0" OrElse WF_Kyusyu0.Value = "0" OrElse WF_CTN0.Value = "0" Then
                WF_SHONIN_LABEL.Text = WW_RENTAL_MISHONIN_MES
            End If
            If WF_Hokkaido1.Value = "0" OrElse WF_Touhoku1.Value = "0" OrElse WF_Kantou1.Value = "0" OrElse
                WF_Tyubu1.Value = "0" OrElse WF_Kansai1.Value = "0" OrElse WF_Kyusyu1.Value = "0" OrElse WF_CTN1.Value = "0" Then
                If WF_SHONIN_LABEL.Text = "" Then
                    WF_SHONIN_LABEL.Text = WW_LEASE_MISHONIN_MES
                Else
                    WF_SHONIN_LABEL.Text &= "、" & WW_LEASE_MISHONIN_MES
                End If
            End If
            If WF_Hokkaido2.Value = "0" OrElse WF_Touhoku2.Value = "0" OrElse WF_Kantou2.Value = "0" OrElse
                WF_Tyubu2.Value = "0" OrElse WF_Kansai2.Value = "0" OrElse WF_Kyusyu2.Value = "0" OrElse WF_CTN2.Value = "0" Then
                If WF_SHONIN_LABEL.Text = "" Then
                    WF_SHONIN_LABEL.Text = WW_WRITE_MISHONIN_MES
                Else
                    WF_SHONIN_LABEL.Text &= "、" & WW_WRITE_MISHONIN_MES
                End If
            End If
            If WF_Hokkaido3.Value = "0" OrElse WF_Touhoku3.Value = "0" OrElse WF_Kantou3.Value = "0" OrElse
                WF_Tyubu3.Value = "0" OrElse WF_Kansai3.Value = "0" OrElse WF_Kyusyu3.Value = "0" OrElse WF_CTN3.Value = "0" Then
                If WF_SHONIN_LABEL.Text = "" Then
                    WF_SHONIN_LABEL.Text = WW_CTNSALE_MISHONIN_MES
                Else
                    WF_SHONIN_LABEL.Text &= "、" & WW_CTNSALE_MISHONIN_MES
                End If
            End If
            If WF_SHONIN_LABEL.Text <> "" Then
                WF_SHONIN_LABEL.Text &= WW_MISHONIN_MES
            End If
            If WF_Hokkaido5.Value = "0" OrElse WF_Touhoku5.Value = "0" OrElse WF_Kantou5.Value = "0" OrElse
                WF_Tyubu5.Value = "0" OrElse WF_Kansai5.Value = "0" OrElse WF_Kyusyu5.Value = "0" OrElse WF_CTN5.Value = "0" Then
                If WF_SHONIN_LABEL.Text = "" Then
                    WF_SHONIN_LABEL.Text = WW_CTNSALECALCULATION_MES
                Else
                    WF_GENKA_LABEL.Text = WW_CTNSALECALCULATION_MES
                End If
            End If
#End Region
            ' 2024/09/04 ver2 名取 ADD END

        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Initialize()

        Const INIT_ERR_PROC As String = "LNT0019D Initialize"

        '一覧データ格納変数
        Dim dtInvHistListData = New DataTable
        Dim dtLeasefeeListData = New DataTable
        Dim dtRentalListData = New DataTable

        Try
            '○HELP表示有無設定
            Master.dispHelp = False
            '○D&D有無設定
            Master.eventDrop = True
            '○Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()

            '○初期値設定
            '共通
            WF_FIELD.Value = ""
            WF_ButtonClick.Value = ""
            WF_LeftboxOpen.Value = ""
            WF_RightboxOpen.Value = ""
            rightview.ResetIndex()
            leftview.ActiveListBox()

            '右Boxへの値設定
            rightview.MAPID = Master.MAPID
            rightview.MAPVARI = Master.MAPvariant
            rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
            rightview.PROFID = Master.PROF_REPORT
            rightview.Initialize(WW_DUMMY)

            Me.InitClosta()

            '○ 画面の値設定
            Me.WW_MAPValueSet()

        Catch sqlex As MySqlException
            Master.Output(C_MESSAGE_NO.CTN_INITIAL_ERROR, C_MESSAGE_TYPE.ABORT, "DBエラー " + INIT_ERR_PROC, , True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "SYS:" & INIT_ERR_PROC
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = sqlex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_INITIAL_ERROR, C_MESSAGE_TYPE.ABORT, INIT_ERR_PROC, , True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "SYS:" & INIT_ERR_PROC
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub WW_MAPValueSet()

        Dim keijoym As String = getKeijoYM() & "01"
        Me.txtDownloadMonth.Text = Format(DateTime.ParseExact(keijoym, "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None), "yyyy/MM")

        'レンタル使用料取得
        Get_Rental()
        'リース料取得処理
        Get_Lease()
        '手書き請求書処理
        Get_Write()
        'コンテナ売却取得処理
        Get_CtnSale()
        'コンテナ売却(原価計算)取得処理
        Get_CtnSaleCalculation()
        '支払料取得処理
        Get_Payment()

        '検索フラグ
        GBL_SEACH_FLG = "1"

        '締め状態検索
        Dim dt As DataTable = Select_Close()

        If dt.Rows.Count = 0 Then
            '締め状態テーブルにないとき未確定にチェック
            WF_CLOSTA.SelectSingleItem("0")
            WF_CloseFLG.Value = "0"
            GBL_KAKUTEI_FLG = 0
        ElseIf dt.Rows.Count = 1 Then

            If dt.Select("CLOSESTATUS = 0").Count = 0 Then
                '確定状態の場合確定にチェック
                WF_CLOSTA.SelectSingleItem("1")
                WF_CloseFLG.Value = "1"
                GBL_KAKUTEI_FLG = 1

            ElseIf dt.Select("CLOSESTATUS = 0").Count = 1 Then
                '未確定状態の場合未確定にチェック
                WF_CLOSTA.SelectSingleItem("0")
                WF_CloseFLG.Value = "0"
                GBL_KAKUTEI_FLG = 0

            End If

        End If

    End Sub

    ''' <summary>
    ''' 締め種別初期化
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub InitClosta()

        WF_CLOSTA.SelectionMode = ListSelectionMode.[Single]
        WF_CLOSTA.NeedsPostbackAfterSelect = True

        Dim dt As DataTable = CmnLNG.GetFixValueTbl(Master.USERCAMP, "CLOSESTATUS")
        WF_CLOSTA.SetTileValues(dt)

    End Sub

#End Region

#Region "イベント処理"

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub WF_ButtonEND_Click()

        '前ページ遷移
        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' テキストボックス変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub WF_Seach_CTN()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        'リセット
        GBL_MISYO_FLG = True

        'レンタル使用料取得
        Get_Rental()
        'リース料取得処理
        Get_Lease()
        '手書き請求書処理
        Get_Write()
        'コンテナ売却取得処理
        Get_CtnSale()
        'コンテナ売却(原価計算)取得処理
        Get_CtnSaleCalculation()
        '支払料取得処理
        Get_Payment()

        '検索フラグ
        GBL_SEACH_FLG = "1"

        '締め状態検索
        Dim dt As DataTable = Select_Close()

        If dt.Rows.Count = 0 Then
            '締め状態テーブルにないとき未確定にチェック
            WF_CLOSTA.SelectSingleItem("0")
            WF_CloseFLG.Value = "0"
            GBL_KAKUTEI_FLG = 0
        ElseIf dt.Rows.Count = 1 Then
            If dt.Select("CLOSESTATUS = 0").Count = 0 Then
                '確定状態の場合確定にチェック
                WF_CLOSTA.SelectSingleItem("1")
                WF_CloseFLG.Value = "1"
                GBL_KAKUTEI_FLG = 1

            ElseIf dt.Select("CLOSESTATUS = 0").Count = 1 Then
                '未確定状態の場合未確定にチェック
                WF_CLOSTA.SelectSingleItem("0")
                WF_CloseFLG.Value = "0"
                GBL_KAKUTEI_FLG = 0

            End If

        End If

    End Sub

    ''' <summary>
    ''' ダウンロードボタン押下時処理時処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub WF_CsvDownload()

        '○ エラーレポート準備
        rightview.SetErrorReport("")
        '〇 請求ヘッダーデータDB更新
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続
            '経理連携
            WF_AccountingRenkei(SQLcon)

        End Using

        '支払料取得処理
        Get_Payment()

    End Sub

    ''' <summary>
    ''' 確認・未確認取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Save_SearchItemOK()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '会社コード
        work.WF_SEL_CAMPCODE.Text = Master.USERCAMP
        '状況
        Dim strSelStatus As String = ""
        Dim selList As ListBox = CType(Me.WF_CLOSTA.GetSelectedListData, ListBox)
        'チェックされている項目を取得
        For intCnt As Integer = 0 To selList.Items.Count - 1
            If intCnt > 0 Then
                strSelStatus = strSelStatus & ","
            End If
            strSelStatus = strSelStatus & "'" & CStr(selList.Items(intCnt).Value) & "'"
        Next

        If strSelStatus = "'0'" Then
            Me.WF_UnConfirmed()
            WF_CloseFLG.Value = "0"
            GBL_KAKUTEI_FLG = 0
        ElseIf strSelStatus = "'1'" Then
            Me.WF_Confirmed()
            WF_CloseFLG.Value = "1"
            'Me.AddDate()
            GBL_KAKUTEI_FLG = 1
        End If

    End Sub

    ''' <summary>
    ''' 確認・未確認取得
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Save_SearchItemNG()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '会社コード
        work.WF_SEL_CAMPCODE.Text = Master.USERCAMP
        '状況
        Dim strSelStatus As String = ""
        Dim selList As ListBox = CType(Me.WF_CLOSTA.GetSelectedListData, ListBox)
        'チェックされている項目を取得
        For intCnt As Integer = 0 To selList.Items.Count - 1
            If intCnt > 0 Then
                strSelStatus = strSelStatus & ","
            End If
            strSelStatus = strSelStatus & "'" & CStr(selList.Items(intCnt).Value) & "'"
        Next

        'チェック状態を戻す
        If strSelStatus = "'0'" Then
            WF_CLOSTA.SelectSingleItem("1")
            WF_CloseFLG.Value = "1"
            GBL_KAKUTEI_FLG = 1
        ElseIf strSelStatus = "'1'" Then
            WF_CLOSTA.SelectSingleItem("0")
            WF_CloseFLG.Value = "0"
            GBL_KAKUTEI_FLG = 0
        End If

    End Sub

    ''' <summary>
    ''' テキストボックスを進める
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub AddDate()

        Dim txtDate As String = txtDownloadMonth.Text
        Dim addtxtDate As Date = CDate(txtDate)

        addtxtDate = addtxtDate.AddMonths(1)

        txtDownloadMonth.Text = Format(addtxtDate, "yyyy/MM")

        '再表示
        Me.WF_Seach_CTN()

    End Sub

    ''' <summary>
    ''' 未確定押下時処理時処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub WF_UnConfirmed()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '更新処理
        Update_Close("0")

        '取得件数が0件ではないことを確認するため
        'レンタル使用料取得
        Get_Rental()
        'リース料取得処理
        Get_Lease()
        '手書き請求書処理
        Get_Write()
        'コンテナ売却取得処理
        Get_CtnSale()
        'コンテナ売却(原価計算)取得処理
        Get_CtnSaleCalculation()
        '支払料取得処理
        Get_Payment()

        '■リースデータ未計上・計上済 更新処理(ストアド実行)
        UpdStoredLeaseSime(Replace(Me.txtDownloadMonth.Text, "/", ""), "0", Master.USERID, Master.USERTERMID)

    End Sub

    ''' <summary>
    ''' 確定押下時処理時処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub WF_Confirmed()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '締め状態検索処理
        Dim dt As DataTable = Select_Close()
        Dim nowDownloadMonth As String

        If dt.Rows.Count = 0 Then

            '締め状態登録処理
            Insert_Close("1")

        ElseIf dt.Rows.Count = 1 Then

            '締め状態更新処理
            Update_Close("1")

        End If

        dt = Nothing

        nowDownloadMonth = txtDownloadMonth.Text
        txtDownloadMonth.Text = Format(DateAdd("m", 1, CDate(txtDownloadMonth.Text + "/01")), "yyyy/MM")

        '締め状態検索処理
        dt = Select_Close()

        If dt.Rows.Count = 0 Then

            '締め状態登録処理
            Insert_Close("0")

        End If

        txtDownloadMonth.Text = nowDownloadMonth

        '■リースデータ未計上・計上済 更新処理(ストアド実行)
        UpdStoredLeaseSime(Replace(Me.txtDownloadMonth.Text, "/", ""), "1", Master.USERID, Master.USERTERMID)

    End Sub

#End Region

#Region "CSV処理"
    ''' <summary>
    ''' '経理連携
    ''' </summary>
    Protected Sub WF_AccountingRenkei(ByVal SQLcon As MySqlConnection)
        Try
            Dim htDataParm As New Hashtable
            Dim csvData As DataTable

            htDataParm = SetSelectAccountingParam(SQLcon)
#Region "経理連携データ"
            'CSV用データテーブル作成
            csvData = EntryAccountingData.SelectAccountingDataCsv(SQLcon, htDataParm)
#End Region

            '******************************
            '帳票作成処理の実行
            '******************************
            Dim Report As New CPT0019_AccountingData_DIODOC("LNT0019S", "経理仕訳データ_sample.xlsx", csvData)
            Dim url As String
            Try
                url = Report.CreateExcelPrintData()
            Catch ex As Exception
                Throw
            End Try

            '○ 別画面でExcelを表示
            WF_PrintURL.Value = url
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.SYSTEM_ADM_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0007L WF_AccountingRenkei", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             ' SUBクラス名
            CS0011LOGWrite.INFPOSI = "LNT0007L WF_AccountingRenkei"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.SYSTEM_ADM_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 ' ログ出力

            Exit Sub
        End Try
    End Sub

#End Region

#Region "DB関連処理"

    ''' <summary>
    ''' レンタル使用料取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Get_Rental()

        '必須入力チェック
        Dim err As String = ""
        'WW_Check(err)
        If err = "ERR" Then
            Exit Sub
        End If

        Dim dt = New DataTable

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine("SELECT ")
            SQLBldr.AppendLine("    RENTALCNT.FLG, RENTALCNT.ORGCODE, SUM(RENTALCNT.TOTAL) AS TOTAL")
            SQLBldr.AppendLine("FROM")
            SQLBldr.AppendLine("(")
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("      1 AS FLG")
            SQLBldr.AppendLine("    , TOTALTBL.ORGCODE AS ORGCODE")
            SQLBldr.AppendLine("    , COUNT(TOTALTBL.ORGCODE) AS TOTAL")
            SQLBldr.AppendLine("FROM")
            SQLBldr.AppendLine("(")
            SQLBldr.AppendLine("     SELECT ")
            SQLBldr.AppendLine("           MAIN.ORGCODE")
            SQLBldr.AppendLine("         , MAIN.TORICODE")
            SQLBldr.AppendLine("         , SUM(CASE HEAD.INVOICETYPE WHEN '2' THEN 1 ELSE 0 END ) RENTALCNT")
            SQLBldr.AppendLine("         , SUM(CASE HEAD.INVOICETYPE WHEN '3' THEN 1 ELSE 0 END ) LEASECNT")
            SQLBldr.AppendLine("         , SUM(CASE HEAD.INVOICETYPE WHEN '4' THEN 1 ELSE 0 END ) TEGAKICNT")
            SQLBldr.AppendLine("     FROM")
            SQLBldr.AppendLine("     (")
            SQLBldr.AppendLine("          SELECT DISTINCT")
            SQLBldr.AppendLine("                 LNT0017.INVFILINGDEPT AS ORGCODE")
            SQLBldr.AppendLine("               , LNT0017.TORICODE AS TORICODE")
            SQLBldr.AppendLine("               , LNT0017.SCHEDATEPAYMENT AS SCHEDATEPAYMENT")
            SQLBldr.AppendLine("			   ,CASE WHEN LNT0067.INVSUBCD IS NOT NULL THEN LNT0067.INVSUBCD")
            SQLBldr.AppendLine("				     ELSE coalesce(LNT0017.INVSUBCD, 0) END AS INVSUBCD")
            SQLBldr.AppendLine("          FROM lng.LNT0017_RESSNF LNT0017")
            SQLBldr.AppendLine("		  LEFT JOIN LNG.LNT0067_INVOICEDATA_RENT LNT0067")
            SQLBldr.AppendLine("			  ON LNT0017.SHIPYMD = LNT0067.SHIPYMD")
            SQLBldr.AppendLine("			  AND LNT0017.CTNTYPE = LNT0067.CTNTYPE")
            SQLBldr.AppendLine("			  AND LNT0017.CTNNO = LNT0067.CTNNO")
            SQLBldr.AppendLine("			  AND LNT0017.SAMEDAYCNT = LNT0067.SAMEDAYCNT")
            SQLBldr.AppendLine("			  AND LNT0017.CTNLINENO = LNT0067.CTNLINENO")
            SQLBldr.AppendLine("          WHERE")
            SQLBldr.AppendLine("              LNT0017.DELFLG = @P02")
            SQLBldr.AppendLine("              AND LNT0017.KEIJOYM = @P01")
            SQLBldr.AppendLine("              AND LNT0017.STACKFREEKBN = '1'        ")
            SQLBldr.AppendLine("              AND LNT0017.ACCOUNTSTATUSKBN IN ('1', '2')")
            SQLBldr.AppendLine("              AND LNT0017.ACCOUNTINGASSETSKBN = '1'")
            SQLBldr.AppendLine("              AND LNT0017.JURISDICTIONCD = '14'")
            SQLBldr.AppendLine("              AND LNT0017.INVFILINGDEPT IS NOT NULL")
            SQLBldr.AppendLine("			  AND LNT0017.ACCOUNTSTATUSKBN <> '8'")
            SQLBldr.AppendLine("			  AND LNT0017.TOTALINCOME <> 0")
            SQLBldr.AppendLine("    ) MAIN")
            SQLBldr.AppendLine("    LEFT JOIN")
            SQLBldr.AppendLine("    (")
            SQLBldr.AppendLine("     SELECT")
            SQLBldr.AppendLine("           LNT0064.INVOICEORGCODE AS ORGCODE")
            SQLBldr.AppendLine("         , TORICODE")
            SQLBldr.AppendLine("         , INVOICETYPE")
            SQLBldr.AppendLine("		 , INVSUBCD")
            SQLBldr.AppendLine("     FROM lng.LNT0064_INVOICEHEAD LNT0064")
            SQLBldr.AppendLine("            WHERE")
            SQLBldr.AppendLine("                DELFLG = @P02")
            SQLBldr.AppendLine("                AND KEIJOYM = @P01")
            SQLBldr.AppendLine("     GROUP BY")
            SQLBldr.AppendLine("           INVOICEORGCODE, TORICODE, INVOICETYPE, INVSUBCD")
            SQLBldr.AppendLine("    ) HEAD")
            SQLBldr.AppendLine("         ON MAIN.ORGCODE = HEAD.ORGCODE")
            SQLBldr.AppendLine("        AND MAIN.TORICODE = HEAD.TORICODE")
            SQLBldr.AppendLine("    GROUP BY")
            SQLBldr.AppendLine("        MAIN.ORGCODE, MAIN.TORICODE, MAIN.INVSUBCD")
            SQLBldr.AppendLine(") TOTALTBL")
            SQLBldr.AppendLine("WHERE")
            SQLBldr.AppendLine("    (TOTALTBL.RENTALCNT >= 1) ")
            SQLBldr.AppendLine(" OR (TOTALTBL.RENTALCNT = 0 AND TOTALTBL.LEASECNT = 0 AND TOTALTBL.TEGAKICNT = 0)")
            SQLBldr.AppendLine(" AND ORGCODE <> ''")
            SQLBldr.AppendLine("GROUP BY")
            SQLBldr.AppendLine("    TOTALTBL.ORGCODE")
            SQLBldr.AppendLine("UNION ALL")
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("      1 AS FLG")
            SQLBldr.AppendLine("    , TOTALTBL.ORGCODE AS ORGCODE")
            SQLBldr.AppendLine("    , COUNT(TOTALTBL.ORGCODE) AS TOTAL")
            SQLBldr.AppendLine("FROM")
            SQLBldr.AppendLine("(")
            SQLBldr.AppendLine("    SELECT")
            SQLBldr.AppendLine("          MAIN.ORGCODE, MAIN.TORICODE")
            SQLBldr.AppendLine("        , RANTAL.ORGCODE AS RCNT")
            SQLBldr.AppendLine("    FROM")
            SQLBldr.AppendLine("    (")
            SQLBldr.AppendLine("        SELECT")
            SQLBldr.AppendLine("             LNT0064.INVOICEORGCODE AS ORGCODE")
            SQLBldr.AppendLine("            ,TORICODE")
            SQLBldr.AppendLine("            ,COUNT(LNT0064.INVOICENUMBER) AS CNT")
            SQLBldr.AppendLine("        FROM lng.LNT0064_INVOICEHEAD LNT0064")
            SQLBldr.AppendLine("               WHERE        ")
            SQLBldr.AppendLine("                   DELFLG = @P02")
            SQLBldr.AppendLine("                   AND KEIJOYM = @P01")
            SQLBldr.AppendLine("                   AND INVOICETYPE = '2'")
            SQLBldr.AppendLine("        GROUP BY         ")
            SQLBldr.AppendLine("              INVOICEORGCODE, TORICODE")
            SQLBldr.AppendLine("        ) MAIN")
            SQLBldr.AppendLine("        LEFT JOIN")
            SQLBldr.AppendLine("        (")
            SQLBldr.AppendLine("         SELECT DISTINCT")
            SQLBldr.AppendLine("             INVFILINGDEPT AS ORGCODE")
            SQLBldr.AppendLine("             ,TORICODE")
            SQLBldr.AppendLine("         FROM lng.LNT0017_RESSNF")
            SQLBldr.AppendLine("         WHERE")
            SQLBldr.AppendLine("             DELFLG = @P02")
            SQLBldr.AppendLine("             AND KEIJOYM = @P01")
            SQLBldr.AppendLine("             AND STACKFREEKBN = '1'")
            SQLBldr.AppendLine("             AND ACCOUNTSTATUSKBN IN ('1', '2')")
            SQLBldr.AppendLine("             AND ACCOUNTINGASSETSKBN = '1'")
            SQLBldr.AppendLine("             AND JURISDICTIONCD = '14'")
            SQLBldr.AppendLine("             AND INVFILINGDEPT IS NOT NULL")
            SQLBldr.AppendLine("			 AND ACCOUNTSTATUSKBN <> '8'")
            SQLBldr.AppendLine("			 AND TOTALINCOME <> 0")
            SQLBldr.AppendLine("         GROUP BY")
            SQLBldr.AppendLine("             INVFILINGDEPT, TORICODE")
            SQLBldr.AppendLine("        ) RANTAL")
            SQLBldr.AppendLine("        ON MAIN.ORGCODE = RANTAL.ORGCODE")
            SQLBldr.AppendLine("        AND MAIN.TORICODE = RANTAL.TORICODE")
            SQLBldr.AppendLine(") TOTALTBL")
            SQLBldr.AppendLine("WHERE")
            SQLBldr.AppendLine("    TOTALTBL.RCNT IS NULL")
            SQLBldr.AppendLine("GROUP BY")
            SQLBldr.AppendLine("    TOTALTBL.ORGCODE")
            SQLBldr.AppendLine(") RENTALCNT")
            SQLBldr.AppendLine("GROUP BY")
            SQLBldr.AppendLine("    RENTALCNT.FLG, RENTALCNT.ORGCODE")
            SQLBldr.AppendLine("UNION ALL")
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("    2 AS FLG")
            SQLBldr.AppendLine("    ,LNT0064.INVOICEORGCODE as ORGCODE")
            SQLBldr.AppendLine("    ,COUNT(LNT0064.INVOICENUMBER) AS TOTAL")
            SQLBldr.AppendLine("FROM lng.LNT0064_INVOICEHEAD LNT0064")
            SQLBldr.AppendLine("       WHERE")
            SQLBldr.AppendLine("           DELFLG = @P02")
            SQLBldr.AppendLine("           AND KEIJOYM = @P01")
            SQLBldr.AppendLine("           AND REQUESTSTATUS= '5'")
            SQLBldr.AppendLine("           AND INVOICETYPE = '2'")
            SQLBldr.AppendLine("GROUP BY")
            SQLBldr.AppendLine("      INVOICEORGCODE")
            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)     '計上年月
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)     '削除フラグ

                    PARA01.Value = Replace(txtDownloadMonth.Text, "/", "")
                    PARA02.Value = C_DELETE_FLG.ALIVE

                    'SQL実行
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                    If Not dt.Rows.Count = 0 Then
                        'データセット
                        '北海道
                        Dim hokkaidoRow As DataRow() = dt.Select("ORGCODE=010102")
                        If Not hokkaidoRow.Length = 0 Then
                            If hokkaidoRow.Count = 2 Then
                                Me.WF_RENTAL_HOKKAIDOSYO.Text = CmnSetFmt.FormatComma(hokkaidoRow(1)("TOTAL").ToString(), True)
                                If Not hokkaidoRow(1)("TOTAL").ToString() = hokkaidoRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf hokkaidoRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_RENTAL_HOKKAIDOSYO.Text = "0"
                            End If
                            Me.WF_RENTAL_HOKKAIDOTOTAL.Text = CmnSetFmt.FormatComma(hokkaidoRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_RENTAL_HOKKAIDOTOTAL.Text)
                        ElseIf hokkaidoRow.Length = 0 Then
                            Me.WF_RENTAL_HOKKAIDOSYO.Text = "0"
                            Me.WF_RENTAL_HOKKAIDOTOTAL.Text = "0"
                        End If
                        WF_Hokkaido0.Value = "0"
                        If Me.WF_RENTAL_HOKKAIDOSYO.Text = Me.WF_RENTAL_HOKKAIDOTOTAL.Text Then
                            WF_Hokkaido0.Value = "1"
                        End If
                        '東北
                        Dim touhokuRow As DataRow() = dt.Select("ORGCODE=010401")
                        If Not touhokuRow.Length = 0 Then
                            If touhokuRow.Count = 2 Then
                                Me.WF_RENTAL_TOUHOKUSYO.Text = CmnSetFmt.FormatComma(touhokuRow(1)("TOTAL").ToString(), True)
                                If Not touhokuRow(1)("TOTAL").ToString() = touhokuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf touhokuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_RENTAL_TOUHOKUSYO.Text = "0"
                            End If
                            Me.WF_RENTAL_TOUHOKUTOTAL.Text = CmnSetFmt.FormatComma(touhokuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_RENTAL_TOUHOKUTOTAL.Text)
                        ElseIf touhokuRow.Length = 0 Then
                            Me.WF_RENTAL_TOUHOKUSYO.Text = "0"
                            Me.WF_RENTAL_TOUHOKUTOTAL.Text = "0"
                        End If
                        WF_Touhoku0.Value = "0"
                        If Me.WF_RENTAL_TOUHOKUSYO.Text = Me.WF_RENTAL_TOUHOKUTOTAL.Text Then
                            WF_Touhoku0.Value = "1"
                        End If
                        '関東
                        Dim kantouRow As DataRow() = dt.Select("ORGCODE=011402")
                        If Not kantouRow.Length = 0 Then
                            If kantouRow.Count = 2 Then
                                Me.WF_RENTAL_KANTOUSYO.Text = CmnSetFmt.FormatComma(kantouRow(1)("TOTAL").ToString(), True)
                                If Not kantouRow(1)("TOTAL").ToString() = kantouRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kantouRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_RENTAL_KANTOUSYO.Text = "0"
                            End If
                            Me.WF_RENTAL_KANTOUTOTAL.Text = CmnSetFmt.FormatComma(kantouRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_RENTAL_KANTOUTOTAL.Text)
                        ElseIf kantouRow.Length = 0 Then
                            Me.WF_RENTAL_KANTOUSYO.Text = "0"
                            Me.WF_RENTAL_KANTOUTOTAL.Text = "0"
                        End If
                        WF_Kantou0.Value = "0"
                        If Me.WF_RENTAL_KANTOUSYO.Text = Me.WF_RENTAL_KANTOUTOTAL.Text Then
                            WF_Kantou0.Value = "1"
                        End If
                        '中部
                        Dim tyubuRow As DataRow() = dt.Select("ORGCODE=012401")
                        If Not tyubuRow.Length = 0 Then
                            If tyubuRow.Count = 2 Then
                                Me.WF_RENTAL_TYUBUSYO.Text = CmnSetFmt.FormatComma(tyubuRow(1)("TOTAL").ToString(), True)
                                If Not tyubuRow(1)("TOTAL").ToString() = tyubuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf tyubuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_RENTAL_TYUBUSYO.Text = "0"
                            End If
                            Me.WF_RENTAL_TYUBUTOTAL.Text = CmnSetFmt.FormatComma(tyubuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_RENTAL_TYUBUTOTAL.Text)
                        ElseIf tyubuRow.Length = 0 Then
                            Me.WF_RENTAL_TYUBUSYO.Text = "0"
                            Me.WF_RENTAL_TYUBUTOTAL.Text = "0"
                        End If
                        WF_Tyubu0.Value = "0"
                        If Me.WF_RENTAL_TYUBUSYO.Text = Me.WF_RENTAL_TYUBUTOTAL.Text Then
                            WF_Tyubu0.Value = "1"
                        End If
                        '関西
                        Dim kansaiRow As DataRow() = dt.Select("ORGCODE=012701")
                        If Not kansaiRow.Length = 0 Then
                            If kansaiRow.Count = 2 Then
                                Me.WF_RENTAL_KANSAISYO.Text = CmnSetFmt.FormatComma(kansaiRow(1)("TOTAL").ToString(), True)
                                If Not kansaiRow(1)("TOTAL").ToString() = kansaiRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kansaiRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_RENTAL_KANSAISYO.Text = "0"
                            End If
                            Me.WF_RENTAL_KANSAITOTAL.Text = CmnSetFmt.FormatComma(kansaiRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_RENTAL_KANSAITOTAL.Text)
                        ElseIf kansaiRow.Length = 0 Then
                            Me.WF_RENTAL_KANSAISYO.Text = "0"
                            Me.WF_RENTAL_KANSAITOTAL.Text = "0"
                        End If
                        WF_Kansai0.Value = "0"
                        If Me.WF_RENTAL_KANSAISYO.Text = Me.WF_RENTAL_KANSAITOTAL.Text Then
                            WF_Kansai0.Value = "1"
                        End If
                        '九州
                        Dim kyusyuRow As DataRow() = dt.Select("ORGCODE=014001")
                        If Not kyusyuRow.Length = 0 Then
                            If kyusyuRow.Count = 2 Then
                                Me.WF_RENTAL_KYUSYUSYO.Text = CmnSetFmt.FormatComma(kyusyuRow(1)("TOTAL").ToString(), True)
                                If Not kyusyuRow(1)("TOTAL").ToString() = kyusyuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kyusyuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_RENTAL_KYUSYUSYO.Text = "0"
                            End If
                            Me.WF_RENTAL_KYUSYUTOTAL.Text = CmnSetFmt.FormatComma(kyusyuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_RENTAL_KYUSYUTOTAL.Text)
                        ElseIf kyusyuRow.Length = 0 Then
                            Me.WF_RENTAL_KYUSYUSYO.Text = "0"
                            Me.WF_RENTAL_KYUSYUTOTAL.Text = "0"
                        End If
                        WF_Kyusyu0.Value = "0"
                        If Me.WF_RENTAL_KYUSYUSYO.Text = Me.WF_RENTAL_KYUSYUTOTAL.Text Then
                            WF_Kyusyu0.Value = "1"
                        End If
                        'コンテナ部
                        Dim CTNRow As DataRow() = dt.Select("ORGCODE=011312")
                        If Not CTNRow.Length = 0 Then
                            If CTNRow.Count = 2 Then
                                Me.WF_RENTAL_CTNSYO.Text = CmnSetFmt.FormatComma(CTNRow(1)("TOTAL").ToString(), True)
                                If Not CTNRow(1)("TOTAL").ToString() = CTNRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf CTNRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_RENTAL_CTNSYO.Text = "0"
                            End If
                            Me.WF_RENTAL_CTNTOTAL.Text = CmnSetFmt.FormatComma(CTNRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_RENTAL_CTNTOTAL.Text)
                        ElseIf CTNRow.Length = 0 Then
                            Me.WF_RENTAL_CTNSYO.Text = "0"
                            Me.WF_RENTAL_CTNTOTAL.Text = "0"
                        End If
                        WF_CTN0.Value = "0"
                        If Me.WF_RENTAL_CTNSYO.Text = Me.WF_RENTAL_CTNTOTAL.Text Then
                            WF_CTN0.Value = "1"
                        End If
                    ElseIf dt.Rows.Count = 0 Then
                        Me.WF_RENTAL_HOKKAIDOSYO.Text = "0"
                        Me.WF_RENTAL_HOKKAIDOTOTAL.Text = "0"
                        Me.WF_RENTAL_TOUHOKUSYO.Text = "0"
                        Me.WF_RENTAL_TOUHOKUTOTAL.Text = "0"
                        Me.WF_RENTAL_KANTOUSYO.Text = "0"
                        Me.WF_RENTAL_KANTOUTOTAL.Text = "0"
                        Me.WF_RENTAL_TYUBUSYO.Text = "0"
                        Me.WF_RENTAL_TYUBUTOTAL.Text = "0"
                        Me.WF_RENTAL_KANSAISYO.Text = "0"
                        Me.WF_RENTAL_KANSAITOTAL.Text = "0"
                        Me.WF_RENTAL_KYUSYUSYO.Text = "0"
                        Me.WF_RENTAL_KYUSYUTOTAL.Text = "0"
                        Me.WF_RENTAL_CTNSYO.Text = "0"
                        Me.WF_RENTAL_CTNTOTAL.Text = "0"
                        WF_Hokkaido0.Value = "1"
                        WF_Touhoku0.Value = "1"
                        WF_Kantou0.Value = "1"
                        WF_Tyubu0.Value = "1"
                        WF_Kansai0.Value = "1"
                        WF_Kyusyu0.Value = "1"
                        WF_CTN0.Value = "1"
                    End If
                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_ACQUISITION_ERROR, C_MESSAGE_TYPE.ABORT, "DBエラー LNT0019D GetRental", needsPopUp:=True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019D GetRental"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' リース料取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Get_Lease()

        Dim dt = New DataTable

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine("SELECT ")
            SQLBldr.AppendLine("    LEASECNT.FLG, LEASECNT.ORGCODE, SUM(LEASECNT.TOTAL) AS TOTAL")
            SQLBldr.AppendLine("FROM")
            SQLBldr.AppendLine("(")
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("      1 AS FLG")
            SQLBldr.AppendLine("    , TOTALTBL.ORGCODE AS ORGCODE")
            SQLBldr.AppendLine("    , COUNT(TOTALTBL.ORGCODE) AS TOTAL")
            SQLBldr.AppendLine("FROM")
            SQLBldr.AppendLine("(")
            SQLBldr.AppendLine("     SELECT ")
            SQLBldr.AppendLine("           MAIN.ORGCODE")
            SQLBldr.AppendLine("         , MAIN.TORICODE")
            SQLBldr.AppendLine("         , SUM(CASE HEAD.INVOICETYPE WHEN '2' THEN 1 ELSE 0 END ) RENTALCNT")
            SQLBldr.AppendLine("         , SUM(CASE HEAD.INVOICETYPE WHEN '3' THEN 1 ELSE 0 END ) LEASECNT")
            SQLBldr.AppendLine("         , SUM(CASE HEAD.INVOICETYPE WHEN '4' THEN 1 ELSE 0 END ) TEGAKICNT")
            SQLBldr.AppendLine("     FROM")
            SQLBldr.AppendLine("     (")
            SQLBldr.AppendLine("          SELECT DISTINCT")
            SQLBldr.AppendLine("                  LNT0042.INVOICEOUTORGCD AS ORGCODE")
            SQLBldr.AppendLine("                 ,LNT0042.TORICODE AS TORICODE")
            SQLBldr.AppendLine("				 ,CASE WHEN LNT0068.INVSUBCD IS NOT NULL THEN LNT0068.INVSUBCD")
            SQLBldr.AppendLine("				       ELSE coalesce(LNT0042.INVSUBCD, 0) END AS INVSUBCD")
            SQLBldr.AppendLine("              FROM lng.LNT0042_LEASEDATA LNT0042")
            SQLBldr.AppendLine("			  LEFT JOIN LNG.LNT0068_INVOICEDATA_LEASE LNT0068")
            SQLBldr.AppendLine("			      ON LNT0042.LEASENO = LNT0068.LEASENO")
            SQLBldr.AppendLine("			      AND LNT0042.CTNTYPE = LNT0068.CTNTYPE")
            SQLBldr.AppendLine("			      AND LNT0042.CTNNO = LNT0068.CTNNO")
            SQLBldr.AppendLine("			      AND LNT0042.KEIJOYM = LNT0068.KEIJOYM")
            SQLBldr.AppendLine("			      AND LNT0042.LEASEMONTHSTARTYMD = LNT0068.LEASEMONTHSTARTYMD")
            SQLBldr.AppendLine("              WHERE")
            SQLBldr.AppendLine("                  LNT0042.DELFLG = @P02")
            SQLBldr.AppendLine("                  AND LNT0042.LEASEAPPLYKBN <> '2'")
            SQLBldr.AppendLine("                  AND LNT0042.KEIJOYM = @P01")
            SQLBldr.AppendLine("                  AND LNT0042.INVOICEOUTORGCD IS NOT NULL")
            SQLBldr.AppendLine("                  AND LNT0042.MONTHLEASEFEE <> 0")
            SQLBldr.AppendLine("    ) MAIN")
            SQLBldr.AppendLine("    LEFT JOIN")
            SQLBldr.AppendLine("    (")
            SQLBldr.AppendLine("     SELECT")
            SQLBldr.AppendLine("           LNT0064.INVOICEORGCODE AS ORGCODE")
            SQLBldr.AppendLine("         , TORICODE")
            SQLBldr.AppendLine("         , INVOICETYPE")
            SQLBldr.AppendLine("         , coalesce(INVSUBCD, 0) AS INVSUBCD")
            SQLBldr.AppendLine("     FROM lng.LNT0064_INVOICEHEAD LNT0064")
            SQLBldr.AppendLine("            WHERE")
            SQLBldr.AppendLine("                DELFLG = @P02")
            SQLBldr.AppendLine("                AND KEIJOYM = @P01")
            SQLBldr.AppendLine("     GROUP BY")
            SQLBldr.AppendLine("           INVOICEORGCODE, TORICODE, INVOICETYPE, INVSUBCD")
            SQLBldr.AppendLine("    ) HEAD")
            SQLBldr.AppendLine("         ON MAIN.ORGCODE = HEAD.ORGCODE")
            SQLBldr.AppendLine("        AND MAIN.TORICODE = HEAD.TORICODE")
            SQLBldr.AppendLine("    GROUP BY")
            SQLBldr.AppendLine("        MAIN.ORGCODE, MAIN.TORICODE, MAIN.INVSUBCD")
            SQLBldr.AppendLine(") TOTALTBL")
            SQLBldr.AppendLine("WHERE")
            SQLBldr.AppendLine("    (TOTALTBL.LEASECNT >= 1) ")
            SQLBldr.AppendLine(" OR (TOTALTBL.RENTALCNT = 0 AND TOTALTBL.LEASECNT = 0 AND TOTALTBL.TEGAKICNT = 0)")
            SQLBldr.AppendLine("GROUP BY")
            SQLBldr.AppendLine("    TOTALTBL.ORGCODE")
            SQLBldr.AppendLine("UNION ALL")
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("      1 AS FLG")
            SQLBldr.AppendLine("    , TOTALTBL.ORGCODE AS ORGCODE")
            SQLBldr.AppendLine("    , COUNT(TOTALTBL.ORGCODE) AS TOTAL")
            SQLBldr.AppendLine("FROM")
            SQLBldr.AppendLine("(")
            SQLBldr.AppendLine("    SELECT")
            SQLBldr.AppendLine("          MAIN.ORGCODE, MAIN.TORICODE")
            SQLBldr.AppendLine("        , LEASE.ORGCODE AS LEASE_ORGCODE")
            SQLBldr.AppendLine("    FROM")
            SQLBldr.AppendLine("    (")
            SQLBldr.AppendLine("        SELECT")
            SQLBldr.AppendLine("             LNT0064.INVOICEORGCODE AS ORGCODE")
            SQLBldr.AppendLine("            ,TORICODE")
            SQLBldr.AppendLine("            ,COUNT(LNT0064.INVOICENUMBER) AS CNT")
            SQLBldr.AppendLine("        FROM lng.LNT0064_INVOICEHEAD LNT0064")
            SQLBldr.AppendLine("               WHERE        ")
            SQLBldr.AppendLine("                   DELFLG = @P02")
            SQLBldr.AppendLine("                   AND KEIJOYM = @P01")
            SQLBldr.AppendLine("                   AND INVOICETYPE = '3'")
            SQLBldr.AppendLine("        GROUP BY         ")
            SQLBldr.AppendLine("              INVOICEORGCODE, TORICODE")
            SQLBldr.AppendLine("        ) MAIN")
            SQLBldr.AppendLine("        LEFT JOIN")
            SQLBldr.AppendLine("        (")
            SQLBldr.AppendLine("          SELECT DISTINCT")
            SQLBldr.AppendLine("                  LNT0042.INVOICEOUTORGCD AS ORGCODE")
            SQLBldr.AppendLine("                 ,LNT0042.TORICODE")
            SQLBldr.AppendLine("              FROM (select * from lng.LNT0042_LEASEDATA where DELFLG = '0' AND coalesce(LEASEAPPLYKBN, '0') <> '2') LNT0042")
            SQLBldr.AppendLine("              WHERE")
            SQLBldr.AppendLine("                  LNT0042.DELFLG = @P02")
            SQLBldr.AppendLine("                  AND LNT0042.LEASEAPPLYKBN <> '2'")
            SQLBldr.AppendLine("                  AND LNT0042.KEIJOYM = @P01")
            SQLBldr.AppendLine("                  AND LNT0042.INVOICEOUTORGCD IS NOT NULL")
            SQLBldr.AppendLine("                  AND LNT0042.MONTHLEASEFEE <> 0")
            SQLBldr.AppendLine("        ) LEASE")
            SQLBldr.AppendLine("        ON MAIN.ORGCODE = LEASE.ORGCODE")
            SQLBldr.AppendLine("        AND MAIN.TORICODE = LEASE.TORICODE")
            SQLBldr.AppendLine(") TOTALTBL")
            SQLBldr.AppendLine("WHERE")
            SQLBldr.AppendLine("    TOTALTBL.LEASE_ORGCODE IS NULL")
            SQLBldr.AppendLine("GROUP BY")
            SQLBldr.AppendLine("    TOTALTBL.ORGCODE")
            SQLBldr.AppendLine(") LEASECNT")
            SQLBldr.AppendLine("GROUP BY")
            SQLBldr.AppendLine("    LEASECNT.FLG, LEASECNT.ORGCODE")
            SQLBldr.AppendLine("UNION ALL")
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("    2 AS FLG")
            SQLBldr.AppendLine("    ,LNT0064.INVOICEORGCODE as ORGCODE")
            SQLBldr.AppendLine("    ,COUNT(LNT0064.INVOICENUMBER) AS TOTAL")
            SQLBldr.AppendLine("FROM lng.LNT0064_INVOICEHEAD LNT0064")
            SQLBldr.AppendLine("       WHERE")
            SQLBldr.AppendLine("           DELFLG = @P02")
            SQLBldr.AppendLine("           AND KEIJOYM = @P01")
            SQLBldr.AppendLine("           AND REQUESTSTATUS= '5'")
            SQLBldr.AppendLine("           AND INVOICETYPE = '3'")
            SQLBldr.AppendLine("GROUP BY")
            SQLBldr.AppendLine("      INVOICEORGCODE")
            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)     '計上年月
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)     '削除フラグ

                    PARA01.Value = Replace(txtDownloadMonth.Text, "/", "")
                    PARA02.Value = C_DELETE_FLG.ALIVE

                    'SQL実行
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                    If Not dt.Rows.Count = 0 Then
                        'データセット
                        '北海道
                        Dim hokkaidoRow As DataRow() = dt.Select("ORGCODE=010102")
                        If Not hokkaidoRow.Length = 0 Then
                            If hokkaidoRow.Count = 2 Then
                                Me.WF_LEASE_HOKKAIDOSYO.Text = CmnSetFmt.FormatComma(hokkaidoRow(1)("TOTAL").ToString(), True)
                                If Not hokkaidoRow(1)("TOTAL").ToString() = hokkaidoRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf hokkaidoRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_LEASE_HOKKAIDOSYO.Text = "0"
                            End If
                            Me.WF_LEASE_HOKKAIDOTOTAL.Text = CmnSetFmt.FormatComma(hokkaidoRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_LEASE_HOKKAIDOTOTAL.Text)
                        ElseIf hokkaidoRow.Length = 0 Then
                            Me.WF_LEASE_HOKKAIDOSYO.Text = "0"
                            Me.WF_LEASE_HOKKAIDOTOTAL.Text = "0"
                        End If
                        WF_Hokkaido1.Value = "0"
                        If Me.WF_LEASE_HOKKAIDOSYO.Text = Me.WF_LEASE_HOKKAIDOTOTAL.Text Then
                            WF_Hokkaido1.Value = "1"
                        End If
                        '東北
                        Dim touhokuRow As DataRow() = dt.Select("ORGCODE=010401")
                        If Not touhokuRow.Length = 0 Then
                            If touhokuRow.Count = 2 Then
                                Me.WF_LEASE_TOUHOKUSYO.Text = CmnSetFmt.FormatComma(touhokuRow(1)("TOTAL").ToString(), True)
                                If Not touhokuRow(1)("TOTAL").ToString() = touhokuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf touhokuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_LEASE_TOUHOKUSYO.Text = "0"
                            End If
                            Me.WF_LEASE_TOUHOKUTOTAL.Text = CmnSetFmt.FormatComma(touhokuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_LEASE_TOUHOKUTOTAL.Text)
                        ElseIf touhokuRow.Length = 0 Then
                            Me.WF_LEASE_TOUHOKUSYO.Text = "0"
                            Me.WF_LEASE_TOUHOKUTOTAL.Text = "0"
                        End If
                        WF_Touhoku1.Value = "0"
                        If Me.WF_LEASE_TOUHOKUSYO.Text = Me.WF_LEASE_TOUHOKUTOTAL.Text Then
                            WF_Touhoku1.Value = "1"
                        End If
                        '関東
                        Dim kantouRow As DataRow() = dt.Select("ORGCODE=011402")
                        If Not kantouRow.Length = 0 Then
                            If kantouRow.Count = 2 Then
                                Me.WF_LEASE_KANTOUSYO.Text = CmnSetFmt.FormatComma(kantouRow(1)("TOTAL").ToString(), True)
                                If Not kantouRow(1)("TOTAL").ToString() = kantouRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kantouRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_LEASE_KANTOUSYO.Text = "0"
                            End If
                            Me.WF_LEASE_KANTOUTOTAL.Text = CmnSetFmt.FormatComma(kantouRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_LEASE_KANTOUTOTAL.Text)
                        ElseIf kantouRow.Length = 0 Then
                            Me.WF_LEASE_KANTOUSYO.Text = "0"
                            Me.WF_LEASE_KANTOUTOTAL.Text = "0"
                        End If
                        WF_Kantou1.Value = "0"
                        If Me.WF_LEASE_KANTOUSYO.Text = Me.WF_LEASE_KANTOUTOTAL.Text Then
                            WF_Kantou1.Value = "1"
                        End If
                        '中部
                        Dim tyubuRow As DataRow() = dt.Select("ORGCODE=012401")
                        If Not tyubuRow.Length = 0 Then
                            If tyubuRow.Count = 2 Then
                                Me.WF_LEASE_TYUBUSYO.Text = CmnSetFmt.FormatComma(tyubuRow(1)("TOTAL").ToString(), True)
                                If Not tyubuRow(1)("TOTAL").ToString() = tyubuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf tyubuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_LEASE_TYUBUSYO.Text = "0"
                            End If
                            Me.WF_LEASE_TYUBUTOTAL.Text = CmnSetFmt.FormatComma(tyubuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_LEASE_TYUBUTOTAL.Text)
                        ElseIf tyubuRow.Length = 0 Then
                            Me.WF_LEASE_TYUBUSYO.Text = "0"
                            Me.WF_LEASE_TYUBUTOTAL.Text = "0"
                        End If
                        WF_Tyubu1.Value = "0"
                        If Me.WF_LEASE_TYUBUSYO.Text = Me.WF_LEASE_TYUBUTOTAL.Text Then
                            WF_Tyubu1.Value = "1"
                        End If
                        '関西
                        Dim kansaiRow As DataRow() = dt.Select("ORGCODE=012701")
                        If Not kansaiRow.Length = 0 Then
                            If kansaiRow.Count = 2 Then
                                Me.WF_LEASE_KANSAISYO.Text = CmnSetFmt.FormatComma(kansaiRow(1)("TOTAL").ToString(), True)
                                If Not kansaiRow(1)("TOTAL").ToString() = kansaiRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kansaiRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_LEASE_KANSAISYO.Text = "0"
                            End If
                            Me.WF_LEASE_KANSAITOTAL.Text = CmnSetFmt.FormatComma(kansaiRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_LEASE_KANSAITOTAL.Text)
                        ElseIf kansaiRow.Length = 0 Then
                            Me.WF_LEASE_KANSAISYO.Text = "0"
                            Me.WF_LEASE_KANSAITOTAL.Text = "0"
                        End If
                        WF_Kansai1.Value = "0"
                        If Me.WF_LEASE_KANSAISYO.Text = Me.WF_LEASE_KANSAITOTAL.Text Then
                            WF_Kansai1.Value = "1"
                        End If
                        '九州
                        Dim kyusyuRow As DataRow() = dt.Select("ORGCODE=014001")
                        If Not kyusyuRow.Length = 0 Then
                            If kyusyuRow.Count = 2 Then
                                Me.WF_LEASE_KYUSYUSYO.Text = CmnSetFmt.FormatComma(kyusyuRow(1)("TOTAL").ToString(), True)
                                If Not kyusyuRow(1)("TOTAL").ToString() = kyusyuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kyusyuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_LEASE_KYUSYUSYO.Text = "0"
                            End If
                            Me.WF_LEASE_KYUSYUTOTAL.Text = CmnSetFmt.FormatComma(kyusyuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_LEASE_KYUSYUTOTAL.Text)
                        ElseIf kyusyuRow.Length = 0 Then
                            Me.WF_LEASE_KYUSYUSYO.Text = "0"
                            Me.WF_LEASE_KYUSYUTOTAL.Text = "0"
                        End If
                        WF_Kyusyu1.Value = "0"
                        If Me.WF_LEASE_KYUSYUSYO.Text = Me.WF_LEASE_KYUSYUTOTAL.Text Then
                            WF_Kyusyu1.Value = "1"
                        End If
                        'コンテナ部
                        Dim CTNRow As DataRow() = dt.Select("ORGCODE=011312")
                        If Not CTNRow.Length = 0 Then
                            If CTNRow.Count = 2 Then
                                Me.WF_LEASE_CTNSYO.Text = CmnSetFmt.FormatComma(CTNRow(1)("TOTAL").ToString(), True)
                                If Not CTNRow(1)("TOTAL").ToString() = CTNRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf CTNRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_LEASE_CTNSYO.Text = "0"
                            End If
                            Me.WF_LEASE_CTNTOTAL.Text = CmnSetFmt.FormatComma(CTNRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_LEASE_CTNTOTAL.Text)
                        ElseIf CTNRow.Length = 0 Then
                            Me.WF_LEASE_CTNSYO.Text = "0"
                            Me.WF_LEASE_CTNTOTAL.Text = "0"
                        End If
                        WF_CTN1.Value = "0"
                        If Me.WF_LEASE_CTNSYO.Text = Me.WF_LEASE_CTNTOTAL.Text Then
                            WF_CTN1.Value = "1"
                        End If
                    ElseIf dt.Rows.Count = 0 Then
                        Me.WF_LEASE_HOKKAIDOSYO.Text = "0"
                        Me.WF_LEASE_HOKKAIDOTOTAL.Text = "0"
                        Me.WF_LEASE_TOUHOKUSYO.Text = "0"
                        Me.WF_LEASE_TOUHOKUTOTAL.Text = "0"
                        Me.WF_LEASE_KANTOUSYO.Text = "0"
                        Me.WF_LEASE_KANTOUTOTAL.Text = "0"
                        Me.WF_LEASE_TYUBUSYO.Text = "0"
                        Me.WF_LEASE_TYUBUTOTAL.Text = "0"
                        Me.WF_LEASE_KANSAISYO.Text = "0"
                        Me.WF_LEASE_KANSAITOTAL.Text = "0"
                        Me.WF_LEASE_KYUSYUSYO.Text = "0"
                        Me.WF_LEASE_KYUSYUTOTAL.Text = "0"
                        Me.WF_LEASE_CTNSYO.Text = "0"
                        Me.WF_LEASE_CTNTOTAL.Text = "0"
                        WF_Hokkaido1.Value = "1"
                        WF_Touhoku1.Value = "1"
                        WF_Kantou1.Value = "1"
                        WF_Tyubu1.Value = "1"
                        WF_Kansai1.Value = "1"
                        WF_Kyusyu1.Value = "1"
                        WF_CTN1.Value = "1"
                    End If
                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_ACQUISITION_ERROR, C_MESSAGE_TYPE.ABORT, "DBエラー LNT0019D GetLease", needsPopUp:=True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019D Lease"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 手書き請求書取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Get_Write()

        Dim dt = New DataTable

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("    1 AS FLG")
            SQLBldr.AppendLine("    ,MAIN.INVOICEORGCODE as ORGCODE")
            SQLBldr.AppendLine("    ,COUNT(MAIN.TORICODE) AS TOTAL")
            SQLBldr.AppendLine("FROM ( SELECT DISTINCT")
            SQLBldr.AppendLine("	       INVOICEORGCODE")
            SQLBldr.AppendLine("		  ,TORICODE")
            SQLBldr.AppendLine("		  ,INVSUBCD")
            SQLBldr.AppendLine("	   FROM lng.LNT0064_INVOICEHEAD")
            SQLBldr.AppendLine("       WHERE")
            SQLBldr.AppendLine("           DELFLG = @P02")
            SQLBldr.AppendLine("           AND KEIJOYM = @P01")
            SQLBldr.AppendLine("		   AND INVOICETYPE = '4'")
            SQLBldr.AppendLine("	 ) MAIN")
            SQLBldr.AppendLine("GROUP BY ")
            SQLBldr.AppendLine("      INVOICEORGCODE")
            SQLBldr.AppendLine("UNION ALL")
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("    2 AS FLG")
            SQLBldr.AppendLine("    ,MAIN2.INVOICEORGCODE as ORGCODE")
            SQLBldr.AppendLine("    ,COUNT(MAIN2.TORICODE) AS TOTAL")
            SQLBldr.AppendLine("FROM ( SELECT DISTINCT")
            SQLBldr.AppendLine("	       INVOICEORGCODE")
            SQLBldr.AppendLine("		  ,TORICODE")
            SQLBldr.AppendLine("		  ,INVSUBCD")
            SQLBldr.AppendLine("	   FROM lng.LNT0064_INVOICEHEAD")
            SQLBldr.AppendLine("       WHERE")
            SQLBldr.AppendLine("           DELFLG = @P02")
            SQLBldr.AppendLine("           AND KEIJOYM = @P01")
            SQLBldr.AppendLine("		   AND INVOICETYPE = '4'")
            SQLBldr.AppendLine("		   AND REQUESTSTATUS = '5'")
            SQLBldr.AppendLine("	 ) MAIN2")
            SQLBldr.AppendLine("GROUP BY ")
            SQLBldr.AppendLine("      INVOICEORGCODE")
            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)     '計上年月
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)     '削除フラグ

                    PARA01.Value = Replace(txtDownloadMonth.Text, "/", "")
                    PARA02.Value = C_DELETE_FLG.ALIVE

                    'SQL実行
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                    If Not dt.Rows.Count = 0 Then
                        'データセット
                        '北海道
                        Dim hokkaidoRow As DataRow() = dt.Select("ORGCODE=010102")
                        If Not hokkaidoRow.Length = 0 Then
                            If hokkaidoRow.Count = 2 Then
                                Me.WF_WRITE_HOKKAIDOSYO.Text = CmnSetFmt.FormatComma(hokkaidoRow(1)("TOTAL").ToString(), True)
                                If Not hokkaidoRow(1)("TOTAL").ToString() = hokkaidoRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf hokkaidoRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_WRITE_HOKKAIDOSYO.Text = "0"
                            End If
                            Me.WF_WRITE_HOKKAIDOTOTAL.Text = CmnSetFmt.FormatComma(hokkaidoRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_WRITE_HOKKAIDOSYO.Text)
                        ElseIf hokkaidoRow.Length = 0 Then
                            Me.WF_WRITE_HOKKAIDOSYO.Text = "0"
                            Me.WF_WRITE_HOKKAIDOTOTAL.Text = "0"
                        End If
                        WF_Hokkaido2.Value = "0"
                        If Me.WF_WRITE_HOKKAIDOSYO.Text = Me.WF_WRITE_HOKKAIDOTOTAL.Text Then
                            WF_Hokkaido2.Value = "1"
                        End If
                        '東北
                        Dim touhokuRow As DataRow() = dt.Select("ORGCODE=010401")
                        If Not touhokuRow.Length = 0 Then
                            If touhokuRow.Count = 2 Then
                                Me.WF_WRITE_TOUHOKUSYO.Text = CmnSetFmt.FormatComma(touhokuRow(1)("TOTAL").ToString(), True)
                                If Not touhokuRow(1)("TOTAL").ToString() = touhokuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf touhokuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_WRITE_TOUHOKUSYO.Text = "0"
                            End If
                            Me.WF_WRITE_TOUHOKUTOTAL.Text = CmnSetFmt.FormatComma(touhokuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_WRITE_TOUHOKUSYO.Text)
                        ElseIf touhokuRow.Length = 0 Then
                            Me.WF_WRITE_TOUHOKUSYO.Text = "0"
                            Me.WF_WRITE_TOUHOKUTOTAL.Text = "0"
                        End If
                        WF_Touhoku2.Value = "0"
                        If Me.WF_WRITE_TOUHOKUSYO.Text = Me.WF_WRITE_TOUHOKUTOTAL.Text Then
                            WF_Touhoku2.Value = "1"
                        End If
                        '関東
                        Dim kantouRow As DataRow() = dt.Select("ORGCODE=011402")
                        If Not kantouRow.Length = 0 Then
                            If kantouRow.Count = 2 Then
                                Me.WF_WRITE_KANTOUSYO.Text = CmnSetFmt.FormatComma(kantouRow(1)("TOTAL").ToString(), True)
                                If Not kantouRow(1)("TOTAL").ToString() = kantouRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kantouRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_WRITE_KANTOUSYO.Text = "0"
                            End If
                            Me.WF_WRITE_KANTOUTOTAL.Text = CmnSetFmt.FormatComma(kantouRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_WRITE_KANTOUSYO.Text)
                        ElseIf kantouRow.Length = 0 Then
                            Me.WF_WRITE_KANTOUSYO.Text = "0"
                            Me.WF_WRITE_KANTOUTOTAL.Text = "0"
                        End If
                        WF_Kantou2.Value = "0"
                        If Me.WF_WRITE_KANTOUSYO.Text = Me.WF_WRITE_KANTOUTOTAL.Text Then
                            WF_Kantou2.Value = "1"
                        End If
                        '中部
                        Dim tyubuRow As DataRow() = dt.Select("ORGCODE=012401")
                        If Not tyubuRow.Length = 0 Then
                            If tyubuRow.Count = 2 Then
                                Me.WF_WRITE_TYUBUSYO.Text = CmnSetFmt.FormatComma(tyubuRow(1)("TOTAL").ToString(), True)
                                If Not tyubuRow(1)("TOTAL").ToString() = tyubuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf tyubuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_WRITE_TYUBUSYO.Text = "0"
                            End If
                            Me.WF_WRITE_TYUBUTOTAL.Text = CmnSetFmt.FormatComma(tyubuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_WRITE_TYUBUSYO.Text)
                        ElseIf tyubuRow.Length = 0 Then
                            Me.WF_WRITE_TYUBUSYO.Text = "0"
                            Me.WF_WRITE_TYUBUTOTAL.Text = "0"
                        End If
                        WF_Tyubu2.Value = "0"
                        If Me.WF_WRITE_TYUBUSYO.Text = Me.WF_WRITE_TYUBUTOTAL.Text Then
                            WF_Tyubu2.Value = "1"
                        End If
                        '関西
                        Dim kansaiRow As DataRow() = dt.Select("ORGCODE=012701")
                        If Not kansaiRow.Length = 0 Then
                            If kansaiRow.Count = 2 Then
                                Me.WF_WRITE_KANSAISYO.Text = CmnSetFmt.FormatComma(kansaiRow(1)("TOTAL").ToString(), True)
                                If Not kansaiRow(1)("TOTAL").ToString() = kansaiRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kansaiRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_WRITE_KANSAISYO.Text = "0"
                            End If
                            Me.WF_WRITE_KANSAITOTAL.Text = CmnSetFmt.FormatComma(kansaiRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_WRITE_KANSAISYO.Text)
                        ElseIf kansaiRow.Length = 0 Then
                            Me.WF_WRITE_KANSAISYO.Text = "0"
                            Me.WF_WRITE_KANSAITOTAL.Text = "0"
                        End If
                        WF_Kansai2.Value = "0"
                        If Me.WF_WRITE_KANSAISYO.Text = Me.WF_WRITE_KANSAITOTAL.Text Then
                            WF_Kansai2.Value = "1"
                        End If
                        '九州
                        Dim kyusyuRow As DataRow() = dt.Select("ORGCODE=014001")
                        If Not kyusyuRow.Length = 0 Then
                            If kyusyuRow.Count = 2 Then
                                Me.WF_WRITE_KYUSYUSYO.Text = CmnSetFmt.FormatComma(kyusyuRow(1)("TOTAL").ToString(), True)
                                If Not kyusyuRow(1)("TOTAL").ToString() = kyusyuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kyusyuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_WRITE_KYUSYUSYO.Text = "0"
                            End If
                            Me.WF_WRITE_KYUSYUTOTAL.Text = CmnSetFmt.FormatComma(kyusyuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_WRITE_KYUSYUSYO.Text)
                        ElseIf kyusyuRow.Length = 0 Then
                            Me.WF_WRITE_KYUSYUSYO.Text = "0"
                            Me.WF_WRITE_KYUSYUTOTAL.Text = "0"
                        End If
                        WF_Kyusyu2.Value = "0"
                        If Me.WF_WRITE_KYUSYUSYO.Text = Me.WF_WRITE_KYUSYUTOTAL.Text Then
                            WF_Kyusyu2.Value = "1"
                        End If
                        'コンテナ部
                        Dim CTNRow As DataRow() = dt.Select("ORGCODE=011312")
                        If Not CTNRow.Length = 0 Then
                            If CTNRow.Count = 2 Then
                                Me.WF_WRITE_CTNSYO.Text = CmnSetFmt.FormatComma(CTNRow(1)("TOTAL").ToString(), True)
                                If Not CTNRow(1)("TOTAL").ToString() = CTNRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf CTNRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_WRITE_CTNSYO.Text = "0"
                            End If
                            Me.WF_WRITE_CTNTOTAL.Text = CmnSetFmt.FormatComma(CTNRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_WRITE_CTNSYO.Text)
                        ElseIf CTNRow.Length = 0 Then
                            Me.WF_WRITE_CTNSYO.Text = "0"
                            Me.WF_WRITE_CTNTOTAL.Text = "0"
                        End If
                        WF_CTN2.Value = "0"
                        If Me.WF_WRITE_CTNSYO.Text = Me.WF_WRITE_CTNTOTAL.Text Then
                            WF_CTN2.Value = "1"
                        End If
                    ElseIf dt.Rows.Count = 0 Then
                        Me.WF_WRITE_HOKKAIDOSYO.Text = "0"
                        Me.WF_WRITE_HOKKAIDOTOTAL.Text = "0"
                        Me.WF_WRITE_TOUHOKUSYO.Text = "0"
                        Me.WF_WRITE_TOUHOKUTOTAL.Text = "0"
                        Me.WF_WRITE_KANTOUSYO.Text = "0"
                        Me.WF_WRITE_KANTOUTOTAL.Text = "0"
                        Me.WF_WRITE_TYUBUSYO.Text = "0"
                        Me.WF_WRITE_TYUBUTOTAL.Text = "0"
                        Me.WF_WRITE_KANSAISYO.Text = "0"
                        Me.WF_WRITE_KANSAITOTAL.Text = "0"
                        Me.WF_WRITE_KYUSYUSYO.Text = "0"
                        Me.WF_WRITE_KYUSYUTOTAL.Text = "0"
                        Me.WF_WRITE_CTNSYO.Text = "0"
                        Me.WF_WRITE_CTNTOTAL.Text = "0"
                        WF_Hokkaido2.Value = "1"
                        WF_Touhoku2.Value = "1"
                        WF_Kantou2.Value = "1"
                        WF_Tyubu2.Value = "1"
                        WF_Kansai2.Value = "1"
                        WF_Kyusyu2.Value = "1"
                        WF_CTN2.Value = "1"
                    End If

                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_ACQUISITION_ERROR, C_MESSAGE_TYPE.ABORT, "DBエラー LNT0019D GetWrite", needsPopUp:=True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019D Write"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' コンテナ売却取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Get_CtnSale()

        Dim dt = New DataTable

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("    1 AS FLG")
            SQLBldr.AppendLine("   ,TOTALTBL.ORGCODE AS ORGCODE")
            SQLBldr.AppendLine("   ,COUNT(TOTALTBL.ORGCODE) AS TOTAL")
            SQLBldr.AppendLine("FROM")
            SQLBldr.AppendLine("(")
            SQLBldr.AppendLine("    SELECT DISTINCT")
            SQLBldr.AppendLine("        INVOICEORGCODE AS ORGCODE")
            SQLBldr.AppendLine("       ,TORICODE")
            SQLBldr.AppendLine("    FROM lng.LNT0089_CONTAINER_STOCK")
            SQLBldr.AppendLine("    WHERE")
            SQLBldr.AppendLine("        DELFLG = @P02")
            SQLBldr.AppendLine("    AND KEIJOYM = @P01")
            SQLBldr.AppendLine("    AND INVOICEORGCODE IS NOT NULL")
            SQLBldr.AppendLine("    AND REQUESTFLG = '1'")
            SQLBldr.AppendLine(") TOTALTBL")
            SQLBldr.AppendLine("GROUP BY")
            SQLBldr.AppendLine("    TOTALTBL.ORGCODE")
            SQLBldr.AppendLine("UNION ALL")
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("    2 AS FLG")
            SQLBldr.AppendLine("   ,LNT0064.INVOICEORGCODE as ORGCODE")
            SQLBldr.AppendLine("   ,COUNT(LNT0064.INVOICENUMBER) AS TOTAL")
            SQLBldr.AppendLine("FROM lng.LNT0064_INVOICEHEAD LNT0064")
            SQLBldr.AppendLine("WHERE")
            SQLBldr.AppendLine("    DELFLG = @P02")
            SQLBldr.AppendLine("AND KEIJOYM = @P01")
            SQLBldr.AppendLine("AND REQUESTSTATUS= '5'")
            SQLBldr.AppendLine("AND INVOICETYPE = '5'")
            SQLBldr.AppendLine("GROUP BY")
            SQLBldr.AppendLine("      INVOICEORGCODE")
            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)     '計上年月
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)     '削除フラグ

                    PARA01.Value = Replace(txtDownloadMonth.Text, "/", "")
                    PARA02.Value = C_DELETE_FLG.ALIVE

                    'SQL実行
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                    If Not dt.Rows.Count = 0 Then
                        'データセット
                        '北海道
                        Dim hokkaidoRow As DataRow() = dt.Select("ORGCODE=010102")
                        If Not hokkaidoRow.Length = 0 Then
                            If hokkaidoRow.Count = 2 Then
                                Me.WF_CTNSALE_HOKKAIDOSYO.Text = CmnSetFmt.FormatComma(hokkaidoRow(1)("TOTAL").ToString(), True)
                                If Not hokkaidoRow(1)("TOTAL").ToString() = hokkaidoRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf hokkaidoRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALE_HOKKAIDOSYO.Text = "0"
                            End If
                            Me.WF_CTNSALE_HOKKAIDOTOTAL.Text = CmnSetFmt.FormatComma(hokkaidoRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALE_HOKKAIDOTOTAL.Text)
                        ElseIf hokkaidoRow.Length = 0 Then
                            Me.WF_CTNSALE_HOKKAIDOSYO.Text = "0"
                            Me.WF_CTNSALE_HOKKAIDOTOTAL.Text = "0"
                        End If
                        WF_Hokkaido3.Value = "0"
                        If Me.WF_CTNSALE_HOKKAIDOSYO.Text = Me.WF_CTNSALE_HOKKAIDOTOTAL.Text Then
                            WF_Hokkaido3.Value = "1"
                        End If
                        '東北
                        Dim touhokuRow As DataRow() = dt.Select("ORGCODE=010401")
                        If Not touhokuRow.Length = 0 Then
                            If touhokuRow.Count = 2 Then
                                Me.WF_CTNSALE_TOUHOKUSYO.Text = CmnSetFmt.FormatComma(touhokuRow(1)("TOTAL").ToString(), True)
                                If Not touhokuRow(1)("TOTAL").ToString() = touhokuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf touhokuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALE_TOUHOKUSYO.Text = "0"
                            End If
                            Me.WF_CTNSALE_TOUHOKUTOTAL.Text = CmnSetFmt.FormatComma(touhokuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALE_TOUHOKUTOTAL.Text)
                        ElseIf touhokuRow.Length = 0 Then
                            Me.WF_CTNSALE_TOUHOKUSYO.Text = "0"
                            Me.WF_CTNSALE_TOUHOKUTOTAL.Text = "0"
                        End If
                        WF_Touhoku3.Value = "0"
                        If Me.WF_CTNSALE_TOUHOKUSYO.Text = Me.WF_CTNSALE_TOUHOKUTOTAL.Text Then
                            WF_Touhoku3.Value = "1"
                        End If
                        '関東
                        Dim kantouRow As DataRow() = dt.Select("ORGCODE=011402")
                        If Not kantouRow.Length = 0 Then
                            If kantouRow.Count = 2 Then
                                Me.WF_CTNSALE_KANTOUSYO.Text = CmnSetFmt.FormatComma(kantouRow(1)("TOTAL").ToString(), True)
                                If Not kantouRow(1)("TOTAL").ToString() = kantouRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kantouRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALE_KANTOUSYO.Text = "0"
                            End If
                            Me.WF_CTNSALE_KANTOUTOTAL.Text = CmnSetFmt.FormatComma(kantouRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALE_KANTOUTOTAL.Text)
                        ElseIf kantouRow.Length = 0 Then
                            Me.WF_CTNSALE_KANTOUSYO.Text = "0"
                            Me.WF_CTNSALE_KANTOUTOTAL.Text = "0"
                        End If
                        WF_Kantou3.Value = "0"
                        If Me.WF_CTNSALE_KANTOUSYO.Text = Me.WF_CTNSALE_KANTOUTOTAL.Text Then
                            WF_Kantou3.Value = "1"
                        End If
                        '中部
                        Dim tyubuRow As DataRow() = dt.Select("ORGCODE=012401")
                        If Not tyubuRow.Length = 0 Then
                            If tyubuRow.Count = 2 Then
                                Me.WF_CTNSALE_TYUBUSYO.Text = CmnSetFmt.FormatComma(tyubuRow(1)("TOTAL").ToString(), True)
                                If Not tyubuRow(1)("TOTAL").ToString() = tyubuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf tyubuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALE_TYUBUSYO.Text = "0"
                            End If
                            Me.WF_CTNSALE_TYUBUTOTAL.Text = CmnSetFmt.FormatComma(tyubuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALE_TYUBUTOTAL.Text)
                        ElseIf tyubuRow.Length = 0 Then
                            Me.WF_CTNSALE_TYUBUSYO.Text = "0"
                            Me.WF_CTNSALE_TYUBUTOTAL.Text = "0"
                        End If
                        WF_Tyubu3.Value = "0"
                        If Me.WF_CTNSALE_TYUBUSYO.Text = Me.WF_CTNSALE_TYUBUTOTAL.Text Then
                            WF_Tyubu3.Value = "1"
                        End If
                        '関西
                        Dim kansaiRow As DataRow() = dt.Select("ORGCODE=012701")
                        If Not kansaiRow.Length = 0 Then
                            If kansaiRow.Count = 2 Then
                                Me.WF_CTNSALE_KANSAISYO.Text = CmnSetFmt.FormatComma(kansaiRow(1)("TOTAL").ToString(), True)
                                If Not kansaiRow(1)("TOTAL").ToString() = kansaiRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kansaiRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALE_KANSAISYO.Text = "0"
                            End If
                            Me.WF_CTNSALE_KANSAITOTAL.Text = CmnSetFmt.FormatComma(kansaiRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALE_KANSAITOTAL.Text)
                        ElseIf kansaiRow.Length = 0 Then
                            Me.WF_CTNSALE_KANSAISYO.Text = "0"
                            Me.WF_CTNSALE_KANSAITOTAL.Text = "0"
                        End If
                        WF_Kansai3.Value = "0"
                        If Me.WF_CTNSALE_KANSAISYO.Text = Me.WF_CTNSALE_KANSAITOTAL.Text Then
                            WF_Kansai3.Value = "1"
                        End If
                        '九州
                        Dim kyusyuRow As DataRow() = dt.Select("ORGCODE=014001")
                        If Not kyusyuRow.Length = 0 Then
                            If kyusyuRow.Count = 2 Then
                                Me.WF_CTNSALE_KYUSYUSYO.Text = CmnSetFmt.FormatComma(kyusyuRow(1)("TOTAL").ToString(), True)
                                If Not kyusyuRow(1)("TOTAL").ToString() = kyusyuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kyusyuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALE_KYUSYUSYO.Text = "0"
                            End If
                            Me.WF_CTNSALE_KYUSYUTOTAL.Text = CmnSetFmt.FormatComma(kyusyuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALE_KYUSYUTOTAL.Text)
                        ElseIf kyusyuRow.Length = 0 Then
                            Me.WF_CTNSALE_KYUSYUSYO.Text = "0"
                            Me.WF_CTNSALE_KYUSYUTOTAL.Text = "0"
                        End If
                        WF_Kyusyu3.Value = "0"
                        If Me.WF_CTNSALE_KYUSYUSYO.Text = Me.WF_CTNSALE_KYUSYUTOTAL.Text Then
                            WF_Kyusyu3.Value = "1"
                        End If
                        'コンテナ部
                        Dim CTNRow As DataRow() = dt.Select("ORGCODE=011312")
                        If Not CTNRow.Length = 0 Then
                            If CTNRow.Count = 2 Then
                                Me.WF_CTNSALE_CTNSYO.Text = CmnSetFmt.FormatComma(CTNRow(1)("TOTAL").ToString(), True)
                                If Not CTNRow(1)("TOTAL").ToString() = CTNRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf CTNRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALE_CTNSYO.Text = "0"
                            End If
                            Me.WF_CTNSALE_CTNTOTAL.Text = CmnSetFmt.FormatComma(CTNRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALE_CTNTOTAL.Text)
                        ElseIf CTNRow.Length = 0 Then
                            Me.WF_CTNSALE_CTNSYO.Text = "0"
                            Me.WF_CTNSALE_CTNTOTAL.Text = "0"
                        End If
                        WF_CTN3.Value = "0"
                        If Me.WF_CTNSALE_CTNSYO.Text = Me.WF_CTNSALE_CTNTOTAL.Text Then
                            WF_CTN3.Value = "1"
                        End If
                    ElseIf dt.Rows.Count = 0 Then
                        Me.WF_CTNSALE_HOKKAIDOSYO.Text = "0"
                        Me.WF_CTNSALE_HOKKAIDOTOTAL.Text = "0"
                        Me.WF_CTNSALE_TOUHOKUSYO.Text = "0"
                        Me.WF_CTNSALE_TOUHOKUTOTAL.Text = "0"
                        Me.WF_CTNSALE_KANTOUSYO.Text = "0"
                        Me.WF_CTNSALE_KANTOUTOTAL.Text = "0"
                        Me.WF_CTNSALE_TYUBUSYO.Text = "0"
                        Me.WF_CTNSALE_TYUBUTOTAL.Text = "0"
                        Me.WF_CTNSALE_KANSAISYO.Text = "0"
                        Me.WF_CTNSALE_KANSAITOTAL.Text = "0"
                        Me.WF_CTNSALE_KYUSYUSYO.Text = "0"
                        Me.WF_CTNSALE_KYUSYUTOTAL.Text = "0"
                        Me.WF_CTNSALE_CTNSYO.Text = "0"
                        Me.WF_CTNSALE_CTNTOTAL.Text = "0"
                        WF_Hokkaido3.Value = "1"
                        WF_Touhoku3.Value = "1"
                        WF_Kantou3.Value = "1"
                        WF_Tyubu3.Value = "1"
                        WF_Kansai3.Value = "1"
                        WF_Kyusyu3.Value = "1"
                        WF_CTN3.Value = "1"
                    End If
                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_ACQUISITION_ERROR, C_MESSAGE_TYPE.ABORT, "DBエラー LNT0019D GetCtnSale", needsPopUp:=True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019D GetCtnSale"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' コンテナ売却(原価計算)取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Get_CtnSaleCalculation()

        Dim dt = New DataTable

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("    1 AS FLG")
            SQLBldr.AppendLine("   ,TOTALTBL.ORGCODE AS ORGCODE")
            SQLBldr.AppendLine("   ,COUNT(TOTALTBL.ORGCODE) AS TOTAL")
            SQLBldr.AppendLine("FROM")
            SQLBldr.AppendLine("(")
            SQLBldr.AppendLine("    SELECT DISTINCT")
            SQLBldr.AppendLine("        INVOICEORGCODE AS ORGCODE")
            SQLBldr.AppendLine("       ,TORICODE")
            SQLBldr.AppendLine("    FROM lng.LNT0089_CONTAINER_STOCK")
            SQLBldr.AppendLine("    WHERE")
            SQLBldr.AppendLine("        DELFLG = @P02")
            SQLBldr.AppendLine("    AND KEIJOYM = @P01")
            SQLBldr.AppendLine("    AND INVOICEORGCODE IS NOT NULL")
            SQLBldr.AppendLine("    AND REQUESTFLG = '1'")
            SQLBldr.AppendLine("    AND NOOPERATINGFLG = '0'")
            SQLBldr.AppendLine("    UNION ALL")
            SQLBldr.AppendLine("    SELECT DISTINCT")
            SQLBldr.AppendLine("        INVOICEORGCODE AS ORGCODE")
            SQLBldr.AppendLine("       ,TORICODE")
            SQLBldr.AppendLine("    FROM lng.LNT0089_CONTAINER_STOCK")
            SQLBldr.AppendLine("    WHERE")
            SQLBldr.AppendLine("        DELFLG = @P02")
            SQLBldr.AppendLine("    AND KEIJOYM = @P01")
            SQLBldr.AppendLine("    AND STOCKSTATUS = '15'")
            SQLBldr.AppendLine(") TOTALTBL")
            SQLBldr.AppendLine("GROUP BY")
            SQLBldr.AppendLine("    TOTALTBL.ORGCODE")
            SQLBldr.AppendLine("UNION ALL")
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("    2 AS FLG")
            SQLBldr.AppendLine("   ,TOTALTBL.ORGCODE AS ORGCODE")
            SQLBldr.AppendLine("   ,COUNT(TOTALTBL.ORGCODE) AS TOTAL")
            SQLBldr.AppendLine("FROM")
            SQLBldr.AppendLine("(")
            SQLBldr.AppendLine("    SELECT DISTINCT")
            SQLBldr.AppendLine("        INVOICEORGCODE AS ORGCODE")
            SQLBldr.AppendLine("       ,TORICODE")
            SQLBldr.AppendLine("    FROM lng.LNT0089_CONTAINER_STOCK")
            SQLBldr.AppendLine("    WHERE")
            SQLBldr.AppendLine("        DELFLG = @P02")
            SQLBldr.AppendLine("    AND KEIJOYM = @P01")
            SQLBldr.AppendLine("    AND INVOICEORGCODE IS NOT NULL")
            SQLBldr.AppendLine("    AND REQUESTFLG = '1'")
            SQLBldr.AppendLine("    AND NOOPERATINGFLG = '0'")
            SQLBldr.AppendLine("    AND COSTACCOUNTINGDATE IS NOT NULL")
            SQLBldr.AppendLine("    UNION ALL")
            SQLBldr.AppendLine("    SELECT DISTINCT")
            SQLBldr.AppendLine("        INVOICEORGCODE AS ORGCODE")
            SQLBldr.AppendLine("       ,TORICODE")
            SQLBldr.AppendLine("    FROM lng.LNT0089_CONTAINER_STOCK")
            SQLBldr.AppendLine("    WHERE")
            SQLBldr.AppendLine("        DELFLG = @P02")
            SQLBldr.AppendLine("    AND KEIJOYM = @P01")
            SQLBldr.AppendLine("    AND COSTACCOUNTINGDATE IS NOT NULL")
            SQLBldr.AppendLine("    AND STOCKSTATUS = '15'")
            SQLBldr.AppendLine(") TOTALTBL")
            SQLBldr.AppendLine("GROUP BY")
            SQLBldr.AppendLine("    TOTALTBL.ORGCODE")
            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)     '計上年月
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)     '削除フラグ

                    PARA01.Value = Replace(txtDownloadMonth.Text, "/", "")
                    PARA02.Value = C_DELETE_FLG.ALIVE

                    'SQL実行
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                    If Not dt.Rows.Count = 0 Then
                        'データセット
                        '北海道
                        Dim hokkaidoRow As DataRow() = dt.Select("ORGCODE=010102")
                        If Not hokkaidoRow.Length = 0 Then
                            If hokkaidoRow.Count = 2 Then
                                Me.WF_CTNSALECALCULATION_HOKKAIDOSYO.Text = CmnSetFmt.FormatComma(hokkaidoRow(1)("TOTAL").ToString(), True)
                                If Not hokkaidoRow(1)("TOTAL").ToString() = hokkaidoRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf hokkaidoRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALECALCULATION_HOKKAIDOSYO.Text = "0"
                            End If
                            Me.WF_CTNSALECALCULATION_HOKKAIDOTOTAL.Text = CmnSetFmt.FormatComma(hokkaidoRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALECALCULATION_HOKKAIDOTOTAL.Text)
                        ElseIf hokkaidoRow.Length = 0 Then
                            Me.WF_CTNSALECALCULATION_HOKKAIDOSYO.Text = "0"
                            Me.WF_CTNSALECALCULATION_HOKKAIDOTOTAL.Text = "0"
                        End If
                        WF_Hokkaido5.Value = "0"
                        If Me.WF_CTNSALECALCULATION_HOKKAIDOSYO.Text = Me.WF_CTNSALECALCULATION_HOKKAIDOTOTAL.Text Then
                            WF_Hokkaido5.Value = "1"
                        End If
                        '東北
                        Dim touhokuRow As DataRow() = dt.Select("ORGCODE=010401")
                        If Not touhokuRow.Length = 0 Then
                            If touhokuRow.Count = 2 Then
                                Me.WF_CTNSALECALCULATION_TOUHOKUSYO.Text = CmnSetFmt.FormatComma(touhokuRow(1)("TOTAL").ToString(), True)
                                If Not touhokuRow(1)("TOTAL").ToString() = touhokuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf touhokuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALECALCULATION_TOUHOKUSYO.Text = "0"
                            End If
                            Me.WF_CTNSALECALCULATION_TOUHOKUTOTAL.Text = CmnSetFmt.FormatComma(touhokuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALECALCULATION_TOUHOKUTOTAL.Text)
                        ElseIf touhokuRow.Length = 0 Then
                            Me.WF_CTNSALECALCULATION_TOUHOKUSYO.Text = "0"
                            Me.WF_CTNSALECALCULATION_TOUHOKUTOTAL.Text = "0"
                        End If
                        WF_Touhoku5.Value = "0"
                        If Me.WF_CTNSALECALCULATION_TOUHOKUSYO.Text = Me.WF_CTNSALECALCULATION_TOUHOKUTOTAL.Text Then
                            WF_Touhoku5.Value = "1"
                        End If
                        '関東
                        Dim kantouRow As DataRow() = dt.Select("ORGCODE=011402")
                        If Not kantouRow.Length = 0 Then
                            If kantouRow.Count = 2 Then
                                Me.WF_CTNSALECALCULATION_KANTOUSYO.Text = CmnSetFmt.FormatComma(kantouRow(1)("TOTAL").ToString(), True)
                                If Not kantouRow(1)("TOTAL").ToString() = kantouRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kantouRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALECALCULATION_KANTOUSYO.Text = "0"
                            End If
                            Me.WF_CTNSALECALCULATION_KANTOUTOTAL.Text = CmnSetFmt.FormatComma(kantouRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALECALCULATION_KANTOUTOTAL.Text)
                        ElseIf kantouRow.Length = 0 Then
                            Me.WF_CTNSALECALCULATION_KANTOUSYO.Text = "0"
                            Me.WF_CTNSALECALCULATION_KANTOUTOTAL.Text = "0"
                        End If
                        WF_Kantou5.Value = "0"
                        If Me.WF_CTNSALECALCULATION_KANTOUSYO.Text = Me.WF_CTNSALECALCULATION_KANTOUTOTAL.Text Then
                            WF_Kantou5.Value = "1"
                        End If
                        '中部
                        Dim tyubuRow As DataRow() = dt.Select("ORGCODE=012401")
                        If Not tyubuRow.Length = 0 Then
                            If tyubuRow.Count = 2 Then
                                Me.WF_CTNSALECALCULATION_TYUBUSYO.Text = CmnSetFmt.FormatComma(tyubuRow(1)("TOTAL").ToString(), True)
                                If Not tyubuRow(1)("TOTAL").ToString() = tyubuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf tyubuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALECALCULATION_TYUBUSYO.Text = "0"
                            End If
                            Me.WF_CTNSALECALCULATION_TYUBUTOTAL.Text = CmnSetFmt.FormatComma(tyubuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALECALCULATION_TYUBUTOTAL.Text)
                        ElseIf tyubuRow.Length = 0 Then
                            Me.WF_CTNSALECALCULATION_TYUBUSYO.Text = "0"
                            Me.WF_CTNSALECALCULATION_TYUBUTOTAL.Text = "0"
                        End If
                        WF_Tyubu5.Value = "0"
                        If Me.WF_CTNSALECALCULATION_TYUBUSYO.Text = Me.WF_CTNSALECALCULATION_TYUBUTOTAL.Text Then
                            WF_Tyubu5.Value = "1"
                        End If
                        '関西
                        Dim kansaiRow As DataRow() = dt.Select("ORGCODE=012701")
                        If Not kansaiRow.Length = 0 Then
                            If kansaiRow.Count = 2 Then
                                Me.WF_CTNSALECALCULATION_KANSAISYO.Text = CmnSetFmt.FormatComma(kansaiRow(1)("TOTAL").ToString(), True)
                                If Not kansaiRow(1)("TOTAL").ToString() = kansaiRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kansaiRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALECALCULATION_KANSAISYO.Text = "0"
                            End If
                            Me.WF_CTNSALECALCULATION_KANSAITOTAL.Text = CmnSetFmt.FormatComma(kansaiRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALECALCULATION_KANSAITOTAL.Text)
                        ElseIf kansaiRow.Length = 0 Then
                            Me.WF_CTNSALECALCULATION_KANSAISYO.Text = "0"
                            Me.WF_CTNSALECALCULATION_KANSAITOTAL.Text = "0"
                        End If
                        WF_Kansai5.Value = "0"
                        If Me.WF_CTNSALECALCULATION_KANSAISYO.Text = Me.WF_CTNSALECALCULATION_KANSAITOTAL.Text Then
                            WF_Kansai5.Value = "1"
                        End If
                        '九州
                        Dim kyusyuRow As DataRow() = dt.Select("ORGCODE=014001")
                        If Not kyusyuRow.Length = 0 Then
                            If kyusyuRow.Count = 2 Then
                                Me.WF_CTNSALECALCULATION_KYUSYUSYO.Text = CmnSetFmt.FormatComma(kyusyuRow(1)("TOTAL").ToString(), True)
                                If Not kyusyuRow(1)("TOTAL").ToString() = kyusyuRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf kyusyuRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALECALCULATION_KYUSYUSYO.Text = "0"
                            End If
                            Me.WF_CTNSALECALCULATION_KYUSYUTOTAL.Text = CmnSetFmt.FormatComma(kyusyuRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALECALCULATION_KYUSYUTOTAL.Text)
                        ElseIf kyusyuRow.Length = 0 Then
                            Me.WF_CTNSALECALCULATION_KYUSYUSYO.Text = "0"
                            Me.WF_CTNSALECALCULATION_KYUSYUTOTAL.Text = "0"
                        End If
                        WF_Kyusyu5.Value = "0"
                        If Me.WF_CTNSALECALCULATION_KYUSYUSYO.Text = Me.WF_CTNSALECALCULATION_KYUSYUTOTAL.Text Then
                            WF_Kyusyu5.Value = "1"
                        End If
                        'コンテナ部
                        Dim CTNRow As DataRow() = dt.Select("ORGCODE=011312")
                        If Not CTNRow.Length = 0 Then
                            If CTNRow.Count = 2 Then
                                Me.WF_CTNSALECALCULATION_CTNSYO.Text = CmnSetFmt.FormatComma(CTNRow(1)("TOTAL").ToString(), True)
                                If Not CTNRow(1)("TOTAL").ToString() = CTNRow(0)("TOTAL").ToString() Then
                                    GBL_MISYO_FLG = False
                                End If
                            ElseIf CTNRow.Count = 1 Then
                                GBL_MISYO_FLG = False
                                Me.WF_CTNSALECALCULATION_CTNSYO.Text = "0"
                            End If
                            Me.WF_CTNSALECALCULATION_CTNTOTAL.Text = CmnSetFmt.FormatComma(CTNRow(0)("TOTAL").ToString(), True)
                            GBL_TOTAL += CInt(Me.WF_CTNSALECALCULATION_CTNTOTAL.Text)
                        ElseIf CTNRow.Length = 0 Then
                            Me.WF_CTNSALECALCULATION_CTNSYO.Text = "0"
                            Me.WF_CTNSALECALCULATION_CTNTOTAL.Text = "0"
                        End If
                        WF_CTN5.Value = "0"
                        If Me.WF_CTNSALECALCULATION_CTNSYO.Text = Me.WF_CTNSALECALCULATION_CTNTOTAL.Text Then
                            WF_CTN5.Value = "1"
                        End If
                    ElseIf dt.Rows.Count = 0 Then
                        Me.WF_CTNSALECALCULATION_HOKKAIDOSYO.Text = "0"
                        Me.WF_CTNSALECALCULATION_HOKKAIDOTOTAL.Text = "0"
                        Me.WF_CTNSALECALCULATION_TOUHOKUSYO.Text = "0"
                        Me.WF_CTNSALECALCULATION_TOUHOKUTOTAL.Text = "0"
                        Me.WF_CTNSALECALCULATION_KANTOUSYO.Text = "0"
                        Me.WF_CTNSALECALCULATION_KANTOUTOTAL.Text = "0"
                        Me.WF_CTNSALECALCULATION_TYUBUSYO.Text = "0"
                        Me.WF_CTNSALECALCULATION_TYUBUTOTAL.Text = "0"
                        Me.WF_CTNSALECALCULATION_KANSAISYO.Text = "0"
                        Me.WF_CTNSALECALCULATION_KANSAITOTAL.Text = "0"
                        Me.WF_CTNSALECALCULATION_KYUSYUSYO.Text = "0"
                        Me.WF_CTNSALECALCULATION_KYUSYUTOTAL.Text = "0"
                        Me.WF_CTNSALECALCULATION_CTNSYO.Text = "0"
                        Me.WF_CTNSALECALCULATION_CTNTOTAL.Text = "0"
                        WF_Hokkaido5.Value = "1"
                        WF_Touhoku5.Value = "1"
                        WF_Kantou5.Value = "1"
                        WF_Tyubu5.Value = "1"
                        WF_Kansai5.Value = "1"
                        WF_Kyusyu5.Value = "1"
                        WF_CTN5.Value = "1"
                    End If
                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_ACQUISITION_ERROR, C_MESSAGE_TYPE.ABORT, "DBエラー LNT0019D Get_CtnSaleCalculation", needsPopUp:=True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019D Get_CtnSaleCalculation"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 支払料取得処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub Get_Payment()

        Dim dt = New DataTable

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("    1 AS FLG")
            SQLBldr.AppendLine("    ,MAIN.PAYFILINGBRANCH as ORGCODE")
            SQLBldr.AppendLine("    ,COUNT(MAIN.TORICODE) AS TOTAL")
            SQLBldr.AppendLine("FROM ( SELECT DISTINCT")
            SQLBldr.AppendLine("	       PAYFILINGBRANCH")
            SQLBldr.AppendLine("		  ,TORICODE")
            SQLBldr.AppendLine("	   FROM lng.LNT0017_RESSNF")
            SQLBldr.AppendLine("       WHERE")
            SQLBldr.AppendLine("           DELFLG = @P02")
            SQLBldr.AppendLine("           AND STACKFREEKBN = '2'")
            SQLBldr.AppendLine("           AND ACCOUNTSTATUSKBN IN ('3', '4', '5', '6', '7', '9')")
            SQLBldr.AppendLine("           AND ACCOUNTINGASSETSKBN = '1'")
            SQLBldr.AppendLine("           AND JURISDICTIONCD = '14' ")
            SQLBldr.AppendLine("           AND KEIJOYM = @P01")
            SQLBldr.AppendLine("           AND TOTALCOST <> 0")
            SQLBldr.AppendLine("	       AND PAYFILINGBRANCH IS NOT NULL")
            SQLBldr.AppendLine("	 ) MAIN")
            SQLBldr.AppendLine("GROUP BY ")
            SQLBldr.AppendLine("      PAYFILINGBRANCH")
            SQLBldr.AppendLine("UNION ALL")
            SQLBldr.AppendLine("SELECT")
            SQLBldr.AppendLine("    2 AS FLG")
            SQLBldr.AppendLine("    ,LNT0077.PAYMENTORGCODE as ORGCODE")
            SQLBldr.AppendLine("    ,COUNT(LNT0077.TORICODE) AS TOTAL")
            SQLBldr.AppendLine("FROM lng.LNT0077_PAYMENTHEAD LNT0077")
            SQLBldr.AppendLine("       WHERE")
            SQLBldr.AppendLine("           DELFLG = @P02")
            SQLBldr.AppendLine("           AND PAYMENTYM = @P01")
            SQLBldr.AppendLine("		   AND REQUESTSTATUS= '5'")
            SQLBldr.AppendLine("GROUP BY ")
            SQLBldr.AppendLine("      PAYMENTORGCODE")
            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar)     '計上年月
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)     '削除フラグ

                    PARA01.Value = Replace(txtDownloadMonth.Text, "/", "")
                    PARA02.Value = C_DELETE_FLG.ALIVE

                    'SQL実行
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using

                    If Not dt.Rows.Count = 0 Then
                        'データセット
                        '北海道
                        Dim hokkaidoRow As DataRow() = dt.Select("ORGCODE=010102")
                        If Not hokkaidoRow.Length = 0 Then
                            If hokkaidoRow.Count = 2 Then
                                Me.WF_PAYMENT_HOKKAIDOSYO.Text = CmnSetFmt.FormatComma(hokkaidoRow(1)("TOTAL").ToString(), True)
                                If Not hokkaidoRow(1)("TOTAL").ToString() = hokkaidoRow(0)("TOTAL").ToString() Then
                                    GBL_PAYMENTMISYO_FLG = False
                                End If
                            ElseIf hokkaidoRow.Count = 1 Then
                                GBL_PAYMENTMISYO_FLG = False
                                Me.WF_PAYMENT_HOKKAIDOSYO.Text = "0"
                            End If
                            Me.WF_PAYMENT_HOKKAIDOTOTAL.Text = CmnSetFmt.FormatComma(hokkaidoRow(0)("TOTAL").ToString(), True)
                        ElseIf hokkaidoRow.Length = 0 Then
                            Me.WF_PAYMENT_HOKKAIDOSYO.Text = "0"
                            Me.WF_PAYMENT_HOKKAIDOTOTAL.Text = "0"
                        End If
                        WF_Hokkaido4.Value = "0"
                        If Me.WF_PAYMENT_HOKKAIDOSYO.Text = Me.WF_PAYMENT_HOKKAIDOTOTAL.Text Then
                            WF_Hokkaido4.Value = "1"
                        End If
                        '東北
                        Dim touhokuRow As DataRow() = dt.Select("ORGCODE=010401")
                        If Not touhokuRow.Length = 0 Then
                            If touhokuRow.Count = 2 Then
                                Me.WF_PAYMENT_TOUHOKUSYO.Text = CmnSetFmt.FormatComma(touhokuRow(1)("TOTAL").ToString(), True)
                                If Not touhokuRow(1)("TOTAL").ToString() = touhokuRow(0)("TOTAL").ToString() Then
                                    GBL_PAYMENTMISYO_FLG = False
                                End If
                            ElseIf touhokuRow.Count = 1 Then
                                GBL_PAYMENTMISYO_FLG = False
                                Me.WF_PAYMENT_TOUHOKUSYO.Text = "0"
                            End If
                            Me.WF_PAYMENT_TOUHOKUTOTAL.Text = CmnSetFmt.FormatComma(touhokuRow(0)("TOTAL").ToString(), True)
                        ElseIf touhokuRow.Length = 0 Then
                            Me.WF_PAYMENT_TOUHOKUSYO.Text = "0"
                            Me.WF_PAYMENT_TOUHOKUTOTAL.Text = "0"
                        End If
                        WF_Touhoku4.Value = "0"
                        If Me.WF_PAYMENT_TOUHOKUSYO.Text = Me.WF_PAYMENT_TOUHOKUTOTAL.Text Then
                            WF_Touhoku4.Value = "1"
                        End If
                        '関東
                        Dim kantouRow As DataRow() = dt.Select("ORGCODE=011402")
                        If Not kantouRow.Length = 0 Then
                            If kantouRow.Count = 2 Then
                                Me.WF_PAYMENT_KANTOUSYO.Text = CmnSetFmt.FormatComma(kantouRow(1)("TOTAL").ToString(), True)
                                If Not kantouRow(1)("TOTAL").ToString() = kantouRow(0)("TOTAL").ToString() Then
                                    GBL_PAYMENTMISYO_FLG = False
                                End If
                            ElseIf kantouRow.Count = 1 Then
                                GBL_PAYMENTMISYO_FLG = False
                                Me.WF_PAYMENT_KANTOUSYO.Text = "0"
                            End If
                            Me.WF_PAYMENT_KANTOUTOTAL.Text = CmnSetFmt.FormatComma(kantouRow(0)("TOTAL").ToString(), True)
                        ElseIf kantouRow.Length = 0 Then
                            Me.WF_PAYMENT_KANTOUSYO.Text = "0"
                            Me.WF_PAYMENT_KANTOUTOTAL.Text = "0"
                        End If
                        WF_Kantou4.Value = "0"
                        If Me.WF_PAYMENT_KANTOUSYO.Text = Me.WF_PAYMENT_KANTOUTOTAL.Text Then
                            WF_Kantou4.Value = "1"
                        End If
                        '中部
                        Dim tyubuRow As DataRow() = dt.Select("ORGCODE=012401")
                        If Not tyubuRow.Length = 0 Then
                            If tyubuRow.Count = 2 Then
                                Me.WF_PAYMENT_TYUBUSYO.Text = CmnSetFmt.FormatComma(tyubuRow(1)("TOTAL").ToString(), True)
                                If Not tyubuRow(1)("TOTAL").ToString() = tyubuRow(0)("TOTAL").ToString() Then
                                    GBL_PAYMENTMISYO_FLG = False
                                End If
                            ElseIf tyubuRow.Count = 1 Then
                                GBL_PAYMENTMISYO_FLG = False
                                Me.WF_PAYMENT_TYUBUSYO.Text = "0"
                            End If
                            Me.WF_PAYMENT_TYUBUTOTAL.Text = CmnSetFmt.FormatComma(tyubuRow(0)("TOTAL").ToString(), True)
                        ElseIf tyubuRow.Length = 0 Then
                            Me.WF_PAYMENT_TYUBUSYO.Text = "0"
                            Me.WF_PAYMENT_TYUBUTOTAL.Text = "0"
                        End If
                        WF_Tyubu4.Value = "0"
                        If Me.WF_PAYMENT_TYUBUSYO.Text = Me.WF_PAYMENT_TYUBUTOTAL.Text Then
                            WF_Tyubu4.Value = "1"
                        End If
                        '関西
                        Dim kansaiRow As DataRow() = dt.Select("ORGCODE=012701")
                        If Not kansaiRow.Length = 0 Then
                            If kansaiRow.Count = 2 Then
                                Me.WF_PAYMENT_KANSAISYO.Text = CmnSetFmt.FormatComma(kansaiRow(1)("TOTAL").ToString(), True)
                                If Not kansaiRow(1)("TOTAL").ToString() = kansaiRow(0)("TOTAL").ToString() Then
                                    GBL_PAYMENTMISYO_FLG = False
                                End If
                            ElseIf kansaiRow.Count = 1 Then
                                GBL_PAYMENTMISYO_FLG = False
                                Me.WF_PAYMENT_KANSAISYO.Text = "0"
                            End If
                            Me.WF_PAYMENT_KANSAITOTAL.Text = CmnSetFmt.FormatComma(kansaiRow(0)("TOTAL").ToString(), True)
                        ElseIf kansaiRow.Length = 0 Then
                            Me.WF_PAYMENT_KANSAISYO.Text = "0"
                            Me.WF_PAYMENT_KANSAITOTAL.Text = "0"
                        End If
                        WF_Kansai4.Value = "0"
                        If Me.WF_PAYMENT_KANSAISYO.Text = Me.WF_PAYMENT_KANSAITOTAL.Text Then
                            WF_Kansai4.Value = "1"
                        End If
                        '九州
                        Dim kyusyuRow As DataRow() = dt.Select("ORGCODE=014001")
                        If Not kyusyuRow.Length = 0 Then
                            If kyusyuRow.Count = 2 Then
                                Me.WF_PAYMENT_KYUSYUSYO.Text = CmnSetFmt.FormatComma(kyusyuRow(1)("TOTAL").ToString(), True)
                                If Not kyusyuRow(1)("TOTAL").ToString() = kyusyuRow(0)("TOTAL").ToString() Then
                                    GBL_PAYMENTMISYO_FLG = False
                                End If
                            ElseIf kyusyuRow.Count = 1 Then
                                GBL_PAYMENTMISYO_FLG = False
                                Me.WF_PAYMENT_KYUSYUSYO.Text = "0"
                            End If
                            Me.WF_PAYMENT_KYUSYUTOTAL.Text = CmnSetFmt.FormatComma(kyusyuRow(0)("TOTAL").ToString(), True)
                        ElseIf kyusyuRow.Length = 0 Then
                            Me.WF_PAYMENT_KYUSYUSYO.Text = "0"
                            Me.WF_PAYMENT_KYUSYUTOTAL.Text = "0"
                        End If
                        WF_Kyusyu4.Value = "0"
                        If Me.WF_PAYMENT_KYUSYUSYO.Text = Me.WF_PAYMENT_KYUSYUTOTAL.Text Then
                            WF_Kyusyu4.Value = "1"
                        End If
                        'コンテナ部
                        Dim CTNRow As DataRow() = dt.Select("ORGCODE=011312")
                        If Not CTNRow.Length = 0 Then
                            If CTNRow.Count = 2 Then
                                Me.WF_PAYMENT_CTNSYO.Text = CmnSetFmt.FormatComma(CTNRow(1)("TOTAL").ToString(), True)
                                If Not CTNRow(1)("TOTAL").ToString() = CTNRow(0)("TOTAL").ToString() Then
                                    GBL_PAYMENTMISYO_FLG = False
                                End If
                            ElseIf CTNRow.Count = 1 Then
                                GBL_PAYMENTMISYO_FLG = False
                                Me.WF_PAYMENT_CTNSYO.Text = "0"
                            End If
                            Me.WF_PAYMENT_CTNTOTAL.Text = CmnSetFmt.FormatComma(CTNRow(0)("TOTAL").ToString(), True)
                        ElseIf CTNRow.Length = 0 Then
                            Me.WF_PAYMENT_CTNSYO.Text = "0"
                            Me.WF_PAYMENT_CTNTOTAL.Text = "0"
                        End If
                        WF_CTN4.Value = "0"
                        If Me.WF_PAYMENT_CTNSYO.Text = Me.WF_PAYMENT_CTNTOTAL.Text Then
                            WF_CTN4.Value = "1"
                        End If
                    ElseIf dt.Rows.Count = 0 Then
                        Me.WF_PAYMENT_HOKKAIDOSYO.Text = "0"
                        Me.WF_PAYMENT_HOKKAIDOTOTAL.Text = "0"
                        Me.WF_PAYMENT_TOUHOKUSYO.Text = "0"
                        Me.WF_PAYMENT_TOUHOKUTOTAL.Text = "0"
                        Me.WF_PAYMENT_KANTOUSYO.Text = "0"
                        Me.WF_PAYMENT_KANTOUTOTAL.Text = "0"
                        Me.WF_PAYMENT_TYUBUSYO.Text = "0"
                        Me.WF_PAYMENT_TYUBUTOTAL.Text = "0"
                        Me.WF_PAYMENT_KANSAISYO.Text = "0"
                        Me.WF_PAYMENT_KANSAITOTAL.Text = "0"
                        Me.WF_PAYMENT_KYUSYUSYO.Text = "0"
                        Me.WF_PAYMENT_KYUSYUTOTAL.Text = "0"
                        Me.WF_PAYMENT_CTNSYO.Text = "0"
                        Me.WF_PAYMENT_CTNTOTAL.Text = "0"
                        WF_Hokkaido4.Value = "1"
                        WF_Touhoku4.Value = "1"
                        WF_Kantou4.Value = "1"
                        WF_Tyubu4.Value = "1"
                        WF_Kansai4.Value = "1"
                        WF_Kyusyu4.Value = "1"
                        WF_CTN4.Value = "1"
                    End If

                End Using

            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.CTN_ACQUISITION_ERROR, C_MESSAGE_TYPE.ABORT, "DBエラー LNT0019D GetPayment", needsPopUp:=True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019D Payment"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 締め状態検索処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Function Select_Close() As DataTable

        Dim para_Datetime As DateTime = DateTime.Now
        Dim dt = New DataTable

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine("  SELECT *")
            SQLBldr.AppendLine("FROM LNG.LNT0081_ACCOUNT_CLOSE")
            SQLBldr.AppendLine("WHERE")
            SQLBldr.AppendLine("    CLOSETYPE = '1'")
            SQLBldr.AppendLine("    AND KEIJOYM = @P02")
            SQLBldr.AppendLine("    AND DELFLG = @P03")
            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.Int32)       '計上年月
                    Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar)  '削除フラグ

                    PARA02.Value = Replace(txtDownloadMonth.Text, "/", "")
                    PARA03.Value = C_DELETE_FLG.ALIVE

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
                Master.Output(C_MESSAGE_NO.CTN_ACQUISITION_ERROR, C_MESSAGE_TYPE.ABORT, "DBエラー LNT0019D InsertClose", needsPopUp:=True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019D InsertClose"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            End Try

        End Using

        Return dt

    End Function

    ''' <summary>
    ''' 締め状態登録処理
    ''' </summary>
    ''' <param name="Confirm">締め状態</param>
    ''' <remarks></remarks>
    Private Sub Insert_Close(Confirm As String)

        Dim para_Datetime As DateTime = DateTime.Now
        Dim dt = New DataTable

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine("INSERT INTO LNG.LNT0081_ACCOUNT_CLOSE (")
            SQLBldr.AppendLine("     CLOSETYPE")
            SQLBldr.AppendLine("   , KEIJOYM")
            SQLBldr.AppendLine("   , CLOSESTATUS")
            SQLBldr.AppendLine("   , LASTUPDUSER")
            SQLBldr.AppendLine("   , LASTUPDYMD")
            SQLBldr.AppendLine("   , DELFLG")
            SQLBldr.AppendLine("   , INITYMD")
            SQLBldr.AppendLine("   , INITUSER")
            SQLBldr.AppendLine("   , INITTERMID")
            SQLBldr.AppendLine("   , INITPGID")
            SQLBldr.AppendLine("   , UPDYMD")
            SQLBldr.AppendLine("   , UPDUSER")
            SQLBldr.AppendLine("   , UPDTERMID")
            SQLBldr.AppendLine(")")
            SQLBldr.AppendLine("VALUES (")
            SQLBldr.AppendLine("     '1'")
            SQLBldr.AppendLine("   , @P01")
            SQLBldr.AppendLine("   , @P07")
            SQLBldr.AppendLine("   , @P02")
            SQLBldr.AppendLine("   , @P03")
            SQLBldr.AppendLine("   , @P04")
            SQLBldr.AppendLine("   , @P03")
            SQLBldr.AppendLine("   , @P02")
            SQLBldr.AppendLine("   , @P05")
            SQLBldr.AppendLine("   , @P06")
            SQLBldr.AppendLine("   , @P03")
            SQLBldr.AppendLine("   , @P02")
            SQLBldr.AppendLine("   , @P05")
            SQLBldr.AppendLine(")")
            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.Int32)        '計上年月
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)   'ユーザーID
                    Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.DateTime)   'システム日付
                    Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar)   '削除フラグ
                    Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar)   '端末
                    Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar)   'プログラムID
                    Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar)   '締め状態

                    PARA01.Value = Replace(txtDownloadMonth.Text, "/", "")
                    PARA02.Value = Master.USERID
                    PARA03.Value = para_Datetime
                    PARA04.Value = C_DELETE_FLG.ALIVE
                    PARA05.Value = Master.USERTERMID
                    PARA06.Value = Me.GetType().BaseType.Name
                    PARA07.Value = Confirm

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
                Master.Output(C_MESSAGE_NO.CTN_ACQUISITION_ERROR, C_MESSAGE_TYPE.ABORT, "DBエラー LNT0019D InsertClose", needsPopUp:=True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019D InsertClose"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 締め状態更新処理
    ''' </summary>
    ''' <param name="Confirm">締め状態</param>
    ''' <remarks></remarks>
    Private Sub Update_Close(Confirm As String)

        Dim para_Datetime As DateTime = DateTime.Now
        Dim dt = New DataTable

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            Dim SQLBldr As New StringBuilder
            SQLBldr.AppendLine("UPDATE LNG.LNT0081_ACCOUNT_CLOSE")
            SQLBldr.AppendLine("SET ")
            SQLBldr.AppendLine("    CLOSESTATUS = @P02")
            SQLBldr.AppendLine("  , LASTUPDUSER = @P03")
            SQLBldr.AppendLine("  , LASTUPDYMD = @P04")
            SQLBldr.AppendLine("  , DELFLG = @P05")
            SQLBldr.AppendLine("  , INITPGID = @P07")
            SQLBldr.AppendLine("  , UPDYMD = @P04")
            SQLBldr.AppendLine("  , UPDUSER = @P03")
            SQLBldr.AppendLine("  , UPDTERMID = @P06")
            SQLBldr.AppendLine("WHERE")
            SQLBldr.AppendLine("    CLOSETYPE = '1'")
            SQLBldr.AppendLine("    AND KEIJOYM = @P01")

            Try
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)

                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.Int32)        '計上年月
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar)   '締め状態
                    Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar)   'ユーザーID
                    Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.DateTime)   'システム日付
                    Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar)   '削除フラグ
                    Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar)   '端末
                    Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar)   'プログラムID

                    PARA01.Value = Replace(txtDownloadMonth.Text, "/", "")
                    PARA02.Value = Confirm
                    PARA03.Value = Master.USERID
                    PARA04.Value = para_Datetime
                    PARA05.Value = C_DELETE_FLG.ALIVE
                    PARA06.Value = Master.USERTERMID
                    PARA07.Value = Me.GetType().BaseType.Name

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
                Master.Output(C_MESSAGE_NO.CTN_ACQUISITION_ERROR, C_MESSAGE_TYPE.ABORT, "DBエラー LNT0019D UpdateClose", needsPopUp:=True)

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNT0019D UpdateClose"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            End Try

        End Using

    End Sub

    ''' <summary>
    ''' 計上年月初期値取得
    ''' </summary>
    ''' <returns></returns>
    Public Function getKeijoYM() As String
        Dim keijoYm As String = Format(Now, "yyyyMM")
        Dim CS0050Session As New CS0050SESSION
        Dim sqlStat As New StringBuilder

        sqlStat.AppendLine("SELECT TOP 1")
        sqlStat.AppendLine("       KEIJOYM AS KEIJOYM")
        sqlStat.AppendLine("  FROM LNG.LNT0081_ACCOUNT_CLOSE with(nolock)")
        sqlStat.AppendLine(" WHERE CLOSETYPE = '1'")
        sqlStat.AppendLine("   And CLOSESTATUS = '0'")
        sqlStat.AppendLine("   And DELFLG = @DELFLG")
        sqlStat.AppendLine(" ORDER BY KEIJOYM DESC")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(sqlStat.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@DELFLG", MySqlDbType.VarChar).Value = C_DELETE_FLG.ALIVE
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        Return keijoYm
                    End If
                    While sqlDr.Read
                        keijoYm = sqlDr("KEIJOYM").ToString
                    End While
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_READDATA_ERR, C_MESSAGE_TYPE.ABORT, "LNT0019D getKeijoYM", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0019D getKeijoYM"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.CTN_READDATA_ERR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try

        Return keijoYm

    End Function

    ''' <summary>
    ''' リースデータ未計上・計上済 更新処理
    ''' </summary>
    ''' <param name="strPrmKeizyoYM">計上年月</param>
    ''' <param name="strPrmKeizyoFlg">計上フラグ</param>
    ''' <param name="strPrmUpdUser">更新ユーザ</param>
    ''' <param name="strPrmUpdTermId">更新端末</param>
    ''' <returns>True:正常、False:異常</returns>
    ''' <remarks></remarks>
    Private Function UpdStoredLeaseSime(ByVal strPrmKeizyoYM As String, ByVal strPrmKeizyoFlg As String,
                                        ByVal strPrmUpdUser As String, ByVal strPrmUpdTermId As String) As Boolean

        Dim htCodeChange As New Hashtable
        Dim param As New Dictionary(Of String, String)

        UpdStoredLeaseSime = False

        Try
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                'パラメータ作成
                param.Add("@piKEIJOYM", strPrmKeizyoYM)         '計上年月
                param.Add("@piKEIJYOFLG", strPrmKeizyoFlg)      '計上フラグ
                param.Add("@piUPDUSER", strPrmUpdUser)          '更新ユーザ
                param.Add("@piUPDTERMID", strPrmUpdTermId)      '更新端末
                param.Add("@piVBFLG", "1")                      'VBから呼ばれたかのフラグ
                '戻り値 VBでは未使用
                param.Add("@poDATAFLG", "")

                Dim dtTable As DataTable = Nothing
                'リースデータ ストアド実行
                CS0050SESSION.executeStoredSQL(SQLcon, C_STORED_NAME.CTN_UPD_LEASEKEIZYO, param, dtTable)
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.CTN_LEASE_UPD_ERR, C_MESSAGE_TYPE.ERR, "DBエラー LNT0005D UpdStoredLease", needsPopUp:=True)
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0019D UpdStoredLeaseSime"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.CTN_LEASE_UPD_ERR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            Exit Function
        End Try

        '返却
        UpdStoredLeaseSime = True

    End Function

#End Region

    ''' <summary>
    ''' 検索パラメータ設定処理(主キー)
    ''' </summary>
    ''' <returns>請求ヘッダーデータ　設定したパラメータ</returns>
    ''' <remarks></remarks>
    Private Function SetSelectAccountingParam(sqlCon As MySqlConnection) As Hashtable

        Dim htApplDataTbl As New Hashtable
        Dim WW_DateNow As DateTime = Date.Now

        htApplDataTbl(SELECT_ACCOUNTING_KEY.SP_KEIJOYM) = Replace(txtDownloadMonth.Text, "/", "")                                                         '請求年月(計上年月)
        htApplDataTbl(SELECT_ACCOUNTING_KEY.SP_CREATEDATE) = Replace(WW_DateNow.ToShortDateString, "/", "")                                               '作成日付
        htApplDataTbl(SELECT_ACCOUNTING_KEY.SP_CREATETIME) = Replace(WW_DateNow.ToLongTimeString, ":", "")                                                '作成時間
        htApplDataTbl(SELECT_ACCOUNTING_KEY.SP_ZERIT) = EntryAccountingData.SelectZERIT(sqlCon, htApplDataTbl(SELECT_ACCOUNTING_KEY.SP_KEIJOYM).ToString) '消費税率
        htApplDataTbl(SELECT_ACCOUNTING_KEY.SP_USERID) = Master.USERID                                                                                    'ユーザーID
        htApplDataTbl(SELECT_ACCOUNTING_KEY.SP_USERTERMID) = Master.USERTERMID                                                                            '登録端末
        htApplDataTbl(SELECT_ACCOUNTING_KEY.SP_DELFLG) = C_DELETE_FLG.ALIVE                                                                               '削除フラグ

        Return htApplDataTbl

    End Function


End Class