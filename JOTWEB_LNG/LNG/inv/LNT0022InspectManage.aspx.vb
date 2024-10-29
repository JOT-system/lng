''************************************************************
' コンテナ検査管理
' 作成日 2023/08/25
' 更新日 2023/09/20
' 作成者 伊草
' 更新者 
'
' 修正履歴 : 2023/08/25 新規作成
'          : 2023/09/08 ①検査種別＝3(洗浄)を追加
'          :            ②検査管理テーブルに点検修理者を追加
'          : 2023/09/11 検査管理テーブルに実施場所を追加
'          : 2023/09/14 実施場所、点検修理者を任意入力に修正
'          : 2023/09/20 「コンテナ番号」列を追加
''************************************************************

Imports GrapeCity.Documents.Excel
Imports Newtonsoft.Json
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0022InspeLNManage
    Inherits System.Web.UI.Page

#Region "定数・変数・関数宣言"
    '○ 検索結果格納Table
    Private LNT0022tbl As DataTable = Nothing       'コンテナＭ
    Private LNT0022SUBtbl As DataTable = Nothing    'コンテナ検査日Ｔ

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite    'ログ出力
    Private CS0050SESSION As New CS0050SESSION      'セッション情報操作処理

    '○ 共通処理結果
    Private WW_DUMMY As String = ""
    Private WW_NARROW_DOWN As String = ""
    Private WW_UPDATED As Boolean = False

    '○ 共通定数
    Private Const CONST_MAX_ROW_CNT As Integer = 50 '頁あたり最大明細数
#End Region

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then

                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then

                    Dim researchFlg As Boolean = False

                    '◯ フラグ初期化
                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonEND"                     '戻るボタン押下
                            WF_ButtonEND_Click()

                        Case "WF_Field_DBClick"                 'フィールドダブルクリック
                            WF_FIELD_DBClick()

                        Case "WF_ButtonSel"                     '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()

                        Case "WF_ButtonCan"                     '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()

                        Case "WF_ListboxDBclick"                '左ボックスダブルクリック
                            WF_ButtonSel_Click()

                        Case "WF_btnFirstPage"                  '先頭頁へ移動
                            WF_btnFirstPage_Click()

                        Case "WF_btnBackPage"                   '前の頁へ移動
                            WF_btnBackPage_Click()

                        Case "WF_btnNextPage"                   '次の頁へ移動
                            WF_btnNextPage_Click()

                        Case "WF_btnLastPage"                   '最終頁へ移動
                            WF_btnLastPage_Click()

                        Case "WF_btnRefreshPage"                '指定頁へ移動
                            WF_btnRefreshPage_Click()

                        Case "WF_ShowDialog"                    '検査登録ダイアログ 表示
                            WF_InspectDialog_Show()
                            Exit Sub

                        Case "WF_RegularInspectRow_Add"         '検査登録ダイアログ 定期検査行追加
                            WF_RegularInspectRow_Add()
                            Exit Sub

                        Case "WF_AdditionInspectRow_Add"        '検査登録ダイアログ 追加検査行追加
                            WF_AdditionInspectRow_Add()
                            Exit Sub

                        Case "WF_InspectRow_Del"                '検査登録ダイアログ 検査行削除
                            WF_InspectRow_Del()
                            Exit Sub

                        Case "WF_INSPECT_UPDATE"                '検査登録ダイアログ 更新ボタン押下
                            If WF_InspectUpdate() Then
                                researchFlg = True
                                WW_UPDATED = True
                            Else
                                Exit Sub
                            End If

                        Case "WF_STATUS",                       '絞り込み条件変更
                             "WF_CTNTYPE",
                             "WF_CTNNO"
                            WW_NARROW_DOWN = WF_ButtonClick.Value

                        Case "WF_STATION"
                            GetStationName()
                            WW_NARROW_DOWN = WF_ButtonClick.Value

                        Case "WF_ButtonInit"                    '初期化ボタンクリック
                            WF_STATUS.SelectedValue = ""
                            WF_CTNTYPE.SelectedValue = ""
                            WF_CTNNO.Text = ""
                            WF_STATION.Text = ""
                            WF_STATIONNAME.Text = ""
                            researchFlg = True

                        Case "WF_Download"                      'ダウンロードボタンクリック
                            WF_Download()
                            Exit Sub

                        Case "WF_FileUpload"                    'アップロードボタンクリック
                            If WF_InspectUpdateByFile() Then
                                researchFlg = True
                                WW_UPDATED = True
                            Else
                                Exit Sub
                            End If

                    End Select

                    '○ コンテナ一覧一覧表示処理
                    If Not WF_ButtonClick.Value.Equals("WF_ButtonEND") AndAlso
                       Not WF_ButtonClick.Value.Equals("WF_ButtonBackToMenu") Then
                        If researchFlg Then
                            GridViewInitialize()
                        Else
                            DisplayGrid()
                        End If
                    End If
                End If
            Else
                '○ 初期化処理
                Initialize()
            End If
        Catch ex As Exception
            Throw ex
        Finally
            '○ 格納Table Close
            If Not IsNothing(LNT0022tbl) Then
                LNT0022tbl.Clear()
                LNT0022tbl.Dispose()
                LNT0022tbl = Nothing
            End If
            If Not IsNothing(LNT0022SUBtbl) Then
                LNT0022SUBtbl.Clear()
                LNT0022SUBtbl.Dispose()
                LNT0022SUBtbl = Nothing
            End If
        End Try
    End Sub

#Region "初期化処理"
    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = LNT0022WRKINC.MAPID

        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = False
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

        '○初期値設定
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

        'テーブル保存領域初期化
        WW_CreateXMLSaveFile()

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ GridView初期設定
        GridViewInitialize()
    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        ''○ 画面遷移による検索条件の初期化処理
        'If Not Context.Handler.ToString().ToUpper().Equals(C_PREV_MAP_LIST.LNT0009D) Then   '照会画面以外からの遷移
        '    ' 入力内容リセット
        '    DroApplicableYYYY.SelectedValue = ""
        '    DroApplicableMM.SelectedValue = ""
        '    TxtKeijoOrgCd.Text = ""
        'Else '照会画面からの遷移
        '    ' 遷移前入力内容を復元
        '    DroApplicableYYYY.SelectedValue = work.WF_SEL_APPLICABLE_YYYY.Text
        '    DroApplicableMM.SelectedValue = work.WF_SEL_APPLICABLE_MM.Text
        '    TxtKeijoOrgCd.Text = work.WF_SEL_KEIJOORGCD.Text
        'End If

    End Sub

#End Region

#Region "画面表示設定処理"
    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()
        '○画面表示設定処理
        WW_ScreenEnabledSet(True)
    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()
        '○ 画面表示設定処理
        WW_ScreenEnabledSet(False)
    End Sub

    ''' <summary>
    ''' 画面表示設定処理
    ''' </summary>
    Protected Sub WW_ScreenEnabledSet(Optional ByVal researchFlg As Boolean = True)

        'テーブル初期化
        If IsNothing(LNT0022tbl) Then
            LNT0022tbl = New DataTable
        End If
        If LNT0022tbl.Columns.Count <> 0 Then
            LNT0022tbl.Columns.Clear()
        End If
        LNT0022tbl.Clear()
        If IsNothing(LNT0022SUBtbl) Then
            LNT0022SUBtbl = New DataTable
        End If
        If LNT0022SUBtbl.Columns.Count <> 0 Then
            LNT0022SUBtbl.Columns.Clear()
        End If
        LNT0022SUBtbl.Clear()

        '○ 画面表示データ取得
        If researchFlg Then
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続
                MapDataGet(SQLcon)
            End Using
            'DB更新後を除いて頁設定を初期化する
            If Not WW_UPDATED Then InitPnlChangePage(LNT0022tbl)
        Else
            'テーブル復元
            Master.RecoverTable(LNT0022tbl, work.WF_SEL_INP_CONM_TBL.Text)
            Master.RecoverTable(LNT0022SUBtbl, work.WF_SEL_INP_CONINS_TBL.Text)
        End If

        '-----------
        ' DDL再設定
        '-----------
        '状態
        If "WF_STATUS".Equals(WW_NARROW_DOWN) Then
            If WF_STATUS.SelectedValue <> "" Then
                Dim typeDt As DataTable = New DataView(LNT0022tbl) With {
                                            .RowFilter = "STATUS = '" & WF_STATUS.SelectedValue & "'"
                                          }.ToTable(True, New String() {"CTNTYPE"})
                For idx As Integer = 1 To WF_CTNTYPE.Items.Count - 1
                    WF_CTNTYPE.Items(idx).Enabled =
                        If(typeDt.Select("CTNTYPE = '" & WF_CTNTYPE.Items(idx).Value & "'").Count > 0, True, False)
                Next
            Else
                For Each itm As ListItem In WF_CTNTYPE.Items
                    itm.Enabled = True
                Next
            End If
        End If
        'コンテナ
        If "WF_CTNTYPE".Equals(WW_NARROW_DOWN) Then
            If WF_CTNTYPE.SelectedValue <> "" Then
                Dim statusDt As DataTable = New DataView(LNT0022tbl) With {
                                                .RowFilter = "CTNTYPE = '" & WF_CTNTYPE.SelectedValue & "'"
                                            }.ToTable(True, New String() {"STATUS"})
                For idx As Integer = 1 To WF_STATUS.Items.Count - 1
                    WF_STATUS.Items(idx).Enabled =
                        If(statusDt.Select("STATUS = '" & WF_STATUS.Items(idx).Value & "'").Count > 0, True, False)
                Next
            Else
                For Each itm As ListItem In WF_STATUS.Items
                    itm.Enabled = True
                Next
            End If
        End If

        '--------------------------
        ' コンテナ一覧表示絞り込み
        '--------------------------
        Dim rowFilter As String = ""
        '状態
        If WF_STATUS.Items.Count > 0 AndAlso WF_STATUS.SelectedIndex <> 0 Then
            rowFilter = "STATUS = " & WF_STATUS.SelectedValue
        End If
        'コンテナ種別
        If WF_CTNTYPE.Items.Count > 0 AndAlso WF_CTNTYPE.SelectedIndex <> 0 Then
            rowFilter &= If(String.IsNullOrEmpty(rowFilter), "", " AND ") &
                         "CTNTYPE = '" & WF_CTNTYPE.SelectedValue & "'"
        End If
        'コンテナ番号
        If Not String.IsNullOrEmpty(WF_CTNNO.Text) Then
            rowFilter &= If(String.IsNullOrEmpty(rowFilter), "", " AND ") &
                         "Convert(CTNNO, System.String) LIKE '%" & WF_CTNNO.Text & "%'"
        End If
        '駅コード
        If Not String.IsNullOrEmpty(WF_STATION.Text) Then
            rowFilter &= If(String.IsNullOrEmpty(rowFilter), "", " AND ") &
                         "Convert(ARRSTATION, System.String) LIKE '%" & WF_STATION.Text & "%'"
        End If
        '絞り込みDataTable生成
        Dim dt As DataTable = New DataView(LNT0022tbl) With {.RowFilter = rowFilter}.ToTable
        '絞り込み変更あり、またはDB更新済みの場合、頁設定初期化
        If Not String.IsNullOrEmpty(WW_NARROW_DOWN) OrElse WW_UPDATED Then InitPnlChangePage(dt)

        'SUB_LINECNT採番
        Dim i As Integer = 0
        For Each dr As DataRow In dt.Rows
            i += 1
            'LINENO
            dr("SUB_LINECNT") = i
        Next
        '一覧(再)表示
        gvLNT0022.DataSource = New DataView(dt) With {
            .RowFilter = "SUB_LINECNT >= " & (1 + (CONST_MAX_ROW_CNT * (CInt(lblNowPage.Text) - 1))).ToString &
                         " AND SUB_LINECNT <= " + (CONST_MAX_ROW_CNT * CInt(lblNowPage.Text)).ToString
        }
        gvLNT0022.DataBind()

    End Sub

    ''' <summary>
    ''' 頁設定初期化処理
    ''' </summary>
    ''' <param name="dt"></param>
    Private Sub InitPnlChangePage(dt As DataTable)

        '頁設定初期化
        lblNowPage.Text = "1"
        lblMaxPage.Text = CStr(Math.Ceiling(dt.Rows.Count / CONST_MAX_ROW_CNT))

        '各ボタン設定初期化
        txtSelectPage.Enabled = False
        btnRefreshPage.Enabled = False
        btnFirstPage.Enabled = False
        btnBackPage.Enabled = False
        btnNextPage.Enabled = False
        btnLastPage.Enabled = False
        If dt.Rows.Count > CONST_MAX_ROW_CNT Then
            txtSelectPage.Enabled = True
            btnRefreshPage.Enabled = True
            btnNextPage.Enabled = True
            btnLastPage.Enabled = True
        End If

        pnlChangePage.Visible = True
        pnlNoData.Visible = True
        WF_ButtonDownload.Disabled = False
        If dt.Rows.Count = 0 Then
            pnlChangePage.Visible = False
            WF_ButtonDownload.Disabled = True
        Else
            pnlNoData.Visible = False
        End If

    End Sub

    ''' <summary>
    ''' 検査コンテナ一覧表(PreRender)
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Protected Sub gvLNT0022_PreRender(sender As Object, e As EventArgs)
        'GridViewオブジェクト取得
        Dim gv As GridView = CType(sender, GridView)
        If Not (gv.HasControls AndAlso gv.Controls(0).HasControls) Then
            Exit Sub
        End If
        'Header行オブジェクト取得
        Dim header As GridViewRow = CType(gv.Controls(0).Controls(0), GridViewRow)
        If Not header.RowType = DataControlRowType.Header Then
            Exit Sub
        End If
        For i As Integer = 7 To header.Cells.Count - 1 Step 6
            Dim cellText As String = ""
            Select Case i
                Case 7
                    cellText = "４年検査"
                Case 13
                    cellText = "８年検査"
                Case 19
                    cellText = "１２年検査"
                Case 25
                    cellText = "追加検査"
            End Select
            header.Cells(i + 1).Visible = False
            header.Cells(i + 2).Visible = False
            header.Cells(i + 3).Visible = False
            header.Cells(i + 4).Visible = False
            header.Cells(i + 5).Visible = False
            header.Cells(i).Text = cellText & "<BR>検査年&nbsp;|&nbsp;検査日&nbsp;|&nbsp;検査種別&nbsp;|&nbsp;種別名&nbsp;|&nbsp;実施場所&nbsp;|&nbsp;点検修理者"
            header.Cells(i).ColumnSpan = 6
        Next
    End Sub

    ''' <summary>
    ''' 検査コンテナ一覧表(DataBound)
    ''' </summary>
    Protected Sub gvLNT0022_DataBound(sender As Object, e As EventArgs)

        'GridViewオブジェクト取得
        Dim gv As GridView = CType(sender, GridView)
        If Not gv.HasControls Then
            Exit Sub
        End If

        For Each gvr As GridViewRow In CType(gv.Controls(0), Table).Rows
            If Not gvr.RowType = DataControlRowType.DataRow Then
                Continue For
            End If
            '行番号取得
            Dim lineCnt As Integer = CInt(CType(gvr.FindControl("LINECNT"), HiddenField).Value)
            'データ取得
            Dim dr As DataRow = LNT0022tbl.Select("LINECNT = " & lineCnt.ToString)(0)

            'title属性付与(現在駅/各点検データの検査名/実施場所/修理点検者
            For i As Integer = 0 To gvr.Cells.Count - 1
                Select Case i
                    Case 4
                        If Not String.IsNullOrEmpty(dr("ARRSTATIONNAME").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("ARRSTATIONNAME").ToString)
                        End If
                    Case 10
                        If Not String.IsNullOrEmpty(dr("YEAR4_INSPECTNAME").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("YEAR4_INSPECTNAME").ToString)
                        End If
                    Case 11
                        If Not String.IsNullOrEmpty(dr("YEAR4_ENFORCEPLACE").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("YEAR4_ENFORCEPLACE").ToString)
                        End If
                    Case 12
                        If Not String.IsNullOrEmpty(dr("YEAR4_INSPECTVENDORNAME").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("YEAR4_INSPECTVENDORNAME").ToString)
                        End If
                    Case 16
                        If Not String.IsNullOrEmpty(dr("YEAR8_INSPECTNAME").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("YEAR8_INSPECTNAME").ToString)
                        End If
                    Case 17
                        If Not String.IsNullOrEmpty(dr("YEAR8_ENFORCEPLACE").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("YEAR8_ENFORCEPLACE").ToString)
                        End If
                    Case 18
                        If Not String.IsNullOrEmpty(dr("YEAR8_INSPECTVENDORNAME").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("YEAR8_INSPECTVENDORNAME").ToString)
                        End If
                    Case 22
                        If Not String.IsNullOrEmpty(dr("YEAR12_INSPECTNAME").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("YEAR12_INSPECTNAME").ToString)
                        End If
                    Case 23
                        If Not String.IsNullOrEmpty(dr("YEAR12_ENFORCEPLACE").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("YEAR12_ENFORCEPLACE").ToString)
                        End If
                    Case 24
                        If Not String.IsNullOrEmpty(dr("YEAR12_INSPECTVENDORNAME").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("YEAR12_INSPECTVENDORNAME").ToString)
                        End If
                    Case 28
                        If Not String.IsNullOrEmpty(dr("ADD_INSPECTNAME").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("ADD_INSPECTNAME").ToString)
                        End If
                    Case 29
                        If Not String.IsNullOrEmpty(dr("ADD_ENFORCEPLACE").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("ADD_ENFORCEPLACE").ToString)
                        End If
                    Case 30
                        If Not String.IsNullOrEmpty(dr("ADD_INSPECTVENDORNAME").ToString) Then
                            gvr.Cells(i).Attributes.Add("title", dr("ADD_INSPECTVENDORNAME").ToString)
                        End If
                End Select
            Next

            '製造年月から当年で１６年以上経過している場合、種別～現在駅のセル色変更
            If "1".Equals(dr("YEAR16OVERFLG").ToString) Then
                For i As Integer = 0 To 4
                    '注意
                    gvr.Cells(i).CssClass &= "caution"
                Next
            End If
            '当年定期検査対象の場合、当該セルを注意表示
            If "1".Equals(dr("YEAR4FLG").ToString) Then
                For i As Integer = 7 To 12
                    gvr.Cells(i).CssClass &= "caution"
                Next
            End If
            '昨年定期検査対象かつ検査未登録の場合、当該セルを警告表示
            If "1".Equals(dr("YEAR4NOREGFLG").ToString) Then
                For i As Integer = 7 To 12
                    gvr.Cells(i).CssClass &= "warning"
                Next
            End If
            If "1".Equals(dr("YEAR8FLG").ToString) Then
                For i As Integer = 13 To 18
                    gvr.Cells(i).CssClass &= "caution"
                Next
            End If
            If "1".Equals(dr("YEAR8NOREGFLG").ToString) Then
                For i As Integer = 13 To 18
                    gvr.Cells(i).CssClass &= "warning"
                Next
            End If
            If "1".Equals(dr("YEAR12FLG").ToString) Then
                For i As Integer = 19 To 24
                    gvr.Cells(i).CssClass &= "caution"
                Next
            End If
            If "1".Equals(dr("YEAR12NOREGFLG").ToString) Then
                For i As Integer = 19 To 24
                    gvr.Cells(i).CssClass &= "warning"
                Next
            End If
            'セル毎にダブルクリックイベント付与
            For Each cell As TableCell In gvr.Cells
                cell.Attributes.Add("ondblclick", "return rowDblClick('" & lineCnt.ToString & "');")
            Next
        Next

    End Sub

    ''' <summary>
    ''' 検査登録ダイアログ表示
    ''' </summary>
    Private Sub WF_InspectDialog_Show()

        txtValidateMessage.Text = ""
        txtValidateMessage.CssClass = "hidden"

        'リスト表示(ヘッダ)
        Dim ctnDt As New DataTable
        Master.RecoverTable(ctnDt, work.WF_SEL_INP_CONM_TBL.Text)
        gvDialogHead.DataSource = New DataView(ctnDt) With {.RowFilter = "LINECNT = " & WF_SelectedIndex.Value}
        gvDialogHead.DataBind()

        'ダイアログ用一時テーブルを設定
        Dim ctnDr As DataRow = ctnDt.Select("LINECNT = " & WF_SelectedIndex.Value)(0)
        Dim inspectDt As New DataTable
        Master.RecoverTable(inspectDt, work.WF_SEL_INP_CONINS_TBL.Text)
        Dim dv As New DataView(inspectDt)
        dv.RowFilter = "CTNTYPE = '" & ctnDr("CTNTYPE") & "' AND CTNNO = " & ctnDr("CTNNO")
        inspectDt = dv.ToTable
        inspectDt.Columns.Add("R_INSPECTSEQ", Type.GetType("System.Int32"))
        inspectDt.Columns.Add("REGISTED", Type.GetType("System.Int32"))
        inspectDt.Columns.Add("CAUTION", Type.GetType("System.Int32"))
        inspectDt.Columns.Add("WARNING", Type.GetType("System.Int32"))
        inspectDt.Columns.Add("INSPECTYEAR_ERR", Type.GetType("System.Int32"))
        inspectDt.Columns.Add("INSPECTYMD_ERR", Type.GetType("System.Int32"))
        inspectDt.Columns.Add("INSPECTCODE_ERR", Type.GetType("System.Int32"))
        inspectDt.Columns.Add("INSPECTNAME_ERR", Type.GetType("System.Int32"))
        inspectDt.Columns.Add("ENFORCEPLACE_ERR", Type.GetType("System.Int32"))
        inspectDt.Columns.Add("INSPECTVENDOR_ERR", Type.GetType("System.Int32"))
        inspectDt.Columns.Add("NOUPDATE", Type.GetType("System.Int32"))
        inspectDt.Columns.Add("DELFLG", Type.GetType("System.Int32"))
        For Each dr As DataRow In inspectDt.Rows
            dr("INSPECTYMD") = dr("INSPECTYMD").ToString.Replace("/", "-")
            dr("R_INSPECTSEQ") = 0
            dr("REGISTED") = 1
            dr("CAUTION") = 0
            dr("WARNING") = 0
            dr("INSPECTYEAR_ERR") = 0
            dr("INSPECTYMD_ERR") = 0
            dr("INSPECTCODE_ERR") = 0
            dr("INSPECTNAME_ERR") = 0
            dr("ENFORCEPLACE_ERR") = 0
            dr("INSPECTVENDOR_ERR") = 0
            dr("NOUPDATE") = 0
            dr("DELFLG") = 0
        Next

        Dim addRow As DataRow

        '4年検査データが無ければ追加
        If inspectDt.Select("INSPELNTYPE = '1' AND INSPECTSEQ = 4").Count = 0 Then
            addRow = inspectDt.NewRow
            addRow("LINECNT") = 0
            addRow("CTNTYPE") = ctnDr("CTNTYPE")
            addRow("CTNNO") = ctnDr("CTNNO")
            addRow("INSPELNTYPE") = "1"
            addRow("INSPECTSEQ") = 4
            addRow("INSPECTYEAR") = ctnDr("YEAR4_AFTER")
            addRow("INSPECTYMD") = ""
            addRow("INSPECTCODE") = ""
            addRow("INSPECTNAME") = ""
            addRow("R_INSPECTSEQ") = 0
            addRow("REGISTED") = 0
            addRow("CAUTION") = ctnDr("YEAR4FLG")
            addRow("WARNING") = ctnDr("YEAR4NOREGFLG")
            addRow("INSPECTYEAR_ERR") = 0
            addRow("INSPECTYMD_ERR") = 0
            addRow("INSPECTCODE_ERR") = 0
            addRow("INSPECTNAME_ERR") = 0
            addRow("ENFORCEPLACE_ERR") = 0
            addRow("INSPECTVENDOR_ERR") = 0
            addRow("NOUPDATE") = 0
            addRow("DELFLG") = 0
            inspectDt.Rows.Add(addRow)
        Else
            inspectDt.Select("INSPELNTYPE = '1' AND INSPECTSEQ = 4")(0)("CAUTION") = ctnDr("YEAR4FLG")
        End If

        '8年検査データが無ければ追加
        If inspectDt.Select("INSPELNTYPE = '1' AND INSPECTSEQ = 8").Count = 0 Then
            addRow = inspectDt.NewRow
            addRow("LINECNT") = 0
            addRow("CTNTYPE") = ctnDr("CTNTYPE")
            addRow("CTNNO") = ctnDr("CTNNO")
            addRow("INSPELNTYPE") = "1"
            addRow("INSPECTSEQ") = 8
            addRow("INSPECTYEAR") = ctnDr("YEAR8_AFTER")
            addRow("INSPECTYMD") = ""
            addRow("INSPECTCODE") = ""
            addRow("INSPECTNAME") = ""
            addRow("R_INSPECTSEQ") = 0
            addRow("REGISTED") = 0
            addRow("CAUTION") = ctnDr("YEAR8FLG")
            addRow("WARNING") = ctnDr("YEAR8NOREGFLG")
            addRow("INSPECTYEAR_ERR") = 0
            addRow("INSPECTYMD_ERR") = 0
            addRow("INSPECTCODE_ERR") = 0
            addRow("INSPECTNAME_ERR") = 0
            addRow("ENFORCEPLACE_ERR") = 0
            addRow("INSPECTVENDOR_ERR") = 0
            addRow("NOUPDATE") = 0
            addRow("DELFLG") = 0
            inspectDt.Rows.Add(addRow)
        Else
            inspectDt.Select("INSPELNTYPE = '1' AND INSPECTSEQ = 8")(0)("CAUTION") = ctnDr("YEAR8FLG")
        End If

        '12年検査データが無ければ追加
        If inspectDt.Select("INSPELNTYPE = '1' AND INSPECTSEQ = 12").Count = 0 Then
            addRow = inspectDt.NewRow
            addRow("LINECNT") = 0
            addRow("CTNTYPE") = ctnDr("CTNTYPE")
            addRow("CTNNO") = ctnDr("CTNNO")
            addRow("INSPELNTYPE") = "1"
            addRow("INSPECTSEQ") = 12
            addRow("INSPECTYEAR") = ctnDr("YEAR12_AFTER")
            addRow("INSPECTYMD") = ""
            addRow("INSPECTCODE") = ""
            addRow("INSPECTNAME") = ""
            addRow("R_INSPECTSEQ") = 0
            addRow("REGISTED") = 0
            addRow("CAUTION") = ctnDr("YEAR12FLG")
            addRow("WARNING") = ctnDr("YEAR12NOREGFLG")
            addRow("INSPECTYEAR_ERR") = 0
            addRow("INSPECTYMD_ERR") = 0
            addRow("INSPECTCODE_ERR") = 0
            addRow("INSPECTNAME_ERR") = 0
            addRow("ENFORCEPLACE_ERR") = 0
            addRow("INSPECTVENDOR_ERR") = 0
            addRow("NOUPDATE") = 0
            addRow("DELFLG") = 0
            inspectDt.Rows.Add(addRow)
        Else
            inspectDt.Select("INSPELNTYPE = '1' AND INSPECTSEQ = 12")(0)("CAUTION") = ctnDr("YEAR12FLG")
        End If

        '追加検査データ追加（最終行）
        addRow = inspectDt.NewRow
        addRow("LINECNT") = 0
        addRow("CTNTYPE") = ctnDr("CTNTYPE")
        addRow("CTNNO") = ctnDr("CTNNO")
        addRow("INSPELNTYPE") = "2"
        addRow("INSPECTSEQ") = inspectDt.Select("INSPELNTYPE = '2'").Count + 1
        addRow("INSPECTYEAR") = 0
        addRow("INSPECTYMD") = ""
        addRow("INSPECTCODE") = ""
        addRow("INSPECTNAME") = ""
        addRow("R_INSPECTSEQ") = 0
        addRow("REGISTED") = 0
        addRow("CAUTION") = 0
        addRow("WARNING") = 0
        addRow("INSPECTYEAR_ERR") = 0
        addRow("INSPECTYMD_ERR") = 0
        addRow("INSPECTCODE_ERR") = 0
        addRow("INSPECTNAME_ERR") = 0
        addRow("ENFORCEPLACE_ERR") = 0
        addRow("INSPECTVENDOR_ERR") = 0
        addRow("NOUPDATE") = 0
        addRow("DELFLG") = 0
        inspectDt.Rows.Add(addRow)

        '検査リスト設定
        SetInspectGridView(inspectDt)

        hdnShowPnlInspectDialog.Value = "1"

    End Sub

    ''' <summary>
    ''' 検査登録ダイアログ　コンテナ情報(DataBound)
    ''' </summary>
    Protected Sub gvDialogHead_DataBound(sender As Object, e As EventArgs)

        'GridViewオブジェクト取得
        Dim gv As GridView = CType(sender, GridView)
        If Not gv.HasControls Then
            Exit Sub
        End If

        'テーブル復元
        Dim ctnDt As New DataTable
        Master.RecoverTable(ctnDt, work.WF_SEL_INP_CONM_TBL.Text)

        For Each gvr As GridViewRow In CType(gv.Controls(0), Table).Rows
            If Not gvr.RowType = DataControlRowType.DataRow Then
                Continue For
            End If
            '行番号取得
            Dim lineCnt As Integer = CInt(CType(gvr.FindControl("LINECNT"), HiddenField).Value)
            'データ取得
            Dim dr As DataRow = ctnDt.Select("LINECNT = " & lineCnt.ToString)(0)
            '製造年月から当年で１６年以上経過している場合、種別～現在駅のセル色変更
            If "1".Equals(dr("YEAR16OVERFLG").ToString) Then
                For i As Integer = 0 To 4
                    '注意
                    gvr.Cells(i).CssClass &= "caution"
                Next
            End If
        Next

    End Sub

    ''' <summary>
    ''' 検査登録ダイアログ　定期検査表(PreRender)
    ''' </summary>
    Protected Sub gvDialogRegularInspects_PreRender(sender As Object, e As EventArgs)
        'GridViewオブジェクト取得
        Dim gv As GridView = CType(sender, GridView)
        If Not (gv.HasControls AndAlso gv.Controls(0).HasControls) Then
            Exit Sub
        End If
        'Header行オブジェクト取得
        Dim header As GridViewRow = CType(gv.Controls(0).Controls(0), GridViewRow)
        If Not header.RowType = DataControlRowType.Header Then
            Exit Sub
        End If
        header.Cells(2).Visible = False
        header.Cells(1).ColumnSpan = 2
        header.Cells(1).Text = "定期検査"
    End Sub

    ''' <summary>
    ''' 検査登録ダイアログ　定期検査表(DataBound)
    ''' </summary>
    Protected Sub gvDialogRegularInspects_DataBound(sender As Object, e As EventArgs)

        'GridViewオブジェクト取得
        Dim gv As GridView = CType(sender, GridView)
        If Not gv.HasControls Then
            Exit Sub
        End If

        Dim inspectDt As New DataTable
        Master.RecoverTable(inspectDt, work.WF_SEL_INP_DIALOG_TBL.Text)
        Dim inspectCodeDt As DataTable = GetInspectCodeTable()
        Dim inspectVendorDt As DataTable = GetInspectVendorTable()
        For Each gvr As GridViewRow In CType(gv.Controls(0), Table).Rows
            If Not gvr.RowType = DataControlRowType.DataRow Then
                Continue For
            End If
            '行番号取得
            Dim lineCnt As Integer = CInt(CType(gvr.FindControl("LINECNT"), HiddenField).Value)
            'データ取得
            Dim dr As DataRow = inspectDt.Select("LINECNT = " & lineCnt.ToString)(0)
            '背景色
            Dim statusCss As String = "nothing"
            If "1".Equals(dr("WARNING").ToString) Then
                statusCss = "warning"
            ElseIf "1".Equals(dr("CAUTION").ToString) Then
                statusCss = "caution"
            ElseIf "1".Equals(dr("REGISTED").ToString) Then
                statusCss = "registed"
            End If
            CType(gvr.FindControl("INSPECTYMD"), TextBox).CssClass = statusCss
            CType(gvr.FindControl("INSPECTCODE"), DropDownList).CssClass = statusCss
            CType(gvr.FindControl("INSPECTNAME"), TextBox).CssClass = statusCss
            CType(gvr.FindControl("ENFORCEPLACE"), TextBox).CssClass = statusCss
            CType(gvr.FindControl("INSPECTVENDOR"), DropDownList).CssClass = statusCss
            'エラーチェック
            If dr("INSPECTYMD_ERR") = 1 Then CType(gvr.FindControl("INSPECTYMD"), TextBox).CssClass &= " error"
            If dr("INSPECTCODE_ERR") = 1 Then CType(gvr.FindControl("INSPECTCODE"), DropDownList).CssClass &= " error"
            If dr("INSPECTNAME_ERR") = 1 Then CType(gvr.FindControl("INSPECTNAME"), TextBox).CssClass &= " error"
            If dr("ENFORCEPLACE_ERR") = 1 Then CType(gvr.FindControl("ENFORCEPLACE"), TextBox).CssClass &= " error"
            If dr("INSPECTVENDOR_ERR") = 1 Then CType(gvr.FindControl("INSPECTVENDOR"), DropDownList).CssClass &= " error"
            '(検査)種別DDL設定
            Dim ddl As DropDownList = CType(gvr.FindControl("INSPECTCODE"), DropDownList)
            ddl.Items.Clear()
            ddl.Items.Add(New ListItem("", ""))
            For Each icdr As DataRow In inspectCodeDt.Rows
                ddl.Items.Add(New ListItem(icdr("code"), icdr("code")))
            Next
            ddl.SelectedValue = dr("INSPECTCODE")
            '点検修理者DDL設定
            ddl = CType(gvr.FindControl("INSPECTVENDOR"), DropDownList)
            ddl.Items.Clear()
            ddl.Items.Add(New ListItem("", ""))
            For Each ivdr As DataRow In inspectVendorDt.Rows
                If ivdr("disabled") = 1 Then
                    Dim item As New ListItem(ivdr("value"), ivdr("key"))
                    item.Attributes.Add("disabled", "disabled")
                    ddl.Items.Add(item)
                Else
                    ddl.Items.Add(New ListItem(ivdr("value"), ivdr("key")))
                End If
            Next
            ddl.SelectedValue = dr("INSPECTVENDOR")
            '追加ボタンイベント追加
            CType(gvr.FindControl("BTN_ADD"), Button).Attributes.Add("onclick", "return AddRegularInspectRow();")
            '削除ボタン非表示/イベント追加
            Dim delBtn As Button = CType(gvr.FindControl("BTN_DEL"), Button)
            If dr("INSPECTSEQ") = 4 OrElse dr("INSPECTSEQ") = 8 OrElse dr("INSPECTSEQ") = 12 Then
                delBtn.Visible = False
            Else
                delBtn.Attributes.Add("onclick", "return DelInspectRow('" & lineCnt & "');")
            End If
        Next

    End Sub

    ''' <summary>
    ''' 検査登録ダイアログ　追加検査表(PreRender)
    ''' </summary>
    Protected Sub gvDialogAdditionInspects_PreRender(sender As Object, e As EventArgs)
        'GridViewオブジェクト取得
        Dim gv As GridView = CType(sender, GridView)
        If Not (gv.HasControls AndAlso gv.Controls(0).HasControls) Then
            Exit Sub
        End If
        'Header行オブジェクト取得
        Dim header As GridViewRow = CType(gv.Controls(0).Controls(0), GridViewRow)
        If Not header.RowType = DataControlRowType.Header Then
            Exit Sub
        End If
        header.Cells(2).Visible = False
        header.Cells(1).ColumnSpan = 2
        header.Cells(1).Text = "追加検査"
    End Sub

    ''' <summary>
    ''' 検査登録ダイアログ　追加検査表(DataBound)
    ''' </summary>
    Protected Sub gvDialogAdditionInspects_DataBound(sender As Object, e As EventArgs)

        'GridViewオブジェクト取得
        Dim gv As GridView = CType(sender, GridView)
        If Not gv.HasControls Then
            Exit Sub
        End If

        Dim inspectDt As New DataTable
        Master.RecoverTable(inspectDt, work.WF_SEL_INP_DIALOG_TBL.Text)
        Dim inspectCodeDt As DataTable = GetInspectCodeTable()
        Dim inspectVendorDt As DataTable = GetInspectVendorTable()
        For Each gvr As GridViewRow In CType(gv.Controls(0), Table).Rows
            If Not gvr.RowType = DataControlRowType.DataRow Then
                Continue For
            End If
            '行番号取得
            Dim lineCnt As Integer = CInt(CType(gvr.FindControl("LINECNT"), HiddenField).Value)
            'データ取得
            Dim dr As DataRow = inspectDt.Select("LINECNT = " & lineCnt.ToString)(0)
            '背景色
            Dim statusCss As String = If("1".Equals(dr("REGISTED").ToString), "registed", "nothing")
            CType(gvr.FindControl("INSPECTYEAR"), TextBox).CssClass = statusCss
            CType(gvr.FindControl("INSPECTYMD"), TextBox).CssClass = statusCss
            CType(gvr.FindControl("INSPECTCODE"), DropDownList).CssClass = statusCss
            CType(gvr.FindControl("INSPECTNAME"), TextBox).CssClass = statusCss
            CType(gvr.FindControl("ENFORCEPLACE"), TextBox).CssClass = statusCss
            CType(gvr.FindControl("INSPECTVENDOR"), DropDownList).CssClass = statusCss
            'エラーチェック
            If dr("INSPECTYEAR_ERR") = 1 Then CType(gvr.FindControl("INSPECTYEAR"), TextBox).CssClass &= " error"
            If dr("INSPECTYMD_ERR") = 1 Then CType(gvr.FindControl("INSPECTYMD"), TextBox).CssClass &= " error"
            If dr("INSPECTCODE_ERR") = 1 Then CType(gvr.FindControl("INSPECTCODE"), DropDownList).CssClass &= " error"
            If dr("INSPECTNAME_ERR") = 1 Then CType(gvr.FindControl("INSPECTNAME"), TextBox).CssClass &= " error"
            If dr("ENFORCEPLACE_ERR") = 1 Then CType(gvr.FindControl("ENFORCEPLACE"), TextBox).CssClass &= " error"
            If dr("INSPECTVENDOR_ERR") = 1 Then CType(gvr.FindControl("INSPECTVENDOR"), DropDownList).CssClass &= " error"
            '(検査)種別DDL設定
            Dim ddl As DropDownList = CType(gvr.FindControl("INSPECTCODE"), DropDownList)
            ddl.Items.Clear()
            ddl.Items.Add(New ListItem("", ""))
            For Each icdr As DataRow In inspectCodeDt.Rows
                ddl.Items.Add(New ListItem(icdr("code"), icdr("code")))
            Next
            ddl.SelectedValue = dr("INSPECTCODE")
            '点検修理者DDL設定
            ddl = CType(gvr.FindControl("INSPECTVENDOR"), DropDownList)
            ddl.Items.Clear()
            ddl.Items.Add(New ListItem("", ""))
            For Each ivdr As DataRow In inspectVendorDt.Rows
                If ivdr("disabled") = 1 Then
                    Dim item As New ListItem(ivdr("value"), ivdr("key"))
                    item.Attributes.Add("disabled", "disabled")
                    ddl.Items.Add(item)
                Else
                    ddl.Items.Add(New ListItem(ivdr("value"), ivdr("key")))
                End If
            Next
            ddl.SelectedValue = dr("INSPECTVENDOR")
            '追加ボタンイベント追加
            CType(gvr.FindControl("BTN_ADD"), Button).Attributes.Add("onclick", "return AddAdditionInspectRow();")
            '削除ボタンイベント追加
            CType(gvr.FindControl("BTN_DEL"), Button).Attributes.Add("onclick", "return DelInspectRow('" & lineCnt & "');")
        Next

    End Sub

    ''' <summary>
    ''' 検査登録ダイアログの検査表入力値をテーブルにフィードバック
    ''' </summary>
    Private Function FeedbackInspects() As DataTable

        Dim inspectDt As New DataTable
        Master.RecoverTable(inspectDt, work.WF_SEL_INP_DIALOG_TBL.Text)

        '定期検査表
        For Each gvr As GridViewRow In gvDialogRegularInspects.Rows
            If Not gvr.RowType = DataControlRowType.DataRow Then
                Continue For
            End If
            '行番号取得
            Dim lineCnt As Integer = CInt(CType(gvr.FindControl("LINECNT"), HiddenField).Value)
            'データ取得
            Dim dr As DataRow = inspectDt.Select("LINECNT = " & lineCnt.ToString)(0)
            dr("INSPECTYMD") = CType(gvr.FindControl("INSPECTYMD"), TextBox).Text
            dr("INSPECTCODE") = CType(gvr.FindControl("INSPECTCODE"), DropDownList).SelectedValue
            dr("INSPECTNAME") = CType(gvr.FindControl("INSPECTNAME"), TextBox).Text
            dr("ENFORCEPLACE") = CType(gvr.FindControl("ENFORCEPLACE"), TextBox).Text
            dr("INSPECTVENDOR") = CType(gvr.FindControl("INSPECTVENDOR"), DropDownList).SelectedValue
            dr("INSPECTVENDORNAME") = CType(gvr.FindControl("INSPECTVENDOR"), DropDownList).SelectedItem.Text
        Next

        '追加検査表
        For Each gvr As GridViewRow In gvDialogAdditionInspects.Rows
            If Not gvr.RowType = DataControlRowType.DataRow Then
                Continue For
            End If
            '行番号取得
            Dim lineCnt As Integer = CInt(CType(gvr.FindControl("LINECNT"), HiddenField).Value)
            'データ取得
            Dim dr As DataRow = inspectDt.Select("LINECNT = " & lineCnt.ToString)(0)
            Dim year As String = CType(gvr.FindControl("INSPECTYEAR"), TextBox).Text
            dr("INSPECTYEAR") = CInt(If(String.IsNullOrEmpty(year), "0", year))
            dr("INSPECTYMD") = CType(gvr.FindControl("INSPECTYMD"), TextBox).Text
            dr("INSPECTCODE") = CType(gvr.FindControl("INSPECTCODE"), DropDownList).SelectedValue
            dr("INSPECTNAME") = CType(gvr.FindControl("INSPECTNAME"), TextBox).Text
            dr("ENFORCEPLACE") = CType(gvr.FindControl("ENFORCEPLACE"), TextBox).Text
            dr("INSPECTVENDOR") = CType(gvr.FindControl("INSPECTVENDOR"), DropDownList).SelectedValue
            dr("INSPECTVENDORNAME") = CType(gvr.FindControl("INSPECTVENDOR"), DropDownList).SelectedItem.Text
        Next

        Return inspectDt

    End Function

    ''' <summary>
    ''' 検査リスト設定
    ''' </summary>
    Private Sub SetInspectGridView(inspectDt As DataTable)

        '行番号採番
        Dim lc As Integer = 0
        For Each dr As DataRow In inspectDt.Select("", "DELFLG, INSPELNTYPE, INSPECTSEQ")
            lc += 1
            dr("LINECNT") = lc
        Next
        Dim r_inspectseq As Integer = 0
        For Each dr As DataRow In inspectDt.Select("INSPELNTYPE = '2'", "DELFLG, INSPECTSEQ")
            If dr("DELFLG") = 0 Then
                r_inspectseq += 1
                dr("R_INSPECTSEQ") = r_inspectseq
            Else
                dr("R_INSPECTSEQ") = 0
            End If
        Next

        '行番号ソート
        Dim dv As DataView = New DataView(inspectDt)
        dv.Sort = "LINECNT"
        inspectDt = dv.ToTable()

        'ダイアログ用一時テーブルを保存
        Master.SaveTable(inspectDt, work.WF_SEL_INP_DIALOG_TBL.Text)

        '定期検査リスト再表示
        gvDialogRegularInspects.DataSource = New DataView(inspectDt) With {.RowFilter = "DELFLG = 0 AND INSPELNTYPE = '1'"}
        gvDialogRegularInspects.DataBind()

        '追加検査リスト再表示
        gvDialogAdditionInspects.DataSource = New DataView(inspectDt) With {.RowFilter = "DELFLG = 0 AND INSPELNTYPE = '2'"}
        gvDialogAdditionInspects.DataBind()

    End Sub

    ''' <summary>
    ''' 定期検査行追加
    ''' </summary>
    Private Sub WF_RegularInspectRow_Add()
        WF_InspectRow_Add(1)
    End Sub

    ''' <summary>
    ''' 追加検査行追加
    ''' </summary>
    Private Sub WF_AdditionInspectRow_Add()
        WF_InspectRow_Add(2)
    End Sub

    ''' <summary>
    ''' 検査行追加
    ''' </summary>
    Private Sub WF_InspectRow_Add(AdditionType As Integer)

        'フィードバック
        Dim inspectDt As DataTable = FeedbackInspects()

        '行追加処理
        Dim lastRow As DataRow = inspectDt.Select("DELFLG = 0 AND INSPELNTYPE = '" & AdditionType & "'",
                                                  "INSPECTSEQ DESC")(0)
        Dim addRow As DataRow = inspectDt.NewRow
        addRow = inspectDt.NewRow
        addRow("LINECNT") = 0
        addRow("CTNTYPE") = lastRow("CTNTYPE")
        addRow("CTNNO") = lastRow("CTNNO")
        addRow("INSPELNTYPE") = AdditionType
        addRow("INSPECTSEQ") = lastRow("INSPECTSEQ") + If(AdditionType = 1, 4, 1)
        addRow("INSPECTYEAR") = If(AdditionType = 1, lastRow("INSPECTYEAR") + 4, 0)
        addRow("INSPECTYMD") = ""
        addRow("INSPECTCODE") = ""
        addRow("INSPECTNAME") = ""
        addRow("ENFORCEPLACE") = ""
        addRow("INSPECTVENDOR") = ""
        addRow("INSPECTVENDORNAME") = ""
        addRow("R_INSPECTSEQ") = 0
        addRow("REGISTED") = 0
        addRow("CAUTION") = 0
        addRow("WARNING") = 0
        addRow("INSPECTYEAR_ERR") = 0
        addRow("INSPECTYMD_ERR") = 0
        addRow("INSPECTCODE_ERR") = 0
        addRow("INSPECTNAME_ERR") = 0
        addRow("ENFORCEPLACE_ERR") = 0
        addRow("INSPECTVENDOR_ERR") = 0
        addRow("NOUPDATE") = 0
        addRow("DELFLG") = 0
        inspectDt.Rows.Add(addRow)

        '検査リスト設定
        SetInspectGridView(inspectDt)

    End Sub

    ''' <summary>
    ''' 検査行削除
    ''' </summary>
    Private Sub WF_InspectRow_Del()

        'フィードバック
        Dim inspectDt As DataTable = FeedbackInspects()

        Dim delRow As DataRow = inspectDt.Select("LINECNT = " & WF_DelInspectRowIndex.Value)(0)
        If delRow("REGISTED") = 0 Then
            inspectDt.Rows.Remove(delRow)
        Else
            delRow("DELFLG") = 1
        End If

        '検査リスト設定
        SetInspectGridView(inspectDt)

    End Sub

    ''' <summary>
    ''' PageRenderオーバーライド(colgroup書き込み)
    ''' </summary>
    Protected Overrides Sub Render(writer As HtmlTextWriter)

        Dim tw As IO.TextWriter = New IO.StringWriter
        Dim htw = New HtmlTextWriter(tw)
        MyBase.Render(htw)
        'コンテナ一覧表colgroup設定
        Dim editedPageSource = tw.ToString
        Dim idPos = editedPageSource.IndexOf("contents1_gvLNT0022")
        If idPos < 0 Then
            writer.Write(editedPageSource)
            Exit Sub
        End If
        Dim findTagStart As Boolean = False
        Dim incVal As Integer = 0
        While findTagStart = False
            incVal += 1
            '無限ループ抑止（終了タグが見つからない場合）
            If idPos + incVal + 1 >= editedPageSource.Length Then
                Exit While
            End If
            If editedPageSource.Substring(idPos + incVal, 1) = ">" Then
                findTagStart = True
                Dim StartHtml = editedPageSource.Substring(0, idPos + incVal + 1)
                Dim EndHtml = editedPageSource.Substring(idPos + incVal + 2)
                editedPageSource = StartHtml & "<colgroup>" &
                                   "<col class=""w69px"">" &
                                   "<col class=""w70px"">" &
                                   "<col class=""w110px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w40px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w20px"">" &
                                   "<col class=""w150px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w150px"">" &
                                   "<col class=""w40px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w20px"">" &
                                   "<col class=""w150px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w150px"">" &
                                   "<col class=""w40px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w20px"">" &
                                   "<col class=""w150px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w150px"">" &
                                   "<col class=""w40px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w20px"">" &
                                   "<col class=""w150px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w150px"">" &
                                   "</colgroup>" & EndHtml

            End If
        End While

        '検査登録ダイアログ定期検査表colgroup設定
        idPos = editedPageSource.IndexOf("contents1_gvDialogRegularInspects")
        If idPos < 0 Then
            writer.Write(editedPageSource)
            Exit Sub
        End If
        findTagStart = False
        incVal = 0
        While findTagStart = False
            incVal += 1
            '無限ループ抑止（終了タグが見つからない場合）
            If idPos + incVal + 1 >= editedPageSource.Length Then
                Exit While
            End If
            If editedPageSource.Substring(idPos + incVal, 1) = ">" Then
                findTagStart = True
                Dim StartHtml = editedPageSource.Substring(0, idPos + incVal + 1)
                Dim EndHtml = editedPageSource.Substring(idPos + incVal + 2)
                editedPageSource = StartHtml & "<colgroup>" &
                                   "<col class=""w30px"">" &
                                   "<col class=""w30px"">" &
                                   "<col class=""w50px"">" &
                                   "<col class=""w130px"">" &
                                   "<col class=""w40px"">" &
                                   "<col class=""w296px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w170px"">" &
                                   "<col class=""w30px"">" &
                                   "</colgroup>" & EndHtml
            End If
        End While

        '検査登録ダイアログ追加検査表colgroup設定
        idPos = editedPageSource.IndexOf("contents1_gvDialogAdditionInspects")
        If idPos < 0 Then
            writer.Write(editedPageSource)
            Exit Sub
        End If
        findTagStart = False
        incVal = 0
        While findTagStart = False
            incVal += 1
            '無限ループ抑止（終了タグが見つからない場合）
            If idPos + incVal + 1 >= editedPageSource.Length Then
                Exit While
            End If
            If editedPageSource.Substring(idPos + incVal, 1) = ">" Then
                findTagStart = True
                Dim StartHtml = editedPageSource.Substring(0, idPos + incVal + 1)
                Dim EndHtml = editedPageSource.Substring(idPos + incVal + 2)
                editedPageSource = StartHtml & "<colgroup>" &
                                   "<col class=""w30px"">" &
                                   "<col class=""w30px"">" &
                                   "<col class=""w50px"">" &
                                   "<col class=""w130px"">" &
                                   "<col class=""w40px"">" &
                                   "<col class=""w296px"">" &
                                   "<col class=""w100px"">" &
                                   "<col class=""w170px"">" &
                                   "<col class=""w30px"">" &
                                   "</colgroup>" & EndHtml
            End If
        End While

        writer.Write(editedPageSource)
    End Sub

#End Region

#Region "頁移動処理"

    ''' <summary>
    ''' 先頭頁移動
    ''' </summary>
    Private Sub WF_btnFirstPage_Click()
        lblNowPage.Text = "1"

        btnFirstPage.Enabled = False
        btnBackPage.Enabled = False
        btnNextPage.Enabled = True
        btnLastPage.Enabled = True
    End Sub

    ''' <summary>
    ''' 前の頁移動
    ''' </summary>
    Private Sub WF_btnBackPage_Click()
        lblNowPage.Text = CInt(lblNowPage.Text) - 1

        btnFirstPage.Enabled = True
        btnBackPage.Enabled = True
        btnNextPage.Enabled = True
        btnLastPage.Enabled = True
        If CInt(lblNowPage.Text) = 1 Then
            btnFirstPage.Enabled = False
            btnBackPage.Enabled = False
        End If
    End Sub

    ''' <summary>
    ''' 次の頁移動
    ''' </summary>
    Private Sub WF_btnNextPage_Click()
        lblNowPage.Text = CInt(lblNowPage.Text) + 1

        btnFirstPage.Enabled = True
        btnBackPage.Enabled = True
        btnNextPage.Enabled = True
        btnLastPage.Enabled = True
        If CInt(lblNowPage.Text) = CInt(lblMaxPage.Text) Then
            btnNextPage.Enabled = False
            btnLastPage.Enabled = False
        End If
    End Sub

    ''' <summary>
    ''' 最終頁移動
    ''' </summary>
    Private Sub WF_btnLastPage_Click()
        lblNowPage.Text = lblMaxPage.Text

        btnFirstPage.Enabled = True
        btnBackPage.Enabled = True
        btnNextPage.Enabled = False
        btnLastPage.Enabled = False
    End Sub

    ''' <summary>
    ''' 指定頁移動
    ''' </summary>
    Private Sub WF_btnRefreshPage_Click()

        lblNowPage.Text = txtSelectPage.Text

        btnFirstPage.Enabled = True
        btnBackPage.Enabled = True
        btnNextPage.Enabled = True
        btnLastPage.Enabled = True

        If CInt(lblNowPage.Text) = 1 Then
            btnFirstPage.Enabled = False
            btnBackPage.Enabled = False
        End If

        If CInt(lblNowPage.Text) = CInt(lblMaxPage.Text) Then
            btnNextPage.Enabled = False
            btnLastPage.Enabled = False
        End If

        txtSelectPage.Text = ""

    End Sub

#End Region

#Region "ボタン押下処理"
    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        Master.TransitionPrevPage()
    End Sub

#End Region

#Region "共通処理"

    ''' <summary>
    ''' ポストバック時退避データ保存先の作成
    ''' </summary>
    Protected Sub WW_CreateXMLSaveFile()
        'コンテナＭ
        If String.IsNullOrEmpty(work.WF_SEL_INP_CONM_TBL.Text) Then
            work.WF_SEL_INP_CONM_TBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") _
            & "-" & Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") _
            & "WF_SEL_INP_CONM_TBL.txt"
        End If
        'コンテナ検査管理Ｔ
        If String.IsNullOrEmpty(work.WF_SEL_INP_CONINS_TBL.Text) Then
            work.WF_SEL_INP_CONINS_TBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") _
            & "-" & Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") _
            & "WF_SEL_INP_CONINS_TBL.txt"
        End If
        '検査登録ダイアログ一時保存用
        If String.IsNullOrEmpty(work.WF_SEL_INP_DIALOG_TBL.Text) Then
            work.WF_SEL_INP_DIALOG_TBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") _
            & "-" & Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") _
            & "WF_SEL_INP_DIALOG_TBL.txt"
        End If
    End Sub

#End Region

#Region "LeftBox関連操作"
    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                If WF_LeftMViewChange.Value <> LIST_BOX_CLASSIFICATION.LC_CALENDAR Then
                    'フィールドによってパラメータを変える
                    Dim WW_PrmData As New Hashtable
                    'Select Case WF_FIELD.Value
                    '    Case TxtKeijoOrgName.ID       '計上先組織コード
                    '        WW_PrmData = work.CreateFinanceItemParam(GL0025FinanceItemList.LS_FINANCEITEM_WITH.ORG_CD, DroApplicableYYYY.SelectedValue + DroApplicableMM.SelectedValue)
                    'End Select
                    .SetListBox(WF_LeftMViewChange.Value, WW_DUMMY, WW_PrmData)
                    .ActiveListBox()
                Else
                    .ActiveCalendar()
                End If
            End With

        End If
    End Sub

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()
        'Dim WW_SelectValue As String = ""
        'Dim WW_SelectText As String = ""

        ''○ 選択内容を取得
        'If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
        '    WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
        '    WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
        '    WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        'End If

        ''○ 選択内容を画面項目へセット
        'Select Case WF_FIELD.Value
        '    Case TxtKeijoOrgName.ID                  '計上先組織コード
        '        TxtKeijoOrgName.Text = WW_SelectText
        '        TxtKeijoOrgCd.Text = WW_SelectValue
        '        TxtKeijoOrgCd.Focus()
        'End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()
        ''○ フォーカスセット
        'Select Case WF_FIELD.Value
        '    Case TxtKeijoOrgName.ID        '計上先組織コード
        '        Me.TxtKeijoOrgName.Focus()
        'End Select

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

#End Region

#Region "データ取得処理"

    ''' <summary>
    ''' データ取得処理(全件)
    ''' </summary>
    Protected Sub MapDataGet(ByVal SQLcon As MySqlConnection)

        Dim SQLBldr As New StringBuilder

        'コンテナ一覧取得
        SQLBldr.Clear()
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("     0 AS LINECNT")
        SQLBldr.AppendLine("     , 0 AS SUB_LINECNT")
        SQLBldr.AppendLine("     , CIM.CTNTYPE")
        SQLBldr.AppendLine("     , CIM.CTNNO")
        SQLBldr.AppendLine("     , CIM.CTNTYPE + '-' + CONVERT(NVARCHAR, CIM.CTNNO) AS CONTNUM")
        SQLBldr.AppendLine("     , CIM.CONTRUCTIONYM")
        SQLBldr.AppendLine("     , coalesce(PREST.ORGCODE, '999999') AS ORGCODE")
        SQLBldr.AppendLine("     , coalesce(PREST.ARRSTATION, 999999) AS ARRSTATION")
        SQLBldr.AppendLine("     , coalesce(RTRIM(STA.NAMES), '') AS ARRSTATIONNAME")
        SQLBldr.AppendLine("     , coalesce(FORMAT(CIM.TRAINSBEFORERUNYMD, 'yyyy/MM/dd'), '') AS TRAINSBEFORERUNYMD")
        SQLBldr.AppendLine("     , coalesce(FORMAT(CIM.TRAINSNEXTRUNYMD, 'yyyy/MM/dd'), '') AS TRAINSNEXTRUNYMD")
        SQLBldr.AppendLine("     , YEAR4_AFTER")
        SQLBldr.AppendLine("     , (CASE WHEN YEAR4_AFTER = YEAR(CURDATE()) THEN 1 ELSE 0 END) AS YEAR4FLG")
        SQLBldr.AppendLine("     , (CASE WHEN YEAR4_AFTER = YEAR(CURDATE()) - 1 THEN 1 ELSE 0 END) AS YEAR4NOREGFLG")
        SQLBldr.AppendLine("     , '' AS YEAR4_INSPECTYMD")
        SQLBldr.AppendLine("     , '' AS YEAR4_INSPECTCODE")
        SQLBldr.AppendLine("     , '' AS YEAR4_INSPECTNAME")
        SQLBldr.AppendLine("     , '' AS YEAR4_ENFORCEPLACE")
        SQLBldr.AppendLine("     , '' AS YEAR4_INSPECTVENDOR")
        SQLBldr.AppendLine("     , '' AS YEAR4_INSPECTVENDORNAME")
        SQLBldr.AppendLine("     , YEAR8_AFTER")
        SQLBldr.AppendLine("     , (CASE WHEN YEAR8_AFTER = YEAR(CURDATE()) THEN 1 ELSE 0 END) AS YEAR8FLG")
        SQLBldr.AppendLine("     , (CASE WHEN YEAR8_AFTER = YEAR(CURDATE()) - 1 THEN 1 ELSE 0 END) AS YEAR8NOREGFLG")
        SQLBldr.AppendLine("     , '' AS YEAR8_INSPECTYMD")
        SQLBldr.AppendLine("     , '' AS YEAR8_INSPECTCODE")
        SQLBldr.AppendLine("     , '' AS YEAR8_INSPECTNAME")
        SQLBldr.AppendLine("     , '' AS YEAR8_ENFORCEPLACE")
        SQLBldr.AppendLine("     , '' AS YEAR8_INSPECTVENDOR")
        SQLBldr.AppendLine("     , '' AS YEAR8_INSPECTVENDORNAME")
        SQLBldr.AppendLine("     , YEAR12_AFTER")
        SQLBldr.AppendLine("     , (CASE WHEN YEAR12_AFTER = YEAR(CURDATE()) THEN 1 ELSE 0 END) AS YEAR12FLG")
        SQLBldr.AppendLine("     , (CASE WHEN YEAR12_AFTER = YEAR(CURDATE()) - 1 THEN 1 ELSE 0 END) AS YEAR12NOREGFLG")
        SQLBldr.AppendLine("     , 0 AS YEAR12_NOREGISTFLG")
        SQLBldr.AppendLine("     , '' AS YEAR12_INSPECTYMD")
        SQLBldr.AppendLine("     , '' AS YEAR12_INSPECTCODE")
        SQLBldr.AppendLine("     , '' AS YEAR12_INSPECTNAME")
        SQLBldr.AppendLine("     , '' AS YEAR12_ENFORCEPLACE")
        SQLBldr.AppendLine("     , '' AS YEAR12_INSPECTVENDOR")
        SQLBldr.AppendLine("     , '' AS YEAR12_INSPECTVENDORNAME")
        SQLBldr.AppendLine("     , YEAR16_AFTER")
        SQLBldr.AppendLine("     , (CASE WHEN YEAR16_AFTER <= YEAR(CURDATE()) THEN 1 ELSE 0 END) AS YEAR16OVERFLG")
        SQLBldr.AppendLine("     , 0 AS YEARN_SEQ")
        SQLBldr.AppendLine("     , 0 AS YEARN_YEAR")
        SQLBldr.AppendLine("     , '' AS YEARN_INSPECTYMD")
        SQLBldr.AppendLine("     , '' AS YEARN_INSPECTCODE")
        SQLBldr.AppendLine("     , '' AS YEARN_INSPECTNAME")
        SQLBldr.AppendLine("     , '' AS YEARN_ENFORCEPLACE")
        SQLBldr.AppendLine("     , '' AS YEARN_INSPECTVENDOR")
        SQLBldr.AppendLine("     , '' AS YEARN_INSPECTVENDORNAME")
        SQLBldr.AppendLine("     , 0 AS ADD_SEQ")
        SQLBldr.AppendLine("     , '' AS ADD_YEAR")
        SQLBldr.AppendLine("     , '' AS ADD_INSPECTYMD")
        SQLBldr.AppendLine("     , '' AS ADD_INSPECTCODE")
        SQLBldr.AppendLine("     , '' AS ADD_INSPECTNAME")
        SQLBldr.AppendLine("     , '' AS ADD_ENFORCEPLACE")
        SQLBldr.AppendLine("     , '' AS ADD_INSPECTVENDOR")
        SQLBldr.AppendLine("     , '' AS ADD_INSPECTVENDORNAME")
        SQLBldr.AppendLine("     , 0 AS STATUS")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     lng.VIW0011_CTNISPMNG AS CIM")
        SQLBldr.AppendLine("     INNER JOIN lng.LNM0002_RECONM AS LNM")
        SQLBldr.AppendLine("         ON  CIM.CTNTYPE = LNM.CTNTYPE")
        SQLBldr.AppendLine("         AND CIM.CTNNO = LNM.CTNNO")
        SQLBldr.AppendLine("     LEFT OUTER JOIN lng.LNT0021_PRESENTSTATE AS PREST")
        SQLBldr.AppendLine("         ON  CIM.CTNTYPE = PREST.CTNTYPE")
        SQLBldr.AppendLine("         AND CIM.CTNNO = PREST.CTNNO")
        SQLBldr.AppendLine("         AND PREST.DELFLG <> '1'")
        SQLBldr.AppendLine("     LEFT OUTER JOIN com.LNS0020_STATION AS STA")
        SQLBldr.AppendLine("         ON  PREST.ARRSTATION = STA.STATION")
        Try
            Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0022tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    LNT0022tbl.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MapDataGet SELECT CONTAINER LIST")
            CS0011LOGWrite.INFSUBCLASS = "LNT0022C"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "MapDataGet SELECT CONTAINER LIST"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        'コンテナ検査管理テーブル取得
        SQLBldr.Clear()
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("     ROW_NUMBER() OVER(ORDER BY CTNTYPE, CTNNO, INSPELNTYPE, INSPECTSEQ) AS LINECNT")
        SQLBldr.AppendLine("     , CTNTYPE")
        SQLBldr.AppendLine("     , CTNNO")
        SQLBldr.AppendLine("     , INSPELNTYPE")
        SQLBldr.AppendLine("     , INSPECTSEQ")
        SQLBldr.AppendLine("     , INSPECTYEAR")
        SQLBldr.AppendLine("     , FORMAT(INSPECTYMD, 'yyyy/MM/dd') AS INSPECTYMD")
        SQLBldr.AppendLine("     , INSPECTCODE")
        SQLBldr.AppendLine("     , INSPECTNAME")
        SQLBldr.AppendLine("     , INSPECTVENDOR")
        SQLBldr.AppendLine("     , INSPECTVENDORNAME")
        SQLBldr.AppendLine("     , ENFORCEPLACE")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("      lng.LNT0092_CTN_INSPECT_MANAGE")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     DELFLG <> '1' ")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     CTNTYPE")
        SQLBldr.AppendLine("     , CTNNO")
        SQLBldr.AppendLine("     , INSPELNTYPE")
        SQLBldr.AppendLine("     , INSPECTSEQ")
        Try
            Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0022SUBtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    LNT0022SUBtbl.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MapDataGet SELECT INSPECT DATA")
            CS0011LOGWrite.INFSUBCLASS = "LNT0022C" 'SUBクラス名
            CS0011LOGWrite.INFPOSI = "MapDataGet SELECT INSPECT DATA"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()         'ログ出力
            Exit Sub
        End Try

        'コンテナ検査管理テーブル保存
        Master.SaveTable(LNT0022SUBtbl, work.WF_SEL_INP_CONINS_TBL.Text)

        'コンテナ一覧内容設定
        For Each dr As DataRow In LNT0022tbl.Rows
            '------------
            ' 検査日設定
            '------------
            '定期検査
            Dim regularRows As DataRow() = LNT0022SUBtbl.Select(
                    String.Format("CTNTYPE ='{0}' AND CTNNO = '{1}' AND INSPELNTYPE = '1'",
                                  dr("CTNTYPE"), dr("CTNNO")), "INSPECTSEQ")
            For Each row As DataRow In regularRows
                If row("INSPECTSEQ") = 4 Then
                    dr("YEAR4_INSPECTYMD") = row("INSPECTYMD")
                    dr("YEAR4_INSPECTCODE") = row("INSPECTCODE")
                    dr("YEAR4_INSPECTNAME") = row("INSPECTNAME")
                    dr("YEAR4_ENFORCEPLACE") = row("ENFORCEPLACE")
                    dr("YEAR4_INSPECTVENDOR") = row("INSPECTVENDOR")
                    dr("YEAR4_INSPECTVENDORNAME") = row("INSPECTVENDORNAME")
                    dr("YEAR4NOREGFLG") = 0
                ElseIf row("INSPECTSEQ") = 8 Then
                    dr("YEAR8_INSPECTYMD") = row("INSPECTYMD")
                    dr("YEAR8_INSPECTCODE") = row("INSPECTCODE")
                    dr("YEAR8_INSPECTNAME") = row("INSPECTNAME")
                    dr("YEAR8_ENFORCEPLACE") = row("ENFORCEPLACE")
                    dr("YEAR8_INSPECTVENDOR") = row("INSPECTVENDOR")
                    dr("YEAR8_INSPECTVENDORNAME") = row("INSPECTVENDORNAME")
                    dr("YEAR8NOREGFLG") = 0
                ElseIf row("INSPECTSEQ") = 12 Then
                    dr("YEAR12_INSPECTYMD") = row("INSPECTYMD")
                    dr("YEAR12_INSPECTCODE") = row("INSPECTCODE")
                    dr("YEAR12_INSPECTNAME") = row("INSPECTNAME")
                    dr("YEAR12_ENFORCEPLACE") = row("ENFORCEPLACE")
                    dr("YEAR12_INSPECTVENDOR") = row("INSPECTVENDOR")
                    dr("YEAR12_INSPECTVENDORNAME") = row("INSPECTVENDORNAME")
                    dr("YEAR12NOREGFLG") = 0
                End If
            Next
            '16年以上の定期検査データ取得
            Dim over16RegularRows As DataRow() = LNT0022SUBtbl.Select(
                    String.Format("CTNTYPE ='{0}' AND CTNNO = '{1}' AND INSPELNTYPE = '1' AND INSPECTSEQ >= 16",
                                  dr("CTNTYPE"), dr("CTNNO")), "INSPECTSEQ DESC")
            If over16RegularRows.Length > 0 Then
                dr("YEARN_SEQ") = over16RegularRows(0)("INSPECTSEQ")
                dr("YEARN_YEAR") = over16RegularRows(0)("INSPECTYEAR")
                dr("YEARN_INSPECTYMD") = over16RegularRows(0)("INSPECTYMD")
                dr("YEARN_INSPECTCODE") = over16RegularRows(0)("INSPECTCODE")
                dr("YEARN_INSPECTNAME") = over16RegularRows(0)("INSPECTNAME")
                dr("YEARN_ENFORCEPLACE") = over16RegularRows(0)("ENFORCEPLACE")
                dr("YEARN_INSPECTVENDOR") = over16RegularRows(0)("INSPECTVENDOR")
                dr("YEARN_INSPECTVENDORNAME") = over16RegularRows(0)("INSPECTVENDORNAME")
            End If
            '追加検査データ取得
            Dim additionRows As DataRow() = LNT0022SUBtbl.Select(
                    String.Format("CTNTYPE ='{0}' AND CTNNO = '{1}' AND INSPELNTYPE = '2'",
                                  dr("CTNTYPE"), dr("CTNNO")), "INSPECTYMD DESC")
            If additionRows.Count > 0 Then
                dr("ADD_SEQ") = additionRows(0)("INSPECTSEQ")
                dr("ADD_YEAR") = additionRows(0)("INSPECTYEAR").ToString
                dr("ADD_INSPECTYMD") = additionRows(0)("INSPECTYMD")
                dr("ADD_INSPECTCODE") = additionRows(0)("INSPECTCODE")
                dr("ADD_INSPECTNAME") = additionRows(0)("INSPECTNAME")
                dr("ADD_ENFORCEPLACE") = additionRows(0)("ENFORCEPLACE")
                dr("ADD_INSPECTVENDOR") = additionRows(0)("INSPECTVENDOR")
                dr("ADD_INSPECTVENDORNAME") = additionRows(0)("INSPECTVENDORNAME")
            End If

            '状態設定
            Dim status As Integer = 11
            '当年検査(未登録/登録済)
            If "1".Equals(dr("YEAR4FLG").ToString) Then
                status = If(String.IsNullOrEmpty(dr("YEAR4_INSPECTYMD").ToString), 1, 4)
            ElseIf "1".Equals(dr("YEAR8FLG").ToString) Then
                status = If(String.IsNullOrEmpty(dr("YEAR8_INSPECTYMD").ToString), 2, 5)
            ElseIf "1".Equals(dr("YEAR12FLG").ToString) Then
                status = If(String.IsNullOrEmpty(dr("YEAR12_INSPECTYMD").ToString), 3, 6)
            End If
            '昨年検査未登録
            If "1".Equals(dr("YEAR4NOREGFLG").ToString) Then
                status = 7
            ElseIf "1".Equals(dr("YEAR8NOREGFLG").ToString) Then
                status = 8
            ElseIf "1".Equals(dr("YEAR12NOREGFLG").ToString) Then
                status = 9
            End If
            '製造から16年以上経過
            If "1".Equals(dr("YEAR16OVERFLG").ToString) Then
                status = 10
            End If
            dr("STATUS") = status
        Next

        'コンテナ一覧ソート
        LNT0022tbl = New DataView(LNT0022tbl) With {
                        .Sort = "YEAR4FLG DESC, YEAR4NOREGFLG DESC, YEAR8FLG DESC, YEAR8NOREGFLG DESC," &
                                "YEAR12FLG DESC, YEAR12NOREGFLG DESC, YEAR16OVERFLG DESC," &
                                "ORGCODE, ARRSTATION, CTNTYPE, CTNNO"
                     }.ToTable()

        'LINECNT採番
        Dim i As Integer = 0
        For Each dr As DataRow In LNT0022tbl.Rows
            i += 1
            'LINENO
            dr("LINECNT") = i
        Next

        'コンテナ一覧テーブル保存
        Master.SaveTable(LNT0022tbl, work.WF_SEL_INP_CONM_TBL.Text)

        '-----------------------
        ' 絞り込み条件DDL初期化
        '-----------------------
        Dim lastValue As String = WF_STATUS.SelectedValue

        '状態
        WF_STATUS.Items.Clear()
        WF_STATUS.Items.Add(New ListItem("", ""))
        For Each dr As DataRow In LNT0022tbl.DefaultView.ToTable(True, New String() {"STATUS"}).Select("", "STATUS")
            Select Case dr("STATUS")
                Case 1
                    WF_STATUS.Items.Add(New ListItem("４年検査（未登録）", "1"))
                Case 2
                    WF_STATUS.Items.Add(New ListItem("８年検査（未登録）", "2"))
                Case 3
                    WF_STATUS.Items.Add(New ListItem("１２年検査（未登録）", "3"))
                Case 4
                    WF_STATUS.Items.Add(New ListItem("４年検査（登録済み）", "4"))
                Case 5
                    WF_STATUS.Items.Add(New ListItem("８年検査（登録済み）", "5"))
                Case 6
                    WF_STATUS.Items.Add(New ListItem("１２年検査（登録済み）", "6"))
                Case 7
                    WF_STATUS.Items.Add(New ListItem("昨年４年検査 未登録", "7"))
                Case 8
                    WF_STATUS.Items.Add(New ListItem("昨年８年検査 未登録", "8"))
                Case 9
                    WF_STATUS.Items.Add(New ListItem("昨年１２年検査 未登録", "9"))
                Case 10
                    WF_STATUS.Items.Add(New ListItem("製造から16年以上経過", "10"))
                Case 11
                    WF_STATUS.Items.Add(New ListItem("注意・警告なし", "11"))
            End Select
        Next
        If IsNothing(WF_STATUS.Items.FindByValue(lastValue)) Then
            WF_STATUS.SelectedValue = ""
        Else
            WF_STATUS.SelectedValue = lastValue
        End If

        'コンテナ種別
        lastValue = WF_CTNTYPE.SelectedValue
        WF_CTNTYPE.Items.Clear()
        WF_CTNTYPE.Items.Add(New ListItem("", ""))
        For Each dr As DataRow In LNT0022tbl.DefaultView.ToTable(True, New String() {"CTNTYPE"}).Select("", "CTNTYPE")
            WF_CTNTYPE.Items.Add(New ListItem(dr("CTNTYPE"), dr("CTNTYPE")))
        Next
        If IsNothing(WF_CTNTYPE.Items.FindByValue(lastValue)) Then
            WF_CTNTYPE.SelectedValue = ""
        Else
            WF_CTNTYPE.SelectedValue = lastValue
        End If

        'クライアント駅マスタ初期化
        Dim stationDt As New DataTable
        SQLBldr.Clear()
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("     STATION AS 'code'")
        SQLBldr.AppendLine("     , TRIM(REPLACE(REPLACE(NAMES, '（', ''), '）', '')) AS 'name'")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     com.LNS0020_STATION")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     DELFLG <> '1'")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     STATION")
        Try
            Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        stationDt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    stationDt.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "MapDataGet SELECT STATION")
            CS0011LOGWrite.INFSUBCLASS = "LNT0022C" 'SUBクラス名
            CS0011LOGWrite.INFPOSI = "MapDataGet SELECT STATION"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()         'ログ出力
            Exit Sub
        End Try
        WF_StationTable.Value = JsonConvert.SerializeObject(stationDt)

        'クライアント検査コードテーブル初期化
        WF_InspectCodes.Value = JsonConvert.SerializeObject(GetInspectCodeTable())

    End Sub

    ''' <summary>
    ''' 駅名取得
    ''' </summary>
    Private Sub GetStationName()

        WF_STATIONNAME.Text = ""

        If String.IsNullOrEmpty(WF_STATION.Text) OrElse Not IsNumeric(WF_STATION.Text) Then
            Exit Sub
        End If

        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("     RTRIM(NAMES) AS STATIONNAME")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     com.LNS0020_STATION")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     STATION = @STATION")
        Try
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)
                    With SQLcmd.Parameters
                        .Add("STATION", MySqlDbType.Int32).Value = CInt(WF_STATION.Text)
                    End With
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        If SQLdr.Read Then
                            WF_STATIONNAME.Text = SQLdr("STATIONNAME")
                        End If
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0022C GetStationName")
            CS0011LOGWrite.INFSUBCLASS = "LNT0022C"         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "GetStationName"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                 'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 検査コードテーブル取得
    ''' </summary>
    Private Function GetInspectCodeTable() As DataTable

        Dim dt As DataTable = New DataTable

        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("     KEYCODE As 'code'")
        SQLBldr.AppendLine("     , RTRIM(VALUE1) AS 'name'")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     com.LNS0006_FIXVALUE")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     CAMPCODE = '01'")
        SQLBldr.AppendLine(" AND CLASS = 'INSPECTCODE'")
        SQLBldr.AppendLine(" AND CURDATE() BETWEEN STYMD AND ENDYMD")
        SQLBldr.AppendLine(" AND DELFLG <> '1'")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     KEYCODE")
        Try
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0022C GetInspectCodeTable")
            CS0011LOGWrite.INFSUBCLASS = "LNT0022C"         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "GetInspectCodeTable"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                 'ログ出力
        End Try

        Return dt

    End Function

    ''' <summary>
    ''' 点検修理者テーブル取得
    ''' </summary>
    Private Function GetInspectVendorTable() As DataTable

        Dim dt As DataTable = New DataTable

        Dim SQLBldr As New StringBuilder
        SQLBldr.AppendLine(" SELECT")
        SQLBldr.AppendLine("     KEYCODE As 'key'")
        SQLBldr.AppendLine("     , RTRIM(VALUE1) AS 'value'")
        SQLBldr.AppendLine("     , CONVERT(INT, VALUE2) AS 'disabled'")
        SQLBldr.AppendLine(" FROM")
        SQLBldr.AppendLine("     com.LNS0006_FIXVALUE")
        SQLBldr.AppendLine(" WHERE")
        SQLBldr.AppendLine("     CAMPCODE = '01'")
        SQLBldr.AppendLine(" AND CLASS = 'INSPECTVENDOR'")
        SQLBldr.AppendLine(" AND CURDATE() BETWEEN STYMD AND ENDYMD")
        SQLBldr.AppendLine(" AND DELFLG <> '1'")
        SQLBldr.AppendLine(" ORDER BY")
        SQLBldr.AppendLine("     KEYCODE")
        Try
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()
                Using SQLcmd As New MySqlCommand(SQLBldr.ToString, SQLcon)
                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            dt.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                        '○ テーブル検索結果をテーブル格納
                        dt.Load(SQLdr)
                    End Using
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0022C GetInspectVendorTable")
            CS0011LOGWrite.INFSUBCLASS = "LNT0022C"         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "GetInspectVendorTable"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                 'ログ出力
        End Try

        Return dt

    End Function

#End Region

#Region "データ更新処理"

    ''' <summary>
    ''' 検査登録ダイアログ 更新処理
    ''' </summary>
    Private Function WF_InspectUpdate() As Boolean

        txtValidateMessage.Text = ""
        txtValidateMessage.CssClass = "hidden"

        'フィードバック
        Dim inspectDt As DataTable = FeedbackInspects()

        '入力チェック
        Dim errorYear As Boolean = False
        Dim errorYmd As Boolean = False
        Dim errorCode As Boolean = False
        Dim errorName As Boolean = False
        Dim nothingUpdate As Boolean = True
        For Each dr As DataRow In inspectDt.Rows
            dr("NOUPDATE") = 0
            dr("INSPECTYEAR_ERR") = 0
            dr("INSPECTYMD_ERR") = 0
            dr("INSPECTCODE_ERR") = 0
            dr("INSPECTNAME_ERR") = 0
            If dr("DELFLG") = 1 Then
                '更新対象
                nothingUpdate = False
            Else
                '定期検査データの場合、検査日/種別/種別名/実施場所/修理点検者
                If "1".Equals(dr("INSPELNTYPE").ToString) AndAlso
                   (String.IsNullOrEmpty(dr("INSPECTYMD").ToString) AndAlso
                    String.IsNullOrEmpty(dr("INSPECTCODE").ToString) AndAlso
                    String.IsNullOrEmpty(dr("INSPECTNAME").ToString) AndAlso
                    String.IsNullOrEmpty(dr("ENFORCEPLACE").ToString) AndAlso
                    String.IsNullOrEmpty(dr("INSPECTVENDOR").ToString)) Then
                    dr("NOUPDATE") = 1
                    Continue For
                End If
                '追加検査データの場合、検査年/検査日/種別/種別名/実施場所/修理点検者
                If "2".Equals(dr("INSPELNTYPE").ToString) AndAlso
                   (dr("INSPECTYEAR") = 0 AndAlso
                    String.IsNullOrEmpty(dr("INSPECTYMD").ToString) AndAlso
                    String.IsNullOrEmpty(dr("INSPECTCODE").ToString) AndAlso
                    String.IsNullOrEmpty(dr("INSPECTNAME").ToString) AndAlso
                    String.IsNullOrEmpty(dr("ENFORCEPLACE").ToString) AndAlso
                    String.IsNullOrEmpty(dr("INSPECTVENDOR").ToString)) Then
                    dr("NOUPDATE") = 1
                    Continue For
                End If
                '更新対象
                nothingUpdate = False
                '検査年未入力
                If dr("INSPECTYEAR") = 0 Then
                    dr("INSPECTYEAR_ERR") = 1
                    errorYear = True
                End If
                '検査日未入力
                If String.IsNullOrEmpty(dr("INSPECTYMD").ToString) Then
                    dr("INSPECTYMD_ERR") = 1
                    errorYmd = True
                End If
                '種別未設定
                If String.IsNullOrEmpty(dr("INSPECTCODE").ToString) Then
                    dr("INSPECTCODE_ERR") = 1
                    errorCode = True
                End If
                '種別名未入力
                If String.IsNullOrEmpty(dr("INSPECTNAME").ToString) Then
                    dr("INSPECTNAME_ERR") = 1
                    errorName = True
                End If
            End If
        Next
        '入力チェックエラー時
        If errorYear OrElse errorYmd OrElse errorCode OrElse errorName Then
            If errorYear Then
                txtValidateMessage.Text &= "検査年が未入力の行があります"
            End If
            If errorYmd Then
                txtValidateMessage.Text &= If(txtValidateMessage.Text.Length > 0, vbCrLf, "") &
                                           "検査日が未入力の行があります"
            End If
            If errorCode Then
                txtValidateMessage.Text &= If(txtValidateMessage.Text.Length > 0, vbCrLf, "") &
                                           "検査種別が未設定の行があります"
            End If
            If errorName Then
                txtValidateMessage.Text &= If(txtValidateMessage.Text.Length > 0, vbCrLf, "") &
                                           "種別名が未入力の行があります"
            End If
            'エラーメッセージ出力
            Master.Output(C_MESSAGE_NO.ERROR_RECORD_EXIST, C_MESSAGE_TYPE.ERR, , , True)
        ElseIf nothingUpdate Then
            txtValidateMessage.Text = "更新対象データがありません（全行未入力＆削除なし）"
            'エラーメッセージ出力
            Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.ERR, , , True)
        End If
        If nothingUpdate OrElse errorYear OrElse errorYmd OrElse errorCode OrElse errorName Then
            '入力チェックメッセージ表示
            txtValidateMessage.CssClass = ""
            '検査リスト設定
            SetInspectGridView(inspectDt)
            '異常終了
            Return False
        End If

        'DB更新処理
        If Not WF_Update(inspectDt) Then
            Return False
        End If

        '検査登録ダイアログ消去
        hdnShowPnlInspectDialog.Value = "0"
        'DB更新完了メッセージ出力
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, , , True)
        '正常終了
        Return True

    End Function

    ''' <summary>
    ''' Integer.TryParse
    ''' 空文、全角数値変換対応
    ''' </summary>
    Private Function TryParseInteger(ByVal iStr As String,
                                     ByRef oInt As Integer) As Boolean

        If String.IsNullOrEmpty(iStr) Then
            Return False
        End If

        '全角→半角変換
        Dim nStr As String = ""
        Try
            nStr = Strings.StrConv(iStr, VbStrConv.Narrow)
        Catch ex As Exception
            Return False
        End Try

        Return Integer.TryParse(nStr, oInt)

    End Function

    ''' <summary>
    ''' Date.TryParse
    ''' 空文、全角数値変換対応
    ''' </summary>
    Private Function TryParseDate(ByVal iStr As String,
                                  ByRef oDate As Date) As Boolean

        If String.IsNullOrEmpty(iStr) Then
            Return False
        End If

        '全角→半角変換
        Dim nStr As String = ""
        Try
            nStr = Strings.StrConv(iStr, VbStrConv.Narrow)
        Catch ex As Exception
            Return False
        End Try

        Return Date.TryParse(nStr, oDate)

    End Function

    ''' <summary>
    ''' Excelファイルアップロードによる更新処理
    ''' </summary>
    ''' <returns></returns>
    Private Function WF_InspectUpdateByFile() As Boolean

        If String.IsNullOrEmpty(WF_FileUpload.FileName) Then
            'エラーメッセージ出力
            Master.Output(C_MESSAGE_NO.CTN_UNIVERSAL_MESSAGE, C_MESSAGE_TYPE.ERR,
                          "アップロードファイルが選択されていません", "",
                          True, messageBoxTitle:="アップロードエラー")
            Return False
        Else
            Dim nameParts As String() = WF_FileUpload.FileName.Split(".")
            Dim extention As String = nameParts(nameParts.Length - 1)
            If Not "xlsx".Equals(extention) Then
                'エラーメッセージ出力
                Master.Output(C_MESSAGE_NO.CTN_UNIVERSAL_MESSAGE, C_MESSAGE_TYPE.ERR,
                              "アップロードしたファイルは、Excel形式ではありません", "",
                              True, messageBoxTitle:="アップロードエラー")
                Return False
            End If
        End If


        Dim uploadTempPath As String = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                              "UPLOAD_TMP", CS0050SESSION.USERID)
        Dim tmpFilePath As String = uploadTempPath & "\" & DateTime.Now.ToString("yyyyMMddHHmmssfff") & ".xlsx"

        'コンテナ一覧テーブル復元
        Dim ctnDt As New DataTable
        Master.RecoverTable(ctnDt, work.WF_SEL_INP_CONM_TBL.Text)

        '検査管理テーブル復元
        Dim inspectDt As New DataTable
        Master.RecoverTable(inspectDt, work.WF_SEL_INP_CONINS_TBL.Text)
        '入力用の検査登録テーブルの定義を生成する
        inspectDt.Columns.Add("NOUPDATE", Type.GetType("System.Int32"))
        inspectDt.Columns.Add("DELFLG", Type.GetType("System.Int32"))
        inspectDt.Rows.Clear()

        Try
            'ディレクトリが存在しなければ作成
            If Not System.IO.Directory.Exists(uploadTempPath) Then
                System.IO.Directory.CreateDirectory(uploadTempPath)
            End If

            '一時アップロード
            WF_FileUpload.SaveAs(tmpFilePath)

            '入力ファイルオープン
            Dim book As New Workbook
            book.Open(tmpFilePath)
            '入力シートを開く
            Dim sheet As IWorksheet = book.Worksheets(0)
            If Not "ID:LNT0022C".Equals(sheet.Range("A1").Value) Then
                'エラーメッセージ出力
                Master.Output(C_MESSAGE_NO.CTN_UNIVERSAL_MESSAGE, C_MESSAGE_TYPE.ERR,
                              "アップロードしたファイルは、コンテナ検査管理用の入力ファイルではありません",
                              "<BR>ダウンロードしたExcelファイルをご利用ください。",
                              True, messageBoxTitle:="アップロードエラー")
                Return False
            End If

            '検査コードテーブル取得
            Dim codeDt As DataTable = GetInspectCodeTable()

            '点検修理者テーブル取得
            Dim vendorDt As DataTable = GetInspectVendorTable()

            'レコード行の読み込み
            Dim ridx As Integer = 5
            Dim range As IRange = Nothing
            Dim ctnType As String = ""
            Dim ctnNo As Integer = 0
            Dim inspectSeq As Integer = 0
            Dim inspectYear As Integer = 0
            Dim inspectYmd As Date
            Dim inspectCode As Integer = 0
            Do
                '行の先頭を設定
                range = sheet.Range("A" & CStr(ridx))
                'コンテナ記号・番号が設定されていなければ読み込み終了
                ctnType = CStr(range.Cells(0, 0).Value)
                If String.IsNullOrEmpty(ctnType) OrElse
                   Not TryParseInteger(CStr(range.Cells(0, 1).Value), ctnNo) Then
                    Exit Do
                End If
                'コンテナデータ取得
                Dim ctnDr As DataRow() = ctnDt.Select("CTNTYPE = '" & ctnType & "' AND CTNNO = " & ctnNo)
                If ctnDr.Length = 0 Then
                    Continue Do
                End If
                '製造年
                Dim contructionYear As Integer = CDate(ctnDr(0)("CONTRUCTIONYM").ToString).Year
                '４年検査
                If TryParseDate(CStr(range.Cells(0, 8).Value), inspectYmd) AndAlso
                   TryParseInteger(CStr(range.Cells(0, 9).Value), inspectCode) Then
                    '検査コード取得
                    Dim codeDrs As DataRow() = codeDt.Select("code ='" + inspectCode.ToString + "'")
                    If codeDrs.Length > 0 Then
                        Dim nRow As DataRow = inspectDt.NewRow
                        nRow("LINECNT") = 0
                        nRow("CTNTYPE") = ctnType
                        nRow("CTNNO") = ctnNo
                        nRow("INSPELNTYPE") = "1"
                        nRow("INSPECTSEQ") = 4
                        '検査年
                        nRow("INSPECTYEAR") = contructionYear + 4
                        '検査日
                        nRow("INSPECTYMD") = inspectYmd.ToString("yyyy/MM/dd")
                        '検査コード
                        nRow("INSPECTCODE") = inspectCode
                        '検査名
                        If String.IsNullOrEmpty(CStr(range.Cells(0, 10).Value)) Then
                            nRow("INSPECTNAME") = codeDrs(0)("name")
                        Else
                            nRow("INSPECTNAME") = CStr(range.Cells(0, 10).Value)
                        End If
                        '修理点検者
                        If Not String.IsNullOrEmpty(CStr(range.Cells(0, 12).Value)) Then
                            Dim vendorStrs As String() = CStr(range.Cells(0, 12).Value).Split(":"c)
                            If vendorStrs.Length = 2 Then
                                Dim vendorDrs As DataRow() = vendorDt.Select("key = '" & vendorStrs(0) & "'")
                                If vendorDrs.Length > 0 Then
                                    nRow("INSPECTVENDOR") = vendorDrs(0)("key")
                                    nRow("INSPECTVENDORNAME") = vendorDrs(0)("value")
                                End If
                            End If
                        Else
                            nRow("INSPECTVENDOR") = ""
                            nRow("INSPECTVENDORNAME") = ""
                        End If
                        '実施場所
                        nRow("ENFORCEPLACE") = CStr(range.Cells(0, 11).Value)
                        nRow("NOUPDATE") = 0
                        nRow("DELFLG") = 0
                        inspectDt.Rows.Add(nRow)
                    End If
                End If
                '８年検査
                If TryParseDate(CStr(range.Cells(0, 14).Value), inspectYmd) AndAlso
                   TryParseInteger(CStr(range.Cells(0, 15).Value), inspectCode) Then
                    '検査コード取得
                    Dim codeDrs As DataRow() = codeDt.Select("code ='" + inspectCode.ToString + "'")
                    If codeDrs.Length > 0 Then
                        Dim nRow As DataRow = inspectDt.NewRow
                        nRow("LINECNT") = 0
                        nRow("CTNTYPE") = ctnType
                        nRow("CTNNO") = ctnNo
                        nRow("INSPELNTYPE") = "1"
                        nRow("INSPECTSEQ") = 8
                        '検査年
                        nRow("INSPECTYEAR") = contructionYear + 8
                        '検査日
                        nRow("INSPECTYMD") = inspectYmd.ToString("yyyy/MM/dd")
                        '検査コード
                        nRow("INSPECTCODE") = inspectCode
                        '検査名
                        If String.IsNullOrEmpty(CStr(range.Cells(0, 16).Value)) Then
                            nRow("INSPECTNAME") = codeDrs(0)("name")
                        Else
                            nRow("INSPECTNAME") = CStr(range.Cells(0, 16).Value)
                        End If
                        '修理点検者
                        If Not String.IsNullOrEmpty(CStr(range.Cells(0, 18).Value)) Then
                            Dim vendorStrs As String() = CStr(range.Cells(0, 18).Value).Split(":"c)
                            If vendorStrs.Length = 2 Then
                                Dim vendorDrs As DataRow() = vendorDt.Select("key = '" & vendorStrs(0) & "'")
                                If vendorDrs.Length > 0 Then
                                    nRow("INSPECTVENDOR") = vendorDrs(0)("key")
                                    nRow("INSPECTVENDORNAME") = vendorDrs(0)("value")
                                End If
                            End If
                        Else
                            nRow("INSPECTVENDOR") = ""
                            nRow("INSPECTVENDORNAME") = ""
                        End If
                        '実施場所
                        nRow("ENFORCEPLACE") = CStr(range.Cells(0, 17).Value)
                        nRow("NOUPDATE") = 0
                        nRow("DELFLG") = 0
                        inspectDt.Rows.Add(nRow)
                    End If
                End If
                '１２年検査
                If TryParseDate(CStr(range.Cells(0, 20).Value), inspectYmd) AndAlso
                   TryParseInteger(CStr(range.Cells(0, 21).Value), inspectCode) Then
                    '検査コード取得
                    Dim codeDrs As DataRow() = codeDt.Select("code ='" + inspectCode.ToString + "'")
                    If codeDrs.Length > 0 Then
                        Dim nRow As DataRow = inspectDt.NewRow
                        nRow("LINECNT") = 0
                        nRow("CTNTYPE") = ctnType
                        nRow("CTNNO") = ctnNo
                        nRow("INSPELNTYPE") = "1"
                        nRow("INSPECTSEQ") = 12
                        '検査年
                        nRow("INSPECTYEAR") = contructionYear + 12
                        '検査日
                        nRow("INSPECTYMD") = inspectYmd.ToString("yyyy/MM/dd")
                        '検査コード
                        nRow("INSPECTCODE") = inspectCode
                        '検査名
                        If String.IsNullOrEmpty(CStr(range.Cells(0, 22).Value)) Then
                            nRow("INSPECTNAME") = codeDrs(0)("name")
                        Else
                            nRow("INSPECTNAME") = CStr(range.Cells(0, 22).Value)
                        End If
                        '修理点検者
                        If Not String.IsNullOrEmpty(CStr(range.Cells(0, 24).Value)) Then
                            Dim vendorStrs As String() = CStr(range.Cells(0, 24).Value).Split(":"c)
                            If vendorStrs.Length = 2 Then
                                Dim vendorDrs As DataRow() = vendorDt.Select("key = '" & vendorStrs(0) & "'")
                                If vendorDrs.Length > 0 Then
                                    nRow("INSPECTVENDOR") = vendorDrs(0)("key")
                                    nRow("INSPECTVENDORNAME") = vendorDrs(0)("value")
                                End If
                            End If
                        Else
                            nRow("INSPECTVENDOR") = ""
                            nRow("INSPECTVENDORNAME") = ""
                        End If
                        '実施場所
                        nRow("ENFORCEPLACE") = CStr(range.Cells(0, 23).Value)
                        nRow("NOUPDATE") = 0
                        nRow("DELFLG") = 0
                        inspectDt.Rows.Add(nRow)
                    End If
                End If
                'Ｎ年検査
                If TryParseInteger(CStr(range.Cells(0, 25).Value), inspectSeq) AndAlso
                   TryParseDate(CStr(range.Cells(0, 27).Value), inspectYmd) AndAlso
                   TryParseInteger(CStr(range.Cells(0, 28).Value), inspectCode) Then
                    '検査コード取得
                    Dim codeDrs As DataRow() = codeDt.Select("code ='" + inspectCode.ToString + "'")
                    If inspectSeq >= 16 AndAlso inspectSeq Mod 4 = 0 AndAlso codeDrs.Length > 0 Then
                        Dim nRow As DataRow = inspectDt.NewRow
                        nRow("LINECNT") = 0
                        nRow("CTNTYPE") = ctnType
                        nRow("CTNNO") = ctnNo
                        nRow("INSPELNTYPE") = "1"
                        nRow("INSPECTSEQ") = inspectSeq
                        '検査年
                        nRow("INSPECTYEAR") = contructionYear + inspectSeq
                        '検査日
                        nRow("INSPECTYMD") = inspectYmd.ToString("yyyy/MM/dd")
                        '検査コード
                        nRow("INSPECTCODE") = inspectCode
                        '検査名
                        If String.IsNullOrEmpty(CStr(range.Cells(0, 29).Value)) Then
                            nRow("INSPECTNAME") = codeDrs(0)("name")
                        Else
                            nRow("INSPECTNAME") = CStr(range.Cells(0, 29).Value)
                        End If
                        '修理点検者
                        If Not String.IsNullOrEmpty(CStr(range.Cells(0, 31).Value)) Then
                            Dim vendorStrs As String() = CStr(range.Cells(0, 31).Value).Split(":"c)
                            If vendorStrs.Length = 2 Then
                                Dim vendorDrs As DataRow() = vendorDt.Select("key = '" & vendorStrs(0) & "'")
                                If vendorDrs.Length > 0 Then
                                    nRow("INSPECTVENDOR") = vendorDrs(0)("key")
                                    nRow("INSPECTVENDORNAME") = vendorDrs(0)("value")
                                End If
                            End If
                        Else
                            nRow("INSPECTVENDOR") = ""
                            nRow("INSPECTVENDORNAME") = ""
                        End If
                        '実施場所
                        nRow("ENFORCEPLACE") = CStr(range.Cells(0, 30).Value)
                        nRow("NOUPDATE") = 0
                        nRow("DELFLG") = 0
                        inspectDt.Rows.Add(nRow)
                    End If
                End If
                '追加検査
                inspectSeq = 0
                If (String.IsNullOrEmpty(CStr(range.Cells(0, 32).Value)) OrElse
                    TryParseInteger(CStr(range.Cells(0, 32).Value), inspectSeq)) AndAlso
                   TryParseInteger(CStr(range.Cells(0, 33).Value), inspectYear) AndAlso
                   TryParseDate(CStr(range.Cells(0, 34).Value), inspectYmd) AndAlso
                   TryParseInteger(CStr(range.Cells(0, 35).Value), inspectCode) Then
                    '検査コード取得
                    Dim codeDrs As DataRow() = codeDt.Select("code ='" + inspectCode.ToString + "'")
                    If codeDrs.Length > 0 Then
                        Dim nRow As DataRow = inspectDt.NewRow
                        nRow("LINECNT") = 0
                        nRow("CTNTYPE") = ctnType
                        nRow("CTNNO") = ctnNo
                        nRow("INSPELNTYPE") = "2"
                        nRow("INSPECTSEQ") = inspectSeq
                        '検査年
                        nRow("INSPECTYEAR") = inspectYear
                        '検査日
                        nRow("INSPECTYMD") = inspectYmd.ToString("yyyy/MM/dd")
                        '検査コード
                        nRow("INSPECTCODE") = inspectCode
                        '検査名
                        If String.IsNullOrEmpty(CStr(range.Cells(0, 36).Value)) Then
                            nRow("INSPECTNAME") = codeDrs(0)("name")
                        Else
                            nRow("INSPECTNAME") = CStr(range.Cells(0, 36).Value)
                        End If
                        '修理点検者
                        If Not String.IsNullOrEmpty(CStr(range.Cells(0, 38).Value)) Then
                            Dim vendorStrs As String() = CStr(range.Cells(0, 38).Value).Split(":"c)
                            If vendorStrs.Length = 2 Then
                                Dim vendorDrs As DataRow() = vendorDt.Select("key = '" & vendorStrs(0) & "'")
                                If vendorDrs.Length > 0 Then
                                    nRow("INSPECTVENDOR") = vendorDrs(0)("key")
                                    nRow("INSPECTVENDORNAME") = vendorDrs(0)("value")
                                End If
                            End If
                        Else
                            nRow("INSPECTVENDOR") = ""
                            nRow("INSPECTVENDORNAME") = ""
                        End If
                        '実施場所
                        nRow("ENFORCEPLACE") = CStr(range.Cells(0, 37).Value)
                        nRow("NOUPDATE") = 0
                        nRow("DELFLG") = 0
                        inspectDt.Rows.Add(nRow)
                    End If
                End If
                '次行処理
                ridx += 1
            Loop

        Catch ex As Exception
            'エラーメッセージ出力
            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT,
                          "Excel入出力ファイル読み込み失敗", "",
                          True, messageBoxTitle:="アップロードエラー")
            'ログ出力
            CS0011LOGWrite.INFSUBCLASS = "LNT0022C"  'SUBクラス名
            CS0011LOGWrite.INFPOSI = "WF_InspectUpdateByFile FileImport"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWrite.CS0011LOGWrite()             'ログ出力
            '異常終了
            Return False
        Finally
            Try
                'アップロード部品を初期化
                WF_FileUpload = New FileUpload()
                '一時アップロードファイルを削除
                IO.File.Delete(tmpFilePath)
            Catch ex As Exception
                '削除時エラーは無視
            End Try
        End Try

        'DB更新処理
        If Not WF_Update(inspectDt, True) Then
            Return False
        End If

        'DB更新完了メッセージ出力
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, , , True)

        '正常終了
        Return True

    End Function

    ''' <summary>
    ''' 検査管理テーブル更新処理
    ''' </summary>
    Private Function WF_Update(inspectDt As DataTable,
                               Optional upload As Boolean = False) As Boolean

#Region "削除SQL"
        '検査管理データ削除SQL
        Dim DelSQLBldr As New StringBuilder()
        DelSQLBldr.AppendLine(" DELETE FROM lng.LNT0092_CTN_INSPECT_MANAGE")
        DelSQLBldr.AppendLine(" WHERE")
        DelSQLBldr.AppendLine("     CTNTYPE = @CTNTYPE")
        DelSQLBldr.AppendLine(" AND CTNNO = @CTNNO")
        DelSQLBldr.AppendLine(" AND INSPELNTYPE = @INSPELNTYPE")
        DelSQLBldr.AppendLine(" AND INSPECTSEQ = @INSPECTSEQ;")
#End Region

#Region "更新SQL"
        '検査管理データ更新SQL生成
        Dim UpdSQLBldr As New StringBuilder()
        UpdSQLBldr.AppendLine(" DECLARE @NEWSEQ AS INT;")
        UpdSQLBldr.AppendLine(" DECLARE @HENSUU AS BIGINT;")
        UpdSQLBldr.AppendLine(" SET @HENSUU = 0;")
        UpdSQLBldr.AppendLine(" DECLARE HENSUU CURSOR LOCAL FOR")
        UpdSQLBldr.AppendLine("     SELECT")
        UpdSQLBldr.AppendLine("         UPDTIMSTP AS HENSUU")
        UpdSQLBldr.AppendLine("     FROM")
        UpdSQLBldr.AppendLine("         lng.LNT0092_CTN_INSPECT_MANAGE")
        UpdSQLBldr.AppendLine("     WHERE")
        UpdSQLBldr.AppendLine("         CTNTYPE = @CTNTYPE")
        UpdSQLBldr.AppendLine("     AND CTNNO = @CTNNO")
        UpdSQLBldr.AppendLine("     AND INSPELNTYPE = @INSPELNTYPE")
        UpdSQLBldr.AppendLine("     AND INSPECTSEQ = @INSPECTSEQ;")
        UpdSQLBldr.AppendLine(" OPEN HENSUU;")
        UpdSQLBldr.AppendLine(" FETCH NEXT FROM HENSUU INTO @HENSUU;")
        UpdSQLBldr.AppendLine(" IF @@FETCH_STATUS <> 0")
        UpdSQLBldr.AppendLine("     BEGIN")
        If upload Then
            UpdSQLBldr.AppendLine("         IF @INSPELNTYPE = '2'")
            UpdSQLBldr.AppendLine("             BEGIN")
            UpdSQLBldr.AppendLine("                 SET @NEWSEQ = (")
            UpdSQLBldr.AppendLine("                     SELECT")
            UpdSQLBldr.AppendLine("                         coalesce(MAX(INSPECTSEQ), 0) + 1")
            UpdSQLBldr.AppendLine("                     FROM")
            UpdSQLBldr.AppendLine("                         lng.LNT0092_CTN_INSPECT_MANAGE")
            UpdSQLBldr.AppendLine("                     WHERE")
            UpdSQLBldr.AppendLine("                         CTNTYPE = @CTNTYPE")
            UpdSQLBldr.AppendLine("                     AND CTNNO = @CTNNO")
            UpdSQLBldr.AppendLine("                     AND INSPELNTYPE = @INSPELNTYPE")
            UpdSQLBldr.AppendLine("                 );")
            UpdSQLBldr.AppendLine("             END;")
            UpdSQLBldr.AppendLine("         ELSE")
            UpdSQLBldr.AppendLine("             BEGIN")
            UpdSQLBldr.AppendLine("                 SET @NEWSEQ = @INSPECTSEQ;")
            UpdSQLBldr.AppendLine("             END;")
        Else
            UpdSQLBldr.AppendLine("         SET @NEWSEQ = @INSPECTSEQ;")
        End If
        UpdSQLBldr.AppendLine("         INSERT INTO lng.LNT0092_CTN_INSPECT_MANAGE( ")
        UpdSQLBldr.AppendLine("             CTNTYPE")
        UpdSQLBldr.AppendLine("             , CTNNO")
        UpdSQLBldr.AppendLine("             , INSPELNTYPE")
        UpdSQLBldr.AppendLine("             , INSPECTSEQ")
        UpdSQLBldr.AppendLine("             , INSPECTYEAR")
        UpdSQLBldr.AppendLine("             , INSPECTYMD")
        UpdSQLBldr.AppendLine("             , INSPECTCODE")
        UpdSQLBldr.AppendLine("             , INSPECTNAME")
        UpdSQLBldr.AppendLine("             , INSPECTVENDOR")
        UpdSQLBldr.AppendLine("             , INSPECTVENDORNAME")
        UpdSQLBldr.AppendLine("             , ENFORCEPLACE")
        UpdSQLBldr.AppendLine("             , DELFLG")
        UpdSQLBldr.AppendLine("             , INITYMD")
        UpdSQLBldr.AppendLine("             , INITUSER")
        UpdSQLBldr.AppendLine("             , INITTERMID")
        UpdSQLBldr.AppendLine("             , INITPGID")
        UpdSQLBldr.AppendLine("             , UPDYMD")
        UpdSQLBldr.AppendLine("             , UPDUSER")
        UpdSQLBldr.AppendLine("             , UPDTERMID")
        UpdSQLBldr.AppendLine("             , UPDPGID")
        UpdSQLBldr.AppendLine("             , RECEIVEYMD")
        UpdSQLBldr.AppendLine("         ) VALUES ( ")
        UpdSQLBldr.AppendLine("             @CTNTYPE")
        UpdSQLBldr.AppendLine("             , @CTNNO")
        UpdSQLBldr.AppendLine("             , @INSPELNTYPE")
        UpdSQLBldr.AppendLine("             , @NEWSEQ")
        UpdSQLBldr.AppendLine("             , @INSPECTYEAR")
        UpdSQLBldr.AppendLine("             , @INSPECTYMD")
        UpdSQLBldr.AppendLine("             , @INSPECTCODE")
        UpdSQLBldr.AppendLine("             , @INSPECTNAME")
        UpdSQLBldr.AppendLine("             , @INSPECTVENDOR")
        UpdSQLBldr.AppendLine("             , @INSPECTVENDORNAME")
        UpdSQLBldr.AppendLine("             , @ENFORCEPLACE")
        UpdSQLBldr.AppendLine("             , '0'")
        UpdSQLBldr.AppendLine("             , @UPDYMD")
        UpdSQLBldr.AppendLine("             , @UPDUSER")
        UpdSQLBldr.AppendLine("             , @UPDTERMID")
        UpdSQLBldr.AppendLine("             , @UPDPGID")
        UpdSQLBldr.AppendLine("             , @UPDYMD")
        UpdSQLBldr.AppendLine("             , @UPDUSER")
        UpdSQLBldr.AppendLine("             , @UPDTERMID")
        UpdSQLBldr.AppendLine("             , @UPDPGID")
        UpdSQLBldr.AppendLine("             , @UPDYMD")
        UpdSQLBldr.AppendLine("         );")
        UpdSQLBldr.AppendLine("     END;")
        UpdSQLBldr.AppendLine(" ELSE")
        UpdSQLBldr.AppendLine("     BEGIN")
        UpdSQLBldr.AppendLine("         UPDATE lng.LNT0092_CTN_INSPECT_MANAGE")
        UpdSQLBldr.AppendLine("         SET")
        UpdSQLBldr.AppendLine("             INSPECTYEAR = @INSPECTYEAR")
        UpdSQLBldr.AppendLine("             , INSPECTYMD = @INSPECTYMD")
        UpdSQLBldr.AppendLine("             , INSPECTCODE = @INSPECTCODE")
        UpdSQLBldr.AppendLine("             , INSPECTNAME = @INSPECTNAME")
        UpdSQLBldr.AppendLine("             , INSPECTVENDOR = @INSPECTVENDOR")
        UpdSQLBldr.AppendLine("             , INSPECTVENDORNAME = @INSPECTVENDORNAME")
        UpdSQLBldr.AppendLine("             , ENFORCEPLACE = @ENFORCEPLACE")
        UpdSQLBldr.AppendLine("             , UPDYMD = @UPDYMD")
        UpdSQLBldr.AppendLine("             , UPDUSER = @UPDUSER")
        UpdSQLBldr.AppendLine("             , UPDTERMID = @UPDTERMID")
        UpdSQLBldr.AppendLine("             , UPDPGID = @UPDPGID")
        UpdSQLBldr.AppendLine("             , RECEIVEYMD = @UPDYMD")
        UpdSQLBldr.AppendLine("         WHERE")
        UpdSQLBldr.AppendLine("             CTNTYPE = @CTNTYPE")
        UpdSQLBldr.AppendLine("         AND CTNNO = @CTNNO")
        UpdSQLBldr.AppendLine("         AND INSPELNTYPE = @INSPELNTYPE")
        UpdSQLBldr.AppendLine("         AND INSPECTSEQ = @INSPECTSEQ")
        UpdSQLBldr.AppendLine("         AND (")
        UpdSQLBldr.AppendLine("             INSPECTYEAR <> @INSPECTYEAR")
        UpdSQLBldr.AppendLine("             OR")
        UpdSQLBldr.AppendLine("             INSPECTYMD <> @INSPECTYMD")
        UpdSQLBldr.AppendLine("             OR")
        UpdSQLBldr.AppendLine("             INSPECTCODE <> @INSPECTCODE")
        UpdSQLBldr.AppendLine("             OR")
        UpdSQLBldr.AppendLine("             INSPECTNAME <> @INSPECTNAME")
        UpdSQLBldr.AppendLine("             OR")
        UpdSQLBldr.AppendLine("             INSPECTVENDOR <> @INSPECTVENDOR")
        UpdSQLBldr.AppendLine("             OR")
        UpdSQLBldr.AppendLine("             INSPECTVENDOR <> @INSPECTVENDORNAME")
        UpdSQLBldr.AppendLine("             OR")
        UpdSQLBldr.AppendLine("             ENFORCEPLACE <> @ENFORCEPLACE")
        UpdSQLBldr.AppendLine("         );")
        UpdSQLBldr.AppendLine("     END;")
        UpdSQLBldr.AppendLine(" CLOSE HENSUU;")
        UpdSQLBldr.AppendLine(" DEALLOCATE HENSUU;")
#End Region

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            'DB接続
            SQLcon.Open()
            'トランザクション開始
            Dim Tran As MySqlTransaction = SQLcon.BeginTransaction
            '削除SQL実行
            Try
                Using SQLcmd As New MySqlCommand(DelSQLBldr.ToString, SQLcon)
                    SQLcmd.Transaction = Tran
                    SQLcmd.CommandTimeout = 300
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@CTNTYPE", MySqlDbType.VarChar, 5)        'コンテナ記号
                    Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@CTNNO", MySqlDbType.Decimal)              'コンテナ番号
                    Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@INSPELNTYPE", MySqlDbType.VarChar, 1)    '検査種別
                    Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@INSPECTSEQ", MySqlDbType.Int32)             '検査SEQ
                    '削除データを抽出して処理する
                    For Each dr As DataRow In inspectDt.Select("DELFLG = 1", "INSPELNTYPE, INSPECTSEQ")
                        'パラメータ設定
                        PARA1.Value = dr("CTNTYPE")
                        PARA2.Value = dr("CTNNO")
                        PARA3.Value = dr("INSPELNTYPE")
                        PARA4.Value = dr("INSPECTSEQ")
                        'SQL実行
                        SQLcmd.ExecuteNonQuery()
                    Next
                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT,
                              "WF_Update DELETE LNT0092_CTN_INSPECT_MANAGE", "", True)
                CS0011LOGWrite.INFSUBCLASS = "LNT0022C"     'SUBクラス名
                CS0011LOGWrite.INFPOSI = "WF_Update DELETE LNT0092_CTN_INSPECT_MANAGE"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()             'ログ出力
                'ロールバック
                Tran.Rollback()
                '異常終了
                Return False
            End Try
            '更新SQL実行
            Try
                Using SQLcmd As New MySqlCommand(UpdSQLBldr.ToString, SQLcon)
                    SQLcmd.Transaction = Tran
                    SQLcmd.CommandTimeout = 300
                    With SQLcmd.Parameters
                        .Add("@UPDYMD", MySqlDbType.DateTime).Value = DateTime.Now
                        .Add("@UPDUSER", MySqlDbType.VarChar, 20).Value = Master.USERID
                        .Add("@UPDTERMID", MySqlDbType.VarChar, 20).Value = Master.USERTERMID
                        .Add("@UPDPGID", MySqlDbType.VarChar, 40).Value = "LNT0022InspeLNManage"
                    End With
                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@CTNTYPE", MySqlDbType.VarChar, 5)           'コンテナ記号
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@CTNNO", MySqlDbType.Decimal)                 'コンテナ番号
                    Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@INSPELNTYPE", MySqlDbType.VarChar, 1)       '検査種別
                    Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@INSPECTSEQ", MySqlDbType.Int32)                '検査SEQ
                    Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@INSPECTYEAR", MySqlDbType.Int32)               '検査年
                    Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@INSPECTYMD", MySqlDbType.Date)               '検査実施日
                    Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@INSPECTCODE", MySqlDbType.VarChar, 1)       '検査コード
                    Dim PARA08 As MySqlParameter = SQLcmd.Parameters.Add("@INSPECTNAME", MySqlDbType.VarChar, 20)      '検査名
                    Dim PARA09 As MySqlParameter = SQLcmd.Parameters.Add("@INSPECTVENDOR", MySqlDbType.VarChar, 3)      '点検修理者コード
                    Dim PARA10 As MySqlParameter = SQLcmd.Parameters.Add("@INSPECTVENDORNAME", MySqlDbType.VarChar, 30) '点検修理者名
                    Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@ENFORCEPLACE", MySqlDbType.VarChar, 30)     '実施場所
                    '未削除＆更新対象データを抽出して処理する
                    For Each dr As DataRow In inspectDt.Select("DELFLG = 0 AND NOUPDATE = 0", "CTNTYPE, CTNNO, INSPELNTYPE, INSPECTSEQ")
                        'パラメータ設定
                        PARA01.Value = dr("CTNTYPE")
                        PARA02.Value = dr("CTNNO")
                        PARA03.Value = dr("INSPELNTYPE")
                        PARA04.Value = dr("INSPECTSEQ")
                        PARA05.Value = dr("INSPECTYEAR")
                        PARA06.Value = CDate(dr("INSPECTYMD").ToString)
                        PARA07.Value = dr("INSPECTCODE")
                        PARA08.Value = dr("INSPECTNAME")
                        PARA09.Value = dr("INSPECTVENDOR")
                        PARA10.Value = dr("INSPECTVENDORNAME")
                        PARA11.Value = dr("ENFORCEPLACE")
                        'SQL実行
                        SQLcmd.ExecuteNonQuery()
                    Next
                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT,
                              "WF_Update INSERT/UPDATE LNT0092_CTN_INSPECT_MANAGE", "", True)
                CS0011LOGWrite.INFSUBCLASS = "LNT0022C"     'SUBクラス名
                CS0011LOGWrite.INFPOSI = "WF_Update INSERT/UPDATE LNT0092_CTN_INSPECT_MANAGE"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()             'ログ出力
                'ロールバック
                Tran.Rollback()
                '異常終了
                Return False
            End Try
            'トランザクションCommit
            Tran.Commit()
        End Using

        '正常終了
        Return True

    End Function

#End Region

#Region "ダウンロード/帳票"

    ''' <summary>
    ''' Excel入出力ファイルダウンロード
    ''' </summary>
    Private Sub WF_Download()

        WF_PrintURL1.Value = ""
        WF_PrintURL2.Value = ""
        WF_PrintURL3.Value = ""
        WF_PrintURL4.Value = ""

        Dim ctnDt As DataTable = Nothing
        'テーブル復元
        Master.RecoverTable(ctnDt, work.WF_SEL_INP_CONM_TBL.Text)

        '--------------------------
        ' コンテナ一覧表示絞り込み
        '--------------------------
        Dim rowFilter As String = ""
        'ログインユーザーの管轄支店が支店の場合、コンテナを組織コードで絞り込み
        If CONST_OFFICECODE_HOKKAIDO.Equals(Master.USER_ORG) OrElse
           CONST_OFFICECODE_TOHOKU.Equals(Master.USER_ORG) OrElse
           CONST_OFFICECODE_KANTO.Equals(Master.USER_ORG) OrElse
           CONST_OFFICECODE_CHUBU.Equals(Master.USER_ORG) OrElse
           CONST_OFFICECODE_KANSAI.Equals(Master.USER_ORG) OrElse
           CONST_OFFICECODE_KYUSYU.Equals(Master.USER_ORG) Then
            rowFilter = String.Format("ORGCODE IN ('{0}', '999999')", Master.USER_ORG)
        End If
        '状態
        If WF_STATUS.Items.Count > 0 AndAlso WF_STATUS.SelectedIndex <> 0 Then
            rowFilter &= If(String.IsNullOrEmpty(rowFilter), "", " AND ") &
                         "STATUS = " & WF_STATUS.SelectedValue
        End If
        'コンテナ種別
        If WF_CTNTYPE.Items.Count > 0 AndAlso WF_CTNTYPE.SelectedIndex <> 0 Then
            rowFilter &= If(String.IsNullOrEmpty(rowFilter), "", " AND ") &
                         "CTNTYPE = '" & WF_CTNTYPE.SelectedValue & "'"
        End If
        'コンテナ番号
        If Not String.IsNullOrEmpty(WF_CTNNO.Text) Then
            rowFilter &= If(String.IsNullOrEmpty(rowFilter), "", " AND ") &
                         "Convert(CTNNO, System.String) LIKE '%" & WF_CTNNO.Text & "%'"
        End If
        '駅コード
        If Not String.IsNullOrEmpty(WF_STATION.Text) Then
            rowFilter &= If(String.IsNullOrEmpty(rowFilter), "", " AND ") &
                         "Convert(ARRSTATION, System.String) LIKE '%" & WF_STATION.Text & "%'"
        End If
        ctnDt = New DataView(ctnDt) With {.RowFilter = rowFilter}.ToTable

        Try
            '帳票URL取得
            WF_PrintURL1.Value = New LNT0022_CustomReport(ctnDt).CreatePrintData()

            'ダウンロード設定
            ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_BulkDownload();", True)

        Catch ex As Exception
            'エラーメッセージ出力
            Master.Output(C_MESSAGE_NO.FILE_IO_ERROR, C_MESSAGE_TYPE.ABORT, "Excel入出力ファイル取得失敗", "", True)
            'ログ出力
            CS0011LOGWrite.INFSUBCLASS = "LNT0022C"  'SUBクラス名
            CS0011LOGWrite.INFPOSI = "WF_Download"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.FILE_IO_ERROR
            CS0011LOGWrite.CS0011LOGWrite()             'ログ出力
        End Try

    End Sub

#End Region

End Class