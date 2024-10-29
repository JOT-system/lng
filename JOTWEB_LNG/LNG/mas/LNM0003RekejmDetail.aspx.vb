''************************************************************
' コンテナ決済マスタメンテ登録画面
' 作成日 2022/02/04
' 更新日 2023/12/28
' 作成者 名取
' 更新者 大浜
'
' 修正履歴 : 2022/02/04 新規作成
'          : 2023/12/28 変更履歴登録機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' コンテナ決済マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0003RekejmDetail
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private LNM0003tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0003INPtbl As DataTable                              'チェック用テーブル
    Private LNM0003UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0003tbl, work.WF_SEL_INPTBL.Text)

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
                        Case "btnUpdateConfirmOK"       '更新ボタン押下後の確認ダイアログでOK押下
                            WF_UPDATE_ConfirmOkClick()
                        Case "mspStationSingleRowSelected"  '[共通]駅選択ポップアップで行選択
                            RowSelected_mspStationSingle()
                        Case "mspBankCodeSingleRowSelected"  '[共通]銀行コード選択ポップアップで行選択
                            RowSelected_mspBankCodeSingle()
                        Case "mspBankBranchCodeSingleRowSelected"  '[共通]支店コード選択ポップアップで行選択
                            RowSelected_mspBankBranchCodeSingle()
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
            If Not IsNothing(LNM0003tbl) Then
                LNM0003tbl.Clear()
                LNM0003tbl.Dispose()
                LNM0003tbl = Nothing
            End If

            If Not IsNothing(LNM0003INPtbl) Then
                LNM0003INPtbl.Clear()
                LNM0003INPtbl.Dispose()
                LNM0003INPtbl = Nothing
            End If

            If Not IsNothing(LNM0003UPDtbl) Then
                LNM0003UPDtbl.Clear()
                LNM0003UPDtbl.Dispose()
                LNM0003UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0003WRKINC.MAPIDD
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0003L Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        LblSelLineCNT.Text = work.WF_SEL_LINECNT.Text
        '削除
        TxtDelFlg.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
        '画面ＩＤ
        TxtMapId.Text = "M00001"
        '発駅コード
        TxtDepStation.Text = work.WF_SEL_DEPSTATION2.Text
        CODENAME_get("DEPSTATION", TxtDepStation.Text, LblDepStationName.Text, WW_Dummy)
        '発受託人コード
        TxtDepTrusteeCd.Text = work.WF_SEL_DEPTRUSTEECD2.Text
        CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCd.Text, LblDepTrusteeCdName.Text, WW_Dummy)
        '発受託人名称
        TxtDepTrusteeNm.Text = work.WF_SEL_DEPTRUSTEENM.Text
        '発受託人名称（カナ）
        TxtDepTrusteeSubKana.Text = work.WF_SEL_DEPTRUSTEESUBKANA.Text
        '発受託人サブコード
        TxtDepTrusteeSubCd.Text = work.WF_SEL_DEPTRUSTEESUBCD2.Text
        CODENAME_get("DEPTRUSTEESUBCD", TxtDepTrusteeSubCd.Text, LblDepTrusteeSubCdName.Text, WW_Dummy)
        '発受託人サブ名称
        TxtDepTrusteeSubNm.Text = work.WF_SEL_DEPTRUSTEESUBNM.Text
        '取引先コード
        TxtToriCode.Text = work.WF_SEL_TORICODE.Text
        CODENAME_get("TORICD", TxtToriCode.Text, LblToriCdName.Text, WW_Dummy)
        '適格請求登録番号
        TxtInvNo.Text = work.WF_SEL_ELIGIBLEINVOICENUMBER.Text
        '請求項目計上店コード
        TxtInvKeijyoBranchCd.Text = work.WF_SEL_INVKEIJYOBRANCHCD.Text
        CODENAME_get("INVKEIJYOBRANCHCD", TxtInvKeijyoBranchCd.Text, LblInvKeijyoBranchCdName.Text, WW_Dummy)
        '請求項目請求サイクル
        TxtInvCycl.Text = work.WF_SEL_INVCYCL.Text
        CODENAME_get("INVCYCL", TxtInvCycl.Text, LblInvCyclName.Text, WW_Dummy)
        '請求項目請求書提出部店
        TxtInvFilingDept.Text = work.WF_SEL_INVFILINGDEPT.Text
        CODENAME_get("INVFILINGDEPT", TxtInvFilingDept.Text, LblInvFilingDeptName.Text, WW_Dummy)
        '請求項目請求書決済区分
        TxtInvKesaiKbn.Text = work.WF_SEL_INVKESAIKBN.Text
        '請求項目請求書細分コード
        TxtInvSubCd.Text = work.WF_SEL_INVSUBCD.Text
        '支払項目費用計上店コード
        TxtPayKeijyoBranchCd.Text = work.WF_SEL_PAYKEIJYOBRANCHCD.Text
        CODENAME_get("PAYKEIJYOBRANCHCD", TxtPayKeijyoBranchCd.Text, LblPayKeijyoBranchCDName.Text, WW_Dummy)
        '支払項目支払書提出支店
        TxtPayFilingBranch.Text = work.WF_SEL_PAYFILINGBRANCH.Text
        CODENAME_get("PAYFILINGBRANCH", TxtPayFilingBranch.Text, LblPayFilingBranchName.Text, WW_Dummy)
        '支払項目消費税計算単位
        TxtTaxCalcUnit.Text = work.WF_SEL_TAXCALCUNIT.Text
        CODENAME_get("TAXCALCUNIT", TxtTaxCalcUnit.Text, LblTaxCalcUnitName.Text, WW_Dummy)
        '支払項目決済区分
        TxtPayKesaiKbn.Text = work.WF_SEL_PAYKESAIKBN.Text
        '支払項目銀行コード
        If work.WF_SEL_PAYBANKCD.Text = "" Then
            TxtPayBankCd.Text = ""
        Else
            TxtPayBankCd.Text = Strings.Right("0000" + work.WF_SEL_PAYBANKCD.Text, 4)
            CODENAME_get("BANKCODE", TxtPayBankCd.Text, LblPayBankCd.Text, WW_Dummy)
        End If
        '支払項目銀行支店コ-ド
        If work.WF_SEL_PAYBANKBRANCHCD.Text = "" Then
            TxtPayBankBranchCd.Text = ""
        Else
            TxtPayBankBranchCd.Text = Strings.Right("000" + work.WF_SEL_PAYBANKBRANCHCD.Text, 3)
            CODENAME_get("BANKBRANCHCODE", TxtPayBankBranchCd.Text, LblPayBankBranchCd.Text, WW_Dummy)
        End If
        '支払項目口座種別
        ddlPayAccountTypeName.SelectedValue = work.WF_SEL_PAYACCOUNTTYPE.Text
        '支払項目口座番号
        TxtPayAccountNo.Text = work.WF_SEL_PAYACCOUNTNO.Text
        '支払項目口座名義人
        TxtPayAccountNm.Text = work.WF_SEL_PAYACCOUNTNM.Text
        '支払項目支払摘要
        TxtPayTekiyo.Text = work.WF_SEL_PAYTEKIYO.Text
        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_DEPSTATION2.Text
        ' 請求摘要取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()
            ' 排他チェック
            SelectClaimTekiyo(SQLcon)
        End Using

        ' 数値(0～9)のみ入力可能とする。
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"                   '削除フラグ
        Me.TxtDepStation.Attributes("onkeyPress") = "CheckNum()"               '発駅コード
        Me.TxtDepTrusteeCd.Attributes("onkeyPress") = "CheckNum()"             '発受託人コード
        Me.TxtDepTrusteeSubCd.Attributes("onkeyPress") = "CheckNum()"          '発受託人サブコード
        Me.TxtToriCode.Attributes("onkeyPress") = "CheckNum()"            　　 '取引先コード
        Me.TxtInvKeijyoBranchCd.Attributes("onkeyPress") = "CheckNum()"        '請求項目計上店コード
        Me.TxtInvCycl.Attributes("onkeyPress") = "CheckNum()"                  '請求項目請求サイクル
        Me.TxtInvFilingDept.Attributes("onkeyPress") = "CheckNum()"            '請求項目請求書提出部店
        Me.TxtInvKesaiKbn.Attributes("onkeyPress") = "CheckNum()"              '請求項目請求書決済区分
        Me.TxtInvSubCd.Attributes("onkeyPress") = "CheckNum()"                 '請求項目請求書細分コード
        Me.TxtPayKeijyoBranchCd.Attributes("onkeyPress") = "CheckNum()"        '支払項目費用計上店コード
        Me.TxtPayFilingBranch.Attributes("onkeyPress") = "CheckNum()"          '支払項目支払書提出支店
        Me.TxtTaxCalcUnit.Attributes("onkeyPress") = "CheckNum()"              '支払項目消費税計算単位
        Me.TxtPayKesaiKbn.Attributes("onkeyPress") = "CheckNum()"              '支払項目決済区分
        Me.TxtPayBankCd.Attributes("onkeyPress") = "CheckNum()"                '支払項目銀行コード
        Me.TxtPayBankBranchCd.Attributes("onkeyPress") = "CheckNum()"          '支払項目銀行支店コード
        Me.ddlPayAccountTypeName.Attributes("onkeyPress") = "CheckNum()"       '支払項目口座種別
        Me.TxtPayAccountNo.Attributes("onkeyPress") = "CheckNum()"             '支払項目口座番号

        '半角のみ入力可能
        Me.TxtInvNo.Attributes("onkeyPress") = "CheckNumAZ()"                  '適格請求書登録番号

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO"></param>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                         " _
            & "     DEPSTATION                 " _
            & "   , DEPTRUSTEECD               " _
            & "   , DEPTRUSTEESUBCD            " _
            & " FROM                           " _
            & "     LNG.LNM0003_REKEJM         " _
            & " WHERE                          " _
            & "         DEPSTATION       = @P1 " _
            & "     AND DEPTRUSTEECD     = @P2 " _
            & "     AND DEPTRUSTEESUBCD  = @P3 " _
            & "     AND DELFLG          <> @P4 "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 6) '発駅コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 5) '発受託人コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 3) '発受託人サブコード
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 1) '削除フラグ

                PARA1.Value = TxtDepStation.Text
                PARA2.Value = TxtDepTrusteeCd.Text
                PARA4.Value = TxtDepTrusteeSubCd.Text
                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0003Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0003Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0003Chk.Load(SQLdr)

                    If LNM0003Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0003C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0003C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 請求摘要取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    Protected Sub SelectClaimTekiyo(ByVal SQLcon As MySqlConnection)

        If String.IsNullOrEmpty(TxtInvSubCd.Text) OrElse TxtInvSubCd.Text = "0" Then
            '○ 対象データ取得
            Dim SQLStr As String =
                  " SELECT                         " _
                & "     SLIPDESCRIPTION1           " _
                & " FROM                           " _
                & "     LNG.LNM0024_KEKKJM         " _
                & " WHERE                          " _
                & "         TORICODE         = @P1 " _
                & "     AND INVFILINGDEPT    = @P2 " _
                & "     AND INVKESAIKBN      = @P3 " _
                & "     AND DELFLG          <> @P4 "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 10) '取引先コード
                    Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 6)  '請求書提出部店
                    Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 2)  '請求書決済区分
                    Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 1)  '削除フラグ

                    PARA1.Value = TxtToriCode.Text
                    PARA2.Value = TxtInvFilingDept.Text
                    PARA3.Value = TxtInvKesaiKbn.Text
                    PARA4.Value = C_DELETE_FLG.DELETE

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                        Dim LNM0003Chk = New DataTable

                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0003Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        LNM0003Chk.Load(SQLdr)

                        If LNM0003Chk.Rows.Count > 0 Then
                            Dim LNM0003row As DataRow
                            LNM0003row = LNM0003Chk.Rows(0)
                            TxtInvTekiyo.Text = LNM0003row("SLIPDESCRIPTION1")
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0003C SELECT_CLAIMTEKIYO")

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNM0003C SELECT_CLAIMTEKIYO"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Exit Sub
            End Try
        Else
            '○ 対象データ取得
            Dim SQLStr As String =
                  " SELECT                         " _
                & "     SUBDIVISION                " _
                & " FROM                           " _
                & "     LNG.LNM0025_KEKSBM         " _
                & " WHERE                          " _
                & "         TORICODE         = @P1 " _
                & "     AND INVFILINGDEPT    = @P2 " _
                & "     AND INVKESAIKBN      = @P3 " _
                & "     AND INVSUBCD         = @P4 " _
                & "     AND DELFLG          <> @P5 "

            Try
                Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 10) '取引先コード
                    Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 6) '請求書提出部店
                    Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 2) '請求書決済区分
                    Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 2) '請求細分コード
                    Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.VarChar, 1) '削除フラグ

                    PARA1.Value = TxtToriCode.Text
                    PARA2.Value = TxtInvFilingDept.Text
                    PARA3.Value = TxtInvKesaiKbn.Text
                    PARA4.Value = TxtInvSubCd.Text
                    PARA5.Value = C_DELETE_FLG.DELETE

                    Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                        Dim LNM0003Chk = New DataTable

                        '○ フィールド名とフィールドの型を取得
                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0003Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next

                        '○ テーブル検索結果をテーブル格納
                        LNM0003Chk.Load(SQLdr)

                        If LNM0003Chk.Rows.Count > 0 Then
                            Dim LNM0003row As DataRow
                            LNM0003row = LNM0003Chk.Rows(0)
                            TxtInvTekiyo.Text = LNM0003row("SUBDIVISION")
                        End If
                    End Using
                End Using
            Catch ex As Exception
                Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0003C SELECT_CLAIMTEKIYO")

                CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
                CS0011LOGWrite.INFPOSI = "DB:LNM0003C SELECT_CLAIMTEKIYO"
                CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                CS0011LOGWrite.TEXT = ex.ToString()
                CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
                CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
                Exit Sub
            End Try
        End If


        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' コンテナ取引先マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(コンテナ決済マスタ)
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;                 " _
            & "     SET @hensuu = 0 ;                       " _
            & " DECLARE hensuu CURSOR FOR                   " _
            & "     SELECT                                  " _
            & "         UPDTIMSTP AS hensuu                 " _
            & "     FROM                                    " _
            & "         LNG.LNM0003_REKEJM                  " _
            & "     WHERE                                   " _
            & "         DEPSTATION      = @P01              " _
            & "     AND DEPTRUSTEECD    = @P02              " _
            & "     AND DEPTRUSTEESUBCD = @P03 ;            " _
            & " OPEN hensuu ;                               " _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;       " _
            & " IF (@@FETCH_STATUS = 0)                     " _
            & "     UPDATE LNG.LNM0003_REKEJM               " _
            & "     SET                                     " _
            & "         DELFLG                = @P00        " _
            & "       , DEPTRUSTEENM          = @P04        " _
            & "       , DEPTRUSTEESUBNM       = @P05        " _
            & "       , DEPTRUSTEESUBKANA     = @P06        " _
            & "       , TORICODE              = @P07        " _
            & "       , ELIGIBLEINVOICENUMBER = @P08        " _
            & "       , INVKEIJYOBRANCHCD     = @P09        " _
            & "       , INVCYCL               = @P10        " _
            & "       , INVFILINGDEPT         = @P11        " _
            & "       , INVKESAIKBN           = @P12        " _
            & "       , INVSUBCD              = @P13        " _
            & "       , PAYKEIJYOBRANCHCD     = @P14        " _
            & "       , PAYFILINGBRANCH       = @P15        " _
            & "       , TAXCALCUNIT           = @P16        " _
            & "       , PAYKESAIKBN           = @P17        " _
            & "       , PAYBANKCD             = @P18        " _
            & "       , PAYBANKBRANCHCD       = @P19        " _
            & "       , PAYACCOUNTTYPE        = @P20        " _
            & "       , PAYACCOUNTNO          = @P21        " _
            & "       , PAYACCOUNTNM          = @P22        " _
            & "       , PAYTEKIYO             = @P23        " _
            & "       , UPDYMD                = @P29        " _
            & "       , UPDUSER               = @P30        " _
            & "       , UPDTERMID             = @P31        " _
            & "       , UPDPGID               = @P32        " _
            & "     WHERE                                   " _
            & "         DEPSTATION      = @P01              " _
            & "     AND DEPTRUSTEECD    = @P02              " _
            & "     AND DEPTRUSTEESUBCD = @P03 ;            " _
            & " IF (@@FETCH_STATUS <> 0)                    " _
            & "     INSERT INTO LNG.LNM0003_REKEJM          " _
            & "        (DELFLG                              " _
            & "       , DEPSTATION                          " _
            & "       , DEPTRUSTEECD                        " _
            & "       , DEPTRUSTEESUBCD                     " _
            & "       , DEPTRUSTEENM                        " _
            & "       , DEPTRUSTEESUBNM                     " _
            & "       , DEPTRUSTEESUBKANA                   " _
            & "       , TORICODE                            " _
            & "       , ELIGIBLEINVOICENUMBER               " _
            & "       , INVKEIJYOBRANCHCD                   " _
            & "       , INVCYCL                             " _
            & "       , INVFILINGDEPT                       " _
            & "       , INVKESAIKBN                         " _
            & "       , INVSUBCD                            " _
            & "       , PAYKEIJYOBRANCHCD                   " _
            & "       , PAYFILINGBRANCH                     " _
            & "       , TAXCALCUNIT                         " _
            & "       , PAYKESAIKBN                         " _
            & "       , PAYBANKCD                           " _
            & "       , PAYBANKBRANCHCD                     " _
            & "       , PAYACCOUNTTYPE                      " _
            & "       , PAYACCOUNTNO                        " _
            & "       , PAYACCOUNTNM                        " _
            & "       , PAYTEKIYO                           " _
            & "       , INITYMD                             " _
            & "       , INITUSER                            " _
            & "       , INITTERMID                          " _
            & "       , INITPGID)                           " _
            & "     VALUES                                  " _
            & "        (@P00                                " _
            & "       , @P01                                " _
            & "       , @P02                                " _
            & "       , @P03                                " _
            & "       , @P04                                " _
            & "       , @P05                                " _
            & "       , @P06                                " _
            & "       , @P07                                " _
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
            & "       , @P18                                " _
            & "       , @P19                                " _
            & "       , @P20                                " _
            & "       , @P21                                " _
            & "       , @P22                                " _
            & "       , @P23                                " _
            & "       , @P25                                " _
            & "       , @P26                                " _
            & "       , @P27                                " _
            & "       , @P28) ;                             " _
            & " CLOSE hensuu ;                              " _
            & " DEALLOCATE hensuu ;                         "

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                     " _
            & "    DELFLG                                  " _
            & "  , DEPSTATION                              " _
            & "  , DEPTRUSTEECD                            " _
            & "  , DEPTRUSTEESUBCD                         " _
            & "  , DEPTRUSTEENM                            " _
            & "  , DEPTRUSTEESUBNM                         " _
            & "  , DEPTRUSTEESUBKANA                       " _
            & "  , TORICODE                                " _
            & "  , ELIGIBLEINVOICENUMBER                   " _
            & "  , INVKEIJYOBRANCHCD                       " _
            & "  , INVCYCL                                 " _
            & "  , INVFILINGDEPT                           " _
            & "  , INVKESAIKBN                             " _
            & "  , INVSUBCD                                " _
            & "  , PAYKEIJYOBRANCHCD                       " _
            & "  , PAYFILINGBRANCH                         " _
            & "  , TAXCALCUNIT                             " _
            & "  , PAYKESAIKBN                             " _
            & "  , PAYBANKCD                               " _
            & "  , PAYBANKBRANCHCD                         " _
            & "  , PAYACCOUNTTYPE                          " _
            & "  , PAYACCOUNTNO                            " _
            & "  , PAYACCOUNTNM                            " _
            & "  , PAYTEKIYO                               " _
            & "  , INITYMD                                 " _
            & "  , INITUSER                                " _
            & "  , INITTERMID                              " _
            & "  , INITPGID                                " _
            & "  , UPDYMD                                  " _
            & "  , UPDUSER                                 " _
            & "  , UPDTERMID                               " _
            & "  , UPDPGID                                 " _
            & "  , RECEIVEYMD                              " _
            & "  , UPDTIMSTP                               " _
            & " FROM                                       " _
            & "    LNG.LNM0003_REKEJM                      " _
            & " WHERE                                      " _
            & "        DEPSTATION      = @P01              " _
            & "    AND DEPTRUSTEECD    = @P02              " _
            & "    AND DEPTRUSTEESUBCD = @P03              "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                ' DB更新用パラメータ
                Dim PARA00 As MySqlParameter = SQLcmd.Parameters.Add("@P00", MySqlDbType.VarChar, 1)     '削除フラグ
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 6)     '発駅コード
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 5)     '発受託人コード
                Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 3)     '発受託人サブコード
                Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar, 32)    '発受託人名称
                Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar, 18)    '発受託人サブ名称
                Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar, 20)    '発受託人名称（カナ）
                Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar, 10)    '取引先コード）
                Dim PARA08 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.VarChar, 20)    '適格請求書登録番号
                Dim PARA09 As MySqlParameter = SQLcmd.Parameters.Add("@P09", MySqlDbType.VarChar, 6)     '請求項目計上店コード
                Dim PARA10 As MySqlParameter = SQLcmd.Parameters.Add("@P10", MySqlDbType.VarChar, 2)     '請求項目請求サイクル
                Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.VarChar, 6)     '請求項目請求書提出部店
                Dim PARA12 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.VarChar, 2)     '請求項目請求書決済区分
                Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar, 2)     '請求項目請求書細分コード
                Dim PARA14 As MySqlParameter = SQLcmd.Parameters.Add("@P14", MySqlDbType.VarChar, 6)     '支払項目費用計上店コード
                Dim PARA15 As MySqlParameter = SQLcmd.Parameters.Add("@P15", MySqlDbType.VarChar, 6)     '支払項目支払書提出支店
                Dim PARA16 As MySqlParameter = SQLcmd.Parameters.Add("@P16", MySqlDbType.VarChar, 2)     '支払項目消費税計算単位
                Dim PARA17 As MySqlParameter = SQLcmd.Parameters.Add("@P17", MySqlDbType.VarChar, 1)     '支払項目決済区分
                Dim PARA18 As MySqlParameter = SQLcmd.Parameters.Add("@P18", MySqlDbType.VarChar, 4)     '支払項目銀行コード
                Dim PARA19 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.VarChar, 3)     '支払項目銀行支店コード
                Dim PARA20 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.VarChar, 1)     '支払項目口座種別
                Dim PARA21 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.VarChar, 8)     '支払項目口座番号
                Dim PARA22 As MySqlParameter = SQLcmd.Parameters.Add("@P22", MySqlDbType.VarChar, 30)    '支払項目口座名義人
                Dim PARA23 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.VarChar, 42)    '支払項目支払摘要
                Dim PARA25 As MySqlParameter = SQLcmd.Parameters.Add("@P25", MySqlDbType.DateTime)        '登録年月日
                Dim PARA26 As MySqlParameter = SQLcmd.Parameters.Add("@P26", MySqlDbType.VarChar, 20)    '登録ユーザーＩＤ
                Dim PARA27 As MySqlParameter = SQLcmd.Parameters.Add("@P27", MySqlDbType.VarChar, 20)    '登録端末
                Dim PARA28 As MySqlParameter = SQLcmd.Parameters.Add("@P28", MySqlDbType.VarChar, 40)    '登録プログラムＩＤ
                Dim PARA29 As MySqlParameter = SQLcmd.Parameters.Add("@P29", MySqlDbType.DateTime)        '更新年月日
                Dim PARA30 As MySqlParameter = SQLcmd.Parameters.Add("@P30", MySqlDbType.VarChar, 20)    '更新ユーザーＩＤ
                Dim PARA31 As MySqlParameter = SQLcmd.Parameters.Add("@P31", MySqlDbType.VarChar, 20)    '更新端末
                Dim PARA32 As MySqlParameter = SQLcmd.Parameters.Add("@P32", MySqlDbType.VarChar, 40)    '更新プログラムＩＤ

                ' 更新ジャーナル出力用パラメータ
                Dim JPARA01 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 6)  '発駅コード
                Dim JPARA02 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P02", MySqlDbType.VarChar, 5)  '発受託人コード
                Dim JPARA03 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P03", MySqlDbType.VarChar, 3)  '発受託人サブコード

                Dim LNM0003row As DataRow = LNM0003INPtbl.Rows(0)

                'Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA00.Value = LNM0003row("DELFLG")                                    '削除フラグ
                PARA01.Value = LNM0003row("DEPSTATION")                                '発駅コード
                PARA02.Value = LNM0003row("DEPTRUSTEECD")                              '発受託人コード
                PARA03.Value = LNM0003row("DEPTRUSTEESUBCD")                           '発受託人サブコード
                PARA04.Value = LNM0003row("DEPTRUSTEENM")                              '発受託人名称
                If String.IsNullOrEmpty(LNM0003row("DEPTRUSTEESUBNM")) Then            '発受託人サブ名称
                    PARA05.Value = DBNull.Value
                Else
                    PARA05.Value = LNM0003row("DEPTRUSTEESUBNM")
                End If
                If String.IsNullOrEmpty(LNM0003row("DEPTRUSTEESUBKANA")) Then                   '発受託人名称（カナ）
                    PARA06.Value = DBNull.Value
                Else
                    PARA06.Value = LNM0003row("DEPTRUSTEESUBKANA")
                End If
                If String.IsNullOrEmpty(LNM0003row("TORICODE")) Then                   '取引先コード
                    PARA07.Value = DBNull.Value
                Else
                    PARA07.Value = LNM0003row("TORICODE")
                End If
                If String.IsNullOrEmpty(LNM0003row("ELIGIBLEINVOICENUMBER")) Then      '適格請求書登録番号
                    PARA08.Value = DBNull.Value
                Else
                    PARA08.Value = LNM0003row("ELIGIBLEINVOICENUMBER")
                End If
                ' 請求項目は全てNULLかNOT NULLを登録
                If Not Trim(Convert.ToString(LNM0003row("INVKEIJYOBRANCHCD"))) = "" OrElse
                    Not Trim(Convert.ToString(LNM0003row("INVCYCL"))) = "" OrElse
                    Not Trim(Convert.ToString(LNM0003row("INVFILINGDEPT"))) = "" OrElse
                    Not Trim(Convert.ToString(LNM0003row("INVKESAIKBN"))) = "" OrElse
                    Not Trim(Convert.ToString(LNM0003row("INVSUBCD"))) = "" Then

                    PARA09.Value = LNM0003row("INVKEIJYOBRANCHCD")   '請求項目計上店コード
                    PARA10.Value = LNM0003row("INVCYCL")             '請求項目請求サイクル
                    PARA11.Value = LNM0003row("INVFILINGDEPT")       '請求項目請求書提出部店
                    PARA12.Value = LNM0003row("INVKESAIKBN")         '請求項目請求書決済区分
                    PARA13.Value = LNM0003row("INVSUBCD")            '請求項目請求書細分コード

                Else
                    PARA09.Value = DBNull.Value                      '請求項目計上店コード
                    PARA10.Value = DBNull.Value                      '請求項目請求サイクル
                    PARA11.Value = DBNull.Value                      '請求項目請求書提出部店
                    PARA12.Value = DBNull.Value                      '請求項目請求書決済区分
                    PARA13.Value = DBNull.Value                      '請求項目請求書細分コード
                End If
                If String.IsNullOrEmpty(LNM0003row("PAYKEIJYOBRANCHCD")) Then          '支払項目費用計上店コード
                    PARA14.Value = DBNull.Value
                Else
                    PARA14.Value = LNM0003row("PAYKEIJYOBRANCHCD")
                End If
                If String.IsNullOrEmpty(LNM0003row("PAYFILINGBRANCH")) Then            '支払項目支払書提出支店
                    PARA15.Value = DBNull.Value
                Else
                    PARA15.Value = LNM0003row("PAYFILINGBRANCH")
                End If
                If String.IsNullOrEmpty(LNM0003row("TAXCALCUNIT")) Then                '支払項目消費税計算単位
                    PARA16.Value = DBNull.Value
                Else
                    PARA16.Value = LNM0003row("TAXCALCUNIT")
                End If
                If String.IsNullOrEmpty(LNM0003row("PAYKESAIKBN")) Then                '支払項目決済区分
                    PARA17.Value = DBNull.Value
                Else
                    PARA17.Value = LNM0003row("PAYKESAIKBN")
                End If
                If String.IsNullOrEmpty(LNM0003row("PAYBANKCD")) Then                  '支払項目銀行コード
                    PARA18.Value = DBNull.Value
                Else
                    PARA18.Value = LNM0003row("PAYBANKCD")
                End If
                If String.IsNullOrEmpty(LNM0003row("PAYBANKBRANCHCD")) Then            '支払項目銀行支店コード
                    PARA19.Value = DBNull.Value
                Else
                    PARA19.Value = LNM0003row("PAYBANKBRANCHCD")
                End If
                If String.IsNullOrEmpty(LNM0003row("PAYACCOUNTTYPE")) Then             '支払項目口座種別
                    PARA20.Value = DBNull.Value
                Else
                    PARA20.Value = LNM0003row("PAYACCOUNTTYPE")
                End If
                If String.IsNullOrEmpty(LNM0003row("PAYACCOUNTNO")) Then             '支払項目口座番号
                    PARA21.Value = DBNull.Value
                Else
                    PARA21.Value = LNM0003row("PAYACCOUNTNO")
                End If
                If String.IsNullOrEmpty(LNM0003row("PAYACCOUNTNM")) Then             '支払項目口座名義人
                    PARA22.Value = DBNull.Value
                Else
                    PARA22.Value = LNM0003row("PAYACCOUNTNM")
                End If
                If String.IsNullOrEmpty(LNM0003row("PAYTEKIYO")) Then             '支払項目支払摘要
                    PARA23.Value = DBNull.Value
                Else
                    PARA23.Value = LNM0003row("PAYTEKIYO")
                End If
                PARA25.Value = WW_NOW                                                  '登録年月日
                PARA26.Value = Master.USERID                                           '登録ユーザーＩＤ
                PARA27.Value = Master.USERTERMID                                       '登録端末
                PARA28.Value = Me.GetType().BaseType.Name                              '登録プログラムＩＤ
                PARA29.Value = WW_NOW                                                  '更新年月日
                PARA30.Value = Master.USERID                                           '更新ユーザーＩＤ
                PARA31.Value = Master.USERTERMID                                       '更新端末
                PARA32.Value = Me.GetType().BaseType.Name                              '更新プログラムＩＤ
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA01.Value = LNM0003row("DEPSTATION")
                JPARA02.Value = LNM0003row("DEPTRUSTEECD")
                JPARA03.Value = LNM0003row("DEPTRUSTEESUBCD")

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0003UPDtbl) Then
                        LNM0003UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0003UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0003UPDtbl.Clear()
                    LNM0003UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0003UPDrow As DataRow In LNM0003UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0003C"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0003UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0003C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0003C UPDATE_INSERT"
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
    Protected Sub REKEJMEXISTS(ByVal SQLcon As MySqlConnection, ByRef WW_MODIFYKBN As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        'コンテナ決済マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DEPSTATION")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0003_REKEJM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        DEPSTATION         = @DEPSTATION")
        SQLStr.AppendLine("    AND DEPTRUSTEECD        = @DEPTRUSTEECD")
        SQLStr.AppendLine("    AND DEPTRUSTEESUBCD     = @DEPTRUSTEESUBCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード

                Dim LNM0003row As DataRow = LNM0003INPtbl.Rows(0)

                P_DEPSTATION.Value = LNM0003row("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = LNM0003row("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = LNM0003row("DEPTRUSTEESUBCD")               '発受託人サブコード

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
                        WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0003C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0003C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0114_REKEJHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEENM  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBNM  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBKANA  ")
        SQLStr.AppendLine("        ,TORICODE  ")
        SQLStr.AppendLine("        ,ELIGIBLEINVOICENUMBER  ")
        SQLStr.AppendLine("        ,INVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,INVCYCL  ")
        SQLStr.AppendLine("        ,INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,INVKESAIKBN  ")
        SQLStr.AppendLine("        ,INVSUBCD  ")
        SQLStr.AppendLine("        ,PAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,PAYFILINGBRANCH  ")
        SQLStr.AppendLine("        ,TAXCALCUNIT  ")
        SQLStr.AppendLine("        ,PAYKESAIKBN  ")
        SQLStr.AppendLine("        ,PAYBANKCD  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCD  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNO  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNM  ")
        SQLStr.AppendLine("        ,PAYTEKIYO  ")
        SQLStr.AppendLine("        ,BEFOREINVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,BEFOREINVFILINGDEPT  ")
        SQLStr.AppendLine("        ,BEFOREPAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,BEFOREPAYFILINGBRANCH  ")
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
        SQLStr.AppendLine("         DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEENM  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBNM  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBKANA  ")
        SQLStr.AppendLine("        ,TORICODE  ")
        SQLStr.AppendLine("        ,ELIGIBLEINVOICENUMBER  ")
        SQLStr.AppendLine("        ,INVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,INVCYCL  ")
        SQLStr.AppendLine("        ,INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,INVKESAIKBN  ")
        SQLStr.AppendLine("        ,INVSUBCD  ")
        SQLStr.AppendLine("        ,PAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,PAYFILINGBRANCH  ")
        SQLStr.AppendLine("        ,TAXCALCUNIT  ")
        SQLStr.AppendLine("        ,PAYKESAIKBN  ")
        SQLStr.AppendLine("        ,PAYBANKCD  ")
        SQLStr.AppendLine("        ,PAYBANKBRANCHCD  ")
        SQLStr.AppendLine("        ,PAYACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNO  ")
        SQLStr.AppendLine("        ,PAYACCOUNTNM  ")
        SQLStr.AppendLine("        ,PAYTEKIYO  ")
        SQLStr.AppendLine("        ,BEFOREINVKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,BEFOREINVFILINGDEPT  ")
        SQLStr.AppendLine("        ,BEFOREPAYKEIJYOBRANCHCD  ")
        SQLStr.AppendLine("        ,BEFOREPAYFILINGBRANCH  ")
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
        SQLStr.AppendLine("        LNG.LNM0003_REKEJM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        DEPSTATION         = @DEPSTATION")
        SQLStr.AppendLine("    AND DEPTRUSTEECD        = @DEPTRUSTEECD")
        SQLStr.AppendLine("    AND DEPTRUSTEESUBCD     = @DEPTRUSTEESUBCD")
        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0003row As DataRow = LNM0003INPtbl.Rows(0)

                ' DB更新
                P_DEPSTATION.Value = LNM0003row("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = LNM0003row("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = LNM0003row("DEPTRUSTEESUBCD")               '発受託人サブコード

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0003WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0003tbl.Rows(0)("DELFLG") = "0" And LNM0003row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0003WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0003WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0114_REKEJHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0114_REKEJHIST  INSERT"
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
    ''' 詳細画面-更新ボタン押下、確認ダイアログOKボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_ConfirmOkClick()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0003INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0003tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0003tbl, work.WF_SEL_INPTBL.Text)

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
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, "コンテナ取引先", needsPopUp:=True)
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
    Protected Sub DetailBoxToLNM0003INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)             '削除フラグ
        Master.EraseCharToIgnore(TxtDepStation.Text)         '発駅コード
        Master.EraseCharToIgnore(TxtDepTrusteeCd.Text)       '発受託人コード
        Master.EraseCharToIgnore(TxtDepTrusteeSubCd.Text)    '発受託人サブコード
        Master.EraseCharToIgnore(TxtDepTrusteeNm.Text)       '発受託人名称
        Master.EraseCharToIgnore(TxtDepTrusteeSubNm.Text)    '発受託人サブ名称
        Master.EraseCharToIgnore(TxtDepTrusteeSubKana.Text)  '発受託人名称（カナ）
        Master.EraseCharToIgnore(TxtToriCode.Text)           '取引先コード
        Master.EraseCharToIgnore(TxtInvNo.Text)              '適格請求書登録番号
        Master.EraseCharToIgnore(TxtInvKeijyoBranchCd.Text)  '請求項目計上店コード
        Master.EraseCharToIgnore(TxtInvCycl.Text)            '請求項目請求サイクル
        Master.EraseCharToIgnore(TxtInvFilingDept.Text)      '請求項目請求書提出部店
        Master.EraseCharToIgnore(TxtInvKesaiKbn.Text)        '請求項目請求書決済区分
        Master.EraseCharToIgnore(TxtInvSubCd.Text)           '請求項目請求書細分コード
        Master.EraseCharToIgnore(TxtPayKeijyoBranchCd.Text)  '支払項目費用計上店コード
        Master.EraseCharToIgnore(TxtPayFilingBranch.Text)    '支払項目支払書提出支店
        Master.EraseCharToIgnore(TxtTaxCalcUnit.Text)        '支払項目消費税計算単位
        Master.EraseCharToIgnore(TxtPayKesaiKbn.Text)        '支払項目決済区分
        Master.EraseCharToIgnore(TxtPayBankCd.Text)          '支払項目銀行コード
        Master.EraseCharToIgnore(TxtPayBankBranchCd.Text)    '支払項目銀行支店コード
        Master.EraseCharToIgnore(ddlPayAccountTypeName.SelectedValue)     '支払項目口座種別
        Master.EraseCharToIgnore(TxtPayAccountNo.Text)       '支払項目口座番号
        Master.EraseCharToIgnore(TxtPayAccountNm.Text)       '支払項目口座名義人
        Master.EraseCharToIgnore(TxtPayTekiyo.Text)          '支払項目支払摘要

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

        Master.CreateEmptyTable(LNM0003INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0003INProw As DataRow = LNM0003INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(LblSelLineCNT.Text) Then
            LNM0003INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(LblSelLineCNT.Text, LNM0003INProw("LINECNT"))
            Catch ex As Exception
                LNM0003INProw("LINECNT") = 0
            End Try
        End If

        LNM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0003INProw("UPDTIMSTP") = 0
        LNM0003INProw("SELECT") = 1
        LNM0003INProw("HIDDEN") = 0

        LNM0003INProw("DELFLG") = TxtDelFlg.Text                              '削除フラグ
        LNM0003INProw("DEPSTATION") = TxtDepStation.Text                      '発駅コード
        LNM0003INProw("DEPTRUSTEECD") = TxtDepTrusteeCd.Text                  '発受託人コード
        LNM0003INProw("DEPTRUSTEESUBCD") = TxtDepTrusteeSubCd.Text            '発受託人サブコード
        LNM0003INProw("DEPTRUSTEENM") = TxtDepTrusteeNm.Text                  '発受託人名称
        LNM0003INProw("DEPTRUSTEESUBNM") = TxtDepTrusteeSubNm.Text            '発受託人サブ名称
        LNM0003INProw("DEPTRUSTEESUBKANA") = TxtDepTrusteeSubKana.Text        '発受託人名称（カナ）
        LNM0003INProw("TORICODE") = TxtToriCode.Text                          '取引先コード
        LNM0003INProw("ELIGIBLEINVOICENUMBER") = TxtInvNo.Text                '適格請求書登録番号
        LNM0003INProw("INVKEIJYOBRANCHCD") = TxtInvKeijyoBranchCd.Text        '請求項目計上店コード
        LNM0003INProw("INVCYCL") = TxtInvCycl.Text                            '請求項目請求サイクル
        LNM0003INProw("INVFILINGDEPT") = TxtInvFilingDept.Text                '請求項目請求書提出部店
        LNM0003INProw("INVKESAIKBN") = TxtInvKesaiKbn.Text                    '請求項目請求書決済区分
        LNM0003INProw("INVSUBCD") = TxtInvSubCd.Text                          '請求項目請求書細分コード
        LNM0003INProw("PAYKEIJYOBRANCHCD") = TxtPayKeijyoBranchCd.Text        '支払項目費用計上店コード
        LNM0003INProw("PAYFILINGBRANCH") = TxtPayFilingBranch.Text            '支払項目支払書提出支店
        LNM0003INProw("TAXCALCUNIT") = TxtTaxCalcUnit.Text                    '支払項目消費税計算単位
        LNM0003INProw("PAYKESAIKBN") = TxtPayKesaiKbn.Text                    '支払項目決済区分
        LNM0003INProw("PAYBANKCD") = Strings.Right("0000" + TxtPayBankCd.Text, 4)  '支払項目銀行コード
        LNM0003INProw("PAYBANKBRANCHCD") = Strings.Right("000" + TxtPayBankBranchCd.Text, 3) '支払項目銀行支店コード
        LNM0003INProw("PAYACCOUNTTYPE") = ddlPayAccountTypeName.SelectedValue '支払項目口座種別
        LNM0003INProw("PAYACCOUNTNO") = TxtPayAccountNo.Text                  '支払項目口座番号
        LNM0003INProw("PAYACCOUNTNM") = TxtPayAccountNm.Text                  '支払項目口座名義人
        LNM0003INProw("PAYTEKIYO") = TxtPayTekiyo.Text                        '支払項目支払摘要

        '○ チェック用テーブルに登録する
        LNM0003INPtbl.Rows.Add(LNM0003INProw)

    End Sub
    ''' <summary>
    ''' 詳細画面-更新ボタン押下時処理
    ''' </summary>
    Protected Sub WF_UPDATE_Click()

        'Dim WW_ErrMessage As String = ""
        'WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        'If String.IsNullOrEmpty(TxtInvKeijyoBranchCd.Text) Then
        '    WW_ErrMessage = "請求項目計上店コード"
        '    WW_ErrSW = Messages.C_MESSAGE_NO.CTN_KEIKOKUMS_ERR
        'End If
        'If String.IsNullOrEmpty(TxtInvCycl.Text) Then
        '    If String.IsNullOrEmpty(WW_ErrMessage) Then
        '        WW_ErrMessage = WW_ErrMessage & "請求項目請求サイクル"
        '    Else
        '        WW_ErrMessage = WW_ErrMessage & "、請求項目請求サイクル"
        '    End If
        '    WW_ErrSW = Messages.C_MESSAGE_NO.CTN_KEIKOKUMS_ERR
        'End If
        'If String.IsNullOrEmpty(TxtInvFilingDept.Text) AndAlso String.IsNullOrEmpty(TxtInvKesaiKbn.Text) Then
        '    If String.IsNullOrEmpty(WW_ErrMessage) Then
        '        WW_ErrMessage = WW_ErrMessage & "請求項目請求書提出部店、請求項目請求書決済区分"
        '    Else
        '        WW_ErrMessage = WW_ErrMessage & "、請求項目請求書提出部店、請求項目請求書決済区分"
        '    End If
        '    WW_ErrSW = Messages.C_MESSAGE_NO.CTN_KEIKOKUMS_ERR
        'End If
        'If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
        '    ' エラーの場合は、確認ダイアログを表示し警告を表示
        '    Master.Output(C_MESSAGE_NO.CTN_KEIKOKUMS_ERR, C_MESSAGE_TYPE.WAR, I_PARA01:=WW_ErrMessage, I_PARA02:="W",
        '            needsPopUp:=True, messageBoxTitle:="警告", IsConfirm:=True, YesButtonId:="btnUpdateConfirmOK")
        'Else
        '    ' エラーではない場合は、確認ダイアログを表示せずに更新処理を実行
        '    WF_UPDATE_ConfirmOkClick()
        'End If

        WF_UPDATE_ConfirmOkClick()
    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0003INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0003INProw As DataRow = LNM0003INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0003row As DataRow In LNM0003tbl.Rows
            ' KEY項目が等しい時
            If LNM0003row("DEPSTATION") = LNM0003INProw("DEPSTATION") AndAlso
                LNM0003row("DEPTRUSTEECD") = LNM0003INProw("DEPTRUSTEECD") AndAlso
                LNM0003row("DEPTRUSTEESUBCD") = LNM0003INProw("DEPTRUSTEESUBCD") Then
                ' KEY項目以外の項目の差異をチェック
                If LNM0003row("DELFLG") = LNM0003INProw("DELFLG") AndAlso
                    LNM0003row("DEPTRUSTEENM") = LNM0003INProw("DEPTRUSTEENM") AndAlso
                    LNM0003row("DEPTRUSTEESUBNM") = LNM0003INProw("DEPTRUSTEESUBNM") AndAlso
                    LNM0003row("DEPTRUSTEESUBKANA") = LNM0003INProw("DEPTRUSTEESUBKANA") AndAlso
                    LNM0003row("TORICODE") = LNM0003INProw("TORICODE") AndAlso
                    LNM0003row("ELIGIBLEINVOICENUMBER") = LNM0003INProw("ELIGIBLEINVOICENUMBER") AndAlso
                    LNM0003row("INVKEIJYOBRANCHCD") = LNM0003INProw("INVKEIJYOBRANCHCD") AndAlso
                    LNM0003row("INVCYCL") = LNM0003INProw("INVCYCL") AndAlso
                    LNM0003row("INVFILINGDEPT") = LNM0003INProw("INVFILINGDEPT") AndAlso
                    LNM0003row("INVKESAIKBN") = LNM0003INProw("INVKESAIKBN") AndAlso
                    LNM0003row("INVSUBCD") = LNM0003INProw("INVSUBCD") AndAlso
                    LNM0003row("PAYKEIJYOBRANCHCD") = LNM0003INProw("PAYKEIJYOBRANCHCD") AndAlso
                    LNM0003row("PAYFILINGBRANCH") = LNM0003INProw("PAYFILINGBRANCH") AndAlso
                    LNM0003row("TAXCALCUNIT") = LNM0003INProw("TAXCALCUNIT") AndAlso
                    LNM0003row("PAYKESAIKBN") = LNM0003INProw("PAYKESAIKBN") AndAlso
                    LNM0003row("PAYBANKCD") = LNM0003INProw("PAYBANKCD") AndAlso
                    LNM0003row("PAYBANKBRANCHCD") = LNM0003INProw("PAYBANKBRANCHCD") AndAlso
                    LNM0003row("PAYACCOUNTTYPE") = LNM0003INProw("PAYACCOUNTTYPE") AndAlso
                    LNM0003row("PAYACCOUNTNO") = LNM0003INProw("PAYACCOUNTNO") AndAlso
                    LNM0003row("PAYTEKIYO") = LNM0003INProw("PAYTEKIYO") Then
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
        For Each LNM0003row As DataRow In LNM0003tbl.Rows
            Select Case LNM0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0003tbl, work.WF_SEL_INPTBL.Text)

        LblSelLineCNT.Text = ""               'LINECNT
        TxtMapId.Text = "M00001"              '画面ＩＤ
        TxtDelFlg.Text = ""                   '削除フラグ

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
                .Visible = true
                ' フィールドによってパラメータを変える
                Select Case WF_FIELD.Value
                    Case "TxtDepStation"         '発駅コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspStationSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub

                    Case "TxtDepTrusteeCd"       '発受託人コード
                        WW_PrmData = work.CreateDepTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtDepStation.Text)
                    Case "TxtDepTrusteeSubCd"  '発受託人サブコード
                        WW_PrmData = work.CreateDepTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtDepStation.Text, TxtDepTrusteeCd.Text)
                    Case "TxtToriCode"
                        WW_PrmData = work.CreateKekkjmParam(GL0018InvKesaiKbnList.LS_INVOICE_WITH.TORICODE)
                    Case "TxtInvKeijyoBranchCd"  '請求項目計上店コード
                        ' 支店を全て取得
                        WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP)
                    Case "TxtInvCycl"            '請求項目請求サイクル
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "INVCYCL")
                    Case "TxtInvFilingDept"      '請求項目請求書提出部店
                        ' 支店を全て取得
                        WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP)
                    Case "TxtInvKesaiKbn"        '請求項目請求書決済区分
                        WW_PrmData = work.CreateKekkjmParam(GL0018InvKesaiKbnList.LS_INVOICE_WITH.INV_KESAI_KBN, TxtToriCode.Text, TxtInvFilingDept.Text)
                    Case "TxtInvSubCd"           '請求項目請求書細分コード
                        WW_PrmData = work.CreateInvSubCdParam(TxtToriCode.Text, TxtInvFilingDept.Text, TxtInvKesaiKbn.Text)
                    Case "TxtPayKeijyoBranchCd"  '支払項目費用計上店コード
                        ' 情報システムの場合、操作ユーザーが所属する会社の組織を全て取得
                        WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP)
                    Case "TxtPayFilingBranch"    '支払項目支払書提出支店
                        ' 支店を全て取得
                        WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP)
                    Case "TxtTaxCalcUnit"        '支払項目消費税計算単位
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "TAXCALCUNIT")
                    Case "TxtDelFlg"             '削除フラグ
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                    Case "TxtPayBankCd",         '銀行コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspBankCodeSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    Case "TxtPayBankBranchCd",   '銀行支店コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspBankBranchCodeSingle()
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
            Case "TxtDelFlg"              '削除フラグ
                CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
                TxtDelFlg.Focus()
            Case "TxtDepStation"          '発駅コード
                CODENAME_get("DEPSTATION", TxtDepStation.Text, LblDepStationName.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblDepStationName.Text) And TxtDepStation.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtDepStation.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtDepStation.Focus()
                End If
            Case "TxtDepTrusteeCd"        '発受託人コード
                CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCd.Text, LblDepTrusteeCdName.Text, WW_RtnSW)
            Case "TxtDepTrusteeSubCd"     '発受託人サブコード
                CODENAME_get("DEPTRUSTEESUBCD", TxtDepTrusteeSubCd.Text, LblDepTrusteeSubCdName.Text, WW_RtnSW)
            Case "TxtToriCode"
                CODENAME_get("TORICD", TxtToriCode.Text, LblToriCdName.Text, WW_Dummy)
            Case "TxtInvKeijyoBranchCd"   '請求項目計上店コード
                CODENAME_get("INVKEIJYOBRANCHCD", TxtInvKeijyoBranchCd.Text, LblInvKeijyoBranchCdName.Text, WW_Dummy)
                TxtInvKeijyoBranchCd.Focus()
            Case "TxtInvCycl"             '請求項目請求サイクル
                CODENAME_get("INVCYCL", TxtInvCycl.Text, LblInvCyclName.Text, WW_Dummy)
                TxtInvCycl.Focus()
            Case "TxtInvFilingDept"       '請求項目請求書提出部店
                CODENAME_get("INVFILINGDEPT", TxtInvFilingDept.Text, LblInvFilingDeptName.Text, WW_Dummy)
                TxtInvFilingDept.Focus()
            Case "TxtInvKesaiKbn"         '請求項目請求書決済区分
                ' 請求摘要取得
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    SelectClaimTekiyo(SQLcon)
                End Using
                TxtInvKesaiKbn.Focus()
            Case "TxtInvSubCd"            '請求項目請求書細分コード
                ' 請求摘要取得
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    SelectClaimTekiyo(SQLcon)
                End Using
                TxtInvSubCd.Focus()
            Case "TxtPayKeijyoBranchCd"   '支払項目費用計上店コード
                CODENAME_get("PAYKEIJYOBRANCHCD", TxtPayKeijyoBranchCd.Text, LblPayKeijyoBranchCDName.Text, WW_Dummy)
                TxtPayKeijyoBranchCd.Focus()
            Case "TxtPayFilingBranch"     '支払項目支払書提出支店
                CODENAME_get("PAYFILINGBRANCH", TxtPayFilingBranch.Text, LblPayFilingBranchName.Text, WW_Dummy)
                TxtPayFilingBranch.Focus()
            Case "TxtTaxCalcUnit"         '支払項目消費税計算単位
                CODENAME_get("SEL_TAXCALCUNIT", TxtTaxCalcUnit.Text, LblTaxCalcUnitName.Text, WW_Dummy)
                TxtTaxCalcUnit.Focus()
            Case "TxtPayBankCd"           '銀行コード
                CODENAME_get("BANKCODE", TxtPayBankCd.Text, LblPayBankCd.Text, WW_Dummy)
                If LblPayBankCd.Text = "" Then
                    TxtPayBankBranchCd.Text = ""
                    LblPayBankBranchCd.Text = ""
                    'TxtPayBankBranchNameKana.Text = ""
                End If
                TxtPayBankCd.Focus()
            Case "TxtPayBankBranchCd"     '銀行支店コード
                CODENAME_get("BANKBRANCHCODE", TxtPayBankBranchCd.Text, LblPayBankBranchCd.Text, WW_Dummy)
                TxtPayBankBranchCd.Focus()

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
                Case "TxtDelFlg"             '削除フラグ
                    TxtDelFlg.Text = WW_SelectValue
                    LblDelFlgName.Text = WW_SelectText
                    TxtDelFlg.Focus()
                Case "TxtDepStation"         '発駅コード
                    TxtDepStation.Text = WW_SelectValue
                    LblDepStationName.Text = WW_SelectText
                    TxtDepStation.Focus()
                Case "TxtDepTrusteeCd"       '発受託人コード
                    TxtDepTrusteeCd.Text = WW_SelectValue
                    LblDepTrusteeCdName.Text = WW_SelectText
                    TxtDepTrusteeCd.Focus()
                Case "TxtDepTrusteeSubCd"    '発受託人サブコード
                    TxtDepTrusteeSubCd.Text = WW_SelectValue
                    LblDepTrusteeSubCdName.Text = WW_SelectText
                    TxtDepTrusteeSubCd.Focus()
                Case "TxtToriCode"           '取引先コード
                    TxtToriCode.Text = WW_SelectValue
                    LblToriCdName.Text = WW_SelectText
                    TxtToriCode.Focus()
                Case "TxtInvKeijyoBranchCd"  '請求項目計上店コード
                    TxtInvKeijyoBranchCd.Text = WW_SelectValue
                    LblInvKeijyoBranchCdName.Text = WW_SelectText
                    TxtInvKeijyoBranchCd.Focus()
                Case "TxtInvCycl"            '請求項目請求サイクル
                    TxtInvCycl.Text = WW_SelectValue
                    LblInvCyclName.Text = WW_SelectText
                    TxtInvCycl.Focus()
                Case "TxtInvFilingDept"      '請求項目請求書提出部店
                    TxtInvFilingDept.Text = WW_SelectValue
                    LblInvFilingDeptName.Text = WW_SelectText
                    TxtInvFilingDept.Focus()
                Case "TxtInvKesaiKbn"        '請求項目請求書決済区分
                    TxtInvKesaiKbn.Text = WW_SelectValue
                    ' 請求摘要取得
                    Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                        ' DataBase接続
                        SQLcon.Open()
                        ' 排他チェック
                        SelectClaimTekiyo(SQLcon)
                    End Using
                    TxtInvKesaiKbn.Focus()
                Case "TxtInvSubCd"           '請求項目請求書細分コード
                    TxtInvSubCd.Text = WW_SelectValue
                    ' 請求摘要取得
                    Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                        ' DataBase接続
                        SQLcon.Open()
                        ' 排他チェック
                        SelectClaimTekiyo(SQLcon)
                    End Using
                    TxtInvSubCd.Focus()
                Case "TxtPayKeijyoBranchCd"  '支払項目費用計上店コード
                    TxtPayKeijyoBranchCd.Text = WW_SelectValue
                    LblPayKeijyoBranchCDName.Text = WW_SelectText
                    TxtPayKeijyoBranchCd.Focus()
                Case "TxtPayFilingBranch"    '支払項目支払書提出支店
                    TxtPayFilingBranch.Text = WW_SelectValue
                    LblPayFilingBranchName.Text = WW_SelectText
                    TxtPayFilingBranch.Focus()
                Case "TxtTaxCalcUnit"        '支払項目消費税計算単位
                    TxtTaxCalcUnit.Text = WW_SelectValue
                    LblTaxCalcUnitName.Text = WW_SelectText
                    TxtTaxCalcUnit.Focus()
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
                Case "TxtDelFlg"              '削除フラグ
                    TxtDelFlg.Focus()
                Case "TxtDepStation"          '発駅コード
                    TxtDepStation.Focus()
                Case "TxtDepTrusteeCd"        '発受託人コード
                    TxtDepTrusteeCd.Focus()
                Case "TxtDepTrusteeSubCd"     '発受託人サブコード
                    TxtDepTrusteeSubCd.Focus()
                Case "TxtToriCode"            '取引先コード
                    TxtToriCode.Focus()
                Case "TxtInvKeijyoBranchCd"   '請求項目計上店コード
                    TxtInvKeijyoBranchCd.Focus()
                Case "TxtInvCycl"             '請求項目請求サイクル
                    TxtInvCycl.Focus()
                Case "TxtInvFilingDept"       '請求項目請求書提出部店
                    TxtInvFilingDept.Focus()
                Case "TxtInvKesaiKbn"         '請求項目請求書決済区分
                    TxtInvKesaiKbn.Focus()
                Case "TxtInvSubCd"            '請求項目請求書細分コード
                    TxtInvSubCd.Focus()
                Case "TxtPayKeijyoBranchCd"   '支払項目費用計上店コード
                    TxtPayKeijyoBranchCd.Focus()
                Case "TxtPayFilingBranch"     '支払項目支払書提出支店
                    TxtPayFilingBranch.Focus()
                Case "TxtTaxCalcUnit"         '支払項目消費税計算単位
                    TxtTaxCalcUnit.Focus()
                Case "TxtPayBankCd"　　　　　 '銀行コード
                    TxtPayBankCd.Focus()
                Case "TxtPayBankBranchCd"     '銀行支店コード
                    TxtPayBankBranchCd.Focus()
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
        Me.mspBankCodeSingle.SQL = CmnSearchSQL.GetBankCodeSQL(TxtPayBankCd.Text)

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

            Case TxtPayBankCd.ID
                Me.TxtPayBankCd.Text = selData("BANKCODE").ToString '銀行コード
                Me.LblPayBankCd.Text = selData("BANKNAME").ToString '銀行名
                'Me.TxtPayBankNameKana.Text = selData("BANKNAMEKANA").ToString '銀行名カナ
                Me.TxtPayBankCd.Focus()
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
        Me.mspBankBranchCodeSingle.SQL = CmnSearchSQL.GetBankBranchCodeSQL(TxtPayBankCd.Text)

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

            Case TxtPayBankBranchCd.ID
                Me.TxtPayBankBranchCd.Text = selData("BANKBRANCHCODE").ToString '銀行支店コード
                Me.LblPayBankBranchCd.Text = selData("BANKBRANCHNAME").ToString '銀行支店名
                'Me.TxtPayBankBranchNameKana.Text = selData("BANKBRANCHNAMEKANA").ToString '銀行支店名カナ
                Me.TxtPayBankBranchCd.Focus()
        End Select

        'ポップアップの非表示
        Me.mspBankBranchCodeSingle.HidePopUp()

    End Sub

    ''' <summary>
    ''' 駅検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspStationSingle(Optional ByVal prmKey As String = "")

        Me.mspStationSingle.InitPopUp()
        Me.mspStationSingle.SelectionMode = ListSelectionMode.Single
        Me.mspStationSingle.SQL = CmnSearchSQL.GetStationSQL(work.WF_SEL_CAMPCODE.Text)

        Me.mspStationSingle.KeyFieldName = "KEYCODE"
        Me.mspStationSingle.DispFieldList.AddRange(CmnSearchSQL.GetStationTitle)

        Me.mspStationSingle.ShowPopUpList(prmKey)

    End Sub

    ''' <summary>
    ''' 取引先選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspStationSingle()

        Dim selData = Me.mspStationSingle.SelectedSingleItem

        Me.TxtDepStation.Text = selData("STATION").ToString
        Me.LblDepStationName.Text = selData("NAMES").ToString
        Me.TxtDepStation.Focus()

        'ポップアップの非表示
        Me.mspStationSingle.HidePopUp()

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
        Dim WW_PayKesaiKbn As Integer

        '○ 画面操作権限チェック
        ' 権限チェック(操作者に更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            WW_CheckMES1 = "・コンテナ取引先マスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        Dim WW_Dummyht = New Hashtable
        Dim WW_BANKht = New Hashtable '銀行称格納HT
        Dim WW_BANKBRANCHht = New Hashtable '支店名称格納HT

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            work.CODENAMEGetBANK(SQLcon, WW_BANKht, WW_Dummyht)
            work.CODENAMEGetBANKBRANCH(SQLcon, LNM0003INPtbl(0)("PAYBANKCD"), WW_BANKBRANCHht, WW_Dummyht)
        End Using

        '○ 単項目チェック
        For Each LNM0003INProw As DataRow In LNM0003INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0003INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0003INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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
            ' 発駅コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPSTATION", LNM0003INProw("DEPSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DEPSTATION", LNM0003INProw("DEPSTATION"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・発駅コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・発駅コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 発受託人コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEECD", LNM0003INProw("DEPTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・発受託人コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 発受託人名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEENM", LNM0003INProw("DEPTRUSTEENM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・発受託人名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 発受託人名称（カナ）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBKANA", LNM0003INProw("DEPTRUSTEESUBKANA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・発受託人名称（カナ）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 発受託人サブコード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBCD", LNM0003INProw("DEPTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・発受託人サブコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 発受託人サブ名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBNM", LNM0003INProw("DEPTRUSTEESUBNM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・発受託人サブ名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 取引先コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORICODE", LNM0003INProw("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' グループ必須チェック(請求項目)
            If Not Trim(Convert.ToString(LNM0003INProw("INVKEIJYOBRANCHCD"))) = "" Then

                '請求項目 計上店コード
                If Trim(Convert.ToString(LNM0003INProw("INVKEIJYOBRANCHCD"))) = "" Then
                    WW_CheckMES1 = "・請求項目 計上店コードエラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                '請求項目 請求サイクル
                If Trim(Convert.ToString(LNM0003INProw("INVCYCL"))) = "" Then
                    WW_CheckMES1 = "・請求項目 請求サイクルエラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                '請求項目 請求書提出部店
                If Trim(Convert.ToString(LNM0003INProw("INVFILINGDEPT"))) = "" Then
                    WW_CheckMES1 = "・請求項目 請求書提出部店エラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                '請求項目 請求書決済区分
                If Trim(Convert.ToString(LNM0003INProw("INVKESAIKBN"))) = "" Then
                    WW_CheckMES1 = "・請求項目 請求書決済区分エラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                '請求項目 請求書細分コード
                If Trim(Convert.ToString(LNM0003INProw("INVSUBCD"))) = "" Then
                    WW_CheckMES1 = "・請求項目 請求書細分コードエラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                ' 請求項目計上店コード(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "INVKEIJYOBRANCHCD", LNM0003INProw("INVKEIJYOBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0003INProw("INVKEIJYOBRANCHCD")) Then
                        ' 名称存在チェック
                        CODENAME_get("INVKEIJYOBRANCHCD", LNM0003INProw("INVKEIJYOBRANCHCD"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・請求項目計上店コードエラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・請求項目計上店コードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 請求項目請求サイクル(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "INVCYCL", LNM0003INProw("INVCYCL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0003INProw("INVCYCL")) Then
                        ' 名称存在チェック
                        CODENAME_get("INVCYCL", LNM0003INProw("INVCYCL"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・請求項目請求サイクルエラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・請求項目請求サイクルエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 請求項目請求書提出部店(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "INVFILINGDEPT", LNM0003INProw("INVFILINGDEPT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0003INProw("INVFILINGDEPT")) Then
                        ' 名称存在チェック
                        CODENAME_get("INVFILINGDEPT", LNM0003INProw("INVFILINGDEPT"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・請求項目請求書提出部店エラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・請求項目請求書提出部店エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 請求項目請求書決済区分(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "INVKESAIKBN", LNM0003INProw("INVKESAIKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0003INProw("INVKESAIKBN")) Then
                        ' 名称存在チェック
                        CODENAME_get("INVKESAIKBN", LNM0003INProw("INVKESAIKBN"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・請求項目請求書決済区分エラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・請求項目請求書決済区分エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 請求項目請求書細分コード(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "INVSUBCD", LNM0003INProw("INVSUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    '    If Not String.IsNullOrEmpty(LNM0003INProw("INVSUBCD")) Then
                    '        ' 名称存在チェック
                    '        CODENAME_get("INVSUBCD", LNM0003INProw("INVSUBCD"), WW_Dummy, WW_RtnSW)
                    '        If Not isNormal(WW_RtnSW) Then
                    '            WW_CheckMES1 = "・請求項目請求書細分コードエラーです。"
                    '            WW_CheckMES2 = "マスタに存在しません。"
                    '            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    '            WW_LineErr = "ERR"
                    '            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    '        End If
                    '    End If
                    'Else
                    WW_CheckMES1 = "・請求項目請求書細分コードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            ' グループ必須チェック(支払項目)
            If Not Trim(Convert.ToString(LNM0003INProw("PAYKEIJYOBRANCHCD"))) = "" Then

                '支払項目 費用計上店コード
                If Trim(Convert.ToString(LNM0003INProw("PAYKEIJYOBRANCHCD"))) = "" Then
                    WW_CheckMES1 = "・支払項目 費用計上店コードエラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                '支払項目 支払書提出支店
                If Trim(Convert.ToString(LNM0003INProw("PAYFILINGBRANCH"))) = "" Then
                    WW_CheckMES1 = "・支払項目 支払書提出支店エラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                '支払項目 消費税計算単位
                If Trim(Convert.ToString(LNM0003INProw("TAXCALCUNIT"))) = "" Then
                    WW_CheckMES1 = "・支払項目 消費税計算単位エラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                '支払項目 銀行コード
                If Trim(Convert.ToString(LNM0003INProw("PAYBANKCD"))) = "0000" Then
                    WW_CheckMES1 = "・支払項目 銀行コードエラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                '支払項目 銀行支店コード
                If Trim(Convert.ToString(LNM0003INProw("PAYBANKBRANCHCD"))) = "000" Then
                    WW_CheckMES1 = "・支払項目 銀行支店コードエラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                '支払項目 口座種別
                If Trim(Convert.ToString(LNM0003INProw("PAYACCOUNTTYPE"))) = "0" Then
                    WW_CheckMES1 = "・支払項目 口座種別エラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                '支払項目 口座番号
                If Trim(Convert.ToString(LNM0003INProw("PAYACCOUNTNO"))) = "" Then
                    WW_CheckMES1 = "・支払項目 口座番号エラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                '支払項目 口座名義人
                If Trim(Convert.ToString(LNM0003INProw("PAYACCOUNTNM"))) = "" Then
                    WW_CheckMES1 = "・支払項目 口座名義人エラーです。"
                    WW_CheckMES2 = "必須入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.PREREQUISITE_ERROR
                End If

                ' 支払項目費用計上店コード(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "PAYKEIJYOBRANCHCD", LNM0003INProw("PAYKEIJYOBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0003INProw("PAYKEIJYOBRANCHCD")) Then
                        ' 名称存在チェック
                        CODENAME_get("PAYKEIJYOBRANCHCD", LNM0003INProw("PAYKEIJYOBRANCHCD"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・支払項目費用計上店コードエラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・支払項目費用計上店コードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 支払項目支払書提出支店(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "PAYFILINGBRANCH", LNM0003INProw("PAYFILINGBRANCH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0003INProw("PAYFILINGBRANCH")) Then
                        ' 名称存在チェック
                        CODENAME_get("PAYFILINGBRANCH", LNM0003INProw("PAYFILINGBRANCH"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・支払項目支払書提出支店エラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・支払項目支払書提出支店エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 支払項目消費税計算単位(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "TAXCALCUNIT", LNM0003INProw("TAXCALCUNIT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0003INProw("TAXCALCUNIT")) Then
                        ' 名称存在チェック
                        CODENAME_get("TAXCALCUNIT", LNM0003INProw("TAXCALCUNIT"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・支払項目消費税計算単位エラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・支払項目消費税計算単位エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 支払項目決済区分(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "PAYKESAIKBN", LNM0003INProw("PAYKESAIKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0003INProw("PAYKESAIKBN")) Then
                        WW_PayKesaiKbn = Integer.Parse(LNM0003INProw("PAYKESAIKBN"))
                        If 1 >= WW_PayKesaiKbn OrElse WW_PayKesaiKbn >= 7 Then
                            WW_CheckMES1 = "・支払項目決済区分エラーです。"
                            WW_CheckMES2 = "2～6で入力してください。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・支払項目決済区分エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 支払項目銀行コード(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "PAYBANKCD", LNM0003INProw("PAYBANKCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not Trim(Convert.ToString(LNM0003INProw("PAYBANKCD"))) = "0000" Then
                        ' コード存在チェック
                        If Not WW_BANKht.ContainsKey(LNM0003INProw("PAYBANKCD")) Then
                            WW_CheckMES1 = "・支払項目銀行コードエラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・支払項目銀行コードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 支払項目銀行支店コード(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "PAYBANKBRANCHCD", LNM0003INProw("PAYBANKBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not Trim(Convert.ToString(LNM0003INProw("PAYBANKBRANCHCD"))) = "000" Then
                        ' コード存在チェック
                        If Not WW_BANKBRANCHht.ContainsKey(LNM0003INProw("PAYBANKBRANCHCD")) Then
                            WW_CheckMES1 = "・支払項目銀行支店コードエラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・支払項目銀行支店コードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 支払項目口座種別(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "PAYACCOUNTTYPE", LNM0003INProw("PAYACCOUNTTYPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・支払項目口座種別エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 支払項目口座番号(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "PAYACCOUNTNO", LNM0003INProw("PAYACCOUNTNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・支払項目口座番号エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 支払項目口座名義人(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "PAYACCOUNTNM", LNM0003INProw("PAYACCOUNTNM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・支払項目口座名義人エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                ' 支払項目支払摘要(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "PAYTEKIYO", LNM0003INProw("PAYTEKIYO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・支払項目支払摘要エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If

            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_DEPSTATION2.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                    work.WF_SEL_DEPSTATION2.Text,
                                    work.WF_SEL_DEPTRUSTEECD2.Text,
                                    work.WF_SEL_DEPTRUSTEESUBCD2.Text,
                                    work.WF_SEL_TIMESTAMP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（発駅コード & 発受託人コード & 発受託人サブコード）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                       "([" & LNM0003INProw("DEPSTATION") & "]" &
                                       " [" & LNM0003INProw("DEPTRUSTEECD") & "])" &
                                       " [" & LNM0003INProw("DEPTRUSTEESUBCD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNM0003INProw("DEPSTATION") = work.WF_SEL_DEPSTATION2.Text OrElse
                    Not LNM0003INProw("DEPTRUSTEECD") = work.WF_SEL_DEPTRUSTEECD2.Text OrElse
                    Not LNM0003INProw("DEPTRUSTEESUBCD") = work.WF_SEL_DEPTRUSTEESUBCD2.Text Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（発駅コード & 発受託人コード & 発受託人サブコード）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & LNM0003INProw("DEPSTATION") & "]" &
                                       " [" & LNM0003INProw("DEPTRUSTEECD") & "])" &
                                       " [" & LNM0003INProw("DEPTRUSTEESUBCD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LineErr = "" Then
                If LNM0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0003INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0003INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0003tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0003tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0003row As DataRow In LNM0003tbl.Rows
            Select Case LNM0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0003INProw As DataRow In LNM0003INPtbl.Rows
            ' エラーレコード読み飛ばし
            If LNM0003INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0003INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0003row As DataRow In LNM0003tbl.Rows
                ' KEY項目が等しい時
                If LNM0003row("DEPSTATION") = LNM0003INProw("DEPSTATION") AndAlso
                    LNM0003row("DEPTRUSTEECD") = LNM0003INProw("DEPTRUSTEECD") AndAlso
                    LNM0003row("DEPTRUSTEESUBCD") = LNM0003INProw("DEPTRUSTEESUBCD") Then
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0003row("DELFLG") = LNM0003INProw("DELFLG") AndAlso
                        LNM0003row("DEPTRUSTEENM") = LNM0003INProw("DEPTRUSTEENM") AndAlso
                        LNM0003row("DEPTRUSTEESUBNM") = LNM0003INProw("DEPTRUSTEESUBNM") AndAlso
                        LNM0003row("DEPTRUSTEESUBKANA") = LNM0003INProw("DEPTRUSTEESUBKANA") AndAlso
                        LNM0003row("TORICODE") = LNM0003INProw("TORICODE") AndAlso
                        LNM0003row("ELIGIBLEINVOICENUMBER") = LNM0003INProw("ELIGIBLEINVOICENUMBER") AndAlso
                        LNM0003row("INVKEIJYOBRANCHCD") = LNM0003INProw("INVKEIJYOBRANCHCD") AndAlso
                        LNM0003row("INVCYCL") = LNM0003INProw("INVCYCL") AndAlso
                        LNM0003row("INVFILINGDEPT") = LNM0003INProw("INVFILINGDEPT") AndAlso
                        LNM0003row("INVKESAIKBN") = LNM0003INProw("INVKESAIKBN") AndAlso
                        LNM0003row("INVSUBCD") = LNM0003INProw("INVSUBCD") AndAlso
                        LNM0003row("PAYKEIJYOBRANCHCD") = LNM0003INProw("PAYKEIJYOBRANCHCD") AndAlso
                        LNM0003row("PAYFILINGBRANCH") = LNM0003INProw("PAYFILINGBRANCH") AndAlso
                        LNM0003row("TAXCALCUNIT") = LNM0003INProw("TAXCALCUNIT") AndAlso
                        LNM0003row("PAYKESAIKBN") = LNM0003INProw("PAYKESAIKBN") AndAlso
                        LNM0003row("PAYBANKCD") = LNM0003INProw("PAYBANKCD") AndAlso
                        LNM0003row("PAYBANKBRANCHCD") = LNM0003INProw("PAYBANKBRANCHCD") AndAlso
                        LNM0003row("PAYACCOUNTTYPE") = LNM0003INProw("PAYACCOUNTTYPE") AndAlso
                        LNM0003row("PAYACCOUNTNO") = LNM0003INProw("PAYACCOUNTNO") AndAlso
                        LNM0003row("PAYACCOUNTNM") = LNM0003INProw("PAYACCOUNTNM") AndAlso
                        LNM0003row("PAYTEKIYO") = LNM0003INProw("PAYTEKIYO") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0003row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0003INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        ' 更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0003INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0003INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0003INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now

                '変更チェック
                REKEJMEXISTS(SQLcon, WW_MODIFYKBN)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0003WRKINC.MODIFYKBN.AFTDATA
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
        For Each LNM0003INProw As DataRow In LNM0003INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0003row As DataRow In LNM0003tbl.Rows
                ' 同一レコードか判定
                If LNM0003INProw("DEPSTATION") = LNM0003row("DEPSTATION") AndAlso
                    LNM0003INProw("DEPTRUSTEECD") = LNM0003row("DEPTRUSTEECD") AndAlso
                    LNM0003INProw("DEPTRUSTEESUBCD") = LNM0003row("DEPTRUSTEESUBCD") Then
                    ' 画面入力テーブル項目設定
                    LNM0003INProw("LINECNT") = LNM0003row("LINECNT")
                    LNM0003INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0003INProw("UPDTIMSTP") = LNM0003row("UPDTIMSTP")
                    LNM0003INProw("SELECT") = 0
                    LNM0003INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0003row.ItemArray = LNM0003INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0003tbl.NewRow
                WW_NRow.ItemArray = LNM0003INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0003tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0003tbl.Rows.Add(WW_NRow)
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
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
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
                Case "BANKCODE"             '銀行コード
                    work.CODENAMEGetBANK(SQLcon, WW_NAMEht, WW_KANAht)
                Case "BANKBRANCHCODE"       '銀行支店コード
                    If Not TxtPayBankCd.Text = "" Then
                        work.CODENAMEGetBANKBRANCH(SQLcon, TxtPayBankCd.Text, WW_NAMEht, WW_KANAht)
                    Else
                        Exit Sub
                    End If
            End Select
        End Using

        Try
            Select Case I_FIELD
                Case "DEPSTATION"         '発駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "DEPTRUSTEECD"       '発受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateDepTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtDepStation.Text))
                Case "DEPTRUSTEESUBCD"    '発受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateDepTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtDepStation.Text, TxtDepTrusteeCd.Text))
                Case "TORICD"
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KEKKJM, I_VALUE, O_TEXT, O_RTN, work.CreateKekkjmParam(GL0018InvKesaiKbnList.LS_INVOICE_WITH.TORICODE))
                Case "INVKEIJYOBRANCHCD"  '請求項目計上店コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "INVCYCL"            '請求項目請求サイクル
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "INVFILINGDEPT"      '請求項目請求書提出部店
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "INVKESAIKBN"        '請求項目請求書決済区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KEKKJM, I_VALUE, O_TEXT, O_RTN, work.CreateKekkjmParam(GL0018InvKesaiKbnList.LS_INVOICE_WITH.INV_KESAI_KBN, TxtToriCode.Text, TxtInvFilingDept.Text))
                Case "INVSUBCD"           '請求項目請求書細分コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KEKSBM, I_VALUE, O_TEXT, O_RTN, work.CreateInvSubCdParam(TxtToriCode.Text, TxtInvFilingDept.Text, TxtInvKesaiKbn.Text))
                Case "PAYKEIJYOBRANCHCD"  '支払項目費用計上店コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "PAYFILINGBRANCH"    '支払項目支払書提出支店
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "TAXCALCUNIT"        '支払項目消費税計算単位
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))

                Case "OUTPUTID"           '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))
                Case "ONOFF"              '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))
                Case "DELFLG"             '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "BANKCODE"           '銀行コード
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
                    O_RTN = C_MESSAGE_NO.NORMAL
                Case "BANKBRANCHCODE"     '銀行支店コード
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
                    O_RTN = C_MESSAGE_NO.NORMAL
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
