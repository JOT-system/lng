''************************************************************
' 回送運賃適用率マスタメンテ登録画面
' 作成日 2022/02/18
' 更新日 
' 作成者 瀬口
' 更新者 大浜
'
' 修正履歴 : 2022/02/18 新規作成
'          : 2024/01/15 変更履歴登録機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 回送運賃適用率マスタメンテ（詳細）
''' </summary>
''' <remarks></remarks>
Public Class LNM0013RektrmDetail
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private LNM0013tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0013INPtbl As DataTable                              'チェック用テーブル
    Private LNM0013UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0013tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(LNM0013tbl) Then
                LNM0013tbl.Clear()
                LNM0013tbl.Dispose()
                LNM0013tbl = Nothing
            End If

            If Not IsNothing(LNM0013INPtbl) Then
                LNM0013INPtbl.Clear()
                LNM0013INPtbl.Dispose()
                LNM0013INPtbl = Nothing
            End If

            If Not IsNothing(LNM0013UPDtbl) Then
                LNM0013UPDtbl.Clear()
                LNM0013UPDtbl.Dispose()
                LNM0013UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0013WRKINC.MAPIDD
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0013L Then
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
        TxtMapId.Text = "LNM0013D"

        '大分類コード
        TxtBigCTNCD.Text = work.WF_SEL_BIGCTNCD2.Text
        CODENAME_get("BIGCTNCD", TxtBigCTNCD.Text, LblBigCTNCDName.Text, WW_Dummy)

        '中分類コード
        TxtMiddleCTNCD.Text = work.WF_SEL_MIDDLECTNCD2.Text
        CODENAME_get("MIDDLECTNCD", TxtMiddleCTNCD.Text, LblMiddleCTNCDName.Text, WW_Dummy)

        '優先順位
        TxtPriorityNO.Text = work.WF_SEL_PRIORITYNO.Text

        '発駅コード
        TxtDepstation.Text = work.WF_SEL_DEPSTATION.Text
        CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationCDName.Text, WW_Dummy)

        'ＪＲ発支社支店コード
        TxtJrDepBranchCD.Text = work.WF_SEL_JRDEPBRANCHCD.Text
        CODENAME_get("JRBRANCHCD", TxtJrDepBranchCD.Text, LblJrDepBranchCDName.Text, WW_Dummy)

        '着駅コード
        TxtArrstation.Text = work.WF_SEL_ARRSTATION.Text
        CODENAME_get("ARRSTATION", TxtArrstation.Text, LblArrstationName.Text, WW_Dummy)

        'ＪＲ着支社支店コード
        TxtJrArrBranchCD.Text = work.WF_SEL_JRARRBRANCHCD.Text
        CODENAME_get("JRBRANCHCD", TxtJrArrBranchCD.Text, LblJrArrBranchCDName.Text, WW_Dummy)

        '使用目的
        TxtPurpose.Text = work.WF_SEL_PURPOSE.Text

        '発受託人コード
        TxtDepTrusteeCD.Text = work.WF_SEL_DEPTRUSTEECD.Text
        CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCD.Text, LblDepTrusteeCDName.Text, WW_Dummy)

        '発受託人サブコード
        TxtDepTrusteeSubCD.Text = work.WF_SEL_DEPTRUSTEESUBCD.Text
        CODENAME_get("DEPTRUSTEESUBCD", TxtDepTrusteeSubCD.Text, LblDepTrusteeSubCDName.Text, WW_Dummy)

        'コンテナ記号
        TxtCTNType.Text = work.WF_SEL_CTNTYPE.Text
        CODENAME_get("CTNTYPE", TxtCTNType.Text, LblCTNTypeName.Text, WW_Dummy)

        'コンテナ番号（開始）
        TxtCTNStNO.Text = work.WF_SEL_CTNSTNO.Text
        '      CODENAME_get("CTNSTNO", TxtCTNStNO.Text, LblCTNStNOName.Text, WW_Dummy)

        'コンテナ番号（終了）
        TxtCTNEndNO.Text = work.WF_SEL_CTNENDNO.Text
        '     CODENAME_get("CTNENDNO", TxtCTNEndNO.Text, LblCTNEndNOName.Text, WW_Dummy)

        '特例置換項目-現行開始適用日
        If Not String.IsNullOrEmpty(work.WF_SEL_SPRCURSTYMD.Text) Then
            WW_ChkDate = Integer.Parse(work.WF_SEL_SPRCURSTYMD.Text.Replace("/", ""))
            If WW_ChkDate < 9999 Then
                TxtSprCurStYMD.Text = work.WF_SEL_SPRCURSTYMD.Text
            Else
                TxtSprCurStYMD.Text = WW_ChkDate.ToString("0000/00/00")
            End If
        Else
            TxtSprCurStYMD.Text = work.WF_SEL_SPRCURSTYMD.Text
        End If

        '特例置換項目-現行終了適用日
        If Not String.IsNullOrEmpty(work.WF_SEL_SPRCURENDYMD.Text) Then
            WW_ChkDate = Integer.Parse(work.WF_SEL_SPRCURENDYMD.Text.Replace("/", ""))
            If WW_ChkDate < 9999 Then
                TxtSprCurEndYMD.Text = work.WF_SEL_SPRCURENDYMD.Text
            Else
                TxtSprCurEndYMD.Text = WW_ChkDate.ToString("0000/00/00")
            End If
        Else
            TxtSprCurEndYMD.Text = work.WF_SEL_SPRCURENDYMD.Text
        End If

        '特例置換項目-現行摘要率
        TxtSprCurApplyRate.Text = work.WF_SEL_SPRCURAPPLYRATE.Text

        '特例置換項目-現行端数処理区分1
        TxtSprCurRoundKbn1.Text = work.WF_SEL_SPRCURROUNDKBN1.Text
        CODENAME_get("SPRCURROUNDKBN1", TxtSprCurRoundKbn1.Text, LblSprCurRoundKbn1Name.Text, WW_Dummy)

        '特例置換項目-現行端数処理区分2
        TxtSprCurRoundKbn2.Text = work.WF_SEL_SPRCURROUNDKBN2.Text
        CODENAME_get("SPRCURROUNDKBN2", TxtSprCurRoundKbn2.Text, LblSprCurRoundKbn2Name.Text, WW_Dummy)

        '特例置換項目-次期開始適用日
        If Not String.IsNullOrEmpty(work.WF_SEL_SPRNEXTSTYMD.Text) Then
            WW_ChkDate = Integer.Parse(work.WF_SEL_SPRNEXTSTYMD.Text.Replace("/", ""))
            If WW_ChkDate < 9999 Then
                TxtSprNextStYMD.Text = work.WF_SEL_SPRNEXTSTYMD.Text
            Else
                TxtSprNextStYMD.Text = WW_ChkDate.ToString("0000/00/00")
            End If
        Else
            TxtSprNextStYMD.Text = work.WF_SEL_SPRNEXTSTYMD.Text
        End If
        '特例置換項目-次期終了適用日
        If Not String.IsNullOrEmpty(work.WF_SEL_SPRNEXTENDYMD.Text) Then
            WW_ChkDate = Integer.Parse(work.WF_SEL_SPRNEXTENDYMD.Text.Replace("/", ""))
            If WW_ChkDate < 9999 Then
                TxtSprNextEndYMD.Text = work.WF_SEL_SPRNEXTENDYMD.Text
            Else
                TxtSprNextEndYMD.Text = WW_ChkDate.ToString("0000/00/00")
            End If
        Else
            TxtSprNextEndYMD.Text = work.WF_SEL_SPRNEXTENDYMD.Text
        End If

        '特例置換項目-次期摘要率
        TxtSprNextApplyRate.Text = work.WF_SEL_SPRNEXTAPPLYRATE.Text

        '特例置換項目-次期端数処理区分1
        TxtSprNextRoundKbn1.Text = work.WF_SEL_SPRNEXTROUNDKBN1.Text
        CODENAME_get("SPRNEXTROUNDKBN1", TxtSprNextRoundKbn1.Text, LblSprNextRoundKbn1Name.Text, WW_Dummy)

        '特例置換項目-次期端数処理区分2
        TxtSprNextRoundKbn2.Text = work.WF_SEL_SPRNEXTROUNDKBN2.Text
        CODENAME_get("SPRNEXTROUNDKBN2", TxtSprNextRoundKbn2.Text, LblSprNextRoundKbn2Name.Text, WW_Dummy)

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_BIGCTNCD2.Text

        ' 数値(0～9)のみ入力可能とする。
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"                   '削除フラグ
        Me.TxtBigCTNCD.Attributes("onkeyPress") = "CheckNum()"                 '大分類コード
        Me.TxtMiddleCTNCD.Attributes("onkeyPress") = "CheckNum()"              '中分類コード
        Me.TxtPriorityNO.Attributes("onkeyPress") = "CheckNum()"               '優先順位
        Me.TxtDepstation.Attributes("onkeyPress") = "CheckNum()"               '発駅コード
        Me.TxtJrDepBranchCD.Attributes("onkeyPress") = "CheckNum()"            'ＪＲ発支社支店コード
        Me.TxtArrstation.Attributes("onkeyPress") = "CheckNum()"               '着駅コード
        Me.TxtJrArrBranchCD.Attributes("onkeyPress") = "CheckNum()"            'ＪＲ着支社支店コード
        Me.TxtCTNStNO.Attributes("onkeyPress") = "CheckNum()"                  'コンテナ番号（開始）
        Me.TxtCTNEndNO.Attributes("onkeyPress") = "CheckNum()"                 'コンテナ番号（終了）
        Me.TxtSprCurApplyRate.Attributes("onkeyPress") = "CheckDeci()"         '特例置換項目-現行適用率
        Me.TxtSprNextApplyRate.Attributes("onkeyPress") = "CheckDeci()"        '特例置換項目-次期適用率

        ' 数値(0～9)＋記号(/)のみ入力可能とする。
        Me.TxtSprCurStYMD.Attributes("onkeyPress") = "CheckCalendar()"         '特例置換項目-現行開始適用日
        Me.TxtSprCurEndYMD.Attributes("onkeyPress") = "CheckCalendar()"        '特例置換項目-現行終了適用日
        Me.TxtSprNextStYMD.Attributes("onkeyPress") = "CheckCalendar()"        '特例置換項目-次期開始適用日
        Me.TxtSprNextEndYMD.Attributes("onkeyPress") = "CheckCalendar()"       '特例置換項目-次期終了適用日

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
            & "   , PRIORITYNO                 " _
            & "   , DEPSTATION                 " _
            & "   , JRDEPBRANCHCD              " _
            & "   , ARRSTATION                 " _
            & "   , JRARRBRANCHCD              " _
            & " FROM                           " _
            & "     LNG.LNM0013_REKTRM         " _
            & " WHERE                          " _
            & "         BIGCTNCD        = @P1  " _
            & "     AND MIDDLECTNCD     = @P2  " _
            & "     AND PRIORITYNO      = @P3  " _
            & "     AND DEPSTATION      = @P4  " _
            & "     AND JRDEPBRANCHCD   = @P5  " _
            & "     AND ARRSTATION      = @P6  " _
            & "     AND JRARRBRANCHCD   = @P7  " _
            & "     AND DELFLG         <> @P8  "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 2) '大分類コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 2) '中分類コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 5) '優先順位
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 6) '発駅コード
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.Int32)         'ＪＲ発支社支店コード
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@P6", MySqlDbType.VarChar, 6) '着駅コード
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@P7", MySqlDbType.Int32)         'ＪＲ着支社支店コード
                Dim PARA8 As MySqlParameter = SQLcmd.Parameters.Add("@P8", MySqlDbType.VarChar, 1) '削除フラグ

                PARA1.Value = TxtBigCTNCD.Text
                PARA2.Value = TxtMiddleCTNCD.Text
                If Not TxtPriorityNO.Text = "" Then
                    PARA3.Value = TxtPriorityNO.Text
                Else
                    PARA3.Value = "0"
                End If
                If Not TxtDepstation.Text = "" Then
                    PARA4.Value = TxtDepstation.Text
                Else
                    PARA4.Value = "0"
                End If
                If Not TxtJrDepBranchCD.Text = "" Then
                    PARA5.Value = CInt(TxtJrDepBranchCD.Text)
                Else
                    PARA5.Value = "0"
                End If
                If Not TxtArrstation.Text = "" Then
                    PARA6.Value = TxtArrstation.Text
                Else
                    PARA6.Value = "0"
                End If
                If Not TxtJrArrBranchCD.Text = "" Then
                    PARA7.Value = CInt(TxtJrArrBranchCD.Text)
                Else
                    PARA7.Value = "0"
                End If
                PARA8.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0013Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0013Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0013Chk.Load(SQLdr)

                    If LNM0013Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0013D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 回送運賃適用率マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(回送運賃適用率マスタ)
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;                 " _
            & "     SET @hensuu = 0 ;                       " _
            & " DECLARE hensuu CURSOR FOR                   " _
            & "     SELECT                                  " _
            & "         UPDTIMSTP AS hensuu                 " _
            & "     FROM                                    " _
            & "         LNG.LNM0013_REKTRM                  " _
            & "     WHERE                                   " _
            & "             BIGCTNCD           = @P01       " _
            & "         AND MIDDLECTNCD        = @P02       " _
            & "         AND PRIORITYNO         = @P03       " _
            & "         AND DEPSTATION         = @P04       " _
            & "         AND JRDEPBRANCHCD      = @P05       " _
            & "         AND ARRSTATION         = @P06       " _
            & "         AND JRARRBRANCHCD      = @P07 ;     " _
            & " OPEN hensuu ;                               " _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;       " _
            & " IF (@@FETCH_STATUS = 0)                     " _
            & "     UPDATE LNG.LNM0013_REKTRM               " _
            & "     SET                                     " _
            & "         PURPOSE            = @P08           " _
            & "       , DEPTRUSTEECD       = @P09           " _
            & "       , DEPTRUSTEESUBCD    = @P10           " _
            & "       , CTNTYPE            = @P11           " _
            & "       , CTNSTNO            = @P12           " _
            & "       , CTNENDNO           = @P13           " _
            & "       , SPRCURSTYMD        = @P14           " _
            & "       , SPRCURENDYMD       = @P15           " _
            & "       , SPRCURAPPLYRATE    = @P16           " _
            & "       , SPRCURROUNDKBN     = @P17           " _
            & "       , SPRNEXTSTYMD       = @P18           " _
            & "       , SPRNEXTENDYMD      = @P19           " _
            & "       , SPRNEXTAPPLYRATE   = @P20           " _
            & "       , SPRNEXTROUNDKBN    = @P21           " _
            & "       , DELFLG             = @P22           " _
            & "       , UPDYMD             = @P27           " _
            & "       , UPDUSER            = @P28           " _
            & "       , UPDTERMID          = @P29           " _
            & "       , UPDPGID            = @P30           " _
            & "     WHERE                                   " _
            & "             BIGCTNCD           = @P01       " _
            & "         AND MIDDLECTNCD        = @P02       " _
            & "         AND PRIORITYNO         = @P03       " _
            & "         AND DEPSTATION         = @P04       " _
            & "         AND JRDEPBRANCHCD      = @P05       " _
            & "         AND ARRSTATION         = @P06       " _
            & "         AND JRARRBRANCHCD      = @P07 ;     " _
            & " IF (@@FETCH_STATUS <> 0)                    " _
            & "     INSERT INTO LNG.LNM0013_REKTRM          " _
            & "        (BIGCTNCD                            " _
            & "       , MIDDLECTNCD                         " _
            & "       , PRIORITYNO                          " _
            & "       , DEPSTATION                          " _
            & "       , JRDEPBRANCHCD                       " _
            & "       , ARRSTATION                          " _
            & "       , JRARRBRANCHCD                       " _
            & "       , PURPOSE                             " _
            & "       , DEPTRUSTEECD                        " _
            & "       , DEPTRUSTEESUBCD                     " _
            & "       , CTNTYPE                             " _
            & "       , CTNSTNO                             " _
            & "       , CTNENDNO                            " _
            & "       , SPRCURSTYMD                         " _
            & "       , SPRCURENDYMD                        " _
            & "       , SPRCURAPPLYRATE                     " _
            & "       , SPRCURROUNDKBN                      " _
            & "       , SPRNEXTSTYMD                        " _
            & "       , SPRNEXTENDYMD                       " _
            & "       , SPRNEXTAPPLYRATE                    " _
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
            & "       , @P25                                " _
            & "       , @P26) ;                             " _
            & " CLOSE hensuu ;                              " _
            & " DEALLOCATE hensuu ;                         "

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                      " _
            & "     DELFLG                                  " _
            & "   , BIGCTNCD                                " _
            & "   , MIDDLECTNCD                             " _
            & "   , PRIORITYNO                              " _
            & "   , DEPSTATION                              " _
            & "   , JRDEPBRANCHCD                           " _
            & "   , ARRSTATION                              " _
            & "   , JRARRBRANCHCD                           " _
            & "   , PURPOSE                                 " _
            & "   , DEPTRUSTEECD                            " _
            & "   , DEPTRUSTEESUBCD                         " _
            & "   , CTNTYPE                                 " _
            & "   , CTNSTNO                                 " _
            & "   , CTNENDNO                                " _
            & "   , SPRCURSTYMD                             " _
            & "   , SPRCURENDYMD                            " _
            & "   , SPRCURAPPLYRATE                         " _
            & "   , SPRCURROUNDKBN                          " _
            & "   , SPRNEXTSTYMD                            " _
            & "   , SPRNEXTENDYMD                           " _
            & "   , SPRNEXTAPPLYRATE                        " _
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
            & "     LNG.LNM0013_REKTRM                      " _
            & " WHERE                                       " _
            & "             BIGCTNCD           = @P01       " _
            & "         AND MIDDLECTNCD        = @P02       " _
            & "         AND PRIORITYNO         = @P03       " _
            & "         AND DEPSTATION         = @P04       " _
            & "         AND JRDEPBRANCHCD      = @P05       " _
            & "         AND ARRSTATION         = @P06       " _
            & "         AND JRARRBRANCHCD      = @P07       "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                ' DB更新用パラメータ
                Dim PARA001 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 2)       '大分類コード                 
                Dim PARA002 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 2)       '中分類コード                 
                Dim PARA003 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 5)       '優先順位                     
                Dim PARA004 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar, 6)       '発駅コード                   
                Dim PARA005 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.Int32)               'ＪＲ発支社支店コード         
                Dim PARA006 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar, 6)       '着駅コード                   
                Dim PARA007 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.Int32)               'ＪＲ着支社支店コード         
                Dim PARA008 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.VarChar, 42)      '使用目的                     
                Dim PARA009 As MySqlParameter = SQLcmd.Parameters.Add("@P09", MySqlDbType.VarChar, 5)       '発受託人コード               
                Dim PARA010 As MySqlParameter = SQLcmd.Parameters.Add("@P10", MySqlDbType.VarChar, 3)       '発受託人サブコード           
                Dim PARA011 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.VarChar, 5)       'コンテナ記号                 
                Dim PARA012 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.VarChar, 8)       'コンテナ番号（開始）         
                Dim PARA013 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar, 8)       'コンテナ番号（終了）         
                Dim PARA014 As MySqlParameter = SQLcmd.Parameters.Add("@P14", MySqlDbType.VarChar, 8)       '特例置換項目-現行開始適用日  
                Dim PARA015 As MySqlParameter = SQLcmd.Parameters.Add("@P15", MySqlDbType.VarChar, 8)       '特例置換項目-現行終了適用日  
                Dim PARA016 As MySqlParameter = SQLcmd.Parameters.Add("@P16", MySqlDbType.Decimal, 5, 4)     '特例置換項目-現行適用率      
                Dim PARA017 As MySqlParameter = SQLcmd.Parameters.Add("@P17", MySqlDbType.VarChar, 2)       '特例置換項目-現行端数処理区分
                Dim PARA018 As MySqlParameter = SQLcmd.Parameters.Add("@P18", MySqlDbType.VarChar, 8)       '特例置換項目-次期開始適用日  
                Dim PARA019 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.VarChar, 8)       '特例置換項目-次期終了適用日  
                Dim PARA020 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.Decimal, 5, 4)     '特例置換項目-次期適用率      
                Dim PARA021 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.VarChar, 2)       '特例置換項目-次期端数処理区分
                Dim PARA022 As MySqlParameter = SQLcmd.Parameters.Add("@P22", MySqlDbType.VarChar, 1)       '削除フラグ                   
                Dim PARA023 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.Date)              '登録年月日                   
                Dim PARA024 As MySqlParameter = SQLcmd.Parameters.Add("@P24", MySqlDbType.VarChar, 20)      '登録ユーザーＩＤ             
                Dim PARA025 As MySqlParameter = SQLcmd.Parameters.Add("@P25", MySqlDbType.VarChar, 20)      '登録端末                     
                Dim PARA026 As MySqlParameter = SQLcmd.Parameters.Add("@P26", MySqlDbType.VarChar, 40)      '登録プログラムＩＤ           
                Dim PARA027 As MySqlParameter = SQLcmd.Parameters.Add("@P27", MySqlDbType.Date)              '更新年月日                   
                Dim PARA028 As MySqlParameter = SQLcmd.Parameters.Add("@P28", MySqlDbType.VarChar, 20)      '更新ユーザーＩＤ             
                Dim PARA029 As MySqlParameter = SQLcmd.Parameters.Add("@P29", MySqlDbType.VarChar, 20)      '更新端末                     
                Dim PARA030 As MySqlParameter = SQLcmd.Parameters.Add("@P30", MySqlDbType.VarChar, 40)      '更新プログラムＩＤ           

                ' 更新ジャーナル出力用パラメータ
                Dim JPARA001 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 2)      '大分類コード                 
                Dim JPARA002 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P02", MySqlDbType.VarChar, 2)      '中分類コード                 
                Dim JPARA003 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P03", MySqlDbType.VarChar, 5)      '優先順位                     
                Dim JPARA004 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P04", MySqlDbType.VarChar, 6)      '発駅コード                   
                Dim JPARA005 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P05", MySqlDbType.Int32)              'ＪＲ発支社支店コード         
                Dim JPARA006 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P06", MySqlDbType.VarChar, 6)      '着駅コード                   
                Dim JPARA007 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P07", MySqlDbType.Int32)              'ＪＲ着支社支店コード         

                Dim LNM0013row As DataRow = LNM0013INPtbl.Rows(0)

                'Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA001.Value = LNM0013row("BIGCTNCD")                                           '大分類コード                 
                PARA002.Value = LNM0013row("MIDDLECTNCD")                                        '中分類コード                 
                PARA003.Value = LNM0013row("PRIORITYNO")                                         '優先順位                     
                PARA004.Value = LNM0013row("DEPSTATION")                                         '発駅コード                   

                If String.IsNullOrEmpty(LNM0013row("JRDEPBRANCHCD")) Then
                    PARA005.Value = 0                                                            'ＪＲ発支社支店コード         
                Else
                    PARA005.Value = CInt(LNM0013row("JRDEPBRANCHCD"))                            'ＪＲ発支社支店コード         
                End If

                PARA006.Value = LNM0013row("ARRSTATION")                                         '着駅コード                   

                If String.IsNullOrEmpty(LNM0013row("JRARRBRANCHCD")) Then
                    PARA007.Value = 0                                                            'ＪＲ着支社支店コード         
                Else
                    PARA007.Value = CInt(LNM0013row("JRARRBRANCHCD"))                            'ＪＲ着支社支店コード         
                End If

                PARA008.Value = LNM0013row("PURPOSE")                                            '使用目的                     

                If String.IsNullOrEmpty(LNM0013row("DEPTRUSTEECD")) Then
                    PARA009.Value = DBNull.Value                                                 '発受託人コード               
                Else
                    PARA009.Value = LNM0013row("DEPTRUSTEECD")                                   '発受託人コード               
                End If

                If String.IsNullOrEmpty(LNM0013row("DEPTRUSTEESUBCD")) Then
                    PARA010.Value = DBNull.Value                                                 '発受託人サブコード           
                Else
                    PARA010.Value = LNM0013row("DEPTRUSTEESUBCD")                                '発受託人サブコード           
                End If

                If String.IsNullOrEmpty(LNM0013row("CTNTYPE")) Then
                    PARA011.Value = DBNull.Value                                                 'コンテナ記号                 
                Else
                    PARA011.Value = LNM0013row("CTNTYPE")                                        'コンテナ記号                 
                End If

                If String.IsNullOrEmpty(LNM0013row("CTNSTNO")) Then
                    PARA012.Value = DBNull.Value                                                 'コンテナ番号（開始）
                Else
                    PARA012.Value = LNM0013row("CTNSTNO")                                        'コンテナ番号（開始）
                End If

                If String.IsNullOrEmpty(LNM0013row("CTNENDNO")) Then
                    PARA013.Value = DBNull.Value                                                 'コンテナ番号（終了）
                Else
                    PARA013.Value = LNM0013row("CTNENDNO")                                       'コンテナ番号（終了）
                End If

                If String.IsNullOrEmpty(LNM0013row("SPRCURSTYMD").Replace("/", "")) Then
                    PARA014.Value = DBNull.Value                                                 '特例置換項目-現行開始適用日  
                Else
                    PARA014.Value = LNM0013row("SPRCURSTYMD").Replace("/", "")                   '特例置換項目-現行開始適用日  
                End If

                If String.IsNullOrEmpty(LNM0013row("SPRCURENDYMD").Replace("/", "")) Then
                    PARA015.Value = DBNull.Value                                                 '特例置換項目-現行終了適用日  
                Else
                    PARA015.Value = LNM0013row("SPRCURENDYMD").Replace("/", "")                  '特例置換項目-現行終了適用日  
                End If

                If String.IsNullOrEmpty(LNM0013row("SPRCURAPPLYRATE")) Then
                    PARA016.Value = "0"                                                          '特例置換項目-現行適用率      
                Else
                    PARA016.Value = LNM0013row("SPRCURAPPLYRATE")                                '特例置換項目-現行適用率      
                End If

                If Not String.IsNullOrEmpty(LNM0013row("SPRCURROUNDKBN1")) AndAlso
                   Not LNM0013row("SPRCURROUNDKBN1") = "0" AndAlso
                   Not String.IsNullOrEmpty(LNM0013row("SPRCURROUNDKBN2")) AndAlso
                   Not LNM0013row("SPRCURROUNDKBN2") = "0" Then
                    PARA017.Value = LNM0013row("SPRCURROUNDKBN1") & LNM0013row("SPRCURROUNDKBN2") '特例置換項目-現行端数処理区分
                Else
                    PARA017.Value = "0"
                End If

                If String.IsNullOrEmpty(LNM0013row("SPRNEXTSTYMD").Replace("/", "")) Then
                    PARA018.Value = DBNull.Value                                                  '特例置換項目-次期開始適用日  
                Else
                    PARA018.Value = LNM0013row("SPRNEXTSTYMD").Replace("/", "")                   '特例置換項目-次期開始適用日  
                End If

                If String.IsNullOrEmpty(LNM0013row("SPRNEXTENDYMD").Replace("/", "")) Then
                    PARA019.Value = DBNull.Value                                                  '特例置換項目-次期終了適用日  
                Else
                    PARA019.Value = LNM0013row("SPRNEXTENDYMD").Replace("/", "")                  '特例置換項目-次期終了適用日  
                End If

                If String.IsNullOrEmpty(LNM0013row("SPRNEXTAPPLYRATE")) Then
                    PARA020.Value = "0"                                                           '特例置換項目-現行適用率      
                Else
                    PARA020.Value = LNM0013row("SPRNEXTAPPLYRATE")                                '特例置換項目-現行適用率      
                End If

                If Not String.IsNullOrEmpty(LNM0013row("SPRNEXTROUNDKBN1")) AndAlso
                   Not LNM0013row("SPRNEXTROUNDKBN1") = "0" AndAlso
                   Not String.IsNullOrEmpty(LNM0013row("SPRNEXTROUNDKBN2")) AndAlso
                   Not LNM0013row("SPRNEXTROUNDKBN2") = "0" Then
                    PARA021.Value = LNM0013row("SPRNEXTROUNDKBN1") & LNM0013row("SPRNEXTROUNDKBN2") '特例置換項目-現行端数処理区分
                Else
                    PARA021.Value = "0"
                End If

                PARA022.Value = LNM0013row("DELFLG")                                             '削除フラグ                   
                PARA023.Value = WW_NOW                                                           '登録年月日                   
                PARA024.Value = Master.USERID                                                    '登録ユーザーＩＤ             
                PARA025.Value = Master.USERTERMID                                                '登録端末                     
                PARA026.Value = Me.GetType().BaseType.Name                                       '登録プログラムＩＤ           
                PARA027.Value = WW_NOW                                                           '更新年月日                   
                PARA028.Value = Master.USERID                                                    '更新ユーザーＩＤ            
                PARA029.Value = Master.USERTERMID                                                '更新端末                    
                PARA030.Value = Me.GetType().BaseType.Name                                       '更新プログラムＩＤ          
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA001.Value = LNM0013row("BIGCTNCD")
                JPARA002.Value = LNM0013row("MIDDLECTNCD")
                JPARA003.Value = LNM0013row("PRIORITYNO")
                JPARA004.Value = LNM0013row("DEPSTATION")
                JPARA005.Value = CInt(LNM0013row("JRDEPBRANCHCD"))
                JPARA006.Value = LNM0013row("ARRSTATION")
                JPARA007.Value = CInt(LNM0013row("JRARRBRANCHCD"))

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0013UPDtbl) Then
                        LNM0013UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0013UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0013UPDtbl.Clear()
                    LNM0013UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0013UPDrow As DataRow In LNM0013UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0013D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0013UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0013D UPDATE_INSERT"
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
    Protected Sub REKTRMEXISTS(ByVal SQLcon As MySqlConnection, ByRef WW_MODIFYKBN As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '回送運賃適用率マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        BIGCTNCD")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0013_REKTRM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        BIGCTNCD       = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD    = @MIDDLECTNCD")
        SQLStr.AppendLine("    AND PRIORITYNO     = @PRIORITYNO")
        SQLStr.AppendLine("    AND DEPSTATION     = @DEPSTATION")
        SQLStr.AppendLine("    AND JRDEPBRANCHCD  = @JRDEPBRANCHCD")
        SQLStr.AppendLine("    AND ARRSTATION     = @ARRSTATION")
        SQLStr.AppendLine("    AND JRARRBRANCHCD  = @JRARRBRANCHCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_JRDEPBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRDEPBRANCHCD", MySqlDbType.Int32)         'ＪＲ発支社支店コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)         '着駅コード
                Dim P_JRARRBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRARRBRANCHCD", MySqlDbType.Int32)         'ＪＲ着支社支店コード

                Dim LNM0013row As DataRow = LNM0013INPtbl.Rows(0)

                P_BIGCTNCD.Value = LNM0013row("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = LNM0013row("MIDDLECTNCD")               '中分類コード
                P_PRIORITYNO.Value = LNM0013row("PRIORITYNO")               '優先順位
                P_DEPSTATION.Value = LNM0013row("DEPSTATION")               '発駅コード
                P_JRDEPBRANCHCD.Value = LNM0013row("JRDEPBRANCHCD")               'ＪＲ発支社支店コード
                P_ARRSTATION.Value = LNM0013row("ARRSTATION")               '着駅コード
                P_JRARRBRANCHCD.Value = LNM0013row("JRARRBRANCHCD")               'ＪＲ着支社支店コード

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0013C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0013C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0116_REKTRHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,JRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,JRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,CTNTYPE  ")
        SQLStr.AppendLine("        ,CTNSTNO  ")
        SQLStr.AppendLine("        ,CTNENDNO  ")
        SQLStr.AppendLine("        ,SPRCURSTYMD  ")
        SQLStr.AppendLine("        ,SPRCURENDYMD  ")
        SQLStr.AppendLine("        ,SPRCURAPPLYRATE  ")
        SQLStr.AppendLine("        ,SPRCURROUNDKBN  ")
        SQLStr.AppendLine("        ,SPRNEXTSTYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTENDYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTAPPLYRATE  ")
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
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,JRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,JRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,CTNTYPE  ")
        SQLStr.AppendLine("        ,CTNSTNO  ")
        SQLStr.AppendLine("        ,CTNENDNO  ")
        SQLStr.AppendLine("        ,SPRCURSTYMD  ")
        SQLStr.AppendLine("        ,SPRCURENDYMD  ")
        SQLStr.AppendLine("        ,SPRCURAPPLYRATE  ")
        SQLStr.AppendLine("        ,SPRCURROUNDKBN  ")
        SQLStr.AppendLine("        ,SPRNEXTSTYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTENDYMD  ")
        SQLStr.AppendLine("        ,SPRNEXTAPPLYRATE  ")
        SQLStr.AppendLine("        ,SPRNEXTROUNDKBN  ")
        SQLStr.AppendLine("        ,@OPERATEKBN AS OPERATEKBN ")
        SQLStr.AppendLine("        ,@MODIFYKBN AS MODIFYKBN ")
        SQLStr.AppendLine("        ,@MODIFYYMD AS MODIFYYMD ")
        SQLStr.AppendLine("        ,@MODIFYUSER AS MODIFYUSER ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine("        ,@INITYMD AS INITYMD ")
        SQLStr.AppendLine("        ,@INITUSER AS INITUSER ")
        SQLStr.AppendLine("        ,@INITTERMID AS INITTERMID ")
        SQLStr.AppendLine("        ,@INITPGID AS INITPGID ")
        SQLStr.AppendLine("  FROM   ")
        SQLStr.AppendLine("        LNG.LNM0013_REKTRM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        BIGCTNCD       = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD    = @MIDDLECTNCD")
        SQLStr.AppendLine("    AND PRIORITYNO     = @PRIORITYNO")
        SQLStr.AppendLine("    AND DEPSTATION     = @DEPSTATION")
        SQLStr.AppendLine("    AND JRDEPBRANCHCD  = @JRDEPBRANCHCD")
        SQLStr.AppendLine("    AND ARRSTATION     = @ARRSTATION")
        SQLStr.AppendLine("    AND JRARRBRANCHCD  = @JRARRBRANCHCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_JRDEPBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRDEPBRANCHCD", MySqlDbType.Int32)         'ＪＲ発支社支店コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)         '着駅コード
                Dim P_JRARRBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRARRBRANCHCD", MySqlDbType.Int32)         'ＪＲ着支社支店コード

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0013row As DataRow = LNM0013INPtbl.Rows(0)

                ' DB更新
                P_BIGCTNCD.Value = LNM0013row("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = LNM0013row("MIDDLECTNCD")               '中分類コード
                P_PRIORITYNO.Value = LNM0013row("PRIORITYNO")               '優先順位
                P_DEPSTATION.Value = LNM0013row("DEPSTATION")               '発駅コード
                P_JRDEPBRANCHCD.Value = LNM0013row("JRDEPBRANCHCD")               'ＪＲ発支社支店コード
                P_ARRSTATION.Value = LNM0013row("ARRSTATION")               '着駅コード
                P_JRARRBRANCHCD.Value = LNM0013row("JRARRBRANCHCD")               'ＪＲ着支社支店コード

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0013WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0013WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0013tbl.Rows(0)("DELFLG") = "0" And LNM0013row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0013WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0013WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0116_REKTRHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0116_REKTRHIST  INSERT"
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
        DetailBoxToLNM0013INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0013tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0013tbl, work.WF_SEL_INPTBL.Text)

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
    Protected Sub DetailBoxToLNM0013INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)            '削除フラグ
        Master.EraseCharToIgnore(TxtBigCTNCD.Text)          '大分類コード
        Master.EraseCharToIgnore(TxtMiddleCTNCD.Text)       '中分類コード
        Master.EraseCharToIgnore(TxtPriorityNO.Text)        '優先順位
        Master.EraseCharToIgnore(TxtDepstation.Text)        '発駅コード
        Master.EraseCharToIgnore(TxtJrDepBranchCD.Text)     'ＪＲ発支社支店コード
        Master.EraseCharToIgnore(TxtArrstation.Text)        '着駅コード
        Master.EraseCharToIgnore(TxtJrArrBranchCD.Text)     'ＪＲ着支社支店コード
        Master.EraseCharToIgnore(TxtPurpose.Text)           '使用目的
        Master.EraseCharToIgnore(TxtDepTrusteeCD.Text)      '発受託人コード
        Master.EraseCharToIgnore(TxtDepTrusteeSubCD.Text)   '発受託人サブコード
        Master.EraseCharToIgnore(TxtCTNType.Text)           'コンテナ記号
        Master.EraseCharToIgnore(TxtCTNStNO.Text)           'コンテナ番号（開始）
        Master.EraseCharToIgnore(TxtCTNEndNO.Text)          'コンテナ番号（終了）
        Master.EraseCharToIgnore(TxtSprCurStYMD.Text)       '特例置換項目-現行開始適用日
        Master.EraseCharToIgnore(TxtSprCurEndYMD.Text)      '特例置換項目-現行終了適用日
        Master.EraseCharToIgnore(TxtSprCurApplyRate.Text)   '特例置換項目-現行適用率
        Master.EraseCharToIgnore(TxtSprCurRoundKbn1.Text)   '特例置換項目-現行端数処理区分1
        Master.EraseCharToIgnore(TxtSprCurRoundKbn2.Text)   '特例置換項目-現行端数処理区分2
        Master.EraseCharToIgnore(TxtSprNextStYMD.Text)      '特例置換項目-次期開始適用日
        Master.EraseCharToIgnore(TxtSprNextEndYMD.Text)     '特例置換項目-次期終了適用日
        Master.EraseCharToIgnore(TxtSprNextApplyRate.Text)  '特例置換項目-次期適用率
        Master.EraseCharToIgnore(TxtSprNextRoundKbn1.Text)  '特例置換項目-次期端数処理区分1
        Master.EraseCharToIgnore(TxtSprNextRoundKbn2.Text)  '特例置換項目-次期端数処理区分2


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

        Master.CreateEmptyTable(LNM0013INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0013INProw As DataRow = LNM0013INPtbl.NewRow

        'LINECNT
        If LblSelLineCNT.Text = "" Then
            LNM0013INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(LblSelLineCNT.Text, LNM0013INProw("LINECNT"))
            Catch ex As Exception
                LNM0013INProw("LINECNT") = 0
            End Try
        End If

        LNM0013INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0013INProw("UPDTIMSTP") = 0
        LNM0013INProw("SELECT") = 1
        LNM0013INProw("HIDDEN") = 0

        LNM0013INProw("BIGCTNCD") = TxtBigCTNCD.Text                                             '大分類コード
        LNM0013INProw("MIDDLECTNCD") = TxtMiddleCTNCD.Text                                       '中分類コード
        LNM0013INProw("PRIORITYNO") = TxtPriorityNO.Text                                         '優先順位
        LNM0013INProw("DEPSTATION") = TxtDepstation.Text                                         '発駅コード
        LNM0013INProw("JRDEPBRANCHCD") = TxtJrDepBranchCD.Text                                   'ＪＲ発支社支店コード
        LNM0013INProw("ARRSTATION") = TxtArrstation.Text                                         '着駅コード
        LNM0013INProw("JRARRBRANCHCD") = TxtJrArrBranchCD.Text                                   'ＪＲ着支社支店コード
        LNM0013INProw("PURPOSE") = TxtPurpose.Text                                               '使用目的
        LNM0013INProw("DEPTRUSTEECD") = TxtDepTrusteeCD.Text                                     '発受託人コード
        LNM0013INProw("DEPTRUSTEESUBCD") = TxtDepTrusteeSubCD.Text                               '発受託人サブコード
        LNM0013INProw("CTNTYPE") = TxtCTNType.Text                                               'コンテナ記号
        LNM0013INProw("CTNSTNO") = TxtCTNStNO.Text                                               'コンテナ番号（開始）
        LNM0013INProw("CTNENDNO") = TxtCTNEndNO.Text                                             'コンテナ番号（終了）
        LNM0013INProw("SPRCURSTYMD") = TxtSprCurStYMD.Text                                       '特例置換項目-現行開始適用日
        LNM0013INProw("SPRCURENDYMD") = TxtSprCurEndYMD.Text                                     '特例置換項目-現行終了適用日
        LNM0013INProw("SPRCURAPPLYRATE") = TxtSprCurApplyRate.Text                               '特例置換項目-現行適用率
        LNM0013INProw("SPRCURROUNDKBN") = TxtSprCurRoundKbn1.Text & TxtSprCurRoundKbn2.Text      '特例置換項目-現行端数処理区分
        LNM0013INProw("SPRCURROUNDKBN1") = TxtSprCurRoundKbn1.Text                               '特例置換項目-現行端数処理区分1
        LNM0013INProw("SPRCURROUNDKBN2") = TxtSprCurRoundKbn2.Text                               '特例置換項目-現行端数処理区分2
        LNM0013INProw("SPRNEXTSTYMD") = TxtSprNextStYMD.Text                                     '特例置換項目-次期開始適用日
        LNM0013INProw("SPRNEXTENDYMD") = TxtSprNextEndYMD.Text                                   '特例置換項目-次期終了適用日
        LNM0013INProw("SPRNEXTAPPLYRATE") = TxtSprNextApplyRate.Text                             '特例置換項目-次期適用率
        LNM0013INProw("SPRNEXTROUNDKBN") = TxtSprNextRoundKbn1.Text & TxtSprNextRoundKbn2.Text   '特例置換項目-次期端数処理区分
        LNM0013INProw("SPRNEXTROUNDKBN1") = TxtSprNextRoundKbn1.Text                             '特例置換項目-次期端数処理区分1
        LNM0013INProw("SPRNEXTROUNDKBN2") = TxtSprNextRoundKbn2.Text                             '特例置換項目-次期端数処理区分2
        LNM0013INProw("DELFLG") = TxtDelFlg.Text                                                 '削除フラグ
        LNM0013INProw("UPDYMD") = Date.Now                                                       '更新日付

        '○ チェック用テーブルに登録する
        LNM0013INPtbl.Rows.Add(LNM0013INProw)

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
        DetailBoxToLNM0013INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0013INProw As DataRow = LNM0013INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0013row As DataRow In LNM0013tbl.Rows
            ' KEY項目が等しい時
            If LNM0013row("BIGCTNCD") = LNM0013INProw("BIGCTNCD") AndAlso
                LNM0013row("MIDDLECTNCD") = LNM0013INProw("MIDDLECTNCD") AndAlso
                LNM0013row("PRIORITYNO") = LNM0013INProw("PRIORITYNO") AndAlso
                LNM0013row("DEPSTATION") = LNM0013INProw("DEPSTATION") AndAlso
                LNM0013row("JRDEPBRANCHCD") = LNM0013INProw("JRDEPBRANCHCD") AndAlso
                LNM0013row("ARRSTATION") = LNM0013INProw("ARRSTATION") AndAlso
                LNM0013row("JRARRBRANCHCD") = LNM0013INProw("JRARRBRANCHCD") Then
                ' KEY項目以外の項目の差異をチェック
                If LNM0013row("PURPOSE") = LNM0013INProw("PURPOSE") AndAlso
                    LNM0013row("DEPTRUSTEECD") = LNM0013INProw("DEPTRUSTEECD") AndAlso
                    LNM0013row("DEPTRUSTEESUBCD") = LNM0013INProw("DEPTRUSTEESUBCD") AndAlso
                    LNM0013row("CTNTYPE") = LNM0013INProw("CTNTYPE") AndAlso
                    LNM0013row("CTNSTNO") = LNM0013INProw("CTNSTNO") AndAlso
                    LNM0013row("CTNENDNO") = LNM0013INProw("CTNENDNO") AndAlso
                    LNM0013row("SPRCURSTYMD") = LNM0013INProw("SPRCURSTYMD") AndAlso
                    LNM0013row("SPRCURENDYMD") = LNM0013INProw("SPRCURENDYMD") AndAlso
                    LNM0013row("SPRCURAPPLYRATE") = LNM0013INProw("SPRCURAPPLYRATE") AndAlso
                    LNM0013row("SPRCURROUNDKBN1") = LNM0013INProw("SPRCURROUNDKBN1") AndAlso
                    LNM0013row("SPRCURROUNDKBN2") = LNM0013INProw("SPRCURROUNDKBN2") AndAlso
                    LNM0013row("SPRNEXTSTYMD") = LNM0013INProw("SPRNEXTSTYMD") AndAlso
                    LNM0013row("SPRNEXTENDYMD") = LNM0013INProw("SPRNEXTENDYMD") AndAlso
                    LNM0013row("SPRNEXTAPPLYRATE") = LNM0013INProw("SPRNEXTAPPLYRATE") AndAlso
                    LNM0013row("SPRNEXTROUNDKBN1") = LNM0013INProw("SPRNEXTROUNDKBN1") AndAlso
                    LNM0013row("SPRNEXTROUNDKBN2") = LNM0013INProw("SPRNEXTROUNDKBN2") AndAlso
                    LNM0013row("DELFLG") = LNM0013INProw("DELFLG") Then
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
        For Each LNM0013row As DataRow In LNM0013tbl.Rows
            Select Case LNM0013row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0013tbl, work.WF_SEL_INPTBL.Text)

        LblSelLineCNT.Text = ""               'LINECNT
        TxtMapId.Text = "M00001"              '画面ＩＤ


        TxtBigCTNCD.Text = ""                 '大分類コード
        TxtMiddleCTNCD.Text = ""              '中分類コード
        TxtPriorityNO.Text = ""               '優先順位
        TxtDepstation.Text = ""               '発駅コード
        TxtJrDepBranchCD.Text = ""            'ＪＲ発支社支店コード
        TxtArrstation.Text = ""               '着駅コード
        TxtJrArrBranchCD.Text = ""            'ＪＲ着支社支店コード
        TxtPurpose.Text = ""                  '使用目的
        TxtDepTrusteeCD.Text = ""             '発受託人コード
        TxtDepTrusteeSubCD.Text = ""          '発受託人サブコード
        TxtCTNType.Text = ""                  'コンテナ記号
        TxtCTNStNO.Text = ""                  'コンテナ番号（開始）
        TxtCTNEndNO.Text = ""                 'コンテナ番号（終了）
        TxtSprCurStYMD.Text = ""              '特例置換項目-現行開始適用日
        TxtSprCurEndYMD.Text = ""             '特例置換項目-現行終了適用日
        TxtSprCurApplyRate.Text = ""          '特例置換項目-現行適用率
        TxtSprCurRoundKbn1.Text = ""           '特例置換項目-現行端数処理区分1
        TxtSprCurRoundKbn2.Text = ""           '特例置換項目-現行端数処理区分2
        TxtSprNextStYMD.Text = ""             '特例置換項目-次期開始適用日
        TxtSprNextEndYMD.Text = ""            '特例置換項目-次期終了適用日
        TxtSprNextApplyRate.Text = ""         '特例置換項目-次期適用率
        TxtSprNextRoundKbn1.Text = ""          '特例置換項目-次期端数処理区分1
        TxtSprNextRoundKbn2.Text = ""          '特例置換項目-次期端数処理区分2
        TxtDelFlg.Text = ""                   '削除フラグ

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
                            Case "TxtSprCurStYMD"           '特例置換項目-現行開始適用日
                                .WF_Calendar.Text = TxtSprCurStYMD.Text
                            Case "TxtSprCurEndYMD"          '特例置換項目-現行終了適用日
                                .WF_Calendar.Text = TxtSprCurEndYMD.Text
                            Case "TxtSprNextStYMD"          '特例置換項目-次期開始適用日
                                .WF_Calendar.Text = TxtSprNextStYMD.Text
                            Case "TxtSprNextEndYMD"         '特例置換項目-次期終了適用日
                                .WF_Calendar.Text = TxtSprNextEndYMD.Text
                        End Select
                        .ActiveCalendar()

                    Case Else
                        ' フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "TxtBigCTNCD"             '大分類コード
                                WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                            Case "TxtMiddleCTNCD"          '中分類コード
                                WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, TxtBigCTNCD.Text)
                            Case "TxtDepstation"          '発駅コード
                                leftview.Visible = False
                                '検索画面
                                DisplayView_mspStationSingle()
                                '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                                WF_LeftboxOpen.Value = ""
                                Exit Sub
                            Case "TxtJrDepBranchCD",      'ＪＲ発支社支店コード
                                 "TxtJrArrBranchCD"       'ＪＲ着支社支店コード
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "JRBRANCHCD")
                            Case "TxtArrstation"          '着駅コード
                                leftview.Visible = False
                                '検索画面
                                DisplayView_mspStationSingle()
                                '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                                WF_LeftboxOpen.Value = ""
                                Exit Sub
                            Case "TxtDepTrusteeCD"        '発受託人コード
                                WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtDepstation.Text)
                            Case "TxtDepTrusteeSubCD"     '発受託人サブコード
                                WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtDepstation.Text, TxtDepTrusteeCD.Text)
                            Case "TxtCTNType"             'コンテナ記号
                                WW_PrmData = work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_TYPE)
                            Case "TxtCTNStNO"             'コンテナ番号（開始）
                                WW_PrmData = work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, TxtCTNType.Text)
                            Case "TxtCTNEndNO"            'コンテナ番号（終了）
                                WW_PrmData = work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, TxtCTNType.Text)
                            Case "TxtSprCurRoundKbn1"      '特例置換項目-現行端数処理区分1
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUU1")
                            Case "TxtSprCurRoundKbn2"      '特例置換項目-現行端数処理区分2
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUU2")
                            Case "TxtSprNextRoundKbn1"     '特例置換項目-次期端数処理区分1
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUU1")
                            Case "TxtSprNextRoundKbn2"     '特例置換項目-次期端数処理区分2
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUU2")
                            Case "TxtDelFlg"              '削除フラグ
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
            Case "TxtDelFlg"              '削除フラグ
                CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
                TxtDelFlg.Focus()
            Case "TxtBigCTNCD"            '大分類コード
                CODENAME_get("BIGCTNCD", TxtBigCTNCD.Text, LblBigCTNCDName.Text, WW_Dummy)
                TxtBigCTNCD.Focus()
                ReSetClassCd()
            Case "TxtMiddleCTNCD"         '中分類コード
                CODENAME_get("MIDDLECTNCD", TxtMiddleCTNCD.Text, LblMiddleCTNCDName.Text, WW_Dummy)
                TxtMiddleCTNCD.Focus()
            Case "TxtDepstation"          '発駅コード
                CODENAME_get("DEPSTATION", TxtDepstation.Text, LblDepstationCDName.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblDepstationCDName.Text) And TxtDepstation.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtDepstation.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtDepstation.Focus()
                End If
            Case "TxtArrstation"          '着駅コード
                CODENAME_get("ARRSTATION", TxtArrstation.Text, LblArrstationName.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblArrstationName.Text) And TxtArrstation.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtArrstation.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtArrstation.Focus()
                End If
            Case "TxtJrDepBranchCD"       'ＪＲ発支社支店コード
                CODENAME_get("JRBRANCHCD", TxtJrDepBranchCD.Text, LblJrDepBranchCDName.Text, WW_Dummy)
                TxtJrDepBranchCD.Focus()
            Case "TxtJrArrBranchCD"       'ＪＲ着支社支店コード
                CODENAME_get("JRBRANCHCD", TxtJrArrBranchCD.Text, LblJrArrBranchCDName.Text, WW_Dummy)
                TxtJrArrBranchCD.Focus()
            Case "TxtDepTrusteeCD"        '発受託人コード
                CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCD.Text, LblDepTrusteeCDName.Text, WW_Dummy)
                TxtDepTrusteeCD.Focus()
            Case "TxtDepTrusteeSubCD"     '発受託人サブコード
                CODENAME_get("DEPTRUSTEESUBCD", TxtDepTrusteeSubCD.Text, LblDepTrusteeSubCDName.Text, WW_Dummy)
                TxtDepTrusteeSubCD.Focus()
            Case "TxtCTNType"             'コンテナ記号
                CODENAME_get("CTNTYPE", TxtCTNType.Text, LblCTNTypeName.Text, WW_Dummy)
                TxtCTNType.Focus()
            Case "TxtCTNStNO"             'コンテナ番号（開始）
                TxtCTNStNO.Focus()
            Case "TxtCTNEndNO"            'コンテナ番号（終了）
                TxtCTNEndNO.Focus()
            Case "TxtSprCurRoundKbn1"      '特例置換項目-現行端数処理区分1
                CODENAME_get("SPRCURROUNDKBN1", TxtSprCurRoundKbn1.Text, LblSprCurRoundKbn1Name.Text, WW_Dummy)
                TxtSprCurRoundKbn1.Focus()
            Case "TxtSprCurRoundKbn2"      '特例置換項目-現行端数処理区分2
                CODENAME_get("SPRCURROUNDKBN2", TxtSprCurRoundKbn2.Text, LblSprCurRoundKbn2Name.Text, WW_Dummy)
                TxtSprCurRoundKbn2.Focus()
            Case "TxtSprNextRoundKbn1"     '特例置換項目-次期端数処理区分1
                CODENAME_get("SPRNEXTROUNDKBN1", TxtSprNextRoundKbn1.Text, LblSprNextRoundKbn1Name.Text, WW_Dummy)
                TxtSprNextRoundKbn1.Focus()
            Case "TxtSprNextRoundKbn2"     '特例置換項目-次期端数処理区分2
                CODENAME_get("SPRNEXTROUNDKBN2", TxtSprNextRoundKbn2.Text, LblSprNextRoundKbn2Name.Text, WW_Dummy)
                TxtSprNextRoundKbn2.Focus()
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

        TxtMiddleCTNCD.Text = ""
        LblMiddleCTNCDName.Text = ""

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
                Case "TxtDelFlg"              '削除フラグ
                    TxtDelFlg.Text = WW_SelectValue
                    LblDelFlgName.Text = WW_SelectText
                    TxtDelFlg.Focus()
                Case "TxtBigCTNCD"            '大分類コード
                    TxtBigCTNCD.Text = WW_SelectValue
                    LblBigCTNCDName.Text = WW_SelectText
                    TxtBigCTNCD.Focus()
                    ReSetClassCd()
                Case "TxtMiddleCTNCD"         '中分類コード
                    TxtMiddleCTNCD.Text = WW_SelectValue
                    LblMiddleCTNCDName.Text = WW_SelectText
                    TxtMiddleCTNCD.Focus()
                Case "TxtDepstation"          '発駅コード
                    TxtDepstation.Text = WW_SelectValue
                    LblDepstationCDName.Text = WW_SelectText
                    TxtDepstation.Focus()
                Case "TxtJrDepBranchCD"       'ＪＲ発支社支店コード
                    TxtJrDepBranchCD.Text = WW_SelectValue
                    LblJrDepBranchCDName.Text = WW_SelectText
                    TxtJrDepBranchCD.Focus()
                Case "TxtArrstation"          '着駅コード
                    TxtArrstation.Text = WW_SelectValue
                    LblArrstationName.Text = WW_SelectText
                    TxtArrstation.Focus()
                Case "TxtJrArrBranchCD"       'ＪＲ着支社支店コード
                    TxtJrArrBranchCD.Text = WW_SelectValue
                    LblJrArrBranchCDName.Text = WW_SelectText
                    TxtJrArrBranchCD.Focus()
                Case "TxtDepTrusteeCD"           '発受託人コード
                    TxtDepTrusteeCD.Text = WW_SelectValue
                    LblDepTrusteeCDName.Text = WW_SelectText
                    TxtDepTrusteeCD.Focus()
                Case "TxtDepTrusteeSubCD"           '発受託人サブコード
                    TxtDepTrusteeSubCD.Text = WW_SelectValue
                    LblDepTrusteeSubCDName.Text = WW_SelectText
                    TxtDepTrusteeSubCD.Focus()
                Case "TxtCTNType"             'コンテナ記号
                    TxtCTNType.Text = WW_SelectValue
                    LblCTNTypeName.Text = WW_SelectText
                    TxtCTNType.Focus()
                Case "TxtCTNStNO"             'コンテナ番号（開始）
                    TxtCTNStNO.Text = WW_SelectValue
                    TxtCTNStNO.Focus()
                Case "TxtCTNEndNO"            'コンテナ番号（終了）
                    TxtCTNEndNO.Text = WW_SelectValue
                    TxtCTNEndNO.Focus()
                Case "TxtSprCurStYMD"         '特例置換項目-現行開始適用日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtSprCurStYMD.Text = ""
                        Else
                            TxtSprCurStYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtSprCurStYMD.Focus()
                Case "TxtSprCurEndYMD"         '特例置換項目-現行終了適用日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtSprCurEndYMD.Text = ""
                        Else
                            TxtSprCurEndYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtSprCurEndYMD.Focus()
                Case "TxtSprCurRoundKbn1"      '特例置換項目-現行端数処理区分1
                    TxtSprCurRoundKbn1.Text = WW_SelectValue
                    LblSprCurRoundKbn1Name.Text = WW_SelectText
                    TxtSprCurRoundKbn1.Focus()
                Case "TxtSprCurRoundKbn2"      '特例置換項目-現行端数処理区分2
                    TxtSprCurRoundKbn2.Text = WW_SelectValue
                    LblSprCurRoundKbn2Name.Text = WW_SelectText
                    TxtSprCurRoundKbn2.Focus()
                Case "TxtSprNextStYMD"         '特例置換項目-次期開始適用日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtSprNextStYMD.Text = ""
                        Else
                            TxtSprNextStYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtSprNextStYMD.Focus()
                Case "TxtSprNextEndYMD"         '特例置換項目-次期終了適用日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtSprNextEndYMD.Text = ""
                        Else
                            TxtSprNextEndYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtSprNextEndYMD.Focus()
                Case "TxtSprNextRoundKbn1"     '特例置換項目-次期端数処理区分1
                    TxtSprNextRoundKbn1.Text = WW_SelectValue
                    LblSprNextRoundKbn1Name.Text = WW_SelectText
                    TxtSprNextRoundKbn1.Focus()
                Case "TxtSprNextRoundKbn2"     '特例置換項目-次期端数処理区分2
                    TxtSprNextRoundKbn2.Text = WW_SelectValue
                    LblSprNextRoundKbn2Name.Text = WW_SelectText
                    TxtSprNextRoundKbn2.Focus()
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
                Case "TxtDelFlg"              '削除フラグ
                    TxtDelFlg.Focus()
                Case "TxtBigCTNCD"               '大分類コード
                    TxtBigCTNCD.Focus()
                Case "TxtMiddleCTNCD"            '中分類コード
                    TxtMiddleCTNCD.Focus()
                Case "TxtPriorityNO"             '優先順位
                    TxtPriorityNO.Focus()
                Case "TxtDepstation"             '発駅コード
                    TxtDepstation.Focus()
                Case "TxtJrDepBranchCD"          'ＪＲ発支社支店コード
                    TxtJrDepBranchCD.Focus()
                Case "TxtArrstation"             '着駅コード
                    TxtArrstation.Focus()
                Case "TxtJrArrBranchCD"          'ＪＲ着支社支店コード
                    TxtJrArrBranchCD.Focus()
                Case "TxtPurpose"                '使用目的
                    TxtPurpose.Focus()
                Case "TxtDepTrusteeCD"           '発受託人コード
                    TxtDepTrusteeCD.Focus()
                Case "TxtDepTrusteeSubCD"        '発受託人サブコード
                    TxtDepTrusteeSubCD.Focus()
                Case "TxtCTNType"                'コンテナ記号
                    TxtCTNType.Focus()
                Case "TxtCTNStNO"                'コンテナ番号（開始）
                    TxtCTNStNO.Focus()
                Case "TxtCTNEndNO"               'コンテナ番号（終了）
                    TxtCTNEndNO.Focus()
                Case "TxtSprCurStYMD"            '特例置換項目-現行開始適用日
                    TxtSprCurStYMD.Focus()
                Case "TxtSprCurEndYMD"           '特例置換項目-現行終了適用日
                    TxtSprCurEndYMD.Focus()
                Case "TxtSprCurApplyRate"        '特例置換項目-現行適用率
                    TxtSprCurApplyRate.Focus()
                Case "TxtSprCurRoundKbn1"         '特例置換項目-現行端数処理区分1
                    TxtSprCurRoundKbn1.Focus()
                Case "TxtSprCurRoundKbn2"         '特例置換項目-現行端数処理区分2
                    TxtSprCurRoundKbn2.Focus()
                Case "TxtSprNextStYMD"           '特例置換項目-次期開始適用日
                    TxtSprNextStYMD.Focus()
                Case "TxtSprNextEndYMD"          '特例置換項目-次期終了適用日
                    TxtSprNextEndYMD.Focus()
                Case "TxtSprNextApplyRate"       '特例置換項目-次期適用率
                    TxtSprNextApplyRate.Focus()
                Case "TxtSprNextRoundKbn1"        '特例置換項目-次期端数処理区分1
                    TxtSprNextRoundKbn1.Focus()
                Case "TxtSprNextRoundKbn2"        '特例置換項目-次期端数処理区分2
                    TxtSprNextRoundKbn2.Focus()
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

            Case TxtDepstation.ID
                Me.TxtDepstation.Text = selData("STATION").ToString
                Me.LblDepstationCDName.Text = selData("NAMES").ToString
                Me.TxtDepstation.Focus()

            Case TxtArrstation.ID
                Me.TxtArrstation.Text = selData("STATION").ToString
                Me.LblArrstationName.Text = selData("NAMES").ToString
                Me.TxtArrstation.Focus()
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

        Dim WW_SPRCURSTYMD_KETA As Integer = "0"
        Dim WW_SPRCURENDYMD_KETA As Integer = "0"
        Dim WW_SPRNEXTSTYMD_KETA As Integer = "0"
        Dim WW_SPRNEXTENDYMD_KETA As Integer = "0"

        Dim WW_SPRCURAPPLYRATE_INT As Integer = 0
        Dim WW_SPRNEXTAPPLYRATE_INT As Integer = 0

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


        For Each LNM0013INProw As DataRow In LNM0013INPtbl.Rows
            '○ 単項目チェック

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0013INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 値存在チェック
                CODENAME_get("DELFLG", LNM0013INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "BIGCTNCD", LNM0013INProw("BIGCTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 値存在チェック
                CODENAME_get("BIGCTNCD", LNM0013INProw("BIGCTNCD"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "MIDDLECTNCD", LNM0013INProw("MIDDLECTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 値存在チェック
                CODENAME_get("MIDDLECTNCD", LNM0013INProw("MIDDLECTNCD"), WW_Dummy, WW_RtnSW)
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

            ' 優先順位(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "PRIORITYNO", LNM0013INProw("PRIORITYNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・優先順位エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発駅コード(バリデーションチェック)

            Master.CheckField(Master.USERCAMP, "DEPSTATION", LNM0013INProw("DEPSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then

                If Not String.IsNullOrEmpty(LNM0013INProw("DEPSTATION")) AndAlso
                   Not LNM0013INProw("DEPSTATION") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("DEPSTATION", LNM0013INProw("DEPSTATION"), WW_Dummy, WW_RtnSW)
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

            ' ＪＲ発支社支店コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "JRDEPBRANCHCD", LNM0013INProw("JRDEPBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then

                If Not String.IsNullOrEmpty(LNM0013INProw("JRDEPBRANCHCD")) AndAlso
                   Not LNM0013INProw("JRDEPBRANCHCD") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("JRBRANCHCD", LNM0013INProw("JRDEPBRANCHCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・ＪＲ発支社支店コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・ＪＲ発支社支店コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 着駅コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ARRSTATION", LNM0013INProw("ARRSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0013INProw("ARRSTATION")) AndAlso
                   Not LNM0013INProw("ARRSTATION") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("ARRSTATION", LNM0013INProw("ARRSTATION"), WW_Dummy, WW_RtnSW)
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

            ' ＪＲ着支社支店コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "JRARRBRANCHCD", LNM0013INProw("JRARRBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0013INProw("JRARRBRANCHCD")) AndAlso
                   Not LNM0013INProw("JRARRBRANCHCD") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("JRBRANCHCD", LNM0013INProw("JRARRBRANCHCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・ＪＲ着支社支店コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・ＪＲ着支社支店コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 使用目的
            Master.CheckField(Master.USERCAMP, "PURPOSE", LNM0013INProw("PURPOSE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・使用目的エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発受託人コード
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEECD", LNM0013INProw("DEPTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0013INProw("DEPTRUSTEECD")) AndAlso
                   Not LNM0013INProw("DEPTRUSTEECD") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("DEPTRUSTEECD", LNM0013INProw("DEPTRUSTEECD"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBCD", LNM0013INProw("DEPTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0013INProw("DEPTRUSTEESUBCD")) Then
                    ' 値存在チェック
                    CODENAME_get("DEPTRUSTEESUBCD", LNM0013INProw("DEPTRUSTEESUBCD"), WW_Dummy, WW_RtnSW)
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

            ' コンテナ記号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CTNTYPE", LNM0013INProw("CTNTYPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0013INProw("CTNTYPE")) Then
                    ' 値存在チェック
                    CODENAME_get("CTNTYPE", LNM0013INProw("CTNTYPE"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・コンテナ記号エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・コンテナ記号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' コンテナ番号（開始）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CTNSTNO", LNM0013INProw("CTNSTNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0013INProw("CTNSTNO")) AndAlso
                   Not LNM0013INProw("CTNSTNO") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("CTNSTNO", LNM0013INProw("CTNSTNO"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・コンテナ番号（開始）エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・コンテナ番号（開始）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' コンテナ番号（終了）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CTNENDNO", LNM0013INProw("CTNENDNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0013INProw("CTNENDNO")) AndAlso
                   Not LNM0013INProw("CTNENDNO") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("CTNENDNO", LNM0013INProw("CTNENDNO"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・コンテナ番号（終了）エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・コンテナ番号（終了）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-現行開始適用日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRCURSTYMD", LNM0013INProw("SPRCURSTYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 画面表示の書式を変更
                If Not String.IsNullOrEmpty(LNM0013INProw("SPRCURSTYMD")) Then
                    Try
                        WW_ChkDate = Integer.Parse(LNM0013INProw("SPRCURSTYMD").Replace("/", ""))
                        If WW_ChkDate <= 9999 Then
                            ' 入力値が4桁以下の場合は2000年として日付チェックを行う
                            LNM0013INProw("SPRCURSTYMD") = WW_ChkDate
                            WW_ChkDate8str = 20000000 + WW_ChkDate
                            WW_ChkDate8ymd = WW_ChkDate8str.Substring(0, 4) & "/" & WW_ChkDate8str.Substring(4, 2) & "/" & WW_ChkDate8str.Substring(6, 2)
                            CDate(WW_ChkDate8ymd).ToString("yyyy/MM/dd")
                        Else
                            LNM0013INProw("SPRCURSTYMD") = CDate(LNM0013INProw("SPRCURSTYMD")).ToString("yyyy/MM/dd")
                        End If
                        WW_SPRCURSTYMD_KETA = WW_ChkDate

                    Catch ex As Exception
                        WW_CheckMES1 = "・特例置換項目（現行）開始適用日エラーです。"
                        WW_CheckMES2 = "日付以外が入力されています。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End Try
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（現行）開始適用日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-現行終了適用日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRCURENDYMD", LNM0013INProw("SPRCURENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 画面表示の書式を変更
                If Not String.IsNullOrEmpty(LNM0013INProw("SPRCURENDYMD")) Then
                    Try
                        WW_ChkDate = Integer.Parse(LNM0013INProw("SPRCURENDYMD").Replace("/", ""))
                        If WW_ChkDate <= 9999 Then
                            ' 入力値が4桁以下の場合は2000年として日付チェックを行う
                            LNM0013INProw("SPRCURENDYMD") = WW_ChkDate
                            WW_ChkDate8str = 20000000 + WW_ChkDate
                            WW_ChkDate8ymd = WW_ChkDate8str.Substring(0, 4) & "/" & WW_ChkDate8str.Substring(4, 2) & "/" & WW_ChkDate8str.Substring(6, 2)
                            CDate(WW_ChkDate8ymd).ToString("yyyy/MM/dd")
                        Else
                            LNM0013INProw("SPRCURENDYMD") = CDate(LNM0013INProw("SPRCURENDYMD")).ToString("yyyy/MM/dd")
                        End If
                        WW_SPRCURENDYMD_KETA = WW_ChkDate

                    Catch ex As Exception
                        WW_CheckMES1 = "・特例置換項目（現行）終了適用日エラーです。"
                        WW_CheckMES2 = "日付以外が入力されています。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End Try
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（現行）終了適用日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-現行適用率
            Master.CheckField(Master.USERCAMP, "SPRCURAPPLYRATE", LNM0013INProw("SPRCURAPPLYRATE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                '適用率を数値に変換する
                If String.IsNullOrEmpty(LNM0013INProw("SPRCURAPPLYRATE")) Then
                    WW_SPRCURAPPLYRATE_INT = 0
                Else
                    WW_SPRCURAPPLYRATE_INT = CType(LNM0013INProw("SPRCURAPPLYRATE"), Integer)
                End If
            Else
                    WW_CheckMES1 = "・特例置換項目（現行）適用率エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-現行端数処理区分1
            Master.CheckField(Master.USERCAMP, "SPRCURROUNDKBN1", LNM0013INProw("SPRCURROUNDKBN1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0013INProw("SPRCURROUNDKBN1")) AndAlso
                   Not LNM0013INProw("SPRCURROUNDKBN1") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("SPRCURROUNDKBN1", LNM0013INProw("SPRCURROUNDKBN1"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "SPRCURROUNDKBN2", LNM0013INProw("SPRCURROUNDKBN2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0013INProw("SPRCURROUNDKBN2")) AndAlso
                   Not LNM0013INProw("SPRCURROUNDKBN2") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("SPRCURROUNDKBN2", LNM0013INProw("SPRCURROUNDKBN2"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "SPRNEXTSTYMD", LNM0013INProw("SPRNEXTSTYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 画面表示の書式を変更
                If Not String.IsNullOrEmpty(LNM0013INProw("SPRNEXTSTYMD")) Then
                    Try
                        WW_ChkDate = Integer.Parse(LNM0013INProw("SPRNEXTSTYMD").Replace("/", ""))
                        If WW_ChkDate <= 9999 Then
                            ' 入力値が4桁以下の場合は2000年として日付チェックを行う
                            LNM0013INProw("SPRNEXTSTYMD") = WW_ChkDate
                            WW_ChkDate8str = 20000000 + WW_ChkDate
                            WW_ChkDate8ymd = WW_ChkDate8str.Substring(0, 4) & "/" & WW_ChkDate8str.Substring(4, 2) & "/" & WW_ChkDate8str.Substring(6, 2)
                            CDate(WW_ChkDate8ymd).ToString("yyyy/MM/dd")
                        Else
                            LNM0013INProw("SPRNEXTSTYMD") = CDate(LNM0013INProw("SPRNEXTSTYMD")).ToString("yyyy/MM/dd")
                        End If
                        WW_SPRNEXTSTYMD_KETA = WW_ChkDate

                    Catch ex As Exception
                        WW_CheckMES1 = "・特例置換項目（次期）開始適用日エラーです。"
                        WW_CheckMES2 = "日付以外が入力されています。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End Try
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（次期）開始適用日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-次期終了適用日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRNEXTENDYMD", LNM0013INProw("SPRNEXTENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 画面表示の書式を変更
                If Not String.IsNullOrEmpty(LNM0013INProw("SPRNEXTENDYMD")) Then
                    Try
                        WW_ChkDate = Integer.Parse(LNM0013INProw("SPRNEXTENDYMD").Replace("/", ""))
                        If WW_ChkDate <= 9999 Then
                            ' 入力値が4桁以下の場合は2000年として日付チェックを行う
                            LNM0013INProw("SPRNEXTENDYMD") = WW_ChkDate
                            WW_ChkDate8str = 20000000 + WW_ChkDate
                            WW_ChkDate8ymd = WW_ChkDate8str.Substring(0, 4) & "/" & WW_ChkDate8str.Substring(4, 2) & "/" & WW_ChkDate8str.Substring(6, 2)
                            CDate(WW_ChkDate8ymd).ToString("yyyy/MM/dd")
                        Else
                            LNM0013INProw("SPRNEXTENDYMD") = CDate(LNM0013INProw("SPRNEXTENDYMD")).ToString("yyyy/MM/dd")
                        End If
                        WW_SPRNEXTENDYMD_KETA = WW_ChkDate

                    Catch ex As Exception
                        WW_CheckMES1 = "・特例置換項目（次期）終了適用日エラーです。"
                        WW_CheckMES2 = "日付以外が入力されています。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End Try
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（次期）終了適用日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-次期適用率
            Master.CheckField(Master.USERCAMP, "SPRNEXTAPPLYRATE", LNM0013INProw("SPRNEXTAPPLYRATE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                '適用率を数値に変換する
                If String.IsNullOrEmpty(LNM0013INProw("SPRNEXTAPPLYRATE")) Then
                    WW_SPRNEXTAPPLYRATE_INT = 0
                Else
                    WW_SPRNEXTAPPLYRATE_INT = CType(LNM0013INProw("SPRNEXTAPPLYRATE"), Integer)
                End If
            Else
                WW_CheckMES1 = "・特例置換項目（次期）適用率エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 特例置換項目-次期端数処理区分1
            Master.CheckField(Master.USERCAMP, "SPRNEXTROUNDKBN1", LNM0013INProw("SPRNEXTROUNDKBN1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0013INProw("SPRNEXTROUNDKBN1")) AndAlso
                   Not LNM0013INProw("SPRNEXTROUNDKBN1") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("SPRNEXTROUNDKBN1", LNM0013INProw("SPRNEXTROUNDKBN1"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "SPRNEXTROUNDKBN2", LNM0013INProw("SPRNEXTROUNDKBN2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0013INProw("SPRNEXTROUNDKBN2")) AndAlso
                   Not LNM0013INProw("SPRNEXTROUNDKBN2") = "0" Then
                    ' 値存在チェック
                    CODENAME_get("SPRNEXTROUNDKBN2", LNM0013INProw("SPRNEXTROUNDKBN2"), WW_Dummy, WW_RtnSW)
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

            '発駅コード、ＪＲ発支社支店コードの同時入力はエラー
            If Not String.IsNullOrEmpty(LNM0013INProw("DEPSTATION")) AndAlso
               Not LNM0013INProw("DEPSTATION") = "0" Then
                If Not String.IsNullOrEmpty(LNM0013INProw("JRDEPBRANCHCD")) AndAlso
                    Not LNM0013INProw("JRDEPBRANCHCD") = "0" Then
                    WW_CheckMES1 = "・発駅コード＆ＪＲ発支社支店コードエラーです。"
                    WW_CheckMES2 = "同時入力は行えません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '発駅コード、ＪＲ発支社支店コードどちらも未入力はエラー
            If String.IsNullOrEmpty(LNM0013INProw("DEPSTATION")) OrElse
                LNM0013INProw("DEPSTATION") = "0" Then
                If String.IsNullOrEmpty(LNM0013INProw("JRDEPBRANCHCD")) OrElse
                LNM0013INProw("JRDEPBRANCHCD") = "0" Then
                    WW_CheckMES1 = "・発駅コード＆ＪＲ発支社支店コードエラーです。"
                    WW_CheckMES2 = "何れかを入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '着駅コード、ＪＲ着支社支店コードの同時入力はエラー
            If Not String.IsNullOrEmpty(LNM0013INProw("ARRSTATION")) AndAlso
               Not LNM0013INProw("ARRSTATION") = "0" Then
                If Not String.IsNullOrEmpty(LNM0013INProw("JRARRBRANCHCD")) AndAlso
                 Not LNM0013INProw("JRARRBRANCHCD") = "0" Then
                    WW_CheckMES1 = "・着駅コード＆ＪＲ着支社支店コードエラーです。"
                    WW_CheckMES2 = "同時入力は行えません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '着駅コード、ＪＲ着支社支店コードどちらも未入力はエラー
            If String.IsNullOrEmpty(LNM0013INProw("ARRSTATION")) OrElse
                LNM0013INProw("ARRSTATION") = "0" Then
                If String.IsNullOrEmpty(LNM0013INProw("JRARRBRANCHCD")) OrElse
                LNM0013INProw("JRARRBRANCHCD") = "0" Then
                    WW_CheckMES1 = "・着駅コード＆ＪＲ着支社支店コードエラーです。"
                    WW_CheckMES2 = "何れかを入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '発受託人コードコード入力時、発駅コードの入力が必要
            If Not String.IsNullOrEmpty(LNM0013INProw("DEPTRUSTEECD")) OrElse
               Not String.IsNullOrEmpty(LNM0013INProw("DEPTRUSTEESUBCD")) Then
                If String.IsNullOrEmpty(LNM0013INProw("DEPSTATION")) OrElse
                LNM0013INProw("DEPSTATION") = "0" Then
                    WW_CheckMES1 = "・発受託人コード＆発受託人サブコードエラーです。"
                    WW_CheckMES2 = "発受託人コードを入力する場合、発駅コードも入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            'コンテナ番号（開始）入力時、コンテナ番号（終了）未入力はエラー
            If Not String.IsNullOrEmpty(LNM0013INProw("CTNSTNO")) AndAlso
               Not LNM0013INProw("CTNSTNO") = "0" Then
                If String.IsNullOrEmpty(LNM0013INProw("CTNENDNO")) OrElse
                 LNM0013INProw("CTNENDNO") = "0" Then
                    WW_CheckMES1 = "・コンテナ番号（終了）エラーです。"
                    WW_CheckMES2 = "コンテナ番号（開始）を入力する場合、コンテナ番号（終了）も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            'コンテナ番号（終了）入力時、コンテナ番号（開始）未入力はエラー
            If Not String.IsNullOrEmpty(LNM0013INProw("CTNENDNO")) AndAlso
               Not LNM0013INProw("CTNENDNO") = "0" Then
                If String.IsNullOrEmpty(LNM0013INProw("CTNSTNO")) OrElse
                LNM0013INProw("CTNSTNO") = "0" Then
                    WW_CheckMES1 = "・コンテナ番号（開始）エラーです。"
                    WW_CheckMES2 = "コンテナ番号（終了）を入力する場合、コンテナ番号（開始）も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            'コンテナ番号（開始）、コンテナ番号（終了）の大小関係チェック
            If Not String.IsNullOrEmpty(LNM0013INProw("CTNSTNO")) AndAlso
               Not LNM0013INProw("CTNSTNO") = "0" AndAlso
               Not String.IsNullOrEmpty(LNM0013INProw("CTNENDNO")) AndAlso
               Not LNM0013INProw("CTNENDNO") = "0" Then
                If LNM0013INProw("CTNSTNO") > LNM0013INProw("CTNENDNO") Then
                    WW_CheckMES1 = "・コンテナ番号（開始）＆コンテナ番号（終了）エラーです。"
                    WW_CheckMES2 = "コンテナ番号大小入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '現行開始適用日入力時、現行終了適用日未入力はエラー
            If Not String.IsNullOrEmpty(LNM0013INProw("SPRCURSTYMD")) AndAlso
                 String.IsNullOrEmpty(LNM0013INProw("SPRCURENDYMD")) Then
                WW_CheckMES1 = "・特例置換項目（現行）終了適用日エラーです。"
                WW_CheckMES2 = "開始適用日を入力する場合、終了適用日も入力して下さい。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '現行終了適用日入力時、現行開始適用日未入力はエラー
            If Not String.IsNullOrEmpty(LNM0013INProw("SPRCURENDYMD")) AndAlso
                 String.IsNullOrEmpty(LNM0013INProw("SPRCURSTYMD")) Then
                WW_CheckMES1 = "・特例置換項目（現行）開始適用日エラーです。"
                WW_CheckMES2 = "終了適用日を入力する場合、開始適用日も入力して下さい。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '現行開始適用日、現行終了適用日の入力が年月日または年月で揃っているかチェック
            If Not WW_SPRCURSTYMD_KETA = 0 AndAlso
               Not WW_SPRCURENDYMD_KETA = 0 Then
                If WW_SPRCURSTYMD_KETA > 9999 AndAlso
                   WW_SPRCURENDYMD_KETA < 9999 OrElse
                   WW_SPRCURSTYMD_KETA < 9999 AndAlso
                   WW_SPRCURENDYMD_KETA > 9999 Then
                    WW_CheckMES1 = "・特例置換項目（現行）開始適用日＆特例置換項目（現行）終了適用日エラーです。"
                    WW_CheckMES2 = "入力内容を年月日または月日で揃えて下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '現行開始適用日、現行終了適用日の大小関係チェック
            If WW_SPRCURSTYMD_KETA > 9999 AndAlso
               WW_SPRCURENDYMD_KETA > 9999 Then
                If WW_SPRCURSTYMD_KETA > WW_SPRCURENDYMD_KETA Then
                    WW_CheckMES1 = "・特例置換項目（現行）開始適用日＆特例置換項目（現行）終了適用日エラーです。"
                    WW_CheckMES2 = "大小入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '現行端数処理区分1、現行端数処理区分2のチェック
            If Not String.IsNullOrEmpty(LNM0013INProw("SPRCURROUNDKBN1")) AndAlso
               Not LNM0013INProw("SPRCURROUNDKBN1") = "0" Then
                If String.IsNullOrEmpty(LNM0013INProw("SPRCURROUNDKBN2")) OrElse
                    LNM0013INProw("SPRCURROUNDKBN2") = "0" Then
                    WW_CheckMES1 = "・特例置換項目（現行）端数処理区分1＆特例置換項目（現行）端数処理区分2エラーです。"
                    WW_CheckMES2 = "端数処理区分1を入力する場合、端数処理区分2も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                If Not String.IsNullOrEmpty(LNM0013INProw("SPRCURROUNDKBN2")) AndAlso
                   Not LNM0013INProw("SPRCURROUNDKBN2") = "0" Then
                    WW_CheckMES1 = "・特例置換項目（現行）端数処理区分1＆特例置換項目（現行）端数処理区分2エラーです。"
                    WW_CheckMES2 = "端数処理区分2を入力する場合、端数処理区分1も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '次期開始適用日入力時、次期終了適用日未入力はエラー
            If Not String.IsNullOrEmpty(LNM0013INProw("SPRNEXTSTYMD")) AndAlso
                 String.IsNullOrEmpty(LNM0013INProw("SPRNEXTENDYMD")) Then
                WW_CheckMES1 = "・特例置換項目（次期）終了適用日エラーです。"
                WW_CheckMES2 = "開始適用日を入力する場合、終了適用日も入力して下さい。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '次期終了適用日入力時、次期開始適用日未入力はエラー
            If Not String.IsNullOrEmpty(LNM0013INProw("SPRNEXTENDYMD")) AndAlso
                 String.IsNullOrEmpty(LNM0013INProw("SPRNEXTSTYMD")) Then
                WW_CheckMES1 = "・特例置換項目（次期）開始適用日エラーです。"
                WW_CheckMES2 = "終了適用日を入力する場合、開始適用日も入力して下さい。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '次期開始適用日、次期終了適用日の入力が年月日または年月で揃っているかチェック
            If Not WW_SPRNEXTSTYMD_KETA = 0 AndAlso
               Not WW_SPRNEXTENDYMD_KETA = 0 Then
                If WW_SPRNEXTSTYMD_KETA > 9999 AndAlso
                   WW_SPRNEXTENDYMD_KETA < 9999 OrElse
                   WW_SPRNEXTSTYMD_KETA < 9999 AndAlso
                   WW_SPRNEXTENDYMD_KETA > 9999 Then
                    WW_CheckMES1 = "・特例置換項目（次期）開始適用日＆特例置換項目（次期）終了適用日エラーです。"
                    WW_CheckMES2 = "入力内容を年月日または月日で揃えて下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '次期開始適用日、次期終了適用日の大小関係チェック
            If WW_SPRNEXTSTYMD_KETA > 9999 AndAlso
               WW_SPRNEXTENDYMD_KETA > 9999 Then
                If WW_SPRNEXTSTYMD_KETA > WW_SPRNEXTENDYMD_KETA Then
                    WW_CheckMES1 = "・特例置換項目（次期）開始適用日＆特例置換項目（次期）終了適用日エラーです。"
                    WW_CheckMES2 = "大小入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '次期端数処理区分1、次期端数処理区分2のチェック
            If Not String.IsNullOrEmpty(LNM0013INProw("SPRNEXTROUNDKBN1")) AndAlso
               Not LNM0013INProw("SPRNEXTROUNDKBN1") = "0" Then
                If String.IsNullOrEmpty(LNM0013INProw("SPRNEXTROUNDKBN2")) OrElse
                    LNM0013INProw("SPRNEXTROUNDKBN2") = "0" Then
                    WW_CheckMES1 = "・特例置換項目（次期）端数処理区分1＆特例置換項目（次期）端数処理区分2エラーです。"
                    WW_CheckMES2 = "端数処理区分1を入力する場合、端数処理区分2も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                If Not String.IsNullOrEmpty(LNM0013INProw("SPRNEXTROUNDKBN2")) AndAlso
                   Not LNM0013INProw("SPRNEXTROUNDKBN2") = "0" Then
                    WW_CheckMES1 = "・特例置換項目（次期）端数処理区分1＆特例置換項目（次期）端数処理区分2エラーです。"
                    WW_CheckMES2 = "端数処理区分2を入力する場合、端数処理区分1も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '現行終了適用日、次期開始適用日の大小関係チェック
            If WW_SPRCURENDYMD_KETA > 9999 AndAlso
               WW_SPRNEXTSTYMD_KETA > 9999 Then
                If WW_SPRCURENDYMD_KETA > WW_SPRNEXTSTYMD_KETA Then
                    WW_CheckMES1 = "・特例置換項目（現行）終了適用日＆特例置換項目（次期）開始適用日エラーです。"
                    WW_CheckMES2 = "大小入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '次期適用日を入力する場合、次期適用率も入力が必要
            If Not WW_SPRNEXTSTYMD_KETA = 0 OrElse
               Not WW_SPRNEXTENDYMD_KETA = 0 Then
                If WW_SPRNEXTAPPLYRATE_INT = 0 Then
                    WW_CheckMES1 = "・特例置換項目（次期）適用率エラーです。"
                    WW_CheckMES2 = "次期開始適用日または次期終了適用日を入力する場合、次期適用率も入力して下さい。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '次期適用率を入力する場合、次期適用日も入力が必要
            If Not WW_SPRNEXTAPPLYRATE_INT = 0 Then
                If WW_SPRNEXTSTYMD_KETA = 0 OrElse
                   WW_SPRNEXTENDYMD_KETA = 0 Then
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
                                    TxtBigCTNCD.Text, TxtMiddleCTNCD.Text,
                                    TxtPriorityNO.Text, TxtDepstation.Text,
                                    TxtJrDepBranchCD.Text, TxtArrstation.Text,
                                    TxtJrArrBranchCD.Text, work.WF_SEL_UPDTIMSTP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（大分類コード & 中分類コード & 優先順位 & 発駅コード & ＪＲ発支社支店コード & 着駅コード & ＪＲ着支社支店コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0013INProw("BIGCTNCD") & "]" &
                                           " [" & LNM0013INProw("MIDDLECTNCD") & "]" &
                                           " [" & LNM0013INProw("PRIORITYNO") & "]" &
                                           " [" & LNM0013INProw("DEPSTATION") & "]" &
                                           " [" & LNM0013INProw("JRDEPBRANCHCD") & "]" &
                                           " [" & LNM0013INProw("ARRSTATION") & "]" &
                                           " [" & LNM0013INProw("JRARRBRANCHCD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNM0013INProw("BIGCTNCD") = work.WF_SEL_BIGCTNCD2.Text OrElse
               Not LNM0013INProw("MIDDLECTNCD") = work.WF_SEL_MIDDLECTNCD2.Text OrElse
               Not LNM0013INProw("PRIORITYNO") = work.WF_SEL_PRIORITYNO.Text OrElse
               Not LNM0013INProw("DEPSTATION") = work.WF_SEL_DEPSTATION.Text OrElse
               Not LNM0013INProw("JRDEPBRANCHCD") = work.WF_SEL_JRDEPBRANCHCD.Text OrElse
               Not LNM0013INProw("ARRSTATION") = work.WF_SEL_ARRSTATION.Text OrElse
               Not LNM0013INProw("JRARRBRANCHCD") = work.WF_SEL_JRARRBRANCHCD.Text Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（大分類コード & 中分類コード & 優先順位 & 発駅コード & ＪＲ発支社支店コード & 着駅コード & ＪＲ着支社支店コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                           "([" & LNM0013INProw("BIGCTNCD") & "]" &
                                           " [" & LNM0013INProw("MIDDLECTNCD") & "]" &
                                           " [" & LNM0013INProw("PRIORITYNO") & "]" &
                                           " [" & LNM0013INProw("DEPSTATION") & "]" &
                                           " [" & LNM0013INProw("JRDEPBRANCHCD") & "]" &
                                           " [" & LNM0013INProw("ARRSTATION") & "]" &
                                           " [" & LNM0013INProw("JRARRBRANCHCD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LineErr = "" Then
                If LNM0013INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0013INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0013INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0013INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0013tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0013tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0013row As DataRow In LNM0013tbl.Rows
            Select Case LNM0013row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0013row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0013INProw As DataRow In LNM0013INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0013INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0013INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0013row As DataRow In LNM0013tbl.Rows
                ' KEY項目が等しい時
                If LNM0013row("BIGCTNCD") = LNM0013INProw("BIGCTNCD") AndAlso
                   LNM0013row("MIDDLECTNCD") = LNM0013INProw("MIDDLECTNCD") AndAlso
                   LNM0013row("PRIORITYNO") = LNM0013INProw("PRIORITYNO") AndAlso
                   LNM0013row("DEPSTATION") = LNM0013INProw("DEPSTATION") AndAlso
                   LNM0013row("JRDEPBRANCHCD") = LNM0013INProw("JRDEPBRANCHCD") AndAlso
                   LNM0013row("ARRSTATION") = LNM0013INProw("ARRSTATION") AndAlso
                   LNM0013row("JRARRBRANCHCD") = LNM0013INProw("JRARRBRANCHCD") Then
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0013row("PURPOSE") = LNM0013INProw("PURPOSE") AndAlso
                        LNM0013row("DEPTRUSTEECD") = LNM0013INProw("DEPTRUSTEECD") AndAlso
                        LNM0013row("DEPTRUSTEESUBCD") = LNM0013INProw("DEPTRUSTEESUBCD") AndAlso
                        LNM0013row("CTNTYPE") = LNM0013INProw("CTNTYPE") AndAlso
                        LNM0013row("CTNSTNO") = LNM0013INProw("CTNSTNO") AndAlso
                        LNM0013row("CTNENDNO") = LNM0013INProw("CTNENDNO") AndAlso
                        LNM0013row("SPRCURSTYMD") = LNM0013INProw("SPRCURSTYMD") AndAlso
                        LNM0013row("SPRCURENDYMD") = LNM0013INProw("SPRCURENDYMD") AndAlso
                        LNM0013row("SPRCURAPPLYRATE") = LNM0013INProw("SPRCURAPPLYRATE") AndAlso
                        LNM0013row("SPRCURROUNDKBN1") = LNM0013INProw("SPRCURROUNDKBN1") AndAlso
                        LNM0013row("SPRCURROUNDKBN2") = LNM0013INProw("SPRCURROUNDKBN2") AndAlso
                        LNM0013row("SPRNEXTSTYMD") = LNM0013INProw("SPRNEXTSTYMD") AndAlso
                        LNM0013row("SPRNEXTENDYMD") = LNM0013INProw("SPRNEXTENDYMD") AndAlso
                        LNM0013row("SPRNEXTAPPLYRATE") = LNM0013INProw("SPRNEXTAPPLYRATE") AndAlso
                        LNM0013row("SPRNEXTROUNDKBN1") = LNM0013INProw("SPRNEXTROUNDKBN1") AndAlso
                        LNM0013row("SPRNEXTROUNDKBN2") = LNM0013INProw("SPRNEXTROUNDKBN2") AndAlso
                        LNM0013row("DELFLG") = LNM0013INProw("DELFLG") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0013row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0013INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0013INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0013INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0013INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0013INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now

                '変更チェック
                REKTRMEXISTS(SQLcon, WW_MODIFYKBN)
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
        For Each LNM0013INProw As DataRow In LNM0013INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0013row As DataRow In LNM0013tbl.Rows
                ' 同一レコードか判定
                If LNM0013INProw("BIGCTNCD") = LNM0013row("BIGCTNCD") AndAlso
                    LNM0013INProw("MIDDLECTNCD") = LNM0013row("MIDDLECTNCD") AndAlso
                    LNM0013INProw("PRIORITYNO") = LNM0013row("PRIORITYNO") AndAlso
                    LNM0013INProw("DEPSTATION") = LNM0013row("DEPSTATION") AndAlso
                    LNM0013INProw("JRDEPBRANCHCD") = LNM0013row("JRDEPBRANCHCD") AndAlso
                    LNM0013INProw("ARRSTATION") = LNM0013row("ARRSTATION") AndAlso
                    LNM0013INProw("JRARRBRANCHCD") = LNM0013row("JRARRBRANCHCD") Then
                    ' 画面入力テーブル項目設定
                    LNM0013INProw("LINECNT") = LNM0013row("LINECNT")
                    LNM0013INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0013INProw("UPDTIMSTP") = LNM0013row("UPDTIMSTP")
                    LNM0013INProw("SELECT") = 0
                    LNM0013INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0013row.ItemArray = LNM0013INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0013tbl.NewRow
                WW_NRow.ItemArray = LNM0013INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0013tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0013tbl.Rows.Add(WW_NRow)
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
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, TxtBigCTNCD.Text))
                Case "DEPSTATION"                 '発駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "JRBRANCHCD"              'ＪＲ発支社支店コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "JRBRANCHCD"))
                Case "ARRSTATION"                 '着駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "DEPTRUSTEECD"               '発受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtDepstation.Text))
                Case "DEPTRUSTEESUBCD"            '発受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtDepstation.Text, TxtDepTrusteeCD.Text))
                Case "CTNTYPE"                    'コンテナ記号
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_TYPE))
                Case "CTNSTNO"                    'コンテナ番号（開始）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, TxtCTNType.Text))
                Case "CTNENDNO"                   'コンテナ番号（終了）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, TxtCTNType.Text))
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
