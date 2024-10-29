''************************************************************
' 支払先マスタメンテ登録画面
' 作成日 2024/05/15
' 更新日
' 作成者 大浜
' 更新者 
'
' 修正履歴:2024/05/15 新規作成
'         :2024/08/02 星 顧客コード、顧客名自動入力処理追加
'         :2024/09/17 インボイス登録番号13桁から14桁に変更
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 支払先マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNT0023PayeeLinkDetail
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private LNT0023tbl As DataTable                                 '一覧格納用テーブル
    Private LNT0023INPtbl As DataTable                              'チェック用テーブル
    Private LNT0023UPDtbl As DataTable                              '更新用テーブル

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

                    '2024/08/02 星ADD START
                    '自動入力項目保存
                    If work.WF_SEL_TORICODE.Text <> TxtToriCode.Text Then
                        TxtClientCode.Text = "01-" + TxtToriCode.Text + "-1"
                    ElseIf TxtToriCode.Text = "" Then
                        TxtClientCode.Text = ""
                    End If
                    TxtClientName.Text = TxtToriName.Text + TxtToriDivName.Text
                    '2024/08/02 星ADD END

                    '○ 画面表示データ復元
                    Master.RecoverTable(LNT0023tbl, work.WF_SEL_INPTBL.Text)

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
                        Case "mspBankCodeSingleRowSelected"  '[共通]銀行コード選択ポップアップで行選択
                            RowSelected_mspBankCodeSingle()
                        Case "mspBankBranchCodeSingleRowSelected"  '[共通]支店コード選択ポップアップで行選択
                            RowSelected_mspBankBranchCodeSingle()
                        Case "mspBankAccountSingleRowSelected"  '[共通]支払元銀行選択ポップアップで行選択
                            RowSelected_mspBankAccountSingle()
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
            If Not IsNothing(LNT0023tbl) Then
                LNT0023tbl.Clear()
                LNT0023tbl.Dispose()
                LNT0023tbl = Nothing
            End If

            If Not IsNothing(LNT0023INPtbl) Then
                LNT0023INPtbl.Clear()
                LNT0023INPtbl.Dispose()
                LNT0023INPtbl = Nothing
            End If

            If Not IsNothing(LNT0023UPDtbl) Then
                LNT0023UPDtbl.Clear()
                LNT0023UPDtbl.Dispose()
                LNT0023UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNT0023WRKINC.MAPIDD
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
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0023L Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        LblSelLineCNT.Text = work.WF_SEL_LINECNT.Text
        '削除
        TxtDelFlg.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy, WW_Dummy)
        '画面ＩＤ
        TxtMapId.Text = "M00001"
        '支払先コード
        TxtToriCode.Text = work.WF_SEL_TORICODE.Text
        '顧客コード
        TxtClientCode.Text = work.WF_SEL_CLIENTCODE.Text
        'インボイス登録番号
        TxtInvoiceNumber.Text = work.WF_SEL_INVOICENUMBER.Text
        '顧客名
        TxtClientName.Text = work.WF_SEL_CLIENTNAME.Text
        '会社名
        TxtToriName.Text = work.WF_SEL_TORINAME.Text
        '部門名
        TxtToriDivName.Text = work.WF_SEL_TORIDIVNAME.Text
        '振込先銀行コード
        TxtPayBankCode.Text = work.WF_SEL_PAYBANKCODE.Text
        '振込先銀行名
        TxtPayBankName.Text = work.WF_SEL_PAYBANKNAME.Text
        '振込先銀行名カナ
        TxtPayBankNameKana.Text = work.WF_SEL_PAYBANKNAMEKANA.Text
        '振込先支店コード
        TxtPayBankBranchCode.Text = work.WF_SEL_PAYBANKBRANCHCODE.Text
        '振込先支店名
        TxtPayBankBranchName.Text = work.WF_SEL_PAYBANKBRANCHNAME.Text
        '振込先支店名カナ
        TxtPayBankBranchNameKana.Text = work.WF_SEL_PAYBANKBRANCHNAMEKANA.Text
        '預金種別
        'TxtPayAccountTypeName.Text = work.WF_SEL_PAYACCOUNTTYPENAME.Text
        ddlPayAccountTypeName.SelectedValue = work.WF_SEL_PAYACCOUNTTYPENAME.Text

        '預金種別コード
        TxtPayAccountType.Text = work.WF_SEL_PAYACCOUNTTYPE.Text
        '口座番号
        TxtPayAccount.Text = work.WF_SEL_PAYACCOUNT.Text
        '口座名義
        TxtPayAccountName.Text = work.WF_SEL_PAYACCOUNTNAME.Text
        '支払元銀行コード
        TxtPayorBankCode.Text = work.WF_SEL_PAYORBANKCODE.Text
        CODENAME_get("PAYORBANKCODE", TxtPayorBankCode.Text, LblPayorBankName.Text, WW_Dummy, WW_Dummy)

        '消費税計算処理区分
        'TxtPayTaxCalcUnit.Text = work.WF_SEL_PAYTAXCALCUNIT.Text
        ddlPayTaxCalcUnit.SelectedValue = work.WF_SEL_PAYTAXCALCUNIT.Text

        '連携状態区分
        TxtLinkStatus.Text = work.WF_SEL_LINKSTATUS.Text

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_TORICODE.Text

        ' 数値(0～9)のみ入力可能とする。
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"                    '削除フラグ
        Me.TxtToriCode.Attributes("onkeyPress") = "CheckNum()"                  '支払先コード
        Me.TxtPayBankCode.Attributes("onkeyPress") = "CheckNum()"               '振込先銀行コード
        Me.TxtPayBankBranchCode.Attributes("onkeyPress") = "CheckNum()"         '振込先支店コード
        Me.TxtPayAccountType.Attributes("onkeyPress") = "CheckNum()"            '預金種別コード
        Me.TxtPayAccount.Attributes("onkeyPress") = "CheckNum()"                '口座番号
        Me.TxtPayorBankCode.Attributes("onkeyPress") = "CheckNum()"             '支払元銀行コード
        Me.TxtLinkStatus.Attributes("onkeyPress") = "CheckNum()"                '連携状態区分

        ' 入力するテキストボックスは数値(0～9、ハイフン(-))のみ可能とする。
        Me.TxtClientCode.Attributes("onkeyPress") = "CheckTel()"               '顧客コード

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO"></param>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                     ")
        SQLStr.AppendLine("     TORICODE               ")
        SQLStr.AppendLine("   , CLIENTCODE             ")
        SQLStr.AppendLine(" FROM                       ")
        SQLStr.AppendLine("     LNG.LNT0072_PAYEE     ")
        SQLStr.AppendLine(" WHERE                      ")
        SQLStr.AppendLine("         TORICODE      = @TORICODE ")
        SQLStr.AppendLine("     AND CLIENTCODE     = @CLIENTCODE ")
        SQLStr.AppendLine("     AND DELFLG      <> @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '支払先コード
                Dim P_CLIENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15)     '顧客コード
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)              '削除フラグ

                P_TORICODE.Value = TxtToriCode.Text       '支払先コード
                P_CLIENTCODE.Value = TxtClientCode.Text   '顧客コード
                P_DELFLG.Value = C_DELETE_FLG.DELETE      '削除フラグ

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNT0023Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0023Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0023Chk.Load(SQLdr)

                    If LNT0023Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0023C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0023C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 支払先マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(支払先マスタ)
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" DECLARE @hensuu AS bigint ;                                        ")
        SQLStr.AppendLine("     SET @hensuu = 0 ;                                              ")
        SQLStr.AppendLine(" DECLARE hensuu CURSOR FOR                                          ")
        SQLStr.AppendLine("     SELECT                                                         ")
        SQLStr.AppendLine("         UPDTIMSTP AS hensuu                                        ")
        SQLStr.AppendLine("     FROM                                                           ")
        SQLStr.AppendLine("         LNG.LNT0072_PAYEE                                          ")
        SQLStr.AppendLine("     WHERE                                                          ")
        SQLStr.AppendLine("         TORICODE         = @TORICODE                               ")
        SQLStr.AppendLine("     AND CLIENTCODE       = @CLIENTCODE                             ")
        SQLStr.AppendLine(" OPEN hensuu ;                                                      ")
        SQLStr.AppendLine(" FETCH NEXT FROM hensuu INTO @hensuu ;                              ")
        SQLStr.AppendLine(" IF (@@FETCH_STATUS = 0)                                            ")
        SQLStr.AppendLine("     UPDATE LNG.LNT0072_PAYEE                                       ")
        SQLStr.AppendLine("     SET                                                            ")
        SQLStr.AppendLine("         DELFLG                  = @DELFLG                          ")
        SQLStr.AppendLine("       , TORICODE                = @TORICODE                        ")
        SQLStr.AppendLine("       , CLIENTCODE              = @CLIENTCODE                      ")
        SQLStr.AppendLine("       , INVOICENUMBER           = @INVOICENUMBER                   ")
        SQLStr.AppendLine("       , CLIENTNAME              = @CLIENTNAME                      ")
        SQLStr.AppendLine("       , TORINAME                = @TORINAME                        ")
        SQLStr.AppendLine("       , TORIDIVNAME             = @TORIDIVNAME                     ")
        SQLStr.AppendLine("       , PAYBANKCODE             = @PAYBANKCODE                     ")
        SQLStr.AppendLine("       , PAYBANKNAME             = @PAYBANKNAME                     ")
        SQLStr.AppendLine("       , PAYBANKNAMEKANA         = @PAYBANKNAMEKANA                 ")
        SQLStr.AppendLine("       , PAYBANKBRANCHCODE       = @PAYBANKBRANCHCODE               ")
        SQLStr.AppendLine("       , PAYBANKBRANCHNAME       = @PAYBANKBRANCHNAME               ")
        SQLStr.AppendLine("       , PAYBANKBRANCHNAMEKANA   = @PAYBANKBRANCHNAMEKANA           ")
        SQLStr.AppendLine("       , PAYACCOUNTTYPENAME      = @PAYACCOUNTTYPENAME              ")
        SQLStr.AppendLine("       , PAYACCOUNTTYPE          = @PAYACCOUNTTYPE                  ")
        SQLStr.AppendLine("       , PAYACCOUNT              = @PAYACCOUNT                      ")
        SQLStr.AppendLine("       , PAYACCOUNTNAME          = @PAYACCOUNTNAME                  ")
        SQLStr.AppendLine("       , PAYORBANKCODE           = @PAYORBANKCODE                   ")
        SQLStr.AppendLine("       , PAYTAXCALCUNIT          = @PAYTAXCALCUNIT                  ")
        SQLStr.AppendLine("       , LINKSTATUS              = @LINKSTATUS                      ")
        'SQLStr.AppendLine("       , LASTLINKYMD             = @LASTLINKYMD                     ")
        SQLStr.AppendLine("       , UPDYMD                  = @UPDYMD                          ")
        SQLStr.AppendLine("       , UPDUSER                 = @UPDUSER                         ")
        SQLStr.AppendLine("       , UPDTERMID               = @UPDTERMID                       ")
        SQLStr.AppendLine("       , UPDPGID                 = @UPDPGID                         ")
        SQLStr.AppendLine("       , RECEIVEYMD              = @RECEIVEYMD                      ")
        SQLStr.AppendLine("     WHERE                                                          ")
        SQLStr.AppendLine("         TORICODE        = @TORICODE                                ")
        SQLStr.AppendLine("     AND CLIENTCODE      = @CLIENTCODE                              ")
        SQLStr.AppendLine(" IF (@@FETCH_STATUS <> 0)                                           ")
        SQLStr.AppendLine("     INSERT INTO LNG.LNT0072_PAYEE                                  ")
        SQLStr.AppendLine("        (DELFLG                                                     ")
        SQLStr.AppendLine("        ,TORICODE                                                   ")
        SQLStr.AppendLine("        ,CLIENTCODE                                                 ")
        SQLStr.AppendLine("        ,INVOICENUMBER                                              ")
        SQLStr.AppendLine("        ,CLIENTNAME                                                 ")
        SQLStr.AppendLine("        ,TORINAME                                                   ")
        SQLStr.AppendLine("        ,TORIDIVNAME                                                ")
        SQLStr.AppendLine("        ,PAYBANKCODE                                                ")
        SQLStr.AppendLine("        ,PAYBANKNAME                                                ")
        SQLStr.AppendLine("        ,PAYBANKNAMEKANA                                            ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCODE                                          ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAME                                          ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAMEKANA                                      ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPENAME                                         ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE                                             ")
        SQLStr.AppendLine("        ,PAYACCOUNT                                                 ")
        SQLStr.AppendLine("        ,PAYACCOUNTNAME                                             ")
        SQLStr.AppendLine("        ,PAYORBANKCODE                                              ")
        SQLStr.AppendLine("        ,PAYTAXCALCUNIT                                             ")
        SQLStr.AppendLine("        ,LINKSTATUS                                                 ")
        SQLStr.AppendLine("        ,LASTLINKYMD                                                ")
        SQLStr.AppendLine("       , INITYMD                                                    ")
        SQLStr.AppendLine("       , INITUSER                                                   ")
        SQLStr.AppendLine("       , INITTERMID                                                 ")
        SQLStr.AppendLine("       , INITPGID                                                   ")
        SQLStr.AppendLine("       , RECEIVEYMD)                                                ")
        SQLStr.AppendLine("     VALUES                                                         ")
        SQLStr.AppendLine("        (@DELFLG                                                    ")
        SQLStr.AppendLine("        ,@TORICODE                                                  ")
        SQLStr.AppendLine("        ,@CLIENTCODE                                                ")
        SQLStr.AppendLine("        ,@INVOICENUMBER                                             ")
        SQLStr.AppendLine("        ,@CLIENTNAME                                                ")
        SQLStr.AppendLine("        ,@TORINAME                                                  ")
        SQLStr.AppendLine("        ,@TORIDIVNAME                                               ")
        SQLStr.AppendLine("        ,@PAYBANKCODE                                               ")
        SQLStr.AppendLine("        ,@PAYBANKNAME                                               ")
        SQLStr.AppendLine("        ,@PAYBANKNAMEKANA                                           ")
        SQLStr.AppendLine("        ,@PAYBANKBRANCHCODE                                         ")
        SQLStr.AppendLine("        ,@PAYBANKBRANCHNAME                                         ")
        SQLStr.AppendLine("        ,@PAYBANKBRANCHNAMEKANA                                     ")
        SQLStr.AppendLine("        ,@PAYACCOUNTTYPENAME                                        ")
        SQLStr.AppendLine("        ,@PAYACCOUNTTYPE                                            ")
        SQLStr.AppendLine("        ,@PAYACCOUNT                                                ")
        SQLStr.AppendLine("        ,@PAYACCOUNTNAME                                            ")
        SQLStr.AppendLine("        ,@PAYORBANKCODE                                             ")
        SQLStr.AppendLine("        ,@PAYTAXCALCUNIT                                            ")
        SQLStr.AppendLine("        ,@LINKSTATUS                                                ")
        SQLStr.AppendLine("        ,@LASTLINKYMD                                               ")
        SQLStr.AppendLine("       , @INITYMD                                                   ")
        SQLStr.AppendLine("       , @INITUSER                                                  ")
        SQLStr.AppendLine("       , @INITTERMID                                                ")
        SQLStr.AppendLine("       , @INITPGID                                                  ")
        SQLStr.AppendLine("       , @RECEIVEYMD) ;                                             ")
        SQLStr.AppendLine(" CLOSE hensuu ;                                                     ")
        SQLStr.AppendLine(" DEALLOCATE hensuu ;                                                ")

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl = New StringBuilder
        SQLJnl.AppendLine(" Select                                     ")
        SQLJnl.AppendLine("    DELFLG                                  ")
        SQLJnl.AppendLine("  ,TORICODE                                 ")
        SQLJnl.AppendLine("  ,CLIENTCODE                               ")
        SQLJnl.AppendLine("  ,INVOICENUMBER                            ")
        SQLJnl.AppendLine("  ,CLIENTNAME                               ")
        SQLJnl.AppendLine("  ,TORINAME                                 ")
        SQLJnl.AppendLine("  ,TORIDIVNAME                              ")
        SQLJnl.AppendLine("  , PAYBANKCODE                             ")
        SQLJnl.AppendLine("  , PAYBANKNAME                             ")
        SQLJnl.AppendLine("  , PAYBANKNAMEKANA                         ")
        SQLJnl.AppendLine("  , PAYBANKBRANCHCODE                       ")
        SQLJnl.AppendLine("  , PAYBANKBRANCHNAME                       ")
        SQLJnl.AppendLine("  , PAYBANKBRANCHNAMEKANA                   ")
        SQLJnl.AppendLine("  , PAYACCOUNTTYPENAME                      ")
        SQLJnl.AppendLine("  , PAYACCOUNTTYPE                          ")
        SQLJnl.AppendLine("  , PAYACCOUNT                              ")
        SQLJnl.AppendLine("  , PAYACCOUNTNAME                          ")
        SQLJnl.AppendLine("  , PAYORBANKCODE                           ")
        SQLJnl.AppendLine("  , PAYTAXCALCUNIT                          ")
        SQLJnl.AppendLine("  , LINKSTATUS                              ")
        SQLJnl.AppendLine("  , LASTLINKYMD                             ")
        SQLJnl.AppendLine("  , INITYMD                                 ")
        SQLJnl.AppendLine("  , INITUSER                                ")
        SQLJnl.AppendLine("  , INITTERMID                              ")
        SQLJnl.AppendLine("  , INITPGID                                ")
        SQLJnl.AppendLine("  , UPDYMD                                  ")
        SQLJnl.AppendLine("  , UPDUSER                                 ")
        SQLJnl.AppendLine("  , UPDTERMID                               ")
        SQLJnl.AppendLine("  , UPDPGID                                 ")
        SQLJnl.AppendLine("  , RECEIVEYMD                              ")
        SQLJnl.AppendLine("  , UPDTIMSTP                               ")
        SQLJnl.AppendLine(" FROM                                       ")
        SQLJnl.AppendLine("    LNG.LNT0072_PAYEE                       ")
        SQLJnl.AppendLine(" WHERE                                      ")
        SQLJnl.AppendLine("        TORICODE          = @TORICODE       ")
        SQLJnl.AppendLine("    AND CLIENTCODE        = @CLIENTCODE     ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl.ToString, SQLcon)
                ' DB更新用パラメータ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)            '削除フラグ
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '支払先コード
                Dim P_CLIENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15)         '顧客コード
                Dim P_INVOICENUMBER As MySqlParameter = SQLcmd.Parameters.Add("@INVOICENUMBER", MySqlDbType.VarChar, 14)         'インボイス登録番号
                Dim P_CLIENTNAME As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTNAME", MySqlDbType.VarChar, 32)         '顧客名
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 32)         '会社名
                Dim P_TORIDIVNAME As MySqlParameter = SQLcmd.Parameters.Add("@TORIDIVNAME", MySqlDbType.VarChar, 32)         '部門名
                Dim P_PAYBANKCODE As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKCODE", MySqlDbType.VarChar, 4)         '振込先銀行コード
                Dim P_PAYBANKNAME As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKNAME", MySqlDbType.VarChar, 30)         '振込先銀行名
                Dim P_PAYBANKNAMEKANA As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKNAMEKANA", MySqlDbType.VarChar, 30)         '振込先銀行名カナ
                Dim P_PAYBANKBRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKBRANCHCODE", MySqlDbType.VarChar, 3)         '振込先支店コード
                Dim P_PAYBANKBRANCHNAME As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKBRANCHNAME", MySqlDbType.VarChar, 30)         '振込先支店名
                Dim P_PAYBANKBRANCHNAMEKANA As MySqlParameter = SQLcmd.Parameters.Add("@PAYBANKBRANCHNAMEKANA", MySqlDbType.VarChar, 30)         '振込先支店名カナ
                Dim P_PAYACCOUNTTYPENAME As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTTYPENAME", MySqlDbType.VarChar, 10)         '預金種別
                Dim P_PAYACCOUNTTYPE As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTTYPE", MySqlDbType.VarChar, 1)         '預金種別コード
                Dim P_PAYACCOUNT As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNT", MySqlDbType.VarChar, 8)         '口座番号
                Dim P_PAYACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@PAYACCOUNTNAME", MySqlDbType.VarChar, 30)         '口座名義
                Dim P_PAYORBANKCODE As MySqlParameter = SQLcmd.Parameters.Add("@PAYORBANKCODE", MySqlDbType.VarChar, 4)         '支払元銀行コード
                Dim P_PAYTAXCALCUNIT As MySqlParameter = SQLcmd.Parameters.Add("@PAYTAXCALCUNIT", MySqlDbType.VarChar, 10)         '消費税計算処理区分
                Dim P_LINKSTATUS As MySqlParameter = SQLcmd.Parameters.Add("@LINKSTATUS", MySqlDbType.VarChar, 1)         '連携状態区分
                Dim P_LASTLINKYMD As MySqlParameter = SQLcmd.Parameters.Add("@LASTLINKYMD", MySqlDbType.DateTime)              '最終連携日
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)              '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)        '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)    '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)        '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)          '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)      '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)          '更新プログラムＩＤ
                Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)        '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JP_TORICODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '支払先コード
                Dim JP_CLIENTCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15) '顧客コード

                Dim LNT0023row As DataRow = LNT0023INPtbl.Rows(0)

                ' DB更新
                P_DELFLG.Value = LNT0023row("DELFLG")                   '削除フラグ
                P_TORICODE.Value = LNT0023row("TORICODE")               '支払先コード
                P_CLIENTCODE.Value = LNT0023row("CLIENTCODE")               '顧客コード
                P_INVOICENUMBER.Value = LNT0023row("INVOICENUMBER")               'インボイス登録番号
                P_CLIENTNAME.Value = LNT0023row("CLIENTNAME")               '顧客名
                P_TORINAME.Value = LNT0023row("TORINAME")               '会社名
                P_TORIDIVNAME.Value = LNT0023row("TORIDIVNAME")               '部門名
                P_PAYBANKCODE.Value = LNT0023row("PAYBANKCODE")               '振込先銀行コード
                P_PAYBANKNAME.Value = LNT0023row("PAYBANKNAME")               '振込先銀行名
                P_PAYBANKNAMEKANA.Value = LNT0023row("PAYBANKNAMEKANA")               '振込先銀行名カナ
                P_PAYBANKBRANCHCODE.Value = LNT0023row("PAYBANKBRANCHCODE")               '振込先支店コード
                P_PAYBANKBRANCHNAME.Value = LNT0023row("PAYBANKBRANCHNAME")               '振込先支店名
                P_PAYBANKBRANCHNAMEKANA.Value = LNT0023row("PAYBANKBRANCHNAMEKANA")               '振込先支店名カナ
                P_PAYACCOUNTTYPENAME.Value = LNT0023row("PAYACCOUNTTYPENAME")               '預金種別
                P_PAYACCOUNTTYPE.Value = LNT0023row("PAYACCOUNTTYPE")               '預金種別コード
                P_PAYACCOUNT.Value = LNT0023row("PAYACCOUNT")               '口座番号
                P_PAYACCOUNTNAME.Value = LNT0023row("PAYACCOUNTNAME")               '口座名義
                P_PAYORBANKCODE.Value = LNT0023row("PAYORBANKCODE")               '支払元銀行コード
                P_PAYTAXCALCUNIT.Value = LNT0023row("PAYTAXCALCUNIT")               '消費税計算処理区分
                'P_LINKSTATUS.Value = LNT0023row("LINKSTATUS")               '連携状態区分
                P_LINKSTATUS.Value = "0"
                P_LASTLINKYMD.Value = DBNull.Value                      '最終連携日
                P_INITYMD.Value = WW_NOW                                '登録年月日
                P_INITUSER.Value = Master.USERID                        '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID                  '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name           '登録プログラムＩＤ
                P_UPDYMD.Value = WW_NOW                                 '更新年月日
                P_UPDUSER.Value = Master.USERID                         '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                   '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name            '更新プログラムＩＤ
                P_RECEIVEYMD.Value = C_DEFAULT_YMD                      '集信日時

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JP_TORICODE.Value = LNT0023row("TORICODE")
                JP_CLIENTCODE.Value = LNT0023row("CLIENTCODE")

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNT0023UPDtbl) Then
                        LNT0023UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNT0023UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNT0023UPDtbl.Clear()
                    LNT0023UPDtbl.Load(SQLdr)
                End Using

                For Each LNT0023UPDrow As DataRow In LNT0023UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNT0023C"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNT0023UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0023C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0023C UPDATE_INSERT"
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

        '支払先マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNT0072_PAYEE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        TORICODE         = @TORICODE")
        SQLStr.AppendLine("    AND CLIENTCODE        = @CLIENTCODE")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '支払先コード
                Dim P_CLIENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15)         '顧客コード

                Dim LNT0023row As DataRow = LNT0023INPtbl.Rows(0)

                P_TORICODE.Value = LNT0023row("TORICODE")               '支払先コード
                P_CLIENTCODE.Value = LNT0023row("CLIENTCODE")           '顧客コード

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
                        WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0023C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0023C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0138_PAYEEHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         TORICODE  ")
        SQLStr.AppendLine("        ,CLIENTCODE  ")
        SQLStr.AppendLine("        ,INVOICENUMBER  ")
        SQLStr.AppendLine("        ,CLIENTNAME  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,PAYBANKCODE  ")
        SQLStr.AppendLine("        ,PAYBANKNAME  ")
        SQLStr.AppendLine("        ,PAYBANKNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAME  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPENAME  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNT  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNAME  ")
        SQLStr.AppendLine("        ,PAYORBANKCODE  ")
        SQLStr.AppendLine("        ,PAYTAXCALCUNIT  ")
        SQLStr.AppendLine("        ,LINKSTATUS  ")
        SQLStr.AppendLine("        ,LASTLINKYMD  ")
        SQLStr.AppendLine("        ,OPERATEKBN  ")
        SQLStr.AppendLine("        ,MODIFYKBN  ")
        SQLStr.AppendLine("        ,MODIFYYMD  ")
        SQLStr.AppendLine("        ,MODIFYUSER  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine("        ,INITYMD  ")
        SQLStr.AppendLine("        ,INITUSER  ")
        SQLStr.AppendLine("        ,INITTERMID  ")
        SQLStr.AppendLine("        ,INITPGID  ")
        SQLStr.AppendLine("  )  ")
        SQLStr.AppendLine("  SELECT  ")
        SQLStr.AppendLine("         TORICODE  ")
        SQLStr.AppendLine("        ,CLIENTCODE  ")
        SQLStr.AppendLine("        ,INVOICENUMBER  ")
        SQLStr.AppendLine("        ,CLIENTNAME  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,PAYBANKCODE  ")
        SQLStr.AppendLine("        ,PAYBANKNAME  ")
        SQLStr.AppendLine("        ,PAYBANKNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAME  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHNAMEKANA  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPENAME  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNT  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNAME  ")
        SQLStr.AppendLine("        ,PAYORBANKCODE  ")
        SQLStr.AppendLine("        ,PAYTAXCALCUNIT  ")
        SQLStr.AppendLine("        ,LINKSTATUS  ")
        SQLStr.AppendLine("        ,LASTLINKYMD  ")
        SQLStr.AppendLine("        ,@OPERATEKBN AS OPERATEKBN ")
        SQLStr.AppendLine("        ,@MODIFYKBN AS MODIFYKBN ")
        SQLStr.AppendLine("        ,@MODIFYYMD AS MODIFYYMD ")
        SQLStr.AppendLine("        ,@MODIFYUSER AS MODIFYUSER ")
        SQLStr.AppendLine("        ,DELFLG ")
        SQLStr.AppendLine("        ,@INITYMD AS INITYMD ")
        SQLStr.AppendLine("        ,@INITUSER AS INITUSER ")
        SQLStr.AppendLine("        ,@INITTERMID AS INITTERMID ")
        SQLStr.AppendLine("        ,@INITPGID AS INITPGID ")
        SQLStr.AppendLine("  FROM   ")
        SQLStr.AppendLine("        LNG.LNT0072_PAYEE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        TORICODE         = @TORICODE")
        SQLStr.AppendLine("    AND CLIENTCODE        = @CLIENTCODE")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '支払先コード
                Dim P_CLIENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@CLIENTCODE", MySqlDbType.VarChar, 15)         '顧客コード

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNT0023row As DataRow = LNT0023INPtbl.Rows(0)

                ' DB更新
                P_TORICODE.Value = LNT0023row("TORICODE")               '支払先コード
                P_CLIENTCODE.Value = LNT0023row("CLIENTCODE")           '顧客コード

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNT0023WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNT0023tbl.Rows(0)("DELFLG") = "0" And LNT0023row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNT0023WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNT0023WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0138_PAYEEHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0138_PAYEEHIST  INSERT"
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
    ''' 詳細画面-更新ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNT0023INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNT0023tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNT0023tbl, work.WF_SEL_INPTBL.Text)

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
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, "支払先", needsPopUp:=True)
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
    Protected Sub DetailBoxToLNT0023INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)                    '削除フラグ
        Master.EraseCharToIgnore(TxtToriCode.Text)                  '支払先コード
        Master.EraseCharToIgnore(TxtPayBankCode.Text)               '振込先銀行コード
        Master.EraseCharToIgnore(TxtPayBankBranchCode.Text)         '振込先支店コード
        Master.EraseCharToIgnore(TxtPayAccountType.Text)            '預金種別コード
        Master.EraseCharToIgnore(TxtPayAccount.Text)                '口座番号
        Master.EraseCharToIgnore(TxtPayorBankCode.Text)             '支払元銀行コード
        Master.EraseCharToIgnore(TxtLinkStatus.Text)                '連携状態区分

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(LblSelLineCNT.Text) AndAlso
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

        Master.CreateEmptyTable(LNT0023INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNT0023INProw As DataRow = LNT0023INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(LblSelLineCNT.Text) Then
            LNT0023INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(LblSelLineCNT.Text, LNT0023INProw("LINECNT"))
            Catch ex As Exception
                LNT0023INProw("LINECNT") = 0
            End Try
        End If

        LNT0023INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNT0023INProw("UPDTIMSTP") = 0
        LNT0023INProw("SELECT") = 1
        LNT0023INProw("HIDDEN") = 0

        LNT0023INProw("DELFLG") = TxtDelFlg.Text                                                 '削除フラグ
        LNT0023INProw("TORICODE") = TxtToriCode.Text                                             '支払先コード
        LNT0023INProw("CLIENTCODE") = TxtClientCode.Text                                         '顧客コード
        LNT0023INProw("INVOICENUMBER") = TxtInvoiceNumber.Text                                   'インボイス登録番号
        LNT0023INProw("CLIENTNAME") = TxtClientName.Text                                         '顧客名
        LNT0023INProw("TORINAME") = TxtToriName.Text                                             '会社名
        LNT0023INProw("TORIDIVNAME") = TxtToriDivName.Text                                       '部門名
        LNT0023INProw("PAYBANKCODE") = Strings.Right("0000" + TxtPayBankCode.Text, 4)            '振込先銀行コード
        LNT0023INProw("PAYBANKNAME") = TxtPayBankName.Text                                       '振込先銀行名
        LNT0023INProw("PAYBANKNAMEKANA") = TxtPayBankNameKana.Text                               '振込先銀行名カナ
        LNT0023INProw("PAYBANKBRANCHCODE") = Strings.Right("000" + TxtPayBankBranchCode.Text, 3) '振込先支店コード
        LNT0023INProw("PAYBANKBRANCHNAME") = TxtPayBankBranchName.Text                           '振込先支店名
        LNT0023INProw("PAYBANKBRANCHNAMEKANA") = TxtPayBankBranchNameKana.Text                   '振込先支店名カナ
        'LNT0023INProw("PAYACCOUNTTYPENAME") = TxtPayAccountTypeName.Text                         '預金種別
        LNT0023INProw("PAYACCOUNTTYPENAME") = ddlPayAccountTypeName.SelectedValue
        LNT0023INProw("PAYACCOUNTTYPE") = Strings.Right("0" + TxtPayAccountType.Text, 1)         '預金種別コード
        LNT0023INProw("PAYACCOUNT") = TxtPayAccount.Text                                         '口座番号
        LNT0023INProw("PAYACCOUNTNAME") = TxtPayAccountName.Text                                 '口座名義
        LNT0023INProw("PAYORBANKCODE") = Strings.Right("0000" + TxtPayorBankCode.Text, 4)        '支払元銀行コード
        'LNT0023INProw("PAYTAXCALCUNIT") = TxtPayTaxCalcUnit.Text                                 '消費税計算処理区分
        LNT0023INProw("PAYTAXCALCUNIT") = ddlPayTaxCalcUnit.SelectedValue
        'LNT0023INProw("LINKSTATUS") = Strings.Right("0" + TxtLinkStatus.Text, 1)                '連携状態区分

        '○ チェック用テーブルに登録する
        LNT0023INPtbl.Rows.Add(LNT0023INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNT0023INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNT0023INProw As DataRow = LNT0023INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNT0023row As DataRow In LNT0023tbl.Rows
            ' KEY項目が等しい時
            If LNT0023row("TORICODE") = LNT0023INProw("TORICODE") AndAlso                                    '支払先コード
               LNT0023row("CLIENTCODE") = LNT0023INProw("CLIENTCODE") Then                                   '顧客コード
                ' KEY項目以外の項目の差異をチェック
                If LNT0023row("DELFLG") = LNT0023INProw("DELFLG") AndAlso                                    '削除フラグ
                    LNT0023row("INVOICENUMBER") = LNT0023INProw("INVOICENUMBER") AndAlso                                    'インボイス登録番号
                    LNT0023row("CLIENTNAME") = LNT0023INProw("CLIENTNAME") AndAlso                                    '顧客名
                    LNT0023row("TORINAME") = LNT0023INProw("TORINAME") AndAlso                                    '会社名
                    LNT0023row("TORIDIVNAME") = LNT0023INProw("TORIDIVNAME") AndAlso                                    '部門名
                    LNT0023row("PAYBANKCODE") = LNT0023INProw("PAYBANKCODE") AndAlso                                    '振込先銀行コード
                    LNT0023row("PAYBANKNAME") = LNT0023INProw("PAYBANKNAME") AndAlso                                    '振込先銀行名
                    LNT0023row("PAYBANKNAMEKANA") = LNT0023INProw("PAYBANKNAMEKANA") AndAlso                                    '振込先銀行名カナ
                    LNT0023row("PAYBANKBRANCHCODE") = LNT0023INProw("PAYBANKBRANCHCODE") AndAlso                                    '振込先支店コード
                    LNT0023row("PAYBANKBRANCHNAME") = LNT0023INProw("PAYBANKBRANCHNAME") AndAlso                                    '振込先支店名
                    LNT0023row("PAYBANKBRANCHNAMEKANA") = LNT0023INProw("PAYBANKBRANCHNAMEKANA") AndAlso                                    '振込先支店名カナ
                    LNT0023row("PAYACCOUNTTYPENAME") = LNT0023INProw("PAYACCOUNTTYPENAME") AndAlso                                    '預金種別
                    LNT0023row("PAYACCOUNTTYPE") = LNT0023INProw("PAYACCOUNTTYPE") AndAlso                                    '預金種別コード
                    LNT0023row("PAYACCOUNT") = LNT0023INProw("PAYACCOUNT") AndAlso                                    '口座番号
                    LNT0023row("PAYACCOUNTNAME") = LNT0023INProw("PAYACCOUNTNAME") AndAlso                                    '口座名義
                    LNT0023row("PAYORBANKCODE") = LNT0023INProw("PAYORBANKCODE") AndAlso                                    '支払元銀行コード
                    LNT0023row("PAYTAXCALCUNIT") = LNT0023INProw("PAYTAXCALCUNIT") Then                                    '消費税計算処理区分
                    'LNT0023row("LINKSTATUS") = LNT0023INProw("LINKSTATUS") Then                                    '連携状態区分

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
        For Each LNT0023row As DataRow In LNT0023tbl.Rows
            Select Case LNT0023row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0023tbl, work.WF_SEL_INPTBL.Text)

        LblSelLineCNT.Text = ""                'LINECNT
        TxtMapId.Text = "M00001"               '画面ＩＤ
        TxtDelFlg.Text = ""                    '削除フラグ
        TxtToriCode.Text = ""                  '支払先コード
        TxtClientCode.Text = ""                '顧客コード
        TxtInvoiceNumber.Text = ""             'インボイス登録番号
        TxtClientName.Text = ""                '顧客名
        TxtToriName.Text = ""                  '会社名
        TxtToriDivName.Text = ""               '部門名
        TxtPayBankCode.Text = ""               '振込先銀行コード
        TxtPayBankName.Text = ""               '振込先銀行名
        TxtPayBankNameKana.Text = ""           '振込先銀行名カナ
        TxtPayBankBranchCode.Text = ""         '振込先支店コード
        TxtPayBankBranchName.Text = ""         '振込先支店名
        TxtPayBankBranchNameKana.Text = ""     '振込先支店名カナ
        'TxtPayAccountTypeName.Text = ""        '預金種別
        ddlPayAccountTypeName.SelectedValue = ""
        TxtPayAccountType.Text = ""            '預金種別コード
        TxtPayAccount.Text = ""                '口座番号
        TxtPayAccountName.Text = ""            '口座名義
        TxtPayorBankCode.Text = ""             '支払元銀行コード
        'TxtPayTaxCalcUnit.Text = ""            '消費税計算処理区分
        ddlPayTaxCalcUnit.SelectedValue = ""
        TxtLinkStatus.Text = ""                '連携状態区分

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
                    Case "TxtDelFlg"               '削除フラグ
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                    Case "TxtPayBankCode",       '振込先銀行コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspBankCodeSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    Case "TxtPayBankBranchCode",       '支店コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspBankBranchCodeSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    Case "TxtPayorBankCode"       '支払元銀行コード 
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspBankAccountSingle()
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
            Case "TxtDelFlg"                        '削除フラグ
                CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy, WW_Dummy)
                TxtDelFlg.Focus()
            Case "TxtPayBankCode"                   '振込先銀行コード
                CODENAME_get("BANKCODE", TxtPayBankCode.Text, TxtPayBankName.Text, TxtPayBankNameKana.Text, WW_Dummy)
                If TxtPayBankName.Text = "" Then
                    TxtPayBankBranchCode.Text = ""
                    TxtPayBankBranchName.Text = ""
                    TxtPayBankBranchNameKana.Text = ""
                End If
                TxtPayBankCode.Focus()
            Case "TxtPayBankBranchCode"             '振込先支店コード
                CODENAME_get("BANKBRANCHCODE", TxtPayBankBranchCode.Text, TxtPayBankBranchName.Text, TxtPayBankBranchNameKana.Text, WW_Dummy)
                TxtPayBankBranchCode.Focus()
            Case "TxtPayorBankCode"                 '支払元銀行コード
                CODENAME_get("PAYORBANKCODE", TxtPayorBankCode.Text, LblPayorBankName.Text, WW_Dummy, WW_Dummy)
                TxtPayorBankCode.Focus()
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
                Case "TxtDelFlg"                         '削除フラグ
                    TxtDelFlg.Text = WW_SelectValue
                    LblDelFlgName.Text = WW_SelectText
                    TxtDelFlg.Focus()
            End Select
        End If

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
                Case "TxtDelFlg"                         '削除フラグ
                    TxtDelFlg.Focus()
                Case "TxtPayBankCode"
                    TxtPayBankCode.Focus()               '振込先銀行コード
                Case "TxtPayBankBranchCode"
                    TxtPayBankBranchCode.Focus()               '振込先支店コード
            End Select
        End If

        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' 銀行コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspBankCodeSingle()

        Me.mspBankCodeSingle.InitPopUp()
        Me.mspBankCodeSingle.SelectionMode = ListSelectionMode.Single
        Me.mspBankCodeSingle.SQL = CmnSearchSQL.GetBankCodeSQL(TxtPayBankCode.Text)

        Me.mspBankCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspBankCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetBankCodeTitle)

        Me.mspBankCodeSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 銀行コード選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspBankCodeSingle()

        Dim selData = Me.mspBankCodeSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtPayBankCode.ID
                Me.TxtPayBankCode.Text = selData("BANKCODE").ToString '振込先銀行コード
                Me.TxtPayBankName.Text = selData("BANKNAME").ToString '振込先銀行名
                Me.TxtPayBankNameKana.Text = selData("BANKNAMEKANA").ToString '振込先銀行名カナ
                Me.TxtPayBankCode.Focus()
        End Select

        'ポップアップの非表示
        Me.mspBankCodeSingle.HidePopUp()

    End Sub

    ''' <summary>
    ''' 支店コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspBankBranchCodeSingle()

        Me.mspBankBranchCodeSingle.InitPopUp()
        Me.mspBankBranchCodeSingle.SelectionMode = ListSelectionMode.Single
        Me.mspBankBranchCodeSingle.SQL = CmnSearchSQL.GetBankBranchCodeSQL(TxtPayBankCode.Text)

        Me.mspBankBranchCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspBankBranchCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetBankBranchCodeTitle)

        Me.mspBankBranchCodeSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 支店コード選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspBankBranchCodeSingle()

        Dim selData = Me.mspBankBranchCodeSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtPayBankBranchCode.ID
                Me.TxtPayBankBranchCode.Text = selData("BANKBRANCHCODE").ToString '振込先支店コード
                Me.TxtPayBankBranchName.Text = selData("BANKBRANCHNAME").ToString '振込先支店名
                Me.TxtPayBankBranchNameKana.Text = selData("BANKBRANCHNAMEKANA").ToString '振込先支店名カナ
                Me.TxtPayBankBranchCode.Focus()
        End Select

        'ポップアップの非表示
        Me.mspBankBranchCodeSingle.HidePopUp()

    End Sub

    ''' <summary>
    ''' 支払元銀行検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspBankAccountSingle()

        Me.mspBankAccountSingle.InitPopUp()
        Me.mspBankAccountSingle.SelectionMode = ListSelectionMode.Single
        Me.mspBankAccountSingle.SQL = CmnSearchSQL.GetBankAccountSQL(TxtPayorBankCode.Text)

        Me.mspBankAccountSingle.KeyFieldName = "KEYCODE"
        Me.mspBankAccountSingle.DispFieldList.AddRange(CmnSearchSQL.GetBankAccountTitle)

        Me.mspBankAccountSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 支払元銀行選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspBankAccountSingle()

        Dim selData = Me.mspBankAccountSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtPayorBankCode.ID
                Me.TxtPayorBankCode.Text = selData("BANKCODE").ToString '支払元銀行コード
                Me.LblPayorBankName.Text = selData("BANKNAME").ToString
                Me.TxtPayorBankCode.Focus()
        End Select

        'ポップアップの非表示
        Me.mspBankAccountSingle.HidePopUp()

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
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""
        Dim WW_DBDataCheck As String = ""
        Dim WW_SlcStMD As String = ""
        Dim WW_SlcEndMD As String = ""

        '○ 画面操作権限チェック
        ' 権限チェック(操作者に更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            WW_CheckMES1 = "・支払先マスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        Dim WW_Dummyht = New Hashtable
        Dim WW_BANKht = New Hashtable '銀行称格納HT
        Dim WW_BANKBRANCHht = New Hashtable '支店名称格納HT
        Dim WW_BANKACCOUNTht = New Hashtable '支払元銀行格納HT

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            work.CODENAMEGetBANK(SQLcon, WW_BANKht, WW_Dummyht)
            work.CODENAMEGetBANKBRANCH(SQLcon, LNT0023INPtbl(0)("PAYBANKCODE"), WW_BANKBRANCHht, WW_Dummyht)
            work.CODENAMEGetBANKACCOUNT(SQLcon, WW_BANKACCOUNTht)
        End Using

        '○ 単項目チェック
        For Each LNT0023INProw As DataRow In LNT0023INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNT0023INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNT0023INProw("DELFLG"), WW_Dummy, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・削除コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・削除コードエラーです"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 支払先コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORICODE", LNT0023INProw("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・支払先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 顧客コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CLIENTCODE", LNT0023INProw("CLIENTCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・顧客コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' インボイス登録番号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "INVOICENUMBER", LNT0023INProw("INVOICENUMBER"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・インボイス登録番号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 顧客名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CLIENTNAME", LNT0023INProw("CLIENTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・顧客名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 会社名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORINAME", LNT0023INProw("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・会社名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 部門名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORIDIVNAME", LNT0023INProw("TORIDIVNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・部門名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 振込先銀行コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYBANKCODE", LNT0023INProw("PAYBANKCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' コード存在チェック
                If Not WW_BANKht.ContainsKey(LNT0023INProw("PAYBANKCODE")) Then
                    WW_CheckMES1 = "・振込先銀行コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・振込先銀行コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            '' 振込先銀行名(バリデーションチェック)
            'Master.CheckField(Master.USERCAMP, "PAYBANKNAME", LNT0023INProw("PAYBANKNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・振込先銀行名エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            '' 振込先銀行名カナ(バリデーションチェック)
            'Master.CheckField(Master.USERCAMP, "PAYBANKNAMEKANA", LNT0023INProw("PAYBANKNAMEKANA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・振込先銀行名カナエラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            ' 振込先支店コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYBANKBRANCHCODE", LNT0023INProw("PAYBANKBRANCHCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' コード存在チェック
                If Not WW_BANKBRANCHht.ContainsKey(LNT0023INProw("PAYBANKBRANCHCODE")) Then
                    WW_CheckMES1 = "・振込先支店コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・振込先支店コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '' 振込先支店名(バリデーションチェック)
            'Master.CheckField(Master.USERCAMP, "PAYBANKBRANCHNAME", LNT0023INProw("PAYBANKBRANCHNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・振込先支店名エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            '' 振込先支店名カナ(バリデーションチェック)
            'Master.CheckField(Master.USERCAMP, "PAYBANKBRANCHNAMEKANA", LNT0023INProw("PAYBANKBRANCHNAMEKANA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・振込先支店名カナエラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            ' 預金種別(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYACCOUNTTYPENAME", LNT0023INProw("PAYACCOUNTTYPENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・預金種別エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 預金種別コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYACCOUNTTYPE", LNT0023INProw("PAYACCOUNTTYPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・預金種別コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 口座番号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYACCOUNT", LNT0023INProw("PAYACCOUNT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・口座番号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 口座名義(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYACCOUNTNAME", LNT0023INProw("PAYACCOUNTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・口座名義エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 支払元銀行コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYORBANKCODE", LNT0023INProw("PAYORBANKCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' コード存在チェック
                If Not WW_BANKACCOUNTht.ContainsKey(LNT0023INProw("PAYORBANKCODE")) Then
                    WW_CheckMES1 = "・支払元銀行コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・支払元銀行コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 消費税計算処理区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PAYTAXCALCUNIT", LNT0023INProw("PAYTAXCALCUNIT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・消費税計算処理区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            '' 連携状態区分(バリデーションチェック)
            'Master.CheckField(Master.USERCAMP, "LINKSTATUS", LNT0023INProw("LINKSTATUS"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・連携状態区分エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_TORICODE.Text) Then  '支払先コード
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                    TxtToriCode.Text, TxtClientCode.Text,
                                    work.WF_SEL_TIMESTAMP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（支払先コード&顧客コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                       "([" & LNT0023INProw("TORICODE") & "]" &
                                       "([" & LNT0023INProw("CLIENTCODE") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNT0023INProw("TORICODE") = work.WF_SEL_TORICODE.Text OrElse     '支払先コード
               Not LNT0023INProw("CLIENTCODE") = work.WF_SEL_CLIENTCODE.Text Then   '顧客コード
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（支払先コード&顧客コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & LNT0023INProw("TORICODE") & "]" &
                                       "([" & LNT0023INProw("CLIENTCODE") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LineErr = "" Then
                If LNT0023INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNT0023INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNT0023INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNT0023INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNT0023tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNT0023tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNT0023row As DataRow In LNT0023tbl.Rows
            Select Case LNT0023row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNT0023row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNT0023INProw As DataRow In LNT0023INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNT0023INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNT0023INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNT0023row As DataRow In LNT0023tbl.Rows
                ' KEY項目が等しい時
                If LNT0023row("TORICODE") = LNT0023INProw("TORICODE") AndAlso                             '支払先コード
                   LNT0023row("CLIENTCODE") = LNT0023INProw("CLIENTCODE") Then                            '顧客コード
                    ' KEY項目以外の項目の差異をチェック                                                           
                    If LNT0023row("DELFLG") = LNT0023INProw("DELFLG") AndAlso                             '削除フラグ
                        LNT0023row("INVOICENUMBER") = LNT0023INProw("INVOICENUMBER") AndAlso                                'インボイス登録番号
                        LNT0023row("CLIENTNAME") = LNT0023INProw("CLIENTNAME") AndAlso                                '顧客名
                        LNT0023row("TORINAME") = LNT0023INProw("TORINAME") AndAlso                                '会社名
                        LNT0023row("TORIDIVNAME") = LNT0023INProw("TORIDIVNAME") AndAlso                                '部門名
                        LNT0023row("PAYBANKCODE") = LNT0023INProw("PAYBANKCODE") AndAlso                                '振込先銀行コード
                        LNT0023row("PAYBANKNAME") = LNT0023INProw("PAYBANKNAME") AndAlso                                '振込先銀行名
                        LNT0023row("PAYBANKNAMEKANA") = LNT0023INProw("PAYBANKNAMEKANA") AndAlso                                '振込先銀行名カナ
                        LNT0023row("PAYBANKBRANCHCODE") = LNT0023INProw("PAYBANKBRANCHCODE") AndAlso                                '振込先支店コード
                        LNT0023row("PAYBANKBRANCHNAME") = LNT0023INProw("PAYBANKBRANCHNAME") AndAlso                                '振込先支店名
                        LNT0023row("PAYBANKBRANCHNAMEKANA") = LNT0023INProw("PAYBANKBRANCHNAMEKANA") AndAlso                                '振込先支店名カナ
                        LNT0023row("PAYACCOUNTTYPENAME") = LNT0023INProw("PAYACCOUNTTYPENAME") AndAlso                                '預金種別
                        LNT0023row("PAYACCOUNTTYPE") = LNT0023INProw("PAYACCOUNTTYPE") AndAlso                                '預金種別コード
                        LNT0023row("PAYACCOUNT") = LNT0023INProw("PAYACCOUNT") AndAlso                                '口座番号
                        LNT0023row("PAYACCOUNTNAME") = LNT0023INProw("PAYACCOUNTNAME") AndAlso                                '口座名義
                        LNT0023row("PAYORBANKCODE") = LNT0023INProw("PAYORBANKCODE") AndAlso                                '支払元銀行コード
                        LNT0023row("PAYTAXCALCUNIT") = LNT0023INProw("PAYTAXCALCUNIT") AndAlso                                '消費税計算処理区分
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNT0023row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        LNT0023INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNT0023INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNT0023INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNT0023INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNT0023INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now

                '変更チェック
                MASTEREXISTS(SQLcon, WW_MODIFYKBN)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNT0023WRKINC.MODIFYKBN.AFTDATA
                End If

                ' マスタ更新
                UpdateMaster(SQLcon, WW_DATE)
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
        For Each LNT0023INProw As DataRow In LNT0023INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNT0023row As DataRow In LNT0023tbl.Rows
                ' 同一レコードか判定
                If LNT0023INProw("TORICODE") = LNT0023row("TORICODE") AndAlso       '支払先コード
                   LNT0023INProw("CLIENTCODE") = LNT0023row("CLIENTCODE") Then      '顧客コード
                    ' 画面入力テーブル項目設定
                    LNT0023INProw("LINECNT") = LNT0023row("LINECNT")
                    LNT0023INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNT0023INProw("UPDTIMSTP") = LNT0023row("UPDTIMSTP")
                    LNT0023INProw("SELECT") = 0
                    LNT0023INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNT0023row.ItemArray = LNT0023INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNT0023tbl.NewRow
                WW_NRow.ItemArray = LNT0023INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNT0023tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNT0023tbl.Rows.Add(WW_NRow)
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
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_TEXT2 As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_TEXT2 = ""
        O_RTN = ""

        If String.IsNullOrEmpty(I_VALUE) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        '名称取得
        Dim WW_NAMEht = New Hashtable '名称格納HT
        Dim WW_KANAht = New Hashtable 'カナ格納HT

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            Select Case I_FIELD
                Case "BANKCODE"             '振込先銀行コード
                    work.CODENAMEGetBANK(SQLcon, WW_NAMEht, WW_KANAht)
                Case "BANKBRANCHCODE"             '振込先支店コード
                    If Not TxtPayBankCode.Text = "" Then
                        work.CODENAMEGetBANKBRANCH(SQLcon, TxtPayBankCode.Text, WW_NAMEht, WW_KANAht)
                    Else
                        Exit Sub
                    End If
                Case "PAYORBANKCODE"             '支払元銀行コード
                    work.CODENAMEGetBANKACCOUNT(SQLcon, WW_NAMEht)
            End Select
        End Using

        Try
            Select Case I_FIELD
                Case "DELFLG"             '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "BANKCODE"             '振込先銀行コード
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
                    If WW_KANAht.ContainsKey(I_VALUE) Then
                        O_TEXT2 = WW_KANAht(I_VALUE)
                    End If
                    O_RTN = C_MESSAGE_NO.NORMAL
                Case "BANKBRANCHCODE"             '振込先支店コード
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
                    If WW_KANAht.ContainsKey(I_VALUE) Then
                        O_TEXT2 = WW_KANAht(I_VALUE)
                    End If
                    O_RTN = C_MESSAGE_NO.NORMAL
                Case "PAYORBANKCODE"             '支払元銀行コード
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
