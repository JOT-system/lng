''************************************************************
' 軽油価格参照先マスタメンテ登録画面
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
''' 軽油価格参照先マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0020DieselPriceSiteDetail
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0020tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0020INPtbl As DataTable                              'チェック用テーブル
    Private LNM0020UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0020tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"              '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_ButtonCLEAR", "LNM0020L"  '戻るボタン押下（LNM0020Lは、パンくずより）
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
                        Case "WF_DIESELPRICESITEChange"     '軽油価格参照先チェンジ
                            Dim WW_HT As New Hashtable
                            For index As Integer = 0 To WF_DIESELPRICE.Items.Count - 1
                                WW_HT.Add(WF_DIESELPRICE.Items(index).Text, WF_DIESELPRICE.Items(index).Value)
                            Next

                            If WW_HT.ContainsKey(WF_DIESELPRICESITENAME.Text) Then
                                WF_DIESELPRICESITEID.Text = WW_HT(WF_DIESELPRICESITENAME.Text)
                            Else
                                WF_DIESELPRICESITEID.Text = ""
                            End If
                            'createListBox("BRANCH")
                            'Case "WF_DIESELPRICESITEKBChange"   '軽油価格参照先区分チェンジ
                            '    Dim WW_HT As New Hashtable
                            '    For index As Integer = 0 To WF_DIESELPRICEKB.Items.Count - 1
                            '        WW_HT.Add(RTrim(WF_DIESELPRICEKB.Items(index).Text), WF_DIESELPRICEKB.Items(index).Value)
                            '    Next

                            '    If WW_HT.ContainsKey(WF_DIESELPRICESITEKBNNAME.Text) Then
                            '        WF_DIESELPRICESITEBRANCH.Text = WW_HT(WF_DIESELPRICESITEKBNNAME.Text)
                            '    Else
                            '        WF_DIESELPRICESITEBRANCH.Text = ""
                            '    End If
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
            If Not IsNothing(LNM0020tbl) Then
                LNM0020tbl.Clear()
                LNM0020tbl.Dispose()
                LNM0020tbl = Nothing
            End If

            If Not IsNothing(LNM0020INPtbl) Then
                LNM0020INPtbl.Clear()
                LNM0020INPtbl.Dispose()
                LNM0020INPtbl = Nothing
            End If

            If Not IsNothing(LNM0020UPDtbl) Then
                LNM0020UPDtbl.Clear()
                LNM0020UPDtbl.Dispose()
                LNM0020UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0020WRKINC.MAPIDD
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
    Protected Sub createListBox(Optional ByVal pKbn As String = "INIT")

        If pKbn = "INIT" Then
            '軽油価格参照先
            Me.WF_DIESELPRICE.Items.Clear()
            'Me.WF_DIESELPRICE.Items.Add("")
            Dim retDieselPriceList As DropDownList = CmnLng.getDropDownDieselPriceList()
            For index As Integer = 0 To retDieselPriceList.Items.Count - 1
                WF_DIESELPRICE.Items.Add(New ListItem(retDieselPriceList.Items(index).Text, retDieselPriceList.Items(index).Value))
            Next
            'コンボボックス化
            Dim WW_DIESELPRICE_OPTIONS As String = ""
            For index As Integer = 0 To retDieselPriceList.Items.Count - 1
                WW_DIESELPRICE_OPTIONS += "<option>" + retDieselPriceList.Items(index).Text + "</option>"
            Next
            WF_DIESELPRICE_DL.InnerHtml = WW_DIESELPRICE_OPTIONS
            Me.WF_DIESELPRICESITENAME.Attributes("list") = Me.WF_DIESELPRICE_DL.ClientID
        End If

        'If pKbn = "INIT" OrElse pKbn = "BRANCH" Then
        '    '軽油価格参照先区分
        '    Me.WF_DIESELPRICEKB.Items.Clear()
        '    'Me.WF_DIESELPRICEKB.Items.Add("")
        '    Dim retDieselPriceKbList As DropDownList = CmnLng.getDropDownDieselPriceKbList(WF_DIESELPRICESITEID.Text)
        '    For index As Integer = 0 To retDieselPriceKbList.Items.Count - 1
        '        WF_DIESELPRICEKB.Items.Add(New ListItem(retDieselPriceKbList.Items(index).Text, retDieselPriceKbList.Items(index).Value))
        '    Next
        '    'コンボボックス化
        '    Dim WW_DIESELPRICEKB_OPTIONS As String = ""
        '    For index As Integer = 0 To retDieselPriceKbList.Items.Count - 1
        '        WW_DIESELPRICEKB_OPTIONS += "<option>" + retDieselPriceKbList.Items(index).Text + "</option>"
        '    Next
        '    WF_DIESELPRICEKB_DL.InnerHtml = WW_DIESELPRICEKB_OPTIONS
        '    Me.WF_DIESELPRICESITEKBNNAME.Attributes("list") = Me.WF_DIESELPRICEKB_DL.ClientID
        'End If

        WF_DIESELPRICESITENAME.Attributes.Add("autocomplete", "off")
        WF_DIESELPRICESITEKBNNAME.Attributes.Add("autocomplete", "off")
        WF_DISPLAYNAME.Attributes.Add("autocomplete", "off")
        WF_DIESELPRICESITEURL.Attributes.Add("autocomplete", "off")
    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0020L Then
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

        '実勢軽油価格参照先ID
        WF_DIESELPRICESITEID.Text = work.WF_SEL_DIESELPRICESITEID.Text
        WF_DIESELPRICESITEID_SAVE.Value = work.WF_SEL_DIESELPRICESITEID.Text
        '実勢軽油価格参照先ID枝番
        WF_DIESELPRICESITEBRANCH.Text = work.WF_SEL_DIESELPRICESITEBRANCH.Text
        WF_DIESELPRICESITEBRANCH_SAVE.Value = work.WF_SEL_DIESELPRICESITEBRANCH.Text
        '実勢軽油価格参照先名
        WF_DIESELPRICESITENAME.Text = work.WF_SEL_DIESELPRICESITENAME.Text
        WF_DIESELPRICESITENAME_SAVE.Value = work.WF_SEL_DIESELPRICESITENAME.Text
        '実勢軽油価格参照先区分名
        WF_DIESELPRICESITEKBNNAME.Text = work.WF_SEL_DIESELPRICESITEKBNNAME.Text
        WF_DIESELPRICESITEKBNNAME_SAVE.Value = work.WF_SEL_DIESELPRICESITEKBNNAME.Text
        '画面表示名称
        WF_DISPLAYNAME.Text = work.WF_SEL_DISPLAYNAME.Text
        '実勢軽油価格参照先URL
        WF_DIESELPRICESITEURL.Text = work.WF_SEL_DIESELPRICESITEURL.Text

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_DIESELPRICESITEID.Text

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU
    End Sub

    ''' <summary>
    ''' 軽油価格参照先マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(特別料金マスタ)
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" INSERT INTO LNG.LNM0020_DIESELPRICESITE ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH  ")
        SQLStr.AppendLine("     ,DIESELPRICESITENAME  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEKBNNAME  ")
        SQLStr.AppendLine("     ,DISPLAYNAME  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEURL  ")
        SQLStr.AppendLine("     ,DELFLG  ")
        SQLStr.AppendLine("     ,INITYMD  ")
        SQLStr.AppendLine("     ,INITUSER  ")
        SQLStr.AppendLine("     ,INITTERMID  ")
        SQLStr.AppendLine("     ,INITPGID  ")
        SQLStr.AppendLine("  )  ")
        SQLStr.AppendLine("   VALUES  ")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      @DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,@DIESELPRICESITEBRANCH  ")
        SQLStr.AppendLine("     ,@DIESELPRICESITENAME  ")
        SQLStr.AppendLine("     ,@DIESELPRICESITEKBNNAME  ")
        SQLStr.AppendLine("     ,@DISPLAYNAME  ")
        SQLStr.AppendLine("     ,@DIESELPRICESITEURL  ")
        SQLStr.AppendLine("     ,@DELFLG  ")
        SQLStr.AppendLine("     ,@INITYMD  ")
        SQLStr.AppendLine("     ,@INITUSER  ")
        SQLStr.AppendLine("     ,@INITTERMID  ")
        SQLStr.AppendLine("     ,@INITPGID  ")
        SQLStr.AppendLine("   )   ")
        SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
        SQLStr.AppendLine("      DIESELPRICESITEID =  @DIESELPRICESITEID")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH =  @DIESELPRICESITEBRANCH")
        SQLStr.AppendLine("     ,DIESELPRICESITENAME =  @DIESELPRICESITENAME")
        SQLStr.AppendLine("     ,DIESELPRICESITEKBNNAME =  @DIESELPRICESITEKBNNAME")
        SQLStr.AppendLine("     ,DISPLAYNAME =  @DISPLAYNAME")
        SQLStr.AppendLine("     ,DIESELPRICESITEURL =  @DIESELPRICESITEURL")
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
        SQLJnl.AppendLine("     DIESELPRICESITEID                      ")
        SQLJnl.AppendLine("   , DIESELPRICESITEBRANCH                  ")
        SQLJnl.AppendLine("   , DIESELPRICESITENAME                    ")
        SQLJnl.AppendLine("   , DIESELPRICESITEKBNNAME                 ")
        SQLJnl.AppendLine("   , DISPLAYNAME                            ")
        SQLJnl.AppendLine("   , DIESELPRICESITEURL                     ")
        SQLJnl.AppendLine("   , DELFLG                                 ")
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
        SQLJnl.AppendLine("     LNG.LNM0020_DIESELPRICESITE            ")
        SQLJnl.AppendLine(" WHERE                                      ")
        SQLJnl.AppendLine("     DIESELPRICESITEID      = @DIESELPRICESITEID    ")
        SQLJnl.AppendLine(" AND DIESELPRICESITEBRANCH  = @DIESELPRICESITEBRANCH")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl.ToString, SQLcon)
                ' DB更新用パラメータ
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar)            '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar)    '実勢軽油価格参照先ID枝番
                Dim P_DIESELPRICESITENAME As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITENAME", MySqlDbType.VarChar)        '実勢軽油価格参照先名
                Dim P_DIESELPRICESITEKBNNAME As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEKBNNAME", MySqlDbType.VarChar)  '実勢軽油価格参照先区分名
                Dim P_DISPLAYNAME As MySqlParameter = SQLcmd.Parameters.Add("@DISPLAYNAME", MySqlDbType.VarChar)                        '画面表示名称
                Dim P_DIESELPRICESITEURL As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEURL", MySqlDbType.VarChar)          '実勢軽油価格参照先URL
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                               '削除フラグ
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                               '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)                          '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)                      '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)                          '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                                 '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)                            '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)                        '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)                            '更新プログラムＩＤ
                Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)                         '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JP_DIESELPRICESITEID As MySqlParameter = SQLcmdJnl.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar)            '実勢軽油価格参照先ID
                Dim JP_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmdJnl.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar)    '実勢軽油価格参照先ID枝番

                Dim LNM0020row As DataRow = LNM0020INPtbl.Rows(0)

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                P_DELFLG.Value = LNM0020row("DELFLG")                           '削除フラグ
                ' ユニークID取得
                Dim WW_DBDataCheck As String = ""
                Dim WW_ID As String = "01"
                If String.IsNullOrEmpty(LNM0020row("DIESELPRICESITEID")) Then
                    work.GetMaxID(SQLcon, WW_DBDataCheck, WW_ID)
                    If isNormal(WW_DBDataCheck) Then
                        LNM0020row("DIESELPRICESITEID") = WW_ID                         '実勢軽油価格参照先ID
                        LNM0020row("DIESELPRICESITEBRANCH") = "01"                      '実勢軽油価格参照先ID枝番
                    End If
                End If
                Dim WW_BRANCH As String = "01"
                If String.IsNullOrEmpty(LNM0020row("DIESELPRICESITEBRANCH")) Then
                    work.GetMaxBRANCH(SQLcon, LNM0020row("DIESELPRICESITEID"), WW_DBDataCheck, WW_BRANCH)
                    If isNormal(WW_DBDataCheck) Then
                        LNM0020row("DIESELPRICESITEBRANCH") = WW_BRANCH                 '実勢軽油価格参照先ID枝番
                    End If
                End If

                P_DIESELPRICESITEID.Value = LNM0020row("DIESELPRICESITEID")             '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = LNM0020row("DIESELPRICESITEBRANCH")     '実勢軽油価格参照先ID枝番
                P_DIESELPRICESITENAME.Value = LNM0020row("DIESELPRICESITENAME")         '実勢軽油価格参照先名
                P_DIESELPRICESITEKBNNAME.Value = LNM0020row("DIESELPRICESITEKBNNAME")   '実勢軽油価格参照先区分名
                P_DISPLAYNAME.Value = LNM0020row("DISPLAYNAME")                         '画面表示名称
                P_DIESELPRICESITEURL.Value = LNM0020row("DIESELPRICESITEURL")           '実勢軽油価格参照先URL
                P_DELFLG.Value = LNM0020row("DELFLG")                                   '削除フラグ

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
                JP_DIESELPRICESITEID.Value = LNM0020row("DIESELPRICESITEID")             '実勢軽油価格参照先ID
                JP_DIESELPRICESITEBRANCH.Value = LNM0020row("DIESELPRICESITEBRANCH")     '実勢軽油価格参照先ID枝番

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0020UPDtbl) Then
                        LNM0020UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0020UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0020UPDtbl.Clear()
                    LNM0020UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0020UPDrow As DataRow In LNM0020UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0020_DIESELPRICESITE"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0020UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0020_DIESELPRICESITE UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0020_DIESELPRICESITE UPDATE_INSERT"
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

        '軽油価格参照先管理マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DIESELPRICESITEID")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0020_DIESELPRICESITE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         DIESELPRICESITEID             = @DIESELPRICESITEID ")
        SQLStr.AppendLine("    AND  DIESELPRICESITEBRANCH         = @DIESELPRICESITEBRANCH ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar)                '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar)        '実勢軽油価格参照先ID枝番

                Dim LNM0020row As DataRow = LNM0020INPtbl.Rows(0)

                P_DIESELPRICESITEID.Value = LNM0020row("DIESELPRICESITEID")                             '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = LNM0020row("DIESELPRICESITEBRANCH")                     '実勢軽油価格参照先ID枝番

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
                        WW_MODIFYKBN = LNM0020WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0020WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0020_DIESELPRICESITE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0020_DIESELPRICESITE Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0033_DIESELPRICESITEHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH  ")
        SQLStr.AppendLine("     ,DIESELPRICESITENAME  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEKBNNAME  ")
        SQLStr.AppendLine("     ,DISPLAYNAME  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEURL  ")
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
        SQLStr.AppendLine("      DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH  ")
        SQLStr.AppendLine("     ,DIESELPRICESITENAME  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEKBNNAME  ")
        SQLStr.AppendLine("     ,DISPLAYNAME  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEURL  ")
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
        SQLStr.AppendLine("        LNG.LNM0020_DIESELPRICESITE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         DIESELPRICESITEID             = @DIESELPRICESITEID ")
        SQLStr.AppendLine("    AND  DIESELPRICESITEBRANCH         = @DIESELPRICESITEBRANCH ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar)                            '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar)                    '実勢軽油価格参照先ID枝番

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)           '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)      '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)               '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)          '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)      '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)          '登録プログラムＩＤ

                Dim LNM0020row As DataRow = LNM0020INPtbl.Rows(0)

                ' DB更新
                P_DIESELPRICESITEID.Value = LNM0020row("DIESELPRICESITEID")                     '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = LNM0020row("DIESELPRICESITEBRANCH")             '実勢軽油価格参照先ID枝番

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0020WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0020WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0020tbl.Rows(0)("DELFLG") = "0" And LNM0020row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0020WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0020WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0033_DIESELPRICESITEHIST INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0033_DIESELPRICESITEHIST INSERT"
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
        DetailBoxToLNM0020INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0020tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0020tbl, work.WF_SEL_INPTBL.Text)

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
    Protected Sub DetailBoxToLNM0020INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(RadioDELFLG.SelectedValue)      '削除フラグ
        Master.EraseCharToIgnore(WF_DIESELPRICESITEID.Text)      '実勢軽油価格参照先ID
        Master.EraseCharToIgnore(WF_DIESELPRICESITEBRANCH.Text)  '実勢軽油価格参照先ID枝番
        Master.EraseCharToIgnore(WF_DIESELPRICESITENAME.Text)    '実勢軽油価格参照先名
        Master.EraseCharToIgnore(WF_DIESELPRICESITEKBNNAME.Text) '実勢軽油価格参照先区分名
        Master.EraseCharToIgnore(WF_DISPLAYNAME.Text)            '画面表示名称
        Master.EraseCharToIgnore(WF_DIESELPRICESITEURL.Text)     '実勢軽油価格参照先URL

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

        Master.CreateEmptyTable(LNM0020INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0020INProw As DataRow = LNM0020INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) Then
            LNM0020INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(TxtSelLineCNT.Text, LNM0020INProw("LINECNT"))
            Catch ex As Exception
                LNM0020INProw("LINECNT") = 0
            End Try
        End If

        LNM0020INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        LNM0020INProw("UPDTIMSTP") = Date.Now
        LNM0020INProw("SELECT") = 1
        LNM0020INProw("HIDDEN") = 0

        LNM0020INProw("DELFLG") = RadioDELFLG.SelectedValue             '削除フラグ

        LNM0020INProw("DIESELPRICESITEID") = WF_DIESELPRICESITEID.Text                  '実勢軽油価格参照先ID
        LNM0020INProw("DIESELPRICESITEBRANCH") = WF_DIESELPRICESITEBRANCH.Text          '実勢軽油価格参照先ID枝番
        If Not DisabledKeyItem.Value = "" Then
            '更新の場合
            'JavaScriptでdisabled=trueの制御を行うとサーバーに送信（レスポンス）されないため保存している名称を使う
            LNM0020INProw("DIESELPRICESITENAME") = WF_DIESELPRICESITENAME_SAVE.Value        '実勢軽油価格参照先名
            LNM0020INProw("DIESELPRICESITEKBNNAME") = WF_DIESELPRICESITEKBNNAME_SAVE.Value  '実勢軽油価格参照先区分名
        Else
            '新規の場合
            LNM0020INProw("DIESELPRICESITENAME") = WF_DIESELPRICESITENAME.Text              '実勢軽油価格参照先名
            LNM0020INProw("DIESELPRICESITEKBNNAME") = WF_DIESELPRICESITEKBNNAME.Text        '実勢軽油価格参照先区分名
        End If

        LNM0020INProw("DISPLAYNAME") = WF_DISPLAYNAME.Text                              '画面表示名称
        LNM0020INProw("DIESELPRICESITEURL") = WF_DIESELPRICESITEURL.Text                '実勢軽油価格参照先URL

        '○ チェック用テーブルに登録する
        LNM0020INPtbl.Rows.Add(LNM0020INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0020INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0020INProw As DataRow = LNM0020INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0020row As DataRow In LNM0020tbl.Rows
            ' KEY項目が等しい時
            If LNM0020row("DIESELPRICESITEID") = LNM0020INProw("DIESELPRICESITEID") AndAlso
               LNM0020row("DIESELPRICESITEBRANCH") = LNM0020INProw("DIESELPRICESITEBRANCH") Then        'ID
                ' KEY項目以外の項目の差異をチェック
                If LNM0020row("DELFLG") = LNM0020INProw("DELFLG") AndAlso
                    LNM0020row("DIESELPRICESITENAME") = LNM0020INProw("DIESELPRICESITENAME") AndAlso                '実勢軽油価格参照先名
                    LNM0020row("DIESELPRICESITEKBNNAME") = LNM0020INProw("DIESELPRICESITEKBNNAME") AndAlso          '実勢軽油価格参照先区分名
                    LNM0020row("DISPLAYNAME") = LNM0020INProw("DISPLAYNAME") AndAlso                                '画面表示名称
                    LNM0020row("DIESELPRICESITEURL") = LNM0020INProw("DIESELPRICESITEURL") Then                     '実勢軽油価格参照先URL

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
        For Each LNM0020row As DataRow In LNM0020tbl.Rows
            Select Case LNM0020row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0020row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0020row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0020row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0020row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0020row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0020tbl, work.WF_SEL_INPTBL.Text)

        TxtSelLineCNT.Text = ""                         'LINECNT
        TxtMapId.Text = "M00001"                        '画面ＩＤ
        RadioDELFLG.SelectedValue = ""                  '削除フラグ
        WF_DIESELPRICESITENAME.Text = ""                '実勢軽油価格参照先名
        WF_DIESELPRICESITEID.Text = ""                  '実勢軽油価格参照先ID
        WF_DIESELPRICESITEKBNNAME.Text = ""             '実勢軽油価格参照先区分名
        WF_DIESELPRICESITEBRANCH.Text = ""              '実勢軽油価格参照先ID枝番
        WF_DISPLAYNAME.Text = ""                        '画面表示名称
        WF_DIESELPRICESITEURL.Text = ""                 '実勢軽油価格参照先URL

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
    ''' 軽油価格参照先管理マスタ更新(削除フラグのみ)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UpdateMasterDelflgOnly()
        Dim WW_MODIFYKBN As String = ""
        Dim WW_DATE As Date = Date.Now

        '初期化
        LNM0020INPtbl = New DataTable
        LNM0020INPtbl.Columns.Add("DIESELPRICESITEID")
        LNM0020INPtbl.Columns.Add("DIESELPRICESITEBRANCH")
        LNM0020INPtbl.Columns.Add("DELFLG")

        Dim row As DataRow
        row = LNM0020INPtbl.NewRow
        row("DIESELPRICESITEID") = work.WF_SEL_DIESELPRICESITEID.Text
        row("DIESELPRICESITEBRANCH") = work.WF_SEL_DIESELPRICESITEBRANCH.Text
        row("DELFLG") = C_DELETE_FLG.DELETE
        LNM0020INPtbl.Rows.Add(row)

        ' DB更新処理
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()

            '履歴テーブルに変更前データを登録
            InsertHist(SQLcon, LNM0020WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '削除フラグ更新
            SetDelflg(SQLcon, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '履歴テーブルに変更後データを登録
            InsertHist(SQLcon, LNM0020WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

        End Using

        '○ 入力値反映
        For Each LNM0020INProw As DataRow In LNM0020INPtbl.Rows
            For Each LNM0020row As DataRow In LNM0020tbl.Rows
                If LNM0020INProw("DIESELPRICESITEID") = LNM0020row("DIESELPRICESITEID") AndAlso
                   LNM0020INProw("DIESELPRICESITEBRANCH") = LNM0020row("DIESELPRICESITEBRANCH") Then
                    ' 画面入力テーブル項目設定              
                    LNM0020row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0020row("DELFLG") = LNM0020INProw("DELFLG")
                    LNM0020row("SELECT") = 0
                    LNM0020row("HIDDEN") = 0
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
        SQLStr.Append(" UPDATE                                            ")
        SQLStr.Append("     LNG.LNM0020_DIESELPRICESITE                   ")
        SQLStr.Append(" SET                                               ")
        SQLStr.Append("     DELFLG               = '1'                    ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD                ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER               ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID             ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID               ")
        SQLStr.Append(" WHERE                                             ")
        SQLStr.Append("     DIESELPRICESITEID    = @DIESELPRICESITEID     ")
        SQLStr.Append(" AND DIESELPRICESITEBRANCH= @DIESELPRICESITEBRANCH ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar)                '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar)        '実勢軽油価格参照先ID枝番
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                                     '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)                                '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)                            '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)                                '更新プログラムＩＤ

                Dim LNM0020row As DataRow = LNM0020INPtbl.Rows(0)
                P_DIESELPRICESITEID.Value = LNM0020row("DIESELPRICESITEID")                         '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = LNM0020row("DIESELPRICESITEBRANCH")                 '実勢軽油価格参照先ID枝番
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0020_DIESELPRICESITE UPDATE"
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

        'Me.mspShukabashoSingle.InitPopUp()
        'Me.mspShukabashoSingle.SelectionMode = ListSelectionMode.Single

        'Me.mspShukabashoSingle.SQL = CmnSearchSQL.GetTankaAvocadoShukabashoSQL(WF_ORDERORGCODE.SelectedValue)

        'Me.mspShukabashoSingle.KeyFieldName = "KEYCODE"
        'Me.mspShukabashoSingle.DispFieldList.AddRange(CmnSearchSQL.GetTankaAvocadoShukabashoTitle)

        'Me.mspShukabashoSingle.ShowPopUpList()

    End Sub
    ''' <summary>
    ''' 届先コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspTodokeCodeSingle()

        'Me.mspTodokeCodeSingle.InitPopUp()
        'Me.mspTodokeCodeSingle.SelectionMode = ListSelectionMode.Single

        'Me.mspTodokeCodeSingle.SQL = CmnSearchSQL.GetTankaAvocadoTodokeSQL(WF_ORDERORGCODE.SelectedValue)

        'Me.mspTodokeCodeSingle.KeyFieldName = "KEYCODE"
        'Me.mspTodokeCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetTankaAvocadoTodokeTitle)

        'Me.mspTodokeCodeSingle.ShowPopUpList()

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
            WW_CheckMES1 = "・軽油価格参照先管理マスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0020INProw As DataRow In LNM0020INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0020INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0020INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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

            '実勢軽油価格参照先ID(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DIESELPRICESITEID", LNM0020INProw("DIESELPRICESITEID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・実勢軽油価格参照先IDエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '実勢軽油価格参照先ID枝番(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DIESELPRICESITEBRANCH", LNM0020INProw("DIESELPRICESITEBRANCH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・実勢軽油価格参照先ID枝番エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '実勢軽油価格参照先名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DIESELPRICESITENAME", LNM0020INProw("DIESELPRICESITENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・実勢軽油価格参照先名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '実勢軽油価格参照先区分名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DIESELPRICESITEKBNNAME", LNM0020INProw("DIESELPRICESITEKBNNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・実勢軽油価格参照先区分名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面表示名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DISPLAYNAME", LNM0020INProw("DISPLAYNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・画面表示名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '実勢軽油価格参照先URL(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DIESELPRICESITEURL", LNM0020INProw("DIESELPRICESITEURL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・実勢軽油価格参照先URLエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '新規（追加）の場合、重複チェック
            If String.IsNullOrEmpty(work.WF_SEL_DIESELPRICESITEID.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()

                    Dim WW_MODIFYKBN As String = ""

                    'なぜかMASTEREXISTS()を実行するとO_RTNが"00000"となるため一旦保存
                    Dim sv As String = O_RTN

                    '変更チェック
                    MASTEREXISTS(SQLcon, WW_MODIFYKBN)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If

                    'なぜかMASTEREXISTS()を実行するとO_RTNが"00000"となるため保存から戻す
                    O_RTN = sv

                    If WW_MODIFYKBN <> LNM0020WRKINC.MODIFYKBN.NEWDATA Then
                        WW_CheckMES1 = "・重複エラーです。"
                        WW_CheckMES2 = "既に登録されています。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.ALREADY_UPDATE_ERROR
                    End If
                End Using
            End If

            ' 変更の場合、排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_DIESELPRICESITEID.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()

                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text, work.WF_SEL_DIESELPRICESITEID.Text, work.WF_SEL_DIESELPRICESITEBRANCH.Text)

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
                If LNM0020INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0020INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0020INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0020INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0020tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0020tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0020row As DataRow In LNM0020tbl.Rows
            Select Case LNM0020row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0020row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0020row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0020row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0020row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0020row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0020INProw As DataRow In LNM0020INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0020INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0020INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0020row As DataRow In LNM0020tbl.Rows
                ' KEY項目が等しい時
                If LNM0020row("DIESELPRICESITEID") = LNM0020INProw("DIESELPRICESITEID") AndAlso
                   LNM0020row("DIESELPRICESITEBRANCH") = LNM0020INProw("DIESELPRICESITEBRANCH") Then        'ID
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0020row("DELFLG") = LNM0020INProw("DELFLG") AndAlso
                        LNM0020row("DIESELPRICESITENAME") = LNM0020INProw("DIESELPRICESITENAME") AndAlso                '実勢軽油価格参照先名
                        LNM0020row("DIESELPRICESITEKBNNAME") = LNM0020INProw("DIESELPRICESITEKBNNAME") AndAlso          '実勢軽油価格参照先区分名
                        LNM0020row("DISPLAYNAME") = LNM0020INProw("DISPLAYNAME") AndAlso                                '画面表示名称
                        LNM0020row("DIESELPRICESITEURL") = LNM0020INProw("DIESELPRICESITEURL") AndAlso                  '実勢軽油価格参照先URL
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0020row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0020INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0020INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0020INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0020INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0020INPtbl.Rows(0)("OPERATION")) Then
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
                If WW_MODIFYKBN = LNM0020WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0020WRKINC.MODIFYKBN.AFTDATA
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
        For Each LNM0020INProw As DataRow In LNM0020INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0020row As DataRow In LNM0020tbl.Rows
                ' 同一レコードか判定
                If LNM0020row("DIESELPRICESITEID") = LNM0020INProw("DIESELPRICESITEID") AndAlso
                   LNM0020row("DIESELPRICESITEBRANCH") = LNM0020INProw("DIESELPRICESITEBRANCH") Then
                    ' 画面入力テーブル項目設定
                    LNM0020INProw("LINECNT") = LNM0020row("LINECNT")
                    LNM0020INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0020INProw("UPDTIMSTP") = LNM0020row("UPDTIMSTP")
                    LNM0020INProw("SELECT") = 0
                    LNM0020INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0020row.ItemArray = LNM0020INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0020tbl.NewRow
                WW_NRow.ItemArray = LNM0020INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0020tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0020tbl.Rows.Add(WW_NRow)
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

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ALL, Master.USERCAMP))
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DELFLG"))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

        O_RTN = C_MESSAGE_NO.NORMAL

    End Sub

End Class
