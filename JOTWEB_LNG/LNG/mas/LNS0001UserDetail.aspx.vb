''************************************************************
' ユーザーマスタメンテ登録画面
' 作成日 2024/12/02
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2024/12/02 新規作成
'          : 
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' ユーザーマスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNS0001UserDetail
    Inherits Page

    ''' <summary>
    ''' ユーザー情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザー情報取得

    '○ 検索結果格納Table
    Private LNS0001tbl As DataTable                                 '一覧格納用テーブル
    Private LNS0001INPtbl As DataTable                              'チェック用テーブル
    Private LNS0001UPDtbl As DataTable                              '更新用テーブル

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
                TxtPassword.Attributes("Value") = TxtPassword.Text
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(LNS0001tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_ButtonCLEAR", "LNS0001L", "LNS0001S"  '戻るボタン押下（LNS0001L、LNS0001Sは、パンくずより）
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
                        Case "WF_ButtonOverlapPeriodsSrcUpdate"    '期間重複調整画面(更新)
                            WF_ButtonOverlapPeriodsSrcUpdateClick()
                        Case "WF_ButtonOverlapPeriodsSrcClose"     '期間重複調整画面(キャンセル)
                            '○ 期間重複調整用子画面を非表示
                            WF_OverlapPeriodsSrc.Value = "hidden"
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
            If Not IsNothing(LNS0001tbl) Then
                LNS0001tbl.Clear()
                LNS0001tbl.Dispose()
                LNS0001tbl = Nothing
            End If

            If Not IsNothing(LNS0001INPtbl) Then
                LNS0001INPtbl.Clear()
                LNS0001INPtbl.Dispose()
                LNS0001INPtbl = Nothing
            End If

            If Not IsNothing(LNS0001UPDtbl) Then
                LNS0001UPDtbl.Clear()
                LNS0001UPDtbl.Dispose()
                LNS0001UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNS0001WRKINC.MAPIDD
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
            '組織ドロップダウンのクリア
            Me.ddlSelectORG.Items.Clear()
            Me.ddlSelectORG.Items.Add("")

            Dim retOfficeList As New DropDownList

            '組織ドロップダウンの生成
            If TxtCampCode.Text = "" Then
                retOfficeList = CmnLng.getDowpDownFixedList(work.WF_SEL_CAMPCODE_D.Text, "ORGCODEDROP")
            Else
                retOfficeList = CmnLng.getDowpDownFixedList(TxtCampCode.Text, "ORGCODEDROP")
            End If

            If retOfficeList.Items.Count > 0 Then
                '情シス、高圧ガス以外
                If LNS0001WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
                    Dim WW_OrgPermitHt As New Hashtable
                    Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()  ' DataBase接続
                        work.GetPermitOrg(SQLcon, Master.USERCAMP, Master.ROLE_ORG, WW_OrgPermitHt)
                        For index As Integer = 0 To retOfficeList.Items.Count - 1
                            If WW_OrgPermitHt.ContainsKey(retOfficeList.Items(index).Value) = True Or retOfficeList.Items(index).Value = Master.ROLE_ORG Then
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNS0001L Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        TxtSelLineCNT.Text = work.WF_SEL_LINECNT.Text
        'ユーザーID
        TxtUserId.Text = work.WF_SEL_USERID.Text
        '社員名（短）
        TxtStaffNameS.Text = work.WF_SEL_STAFFNAMES.Text
        '社員名（長）
        TxtStaffNameL.Text = work.WF_SEL_STAFFNAMEL.Text
        '画面ＩＤ
        TxtMapId.Text = "M00001"
        'パスワード
        TxtPassword.Text = work.WF_SEL_PASSWORD.Text
        TxtPassword.Attributes("Value") = work.WF_SEL_PASSWORD.Text
        '誤り回数
        TxtMissCNT.Text = work.WF_SEL_MISSCNT.Text
        'パスワード有効期限
        TxtPassEndYMD.Text = work.WF_SEL_PASSENDYMD.Text
        '開始年月日
        WF_StYMD.Value = work.WF_SEL_STYMD2.Text.ToString.Replace("/", "-")
        '終了年月日
        WF_EndYMD.Value = work.WF_SEL_ENDYMD2.Text.ToString.Replace("/", "-")
        '会社コード
        TxtCampCode.Text = work.WF_SEL_CAMPCODE_D.Text
        CODENAME_get("CAMPCODE", TxtCampCode.Text, LblCampCodeName.Text, WW_RtnSW)
        '組織コード
        ddlSelectORG.SelectedValue = work.WF_SEL_ORG2.Text
        'CODENAME_get("ORG", ddlSelectORG.SelectedValue, LblOrgName.Text, WW_Dummy)
        'メールアドレス
        TxtEMail.Text = work.WF_SEL_EMAIL.Text
        ''メニュー表示制御ロール
        'TxtMenuRole.Text = work.WF_SEL_MENUROLE.Text
        'CODENAME_get("MENU", TxtMenuRole.Text, LblMenuRoleName.Text, WW_Dummy)
        ''画面参照更新制御ロール
        'TxtMapRole.Text = work.WF_SEL_MAPROLE.Text
        'CODENAME_get("MAP", TxtMapRole.Text, LblMapRoleName.Text, WW_Dummy)
        ''画面表示項目制御ロール
        'TxtViewProfId.Text = work.WF_SEL_VIEWPROFID.Text
        'CODENAME_get("VIEW", TxtViewProfId.Text, LblViewProfIdName.Text, WW_Dummy)
        ''エクセル出力制御ロール
        'TxtRprtProfId.Text = work.WF_SEL_RPRTPROFID.Text
        'CODENAME_get("XML", TxtRprtProfId.Text, LblRprtProfIdName.Text, WW_Dummy)
        ''画面初期値ロール
        'TxtVariant.Text = work.WF_SEL_VARIANT.Text
        ''承認権限ロール
        'TxtApproValid.Text = work.WF_SEL_APPROVALID.Text
        'CODENAME_get("APPROVAL", TxtApproValid.Text, LblApproValidName.Text, WW_Dummy)
        '削除
        TxtDelFlg.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_USERID.Text

        ' 削除フラグ・誤り回数・会社コード・組織コードを入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtMissCNT.Attributes("onkeyPress") = "CheckNum()"
        'Me.TxtOrg.Attributes("onkeyPress") = "CheckNum()"

        ' パスワード有効期限・開始年月日・終了年月日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.TxtPassEndYMD.Attributes("onkeyPress") = "CheckCalendar()"
        Me.WF_StYMD.Attributes("onkeyPress") = "CheckCalendar()"
        Me.WF_EndYMD.Attributes("onkeyPress") = "CheckCalendar()"

        '情報システム部以外は変更不可
        If Master.USER_ORG <> CONST_OFFICECODE_SYSTEM Then
            DisabledKeyItemUserId.Value = work.WF_SEL_USERID.Text
            '追加時は入力可能
            If work.WF_SEL_USERID.Text <> "" Then
                TxtDelFlg.Enabled = False
                TxtStaffNameS.Enabled = False
                TxtStaffNameL.Enabled = False
                TxtMissCNT.Enabled = False
                TxtPassEndYMD.Enabled = False
                'WF_EndYMD.Enabled = False
                TxtCampCode.Enabled = False
                ddlSelectORG.Enabled = False
                TxtEMail.Enabled = False
                'TxtMenuRole.Enabled = False
                'TxtMapRole.Enabled = False
                'TxtViewProfId.Enabled = False
                'TxtRprtProfId.Enabled = False
                'TxtVariant.Enabled = False
                'TxtApproValid.Enabled = False
            End If
            'ログインユーザーと同じ場合パスワードのみ入力可能
            If TxtUserId.Text <> Master.USERID Then
                DisabledKeyItemPass.Value = work.WF_SEL_USERID.Text
            End If
        End If

        '情報システム部の場合
        If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
            DisabledKeySystem.Value = CONST_OFFICECODE_SYSTEM
            TxtCampCode.Enabled = True
        Else
            DisabledKeySystem.Value = ""
            TxtCampCode.Enabled = False
        End If

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU
    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO"></param>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String,
                                 Optional ByVal StYMD As String = "", Optional ByVal EnYMD As String = "")

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                 ")
        SQLStr.AppendLine("     USERID             ")
        SQLStr.AppendLine("   , STYMD              ")
        SQLStr.AppendLine(" FROM                   ")
        SQLStr.AppendLine("     COM.LNS0001_USER   ")
        SQLStr.AppendLine(" WHERE                  ")
        SQLStr.AppendLine("         USERID  = @USERID  ")
        SQLStr.AppendLine("     AND STYMD   = @STYMD  ")
        SQLStr.AppendLine("     AND ENDYMD   = @ENDYMD  ")
        SQLStr.AppendLine("     AND DELFLG <> @DELFLG  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 20) 'ユーザーID
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.VarChar, 20) '開始年月日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.VarChar, 20) '終了年月日
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)  '削除フラグ

                P_USERID.Value = TxtUserId.Text       'ユーザーID
                If StYMD = "" Then
                    P_STYMD.Value = WF_StYMD.Value    '開始年月日
                Else
                    P_STYMD.Value = StYMD            '開始年月日
                End If
                If EnYMD = "" Then
                    P_ENDYMD.Value = WF_EndYMD.Value    '終了年月日
                Else
                    P_ENDYMD.Value = EnYMD            '終了年月日
                End If
                P_DELFLG.Value = C_DELETE_FLG.DELETE  '削除フラグ

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNS0001Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNS0001Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNS0001Chk.Load(SQLdr)

                    If LNS0001Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0001D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 期間重複チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="flg"></param>
    Protected Sub OverlapPeriodsCheck(ByVal SQLcon As MySqlConnection, ByRef flg As String)

        flg = 0

        ' 項目
        pnlTxtLastStYMD.Text = ""
        pnlTxtLastEndYMD.Text = ""
        pnlTxtNextStYMD.Text = ""
        pnlTxtNextEndYMD.Text = ""
        pnlTxtInputStYMD.Text = ""
        pnlTxtInputEndYMD.Text = ""
        pnlTxtAdjustLastStYMD.Text = ""
        pnlTxtAdjustLastEndYMD.Text = ""
        pnlTxtAdjustNextStYMD.Text = ""
        pnlTxtAdjustNextEndYMD.Text = ""

        ' 制御項目
        VisibleKey_OverlapPeriodsLast.Value = ""
        VisibleKey_OverlapPeriodsNext.Value = ""
        DisabledKey_OverlapPeriodsInput_Start.Value = "disabled"
        DisabledKey_OverlapPeriodsInput_End.Value = "disabled"

        '○ 期間重複前回(一つ前)データ取得
        Dim SQLStr As String =
              "SELECT               " _
            & "    USERID           " _
            & "   ,STYMD            " _
            & "   ,ENDYMD           " _
            & "FROM                 " _
            & "    COM.LNS0001_USER " _
            & "WHERE                " _
            & "    USERID  = @P1    " _
            & "AND DELFLG  = @P2    " _
            & "AND STYMD  <= @P3    " _
            & "AND ENDYMD >= @P3    " _
            & "ORDER BY             " _
            & "    ENDYMD DESC      " _
            & "LIMIT 1              "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 20) 'ユーザーID
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 1)  '削除フラグ
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 20) '利用開始日

                PARA1.Value = TxtUserId.Text       'ユーザーID
                PARA2.Value = C_DELETE_FLG.ALIVE   '削除フラグ
                PARA3.Value = WF_StYMD.Value        '利用開始日

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNS0001Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNS0001Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNS0001Chk.Load(SQLdr)

                    If LNS0001Chk.Rows.Count <> 0 Then
                        Dim LastLNS0001row As DataRow = LNS0001Chk.Rows(0)
                        ' 期間重複が同じデータで無い場合のみ次回情報を表示
                        If WF_StYMD.Value <> CDate(LastLNS0001row("STYMD")).ToString("yyyy-MM-dd") Then
                            flg = 1
                            DisabledKey_OverlapPeriodsInput_Start.Value = ""
                            pnlTxtAdjustLastStYMD.Text = CDate(LastLNS0001row("STYMD")).ToString("yyyy/MM/dd")
                            pnlTxtAdjustLastEndYMD.Text = CDate(LastLNS0001row("ENDYMD")).ToString("yyyy/MM/dd")
                            pnlTxtInputStYMD.Text = WF_StYMD.Value
                            pnlTxtInputEndYMD.Text = WF_EndYMD.Value
                            pnlTxtLastStYMD.Text = CDate(LastLNS0001row("STYMD")).ToString("yyyy/MM/dd")
                            If pnlTxtInputStYMD.Text = pnlTxtLastStYMD.Text Then
                                pnlTxtLastEndYMD.Text = pnlTxtLastStYMD.Text
                            Else
                                pnlTxtLastEndYMD.Text = DateAdd("d", -1, CDate(pnlTxtInputStYMD.Text)).ToString("yyyy-MM-dd")
                            End If
                        Else
                            VisibleKey_OverlapPeriodsLast.Value = "none"
                        End If
                    Else
                        VisibleKey_OverlapPeriodsLast.Value = "none"
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001D OverlapPeriodsCheck")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0001D OverlapPeriodsCheck"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        '○ 期間重複次回(一つ後)データ取得
        SQLStr =
              "SELECT               " _
            & "    USERID           " _
            & "   ,STYMD            " _
            & "   ,ENDYMD           " _
            & "FROM                 " _
            & "    COM.LNS0001_USER " _
            & "WHERE                " _
            & "    USERID = @P1     " _
            & "AND DELFLG = @P2     " _
            & "AND STYMD  > @P3     " _
            & "ORDER BY             " _
            & "    STYMD            " _
            & "LIMIT 1              "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 20) 'ユーザーID
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 1)  '削除フラグ
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 20) '利用開始日

                PARA1.Value = TxtUserId.Text       'ユーザーID
                PARA2.Value = C_DELETE_FLG.ALIVE   '削除フラグ
                PARA3.Value = WF_StYMD.Value        '利用開始日

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNS0001Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNS0001Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNS0001Chk.Load(SQLdr)

                    If LNS0001Chk.Rows.Count <> 0 Then
                        Dim NextLNS0001row As DataRow = LNS0001Chk.Rows(0)
                        If WF_EndYMD.Value >= CDate(NextLNS0001row("STYMD")).ToString("yyyy-MM-dd") Or flg = 1 Then
                            flg = 1
                            ' 期間重複が同じデータで無い場合のみ次回情報を表示
                            If pnlTxtLastStYMD.Text <> CDate(NextLNS0001row("STYMD")).ToString("yyyy-MM-dd") Then
                                DisabledKey_OverlapPeriodsInput_End.Value = ""
                                pnlTxtAdjustNextStYMD.Text = CDate(NextLNS0001row("STYMD")).ToString("yyyy/MM/dd")
                                pnlTxtAdjustNextEndYMD.Text = CDate(NextLNS0001row("ENDYMD")).ToString("yyyy/MM/dd")
                                pnlTxtInputStYMD.Text = WF_StYMD.Value
                                pnlTxtInputEndYMD.Text = WF_EndYMD.Value
                                pnlTxtNextStYMD.Text = CDate(NextLNS0001row("STYMD")).ToString("yyyy-MM-dd")
                                pnlTxtNextEndYMD.Text = CDate(NextLNS0001row("ENDYMD")).ToString("yyyy/MM/dd")
                                If pnlTxtInputEndYMD.Text = pnlTxtNextEndYMD.Text Then
                                    pnlTxtNextStYMD.Text = pnlTxtNextEndYMD.Text
                                ElseIf CDate(pnlTxtInputEndYMD.Text) < CDate(pnlTxtNextEndYMD.Text) Then
                                    pnlTxtNextStYMD.Text = DateAdd("d", 1, CDate(pnlTxtInputEndYMD.Text)).ToString("yyyy-MM-dd")
                                End If
                            Else
                                VisibleKey_OverlapPeriodsNext.Value = "none"
                            End If
                        Else
                            VisibleKey_OverlapPeriodsNext.Value = "none"
                        End If
                    Else
                        VisibleKey_OverlapPeriodsNext.Value = "none"
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001D OverlapPeriodsCheck")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0001D OverlapPeriodsCheck"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' ユーザーマスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(ユーザーマスタ)
        Dim SQLStr As String =
              "     INSERT INTO COM.LNS0001_USER            " _
            & "        (DELFLG                              " _
            & "       , USERID                              " _
            & "       , STAFFNAMES                          " _
            & "       , STAFFNAMEL                          " _
            & "       , MAPID                               " _
            & "       , STYMD                               " _
            & "       , ENDYMD                              " _
            & "       , CAMPCODE                            " _
            & "       , ORG                                 " _
            & "       , EMAIL                               " _
            & "       , MENUROLE                            " _
            & "       , MAPROLE                             " _
            & "       , VIEWPROFID                          " _
            & "       , RPRTPROFID                          " _
            & "       , VARIANT                             " _
            & "       , INITYMD                             " _
            & "       , INITUSER                            " _
            & "       , INITTERMID                          " _
            & "       , INITPGID                            " _
            & "       , UPDYMD                              " _
            & "       , UPDUSER                             " _
            & "       , UPDTERMID                           " _
            & "       , UPDPGID                             " _
            & "       , RECEIVEYMD)                         " _
            & "     VALUES                                  " _
            & "        (@P00                                " _
            & "       , @P01                                " _
            & "       , @P02                                " _
            & "       , @P03                                " _
            & "       , @P04                                " _
            & "       , @P08                                " _
            & "       , @P09                                " _
            & "       , @P10                                " _
            & "       , @P11                                " _
            & "       , @P12                                " _
            & "       , @P13                                " _
            & "       , @P14                                " _
            & "       , @P15                                " _
            & "       , @P16                                " _
            & "       , @P17                                " _
            & "       , @P19                                " _
            & "       , @P20                                " _
            & "       , @P21                                " _
            & "       , @P22                                " _
            & "       , @P23                                " _
            & "       , @P24                                " _
            & "       , @P25                                " _
            & "       , @P26                                " _
            & "       , @P27)                               " _
            & "     ON DUPLICATE KEY UPDATE                 " _
            & "         DELFLG     = @P00                   " _
            & "       , STAFFNAMES = @P02                   " _
            & "       , STAFFNAMEL = @P03                   " _
            & "       , MAPID      = @P04                   " _
            & "       , ENDYMD     = @P09                   " _
            & "       , ORG        = @P11                   " _
            & "       , EMAIL      = @P12                   " _
            & "       , UPDYMD     = @P23                   " _
            & "       , UPDUSER    = @P24                   " _
            & "       , UPDTERMID  = @P25                   " _
            & "       , UPDPGID    = @P26                   " _
            & "       , RECEIVEYMD = @P27                   " _

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                     " _
            & "     DELFLG                                 " _
            & "   , USERID                                 " _
            & "   , STAFFNAMES                             " _
            & "   , STAFFNAMEL                             " _
            & "   , MAPID                                  " _
            & "   , STYMD                                  " _
            & "   , ENDYMD                                 " _
            & "   , CAMPCODE                               " _
            & "   , ORG                                    " _
            & "   , EMAIL                                  " _
            & "   , MENUROLE                               " _
            & "   , MAPROLE                                " _
            & "   , VIEWPROFID                             " _
            & "   , RPRTPROFID                             " _
            & "   , VARIANT                                " _
            & "   , INITYMD                                " _
            & "   , INITUSER                               " _
            & "   , INITTERMID                             " _
            & "   , INITPGID                               " _
            & "   , UPDYMD                                 " _
            & "   , UPDUSER                                " _
            & "   , UPDTERMID                              " _
            & "   , UPDPGID                                " _
            & "   , RECEIVEYMD                             " _
            & "   , UPDTIMSTP                              " _
            & " FROM                                       " _
            & "     COM.LNS0001_USER                       " _
            & " WHERE                                      " _
            & "         USERID = @P01                      " _
            & "     AND STYMD  = @P08                      " _
            & "     AND ENDYMD  = @P09                      "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                ' DB更新用パラメータ
                Dim PARA00 As MySqlParameter = SQLcmd.Parameters.Add("@P00", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 20)        'ユーザーID
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 20)        '社員名（短）
                Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 50)        '社員名（長）
                Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar, 20)        '画面ＩＤ
                Dim PARA08 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.Date)                '開始年月日
                Dim PARA09 As MySqlParameter = SQLcmd.Parameters.Add("@P09", MySqlDbType.Date)                '終了年月日
                Dim PARA10 As MySqlParameter = SQLcmd.Parameters.Add("@P10", MySqlDbType.VarChar, 2)         '会社コード
                Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.VarChar, 6)         '組織コード
                Dim PARA12 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.VarChar, 128)       'メールアドレス
                Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar, 20)        'メニュー表示制御ロール
                Dim PARA14 As MySqlParameter = SQLcmd.Parameters.Add("@P14", MySqlDbType.VarChar, 20)        '画面参照更新制御ロール
                Dim PARA15 As MySqlParameter = SQLcmd.Parameters.Add("@P15", MySqlDbType.VarChar, 20)        '画面表示項目制御ロール
                Dim PARA16 As MySqlParameter = SQLcmd.Parameters.Add("@P16", MySqlDbType.VarChar, 20)        'エクセル出力制御ロール
                Dim PARA17 As MySqlParameter = SQLcmd.Parameters.Add("@P17", MySqlDbType.VarChar, 20)        '画面初期値ロール
                'Dim PARA18 As MySqlParameter = SQLcmd.Parameters.Add("@P18", MySqlDbType.VarChar, 20)        '承認権限ロール
                Dim PARA19 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.DateTime)            '登録年月日
                Dim PARA20 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.VarChar, 20)        '登録ユーザーＩＤ
                Dim PARA21 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.VarChar, 20)        '登録端末
                Dim PARA22 As MySqlParameter = SQLcmd.Parameters.Add("@P22", MySqlDbType.VarChar, 40)        '登録プログラムＩＤ
                Dim PARA23 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.DateTime)            '更新年月日
                Dim PARA24 As MySqlParameter = SQLcmd.Parameters.Add("@P24", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim PARA25 As MySqlParameter = SQLcmd.Parameters.Add("@P25", MySqlDbType.VarChar, 20)        '更新端末
                Dim PARA26 As MySqlParameter = SQLcmd.Parameters.Add("@P26", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ
                Dim PARA27 As MySqlParameter = SQLcmd.Parameters.Add("@P27", MySqlDbType.DateTime)            '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JPARA01 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 20)    'ユーザーID
                Dim JPARA08 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P08", MySqlDbType.Date)            '開始年月日
                Dim JPARA09 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P09", MySqlDbType.Date)               '終了年月日

                Dim LNS0001row As DataRow = LNS0001INPtbl.Rows(0)

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新

                PARA00.Value = LNS0001row("DELFLG")                            '削除フラグ
                PARA01.Value = LNS0001row("USERID")                            'ユーザーID
                PARA02.Value = LNS0001row("STAFFNAMES")                        '社員名（短）
                PARA03.Value = LNS0001row("STAFFNAMEL")                        '社員名（長）
                PARA04.Value = LNS0001row("MAPID")                             '画面ＩＤ
                If Not String.IsNullOrEmpty(RTrim(LNS0001row("STYMD"))) Then   '開始年月日
                    PARA08.Value = RTrim(LNS0001row("STYMD"))
                Else
                    PARA08.Value = C_DEFAULT_YMD
                End If
                If Not String.IsNullOrEmpty(RTrim(LNS0001row("ENDYMD"))) Then  '終了年月日
                    PARA09.Value = RTrim(LNS0001row("ENDYMD"))
                Else
                    PARA09.Value = C_DEFAULT_YMD
                End If
                PARA10.Value = LNS0001row("CAMPCODE")                          '会社コード
                PARA11.Value = LNS0001row("ORG")                               '組織コード
                PARA12.Value = LNS0001row("EMAIL")                             'メールアドレス
                'PARA13.Value = LNS0001row("MENUROLE")                          'メニュー表示制御ロール
                'PARA14.Value = LNS0001row("MAPROLE")                           '画面参照更新制御ロール
                'PARA15.Value = LNS0001row("VIEWPROFID")                        '画面表示項目制御ロール
                'PARA16.Value = LNS0001row("RPRTPROFID")                        'エクセル出力制御ロール
                'PARA17.Value = LNS0001row("VARIANT")                           '画面初期値ロール

                Master.GetFirstValue(Master.USERCAMP, "MENUROLE", PARA13.Value) 'メニュー表示制御ロール
                Master.GetFirstValue(Master.USERCAMP, "MAPROLE", PARA14.Value) '画面参照更新制御ロール
                Master.GetFirstValue(Master.USERCAMP, "VIEWPROFID", PARA15.Value) '画面表示項目制御ロール
                Master.GetFirstValue(Master.USERCAMP, "RPRTPROFID", PARA16.Value) 'エクセル出力制御ロール
                Master.GetFirstValue(Master.USERCAMP, "VARIANT", PARA17.Value) '画面初期値ロール

                'PARA18.Value = LNS0001row("APPROVALID")                        '承認権限ロール
                PARA19.Value = WW_DateNow                                      '登録年月日
                PARA20.Value = Master.USERID                                   '登録ユーザーＩＤ
                PARA21.Value = Master.USERTERMID                               '登録端末
                PARA22.Value = Me.GetType().BaseType.Name                      '登録プログラムＩＤ
                PARA23.Value = WW_DateNow                                      '更新年月日
                PARA24.Value = Master.USERID                                   '更新ユーザーＩＤ
                PARA25.Value = Master.USERTERMID                               '更新端末
                PARA26.Value = Me.GetType().BaseType.Name                      '更新プログラムＩＤ
                PARA27.Value = C_DEFAULT_YMD                                   '集信日時

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA01.Value = LNS0001row("USERID")                          'ユーザーID
                If Not String.IsNullOrEmpty(RTrim(LNS0001row("STYMD"))) Then  '開始年月日
                    JPARA08.Value = RTrim(LNS0001row("STYMD"))
                Else
                    JPARA08.Value = C_DEFAULT_YMD
                End If
                If Not String.IsNullOrEmpty(RTrim(LNS0001row("ENDYMD"))) Then  '終了年月日
                    JPARA09.Value = RTrim(LNS0001row("ENDYMD"))
                Else
                    JPARA09.Value = C_DEFAULT_YMD
                End If

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNS0001UPDtbl) Then
                        LNS0001UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNS0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNS0001UPDtbl.Clear()
                    LNS0001UPDtbl.Load(SQLdr)
                End Using

                For Each LNS0001UPDrow As DataRow In LNS0001UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNS0001D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNS0001UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0001D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        '○ DB更新SQL(ユーザーパスワードマスタ)
        SQLStr =
              "     INSERT INTO COM.LNS0002_USERPASS                                  " _
            & "        (DELFLG                                                        " _
            & "       , USERID                                                        " _
            & "       , PASSWORD                                                      " _
            & "       , MISSCNT                                                       " _
            & "       , PASSENDYMD                                                    " _
            & "       , INITYMD                                                       " _
            & "       , INITUSER                                                      " _
            & "       , INITTERMID                                                    " _
            & "       , RECEIVEYMD)                                                   " _
            & "     VALUES                                                            " _
            & "        (@P00                                                          " _
            & "       , @P01                                                          " _
            & "       , AES_ENCRYPT(@P05, 'loginpasskey')                             " _
            & "       , @P06                                                          " _
            & "       , @P07                                                          " _
            & "       , @P19                                                          " _
            & "       , @P20                                                          " _
            & "       , @P21                                                          " _
            & "       , @P27)                                                         " _
            & "     ON DUPLICATE KEY UPDATE                                           " _
            & "         DELFLG     = @P00                                             " _
            & "       , PASSWORD   = AES_ENCRYPT(@P05, 'loginpasskey')                " _
            & "       , MISSCNT    = @P06                                             " _
            & "       , PASSENDYMD = @P07                                             " _
            & "       , UPDYMD     = @P23                                             " _
            & "       , UPDUSER    = @P24                                             " _
            & "       , UPDTERMID  = @P25                                             " _
            & "       , RECEIVEYMD = @P27                                             " _

        '○ 更新ジャーナル出力SQL
        SQLJnl =
              " Select                                     " _
            & "     DELFLG                                 " _
            & "   , USERID                                 " _
            & "   , PASSWORD                               " _
            & "   , MISSCNT                                " _
            & "   , PASSENDYMD                             " _
            & "   , INITYMD                                " _
            & "   , INITUSER                               " _
            & "   , INITTERMID                             " _
            & "   , UPDYMD                                 " _
            & "   , UPDUSER                                " _
            & "   , UPDTERMID                              " _
            & "   , RECEIVEYMD                             " _
            & "   , UPDTIMSTP                              " _
            & " FROM                                       " _
            & "     COM.LNS0002_USERPASS                   " _
            & " WHERE                                      " _
            & "     USERID = @P01                          "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                ' DB更新用パラメータ(ユーザーパスワードマスタ)
                Dim PARA00 As MySqlParameter = SQLcmd.Parameters.Add("@P00", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 20)        'ユーザーID
                Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar, 200)       'パスワード
                Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.Int32)                 '誤り回数
                Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.Date)                'パスワード有効期限
                Dim PARA19 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.DateTime)            '登録年月日
                Dim PARA20 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.VarChar, 20)        '登録ユーザーＩＤ
                Dim PARA21 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.VarChar, 20)        '登録端末
                'Dim PARA22 As MySqlParameter = SQLcmd.Parameters.Add("@P22", MySqlDbType.VarChar, 40)        '登録プログラムＩＤ
                Dim PARA23 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.DateTime)            '更新年月日
                Dim PARA24 As MySqlParameter = SQLcmd.Parameters.Add("@P24", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim PARA25 As MySqlParameter = SQLcmd.Parameters.Add("@P25", MySqlDbType.VarChar, 20)        '更新端末
                'Dim PARA26 As MySqlParameter = SQLcmd.Parameters.Add("@P26", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ
                Dim PARA27 As MySqlParameter = SQLcmd.Parameters.Add("@P27", MySqlDbType.DateTime)            '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JPARA01 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 20)    'ユーザーID

                Dim LNS0001row As DataRow = LNS0001INPtbl.Rows(0)

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA00.Value = LNS0001row("DELFLG")                                '削除フラグ
                PARA01.Value = LNS0001row("USERID")                                'ユーザーID
                PARA05.Value = LNS0001row("PASSWORD")                              'パスワード
                If Not String.IsNullOrEmpty(LNS0001row("MISSCNT")) Then            '誤り回数
                    PARA06.Value = LNS0001row("MISSCNT")
                Else
                    PARA06.Value = "0"
                End If
                If Not String.IsNullOrEmpty(RTrim(LNS0001row("PASSENDYMD"))) Then  'パスワード有効期限
                    PARA07.Value = RTrim(LNS0001row("PASSENDYMD"))
                Else
                    PARA07.Value = C_DEFAULT_YMD
                End If
                PARA19.Value = WW_DateNow                                          '登録年月日
                PARA20.Value = Master.USERID                                       '登録ユーザーＩＤ
                PARA21.Value = Master.USERTERMID                                   '登録端末
                'PARA22.Value = Me.GetType().BaseType.Name                          '登録プログラムＩＤ
                PARA23.Value = WW_DateNow                                          '更新年月日
                PARA24.Value = Master.USERID                                       '更新ユーザーＩＤ
                PARA25.Value = Master.USERTERMID                                   '更新端末
                'PARA26.Value = Me.GetType().BaseType.Name                          '更新プログラムＩＤ
                PARA27.Value = C_DEFAULT_YMD                                       '集信日時
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                LNS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA

                ' 更新ジャーナル出力
                JPARA01.Value = LNS0001row("USERID")  'ユーザーID

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNS0001UPDtbl) Then
                        LNS0001UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNS0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNS0001UPDtbl.Clear()
                    LNS0001UPDtbl.Load(SQLdr)
                End Using

                For Each LNS0001UPDrow As DataRow In LNS0001UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNS0001D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNS0001UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0001D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' ユーザーマスタ期間重複データ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateOverlapPeriodsMasterData(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(ユーザーマスタ)
        Dim SQLStr As String =
              "UPDATE COM.LNS0001_USER " _
            & "SET                     " _
            & "    ENDYMD     = @P03   " _
            & "   ,UPDYMD     = @P04   " _
            & "   ,UPDUSER    = @P05   " _
            & "   ,UPDTERMID  = @P06   " _
            & "   ,UPDPGID    = @P07   " _
            & "   ,RECEIVEYMD = @P08   " _
            & "WHERE                   " _
            & "        USERID = @P01   " _
            & "    AND STYMD  = @P02   " _

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                     " _
            & "     DELFLG                                 " _
            & "   , USERID                                 " _
            & "   , STAFFNAMES                             " _
            & "   , STAFFNAMEL                             " _
            & "   , MAPID                                  " _
            & "   , STYMD                                  " _
            & "   , ENDYMD                                 " _
            & "   , CAMPCODE                               " _
            & "   , ORG                                    " _
            & "   , EMAIL                                  " _
            & "   , MENUROLE                               " _
            & "   , MAPROLE                                " _
            & "   , VIEWPROFID                             " _
            & "   , RPRTPROFID                             " _
            & "   , VARIANT                                " _
            & "   , INITYMD                                " _
            & "   , INITUSER                               " _
            & "   , INITTERMID                             " _
            & "   , INITPGID                               " _
            & "   , UPDYMD                                 " _
            & "   , UPDUSER                                " _
            & "   , UPDTERMID                              " _
            & "   , UPDPGID                                " _
            & "   , RECEIVEYMD                             " _
            & "   , UPDTIMSTP                              " _
            & " FROM                                       " _
            & "     COM.LNS0001_USER                       " _
            & " WHERE                                      " _
            & "         USERID = @P01                      " _
            & "     AND STYMD  = @P02                      "

        If Not String.IsNullOrEmpty(pnlTxtLastStYMD.Text) Then
            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                    ' DB更新用パラメータ
                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 20)        'ユーザーID
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.Date)                '開始年月日
                    Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.Date)                '終了年月日
                    Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.DateTime)            '更新年月日
                    Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                    Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar, 20)        '更新端末
                    Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ
                    Dim PARA08 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.DateTime)            '集信日時

                    ' 更新ジャーナル出力用パラメータ
                    Dim JPARA01 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 20)    'ユーザーID
                    Dim JPARA02 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P02", MySqlDbType.Date)            '開始年月日

                    Dim LNS0001row As DataRow = LNS0001INPtbl.Rows(0)

                    Dim WW_DateNow As DateTime = Date.Now

                    ' DB更新

                    PARA01.Value = LNS0001row("USERID")                           'ユーザーID
                    PARA02.Value = RTrim(pnlTxtLastStYMD.Text)                    '開始年月日
                    PARA03.Value = RTrim(pnlTxtLastEndYMD.Text)                   '終了年月日
                    PARA04.Value = WW_DateNow                                     '更新年月日
                    PARA05.Value = Master.USERID                                  '更新ユーザーＩＤ
                    PARA06.Value = Master.USERTERMID                              '更新端末
                    PARA07.Value = Me.GetType().BaseType.Name                     '更新プログラムＩＤ
                    PARA08.Value = C_DEFAULT_YMD                                  '集信日時
                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    ' 更新ジャーナル出力
                    JPARA01.Value = LNS0001row("USERID")                          'ユーザーID
                    JPARA02.Value = RTrim(pnlTxtLastStYMD.Text)                   '開始年月日

                    Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(LNS0001UPDtbl) Then
                            LNS0001UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                LNS0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        LNS0001UPDtbl.Clear()
                        LNS0001UPDtbl.Load(SQLdr)
                    End Using

                    For Each LNS0001UPDrow As DataRow In LNS0001UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "LNS0001D"
                        CS0020JOURNAL.ACTION = "LASTDATA_UPDATE"
                        CS0020JOURNAL.ROW = LNS0001UPDrow
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
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001D LASTDATA_UPDATE")

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNS0001D LASTDATA_UPDATE"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try
        End If

        If Not String.IsNullOrEmpty(pnlTxtNextStYMD.Text) Then

            '○ DB更新SQL(ユーザーマスタ)
            SQLStr =
              "UPDATE COM.LNS0001_USER " _
            & "SET                     " _
            & "    STYMD      = @P03   " _
            & "   ,UPDYMD     = @P04   " _
            & "   ,UPDUSER    = @P05   " _
            & "   ,UPDTERMID  = @P06   " _
            & "   ,UPDPGID    = @P07   " _
            & "   ,RECEIVEYMD = @P08   " _
            & "WHERE                   " _
            & "        USERID = @P01   " _
            & "    AND STYMD  = @P02   "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                    ' DB更新用パラメータ
                    Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 20)        'ユーザーID
                    Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.Date)                '開始年月日
                    Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.Date)                '終了年月日
                    Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.DateTime)            '更新年月日
                    Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                    Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar, 20)        '更新端末
                    Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ
                    Dim PARA08 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.DateTime)            '集信日時

                    ' 更新ジャーナル出力用パラメータ
                    Dim JPARA01 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 20)    'ユーザーID
                    Dim JPARA02 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P02", MySqlDbType.Date)            '開始年月日

                    Dim LNS0001row As DataRow = LNS0001INPtbl.Rows(0)

                    Dim WW_DateNow As DateTime = Date.Now

                    ' DB更新
                    PARA01.Value = LNS0001row("USERID")                           'ユーザーID
                    PARA02.Value = RTrim(pnlTxtAdjustNextStYMD.Text)       '開始年月日(調整前)
                    PARA03.Value = RTrim(pnlTxtNextStYMD.Text)                    '開始年月日(調整後)
                    PARA04.Value = WW_DateNow                                     '更新年月日
                    PARA05.Value = Master.USERID                                  '更新ユーザーＩＤ
                    PARA06.Value = Master.USERTERMID                              '更新端末
                    PARA07.Value = Me.GetType().BaseType.Name                     '更新プログラムＩＤ
                    PARA08.Value = C_DEFAULT_YMD                                  '集信日時
                    SQLcmd.CommandTimeout = 300
                    SQLcmd.ExecuteNonQuery()

                    ' 更新ジャーナル出力
                    JPARA01.Value = LNS0001row("USERID")                          'ユーザーID
                    JPARA02.Value = RTrim(pnlTxtAdjustNextStYMD.Text)      '開始年月日

                    Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                        If IsNothing(LNS0001UPDtbl) Then
                            LNS0001UPDtbl = New DataTable

                            For index As Integer = 0 To SQLdr.FieldCount - 1
                                LNS0001UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                            Next
                        End If

                        LNS0001UPDtbl.Clear()
                        LNS0001UPDtbl.Load(SQLdr)
                    End Using

                    For Each LNS0001UPDrow As DataRow In LNS0001UPDtbl.Rows
                        CS0020JOURNAL.TABLENM = "LNS0001D"
                        CS0020JOURNAL.ACTION = "NEXTDATA_UPDATE"
                        CS0020JOURNAL.ROW = LNS0001UPDrow
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
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001D NEXTDATA_UPDATE")

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNS0001D NEXTDATA_UPDATE"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

                rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
                WW_ErrSW = C_MESSAGE_NO.DB_ERROR
                Exit Sub
            End Try
        End If
    End Sub

#Region "変更履歴テーブル登録"
    ''' <summary>
    ''' 変更チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MASTEREXISTS(ByVal SQLcon As MySqlConnection, ByRef WW_MODIFYKBN As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        'ユーザマスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        USERID")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        COM.LNS0001_USER")
        SQLStr.AppendLine("    WHERE")
        SQLStr.Append("         USERID = @USERID                    ")
        SQLStr.Append("     AND STYMD   = @STYMD                    ")
        SQLStr.Append("     AND ENDYMD   = @ENDYMD                  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 20)     'ユーザーID
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '開始年月日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '終了年月日

                Dim LNS0001row As DataRow = LNS0001INPtbl.Rows(0)

                P_USERID.Value = LNS0001row("USERID")           'ユーザーID
                P_STYMD.Value = LNS0001row("STYMD")           '開始年月日
                P_ENDYMD.Value = LNS0001row("ENDYMD")           '終了年月日


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
                        WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0001C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0001C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0002_USERHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      USERID  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,CAMPCODE  ")
        SQLStr.AppendLine("     ,ORG  ")
        SQLStr.AppendLine("     ,STAFFNAMES  ")
        SQLStr.AppendLine("     ,STAFFNAMEL  ")
        SQLStr.AppendLine("     ,EMAIL  ")
        SQLStr.AppendLine("     ,MENUROLE  ")
        SQLStr.AppendLine("     ,MAPROLE  ")
        SQLStr.AppendLine("     ,VIEWPROFID  ")
        SQLStr.AppendLine("     ,RPRTPROFID  ")
        SQLStr.AppendLine("     ,MAPID  ")
        SQLStr.AppendLine("     ,VARIANT  ")
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
        SQLStr.AppendLine("      USERID  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,CAMPCODE  ")
        SQLStr.AppendLine("     ,ORG  ")
        SQLStr.AppendLine("     ,STAFFNAMES  ")
        SQLStr.AppendLine("     ,STAFFNAMEL  ")
        SQLStr.AppendLine("     ,EMAIL  ")
        SQLStr.AppendLine("     ,MENUROLE  ")
        SQLStr.AppendLine("     ,MAPROLE  ")
        SQLStr.AppendLine("     ,VIEWPROFID  ")
        SQLStr.AppendLine("     ,RPRTPROFID  ")
        SQLStr.AppendLine("     ,MAPID  ")
        SQLStr.AppendLine("     ,VARIANT  ")
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
        SQLStr.AppendLine("        COM.LNS0001_USER")
        SQLStr.AppendLine("    WHERE")
        SQLStr.Append("         USERID = @USERID                    ")
        SQLStr.Append("     AND STYMD   = @STYMD                    ")
        SQLStr.Append("     AND ENDYMD   = @ENDYMD                  ")
        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 20)     'ユーザーID
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '開始年月日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '終了年月日

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNS0001row As DataRow = LNS0001INPtbl.Rows(0)

                ' DB更新
                P_USERID.Value = LNS0001row("USERID")           'ユーザーID
                P_STYMD.Value = LNS0001row("STYMD")           '開始年月日
                P_ENDYMD.Value = LNS0001row("ENDYMD")           '終了年月日

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNS0001WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNS0001tbl.Rows(0)("DELFLG") = "0" And LNS0001row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNS0001WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNS0001WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0002_USERHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0002_USERHIST  INSERT"
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
        DetailBoxToLNS0001INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then

            ' 有効期間重複チェック
            Dim LNS0001row As DataRow = LNS0001tbl.Rows(0)
            Dim OverlapPeriodsFlg As Integer = 0
            ' 新規登録チェック
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' 有効期間重複チェック
                OverlapPeriodsCheck(SQLcon, OverlapPeriodsFlg)
            End Using

            If OverlapPeriodsFlg <> 0 Then
                '○ 期間重複調整用子画面を表示
                WF_OverlapPeriodsSrc.Value = "visible"
                ' 有効期間重複エラー
                Master.Output(C_MESSAGE_NO.CTN_OVERLAPPERIODS_ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Exit Sub
            End If

            LNS0001tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNS0001tbl, work.WF_SEL_INPTBL.Text)

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
    ''' 詳細画面-期間重複調整子画面-更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonOverlapPeriodsSrcUpdateClick()

        '○ エラーレポート準備
        rightview.SetErrorReport("")
        Dim OverlapPeriodsFlg As Integer = 0

        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNS0001INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目再チェック
        INPOverlapPeriodsCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            '○ DetailBoxをINPtblへ退避
            Dim LNS0001INProw As DataRow = LNS0001INPtbl.Rows(0)
            LNS0001INProw("STYMD") = pnlTxtInputStYMD.Text             '開始年月日
            LNS0001INProw("ENDYMD") = pnlTxtInputEndYMD.Text           '終了年月日

            ' 期間重複データ DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' マスタ更新
                UpdateOverlapPeriodsMasterData(SQLcon)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If
            End Using

            LNS0001tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If

            '○ 期間重複調整用個画面を非表示
            WF_OverlapPeriodsSrc.Value = "hidden"
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNS0001tbl, work.WF_SEL_INPTBL.Text)

        '入力日付保存
        If Not String.IsNullOrEmpty(pnlTxtLastEndYMD.Text) Then
            pnlTxtLastEndYMD.Text = CDate(pnlTxtLastEndYMD.Text).ToString("yyyy-MM-dd")
        End If
        If Not String.IsNullOrEmpty(pnlTxtInputStYMD.Text) Then
            pnlTxtInputStYMD.Text = CDate(pnlTxtInputStYMD.Text).ToString("yyyy-MM-dd")
        End If
        If Not String.IsNullOrEmpty(pnlTxtInputEndYMD.Text) Then
            pnlTxtInputEndYMD.Text = CDate(pnlTxtInputEndYMD.Text).ToString("yyyy-MM-dd")
        End If
        If Not String.IsNullOrEmpty(pnlTxtNextStYMD.Text) Then
            pnlTxtNextStYMD.Text = CDate(pnlTxtNextStYMD.Text).ToString("yyyy-MM-dd")
        End If

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
            ElseIf WW_ErrSW = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR Then
                ' 排他エラー
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            ElseIf WW_ErrSW = C_MESSAGE_NO.CTN_OVERLAPPERIODS_NOTDATE_ERR Then
                ' 不正値入力エラー
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            ElseIf WW_ErrSW = C_MESSAGE_NO.CTN_OVERLAPPERIODS_ERR Then
                ' 日付大小入力エラー
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            ElseIf WW_ErrSW = C_MESSAGE_NO.CTN_OVERLAPPERIODS_PASTDATE_ERR Then
                ' 過去日入力エラー
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Else
                ' その他エラー
                Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
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
    Protected Sub DetailBoxToLNS0001INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)      '削除フラグ
        Master.EraseCharToIgnore(TxtUserId.Text)      'ユーザーID
        Master.EraseCharToIgnore(TxtStaffNameS.Text)  '社員名（短）
        Master.EraseCharToIgnore(TxtStaffNameL.Text)  '社員名（長）
        Master.EraseCharToIgnore(TxtMapId.Text)       '画面ＩＤ
        Master.EraseCharToIgnore(TxtPassword.Text)    'パスワード
        Master.EraseCharToIgnore(TxtMissCNT.Text)     '誤り回数
        Master.EraseCharToIgnore(TxtPassEndYMD.Text)  'パスワード有効期限
        Master.EraseCharToIgnore(WF_StYMD.Value)       '開始年月日
        Master.EraseCharToIgnore(WF_EndYMD.Value)      '終了年月日
        Master.EraseCharToIgnore(TxtCampCode.Text)    '会社コード
        Master.EraseCharToIgnore(ddlSelectORG.SelectedValue)         '組織コード
        Master.EraseCharToIgnore(TxtEMail.Text)       'メールアドレス
        'Master.EraseCharToIgnore(TxtMenuRole.Text)    'メニュー表示制御ロール
        'Master.EraseCharToIgnore(TxtMapRole.Text)     '画面参照更新制御ロール
        'Master.EraseCharToIgnore(TxtViewProfId.Text)  '画面表示項目制御ロール
        'Master.EraseCharToIgnore(TxtRprtProfId.Text)  'エクセル出力制御ロール
        'Master.EraseCharToIgnore(TxtVariant.Text)     '画面初期値ロール
        'Master.EraseCharToIgnore(TxtApproValid.Text)  '承認権限ロール

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

        Master.CreateEmptyTable(LNS0001INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNS0001INProw As DataRow = LNS0001INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) Then
            LNS0001INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(TxtSelLineCNT.Text, LNS0001INProw("LINECNT"))
            Catch ex As Exception
                LNS0001INProw("LINECNT") = 0
            End Try
        End If

        LNS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNS0001INProw("UPDTIMSTP") = 0
        LNS0001INProw("SELECT") = 1
        LNS0001INProw("HIDDEN") = 0

        LNS0001INProw("DELFLG") = TxtDelFlg.Text           '削除フラグ
        LNS0001INProw("USERID") = TxtUserId.Text           'ユーザーID
        LNS0001INProw("STAFFNAMES") = TxtStaffNameS.Text   '社員名（短）
        LNS0001INProw("STAFFNAMEL") = TxtStaffNameL.Text   '社員名（長）
        LNS0001INProw("MAPID") = TxtMapId.Text             '画面ＩＤ
        LNS0001INProw("PASSWORD") = TxtPassword.Text       'パスワード
        LNS0001INProw("MISSCNT") = TxtMissCNT.Text         '誤り回数
        LNS0001INProw("PASSENDYMD") = TxtPassEndYMD.Text   'パスワード有効期限
        LNS0001INProw("STYMD") = WF_StYMD.Value             '開始年月日
        LNS0001INProw("ENDYMD") = WF_EndYMD.Value           '終了年月日
        LNS0001INProw("CAMPCODE") = TxtCampCode.Text       '会社コード
        LNS0001INProw("ORG") = ddlSelectORG.SelectedValue                 '組織コード
        LNS0001INProw("EMAIL") = TxtEMail.Text             'メールアドレス
        'LNS0001INProw("MENUROLE") = TxtMenuRole.Text       'メニュー表示制御ロール
        'LNS0001INProw("MAPROLE") = TxtMapRole.Text         '画面参照更新制御ロール
        'LNS0001INProw("VIEWPROFID") = TxtViewProfId.Text   '画面表示項目制御ロール
        'LNS0001INProw("RPRTPROFID") = TxtRprtProfId.Text   'エクセル出力制御ロール
        'LNS0001INProw("VARIANT") = TxtVariant.Text         '画面初期値ロール
        'LNS0001INProw("APPROVALID") = TxtApproValid.Text   '承認権限ロール

        '○ チェック用テーブルに登録する
        LNS0001INPtbl.Rows.Add(LNS0001INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNS0001INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNS0001INProw As DataRow = LNS0001INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNS0001row As DataRow In LNS0001tbl.Rows
            ' KEY項目が等しい時
            If LNS0001row("USERID") = LNS0001INProw("USERID") AndAlso
                LNS0001row("STYMD") = LNS0001INProw("STYMD") Then
                ' KEY項目以外の項目の差異をチェック
                If LNS0001row("DELFLG") = LNS0001INProw("DELFLG") AndAlso
                    LNS0001row("STAFFNAMES") = LNS0001INProw("STAFFNAMES") AndAlso
                    LNS0001row("STAFFNAMEL") = LNS0001INProw("STAFFNAMEL") AndAlso
                    LNS0001row("MAPID") = LNS0001INProw("MAPID") AndAlso
                    LNS0001row("PASSWORD") = LNS0001INProw("PASSWORD") AndAlso
                    LNS0001row("MISSCNT") = LNS0001INProw("MISSCNT") AndAlso
                    LNS0001row("PASSENDYMD") = LNS0001INProw("PASSENDYMD") AndAlso
                    LNS0001row("ENDYMD") = LNS0001INProw("ENDYMD") AndAlso
                    LNS0001row("CAMPCODE") = LNS0001INProw("CAMPCODE") AndAlso
                    LNS0001row("ORG") = LNS0001INProw("ORG") AndAlso
                    LNS0001row("EMAIL") = LNS0001INProw("EMAIL") Then
                    ' 変更がない時は、入力変更フラグをOFFにする
                    WW_InputChangeFlg = False
                End If

                Exit For

            End If
        Next

        'パンくずから検索を選択した場合
        If WF_ButtonClick.Value = "LNS0001S" Then
            WF_BeforeMAPID.Value = LNS0001WRKINC.MAPIDL
        Else
            WF_BeforeMAPID.Value = LNS0001WRKINC.MAPIDD
        End If

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

        Master.MAPID = WF_BeforeMAPID.Value
        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each LNS0001row As DataRow In LNS0001tbl.Rows
            Select Case LNS0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNS0001row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNS0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNS0001tbl, work.WF_SEL_INPTBL.Text)

        TxtSelLineCNT.Text = ""              'LINECNT
        TxtUserId.Text = ""                   'ユーザーID
        TxtStaffNameS.Text = ""               '社員名（短）
        TxtStaffNameL.Text = ""               '社員名（長）
        TxtMapId.Text = "M00001"              '画面ＩＤ
        TxtPassword.Text = ""                 'パスワード
        TxtPassword.Attributes("Value") = ""
        TxtMissCNT.Text = ""                  '誤り回数
        TxtPassEndYMD.Text = ""               'パスワード有効期限
        WF_StYMD.Value = ""                    '開始年月日
        WF_EndYMD.Value = ""                   '終了年月日
        TxtCampCode.Text = ""                 '会社コード
        ddlSelectORG.SelectedValue = ""                      '組織コード
        TxtEMail.Text = ""                    'メールアドレス
        'TxtMenuRole.Text = ""                 'メニュー表示制御ロール
        'TxtMapRole.Text = ""                  '画面参照更新制御ロール
        'TxtViewProfId.Text = ""               '画面表示項目制御ロール
        'TxtRprtProfId.Text = ""               'エクセル出力制御ロール
        'TxtVariant.Text = ""                  '画面初期値ロール
        'TxtApproValid.Text = ""               '承認権限ロール
        TxtDelFlg.Text = ""                   '削除フラグ
        LblDelFlgName.Text = ""              '削除フラグ名称

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
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        ' 日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "TxtPassEndYMD"  'パスワード有効期限
                                .WF_Calendar.Text = TxtPassEndYMD.Text
                            Case "WF_StYMD"       '有効年月日(From)
                                .WF_Calendar.Text = WF_StYMD.Value
                            Case "WF_EndYMD"      '有効年月日(To)
                                .WF_Calendar.Text = WF_EndYMD.Value
                        End Select
                        .ActiveCalendar()
                    Case Else
                        ' フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "TxtCampCode"               '会社コード
                                If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                                    ' 情報システムの場合
                                    WW_PrmData = work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ALL, TxtCampCode.Text)
                                Else
                                    WW_PrmData = work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ROLE, TxtCampCode.Text)
                                End If
                            Case "TxtOrg"         '組織コード
                                If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                                    ' 情報システムの場合、操作ユーザーが所属する会社の組織を全て取得
                                    WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG, TxtCampCode.Text)
                                Else
                                    ' その他の場合、操作ユーザーの組織のみ取得
                                    WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY, TxtCampCode.Text)
                                End If
                            Case "TxtMenuRole"    'メニュー表示制御ロール
                                WW_PrmData = work.CreateRoleList(TxtCampCode.Text, "MENU")
                            Case "TxtMapRole"     '画面参照更新制御ロール
                                WW_PrmData = work.CreateRoleList(TxtCampCode.Text, "MAP")
                            Case "TxtViewProfId"  '画面表示項目制御ロール
                                WW_PrmData = work.CreateRoleList(TxtCampCode.Text, "VIEW")
                            Case "TxtRprtProfId"  'エクセル出力制御ロール
                                WW_PrmData = work.CreateRoleList(TxtCampCode.Text, "XML")
                            'Case "TxtApproValid"  '承認権限ロール
                            '    WW_PrmData = work.CreateRoleList(TxtCampCode.Text, "APPROVAL")
                            Case "TxtDelFlg"
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                        End Select
                        .SetListBox(WF_LeftMViewChange.Value, WW_Dummy, WW_PrmData)
                        .ActiveListBox()
                End Select
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
            Case "TxtCampCode"                   '会社コード
                CODENAME_get("CAMPCODE", TxtCampCode.Text, LblCampCodeName.Text, WW_Dummy)
                createListBox()
                TxtCampCode.Focus()
            Case "TxtPassword"    'パスワード
                TxtPassword.Attributes("Value") = work.WF_SEL_PASSWORD.Text
                TxtPassword.Focus()
            'Case "TxtOrg"         '組織コード
            '    CODENAME_get("ORG", ddlSelectORG.SelectedValue, LblOrgName.Text, WW_RtnSW)
            '    TxtOrg.Focus()
            'Case "TxtMenuRole"    'メニュー表示制御ロール
            '    CODENAME_get("MENU", TxtMenuRole.Text, LblMenuRoleName.Text, WW_Dummy)
            '    TxtMenuRole.Focus()
            'Case "TxtMapRole"     '画面参照更新制御ロール
            '    CODENAME_get("MAP", TxtMapRole.Text, LblMapRoleName.Text, WW_Dummy)
            '    TxtMapRole.Focus()
            'Case "TxtViewProfId"  '画面表示項目制御ロール
            '    CODENAME_get("VIEW", TxtViewProfId.Text, LblViewProfIdName.Text, WW_Dummy)
            '    TxtViewProfId.Focus()
            'Case "TxtRprtProfId"  'エクセル出力制御ロール
            '    CODENAME_get("XML", TxtRprtProfId.Text, LblRprtProfIdName.Text, WW_Dummy)
            '    TxtRprtProfId.Focus()
            'Case "TxtApproValid"  '承認権限ロール
            '    CODENAME_get("APPROVAL", TxtApproValid.Text, LblApproValidName.Text, WW_Dummy)
            '    TxtApproValid.Focus()
            Case "TxtDelFlg"      '削除フラグ
                CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
                TxtDelFlg.Focus()

        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' ユーザマスタ更新(削除フラグのみ)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UpdateMasterDelflgOnly()
        Dim WW_MODIFYKBN As String = ""
        Dim WW_DATE As Date = Date.Now

        '初期化
        LNS0001INPtbl = New DataTable
        LNS0001INPtbl.Columns.Add("USERID")
        LNS0001INPtbl.Columns.Add("STYMD")
        LNS0001INPtbl.Columns.Add("ENDYMD")
        LNS0001INPtbl.Columns.Add("DELFLG")

        Dim row As DataRow
        row = LNS0001INPtbl.NewRow
        row("USERID") = TxtUserId.Text
        row("STYMD") = WF_StYMD.Value
        row("ENDYMD") = WF_EndYMD.Value
        row("DELFLG") = C_DELETE_FLG.DELETE
        LNS0001INPtbl.Rows.Add(row)

        ' DB更新処理
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()

            '履歴テーブルに変更前データを登録
            InsertHist(SQLcon, LNS0001WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '削除フラグ更新
            SetDelflg(SQLcon, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '履歴テーブルに変更後データを登録
            InsertHist(SQLcon, LNS0001WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

        End Using

        '○ 入力値反映
        For Each LNS0001INProw As DataRow In LNS0001INPtbl.Rows
            For Each CTM0002row As DataRow In LNS0001tbl.Rows
                If LNS0001INProw("USERID") = CTM0002row("USERID") AndAlso
                    LNS0001INProw("STYMD") = CTM0002row("STYMD") AndAlso
                    LNS0001INProw("ENDYMD") = CTM0002row("ENDYMD") Then
                    ' 画面入力テーブル項目設定              
                    CTM0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    CTM0002row("DELFLG") = LNS0001INProw("DELFLG")
                    CTM0002row("SELECT") = 0
                    CTM0002row("HIDDEN") = 0
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

        '○ 対象データ取得
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        SQLStr.Append("     COM.LNS0001_USER                        ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         USERID = @USERID                    ")
        SQLStr.Append("     AND STYMD   = @STYMD                    ")
        SQLStr.Append("     AND ENDYMD   = @ENDYMD                  ")


        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_USERID As MySqlParameter = SQLcmd.Parameters.Add("@USERID", MySqlDbType.VarChar, 20)     'ユーザーID
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '開始年月日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '終了年月日
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                Dim LNS0001row As DataRow = LNS0001INPtbl.Rows(0)
                P_USERID.Value = LNS0001row("USERID")           'ユーザーID
                P_STYMD.Value = LNS0001row("STYMD")           '開始年月日
                P_ENDYMD.Value = LNS0001row("ENDYMD")           '終了年月日
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
            CS0011LOGWrite.INFPOSI = "DB:CTM0002C UPDATE"
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
        Dim WW_Date As Date

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SELectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SELectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SELectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        If String.IsNullOrEmpty(WF_FIELD_REP.Value) Then
            Select Case WF_FIELD.Value
                Case "TxtCampCode"      '会社コード
                    TxtCampCode.Text = WW_SelectValue
                    LblCampCodeName.Text = WW_SelectText
                    createListBox()
                    TxtCampCode.Focus()
                Case "TxtDelFlg"      '削除フラグ
                    TxtDelFlg.Text = WW_SelectValue
                    LblDelFlgName.Text = WW_SelectText
                    TxtDelFlg.Focus()
                Case "TxtPassEndYMD"  'パスワード有効期限
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtPassEndYMD.Text = ""
                        Else
                            TxtPassEndYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy-MM-dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtPassEndYMD.Focus()
                Case "WF_StYMD"       '有効年月日(From)
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            WF_StYMD.Value = ""
                        Else
                            WF_StYMD.Value = CDate(leftview.WF_Calendar.Text).ToString("yyyy-MM-dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_StYMD.Focus()
                Case "WF_EndYMD"      '有効年月日(To)
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            WF_EndYMD.Value = ""
                        Else
                            WF_EndYMD.Value = CDate(leftview.WF_Calendar.Text).ToString("yyyy-MM-dd")
                        End If
                    Catch ex As Exception
                    End Try
                    WF_EndYMD.Focus()
                    'Case "TxtOrg"         '組織コード
                    '    ddlSelectORG.SelectedValue = WW_SelectValue
                    '    LblOrgName.Text = WW_SelectText
                    '    TxtOrg.Focus()
                    'Case "TxtMenuRole"    'メニュー表示制御ロール
                    '    TxtMenuRole.Text = WW_SelectValue
                    '    LblMenuRoleName.Text = WW_SelectText
                    '    TxtMenuRole.Focus()
                    'Case "TxtMapRole"     '画面参照更新制御ロール
                    '    TxtMapRole.Text = WW_SelectValue
                    '    LblMapRoleName.Text = WW_SelectText
                    '    TxtMapRole.Focus()
                    'Case "TxtViewProfId"  '画面表示項目制御ロール
                    '    TxtViewProfId.Text = WW_SelectValue
                    '    LblViewProfIdName.Text = WW_SelectText
                    '    TxtViewProfId.Focus()
                    'Case "TxtRprtProfId"  'エクセル出力制御ロール
                    '    TxtRprtProfId.Text = WW_SelectValue
                    '    LblRprtProfIdName.Text = WW_SelectText
                    '    TxtRprtProfId.Focus()
                    'Case "TxtApproValid"  '承認権限ロール
                    '    TxtApproValid.Text = WW_SelectValue
                    '    LblApproValidName.Text = WW_SelectText
                    '    TxtApproValid.Focus()
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
                Case "TxtCampCode"          '会社コード
                    TxtCampCode.Focus()
                Case "TxtPassEndYMD"        'パスワード有効期限
                    TxtPassEndYMD.Focus()
                Case "WF_StYMD"             '有効年月日(From)
                    WF_StYMD.Focus()
                Case "WF_EndYMD"            '有効年月日(To)
                    WF_EndYMD.Focus()
                    'Case "TxtOrg"               '組織コード
                    '    TxtOrg.Focus()
                    'Case "TxtMenuRole"          'メニュー表示制御ロール
                    '    TxtMenuRole.Focus()
                    'Case "TxtMapRole"           '画面参照更新制御ロール
                    '    TxtMapRole.Focus()
                    'Case "TxtViewProfId"        '画面表示項目制御ロール
                    '    TxtViewProfId.Focus()
                    'Case "TxtRprtProfId"        'エクセル出力制御ロール
                    '    TxtRprtProfId.Focus()
                    'Case "TxtApproValid"        '承認権限ロール
                    '    TxtApproValid.Focus()
            End Select
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

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
        ' 権限チェック(操作者がデータ内USERの更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            WW_CheckMES1 = "・ユーザーマスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNS0001INProw As DataRow In LNS0001INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNS0001INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNS0001INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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
            ' ユーザーID(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "USERID", LNS0001INProw("USERID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・ユーザーID入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 社員名（短）(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "STAFFNAMES", LNS0001INProw("STAFFNAMES"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・社員名（短）入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 社員名（長）(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "STAFFNAMEL", LNS0001INProw("STAFFNAMEL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・社員名（長）入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 誤り回数(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "MISSCNT", LNS0001INProw("MISSCNT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・誤り回数入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '### 20240129 START パスワードポリシー対応 
            '' パスワード(バリデーションチェック）
            'Master.CheckField(Master.USERCAMP, "PASSWORD", LNS0001INProw("PASSWORD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・パスワード入力エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If

            ' パスワード(バリデーションチェック）
            'If Not ChkUserPassword(LNS0001INProw("PASSWORD"), WW_CheckMES2) Then
            'パスワード変更ありの場合のみチェックする
            If LNS0001INProw("PASSWORD") <> work.WF_SEL_PASSWORD.Text Then
                If Not LNS0001INProw("DELFLG").ToString = C_DELETE_FLG.DELETE AndAlso
                Not LNS0001WRKINC.ChkUserPassword(LNS0001INProw("PASSWORD"), WW_CheckMES2) Then
                    WW_CheckMES1 = "・パスワード入力エラーです。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If


            If LNS0001INProw("PASSWORD") <> work.WF_SEL_PASSWORD.Text Then
                ' パスワード有効期限
                NowDate = NowDate.AddDays(ADDDATE)
                LNS0001INProw("PASSENDYMD") = CDate(NowDate).ToShortDateString
            End If
            ' 開始年月日(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "STYMD", LNS0001INProw("STYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                LNS0001INProw("STYMD") = CDate(LNS0001INProw("STYMD")).ToString("yyyy/MM/dd")
            Else
                WW_CheckMES1 = "・開始年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 終了年月日(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ENDYMD", LNS0001INProw("ENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Date.Now > LNS0001INProw("ENDYMD") And LNS0001INProw("ENDYMD") <> work.WF_SEL_ENDYMD.Text Then
                    WW_CheckMES1 = "・終了年月日エラーです。"
                    WW_CheckMES2 = "過去日入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                Else
                    LNS0001INProw("ENDYMD") = CDate(LNS0001INProw("ENDYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・終了年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 会社コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "CAMPCODE", LNS0001INProw("CAMPCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("CAMPCODE", LNS0001INProw("CAMPCODE"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・会社コード入力エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・会社コード入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 組織コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ORG", LNS0001INProw("ORG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                '' 名称存在チェック
                'CODENAME_get("ORG", LNS0001INProw("ORG"), WW_Dummy, WW_RtnSW)
                'If Not isNormal(WW_RtnSW) Then
                '    WW_CheckMES1 = "・組織コード入力エラーです。"
                '    WW_CheckMES2 = "マスタに存在しません。"
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                '    WW_LineErr = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
            Else
                WW_CheckMES1 = "・組織コード入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' メールアドレス(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "EMAIL", LNS0001INProw("EMAIL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・メールアドレス入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            '' メニュー表示制御ロール(バリデーションチェック）
            'Master.CheckField(Master.USERCAMP, "MENUROLE", LNS0001INProw("MENUROLE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If isNormal(WW_CS0024FCheckerr) Then
            '    ' 名称存在チェック
            '    CODENAME_get("MENU", LNS0001INProw("MENUROLE"), WW_Dummy, WW_RtnSW)
            '    If Not isNormal(WW_RtnSW) Then
            '        WW_CheckMES1 = "・メニュー表示制御ロール入力エラーです。"
            '        WW_CheckMES2 = "マスタに存在しません。"
            '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '        WW_LineErr = "ERR"
            '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '    End If
            'Else
            '    WW_CheckMES1 = "・メニュー表示制御ロール入力エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            '' 画面参照更新制御ロール(バリデーションチェック）
            'Master.CheckField(Master.USERCAMP, "MAPROLE", LNS0001INProw("MAPROLE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If isNormal(WW_CS0024FCheckerr) Then
            '    ' 名称存在チェック
            '    CODENAME_get("MAP", LNS0001INProw("MAPROLE"), WW_Dummy, WW_RtnSW)
            '    If Not isNormal(WW_RtnSW) Then
            '        WW_CheckMES1 = "・画面参照更新制御ロール入力エラーです。"
            '        WW_CheckMES2 = "マスタに存在しません。"
            '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '        WW_LineErr = "ERR"
            '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '    End If
            'Else
            '    WW_CheckMES1 = "・画面参照更新制御ロール入力エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            ''画面表示項目制御ロール(バリデーションチェック）
            'Master.CheckField(Master.USERCAMP, "VIEWPROFID", LNS0001INProw("VIEWPROFID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If isNormal(WW_CS0024FCheckerr) Then
            '    '名称存在チェック
            '    CODENAME_get("VIEW", LNS0001INProw("VIEWPROFID"), WW_Dummy, WW_RtnSW)
            '    If Not isNormal(WW_RtnSW) Then
            '        WW_CheckMES1 = "・画面表示項目制御ロール入力エラーです。"
            '        WW_CheckMES2 = "マスタに存在しません。"
            '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '        WW_LineErr = "ERR"
            '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '    End If
            'Else
            '    WW_CheckMES1 = "・画面表示項目制御ロール入力エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            ''エクセル出力制御ロール(バリデーションチェック）
            'Master.CheckField(Master.USERCAMP, "RPRTPROFID", LNS0001INProw("RPRTPROFID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If isNormal(WW_CS0024FCheckerr) Then
            '    '名称存在チェック
            '    CODENAME_get("XML", LNS0001INProw("RPRTPROFID"), WW_Dummy, WW_RtnSW)
            '    If Not isNormal(WW_RtnSW) Then
            '        WW_CheckMES1 = "・エクセル出力制御ロール入力エラーです。"
            '        WW_CheckMES2 = "マスタに存在しません。"
            '        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '        WW_LineErr = "ERR"
            '        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '    End If
            'Else
            '    WW_CheckMES1 = "・エクセル出力制御ロール入力エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            ''画面初期値ロール(バリデーションチェック）
            'Master.CheckField(Master.USERCAMP, "VARIANT", LNS0001INProw("VARIANT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・画面初期値ロール入力エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            ''承認権限ロール(バリデーションチェック）
            'Master.CheckField(Master.USERCAMP, "APPROVALID", LNS0001INProw("APPROVALID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If isNormal(WW_CS0024FCheckerr) Then
            '    If Not String.IsNullOrEmpty(LNS0001INProw("APPROVALID")) Then
            '        '名称存在チェック
            '        CODENAME_get("APPROVAL", LNS0001INProw("APPROVALID"), WW_Dummy, WW_RtnSW)
            '        If Not isNormal(WW_RtnSW) Then
            '            WW_CheckMES1 = "・承認権限ロール入力エラーです。"
            '            WW_CheckMES2 = "マスタに存在しません。"
            '            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '            WW_LineErr = "ERR"
            '            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '        End If
            '    End If
            'Else
            '    WW_CheckMES1 = "・承認権限ロール入力エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            ' 日付大小チェック
            If Not String.IsNullOrEmpty(LNS0001INProw("STYMD")) AndAlso
                Not String.IsNullOrEmpty(LNS0001INProw("ENDYMD")) Then
                If CDate(LNS0001INProw("STYMD")) > CDate(LNS0001INProw("ENDYMD")) Then
                    WW_CheckMES1 = "・開始年月日＆終了年月日エラーです。"
                    WW_CheckMES2 = "日付大小入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_USERID.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_USERID.Text,
                                                            work.WF_SEL_STYMD2.Text,
                                                            work.WF_SEL_TIMESTAMP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（ユーザーID & 開始年月日）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNS0001INProw("USERID") & "]" &
                                           " [" & LNS0001INProw("STYMD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNS0001INProw("USERID") = work.WF_SEL_USERID.Text OrElse Not LNS0001INProw("STYMD") = work.WF_SEL_STYMD2.Text Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（ユーザーID & 開始年月日 & 終了年月日）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & LNS0001INProw("USERID") & "]" &
                                       "([" & LNS0001INProw("STYMD") & "]" &
                                       " [" & LNS0001INProw("ENDYMD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If String.IsNullOrEmpty(WW_LineErr) Then
                If LNS0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNS0001INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' 期間調整画面入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPOverlapPeriodsCheck(ByRef O_RTN As String)
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
        ' 権限チェック(操作者がデータ内USERの更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            WW_CheckMES1 = "・ユーザーマスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNS0001INProw As DataRow In LNS0001INPtbl.Rows

            WW_LineErr = ""

            If LNS0001INProw("PASSWORD") <> work.WF_SEL_PASSWORD.Text Then
                ' パスワード有効期限
                NowDate = NowDate.AddDays(ADDDATE)
                LNS0001INProw("PASSENDYMD") = CDate(NowDate).ToShortDateString
            End If

            ' 登録済前回期間-終了年月日(バリデーションチェック）
            If Not String.IsNullOrEmpty(pnlTxtLastEndYMD.Text) Then
                Master.CheckField(Master.USERCAMP, "ENDYMD", pnlTxtLastEndYMD.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・登録済前回期間-終了年月日エラー"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_OVERLAPPERIODS_NOTDATE_ERR
                    ' 単項目チェックエラーをセット
                    LNS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    Exit Sub
                End If
            End If
            ' 今回入力期間-開始年月日(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "STYMD", pnlTxtInputStYMD.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・今回入力期間-開始年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.CTN_OVERLAPPERIODS_NOTDATE_ERR
                ' 単項目チェックエラーをセット
                LNS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                Exit Sub
            End If
            ' 今回入力期間-終了年月日(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ENDYMD", pnlTxtInputEndYMD.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Date.Now > pnlTxtInputEndYMD.Text And pnlTxtInputEndYMD.Text <> work.WF_SEL_ENDYMD.Text Then
                    WW_CheckMES1 = "・今回入力期間-終了年月日エラー"
                    WW_CheckMES2 = "過去日入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_OVERLAPPERIODS_PASTDATE_ERR
                    ' 単項目チェックエラーをセット
                    LNS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    Exit Sub
                End If
            Else
                WW_CheckMES1 = "・今回入力期間-終了年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.CTN_OVERLAPPERIODS_NOTDATE_ERR
                ' 単項目チェックエラーをセット
                LNS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                Exit Sub
            End If
            ' 登録済次回期間-開始年月日(バリデーションチェック）
            If Not String.IsNullOrEmpty(pnlTxtNextStYMD.Text) Then
                Master.CheckField(Master.USERCAMP, "STYMD", pnlTxtNextStYMD.Text, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・登録済次回期間-開始年月日エラー"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_OVERLAPPERIODS_NOTDATE_ERR
                    ' 単項目チェックエラーをセット
                    LNS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    Exit Sub
                End If
            End If
            ' 登録済前回期間-開始年月日・登録済前回期間-終了年月日 日付大小チェック
            If Not String.IsNullOrEmpty(pnlTxtLastStYMD.Text) AndAlso
                Not String.IsNullOrEmpty(pnlTxtLastEndYMD.Text) Then
                If CDate(pnlTxtLastStYMD.Text) > CDate(pnlTxtLastEndYMD.Text) Then
                    WW_CheckMES1 = "・登録済前回期間-開始年月日と登録済前回期間-終了年月日の期間重複エラーです。"
                    WW_CheckMES2 = "登録済前回期間-終了年月日は登録済前回期間-開始年月日より未来の日付を入力してください。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_OVERLAPPERIODS_ERR
                End If
            End If
            ' 登録済前回期間-終了年月日・今回入力期間-開始年月日 日付大小チェック
            If Not String.IsNullOrEmpty(pnlTxtLastEndYMD.Text) AndAlso
                Not String.IsNullOrEmpty(pnlTxtInputStYMD.Text) Then
                If CDate(pnlTxtLastEndYMD.Text) >= CDate(pnlTxtInputStYMD.Text) Then
                    WW_CheckMES1 = "・登録済前回期間-終了年月日と今回入力期間-開始年月日の期間重複エラーです。"
                    WW_CheckMES2 = "今回入力期間-開始年月日は登録済前回期間-終了年月日より未来の日付を入力してください。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_OVERLAPPERIODS_ERR
                End If
            End If
            ' 今回入力期間-開始年月日・今回入力期間-終了年月日 日付大小チェック
            If Not String.IsNullOrEmpty(pnlTxtInputStYMD.Text) AndAlso
                Not String.IsNullOrEmpty(pnlTxtInputEndYMD.Text) Then
                If CDate(pnlTxtInputStYMD.Text) > CDate(pnlTxtInputEndYMD.Text) Then
                    WW_CheckMES1 = "・今回入力期間-開始年月日と今回入力期間-終了年月日の期間重複エラーです。"
                    WW_CheckMES2 = "今回入力期間-終了年月日は今回入力期間-開始年月日より未来の日付を入力してください。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_OVERLAPPERIODS_ERR
                End If
            End If
            ' 今回入力期間-終了年月日・登録済次回期間-開始年月日 日付大小チェック
            If Not String.IsNullOrEmpty(pnlTxtInputEndYMD.Text) AndAlso
                Not String.IsNullOrEmpty(pnlTxtNextStYMD.Text) Then
                If CDate(pnlTxtInputEndYMD.Text) >= CDate(pnlTxtNextStYMD.Text) Then
                    WW_CheckMES1 = "・今回入力期間-終了年月日と登録済次回期間-開始年月日の期間重複エラーです。"
                    WW_CheckMES2 = "登録済次回期間-開始年月日は今回入力期間-終了年月日より未来の日付を入力してください。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_OVERLAPPERIODS_ERR
                End If
            End If
            ' 登録済次回期間-開始年月日・登録済次回期間-終了年月日 日付大小チェック
            If Not String.IsNullOrEmpty(pnlTxtNextStYMD.Text) AndAlso
                Not String.IsNullOrEmpty(pnlTxtNextEndYMD.Text) Then
                If CDate(pnlTxtNextStYMD.Text) > CDate(pnlTxtNextEndYMD.Text) Then
                    WW_CheckMES1 = "・登録済次回期間-開始年月日と登録済次回期間-終了年月日の期間重複エラーです。"
                    WW_CheckMES2 = "登録済次回期間-開始年月日は登録済次回期間-終了年月日より過去の日付を入力してください。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_OVERLAPPERIODS_ERR
                End If
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_USERID.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_USERID.Text,
                                                            pnlTxtInputStYMD.Text,
                                                            work.WF_SEL_TIMESTAMP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    ' 排他エラー
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not String.IsNullOrEmpty(pnlTxtNextStYMD.Text) Then
                If pnlTxtNextStYMD.Text <> pnlTxtAdjustNextStYMD.Text Then
                    Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                        ' DataBase接続
                        SQLcon.Open()
                        ' 一意制約チェック
                        UniqueKeyCheck(SQLcon, WW_DBDataCheck, pnlTxtNextStYMD.Text)
                    End Using

                    If Not isNormal(WW_DBDataCheck) Then
                        ' 一意制約エラー
                        O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    End If
                End If
            End If

            If String.IsNullOrEmpty(WW_LineErr) Then
                If LNS0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNS0001INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNS0001INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNS0001tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNS0001tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNS0001row As DataRow In LNS0001tbl.Rows
            Select Case LNS0001row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNS0001row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNS0001row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNS0001row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNS0001INProw As DataRow In LNS0001INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNS0001INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNS0001INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNS0001row As DataRow In LNS0001tbl.Rows
                ' KEY項目が等しい時
                If LNS0001row("USERID") = LNS0001INProw("USERID") AndAlso
                    LNS0001row("STYMD") = LNS0001INProw("STYMD") Then
                    ' KEY項目以外の項目の差異をチェック
                    If LNS0001row("DELFLG") = LNS0001INProw("DELFLG") AndAlso
                        LNS0001row("STAFFNAMES") = LNS0001INProw("STAFFNAMES") AndAlso
                        LNS0001row("STAFFNAMEL") = LNS0001INProw("STAFFNAMEL") AndAlso
                        LNS0001row("MAPID") = LNS0001INProw("MAPID") AndAlso
                        LNS0001row("PASSWORD") = LNS0001INProw("PASSWORD") AndAlso
                        LNS0001row("MISSCNT") = LNS0001INProw("MISSCNT") AndAlso
                        LNS0001row("PASSENDYMD") = LNS0001INProw("PASSENDYMD") AndAlso
                        LNS0001row("ENDYMD") = LNS0001INProw("ENDYMD") AndAlso
                        LNS0001row("CAMPCODE") = LNS0001INProw("CAMPCODE") AndAlso
                        LNS0001row("ORG") = LNS0001INProw("ORG") AndAlso
                        LNS0001row("EMAIL") = LNS0001INProw("EMAIL") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNS0001row("OPERATION")) Then

                        ' 変更がない時は「操作」の項目は空白にする
                        LNS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNS0001INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNS0001INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNS0001INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNS0001INPtbl.Rows(0)("OPERATION")) Then
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
                If WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNS0001WRKINC.MODIFYKBN.AFTDATA
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
        For Each LNS0001INProw As DataRow In LNS0001INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNS0001row As DataRow In LNS0001tbl.Rows
                ' 同一レコードか判定
                If LNS0001INProw("USERID") = LNS0001row("USERID") AndAlso
                    LNS0001INProw("STYMD") = LNS0001row("STYMD") Then
                    ' 画面入力テーブル項目設定
                    LNS0001INProw("LINECNT") = LNS0001row("LINECNT")
                    LNS0001INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNS0001INProw("UPDTIMSTP") = LNS0001row("UPDTIMSTP")
                    LNS0001INProw("SELECT") = 0
                    LNS0001INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNS0001row.ItemArray = LNS0001INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNS0001tbl.NewRow
                WW_NRow.ItemArray = LNS0001INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNS0001tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNS0001tbl.Rows.Add(WW_NRow)
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
                    If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ALL, TxtCampCode.Text))
                    Else
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ROLE, TxtCampCode.Text))
                    End If
                Case "ORG"              '組織コード
                    If Master.USER_ORG = CONST_OFFICECODE_SYSTEM Then
                        ' 情報システムの場合、操作ユーザーが所属する会社の組織を全て取得
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY_WITH_CMPORG, TxtCampCode.Text))
                    Else
                        ' その他の場合、操作ユーザーの組織のみ取得
                        leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY, TxtCampCode.Text))
                    End If
                Case "MENU"             'メニュー表示制御ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, work.CreateRoleList(TxtCampCode.Text, I_FIELD))
                Case "MAP"              '画面参照更新制御ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, work.CreateRoleList(TxtCampCode.Text, I_FIELD))
                Case "VIEW"             '画面表示項目制御ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, work.CreateRoleList(TxtCampCode.Text, I_FIELD))
                Case "XML"              'エクセル出力制御ロール
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, work.CreateRoleList(TxtCampCode.Text, I_FIELD))
                'Case "APPROVAL"         '承認権限ロール
                '    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ROLE, I_VALUE, O_TEXT, O_RTN, work.CreateRoleList(TxtCampCode.Text, I_FIELD))

                Case "OUTPUTID"         '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))
                Case "ONOFF"            '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DELFLG"))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
