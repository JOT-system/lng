''************************************************************
' 【廃止】特別料金マスタメンテ登録画面
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
''' 特別料金マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0010SprateDetail
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
                        Case "mspToriCodeSingleRowSelected"  '[共通]取引先コード選択ポップアップで行選択
                            RowSelected_mspToriCodeSingle()
                        Case "mspKasanOrgCodeSingleRowSelected"  '[共通]加算先部門コード選択ポップアップで行選択
                            RowSelected_mspKASANORGCodeSingle()
                        Case "mspTodokeCodeSingleRowSelected"  '[共通]届先コード選択ポップアップで行選択
                            RowSelected_mspTodokeCodeSingle()
                        Case "WF_ORGChange"    '部門コードチェンジ
                            If Not ddlSelectORG.SelectedValue = "" Then
                                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                                    SQLcon.Open()  ' DataBase接続
                                    GetKasanOrg(SQLcon, ddlSelectORG.SelectedValue)
                                End Using
                            Else
                                TxtKASANORGCODE.Text = ""
                                TxtKASANORGNAME.Text = ""
                            End If
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

        '○ ドロップダウンリスト生成
        createListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' ドロップダウン生成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub createListBox()
        Try
            '部門ドロップダウンのクリア
            Me.ddlSelectORG.Items.Clear()
            Me.ddlSelectORG.Items.Add("")

            '部門ドロップダウンの生成
            'Dim retOfficeList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "ORGCODEDROP")
            Dim retOfficeList As DropDownList = CmnLng.getDowpDownFixedList("02", "ORGCODEDROP")
            If retOfficeList.Items.Count > 0 Then
                '情シス、高圧ガス以外
                If LNM0010WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
                    Dim WW_OrgPermitHt As New Hashtable
                    Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()  ' DataBase接続
                        work.GetPermitOrg(SQLcon, Master.USERCAMP, Master.ROLE_ORG, WW_OrgPermitHt)
                        For index As Integer = 0 To retOfficeList.Items.Count - 1
                            If WW_OrgPermitHt.ContainsKey(retOfficeList.Items(index).Value) = True Then
                                ddlSelectORG.Items.Add(New ListItem(retOfficeList.Items(index).Text, retOfficeList.Items(index).Value))
                            End If
                        Next
                    End Using
                Else
                    For index As Integer = 0 To retOfficeList.Items.Count - 1
                        ddlSelectORG.Items.Add(New ListItem(retOfficeList.Items(index).Text, retOfficeList.Items(index).Value))
                    Next
                End If
            End If

        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = New StackFrame(0, False).GetMethod.DeclaringType.FullName  ' クラス名
            CS0011LOGWrite.INFPOSI = Reflection.MethodBase.GetCurrentMethod.Name                    ' メソッド名
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             ' ログ出力
        End Try

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0010L Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        TxtSelLineCNT.Text = work.WF_SEL_LINECNT.Text
        '削除
        ddlDELFLG.SelectedValue = work.WF_SEL_DELFLG.Text
        'CODENAME_get("DELFLG", ddlDELFLG.SelectedValue, LblDelFlgName.Text, WW_Dummy)
        '画面ＩＤ
        TxtMapId.Text = "M00001"
        '会社コード
        TxtCampCode.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", TxtCampCode.Text, LblCampCodeName.Text, WW_RtnSW)
        'レコードID
        TxtRECOID.Text = work.WF_SEL_RECOID.Text
        'レコード名
        TxtRECONAME.Text = work.WF_SEL_RECONAME.Text
        '取引先コード
        TxtTORICODE.Text = work.WF_SEL_TORICODE.Text
        '取引先名称
        TxtTORINAME.Text = work.WF_SEL_TORINAME.Text
        '部門コード
        ddlSelectORG.SelectedValue = work.WF_SEL_ORGCODE.Text
        '加算先部門コード
        TxtKASANORGCODE.Text = work.WF_SEL_KASANORGCODE.Text
        '加算先部門名称
        TxtKASANORGNAME.Text = work.WF_SEL_KASANORGNAME.Text
        '届先コード
        TxtTODOKECODE.Text = work.WF_SEL_TODOKECODE.Text
        '届先名称
        TxtTODOKENAME.Text = work.WF_SEL_TODOKENAME.Text
        '有効開始日
        WF_StYMD.Value = work.WF_SEL_STYMD.Text
        '有効終了日
        WF_EndYMD.Value = work.WF_SEL_ENDYMD.Text
        '金額
        TxtKINGAKU.Text = work.WF_SEL_KINGAKU.Text
        '車番
        TxtSYABAN.Text = work.WF_SEL_SYABAN.Text
        '対象年月
        WF_TAISHOYM.Value = work.WF_SEL_TAISHOYM.Text
        '車腹
        TxtSYABARA.Text = work.WF_SEL_SYABARA.Text
        '固定費
        TxtKOTEIHI.Text = work.WF_SEL_KOTEIHI.Text
        '走行距離
        TxtKYORI.Text = work.WF_SEL_KYORI.Text
        '実勢軽油価格
        TxtKEIYU.Text = work.WF_SEL_KEIYU.Text
        '基準価格
        TxtKIZYUN.Text = work.WF_SEL_KIZYUN.Text
        '単価差
        TxtTANKASA.Text = work.WF_SEL_TANKASA.Text
        '輸送回数
        TxtKAISU.Text = work.WF_SEL_KAISU.Text
        '回数
        TxtCOUNT.Text = work.WF_SEL_COUNT.Text
        '燃料使用量
        TxtUSAGECHARGE.Text = work.WF_SEL_USAGECHARGE.Text
        'サーチャージ
        TxtSURCHARGE.Text = work.WF_SEL_SURCHARGE.Text
        '備考1
        TxtBIKOU1.Text = work.WF_SEL_BIKOU1.Text
        '備考2
        TxtBIKOU2.Text = work.WF_SEL_BIKOU2.Text
        '備考3
        TxtBIKOU3.Text = work.WF_SEL_BIKOU3.Text

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_ORGCODE.Text

        '表示制御項目
        'テーブル毎の表示項目制御
        VisibleKeyControlTable.Value = work.WF_SEL_CONTROLTABLE.Text

        If DisabledKeyItem.Value = "" Then
            '部門コード件数取得
            DisabledKeyOrgCount.Value = ddlSelectORG.Items.Count
            '部門コード件数が2件(空白行1件と選択可能行1件)の場合取引先件数取得
            If ddlSelectORG.Items.Count = 2 Then
                If TxtKASANORGCODE.Text = "" Then
                    Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()  ' DataBase接続
                        ddlSelectORG.SelectedIndex = 1
                        ddlSelectORG.Enabled = False
                        DisabledKeyToriCount.Value = GetToriCnt(SQLcon, ddlSelectORG.SelectedValue)
                        '取引先件数1件の場合取引先、加算先、届け先取得
                        If CInt(DisabledKeyToriCount.Value) = 1 Then
                            GetToriKasan(SQLcon, ddlSelectORG.SelectedValue,
                                       TxtTORICODE.Text, TxtTORINAME.Text,
                                       TxtKASANORGCODE.Text, TxtKASANORGNAME.Text)
                            'TxtTORICODE.Enabled = False
                            'TxtTORINAME.Enabled = False
                            TxtKASANORGCODE.Enabled = False
                            TxtKASANORGNAME.Enabled = False

                        End If
                    End Using
                End If

            End If
        Else
            'TxtTORICODE.Enabled = False
            'TxtTORINAME.Enabled = False
            ddlSelectORG.Enabled = False
            TxtKASANORGCODE.Enabled = False
            TxtKASANORGNAME.Enabled = False
        End If

        '情シス、高圧ガス以外の場合
        If LNM0010WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            TxtTORICODE.Enabled = False
            TxtTORINAME.Enabled = False
        End If

        ' 削除フラグ・取引先コード・加算先部門コード・届先コード
        ' 金額・車腹・固定費・走行距離
        ' 実勢軽油価格・基準価格・単価差・輸送回数
        ' 回数・サーチャージを入力するテキストボックスは数値(0～9)のみ可能とする。
        'Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTORICODE.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtKASANORGCODE.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTODOKECODE.Attributes("onkeyPress") = "CheckNum()"

        Me.TxtKINGAKU.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtSYABARA.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtKOTEIHI.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtKYORI.Attributes("onkeyPress") = "CheckNum()"

        Me.TxtKEIYU.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtKIZYUN.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTANKASA.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtKAISU.Attributes("onkeyPress") = "CheckNum()"

        Me.TxtCOUNT.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtSURCHARGE.Attributes("onkeyPress") = "CheckNum()"

        ' 有効開始日・有効終了日・対象年月を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.WF_StYMD.Attributes("onkeyPress") = "CheckCalendar()"
        Me.WF_EndYMD.Attributes("onkeyPress") = "CheckCalendar()"
        'Me.WF_TAISHOYM.Attributes("onkeyPress") = "CheckCalendar()"

        ' 入力するテキストボックスは数値(0～9)＋記号(.)のみ可能とする。
        Me.TxtSYABARA.Attributes("onkeyPress") = "CheckDeci()"             '車腹

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU
    End Sub

    ''' <summary>
    ''' 取引先件数取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Function GetToriCnt(ByVal SQLcon As MySqlConnection, ByVal WW_ORGCODE As String) As Integer
        GetToriCnt = 0

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT ")
        SQLStr.AppendLine("       TORICODE")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.VIW0004_SPRATE")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("        ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND  DELFLG  = '0'                         ")
        SQLStr.AppendLine("  AND TABLEID = @TABLEID                      ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_TABLEID As MySqlParameter = SQLcmd.Parameters.Add("@TABLEID", MySqlDbType.VarChar, 30) 'テーブルID

                P_ORGCODE.Value = WW_ORGCODE '部門コード

                Dim WW_TABLEID As String = ""

                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        WW_TABLEID = LNM0010WRKINC.TBLHACHINOHESPRATE
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        WW_TABLEID = LNM0010WRKINC.TBLENEOSCOMFEE
                    Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                        WW_TABLEID = LNM0010WRKINC.TBLTOHOKUSPRATE
                    'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                    '    WW_TABLEID = LNM0010WRKINC.TBLKGSPRATE
                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        WW_TABLEID = LNM0010WRKINC.TBLSKSPRATE
                    Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                        WW_TABLEID = LNM0010WRKINC.TBLSKSURCHARGE
                End Select

                P_TABLEID.Value = WW_TABLEID 'テーブルID

                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)
                End Using

                GetToriCnt = WW_Tbl.Rows.Count
            End Using
        Catch ex As Exception
        End Try
    End Function

    ''' <summary>
    ''' 加算先部門取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub GetKasanOrg(ByVal SQLcon As MySqlConnection, ByVal WW_ORGCODE As String)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT ")
        SQLStr.AppendLine("       KASANORGCODE")
        SQLStr.AppendLine("      ,KASANORGNAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.VIW0004_SPRATE")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("        ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND  DELFLG  = '0'                         ")
        SQLStr.AppendLine("   AND TABLEID = @TABLEID                      ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_TABLEID As MySqlParameter = SQLcmd.Parameters.Add("@TABLEID", MySqlDbType.VarChar, 30) 'テーブルID

                P_ORGCODE.Value = WW_ORGCODE '部門コード

                Dim WW_TABLEID As String = ""
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        WW_TABLEID = LNM0010WRKINC.TBLHACHINOHESPRATE
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        WW_TABLEID = LNM0010WRKINC.TBLENEOSCOMFEE
                    Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                        WW_TABLEID = LNM0010WRKINC.TBLTOHOKUSPRATE
                    'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                    '    WW_TABLEID = LNM0010WRKINC.TBLKGSPRATE
                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        WW_TABLEID = LNM0010WRKINC.TBLSKSPRATE
                    Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                        WW_TABLEID = LNM0010WRKINC.TBLSKSURCHARGE
                End Select

                P_TABLEID.Value = WW_TABLEID 'テーブルID

                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)
                End Using

                '１件の場合は加算先部門入力欄に入れる
                If WW_Tbl.Rows.Count = 1 Then
                    TxtKASANORGCODE.Text = WW_Tbl.Rows(0)("KASANORGCODE")
                    TxtKASANORGNAME.Text = WW_Tbl.Rows(0)("KASANORGNAME")
                Else
                    TxtKASANORGCODE.Text = ""
                    TxtKASANORGNAME.Text = ""
                End If
            End Using
        Catch ex As Exception
        End Try
    End Sub

    ''' <summary>
    ''' 取引先、加算先取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub GetToriKasan(ByVal SQLcon As MySqlConnection, ByVal WW_ORGCODE As String,
                                 ByRef WW_TORICODE As String, ByRef WW_TORINAME As String,
                                 ByRef WW_KASANORGCODE As String, ByRef WW_KASANORGNAME As String)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT ")
        SQLStr.AppendLine("       TORICODE")
        SQLStr.AppendLine("      ,TORINAME")
        SQLStr.AppendLine("      ,KASANORGCODE")
        SQLStr.AppendLine("      ,KASANORGNAME")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.VIW0004_SPRATE")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("        ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND  DELFLG  = '0'                         ")
        SQLStr.AppendLine("   AND TABLEID = @TABLEID                      ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_TABLEID As MySqlParameter = SQLcmd.Parameters.Add("@TABLEID", MySqlDbType.VarChar, 30) 'テーブルID

                P_ORGCODE.Value = WW_ORGCODE '部門コード

                Dim WW_TABLEID As String = ""
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        WW_TABLEID = LNM0010WRKINC.TBLHACHINOHESPRATE
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        WW_TABLEID = LNM0010WRKINC.TBLENEOSCOMFEE
                    Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                        WW_TABLEID = LNM0010WRKINC.TBLTOHOKUSPRATE
                    'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                    '    WW_TABLEID = LNM0010WRKINC.TBLKGSPRATE
                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        WW_TABLEID = LNM0010WRKINC.TBLSKSPRATE
                    Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                        WW_TABLEID = LNM0010WRKINC.TBLSKSURCHARGE
                End Select

                P_TABLEID.Value = WW_TABLEID 'テーブルID

                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count > 0 Then
                        WW_TORICODE = WW_Tbl.Rows(0)("TORICODE")
                        WW_TORINAME = WW_Tbl.Rows(0)("TORINAME")
                        WW_KASANORGCODE = WW_Tbl.Rows(0)("KASANORGCODE")
                        WW_KASANORGNAME = WW_Tbl.Rows(0)("KASANORGNAME")

                    End If

                End Using
            End Using
        Catch ex As Exception
        End Try
    End Sub

    ''' <summary>
    ''' 特別料金マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        Dim SQLStr = New StringBuilder
        Dim SQLJnl = New StringBuilder
        Dim LNM0010row As DataRow = LNM0010INPtbl.Rows(0)
        Dim WW_DateNow As DateTime = Date.Now

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
#Region "八戸特別料金マスタ"
                '○ DB更新SQL
                SQLStr.AppendLine("     INSERT INTO LNG.LNM0010_HACHINOHESPRATE           ")
                SQLStr.AppendLine("        (                                    ")
                SQLStr.AppendLine("         DELFLG                              ")
                SQLStr.AppendLine("       , RECOID                              ")
                SQLStr.AppendLine("       , RECONAME                            ")
                SQLStr.AppendLine("       , TORICODE                            ")
                SQLStr.AppendLine("       , TORINAME                            ")
                SQLStr.AppendLine("       , ORGCODE                             ")
                SQLStr.AppendLine("       , ORGNAME                             ")
                SQLStr.AppendLine("       , KASANORGCODE                        ")
                SQLStr.AppendLine("       , KASANORGNAME                        ")
                SQLStr.AppendLine("       , STYMD                               ")
                SQLStr.AppendLine("       , ENDYMD                              ")
                SQLStr.AppendLine("       , KINGAKU                             ")
                SQLStr.AppendLine("       , INITYMD                             ")
                SQLStr.AppendLine("       , INITUSER                            ")
                SQLStr.AppendLine("       , INITTERMID                          ")
                SQLStr.AppendLine("       , INITPGID                            ")
                SQLStr.AppendLine("       , RECEIVEYMD                          ")
                SQLStr.AppendLine("        )                                    ")
                SQLStr.AppendLine("     VALUES                                  ")
                SQLStr.AppendLine("        (                                    ")
                SQLStr.AppendLine("         @DELFLG                             ")
                SQLStr.AppendLine("       , @RECOID                            ")
                SQLStr.AppendLine("       , @RECONAME                            ")
                SQLStr.AppendLine("       , @TORICODE                            ")
                SQLStr.AppendLine("       , @TORINAME                            ")
                SQLStr.AppendLine("       , @ORGCODE                            ")
                SQLStr.AppendLine("       , @ORGNAME                            ")
                SQLStr.AppendLine("       , @KASANORGCODE                            ")
                SQLStr.AppendLine("       , @KASANORGNAME                            ")
                SQLStr.AppendLine("       , @STYMD                            ")
                SQLStr.AppendLine("       , @ENDYMD                            ")
                SQLStr.AppendLine("       , @KINGAKU                            ")
                SQLStr.AppendLine("       , @INITYMD                            ")
                SQLStr.AppendLine("       , @INITUSER                           ")
                SQLStr.AppendLine("       , @INITTERMID                         ")
                SQLStr.AppendLine("       , @INITPGID                           ")
                SQLStr.AppendLine("       , @RECEIVEYMD                         ")
                SQLStr.AppendLine("        )                                    ")
                SQLStr.AppendLine("     ON DUPLICATE KEY UPDATE                 ")
                SQLStr.AppendLine("         DELFLG         = @DELFLG            ")
                SQLStr.AppendLine("       , RECONAME     = @RECONAME                            ")
                SQLStr.AppendLine("       , TORINAME     = @TORINAME                            ")
                SQLStr.AppendLine("       , ORGNAME     = @ORGNAME                            ")
                SQLStr.AppendLine("       , KASANORGCODE     = @KASANORGCODE                            ")
                SQLStr.AppendLine("       , KASANORGNAME     = @KASANORGNAME                            ")
                SQLStr.AppendLine("       , ENDYMD     = @ENDYMD                            ")
                SQLStr.AppendLine("       , KINGAKU     = @KINGAKU                            ")
                SQLStr.AppendLine("       , UPDYMD         = @UPDYMD            ")
                SQLStr.AppendLine("       , UPDUSER        = @UPDUSER           ")
                SQLStr.AppendLine("       , UPDTERMID      = @UPDTERMID         ")
                SQLStr.AppendLine("       , UPDPGID        = @UPDPGID           ")
                SQLStr.AppendLine("       , RECEIVEYMD     = @RECEIVEYMD        ")

                '○ 更新ジャーナル出力SQL

                SQLJnl.AppendLine(" SELECT                                     ")
                SQLJnl.AppendLine("     DELFLG                                 ")
                SQLJnl.AppendLine("   , RECOID                              ")
                SQLJnl.AppendLine("   , RECONAME                              ")
                SQLJnl.AppendLine("   , TORICODE                              ")
                SQLJnl.AppendLine("   , TORINAME                              ")
                SQLJnl.AppendLine("   , ORGCODE                              ")
                SQLJnl.AppendLine("   , ORGNAME                              ")
                SQLJnl.AppendLine("   , KASANORGCODE                              ")
                SQLJnl.AppendLine("   , KASANORGNAME                              ")
                SQLJnl.AppendLine("   , STYMD                              ")
                SQLJnl.AppendLine("   , ENDYMD                              ")
                SQLJnl.AppendLine("   , KINGAKU                              ")
                SQLJnl.AppendLine("   , INITYMD                                ")
                SQLJnl.AppendLine("   , INITUSER                               ")
                SQLJnl.AppendLine("   , INITTERMID                             ")
                SQLJnl.AppendLine("   , INITPGID                               ")
                SQLJnl.AppendLine("   , UPDYMD                                 ")
                SQLJnl.AppendLine("   , UPDUSER                                ")
                SQLJnl.AppendLine("   , UPDTERMID                              ")
                SQLJnl.AppendLine("   , UPDPGID                                ")
                SQLJnl.AppendLine("   , RECEIVEYMD                             ")
                SQLJnl.AppendLine("   , UPDTIMSTP                              ")
                SQLJnl.AppendLine(" FROM                                       ")
                SQLJnl.AppendLine("     LNG.LNM0010_HACHINOHESPRATE                      ")
                SQLJnl.AppendLine(" WHERE                                      ")
                SQLJnl.AppendLine("       RECOID  = @RECOID                ")
                SQLJnl.AppendLine("   AND TORICODE  = @TORICODE                  ")
                SQLJnl.AppendLine("   AND ORGCODE  = @ORGCODE        ")
                SQLJnl.AppendLine("   AND STYMD  = @STYMD                      ")
#End Region
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
#Region "ENEOS業務委託料マスタ"
                '○ DB更新SQL
                SQLStr.AppendLine("     INSERT INTO LNG.LNM0011_ENEOSCOMFEE           ")
                SQLStr.AppendLine("        (                                    ")
                SQLStr.AppendLine("         DELFLG                              ")
                SQLStr.AppendLine("     ,RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,KINGAKU  ")
                SQLStr.AppendLine("       , INITYMD                             ")
                SQLStr.AppendLine("       , INITUSER                            ")
                SQLStr.AppendLine("       , INITTERMID                          ")
                SQLStr.AppendLine("       , INITPGID                            ")
                SQLStr.AppendLine("       , RECEIVEYMD                          ")
                SQLStr.AppendLine("        )                                    ")
                SQLStr.AppendLine("     VALUES                                  ")
                SQLStr.AppendLine("        (                                    ")
                SQLStr.AppendLine("         @DELFLG                             ")
                SQLStr.AppendLine("       , @RECOID                            ")
                SQLStr.AppendLine("       , @RECONAME                            ")
                SQLStr.AppendLine("       , @TORICODE                            ")
                SQLStr.AppendLine("       , @TORINAME                            ")
                SQLStr.AppendLine("       , @ORGCODE                            ")
                SQLStr.AppendLine("       , @ORGNAME                            ")
                SQLStr.AppendLine("       , @KASANORGCODE                            ")
                SQLStr.AppendLine("       , @KASANORGNAME                            ")
                SQLStr.AppendLine("       , @STYMD                            ")
                SQLStr.AppendLine("       , @ENDYMD                            ")
                SQLStr.AppendLine("       , @KINGAKU                            ")
                SQLStr.AppendLine("       , @INITYMD                            ")
                SQLStr.AppendLine("       , @INITUSER                           ")
                SQLStr.AppendLine("       , @INITTERMID                         ")
                SQLStr.AppendLine("       , @INITPGID                           ")
                SQLStr.AppendLine("       , @RECEIVEYMD                         ")
                SQLStr.AppendLine("        )                                    ")
                SQLStr.AppendLine("     ON DUPLICATE KEY UPDATE                 ")
                SQLStr.AppendLine("         DELFLG         = @DELFLG            ")
                SQLStr.AppendLine("       , RECONAME     = @RECONAME                            ")
                SQLStr.AppendLine("       , TORINAME     = @TORINAME                            ")
                SQLStr.AppendLine("       , ORGNAME     = @ORGNAME                            ")
                SQLStr.AppendLine("       , KASANORGCODE     = @KASANORGCODE                            ")
                SQLStr.AppendLine("       , KASANORGNAME     = @KASANORGNAME                            ")
                SQLStr.AppendLine("       , ENDYMD     = @ENDYMD                            ")
                SQLStr.AppendLine("       , KINGAKU     = @KINGAKU                            ")
                SQLStr.AppendLine("       , UPDYMD         = @UPDYMD            ")
                SQLStr.AppendLine("       , UPDUSER        = @UPDUSER           ")
                SQLStr.AppendLine("       , UPDTERMID      = @UPDTERMID         ")
                SQLStr.AppendLine("       , UPDPGID        = @UPDPGID           ")
                SQLStr.AppendLine("       , RECEIVEYMD     = @RECEIVEYMD        ")

                '○ 更新ジャーナル出力SQL

                SQLJnl.AppendLine(" SELECT                                     ")
                SQLJnl.AppendLine("     DELFLG                                 ")
                SQLJnl.AppendLine("   , RECOID                              ")
                SQLJnl.AppendLine("   , RECONAME                              ")
                SQLJnl.AppendLine("   , TORICODE                              ")
                SQLJnl.AppendLine("   , TORINAME                              ")
                SQLJnl.AppendLine("   , ORGCODE                              ")
                SQLJnl.AppendLine("   , ORGNAME                              ")
                SQLJnl.AppendLine("   , KASANORGCODE                              ")
                SQLJnl.AppendLine("   , KASANORGNAME                              ")
                SQLJnl.AppendLine("   , STYMD                              ")
                SQLJnl.AppendLine("   , ENDYMD                              ")
                SQLJnl.AppendLine("   , KINGAKU                              ")
                SQLJnl.AppendLine("   , INITYMD                                ")
                SQLJnl.AppendLine("   , INITUSER                               ")
                SQLJnl.AppendLine("   , INITTERMID                             ")
                SQLJnl.AppendLine("   , INITPGID                               ")
                SQLJnl.AppendLine("   , UPDYMD                                 ")
                SQLJnl.AppendLine("   , UPDUSER                                ")
                SQLJnl.AppendLine("   , UPDTERMID                              ")
                SQLJnl.AppendLine("   , UPDPGID                                ")
                SQLJnl.AppendLine("   , RECEIVEYMD                             ")
                SQLJnl.AppendLine("   , UPDTIMSTP                              ")
                SQLJnl.AppendLine(" FROM                                       ")
                SQLJnl.AppendLine("     LNG.LNM0011_ENEOSCOMFEE                      ")
                SQLJnl.AppendLine(" WHERE                                      ")
                SQLJnl.AppendLine("       RECOID  = @RECOID                ")
                SQLJnl.AppendLine("   AND TORICODE  = @TORICODE                  ")
                SQLJnl.AppendLine("   AND ORGCODE  = @ORGCODE        ")
                SQLJnl.AppendLine("   AND STYMD  = @STYMD                      ")
#End Region
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
#Region "東北電力車両別追加料金マスタ"
                '○ DB更新SQL
                SQLStr.AppendLine("     INSERT INTO LNG.LNM0012_TOHOKUSPRATE           ")
                SQLStr.AppendLine("        (                                    ")
                SQLStr.AppendLine("         DELFLG                              ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("       , INITYMD                             ")
                SQLStr.AppendLine("       , INITUSER                            ")
                SQLStr.AppendLine("       , INITTERMID                          ")
                SQLStr.AppendLine("       , INITPGID                            ")
                SQLStr.AppendLine("       , RECEIVEYMD                          ")
                SQLStr.AppendLine("        )                                    ")
                SQLStr.AppendLine("     VALUES                                  ")
                SQLStr.AppendLine("        (                                    ")
                SQLStr.AppendLine("         @DELFLG                             ")
                SQLStr.AppendLine("       , @TORICODE                            ")
                SQLStr.AppendLine("       , @TORINAME                            ")
                SQLStr.AppendLine("       , @ORGCODE                            ")
                SQLStr.AppendLine("       , @ORGNAME                            ")
                SQLStr.AppendLine("       , @KASANORGCODE                            ")
                SQLStr.AppendLine("       , @KASANORGNAME                            ")
                SQLStr.AppendLine("       , @STYMD                            ")
                SQLStr.AppendLine("       , @ENDYMD                            ")
                SQLStr.AppendLine("       , @SYABAN                            ")
                SQLStr.AppendLine("       , @KOTEIHI                            ")
                SQLStr.AppendLine("       , @KAISU                            ")
                SQLStr.AppendLine("       , @INITYMD                            ")
                SQLStr.AppendLine("       , @INITUSER                           ")
                SQLStr.AppendLine("       , @INITTERMID                         ")
                SQLStr.AppendLine("       , @INITPGID                           ")
                SQLStr.AppendLine("       , @RECEIVEYMD                         ")
                SQLStr.AppendLine("        )                                    ")
                SQLStr.AppendLine("     ON DUPLICATE KEY UPDATE                 ")
                SQLStr.AppendLine("         DELFLG         = @DELFLG            ")
                SQLStr.AppendLine("       , TORINAME     = @TORINAME                            ")
                SQLStr.AppendLine("       , ORGNAME     = @ORGNAME                            ")
                SQLStr.AppendLine("       , KASANORGCODE     = @KASANORGCODE                            ")
                SQLStr.AppendLine("       , KASANORGNAME     = @KASANORGNAME                            ")
                SQLStr.AppendLine("       , ENDYMD     = @ENDYMD                            ")
                SQLStr.AppendLine("       , KOTEIHI     = @KOTEIHI                            ")
                SQLStr.AppendLine("       , KAISU     = @KAISU                            ")
                SQLStr.AppendLine("       , UPDYMD         = @UPDYMD            ")
                SQLStr.AppendLine("       , UPDUSER        = @UPDUSER           ")
                SQLStr.AppendLine("       , UPDTERMID      = @UPDTERMID         ")
                SQLStr.AppendLine("       , UPDPGID        = @UPDPGID           ")
                SQLStr.AppendLine("       , RECEIVEYMD     = @RECEIVEYMD        ")

                '○ 更新ジャーナル出力SQL

                SQLJnl.AppendLine(" SELECT                                     ")
                SQLJnl.AppendLine("     DELFLG                                 ")
                SQLJnl.AppendLine("   , TORICODE                              ")
                SQLJnl.AppendLine("   , TORINAME                              ")
                SQLJnl.AppendLine("   , ORGCODE                              ")
                SQLJnl.AppendLine("   , ORGNAME                              ")
                SQLJnl.AppendLine("   , KASANORGCODE                              ")
                SQLJnl.AppendLine("   , KASANORGNAME                              ")
                SQLJnl.AppendLine("   , STYMD                              ")
                SQLJnl.AppendLine("   , ENDYMD                              ")
                SQLJnl.AppendLine("   , SYABAN                              ")
                SQLJnl.AppendLine("   , KOTEIHI                              ")
                SQLJnl.AppendLine("   , KAISU                              ")
                SQLJnl.AppendLine("   , INITYMD                                ")
                SQLJnl.AppendLine("   , INITUSER                               ")
                SQLJnl.AppendLine("   , INITTERMID                             ")
                SQLJnl.AppendLine("   , INITPGID                               ")
                SQLJnl.AppendLine("   , UPDYMD                                 ")
                SQLJnl.AppendLine("   , UPDUSER                                ")
                SQLJnl.AppendLine("   , UPDTERMID                              ")
                SQLJnl.AppendLine("   , UPDPGID                                ")
                SQLJnl.AppendLine("   , RECEIVEYMD                             ")
                SQLJnl.AppendLine("   , UPDTIMSTP                              ")
                SQLJnl.AppendLine(" FROM                                       ")
                SQLJnl.AppendLine("     LNG.LNM0012_TOHOKUSPRATE                      ")
                SQLJnl.AppendLine(" WHERE                                      ")
                SQLJnl.AppendLine("       TORICODE  = @TORICODE                ")
                SQLJnl.AppendLine("   AND ORGCODE  = @ORGCODE                  ")
                SQLJnl.AppendLine("   AND STYMD  = @STYMD        ")
                SQLJnl.AppendLine("   AND SYABAN  = @SYABAN                      ")
#End Region
            'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
#Region "SK特別料金マスタ"
                '○ DB更新SQL
                SQLStr.AppendLine("     INSERT INTO LNG.LNM0014_SKSPRATE           ")
                SQLStr.AppendLine("        (                                    ")
                SQLStr.AppendLine("         DELFLG                              ")
                SQLStr.AppendLine("     ,RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABARA  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
                SQLStr.AppendLine("     ,BIKOU1  ")
                SQLStr.AppendLine("     ,BIKOU2  ")
                SQLStr.AppendLine("     ,BIKOU3  ")
                SQLStr.AppendLine("       , INITYMD                             ")
                SQLStr.AppendLine("       , INITUSER                            ")
                SQLStr.AppendLine("       , INITTERMID                          ")
                SQLStr.AppendLine("       , INITPGID                            ")
                SQLStr.AppendLine("       , RECEIVEYMD                          ")
                SQLStr.AppendLine("        )                                    ")
                SQLStr.AppendLine("     VALUES                                  ")
                SQLStr.AppendLine("        (                                    ")
                SQLStr.AppendLine("         @DELFLG                             ")
                SQLStr.AppendLine("       , @RECOID                            ")
                SQLStr.AppendLine("       , @RECONAME                            ")
                SQLStr.AppendLine("       , @TORICODE                            ")
                SQLStr.AppendLine("       , @TORINAME                            ")
                SQLStr.AppendLine("       , @ORGCODE                            ")
                SQLStr.AppendLine("       , @ORGNAME                            ")
                SQLStr.AppendLine("       , @KASANORGCODE                            ")
                SQLStr.AppendLine("       , @KASANORGNAME                            ")
                SQLStr.AppendLine("       , @TODOKECODE                            ")
                SQLStr.AppendLine("       , @TODOKENAME                            ")
                SQLStr.AppendLine("       , @STYMD                            ")
                SQLStr.AppendLine("       , @ENDYMD                            ")
                SQLStr.AppendLine("       , @SYABARA                            ")
                SQLStr.AppendLine("       , @KOTEIHI                            ")
                SQLStr.AppendLine("       , @BIKOU1                            ")
                SQLStr.AppendLine("       , @BIKOU2                            ")
                SQLStr.AppendLine("       , @BIKOU3                            ")
                SQLStr.AppendLine("       , @INITYMD                            ")
                SQLStr.AppendLine("       , @INITUSER                           ")
                SQLStr.AppendLine("       , @INITTERMID                         ")
                SQLStr.AppendLine("       , @INITPGID                           ")
                SQLStr.AppendLine("       , @RECEIVEYMD                         ")
                SQLStr.AppendLine("        )                                    ")
                SQLStr.AppendLine("     ON DUPLICATE KEY UPDATE                 ")
                SQLStr.AppendLine("         DELFLG         = @DELFLG            ")
                SQLStr.AppendLine("       , RECONAME     = @RECONAME                            ")
                SQLStr.AppendLine("       , TORINAME     = @TORINAME                            ")
                SQLStr.AppendLine("       , ORGNAME     = @ORGNAME                            ")
                SQLStr.AppendLine("       , KASANORGCODE     = @KASANORGCODE                            ")
                SQLStr.AppendLine("       , KASANORGNAME     = @KASANORGNAME                            ")
                SQLStr.AppendLine("       , TODOKECODE     = @TODOKECODE                            ")
                SQLStr.AppendLine("       , TODOKENAME     = @TODOKENAME                            ")
                SQLStr.AppendLine("       , ENDYMD     = @ENDYMD                            ")
                SQLStr.AppendLine("       , SYABARA     = @SYABARA                            ")
                SQLStr.AppendLine("       , KOTEIHI     = @KOTEIHI                            ")
                SQLStr.AppendLine("       , BIKOU1     = @BIKOU1                            ")
                SQLStr.AppendLine("       , BIKOU2     = @BIKOU2                            ")
                SQLStr.AppendLine("       , BIKOU3     = @BIKOU3                            ")

                '○ 更新ジャーナル出力SQL

                SQLJnl.AppendLine(" SELECT                                     ")
                SQLJnl.AppendLine("     DELFLG                                 ")
                SQLJnl.AppendLine("   , RECOID                              ")
                SQLJnl.AppendLine("   , RECONAME                              ")
                SQLJnl.AppendLine("   , TORICODE                              ")
                SQLJnl.AppendLine("   , TORINAME                              ")
                SQLJnl.AppendLine("   , ORGCODE                              ")
                SQLJnl.AppendLine("   , ORGNAME                              ")
                SQLJnl.AppendLine("   , KASANORGCODE                              ")
                SQLJnl.AppendLine("   , KASANORGNAME                              ")
                SQLJnl.AppendLine("   , TODOKECODE                              ")
                SQLJnl.AppendLine("   , TODOKENAME                              ")
                SQLJnl.AppendLine("   , STYMD                              ")
                SQLJnl.AppendLine("   , ENDYMD                              ")
                SQLJnl.AppendLine("   , SYABARA                              ")
                SQLJnl.AppendLine("   , KOTEIHI                              ")
                SQLJnl.AppendLine("   , BIKOU1                              ")
                SQLJnl.AppendLine("   , BIKOU2                              ")
                SQLJnl.AppendLine("   , BIKOU3                              ")
                SQLJnl.AppendLine("   , INITYMD                                ")
                SQLJnl.AppendLine("   , INITUSER                               ")
                SQLJnl.AppendLine("   , INITTERMID                             ")
                SQLJnl.AppendLine("   , INITPGID                               ")
                SQLJnl.AppendLine("   , UPDYMD                                 ")
                SQLJnl.AppendLine("   , UPDUSER                                ")
                SQLJnl.AppendLine("   , UPDTERMID                              ")
                SQLJnl.AppendLine("   , UPDPGID                                ")
                SQLJnl.AppendLine("   , RECEIVEYMD                             ")
                SQLJnl.AppendLine("   , UPDTIMSTP                              ")
                SQLJnl.AppendLine(" FROM                                       ")
                SQLJnl.AppendLine("     LNG.LNM0014_SKSPRATE                      ")
                SQLJnl.AppendLine(" WHERE                                      ")
                SQLJnl.AppendLine("       RECOID  = @RECOID                ")
                SQLJnl.AppendLine("   AND TORICODE  = @TORICODE                  ")
                SQLJnl.AppendLine("   AND ORGCODE  = @ORGCODE        ")
                SQLJnl.AppendLine("   AND STYMD  = @STYMD                      ")
#End Region
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
#Region "SK燃料サーチャージマスタ"
                '○ DB更新SQL
                SQLStr.AppendLine("     INSERT INTO LNG.LNM0015_SKSURCHARGE           ")
                SQLStr.AppendLine("        (                                    ")
                SQLStr.AppendLine("         DELFLG                              ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,KYORI  ")
                SQLStr.AppendLine("     ,KEIYU  ")
                SQLStr.AppendLine("     ,KIZYUN  ")
                SQLStr.AppendLine("     ,TANKASA  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("     ,USAGECHARGE  ")
                SQLStr.AppendLine("     ,SURCHARGE  ")
                SQLStr.AppendLine("     ,BIKOU1  ")
                SQLStr.AppendLine("       , INITYMD                             ")
                SQLStr.AppendLine("       , INITUSER                            ")
                SQLStr.AppendLine("       , INITTERMID                          ")
                SQLStr.AppendLine("       , INITPGID                            ")
                SQLStr.AppendLine("       , RECEIVEYMD                          ")
                SQLStr.AppendLine("        )                                    ")
                SQLStr.AppendLine("     VALUES                                  ")
                SQLStr.AppendLine("        (                                    ")
                SQLStr.AppendLine("         @DELFLG                             ")
                SQLStr.AppendLine("       , @TORICODE                            ")
                SQLStr.AppendLine("       , @TORINAME                            ")
                SQLStr.AppendLine("       , @ORGCODE                            ")
                SQLStr.AppendLine("       , @ORGNAME                            ")
                SQLStr.AppendLine("       , @KASANORGCODE                            ")
                SQLStr.AppendLine("       , @KASANORGNAME                            ")
                SQLStr.AppendLine("       , @TODOKECODE                            ")
                SQLStr.AppendLine("       , @TODOKENAME                            ")
                SQLStr.AppendLine("       , @TAISHOYM                            ")
                SQLStr.AppendLine("       , @KYORI                            ")
                SQLStr.AppendLine("       , @KEIYU                            ")
                SQLStr.AppendLine("       , @KIZYUN                            ")
                SQLStr.AppendLine("       , @TANKASA                            ")
                SQLStr.AppendLine("       , @KAISU                            ")
                SQLStr.AppendLine("       , @USAGECHARGE                            ")
                SQLStr.AppendLine("       , @SURCHARGE                            ")
                SQLStr.AppendLine("       , @BIKOU1                            ")
                SQLStr.AppendLine("       , @INITYMD                            ")
                SQLStr.AppendLine("       , @INITUSER                           ")
                SQLStr.AppendLine("       , @INITTERMID                         ")
                SQLStr.AppendLine("       , @INITPGID                           ")
                SQLStr.AppendLine("       , @RECEIVEYMD                         ")
                SQLStr.AppendLine("        )                                    ")
                SQLStr.AppendLine("     ON DUPLICATE KEY UPDATE                 ")
                SQLStr.AppendLine("         DELFLG         = @DELFLG            ")
                SQLStr.AppendLine("       , TORINAME     = @TORINAME                            ")
                SQLStr.AppendLine("       , ORGNAME     = @ORGNAME                            ")
                SQLStr.AppendLine("       , KASANORGCODE     = @KASANORGCODE                            ")
                SQLStr.AppendLine("       , KASANORGNAME     = @KASANORGNAME                            ")
                SQLStr.AppendLine("       , TODOKECODE     = @TODOKECODE                            ")
                SQLStr.AppendLine("       , TODOKENAME     = @TODOKENAME                            ")
                SQLStr.AppendLine("       , KYORI     = @KYORI                            ")
                SQLStr.AppendLine("       , KEIYU     = @KEIYU                            ")
                SQLStr.AppendLine("       , KIZYUN     = @KIZYUN                            ")
                SQLStr.AppendLine("       , TANKASA     = @TANKASA                            ")
                SQLStr.AppendLine("       , KAISU     = @KAISU                            ")
                SQLStr.AppendLine("       , USAGECHARGE     = @USAGECHARGE                            ")
                SQLStr.AppendLine("       , SURCHARGE     = @SURCHARGE                            ")
                SQLStr.AppendLine("       , BIKOU1     = @BIKOU1                            ")

                '○ 更新ジャーナル出力SQL

                SQLJnl.AppendLine(" SELECT                                     ")
                SQLJnl.AppendLine("     DELFLG                                 ")
                SQLJnl.AppendLine("   , TORICODE                              ")
                SQLJnl.AppendLine("   , TORINAME                              ")
                SQLJnl.AppendLine("   , ORGCODE                              ")
                SQLJnl.AppendLine("   , ORGNAME                              ")
                SQLJnl.AppendLine("   , KASANORGCODE                              ")
                SQLJnl.AppendLine("   , KASANORGNAME                              ")
                SQLJnl.AppendLine("   , TODOKECODE                              ")
                SQLJnl.AppendLine("   , TODOKENAME                              ")
                SQLJnl.AppendLine("   , TAISHOYM                              ")
                SQLJnl.AppendLine("   , KYORI                              ")
                SQLJnl.AppendLine("   , KEIYU                              ")
                SQLJnl.AppendLine("   , KIZYUN                              ")
                SQLJnl.AppendLine("   , TANKASA                              ")
                SQLJnl.AppendLine("   , KAISU                              ")
                SQLJnl.AppendLine("   , USAGECHARGE                              ")
                SQLJnl.AppendLine("   , SURCHARGE                              ")
                SQLJnl.AppendLine("   , BIKOU1                              ")
                SQLJnl.AppendLine("   , INITYMD                                ")
                SQLJnl.AppendLine("   , INITUSER                               ")
                SQLJnl.AppendLine("   , INITTERMID                             ")
                SQLJnl.AppendLine("   , INITPGID                               ")
                SQLJnl.AppendLine("   , UPDYMD                                 ")
                SQLJnl.AppendLine("   , UPDUSER                                ")
                SQLJnl.AppendLine("   , UPDTERMID                              ")
                SQLJnl.AppendLine("   , UPDPGID                                ")
                SQLJnl.AppendLine("   , RECEIVEYMD                             ")
                SQLJnl.AppendLine("   , UPDTIMSTP                              ")
                SQLJnl.AppendLine(" FROM                                       ")
                SQLJnl.AppendLine("     LNG.LNM0015_SKSURCHARGE                      ")
                SQLJnl.AppendLine(" WHERE                                      ")
                SQLJnl.AppendLine("       TORICODE  = @TORICODE                ")
                SQLJnl.AppendLine("   AND ORGCODE  = @ORGCODE                  ")
                SQLJnl.AppendLine("   AND TAISHOYM  = @TAISHOYM        ")
#End Region
        End Select

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl.ToString, SQLcon)
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
#Region "八戸特別料金マスタ"
                        ' DB更新用パラメータ
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                        Dim P_KINGAKU As MySqlParameter = SQLcmd.Parameters.Add("@KINGAKU", MySqlDbType.Decimal, 10)     '金額
                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                        Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                        ' 更新ジャーナル出力用パラメータ
                        Dim JP_RECOID As MySqlParameter = SQLcmdJnl.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim JP_TORICODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim JP_ORGCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim JP_STYMD As MySqlParameter = SQLcmdJnl.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        ' DB更新
                        P_DELFLG.Value = LNM0010row("DELFLG")               '削除フラグ
                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_RECONAME.Value = LNM0010row("RECONAME")           'レコード名
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_TORINAME.Value = LNM0010row("TORINAME")           '取引先名称
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_ORGNAME.Value = LNM0010row("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = LNM0010row("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = LNM0010row("KASANORGNAME")           '加算先部門名称
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日
                        'P_ENDYMD.Value = LNM0010row("ENDYMD")           '有効終了日
                        '有効終了日(画面入力済みの場合画面入力を優先)
                        If Not WF_EndYMD.Value = "" Then
                            P_ENDYMD.Value = LNM0010row("ENDYMD")
                        Else
                            P_ENDYMD.Value = WF_AUTOENDYMD.Value
                        End If
                        P_KINGAKU.Value = LNM0010row("KINGAKU")           '金額

                        P_INITYMD.Value = WW_DateNow                        '登録年月日
                        P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID              '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                        P_UPDYMD.Value = WW_DateNow                         '更新年月日
                        P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                        ' 更新ジャーナル出力
                        JP_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        JP_TORICODE.Value = LNM0010row("TORICODE") '取引先コード
                        JP_ORGCODE.Value = LNM0010row("ORGCODE") '部門コード
                        JP_STYMD.Value = LNM0010row("STYMD")           '有効開始日

#End Region
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
#Region "ENEOS業務委託料マスタ"
                        ' DB更新用パラメータ
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                        Dim P_KINGAKU As MySqlParameter = SQLcmd.Parameters.Add("@KINGAKU", MySqlDbType.Decimal, 10)     '金額
                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                        Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                        ' 更新ジャーナル出力用パラメータ
                        Dim JP_RECOID As MySqlParameter = SQLcmdJnl.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim JP_TORICODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim JP_ORGCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim JP_STYMD As MySqlParameter = SQLcmdJnl.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        ' DB更新
                        P_DELFLG.Value = LNM0010row("DELFLG")               '削除フラグ
                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_RECONAME.Value = LNM0010row("RECONAME")           'レコード名
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_TORINAME.Value = LNM0010row("TORINAME")           '取引先名称
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_ORGNAME.Value = LNM0010row("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = LNM0010row("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = LNM0010row("KASANORGNAME")           '加算先部門名称
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日
                        'P_ENDYMD.Value = LNM0010row("ENDYMD")           '有効終了日
                        '有効終了日(画面入力済みの場合画面入力を優先)
                        If Not WF_EndYMD.Value = "" Then
                            P_ENDYMD.Value = LNM0010row("ENDYMD")
                        Else
                            P_ENDYMD.Value = WF_AUTOENDYMD.Value
                        End If
                        P_KINGAKU.Value = LNM0010row("KINGAKU")           '金額

                        P_INITYMD.Value = WW_DateNow                        '登録年月日
                        P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID              '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                        P_UPDYMD.Value = WW_DateNow                         '更新年月日
                        P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                        ' 更新ジャーナル出力
                        JP_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        JP_TORICODE.Value = LNM0010row("TORICODE") '取引先コード
                        JP_ORGCODE.Value = LNM0010row("ORGCODE") '部門コード
                        JP_STYMD.Value = LNM0010row("STYMD")           '有効開始日
#End Region
                    Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
#Region "東北電力車両別追加料金マスタ"
                        ' DB更新用パラメータ
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                        Dim P_KOTEIHI As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHI", MySqlDbType.Decimal, 8)     '固定費
                        Dim P_KAISU As MySqlParameter = SQLcmd.Parameters.Add("@KAISU", MySqlDbType.Decimal, 3)     '回数

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                        Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                        ' 更新ジャーナル出力用パラメータ
                        Dim JP_TORICODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim JP_ORGCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim JP_STYMD As MySqlParameter = SQLcmdJnl.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim JP_SYABAN As MySqlParameter = SQLcmdJnl.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番

                        ' DB更新
                        P_DELFLG.Value = LNM0010row("DELFLG")               '削除フラグ
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_TORINAME.Value = LNM0010row("TORINAME")           '取引先名称
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_ORGNAME.Value = LNM0010row("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = LNM0010row("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = LNM0010row("KASANORGNAME")           '加算先部門名称
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日
                        'P_ENDYMD.Value = LNM0010row("ENDYMD")           '有効終了日
                        '有効終了日(画面入力済みの場合画面入力を優先)
                        If Not WF_EndYMD.Value = "" Then
                            P_ENDYMD.Value = LNM0010row("ENDYMD")
                        Else
                            P_ENDYMD.Value = WF_AUTOENDYMD.Value
                        End If

                        P_SYABAN.Value = LNM0010row("SYABAN")           '車番
                        P_KOTEIHI.Value = LNM0010row("KOTEIHI")           '固定費
                        P_KAISU.Value = LNM0010row("KAISU")           '回数

                        P_INITYMD.Value = WW_DateNow                        '登録年月日
                        P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID              '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                        P_UPDYMD.Value = WW_DateNow                         '更新年月日
                        P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                        ' 更新ジャーナル出力
                        JP_TORICODE.Value = LNM0010row("TORICODE") '取引先コード
                        JP_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        JP_STYMD.Value = LNM0010row("STYMD")           '有効開始日
                        JP_SYABAN.Value = LNM0010row("SYABAN")           '車番
#End Region
                    'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
#Region "SK特別料金マスタ"
                        ' DB更新用パラメータ
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_RECONAME As MySqlParameter = SQLcmd.Parameters.Add("@RECONAME", MySqlDbType.VarChar, 100)     'レコード名
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                        Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                        Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10)     '車腹
                        Dim P_KOTEIHI As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHI", MySqlDbType.Decimal, 8)     '固定費
                        Dim P_BIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU1", MySqlDbType.VarChar, 50)     '備考1
                        Dim P_BIKOU2 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU2", MySqlDbType.VarChar, 50)     '備考2
                        Dim P_BIKOU3 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU3", MySqlDbType.VarChar, 50)     '備考3

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                        Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                        ' 更新ジャーナル出力用パラメータ
                        Dim JP_RECOID As MySqlParameter = SQLcmdJnl.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim JP_TORICODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim JP_ORGCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim JP_STYMD As MySqlParameter = SQLcmdJnl.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        ' DB更新
                        P_DELFLG.Value = LNM0010row("DELFLG")               '削除フラグ
                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_RECONAME.Value = LNM0010row("RECONAME")           'レコード名
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_TORINAME.Value = LNM0010row("TORINAME")           '取引先名称
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_ORGNAME.Value = LNM0010row("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = LNM0010row("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = LNM0010row("KASANORGNAME")           '加算先部門名称
                        P_TODOKECODE.Value = LNM0010row("TODOKECODE")           '届先コード
                        P_TODOKENAME.Value = LNM0010row("TODOKENAME")           '届先名称
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日
                        'P_ENDYMD.Value = LNM0010row("ENDYMD")           '有効終了日
                        '有効終了日(画面入力済みの場合画面入力を優先)
                        If Not WF_EndYMD.Value = "" Then
                            P_ENDYMD.Value = LNM0010row("ENDYMD")
                        Else
                            P_ENDYMD.Value = WF_AUTOENDYMD.Value
                        End If
                        P_SYABARA.Value = LNM0010row("SYABARA")           '車腹
                        P_KOTEIHI.Value = LNM0010row("KOTEIHI")           '固定費
                        P_BIKOU1.Value = LNM0010row("BIKOU1")           '備考1
                        P_BIKOU2.Value = LNM0010row("BIKOU2")           '備考2
                        P_BIKOU3.Value = LNM0010row("BIKOU3")           '備考3

                        P_INITYMD.Value = WW_DateNow                        '登録年月日
                        P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID              '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                        P_UPDYMD.Value = WW_DateNow                         '更新年月日
                        P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                        ' 更新ジャーナル出力
                        JP_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        JP_TORICODE.Value = LNM0010row("TORICODE") '取引先コード
                        JP_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        JP_STYMD.Value = LNM0010row("STYMD")           '有効開始日

#End Region
                    Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
#Region "SK燃料サーチャージマスタ"
                        ' DB更新用パラメータ
                        Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                        Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                        Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                        Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                        Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.Decimal, 6)     '対象年月
                        Dim P_KYORI As MySqlParameter = SQLcmd.Parameters.Add("@KYORI", MySqlDbType.Decimal, 5)     '走行距離
                        Dim P_KEIYU As MySqlParameter = SQLcmd.Parameters.Add("@KEIYU", MySqlDbType.Decimal, 5)     '実勢軽油価格
                        Dim P_KIZYUN As MySqlParameter = SQLcmd.Parameters.Add("@KIZYUN", MySqlDbType.Decimal, 5)     '基準価格
                        Dim P_TANKASA As MySqlParameter = SQLcmd.Parameters.Add("@TANKASA", MySqlDbType.Decimal, 5)     '単価差
                        Dim P_KAISU As MySqlParameter = SQLcmd.Parameters.Add("@KAISU", MySqlDbType.VarChar, 3)     '輸送回数
                        Dim P_USAGECHARGE As MySqlParameter = SQLcmd.Parameters.Add("@USAGECHARGE", MySqlDbType.VarChar, 5)     '燃料使用量
                        Dim P_SURCHARGE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGE", MySqlDbType.Decimal, 8)     'サーチャージ
                        Dim P_BIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU1", MySqlDbType.VarChar, 50)     '備考1

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                        Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                        Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                        Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                        Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                        Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                        ' 更新ジャーナル出力用パラメータ
                        Dim JP_TORICODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim JP_ORGCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim JP_TAISHOYM As MySqlParameter = SQLcmdJnl.Parameters.Add("@TAISHOYM", MySqlDbType.Decimal, 6)     '対象年月

                        ' DB更新
                        P_DELFLG.Value = LNM0010row("DELFLG")               '削除フラグ
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_TORINAME.Value = LNM0010row("TORINAME")           '取引先名称
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_ORGNAME.Value = LNM0010row("ORGNAME")           '部門名称
                        P_KASANORGCODE.Value = LNM0010row("KASANORGCODE")           '加算先部門コード
                        P_KASANORGNAME.Value = LNM0010row("KASANORGNAME")           '加算先部門名称
                        P_TODOKECODE.Value = LNM0010row("TODOKECODE")           '届先コード
                        P_TODOKENAME.Value = LNM0010row("TODOKENAME")           '届先名称
                        P_TAISHOYM.Value = LNM0010row("TAISHOYM")           '対象年月
                        P_KYORI.Value = LNM0010row("KYORI")           '走行距離
                        P_KEIYU.Value = LNM0010row("KEIYU")           '実勢軽油価格
                        P_KIZYUN.Value = LNM0010row("KIZYUN")           '基準価格
                        P_TANKASA.Value = LNM0010row("TANKASA")           '単価差
                        P_KAISU.Value = LNM0010row("KAISU")           '輸送回数
                        P_USAGECHARGE.Value = LNM0010row("USAGECHARGE")           '燃料使用量
                        P_SURCHARGE.Value = LNM0010row("SURCHARGE")           'サーチャージ
                        P_BIKOU1.Value = LNM0010row("BIKOU1")           '備考1

                        P_INITYMD.Value = WW_DateNow                        '登録年月日
                        P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                        P_INITTERMID.Value = Master.USERTERMID              '登録端末
                        P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                        P_UPDYMD.Value = WW_DateNow                         '更新年月日
                        P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                        P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                        P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                        P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                        ' 更新ジャーナル出力
                        JP_TORICODE.Value = LNM0010row("TORICODE") '取引先コード
                        JP_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        JP_TAISHOYM.Value = LNM0010row("TAISHOYM")           '対象年月
#End Region
                End Select

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0010UPDtbl) Then
                        LNM0010UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0010UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0010UPDtbl.Clear()
                    LNM0010UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0010UPDrow As DataRow In LNM0010UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0010D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0010UPDrow
                    CS0020JOURNAL.CS0020JOURNAL()
                    If Not isNormal(CS0020JOURNAL.ERR) Then
                        Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                        CS0011LOGWrite.INFSUBCLASS = "MAIN"               'SUBクラス名
                        CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                        CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                        CS0011LOGWrite.CS0011LOGWrite()                   'ログ出力

                        rightview.AddErrorReport("DB更新ジャーナル出力エラーが発生しました。システム管理者にお問い合わせ下さい。")
                        WW_ErrSW = CS0020JOURNAL.ERR
                        Exit Sub
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0010D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0010D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

#Region "変更履歴テーブル登録"
    ''' <summary>
    ''' 変更チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MASTEREXISTS(ByVal SQLcon As MySqlConnection, ByRef WW_MODIFYKBN As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
#Region "八戸特別料金マスタ"
                '八戸特別料金マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0010_HACHINOHESPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y%m%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y%m%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        Dim LNM0010row As DataRow = LNM0010INPtbl.Rows(0)

                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日

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
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA '変更前
                            Else
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0010_HACHINOHESPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0010_HACHINOHESPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
#Region "ENEOS業務委託料マスタ"
                'ENEOS業務委託料マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0011_ENEOSCOMFEE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y%m%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y%m%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        Dim LNM0010row As DataRow = LNM0010INPtbl.Rows(0)

                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日

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
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA '変更前
                            Else
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011_ENEOSCOMFEE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0011_ENEOSCOMFEE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
#Region "東北電力車両別追加料金マスタ"
                '東北電力車両別追加料金マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0012_TOHOKUSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y%m%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y%m%d'), '') ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番

                        Dim LNM0010row As DataRow = LNM0010INPtbl.Rows(0)

                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日
                        P_SYABAN.Value = LNM0010row("SYABAN")           '車番

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
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA '変更前
                            Else
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0012_TOHOKUSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0012_TOHOKUSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
#Region "SK特別料金マスタ"
                'SK特別料金マスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0014_SKSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y%m%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y%m%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        Dim LNM0010row As DataRow = LNM0010INPtbl.Rows(0)

                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日

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
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA '変更前
                            Else
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014_SKSPRATE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0014_SKSPRATE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
#Region "SK燃料サーチャージマスタ"
                'SK燃料サーチャージマスタに同一キーのデータが存在するか確認する。
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine("    SELECT")
                SQLStr.AppendLine("        TORICODE")
                SQLStr.AppendLine("    FROM")
                SQLStr.AppendLine("        LNG.LNM0015_SKSURCHARGE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '')             = @TAISHOYM ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月

                        Dim LNM0010row As DataRow = LNM0010INPtbl.Rows(0)

                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = Replace(LNM0010row("TAISHOYM").ToString, "/", "")           '対象年月

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
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.BEFDATA '変更前
                            Else
                                WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA '新規
                            End If
                        End Using
                    End Using
                Catch ex As Exception
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0015_SKSURCHARGE SELECT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:LNM0015_SKSURCHARGE SELECT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
        End Select
    End Sub

    ''' <summary>
    ''' 履歴テーブル登録
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsertHist(ByVal SQLcon As MySqlConnection, ByVal WW_MODIFYKBN As String, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
#Region "八戸特別料金マスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0009_HACHINOHESPRATEHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("       RECOID  ")
                SQLStr.AppendLine("       ,RECONAME  ")
                SQLStr.AppendLine("       ,TORICODE  ")
                SQLStr.AppendLine("       ,TORINAME  ")
                SQLStr.AppendLine("       ,ORGCODE  ")
                SQLStr.AppendLine("       ,ORGNAME  ")
                SQLStr.AppendLine("       ,KASANORGCODE  ")
                SQLStr.AppendLine("       ,KASANORGNAME  ")
                SQLStr.AppendLine("       ,STYMD  ")
                SQLStr.AppendLine("       ,ENDYMD  ")
                SQLStr.AppendLine("       ,KINGAKU  ")
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
                SQLStr.AppendLine("       RECOID  ")
                SQLStr.AppendLine("       ,RECONAME  ")
                SQLStr.AppendLine("       ,TORICODE  ")
                SQLStr.AppendLine("       ,TORINAME  ")
                SQLStr.AppendLine("       ,ORGCODE  ")
                SQLStr.AppendLine("       ,ORGNAME  ")
                SQLStr.AppendLine("       ,KASANORGCODE  ")
                SQLStr.AppendLine("       ,KASANORGNAME  ")
                SQLStr.AppendLine("       ,STYMD  ")
                SQLStr.AppendLine("       ,ENDYMD  ")
                SQLStr.AppendLine("       ,KINGAKU  ")
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
                SQLStr.AppendLine("        LNG.LNM0010_HACHINOHESPRATE")
                SQLStr.AppendLine("    WHERE")

                SQLStr.AppendLine("       RECOID  = @RECOID                ")
                SQLStr.AppendLine("   AND TORICODE  = @TORICODE                  ")
                SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                  ")
                SQLStr.AppendLine("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                        Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                        Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                        Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                        Dim LNM0010row As DataRow = LNM0010INPtbl.Rows(0)

                        ' DB更新
                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_TORICODE.Value = LNM0010row("TORICODE") '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE") '部門コード
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日

                        '操作区分
                        '変更区分が新規の場合
                        If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA Then
                            P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.NEWDATA).ToString
                        Else
                            '削除データの場合
                            If LNM0010tbl.Rows(0)("DELFLG") = "0" And LNM0010row("DELFLG") = "1" Then
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.DELDATA).ToString
                            Else
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.UPDDATA).ToString
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0009_HACHINOHESPRATEHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0009_HACHINOHESPRATEHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
#Region "ENEOS業務委託料マスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0010_ENEOSCOMFEEHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("       RECOID  ")
                SQLStr.AppendLine("       ,RECONAME  ")
                SQLStr.AppendLine("       ,TORICODE  ")
                SQLStr.AppendLine("       ,TORINAME  ")
                SQLStr.AppendLine("       ,ORGCODE  ")
                SQLStr.AppendLine("       ,ORGNAME  ")
                SQLStr.AppendLine("       ,KASANORGCODE  ")
                SQLStr.AppendLine("       ,KASANORGNAME  ")
                SQLStr.AppendLine("       ,STYMD  ")
                SQLStr.AppendLine("       ,ENDYMD  ")
                SQLStr.AppendLine("       ,KINGAKU  ")
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
                SQLStr.AppendLine("       RECOID  ")
                SQLStr.AppendLine("       ,RECONAME  ")
                SQLStr.AppendLine("       ,TORICODE  ")
                SQLStr.AppendLine("       ,TORINAME  ")
                SQLStr.AppendLine("       ,ORGCODE  ")
                SQLStr.AppendLine("       ,ORGNAME  ")
                SQLStr.AppendLine("       ,KASANORGCODE  ")
                SQLStr.AppendLine("       ,KASANORGNAME  ")
                SQLStr.AppendLine("       ,STYMD  ")
                SQLStr.AppendLine("       ,ENDYMD  ")
                SQLStr.AppendLine("       ,KINGAKU  ")
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
                SQLStr.AppendLine("        LNG.LNM0011_ENEOSCOMFEE")
                SQLStr.AppendLine("    WHERE")

                SQLStr.AppendLine("       RECOID  = @RECOID                ")
                SQLStr.AppendLine("   AND TORICODE  = @TORICODE                  ")
                SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                  ")
                SQLStr.AppendLine("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                        Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                        Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                        Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                        Dim LNM0010row As DataRow = LNM0010INPtbl.Rows(0)

                        ' DB更新
                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_TORICODE.Value = LNM0010row("TORICODE") '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE") '部門コード
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日

                        '操作区分
                        '変更区分が新規の場合
                        If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA Then
                            P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.NEWDATA).ToString
                        Else
                            '削除データの場合
                            If LNM0010tbl.Rows(0)("DELFLG") = "0" And LNM0010row("DELFLG") = "1" Then
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.DELDATA).ToString
                            Else
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.UPDDATA).ToString
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0010_ENEOSCOMFEEHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0010_ENEOSCOMFEEHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
#Region "東北電力車両別追加料金マスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0011_TOHOKUSPRATEHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
                SQLStr.AppendLine("     ,KAISU  ")
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
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABAN  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
                SQLStr.AppendLine("     ,KAISU  ")
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
                SQLStr.AppendLine("        LNG.LNM0012_TOHOKUSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("       TORICODE  = @TORICODE                ")
                SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                  ")
                SQLStr.AppendLine("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
                SQLStr.AppendLine("   AND SYABAN  = @SYABAN                      ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番

                        Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                        Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                        Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                        Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                        Dim LNM0010row As DataRow = LNM0010INPtbl.Rows(0)

                        P_TORICODE.Value = LNM0010row("TORICODE") '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日
                        P_SYABAN.Value = LNM0010row("SYABAN")           '車番' DB更新

                        '操作区分
                        '変更区分が新規の場合
                        If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA Then
                            P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.NEWDATA).ToString
                        Else
                            '削除データの場合
                            If LNM0010tbl.Rows(0)("DELFLG") = "0" And LNM0010row("DELFLG") = "1" Then
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.DELDATA).ToString
                            Else
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.UPDDATA).ToString
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0011_TOHOKUSPRATEHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0011_TOHOKUSPRATEHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
#Region "SK特別料金マスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0013_SKSPRATEHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("      RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABARA  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
                SQLStr.AppendLine("     ,BIKOU1  ")
                SQLStr.AppendLine("     ,BIKOU2  ")
                SQLStr.AppendLine("     ,BIKOU3  ")
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
                SQLStr.AppendLine("      RECOID  ")
                SQLStr.AppendLine("     ,RECONAME  ")
                SQLStr.AppendLine("     ,TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,STYMD  ")
                SQLStr.AppendLine("     ,ENDYMD  ")
                SQLStr.AppendLine("     ,SYABARA  ")
                SQLStr.AppendLine("     ,KOTEIHI  ")
                SQLStr.AppendLine("     ,BIKOU1  ")
                SQLStr.AppendLine("     ,BIKOU2  ")
                SQLStr.AppendLine("     ,BIKOU3  ")
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
                SQLStr.AppendLine("        LNG.LNM0014_SKSPRATE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("       RECOID  = @RECOID                ")
                SQLStr.AppendLine("   AND TORICODE  = @TORICODE                  ")
                SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE        ")
                SQLStr.AppendLine("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                        Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                        Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                        Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                        Dim LNM0010row As DataRow = LNM0010INPtbl.Rows(0)

                        ' DB更新
                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日

                        '操作区分
                        '変更区分が新規の場合
                        If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA Then
                            P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.NEWDATA).ToString
                        Else
                            '削除データの場合
                            If LNM0010tbl.Rows(0)("DELFLG") = "0" And LNM0010row("DELFLG") = "1" Then
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.DELDATA).ToString
                            Else
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.UPDDATA).ToString
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0013_SKSPRATEHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0013_SKSPRATEHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try
#End Region
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
#Region "SK燃料サーチャージマスタ"
                '○ ＤＢ更新
                Dim SQLStr = New StringBuilder
                SQLStr.AppendLine(" INSERT INTO LNG.LNT0014_SKSURCHARGEHIST ")
                SQLStr.AppendLine("  (  ")
                SQLStr.AppendLine("      TORICODE  ")
                SQLStr.AppendLine("     ,TORINAME  ")
                SQLStr.AppendLine("     ,ORGCODE  ")
                SQLStr.AppendLine("     ,ORGNAME  ")
                SQLStr.AppendLine("     ,KASANORGCODE  ")
                SQLStr.AppendLine("     ,KASANORGNAME  ")
                SQLStr.AppendLine("     ,TODOKECODE  ")
                SQLStr.AppendLine("     ,TODOKENAME  ")
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,KYORI  ")
                SQLStr.AppendLine("     ,KEIYU  ")
                SQLStr.AppendLine("     ,KIZYUN  ")
                SQLStr.AppendLine("     ,TANKASA  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("     ,USAGECHARGE  ")
                SQLStr.AppendLine("     ,SURCHARGE  ")
                SQLStr.AppendLine("     ,BIKOU1  ")
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
                SQLStr.AppendLine("     ,TAISHOYM  ")
                SQLStr.AppendLine("     ,KYORI  ")
                SQLStr.AppendLine("     ,KEIYU  ")
                SQLStr.AppendLine("     ,KIZYUN  ")
                SQLStr.AppendLine("     ,TANKASA  ")
                SQLStr.AppendLine("     ,KAISU  ")
                SQLStr.AppendLine("     ,USAGECHARGE  ")
                SQLStr.AppendLine("     ,SURCHARGE  ")
                SQLStr.AppendLine("     ,BIKOU1  ")
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
                SQLStr.AppendLine("        LNG.LNM0015_SKSURCHARGE")
                SQLStr.AppendLine("    WHERE")
                SQLStr.AppendLine("       TORICODE  = @TORICODE                ")
                SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                  ")
                SQLStr.AppendLine("   AND TAISHOYM  = @TAISHOYM        ")

                Try
                    Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.Decimal, 6)     '対象年月

                        Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                        Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                        Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                        Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                        Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                        Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                        Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                        Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                        Dim LNM0010row As DataRow = LNM0010INPtbl.Rows(0)

                        ' DB更新
                        P_TORICODE.Value = LNM0010row("TORICODE") '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = LNM0010row("TAISHOYM")           '対象年月

                        '操作区分
                        '変更区分が新規の場合
                        If WW_MODIFYKBN = LNM0010WRKINC.MODIFYKBN.NEWDATA Then
                            P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.NEWDATA).ToString
                        Else
                            '削除データの場合
                            If LNM0010tbl.Rows(0)("DELFLG") = "0" And LNM0010row("DELFLG") = "1" Then
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.DELDATA).ToString
                            Else
                                P_OPERATEKBN.Value = CInt(LNM0010WRKINC.OPERATEKBN.UPDDATA).ToString
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
                    Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0014_SKSURCHARGEHIST INSERT")

                    CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
                    CS0011LOGWrite.INFPOSI = "DB:" + "LNT0014_SKSURCHARGEHIST INSERT"
                    CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                    CS0011LOGWrite.TEXT = ex.ToString()
                    CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                    CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

                    rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                    WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                    Exit Sub
                End Try


#End Region
        End Select
    End Sub
#End Region

    ''' <summary>
    ''' 有効終了日更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    Public Sub UpdateENDYMD(ByVal SQLcon As MySqlConnection, ByVal WW_CONTROLTABLE As String, ByVal WW_ROW As DataRow,
                            ByRef O_MESSAGENO As String, ByVal WW_NOW As String)


        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ更新
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        Select Case WW_CONTROLTABLE
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                SQLStr.Append("     LNG.LNM0010_HACHINOHESPRATE             ")
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                SQLStr.Append("     LNG.LNM0011_ENEOSCOMFEE           ")
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                SQLStr.Append("     LNG.LNM0012_TOHOKUSPRATE          ")
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                SQLStr.Append("     LNG.LNM0014_SKSPRATE          ")
        End Select
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     ENDYMD               = @ENDYMD          ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        Select Case WW_CONTROLTABLE
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                SQLStr.Append("       RECOID  = @RECOID             ")
                SQLStr.Append("   AND TORICODE  = @TORICODE                 ")
                SQLStr.Append("   AND ORGCODE  = @ORGCODE                   ")
                SQLStr.Append("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                SQLStr.Append("       RECOID  = @RECOID             ")
                SQLStr.Append("   AND TORICODE  = @TORICODE                 ")
                SQLStr.Append("   AND ORGCODE  = @ORGCODE                   ")
                SQLStr.Append("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                SQLStr.Append("       TORICODE  = @TORICODE                 ")
                SQLStr.Append("   AND ORGCODE  = @ORGCODE                   ")
                SQLStr.Append("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
                SQLStr.Append("   AND SYABAN  = @SYABAN             ")
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                SQLStr.Append("       RECOID  = @RECOID             ")
                SQLStr.Append("   AND TORICODE  = @TORICODE                 ")
                SQLStr.Append("   AND ORGCODE  = @ORGCODE                   ")
                SQLStr.Append("   AND COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        End Select

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)

                Select Case WW_CONTROLTABLE
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                        P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                        P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                        P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                        P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日
                    Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番

                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                        P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                        P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日
                        P_SYABAN.Value = WW_ROW("SYABAN")           '車番
                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日

                        P_RECOID.Value = WW_ROW("RECOID")           'レコードID
                        P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                        P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                        P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                        P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日
                End Select

                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ


                P_UPDYMD.Value = WW_NOW                '更新年月日
                P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using
        Catch ex As Exception
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007_KOTEIHI UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try
    End Sub


    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '論理削除の場合は入力チェックを省略、削除フラグのみ更新
        If Not DisabledKeyItem.Value = "" And
            work.WF_SEL_DELFLG.Text = C_DELETE_FLG.ALIVE And
            ddlDELFLG.SelectedValue = C_DELETE_FLG.DELETE Then

            ' マスタ更新(削除フラグのみ)
            UpdateMasterDelflgOnly()
            If Not isNormal(WW_ErrSW) Then
                Exit Sub
            End If
            work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = "Update Success!!"
            ' 前ページ遷移
            Master.TransitionPrevPage()
            Exit Sub
        End If

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0010INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0010tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0010tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        ' 右BOXクローズ
        WF_RightboxOpen.Value = ""
        If String.IsNullOrEmpty(WW_ErrSW) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ErrSW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            ElseIf WW_ErrSW = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR Then
                ' 一意制約エラー
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, "ユーザー", needsPopUp:=True)
                ' 右BOXオープン
                WF_RightboxOpen.Value = "Open"
            ElseIf WW_ErrSW = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR Then
                ' 排他エラー
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                ' 右BOXオープン
                WF_RightboxOpen.Value = "Open"
            Else
                ' その他エラー
                Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                ' 右BOXオープン
                WF_RightboxOpen.Value = "Open"
            End If
        End If

        If isNormal(WW_ErrSW) Then
            ' 前ページ遷移
            Master.TransitionPrevPage()
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToLNM0010INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtRECOID.Text)  'レコードID
        Master.EraseCharToIgnore(TxtRECONAME.Text)  'レコード名
        Master.EraseCharToIgnore(TxtTORICODE.Text)  '取引先コード
        Master.EraseCharToIgnore(TxtTORINAME.Text)  '取引先名称
        Master.EraseCharToIgnore(TxtKASANORGCODE.Text)  '加算先部門コード
        Master.EraseCharToIgnore(TxtKASANORGNAME.Text)  '加算先部門名称
        Master.EraseCharToIgnore(TxtTODOKECODE.Text)  '届先コード
        Master.EraseCharToIgnore(TxtTODOKENAME.Text)  '届先名称
        Master.EraseCharToIgnore(WF_StYMD.Value)  '有効開始日
        Master.EraseCharToIgnore(WF_EndYMD.Value)  '有効終了日
        Master.EraseCharToIgnore(TxtKINGAKU.Text)  '金額
        Master.EraseCharToIgnore(TxtSYABAN.Text)  '車番
        Master.EraseCharToIgnore(WF_TAISHOYM.Value)  '対象年月
        Master.EraseCharToIgnore(TxtSYABARA.Text)  '車腹
        Master.EraseCharToIgnore(TxtKOTEIHI.Text)  '固定費
        Master.EraseCharToIgnore(TxtKYORI.Text)  '走行距離
        Master.EraseCharToIgnore(TxtKEIYU.Text)  '実勢軽油価格
        Master.EraseCharToIgnore(TxtKIZYUN.Text)  '基準価格
        Master.EraseCharToIgnore(TxtTANKASA.Text)  '単価差
        Master.EraseCharToIgnore(TxtKAISU.Text)  '輸送回数
        Master.EraseCharToIgnore(TxtCOUNT.Text)  '回数
        Master.EraseCharToIgnore(TxtUSAGECHARGE.Text)  '燃料使用量
        Master.EraseCharToIgnore(TxtSURCHARGE.Text)  'サーチャージ
        Master.EraseCharToIgnore(TxtBIKOU1.Text)  '備考1
        Master.EraseCharToIgnore(TxtBIKOU2.Text)  '備考2
        Master.EraseCharToIgnore(TxtBIKOU3.Text)  '備考3

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) AndAlso
            String.IsNullOrEmpty(ddlDELFLG.SelectedValue) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"                'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(LNM0010INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0010INProw As DataRow = LNM0010INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) Then
            LNM0010INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(TxtSelLineCNT.Text, LNM0010INProw("LINECNT"))
            Catch ex As Exception
                LNM0010INProw("LINECNT") = 0
            End Try
        End If

        LNM0010INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0010INProw("UPDTIMSTP") = 0
        LNM0010INProw("SELECT") = 1
        LNM0010INProw("HIDDEN") = 0

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                LNM0010INProw("TABLEID") = LNM0010WRKINC.TBLHACHINOHESPRATE
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                LNM0010INProw("TABLEID") = LNM0010WRKINC.TBLENEOSCOMFEE
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                LNM0010INProw("TABLEID") = LNM0010WRKINC.TBLTOHOKUSPRATE
            'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                LNM0010INProw("TABLEID") = LNM0010WRKINC.TBLSKSPRATE
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                LNM0010INProw("TABLEID") = LNM0010WRKINC.TBLSKSURCHARGE
        End Select

        LNM0010INProw("DELFLG") = ddlDELFLG.SelectedValue             '削除フラグ

        LNM0010INProw("RECOID") = TxtRECOID.Text            'レコードID
        If TxtRECOID.Text = "" Then
            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                    LNM0010INProw("RECOID") = LNM0010WRKINC.GenerateRECOID(LNM0010WRKINC.TBLHACHINOHESPRATE)
                Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                    LNM0010INProw("RECOID") = LNM0010WRKINC.GenerateRECOID(LNM0010WRKINC.TBLENEOSCOMFEE)
                Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                    LNM0010INProw("RECOID") = LNM0010WRKINC.GenerateRECOID(LNM0010WRKINC.TBLSKSPRATE)
            End Select
        End If

        LNM0010INProw("RECONAME") = TxtRECONAME.Text            'レコード名
        LNM0010INProw("TORICODE") = TxtTORICODE.Text            '取引先コード
        LNM0010INProw("TORINAME") = TxtTORINAME.Text            '取引先名称
        LNM0010INProw("ORGCODE") = ddlSelectORG.SelectedValue           '部門コード
        LNM0010INProw("ORGNAME") = ddlSelectORG.SelectedItem           '部門名称
        LNM0010INProw("KASANORGCODE") = TxtKASANORGCODE.Text            '加算先部門コード
        LNM0010INProw("KASANORGNAME") = TxtKASANORGNAME.Text            '加算先部門名称
        LNM0010INProw("TODOKECODE") = TxtTODOKECODE.Text            '届先コード
        LNM0010INProw("TODOKENAME") = TxtTODOKENAME.Text            '届先名称
        LNM0010INProw("STYMD") = WF_StYMD.Value              '有効開始日
        LNM0010INProw("ENDYMD") = WF_EndYMD.Value            '有効終了日
        LNM0010INProw("KINGAKU") = TxtKINGAKU.Text            '金額
        LNM0010INProw("SYABAN") = TxtSYABAN.Text            '車番
        '対象年月
        If Not WF_TAISHOYM.Value = "" Then
            LNM0010INProw("TAISHOYM") = Replace(WF_TAISHOYM.Value, "/", "")
        Else
            LNM0010INProw("TAISHOYM") = WF_TAISHOYM.Value
        End If
        LNM0010INProw("SYABARA") = TxtSYABARA.Text            '車腹
        LNM0010INProw("KOTEIHI") = TxtKOTEIHI.Text            '固定費
        LNM0010INProw("KYORI") = TxtKYORI.Text            '走行距離
        LNM0010INProw("KEIYU") = TxtKEIYU.Text            '実勢軽油価格
        LNM0010INProw("KIZYUN") = TxtKIZYUN.Text            '基準価格
        LNM0010INProw("TANKASA") = TxtTANKASA.Text            '単価差
        LNM0010INProw("KAISU") = TxtKAISU.Text            '輸送回数
        LNM0010INProw("COUNT") = TxtCOUNT.Text            '回数
        LNM0010INProw("USAGECHARGE") = TxtUSAGECHARGE.Text            '燃料使用量
        LNM0010INProw("SURCHARGE") = TxtSURCHARGE.Text            'サーチャージ
        LNM0010INProw("BIKOU1") = TxtBIKOU1.Text            '備考1
        LNM0010INProw("BIKOU2") = TxtBIKOU2.Text            '備考2
        LNM0010INProw("BIKOU3") = TxtBIKOU3.Text            '備考3

        '○ チェック用テーブルに登録する
        LNM0010INPtbl.Rows.Add(LNM0010INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0010INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0010INProw As DataRow = LNM0010INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0010row As DataRow In LNM0010tbl.Rows
            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                    ' KEY項目が等しい時
                    If LNM0010row("TABLEID") = LNM0010INProw("TABLEID") AndAlso
                        LNM0010row("RECOID") = LNM0010INProw("RECOID") AndAlso
                        LNM0010row("TORICODE") = LNM0010INProw("TORICODE") AndAlso
                        LNM0010row("ORGCODE") = LNM0010INProw("ORGCODE") AndAlso
                        LNM0010row("STYMD") = LNM0010INProw("STYMD") Then
                        ' KEY項目以外の項目の差異をチェック
                        If LNM0010row("DELFLG") = LNM0010INProw("DELFLG") AndAlso
                            LNM0010row("RECONAME") = LNM0010INProw("RECONAME") AndAlso                                'レコード名
                            LNM0010row("TORINAME") = LNM0010INProw("TORINAME") AndAlso                                '取引先名称
                            LNM0010row("ORGNAME") = LNM0010INProw("ORGNAME") AndAlso                                '部門名称
                            LNM0010row("KASANORGCODE") = LNM0010INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                            LNM0010row("KASANORGNAME") = LNM0010INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                            LNM0010row("ENDYMD") = LNM0010INProw("ENDYMD") AndAlso                                '有効終了日
                            LNM0010row("KINGAKU") = LNM0010INProw("KINGAKU") Then                                '金額
                            ' 変更がない時は、入力変更フラグをOFFにする
                            WW_InputChangeFlg = False
                        End If
                        Exit For
                    End If
                Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                    ' KEY項目が等しい時
                    If LNM0010row("TABLEID") = LNM0010INProw("TABLEID") AndAlso
                        LNM0010row("RECOID") = LNM0010INProw("RECOID") AndAlso
                        LNM0010row("TORICODE") = LNM0010INProw("TORICODE") AndAlso
                        LNM0010row("ORGCODE") = LNM0010INProw("ORGCODE") AndAlso
                        LNM0010row("STYMD") = LNM0010INProw("STYMD") Then
                        ' KEY項目以外の項目の差異をチェック
                        If LNM0010row("DELFLG") = LNM0010INProw("DELFLG") AndAlso
                            LNM0010row("RECONAME") = LNM0010INProw("RECONAME") AndAlso                                'レコード名
                            LNM0010row("TORINAME") = LNM0010INProw("TORINAME") AndAlso                                '取引先名称
                            LNM0010row("ORGNAME") = LNM0010INProw("ORGNAME") AndAlso                                '部門名称
                            LNM0010row("KASANORGCODE") = LNM0010INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                            LNM0010row("KASANORGNAME") = LNM0010INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                            LNM0010row("ENDYMD") = LNM0010INProw("ENDYMD") AndAlso                                '有効終了日
                            LNM0010row("KINGAKU") = LNM0010INProw("KINGAKU") Then                                '金額
                            ' 変更がない時は、入力変更フラグをOFFにする
                            WW_InputChangeFlg = False
                        End If

                        Exit For
                    End If
                Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                    ' KEY項目が等しい時
                    If LNM0010row("TABLEID") = LNM0010INProw("TABLEID") AndAlso
                        LNM0010row("TORICODE") = LNM0010INProw("TORICODE") AndAlso
                        LNM0010row("ORGCODE") = LNM0010INProw("ORGCODE") AndAlso
                        LNM0010row("STYMD") = LNM0010INProw("STYMD") AndAlso
                        LNM0010row("SYABAN") = LNM0010INProw("SYABAN") Then
                        ' KEY項目以外の項目の差異をチェック
                        If LNM0010row("DELFLG") = LNM0010INProw("DELFLG") AndAlso
                            LNM0010row("TORINAME") = LNM0010INProw("TORINAME") AndAlso                                '取引先名称
                            LNM0010row("ORGNAME") = LNM0010INProw("ORGNAME") AndAlso                                '部門名称
                            LNM0010row("KASANORGCODE") = LNM0010INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                            LNM0010row("KASANORGNAME") = LNM0010INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                            LNM0010row("ENDYMD") = LNM0010INProw("ENDYMD") AndAlso                                '有効終了日
                            LNM0010row("KOTEIHI") = LNM0010INProw("KOTEIHI") AndAlso                                '固定費
                            LNM0010row("KAISU") = LNM0010INProw("KAISU") Then
                            ' 変更がない時は、入力変更フラグをOFFにする
                            WW_InputChangeFlg = False
                        End If
                        Exit For
                    End If
                'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ

                Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                    ' KEY項目が等しい時
                    If LNM0010row("TABLEID") = LNM0010INProw("TABLEID") AndAlso
                        LNM0010row("RECOID") = LNM0010INProw("RECOID") AndAlso
                        LNM0010row("TORICODE") = LNM0010INProw("TORICODE") AndAlso
                        LNM0010row("ORGCODE") = LNM0010INProw("ORGCODE") AndAlso
                        LNM0010row("STYMD") = LNM0010INProw("STYMD") Then
                        ' KEY項目以外の項目の差異をチェック
                        If LNM0010row("DELFLG") = LNM0010INProw("DELFLG") AndAlso
                            LNM0010row("RECONAME") = LNM0010INProw("RECONAME") AndAlso                                'レコード名
                            LNM0010row("TORINAME") = LNM0010INProw("TORINAME") AndAlso                                '取引先名称
                            LNM0010row("ORGNAME") = LNM0010INProw("ORGNAME") AndAlso                                '部門名称
                            LNM0010row("KASANORGCODE") = LNM0010INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                            LNM0010row("KASANORGNAME") = LNM0010INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                            LNM0010row("TODOKECODE") = LNM0010INProw("TODOKECODE") AndAlso                                '届先コード
                            LNM0010row("TODOKENAME") = LNM0010INProw("TODOKENAME") AndAlso                                '届先名称
                            LNM0010row("ENDYMD") = LNM0010INProw("ENDYMD") AndAlso                                '有効終了日
                            LNM0010row("SYABARA") = LNM0010INProw("SYABARA") AndAlso                                '車腹
                            LNM0010row("KOTEIHI") = LNM0010INProw("KOTEIHI") AndAlso                                '固定費
                            LNM0010row("BIKOU1") = LNM0010INProw("BIKOU1") AndAlso                                '備考1
                            LNM0010row("BIKOU2") = LNM0010INProw("BIKOU2") AndAlso                                '備考2
                            LNM0010row("BIKOU3") = LNM0010INProw("BIKOU3") Then                                '備考3
                            ' 変更がない時は、入力変更フラグをOFFにする
                            WW_InputChangeFlg = False
                        End If
                        Exit For
                    End If
                Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                    ' KEY項目が等しい時
                    If LNM0010row("TABLEID") = LNM0010INProw("TABLEID") AndAlso
                        LNM0010row("TORICODE") = LNM0010INProw("TORICODE") AndAlso
                        LNM0010row("ORGCODE") = LNM0010INProw("ORGCODE") AndAlso
                        LNM0010row("TAISHOYM") = LNM0010INProw("TAISHOYM") Then
                        ' KEY項目以外の項目の差異をチェック
                        If LNM0010row("DELFLG") = LNM0010INProw("DELFLG") AndAlso
                            LNM0010row("TORINAME") = LNM0010INProw("TORINAME") AndAlso                                '取引先名称
                            LNM0010row("ORGNAME") = LNM0010INProw("ORGNAME") AndAlso                                '部門名称
                            LNM0010row("KASANORGCODE") = LNM0010INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                            LNM0010row("KASANORGNAME") = LNM0010INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                            LNM0010row("TODOKECODE") = LNM0010INProw("TODOKECODE") AndAlso                                '届先コード
                            LNM0010row("TODOKENAME") = LNM0010INProw("TODOKENAME") AndAlso                                '届先名称
                            LNM0010row("KYORI") = LNM0010INProw("KYORI") AndAlso                                '走行距離
                            LNM0010row("KEIYU") = LNM0010INProw("KEIYU") AndAlso                                '実勢軽油価格
                            LNM0010row("KIZYUN") = LNM0010INProw("KIZYUN") AndAlso                                '基準価格
                            LNM0010row("TANKASA") = LNM0010INProw("TANKASA") AndAlso                                '単価差
                            LNM0010row("KAISU") = LNM0010INProw("KAISU") AndAlso                                '輸送回数
                            LNM0010row("USAGECHARGE") = LNM0010INProw("USAGECHARGE") AndAlso                                '燃料使用量
                            LNM0010row("SURCHARGE") = LNM0010INProw("SURCHARGE") AndAlso                                'サーチャージ
                            LNM0010row("BIKOU1") = LNM0010INProw("BIKOU1") Then                                '備考1
                            ' 変更がない時は、入力変更フラグをOFFにする
                            WW_InputChangeFlg = False
                        End If

                        Exit For
                    End If
            End Select
        Next

        If WW_InputChangeFlg Then
            ' 変更がある場合は、確認ダイアログを表示
            Master.Output(C_MESSAGE_NO.UPDATE_CANCEL_CONFIRM, C_MESSAGE_TYPE.QUES, I_PARA02:="W",
                needsPopUp:=True, messageBoxTitle:="確認", IsConfirm:=True, YesButtonId:="btnClearConfirmOK")
        Else
            ' 変更がない場合は、確認ダイアログを表示せずに、前画面に戻る
            WF_CLEAR_ConfirmOkClick()
        End If

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

        '○ 状態をクリア
        For Each LNM0010row As DataRow In LNM0010tbl.Rows
            Select Case LNM0010row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0010tbl, work.WF_SEL_INPTBL.Text)

        TxtSelLineCNT.Text = ""              'LINECNT
        TxtMapId.Text = "M00001"             '画面ＩＤ
        ddlDELFLG.SelectedValue = ""                  '削除フラグ
        TxtRECOID.Text = ""                    'レコードID
        TxtRECONAME.Text = ""                    'レコード名
        TxtTORICODE.Text = ""                    '取引先コード
        TxtTORINAME.Text = ""                    '取引先名称
        TxtKASANORGCODE.Text = ""                    '加算先部門コード
        TxtKASANORGNAME.Text = ""                    '加算先部門名称
        TxtTODOKECODE.Text = ""                    '届先コード
        TxtTODOKENAME.Text = ""                    '届先名称
        WF_StYMD.Value = ""                  '有効開始日
        WF_EndYMD.Value = ""                 '有効終了日
        TxtKINGAKU.Text = ""                    '金額
        TxtSYABAN.Text = ""                    '車番
        WF_TAISHOYM.Value = ""                    '対象年月
        TxtSYABARA.Text = ""                    '車腹
        TxtKOTEIHI.Text = ""                    '固定費
        TxtKYORI.Text = ""                    '走行距離
        TxtKEIYU.Text = ""                    '実勢軽油価格
        TxtKIZYUN.Text = ""                    '基準価格
        TxtTANKASA.Text = ""                    '単価差
        TxtKAISU.Text = ""                    '輸送回数
        TxtCOUNT.Text = ""                    '回数
        TxtUSAGECHARGE.Text = ""                    '燃料使用量
        TxtSURCHARGE.Text = ""                    'サーチャージ
        TxtBIKOU1.Text = ""                    '備考1
        TxtBIKOU2.Text = ""                    '備考2
        TxtBIKOU3.Text = ""                    '備考3

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
                    Case "TxtDelFlg"
                        leftview.Visible = True
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                    Case "TxtTORICODE"       '取引先コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspToriCodeSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    Case "TxtKASANORGCODE"       '加算先部門コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspKasanOrgCodeSingle()
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
            'Case "TxtDelFlg"      '削除フラグ
            '    CODENAME_get("DELFLG", ddlDELFLG.SelectedValue, LblDelFlgName.Text, WW_Dummy)
            '    TxtDelFlg.Focus()
            Case "TxtTORICODE"
                CODENAME_get("TORICODE", TxtTORICODE.Text, TxtTORINAME.Text, WW_RtnSW)  '取引先コード
                TxtTORICODE.Focus()
            Case "TxtKASANORGCODE"
                CODENAME_get("KASANORGCODE", TxtKASANORGCODE.Text, TxtKASANORGNAME.Text, WW_RtnSW)  '加算先部門コード
                TxtKASANORGCODE.Focus()
            Case "TxtTODOKECODE"
                CODENAME_get("TODOKECODE", TxtTODOKECODE.Text, TxtTODOKENAME.Text, WW_RtnSW)  '届先コード
                TxtTODOKECODE.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 特別料金マスタ更新(削除フラグのみ)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UpdateMasterDelflgOnly()
        Dim WW_MODIFYKBN As String = ""
        Dim WW_DATE As Date = Date.Now

        '初期化
        LNM0010INPtbl = New DataTable
        LNM0010INPtbl.Columns.Add("RECOID")
        LNM0010INPtbl.Columns.Add("TABLEID")
        LNM0010INPtbl.Columns.Add("TORICODE")
        LNM0010INPtbl.Columns.Add("ORGCODE")
        LNM0010INPtbl.Columns.Add("STYMD")
        LNM0010INPtbl.Columns.Add("TAISHOYM")
        LNM0010INPtbl.Columns.Add("SYABAN")
        LNM0010INPtbl.Columns.Add("DELFLG")

        Dim row As DataRow
        row = LNM0010INPtbl.NewRow
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                row("TABLEID") = LNM0010WRKINC.TBLHACHINOHESPRATE
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                row("TABLEID") = LNM0010WRKINC.TBLENEOSCOMFEE
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                row("TABLEID") = LNM0010WRKINC.TBLTOHOKUSPRATE
            'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                row("TABLEID") = LNM0010WRKINC.TBLSKSPRATE
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                row("TABLEID") = LNM0010WRKINC.TBLSKSURCHARGE
        End Select

        row("RECOID") = TxtRECOID.Text
        row("TORICODE") = TxtTORICODE.Text
        row("ORGCODE") = ddlSelectORG.SelectedValue
        row("STYMD") = WF_StYMD.Value
        row("TAISHOYM") = WF_TAISHOYM.Value
        row("SYABAN") = TxtSYABAN.Text
        row("DELFLG") = C_DELETE_FLG.DELETE
        LNM0010INPtbl.Rows.Add(row)

        ' DB更新処理
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()

            '履歴テーブルに変更前データを登録
            InsertHist(SQLcon, LNM0010WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '削除フラグ更新
            SetDelflg(SQLcon, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '履歴テーブルに変更後データを登録
            InsertHist(SQLcon, LNM0010WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

        End Using

        '○ 入力値反映
        For Each LNM0010INProw As DataRow In LNM0010INPtbl.Rows
            For Each LNM0010row As DataRow In LNM0010tbl.Rows
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        If LNM0010INProw("TABLEID") = LNM0010row("TABLEID") AndAlso
                            LNM0010INProw("RECOID") = LNM0010row("RECOID") AndAlso
                            LNM0010INProw("TORICODE") = LNM0010row("TORICODE") AndAlso
                            LNM0010INProw("ORGCODE") = LNM0010row("ORGCODE") AndAlso
                            LNM0010INProw("STYMD") = LNM0010row("STYMD") Then
                            ' 画面入力テーブル項目設定              
                            LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            LNM0010row("DELFLG") = LNM0010INProw("DELFLG")
                            LNM0010row("SELECT") = 0
                            LNM0010row("HIDDEN") = 0
                            Exit For
                        End If
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        If LNM0010INProw("TABLEID") = LNM0010row("TABLEID") AndAlso
                            LNM0010INProw("RECOID") = LNM0010row("RECOID") AndAlso
                            LNM0010INProw("TORICODE") = LNM0010row("TORICODE") AndAlso
                            LNM0010INProw("ORGCODE") = LNM0010row("ORGCODE") AndAlso
                            LNM0010INProw("STYMD") = LNM0010row("STYMD") Then
                            ' 画面入力テーブル項目設定              
                            LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            LNM0010row("DELFLG") = LNM0010INProw("DELFLG")
                            LNM0010row("SELECT") = 0
                            LNM0010row("HIDDEN") = 0
                            Exit For
                        End If
                    Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                        If LNM0010INProw("TABLEID") = LNM0010row("TABLEID") AndAlso
                            LNM0010INProw("TORICODE") = LNM0010row("TORICODE") AndAlso
                            LNM0010INProw("ORGCODE") = LNM0010row("ORGCODE") AndAlso
                            LNM0010INProw("STYMD") = LNM0010row("STYMD") AndAlso
                            LNM0010INProw("SYABAN") = LNM0010row("SYABAN") Then
                            ' 画面入力テーブル項目設定              
                            LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            LNM0010row("DELFLG") = LNM0010INProw("DELFLG")
                            LNM0010row("SELECT") = 0
                            LNM0010row("HIDDEN") = 0
                            Exit For
                        End If
                    'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ

                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        If LNM0010INProw("TABLEID") = LNM0010row("TABLEID") AndAlso
                            LNM0010INProw("RECOID") = LNM0010row("RECOID") AndAlso
                            LNM0010INProw("TORICODE") = LNM0010row("TORICODE") AndAlso
                            LNM0010INProw("ORGCODE") = LNM0010row("ORGCODE") AndAlso
                            LNM0010INProw("STYMD") = LNM0010row("STYMD") Then
                            ' 画面入力テーブル項目設定              
                            LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            LNM0010row("DELFLG") = LNM0010INProw("DELFLG")
                            LNM0010row("SELECT") = 0
                            LNM0010row("HIDDEN") = 0
                            Exit For
                        End If
                    Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                        If LNM0010INProw("TABLEID") = LNM0010row("TABLEID") AndAlso
                            LNM0010INProw("TORICODE") = LNM0010row("TORICODE") AndAlso
                            LNM0010INProw("ORGCODE") = LNM0010row("ORGCODE") AndAlso
                            LNM0010INProw("TAISHOYM") = LNM0010row("TAISHOYM") Then
                            ' 画面入力テーブル項目設定              
                            LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            LNM0010row("DELFLG") = LNM0010INProw("DELFLG")
                            LNM0010row("SELECT") = 0
                            LNM0010row("HIDDEN") = 0
                            Exit For
                        End If
                End Select
            Next
        Next

    End Sub

    ''' <summary>
    ''' 削除フラグ更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_NOW"></param>
    ''' <remarks></remarks>
    Public Sub SetDelflg(ByVal SQLcon As MySqlConnection, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ更新
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                SQLStr.AppendLine("     LNG.LNM0010_HACHINOHESPRATE           ")
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                SQLStr.AppendLine("     LNG.LNM0011_ENEOSCOMFEE           ")
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                SQLStr.AppendLine("     LNG.LNM0012_TOHOKUSPRATE           ")
            'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                SQLStr.AppendLine("     LNG.LNM0014_SKSPRATE           ")
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                SQLStr.AppendLine("     LNG.LNM0015_SKSURCHARGE           ")
        End Select
        ' テーブル共通
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")

        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y%m%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y%m%d'), '') ")
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y%m%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y%m%d'), '') ")
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y%m%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y%m%d'), '') ")
                SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
            'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ

            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                SQLStr.AppendLine("         COALESCE(RECOID, '')             = @RECOID ")
                SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y%m%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y%m%d'), '') ")
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
                SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
                SQLStr.AppendLine("    AND  COALESCE(TAISHOYM, '')             = @TAISHOYM ")
        End Select

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                ' テーブル共通
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_UPDYMD.Value = WW_NOW                '更新年月日
                P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                ' テーブル別項目
                Dim LNM0010row As DataRow = LNM0010INPtbl.Rows(0)
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日
                    Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                        Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番

                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日
                        P_SYABAN.Value = LNM0010row("SYABAN")           '車番

                    'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ

                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_STYMD.Value = LNM0010row("STYMD")           '有効開始日
                    Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                        Dim P_RECOID As MySqlParameter = SQLcmd.Parameters.Add("@RECOID", MySqlDbType.VarChar, 10)     'レコードID
                        Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                        Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                        Dim P_TAISHOYM As MySqlParameter = SQLcmd.Parameters.Add("@TAISHOYM", MySqlDbType.VarChar, 6)     '対象年月

                        P_RECOID.Value = LNM0010row("RECOID")           'レコードID
                        P_TORICODE.Value = LNM0010row("TORICODE")           '取引先コード
                        P_ORGCODE.Value = LNM0010row("ORGCODE")           '部門コード
                        P_TAISHOYM.Value = LNM0010row("TAISHOYM")           '対象年月
                End Select

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0010D UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SELectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SELectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SELectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        If String.IsNullOrEmpty(WF_FIELD_REP.Value) Then
            Select Case WF_FIELD.Value
                'Case "TxtDelFlg"      '削除フラグ
                '    ddlDELFLG.SelectedValue = WW_SelectValue
                '    LblDelFlgName.Text = WW_SelectText
                '    TxtDelFlg.Focus()
            End Select
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        If String.IsNullOrEmpty(WF_FIELD_REP.Value) Then
            Select Case WF_FIELD.Value
                'Case "TxtDelFlg"            '削除フラグ
                '    TxtDelFlg.Focus()
            End Select
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' 取引先コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspToriCodeSingle()

        Me.mspToriCodeSingle.InitPopUp()
        Me.mspToriCodeSingle.SelectionMode = ListSelectionMode.Single

        Dim WW_TABLEID As String = ""
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                WW_TABLEID = LNM0010WRKINC.TBLHACHINOHESPRATE
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                WW_TABLEID = LNM0010WRKINC.TBLENEOSCOMFEE
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                WW_TABLEID = LNM0010WRKINC.TBLTOHOKUSPRATE
            'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ

            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                WW_TABLEID = LNM0010WRKINC.TBLSKSPRATE
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                WW_TABLEID = LNM0010WRKINC.TBLSKSURCHARGE
        End Select

        Me.mspToriCodeSingle.SQL = CmnSearchSQL.GetSprateToriSQL(WW_TABLEID, ddlSelectORG.SelectedValue)

        Me.mspToriCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspToriCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetSprateToriTitle)

        Me.mspToriCodeSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 取引先コード選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspToriCodeSingle()

        Dim selData = Me.mspToriCodeSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtTORICODE.ID
                Me.TxtTORICODE.Text = selData("TORICODE").ToString '取引先コード
                Me.TxtTORINAME.Text = selData("TORINAME").ToString '取引先名
                Me.TxtTORICODE.Focus()
        End Select

        'ポップアップの非表示
        Me.mspToriCodeSingle.HidePopUp()

    End Sub


    ''' <summary>
    ''' 加算先部門コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspKasanOrgCodeSingle()

        Me.mspKasanOrgCodeSingle.InitPopUp()
        Me.mspKasanOrgCodeSingle.SelectionMode = ListSelectionMode.Single

        Dim WW_TABLEID As String = ""
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                WW_TABLEID = LNM0010WRKINC.TBLHACHINOHESPRATE
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                WW_TABLEID = LNM0010WRKINC.TBLENEOSCOMFEE
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                WW_TABLEID = LNM0010WRKINC.TBLTOHOKUSPRATE
            'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ

            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                WW_TABLEID = LNM0010WRKINC.TBLSKSPRATE
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                WW_TABLEID = LNM0010WRKINC.TBLSKSURCHARGE
        End Select

        Me.mspKasanOrgCodeSingle.SQL = CmnSearchSQL.GetSprateKasanOrgSQL(WW_TABLEID, ddlSelectORG.SelectedValue)

        Me.mspKasanOrgCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspKasanOrgCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetSprateKasanOrgTitle)

        Me.mspKasanOrgCodeSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 加算先部門選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspKASANORGCodeSingle()

        Dim selData = Me.mspKasanOrgCodeSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtKASANORGCODE.ID
                Me.TxtKASANORGCODE.Text = selData("KASANORGCODE").ToString '加算先部門コード
                Me.TxtKASANORGNAME.Text = selData("KASANORGNAME").ToString '加算先部門名
                Me.TxtKASANORGCODE.Focus()
        End Select

        'ポップアップの非表示
        Me.mspKasanOrgCodeSingle.HidePopUp()

    End Sub

    ''' <summary>
    ''' 届先コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspTodokeCodeSingle()

        Me.mspTodokeCodeSingle.InitPopUp()
        Me.mspTodokeCodeSingle.SelectionMode = ListSelectionMode.Single

        Dim WW_TABLEID As String = ""
        Select Case work.WF_SEL_CONTROLTABLE.Text
            Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                WW_TABLEID = LNM0010WRKINC.TBLHACHINOHESPRATE
            Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                WW_TABLEID = LNM0010WRKINC.TBLENEOSCOMFEE
            Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                WW_TABLEID = LNM0010WRKINC.TBLTOHOKUSPRATE
            'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ

            Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                WW_TABLEID = LNM0010WRKINC.TBLSKSPRATE
            Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                WW_TABLEID = LNM0010WRKINC.TBLSKSURCHARGE
        End Select

        Me.mspTodokeCodeSingle.SQL = CmnSearchSQL.GetSprateTodokeSQL(WW_TABLEID, ddlSelectORG.SelectedValue)

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
                Me.TxtTODOKENAME.Text = selData("TODOKENAME").ToString '届先名
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
    Protected Sub INPTableCheck(ByRef O_RTN As String)
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
        For Each LNM0010INProw As DataRow In LNM0010INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0010INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0010INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLHA,'八戸特別料金マスタ
                     LNM0010WRKINC.MAPIDLEN, 'ENEOS業務委託料マスタ
                    LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ

                    '' レコードID(バリデーションチェック)
                    'Master.CheckField(Master.USERCAMP, "RECOID", LNM0010INProw("RECOID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    'If Not isNormal(WW_CS0024FCheckerr) Then
                    '    WW_CheckMES1 = "・レコードIDエラーです。"
                    '    WW_CheckMES2 = WW_CS0024FCheckReport
                    '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    '    WW_LineErr = "ERR"
                    '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    'End If
                    ' レコード名(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "RECONAME", LNM0010INProw("RECONAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・レコード名エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
            End Select

            ' 取引先コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORICODE", LNM0010INProw("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 取引先名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORINAME", LNM0010INProw("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 部門コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ORGCODE", LNM0010INProw("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・部門コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 部門名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ORGNAME", LNM0010INProw("ORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・部門名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 加算先部門コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KASANORGCODE", LNM0010INProw("KASANORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・加算先部門コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 加算先部門名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KASANORGNAME", LNM0010INProw("KASANORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・加算先部門名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLSKSP, 'SK特別料金マスタ
                     LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                    ' 届先コード(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "TODOKECODE", LNM0010INProw("TODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・届先コードエラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                    ' 届先名称(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "TODOKENAME", LNM0010INProw("TODOKENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・届先名称エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
            End Select

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLHA,'八戸特別料金マスタ
                     LNM0010WRKINC.MAPIDLEN, 'ENEOS業務委託料マスタ
                     LNM0010WRKINC.MAPIDLTO, '東北電力車両別追加料金マスタ
                     LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                    ' 有効開始日(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "STYMD", LNM0010INProw("STYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・有効開始日エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If

                    '画面で入力済みの場合のみ
                    If Not WF_EndYMD.Value = "" Then
                        ' 有効終了日(バリデーションチェック)
                        Master.CheckField(Master.USERCAMP, "ENDYMD", LNM0010INProw("ENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                        If Not isNormal(WW_CS0024FCheckerr) Then
                            WW_CheckMES1 = "・有効終了日エラーです。"
                            WW_CheckMES2 = WW_CS0024FCheckReport
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
            End Select

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLHA,'八戸特別料金マスタ
                     LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ

                    ' 金額(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "KINGAKU", LNM0010INProw("KINGAKU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・金額エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
            End Select

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                    ' 車番(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "SYABAN", LNM0010INProw("SYABAN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・車番エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
            End Select

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                    ' 対象年月(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "TAISHOYM", LNM0010INProw("TAISHOYM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・対象年月エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
            End Select

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                    ' 車腹(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "SYABARA", LNM0010INProw("SYABARA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・車腹エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
            End Select

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLTO, '東北電力車両別追加料金マスタ
                     LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                    ' 固定費(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "KOTEIHI", LNM0010INProw("KOTEIHI"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・固定費エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
            End Select

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                    ' 走行距離(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "KYORI", LNM0010INProw("KYORI"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・走行距離エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                    ' 実勢軽油価格(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "KEIYU", LNM0010INProw("KEIYU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・実勢軽油価格エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                    ' 基準価格(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "KIZYUN", LNM0010INProw("KIZYUN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・基準価格エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                    ' 単価差(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "TANKASA", LNM0010INProw("TANKASA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・単価差エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                    ' 輸送回数(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "KAISU", LNM0010INProw("KAISU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・輸送回数エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
            End Select


            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                    ' 回数(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "KAISU", LNM0010INProw("KAISU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・回数エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
            End Select

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                    ' 燃料使用量(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "USAGECHARGE", LNM0010INProw("USAGECHARGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・燃料使用量エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                    ' サーチャージ(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "SURCHARGE", LNM0010INProw("SURCHARGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・サーチャージエラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
            End Select

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLSKSP, 'SK特別料金マスタ
                     LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                    ' 備考1(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "BIKOU1", LNM0010INProw("BIKOU1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・備考1エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
            End Select

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                    ' 備考2(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "BIKOU2", LNM0010INProw("BIKOU2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・備考2エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                    ' 備考3(バリデーションチェック)
                    Master.CheckField(Master.USERCAMP, "BIKOU3", LNM0010INProw("BIKOU3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                    If Not isNormal(WW_CS0024FCheckerr) Then
                        WW_CheckMES1 = "・備考3エラーです。"
                        WW_CheckMES2 = WW_CS0024FCheckReport
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
            End Select

            Select Case work.WF_SEL_CONTROLTABLE.Text
                Case LNM0010WRKINC.MAPIDLHA,'八戸特別料金マスタ
                     LNM0010WRKINC.MAPIDLEN, 'ENEOS業務委託料マスタ
                     LNM0010WRKINC.MAPIDLTO, '東北電力車両別追加料金マスタ
                     LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ

                    '画面で入力済みの場合のみ
                    If Not WF_EndYMD.Value = "" Then
                        ' 日付大小チェック
                        If Not String.IsNullOrEmpty(LNM0010INProw("STYMD")) AndAlso
                                Not String.IsNullOrEmpty(LNM0010INProw("ENDYMD")) Then
                            If CDate(LNM0010INProw("STYMD")) > CDate(LNM0010INProw("ENDYMD")) Then
                                WW_CheckMES1 = "・有効開始日＆有効終了日エラーです。"
                                WW_CheckMES2 = "日付大小入力エラー"
                                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                                WW_LineErr = "ERR"
                                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                            End If
                        End If
                    End If
            End Select

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_SYABAN.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()

                    Select Case work.WF_SEL_CONTROLTABLE.Text
                        Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                            work.HaitaCheckHACHINOHESPRATE(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_RECOID.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_STYMD.Text)
                        Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                            work.HaitaCheckENEOSCOMFEE(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_RECOID.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_STYMD.Text)
                        Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                            work.HaitaCheckTOHOKUSPRATE(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_STYMD.Text,
                    work.WF_SEL_SYABAN.Text)
                        Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                            work.HaitaCheckSKSPRATE(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_RECOID.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_STYMD.Text)
                        Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                            work.HaitaCheckSKSURCHARGE(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                    work.WF_SEL_TAISHOYM.Text)
                    End Select
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    Select Case work.WF_SEL_CONTROLTABLE.Text
                        Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                            WW_CheckMES1 = "・排他エラー（レコードID & 取引先コード & 部門コード & 有効開始日）"
                            WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0010INProw("RECOID") & "]" &
                                           "([" & LNM0010INProw("TORICODE") & "]" &
                                           "([" & LNM0010INProw("ORGCODE") & "]" &
                                           " [" & LNM0010INProw("STYMD") & "])"
                        Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                            WW_CheckMES1 = "・排他エラー（レコードID & 取引先コード & 部門コード & 有効開始日）"
                            WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0010INProw("RECOID") & "]" &
                                           "([" & LNM0010INProw("TORICODE") & "]" &
                                           "([" & LNM0010INProw("ORGCODE") & "]" &
                                           " [" & LNM0010INProw("STYMD") & "])"
                        Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                            WW_CheckMES1 = "・排他エラー（取引先コード & 部門コード & 有効開始日 & 車番）"
                            WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0010INProw("TORICODE") & "]" &
                                           "([" & LNM0010INProw("ORGCODE") & "]" &
                                           "([" & LNM0010INProw("STYMD") & "]" &
                                           " [" & LNM0010INProw("SYABAN") & "])"
                        'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                        Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                            WW_CheckMES1 = "・排他エラー（レコードID & 取引先コード & 部門コード & 有効開始日）"
                            WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0010INProw("RECOID") & "]" &
                                           "([" & LNM0010INProw("TORICODE") & "]" &
                                           "([" & LNM0010INProw("ORGCODE") & "]" &
                                           " [" & LNM0010INProw("STYMD") & "])"
                        Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                            WW_CheckMES1 = "・排他エラー（取引先コード & 部門コード & 対象年月）"
                            WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0010INProw("TORICODE") & "]" &
                                           "([" & LNM0010INProw("ORGCODE") & "]" &
                                           " [" & LNM0010INProw("TAISHOYM") & "])"
                    End Select

                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If

            If String.IsNullOrEmpty(WW_LineErr) Then
                If LNM0010INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0010INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0010INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0010INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

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
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0010row As DataRow In LNM0010tbl.Rows
            Select Case LNM0010row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0010row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0010INProw As DataRow In LNM0010INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0010INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0010INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0010row As DataRow In LNM0010tbl.Rows
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        ' KEY項目が等しい時
                        If LNM0010row("TABLEID") = LNM0010INProw("TABLEID") AndAlso
                            LNM0010row("RECOID") = LNM0010INProw("RECOID") AndAlso
                            LNM0010row("TORICODE") = LNM0010INProw("TORICODE") AndAlso
                            LNM0010row("ORGCODE") = LNM0010INProw("ORGCODE") AndAlso
                            LNM0010row("STYMD") = LNM0010INProw("STYMD") Then
                            ' KEY項目以外の項目の差異をチェック
                            If LNM0010row("DELFLG") = LNM0010INProw("DELFLG") AndAlso
                                LNM0010row("RECONAME") = LNM0010INProw("RECONAME") AndAlso                                'レコード名
                                LNM0010row("TORINAME") = LNM0010INProw("TORINAME") AndAlso                                '取引先名称
                                LNM0010row("ORGNAME") = LNM0010INProw("ORGNAME") AndAlso                                '部門名称
                                LNM0010row("KASANORGCODE") = LNM0010INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                                LNM0010row("KASANORGNAME") = LNM0010INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                                LNM0010row("ENDYMD") = LNM0010INProw("ENDYMD") AndAlso                                '有効終了日
                                LNM0010row("KINGAKU") = LNM0010INProw("KINGAKU") AndAlso                                '金額
                                Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0010row("OPERATION")) Then

                                ' 変更がない時は「操作」の項目は空白にする
                                LNM0010INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            Else
                                ' 変更がある時は「操作」の項目を「更新」に設定する
                                LNM0010INProw("OPERATION") = CONST_UPDATE
                            End If

                            Exit For
                        End If
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        ' KEY項目が等しい時
                        If LNM0010row("TABLEID") = LNM0010INProw("TABLEID") AndAlso
                            LNM0010row("RECOID") = LNM0010INProw("RECOID") AndAlso
                            LNM0010row("TORICODE") = LNM0010INProw("TORICODE") AndAlso
                            LNM0010row("ORGCODE") = LNM0010INProw("ORGCODE") AndAlso
                            LNM0010row("STYMD") = LNM0010INProw("STYMD") Then
                            ' KEY項目以外の項目の差異をチェック
                            If LNM0010row("DELFLG") = LNM0010INProw("DELFLG") AndAlso
                                LNM0010row("RECONAME") = LNM0010INProw("RECONAME") AndAlso                                'レコード名
                                LNM0010row("TORINAME") = LNM0010INProw("TORINAME") AndAlso                                '取引先名称
                                LNM0010row("ORGNAME") = LNM0010INProw("ORGNAME") AndAlso                                '部門名称
                                LNM0010row("KASANORGCODE") = LNM0010INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                                LNM0010row("KASANORGNAME") = LNM0010INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                                LNM0010row("ENDYMD") = LNM0010INProw("ENDYMD") AndAlso                                '有効終了日
                                LNM0010row("KINGAKU") = LNM0010INProw("KINGAKU") AndAlso                                '金額
                                Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0010row("OPERATION")) Then

                                ' 変更がない時は「操作」の項目は空白にする
                                LNM0010INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            Else
                                ' 変更がある時は「操作」の項目を「更新」に設定する
                                LNM0010INProw("OPERATION") = CONST_UPDATE
                            End If

                            Exit For
                        End If
                    Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                        ' KEY項目が等しい時
                        If LNM0010row("TABLEID") = LNM0010INProw("TABLEID") AndAlso
                            LNM0010row("TORICODE") = LNM0010INProw("TORICODE") AndAlso
                            LNM0010row("ORGCODE") = LNM0010INProw("ORGCODE") AndAlso
                            LNM0010row("STYMD") = LNM0010INProw("STYMD") AndAlso
                            LNM0010row("SYABAN") = LNM0010INProw("SYABAN") Then
                            ' KEY項目以外の項目の差異をチェック
                            If LNM0010row("DELFLG") = LNM0010INProw("DELFLG") AndAlso
                                LNM0010row("TORINAME") = LNM0010INProw("TORINAME") AndAlso                                '取引先名称
                                LNM0010row("ORGNAME") = LNM0010INProw("ORGNAME") AndAlso                                '部門名称
                                LNM0010row("KASANORGCODE") = LNM0010INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                                LNM0010row("KASANORGNAME") = LNM0010INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                                LNM0010row("ENDYMD") = LNM0010INProw("ENDYMD") AndAlso                                '有効終了日
                                LNM0010row("KOTEIHI") = LNM0010INProw("KOTEIHI") AndAlso                                '固定費
                                LNM0010row("KAISU") = LNM0010INProw("KAISU") AndAlso
                                Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0010row("OPERATION")) Then

                                ' 変更がない時は「操作」の項目は空白にする
                                LNM0010INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            Else
                                ' 変更がある時は「操作」の項目を「更新」に設定する
                                LNM0010INProw("OPERATION") = CONST_UPDATE
                            End If

                            Exit For
                        End If
                    'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        ' KEY項目が等しい時
                        If LNM0010row("TABLEID") = LNM0010INProw("TABLEID") AndAlso
                            LNM0010row("RECOID") = LNM0010INProw("RECOID") AndAlso
                            LNM0010row("TORICODE") = LNM0010INProw("TORICODE") AndAlso
                            LNM0010row("ORGCODE") = LNM0010INProw("ORGCODE") AndAlso
                            LNM0010row("STYMD") = LNM0010INProw("STYMD") Then
                            ' KEY項目以外の項目の差異をチェック
                            If LNM0010row("DELFLG") = LNM0010INProw("DELFLG") AndAlso
                                LNM0010row("RECONAME") = LNM0010INProw("RECONAME") AndAlso                                'レコード名
                                LNM0010row("TORINAME") = LNM0010INProw("TORINAME") AndAlso                                '取引先名称
                                LNM0010row("ORGNAME") = LNM0010INProw("ORGNAME") AndAlso                                '部門名称
                                LNM0010row("KASANORGCODE") = LNM0010INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                                LNM0010row("KASANORGNAME") = LNM0010INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                                LNM0010row("TODOKECODE") = LNM0010INProw("TODOKECODE") AndAlso                                '届先コード
                                LNM0010row("TODOKENAME") = LNM0010INProw("TODOKENAME") AndAlso                                '届先名称
                                LNM0010row("ENDYMD") = LNM0010INProw("ENDYMD") AndAlso                                '有効終了日
                                LNM0010row("SYABARA") = LNM0010INProw("SYABARA") AndAlso                                '車腹
                                LNM0010row("KOTEIHI") = LNM0010INProw("KOTEIHI") AndAlso                                '固定費
                                LNM0010row("BIKOU1") = LNM0010INProw("BIKOU1") AndAlso                                '備考1
                                LNM0010row("BIKOU2") = LNM0010INProw("BIKOU2") AndAlso                                '備考2
                                LNM0010row("BIKOU3") = LNM0010INProw("BIKOU3") AndAlso                                '備考3
                                Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0010row("OPERATION")) Then

                                ' 変更がない時は「操作」の項目は空白にする
                                LNM0010INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            Else
                                ' 変更がある時は「操作」の項目を「更新」に設定する
                                LNM0010INProw("OPERATION") = CONST_UPDATE
                            End If

                            Exit For
                        End If
                    Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                        ' KEY項目が等しい時
                        If LNM0010row("TABLEID") = LNM0010INProw("TABLEID") AndAlso
                            LNM0010row("TORICODE") = LNM0010INProw("TORICODE") AndAlso
                            LNM0010row("ORGCODE") = LNM0010INProw("ORGCODE") AndAlso
                            LNM0010row("TAISHOYM") = LNM0010INProw("TAISHOYM") Then
                            ' KEY項目以外の項目の差異をチェック
                            If LNM0010row("DELFLG") = LNM0010INProw("DELFLG") AndAlso
                                LNM0010row("TORINAME") = LNM0010INProw("TORINAME") AndAlso                                '取引先名称
                                LNM0010row("ORGNAME") = LNM0010INProw("ORGNAME") AndAlso                                '部門名称
                                LNM0010row("KASANORGCODE") = LNM0010INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                                LNM0010row("KASANORGNAME") = LNM0010INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                                LNM0010row("TODOKECODE") = LNM0010INProw("TODOKECODE") AndAlso                                '届先コード
                                LNM0010row("TODOKENAME") = LNM0010INProw("TODOKENAME") AndAlso                                '届先名称
                                LNM0010row("KYORI") = LNM0010INProw("KYORI") AndAlso                                '走行距離
                                LNM0010row("KEIYU") = LNM0010INProw("KEIYU") AndAlso                                '実勢軽油価格
                                LNM0010row("KIZYUN") = LNM0010INProw("KIZYUN") AndAlso                                '基準価格
                                LNM0010row("TANKASA") = LNM0010INProw("TANKASA") AndAlso                                '単価差
                                LNM0010row("KAISU") = LNM0010INProw("KAISU") AndAlso                                '輸送回数
                                LNM0010row("USAGECHARGE") = LNM0010INProw("USAGECHARGE") AndAlso                                '燃料使用量
                                LNM0010row("SURCHARGE") = LNM0010INProw("SURCHARGE") AndAlso                                'サーチャージ
                                LNM0010row("BIKOU1") = LNM0010INProw("BIKOU1") AndAlso                                '備考1
                                Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0010row("OPERATION")) Then

                                ' 変更がない時は「操作」の項目は空白にする
                                LNM0010INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                            Else
                                ' 変更がある時は「操作」の項目を「更新」に設定する
                                LNM0010INProw("OPERATION") = CONST_UPDATE
                            End If

                            Exit For
                        End If
                End Select
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0010INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0010INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0010INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now
                Dim WW_DBDataCheck As String = ""
                Dim WW_BeforeMAXSTYMD As String = ""
                Dim WW_STYMD_SAVE As String = ""
                Dim WW_ENDYMD_SAVE As String = ""

#Region "八戸特別料金マスタ、ENEOS業務委託料マスタ、東北電力車両別追加料金マスタ、SK特別料金マスタ"
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA, LNM0010WRKINC.MAPIDLEN, LNM0010WRKINC.MAPIDLTO,
                         LNM0010WRKINC.MAPIDLSKSP

                        '更新前の最大有効開始日取得
                        WW_BeforeMAXSTYMD = LNM0010WRKINC.GetSTYMD(SQLcon, work.WF_SEL_CONTROLTABLE.Text,
                                                                   LNM0010INPtbl.Rows(0), WW_DBDataCheck)
                        If Not isNormal(WW_DBDataCheck) Then
                            Exit Sub
                        End If

                        WF_AUTOENDYMD.Value = ""

                        Select Case True
                        'DBに登録されている有効開始日が無かった場合
                            Case WW_BeforeMAXSTYMD = ""
                                WF_AUTOENDYMD.Value = LNM0010WRKINC.MAX_ENDYMD
                            '同一の場合
                            Case WW_BeforeMAXSTYMD = CDate(LNM0010INPtbl.Rows(0)("STYMD")).ToString("yyyy/MM/dd")
                                WF_AUTOENDYMD.Value = LNM0010WRKINC.MAX_ENDYMD
                            Case WW_BeforeMAXSTYMD < CDate(LNM0010INPtbl.Rows(0)("STYMD")).ToString("yyyy/MM/dd")
                                'DBに登録されている有効開始日の有効終了日を登録しようとしている有効開始日-1にする

                                '変更後の有効開始日退避
                                WW_STYMD_SAVE = LNM0010INPtbl.Rows(0)("STYMD")
                                '変更後の有効終了日退避
                                WW_ENDYMD_SAVE = LNM0010INPtbl.Rows(0)("ENDYMD")

                                '変更後テーブルに変更前の有効開始日格納
                                LNM0010INPtbl.Rows(0)("STYMD") = WW_BeforeMAXSTYMD
                                '変更後テーブルに更新用の有効終了日格納
                                LNM0010INPtbl.Rows(0)("ENDYMD") = DateTime.Parse(WW_STYMD_SAVE).AddDays(-1).ToString("yyyy/MM/dd")
                                '履歴テーブルに変更前データを登録
                                InsertHist(SQLcon, LNM0010WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
                                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                    Exit Sub
                                End If
                                '変更前の有効終了日更新
                                UpdateENDYMD(SQLcon, work.WF_SEL_CONTROLTABLE.Text,
                                             LNM0010INPtbl.Rows(0), WW_DBDataCheck, WW_DATE)
                                If Not isNormal(WW_DBDataCheck) Then
                                    Exit Sub
                                End If
                                '履歴テーブルに変更後データを登録
                                InsertHist(SQLcon, LNM0010WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
                                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                    Exit Sub
                                End If
                                '退避した有効開始日を元に戻す
                                LNM0010INPtbl.Rows(0)("STYMD") = WW_STYMD_SAVE
                                '退避した有効終了日を元に戻す
                                LNM0010INPtbl.Rows(0)("ENDYMD") = WW_ENDYMD_SAVE
                                '有効終了日に最大値を入れる
                                WF_AUTOENDYMD.Value = LNM0010WRKINC.MAX_ENDYMD
                            Case Else
                                '有効終了日に有効開始日の月の末日を入れる
                                Dim WW_NEXT_YM As String = DateTime.Parse(LNM0010INPtbl.Rows(0)("STYMD")).AddMonths(1).ToString("yyyy/MM")
                                WF_AUTOENDYMD.Value = DateTime.Parse(WW_NEXT_YM & "/01").AddDays(-1).ToString("yyyy/MM/dd")
                        End Select
                End Select
#End Region

                '変更チェック
                MASTEREXISTS(SQLcon, WW_MODIFYKBN)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.AFTDATA
                End If

                ' マスタ更新
                UpdateMaster(SQLcon)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '履歴登録(新規・変更後)
                InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = "Update Success!!"
            End Using
        End If

        '○ 変更有無判定 & 入力値反映
        For Each LNM0010INProw As DataRow In LNM0010INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0010row As DataRow In LNM0010tbl.Rows
                Select Case work.WF_SEL_CONTROLTABLE.Text
                    Case LNM0010WRKINC.MAPIDLHA '八戸特別料金マスタ
                        ' 同一レコードか判定
                        If LNM0010INProw("TABLEID") = LNM0010row("TABLEID") AndAlso
                            LNM0010INProw("RECOID") = LNM0010row("RECOID") AndAlso
                            LNM0010INProw("TORICODE") = LNM0010row("TORICODE") AndAlso
                            LNM0010INProw("ORGCODE") = LNM0010row("ORGCODE") AndAlso
                            LNM0010INProw("STYMD") = LNM0010row("STYMD") Then
                            ' 画面入力テーブル項目設定
                            LNM0010INProw("LINECNT") = LNM0010row("LINECNT")
                            LNM0010INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            LNM0010INProw("UPDTIMSTP") = LNM0010row("UPDTIMSTP")
                            LNM0010INProw("SELECT") = 0
                            LNM0010INProw("HIDDEN") = 0
                            ' 項目テーブル項目設定
                            LNM0010row.ItemArray = LNM0010INProw.ItemArray
                            ' 発見フラグON
                            WW_IsFound = True
                            Exit For
                        End If
                    Case LNM0010WRKINC.MAPIDLEN 'ENEOS業務委託料マスタ
                        ' 同一レコードか判定
                        If LNM0010INProw("TABLEID") = LNM0010row("TABLEID") AndAlso
                            LNM0010INProw("RECOID") = LNM0010row("RECOID") AndAlso
                            LNM0010INProw("TORICODE") = LNM0010row("TORICODE") AndAlso
                            LNM0010INProw("ORGCODE") = LNM0010row("ORGCODE") AndAlso
                            LNM0010INProw("STYMD") = LNM0010row("STYMD") Then
                            ' 画面入力テーブル項目設定
                            LNM0010INProw("LINECNT") = LNM0010row("LINECNT")
                            LNM0010INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            LNM0010INProw("UPDTIMSTP") = LNM0010row("UPDTIMSTP")
                            LNM0010INProw("SELECT") = 0
                            LNM0010INProw("HIDDEN") = 0
                            ' 項目テーブル項目設定
                            LNM0010row.ItemArray = LNM0010INProw.ItemArray
                            ' 発見フラグON
                            WW_IsFound = True
                            Exit For
                        End If
                    Case LNM0010WRKINC.MAPIDLTO '東北電力車両別追加料金マスタ
                        ' 同一レコードか判定
                        If LNM0010INProw("TABLEID") = LNM0010row("TABLEID") AndAlso
                            LNM0010INProw("TORICODE") = LNM0010row("TORICODE") AndAlso
                            LNM0010INProw("ORGCODE") = LNM0010row("ORGCODE") AndAlso
                            LNM0010INProw("STYMD") = LNM0010row("STYMD") AndAlso
                            LNM0010INProw("SYABAN") = LNM0010row("SYABAN") Then
                            ' 画面入力テーブル項目設定
                            LNM0010INProw("LINECNT") = LNM0010row("LINECNT")
                            LNM0010INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            LNM0010INProw("UPDTIMSTP") = LNM0010row("UPDTIMSTP")
                            LNM0010INProw("SELECT") = 0
                            LNM0010INProw("HIDDEN") = 0
                            ' 項目テーブル項目設定
                            LNM0010row.ItemArray = LNM0010INProw.ItemArray
                            ' 発見フラグON
                            WW_IsFound = True
                            Exit For
                        End If
                    'Case LNM0010WRKINC.MAPIDLKG '北海道ガス特別料金マスタ
                    Case LNM0010WRKINC.MAPIDLSKSP 'SK特別料金マスタ
                        ' 同一レコードか判定
                        If LNM0010INProw("TABLEID") = LNM0010row("TABLEID") AndAlso
                            LNM0010INProw("RECOID") = LNM0010row("RECOID") AndAlso
                            LNM0010INProw("TORICODE") = LNM0010row("TORICODE") AndAlso
                            LNM0010INProw("ORGCODE") = LNM0010row("ORGCODE") AndAlso
                            LNM0010INProw("STYMD") = LNM0010row("STYMD") Then
                            ' 画面入力テーブル項目設定
                            LNM0010INProw("LINECNT") = LNM0010row("LINECNT")
                            LNM0010INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            LNM0010INProw("UPDTIMSTP") = LNM0010row("UPDTIMSTP")
                            LNM0010INProw("SELECT") = 0
                            LNM0010INProw("HIDDEN") = 0
                            ' 項目テーブル項目設定
                            LNM0010row.ItemArray = LNM0010INProw.ItemArray
                            ' 発見フラグON
                            WW_IsFound = True
                            Exit For
                        End If
                    Case LNM0010WRKINC.MAPIDLSKSU 'SK燃料サーチャージマスタ
                        ' 同一レコードか判定
                        If LNM0010INProw("TABLEID") = LNM0010row("TABLEID") AndAlso
                            LNM0010INProw("TORICODE") = LNM0010row("TORICODE") AndAlso
                            LNM0010INProw("ORGCODE") = LNM0010row("ORGCODE") AndAlso
                            LNM0010INProw("TAISHOYM") = LNM0010row("TAISHOYM") Then
                            ' 画面入力テーブル項目設定
                            LNM0010INProw("LINECNT") = LNM0010row("LINECNT")
                            LNM0010INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                            LNM0010INProw("UPDTIMSTP") = LNM0010row("UPDTIMSTP")
                            LNM0010INProw("SELECT") = 0
                            LNM0010INProw("HIDDEN") = 0
                            ' 項目テーブル項目設定
                            LNM0010row.ItemArray = LNM0010INProw.ItemArray
                            ' 発見フラグON
                            WW_IsFound = True
                            Exit For
                        End If
                End Select
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0010tbl.NewRow
                WW_NRow.ItemArray = LNM0010INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0010tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0010tbl.Rows.Add(WW_NRow)
            End If
        Next

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
                Case "TORICODE"             '取引先コード
                    work.CODENAMEGetTORI(SQLcon, WW_NAMEht)
                Case "KASANORGCODE"        '加算先部門コード
                    work.CODENAMEGetKASANORG(SQLcon, WW_NAMEht)
                Case "TODOKECODE"        '加算先部門コード
                    work.CODENAMEGetTODOKE(SQLcon, WW_NAMEht)
            End Select
        End Using

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ALL, Master.USERCAMP))
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DELFLG"))
                Case "TORICODE"              '取引先コード
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
                Case "KASANORGCODE"         '加算先部門コード
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
