''************************************************************
' 【廃止】特別料金マスタメンテ登録画面(北海道ガス特別料金)
' 作成日 2025/02/06
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2025/02/06 新規作成
'          : 2025/03/18 廃止　→ LNM0014Sprate(統合版特別料金マスタへ変更)
''************************************************************
Imports MySql.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 北海道ガス特別料金マスタ更新
''' </summary>
''' <remarks></remarks>
Public Class LNM0010SprateDetailKG
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0010tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0010INPtbl As DataTable                              'チェック用テーブル
    Private LNM0010UPDtbl As DataTable                              '更新用テーブル

    '○ データOPERATION用
    Private Const CONST_INSERT As String = "Insert"                 'データ追加
    Private Const CONST_UPDATE As String = "Update"                 'データ更新
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         '関連チェックエラー
    Private Const ADDDATE As Integer = 90                           '有効期限追加日数

    Private Const CONST_MAX_ITEM_CNT As Integer = 20              '大項目最大件数
    Private Const CONST_MAX_RECO_CNT As Integer = 20              '大項目毎レコード最大件数

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    '○ 共通処理結果
    Private WW_ErrSW As String = ""
    Private WW_RtnSW As String = ""
    Private WW_Dummy As String = ""
    Private WW_ErrCode As String                                    'サブ用リターンコード

    ''' <summary>
    ''' サーバー処理の遷移先
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(LNM0010tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_ButtonCLEAR", "LNM0010L"  '戻るボタン押下（LNM0010Lは、パンくずより）
                            WF_CLEAR_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_LeftBoxSelectClick"    'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "btnClearConfirmOK"        '戻るボタン押下後の確認ダイアログでOK押下
                            WF_CLEAR_ConfirmOkClick()
                        Case "mspItemCodeSingleRowSelected"  '[共通]大項目選択ポップアップで行選択
                            RowSelected_mspItemCodeSingle()
                        Case "mspTodokeCodeSingleRowSelected"  '[共通]届先コード選択ポップアップで行選択
                            RowSelected_mspTodokeCodeSingle()
                    End Select
                End If
            Else
                '○ 初期化処理
                Initialize()
            End If

            '○ 画面モード(更新・参照)設定
            If Master.MAPpermitcode = C_PERMISSION.UPDATE Then
                WF_MAPpermitcode.Value = "TRUE"
            Else
                WF_MAPpermitcode.Value = "FALSE"
            End If

            WF_BOXChange.Value = "detailbox"

        Finally
            '○ 格納Table Close
            If Not IsNothing(LNM0010tbl) Then
                LNM0010tbl.Clear()
                LNM0010tbl.Dispose()
                LNM0010tbl = Nothing
            End If

            If Not IsNothing(LNM0010INPtbl) Then
                LNM0010INPtbl.Clear()
                LNM0010INPtbl.Dispose()
                LNM0010INPtbl = Nothing
            End If

            If Not IsNothing(LNM0010UPDtbl) Then
                LNM0010UPDtbl.Clear()
                LNM0010UPDtbl.Dispose()
                LNM0010UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0010WRKINC.MAPIDD
        '○ HELP表示有無設定
        Master.dispHelp = False
        '○ D&D有無設定
        Master.eventDrop = True

        '○ 初期値設定
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        rightview.ResetIndex()
        leftview.ActiveListBox()

        '○ 右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = Master.USERCAMP
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_Dummy)


        '○ 画面の値設定
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続

            WW_MAPValueSet(SQLcon)
        End Using

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet(ByVal SQLcon As MySqlConnection)
        '対象年月
        WF_TAISHOYM.Value = work.WF_SEL_TAISHOYM.Text

        '一覧件数0の場合(新規)
        If work.WF_SEL_LISTCOUNT.Text = 0 Then
            WF_OPERATION.Value = CONST_INSERT
            WF_RAD_DELFLG.SelectedValue = "0"

            ' 単価・回数を入力するテキストボックスは数値(0～9)のみ可能とする。
            Me.TxtTANKA.Attributes("onkeyPress") = "CheckNum()"
            Me.TxtCOUNT.Attributes("onkeyPress") = "CheckNum()"

            '更新
        Else
            WF_OPERATION.Value = CONST_UPDATE
            '○ 対象データ取得(大項目)
            Dim WW_ITEMTBL As DataTable = GetITEMIDLIST(SQLcon)
            Dim WW_LARGEIDX As Integer = 1
            For Each WW_LARGEROW As DataRow In WW_ITEMTBL.Rows

                Dim WW_RECOIDX As Integer = 1
                Dim WW_RECOTBL As New DataTable
                Select Case WW_LARGEIDX
                    Case 1
#Region "大項目01"
                        '大項目
                        WF_SEL_ITEM_01_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_01_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_01_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_01_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_01_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_01_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_01_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_01_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_01_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_01_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_01_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_01_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_01_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_01_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_01_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_01_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_01_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_01_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_01_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_01_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_01_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_01_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_01_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_01_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_01_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_01_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_01_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_01_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_01_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_01_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_01_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 2
#Region "大項目02"
                        '大項目
                        WF_SEL_ITEM_02_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_02_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_02_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_02_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_02_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_02_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_02_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_02_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_02_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_02_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_02_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_02_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_02_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_02_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_02_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_02_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_02_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_02_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_02_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_02_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_02_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_02_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_02_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_02_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_02_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_02_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_02_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_02_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_02_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_02_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_02_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 3
#Region "大項目03"
                        '大項目
                        WF_SEL_ITEM_03_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_03_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_03_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_03_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_03_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_03_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_03_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_03_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_03_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_03_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_03_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_03_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_03_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_03_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_03_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_03_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_03_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_03_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_03_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_03_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_03_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_03_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_03_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_03_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_03_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_03_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_03_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_03_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_03_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_03_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_03_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 4
#Region "大項目04"
                        '大項目
                        WF_SEL_ITEM_04_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_04_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_04_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_04_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_04_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_04_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_04_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_04_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_04_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_04_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_04_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_04_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_04_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_04_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_04_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_04_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_04_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_04_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_04_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_04_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_04_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_04_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_04_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_04_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_04_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_04_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_04_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_04_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_04_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_04_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_04_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 5
#Region "大項目05"
                        '大項目
                        WF_SEL_ITEM_05_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_05_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_05_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_05_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_05_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_05_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_05_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_05_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_05_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_05_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_05_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_05_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_05_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_05_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_05_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_05_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_05_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_05_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_05_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_05_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_05_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_05_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_05_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_05_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_05_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_05_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_05_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_05_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_05_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_05_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_05_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 6
#Region "大項目06"
                        '大項目
                        WF_SEL_ITEM_06_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_06_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_06_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_06_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_06_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_06_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_06_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_06_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_06_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_06_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_06_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_06_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_06_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_06_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_06_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_06_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_06_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_06_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_06_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_06_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_06_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_06_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_06_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_06_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_06_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_06_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_06_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_06_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_06_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_06_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_06_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 7
#Region "大項目07"
                        '大項目
                        WF_SEL_ITEM_07_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_07_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_07_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_07_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_07_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_07_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_07_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_07_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_07_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_07_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_07_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_07_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_07_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_07_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_07_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_07_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_07_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_07_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_07_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_07_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_07_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_07_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_07_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_07_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_07_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_07_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_07_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_07_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_07_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_07_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_07_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 8
#Region "大項目08"
                        '大項目
                        WF_SEL_ITEM_08_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_08_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_08_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_08_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_08_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_08_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_08_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_08_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_08_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_08_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_08_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_08_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_08_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_08_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_08_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_08_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_08_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_08_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_08_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_08_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_08_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_08_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_08_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_08_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_08_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_08_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_08_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_08_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_08_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_08_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_08_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 9
#Region "大項目09"
                        '大項目
                        WF_SEL_ITEM_09_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_09_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_09_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_09_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_09_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_09_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_09_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_09_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_09_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_09_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_09_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_09_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_09_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_09_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_09_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_09_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_09_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_09_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_09_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_09_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_09_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_09_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_09_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_09_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_09_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_09_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_09_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_09_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_09_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_09_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_09_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 10
#Region "大項目10"
                        '大項目
                        WF_SEL_ITEM_10_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_10_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_10_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_10_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_10_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_10_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_10_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_10_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_10_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_10_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_10_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_10_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_10_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_10_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_10_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_10_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_10_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_10_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_10_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_10_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_10_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_10_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_10_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_10_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_10_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_10_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_10_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_10_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_10_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_10_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_10_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 11
#Region "大項目11"
                        '大項目
                        WF_SEL_ITEM_11_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_11_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_11_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_11_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_11_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_11_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_11_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_11_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_11_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_11_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_11_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_11_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_11_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_11_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_11_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_11_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_11_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_11_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_11_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_11_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_11_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_11_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_11_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_11_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_11_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_11_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_11_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_11_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_11_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_11_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_11_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 12
#Region "大項目12"
                        '大項目
                        WF_SEL_ITEM_12_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_12_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_12_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_12_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_12_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_12_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_12_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_12_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_12_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_12_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_12_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_12_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_12_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_12_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_12_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_12_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_12_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_12_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_12_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_12_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_12_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_12_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_12_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_12_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_12_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_12_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_12_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_12_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_12_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_12_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_12_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 13
#Region "大項目13"
                        '大項目
                        WF_SEL_ITEM_13_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_13_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_13_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_13_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_13_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_13_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_13_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_13_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_13_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_13_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_13_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_13_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_13_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_13_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_13_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_13_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_13_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_13_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_13_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_13_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_13_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_13_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_13_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_13_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_13_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_13_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_13_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_13_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_13_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_13_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_13_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 14
#Region "大項目14"
                        '大項目
                        WF_SEL_ITEM_14_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_14_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_14_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_14_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_14_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_14_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_14_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_14_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_14_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_14_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_14_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_14_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_14_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_14_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_14_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_14_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_14_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_14_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_14_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_14_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_14_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_14_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_14_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_14_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_14_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_14_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_14_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_14_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_14_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_14_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_14_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 15
#Region "大項目15"
                        '大項目
                        WF_SEL_ITEM_15_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_15_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_15_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_15_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_15_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_15_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_15_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_15_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_15_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_15_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_15_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_15_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_15_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_15_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_15_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_15_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_15_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_15_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_15_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_15_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_15_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_15_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_15_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_15_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_15_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_15_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_15_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_15_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_15_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_15_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_15_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 16
#Region "大項目16"
                        '大項目
                        WF_SEL_ITEM_16_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_16_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_16_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_16_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_16_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_16_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_16_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_16_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_16_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_16_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_16_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_16_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_16_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_16_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_16_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_16_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_16_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_16_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_16_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_16_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_16_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_16_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_16_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_16_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_16_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_16_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_16_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_16_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_16_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_16_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_16_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 17
#Region "大項目17"
                        '大項目
                        WF_SEL_ITEM_17_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_17_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_17_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_17_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_17_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_17_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_17_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_17_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_17_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_17_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_17_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_17_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_17_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_17_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_17_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_17_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_17_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_17_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_17_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_17_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_17_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_17_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_17_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_17_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_17_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_17_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_17_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_17_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_17_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_17_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_17_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 18
#Region "大項目18"
                        '大項目
                        WF_SEL_ITEM_18_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_18_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_18_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_18_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_18_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_18_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_18_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_18_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_18_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_18_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_18_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_18_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_18_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_18_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_18_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_18_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_18_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_18_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_18_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_18_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_18_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_18_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_18_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_18_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_18_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_18_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_18_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_18_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_18_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_18_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_18_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 19
#Region "大項目19"
                        '大項目
                        WF_SEL_ITEM_19_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_19_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_19_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_19_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_19_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_19_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_19_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_19_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_19_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_19_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_19_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_19_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_19_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_19_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_19_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_19_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_19_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_19_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_19_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_19_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_19_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_19_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_19_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_19_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_19_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_19_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_19_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_19_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_19_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_19_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_19_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                    Case 20
#Region "大項目20"
                        '大項目
                        WF_SEL_ITEM_20_ID.Text = WW_LARGEROW("ITEMID")
                        WF_SEL_ITEM_20_NAME.Text = WW_LARGEROW("ITEMNAME")
                        'レコード名、削除フラグ
                        WW_RECOTBL = GetRECOLIST(SQLcon, WW_LARGEROW("ITEMID"))

                        For Each WW_RECOROW As DataRow In WW_RECOTBL.Rows
                            Select Case WW_RECOIDX
                                Case 1
                                    WF_SEL_ITEM_20_RECOID_01.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_01.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_01.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_01.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_01.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_01.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_01.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_01.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_01.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_01.SelectedValue = WW_RECOROW("DELFLG")
                                Case 2
                                    WF_SEL_ITEM_20_RECOID_02.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_02.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_02.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_02.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_02.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_02.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_02.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_02.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_02.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_02.SelectedValue = WW_RECOROW("DELFLG")
                                Case 3
                                    WF_SEL_ITEM_20_RECOID_03.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_03.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_03.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_03.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_03.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_03.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_03.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_03.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_03.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_03.SelectedValue = WW_RECOROW("DELFLG")
                                Case 4
                                    WF_SEL_ITEM_20_RECOID_04.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_04.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_04.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_04.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_04.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_04.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_04.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_04.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_04.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_04.SelectedValue = WW_RECOROW("DELFLG")
                                Case 5
                                    WF_SEL_ITEM_20_RECOID_05.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_05.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_05.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_05.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_05.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_05.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_05.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_05.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_05.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_05.SelectedValue = WW_RECOROW("DELFLG")
                                Case 6
                                    WF_SEL_ITEM_20_RECOID_06.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_06.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_06.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_06.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_06.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_06.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_06.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_06.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_06.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_06.SelectedValue = WW_RECOROW("DELFLG")
                                Case 7
                                    WF_SEL_ITEM_20_RECOID_07.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_07.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_07.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_07.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_07.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_07.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_07.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_07.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_07.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_07.SelectedValue = WW_RECOROW("DELFLG")
                                Case 8
                                    WF_SEL_ITEM_20_RECOID_08.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_08.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_08.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_08.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_08.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_08.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_08.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_08.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_08.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_08.SelectedValue = WW_RECOROW("DELFLG")
                                Case 9
                                    WF_SEL_ITEM_20_RECOID_09.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_09.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_09.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_09.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_09.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_09.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_09.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_09.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_09.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_09.SelectedValue = WW_RECOROW("DELFLG")
                                Case 10
                                    WF_SEL_ITEM_20_RECOID_10.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_10.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_10.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_10.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_10.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_10.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_10.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_10.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_10.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_10.SelectedValue = WW_RECOROW("DELFLG")
                                Case 11
                                    WF_SEL_ITEM_20_RECOID_11.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_11.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_11.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_11.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_11.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_11.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_11.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_11.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_11.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_11.SelectedValue = WW_RECOROW("DELFLG")
                                Case 12
                                    WF_SEL_ITEM_20_RECOID_12.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_12.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_12.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_12.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_12.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_12.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_12.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_12.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_12.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_12.SelectedValue = WW_RECOROW("DELFLG")
                                Case 13
                                    WF_SEL_ITEM_20_RECOID_13.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_13.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_13.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_13.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_13.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_13.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_13.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_13.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_13.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_13.SelectedValue = WW_RECOROW("DELFLG")
                                Case 14
                                    WF_SEL_ITEM_20_RECOID_14.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_14.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_14.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_14.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_14.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_14.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_14.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_14.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_14.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_14.SelectedValue = WW_RECOROW("DELFLG")
                                Case 15
                                    WF_SEL_ITEM_20_RECOID_15.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_15.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_15.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_15.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_15.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_15.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_15.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_15.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_15.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_15.SelectedValue = WW_RECOROW("DELFLG")
                                Case 16
                                    WF_SEL_ITEM_20_RECOID_16.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_16.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_16.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_16.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_16.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_16.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_16.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_16.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_16.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_16.SelectedValue = WW_RECOROW("DELFLG")
                                Case 17
                                    WF_SEL_ITEM_20_RECOID_17.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_17.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_17.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_17.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_17.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_17.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_17.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_17.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_17.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_17.SelectedValue = WW_RECOROW("DELFLG")
                                Case 18
                                    WF_SEL_ITEM_20_RECOID_18.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_18.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_18.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_18.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_18.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_18.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_18.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_18.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_18.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_18.SelectedValue = WW_RECOROW("DELFLG")
                                Case 19
                                    WF_SEL_ITEM_20_RECOID_19.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_19.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_19.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_19.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_19.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_19.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_19.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_19.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_19.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_19.SelectedValue = WW_RECOROW("DELFLG")
                                Case 20
                                    WF_SEL_ITEM_20_RECOID_20.Text = WW_RECOROW("RECOID")
                                    WF_SEL_ITEM_20_RECONAME_20.Text = WW_RECOROW("RECONAME")
                                    WF_SEL_ITEM_20_TANKA_20.Text = WW_RECOROW("TANKA")
                                    WF_SEL_ITEM_20_COUNT_20.Text = WW_RECOROW("COUNT")
                                    WF_SEL_ITEM_20_FEE_20.Text = WW_RECOROW("FEE")
                                    WF_SEL_ITEM_20_TODOKECODE_20.Text = WW_RECOROW("TODOKECODE")
                                    WF_SEL_ITEM_20_TODOKENAME_20.Text = WW_RECOROW("TODOKENAME")
                                    WF_SEL_ITEM_20_SYABAN_20.Text = WW_RECOROW("SYABAN")
                                    WF_SEL_ITEM_20_BIKOU_20.Text = WW_RECOROW("BIKOU")
                                    WF_SEL_ITEM_20_DELFLG_20.SelectedValue = WW_RECOROW("DELFLG")
                            End Select

                            WW_RECOIDX = WW_RECOIDX + 1
                        Next
#End Region
                End Select
                WW_LARGEIDX = WW_LARGEIDX + 1
            Next
        End If

#Region "フォーカスアウト時イベント追加"
#Region "大項目01"
        '単価
        WF_SEL_ITEM_01_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_01_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_01_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目02"
        '単価
        WF_SEL_ITEM_02_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_02_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_02_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目03"
        '単価
        WF_SEL_ITEM_03_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_03_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_03_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目04"
        '単価
        WF_SEL_ITEM_04_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_04_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_04_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目05"
        '単価
        WF_SEL_ITEM_05_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_05_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_05_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目06"
        '単価
        WF_SEL_ITEM_06_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_06_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_06_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目07"
        '単価
        WF_SEL_ITEM_07_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_07_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_07_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目08"
        '単価
        WF_SEL_ITEM_08_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_08_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_08_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目09"
        '単価
        WF_SEL_ITEM_09_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_09_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_09_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目10"
        '単価
        WF_SEL_ITEM_10_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_10_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_10_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目11"
        '単価
        WF_SEL_ITEM_11_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_11_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_11_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目12"
        '単価
        WF_SEL_ITEM_12_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_12_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_12_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目13"
        '単価
        WF_SEL_ITEM_13_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_13_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_13_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目14"
        '単価
        WF_SEL_ITEM_14_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_14_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_14_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目15"
        '単価
        WF_SEL_ITEM_15_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_15_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_15_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目16"
        '単価
        WF_SEL_ITEM_16_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_16_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_16_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目17"
        '単価
        WF_SEL_ITEM_17_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_17_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_17_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目18"
        '単価
        WF_SEL_ITEM_18_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_18_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_18_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目19"
        '単価
        WF_SEL_ITEM_19_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_19_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_19_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#Region "大項目20"
        '単価
        WF_SEL_ITEM_20_TANKA_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_TANKA_20.Attributes("onblur") = "txtOnblur()"
        '回数
        WF_SEL_ITEM_20_COUNT_01.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_02.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_03.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_04.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_05.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_06.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_07.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_08.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_09.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_10.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_11.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_12.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_13.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_14.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_15.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_16.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_17.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_18.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_19.Attributes("onblur") = "txtOnblur()"
        WF_SEL_ITEM_20_COUNT_20.Attributes("onblur") = "txtOnblur()"
#End Region
#End Region
#Region "onkeyPressイベント追加"
#Region "大項目01"
        '単価
        WF_SEL_ITEM_01_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_01_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_01_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目02"
        '単価
        WF_SEL_ITEM_02_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_02_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_02_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目03"
        '単価
        WF_SEL_ITEM_03_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_03_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_03_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目04"
        '単価
        WF_SEL_ITEM_04_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_04_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_04_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目05"
        '単価
        WF_SEL_ITEM_05_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_05_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_05_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目06"
        '単価
        WF_SEL_ITEM_06_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_06_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_06_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目07"
        '単価
        WF_SEL_ITEM_07_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_07_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_07_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目08"
        '単価
        WF_SEL_ITEM_08_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_08_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_08_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目09"
        '単価
        WF_SEL_ITEM_09_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_09_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_09_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目10"
        '単価
        WF_SEL_ITEM_10_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_10_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_10_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目11"
        '単価
        WF_SEL_ITEM_11_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_11_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_11_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目12"
        '単価
        WF_SEL_ITEM_12_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_12_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_12_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目13"
        '単価
        WF_SEL_ITEM_13_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_13_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_13_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目14"
        '単価
        WF_SEL_ITEM_14_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_14_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_14_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目15"
        '単価
        WF_SEL_ITEM_15_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_15_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_15_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目16"
        '単価
        WF_SEL_ITEM_16_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_16_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_16_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目17"
        '単価
        WF_SEL_ITEM_17_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_17_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_17_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目18"
        '単価
        WF_SEL_ITEM_18_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_18_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_18_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目19"
        '単価
        WF_SEL_ITEM_19_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_19_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_19_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#Region "大項目20"
        '単価
        WF_SEL_ITEM_20_TANKA_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_TANKA_20.Attributes("onkeyPress") = "CheckNum()"
        '回数
        WF_SEL_ITEM_20_COUNT_01.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_02.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_03.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_04.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_05.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_06.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_07.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_08.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_09.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_10.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_11.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_12.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_13.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_14.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_15.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_16.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_17.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_18.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_19.Attributes("onkeyPress") = "CheckNum()"
        WF_SEL_ITEM_20_COUNT_20.Attributes("onkeyPress") = "CheckNum()"
#End Region
#End Region

        ClientScript.RegisterStartupScript(Me.GetType(), "key", "txtOnblur();", True)

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU
    End Sub

    ''' <summary>
    ''' 大項目一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function GetITEMIDLIST(ByVal SQLcon As MySqlConnection) As DataTable
        Dim WW_Tbl = New DataTable

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT")
        SQLStr.AppendLine("       ITEMID AS ITEMID")
        SQLStr.AppendLine("      ,RTRIM(ITEMNAME) AS ITEMNAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0013_KGSPRATE")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("       DELFLG <> '1'                 ")
        SQLStr.AppendLine("   AND TORICODE  = @TORICODE                 ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND TAISHOYM  = @TAISHOYM             ")
        SQLStr.AppendLine(" ORDER BY                          ")
        SQLStr.AppendLine("   ITEMID                          ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6) '対象年月

                P_TORICODE.Value = work.WF_SEL_TORICODE.Text '取引先コード
                P_ORGCODE.Value = LNM0010WRKINC.ORGISHIKARI '部門コード
                P_TAISHOYM.Value = Replace(work.WF_SEL_TAISHOYM.Text, "/", "") '対象年月

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
        End Try
        Return WW_Tbl
    End Function

    ''' <summary>
    ''' レコード一覧取得
    ''' </summary>
    ''' <remarks></remarks>
    Protected Function GetRECOLIST(ByVal SQLcon As MySqlConnection, ByVal WW_ITEMID As String) As DataTable
        Dim WW_Tbl = New DataTable

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT")
        SQLStr.AppendLine("       RECOID AS RECOID")
        SQLStr.AppendLine("      ,RTRIM(RECONAME) AS RECONAME")
        SQLStr.AppendLine("      ,COALESCE(RTRIM(TANKA), '0') AS TANKA ")
        SQLStr.AppendLine("      ,COALESCE(RTRIM(COUNT), '0') AS COUNT ")
        SQLStr.AppendLine("      ,COALESCE(RTRIM(FEE), '0') AS FEE ")
        SQLStr.AppendLine("      ,TODOKECODE AS TODOKECODE")
        SQLStr.AppendLine("      ,RTRIM(TODOKENAME) AS TODOKENAME")
        SQLStr.AppendLine("      ,RTRIM(SYABAN) AS SYABAN")
        SQLStr.AppendLine("      ,RTRIM(BIKOU) AS BIKOU")
        SQLStr.AppendLine("      ,DELFLG")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0013_KGSPRATE")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("       DELFLG <> '1'                 ")
        SQLStr.AppendLine("   AND TORICODE  = @TORICODE                 ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND TAISHOYM  = @TAISHOYM             ")
        SQLStr.AppendLine("   AND ITEMID  = @ITEMID             ")
        SQLStr.AppendLine(" ORDER BY                          ")
        SQLStr.AppendLine("   RECOID                          ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6) '対象年月
                Dim P_ITEMID As MySqlParameter = SQLcmd.Parameters.Add("@ITEMID", MySqlDbType.VarChar, 2) '大項目

                P_TORICODE.Value = work.WF_SEL_TORICODE.Text '取引先コード
                P_ORGCODE.Value = LNM0010WRKINC.ORGISHIKARI '部門コード
                P_TAISHOYM.Value = Replace(work.WF_SEL_TAISHOYM.Text, "/", "") '対象年月
                P_ITEMID.Value = WW_ITEMID '大項目

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)
                End Using
            End Using
        Catch ex As Exception
        End Try
        Return WW_Tbl
    End Function

    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()
        '初期化
        LNM0010INPtbl = New DataTable
        LNM0010INPtbl.Columns.Add("TORICODE")
        LNM0010INPtbl.Columns.Add("TORINAME")
        LNM0010INPtbl.Columns.Add("ORGCODE")
        LNM0010INPtbl.Columns.Add("ORGNAME")
        LNM0010INPtbl.Columns.Add("KASANORGCODE")
        LNM0010INPtbl.Columns.Add("KASANORGNAME")
        LNM0010INPtbl.Columns.Add("TODOKECODE")
        LNM0010INPtbl.Columns.Add("TODOKENAME")
        LNM0010INPtbl.Columns.Add("SYABAN")
        LNM0010INPtbl.Columns.Add("TAISHOYM")
        LNM0010INPtbl.Columns.Add("ITEMID")
        LNM0010INPtbl.Columns.Add("ITEMNAME")
        LNM0010INPtbl.Columns.Add("RECOID")
        LNM0010INPtbl.Columns.Add("RECONAME")
        LNM0010INPtbl.Columns.Add("TANKA")
        LNM0010INPtbl.Columns.Add("COUNT")
        LNM0010INPtbl.Columns.Add("FEE")
        LNM0010INPtbl.Columns.Add("BIKOU")
        LNM0010INPtbl.Columns.Add("DELFLG")

        Dim Row As DataRow

        '新規
        If WF_OPERATION.Value = CONST_INSERT Then
            Row = LNM0010INPtbl.NewRow

            Row("TORICODE") = work.WF_SEL_TORICODE.Text
            Row("TORINAME") = work.WF_SEL_TORINAME.Text
            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
            Row("TODOKECODE") = TxtTODOKECODE.Text
            Row("TODOKENAME") = WF_TODOKENAME.Text
            Row("SYABAN") = TxtSYABAN.Text
            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")

            If Not TxtITEMID.Text = "" Then
                Row("ITEMID") = TxtITEMID.Text
            Else
                Row("ITEMID") = GenerateITEMID()
            End If

            Row("ITEMNAME") = TxtITEMNAME.Text
            Row("RECOID") = LNM0010WRKINC.GenerateRECOID(LNM0010WRKINC.TBLKGSPRATE)
            Row("RECONAME") = TxtRECONAME.Text

            If TxtTANKA.Text = "" Then
                Row("TANKA") = 0
            Else
                Row("TANKA") = TxtTANKA.Text
            End If
            If TxtCOUNT.Text = "" Then
                Row("COUNT") = 0
            Else
                Row("COUNT") = TxtCOUNT.Text
            End If
            Row("FEE") = CInt(Row("TANKA")) * CInt(Row("COUNT"))
            Row("BIKOU") = TxtBIKOU.Text
            Row("DELFLG") = WF_RAD_DELFLG.SelectedValue

            '○ エラーレポート準備
            rightview.SetErrorReport("")

            '○ 項目チェック
            INPTableCheck(Row, WW_ErrSW)
            If Not isNormal(WW_ErrSW) Then
                Exit Sub
            End If

            LNM0010INPtbl.Rows.Add(Row)

            '更新
        Else
            Dim WW_LARGEIDX As Integer
            Dim WW_RECOIDX As Integer

            Dim WW_ITEMNAME As String = ""
            Dim WW_WW_RECONAME As String = ""

            For WW_LARGEIDX = 1 To CONST_MAX_ITEM_CNT
                '大項目名称取得
                Select Case WW_LARGEIDX
                    Case 1 : WW_ITEMNAME = WF_SEL_ITEM_01_NAME.Text
                    Case 2 : WW_ITEMNAME = WF_SEL_ITEM_02_NAME.Text
                    Case 3 : WW_ITEMNAME = WF_SEL_ITEM_03_NAME.Text
                    Case 4 : WW_ITEMNAME = WF_SEL_ITEM_04_NAME.Text
                    Case 5 : WW_ITEMNAME = WF_SEL_ITEM_05_NAME.Text
                    Case 6 : WW_ITEMNAME = WF_SEL_ITEM_06_NAME.Text
                    Case 7 : WW_ITEMNAME = WF_SEL_ITEM_07_NAME.Text
                    Case 8 : WW_ITEMNAME = WF_SEL_ITEM_08_NAME.Text
                    Case 9 : WW_ITEMNAME = WF_SEL_ITEM_09_NAME.Text
                    Case 10 : WW_ITEMNAME = WF_SEL_ITEM_10_NAME.Text
                    Case 11 : WW_ITEMNAME = WF_SEL_ITEM_11_NAME.Text
                    Case 12 : WW_ITEMNAME = WF_SEL_ITEM_12_NAME.Text
                    Case 13 : WW_ITEMNAME = WF_SEL_ITEM_13_NAME.Text
                    Case 14 : WW_ITEMNAME = WF_SEL_ITEM_14_NAME.Text
                    Case 15 : WW_ITEMNAME = WF_SEL_ITEM_15_NAME.Text
                    Case 16 : WW_ITEMNAME = WF_SEL_ITEM_16_NAME.Text
                    Case 17 : WW_ITEMNAME = WF_SEL_ITEM_17_NAME.Text
                    Case 18 : WW_ITEMNAME = WF_SEL_ITEM_18_NAME.Text
                    Case 19 : WW_ITEMNAME = WF_SEL_ITEM_19_NAME.Text
                    Case 20 : WW_ITEMNAME = WF_SEL_ITEM_20_NAME.Text
                End Select

                '大項目名称が未入力の場合処理を終了
                If WW_ITEMNAME = "" Then
                    Exit For
                End If

                Select Case WW_LARGEIDX
                    Case 1
#Region "大項目01"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_01_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_01_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_01_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_01_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_01_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_01_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_01_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_01_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_01_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_01_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_01_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_01_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_01_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 2
#Region "大項目02"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_02_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_02_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_02_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_02_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_02_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_02_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_02_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_02_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_02_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_02_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_02_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_02_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_02_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 3
#Region "大項目03"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_03_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_03_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_03_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_03_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_03_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_03_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_03_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_03_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_03_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_03_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_03_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_03_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_03_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 4
#Region "大項目04"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_04_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_04_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_04_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_04_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_04_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_04_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_04_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_04_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_04_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_04_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_04_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_04_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_04_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 5
#Region "大項目05"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_05_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_05_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_05_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_05_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_05_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_05_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_05_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_05_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_05_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_05_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_05_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_05_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_05_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 6
#Region "大項目06"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_06_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_06_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_06_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_06_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_06_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_06_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_06_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_06_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_06_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_06_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_06_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_06_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_06_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 7
#Region "大項目07"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_07_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_07_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_07_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_07_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_07_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_07_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_07_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_07_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_07_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_07_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_07_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_07_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_07_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 8
#Region "大項目08"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_08_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_08_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_08_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_08_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_08_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_08_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_08_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_08_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_08_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_08_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_08_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_08_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_08_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 9
#Region "大項目09"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_09_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_09_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_09_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_09_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_09_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_09_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_09_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_09_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_09_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_09_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_09_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_09_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_09_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 10
#Region "大項目10"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_10_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_10_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_10_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_10_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_10_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_10_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_10_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_10_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_10_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_10_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_10_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_10_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_10_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 11
#Region "大項目11"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_11_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_11_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_11_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_11_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_11_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_11_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_11_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_11_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_11_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_11_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_11_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_11_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_11_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 12
#Region "大項目12"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_12_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_12_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_12_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_12_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_12_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_12_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_12_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_12_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_12_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_12_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_12_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_12_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_12_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 13
#Region "大項目13"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_13_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_13_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_13_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_13_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_13_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_13_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_13_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_13_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_13_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_13_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_13_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_13_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_13_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 14
#Region "大項目14"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_14_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_14_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_14_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_14_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_14_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_14_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_14_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_14_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_14_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_14_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_14_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_14_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_14_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 15
#Region "大項目15"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_15_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_15_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_15_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_15_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_15_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_15_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_15_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_15_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_15_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_15_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_15_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_15_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_15_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 16
#Region "大項目16"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_16_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_16_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_16_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_16_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_16_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_16_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_16_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_16_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_16_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_16_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_16_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_16_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_16_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 17
#Region "大項目17"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_17_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_17_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_17_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_17_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_17_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_17_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_17_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_17_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_17_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_17_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_17_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_17_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_17_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 18
#Region "大項目18"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_18_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_18_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_18_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_18_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_18_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_18_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_18_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_18_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_18_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_18_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_18_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_18_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_18_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 19
#Region "大項目19"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_19_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_19_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_19_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_19_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_19_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_19_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_19_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_19_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_19_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_19_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_19_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_19_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_19_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                    Case 20
#Region "大項目20"
                        For WW_RECOIDX = 1 To CONST_MAX_RECO_CNT

                            Row = LNM0010INPtbl.NewRow

                            Row("TORICODE") = work.WF_SEL_TORICODE.Text
                            Row("TORINAME") = work.WF_SEL_TORINAME.Text
                            Row("ORGCODE") = LNM0010WRKINC.ORGISHIKARI
                            Row("ORGNAME") = LNM0010WRKINC.ORGISHIKARINAME
                            Row("KASANORGCODE") = LNM0010WRKINC.KASANORGHOKKAIDO
                            Row("KASANORGNAME") = LNM0010WRKINC.KASANORGHOKKAIDONAME
                            Row("TAISHOYM") = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                            Row("ITEMID") = WF_SEL_ITEM_20_ID.Text
                            Row("ITEMNAME") = WF_SEL_ITEM_20_NAME.Text

                            'レコード名称取得
                            Select Case WW_RECOIDX
                                Case 1 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_01.Text
                                Case 2 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_02.Text
                                Case 3 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_03.Text
                                Case 4 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_04.Text
                                Case 5 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_05.Text
                                Case 6 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_06.Text
                                Case 7 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_07.Text
                                Case 8 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_08.Text
                                Case 9 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_09.Text
                                Case 10 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_10.Text
                                Case 11 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_11.Text
                                Case 12 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_12.Text
                                Case 13 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_13.Text
                                Case 14 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_14.Text
                                Case 15 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_15.Text
                                Case 16 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_16.Text
                                Case 17 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_17.Text
                                Case 18 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_18.Text
                                Case 19 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_19.Text
                                Case 20 : WW_WW_RECONAME = WF_SEL_ITEM_20_RECONAME_20.Text
                            End Select

                            '大項目名称が未入力の場合処理を終了
                            If WW_WW_RECONAME = "" Then
                                Exit For
                            End If

                            'データセット
                            Select Case WW_RECOIDX
                                Case 1
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_01.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_01.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_01.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_01.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_01.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_01.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_01.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_01.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_01.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_01.SelectedValue
                                Case 2
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_02.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_02.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_02.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_02.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_02.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_02.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_02.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_02.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_02.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_02.SelectedValue
                                Case 3
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_03.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_03.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_03.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_03.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_03.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_03.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_03.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_03.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_03.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_03.SelectedValue
                                Case 4
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_04.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_04.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_04.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_04.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_04.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_04.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_04.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_04.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_04.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_04.SelectedValue
                                Case 5
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_05.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_05.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_05.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_05.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_05.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_05.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_05.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_05.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_05.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_05.SelectedValue
                                Case 6
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_06.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_06.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_06.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_06.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_06.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_06.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_06.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_06.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_06.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_06.SelectedValue
                                Case 7
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_07.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_07.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_07.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_07.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_07.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_07.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_07.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_07.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_07.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_07.SelectedValue
                                Case 8
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_08.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_08.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_08.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_08.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_08.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_08.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_08.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_08.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_08.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_08.SelectedValue
                                Case 9
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_09.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_09.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_09.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_09.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_09.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_09.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_09.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_09.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_09.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_09.SelectedValue
                                Case 10
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_10.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_10.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_10.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_10.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_10.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_10.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_10.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_10.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_10.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_10.SelectedValue
                                Case 11
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_11.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_11.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_11.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_11.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_11.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_11.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_11.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_11.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_11.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_11.SelectedValue
                                Case 12
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_12.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_12.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_12.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_12.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_12.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_12.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_12.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_12.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_12.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_12.SelectedValue
                                Case 13
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_13.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_13.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_13.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_13.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_13.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_13.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_13.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_13.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_13.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_13.SelectedValue
                                Case 14
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_14.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_14.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_14.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_14.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_14.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_14.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_14.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_14.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_14.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_14.SelectedValue
                                Case 15
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_15.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_15.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_15.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_15.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_15.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_15.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_15.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_15.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_15.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_15.SelectedValue
                                Case 16
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_16.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_16.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_16.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_16.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_16.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_16.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_16.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_16.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_16.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_16.SelectedValue
                                Case 17
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_17.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_17.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_17.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_17.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_17.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_17.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_17.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_17.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_17.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_17.SelectedValue
                                Case 18
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_18.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_18.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_18.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_18.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_18.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_18.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_18.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_18.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_18.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_18.SelectedValue
                                Case 19
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_19.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_19.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_19.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_19.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_19.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_19.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_19.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_19.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_19.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_19.SelectedValue
                                Case 20
                                    Row("TODOKECODE") = WF_SEL_ITEM_20_TODOKECODE_20.Text
                                    Row("TODOKENAME") = WF_SEL_ITEM_20_TODOKENAME_20.Text
                                    Row("SYABAN") = WF_SEL_ITEM_20_SYABAN_20.Text
                                    Row("RECOID") = WF_SEL_ITEM_20_RECOID_20.Text
                                    Row("RECONAME") = WF_SEL_ITEM_20_RECONAME_20.Text
                                    Row("TANKA") = WF_SEL_ITEM_20_TANKA_20.Text
                                    Row("COUNT") = WF_SEL_ITEM_20_COUNT_20.Text
                                    Row("FEE") = WF_SEL_ITEM_20_FEE_20.Text
                                    Row("BIKOU") = WF_SEL_ITEM_20_BIKOU_20.Text
                                    Row("DELFLG") = WF_SEL_ITEM_20_DELFLG_20.SelectedValue
                            End Select
                            LNM0010INPtbl.Rows.Add(Row)
                        Next
#End Region
                End Select
            Next
        End If

        Dim DATENOW As DateTime = Date.Now
        Dim WW_ErrData As Boolean = False
        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            For Each UpdRow As DataRow In LNM0010INPtbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, UpdRow) = False Then

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    MASTEREXISTS(SQLcon, UpdRow, WW_BEFDELFLG, WW_MODIFYKBN, WW_ErrSW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, UpdRow, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                        If Not isNormal(WW_ErrSW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '登録、更新する
                    InsUpdData(SQLcon, UpdRow, DATENOW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If

                    '履歴登録(新規・変更後)
                    InsertHist(SQLcon, UpdRow, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If
                End If
            Next
        End Using

        If isNormal(WW_ErrSW) Then
            work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = "Update Success!!"
            ' 前ページ遷移
            Master.TransitionPrevPage()
        End If
    End Sub

    '' <summary>
    '' 大項目採番
    '' </summary>
    Protected Function GenerateITEMID() As String
        GenerateITEMID = ""

        Dim CS0050Session As New CS0050SESSION

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("       MAX(ITEMID) AS ITEMID")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0013_KGSPRATE")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        'SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@TORICODE", MySqlDbType.VarChar).Value = work.WF_SEL_TORICODE.Text
                    .Add("@ORGCODE", MySqlDbType.VarChar).Value = LNM0010WRKINC.ORGISHIKARI
                    '.Add("@TAISHOYM", MySqlDbType.VarChar).Value = Replace(work.WF_SEL_TAISHOYM.Text, "/", "")
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        Return ""
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If WW_Tbl.Rows.Count >= 1 Then
                        Return (CInt(WW_Tbl.Rows(0)("ITEMID")) + 1).ToString
                    Else
                        Return "1"
                    End If
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try
    End Function

    '' <summary>
    '' 今回アップロードしたデータと完全一致するデータがあるか確認する
    '' </summary>
    Protected Function SameDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        SameDataChk = False

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0013_KGSPRATE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(TORINAME, '')             = @TORINAME ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGNAME, '')             = @ORGNAME ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGNAME, '')             = @KASANORGNAME ")
        SQLStr.AppendLine("    AND  COALESCE(TODOKECODE, '')             = @TODOKECODE ")
        SQLStr.AppendLine("    AND  COALESCE(TODOKENAME, '')             = @TODOKENAME ")
        SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
        SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '')             = @TAISHOYM ")
        SQLStr.AppendLine("    AND  COALESCE(ITEMID, '')             = @ITEMID ")
        SQLStr.AppendLine("    AND  COALESCE(ITEMNAME, '')             = @ITEMNAME ")
        SQLStr.AppendLine("    AND  COALESCE(RECOID, '')             = @RECOID ")
        SQLStr.AppendLine("    AND  COALESCE(RECONAME, '')             = @RECONAME ")
        SQLStr.AppendLine("    AND  COALESCE(TANKA, '0')             = @TANKA ")
        SQLStr.AppendLine("    AND  COALESCE(COUNT, '0')             = @COUNT ")
        SQLStr.AppendLine("    AND  COALESCE(FEE, '0')             = @FEE ")
        SQLStr.AppendLine("    AND  COALESCE(BIKOU, '')             = @BIKOU ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.Decimal, 6)     '対象年月
                Dim P_ITEMID As MySqlParameter = SQLcmd.Parameters.Add("@ITEMID", MySqlDbType.VarChar, 2)     '大項目
                Dim P_ITEMNAME As MySqlParameter = SQLcmd.Parameters.Add("@ITEMNAME", MySqlDbType.VarChar, 100)     '項目名
                Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 5)     'レコードID
                Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal, 8)     '単価
                Dim P_COUNT As MySqlParameter = SQLcmd.Parameters.Add("@COUNT", MySqlDbType.Decimal, 3)     '回数
                Dim P_FEE As MySqlParameter = SQLcmd.Parameters.Add("@FEE", MySqlDbType.Decimal, 8)     '料金
                Dim P_BIKOU As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU", MySqlDbType.VarChar, 100)     '備考

                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '届先コード
                P_TODOKENAME.Value = WW_ROW("TODOKENAME")           '届先名称
                P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                P_ITEMID.Value = WW_ROW("ITEMID")           '大項目
                P_ITEMNAME.Value = WW_ROW("ITEMNAME")           '項目名
                P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                P_RECONAME.Value = WW_ROW("RECONAME")           'レコード名
                P_TANKA.Value = WW_ROW("TANKA")           '単価
                P_COUNT.Value = WW_ROW("COUNT")           '回数
                P_FEE.Value = WW_ROW("FEE")           '料金
                P_BIKOU.Value = WW_ROW("BIKOU")           '備考

                P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ


                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    Dim WW_Tbl = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count >= 1 Then
                        Exit Function
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013_KGSPRATE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0013_KGSPRATE SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Function
        End Try
        SameDataChk = True
    End Function

    ''' <summary>
    ''' 変更チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MASTEREXISTS(ByVal SQLcon As MySqlConnection,
                               ByVal WW_ROW As DataRow,
                               ByRef WW_BEFDELFLG As String,
                               ByRef WW_MODIFYKBN As String,
                               ByRef O_RTN As String)

        O_RTN = Messages.C_MESSAGE_NO.NORMAL

        '北海道ガス特別料金マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0013_KGSPRATE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
        SQLStr.AppendLine("    AND  COALESCE(RECOID, '')             = @RECOID ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID

                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                P_RECOID.Value = WW_ROW("RECOID")           'レコードID

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    '更新の場合(データが存在した場合)は変更区分に変更前をセット
                    If WW_Tbl.Rows.Count > 0 Then
                        WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013_KGSPRATE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0013_KGSPRATE SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 履歴テーブル登録
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsertHist(ByVal SQLcon As MySqlConnection,
                             ByVal WW_ROW As DataRow,
                             ByVal WW_BEFDELFLG As String,
                             ByVal WW_MODIFYKBN As String,
                             ByVal WW_NOW As Date,
                             ByRef O_RTN As String)

        O_RTN = Messages.C_MESSAGE_NO.NORMAL

        '○ ＤＢ更新
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0012_KGSPRATEHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKENAME  ")
        SQLStr.AppendLine("     ,SYABAN  ")
        SQLStr.AppendLine("     ,TAISHOYM  ")
        SQLStr.AppendLine("     ,ITEMID  ")
        SQLStr.AppendLine("     ,ITEMNAME  ")
        SQLStr.AppendLine("     ,RECOID  ")
        SQLStr.AppendLine("     ,RECONAME  ")
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,COUNT  ")
        SQLStr.AppendLine("     ,FEE  ")
        SQLStr.AppendLine("     ,BIKOU  ")
        SQLStr.AppendLine("     ,OPERATEKBN  ")
        SQLStr.AppendLine("     ,MODIFYKBN  ")
        SQLStr.AppendLine("     ,MODIFYYMD  ")
        SQLStr.AppendLine("     ,MODIFYUSER  ")
        SQLStr.AppendLine("     ,DELFLG  ")
        SQLStr.AppendLine("     ,INITYMD  ")
        SQLStr.AppendLine("     ,INITUSER  ")
        SQLStr.AppendLine("     ,INITTERMID  ")
        SQLStr.AppendLine("     ,INITPGID  ")
        SQLStr.AppendLine("  )  ")
        SQLStr.AppendLine("  SELECT  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKENAME  ")
        SQLStr.AppendLine("     ,SYABAN  ")
        SQLStr.AppendLine("     ,TAISHOYM  ")
        SQLStr.AppendLine("     ,ITEMID  ")
        SQLStr.AppendLine("     ,ITEMNAME  ")
        SQLStr.AppendLine("     ,RECOID  ")
        SQLStr.AppendLine("     ,RECONAME  ")
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,COUNT  ")
        SQLStr.AppendLine("     ,FEE  ")
        SQLStr.AppendLine("     ,BIKOU  ")
        SQLStr.AppendLine("     ,@OPERATEKBN AS OPERATEKBN ")
        SQLStr.AppendLine("     ,@MODIFYKBN AS MODIFYKBN ")
        SQLStr.AppendLine("     ,@MODIFYYMD AS MODIFYYMD ")
        SQLStr.AppendLine("     ,@MODIFYUSER AS MODIFYUSER ")
        SQLStr.AppendLine("     ,DELFLG ")
        SQLStr.AppendLine("     ,@INITYMD AS INITYMD ")
        SQLStr.AppendLine("     ,@INITUSER AS INITUSER ")
        SQLStr.AppendLine("     ,@INITTERMID AS INITTERMID ")
        SQLStr.AppendLine("     ,@INITPGID AS INITPGID ")
        SQLStr.AppendLine("  FROM   ")
        SQLStr.AppendLine("        LNG.LNM0013_KGSPRATE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '0') = @TAISHOYM ")
        SQLStr.AppendLine("    AND  COALESCE(RECOID, '')             = @RECOID ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                ' DB更新
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                P_RECOID.Value = WW_ROW("RECOID")           'レコードID

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0006WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0006WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0006WRKINC.OPERATEKBN.UPDDATA).ToString
                    End If
                End If

                P_MODIFYKBN.Value = WW_MODIFYKBN             '変更区分
                P_MODIFYYMD.Value = WW_NOW               '変更日時
                P_MODIFYUSER.Value = Master.USERID               '変更ユーザーＩＤ

                P_INITYMD.Value = WW_NOW              '登録年月日
                P_INITUSER.Value = Master.USERID             '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID                '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name          '登録プログラムＩＤ

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0012_KGSPRATEHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0012_KGSPRATEHIST  INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' データ登録・更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsUpdData(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByVal WW_DATENOW As DateTime)
        WW_ErrSW = C_MESSAGE_NO.NORMAL

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("  INSERT INTO LNG.LNM0013_KGSPRATE")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKENAME  ")
        SQLStr.AppendLine("     ,SYABAN  ")
        SQLStr.AppendLine("     ,TAISHOYM  ")
        SQLStr.AppendLine("     ,ITEMID  ")
        SQLStr.AppendLine("     ,ITEMNAME  ")
        SQLStr.AppendLine("     ,RECOID  ")
        SQLStr.AppendLine("     ,RECONAME  ")
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,COUNT  ")
        SQLStr.AppendLine("     ,FEE  ")
        SQLStr.AppendLine("     ,BIKOU  ")
        SQLStr.AppendLine("     ,DELFLG  ")
        SQLStr.AppendLine("     ,INITYMD  ")
        SQLStr.AppendLine("     ,INITUSER  ")
        SQLStr.AppendLine("     ,INITTERMID  ")
        SQLStr.AppendLine("     ,INITPGID  ")
        SQLStr.AppendLine("   )  ")
        SQLStr.AppendLine("   VALUES  ")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      @TORICODE  ")
        SQLStr.AppendLine("     ,@TORINAME  ")
        SQLStr.AppendLine("     ,@ORGCODE  ")
        SQLStr.AppendLine("     ,@ORGNAME  ")
        SQLStr.AppendLine("     ,@KASANORGCODE  ")
        SQLStr.AppendLine("     ,@KASANORGNAME  ")
        SQLStr.AppendLine("     ,@TODOKECODE  ")
        SQLStr.AppendLine("     ,@TODOKENAME  ")
        SQLStr.AppendLine("     ,@SYABAN  ")
        SQLStr.AppendLine("     ,@TAISHOYM  ")
        SQLStr.AppendLine("     ,@ITEMID  ")
        SQLStr.AppendLine("     ,@ITEMNAME  ")
        SQLStr.AppendLine("     ,@RECOID  ")
        SQLStr.AppendLine("     ,@RECONAME  ")
        SQLStr.AppendLine("     ,@TANKA  ")
        SQLStr.AppendLine("     ,@COUNT  ")
        SQLStr.AppendLine("     ,@FEE  ")
        SQLStr.AppendLine("     ,@BIKOU  ")
        SQLStr.AppendLine("     ,@DELFLG  ")
        SQLStr.AppendLine("     ,@INITYMD  ")
        SQLStr.AppendLine("     ,@INITUSER  ")
        SQLStr.AppendLine("     ,@INITTERMID  ")
        SQLStr.AppendLine("     ,@INITPGID  ")
        SQLStr.AppendLine("   )   ")
        SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
        SQLStr.AppendLine("      TORINAME =  @TORINAME")
        SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
        SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
        SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
        SQLStr.AppendLine("     ,TODOKECODE =  @TODOKECODE")
        SQLStr.AppendLine("     ,TODOKENAME =  @TODOKENAME")
        SQLStr.AppendLine("     ,SYABAN =  @SYABAN")
        SQLStr.AppendLine("     ,ITEMID =  @ITEMID")
        SQLStr.AppendLine("     ,ITEMNAME =  @ITEMNAME")
        SQLStr.AppendLine("     ,RECONAME =  @RECONAME")
        SQLStr.AppendLine("     ,TANKA =  @TANKA")
        SQLStr.AppendLine("     ,COUNT =  @COUNT")
        SQLStr.AppendLine("     ,FEE =  @FEE")
        SQLStr.AppendLine("     ,BIKOU =  @BIKOU")
        SQLStr.AppendLine("     ,DELFLG =  @DELFLG ")
        SQLStr.AppendLine("     ,UPDYMD =  @UPDYMD ")
        SQLStr.AppendLine("     ,UPDUSER =  @UPDUSER ")
        SQLStr.AppendLine("     ,UPDTERMID =  @UPDTERMID ")
        SQLStr.AppendLine("     ,UPDPGID =  @UPDPGID ")
        SQLStr.AppendLine("    ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.Decimal, 6)     '対象年月
                Dim P_ITEMID As MySqlParameter = SQLcmd.Parameters.Add("@ITEMID", MySqlDbType.VarChar, 2)     '大項目
                Dim P_ITEMNAME As MySqlParameter = SQLcmd.Parameters.Add("@ITEMNAME", MySqlDbType.VarChar, 100)     '項目名
                Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 5)     'レコードID
                Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal, 8)     '単価
                Dim P_COUNT As MySqlParameter = SQLcmd.Parameters.Add("@COUNT", MySqlDbType.Decimal, 3)     '回数
                Dim P_FEE As MySqlParameter = SQLcmd.Parameters.Add("@FEE", MySqlDbType.Decimal, 8)     '料金
                Dim P_BIKOU As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU", MySqlDbType.VarChar, 100)     '備考
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                'DB更新
                P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ
                P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名称
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名称
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名称
                P_TODOKECODE.Value = WW_ROW("TODOKECODE")           '届先コード
                P_TODOKENAME.Value = WW_ROW("TODOKENAME")           '届先名称
                P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                P_TAISHOYM.Value = WW_ROW("TAISHOYM")           '対象年月
                P_ITEMID.Value = WW_ROW("ITEMID")           '大項目
                P_ITEMNAME.Value = WW_ROW("ITEMNAME")           '項目名
                P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                P_RECONAME.Value = WW_ROW("RECONAME")           'レコード名
                P_TANKA.Value = WW_ROW("TANKA")           '単価
                P_COUNT.Value = WW_ROW("COUNT")           '回数
                P_FEE.Value = WW_ROW("FEE")           '料金
                P_BIKOU.Value = WW_ROW("BIKOU")           '備考

                P_INITYMD.Value = WW_DATENOW                        '登録年月日
                P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID              '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                P_UPDYMD.Value = WW_DATENOW                         '更新年月日
                P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013_KGSPRATE  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0013_KGSPRATE  INSERTUPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToLNM0010INPtbl(ByRef O_RTN As String)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        Master.TransitionPrevPage()
    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時、確認ダイアログOKボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_ConfirmOkClick()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()
        Dim WW_PrmData As New Hashtable

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                ' フィールドによってパラメータを変える
                Select Case WF_FIELD.Value
                    Case "TxtITEMID"       '大項目
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspItemCodeSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    Case "TxtTODOKECODE"       '届先コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspTodokeCodeSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                End Select
                .SetListBox(WF_LeftMViewChange.Value, WW_Dummy, WW_PrmData)
                .ActiveListBox()
            End With
        End If
    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()
        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            Case "TxtITEMID"
                'CODENAME_get("ITEMID", TxtITEMID.Text, TxtITEMNAME.Text, WW_RtnSW)  '大項目
                CODENAME_get("ITEMID", TxtITEMNAME.Text, TxtITEMID.Text, WW_RtnSW)  '大項目
                TxtITEMNAME.Focus()
            Case "TxtTODOKECODE"
                CODENAME_get("TODOKECODE", TxtTODOKECODE.Text, WF_TODOKENAME.Text, WW_RtnSW)  '届先コード
                TxtTODOKECODE.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If
    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

    End Sub

    ''' <summary>
    ''' 大項目検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspItemCodeSingle()

        Me.mspItemCodeSingle.InitPopUp()
        Me.mspItemCodeSingle.SelectionMode = ListSelectionMode.Single

        Dim WW_TABLEID As String = ""
        WW_TABLEID = LNM0010WRKINC.TBLKGSPRATE

        'Me.mspItemCodeSingle.SQL = CmnSearchSQL.GetSprateItemSQL(WW_TABLEID, Replace(WF_TAISHOYM.Value, "/", ""), LNM0010WRKINC.ORGISHIKARI)
        Me.mspItemCodeSingle.SQL = CmnSearchSQL.GetSprateItemSQL(WW_TABLEID, LNM0010WRKINC.ORGISHIKARI)

        Me.mspItemCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspItemCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetSprateItemTitle)

        Me.mspItemCodeSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 大項目選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspItemCodeSingle()

        Dim selData = Me.mspItemCodeSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtITEMID.ID
                Me.TxtITEMID.Text = selData("ITEMID").ToString '大項目
                Me.TxtITEMNAME.Text = selData("ITEMNAME").ToString '項目名
                Me.TxtITEMNAME.Focus()
        End Select

        'ポップアップの非表示
        Me.mspItemCodeSingle.HidePopUp()

    End Sub

    ''' <summary>
    ''' 届先コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspTodokeCodeSingle()

        Me.mspTodokeCodeSingle.InitPopUp()
        Me.mspTodokeCodeSingle.SelectionMode = ListSelectionMode.Single

        Dim WW_TABLEID As String = ""
        WW_TABLEID = LNM0010WRKINC.TBLKGSPRATE

        Me.mspTodokeCodeSingle.SQL = CmnSearchSQL.GetSprateTodokeSQL(WW_TABLEID, LNM0010WRKINC.ORGISHIKARI)

        Me.mspTodokeCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspTodokeCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetSprateTodokeTitle)

        Me.mspTodokeCodeSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 届先選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspTodokeCodeSingle()

        Dim selData = Me.mspTodokeCodeSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtTODOKECODE.ID
                Me.TxtTODOKECODE.Text = selData("TODOKECODE").ToString '届先コード
                Me.WF_TODOKENAME.Text = selData("TODOKENAME").ToString '届先名
                Me.TxtTODOKECODE.Focus()
        End Select

        'ポップアップの非表示
        Me.mspTodokeCodeSingle.HidePopUp()

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByVal WW_ROW As DataRow, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LineErr As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_StyDateFlag As String = ""
        Dim WW_NewPassEndDate As String = ""
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""
        Dim WW_DBDataCheck As String = ""
        Dim NowDate As DateTime = Date.Now

        '○ 画面操作権限チェック
        ' 権限チェック(操作者に更新権限があるかチェック)
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            WW_CheckMES1 = "・特別料金マスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック

        WW_LineErr = ""

        ' 削除フラグ(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "DELFLG", WW_ROW("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("DELFLG", WW_ROW("DELFLG"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・削除コード入力エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・削除コードエラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 取引先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORICODE", WW_ROW("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 取引先名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORINAME", WW_ROW("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 部門コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ORGCODE", WW_ROW("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・部門コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 部門名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ORGNAME", WW_ROW("ORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・部門名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 加算先部門コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KASANORGCODE", WW_ROW("KASANORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・加算先部門コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 加算先部門名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KASANORGNAME", WW_ROW("KASANORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・加算先部門名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 届先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TODOKECODE", WW_ROW("TODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・届先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 届先名称(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TODOKENAME", WW_ROW("TODOKENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・届先名称エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 車番(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SYABAN", WW_ROW("SYABAN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・車番エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 対象年月(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TAISHOYM", WW_ROW("TAISHOYM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・対象年月エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        '' 大項目(バリデーションチェック)
        'Master.CheckField(Master.USERCAMP, "ITEMID", WW_ROW("ITEMID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        'If Not isNormal(WW_CS0024FCheckerr) Then
        '    WW_CheckMES1 = "・大項目エラーです。"
        '    WW_CheckMES2 = WW_CS0024FCheckReport
        '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
        '    WW_LineErr = "ERR"
        '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        'End If
        ' 項目名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ITEMNAME", WW_ROW("ITEMNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・項目名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' レコードID(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "RECOID", WW_ROW("RECOID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・レコードIDエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' レコード名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "RECONAME", WW_ROW("RECONAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・レコード名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TANKA", WW_ROW("TANKA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 回数(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "COUNT", WW_ROW("COUNT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・回数エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 料金(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "FEE", WW_ROW("FEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・料金エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 備考(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "BIKOU", WW_ROW("BIKOU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・備考エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String)

        Dim WW_ErrMes As String = ""
        WW_ErrMes = MESSAGE1
        If Not String.IsNullOrEmpty(MESSAGE2) Then
            WW_ErrMes &= vbCr & "   -->" & MESSAGE2
        End If

        rightview.AddErrorReport(WW_ErrMes)

    End Sub

    ''' <summary>
    ''' LNM0010tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0010tbl_UPD()

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrEmpty(I_VALUE) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        '名称取得
        Dim WW_NAMEht = New Hashtable '名称格納HT
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            Select Case I_FIELD
                Case "ITEMID"        '大項目
                    work.CODEIDGetITEM(SQLcon, WW_NAMEht)
                Case "TODOKECODE"        '届先コード
                    work.CODENAMEGetTODOKE(SQLcon, WW_NAMEht)
            End Select
        End Using

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ALL, Master.USERCAMP))
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DELFLG"))
                Case "ITEMID"        '大項目
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
                Case "TODOKECODE"         '届先コード
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try


    End Sub

End Class
