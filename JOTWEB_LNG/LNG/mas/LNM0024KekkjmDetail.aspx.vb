''************************************************************
' 営業収入決済条件マスタメンテ登録画面
' 作成日 2022/05/23
' 更新日 2023/10/26
' 作成者 瀬口
' 更新者 大浜
'
' 修正履歴 : 2022/05/23 新規作成
'          : 2023/10/26 変更履歴画面、UL/DL機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 営業収入決済条件マスタメンテ（詳細）
''' </summary>
''' <remarks></remarks>
Public Class LNM0024KekkjmDetail
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private LNM0024tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0024INPtbl As DataTable                              'チェック用テーブル
    Private LNM0024UPDtbl As DataTable                              '更新用テーブル

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
    Private WW_ChkDate As Integer = 0
    Private WW_ChkDate8str As String = ""
    Private WW_ChkDate8ymd As String = ""
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
                    Master.RecoverTable(LNM0024tbl, work.WF_SEL_INPTBL.Text)

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
                        Case "btnUpdateConfirmOK"        '戻るボタン押下後の確認ダイアログでOK押下
                            WF_UPDATE_ConfirmOkClick()
                        Case "mspToriSingleRowSelected"         '[共通]取引先選択ポップアップで行選択
                            RowSelected_mspToriSingle()
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
            If Not IsNothing(LNM0024tbl) Then
                LNM0024tbl.Clear()
                LNM0024tbl.Dispose()
                LNM0024tbl = Nothing
            End If

            If Not IsNothing(LNM0024INPtbl) Then
                LNM0024INPtbl.Clear()
                LNM0024INPtbl.Dispose()
                LNM0024INPtbl = Nothing
            End If

            If Not IsNothing(LNM0024UPDtbl) Then
                LNM0024UPDtbl.Clear()
                LNM0024UPDtbl.Dispose()
                LNM0024UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0024WRKINC.MAPIDD
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0024L Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        lblSelLineCNT.Text = work.WF_SEL_LINECNT.Text
        '削除
        txtDelFlg.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", txtDelFlg.Text, lblDelFlgName.Text, WW_Dummy)
        '画面ＩＤ
        txtMapId.Text = "LNM0024D"

        '取引先コード
        txtToriCode.Text = work.WF_SEL_TORICODE2.Text
        CODENAME_get("TORICODE", txtToriCode.Text, lblToriCodeName.Text, WW_Dummy)

        '請求書提出部店
        txtInvFilingDept.Text = work.WF_SEL_INVFILINGDEPT2.Text
        CODENAME_get("INVFILINGDEPT", txtInvFilingDept.Text, lblInvFilingDeptName.Text, WW_Dummy)

        '請求書決済区分
        txtInvKesaiKbn.Text = work.WF_SEL_INVKESAIKBN2.Text
        CODENAME_get("INVKESAIKBN", txtInvKesaiKbn.Text, lblInvKesaiKbnName.Text, WW_Dummy)

        '取引先名称
        txtToriName.Text = work.WF_SEL_TORINAME.Text

        '取引先略称
        txtToriNameS.Text = work.WF_SEL_TORINAMES.Text

        '取引先カナ名称
        txtToriNameKana.Text = work.WF_SEL_TORINAMEKANA.Text

        '取引先部門名称
        txtToriDivName.Text = work.WF_SEL_TORIDIVNAME.Text

        '取引先担当者
        txtToriCharge.Text = work.WF_SEL_TORICHARGE.Text

        '取引先区分
        txtToriKbn.Text = work.WF_SEL_TORIKBN.Text

        '郵便番号（上）
        txtPostNum1.Text = work.WF_SEL_POSTNUM1.Text

        '郵便番号（下）
        txtPostNum2.Text = work.WF_SEL_POSTNUM2.Text

        '住所１
        txtAddr1.Text = work.WF_SEL_ADDR1.Text

        '住所２
        txtAddr2.Text = work.WF_SEL_ADDR2.Text

        '住所３
        txtAddr3.Text = work.WF_SEL_ADDR3.Text

        '住所４
        txtAddr4.Text = work.WF_SEL_ADDR4.Text

        '電話番号
        txtTel.Text = work.WF_SEL_TEL.Text

        'ＦＡＸ番号
        txtFax.Text = work.WF_SEL_FAX.Text

        'メールアドレス
        txtMail.Text = work.WF_SEL_MAIL.Text

        '銀行コード
        txtBankCode.Text = work.WF_SEL_BANKCODE.Text

        '支店コード
        txtBankBranchCode.Text = work.WF_SEL_BANKBRANCHCODE.Text

        '口座種別
        txtAccountType.Text = work.WF_SEL_ACCOUNTTYPE.Text

        '口座番号
        txtAccountNumber.Text = work.WF_SEL_ACCOUNTNUMBER.Text

        '口座名義
        txtAccountName.Text = work.WF_SEL_ACCOUNTNAME.Text

        '社内口座コード
        txtInAccountCd.Text = work.WF_SEL_INACCOUNTCD.Text

        '税計算区分
        txtTaxcalculation.Text = work.WF_SEL_TAXCALCULATION.Text

        '入金日
        txtDepositDay.Text = work.WF_SEL_DEPOSITDAY.Text

        '入金月区分
        txtDepositMonthKbn.Text = work.WF_SEL_DEPOSITMONTHKBN.Text
        CODENAME_get("DEPOSITMONTHKBN", txtDepositMonthKbn.Text, lblDepositMonthKbnName.Text, WW_Dummy)

        '計上締日
        txtClosingday.Text = work.WF_SEL_CLOSINGDAY.Text

        '計上月区分
        txtAccountingMonth.Text = work.WF_SEL_ACCOUNTINGMONTH.Text
        CODENAME_get("KEIJOMKBN", txtAccountingMonth.Text, lblAccountingMonthName.Text, WW_Dummy)

        '伝票摘要１
        txtSlipDescription1.Text = work.WF_SEL_SLIPDESCRIPTION1.Text

        '伝票摘要２
        txtSlipDescription2.Text = work.WF_SEL_SLIPDESCRIPTION2.Text

        '運賃翌月未決済区分
        txtNextMonthUnSettledKbn.Text = work.WF_SEL_NEXTMONTHUNSETTLEDKBN.Text


        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_TORICODE2.Text

        ' 数値(0～9)のみ入力可能とする。
        Me.txtDelFlg.Attributes("onkeyPress") = "CheckNum()"                   '削除フラグ
        Me.txtToriCode.Attributes("onkeyPress") = "CheckNum()"                 '取引先コード
        Me.txtInvFilingDept.Attributes("onkeyPress") = "CheckNum()"            '請求書提出部店
        Me.txtInvKesaiKbn.Attributes("onkeyPress") = "CheckNum()"              '請求書決済区分
        Me.txtToriKbn.Attributes("onkeyPress") = "CheckNum()"                  '取引先区分
        Me.txtPostNum1.Attributes("onkeyPress") = "CheckNum()"                 '郵便番号（上）
        Me.txtPostNum2.Attributes("onkeyPress") = "CheckNum()"                 '郵便番号（下）
        Me.txtTel.Attributes("onkeyPress") = "CheckNum()"                      '電話番号
        Me.txtFax.Attributes("onkeyPress") = "CheckNum()"                      'ＦＡＸ番号
        Me.txtTaxcalculation.Attributes("onkeyPress") = "CheckNum()"           '税計算区分
        Me.txtDepositDay.Attributes("onkeyPress") = "CheckNum()"               '入金日
        Me.txtDepositMonthKbn.Attributes("onkeyPress") = "CheckNum()"          '入金月区分
        Me.txtClosingday.Attributes("onkeyPress") = "CheckNum()"               '計上締日
        Me.txtAccountingMonth.Attributes("onkeyPress") = "CheckNum()"          '計上月区分
        Me.txtNextMonthUnSettledKbn.Attributes("onkeyPress") = "CheckNum()"    '運賃翌月未決済区分

        '取引先
        Dim retToriList As DropDownList = CmnSearchSQL.getDdlTori()
        If retToriList.Items.Count > 0 Then
            Me.hdnSelectTori.Items.AddRange(retToriList.Items.Cast(Of ListItem).ToArray)
        End If

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
            & "     TORICODE                   " _
            & "   , INVFILINGDEPT              " _
            & "   , INVKESAIKBN                " _
            & " FROM                           " _
            & "     LNG.LNM0024_KEKKJM         " _
            & " WHERE                          " _
            & "         TORICODE        = @P1  " _
            & "     AND INVFILINGDEPT   = @P3  " _
            & "     AND INVKESAIKBN     = @P4  " _
            & "     AND DELFLG         <> @P5  "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 10) '取引先コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 6)  '請求書提出部店
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 2)  '請求書決済区分
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.VarChar, 1)  '削除フラグ

                PARA1.Value = txtToriCode.Text
                PARA3.Value = txtInvFilingDept.Text
                PARA4.Value = txtInvKesaiKbn.Text
                PARA5.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0024Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0024Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0024Chk.Load(SQLdr)

                    If LNM0024Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0024D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0024D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 営業収入決済条件マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(営業収入決済条件マスタ)
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;                 " _
            & "     SET @hensuu = 0 ;                       " _
            & " DECLARE hensuu CURSOR FOR                   " _
            & "     SELECT                                  " _
            & "         UPDTIMSTP AS hensuu                 " _
            & "     FROM                                    " _
            & "         LNG.LNM0024_KEKKJM                  " _
            & "     WHERE                                   " _
            & "             TORICODE           = @P01       " _
            & "         AND INVFILINGDEPT      = @P03       " _
            & "         AND INVKESAIKBN        = @P04 ;     " _
            & " OPEN hensuu ;                               " _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;       " _
            & " IF (@@FETCH_STATUS = 0)                     " _
            & "     UPDATE LNG.LNM0024_KEKKJM               " _
            & "     SET                                     " _
            & "         TORINAME               = @P05       " _
            & "       , TORINAMES              = @P06       " _
            & "       , TORINAMEKANA           = @P07       " _
            & "       , TORIDIVNAME            = @P08       " _
            & "       , TORICHARGE             = @P09       " _
            & "       , TORIKBN                = @P10       " _
            & "       , POSTNUM1               = @P11       " _
            & "       , POSTNUM2               = @P12       " _
            & "       , ADDR1                  = @P13       " _
            & "       , ADDR2                  = @P14       " _
            & "       , ADDR3                  = @P15       " _
            & "       , ADDR4                  = @P16       " _
            & "       , TEL                    = @P17       " _
            & "       , FAX                    = @P18       " _
            & "       , MAIL                   = @P19       " _
            & "       , BANKCODE               = @P20       " _
            & "       , BANKBRANCHCODE         = @P21       " _
            & "       , ACCOUNTTYPE            = @P22       " _
            & "       , ACCOUNTNUMBER          = @P23       " _
            & "       , ACCOUNTNAME            = @P24       " _
            & "       , INACCOUNTCD            = @P25       " _
            & "       , TAXCALCULATION         = @P26       " _
            & "       , DEPOSITDAY             = @P27       " _
            & "       , DEPOSITMONTHKBN        = @P28       " _
            & "       , CLOSINGDAY          　 = @P29       " _
            & "       , ACCOUNTINGMONTH        = @ACCOUNTINGMONTH  " _
            & "       , SLIPDESCRIPTION1       = @P30       " _
            & "       , SLIPDESCRIPTION2       = @P31       " _
            & "       , NEXTMONTHUNSETTLEDKBN  = @P32       " _
            & "       , DELFLG                 = @P33       " _
            & "       , INITYMD                = @P34       " _
            & "       , INITUSER               = @P35       " _
            & "       , INITTERMID             = @P36       " _
            & "       , INITPGID               = @P37       " _
            & "       , UPDYMD                 = @P38       " _
            & "       , UPDUSER                = @P39       " _
            & "       , UPDTERMID              = @P40       " _
            & "       , UPDPGID                = @P41       " _
            & "       , RECEIVEYMD             = @P42       " _
            & "     WHERE                                   " _
            & "             TORICODE           = @P01       " _
            & "         AND INVFILINGDEPT      = @P03       " _
            & "         AND INVKESAIKBN        = @P04 ;     " _
            & " IF (@@FETCH_STATUS <> 0)                    " _
            & "     INSERT INTO LNG.LNM0024_KEKKJM          " _
            & "        (TORICODE                            " _
            & "       , INVFILINGDEPT                       " _
            & "       , INVKESAIKBN                         " _
            & "       , TORINAME                            " _
            & "       , TORINAMES                           " _
            & "       , TORINAMEKANA                        " _
            & "       , TORIDIVNAME                         " _
            & "       , TORICHARGE                          " _
            & "       , TORIKBN                             " _
            & "       , POSTNUM1                            " _
            & "       , POSTNUM2                            " _
            & "       , ADDR1                               " _
            & "       , ADDR2                               " _
            & "       , ADDR3                               " _
            & "       , ADDR4                               " _
            & "       , TEL                                 " _
            & "       , FAX                                 " _
            & "       , MAIL                                " _
            & "       , BANKCODE                            " _
            & "       , BANKBRANCHCODE                      " _
            & "       , ACCOUNTTYPE                         " _
            & "       , ACCOUNTNUMBER                       " _
            & "       , ACCOUNTNAME                         " _
            & "       , INACCOUNTCD                         " _
            & "       , TAXCALCULATION                      " _
            & "       , DEPOSITDAY                          " _
            & "       , DEPOSITMONTHKBN                     " _
            & "       , CLOSINGDAY                          " _
            & "       , ACCOUNTINGMONTH                     " _
            & "       , SLIPDESCRIPTION1                    " _
            & "       , SLIPDESCRIPTION2                    " _
            & "       , NEXTMONTHUNSETTLEDKBN               " _
            & "       , DELFLG                              " _
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
            & "        (@P01                                " _
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
            & "       , @P24                                " _
            & "       , @P25                                " _
            & "       , @P26                                " _
            & "       , @P27                                " _
            & "       , @P28                                " _
            & "       , @P29                                " _
            & "       , @ACCOUNTINGMONTH                    " _
            & "       , @P30                                " _
            & "       , @P31                                " _
            & "       , @P32                                " _
            & "       , @P33                                " _
            & "       , @P34                                " _
            & "       , @P35                                " _
            & "       , @P36                                " _
            & "       , @P37                                " _
            & "       , @P38                                " _
            & "       , @P39                                " _
            & "       , @P40                                " _
            & "       , @P41                                " _
            & "       , @P42) ;                             " _
            & " CLOSE hensuu ;                              " _
            & " DEALLOCATE hensuu ;                         "

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                      " _
            & "     DELFLG                                  " _
            & "   , TORICODE                                " _
            & "   , INVFILINGDEPT                           " _
            & "   , INVKESAIKBN                             " _
            & "   , TORINAME                                " _
            & "   , TORINAMES                               " _
            & "   , TORINAMEKANA                            " _
            & "   , TORIDIVNAME                             " _
            & "   , TORICHARGE                              " _
            & "   , TORIKBN                                 " _
            & "   , POSTNUM1                                " _
            & "   , POSTNUM2                                " _
            & "   , ADDR1                                   " _
            & "   , ADDR2                                   " _
            & "   , ADDR3                                   " _
            & "   , ADDR4                                   " _
            & "   , TEL                                     " _
            & "   , FAX                                     " _
            & "   , MAIL                                    " _
            & "   , BANKCODE                                " _
            & "   , BANKBRANCHCODE                          " _
            & "   , ACCOUNTTYPE                             " _
            & "   , ACCOUNTNUMBER                           " _
            & "   , ACCOUNTNAME                             " _
            & "   , INACCOUNTCD                             " _
            & "   , TAXCALCULATION                          " _
            & "   , DEPOSITDAY                              " _
            & "   , DEPOSITMONTHKBN                         " _
            & "   , CLOSINGDAY                              " _
            & "   , ACCOUNTINGMONTH                         " _
            & "   , SLIPDESCRIPTION1                        " _
            & "   , SLIPDESCRIPTION2                        " _
            & "   , NEXTMONTHUNSETTLEDKBN                   " _
            & "   , INITYMD                                 " _
            & "   , INITUSER                                " _
            & "   , INITTERMID                              " _
            & "   , INITPGID                                " _
            & "   , UPDYMD                                  " _
            & "   , UPDUSER                                 " _
            & "   , UPDTERMID                               " _
            & "   , UPDPGID                                 " _
            & "   , RECEIVEYMD                              " _
            & "   , UPDTIMSTP                               " _
            & " FROM                                        " _
            & "     LNG.LNM0024_KEKKJM                      " _
            & " WHERE                                       " _
            & "             TORICODE              = @P01    " _
            & "         AND INVFILINGDEPT         = @P03    " _
            & "         AND INVKESAIKBN           = @P04    "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                ' DB更新用パラメータ
                Dim PARA001 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 10)        '取引先コード
                Dim PARA003 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 6)         '請求書提出部店
                Dim PARA004 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.Decimal, 2)          '請求書決済区分
                Dim PARA005 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar, 100)       '取引先名称
                Dim PARA006 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar, 50)        '取引先略称
                Dim PARA007 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar, 100)       '取引先カナ名称
                Dim PARA008 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.VarChar, 50)        '取引先部門名称
                Dim PARA009 As MySqlParameter = SQLcmd.Parameters.Add("@P09", MySqlDbType.VarChar, 20)        '取引先担当者
                Dim PARA010 As MySqlParameter = SQLcmd.Parameters.Add("@P10", MySqlDbType.VarChar, 1)         '取引先区分
                Dim PARA011 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.VarChar, 3)         '郵便番号（上）
                Dim PARA012 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.VarChar, 4)         '郵便番号（下）
                Dim PARA013 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar, 120)       '住所１
                Dim PARA014 As MySqlParameter = SQLcmd.Parameters.Add("@P14", MySqlDbType.VarChar, 120)       '住所２
                Dim PARA015 As MySqlParameter = SQLcmd.Parameters.Add("@P15", MySqlDbType.VarChar, 120)       '住所３
                Dim PARA016 As MySqlParameter = SQLcmd.Parameters.Add("@P16", MySqlDbType.VarChar, 120)       '住所４
                Dim PARA017 As MySqlParameter = SQLcmd.Parameters.Add("@P17", MySqlDbType.VarChar, 15)        '電話番号
                Dim PARA018 As MySqlParameter = SQLcmd.Parameters.Add("@P18", MySqlDbType.VarChar, 15)        'ＦＡＸ番号
                Dim PARA019 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.VarChar, 128)       'メールアドレス
                Dim PARA020 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.VarChar, 10)        '銀行コード
                Dim PARA021 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.VarChar, 10)        '支店コード
                Dim PARA022 As MySqlParameter = SQLcmd.Parameters.Add("@P22", MySqlDbType.VarChar, 1)         '口座種別
                Dim PARA023 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.VarChar, 10)        '口座番号
                Dim PARA024 As MySqlParameter = SQLcmd.Parameters.Add("@P24", MySqlDbType.VarChar, 30)        '口座名義
                Dim PARA025 As MySqlParameter = SQLcmd.Parameters.Add("@P25", MySqlDbType.VarChar, 4)         '社内口座コード
                Dim PARA026 As MySqlParameter = SQLcmd.Parameters.Add("@P26", MySqlDbType.Decimal, 1)          '税計算区分
                Dim PARA027 As MySqlParameter = SQLcmd.Parameters.Add("@P27", MySqlDbType.Decimal, 2)          '入金日
                Dim PARA028 As MySqlParameter = SQLcmd.Parameters.Add("@P28", MySqlDbType.VarChar, 1)         '入金月区分
                Dim PARA029 As MySqlParameter = SQLcmd.Parameters.Add("@P29", MySqlDbType.VarChar, 2)         '計上締日
                Dim P_ACCOUNTINGMONTH As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTINGMONTH", MySqlDbType.VarChar, 1) '計上月区分
                Dim PARA030 As MySqlParameter = SQLcmd.Parameters.Add("@P30", MySqlDbType.VarChar, 42)        '伝票摘要１
                Dim PARA031 As MySqlParameter = SQLcmd.Parameters.Add("@P31", MySqlDbType.VarChar, 42)        '伝票摘要２
                Dim PARA032 As MySqlParameter = SQLcmd.Parameters.Add("@P32", MySqlDbType.Decimal, 1)          '運賃翌月未決済区分
                Dim PARA033 As MySqlParameter = SQLcmd.Parameters.Add("@P33", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim PARA034 As MySqlParameter = SQLcmd.Parameters.Add("@P34", MySqlDbType.DateTime)            '登録年月日
                Dim PARA035 As MySqlParameter = SQLcmd.Parameters.Add("@P35", MySqlDbType.VarChar, 20)        '登録ユーザーＩＤ
                Dim PARA036 As MySqlParameter = SQLcmd.Parameters.Add("@P36", MySqlDbType.VarChar, 20)        '登録端末
                Dim PARA037 As MySqlParameter = SQLcmd.Parameters.Add("@P37", MySqlDbType.VarChar, 40)        '登録プログラムＩＤ
                Dim PARA038 As MySqlParameter = SQLcmd.Parameters.Add("@P38", MySqlDbType.DateTime)            '更新年月日
                Dim PARA039 As MySqlParameter = SQLcmd.Parameters.Add("@P39", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim PARA040 As MySqlParameter = SQLcmd.Parameters.Add("@P40", MySqlDbType.VarChar, 20)        '更新端末
                Dim PARA041 As MySqlParameter = SQLcmd.Parameters.Add("@P41", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ
                Dim PARA042 As MySqlParameter = SQLcmd.Parameters.Add("@P42", MySqlDbType.DateTime)            '集信日時


                ' 更新ジャーナル出力用パラメータ
                Dim JPARA001 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 10)    '取引先コード
                Dim JPARA003 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P03", MySqlDbType.VarChar, 6)     '請求書提出部店
                Dim JPARA004 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P04", MySqlDbType.Decimal, 2)      '請求書決済区分

                Dim LNM0024row As DataRow = LNM0024INPtbl.Rows(0)

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA001.Value = LNM0024row("TORICODE")                                                '取引先コード
                PARA003.Value = LNM0024row("INVFILINGDEPT")                                           '請求書提出部店
                PARA004.Value = LNM0024row("INVKESAIKBN")                                             '請求書決済区分

                If String.IsNullOrEmpty(LNM0024row("TORINAME")) Then
                    PARA005.Value = DBNull.Value                                                      '取引先名称
                Else
                    PARA005.Value = LNM0024row("TORINAME")                                            '取引先名称
                End If

                If String.IsNullOrEmpty(LNM0024row("TORINAMES")) Then
                    PARA006.Value = DBNull.Value                                                      '取引先略称
                Else
                    PARA006.Value = LNM0024row("TORINAMES")                                           '取引先略称
                End If

                If String.IsNullOrEmpty(LNM0024row("TORINAMEKANA")) Then
                    PARA007.Value = DBNull.Value                                                      '取引先カナ名称
                Else
                    PARA007.Value = LNM0024row("TORINAMEKANA")                                        '取引先カナ名称
                End If

                If String.IsNullOrEmpty(LNM0024row("TORIDIVNAME")) Then
                    PARA008.Value = DBNull.Value                                                      '取引先部門名称
                Else
                    PARA008.Value = LNM0024row("TORIDIVNAME")                                         '取引先部門名称
                End If

                If String.IsNullOrEmpty(LNM0024row("TORICHARGE")) Then
                    PARA009.Value = DBNull.Value                                                      '取引先担当者
                Else
                    PARA009.Value = LNM0024row("TORICHARGE")                                          '取引先担当者
                End If

                If String.IsNullOrEmpty(LNM0024row("TORIKBN")) Then
                    PARA010.Value = DBNull.Value                                                      '取引先区分
                Else
                    PARA010.Value = LNM0024row("TORIKBN")                                             '取引先区分
                End If

                If String.IsNullOrEmpty(LNM0024row("POSTNUM1")) Then
                    PARA011.Value = DBNull.Value                                                      '郵便番号（上）
                Else
                    PARA011.Value = LNM0024row("POSTNUM1")                                            '郵便番号（上）
                End If

                If String.IsNullOrEmpty(LNM0024row("POSTNUM2")) Then
                    PARA012.Value = DBNull.Value                                                      '郵便番号（下）
                Else
                    PARA012.Value = LNM0024row("POSTNUM2")                                            '郵便番号（下）
                End If

                If String.IsNullOrEmpty(LNM0024row("ADDR1")) Then
                    PARA013.Value = DBNull.Value                                                   '住所１
                Else
                    PARA013.Value = LNM0024row("ADDR1")                                            '住所１
                End If

                If String.IsNullOrEmpty(LNM0024row("ADDR2")) Then
                    PARA014.Value = DBNull.Value                                                   '住所２
                Else
                    PARA014.Value = LNM0024row("ADDR2")                                            '住所２
                End If

                If String.IsNullOrEmpty(LNM0024row("ADDR3")) Then
                    PARA015.Value = DBNull.Value                                                   '住所３
                Else
                    PARA015.Value = LNM0024row("ADDR3")                                            '住所３
                End If

                If String.IsNullOrEmpty(LNM0024row("ADDR4")) Then
                    PARA016.Value = DBNull.Value                                                   '住所４
                Else
                    PARA016.Value = LNM0024row("ADDR4")                                            '住所４
                End If

                If String.IsNullOrEmpty(LNM0024row("TEL")) Then
                    PARA017.Value = DBNull.Value                                                 '電話番号
                Else
                    PARA017.Value = LNM0024row("TEL")                                            '電話番号
                End If

                If String.IsNullOrEmpty(LNM0024row("FAX")) Then
                    PARA018.Value = DBNull.Value                                                 'ＦＡＸ番号
                Else
                    PARA018.Value = LNM0024row("FAX")                                            'ＦＡＸ番号
                End If

                If String.IsNullOrEmpty(LNM0024row("MAIL")) Then
                    PARA019.Value = DBNull.Value                                                 'メールアドレス
                Else
                    PARA019.Value = LNM0024row("MAIL")                                            'メールアドレス
                End If

                If String.IsNullOrEmpty(LNM0024row("BANKCODE")) Then
                    PARA020.Value = DBNull.Value                                                 '銀行コード
                Else
                    PARA020.Value = LNM0024row("BANKCODE")                                       '銀行コード
                End If

                If String.IsNullOrEmpty(LNM0024row("BANKBRANCHCODE")) Then
                    PARA021.Value = DBNull.Value                                                 '支店コード
                Else
                    PARA021.Value = LNM0024row("BANKBRANCHCODE")                                 '支店コード
                End If

                If String.IsNullOrEmpty(LNM0024row("ACCOUNTTYPE")) Then
                    PARA022.Value = DBNull.Value                                                 '口座種別
                Else
                    PARA022.Value = LNM0024row("ACCOUNTTYPE")                                    '口座種別
                End If

                If String.IsNullOrEmpty(LNM0024row("ACCOUNTNUMBER")) Then
                    PARA023.Value = DBNull.Value                                                 '口座番号
                Else
                    PARA023.Value = LNM0024row("ACCOUNTNUMBER")                                  '口座番号
                End If

                If String.IsNullOrEmpty(LNM0024row("ACCOUNTNAME")) Then
                    PARA024.Value = DBNull.Value                                                 '口座名義
                Else
                    PARA024.Value = LNM0024row("ACCOUNTNAME")                                    '口座名義
                End If

                If String.IsNullOrEmpty(LNM0024row("INACCOUNTCD")) Then
                    PARA025.Value = DBNull.Value                                                 '社内口座コード
                Else
                    PARA025.Value = LNM0024row("INACCOUNTCD").PadLeft(4, "0"c)                   '社内口座コード
                End If

                If String.IsNullOrEmpty(LNM0024row("TAXCALCULATION")) Then
                    PARA026.Value = DBNull.Value                                                 '税計算区分
                Else
                    PARA026.Value = LNM0024row("TAXCALCULATION")                                 '税計算区分
                End If

                If String.IsNullOrEmpty(LNM0024row("DEPOSITDAY")) Then
                    PARA027.Value = DBNull.Value                                                 '入金日
                Else
                    PARA027.Value = LNM0024row("DEPOSITDAY")                                     '入金日
                End If

                If String.IsNullOrEmpty(LNM0024row("DEPOSITMONTHKBN")) Then
                    PARA028.Value = DBNull.Value                                                 '入金月区分
                Else
                    PARA028.Value = LNM0024row("DEPOSITMONTHKBN")                                '入金月区分
                End If

                If String.IsNullOrEmpty(LNM0024row("CLOSINGDAY")) Then
                    PARA029.Value = DBNull.Value                                                 '計上締日
                Else
                    PARA029.Value = LNM0024row("CLOSINGDAY")                                     '計上締日
                End If

                If String.IsNullOrEmpty(LNM0024row("ACCOUNTINGMONTH")) Then
                    P_ACCOUNTINGMONTH.Value = DBNull.Value                                       '計上月区分
                Else
                    P_ACCOUNTINGMONTH.Value = LNM0024row("ACCOUNTINGMONTH")                      '計上月区分
                End If

                If String.IsNullOrEmpty(LNM0024row("SLIPDESCRIPTION1")) Then
                    PARA030.Value = DBNull.Value                                                 '伝票摘要１
                Else
                    PARA030.Value = LNM0024row("SLIPDESCRIPTION1")                               '伝票摘要１
                End If

                If String.IsNullOrEmpty(LNM0024row("SLIPDESCRIPTION2")) Then
                    PARA031.Value = DBNull.Value                                                 '伝票摘要２
                Else
                    PARA031.Value = LNM0024row("SLIPDESCRIPTION2")                               '伝票摘要２
                End If

                If String.IsNullOrEmpty(LNM0024row("NEXTMONTHUNSETTLEDKBN")) Then
                    PARA032.Value = "0"                                                          '運賃翌月未決済区分
                Else
                    PARA032.Value = LNM0024row("NEXTMONTHUNSETTLEDKBN")                          '運賃翌月未決済区分
                End If

                PARA033.Value = LNM0024row("DELFLG")                                             '削除フラグ                   
                PARA034.Value = WW_DateNow                                                       '登録年月日                   
                PARA035.Value = Master.USERID                                                    '登録ユーザーＩＤ             
                PARA036.Value = Master.USERTERMID                                                '登録端末                     
                PARA037.Value = Me.GetType().BaseType.Name                                       '登録プログラムＩＤ           
                PARA038.Value = WW_DateNow                                                       '更新年月日                   
                PARA039.Value = Master.USERID                                                    '更新ユーザーＩＤ            
                PARA040.Value = Master.USERTERMID                                                '更新端末                    
                PARA041.Value = Me.GetType().BaseType.Name                                       '更新プログラムＩＤ          
                PARA042.Value = C_DEFAULT_YMD                                                    '集信日時                     
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA001.Value = LNM0024row("TORICODE")
                JPARA003.Value = LNM0024row("INVFILINGDEPT")
                JPARA004.Value = LNM0024row("INVKESAIKBN")


                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0024UPDtbl) Then
                        LNM0024UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0024UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0024UPDtbl.Clear()
                    LNM0024UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0024UPDrow As DataRow In LNM0024UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0024D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0024UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0024D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0024D UPDATE_INSERT"
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
    Protected Sub KEKKJMEXISTS(ByVal SQLcon As MySqlConnection, ByRef WW_MODIFYKBN As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '営業収入決済条件マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0024_KEKKJM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        TORICODE         = @TORICODE")
        SQLStr.AppendLine("    AND INVFILINGDEPT    = @INVFILINGDEPT")
        SQLStr.AppendLine("    AND INVKESAIKBN      = @INVKESAIKBN")
        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '取引先コード
                Dim P_INVFILINGDEPT As MySqlParameter = SQLcmd.Parameters.Add("@INVFILINGDEPT", MySqlDbType.VarChar, 6)         '請求書提出部店
                Dim P_INVKESAIKBN As MySqlParameter = SQLcmd.Parameters.Add("@INVKESAIKBN", MySqlDbType.VarChar, 2)         '請求書決済区分

                Dim LNM0024row As DataRow = LNM0024INPtbl.Rows(0)

                P_TORICODE.Value = LNM0024row("TORICODE")               '取引先コード
                P_INVFILINGDEPT.Value = LNM0024row("INVFILINGDEPT")             '請求書提出部店
                P_INVKESAIKBN.Value = LNM0024row("INVKESAIKBN")       '請求書決済区分


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
                        WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0024C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0024C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0097_KEKKJHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         TORICODE  ")
        SQLStr.AppendLine("        ,INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,INVKESAIKBN  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORINAMES  ")
        SQLStr.AppendLine("        ,TORINAMEKANA  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,TORICHARGE  ")
        SQLStr.AppendLine("        ,TORIKBN  ")
        SQLStr.AppendLine("        ,POSTNUM1  ")
        SQLStr.AppendLine("        ,POSTNUM2  ")
        SQLStr.AppendLine("        ,ADDR1  ")
        SQLStr.AppendLine("        ,ADDR2  ")
        SQLStr.AppendLine("        ,ADDR3  ")
        SQLStr.AppendLine("        ,ADDR4  ")
        SQLStr.AppendLine("        ,TEL  ")
        SQLStr.AppendLine("        ,FAX  ")
        SQLStr.AppendLine("        ,MAIL  ")
        SQLStr.AppendLine("        ,BANKCODE  ")
        SQLStr.AppendLine("        ,BANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,ACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,ACCOUNTNUMBER  ")
        SQLStr.AppendLine("        ,ACCOUNTNAME  ")
        SQLStr.AppendLine("        ,INACCOUNTCD  ")
        SQLStr.AppendLine("        ,TAXCALCULATION  ")
        SQLStr.AppendLine("        ,ACCOUNTINGMONTH  ")
        SQLStr.AppendLine("        ,CLOSINGDAY  ")
        SQLStr.AppendLine("        ,DEPOSITDAY  ")
        SQLStr.AppendLine("        ,DEPOSITMONTHKBN  ")
        SQLStr.AppendLine("        ,SLIPDESCRIPTION1  ")
        SQLStr.AppendLine("        ,SLIPDESCRIPTION2  ")
        SQLStr.AppendLine("        ,NEXTMONTHUNSETTLEDKBN  ")
        SQLStr.AppendLine("        ,BEFOREINVFILINGDEPT  ")
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
        SQLStr.AppendLine("        ,INVFILINGDEPT  ")
        SQLStr.AppendLine("        ,INVKESAIKBN  ")
        SQLStr.AppendLine("        ,TORINAME  ")
        SQLStr.AppendLine("        ,TORINAMES  ")
        SQLStr.AppendLine("        ,TORINAMEKANA  ")
        SQLStr.AppendLine("        ,TORIDIVNAME  ")
        SQLStr.AppendLine("        ,TORICHARGE  ")
        SQLStr.AppendLine("        ,TORIKBN  ")
        SQLStr.AppendLine("        ,POSTNUM1  ")
        SQLStr.AppendLine("        ,POSTNUM2  ")
        SQLStr.AppendLine("        ,ADDR1  ")
        SQLStr.AppendLine("        ,ADDR2  ")
        SQLStr.AppendLine("        ,ADDR3  ")
        SQLStr.AppendLine("        ,ADDR4  ")
        SQLStr.AppendLine("        ,TEL  ")
        SQLStr.AppendLine("        ,FAX  ")
        SQLStr.AppendLine("        ,MAIL  ")
        SQLStr.AppendLine("        ,BANKCODE  ")
        SQLStr.AppendLine("        ,BANKBRANCHCODE  ")
        SQLStr.AppendLine("        ,ACCOUNTTYPE  ")
        SQLStr.AppendLine("        ,ACCOUNTNUMBER  ")
        SQLStr.AppendLine("        ,ACCOUNTNAME  ")
        SQLStr.AppendLine("        ,INACCOUNTCD  ")
        SQLStr.AppendLine("        ,TAXCALCULATION  ")
        SQLStr.AppendLine("        ,ACCOUNTINGMONTH  ")
        SQLStr.AppendLine("        ,CLOSINGDAY  ")
        SQLStr.AppendLine("        ,DEPOSITDAY  ")
        SQLStr.AppendLine("        ,DEPOSITMONTHKBN  ")
        SQLStr.AppendLine("        ,SLIPDESCRIPTION1  ")
        SQLStr.AppendLine("        ,SLIPDESCRIPTION2  ")
        SQLStr.AppendLine("        ,NEXTMONTHUNSETTLEDKBN  ")
        SQLStr.AppendLine("        ,BEFOREINVFILINGDEPT  ")
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
        SQLStr.AppendLine("        LNG.LNM0024_KEKKJM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        TORICODE         = @TORICODE")
        SQLStr.AppendLine("    AND INVFILINGDEPT    = @INVFILINGDEPT")
        SQLStr.AppendLine("    AND INVKESAIKBN      = @INVKESAIKBN")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)         '取引先コード
                Dim P_INVFILINGDEPT As MySqlParameter = SQLcmd.Parameters.Add("@INVFILINGDEPT", MySqlDbType.VarChar, 6)         '請求書提出部店
                Dim P_INVKESAIKBN As MySqlParameter = SQLcmd.Parameters.Add("@INVKESAIKBN", MySqlDbType.VarChar, 2)         '請求書決済区分

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0024row As DataRow = LNM0024INPtbl.Rows(0)

                ' DB更新
                P_TORICODE.Value = LNM0024row("TORICODE")               '取引先コード
                P_INVFILINGDEPT.Value = LNM0024row("INVFILINGDEPT")             '請求書提出部店
                P_INVKESAIKBN.Value = LNM0024row("INVKESAIKBN")       '請求書決済区分

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0024WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0024tbl.Rows(0)("DELFLG") = "0" And LNM0024row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0024WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0024WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0097_KEKKJHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0097_KEKKJHIST  INSERT"
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
        DetailBoxToLNM0024INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0024tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0024tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        ' 右BOXクローズ
        WF_RightboxOpen.Value = ""
        If WW_ErrSW = "" Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ErrSW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            ElseIf WW_ErrSW = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR Then
                ' 一意制約エラー
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, "", needsPopUp:=True)
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
    Protected Sub DetailBoxToLNM0024INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(txtDelFlg.Text)                      '削除フラグ
        Master.EraseCharToIgnore(txtToriCode.Text)                    '取引先コード
        Master.EraseCharToIgnore(txtInvFilingDept.Text)               '請求書提出部店
        Master.EraseCharToIgnore(txtInvKesaiKbn.Text)                 '請求書決済区分
        Master.EraseCharToIgnore(txtToriName.Text)                    '取引先名称
        Master.EraseCharToIgnore(txtToriNameS.Text)                   '取引先略称
        Master.EraseCharToIgnore(txtToriNameKana.Text)                '取引先カナ名称
        Master.EraseCharToIgnore(txtToriDivName.Text)                 '取引先部門名称
        Master.EraseCharToIgnore(txtToriCharge.Text)                  '取引先担当者
        Master.EraseCharToIgnore(txtToriKbn.Text)                     '取引先区分
        Master.EraseCharToIgnore(txtPostNum1.Text)                    '郵便番号（上）
        Master.EraseCharToIgnore(txtPostNum2.Text)                    '郵便番号（下）
        Master.EraseCharToIgnore(txtAddr1.Text)                       '住所１
        Master.EraseCharToIgnore(txtAddr2.Text)                       '住所２
        Master.EraseCharToIgnore(txtAddr3.Text)                       '住所３
        Master.EraseCharToIgnore(txtAddr4.Text)                       '住所４
        Master.EraseCharToIgnore(txtTel.Text)                         '電話番号
        Master.EraseCharToIgnore(txtFax.Text)                         'ＦＡＸ番号
        Master.EraseCharToIgnore(txtMail.Text)                        'メールアドレス
        Master.EraseCharToIgnore(txtBankCode.Text)                    '銀行コード
        Master.EraseCharToIgnore(txtBankBranchCode.Text)              '支店コード
        Master.EraseCharToIgnore(txtAccountType.Text)                 '口座種別
        Master.EraseCharToIgnore(txtAccountNumber.Text)               '口座番号
        Master.EraseCharToIgnore(txtAccountName.Text)                 '口座名義
        Master.EraseCharToIgnore(txtInAccountCd.Text)                 '社内口座コード
        Master.EraseCharToIgnore(txtTaxcalculation.Text)              '税計算区分
        Master.EraseCharToIgnore(txtDepositDay.Text)                  '入金日
        Master.EraseCharToIgnore(txtDepositMonthKbn.Text)             '入金月区分
        Master.EraseCharToIgnore(txtClosingday.Text)                  '計上締日
        Master.EraseCharToIgnore(txtAccountingMonth.Text)             '計上月区分
        Master.EraseCharToIgnore(txtSlipDescription1.Text)            '伝票摘要１
        Master.EraseCharToIgnore(txtSlipDescription2.Text)            '伝票摘要２
        Master.EraseCharToIgnore(txtNextMonthUnSettledKbn.Text)       '運賃翌月未決済区分


        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(lblSelLineCNT.Text) AndAlso
            String.IsNullOrEmpty(txtDelFlg.Text) Then
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

        Master.CreateEmptyTable(LNM0024INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0024INProw As DataRow = LNM0024INPtbl.NewRow

        'LINECNT
        If lblSelLineCNT.Text = "" Then
            LNM0024INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(lblSelLineCNT.Text, LNM0024INProw("LINECNT"))
            Catch ex As Exception
                LNM0024INProw("LINECNT") = 0
            End Try
        End If

        LNM0024INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0024INProw("UPDTIMSTP") = 0
        LNM0024INProw("SELECT") = 1
        LNM0024INProw("HIDDEN") = 0

        LNM0024INProw("TORICODE") = txtToriCode.Text                                '取引先コード
        LNM0024INProw("INVFILINGDEPT") = txtInvFilingDept.Text                      '請求書提出部店
        LNM0024INProw("INVKESAIKBN") = txtInvKesaiKbn.Text                          '請求書決済区分
        LNM0024INProw("TORINAME") = txtToriName.Text                                '取引先名称
        LNM0024INProw("TORINAMES") = txtToriNameS.Text                              '取引先略称
        LNM0024INProw("TORINAMEKANA") = txtToriNameKana.Text                        '取引先カナ名称
        LNM0024INProw("TORIDIVNAME") = txtToriDivName.Text                          '取引先部門名称
        LNM0024INProw("TORICHARGE") = txtToriCharge.Text                            '取引先担当者
        LNM0024INProw("TORIKBN") = txtToriKbn.Text                                  '取引先区分
        LNM0024INProw("POSTNUM1") = txtPostNum1.Text                                '郵便番号（上）
        LNM0024INProw("POSTNUM2") = txtPostNum2.Text                                '郵便番号（下）
        LNM0024INProw("ADDR1") = txtAddr1.Text                                      '住所１
        LNM0024INProw("ADDR2") = txtAddr2.Text                                      '住所２
        LNM0024INProw("ADDR3") = txtAddr3.Text                                      '住所３
        LNM0024INProw("ADDR4") = txtAddr4.Text                                      '住所４
        LNM0024INProw("TEL") = txtTel.Text                                          '電話番号
        LNM0024INProw("FAX") = txtFax.Text                                          'ＦＡＸ番号
        LNM0024INProw("MAIL") = txtMail.Text                                        'メールアドレス
        LNM0024INProw("BANKCODE") = txtBankCode.Text                                '銀行コード
        LNM0024INProw("BANKBRANCHCODE") = txtBankBranchCode.Text                    '支店コード
        LNM0024INProw("ACCOUNTTYPE") = txtAccountType.Text                          '口座種別
        LNM0024INProw("ACCOUNTNUMBER") = txtAccountNumber.Text                      '口座番号
        LNM0024INProw("ACCOUNTNAME") = txtAccountName.Text                          '口座名義
        LNM0024INProw("INACCOUNTCD") = txtInAccountCd.Text                          '社内口座コード
        LNM0024INProw("TAXCALCULATION") = txtTaxcalculation.Text                    '税計算区分
        LNM0024INProw("DEPOSITDAY") = txtDepositDay.Text                            '入金日
        LNM0024INProw("DEPOSITMONTHKBN") = txtDepositMonthKbn.Text                  '入金月区分
        LNM0024INProw("CLOSINGDAY") = txtClosingday.Text                            '計上締日
        LNM0024INProw("ACCOUNTINGMONTH") = txtAccountingMonth.Text                  '計上月区分
        LNM0024INProw("SLIPDESCRIPTION1") = txtSlipDescription1.Text                '伝票摘要１
        LNM0024INProw("SLIPDESCRIPTION2") = txtSlipDescription2.Text                '伝票摘要２
        LNM0024INProw("NEXTMONTHUNSETTLEDKBN") = txtNextMonthUnSettledKbn.Text      '運賃翌月未決済区分

        LNM0024INProw("DELFLG") = txtDelFlg.Text                                    '削除フラグ
        LNM0024INProw("UPDYMD") = Date.Now                                          '更新日付

        '○ チェック用テーブルに登録する
        LNM0024INPtbl.Rows.Add(LNM0024INProw)

    End Sub
    ''' <summary>
    ''' 詳細画面-更新ボタン押下時処理
    ''' </summary>
    Protected Sub WF_UPDATE_Click()

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '警告レベルチェックStart

        '警告レベルチェックEnd

        If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
            ' エラーの場合は、確認ダイアログを表示し警告を表示
            Master.Output(C_MESSAGE_NO.CTN_KOBANCYCLE_ERR, C_MESSAGE_TYPE.WAR, I_PARA02:="W",
                    needsPopUp:=True, messageBoxTitle:="警告", IsConfirm:=True, YesButtonId:="btnUpdateConfirmOK")
        Else
            ' エラーではない場合は、確認ダイアログを表示せずに更新処理を実行
            WF_UPDATE_ConfirmOkClick()
        End If
    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0024INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0024INProw As DataRow = LNM0024INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0024row As DataRow In LNM0024tbl.Rows
            ' KEY項目が等しい時
            If LNM0024row("TORICODE") = LNM0024INProw("TORICODE") AndAlso
                LNM0024row("INVFILINGDEPT") = LNM0024INProw("INVFILINGDEPT") AndAlso
                LNM0024row("INVKESAIKBN") = LNM0024INProw("INVKESAIKBN") Then
                ' KEY項目以外の項目の差異をチェック
                If LNM0024row("TORINAME") = LNM0024INProw("TORINAME") AndAlso
                    LNM0024row("TORINAMES") = LNM0024INProw("TORINAMES") AndAlso
                    LNM0024row("TORINAMEKANA") = LNM0024INProw("TORINAMEKANA") AndAlso
                    LNM0024row("TORIDIVNAME") = LNM0024INProw("TORIDIVNAME") AndAlso
                    LNM0024row("TORICHARGE") = LNM0024INProw("TORICHARGE") AndAlso
                    LNM0024row("TORIKBN") = LNM0024INProw("TORIKBN") AndAlso
                    LNM0024row("POSTNUM1") = LNM0024INProw("POSTNUM1") AndAlso
                    LNM0024row("POSTNUM2") = LNM0024INProw("POSTNUM2") AndAlso
                    LNM0024row("ADDR1") = LNM0024INProw("ADDR1") AndAlso
                    LNM0024row("ADDR2") = LNM0024INProw("ADDR2") AndAlso
                    LNM0024row("ADDR3") = LNM0024INProw("ADDR3") AndAlso
                    LNM0024row("ADDR4") = LNM0024INProw("ADDR4") AndAlso
                    LNM0024row("TEL") = LNM0024INProw("TEL") AndAlso
                    LNM0024row("FAX") = LNM0024INProw("FAX") AndAlso
                    LNM0024row("MAIL") = LNM0024INProw("MAIL") AndAlso
                    LNM0024row("BANKCODE") = LNM0024INProw("BANKCODE") AndAlso
                    LNM0024row("BANKBRANCHCODE") = LNM0024INProw("BANKBRANCHCODE") AndAlso
                    LNM0024row("ACCOUNTTYPE") = LNM0024INProw("ACCOUNTTYPE") AndAlso
                    LNM0024row("ACCOUNTNUMBER") = LNM0024INProw("ACCOUNTNUMBER") AndAlso
                    LNM0024row("ACCOUNTNAME") = LNM0024INProw("ACCOUNTNAME") AndAlso
                    LNM0024row("INACCOUNTCD") = LNM0024INProw("INACCOUNTCD") AndAlso
                    LNM0024row("TAXCALCULATION") = LNM0024INProw("TAXCALCULATION") AndAlso
                    LNM0024row("DEPOSITDAY") = LNM0024INProw("DEPOSITDAY") AndAlso
                    LNM0024row("DEPOSITMONTHKBN") = LNM0024INProw("DEPOSITMONTHKBN") AndAlso
                    LNM0024row("CLOSINGDAY") = LNM0024INProw("CLOSINGDAY") AndAlso
                    LNM0024row("ACCOUNTINGMONTH") = LNM0024INProw("ACCOUNTINGMONTH") AndAlso
                    LNM0024row("SLIPDESCRIPTION1") = LNM0024INProw("SLIPDESCRIPTION1") AndAlso
                    LNM0024row("SLIPDESCRIPTION2") = LNM0024INProw("SLIPDESCRIPTION2") AndAlso
                    LNM0024row("NEXTMONTHUNSETTLEDKBN") = LNM0024INProw("NEXTMONTHUNSETTLEDKBN") AndAlso
                    LNM0024row("DELFLG") = LNM0024INProw("DELFLG") Then
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
        For Each LNM0024row As DataRow In LNM0024tbl.Rows
            Select Case LNM0024row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0024tbl, work.WF_SEL_INPTBL.Text)

        lblSelLineCNT.Text = ""               'LINECNT
        txtMapId.Text = "M00001"              '画面ＩＤ

        txtToriCode.Text = ""                 '取引先コード
        txtInvFilingDept.Text = ""            '請求書提出部店
        txtInvKesaiKbn.Text = ""              '請求書決済区分
        txtToriName.Text = ""                 '取引先名称
        txtToriNameS.Text = ""                '取引先略称
        txtToriNameKana.Text = ""             '取引先カナ名称
        txtToriDivName.Text = ""              '取引先部門名称
        txtToriCharge.Text = ""               '取引先担当者
        txtToriKbn.Text = ""                  '取引先区分
        txtPostNum1.Text = ""                 '郵便番号（上）
        txtPostNum2.Text = ""                 '郵便番号（下）
        txtAddr1.Text = ""                    '住所１
        txtAddr2.Text = ""                    '住所２
        txtAddr3.Text = ""                    '住所３
        txtAddr4.Text = ""                    '住所４
        txtTel.Text = ""                      '電話番号
        txtFax.Text = ""                      'ＦＡＸ番号
        txtMail.Text = ""                     'メールアドレス
        txtBankCode.Text = ""                 '銀行コード
        txtBankBranchCode.Text = ""           '支店コード
        txtAccountType.Text = ""              '口座種別
        txtAccountNumber.Text = ""            '口座番号
        txtAccountName.Text = ""              '口座名義
        txtInAccountCd.Text = ""              '社内口座コード
        txtTaxcalculation.Text = ""           '税計算区分
        txtDepositDay.Text = ""               '入金日
        txtDepositMonthKbn.Text = ""          '入金月
        txtClosingday.Text = ""               '計上締日
        txtAccountingMonth.Text = ""          '計上月区分
        txtSlipDescription1.Text = ""         '伝票摘要１
        txtSlipDescription2.Text = ""         '伝票摘要２
        txtNextMonthUnSettledKbn.Text = ""    '運賃翌月未決済区分

        txtDelFlg.Text = ""                   '削除フラグ

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        Dim WW_PrmData As New Hashtable
        Dim WW_AuthorityAllFlg As String = "0"

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                leftview.Visible = True
                ' フィールドによってパラメータを変える
                Select Case WF_FIELD.Value
                    'Case "txtToriCode"               '取引先コード
                    '    WW_PrmData = work.CreateKekkjmParam(GL0018InvKesaiKbnList.LS_INVOICE_WITH.TORICODE)
                    Case "txtToriCode"        '
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspToriSingle(txtInvFilingDept.Text)
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    Case "txtInvFilingDept"          '請求書提出部店
                        WW_PrmData = work.CreateUORGParam(Master.USERCAMP)
                    Case "txtInvKesaiKbn"            '運賃翌月未決済区分
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "NEXTMONTHUNSETTLEDKB")
                    Case "txtAccountType"            '当座種別
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTTYPE")
                    Case "txtDepositMonthKbn"        '入金月区分
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "DEPOSITMONTHKBN")
                    Case "txtAccountingMonth"        '計上月区分
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "KEIJOMKBN")
                    Case "txtDelFlg"                 '削除フラグ
                        WW_PrmData.Item(C_PARAMETERS.LP_COMPANY) = Master.USERCAMP
                        WW_PrmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
                End Select

                .SetListBox(WF_LeftMViewChange.Value, WW_Dummy, WW_PrmData)
                .ActiveListBox()
            End With
        End If

    End Sub

    ''' <summary>
    ''' 取引先検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspToriSingle(Optional ByVal prmKey As String = "")

        Me.mspToriSingle.InitPopUp()
        Me.mspToriSingle.SelectionMode = ListSelectionMode.Single
        Me.mspToriSingle.SQL = CmnSearchSQL.GetToriSQL

        Me.mspToriSingle.KeyFieldName = "KEYCODE"
        Me.mspToriSingle.DispFieldList.AddRange(CmnSearchSQL.GetToriTitle)

        '画面表示する絞り込みドロップダウンの設定(組織コード)
        Me.mspToriSingle.FilterField.Add("ORGNAMES", "提出部店")

        Me.mspToriSingle.ShowPopUpList(prmKey)

        '組織名取得
        Dim orgName = Master.USER_ORGNAME
        Me.mspToriSingle.ddlFilterInit("", orgName)

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_Change()

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            Case "txtDelFlg"                '削除フラグ
                CODENAME_get("DELFLG", txtDelFlg.Text, lblDelFlgName.Text, WW_Dummy)
                txtDelFlg.Focus()
            Case "txtToriCode"              '取引先コード
                CODENAME_get("TORICODE", txtToriCode.Text, lblToriCodeName.Text, WW_Dummy)
                txtToriCode.Focus()
            Case "txtInvFilingDept"         '請求書提出部店
                CODENAME_get("INVFILINGDEPT", txtInvFilingDept.Text, lblInvFilingDeptName.Text, WW_Dummy)
                txtInvFilingDept.Focus()
            Case "txtAccountType"           '口座種別
                CODENAME_get("ACCOUNTTYPE", txtAccountType.Text, lblAccountType.Text, WW_Dummy)
                txtAccountType.Focus()
            Case "txtDepositMonthKbn"       '入金月区分
                CODENAME_get("DEPOSITMONTHKBN", txtDepositMonthKbn.Text, lblDepositMonthKbnName.Text, WW_Dummy)
                txtDepositMonthKbn.Focus()
            Case "txtAccountingMonth"       '計上月区分
                CODENAME_get("KEIJOMKBN", txtAccountingMonth.Text, lblAccountingMonthName.Text, WW_Dummy)
                txtAccountingMonth.Focus()
            Case "txtNextMonthUnSettledKbn" '運賃翌月未決済区分
                CODENAME_get("NEXTMONTHUNSETTLEDKB", txtNextMonthUnSettledKbn.Text, lblNextMonthUnSettledKbnName.Text, WW_Dummy)
                txtNextMonthUnSettledKbn.Focus()
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
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case "txtDelFlg"                                    '削除フラグ
                    txtDelFlg.Text = WW_SelectValue
                    lblDelFlgName.Text = WW_SelectText
                    txtDelFlg.Focus()
                Case "txtToriCode"                                  '取引先コード
                    txtToriCode.Text = WW_SelectValue
                    lblToriCodeName.Text = WW_SelectText
                    txtToriCode.Focus()
                Case "txtInvFilingDept"                             '請求書提出部店
                    txtInvFilingDept.Text = WW_SelectValue
                    lblInvFilingDeptName.Text = WW_SelectText
                    txtInvFilingDept.Focus()
                Case "txtAccountType"                               '口座種別
                    txtAccountType.Text = WW_SelectValue
                    lblAccountType.Text = WW_SelectText
                    txtAccountType.Focus()
                Case "txtDepositMonthKbn"                           '入金月区分
                    txtDepositMonthKbn.Text = WW_SelectValue
                    lblDepositMonthKbnName.Text = WW_SelectText
                    txtDepositMonthKbn.Focus()
                Case "txtAccountingMonth"                           '計上月区分
                    txtAccountingMonth.Text = WW_SelectValue
                    lblAccountingMonthName.Text = WW_SelectText
                    txtAccountingMonth.Focus()
                Case "txtNextMonthUnSettledKbn"                     '運賃翌月未決済区分
                    txtNextMonthUnSettledKbn.Text = WW_SelectValue
                    lblNextMonthUnSettledKbnName.Text = WW_SelectText
                    lblNextMonthUnSettledKbnName.Focus()
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

        ' ○ フォーカスセット
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case "txtDelFlg"                       '削除フラグ
                    txtDelFlg.Focus()
                Case "txtToriCode"                     '取引先コード
                    txtToriCode.Focus()
                Case "txtInvFilingDept"                '請求書提出部店
                    txtInvFilingDept.Focus()
                Case "txtAccountType"                  '口座種別
                    txtAccountType.Focus()
                Case "txtDepositMonthKbn"                  '入金月区分
                    txtDepositMonthKbn.Focus()
                Case "txtAccountingMonth"                  '計上月区分
                    txtAccountingMonth.Focus()
                Case "txtNextMonthUnSettledKbn"        '運賃翌月未決済区分
                    txtNextMonthUnSettledKbn.Focus()
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
    ''' 取引先選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspToriSingle()

        Dim selData = Me.mspToriSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case txtToriCode.ID
                Me.txtToriCode.Text = selData("TORICODE").ToString
                Me.lblToriCodeName.Text = selData("TORINAME").ToString & selData("DIVNAME").ToString
                Me.txtToriCode.Focus()

                'Case TxtDepStationName.ID
                '    Me.TxtDepStationCode.Text = selData("STATION").ToString
                '    Me.TxtDepStationName.Text = selData("NAMES").ToString
                '    Me.TxtDepStationName.Focus()

                'Case TxtArrStationName.ID
                '    Me.TxtArrStationCode.Text = selData("STATION").ToString
                '    Me.TxtArrStationName.Text = selData("NAMES").ToString
                '    Me.TxtArrStationName.Focus()
        End Select

        'ポップアップの非表示
        Me.mspToriSingle.HidePopUp()

    End Sub

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
        Dim WW_ConstructionYMD As String = ""

        '○ 画面操作権限チェック
        ' 権限チェック(操作者に更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            WW_CheckMES1 = "・コンテナ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If


        For Each LNM0024INProw As DataRow In LNM0024INPtbl.Rows
            '○ 単項目チェック

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0024INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 値存在チェック
                CODENAME_get("DELFLG", LNM0024INProw("DELFLG"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・削除コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・削除コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 取引先コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORICODE", LNM0024INProw("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                '***2023/12/14 コンテナ決済マスタ(LNM0003_REKEJM)側でも同様のチェック処理をしており互いに登録できなくなるためこちらのチェックを外す
                '' 値存在チェック
                'CODENAME_get("TORICODE", LNM0024INProw("TORICODE"), WW_Dummy, WW_RtnSW)
                'If Not isNormal(WW_RtnSW) Then
                '    WW_CheckMES1 = "・得意先コードエラーです。"
                '    WW_CheckMES2 = "マスタに存在しません。"
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                '    WW_LineErr = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
            Else
                WW_CheckMES1 = "・得意先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 請求書提出部店(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "INVFILINGDEPT", LNM0024INProw("INVFILINGDEPT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 値存在チェック
                CODENAME_get("INVFILINGDEPT", LNM0024INProw("INVFILINGDEPT"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・請求書提出部店エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・請求書提出部店エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If


            ' 請求書決済区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "INVKESAIKBN", LNM0024INProw("INVKESAIKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                ' 値存在チェック
                'CODENAME_get("INVKESAIKBN", LNM0024INProw("INVKESAIKBN"), WW_Dummy, WW_RtnSW)
                'If Not isNormal(WW_RtnSW) Then
                'WW_CheckMES1 = "・請求書決済区分エラーです。"
                'WW_CheckMES2 = "マスタに存在しません。"
                'WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                'WW_LineErr = "ERR"
                'O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_CheckMES1 = "・請求書決済区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'End If
            End If

            ' 取引先名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORINAME", LNM0024INProw("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 取引先略称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORINAMES", LNM0024INProw("TORINAMES"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先略称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 取引先カナ名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORINAMEKANA", LNM0024INProw("TORINAMEKANA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先カナ名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 取引先部門名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORIDIVNAME", LNM0024INProw("TORIDIVNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先部門名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 取引先担当者(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORICHARGE", LNM0024INProw("TORICHARGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先担当者エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 取引先区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORIKBN", LNM0024INProw("TORIKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 郵便番号（上）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "POSTNUM1", LNM0024INProw("POSTNUM1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・郵便番号（上）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 郵便番号（下）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "POSTNUM2", LNM0024INProw("POSTNUM2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・郵便番号（下）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 住所１(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDR1", LNM0024INProw("ADDR1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・住所１エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 住所２(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDR2", LNM0024INProw("ADDR2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・住所２エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 住所３(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDR3", LNM0024INProw("ADDR3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・住所３エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 住所４(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDR4", LNM0024INProw("ADDR4"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・住所４エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 電話番号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TEL", LNM0024INProw("TEL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・電話番号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' ＦＡＸ番号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "FAX", LNM0024INProw("FAX"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・ＦＡＸ番号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' メールアドレス(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "MAIL", LNM0024INProw("MAIL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・メールアドレスエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 銀行コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BANKCODE", LNM0024INProw("BANKCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・銀行コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 支店コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BANKBRANCHCODE", LNM0024INProw("BANKBRANCHCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・支店コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 口座種別(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ACCOUNTTYPE", LNM0024INProw("ACCOUNTTYPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・口座種別エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 口座番号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ACCOUNTNUMBER", LNM0024INProw("ACCOUNTNUMBER"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・口座番号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 口座名義(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ACCOUNTNAME", LNM0024INProw("ACCOUNTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・口座名義エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 社内口座コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "INACCOUNTCD", LNM0024INProw("INACCOUNTCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・社内口座コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 入金日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPOSITDAY", LNM0024INProw("DEPOSITDAY"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・入金日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 入金月区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPOSITMONTHKBN", LNM0024INProw("DEPOSITMONTHKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・入金月区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 計上月区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ACCOUNTINGMONTH", LNM0024INProw("ACCOUNTINGMONTH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・計上月区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 伝票摘要１(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLIPDESCRIPTION1", LNM0024INProw("SLIPDESCRIPTION1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・伝票摘要１エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 伝票摘要２(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLIPDESCRIPTION2", LNM0024INProw("SLIPDESCRIPTION2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・伝票摘要２エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 運賃翌月未決済区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "NEXTMONTHUNSETTLEDKBN", LNM0024INProw("NEXTMONTHUNSETTLEDKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・運賃翌月未決済区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If




            ' 排他チェック
            If Not work.WF_SEL_TORICODE2.Text = "" Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                    work.WF_SEL_TORICODE2.Text, work.WF_SEL_INVFILINGDEPT2.Text,
                                    work.WF_SEL_INVKESAIKBN2.Text, work.WF_SEL_UPDTIMSTP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（取引先コード & 請求書提出部店 & 請求書決済区分）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0024INProw("TORICODE") & "]" &
                                           " [" & LNM0024INProw("INVFILINGDEPT") & "]" &
                                           " [" & LNM0024INProw("INVKESAIKBN") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNM0024INProw("TORICODE") = work.WF_SEL_TORICODE2.Text OrElse
               Not LNM0024INProw("INVFILINGDEPT") = work.WF_SEL_INVFILINGDEPT2.Text OrElse
               Not LNM0024INProw("INVKESAIKBN") = work.WF_SEL_INVKESAIKBN2.Text Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（取引先コード & 請求書提出部店 & 請求書決済区分）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                           "([" & LNM0024INProw("TORICODE") & "]" &
                                           " [" & LNM0024INProw("INVFILINGDEPT") & "]" &
                                           " [" & LNM0024INProw("INVKESAIKBN") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LineErr = "" Then
                If LNM0024INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0024INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0024INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0024INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' パスワード有効期限チェック
    ''' </summary>
    ''' <param name="PassEndDate"></param>
    ''' <param name="NowDate"></param>
    ''' <param name="WW_StyDateFlag"></param>
    ''' <param name="WW_NewPassEndDate"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckPassEndDate(ByVal PassEndDate As DateTime, ByVal NowDate As DateTime, ByRef WW_StyDateFlag As String, ByRef WW_NewPassEndDate As String)

        WW_StyDateFlag = "1"

        NowDate = NowDate.AddDays(ADDDATE)

        WW_NewPassEndDate = NowDate

        If Not PassEndDate.ToString = "" Then
            If NowDate <= PassEndDate Then
                WW_StyDateFlag = "0"
            End If
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
        If MESSAGE2 <> "" Then
            WW_ErrMes &= vbCr & "   -->" & MESSAGE2
        End If

        rightview.AddErrorReport(WW_ErrMes)

    End Sub

    ''' <summary>
    ''' LNM0024tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0024tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0024row As DataRow In LNM0024tbl.Rows
            Select Case LNM0024row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0024row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0024INProw As DataRow In LNM0024INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0024INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0024INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0024row As DataRow In LNM0024tbl.Rows
                ' KEY項目が等しい時
                If LNM0024row("TORICODE") = LNM0024INProw("TORICODE") AndAlso
                   LNM0024row("INVFILINGDEPT") = LNM0024INProw("INVFILINGDEPT") AndAlso
                   LNM0024row("INVKESAIKBN") = LNM0024INProw("INVKESAIKBN") Then
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0024row("TORINAME") = LNM0024INProw("TORINAME") AndAlso
                        LNM0024row("TORINAMES") = LNM0024INProw("TORINAMES") AndAlso
                        LNM0024row("TORINAMEKANA") = LNM0024INProw("TORINAMEKANA") AndAlso
                        LNM0024row("TORIDIVNAME") = LNM0024INProw("TORIDIVNAME") AndAlso
                        LNM0024row("TORICHARGE") = LNM0024INProw("TORICHARGE") AndAlso
                        LNM0024row("TORIKBN") = LNM0024INProw("TORIKBN") AndAlso
                        LNM0024row("POSTNUM1") = LNM0024INProw("POSTNUM1") AndAlso
                        LNM0024row("POSTNUM2") = LNM0024INProw("POSTNUM2") AndAlso
                        LNM0024row("ADDR1") = LNM0024INProw("ADDR1") AndAlso
                        LNM0024row("ADDR2") = LNM0024INProw("ADDR2") AndAlso
                        LNM0024row("ADDR3") = LNM0024INProw("ADDR3") AndAlso
                        LNM0024row("ADDR4") = LNM0024INProw("ADDR4") AndAlso
                        LNM0024row("TEL") = LNM0024INProw("TEL") AndAlso
                        LNM0024row("FAX") = LNM0024INProw("FAX") AndAlso
                        LNM0024row("MAIL") = LNM0024INProw("MAIL") AndAlso
                        LNM0024row("BANKCODE") = LNM0024INProw("BANKCODE") AndAlso
                        LNM0024row("BANKBRANCHCODE") = LNM0024INProw("BANKBRANCHCODE") AndAlso
                        LNM0024row("ACCOUNTTYPE") = LNM0024INProw("ACCOUNTTYPE") AndAlso
                        LNM0024row("ACCOUNTNUMBER") = LNM0024INProw("ACCOUNTNUMBER") AndAlso
                        LNM0024row("ACCOUNTNAME") = LNM0024INProw("ACCOUNTNAME") AndAlso
                        LNM0024row("INACCOUNTCD") = LNM0024INProw("INACCOUNTCD") AndAlso
                        LNM0024row("TAXCALCULATION") = LNM0024INProw("TAXCALCULATION") AndAlso
                        LNM0024row("DEPOSITDAY") = LNM0024INProw("DEPOSITDAY") AndAlso
                        LNM0024row("DEPOSITMONTHKBN") = LNM0024INProw("DEPOSITMONTHKBN") AndAlso
                        LNM0024row("CLOSINGDAY") = LNM0024INProw("CLOSINGDAY") AndAlso
                        LNM0024row("ACCOUNTINGMONTH") = LNM0024INProw("ACCOUNTINGMONTH") AndAlso
                        LNM0024row("SLIPDESCRIPTION1") = LNM0024INProw("SLIPDESCRIPTION1") AndAlso
                        LNM0024row("SLIPDESCRIPTION2") = LNM0024INProw("SLIPDESCRIPTION2") AndAlso
                        LNM0024row("NEXTMONTHUNSETTLEDKBN") = LNM0024INProw("NEXTMONTHUNSETTLEDKBN") AndAlso
                        LNM0024row("DELFLG") = LNM0024INProw("DELFLG") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0024row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0024INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0024INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0024INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0024INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0024INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now

                '変更チェック
                KEKKJMEXISTS(SQLcon, WW_MODIFYKBN)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0024WRKINC.MODIFYKBN.AFTDATA
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
        For Each LNM0024INProw As DataRow In LNM0024INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0024row As DataRow In LNM0024tbl.Rows
                ' 同一レコードか判定
                If LNM0024INProw("TORICODE") = LNM0024row("TORICODE") AndAlso
                    LNM0024INProw("INVFILINGDEPT") = LNM0024row("INVFILINGDEPT") AndAlso
                    LNM0024INProw("INVKESAIKBN") = LNM0024row("INVKESAIKBN") Then
                    ' 画面入力テーブル項目設定
                    LNM0024INProw("LINECNT") = LNM0024row("LINECNT")
                    LNM0024INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0024INProw("UPDTIMSTP") = LNM0024row("UPDTIMSTP")
                    LNM0024INProw("SELECT") = 0
                    LNM0024INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0024row.ItemArray = LNM0024INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0024tbl.NewRow
                WW_NRow.ItemArray = LNM0024INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0024tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0024tbl.Rows.Add(WW_NRow)
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
        Dim WW_AuthorityAllFlg As String = "0"

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If
        Dim WW_PrmData As New Hashtable

        Try
            Select Case I_FIELD
                Case "TORICODE"               '取引先コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_KEKKJM, I_VALUE, O_TEXT, O_RTN, work.CreateKekkjmParam(GL0018InvKesaiKbnList.LS_INVOICE_WITH.TORICODE))
                Case "INVFILINGDEPT"               '請求書提出部店
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateUORGParam(Master.USERCAMP))
                Case "INVKESAIKBN"                 '請求書決済区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "INVKESAIKBN"))
                Case "NEXTMONTHUNSETTLEDKB"       '運賃翌月未決済区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "NEXTMONTHUNSETTLEDKBN"))
                Case "DEPOSITMONTHKBN"       '入金月区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DEPOSITMONTHKBN"))
                Case "KEIJOMKBN"       '計上月区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "KEIJOMKBN"))
                Case "DELFLG"                      '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "OUTPUTID"                    '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))
                Case "ONOFF"                       '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
