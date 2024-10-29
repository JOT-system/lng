''************************************************************
' 通運発送料マスタメンテ登録画面
' 作成日 2022/03/03
' 更新日 2024/01/18
' 作成者 瀬口
' 更新者 大浜
'
' 修正履歴 : 2022/03/03 新規作成
'          : 2024/01/18 変更履歴登録機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 通運発送料マスタメンテ（詳細）
''' </summary>
''' <remarks></remarks>
Public Class LNM0014ReutrmDetail
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private LNM0014tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0014INPtbl As DataTable                              'チェック用テーブル
    Private LNM0014UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0014tbl, work.WF_SEL_INPTBL.Text)

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
                        Case "mspStationSingleRowSelected" '[共通]駅選択ポップアップで行選択
                            RowSelected_mspStationSingle()
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
            If Not IsNothing(LNM0014tbl) Then
                LNM0014tbl.Clear()
                LNM0014tbl.Dispose()
                LNM0014tbl = Nothing
            End If

            If Not IsNothing(LNM0014INPtbl) Then
                LNM0014INPtbl.Clear()
                LNM0014INPtbl.Dispose()
                LNM0014INPtbl = Nothing
            End If

            If Not IsNothing(LNM0014UPDtbl) Then
                LNM0014UPDtbl.Clear()
                LNM0014UPDtbl.Dispose()
                LNM0014UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0014WRKINC.MAPIDD
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0014L Then
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
        txtMapId.Text = "LNM0014D"

        '大分類コード
        txtBigCtnCd.Text = work.WF_SEL_BIGCTNCD2.Text
        CODENAME_get("BIGCTNCD", txtBigCTNCD.Text, lblBigCTNCDName.Text, WW_Dummy)

        '中分類コード
        txtMiddleCtnCd.Text = work.WF_SEL_MIDDLECTNCD2.Text
        CODENAME_get("MIDDLECTNCD", txtMiddleCTNCD.Text, lblMiddleCTNCDName.Text, WW_Dummy)

        '発駅コード
        txtDepStation.Text = work.WF_SEL_DEPSTATION2.Text
        CODENAME_get("DEPSTATION", txtDepStation.Text, lblDepStationCDName.Text, WW_Dummy)

        '発受託人コード
        txtDepTrusteeCd.Text = work.WF_SEL_DEPTRUSTEECD2.Text
        CODENAME_get("DEPTRUSTEECD", txtDepTrusteeCd.Text, lblDepTrusteeCdName.Text, WW_Dummy)

        '発受託人サブコード
        txtDepTrusteeSubCd.Text = work.WF_SEL_DEPTRUSTEESUBCD2.Text
        CODENAME_get("DEPTRUSTEESUBCD", txtDepTrusteeSubCd.Text, lblDepTrusteeSubCdName.Text, WW_Dummy)

        '優先順位
        txtPriorityNo.Text = work.WF_SEL_PRIORITYNO.Text

        '使用目的
        txtPurpose.Text = work.WF_SEL_PURPOSE.Text

        '着駅コード
        txtArrstation.Text = work.WF_SEL_ARRSTATION.Text
        CODENAME_get("ARRSTATION", txtArrstation.Text, lblArrstationName.Text, WW_Dummy)

        '着受託人コード
        txtArrTrusteeCd.Text = work.WF_SEL_ARRTRUSTEECD.Text
        CODENAME_get("ARRTRUSTEECD", txtArrTrusteeCd.Text, lblArrTrusteeCdName.Text, WW_Dummy)

        '発受託人サブコード
        txtArrTrusteeSubCd.Text = work.WF_SEL_ARRTRUSTEESUBCD.Text
        CODENAME_get("ARRTRUSTEESUBCD", txtArrTrusteeSubCd.Text, lblArrTrusteeSubCdName.Text, WW_Dummy)

        '特例置換項目-現行開始適用日
        txtSprCurStYmd.Text = work.WF_SEL_SPRCURSTYMD.Text

        '特例置換項目-現行終了適用日
        txtSprCurEndYmd.Text = work.WF_SEL_SPRCURENDYMD.Text

        '特例置換項目-現行発送料
        txtSprCurShipFee.Text = work.WF_SEL_SPRCURSHIPFEE.Text

        '特例置換項目-現行到着料
        txtSprCurArriveFee.Text = work.WF_SEL_SPRCURARRIVEFEE.Text

        '特例置換項目-現行端数処理区分1
        txtSprCurRoundKbn1.Text = work.WF_SEL_SPRCURROUNDKBN1.Text
        CODENAME_get("SPRCURROUNDKBN1", txtSprCurRoundKbn1.Text, lblSprCurRoundKbn1Name.Text, WW_Dummy)

        '特例置換項目-現行端数処理区分2
        txtSprCurRoundKbn2.Text = work.WF_SEL_SPRCURROUNDKBN2.Text
        CODENAME_get("SPRCURROUNDKBN2", txtSprCurRoundKbn2.Text, lblSprCurRoundKbn2Name.Text, WW_Dummy)

        '特例置換項目-次期開始適用日
        txtSprNextStYmd.Text = work.WF_SEL_SPRNEXTSTYMD.Text

        '特例置換項目-次期終了適用日
        txtSprNextEndYmd.Text = work.WF_SEL_SPRNEXTENDYMD.Text

        '特例置換項目-次期発送料
        txtSprNextShipFee.Text = work.WF_SEL_SPRNEXTSHIPFEE.Text

        '特例置換項目-次期到着料
        txtSprNextArriveFee.Text = work.WF_SEL_SPRNEXTARRIVEFEE.Text

        '特例置換項目-次期端数処理区分1
        txtSprNextRoundKbn1.Text = work.WF_SEL_SPRNEXTROUNDKBN1.Text
        CODENAME_get("SPRNEXTROUNDKBN1", txtSprNextRoundKbn1.Text, lblSprNextRoundKbn1Name.Text, WW_Dummy)

        '特例置換項目-次期端数処理区分2
        txtSprNextRoundKbn2.Text = work.WF_SEL_SPRNEXTROUNDKBN2.Text
        CODENAME_get("SPRNEXTROUNDKBN2", txtSprNextRoundKbn2.Text, lblSprNextRoundKbn2Name.Text, WW_Dummy)

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_BIGCTNCD2.Text

        ' 数値(0～9)のみ入力可能とする。
        Me.txtDelFlg.Attributes("onkeyPress") = "CheckNum()"                   '削除フラグ
        Me.txtBigCtnCd.Attributes("onkeyPress") = "CheckNum()"                 '大分類コード
        Me.txtMiddleCtnCd.Attributes("onkeyPress") = "CheckNum()"              '中分類コード
        Me.txtDepStation.Attributes("onkeyPress") = "CheckNum()"               '発駅コード
        Me.txtDepTrusteeCd.Attributes("onkeyPress") = "CheckNum()"             '発受託人コード
        Me.txtDepTrusteeSubCd.Attributes("onkeyPress") = "CheckNum()"          '発受託人サブコード
        Me.txtPriorityNo.Attributes("onkeyPress") = "CheckNum()"               '優先順位
        Me.txtArrstation.Attributes("onkeyPress") = "CheckNum()"               '着駅コード
        Me.txtArrTrusteeCd.Attributes("onkeyPress") = "CheckNum()"             '着受託人コード
        Me.txtArrTrusteeSubCd.Attributes("onkeyPress") = "CheckNum()"          '着受託人サブコード
        Me.txtSprCurShipFee.Attributes("onkeyPress") = "CheckNum()"            '特例置換項目-現行発送料
        Me.txtSprCurArriveFee.Attributes("onkeyPress") = "CheckNum()"          '特例置換項目-現行到着料
        Me.txtSprCurRoundKbn1.Attributes("onkeyPress") = "CheckNum()"          '特例置換項目-現行端数処理区分1
        Me.txtSprCurRoundKbn2.Attributes("onkeyPress") = "CheckNum()"          '特例置換項目-現行端数処理区分2
        Me.txtSprNextShipFee.Attributes("onkeyPress") = "CheckNum()"           '特例置換項目-次期発送料
        Me.txtSprNextArriveFee.Attributes("onkeyPress") = "CheckNum()"         '特例置換項目-次期到着料
        Me.txtSprNextRoundKbn1.Attributes("onkeyPress") = "CheckNum()"         '特例置換項目-次期端数処理区分1
        Me.txtSprNextRoundKbn2.Attributes("onkeyPress") = "CheckNum()"         '特例置換項目-次期端数処理区分2

        ' 数値(0～9)＋記号(/)のみ入力可能とする。
        Me.txtSprCurStYmd.Attributes("onkeyPress") = "CheckCalendar()"         '特例置換項目-現行開始適用日
        Me.txtSprCurEndYmd.Attributes("onkeyPress") = "CheckCalendar()"        '特例置換項目-現行終了適用日
        Me.txtSprNextStYmd.Attributes("onkeyPress") = "CheckCalendar()"        '特例置換項目-次期開始適用日
        Me.txtSprNextEndYmd.Attributes("onkeyPress") = "CheckCalendar()"       '特例置換項目-次期終了適用日

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
            & "     BIGCTNCD                   " _
            & "   , MIDDLECTNCD                " _
            & "   , DEPSTATION                 " _
            & "   , DEPTRUSTEECD               " _
            & "   , DEPTRUSTEESUBCD            " _
            & "   , PRIORITYNO                 " _
            & " FROM                           " _
            & "     LNG.LNM0014_REUTRM         " _
            & " WHERE                          " _
            & "         BIGCTNCD        = @P1  " _
            & "     AND MIDDLECTNCD     = @P2  " _
            & "     AND DEPSTATION      = @P3  " _
            & "     AND DEPTRUSTEECD    = @P4  " _
            & "     AND DEPTRUSTEESUBCD = @P5  " _
            & "     AND PRIORITYNO      = @P6  " _
            & "     AND DELFLG         <> @P7  "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 2) '大分類コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 2) '中分類コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 6) '発駅コード
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 5) '発受託人コード
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.VarChar, 3) '発受託人サブコード
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@P6", MySqlDbType.VarChar, 5) '優先順位
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@P7", MySqlDbType.VarChar, 1) '削除フラグ

                PARA1.Value = txtBigCtnCd.Text
                PARA2.Value = txtMiddleCtnCd.Text
                PARA3.Value = txtDepStation.Text
                PARA4.Value = txtDepTrusteeCd.Text
                PARA5.Value = txtDepTrusteeSubCd.Text
                PARA6.Value = txtPriorityNo.Text
                PARA7.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0014Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0014Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0014Chk.Load(SQLdr)

                    If LNM0014Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 通運発送料マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(通運発送料マスタ)
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;                 " _
            & "     SET @hensuu = 0 ;                       " _
            & " DECLARE hensuu CURSOR FOR                   " _
            & "     SELECT                                  " _
            & "         UPDTIMSTP AS hensuu                 " _
            & "     FROM                                    " _
            & "         LNG.LNM0014_REUTRM                  " _
            & "     WHERE                                   " _
            & "             BIGCTNCD           = @P01       " _
            & "         AND MIDDLECTNCD        = @P02       " _
            & "         AND DEPSTATION         = @P03       " _
            & "         AND DEPTRUSTEECD       = @P04       " _
            & "         AND DEPTRUSTEESUBCD    = @P05       " _
            & "         AND PRIORITYNO         = @P06 ;     " _
            & " OPEN hensuu ;                               " _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;       " _
            & " IF (@@FETCH_STATUS = 0)                     " _
            & "     UPDATE LNG.LNM0014_REUTRM               " _
            & "     SET                                     " _
            & "         PURPOSE                = @P07       " _
            & "       , ARRSTATION             = @P08       " _
            & "       , ARRTRUSTEECD           = @P09       " _
            & "       , ARRTRUSTEESUBCD        = @P10       " _
            & "       , SPRCURSTYMD            = @P11       " _
            & "       , SPRCURENDYMD           = @P12       " _
            & "       , SPRCURSHIPFEE          = @P13       " _
            & "       , SPRCURARRIVEFEE        = @P14       " _
            & "       , SPRCURROUNDKBN         = @P15       " _
            & "       , SPRNEXTSTYMD           = @P16       " _
            & "       , SPRNEXTENDYMD          = @P17       " _
            & "       , SPRNEXTSHIPFEE         = @P18       " _
            & "       , SPRNEXTARRIVEFEE       = @P19       " _
            & "       , SPRNEXTROUNDKBN        = @P20       " _
            & "       , DELFLG                 = @P21       " _
            & "       , UPDYMD                 = @P26       " _
            & "       , UPDUSER                = @P27       " _
            & "       , UPDTERMID              = @P28       " _
            & "       , UPDPGID                = @P29       " _
            & "     WHERE                                   " _
            & "             BIGCTNCD           = @P01       " _
            & "         AND MIDDLECTNCD        = @P02       " _
            & "         AND DEPSTATION         = @P03       " _
            & "         AND DEPTRUSTEECD       = @P04       " _
            & "         AND DEPTRUSTEESUBCD    = @P05       " _
            & "         AND PRIORITYNO         = @P06 ;     " _
            & " IF (@@FETCH_STATUS <> 0)                    " _
            & "     INSERT INTO LNG.LNM0014_REUTRM          " _
            & "        (BIGCTNCD                            " _
            & "       , MIDDLECTNCD                         " _
            & "       , DEPSTATION                          " _
            & "       , DEPTRUSTEECD                        " _
            & "       , DEPTRUSTEESUBCD                     " _
            & "       , PRIORITYNO                          " _
            & "       , PURPOSE                             " _
            & "       , ARRSTATION                          " _
            & "       , ARRTRUSTEECD                        " _
            & "       , ARRTRUSTEESUBCD                     " _
            & "       , SPRCURSTYMD                         " _
            & "       , SPRCURENDYMD                        " _
            & "       , SPRCURSHIPFEE                       " _
            & "       , SPRCURARRIVEFEE                     " _
            & "       , SPRCURROUNDKBN                      " _
            & "       , SPRNEXTSTYMD                        " _
            & "       , SPRNEXTENDYMD                       " _
            & "       , SPRNEXTSHIPFEE                      " _
            & "       , SPRNEXTARRIVEFEE                    " _
            & "       , SPRNEXTROUNDKBN                     " _
            & "       , DELFLG                              " _
            & "       , INITYMD                             " _
            & "       , INITUSER                            " _
            & "       , INITTERMID                          " _
            & "       , INITPGID)                           " _
            & "     VALUES                                  " _
            & "        (@P01                                " _
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
            & "       , @P24                                " _
            & "       , @P25) ;                             " _
            & " CLOSE hensuu ;                              " _
            & " DEALLOCATE hensuu ;                         "

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                      " _
            & "     DELFLG                                  " _
            & "   , BIGCTNCD                                " _
            & "   , MIDDLECTNCD                             " _
            & "   , DEPSTATION                              " _
            & "   , DEPTRUSTEECD                            " _
            & "   , DEPTRUSTEESUBCD                         " _
            & "   , PRIORITYNO                              " _
            & "   , PURPOSE                                 " _
            & "   , ARRSTATION                              " _
            & "   , ARRTRUSTEECD                            " _
            & "   , ARRTRUSTEESUBCD                         " _
            & "   , SPRCURSTYMD                             " _
            & "   , SPRCURENDYMD                            " _
            & "   , SPRCURSHIPFEE                           " _
            & "   , SPRCURARRIVEFEE                         " _
            & "   , SPRCURROUNDKBN                          " _
            & "   , SPRNEXTSTYMD                            " _
            & "   , SPRNEXTENDYMD                           " _
            & "   , SPRNEXTSHIPFEE                          " _
            & "   , SPRNEXTARRIVEFEE                        " _
            & "   , SPRNEXTROUNDKBN                         " _
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
            & "     LNG.LNM0014_REUTRM                      " _
            & " WHERE                                       " _
            & "             BIGCTNCD              = @P01    " _
            & "         AND MIDDLECTNCD           = @P02    " _
            & "         AND DEPSTATION            = @P03    " _
            & "         AND DEPTRUSTEECD          = @P04    " _
            & "         AND DEPTRUSTEESUBCD       = @P05    " _
            & "         AND PRIORITYNO            = @P06    "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                ' DB更新用パラメータ
                Dim PARA001 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 2)         '大分類コード
                Dim PARA002 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 2)         '中分類コード
                Dim PARA003 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 6)         '発駅コード
                Dim PARA004 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim PARA005 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim PARA006 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar, 5)         '優先順位
                Dim PARA007 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar, 42)        '使用目的
                Dim PARA008 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.VarChar, 6)         '着駅コード
                Dim PARA009 As MySqlParameter = SQLcmd.Parameters.Add("@P09", MySqlDbType.VarChar, 5)         '着受託人コード
                Dim PARA010 As MySqlParameter = SQLcmd.Parameters.Add("@P10", MySqlDbType.VarChar, 3)         '着受託人サブコード
                Dim PARA011 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.Date)                '特例置換項目-現行開始適用日
                Dim PARA012 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.Date)                '特例置換項目-現行終了適用日
                Dim PARA013 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar, 5)         '特例置換項目-現行発送料
                Dim PARA014 As MySqlParameter = SQLcmd.Parameters.Add("@P14", MySqlDbType.VarChar, 5)         '特例置換項目-現行到着料
                Dim PARA015 As MySqlParameter = SQLcmd.Parameters.Add("@P15", MySqlDbType.VarChar, 2)         '特例置換項目-現行端数処理区分
                Dim PARA016 As MySqlParameter = SQLcmd.Parameters.Add("@P16", MySqlDbType.Date)                '特例置換項目-次期開始適用日
                Dim PARA017 As MySqlParameter = SQLcmd.Parameters.Add("@P17", MySqlDbType.Date)                '特例置換項目-次期終了適用日
                Dim PARA018 As MySqlParameter = SQLcmd.Parameters.Add("@P18", MySqlDbType.VarChar, 5)         '特例置換項目-次期発送料
                Dim PARA019 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.VarChar, 5)         '特例置換項目-次期到着料
                Dim PARA020 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.VarChar, 2)         '特例置換項目-次期端数処理区分
                Dim PARA021 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim PARA022 As MySqlParameter = SQLcmd.Parameters.Add("@P22", MySqlDbType.DateTime)            '登録年月日
                Dim PARA023 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.VarChar, 20)        '登録ユーザーＩＤ
                Dim PARA024 As MySqlParameter = SQLcmd.Parameters.Add("@P24", MySqlDbType.VarChar, 20)        '登録端末
                Dim PARA025 As MySqlParameter = SQLcmd.Parameters.Add("@P25", MySqlDbType.VarChar, 40)        '登録プログラムＩＤ
                Dim PARA026 As MySqlParameter = SQLcmd.Parameters.Add("@P26", MySqlDbType.DateTime)            '更新年月日
                Dim PARA027 As MySqlParameter = SQLcmd.Parameters.Add("@P27", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim PARA028 As MySqlParameter = SQLcmd.Parameters.Add("@P28", MySqlDbType.VarChar, 20)        '更新端末
                Dim PARA029 As MySqlParameter = SQLcmd.Parameters.Add("@P29", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ

                ' 更新ジャーナル出力用パラメータ
                Dim JPARA001 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 2)     '大分類コード
                Dim JPARA002 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P02", MySqlDbType.VarChar, 2)     '中分類コード
                Dim JPARA003 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P03", MySqlDbType.VarChar, 6)     '発駅コード
                Dim JPARA004 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P04", MySqlDbType.VarChar, 5)     '発受託人コード
                Dim JPARA005 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P05", MySqlDbType.VarChar, 3)     '発受託人サブコード
                Dim JPARA006 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P06", MySqlDbType.VarChar, 5)     '優先順位

                Dim LNM0014row As DataRow = LNM0014INPtbl.Rows(0)

                'Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA001.Value = LNM0014row("BIGCTNCD")                                            '大分類コード
                PARA002.Value = LNM0014row("MIDDLECTNCD")                                         '中分類コード
                PARA003.Value = LNM0014row("DEPSTATION")                                          '発駅コード
                PARA004.Value = LNM0014row("DEPTRUSTEECD")                                        '発受託人コード
                PARA005.Value = LNM0014row("DEPTRUSTEESUBCD")                                     '発受託人サブコード
                PARA006.Value = LNM0014row("PRIORITYNO")                                          '優先順位
                PARA007.Value = LNM0014row("PURPOSE")                                             '使用目的

                If String.IsNullOrEmpty(LNM0014row("ARRSTATION")) Then
                    PARA008.Value = DBNull.Value                                                  '着駅コード
                Else
                    PARA008.Value = LNM0014row("ARRSTATION")                                      '着駅コード
                End If

                If String.IsNullOrEmpty(LNM0014row("ARRTRUSTEECD")) Then
                    PARA009.Value = DBNull.Value                                                  '着受託人コード
                Else
                    PARA009.Value = LNM0014row("ARRTRUSTEECD")                                    '着受託人コード
                End If

                If String.IsNullOrEmpty(LNM0014row("ARRTRUSTEESUBCD")) Then
                    PARA010.Value = DBNull.Value                                                  '着受託人サブコード
                Else
                    PARA010.Value = LNM0014row("ARRTRUSTEESUBCD")                                 '着受託人サブコード
                End If

                If Not String.IsNullOrEmpty(RTrim(LNM0014row("SPRCURSTYMD"))) Then                '特例置換項目-現行開始適用日
                    PARA011.Value = RTrim(LNM0014row("SPRCURSTYMD"))
                Else
                    PARA011.Value = DBNull.Value                                                  '特例置換項目-現行開始適用日
                End If

                If Not String.IsNullOrEmpty(RTrim(LNM0014row("SPRCURENDYMD"))) Then               '特例置換項目-現行終了適用日
                    PARA012.Value = RTrim(LNM0014row("SPRCURENDYMD"))
                Else
                    PARA012.Value = DBNull.Value                                                  '特例置換項目-現行終了適用日
                End If

                If String.IsNullOrEmpty(LNM0014row("SPRCURSHIPFEE")) Then
                    PARA013.Value = "0"                                                           '特例置換項目-現行発送料
                Else
                    PARA013.Value = LNM0014row("SPRCURSHIPFEE")                                   '特例置換項目-現行発送料
                End If

                If String.IsNullOrEmpty(LNM0014row("SPRCURARRIVEFEE")) Then
                    PARA014.Value = "0"                                                           '特例置換項目-現行到着料
                Else
                    PARA014.Value = LNM0014row("SPRCURARRIVEFEE")                                 '特例置換項目-現行到着料
                End If

                If Not String.IsNullOrEmpty(LNM0014row("SPRCURROUNDKBN1")) AndAlso
                   Not LNM0014row("SPRCURROUNDKBN1") = "0" AndAlso
                   Not String.IsNullOrEmpty(LNM0014row("SPRCURROUNDKBN2")) AndAlso
                   Not LNM0014row("SPRCURROUNDKBN2") = "0" Then
                    PARA015.Value = LNM0014row("SPRCURROUNDKBN1") & LNM0014row("SPRCURROUNDKBN2") '特例置換項目-現行端数処理区分
                Else
                    PARA015.Value = "0"                                                           '特例置換項目-現行端数処理区分
                End If

                If Not String.IsNullOrEmpty(RTrim(LNM0014row("SPRNEXTSTYMD"))) Then               '特例置換項目-現行開始適用日
                    PARA016.Value = RTrim(LNM0014row("SPRNEXTSTYMD"))
                Else
                    PARA016.Value = DBNull.Value                                                  '特例置換項目-現行開始適用日
                End If

                If Not String.IsNullOrEmpty(RTrim(LNM0014row("SPRNEXTENDYMD"))) Then              '特例置換項目-現行終了適用日
                    PARA017.Value = RTrim(LNM0014row("SPRNEXTENDYMD"))
                Else
                    PARA017.Value = DBNull.Value                                                  '特例置換項目-現行終了適用日
                End If

                If String.IsNullOrEmpty(LNM0014row("SPRNEXTSHIPFEE")) Then
                    PARA018.Value = "0"                                                           '特例置換項目-現行発送料
                Else
                    PARA018.Value = LNM0014row("SPRNEXTSHIPFEE")                                  '特例置換項目-次期発送料
                End If

                If String.IsNullOrEmpty(LNM0014row("SPRNEXTARRIVEFEE")) Then
                    PARA019.Value = "0"                                                           '特例置換項目-次期到着料
                Else
                    PARA019.Value = LNM0014row("SPRNEXTARRIVEFEE")                                '特例置換項目-次期到着料
                End If

                If Not String.IsNullOrEmpty(LNM0014row("SPRNEXTROUNDKBN1")) AndAlso
                   Not LNM0014row("SPRNEXTROUNDKBN1") = "0" AndAlso
                   Not String.IsNullOrEmpty(LNM0014row("SPRNEXTROUNDKBN2")) AndAlso
                   Not LNM0014row("SPRNEXTROUNDKBN2") = "0" Then
                    PARA020.Value = LNM0014row("SPRNEXTROUNDKBN1") & LNM0014row("SPRNEXTROUNDKBN2") '特例置換項目-次期端数処理区分
                Else
                    PARA020.Value = "0"
                End If

                PARA021.Value = LNM0014row("DELFLG")                                              '削除フラグ                   
                PARA022.Value = WW_NOW                                                            '登録年月日                   
                PARA023.Value = Master.USERID                                                     '登録ユーザーＩＤ             
                PARA024.Value = Master.USERTERMID                                                 '登録端末                     
                PARA025.Value = Me.GetType().BaseType.Name                                        '登録プログラムＩＤ           
                PARA026.Value = WW_NOW                                                            '更新年月日                   
                PARA027.Value = Master.USERID                                                     '更新ユーザーＩＤ            
                PARA028.Value = Master.USERTERMID                                                 '更新端末                    
                PARA029.Value = Me.GetType().BaseType.Name                                        '更新プログラムＩＤ                            
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA001.Value = LNM0014row("BIGCTNCD")
                JPARA002.Value = LNM0014row("MIDDLECTNCD")
                JPARA003.Value = LNM0014row("DEPSTATION")
                JPARA004.Value = LNM0014row("DEPTRUSTEECD")
                JPARA005.Value = LNM0014row("DEPTRUSTEESUBCD")
                JPARA006.Value = LNM0014row("PRIORITYNO")

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0014UPDtbl) Then
                        LNM0014UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0014UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0014UPDtbl.Clear()
                    LNM0014UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0014UPDrow As DataRow In LNM0014UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0014D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0014UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014D UPDATE_INSERT"
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
    Protected Sub REUTRMEXISTS(ByVal SQLcon As MySqlConnection, ByRef WW_MODIFYKBN As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '回送運賃適用率マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        BIGCTNCD")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0014_REUTRM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        BIGCTNCD       = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD    = @MIDDLECTNCD")
        SQLStr.AppendLine("    AND DEPSTATION     = @DEPSTATION")
        SQLStr.AppendLine("    AND DEPTRUSTEECD     = @DEPTRUSTEECD")
        SQLStr.AppendLine("    AND DEPTRUSTEESUBCD  = @DEPTRUSTEESUBCD")
        SQLStr.AppendLine("    AND PRIORITYNO     = @PRIORITYNO")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位

                Dim LNM0014row As DataRow = LNM0014INPtbl.Rows(0)

                P_BIGCTNCD.Value = LNM0014row("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = LNM0014row("MIDDLECTNCD")               '中分類コード
                P_DEPSTATION.Value = LNM0014row("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = LNM0014row("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = LNM0014row("DEPTRUSTEESUBCD")               '発受託人サブコード
                P_PRIORITYNO.Value = LNM0014row("PRIORITYNO")               '優先順位

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
                        WW_MODIFYKBN = LNM0013WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0013WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0117_REUTRHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,ARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,ARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRCURSTYMD  ")
        SQLStr.AppendLine("        ,SPRCURENDYMD  ")
        SQLStr.AppendLine("        ,SPRCURSHIPFEE  ")
        SQLStr.AppendLine("        ,SPRCURARRIVEFEE  ")
        SQLStr.AppendLine("        ,SPRCURROUNDKBN  ")
        SQLStr.AppendLine("        ,SPRNEXTSTYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTENDYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTSHIPFEE  ")
        SQLStr.AppendLine("        ,SPRNEXTARRIVEFEE  ")
        SQLStr.AppendLine("        ,SPRNEXTROUNDKBN  ")
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
        SQLStr.AppendLine("         BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,ARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,ARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRCURSTYMD  ")
        SQLStr.AppendLine("        ,SPRCURENDYMD  ")
        SQLStr.AppendLine("        ,SPRCURSHIPFEE  ")
        SQLStr.AppendLine("        ,SPRCURARRIVEFEE  ")
        SQLStr.AppendLine("        ,SPRCURROUNDKBN  ")
        SQLStr.AppendLine("        ,SPRNEXTSTYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTENDYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTSHIPFEE  ")
        SQLStr.AppendLine("        ,SPRNEXTARRIVEFEE  ")
        SQLStr.AppendLine("        ,SPRNEXTROUNDKBN  ")
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
        SQLStr.AppendLine("        LNG.LNM0014_REUTRM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        BIGCTNCD       = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD    = @MIDDLECTNCD")
        SQLStr.AppendLine("    AND DEPSTATION     = @DEPSTATION")
        SQLStr.AppendLine("    AND DEPTRUSTEECD     = @DEPTRUSTEECD")
        SQLStr.AppendLine("    AND DEPTRUSTEESUBCD  = @DEPTRUSTEESUBCD")
        SQLStr.AppendLine("    AND PRIORITYNO     = @PRIORITYNO")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0014row As DataRow = LNM0014INPtbl.Rows(0)

                ' DB更新
                P_BIGCTNCD.Value = LNM0014row("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = LNM0014row("MIDDLECTNCD")               '中分類コード
                P_DEPSTATION.Value = LNM0014row("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = LNM0014row("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = LNM0014row("DEPTRUSTEESUBCD")               '発受託人サブコード
                P_PRIORITYNO.Value = LNM0014row("PRIORITYNO")               '優先順位

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0014WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0014tbl.Rows(0)("DELFLG") = "0" And LNM0014row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0014WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0014WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0117_REUTRHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0117_REUTRHIST  INSERT"
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
        DetailBoxToLNM0014INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0014tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0014tbl, work.WF_SEL_INPTBL.Text)

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
    Protected Sub DetailBoxToLNM0014INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(txtDelFlg.Text)             '削除フラグ
        Master.EraseCharToIgnore(txtBigCtnCd.Text)           '大分類コード
        Master.EraseCharToIgnore(txtMiddleCtnCd.Text)        '中分類コード
        Master.EraseCharToIgnore(txtDepStation.Text)         '発駅コード
        Master.EraseCharToIgnore(txtDepTrusteeCd.Text)       '発受託人コード
        Master.EraseCharToIgnore(txtDepTrusteeSubCd.Text)    '発受託人サブコード
        Master.EraseCharToIgnore(txtPriorityNo.Text)         '優先順位
        Master.EraseCharToIgnore(txtPurpose.Text)            '使用目的
        Master.EraseCharToIgnore(txtArrStation.Text)         '着駅コード
        Master.EraseCharToIgnore(txtArrTrusteeCd.Text)       '着受託人コード
        Master.EraseCharToIgnore(txtArrTrusteeSubCd.Text)    '着受託人サブコード
        Master.EraseCharToIgnore(txtSprCurStYmd.Text)        '特例置換項目-現行開始適用日
        Master.EraseCharToIgnore(txtSprCurEndYmd.Text)       '特例置換項目-現行終了適用日
        Master.EraseCharToIgnore(txtSprCurShipFee.Text)      '特例置換項目-現行発送料
        Master.EraseCharToIgnore(txtSprCurArriveFee.Text)    '特例置換項目-現行到着料
        Master.EraseCharToIgnore(txtSprCurRoundKbn1.Text)    '特例置換項目-現行端数処理区分1
        Master.EraseCharToIgnore(txtSprCurRoundKbn2.Text)    '特例置換項目-現行端数処理区分2
        Master.EraseCharToIgnore(txtSprNextStYmd.Text)       '特例置換項目-次期開始適用日
        Master.EraseCharToIgnore(txtSprNextEndYmd.Text)      '特例置換項目-次期終了適用日
        Master.EraseCharToIgnore(txtSprNextShipFee.Text)     '特例置換項目-次期発送料
        Master.EraseCharToIgnore(txtSprNextArriveFee.Text)   '特例置換項目-次期到着料
        Master.EraseCharToIgnore(txtSprNextRoundKbn1.Text)   '特例置換項目-次期端数処理区分1
        Master.EraseCharToIgnore(txtSprNextRoundKbn2.Text)   '特例置換項目-次期端数処理区分2

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

        Master.CreateEmptyTable(LNM0014INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0014INProw As DataRow = LNM0014INPtbl.NewRow

        'LINECNT
        If lblSelLineCNT.Text = "" Then
            LNM0014INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(lblSelLineCNT.Text, LNM0014INProw("LINECNT"))
            Catch ex As Exception
                LNM0014INProw("LINECNT") = 0
            End Try
        End If

        LNM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0014INProw("UPDTIMSTP") = 0
        LNM0014INProw("SELECT") = 1
        LNM0014INProw("HIDDEN") = 0

        LNM0014INProw("BIGCTNCD") = txtBigCtnCd.Text                                             '大分類コード
        LNM0014INProw("MIDDLECTNCD") = txtMiddleCtnCd.Text                                       '中分類コード
        LNM0014INProw("DEPSTATION") = txtDepStation.Text                                         '発駅コード
        LNM0014INProw("DEPTRUSTEECD") = txtDepTrusteeCd.Text                                     '発受託人コード
        LNM0014INProw("DEPTRUSTEESUBCD") = txtDepTrusteeSubCd.Text                               '発受託人サブコード
        LNM0014INProw("PRIORITYNO") = txtPriorityNo.Text                                         '優先順位
        LNM0014INProw("PURPOSE") = txtPurpose.Text                                               '使用目的
        LNM0014INProw("ARRSTATION") = txtArrstation.Text                                         '着駅コード
        LNM0014INProw("ARRTRUSTEECD") = txtArrTrusteeCd.Text                                     '着受託人コード
        LNM0014INProw("ARRTRUSTEESUBCD") = txtArrTrusteeSubCd.Text                               '着受託人サブコード
        LNM0014INProw("SPRCURSTYMD") = txtSprCurStYmd.Text                                       '特例置換項目-現行開始適用日
        LNM0014INProw("SPRCURENDYMD") = txtSprCurEndYmd.Text                                     '特例置換項目-現行終了適用日
        LNM0014INProw("SPRCURSHIPFEE") = txtSprCurShipFee.Text                                   '特例置換項目-現行発送料
        LNM0014INProw("SPRCURARRIVEFEE") = txtSprCurArriveFee.Text                               '特例置換項目-現行到着料
        LNM0014INProw("SPRCURROUNDKBN") = txtSprCurRoundKbn1.Text & txtSprCurRoundKbn2.Text      '特例置換項目-現行端数処理区分
        LNM0014INProw("SPRCURROUNDKBN1") = txtSprCurRoundKbn1.Text                               '特例置換項目-現行端数処理区分1
        LNM0014INProw("SPRCURROUNDKBN2") = txtSprCurRoundKbn2.Text                               '特例置換項目-現行端数処理区分2
        LNM0014INProw("SPRNEXTSTYMD") = txtSprNextStYmd.Text                                     '特例置換項目-次期開始適用日
        LNM0014INProw("SPRNEXTENDYMD") = txtSprNextEndYmd.Text                                   '特例置換項目-次期終了適用日
        LNM0014INProw("SPRNEXTSHIPFEE") = txtSprNextShipFee.Text                                 '特例置換項目-次期発送料
        LNM0014INProw("SPRNEXTARRIVEFEE") = txtSprNextArriveFee.Text                             '特例置換項目-次期到着料
        LNM0014INProw("SPRNEXTROUNDKBN") = txtSprNextRoundKbn1.Text & txtSprNextRoundKbn2.Text   '特例置換項目-次期端数処理区分
        LNM0014INProw("SPRNEXTROUNDKBN1") = txtSprNextRoundKbn1.Text                             '特例置換項目-次期端数処理区分1
        LNM0014INProw("SPRNEXTROUNDKBN2") = txtSprNextRoundKbn2.Text                             '特例置換項目-次期端数処理区分2
        LNM0014INProw("DELFLG") = txtDelFlg.Text                                                 '削除フラグ
        LNM0014INProw("UPDYMD") = Date.Now                                                       '更新日付

        '○ チェック用テーブルに登録する
        LNM0014INPtbl.Rows.Add(LNM0014INProw)

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
        DetailBoxToLNM0014INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0014INProw As DataRow = LNM0014INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0014row As DataRow In LNM0014tbl.Rows
            ' KEY項目が等しい時
            If LNM0014row("BIGCTNCD") = LNM0014INProw("BIGCTNCD") AndAlso
                LNM0014row("MIDDLECTNCD") = LNM0014INProw("MIDDLECTNCD") AndAlso
                LNM0014row("DEPSTATION") = LNM0014INProw("DEPSTATION") AndAlso
                LNM0014row("DEPTRUSTEECD") = LNM0014INProw("DEPTRUSTEECD") AndAlso
                LNM0014row("DEPTRUSTEESUBCD") = LNM0014INProw("DEPTRUSTEESUBCD") AndAlso
                LNM0014row("PRIORITYNO") = LNM0014INProw("PRIORITYNO") Then
                ' KEY項目以外の項目の差異をチェック
                If LNM0014row("PURPOSE") = LNM0014INProw("PURPOSE") AndAlso
                    LNM0014row("ARRSTATION") = LNM0014INProw("ARRSTATION") AndAlso
                    LNM0014row("ARRTRUSTEECD") = LNM0014INProw("ARRTRUSTEECD") AndAlso
                    LNM0014row("ARRTRUSTEESUBCD") = LNM0014INProw("ARRTRUSTEESUBCD") AndAlso
                    LNM0014row("SPRCURSTYMD") = LNM0014INProw("SPRCURSTYMD") AndAlso
                    LNM0014row("SPRCURENDYMD") = LNM0014INProw("SPRCURENDYMD") AndAlso
                    LNM0014row("SPRCURSHIPFEE") = LNM0014INProw("SPRCURSHIPFEE") AndAlso
                    LNM0014row("SPRCURARRIVEFEE") = LNM0014INProw("SPRCURARRIVEFEE") AndAlso
                    LNM0014row("SPRCURROUNDKBN1") = LNM0014INProw("SPRCURROUNDKBN1") AndAlso
                    LNM0014row("SPRCURROUNDKBN2") = LNM0014INProw("SPRCURROUNDKBN2") AndAlso
                    LNM0014row("SPRNEXTSTYMD") = LNM0014INProw("SPRNEXTSTYMD") AndAlso
                    LNM0014row("SPRNEXTENDYMD") = LNM0014INProw("SPRNEXTENDYMD") AndAlso
                    LNM0014row("SPRNEXTSHIPFEE") = LNM0014INProw("SPRNEXTSHIPFEE") AndAlso
                    LNM0014row("SPRNEXTARRIVEFEE") = LNM0014INProw("SPRNEXTARRIVEFEE") AndAlso
                    LNM0014row("SPRNEXTROUNDKBN1") = LNM0014INProw("SPRNEXTROUNDKBN1") AndAlso
                    LNM0014row("SPRNEXTROUNDKBN2") = LNM0014INProw("SPRNEXTROUNDKBN2") AndAlso
                    LNM0014row("DELFLG") = LNM0014INProw("DELFLG") Then
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
        For Each LNM0014row As DataRow In LNM0014tbl.Rows
            Select Case LNM0014row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0014tbl, work.WF_SEL_INPTBL.Text)

        lblSelLineCNT.Text = ""               'LINECNT
        txtMapId.Text = "M00001"              '画面ＩＤ

        txtBigCtnCd.Text = ""                 '大分類コード
        txtMiddleCtnCd.Text = ""              '中分類コード
        txtDepStation.Text = ""               '発駅コード
        txtDepTrusteeCd.Text = ""             '発受託人コード
        txtDepTrusteeSubCd.Text = ""          '発受託人サブコード
        txtPriorityNo.Text = ""               '優先順位
        txtPurpose.Text = ""                  '使用目的
        txtArrstation.Text = ""               '着駅コード
        txtArrTrusteeCd.Text = ""             '着受託人コード
        txtArrTrusteeSubCd.Text = ""          '着受託人サブコード
        txtSprCurStYmd.Text = ""              '特例置換項目-現行開始適用日
        txtSprCurEndYmd.Text = ""             '特例置換項目-現行終了適用日
        txtSprCurShipFee.Text = ""            '特例置換項目-現行発送料
        txtSprCurArriveFee.Text = ""          '特例置換項目-現行到着料
        txtSprCurRoundKbn1.Text = ""          '特例置換項目-現行端数処理区分1
        txtSprCurRoundKbn2.Text = ""          '特例置換項目-現行端数処理区分2
        txtSprNextStYmd.Text = ""             '特例置換項目-次期開始適用日
        txtSprNextEndYmd.Text = ""            '特例置換項目-次期終了適用日
        txtSprNextShipFee.Text = ""           '特例置換項目-次期発送料
        txtSprNextArriveFee.Text = ""         '特例置換項目-次期到着料
        txtSprNextRoundKbn1.Text = ""         '特例置換項目-次期端数処理区分1
        txtSprNextRoundKbn2.Text = ""         '特例置換項目-次期端数処理区分2
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
                .Visible = true
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        ' 日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "txtSprCurStYmd"           '特例置換項目-現行開始適用日
                                .WF_Calendar.Text = txtSprCurStYmd.Text
                            Case "txtSprCurEndYmd"          '特例置換項目-現行終了適用日
                                .WF_Calendar.Text = txtSprCurEndYmd.Text
                            Case "txtSprNextStYmd"          '特例置換項目-次期開始適用日
                                .WF_Calendar.Text = txtSprNextStYmd.Text
                            Case "txtSprNextEndYmd"         '特例置換項目-次期終了適用日
                                .WF_Calendar.Text = txtSprNextEndYmd.Text
                        End Select
                        .ActiveCalendar()

                    Case Else
                        ' フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "txtBigCtnCd"             '大分類コード
                                WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                            Case "txtMiddleCtnCd"          '中分類コード
                                WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, txtBigCtnCd.Text)
                            Case "txtDepStation"          '発駅コード
                                leftview.Visible = False
                                '検索画面
                                DisplayView_mspStationSingle()
                                '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                                WF_LeftboxOpen.Value = ""
                                Exit Sub
                            Case "txtDepTrusteeCd"        '発受託人コード
                                WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, txtDepStation.Text)
                            Case "txtDepTrusteeSubCd"     '発受託人サブコード
                                WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, txtDepStation.Text, txtDepTrusteeCd.Text)
                            Case "txtArrStation"          '着駅コード
                                leftview.Visible = False
                                '検索画面
                                DisplayView_mspStationSingle()
                                '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                                WF_LeftboxOpen.Value = ""
                                Exit Sub
                            Case "txtArrTrusteeCd"        '着受託人コード
                                WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, txtArrStation.Text)
                            Case "txtArrTrusteeSubCd"     '着受託人サブコード
                                WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, txtArrStation.Text, txtArrTrusteeCd.Text)
                            Case "txtSprCurRoundKbn1"      '特例置換項目-現行端数処理区分1
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUU1")
                            Case "txtSprCurRoundKbn2"      '特例置換項目-現行端数処理区分2
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUU2")
                            Case "txtSprNextRoundKbn1"     '特例置換項目-次期端数処理区分1
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUU1")
                            Case "txtSprNextRoundKbn2"     '特例置換項目-次期端数処理区分2
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUU2")
                            Case "txtDelFlg"              '削除フラグ
                                WW_PrmData.Item(C_PARAMETERS.LP_COMPANY) = Master.USERCAMP
                                WW_PrmData.Item(C_PARAMETERS.LP_TYPEMODE) = "2"
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
            Case "txtDelFlg"              '削除フラグ
                CODENAME_get("DELFLG", txtDelFlg.Text, lblDelFlgName.Text, WW_Dummy)
                txtDelFlg.Focus()
            Case "txtBigCtnCd"            '大分類コード
                CODENAME_get("BIGCTNCD", txtBigCtnCd.Text, lblBigCtnCdName.Text, WW_Dummy)
                txtBigCtnCd.Focus()
                ReSetClassCd()
            Case "txtMiddleCtnCd"         '中分類コード
                CODENAME_get("MIDDLECTNCD", txtMiddleCtnCd.Text, lblMiddleCtnCdName.Text, WW_Dummy)
                txtMiddleCtnCd.Focus()
            Case "txtDepStation"          '発駅コード
                CODENAME_get("DEPSTATION", txtDepStation.Text, lblDepStationCDName.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(lblDepStationCDName.Text) And txtDepStation.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(txtDepStation.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    txtDepStation.Focus()
                End If
            Case "txtDepTrusteeCd"        '発受託人コード
                CODENAME_get("DEPTRUSTEECD", txtDepTrusteeCd.Text, lblDepTrusteeCdName.Text, WW_Dummy)
                txtDepTrusteeCd.Focus()
            Case "txtDepTrusteeSubCd"     '発受託人サブコード
                CODENAME_get("DEPTRUSTEESUBCD", txtDepTrusteeSubCd.Text, lblDepTrusteeSubCdName.Text, WW_Dummy)
                txtDepTrusteeSubCd.Focus()
            Case "txtArrStation"          '着駅コード
                CODENAME_get("ARRSTATION", txtArrStation.Text, lblArrStationName.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(lblArrStationName.Text) And txtArrStation.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(txtArrStation.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    txtArrStation.Focus()
                End If
            Case "txtArrTrusteeCd"        '着受託人コード
                CODENAME_get("ARRTRUSTEECD", txtArrTrusteeCd.Text, lblArrTrusteeCdName.Text, WW_Dummy)
                txtArrTrusteeCd.Focus()
            Case "txtArrTrusteeSubCd"     '着受託人サブコード
                CODENAME_get("ARRTRUSTEESUBCD", txtArrTrusteeSubCd.Text, lblArrTrusteeSubCdName.Text, WW_Dummy)
                txtArrTrusteeSubCd.Focus()
            Case "txtSprCurRoundKbn1"      '特例置換項目-現行端数処理区分1
                CODENAME_get("SPRCURROUNDKBN1", txtSprCurRoundKbn1.Text, lblSprCurRoundKbn1Name.Text, WW_Dummy)
                txtSprCurRoundKbn1.Focus()
            Case "txtSprCurRoundKbn2"      '特例置換項目-現行端数処理区分2
                CODENAME_get("SPRCURROUNDKBN2", txtSprCurRoundKbn2.Text, lblSprCurRoundKbn2Name.Text, WW_Dummy)
                txtSprCurRoundKbn2.Focus()
            Case "txtSprNextRoundKbn1"     '特例置換項目-次期端数処理区分1
                CODENAME_get("SPRNEXTROUNDKBN1", txtSprNextRoundKbn1.Text, lblSprNextRoundKbn1Name.Text, WW_Dummy)
                txtSprNextRoundKbn1.Focus()
            Case "txtSprNextRoundKbn2"     '特例置換項目-次期端数処理区分2
                CODENAME_get("SPRNEXTROUNDKBN2", txtSprNextRoundKbn2.Text, lblSprNextRoundKbn2Name.Text, WW_Dummy)
                txtSprNextRoundKbn2.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 中小分類リセット
    ''' </summary>
    Protected Sub ReSetClassCd()

        txtMiddleCtnCd.Text = ""
        lblMiddleCtnCdName.Text = ""

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
        If WF_FIELD_REP.Value = "" Then
            Select Case WF_FIELD.Value
                Case "txtDelFlg"                                    '削除フラグ
                    txtDelFlg.Text = WW_SelectValue
                    lblDelFlgName.Text = WW_SelectText
                    txtDelFlg.Focus()
                Case "txtBigCtnCd"                                  '大分類コード
                    txtBigCtnCd.Text = WW_SelectValue
                    lblBigCtnCdName.Text = WW_SelectText
                    txtBigCtnCd.Focus()
                    ReSetClassCd()
                Case "txtMiddleCtnCd"                               '中分類コード
                    txtMiddleCtnCd.Text = WW_SelectValue
                    lblMiddleCtnCdName.Text = WW_SelectText
                    txtMiddleCtnCd.Focus()
                Case "txtDepStation"                                '発駅コード
                    txtDepStation.Text = WW_SelectValue
                    lblDepStationCDName.Text = WW_SelectText
                    txtDepStation.Focus()
                Case "txtDepTrusteeCd"                              '発受託人コード
                    txtDepTrusteeCd.Text = WW_SelectValue
                    lblDepTrusteeCdName.Text = WW_SelectText
                    txtDepTrusteeCd.Focus()
                Case "txtDepTrusteeSubCd"                           '発受託人サブコード
                    txtDepTrusteeSubCd.Text = WW_SelectValue
                    lblDepTrusteeSubCdName.Text = WW_SelectText
                    txtDepTrusteeSubCd.Focus()
                Case "txtArrStation"                                '着駅コード
                    txtArrStation.Text = WW_SelectValue
                    lblArrStationName.Text = WW_SelectText
                    txtArrStation.Focus()
                Case "txtArrTrusteeCd"                              '着受託人コード
                    txtArrTrusteeCd.Text = WW_SelectValue
                    lblArrTrusteeCdName.Text = WW_SelectText
                    txtArrTrusteeCd.Focus()
                Case "txtArrTrusteeSubCd"                           '着受託人サブコード
                    txtArrTrusteeSubCd.Text = WW_SelectValue
                    lblArrTrusteeSubCdName.Text = WW_SelectText
                    txtArrTrusteeSubCd.Focus()
                Case "txtSprCurStYmd"                               '特例置換項目-現行開始適用日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            txtSprCurStYmd.Text = ""
                        Else
                            txtSprCurStYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    txtSprCurStYmd.Focus()
                Case "txtSprCurEndYmd"                              '特例置換項目-現行終了適用日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            txtSprCurEndYmd.Text = ""
                        Else
                            txtSprCurEndYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    txtSprCurEndYmd.Focus()
                Case "txtSprCurRoundKbn1"      '特例置換項目-現行端数処理区分1
                    txtSprCurRoundKbn1.Text = WW_SelectValue
                    lblSprCurRoundKbn1Name.Text = WW_SelectText
                    txtSprCurRoundKbn1.Focus()
                Case "txtSprCurRoundKbn2"      '特例置換項目-現行端数処理区分2
                    txtSprCurRoundKbn2.Text = WW_SelectValue
                    lblSprCurRoundKbn2Name.Text = WW_SelectText
                    txtSprCurRoundKbn2.Focus()
                Case "txtSprNextStYmd"         '特例置換項目-次期開始適用日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            txtSprNextStYmd.Text = ""
                        Else
                            txtSprNextStYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    txtSprNextStYmd.Focus()
                Case "txtSprNextEndYmd"         '特例置換項目-次期終了適用日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            txtSprNextEndYmd.Text = ""
                        Else
                            txtSprNextEndYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    txtSprNextEndYmd.Focus()
                Case "txtSprNextRoundKbn1"     '特例置換項目-次期端数処理区分1
                    txtSprNextRoundKbn1.Text = WW_SelectValue
                    lblSprNextRoundKbn1Name.Text = WW_SelectText
                    txtSprNextRoundKbn1.Focus()
                Case "txtSprNextRoundKbn2"     '特例置換項目-次期端数処理区分2
                    txtSprNextRoundKbn2.Text = WW_SelectValue
                    lblSprNextRoundKbn2Name.Text = WW_SelectText
                    txtSprNextRoundKbn2.Focus()
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
                Case "txtDelFlg"                  '削除フラグ
                    txtDelFlg.Focus()
                Case "txtBigCtnCd"                '大分類コード
                    txtBigCtnCd.Focus()
                Case "txtMiddleCtnCd"             '中分類コード
                    txtMiddleCtnCd.Focus()
                Case "txtDepStation"              '発駅コード
                    txtDepStation.Focus()
                Case "txtDepTrusteeCd"            '発受託人コード
                    txtDepTrusteeCd.Focus()
                Case "txtDepTrusteeSubCd"         '発受託人サブコード
                    txtDepTrusteeSubCd.Focus()
                Case "txtPriorityNo"              '優先順位
                    txtPriorityNo.Focus()
                Case "txtPurpose"                 '使用目的
                    txtPurpose.Focus()
                Case "txtArrStation"              '着駅コード
                    txtArrStation.Focus()
                Case "txtArrTrusteeCd"            '着受託人コード
                    txtArrTrusteeCd.Focus()
                Case "txtArrTrusteeSubCd"         '着受託人サブコード
                    txtArrTrusteeSubCd.Focus()
                Case "txtSprCurStYmd"             '特例置換項目-現行開始適用日
                    txtSprCurStYmd.Focus()
                Case "txtSprCurEndYmd"            '特例置換項目-現行終了適用日
                    txtSprCurEndYmd.Focus()
                Case "txtSprCurShipFee"           '特例置換項目-現行適用率
                    txtSprCurShipFee.Focus()
                Case "txtSprCurArriveFee"         '特例置換項目-現行適用率
                    txtSprCurArriveFee.Focus()
                Case "txtSprCurRoundKbn1"         '特例置換項目-現行端数処理区分1
                    txtSprCurRoundKbn1.Focus()
                Case "txtSprCurRoundKbn2"         '特例置換項目-現行端数処理区分2
                    txtSprCurRoundKbn2.Focus()
                Case "txtSprNextStYmd"            '特例置換項目-次期開始適用日
                    txtSprNextStYmd.Focus()
                Case "txtSprNextEndYmd"           '特例置換項目-次期終了適用日
                    txtSprNextEndYmd.Focus()
                Case "txtSprNextShipFee"          '特例置換項目-次期適用率
                    txtSprNextShipFee.Focus()
                Case "txtSprNextArriveFee"        '特例置換項目-次期適用率
                    txtSprNextArriveFee.Focus()
                Case "txtSprNextRoundKbn1"        '特例置換項目-次期端数処理区分1
                    txtSprNextRoundKbn1.Focus()
                Case "txtSprNextRoundKbn2"        '特例置換項目-次期端数処理区分2
                    txtSprNextRoundKbn2.Focus()
            End Select
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

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
    ''' 駅選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspStationSingle()

        Dim selData = Me.mspStationSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case txtDepStation.ID
                Me.txtDepStation.Text = selData("STATION").ToString
                Me.lblDepStationCDName.Text = selData("NAMES").ToString
                Me.txtDepStation.Focus()

            Case txtArrStation.ID
                Me.txtArrStation.Text = selData("STATION").ToString
                Me.lblArrStationName.Text = selData("NAMES").ToString
                Me.txtArrStation.Focus()
        End Select

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


        For Each LNM0014INProw As DataRow In LNM0014INPtbl.Rows
            '○ 単項目チェック

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0014INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 値存在チェック
                CODENAME_get("DELFLG", LNM0014INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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

            ' 大分類コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIGCTNCD", LNM0014INProw("BIGCTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 値存在チェック
                CODENAME_get("BIGCTNCD", LNM0014INProw("BIGCTNCD"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・大分類コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・大分類コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 中分類コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "MIDDLECTNCD", LNM0014INProw("MIDDLECTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 値存在チェック
                CODENAME_get("MIDDLECTNCD", LNM0014INProw("MIDDLECTNCD"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・中分類コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・中分類コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発駅コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPSTATION", LNM0014INProw("DEPSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then

                If Not String.IsNullOrEmpty(LNM0014INProw("DEPSTATION")) AndAlso
                   Not LNM0014INProw("DEPSTATION") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("DEPSTATION", LNM0014INProw("DEPSTATION"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・発駅コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・発駅コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発受託人コード
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEECD", LNM0014INProw("DEPTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("DEPTRUSTEECD")) AndAlso
                   Not LNM0014INProw("DEPTRUSTEECD") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("DEPTRUSTEECD", LNM0014INProw("DEPTRUSTEECD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・発受託人コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・発受託人コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発受託人サブコード
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBCD", LNM0014INProw("DEPTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("DEPTRUSTEESUBCD")) Then
                    ' 値存在チェック
                    CODENAME_get("DEPTRUSTEESUBCD", LNM0014INProw("DEPTRUSTEESUBCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・発受託人サブコードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・発受託人サブコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 優先順位(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "PRIORITYNO", LNM0014INProw("PRIORITYNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・優先順位エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 使用目的
            Master.CheckField(Master.USERCAMP, "PURPOSE", LNM0014INProw("PURPOSE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・使用目的エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 着駅コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ARRSTATION", LNM0014INProw("ARRSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("ARRSTATION")) AndAlso
                   Not LNM0014INProw("ARRSTATION") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("ARRSTATION", LNM0014INProw("ARRSTATION"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・着駅コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・着駅コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 着受託人コード
            Master.CheckField(Master.USERCAMP, "ARRTRUSTEECD", LNM0014INProw("ARRTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("ARRTRUSTEECD")) AndAlso
                   Not LNM0014INProw("ARRTRUSTEECD") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("ARRTRUSTEECD", LNM0014INProw("ARRTRUSTEECD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・着受託人コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・着受託人コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 着受託人サブコード
            Master.CheckField(Master.USERCAMP, "ARRTRUSTEESUBCD", LNM0014INProw("ARRTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("ARRTRUSTEESUBCD")) Then
                    ' 値存在チェック
                    CODENAME_get("ARRTRUSTEESUBCD", LNM0014INProw("ARRTRUSTEESUBCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・着受託人サブコードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・着受託人サブコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If


            ' 特例置換項目-現行開始適用日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRCURSTYMD", LNM0014INProw("SPRCURSTYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("SPRCURSTYMD")) Then
                    LNM0014INProw("SPRCURSTYMD") = CDate(LNM0014INProw("SPRCURSTYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（現行）開始適用日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-現行終了適用日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRCURENDYMD", LNM0014INProw("SPRCURENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("SPRCURENDYMD")) Then
                    LNM0014INProw("SPRCURENDYMD") = CDate(LNM0014INProw("SPRCURENDYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（現行）終了適用日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-現行発送料
            Master.CheckField(Master.USERCAMP, "SPRCURSHIPFEE", LNM0014INProw("SPRCURSHIPFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目（現行）現行発送料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-現行到着料
            Master.CheckField(Master.USERCAMP, "SPRCURARRIVEFEE", LNM0014INProw("SPRCURARRIVEFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目（現行）現行到着料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-現行端数処理区分1
            Master.CheckField(Master.USERCAMP, "SPRCURROUNDKBN1", LNM0014INProw("SPRCURROUNDKBN1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("SPRCURROUNDKBN1")) AndAlso
                   Not LNM0014INProw("SPRCURROUNDKBN1") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("SPRCURROUNDKBN1", LNM0014INProw("SPRCURROUNDKBN1"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目（現行）端数処理区分1エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（現行）端数処理区分1エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-現行端数処理区分2
            Master.CheckField(Master.USERCAMP, "SPRCURROUNDKBN2", LNM0014INProw("SPRCURROUNDKBN2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("SPRCURROUNDKBN2")) AndAlso
                   Not LNM0014INProw("SPRCURROUNDKBN2") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("SPRCURROUNDKBN2", LNM0014INProw("SPRCURROUNDKBN2"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目（現行）端数処理区分2エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（現行）端数処理区分2エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-次期開始適用日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRNEXTSTYMD", LNM0014INProw("SPRNEXTSTYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTSTYMD")) Then
                    LNM0014INProw("SPRNEXTSTYMD") = CDate(LNM0014INProw("SPRNEXTSTYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（次期）開始適用日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-次期終了適用日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRNEXTENDYMD", LNM0014INProw("SPRNEXTENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTENDYMD")) Then
                    LNM0014INProw("SPRNEXTENDYMD") = CDate(LNM0014INProw("SPRNEXTENDYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（次期）終了適用日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-次期発送料
            Master.CheckField(Master.USERCAMP, "SPRNEXTSHIPFEE", LNM0014INProw("SPRNEXTSHIPFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目（次期）発送料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-次期到着料
            Master.CheckField(Master.USERCAMP, "SPRNEXTARRIVEFEE", LNM0014INProw("SPRNEXTARRIVEFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目（次期）到着料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-次期端数処理区分1
            Master.CheckField(Master.USERCAMP, "SPRNEXTROUNDKBN1", LNM0014INProw("SPRNEXTROUNDKBN1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTROUNDKBN1")) AndAlso
                   Not LNM0014INProw("SPRNEXTROUNDKBN1") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("SPRNEXTROUNDKBN1", LNM0014INProw("SPRNEXTROUNDKBN1"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目（次期）端数処理区分1エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（次期）端数処理区分1エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-次期端数処理区分2
            Master.CheckField(Master.USERCAMP, "SPRNEXTROUNDKBN2", LNM0014INProw("SPRNEXTROUNDKBN2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTROUNDKBN2")) AndAlso
                   Not LNM0014INProw("SPRNEXTROUNDKBN2") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("SPRNEXTROUNDKBN2", LNM0014INProw("SPRNEXTROUNDKBN2"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目（次期）端数処理区分2エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（次期）端数処理区分2エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '○ 項目間の整合チェック

            '発受託人コードコード入力時、発駅コードの入力が必要
            If Not String.IsNullOrEmpty(LNM0014INProw("DEPTRUSTEECD")) OrElse
               Not String.IsNullOrEmpty(LNM0014INProw("DEPTRUSTEESUBCD")) Then
                If String.IsNullOrEmpty(LNM0014INProw("DEPSTATION")) OrElse
                LNM0014INProw("DEPSTATION") = "0" Then
                    WW_CheckMES1 = "・発受託人コード＆発受託人サブコードエラーです。"
                    WW_CheckMES2 = "発駅コードを入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '着受託人コードコード入力時、着駅コードの入力が必要
            If Not String.IsNullOrEmpty(LNM0014INProw("ARRTRUSTEECD")) OrElse
               Not String.IsNullOrEmpty(LNM0014INProw("ARRTRUSTEESUBCD")) Then
                If String.IsNullOrEmpty(LNM0014INProw("ARRSTATION")) OrElse
                LNM0014INProw("ARRSTATION") = "0" Then
                    WW_CheckMES1 = "・着受託人コード＆着受託人サブコードエラーです。"
                    WW_CheckMES2 = "着駅コードを入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '着受託人コード入力時、着受託人サブコードの入力が必要
            If Not String.IsNullOrEmpty(LNM0014INProw("ARRTRUSTEECD")) Then
                If String.IsNullOrEmpty(LNM0014INProw("ARRTRUSTEESUBCD")) Then
                    WW_CheckMES1 = "・着受託人サブコードエラーです。"
                    WW_CheckMES2 = "着受託人サブコードを入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '着受託人サブコード入力時、着受託人コードの入力が必要
            If Not String.IsNullOrEmpty(LNM0014INProw("ARRTRUSTEESUBCD")) Then
                If String.IsNullOrEmpty(LNM0014INProw("ARRTRUSTEECD")) Then
                    WW_CheckMES1 = "・着受託人コードエラーです。"
                    WW_CheckMES2 = "着受託人コードを入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '現行開始適用日、現行終了適用日の大小関係チェック
            If LNM0014INProw("SPRCURSTYMD") > LNM0014INProw("SPRCURENDYMD") Then
                WW_CheckMES1 = "・特例置換項目（現行）開始適用日＆特例置換項目（現行）終了適用日エラーです。"
                WW_CheckMES2 = "大小入力エラー"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '現行端数処理区分1、現行端数処理区分2のチェック
            If Not String.IsNullOrEmpty(LNM0014INProw("SPRCURROUNDKBN1")) AndAlso
               Not LNM0014INProw("SPRCURROUNDKBN1") = "0" Then
                If String.IsNullOrEmpty(LNM0014INProw("SPRCURROUNDKBN2")) OrElse
                    LNM0014INProw("SPRCURROUNDKBN2") = "0" Then
                    WW_CheckMES1 = "・特例置換項目（現行）端数処理区分1＆特例置換項目（現行）端数処理区分2エラーです。"
                    WW_CheckMES2 = "端数処理区分1を入力する場合、端数処理区分2も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                If Not String.IsNullOrEmpty(LNM0014INProw("SPRCURROUNDKBN2")) AndAlso
                   Not LNM0014INProw("SPRCURROUNDKBN2") = "0" Then
                    WW_CheckMES1 = "・特例置換項目（現行）端数処理区分1＆特例置換項目（現行）端数処理区分2エラーです。"
                    WW_CheckMES2 = "端数処理区分2を入力する場合、端数処理区分1も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '次期開始適用日入力時、次期終了適用日未入力はエラー
            If Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTSTYMD")) AndAlso
                 String.IsNullOrEmpty(LNM0014INProw("SPRNEXTENDYMD")) Then
                WW_CheckMES1 = "・特例置換項目（次期）終了適用日エラーです。"
                WW_CheckMES2 = "開始適用日を入力する場合、終了適用日も入力して下さい。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '次期終了適用日入力時、次期開始適用日未入力はエラー
            If Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTENDYMD")) AndAlso
                 String.IsNullOrEmpty(LNM0014INProw("SPRNEXTSTYMD")) Then
                WW_CheckMES1 = "・特例置換項目（次期）開始適用日エラーです。"
                WW_CheckMES2 = "終了適用日を入力する場合、開始適用日も入力して下さい。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '次期開始適用日、次期終了適用日の大小関係チェック
            If LNM0014INProw("SPRNEXTSTYMD") > LNM0014INProw("SPRNEXTENDYMD") Then
                WW_CheckMES1 = "・特例置換項目（次期）開始適用日＆特例置換項目（次期）終了適用日エラーです。"
                WW_CheckMES2 = "大小入力エラー"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '次期端数処理区分1、次期端数処理区分2のチェック
            If Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTROUNDKBN1")) AndAlso
               Not LNM0014INProw("SPRNEXTROUNDKBN1") = "0" Then
                If String.IsNullOrEmpty(LNM0014INProw("SPRNEXTROUNDKBN2")) OrElse
                    LNM0014INProw("SPRNEXTROUNDKBN2") = "0" Then
                    WW_CheckMES1 = "・特例置換項目（次期）端数処理区分1＆特例置換項目（次期）端数処理区分2エラーです。"
                    WW_CheckMES2 = "端数処理区分1を入力する場合、端数処理区分2も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                If Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTROUNDKBN2")) AndAlso
                   Not LNM0014INProw("SPRNEXTROUNDKBN2") = "0" Then
                    WW_CheckMES1 = "・特例置換項目（次期）端数処理区分1＆特例置換項目（次期）端数処理区分2エラーです。"
                    WW_CheckMES2 = "端数処理区分2を入力する場合、端数処理区分1も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '現行終了適用日、次期開始適用日の大小関係チェック
            If Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTSTYMD")) Then
                If LNM0014INProw("SPRCURENDYMD") > LNM0014INProw("SPRNEXTSTYMD") Then
                    WW_CheckMES1 = "・特例置換項目（現行）終了適用日＆特例置換項目（次期）開始適用日エラーです。"
                    WW_CheckMES2 = "大小入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '次期適用日を入力する場合、次期発送料または到着料も入力が必要
            If Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTSTYMD")) OrElse
               Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTENDYMD")) Then
                If String.IsNullOrEmpty(LNM0014INProw("SPRNEXTSHIPFEE")) OrElse
                   LNM0014INProw("SPRNEXTARRIVEFEE") = "0" Then
                    WW_CheckMES1 = "・特例置換項目（次期）適用率エラーです。"
                    WW_CheckMES2 = "次期適用日を入力する場合、次期発送料または到着料も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '次期発送料または到着料を入力する場合、次期適用日も入力が必要
            If Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTSHIPFEE")) AndAlso
               Not LNM0014INProw("SPRNEXTSHIPFEE") = "0" OrElse
               Not String.IsNullOrEmpty(LNM0014INProw("SPRNEXTARRIVEFEE")) AndAlso
               Not LNM0014INProw("SPRNEXTARRIVEFEE") = "0" Then
                If String.IsNullOrEmpty(LNM0014INProw("SPRNEXTSTYMD")) OrElse
                        String.IsNullOrEmpty(LNM0014INProw("SPRNEXTENDYMD")) Then
                    WW_CheckMES1 = "・特例置換項目（次期）開始適用日＆特例置換項目（次期）終了適用日エラーです。"
                    WW_CheckMES2 = "次期適用率を入力する場合、次期開始適用日及び次期終了適用日も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            ' 排他チェック
            If Not work.WF_SEL_BIGCTNCD2.Text = "" Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                    txtBigCtnCd.Text, txtMiddleCtnCd.Text,
                                    txtDepStation.Text, txtDepTrusteeCd.Text,
                                    txtDepTrusteeSubCd.Text, txtPriorityNo.Text,
                                    work.WF_SEL_UPDTIMSTP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（大分類コード & 中分類コード & 発駅コード & 発受託人コード & 発受託人サブコード & 優先順位）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0014INProw("BIGCTNCD") & "]" &
                                           " [" & LNM0014INProw("MIDDLECTNCD") & "]" &
                                           " [" & LNM0014INProw("DEPSTATION") & "]" &
                                           " [" & LNM0014INProw("DEPTRUSTEECD") & "]" &
                                           " [" & LNM0014INProw("DEPTRUSTEESUBCD") & "]" &
                                           " [" & LNM0014INProw("PRIORITYNO") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNM0014INProw("BIGCTNCD") = work.WF_SEL_BIGCTNCD2.Text OrElse
               Not LNM0014INProw("MIDDLECTNCD") = work.WF_SEL_MIDDLECTNCD2.Text OrElse
               Not LNM0014INProw("DEPSTATION") = work.WF_SEL_DEPSTATION2.Text OrElse
               Not LNM0014INProw("DEPTRUSTEECD") = work.WF_SEL_DEPTRUSTEECD2.Text OrElse
               Not LNM0014INProw("DEPTRUSTEESUBCD") = work.WF_SEL_DEPTRUSTEESUBCD2.Text OrElse
               Not LNM0014INProw("PRIORITYNO") = work.WF_SEL_PRIORITYNO.Text Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（大分類コード & 中分類コード & 発駅コード & 発受託人コード & 発受託人サブコード & 優先順位）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                           "([" & LNM0014INProw("BIGCTNCD") & "]" &
                                           " [" & LNM0014INProw("MIDDLECTNCD") & "]" &
                                           " [" & LNM0014INProw("DEPSTATION") & "]" &
                                           " [" & LNM0014INProw("DEPTRUSTEECD") & "]" &
                                           " [" & LNM0014INProw("DEPTRUSTEESUBCD") & "]" &
                                           " [" & LNM0014INProw("PRIORITYNO") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LineErr = "" Then
                If LNM0014INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0014INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0014INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0014tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0014tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0014row As DataRow In LNM0014tbl.Rows
            Select Case LNM0014row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0014INProw As DataRow In LNM0014INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0014INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0014INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0014row As DataRow In LNM0014tbl.Rows
                ' KEY項目が等しい時
                If LNM0014row("BIGCTNCD") = LNM0014INProw("BIGCTNCD") AndAlso
                   LNM0014row("MIDDLECTNCD") = LNM0014INProw("MIDDLECTNCD") AndAlso
                   LNM0014row("DEPSTATION") = LNM0014INProw("DEPSTATION") AndAlso
                   LNM0014row("DEPTRUSTEECD") = LNM0014INProw("DEPTRUSTEECD") AndAlso
                   LNM0014row("DEPTRUSTEESUBCD") = LNM0014INProw("DEPTRUSTEESUBCD") AndAlso
                   LNM0014row("PRIORITYNO") = LNM0014INProw("PRIORITYNO") Then
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0014row("PURPOSE") = LNM0014INProw("PURPOSE") AndAlso
                        LNM0014row("DEPTRUSTEECD") = LNM0014INProw("DEPTRUSTEECD") AndAlso
                        LNM0014row("ARRSTATION") = LNM0014INProw("ARRSTATION") AndAlso
                        LNM0014row("ARRTRUSTEECD") = LNM0014INProw("ARRTRUSTEECD") AndAlso
                        LNM0014row("ARRTRUSTEESUBCD") = LNM0014INProw("ARRTRUSTEESUBCD") AndAlso
                        LNM0014row("SPRCURSTYMD") = LNM0014INProw("SPRCURSTYMD") AndAlso
                        LNM0014row("SPRCURENDYMD") = LNM0014INProw("SPRCURENDYMD") AndAlso
                        LNM0014row("SPRCURSHIPFEE") = LNM0014INProw("SPRCURSHIPFEE") AndAlso
                        LNM0014row("SPRCURARRIVEFEE") = LNM0014INProw("SPRCURARRIVEFEE") AndAlso
                        LNM0014row("SPRCURROUNDKBN1") = LNM0014INProw("SPRCURROUNDKBN1") AndAlso
                        LNM0014row("SPRCURROUNDKBN2") = LNM0014INProw("SPRCURROUNDKBN2") AndAlso
                        LNM0014row("SPRNEXTSTYMD") = LNM0014INProw("SPRNEXTSTYMD") AndAlso
                        LNM0014row("SPRNEXTENDYMD") = LNM0014INProw("SPRNEXTENDYMD") AndAlso
                        LNM0014row("SPRNEXTSHIPFEE") = LNM0014INProw("SPRNEXTSHIPFEE") AndAlso
                        LNM0014row("SPRNEXTARRIVEFEE") = LNM0014INProw("SPRNEXTARRIVEFEE") AndAlso
                        LNM0014row("SPRNEXTROUNDKBN1") = LNM0014INProw("SPRNEXTROUNDKBN1") AndAlso
                        LNM0014row("SPRNEXTROUNDKBN2") = LNM0014INProw("SPRNEXTROUNDKBN2") AndAlso
                        LNM0014row("DELFLG") = LNM0014INProw("DELFLG") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0014row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0014INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0014INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0014INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0014INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now

                '変更チェック
                REUTRMEXISTS(SQLcon, WW_MODIFYKBN)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.AFTDATA
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
        For Each LNM0014INProw As DataRow In LNM0014INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0014row As DataRow In LNM0014tbl.Rows
                ' 同一レコードか判定
                If LNM0014INProw("BIGCTNCD") = LNM0014row("BIGCTNCD") AndAlso
                    LNM0014INProw("MIDDLECTNCD") = LNM0014row("MIDDLECTNCD") AndAlso
                    LNM0014INProw("DEPSTATION") = LNM0014row("DEPSTATION") AndAlso
                    LNM0014INProw("DEPTRUSTEECD") = LNM0014row("DEPTRUSTEECD") AndAlso
                    LNM0014INProw("DEPTRUSTEESUBCD") = LNM0014row("DEPTRUSTEESUBCD") AndAlso
                    LNM0014INProw("PRIORITYNO") = LNM0014row("PRIORITYNO") Then
                    ' 画面入力テーブル項目設定
                    LNM0014INProw("LINECNT") = LNM0014row("LINECNT")
                    LNM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0014INProw("UPDTIMSTP") = LNM0014row("UPDTIMSTP")
                    LNM0014INProw("SELECT") = 0
                    LNM0014INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0014row.ItemArray = LNM0014INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0014tbl.NewRow
                WW_NRow.ItemArray = LNM0014INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0014tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0014tbl.Rows.Add(WW_NRow)
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
                Case "BIGCTNCD"                   '大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS))
                Case "MIDDLECTNCD"                '中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, txtBigCtnCd.Text))
                Case "DEPSTATION"                 '発駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "DEPTRUSTEECD"               '発受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, txtDepStation.Text))
                Case "DEPTRUSTEESUBCD"            '発受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, txtDepStation.Text, txtDepTrusteeCd.Text))
                Case "ARRSTATION"                 '着駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "ARRTRUSTEECD"               '着受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, txtArrstation.Text))
                Case "ARRTRUSTEESUBCD"            '着受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, txtArrstation.Text, txtArrTrusteeCd.Text))
                Case "SPRCURROUNDKBN1"             '特例置換項目-現行端数処理区分1
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "HASUU1"))
                Case "SPRCURROUNDKBN2"             '特例置換項目-現行端数処理区分2
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "HASUU2"))
                Case "SPRNEXTROUNDKBN1"            '特例置換項目-次期端数処理区分1
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "HASUU1"))
                Case "SPRNEXTROUNDKBN2"            '特例置換項目-次期端数処理区分2
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "HASUU2"))
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "OUTPUTID"         '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))
                Case "ONOFF"            '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
