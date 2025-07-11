''************************************************************
' 休日割増単価マスタメンテ登録画面
' 作成日 2025/07/01
' 更新日 
' 作成者 三宅
' 更新者 
'
' 修正履歴 : 2025/07/01 新規作成
''************************************************************
Imports MySql.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 休日割増単価マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0017HolidayRateDetail
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0017tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0017INPtbl As DataTable                              'チェック用テーブル
    Private LNM0017UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0017tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"              '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_ButtonCLEAR", "LNM0017L"  '戻るボタン押下（LNM0017Lは、パンくずより）
                            WF_CLEAR_Click()
                        Case "WF_Field_DBClick"             'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_LeftBoxSelectClick"        'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_ButtonSel"                 '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"                 '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"            '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "btnClearConfirmOK"            '戻るボタン押下後の確認ダイアログでOK押下
                            WF_CLEAR_ConfirmOkClick()
                        Case "WF_TORIChange"                '取引先コードチェンジ
                            WF_TORICODE_TEXT.Text = WF_TORICODE.SelectedValue
                        Case "WF_ORGChange"                  '受注受付部署コードチェンジ
                            WF_ORDERORGCODE_TEXT.Text = WF_ORDERORGCODE.SelectedValue
                        Case "WF_ORDERORGCATEGORYChange"    '受注受付部署判定区分コードチェンジ
                            WF_ORDERORGCATEGORY_TEXT.Text = WF_ORDERORGCATEGORY.SelectedValue
                        Case "WF_SHUKABASHOChange"          '出荷場所コードチェンジ
                            WF_SHUKABASHO_TEXT.Text = WF_SHUKABASHO.SelectedValue
                        Case "WF_SHUKABASHOCATEGORYChange"  '出荷場所判定区分コードチェンジ
                            WF_SHUKABASHOCATEGORY_TEXT.Text = WF_SHUKABASHOCATEGORY.SelectedValue
                        Case "mspTodokeCodeSingleRowSelected"  '[共通]届先コード選択ポップアップで行選択
                            RowSelected_mspTodokeCodeSingle()
                        Case "WF_TODOKECATEGORYChange"      '届先判定区分コードチェンジ
                            WF_TODOKECATEGORY_TEXT.Text = WF_TODOKECATEGORY.SelectedValue
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
            If Not IsNothing(LNM0017tbl) Then
                LNM0017tbl.Clear()
                LNM0017tbl.Dispose()
                LNM0017tbl = Nothing
            End If

            If Not IsNothing(LNM0017INPtbl) Then
                LNM0017INPtbl.Clear()
                LNM0017INPtbl.Dispose()
                LNM0017INPtbl = Nothing
            End If

            If Not IsNothing(LNM0017UPDtbl) Then
                LNM0017UPDtbl.Clear()
                LNM0017UPDtbl.Dispose()
                LNM0017UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0017WRKINC.MAPIDD
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
        '荷主
        Me.WF_TORICODE.Items.Clear()
        Me.WF_TORICODE.Items.Add("")
        Dim retToriList As DropDownList = CmnLng.getDowpDownNewTankaList("TORI")
        For index As Integer = 0 To retToriList.Items.Count - 1
            WF_TORICODE.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
        Next

        '受注受付部署部署
        Me.WF_ORDERORGCODE.Items.Clear()
        Me.WF_ORDERORGCODE.Items.Add("")
        Dim retOrgList As DropDownList = CmnLng.getDowpDownNewTankaList("ORG")
        If retOrgList.Items.Count > 0 Then
            '情シス、高圧ガス以外
            If LNM0017WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
                Dim WW_OrgPermitHt As New Hashtable
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()  ' DataBase接続
                    work.GetPermitOrg(SQLcon, Master.USERCAMP, Master.ROLE_ORG, WW_OrgPermitHt)
                    For index As Integer = 0 To retOrgList.Items.Count - 1
                        If WW_OrgPermitHt.ContainsKey(retOrgList.Items(index).Value) = True Then
                            WF_ORDERORGCODE.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
                        End If
                    Next
                End Using
            Else
                For index As Integer = 0 To retOrgList.Items.Count - 1
                    WF_ORDERORGCODE.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
                Next
            End If
        End If

        '受注受付部署部署判定区分
        Me.WF_ORDERORGCATEGORY.Items.Clear()
        Me.WF_ORDERORGCATEGORY.Items.Add("")
        Dim retOrgCategoryList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "CATEGORY")
        For index As Integer = 0 To retOrgCategoryList.Items.Count - 1
            WF_ORDERORGCATEGORY.Items.Add(New ListItem(retOrgCategoryList.Items(index).Text, retOrgCategoryList.Items(index).Value))
        Next

        '出荷場所
        Me.WF_SHUKABASHO.Items.Clear()
        Me.WF_SHUKABASHO.Items.Add("")
        Dim retShukabashoList As DropDownList = CmnLng.getDowpDownNewTankaList("SHUKABASHO")
        For index As Integer = 0 To retShukabashoList.Items.Count - 1
            WF_SHUKABASHO.Items.Add(New ListItem(retShukabashoList.Items(index).Text, retShukabashoList.Items(index).Value))
        Next

        '受出荷場所判定区分
        Me.WF_SHUKABASHOCATEGORY.Items.Clear()
        Me.WF_SHUKABASHOCATEGORY.Items.Add("")
        Dim retShukabashoCategoryList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "CATEGORY")
        For index As Integer = 0 To retShukabashoCategoryList.Items.Count - 1
            WF_SHUKABASHOCATEGORY.Items.Add(New ListItem(retShukabashoCategoryList.Items(index).Text, retShukabashoCategoryList.Items(index).Value))
        Next

        '届先
        'Me.WF_TODOKECODE.Items.Clear()
        'Me.WF_TODOKECODE.Items.Add("")
        'Dim retTodokeList As DropDownList = CmnLng.getDowpDownNewTankaList("TODOKE")
        'For index As Integer = 0 To retTodokeList.Items.Count - 1
        '    WF_TODOKECODE.Items.Add(New ListItem(retTodokeList.Items(index).Text, retTodokeList.Items(index).Value))
        'Next

        '届先判定区分
        Me.WF_TODOKECATEGORY.Items.Clear()
        Me.WF_TODOKECATEGORY.Items.Add("")
        Dim retTodokeCategoryList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "CATEGORY")
        For index As Integer = 0 To retTodokeCategoryList.Items.Count - 1
            WF_TODOKECATEGORY.Items.Add(New ListItem(retTodokeCategoryList.Items(index).Text, retTodokeCategoryList.Items(index).Value))
        Next

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0017L Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        TxtSelLineCNT.Text = work.WF_SEL_LINECNT.Text
        '削除
        RadioDELFLG.SelectedValue = work.WF_SEL_DELFLG.Text
        '画面ＩＤ
        TxtMapId.Text = "M00001"
        '会社コード
        TxtCampCode.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", TxtCampCode.Text, LblCampCodeName.Text, WW_RtnSW)
        '取引先コード、名称
        WF_TORICODE_TEXT.Text = work.WF_SEL_TORICODE.Text
        WF_TORICODE.Text = work.WF_SEL_TORINAME.Text
        WF_TORICODE_TEXT_SAVE.Value = work.WF_SEL_TORICODE.Text
        WF_TORINAME_SAVE.Value = work.WF_SEL_TORINAME.Text

        'ユニークID
        WF_ID.Text = work.WF_SEL_ID.Text
        '取引先コード
        WF_TORICODE_TEXT.Text = work.WF_SEL_TORICODE.Text
        '取引先名称
        WF_TORICODE.SelectedValue = work.WF_SEL_TORICODE.Text
        '受注受付部署名称
        WF_ORDERORGCODE_TEXT.Text = work.WF_SEL_ORDERORGCODE.Text
        '受注受付部署コード
        WF_ORDERORGCODE.SelectedValue = work.WF_SEL_ORDERORGCODE.Text
        '受注受付部署判定区分名称
        WF_ORDERORGCATEGORY_TEXT.Text = work.WF_SEL_ORDERORGCATEGORY.Text
        '受注受付部署判定区分
        WF_ORDERORGCATEGORY.SelectedValue = work.WF_SEL_ORDERORGCATEGORY.Text
        '出荷場所名称
        WF_SHUKABASHO_TEXT.Text = work.WF_SEL_SHUKABASHO.Text
        '出荷場所コード
        WF_SHUKABASHO.SelectedValue = work.WF_SEL_SHUKABASHO.Text
        '出荷場所判定区分名称
        WF_SHUKABASHOCATEGORY_TEXT.Text = work.WF_SEL_SHUKABASHOCATEGORY.Text
        '出荷場所判定区分
        WF_SHUKABASHOCATEGORY.SelectedValue = work.WF_SEL_SHUKABASHOCATEGORY.Text
        '届先名称
        WF_TODOKECODE_TEXT.Text = work.WF_SEL_TODOKECODE.Text
        '届先コード
        WF_TODOKECODE.Text = work.WF_SEL_TODOKENAME.Text
        '届先判定区分名称
        WF_TODOKECATEGORY_TEXT.Text = work.WF_SEL_TODOKECATEGORY.Text
        '届先判定区分
        WF_TODOKECATEGORY.Text = work.WF_SEL_TODOKECATEGORY.Text
        '休日範囲コード
        For i As Integer = 1 To work.WF_SEL_RANGECODE.Text.Length
            For Each item As ListItem In WF_RANGECODE.Items
                If Mid(work.WF_SEL_RANGECODE.Text, i, 1) = item.Value Then
                    item.Selected = True
                End If
            Next
        Next
        WF_RANGECODE.Text = work.WF_SEL_RANGECODE.Text
        '車番（開始）
        WF_GYOMUTANKNUMFROM.Text = work.WF_SEL_GYOMUTANKNUMFROM.Text
        '車番（終了）
        WF_GYOMUTANKNUMTO.Text = work.WF_SEL_GYOMUTANKNUMTO.Text
        '単価
        WF_TANKA.Text = work.WF_SEL_TANKA.Text

        ' 単価を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.WF_TANKA.Attributes("onkeyPress") = "CheckNum()"

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_ID.Text

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU
    End Sub

    ''' <summary>
    ''' 休日割増単価マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(特別料金マスタ)
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("  INSERT INTO LNG.LNM0017_HOLIDAYRATE")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      ID  ")
        SQLStr.AppendLine("     ,TORICODE  ")
        SQLStr.AppendLine("     ,ORDERORGCODE  ")
        SQLStr.AppendLine("     ,ORDERORGCATEGORY  ")
        SQLStr.AppendLine("     ,SHUKABASHO  ")
        SQLStr.AppendLine("     ,SHUKABASHOCATEGORY  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKECATEGORY  ")
        SQLStr.AppendLine("     ,RANGECODE  ")
        SQLStr.AppendLine("     ,GYOMUTANKNUMFROM  ")
        SQLStr.AppendLine("     ,GYOMUTANKNUMTO  ")
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,DELFLG  ")
        SQLStr.AppendLine("     ,INITYMD  ")
        SQLStr.AppendLine("     ,INITUSER  ")
        SQLStr.AppendLine("     ,INITTERMID  ")
        SQLStr.AppendLine("     ,INITPGID  ")
        SQLStr.AppendLine("   )  ")
        SQLStr.AppendLine("   VALUES  ")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      @ID  ")
        SQLStr.AppendLine("     ,@TORICODE  ")
        SQLStr.AppendLine("     ,@ORDERORGCODE  ")
        SQLStr.AppendLine("     ,@ORDERORGCATEGORY  ")
        SQLStr.AppendLine("     ,@SHUKABASHO  ")
        SQLStr.AppendLine("     ,@SHUKABASHOCATEGORY  ")
        SQLStr.AppendLine("     ,@TODOKECODE  ")
        SQLStr.AppendLine("     ,@TODOKECATEGORY  ")
        SQLStr.AppendLine("     ,@RANGECODE  ")
        SQLStr.AppendLine("     ,@GYOMUTANKNUMFROM  ")
        SQLStr.AppendLine("     ,@GYOMUTANKNUMTO  ")
        SQLStr.AppendLine("     ,@TANKA  ")
        SQLStr.AppendLine("     ,@DELFLG  ")
        SQLStr.AppendLine("     ,@INITYMD  ")
        SQLStr.AppendLine("     ,@INITUSER  ")
        SQLStr.AppendLine("     ,@INITTERMID  ")
        SQLStr.AppendLine("     ,@INITPGID  ")
        SQLStr.AppendLine("   )   ")
        SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
        SQLStr.AppendLine("      ID =  @ID")
        SQLStr.AppendLine("     ,TORICODE =  @TORICODE")
        SQLStr.AppendLine("     ,ORDERORGCODE =  @ORDERORGCODE")
        SQLStr.AppendLine("     ,ORDERORGCATEGORY =  @ORDERORGCATEGORY")
        SQLStr.AppendLine("     ,SHUKABASHO =  @SHUKABASHO")
        SQLStr.AppendLine("     ,SHUKABASHOCATEGORY =  @SHUKABASHOCATEGORY")
        SQLStr.AppendLine("     ,TODOKECODE =  @TODOKECODE")
        SQLStr.AppendLine("     ,TODOKECATEGORY =  @TODOKECATEGORY")
        SQLStr.AppendLine("     ,RANGECODE =  @RANGECODE")
        SQLStr.AppendLine("     ,GYOMUTANKNUMFROM =  @GYOMUTANKNUMFROM")
        SQLStr.AppendLine("     ,GYOMUTANKNUMTO =  @GYOMUTANKNUMTO")
        SQLStr.AppendLine("     ,TANKA =  @TANKA")
        SQLStr.AppendLine("     ,DELFLG =  @DELFLG")
        SQLStr.AppendLine("     ,UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("     ,UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("     ,UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("     ,UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("     ,RECEIVEYMD =  @RECEIVEYMD")
        SQLStr.AppendLine("    ;  ")

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl = New StringBuilder
        SQLJnl.AppendLine(" SELECT                                     ")
        SQLJnl.AppendLine("     DELFLG                                 ")
        SQLJnl.AppendLine("   , ID                                     ")
        SQLJnl.AppendLine("   , TORICODE                               ")
        SQLJnl.AppendLine("   , ORDERORGCODE                           ")
        SQLJnl.AppendLine("   , ORDERORGCATEGORY                       ")
        SQLJnl.AppendLine("   , SHUKABASHO                             ")
        SQLJnl.AppendLine("   , SHUKABASHOCATEGORY                     ")
        SQLJnl.AppendLine("   , TODOKECODE                             ")
        SQLJnl.AppendLine("   , TODOKECATEGORY                         ")
        SQLJnl.AppendLine("   , RANGECODE                              ")
        SQLJnl.AppendLine("   , GYOMUTANKNUMFROM                       ")
        SQLJnl.AppendLine("   , GYOMUTANKNUMTO                         ")
        SQLJnl.AppendLine("   , TANKA                                  ")
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
        SQLJnl.AppendLine("     LNG.LNM0017_HOLIDAYRATE                ")
        SQLJnl.AppendLine(" WHERE                                      ")
        SQLJnl.AppendLine("         ID   = @ID                         ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl.ToString, SQLcon)
                ' DB更新用パラメータ
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.Int16)                                        'ユニークID
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)                      '取引先コード
                Dim P_ORDERORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCODE", MySqlDbType.VarChar, 20)              '受注受付部署コード
                Dim P_ORDERORGCATEGORY As MySqlParameter = SQLcmd.Parameters.Add("@ORDERORGCATEGORY", MySqlDbType.VarChar, 1)       '受注受付部署判定区分
                Dim P_SHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABASHO", MySqlDbType.VarChar, 20)                  '出荷場所コード
                Dim P_SHUKABASHOCATEGORY As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABASHOCATEGORY", MySqlDbType.VarChar, 1)   '出荷場所判定区分
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 20)                  '届先コード
                Dim P_TODOKECATEGORY As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECATEGORY", MySqlDbType.VarChar, 1)           '届先判定区分
                Dim P_RANGECODE As MySqlParameter = SQLcmd.Parameters.Add("@RANGECODE", MySqlDbType.VarChar, 5)                     '休日範囲コード
                Dim P_GYOMUTANKNUMFROM As MySqlParameter = SQLcmd.Parameters.Add("@GYOMUTANKNUMFROM", MySqlDbType.VarChar, 20)      '車番（開始）
                Dim P_GYOMUTANKNUMTO As MySqlParameter = SQLcmd.Parameters.Add("@GYOMUTANKNUMTO", MySqlDbType.VarChar, 20)          '車番（終了）
                Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal, 8)                             '単価
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                           '削除フラグ
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                           '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)                      '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)                  '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)                      '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                             '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)                        '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)                    '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)                        '更新プログラムＩＤ
                Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)                     '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JP_ID As MySqlParameter = SQLcmdJnl.Parameters.Add("@ID", MySqlDbType.Int16)                                    'ユニークID

                Dim LNM0017row As DataRow = LNM0017INPtbl.Rows(0)

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                P_DELFLG.Value = LNM0017row("DELFLG")                           '削除フラグ
                ' ユニークID取得
                Dim WW_DBDataCheck As String = ""
                Dim WW_ID As Integer = 0
                If LNM0017row("ID") = 0 Then
                    work.GetMaxID(SQLcon, WW_DBDataCheck, WW_ID)
                    If isNormal(WW_DBDataCheck) Then
                        LNM0017row("ID") = WW_ID                                'ユニークID
                    End If
                End If
                P_ID.Value = LNM0017row("ID")                                   'ユニークID
                P_TORICODE.Value = LNM0017row("TORICODE")                       '取引先コード
                P_ORDERORGCODE.Value = LNM0017row("ORDERORGCODE")               '受注受付部署コード
                P_ORDERORGCATEGORY.Value = LNM0017row("ORDERORGCATEGORY")       '受注受付部署判定区分
                P_SHUKABASHO.Value = LNM0017row("SHUKABASHO")                   '出荷場所コード
                P_SHUKABASHOCATEGORY.Value = LNM0017row("SHUKABASHOCATEGORY")   '出荷場所判定区分
                P_TODOKECODE.Value = LNM0017row("TODOKECODE")                   '届先コード
                P_TODOKECATEGORY.Value = LNM0017row("TODOKECATEGORY")           '届先判定区分
                P_RANGECODE.Value = LNM0017row("RANGECODE")                     '休日範囲コード
                P_GYOMUTANKNUMFROM.Value = LNM0017row("GYOMUTANKNUMFROM")       '車番（開始）
                P_GYOMUTANKNUMTO.Value = LNM0017row("GYOMUTANKNUMTO")           '車番（終了）
                P_TANKA.Value = LNM0017row("TANKA")                             '単価
                P_DELFLG.Value = LNM0017row("DELFLG")                           '削除フラグ

                P_INITYMD.Value = WW_DateNow                        '登録年月日
                P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID              '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                P_UPDYMD.Value = WW_DateNow                         '更新年月日
                P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JP_ID.Value = LNM0017row("ID")                                  'ユニークID

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0017UPDtbl) Then
                        LNM0017UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0017UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0017UPDtbl.Clear()
                    LNM0017UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0017UPDrow As DataRow In LNM0017UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0017_HOLIDAYRATE"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0017UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0017_HOLIDAYRATE UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0017_HOLIDAYRATE UPDATE_INSERT"
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

        '休日割増単価マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0017_HOLIDAYRATE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         ID             = @ID ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.Int16)                'ID

                Dim LNM0017row As DataRow = LNM0017INPtbl.Rows(0)

                P_ID.Value = LNM0017row("ID")                     'ID

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
                        WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0017_HOLIDAYRATE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0017_HOLIDAYRATE Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 履歴テーブル登録
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsertHist(ByVal SQLcon As MySqlConnection, ByVal WW_MODIFYKBN As String, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ ＤＢ更新
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0029_HOLIDAYRATEHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      ID  ")
        SQLStr.AppendLine("     ,TORICODE  ")
        SQLStr.AppendLine("     ,ORDERORGCODE  ")
        SQLStr.AppendLine("     ,ORDERORGCATEGORY  ")
        SQLStr.AppendLine("     ,SHUKABASHO  ")
        SQLStr.AppendLine("     ,SHUKABASHOCATEGORY  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKECATEGORY  ")
        SQLStr.AppendLine("     ,RANGECODE  ")
        SQLStr.AppendLine("     ,GYOMUTANKNUMFROM  ")
        SQLStr.AppendLine("     ,GYOMUTANKNUMTO  ")
        SQLStr.AppendLine("     ,TANKA  ")
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
        SQLStr.AppendLine("      ID  ")
        SQLStr.AppendLine("     ,TORICODE  ")
        SQLStr.AppendLine("     ,ORDERORGCODE  ")
        SQLStr.AppendLine("     ,ORDERORGCATEGORY  ")
        SQLStr.AppendLine("     ,SHUKABASHO  ")
        SQLStr.AppendLine("     ,SHUKABASHOCATEGORY  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKECATEGORY  ")
        SQLStr.AppendLine("     ,RANGECODE  ")
        SQLStr.AppendLine("     ,GYOMUTANKNUMFROM  ")
        SQLStr.AppendLine("     ,GYOMUTANKNUMTO  ")
        SQLStr.AppendLine("     ,TANKA  ")
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
        SQLStr.AppendLine("        LNG.LNM0017_HOLIDAYRATE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         ID             = @ID ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.Int16)                            'ID

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)           '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)      '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)               '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)          '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)      '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)          '登録プログラムＩＤ

                Dim LNM0017row As DataRow = LNM0017INPtbl.Rows(0)

                ' DB更新
                P_ID.Value = LNM0017row("ID")           'ID

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0017WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0017tbl.Rows(0)("DELFLG") = "0" And LNM0017row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0017WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0017WRKINC.OPERATEKBN.UPDDATA).ToString
                    End If
                End If

                P_MODIFYKBN.Value = WW_MODIFYKBN                '変更区分
                P_MODIFYYMD.Value = WW_NOW                      '変更日時
                P_MODIFYUSER.Value = Master.USERID              '変更ユーザーＩＤ

                P_INITYMD.Value = WW_NOW                        '登録年月日
                P_INITUSER.Value = Master.USERID                '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID          '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name   '登録プログラムＩＤ

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0029_HOLIDAYRATEHIST INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0029_HOLIDAYRATEHIST INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub
#End Region

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
            RadioDELFLG.SelectedValue = C_DELETE_FLG.DELETE Then

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
        DetailBoxToLNM0017INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0017tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0017tbl, work.WF_SEL_INPTBL.Text)

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
    Protected Sub DetailBoxToLNM0017INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(RadioDELFLG.SelectedValue)      '削除フラグ
        Master.EraseCharToIgnore(WF_TORICODE.Text)               '取引先コード
        Master.EraseCharToIgnore(WF_ORDERORGCODE.Text)           '受注受付部署コード
        Master.EraseCharToIgnore(WF_ORDERORGCATEGORY.Text)       '受注受付部署判定区分
        Master.EraseCharToIgnore(WF_SHUKABASHO.Text)             '出荷場所コード
        Master.EraseCharToIgnore(WF_SHUKABASHOCATEGORY.Text)     '出荷場所判定区分
        Master.EraseCharToIgnore(WF_TODOKECODE.Text)             '届先コード
        Master.EraseCharToIgnore(WF_TODOKECATEGORY.Text)         '届先判定区分
        'Master.EraseCharToIgnore(WF_RANGECODE.SelectedValue)     '休日範囲コード
        Master.EraseCharToIgnore(WF_GYOMUTANKNUMFROM.Text)       '車番（開始）
        Master.EraseCharToIgnore(WF_GYOMUTANKNUMTO.Text)         '車番（終了）
        Master.EraseCharToIgnore(WF_TANKA.Text)                  '単価

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) AndAlso
            String.IsNullOrEmpty(RadioDELFLG.SelectedValue) Then
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

        Master.CreateEmptyTable(LNM0017INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0017INProw As DataRow = LNM0017INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) Then
            LNM0017INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(TxtSelLineCNT.Text, LNM0017INProw("LINECNT"))
            Catch ex As Exception
                LNM0017INProw("LINECNT") = 0
            End Try
        End If

        LNM0017INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        LNM0017INProw("UPDTIMSTP") = Date.Now
        LNM0017INProw("SELECT") = 1
        LNM0017INProw("HIDDEN") = 0

        LNM0017INProw("DELFLG") = RadioDELFLG.SelectedValue             '削除フラグ

        '更新の場合
        If Not DisabledKeyItem.Value = "" Then
            LNM0017INProw("ID") = work.WF_SEL_ID.Text                   'ID
        Else
            LNM0017INProw("ID") = 0                                     'ID
        End If

        LNM0017INProw("TORICODE") = WF_TORICODE.SelectedValue                           '取引先コード
        LNM0017INProw("TORINAME") = WF_TORICODE.SelectedItem                            '取引先名称
        LNM0017INProw("ORDERORGCODE") = WF_ORDERORGCODE.SelectedValue                   '受注受付部署コード
        LNM0017INProw("ORDERORGNAME") = WF_ORDERORGCODE.SelectedItem                    '受注受付部署名称
        LNM0017INProw("ORDERORGCATEGORY") = WF_ORDERORGCATEGORY.SelectedValue           '受注受付部署判定区分
        LNM0017INProw("ORDERORGCATEGORYNAME") = WF_ORDERORGCATEGORY.SelectedItem        '受注受付部署判定区分名称
        LNM0017INProw("SHUKABASHO") = WF_SHUKABASHO.SelectedValue                       '出荷場所コード
        LNM0017INProw("SHUKABASHONAME") = WF_SHUKABASHO.SelectedItem                    '出荷場所名称
        LNM0017INProw("SHUKABASHOCATEGORY") = WF_SHUKABASHOCATEGORY.SelectedValue       '出荷場所判定区分
        LNM0017INProw("SHUKABASHOCATEGORYNAME") = WF_SHUKABASHOCATEGORY.SelectedItem    '出荷場所判定区分名称
        LNM0017INProw("TODOKECODE") = WF_TODOKECODE_TEXT.Text                           '届先コード
        LNM0017INProw("TODOKENAME") = WF_TODOKECODE.Text                                '届先コード
        LNM0017INProw("TODOKECATEGORY") = WF_TODOKECATEGORY.SelectedValue               '届先判定区分
        LNM0017INProw("TODOKECATEGORYNAME") = WF_TODOKECATEGORY.SelectedItem            '届先判定区分名称
        LNM0017INProw("RANGECODE") = ""                                                 '休日範囲コード
        For Each item As ListItem In WF_RANGECODE.Items
            If item.Selected Then
                LNM0017INProw("RANGECODE") += item.Value                                '休日範囲コード
            End If
        Next
        LNM0017INProw("GYOMUTANKNUMFROM") = WF_GYOMUTANKNUMFROM.Text                    '車番（開始）
        LNM0017INProw("GYOMUTANKNUMTO") = WF_GYOMUTANKNUMTO.Text                        '車番（終了）
        LNM0017INProw("TANKA") = WF_TANKA.Text                                          '単価

        '○ チェック用テーブルに登録する
        LNM0017INPtbl.Rows.Add(LNM0017INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0017INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0017INProw As DataRow = LNM0017INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0017row As DataRow In LNM0017tbl.Rows
            ' KEY項目が等しい時
            If LNM0017row("ID") = LNM0017INProw("ID") Then                                             'ID
                ' KEY項目以外の項目の差異をチェック
                If LNM0017row("DELFLG") = LNM0017INProw("DELFLG") AndAlso
                    LNM0017row("TORICODE") = LNM0017INProw("TORICODE") AndAlso                         '取引先コード
                    LNM0017row("ORDERORGCODE") = LNM0017INProw("ORDERORGCODE") AndAlso                 '受注受付部署コード
                    LNM0017row("ORDERORGCATEGORY") = LNM0017INProw("ORDERORGCATEGORY") AndAlso         '受注受付部署判定区分
                    LNM0017row("SHUKABASHO") = LNM0017INProw("SHUKABASHO") AndAlso                     '出荷場所
                    LNM0017row("SHUKABASHOCATEGORY") = LNM0017INProw("SHUKABASHOCATEGORY") AndAlso     '出荷場所判定区分
                    LNM0017row("TODOKECODE") = LNM0017INProw("TODOKECODE") AndAlso                     '届先コード
                    LNM0017row("TODOKECATEGORY") = LNM0017INProw("TODOKECATEGORY") AndAlso             '届先判定区分
                    LNM0017row("RANGECODE") = LNM0017INProw("RANGECODE") AndAlso                       '休日範囲コード
                    LNM0017row("GYOMUTANKNUMFROM") = LNM0017INProw("GYOMUTANKNUMFROM") AndAlso         '車番（開始）
                    LNM0017row("GYOMUTANKNUMTO") = LNM0017INProw("GYOMUTANKNUMTO") AndAlso             '車番（終了）
                    LNM0017row("TANKA") = LNM0017INProw("TANKA") Then                                  '単価

                    ' 変更がない時は、入力変更フラグをOFFにする
                    WW_InputChangeFlg = False
                End If

                Exit For

            End If
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
        For Each LNM0017row As DataRow In LNM0017tbl.Rows
            Select Case LNM0017row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0017row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0017row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0017row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0017row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0017row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0017tbl, work.WF_SEL_INPTBL.Text)

        TxtSelLineCNT.Text = ""                         'LINECNT
        TxtMapId.Text = "M00001"                        '画面ＩＤ
        RadioDELFLG.SelectedValue = ""                  '削除フラグ
        WF_ID.Text = ""                                 'ユニークID
        WF_TORICODE.Text = ""                           '取引先名称
        WF_TORICODE_TEXT.Text = ""                      '取引先コード
        WF_ORDERORGCODE.Text = ""                       '受注受付部署名称
        WF_ORDERORGCODE_TEXT.Text = ""                  '受注受付部署コード
        WF_ORDERORGCATEGORY.Text = ""                   '受注受付部署判定区分名称
        WF_ORDERORGCATEGORY_TEXT.Text = ""              '受注受付部署判定区分
        WF_SHUKABASHO.Text = ""                         '出荷場所名称
        WF_SHUKABASHO_TEXT.Text = ""                    '出荷場所コード
        WF_SHUKABASHOCATEGORY.Text = ""                 '出荷場所判定区分名称
        WF_SHUKABASHOCATEGORY_TEXT.Text = ""            '出荷場所判定区分
        WF_TODOKECODE.Text = ""                         '届先名称
        WF_TODOKECODE_TEXT.Text = ""                    '届先コード
        WF_TODOKECATEGORY.Text = ""                     '届先判定区分名称
        WF_TODOKECATEGORY_TEXT.Text = ""                '届先判定区分
        WF_RANGECODE.Text = ""                          '休日範囲コード
        WF_GYOMUTANKNUMFROM.Text = ""                   '車番（開始）
        WF_GYOMUTANKNUMTO.Text = ""                     '車番（終了）
        WF_TANKA.Text = ""                              '単価

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
                    Case "WF_SHUKABASHO_TEXT"       '出荷場所コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspShukabashoSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    Case "WF_TODOKECODE_TEXT"       '届先コード
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
            '    CODENAME_get("DELFLG", RadioDELFLG.SelectedValue, LblDelFlgName.Text, WW_Dummy)
            '    TxtDelFlg.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 休日割増単価マスタ更新(削除フラグのみ)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UpdateMasterDelflgOnly()
        Dim WW_MODIFYKBN As String = ""
        Dim WW_DATE As Date = Date.Now

        '初期化
        LNM0017INPtbl = New DataTable
        LNM0017INPtbl.Columns.Add("ID")
        LNM0017INPtbl.Columns.Add("DELFLG")

        Dim row As DataRow
        row = LNM0017INPtbl.NewRow
        row("ID") = work.WF_SEL_ID.Text
        row("DELFLG") = C_DELETE_FLG.DELETE
        LNM0017INPtbl.Rows.Add(row)

        ' DB更新処理
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()

            '履歴テーブルに変更前データを登録
            InsertHist(SQLcon, LNM0017WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '削除フラグ更新
            SetDelflg(SQLcon, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '履歴テーブルに変更後データを登録
            InsertHist(SQLcon, LNM0017WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

        End Using

        '○ 入力値反映
        For Each LNM0017INProw As DataRow In LNM0017INPtbl.Rows
            For Each LNM0017row As DataRow In LNM0017tbl.Rows
                If LNM0017INProw("ID") = LNM0017row("ID") Then
                    ' 画面入力テーブル項目設定              
                    LNM0017row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0017row("DELFLG") = LNM0017INProw("DELFLG")
                    LNM0017row("SELECT") = 0
                    LNM0017row("HIDDEN") = 0
                    Exit For
                End If
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
        SQLStr.Append("     LNG.LNM0017_HOLIDAYRATE                 ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         ID               = @ID              ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ID As MySqlParameter = SQLcmd.Parameters.Add("@ID", MySqlDbType.Int16)          'ID
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                Dim LNM0017row As DataRow = LNM0017INPtbl.Rows(0)
                P_ID.Value = LNM0017row("ID")                   'ID
                P_UPDYMD.Value = WW_NOW                         '更新年月日
                P_UPDUSER.Value = Master.USERID                 '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID           '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name    '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0017_HOLIDAYRATE UPDATE"
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
                '    RadioDELFLG.SelectedValue = WW_SelectValue
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
    ''' 出荷場所コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspShukabashoSingle()

        Me.mspShukabashoSingle.InitPopUp()
        Me.mspShukabashoSingle.SelectionMode = ListSelectionMode.Single

        Me.mspShukabashoSingle.SQL = CmnSearchSQL.GetTankaAvocadoShukabashoSQL(WF_ORDERORGCODE.SelectedValue)

        Me.mspShukabashoSingle.KeyFieldName = "KEYCODE"
        Me.mspShukabashoSingle.DispFieldList.AddRange(CmnSearchSQL.GetTankaAvocadoShukabashoTitle)

        Me.mspShukabashoSingle.ShowPopUpList()

    End Sub
    ''' <summary>
    ''' 届先コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspTodokeCodeSingle()

        Me.mspTodokeCodeSingle.InitPopUp()
        Me.mspTodokeCodeSingle.SelectionMode = ListSelectionMode.Single

        Me.mspTodokeCodeSingle.SQL = CmnSearchSQL.GetTankaAvocadoTodokeSQL(WF_ORDERORGCODE.SelectedValue)

        Me.mspTodokeCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspTodokeCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetTankaAvocadoTodokeTitle)

        Me.mspTodokeCodeSingle.ShowPopUpList()

    End Sub
    ''' <summary>
    ''' 出荷場所選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspShukabashoSingle()

        Dim selData = Me.mspShukabashoSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case WF_SHUKABASHO_TEXT.ID
                Me.WF_SHUKABASHO_TEXT.Text = selData("AVOCADOSHUKABASHO").ToString '出荷場所コード
                Me.WF_SHUKABASHO.Text = selData("AVOCADOSHUKANAME").ToString '出荷場所名
                Me.WF_SHUKABASHO.Focus()
        End Select

        'ポップアップの非表示
        Me.mspShukabashoSingle.HidePopUp()

    End Sub
    ''' <summary>
    ''' 届先選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspTodokeCodeSingle()

        Dim selData = Me.mspTodokeCodeSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case WF_TODOKECODE_TEXT.ID
                Me.WF_TODOKECODE_TEXT.Text = selData("AVOCADOTODOKECODE").ToString '届先コード
                Me.WF_TODOKECODE.Text = selData("AVOCADOTODOKENAME").ToString '届先名
                Me.WF_TODOKECODE.Focus()
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
            WW_CheckMES1 = "・休日割増単価マスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0017INProw As DataRow In LNM0017INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0017INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0017INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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

            'ユニークID(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ID", LNM0017INProw("ID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・ユニークIDエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引先コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORICODE", LNM0017INProw("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("TORICODE", LNM0017INProw("TORICODE"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・取引先コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・取引先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '受注受付部署コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ORDERORGCODE", LNM0017INProw("ORDERORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("ORDERORGCODE", LNM0017INProw("ORDERORGCODE"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・受注受付部署コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・受注受付部署コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '受注受付部署判定区分(バリデーションチェック)
            '受注受付部署が指定されている時のみチェックし、指定なしの場合は、強制クリア
            If LNM0017INProw("ORDERORGCODE") = "" Then
                LNM0017INProw("ORDERORGCATEGORY") = ""
            Else
                Master.CheckField(Master.USERCAMP, "ORDERORGCATEGORY", LNM0017INProw("ORDERORGCATEGORY"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If String.IsNullOrEmpty(LNM0017INProw("ORDERORGCATEGORY")) Then
                        WW_CheckMES1 = "・受注受付部署判定区分エラーです。"
                        WW_CheckMES2 = "受付部署が入力された場合、必須です。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        ' 名称存在チェック
                        CODENAME_get("CATEGORY", LNM0017INProw("ORDERORGCATEGORY"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・受注受付部署判定区分エラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・受注受付部署判定区分エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '出荷場所コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SHUKABASHO", LNM0017INProw("SHUKABASHO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("SHUKABASHO", LNM0017INProw("SHUKABASHO"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・出荷場所コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・出荷場所コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '出荷場所判定区分(バリデーションチェック)
            '出荷場所が指定されている時のみチェックし、指定なしの場合は、強制クリア
            If LNM0017INProw("SHUKABASHO") = "" Then
                LNM0017INProw("SHUKABASHOCATEGORY") = ""
            Else
                Master.CheckField(Master.USERCAMP, "SHUKABASHOCATEGORY", LNM0017INProw("SHUKABASHOCATEGORY"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If String.IsNullOrEmpty(LNM0017INProw("SHUKABASHOCATEGORY")) Then
                        WW_CheckMES1 = "・出荷場所判定区分エラーです。"
                        WW_CheckMES2 = "出荷場所が入力された場合、必須です。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        ' 名称存在チェック
                        CODENAME_get("CATEGORY", LNM0017INProw("SHUKABASHOCATEGORY"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・出荷場所判定区分エラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・出荷場所判定区分エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            '届先コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TODOKECODE", LNM0017INProw("TODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("TODOKECODE", LNM0017INProw("TODOKECODE"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・届先コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・届先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '届先判定区分(バリデーションチェック)
            '届先が指定されている時のみチェックし、指定なしの場合は、強制クリア
            If LNM0017INProw("TODOKECODE") = "" Then
                LNM0017INProw("TODOKECATEGORY") = ""
            Else
                Master.CheckField(Master.USERCAMP, "TODOKECATEGORY", LNM0017INProw("TODOKECATEGORY"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If String.IsNullOrEmpty(LNM0017INProw("TODOKECATEGORY")) Then
                        WW_CheckMES1 = "・届先判定区エラーです。"
                        WW_CheckMES2 = "届先が入力された場合、必須です。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        ' 名称存在チェック
                        CODENAME_get("CATEGORY", LNM0017INProw("TODOKECATEGORY"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・届先判定区エラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・届先判定区分エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '休日範囲コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "RANGECODE", LNM0017INProw("RANGECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・休日範囲コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車番（開始）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "GYOMUTANKNUMFROM", LNM0017INProw("GYOMUTANKNUMFROM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・車番（開始）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '車番（終了）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "GYOMUTANKNUMTO", LNM0017INProw("GYOMUTANKNUMTO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・車番（終了）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '単価(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TANKA", LNM0017INProw("TANKA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・単価エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_ID.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()

                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text, work.WF_SEL_ID.Text)

                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If

            If String.IsNullOrEmpty(WW_LineErr) Then
                If LNM0017INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0017INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0017INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0017INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0017tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0017tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0017row As DataRow In LNM0017tbl.Rows
            Select Case LNM0017row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0017row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0017row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0017row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0017row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0017row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0017INProw As DataRow In LNM0017INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0017INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0017INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0017row As DataRow In LNM0017tbl.Rows
                ' KEY項目が等しい時
                If LNM0017row("ID") = LNM0017INProw("ID") Then                                                      'ID
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0017row("DELFLG") = LNM0017INProw("DELFLG") AndAlso
                        LNM0017row("TORICODE") = LNM0017INProw("TORICODE") AndAlso                                  '取引先
                        LNM0017row("ORDERORGCODE") = LNM0017INProw("ORDERORGCODE") AndAlso                          '受注受付部署コード
                        LNM0017row("ORDERORGCATEGORY") = LNM0017INProw("ORDERORGCATEGORY") AndAlso                  '受注受付部署判定区分
                        LNM0017row("SHUKABASHO") = LNM0017INProw("SHUKABASHO") AndAlso                              '出荷場所コード
                        LNM0017row("SHUKABASHOCATEGORY") = LNM0017INProw("SHUKABASHOCATEGORY") AndAlso              '出荷場所判定区分
                        LNM0017row("TODOKECODE") = LNM0017INProw("TODOKECODE") AndAlso                              '届先コード
                        LNM0017row("TODOKECATEGORY") = LNM0017INProw("TODOKECATEGORY") AndAlso                      '届先判定区分
                        LNM0017row("RANGECODE") = LNM0017INProw("RANGECODE") AndAlso                                '休日範囲コード
                        LNM0017row("GYOMUTANKNUMFROM") = LNM0017INProw("GYOMUTANKNUMFROM") AndAlso                  '車番（開始）
                        LNM0017row("GYOMUTANKNUMTO") = LNM0017INProw("GYOMUTANKNUMTO") AndAlso                      '車番（終了）
                        LNM0017row("TANKA") = LNM0017INProw("TANKA") AndAlso                                        '単価
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0017row("OPERATION")) Then

                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0017INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0017INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0017INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0017INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0017INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now
                Dim WW_DBDataCheck As String = ""

                '変更チェック
                MASTEREXISTS(SQLcon, WW_MODIFYKBN)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0017WRKINC.MODIFYKBN.AFTDATA
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
        For Each LNM0017INProw As DataRow In LNM0017INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0017row As DataRow In LNM0017tbl.Rows
                ' 同一レコードか判定
                If LNM0017row("ID") = LNM0017INProw("ID") Then                                'ID
                    ' 画面入力テーブル項目設定
                    LNM0017INProw("LINECNT") = LNM0017row("LINECNT")
                    LNM0017INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0017INProw("UPDTIMSTP") = LNM0017row("UPDTIMSTP")
                    LNM0017INProw("SELECT") = 0
                    LNM0017INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0017row.ItemArray = LNM0017INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0017tbl.NewRow
                WW_NRow.ItemArray = LNM0017INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0017tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0017tbl.Rows.Add(WW_NRow)
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
                Case "ORDERORGCODE"         '部門コード
                    work.CODENAMEGetORG(SQLcon, WW_NAMEht)
                Case "SHUKABASHO"           '出荷場所コード
                    work.CODENAMEGetSHUKABASHO(SQLcon, WW_NAMEht)
                Case "TODOKECODE"           '届先コード
                    work.CODENAMEGetTODOKE(SQLcon, WW_NAMEht)
            End Select
        End Using

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ALL, Master.USERCAMP))
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DELFLG"))
                Case "CATEGORY"         '範疇フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "CATEGORY"))
                Case "HOLIDAYRANGE"     '休日範囲フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "HOLIDAYRANGE"))
                Case "TORICODE", "ORDERORGCODE", "SHUKABASHO", "TODOKECODE"        '取引先、部門、出荷場所、届先
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

End Class
