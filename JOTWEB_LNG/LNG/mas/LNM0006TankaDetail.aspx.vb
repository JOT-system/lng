''************************************************************
' 単価マスタメンテ登録画面
' 作成日 2024/12/16
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2024/12/16 新規作成
'          : 
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 単価マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0006TankaDetail
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0006tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0006INPtbl As DataTable                              'チェック用テーブル
    Private LNM0006UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0006tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_ButtonCLEAR"           '戻るボタン押下
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
            If Not IsNothing(LNM0006tbl) Then
                LNM0006tbl.Clear()
                LNM0006tbl.Dispose()
                LNM0006tbl = Nothing
            End If

            If Not IsNothing(LNM0006INPtbl) Then
                LNM0006INPtbl.Clear()
                LNM0006INPtbl.Dispose()
                LNM0006INPtbl = Nothing
            End If

            If Not IsNothing(LNM0006UPDtbl) Then
                LNM0006UPDtbl.Clear()
                LNM0006UPDtbl.Dispose()
                LNM0006UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0006WRKINC.MAPIDD
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
                If LNM0006WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
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

            '車型ドロップダウンのクリア
            Me.ddlSelectSYAGATA.Items.Clear()
            Me.ddlSelectSYAGATA.Items.Add("")

            '車型ドロップダウンの生成
            Dim retSyagataList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "SYAGATA")
            If retOfficeList.Items.Count > 0 Then
                For index As Integer = 0 To retSyagataList.Items.Count - 1
                    ddlSelectSYAGATA.Items.Add(New ListItem(retSyagataList.Items(index).Text, retSyagataList.Items(index).Value))
                Next
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0006L Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        TxtSelLineCNT.Text = work.WF_SEL_LINECNT.Text
        '削除
        TxtDelFlg.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
        '画面ＩＤ
        TxtMapId.Text = "M00001"
        '会社コード
        TxtCampCode.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", TxtCampCode.Text, LblCampCodeName.Text, WW_RtnSW)

        '取引先コード
        TxtTORICODE.Text = work.WF_SEL_TORICODE.Text
        '取引先名称
        TxtTORINAME.Text = work.WF_SEL_TORINAME.Text
        '部門コード
        ddlSelectORG.SelectedValue = work.WF_SEL_ORGCODE.Text
        '部門名称
        ddlSelectORG.SelectedValue = work.WF_SEL_ORGNAME.Text
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
        '枝番
        TxtBRANCHCODE.Text = work.WF_SEL_BRANCHCODE.Text
        '単価
        TxtTANKA.Text = work.WF_SEL_TANKA.Text
        '車型
        ddlSelectSYAGATA.SelectedValue = work.WF_SEL_SYAGATA.Text
        '車号
        TxtSYAGOU.Text = work.WF_SEL_SYAGOU.Text
        '車腹
        TxtSYABARA.Text = work.WF_SEL_SYABARA.Text
        '種別
        TxtSYUBETSU.Text = work.WF_SEL_SYUBETSU.Text
        '備考1
        TxtBIKOU1.Text = work.WF_SEL_BIKOU1.Text
        '備考2
        TxtBIKOU2.Text = work.WF_SEL_BIKOU2.Text
        '備考3
        TxtBIKOU3.Text = work.WF_SEL_BIKOU3.Text

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_TORICODE.Text
        '表示制御項目
        '情シス、高圧ガス以外の場合
        If LNM0006WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            VisibleKeyOrgCode.Value = ""
        Else
            VisibleKeyOrgCode.Value = Master.ROLE_ORG
        End If

        ' 削除フラグ・取引先コード・加算先部門コード・届先コード・単価を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTORICODE.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtKASANORGCODE.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTODOKECODE.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTANKA.Attributes("onkeyPress") = "CheckNum()"

        ' 有効開始日・有効終了日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.WF_StYMD.Attributes("onkeyPress") = "CheckCalendar()"
        Me.WF_EndYMD.Attributes("onkeyPress") = "CheckCalendar()"

        ' 入力するテキストボックスは数値(0～9)＋記号(.)のみ可能とする。
        Me.TxtSYABARA.Attributes("onkeyPress") = "CheckDeci()"             '車腹

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU
    End Sub

    ''' <summary>
    ''' 単価マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(単価マスタ)
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("     INSERT INTO LNG.LNM0006_TANKA           ")
        SQLStr.AppendLine("        (                                    ")
        SQLStr.AppendLine("         DELFLG                              ")
        SQLStr.AppendLine("       , TORICODE                            ")
        SQLStr.AppendLine("       , TORINAME                            ")
        SQLStr.AppendLine("       , ORGCODE                             ")
        SQLStr.AppendLine("       , ORGNAME                             ")
        SQLStr.AppendLine("       , KASANORGCODE                        ")
        SQLStr.AppendLine("       , KASANORGNAME                        ")
        SQLStr.AppendLine("       , TODOKECODE                          ")
        SQLStr.AppendLine("       , TODOKENAME                          ")
        SQLStr.AppendLine("       , STYMD                               ")
        SQLStr.AppendLine("       , ENDYMD                              ")
        SQLStr.AppendLine("       , BRANCHCODE                          ")
        SQLStr.AppendLine("       , TANKA                               ")
        SQLStr.AppendLine("       , SYAGATA                             ")
        SQLStr.AppendLine("       , SYAGATANAME                         ")
        SQLStr.AppendLine("       , SYAGOU                              ")
        SQLStr.AppendLine("       , SYABARA                             ")
        SQLStr.AppendLine("       , SYUBETSU                            ")
        SQLStr.AppendLine("       , BIKOU1                              ")
        SQLStr.AppendLine("       , BIKOU2                              ")
        SQLStr.AppendLine("       , BIKOU3                              ")
        SQLStr.AppendLine("       , INITYMD                             ")
        SQLStr.AppendLine("       , INITUSER                            ")
        SQLStr.AppendLine("       , INITTERMID                          ")
        SQLStr.AppendLine("       , INITPGID                            ")
        SQLStr.AppendLine("       , RECEIVEYMD                          ")
        SQLStr.AppendLine("        )                                    ")
        SQLStr.AppendLine("     VALUES                                  ")
        SQLStr.AppendLine("        (                                    ")
        SQLStr.AppendLine("         @DELFLG                             ")
        SQLStr.AppendLine("       , @TORICODE                           ")
        SQLStr.AppendLine("       , @TORINAME                           ")
        SQLStr.AppendLine("       , @ORGCODE                            ")
        SQLStr.AppendLine("       , @ORGNAME                            ")
        SQLStr.AppendLine("       , @KASANORGCODE                       ")
        SQLStr.AppendLine("       , @KASANORGNAME                       ")
        SQLStr.AppendLine("       , @TODOKECODE                         ")
        SQLStr.AppendLine("       , @TODOKENAME                         ")
        SQLStr.AppendLine("       , @STYMD                              ")
        SQLStr.AppendLine("       , @ENDYMD                             ")
        SQLStr.AppendLine("       , @BRANCHCODE                         ")
        SQLStr.AppendLine("       , @TANKA                              ")
        SQLStr.AppendLine("       , @SYAGATA                            ")
        SQLStr.AppendLine("       , @SYAGATANAME                        ")
        SQLStr.AppendLine("       , @SYAGOU                             ")
        SQLStr.AppendLine("       , @SYABARA                            ")
        SQLStr.AppendLine("       , @SYUBETSU                           ")
        SQLStr.AppendLine("       , @BIKOU1                             ")
        SQLStr.AppendLine("       , @BIKOU2                             ")
        SQLStr.AppendLine("       , @BIKOU3                             ")
        SQLStr.AppendLine("       , @INITYMD                            ")
        SQLStr.AppendLine("       , @INITUSER                           ")
        SQLStr.AppendLine("       , @INITTERMID                         ")
        SQLStr.AppendLine("       , @INITPGID                           ")
        SQLStr.AppendLine("       , @RECEIVEYMD                         ")
        SQLStr.AppendLine("        )                                    ")
        SQLStr.AppendLine("     ON DUPLICATE KEY UPDATE                 ")
        SQLStr.AppendLine("         DELFLG         = @DELFLG            ")
        SQLStr.AppendLine("       , TORINAME       = @TORINAME          ")
        SQLStr.AppendLine("       , ORGNAME        = @ORGNAME           ")
        SQLStr.AppendLine("       , KASANORGNAME   = @KASANORGNAME      ")
        SQLStr.AppendLine("       , TODOKENAME     = @TODOKENAME        ")
        SQLStr.AppendLine("       , ENDYMD         = @ENDYMD            ")
        SQLStr.AppendLine("       , TANKA          = @TANKA             ")
        SQLStr.AppendLine("       , SYAGATA        = @SYAGATA           ")
        SQLStr.AppendLine("       , SYAGATANAME    = @SYAGATANAME       ")
        SQLStr.AppendLine("       , SYAGOU         = @SYAGOU            ")
        SQLStr.AppendLine("       , SYABARA        = @SYABARA           ")
        SQLStr.AppendLine("       , SYUBETSU       = @SYUBETSU          ")
        SQLStr.AppendLine("       , BIKOU1         = @BIKOU1            ")
        SQLStr.AppendLine("       , BIKOU2         = @BIKOU2            ")
        SQLStr.AppendLine("       , BIKOU3         = @BIKOU3            ")
        SQLStr.AppendLine("       , UPDYMD         = @UPDYMD            ")
        SQLStr.AppendLine("       , UPDUSER        = @UPDUSER           ")
        SQLStr.AppendLine("       , UPDTERMID      = @UPDTERMID         ")
        SQLStr.AppendLine("       , UPDPGID        = @UPDPGID           ")
        SQLStr.AppendLine("       , RECEIVEYMD     = @RECEIVEYMD        ")

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl = New StringBuilder
        SQLJnl.AppendLine(" SELECT                                     ")
        SQLJnl.AppendLine("     DELFLG                                 ")
        SQLJnl.AppendLine("   , TORICODE                               ")
        SQLJnl.AppendLine("   , TORINAME                               ")
        SQLJnl.AppendLine("   , ORGCODE                                ")
        SQLJnl.AppendLine("   , ORGNAME                                ")
        SQLJnl.AppendLine("   , KASANORGCODE                           ")
        SQLJnl.AppendLine("   , KASANORGNAME                           ")
        SQLJnl.AppendLine("   , TODOKECODE                             ")
        SQLJnl.AppendLine("   , TODOKENAME                             ")
        SQLJnl.AppendLine("   , STYMD                                  ")
        SQLJnl.AppendLine("   , ENDYMD                                 ")
        SQLJnl.AppendLine("   , BRANCHCODE                             ")
        SQLJnl.AppendLine("   , TANKA                                  ")
        SQLJnl.AppendLine("   , SYAGATA                                ")
        SQLJnl.AppendLine("   , SYAGATANAME                            ")
        SQLJnl.AppendLine("   , SYAGOU                                 ")
        SQLJnl.AppendLine("   , SYABARA                                ")
        SQLJnl.AppendLine("   , SYUBETSU                               ")
        SQLJnl.AppendLine("   , BIKOU1                                 ")
        SQLJnl.AppendLine("   , BIKOU2                                 ")
        SQLJnl.AppendLine("   , BIKOU3                                 ")
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
        SQLJnl.AppendLine("     LNG.LNM0006_TANKA                      ")
        SQLJnl.AppendLine(" WHERE                                      ")
        SQLJnl.AppendLine("       TORICODE  = @TORICODE                ")
        SQLJnl.AppendLine("   AND ORGCODE  = @ORGCODE                  ")
        SQLJnl.AppendLine("   AND KASANORGCODE  = @KASANORGCODE        ")
        SQLJnl.AppendLine("   AND TODOKECODE  = @TODOKECODE            ")
        SQLJnl.AppendLine("   AND STYMD  = @STYMD                      ")
        SQLJnl.AppendLine("   AND BRANCHCODE  = @BRANCHCODE            ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl.ToString, SQLcon)
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
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2)     '枝番
                Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal)     '単価
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYAGATANAME As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATANAME", MySqlDbType.VarChar, 50)     '車型名
                Dim P_SYAGOU As MySqlParameter = SQLcmd.Parameters.Add("@SYAGOU", MySqlDbType.VarChar, 3)     '車号
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                Dim P_SYUBETSU As MySqlParameter = SQLcmd.Parameters.Add("@SYUBETSU", MySqlDbType.VarChar, 20)     '種別
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
                Dim JP_TORICODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim JP_ORGCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim JP_KASANORGCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim JP_TODOKECODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6) '届先コード
                Dim JP_STYMD As MySqlParameter = SQLcmdJnl.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim JP_BRANCHCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番

                Dim LNM0006row As DataRow = LNM0006INPtbl.Rows(0)

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                P_DELFLG.Value = LNM0006row("DELFLG")               '削除フラグ
                P_TORICODE.Value = LNM0006row("TORICODE")           '取引先コード
                P_TORINAME.Value = LNM0006row("TORINAME")           '取引先名称
                P_ORGCODE.Value = LNM0006row("ORGCODE")             '部門コード
                P_ORGNAME.Value = LNM0006row("ORGNAME")             '部門名称
                P_KASANORGCODE.Value = LNM0006row("KASANORGCODE")   '加算先部門コード
                P_KASANORGNAME.Value = LNM0006row("KASANORGNAME")   '加算先部門名称
                P_TODOKECODE.Value = LNM0006row("TODOKECODE")       '届先コード
                P_TODOKENAME.Value = LNM0006row("TODOKENAME")       '届先名称
                P_STYMD.Value = LNM0006row("STYMD")                 '有効開始日
                P_ENDYMD.Value = LNM0006row("ENDYMD")               '有効終了日
                P_BRANCHCODE.Value = LNM0006row("BRANCHCODE")       '枝番
                P_TANKA.Value = LNM0006row("TANKA")                 '単価
                P_SYAGATA.Value = LNM0006row("SYAGATA")             '車型
                P_SYAGATANAME.Value = LNM0006row("SYAGATANAME")     '車型名
                P_SYAGOU.Value = LNM0006row("SYAGOU")               '車号

                If LNM0006row("SYABARA") = "" Then
                    P_SYABARA.Value = DBNull.Value
                Else
                    P_SYABARA.Value = LNM0006row("SYABARA")             '車腹
                End If

                P_SYUBETSU.Value = LNM0006row("SYUBETSU")           '種別
                P_BIKOU1.Value = LNM0006row("BIKOU1")               '備考1
                P_BIKOU2.Value = LNM0006row("BIKOU2")               '備考2
                P_BIKOU3.Value = LNM0006row("BIKOU3")               '備考3

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
                JP_TORICODE.Value = LNM0006row("TORICODE") '取引先コード
                JP_ORGCODE.Value = LNM0006row("ORGCODE") '部門コード
                JP_KASANORGCODE.Value = LNM0006row("KASANORGCODE") '加算先部門コード
                JP_TODOKECODE.Value = LNM0006row("TODOKECODE") '届先コード
                JP_STYMD.Value = LNM0006row("STYMD")           '有効開始日
                JP_BRANCHCODE.Value = LNM0006row("BRANCHCODE") '枝番

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0006UPDtbl) Then
                        LNM0006UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0006UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0006UPDtbl.Clear()
                    LNM0006UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0006UPDrow As DataRow In LNM0006UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0006D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0006UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0006D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0006D UPDATE_INSERT"
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

        '単価マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0006_TANKA")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("       TORICODE  = @TORICODE                ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                  ")
        SQLStr.AppendLine("   AND KASANORGCODE  = @KASANORGCODE        ")
        SQLStr.AppendLine("   AND TODOKECODE  = @TODOKECODE            ")
        SQLStr.AppendLine("   AND STYMD  = @STYMD                      ")
        SQLStr.AppendLine("   AND BRANCHCODE  = @BRANCHCODE            ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6) '届先コード
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番

                Dim LNM0006row As DataRow = LNM0006INPtbl.Rows(0)

                P_TORICODE.Value = LNM0006row("TORICODE") '取引先コード
                P_ORGCODE.Value = LNM0006row("ORGCODE") '部門コード
                P_KASANORGCODE.Value = LNM0006row("KASANORGCODE") '加算先部門コード
                P_TODOKECODE.Value = LNM0006row("TODOKECODE") '届先コード
                P_STYMD.Value = LNM0006row("STYMD")                 '有効開始日
                P_BRANCHCODE.Value = LNM0006row("BRANCHCODE") '枝番

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
                    Else
                        WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0006C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0006C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0005_TANKAHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKENAME  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,BRANCHCODE  ")
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,SYAGATA  ")
        SQLStr.AppendLine("     ,SYAGATANAME  ")
        SQLStr.AppendLine("     ,SYAGOU  ")
        SQLStr.AppendLine("     ,SYABARA  ")
        SQLStr.AppendLine("     ,SYUBETSU  ")
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
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKENAME  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,BRANCHCODE  ")
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,SYAGATA  ")
        SQLStr.AppendLine("     ,SYAGATANAME  ")
        SQLStr.AppendLine("     ,SYAGOU  ")
        SQLStr.AppendLine("     ,SYABARA  ")
        SQLStr.AppendLine("     ,SYUBETSU  ")
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
        SQLStr.AppendLine("        LNG.LNM0006_TANKA")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("       TORICODE  = @TORICODE                ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                  ")
        SQLStr.AppendLine("   AND KASANORGCODE  = @KASANORGCODE        ")
        SQLStr.AppendLine("   AND TODOKECODE  = @TODOKECODE            ")
        SQLStr.AppendLine("   AND STYMD  = @STYMD                      ")
        SQLStr.AppendLine("   AND BRANCHCODE  = @BRANCHCODE            ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6) '届先コード
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0006row As DataRow = LNM0006INPtbl.Rows(0)

                ' DB更新
                P_TORICODE.Value = LNM0006row("TORICODE") '取引先コード
                P_ORGCODE.Value = LNM0006row("ORGCODE") '部門コード
                P_KASANORGCODE.Value = LNM0006row("KASANORGCODE") '加算先部門コード
                P_TODOKECODE.Value = LNM0006row("TODOKECODE") '届先コード
                P_STYMD.Value = LNM0006row("STYMD")           '有効開始日
                P_BRANCHCODE.Value = LNM0006row("BRANCHCODE") '枝番

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0006WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0006tbl.Rows(0)("DELFLG") = "0" And LNM0006row("DELFLG") = "1" Then
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0005_TANKAHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0005_TANKAHIST  INSERT"
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

    ''' <summary>
    ''' 有効終了日更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    Public Sub UpdateENDYMD(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow,
                            ByRef O_MESSAGENO As String, ByVal WW_NOW As String)


        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ更新
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        SQLStr.Append("     LNG.LNM0006_TANKA                       ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     ENDYMD               = @ENDYMD          ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("       TORICODE  = @TORICODE                 ")
        SQLStr.Append("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.Append("   AND KASANORGCODE  = @KASANORGCODE         ")
        SQLStr.Append("   AND TODOKECODE  = @TODOKECODE             ")
        SQLStr.Append("   AND STYMD  = @STYMD                       ")
        SQLStr.Append("   AND BRANCHCODE  = @BRANCHCODE             ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6) '届先コード
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE") '加算先部門コード
                P_TODOKECODE.Value = WW_ROW("TODOKECODE") '届先コード
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                'P_ENDYMD.Value = DateTime.Parse(WW_NEWSTYMD).AddDays(-1).ToString("yyyy/MM/dd") '有効終了日
                P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日
                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE") '枝番
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0006_TANKA UPDATE"
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
            TxtDelFlg.Text = C_DELETE_FLG.DELETE Then

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
        DetailBoxToLNM0006INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0006tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0006tbl, work.WF_SEL_INPTBL.Text)

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
    Protected Sub DetailBoxToLNM0006INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)      '削除フラグ
        Master.EraseCharToIgnore(TxtTORICODE.Text)  '取引先コード
        Master.EraseCharToIgnore(TxtTORINAME.Text)  '取引先名称
        Master.EraseCharToIgnore(ddlSelectORG.SelectedValue)  '部門コード
        Master.EraseCharToIgnore(ddlSelectORG.SelectedValue)  '部門名称
        Master.EraseCharToIgnore(TxtKASANORGCODE.Text)  '加算先部門コード
        Master.EraseCharToIgnore(TxtKASANORGNAME.Text)  '加算先部門名称
        Master.EraseCharToIgnore(TxtTODOKECODE.Text)  '届先コード
        Master.EraseCharToIgnore(TxtTODOKENAME.Text)  '届先名称
        Master.EraseCharToIgnore(WF_StYMD.Value)  '有効開始日
        Master.EraseCharToIgnore(WF_EndYMD.Value)  '有効終了日
        Master.EraseCharToIgnore(TxtBRANCHCODE.Text)  '枝番
        Master.EraseCharToIgnore(TxtTANKA.Text)  '単価
        Master.EraseCharToIgnore(TxtSYAGOU.Text)  '車号
        Master.EraseCharToIgnore(TxtSYABARA.Text)  '車腹
        Master.EraseCharToIgnore(TxtSYUBETSU.Text)  '種別
        Master.EraseCharToIgnore(TxtBIKOU1.Text)  '備考1
        Master.EraseCharToIgnore(TxtBIKOU2.Text)  '備考2
        Master.EraseCharToIgnore(TxtBIKOU3.Text)  '備考3

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) AndAlso
            String.IsNullOrEmpty(TxtDelFlg.Text) Then
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

        Master.CreateEmptyTable(LNM0006INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0006INProw As DataRow = LNM0006INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) Then
            LNM0006INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(TxtSelLineCNT.Text, LNM0006INProw("LINECNT"))
            Catch ex As Exception
                LNM0006INProw("LINECNT") = 0
            End Try
        End If

        LNM0006INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0006INProw("UPDTIMSTP") = 0
        LNM0006INProw("SELECT") = 1
        LNM0006INProw("HIDDEN") = 0

        LNM0006INProw("DELFLG") = TxtDelFlg.Text             '削除フラグ
        LNM0006INProw("TORICODE") = TxtTORICODE.Text         '取引先コード
        LNM0006INProw("TORINAME") = TxtTORINAME.Text         '取引先名称
        LNM0006INProw("ORGCODE") = ddlSelectORG.SelectedValue           '部門コード
        LNM0006INProw("ORGNAME") = ddlSelectORG.SelectedItem           '部門名称
        LNM0006INProw("KASANORGCODE") = TxtKASANORGCODE.Text '加算先部門コード
        LNM0006INProw("KASANORGNAME") = TxtKASANORGNAME.Text '加算先部門名称
        LNM0006INProw("TODOKECODE") = TxtTODOKECODE.Text     '届先コード
        LNM0006INProw("TODOKENAME") = TxtTODOKENAME.Text     '届先名称
        LNM0006INProw("STYMD") = WF_StYMD.Value              '有効開始日
        LNM0006INProw("ENDYMD") = WF_EndYMD.Value            '有効終了日
        LNM0006INProw("BRANCHCODE") = TxtBRANCHCODE.Text     '枝番
        LNM0006INProw("TANKA") = TxtTANKA.Text               '単価
        LNM0006INProw("SYAGATA") = ddlSelectSYAGATA.SelectedValue           '車型
        LNM0006INProw("SYAGATANAME") = ddlSelectSYAGATA.SelectedItem        '車型名
        LNM0006INProw("SYAGOU") = TxtSYAGOU.Text             '車号
        LNM0006INProw("SYABARA") = TxtSYABARA.Text           '車腹
        LNM0006INProw("SYUBETSU") = TxtSYUBETSU.Text         '種別
        LNM0006INProw("BIKOU1") = TxtBIKOU1.Text             '備考1
        LNM0006INProw("BIKOU2") = TxtBIKOU2.Text             '備考2
        LNM0006INProw("BIKOU3") = TxtBIKOU3.Text             '備考3

        '○ チェック用テーブルに登録する
        LNM0006INPtbl.Rows.Add(LNM0006INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0006INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0006INProw As DataRow = LNM0006INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0006row As DataRow In LNM0006tbl.Rows
            ' KEY項目が等しい時
            If LNM0006row("TORICODE") = LNM0006INProw("TORICODE") AndAlso
                LNM0006row("ORGCODE") = LNM0006INProw("ORGCODE") AndAlso
                LNM0006row("KASANORGCODE") = LNM0006INProw("KASANORGCODE") AndAlso
                LNM0006row("TODOKECODE") = LNM0006INProw("TODOKECODE") AndAlso
                LNM0006row("STYMD") = LNM0006INProw("STYMD") AndAlso
                LNM0006row("BRANCHCODE") = LNM0006INProw("BRANCHCODE") Then
                ' KEY項目以外の項目の差異をチェック
                If LNM0006row("DELFLG") = LNM0006INProw("DELFLG") AndAlso
                    LNM0006row("TORINAME") = LNM0006INProw("TORINAME") AndAlso
                    LNM0006row("ORGNAME") = LNM0006INProw("ORGNAME") AndAlso
                    LNM0006row("KASANORGNAME") = LNM0006INProw("KASANORGNAME") AndAlso
                    LNM0006row("TODOKENAME") = LNM0006INProw("TODOKENAME") AndAlso
                    LNM0006row("ENDYMD") = LNM0006INProw("ENDYMD") AndAlso
                    LNM0006row("TANKA") = LNM0006INProw("TANKA") AndAlso
                    LNM0006row("SYAGATA") = LNM0006INProw("SYAGATA") AndAlso
                    LNM0006row("SYAGOU") = LNM0006INProw("SYAGOU") AndAlso
                    LNM0006row("SYABARA") = LNM0006INProw("SYABARA") AndAlso
                    LNM0006row("SYUBETSU") = LNM0006INProw("SYUBETSU") AndAlso
                    LNM0006row("BIKOU1") = LNM0006INProw("BIKOU1") AndAlso
                    LNM0006row("BIKOU2") = LNM0006INProw("BIKOU2") AndAlso
                    LNM0006row("BIKOU3") = LNM0006INProw("BIKOU3") Then
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
        For Each LNM0006row As DataRow In LNM0006tbl.Rows
            Select Case LNM0006row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0006tbl, work.WF_SEL_INPTBL.Text)

        TxtSelLineCNT.Text = ""              'LINECNT
        TxtMapId.Text = "M00001"             '画面ＩＤ
        TxtDelFlg.Text = ""                  '削除フラグ
        TxtTORICODE.Text = ""                '取引先コード
        TxtTORINAME.Text = ""                '取引先名称
        ddlSelectORG.SelectedValue = ""                 '部門コード
        ddlSelectORG.SelectedValue = ""                 '部門名称
        TxtKASANORGCODE.Text = ""            '加算先部門コード
        TxtKASANORGNAME.Text = ""            '加算先部門名称
        TxtTODOKECODE.Text = ""              '届先コード
        TxtTODOKENAME.Text = ""              '届先名称
        WF_StYMD.Value = ""                  '有効開始日
        WF_EndYMD.Value = ""                 '有効終了日
        TxtBRANCHCODE.Text = ""              '枝番
        TxtTANKA.Text = ""                   '単価
        ddlSelectSYAGATA.SelectedValue = ""                 '車型
        TxtSYAGOU.Text = ""                  '車号
        TxtSYABARA.Text = ""                 '車腹
        TxtSYUBETSU.Text = ""                '種別
        TxtBIKOU1.Text = ""                  '備考1
        TxtBIKOU2.Text = ""                  '備考2
        TxtBIKOU3.Text = ""                  '備考3

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
            Case "TxtDelFlg"      '削除フラグ
                CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
                TxtDelFlg.Focus()
            Case "TxtTORICODE"
                CODENAME_get("TORICODE", TxtTORICODE.Text, TxtTORINAME.Text, WW_RtnSW)  '取引先コード
                TxtTORICODE.Focus()
            Case "TxtKASANORGCODE"
                CODENAME_get("KASANORGCODE", TxtKASANORGCODE.Text, TxtKASANORGNAME.Text, WW_RtnSW)  '加算先部門コード
                TxtKASANORGCODE.Focus()
            Case "TxtTODOKECODE"
                CODENAME_get("TODOKECODE", TxtTODOKECODE.Text, TxtTODOKENAME.Text, WW_RtnSW)  '届先コード
                TxtKASANORGCODE.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 単価マスタ更新(削除フラグのみ)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UpdateMasterDelflgOnly()
        Dim WW_MODIFYKBN As String = ""
        Dim WW_DATE As Date = Date.Now

        '初期化
        LNM0006INPtbl = New DataTable
        LNM0006INPtbl.Columns.Add("TORICODE")
        LNM0006INPtbl.Columns.Add("ORGCODE")
        LNM0006INPtbl.Columns.Add("KASANORGCODE")
        LNM0006INPtbl.Columns.Add("TODOKECODE")
        LNM0006INPtbl.Columns.Add("BRANCHCODE")
        LNM0006INPtbl.Columns.Add("STYMD")
        LNM0006INPtbl.Columns.Add("DELFLG")

        Dim row As DataRow
        row = LNM0006INPtbl.NewRow
        row("TORICODE") = TxtTORICODE.Text
        row("ORGCODE") = ddlSelectORG.SelectedValue
        row("KASANORGCODE") = TxtKASANORGCODE.Text
        row("TODOKECODE") = TxtTODOKECODE.Text
        row("STYMD") = WF_StYMD.Value
        row("BRANCHCODE") = TxtBRANCHCODE.Text
        row("DELFLG") = C_DELETE_FLG.DELETE
        LNM0006INPtbl.Rows.Add(row)

        ' DB更新処理
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()

            '履歴テーブルに変更前データを登録
            InsertHist(SQLcon, LNM0006WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '削除フラグ更新
            SetDelflg(SQLcon, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '履歴テーブルに変更後データを登録
            InsertHist(SQLcon, LNM0006WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

        End Using

        '○ 入力値反映
        For Each LNM0006INProw As DataRow In LNM0006INPtbl.Rows
            For Each LNM0006row As DataRow In LNM0006tbl.Rows
                If LNM0006INProw("TORICODE") = LNM0006row("TORICODE") AndAlso
                    LNM0006INProw("ORGCODE") = LNM0006row("ORGCODE") AndAlso
                    LNM0006INProw("KASANORGCODE") = LNM0006row("KASANORGCODE") AndAlso
                    LNM0006INProw("TODOKECODE") = LNM0006row("TODOKECODE") AndAlso
                    LNM0006INProw("STYMD") = LNM0006row("STYMD") AndAlso
                    LNM0006INProw("BRANCHCODE") = LNM0006row("BRANCHCODE") Then
                    ' 画面入力テーブル項目設定              
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0006row("DELFLG") = LNM0006INProw("DELFLG")
                    LNM0006row("SELECT") = 0
                    LNM0006row("HIDDEN") = 0
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
        SQLStr.Append("     LNG.LNM0006_TANKA                       ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("       TORICODE  = @TORICODE                 ")
        SQLStr.Append("   AND ORGCODE  = @ORGCODE                   ")
        SQLStr.Append("   AND KASANORGCODE  = @KASANORGCODE         ")
        SQLStr.Append("   AND TODOKECODE  = @TODOKECODE             ")
        SQLStr.Append("   AND STYMD  = @STYMD                       ")
        SQLStr.Append("   AND BRANCHCODE  = @BRANCHCODE             ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6) '届先コード
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                Dim LNM0006row As DataRow = LNM0006INPtbl.Rows(0)
                P_TORICODE.Value = LNM0006row("TORICODE") '取引先コード
                P_ORGCODE.Value = LNM0006row("ORGCODE") '部門コード
                P_KASANORGCODE.Value = LNM0006row("KASANORGCODE") '加算先部門コード
                P_TODOKECODE.Value = LNM0006row("TODOKECODE") '届先コード
                P_STYMD.Value = LNM0006row("STYMD")           '有効開始日
                P_BRANCHCODE.Value = LNM0006row("BRANCHCODE") '枝番
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0006C UPDATE"
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
                Case "TxtDelFlg"      '削除フラグ
                    TxtDelFlg.Text = WW_SelectValue
                    LblDelFlgName.Text = WW_SelectText
                    TxtDelFlg.Focus()
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
                Case "TxtDelFlg"            '削除フラグ
                    TxtDelFlg.Focus()
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

        '情シス、高圧ガス以外
        If LNM0006WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            Me.mspToriCodeSingle.SQL = CmnSearchSQL.GetTankaToriSQL(ddlSelectORG.SelectedValue)
        Else
            Me.mspToriCodeSingle.SQL = CmnSearchSQL.GetTankaToriSQL()
        End If

        Me.mspToriCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspToriCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetTankaToriTitle)

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


        '情シス、高圧ガス以外
        If LNM0006WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            Me.mspKasanOrgCodeSingle.SQL = CmnSearchSQL.GetTankaKasanOrgSQL(ddlSelectORG.SelectedValue)
        Else
            Me.mspKasanOrgCodeSingle.SQL = CmnSearchSQL.GetTankaKasanOrgSQL()
        End If

        Me.mspKasanOrgCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspKasanOrgCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetTankaKasanOrgTitle)

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

        '情シス、高圧ガス以外
        If LNM0006WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            Me.mspTodokeCodeSingle.SQL = CmnSearchSQL.GetTankaTodokeSQL(ddlSelectORG.SelectedValue)
        Else
            Me.mspTodokeCodeSingle.SQL = CmnSearchSQL.GetTankaTodokeSQL()
        End If

        Me.mspTodokeCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspTodokeCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetTankaTodokeTitle)

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
            WW_CheckMES1 = "・単価マスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0006INProw As DataRow In LNM0006INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0006INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0006INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "TORICODE", LNM0006INProw("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 取引先名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORINAME", LNM0006INProw("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 部門コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ORGCODE", LNM0006INProw("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・部門コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 加算先部門コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KASANORGCODE", LNM0006INProw("KASANORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・加算先部門コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 加算先部門名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KASANORGNAME", LNM0006INProw("KASANORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・加算先部門名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 届先コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TODOKECODE", LNM0006INProw("TODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・届先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 届先名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TODOKENAME", LNM0006INProw("TODOKENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・届先名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 有効開始日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "STYMD", LNM0006INProw("STYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                LNM0006INProw("STYMD") = CDate(LNM0006INProw("STYMD")).ToString("yyyy/MM/dd")
            Else
                WW_CheckMES1 = "・有効開始日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 有効終了日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ENDYMD", LNM0006INProw("ENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                LNM0006INProw("ENDYMD") = CDate(LNM0006INProw("ENDYMD")).ToString("yyyy/MM/dd")
            Else
                WW_CheckMES1 = "・有効終了日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 単価(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TANKA", LNM0006INProw("TANKA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・単価エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 車号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SYAGOU", LNM0006INProw("SYAGOU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・車号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 車腹(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SYABARA", LNM0006INProw("SYABARA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・車腹エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 種別(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SYUBETSU", LNM0006INProw("SYUBETSU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・種別エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 備考1(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIKOU1", LNM0006INProw("BIKOU1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・備考1エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 備考2(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIKOU2", LNM0006INProw("BIKOU2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・備考2エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 備考3(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIKOU3", LNM0006INProw("BIKOU3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・備考3エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 日付大小チェック
            If Not String.IsNullOrEmpty(LNM0006INProw("STYMD")) AndAlso
                    Not String.IsNullOrEmpty(LNM0006INProw("ENDYMD")) Then
                If CDate(LNM0006INProw("STYMD")) > CDate(LNM0006INProw("ENDYMD")) Then
                    WW_CheckMES1 = "・有効開始日＆有効終了日エラーです。"
                    WW_CheckMES2 = "日付大小入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_TORICODE.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text, work.WF_SEL_KASANORGCODE.Text,
                                    work.WF_SEL_TODOKECODE.Text, work.WF_SEL_STYMD.Text, work.WF_SEL_BRANCHCODE.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（取引先コード & 部門コード & 加算先部門コード & 届先コード & 有効開始日 & 枝番）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0006INProw("TORICODE") & "]" &
                                           "([" & LNM0006INProw("ORGCODE") & "]" &
                                           "([" & LNM0006INProw("KASANORGCODE") & "]" &
                                           "([" & LNM0006INProw("TODOKECODE") & "]" &
                                           "([" & LNM0006INProw("STYMD") & "]" &
                                           " [" & LNM0006INProw("BRANCHCODE") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If

            If String.IsNullOrEmpty(WW_LineErr) Then
                If LNM0006INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0006INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0006INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0006INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0006tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0006tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0006row As DataRow In LNM0006tbl.Rows
            Select Case LNM0006row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0006INProw As DataRow In LNM0006INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0006INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0006INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0006row As DataRow In LNM0006tbl.Rows
                ' KEY項目が等しい時
                If LNM0006row("TORICODE") = LNM0006INProw("TORICODE") AndAlso
                    LNM0006row("ORGCODE") = LNM0006INProw("ORGCODE") AndAlso
                    LNM0006row("KASANORGCODE") = LNM0006INProw("KASANORGCODE") AndAlso
                    LNM0006row("TODOKECODE") = LNM0006INProw("TODOKECODE") AndAlso
                    LNM0006row("BRANCHCODE") = LNM0006INProw("BRANCHCODE") Then
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0006row("DELFLG") = LNM0006INProw("DELFLG") AndAlso
                        LNM0006row("TORINAME") = LNM0006INProw("TORINAME") AndAlso
                        LNM0006row("ORGNAME") = LNM0006INProw("ORGNAME") AndAlso
                        LNM0006row("KASANORGNAME") = LNM0006INProw("KASANORGNAME") AndAlso
                        LNM0006row("TODOKENAME") = LNM0006INProw("TODOKENAME") AndAlso
                        LNM0006row("STYMD") = LNM0006INProw("STYMD") AndAlso
                        LNM0006row("ENDYMD") = LNM0006INProw("ENDYMD") AndAlso
                        LNM0006row("TANKA") = LNM0006INProw("TANKA") AndAlso
                        LNM0006row("SYAGATA") = LNM0006INProw("SYAGATA") AndAlso
                        LNM0006row("SYAGOU") = LNM0006INProw("SYAGOU") AndAlso
                        LNM0006row("SYABARA") = LNM0006INProw("SYABARA") AndAlso
                        LNM0006row("SYUBETSU") = LNM0006INProw("SYUBETSU") AndAlso
                        LNM0006row("BIKOU1") = LNM0006INProw("BIKOU1") AndAlso
                        LNM0006row("BIKOU2") = LNM0006INProw("BIKOU2") AndAlso
                        LNM0006row("BIKOU3") = LNM0006INProw("BIKOU3") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0006row("OPERATION")) Then

                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0006INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0006INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0006INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0006INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0006INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now
                Dim WW_DBDataCheck As String = ""
                Dim WW_STYMD_SAVE As String = ""
                Dim WW_ENDYMD_SAVE As String = ""
                Dim WW_PASTSTYMD As String = "" '過去有効開始日格納
                Dim WW_PASTENDYMD As String = "" '過去有効終了日格納

                '枝番が新規、有効開始日が変更されたときの対応
                Select Case True
                    Case LNM0006INPtbl.Rows(0)("BRANCHCODE").ToString = "" '枝番なし(新規の場合)
                        '枝番を生成
                        LNM0006INPtbl.Rows(0)("BRANCHCODE") = LNM0006WRKINC.GenerateBranchCode(SQLcon, LNM0006INPtbl.Rows(0), WW_DBDataCheck)
                        If Not isNormal(WW_DBDataCheck) Then
                            Exit Sub
                        End If
                    Case LNM0006tbl.Rows(0)("STYMD") < LNM0006INPtbl.Rows(0)("STYMD") '更新前有効開始日 <　入力有効開始日
                        '変更後の有効開始日、有効終了日退避
                        WW_STYMD_SAVE = LNM0006INPtbl.Rows(0)("STYMD")
                        WW_ENDYMD_SAVE = LNM0006INPtbl.Rows(0)("ENDYMD")
                        '変更後テーブルに変更前の有効開始日格納
                        LNM0006INPtbl.Rows(0)("STYMD") = LNM0006tbl.Rows(0)("STYMD")
                        '変更後テーブルに更新用の有効終了日格納
                        LNM0006INPtbl.Rows(0)("ENDYMD") = DateTime.Parse(WW_STYMD_SAVE).AddDays(-1).ToString("yyyy/MM/dd")

                        '履歴テーブルに変更前データを登録
                        InsertHist(SQLcon, LNM0006WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
                        If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                            Exit Sub
                        End If
                        '変更前の有効終了日更新
                        UpdateENDYMD(SQLcon, LNM0006INPtbl.Rows(0), WW_DBDataCheck, WW_DATE)
                        If Not isNormal(WW_DBDataCheck) Then
                            Exit Sub
                        End If
                        '履歴テーブルに変更後データを登録
                        InsertHist(SQLcon, LNM0006WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
                        If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                            Exit Sub
                        End If
                        '退避した有効開始日、有効終了日を元に戻す
                        LNM0006INPtbl.Rows(0)("STYMD") = WW_STYMD_SAVE
                        LNM0006INPtbl.Rows(0)("ENDYMD") = WW_ENDYMD_SAVE
                    Case LNM0006tbl.Rows(0)("STYMD") > LNM0006INPtbl.Rows(0)("STYMD") '更新前有効開始日 >　入力有効開始日
                        '過去の有効開始日、有効終了日取得
                        work.GetPastSTENDYMD(SQLcon, LNM0006INPtbl.Rows(0), WW_PASTSTYMD, WW_PASTENDYMD)
                        '取得できた場合
                        If Not WW_PASTSTYMD = "" Then
                            '変更後の有効開始日退避
                            WW_STYMD_SAVE = LNM0006INPtbl.Rows(0)("STYMD")
                            '変更後テーブルに取得した有効開始日格納
                            LNM0006INPtbl.Rows(0)("STYMD") = WW_PASTSTYMD
                            '変更後テーブルに更新用の有効終了日格納
                            LNM0006INPtbl.Rows(0)("ENDYMD") = DateTime.Parse(WW_STYMD_SAVE).AddDays(-1).ToString("yyyy/MM/dd")
                            '履歴テーブルに変更前データを登録
                            InsertHist(SQLcon, LNM0006WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
                            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                Exit Sub
                            End If
                            '変更前の有効終了日更新
                            UpdateENDYMD(SQLcon, LNM0006INPtbl.Rows(0), WW_DBDataCheck, WW_DATE)
                            If Not isNormal(WW_DBDataCheck) Then
                                Exit Sub
                            End If
                            '履歴テーブルに変更後データを登録
                            InsertHist(SQLcon, LNM0006WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
                            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                Exit Sub
                            End If
                            '退避した有効開始日を元に戻す
                            LNM0006INPtbl.Rows(0)("STYMD") = WW_STYMD_SAVE
                            '有効終了日を取得した過去の有効終了日に設定する
                            LNM0006INPtbl.Rows(0)("ENDYMD") = WW_PASTENDYMD
                        Else
                            '取得できなかった場合(過去のデータが1件もなかった場合)
                            '有効終了日を変更前の開始日-1にする
                            LNM0006INPtbl.Rows(0)("ENDYMD") = DateTime.Parse(LNM0006tbl.Rows(0)("STYMD")).AddDays(-1).ToString("yyyy/MM/dd")
                        End If
                End Select

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
        For Each LNM0006INProw As DataRow In LNM0006INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0006row As DataRow In LNM0006tbl.Rows
                ' 同一レコードか判定
                If LNM0006INProw("TORICODE") = LNM0006row("TORICODE") AndAlso
                    LNM0006INProw("ORGCODE") = LNM0006row("ORGCODE") AndAlso
                    LNM0006INProw("KASANORGCODE") = LNM0006row("KASANORGCODE") AndAlso
                    LNM0006INProw("TODOKECODE") = LNM0006row("TODOKECODE") AndAlso
                    LNM0006INProw("BRANCHCODE") = LNM0006row("BRANCHCODE") Then
                    ' 画面入力テーブル項目設定
                    LNM0006INProw("LINECNT") = LNM0006row("LINECNT")
                    LNM0006INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0006INProw("UPDTIMSTP") = LNM0006row("UPDTIMSTP")
                    LNM0006INProw("SELECT") = 0
                    LNM0006INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0006row.ItemArray = LNM0006INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0006tbl.NewRow
                WW_NRow.ItemArray = LNM0006INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0006tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0006tbl.Rows.Add(WW_NRow)
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
