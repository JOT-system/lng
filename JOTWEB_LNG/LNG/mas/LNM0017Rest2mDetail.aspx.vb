''************************************************************
' 使用料特例２マスタメンテ登録画面
' 作成日 2022/02/25
' 更新日 2023/10/02
' 作成者 名取
' 更新者 大浜
'
' 修正履歴 : 2022/02/25 新規作成
'          : 2023/10/02 変更履歴登録機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 使用料特例２マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0017Rest2mDetail
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

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

        '○ 画面の値設定
        WW_MAPValueSet()

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
        LblSelLineCNT.Text = work.WF_SEL_LINECNT.Text
        '削除
        TxtDelFlg.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
        '画面ＩＤ
        TxtMapId.Text = "M00001"
        '組織コード
        TxtOrgCode.Text = work.WF_SEL_ORG2.Text
        CODENAME_get("ORG", TxtOrgCode.Text, LblOrgName.Text, WW_Dummy)
        '大分類コード
        TxtBigCTNCD.Text = work.WF_SEL_BIGCTNCD2.Text
        CODENAME_get("BIGCTNCD", TxtBigCTNCD.Text, LblBigCTNCDName.Text, WW_Dummy)
        '中分類コード
        TxtMiddleCTNCD.Text = work.WF_SEL_MIDDLECTNCD2.Text
        CODENAME_get("MIDDLECTNCD", TxtMiddleCTNCD.Text, LblMiddleCTNCDName.Text, WW_Dummy)
        '使用目的
        TxtPurpose.Text = work.WF_SEL_PURPOSE.Text
        '特例置換項目-使用料金額
        TxtSprUseFee.Text = work.WF_SEL_SPRUSEFEE.Text
        '特例置換項目-使用料率
        TxtSprUseFeeRate.Text = work.WF_SEL_SPRUSEFEERATE.Text
        '特例置換項目-使用料率端数整理
        TxtSprUseFeeRateRound1.Text = work.WF_SEL_SPRUSEFEERATEROUND1.Text
        CODENAME_get("HASUU1", TxtSprUseFeeRateRound1.Text, LblSprUseFeeRateRound1Name.Text, WW_Dummy)
        TxtSprUseFeeRateRound2.Text = work.WF_SEL_SPRUSEFEERATEROUND2.Text
        CODENAME_get("HASUU2", TxtSprUseFeeRateRound2.Text, LblSprUseFeeRateRound2Name.Text, WW_Dummy)
        '特例置換項目-使用料率加減額
        TxtSprUseFeeRateAddSub.Text = work.WF_SEL_SPRUSEFEERATEADDSUB.Text
        '特例置換項目-使用料率加減額端数整理
        TxtSprUseFeeRateAddSubCond1.Text = work.WF_SEL_SPRUSEFEERATEADDSUBCOND1.Text
        CODENAME_get("HASUU1", TxtSprUseFeeRateAddSubCond1.Text, LblSprUseFeeRateAddSubCond1Name.Text, WW_Dummy)
        TxtSprUseFeeRateAddSubCond2.Text = work.WF_SEL_SPRUSEFEERATEADDSUBCOND2.Text
        CODENAME_get("HASUU2", TxtSprUseFeeRateAddSubCond2.Text, LblSprUseFeeRateAddSubCond2Name.Text, WW_Dummy)
        '特例置換項目-端数処理時点区分
        TxtSprRoundPointKbn.Text = work.WF_SEL_SPRROUNDPOINTKBN.Text
        CODENAME_get("HASUUPOINTKBN", TxtSprRoundPointKbn.Text, LblSprRoundPointKbnName.Text, WW_Dummy)
        '特例置換項目-使用料無料特認
        TxtSprUseFreeSpe.Text = work.WF_SEL_SPRUSEFREESPE.Text
        CODENAME_get("USEFREEKBN", TxtSprUseFreeSpe.Text, LblSprUseFreeSpeName.Text, WW_Dummy)
        '特例置換項目-通運負担回送運賃
        TxtSprNittsuFreeSendFee.Text = work.WF_SEL_SPRNITTSUFREESENDFEE.Text
        '特例置換項目-運行管理料
        TxtSprManageFee.Text = work.WF_SEL_SPRMANAGEFEE.Text
        '特例置換項目-荷主負担運賃
        TxtSprShipBurdenFee.Text = work.WF_SEL_SPRSHIPBURDENFEE.Text
        '特例置換項目-発送料
        TxtSprShipFee.Text = work.WF_SEL_SPRSHIPFEE.Text
        '特例置換項目-到着料
        TxtSprArriveFee.Text = work.WF_SEL_SPRARRIVEFEE.Text
        '特例置換項目-集荷料
        TxtSprPickUpFee.Text = work.WF_SEL_SPRPICKUPFEE.Text
        '特例置換項目-配達料
        TxtSprDeliveryFee.Text = work.WF_SEL_SPRDELIVERYFEE.Text
        '特例置換項目-その他１
        TxtSprOther1.Text = work.WF_SEL_SPROTHER1.Text
        '特例置換項目-その他２
        TxtSprOther2.Text = work.WF_SEL_SPROTHER2.Text
        '特例置換項目-適合区分
        TxtSprFitKbn.Text = work.WF_SEL_SPRFITKBN.Text
        CODENAME_get("FITKBN", TxtSprFitKbn.Text, LblSprFitKbnName.Text, WW_Dummy)

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_ORG2.Text

        ' 数値(0～9)のみ入力可能とする。
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"                    '削除フラグ
        Me.TxtOrgCode.Attributes("onkeyPress") = "CheckNum()"                   '組織コード
        Me.TxtBigCTNCD.Attributes("onkeyPress") = "CheckNum()"                  '大分類コード
        Me.TxtMiddleCTNCD.Attributes("onkeyPress") = "CheckNum()"               '中分類コード
        Me.TxtSprUseFee.Attributes("onkeyPress") = "CheckNum()"                 '特例置換項目-使用料金額
        Me.TxtSprUseFeeRateRound1.Attributes("onkeyPress") = "CheckNum()"       '特例置換項目-使用料率端数整理1
        Me.TxtSprUseFeeRateRound2.Attributes("onkeyPress") = "CheckNum()"       '特例置換項目-使用料率端数整理2
        Me.TxtSprUseFeeRateAddSub.Attributes("onkeyPress") = "CheckNum()"       '特例置換項目-使用料率加減額
        Me.TxtSprUseFeeRateAddSubCond1.Attributes("onkeyPress") = "CheckNum()"  '特例置換項目-使用料率加減額端数整理1
        Me.TxtSprUseFeeRateAddSubCond2.Attributes("onkeyPress") = "CheckNum()"  '特例置換項目-使用料率加減額端数整理2
        Me.TxtSprRoundPointKbn.Attributes("onkeyPress") = "CheckNum()"          '特例置換項目-端数処理時点区分
        Me.TxtSprUseFreeSpe.Attributes("onkeyPress") = "CheckNum()"             '特例置換項目-使用料無料特認
        Me.TxtSprNittsuFreeSendFee.Attributes("onkeyPress") = "CheckNum()"      '特例置換項目-通運負担回送運賃
        Me.TxtSprManageFee.Attributes("onkeyPress") = "CheckNum()"              '特例置換項目-運行管理料
        Me.TxtSprShipBurdenFee.Attributes("onkeyPress") = "CheckNum()"          '特例置換項目-荷主負担運賃
        Me.TxtSprShipFee.Attributes("onkeyPress") = "CheckNum()"                '特例置換項目-発送料
        Me.TxtSprArriveFee.Attributes("onkeyPress") = "CheckNum()"              '特例置換項目-到着料
        Me.TxtSprPickUpFee.Attributes("onkeyPress") = "CheckNum()"              '特例置換項目-集荷料
        Me.TxtSprDeliveryFee.Attributes("onkeyPress") = "CheckNum()"            '特例置換項目-配達料
        Me.TxtSprOther1.Attributes("onkeyPress") = "CheckNum()"                 '特例置換項目-その他１
        Me.TxtSprOther2.Attributes("onkeyPress") = "CheckNum()"                 '特例置換項目-その他２
        Me.TxtSprFitKbn.Attributes("onkeyPress") = "CheckNum()"                 '特例置換項目-適合区分

        ' 入力するテキストボックスは数値(0～9)＋記号(.)のみ可能とする。
        Me.TxtSprUseFeeRate.Attributes("onkeyPress") = "CheckDeci()"             '特例置換項目-使用料率

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO"></param>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                     " _
            & "     ORGCODE                " _
            & "   , BIGCTNCD               " _
            & "   , MIDDLECTNCD            " _
            & " FROM                       " _
            & "     LNG.LNM0017_REST2M     " _
            & " WHERE                      " _
            & "         ORGCODE      = @P1 " _
            & "     AND BIGCTNCD     = @P2 " _
            & "     AND MIDDLECTNCD  = @P3 " _
            & "     AND DELFLG      <> @P4 "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 6) '組織コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 2) '大分類コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 2) '中分類コード
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 1) '削除フラグ

                PARA1.Value = TxtOrgCode.Text      '組織コード
                PARA2.Value = TxtBigCTNCD.Text     '大分類コード
                PARA3.Value = TxtMiddleCTNCD.Text  '中分類コード
                PARA4.Value = C_DELETE_FLG.DELETE  '削除フラグ

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0017Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0017Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0017Chk.Load(SQLdr)

                    If LNM0017Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0017C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0017C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 使用料特例マスタ２登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(使用料特例マスタ２)
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;                 " _
            & "     SET @hensuu = 0 ;                       " _
            & " DECLARE hensuu CURSOR FOR                   " _
            & "     SELECT                                  " _
            & "         UPDTIMSTP AS hensuu                 " _
            & "     FROM                                    " _
            & "         LNG.LNM0017_REST2M                  " _
            & "     WHERE                                   " _
            & "         ORGCODE         = @P01              " _
            & "     AND BIGCTNCD        = @P02              " _
            & "     AND MIDDLECTNCD     = @P03 ;            " _
            & " OPEN hensuu ;                               " _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;       " _
            & " IF (@@FETCH_STATUS = 0)                     " _
            & "     UPDATE LNG.LNM0017_REST2M               " _
            & "     SET                                     " _
            & "         DELFLG                  = @P00      " _
            & "       , PURPOSE                 = @P04      " _
            & "       , SPRUSEFEE               = @P05      " _
            & "       , SPRUSEFEERATE           = @P06      " _
            & "       , SPRUSEFEERATEROUND      = @P07      " _
            & "       , SPRUSEFEERATEADDSUB     = @P08      " _
            & "       , SPRUSEFEERATEADDSUBCOND = @P09      " _
            & "       , SPRROUNDPOINTKBN        = @P10      " _
            & "       , SPRUSEFREESPE           = @P11      " _
            & "       , SPRNITTSUFREESENDFEE    = @P12      " _
            & "       , SPRMANAGEFEE            = @P13      " _
            & "       , SPRSHIPBURDENFEE        = @P14      " _
            & "       , SPRSHIPFEE              = @P15      " _
            & "       , SPRARRIVEFEE            = @P16      " _
            & "       , SPRPICKUPFEE            = @P17      " _
            & "       , SPRDELIVERYFEE          = @P18      " _
            & "       , SPROTHER1               = @P19      " _
            & "       , SPROTHER2               = @P20      " _
            & "       , SPRFITKBN               = @P21      " _
            & "       , UPDYMD                  = @P27      " _
            & "       , UPDUSER                 = @P28      " _
            & "       , UPDTERMID               = @P29      " _
            & "       , UPDPGID                 = @P30      " _
            & "       , RECEIVEYMD              = @P31      " _
            & "     WHERE                                   " _
            & "         ORGCODE         = @P01              " _
            & "     AND BIGCTNCD        = @P02              " _
            & "     AND MIDDLECTNCD     = @P03 ;            " _
            & " IF (@@FETCH_STATUS <> 0)                    " _
            & "     INSERT INTO LNG.LNM0017_REST2M          " _
            & "        (DELFLG                              " _
            & "       , ORGCODE                             " _
            & "       , BIGCTNCD                            " _
            & "       , MIDDLECTNCD                         " _
            & "       , PURPOSE                             " _
            & "       , SPRUSEFEE                           " _
            & "       , SPRUSEFEERATE                       " _
            & "       , SPRUSEFEERATEROUND                  " _
            & "       , SPRUSEFEERATEADDSUB                 " _
            & "       , SPRUSEFEERATEADDSUBCOND             " _
            & "       , SPRROUNDPOINTKBN                    " _
            & "       , SPRUSEFREESPE                       " _
            & "       , SPRNITTSUFREESENDFEE                " _
            & "       , SPRMANAGEFEE                        " _
            & "       , SPRSHIPBURDENFEE                    " _
            & "       , SPRSHIPFEE                          " _
            & "       , SPRARRIVEFEE                        " _
            & "       , SPRPICKUPFEE                        " _
            & "       , SPRDELIVERYFEE                      " _
            & "       , SPROTHER1                           " _
            & "       , SPROTHER2                           " _
            & "       , SPRFITKBN                           " _
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
            & "       , @P23                                " _
            & "       , @P24                                " _
            & "       , @P25                                " _
            & "       , @P26                                " _
            & "       , @P27                                " _
            & "       , @P28                                " _
            & "       , @P29                                " _
            & "       , @P30                                " _
            & "       , @P31) ;                             " _
            & " CLOSE hensuu ;                              " _
            & " DEALLOCATE hensuu ;                         "

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                     " _
            & "    DELFLG                                  " _
            & "  , ORGCODE                                 " _
            & "  , BIGCTNCD                                " _
            & "  , MIDDLECTNCD                             " _
            & "  , PURPOSE                                 " _
            & "  , SPRUSEFEE                               " _
            & "  , SPRUSEFEERATE                           " _
            & "  , SPRUSEFEERATEROUND                      " _
            & "  , SPRUSEFEERATEADDSUB                     " _
            & "  , SPRUSEFEERATEADDSUBCOND                 " _
            & "  , SPRROUNDPOINTKBN                        " _
            & "  , SPRUSEFREESPE                           " _
            & "  , SPRNITTSUFREESENDFEE                    " _
            & "  , SPRMANAGEFEE                            " _
            & "  , SPRSHIPBURDENFEE                        " _
            & "  , SPRSHIPFEE                              " _
            & "  , SPRARRIVEFEE                            " _
            & "  , SPRPICKUPFEE                            " _
            & "  , SPRDELIVERYFEE                          " _
            & "  , SPROTHER1                               " _
            & "  , SPROTHER2                               " _
            & "  , SPRFITKBN                               " _
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
            & "    LNG.LNM0017_REST2M                      " _
            & " WHERE                                      " _
            & "        ORGCODE         = @P01              " _
            & "    AND BIGCTNCD        = @P02              " _
            & "    AND MIDDLECTNCD     = @P03              "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                ' DB更新用パラメータ
                Dim PARA00 As MySqlParameter = SQLcmd.Parameters.Add("@P00", MySqlDbType.VarChar, 1)     '削除フラグ
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 6)     '組織コード
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 2)     '大分類コード
                Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 2)     '中分類コード
                Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar, 42)    '使用目的
                Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar, 7)     '特例置換項目-使用料金額
                Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.Decimal, 5, 4)   '特例置換項目-使用料率
                Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar, 2)     '特例置換項目-使用料率端数整理
                Dim PARA08 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.VarChar, 7)     '特例置換項目-使用料率加減額
                Dim PARA09 As MySqlParameter = SQLcmd.Parameters.Add("@P09", MySqlDbType.VarChar, 2)     '特例置換項目-使用料率加減額端数整理
                Dim PARA10 As MySqlParameter = SQLcmd.Parameters.Add("@P10", MySqlDbType.VarChar, 2)     '特例置換項目-端数処理時点区分
                Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.VarChar, 2)     '特例置換項目-使用料無料特認
                Dim PARA12 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.VarChar, 7)     '特例置換項目-通運負担回送運賃
                Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar, 7)     '特例置換項目-運行管理料
                Dim PARA14 As MySqlParameter = SQLcmd.Parameters.Add("@P14", MySqlDbType.VarChar, 7)     '特例置換項目-荷主負担運賃
                Dim PARA15 As MySqlParameter = SQLcmd.Parameters.Add("@P15", MySqlDbType.VarChar, 7)     '特例置換項目-発送料
                Dim PARA16 As MySqlParameter = SQLcmd.Parameters.Add("@P16", MySqlDbType.VarChar, 7)     '特例置換項目-到着料
                Dim PARA17 As MySqlParameter = SQLcmd.Parameters.Add("@P17", MySqlDbType.VarChar, 7)     '特例置換項目-集荷料
                Dim PARA18 As MySqlParameter = SQLcmd.Parameters.Add("@P18", MySqlDbType.VarChar, 7)     '特例置換項目-配達料
                Dim PARA19 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.VarChar, 7)     '特例置換項目-その他１
                Dim PARA20 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.VarChar, 7)     '特例置換項目-その他２
                Dim PARA21 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.VarChar, 2)     '特例置換項目-適合区分
                Dim PARA23 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.DateTime)        '登録年月日
                Dim PARA24 As MySqlParameter = SQLcmd.Parameters.Add("@P24", MySqlDbType.VarChar, 20)    '登録ユーザーＩＤ
                Dim PARA25 As MySqlParameter = SQLcmd.Parameters.Add("@P25", MySqlDbType.VarChar, 20)    '登録端末
                Dim PARA26 As MySqlParameter = SQLcmd.Parameters.Add("@P26", MySqlDbType.VarChar, 40)    '登録プログラムＩＤ
                Dim PARA27 As MySqlParameter = SQLcmd.Parameters.Add("@P27", MySqlDbType.DateTime)        '更新年月日
                Dim PARA28 As MySqlParameter = SQLcmd.Parameters.Add("@P28", MySqlDbType.VarChar, 20)    '更新ユーザーＩＤ
                Dim PARA29 As MySqlParameter = SQLcmd.Parameters.Add("@P29", MySqlDbType.VarChar, 20)    '更新端末
                Dim PARA30 As MySqlParameter = SQLcmd.Parameters.Add("@P30", MySqlDbType.VarChar, 40)    '更新プログラムＩＤ
                Dim PARA31 As MySqlParameter = SQLcmd.Parameters.Add("@P31", MySqlDbType.DateTime)        '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JPARA01 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 6)  '組織コード
                Dim JPARA02 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P02", MySqlDbType.VarChar, 2)  '大分類コード
                Dim JPARA03 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P03", MySqlDbType.VarChar, 2)  '中分類コード

                Dim LNM0017row As DataRow = LNM0017INPtbl.Rows(0)

                ' DB更新
                PARA00.Value = LNM0017row("DELFLG")                                                '削除フラグ
                PARA01.Value = LNM0017row("ORGCODE")                                               '組織コード
                PARA02.Value = LNM0017row("BIGCTNCD")                                              '大分類コード
                PARA03.Value = LNM0017row("MIDDLECTNCD")                                           '中分類コード
                If String.IsNullOrEmpty(LNM0017row("PURPOSE")) Then                                '使用目的
                    PARA04.Value = DBNull.Value
                Else
                    PARA04.Value = LNM0017row("PURPOSE")
                End If

                If String.IsNullOrEmpty(LNM0017row("SPRUSEFEE")) Then                              '特例置換項目-使用料金額
                    PARA05.Value = DBNull.Value
                Else
                    PARA05.Value = LNM0017row("SPRUSEFEE")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRUSEFEERATE")) Then                          '特例置換項目-使用料率
                    PARA06.Value = "0"
                Else
                    PARA06.Value = LNM0017row("SPRUSEFEERATE")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRUSEFEERATEROUND")) Then                     '特例置換項目-使用料率端数整理
                    PARA07.Value = "0"
                Else
                    PARA07.Value = LNM0017row("SPRUSEFEERATEROUND")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRUSEFEERATEADDSUB")) Then                    '特例置換項目-使用料率加減額
                    PARA08.Value = DBNull.Value
                Else
                    PARA08.Value = LNM0017row("SPRUSEFEERATEADDSUB")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRUSEFEERATEADDSUBCOND")) Then                '特例置換項目-使用料率加減額端数整理
                    PARA09.Value = "0"
                Else
                    PARA09.Value = LNM0017row("SPRUSEFEERATEADDSUBCOND")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRROUNDPOINTKBN")) Then                       '特例置換項目-端数処理時点区分
                    PARA10.Value = DBNull.Value
                Else
                    PARA10.Value = LNM0017row("SPRROUNDPOINTKBN")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRUSEFREESPE")) Then                          '特例置換項目-使用料無料特認
                    PARA11.Value = "0"
                Else
                    PARA11.Value = LNM0017row("SPRUSEFREESPE")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRNITTSUFREESENDFEE")) Then                   '特例置換項目-通運負担回送運賃
                    PARA12.Value = DBNull.Value
                Else
                    PARA12.Value = LNM0017row("SPRNITTSUFREESENDFEE")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRMANAGEFEE")) Then                           '特例置換項目-運行管理料
                    PARA13.Value = DBNull.Value
                Else
                    PARA13.Value = LNM0017row("SPRMANAGEFEE")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRSHIPBURDENFEE")) Then                       '特例置換項目-荷主負担運賃
                    PARA14.Value = DBNull.Value
                Else
                    PARA14.Value = LNM0017row("SPRSHIPBURDENFEE")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRSHIPFEE")) Then                             '特例置換項目-発送料
                    PARA15.Value = DBNull.Value
                Else
                    PARA15.Value = LNM0017row("SPRSHIPFEE")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRARRIVEFEE")) Then                           '特例置換項目-到着料
                    PARA16.Value = DBNull.Value
                Else
                    PARA16.Value = LNM0017row("SPRARRIVEFEE")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRPICKUPFEE")) Then                           '特例置換項目-集荷料
                    PARA17.Value = DBNull.Value
                Else
                    PARA17.Value = LNM0017row("SPRPICKUPFEE")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRDELIVERYFEE")) Then                         '特例置換項目-配達料
                    PARA18.Value = DBNull.Value
                Else
                    PARA18.Value = LNM0017row("SPRDELIVERYFEE")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPROTHER1")) Then                              '特例置換項目-その他１
                    PARA19.Value = DBNull.Value
                Else
                    PARA19.Value = LNM0017row("SPROTHER1")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPROTHER2")) Then                              '特例置換項目-その他２
                    PARA20.Value = DBNull.Value
                Else
                    PARA20.Value = LNM0017row("SPROTHER2")
                End If
                If String.IsNullOrEmpty(LNM0017row("SPRFITKBN")) Then                              '特例置換項目-適合区分
                    PARA21.Value = DBNull.Value
                Else
                    PARA21.Value = LNM0017row("SPRFITKBN")
                End If
                PARA23.Value = WW_NOW                                                              '登録年月日
                PARA24.Value = Master.USERID                                                       '登録ユーザーＩＤ
                PARA25.Value = Master.USERTERMID                                                   '登録端末
                PARA26.Value = Me.GetType().BaseType.Name                                          '登録プログラムＩＤ
                PARA27.Value = WW_NOW                                                              '更新年月日
                PARA28.Value = Master.USERID                                                       '更新ユーザーＩＤ
                PARA29.Value = Master.USERTERMID                                                   '更新端末
                PARA30.Value = Me.GetType().BaseType.Name                                          '更新プログラムＩＤ
                PARA31.Value = C_DEFAULT_YMD                                                       '集信日時
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA01.Value = LNM0017row("ORGCODE")
                JPARA02.Value = LNM0017row("BIGCTNCD")
                JPARA03.Value = LNM0017row("MIDDLECTNCD")

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
                    CS0020JOURNAL.TABLENM = "LNM0017C"
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0017C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0017C UPDATE_INSERT"
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
    Protected Sub REST2MEXISTS(ByVal SQLcon As MySqlConnection, ByRef WW_MODIFYKBN As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '使用料特例マスタ２に同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        ORGCODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0017_REST2M")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        ORGCODE         = @ORGCODE")
        SQLStr.AppendLine("    AND BIGCTNCD        = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD     = @MIDDLECTNCD")
        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード

                Dim LNM0016row As DataRow = LNM0017INPtbl.Rows(0)

                P_ORGCODE.Value = LNM0016row("ORGCODE")               '組織コード
                P_BIGCTNCD.Value = LNM0016row("BIGCTNCD")             '大分類コード
                P_MIDDLECTNCD.Value = LNM0016row("MIDDLECTNCD")       '中分類コード


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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0017C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0017C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0096_REST2HIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         ORGCODE  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,SPRUSEFEE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEROUND  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUB  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUBCOND  ")
        SQLStr.AppendLine("        ,SPRROUNDPOINTKBN  ")
        SQLStr.AppendLine("        ,SPRUSEFREESPE  ")
        SQLStr.AppendLine("        ,SPRNITTSUFREESENDFEE  ")
        SQLStr.AppendLine("        ,SPRMANAGEFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPBURDENFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPFEE  ")
        SQLStr.AppendLine("        ,SPRARRIVEFEE  ")
        SQLStr.AppendLine("        ,SPRPICKUPFEE  ")
        SQLStr.AppendLine("        ,SPRDELIVERYFEE  ")
        SQLStr.AppendLine("        ,SPROTHER1  ")
        SQLStr.AppendLine("        ,SPROTHER2  ")
        SQLStr.AppendLine("        ,SPRFITKBN  ")
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
        SQLStr.AppendLine("         ORGCODE  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,SPRUSEFEE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEROUND  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUB  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUBCOND  ")
        SQLStr.AppendLine("        ,SPRROUNDPOINTKBN  ")
        SQLStr.AppendLine("        ,SPRUSEFREESPE  ")
        SQLStr.AppendLine("        ,SPRNITTSUFREESENDFEE  ")
        SQLStr.AppendLine("        ,SPRMANAGEFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPBURDENFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPFEE  ")
        SQLStr.AppendLine("        ,SPRARRIVEFEE  ")
        SQLStr.AppendLine("        ,SPRPICKUPFEE  ")
        SQLStr.AppendLine("        ,SPRDELIVERYFEE  ")
        SQLStr.AppendLine("        ,SPROTHER1  ")
        SQLStr.AppendLine("        ,SPROTHER2  ")
        SQLStr.AppendLine("        ,SPRFITKBN  ")
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
        SQLStr.AppendLine("        LNG.LNM0017_REST2M")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        ORGCODE         = @ORGCODE")
        SQLStr.AppendLine("    AND BIGCTNCD        = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD     = @MIDDLECTNCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0017row As DataRow = LNM0017INPtbl.Rows(0)

                ' DB更新
                P_ORGCODE.Value = LNM0017row("ORGCODE")               '組織コード
                P_BIGCTNCD.Value = LNM0017row("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = LNM0017row("MIDDLECTNCD")               '中分類コード

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0096_REST2HIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0096_REST2HIST  INSERT"
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
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, "使用料特例２", needsPopUp:=True)
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
        Master.EraseCharToIgnore(TxtDelFlg.Text)                    '削除フラグ
        Master.EraseCharToIgnore(TxtOrgCode.Text)                   '組織コード
        Master.EraseCharToIgnore(TxtBigCTNCD.Text)                  '大分類コード
        Master.EraseCharToIgnore(TxtMiddleCTNCD.Text)               '中分類コード
        Master.EraseCharToIgnore(TxtPurpose.Text)                   '使用目的
        Master.EraseCharToIgnore(TxtSprUseFee.Text)                 '特例置換項目-使用料金額
        Master.EraseCharToIgnore(TxtSprUseFeeRate.Text)             '特例置換項目-使用料率
        Master.EraseCharToIgnore(TxtSprUseFeeRateRound1.Text)       '特例置換項目-使用料率端数整理1
        Master.EraseCharToIgnore(TxtSprUseFeeRateRound2.Text)       '特例置換項目-使用料率端数整理2
        Master.EraseCharToIgnore(TxtSprUseFeeRateAddSub.Text)       '特例置換項目-使用料率加減額
        Master.EraseCharToIgnore(TxtSprUseFeeRateAddSubCond1.Text)  '特例置換項目-使用料率加減額端数整理1
        Master.EraseCharToIgnore(TxtSprUseFeeRateAddSubCond2.Text)  '特例置換項目-使用料率加減額端数整理2
        Master.EraseCharToIgnore(TxtSprRoundPointKbn.Text)          '特例置換項目-端数処理時点区分
        Master.EraseCharToIgnore(TxtSprUseFreeSpe.Text)             '特例置換項目-使用料無料特認
        Master.EraseCharToIgnore(TxtSprNittsuFreeSendFee.Text)      '特例置換項目-通運負担回送運賃
        Master.EraseCharToIgnore(TxtSprManageFee.Text)              '特例置換項目-運行管理料
        Master.EraseCharToIgnore(TxtSprShipBurdenFee.Text)          '特例置換項目-荷主負担運賃
        Master.EraseCharToIgnore(TxtSprShipFee.Text)                '特例置換項目-発送料
        Master.EraseCharToIgnore(TxtSprArriveFee.Text)              '特例置換項目-到着料
        Master.EraseCharToIgnore(TxtSprPickUpFee.Text)              '特例置換項目-集荷料
        Master.EraseCharToIgnore(TxtSprDeliveryFee.Text)            '特例置換項目-配達料
        Master.EraseCharToIgnore(TxtSprOther1.Text)                 '特例置換項目-その他１
        Master.EraseCharToIgnore(TxtSprOther2.Text)                 '特例置換項目-その他２
        Master.EraseCharToIgnore(TxtSprFitKbn.Text)                 '特例置換項目-適合区分

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

        Master.CreateEmptyTable(LNM0017INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0017INProw As DataRow = LNM0017INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(LblSelLineCNT.Text) Then
            LNM0017INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(LblSelLineCNT.Text, LNM0017INProw("LINECNT"))
            Catch ex As Exception
                LNM0017INProw("LINECNT") = 0
            End Try
        End If

        LNM0017INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0017INProw("UPDTIMSTP") = 0
        LNM0017INProw("SELECT") = 1
        LNM0017INProw("HIDDEN") = 0

        LNM0017INProw("DELFLG") = TxtDelFlg.Text                                        '削除フラグ
        LNM0017INProw("ORGCODE") = TxtOrgCode.Text                                      '組織コード
        LNM0017INProw("BIGCTNCD") = TxtBigCTNCD.Text                                    '大分類コード
        LNM0017INProw("MIDDLECTNCD") = TxtMiddleCTNCD.Text                              '中分類コード
        LNM0017INProw("PURPOSE") = TxtPurpose.Text                                      '使用目的
        LNM0017INProw("SPRUSEFEE") = TxtSprUseFee.Text                                  '特例置換項目-使用料金額
        LNM0017INProw("SPRUSEFEERATE") = TxtSprUseFeeRate.Text                          '特例置換項目-使用料率
        LNM0017INProw("SPRUSEFEERATEROUND") = TxtSprUseFeeRateRound1.Text &             '特例置換項目-使用料率端数整理
                                              TxtSprUseFeeRateRound2.Text
        LNM0017INProw("SPRUSEFEERATEROUND1") = TxtSprUseFeeRateRound1.Text              '特例置換項目-使用料率端数整理1
        LNM0017INProw("SPRUSEFEERATEROUND2") = TxtSprUseFeeRateRound2.Text              '特例置換項目-使用料率端数整理2
        LNM0017INProw("SPRUSEFEERATEADDSUB") = TxtSprUseFeeRateAddSub.Text              '特例置換項目-使用料率加減額
        LNM0017INProw("SPRUSEFEERATEADDSUBCOND") = TxtSprUseFeeRateAddSubCond1.Text &   '特例置換項目-使用料率加減額端数整理
                                                   TxtSprUseFeeRateAddSubCond2.Text
        LNM0017INProw("SPRUSEFEERATEADDSUBCOND1") = TxtSprUseFeeRateAddSubCond1.Text    '特例置換項目-使用料率加減額端数整理1
        LNM0017INProw("SPRUSEFEERATEADDSUBCOND2") = TxtSprUseFeeRateAddSubCond2.Text    '特例置換項目-使用料率加減額端数整理2
        LNM0017INProw("SPRROUNDPOINTKBN") = TxtSprRoundPointKbn.Text                    '特例置換項目-端数処理時点区分
        LNM0017INProw("SPRUSEFREESPE") = TxtSprUseFreeSpe.Text                          '特例置換項目-使用料無料特認
        LNM0017INProw("SPRNITTSUFREESENDFEE") = TxtSprNittsuFreeSendFee.Text            '特例置換項目-通運負担回送運賃
        LNM0017INProw("SPRMANAGEFEE") = TxtSprManageFee.Text                            '特例置換項目-運行管理料
        LNM0017INProw("SPRSHIPBURDENFEE") = TxtSprShipBurdenFee.Text                    '特例置換項目-荷主負担運賃
        LNM0017INProw("SPRSHIPFEE") = TxtSprShipFee.Text                                '特例置換項目-発送料
        LNM0017INProw("SPRARRIVEFEE") = TxtSprArriveFee.Text                            '特例置換項目-到着料
        LNM0017INProw("SPRPICKUPFEE") = TxtSprPickUpFee.Text                            '特例置換項目-集荷料
        LNM0017INProw("SPRDELIVERYFEE") = TxtSprDeliveryFee.Text                        '特例置換項目-配達料
        LNM0017INProw("SPROTHER1") = TxtSprOther1.Text                                  '特例置換項目-その他１
        LNM0017INProw("SPROTHER2") = TxtSprOther2.Text                                  '特例置換項目-その他２
        LNM0017INProw("SPRFITKBN") = TxtSprFitKbn.Text                                  '特例置換項目-適合区分

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
            If LNM0017row("ORGCODE") = LNM0017INProw("ORGCODE") AndAlso                                      '組織コード
               LNM0017row("BIGCTNCD") = LNM0017INProw("BIGCTNCD") AndAlso                                    '大分類コード
               LNM0017row("MIDDLECTNCD") = LNM0017INProw("MIDDLECTNCD") Then                                 '中分類コード
                ' KEY項目以外の項目の差異をチェック
                If LNM0017row("DELFLG") = LNM0017INProw("DELFLG") AndAlso                                    '削除フラグ
                   LNM0017row("PURPOSE") = LNM0017INProw("PURPOSE") AndAlso                                  '使用目的
                   LNM0017row("SPRUSEFEE") = LNM0017INProw("SPRUSEFEE") AndAlso                              '特例置換項目-使用料金額
                   LNM0017row("SPRUSEFEERATE") = LNM0017INProw("SPRUSEFEERATE") AndAlso                      '特例置換項目-使用料率
                   LNM0017row("SPRUSEFEERATEROUND") = LNM0017INProw("SPRUSEFEERATEROUND") AndAlso            '特例置換項目-使用料率端数整理
                   LNM0017row("SPRUSEFEERATEADDSUB") = LNM0017INProw("SPRUSEFEERATEADDSUB") AndAlso          '特例置換項目-使用料率加減額
                   LNM0017row("SPRUSEFEERATEADDSUBCOND") = LNM0017INProw("SPRUSEFEERATEADDSUBCOND") AndAlso  '特例置換項目-使用料率加減額端数整理
                   LNM0017row("SPRROUNDPOINTKBN") = LNM0017INProw("SPRROUNDPOINTKBN") AndAlso                '特例置換項目-端数処理時点区分
                   LNM0017row("SPRUSEFREESPE") = LNM0017INProw("SPRUSEFREESPE") AndAlso                      '特例置換項目-使用料無料特認
                   LNM0017row("SPRNITTSUFREESENDFEE") = LNM0017INProw("SPRNITTSUFREESENDFEE") AndAlso        '特例置換項目-通運負担回送運賃
                   LNM0017row("SPRMANAGEFEE") = LNM0017INProw("SPRMANAGEFEE") AndAlso                        '特例置換項目-運行管理料
                   LNM0017row("SPRSHIPBURDENFEE") = LNM0017INProw("SPRSHIPBURDENFEE") AndAlso                '特例置換項目-荷主負担運賃
                   LNM0017row("SPRSHIPFEE") = LNM0017INProw("SPRSHIPFEE") AndAlso                            '特例置換項目-発送料
                   LNM0017row("SPRARRIVEFEE") = LNM0017INProw("SPRARRIVEFEE") AndAlso                        '特例置換項目-到着料
                   LNM0017row("SPRPICKUPFEE") = LNM0017INProw("SPRPICKUPFEE") AndAlso                        '特例置換項目-集荷料
                   LNM0017row("SPRDELIVERYFEE") = LNM0017INProw("SPRDELIVERYFEE") AndAlso                    '特例置換項目-配達料
                   LNM0017row("SPROTHER1") = LNM0017INProw("SPROTHER1") AndAlso                              '特例置換項目-その他１
                   LNM0017row("SPROTHER2") = LNM0017INProw("SPROTHER2") AndAlso                              '特例置換項目-その他２
                   LNM0017row("SPRFITKBN") = LNM0017INProw("SPRFITKBN") Then                                 '特例置換項目-適合区分
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

        LblSelLineCNT.Text = ""                'LINECNT
        TxtMapId.Text = "M00001"               '画面ＩＤ
        TxtDelFlg.Text = ""                    '削除フラグ
        TxtOrgCode.Text = ""                   '組織コード
        TxtBigCTNCD.Text = ""                  '大分類コード
        TxtMiddleCTNCD.Text = ""               '中分類コード
        TxtPurpose.Text = ""                   '使用目的
        TxtSprUseFee.Text = ""                 '特例置換項目-使用料金額
        TxtSprUseFeeRate.Text = ""             '特例置換項目-使用料率
        TxtSprUseFeeRateRound1.Text = ""       '特例置換項目-使用料率端数整理
        TxtSprUseFeeRateRound2.Text = ""
        TxtSprUseFeeRateAddSub.Text = ""       '特例置換項目-使用料率加減額
        TxtSprUseFeeRateAddSubCond1.Text = ""  '特例置換項目-使用料率加減額端数整理
        TxtSprUseFeeRateAddSubCond2.Text = ""
        TxtSprRoundPointKbn.Text = ""         '特例置換項目-端数処理時点区分
        TxtSprUseFreeSpe.Text = ""             '特例置換項目-使用料無料特認
        TxtSprNittsuFreeSendFee.Text = ""      '特例置換項目-通運負担回送運賃
        TxtSprManageFee.Text = ""              '特例置換項目-運行管理料
        TxtSprShipBurdenFee.Text = ""          '特例置換項目-荷主負担運賃
        TxtSprShipFee.Text = ""                '特例置換項目-発送料
        TxtSprArriveFee.Text = ""              '特例置換項目-到着料
        TxtSprPickUpFee.Text = ""              '特例置換項目-集荷料
        TxtSprDeliveryFee.Text = ""            '特例置換項目-配達料
        TxtSprOther1.Text = ""                 '特例置換項目-その他１
        TxtSprOther2.Text = ""                 '特例置換項目-その他２
        TxtSprFitKbn.Text = ""                 '特例置換項目-適合区分

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
                    Case "TxtOrgCode",              '組織コード
                         "TxtSlcJrDepBranchCd",              '選択比較項目-ＪＲ発支社支店コード
                         "TxtSlcJrArrBranchCd",              '選択比較項目-ＪＲ着支社支店コード
                         "TxtSlcJotArrOrgCode"               '選択比較項目-ＪＯＴ着組織コード
                        WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP)
                    Case "TxtBigCTNCD"                       '大分類コード
                        WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                    Case "TxtMiddleCTNCD"                    '中分類コード
                        WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, TxtBigCTNCD.Text)
                    Case "TxtSprUseFeeRateRound1",           '特例置換項目-使用料率端数整理1
                         "TxtSprUseFeeRateAddSubCond1"       '特例置換項目-使用料率加減額端数整理1
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUU1")
                    Case "TxtSprUseFeeRateRound2",           '特例置換項目-使用料率端数整理2
                         "TxtSprUseFeeRateAddSubCond2"       '特例置換項目-使用料率加減額端数整理2
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUU2")
                    Case "TxtSprRoundPointKbn"               '特例置換項目-端数処理時点区分
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUUPOINTKBN")
                    Case "TxtSprUseFreeSpe"                  '特例置換項目-使用料無料特認
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "USEFREEKBN")
                    Case "TxtSprFitKbn"                      '特例置換項目-適合区分
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "FITKBN")
                    Case "TxtDelFlg"               '削除フラグ
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
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
            Case "TxtDelFlg"                   '削除フラグ
                CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
                TxtDelFlg.Focus()
            Case "TxtOrgCode"                  '組織コード
                CODENAME_get("ORG", TxtOrgCode.Text, LblOrgName.Text, WW_Dummy)
                TxtOrgCode.Focus()
            Case "TxtBigCTNCD"                 '大分類コード
                CODENAME_get("BIGCTNCD", TxtBigCTNCD.Text, LblBigCTNCDName.Text, WW_Dummy)
                TxtBigCTNCD.Focus()
            Case "TxtMiddleCTNCD"              '中分類コード
                CODENAME_get("MIDDLECTNCD", TxtMiddleCTNCD.Text, LblMiddleCTNCDName.Text, WW_Dummy)
                TxtMiddleCTNCD.Focus()
            Case "TxtSprUseFeeRateRound1"            '特例置換項目-使用料率端数整理1
                CODENAME_get("HASUU1", TxtSprUseFeeRateRound1.Text, LblSprUseFeeRateRound1Name.Text, WW_Dummy)
                TxtSprUseFeeRateRound1.Focus()
            Case "TxtSprUseFeeRateRound2"            '特例置換項目-使用料率端数整理2
                CODENAME_get("HASUU2", TxtSprUseFeeRateRound2.Text, LblSprUseFeeRateRound2Name.Text, WW_Dummy)
                TxtSprUseFeeRateRound2.Focus()
            Case "TxtSprUseFeeRateAddSubCond1"       '特例置換項目-使用料率加減額端数整理1
                CODENAME_get("HASUU1", TxtSprUseFeeRateAddSubCond1.Text, LblSprUseFeeRateAddSubCond1Name.Text, WW_Dummy)
                TxtSprUseFeeRateAddSubCond1.Focus()
            Case "TxtSprUseFeeRateAddSubCond2"       '特例置換項目-使用料率加減額端数整理2
                CODENAME_get("HASUU2", TxtSprUseFeeRateAddSubCond2.Text, LblSprUseFeeRateAddSubCond2Name.Text, WW_Dummy)
                TxtSprUseFeeRateAddSubCond2.Focus()
            Case "TxtSprRoundPointKbn"               '特例置換項目-端数処理時点区分
                CODENAME_get("HASUUPOINTKBN", TxtSprRoundPointKbn.Text, LblSprRoundPointKbnName.Text, WW_Dummy)
                TxtSprRoundPointKbn.Focus()
            Case "TxtSprUseFreeSpe"                  '特例置換項目-使用料無料特認
                CODENAME_get("USEFREEKBN", TxtSprUseFreeSpe.Text, LblSprUseFreeSpeName.Text, WW_Dummy)
                TxtSprUseFreeSpe.Focus()
            Case "TxtSprFitKbn"                      '特例置換項目-適合区分
                CODENAME_get("FITKBN", TxtSprFitKbn.Text, LblSprFitKbnName.Text, WW_Dummy)
                TxtSprFitKbn.Focus()
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
                Case "TxtOrgCode"                        '組織コード
                    TxtOrgCode.Text = WW_SelectValue
                    LblOrgName.Text = WW_SelectText
                    TxtOrgCode.Focus()
                Case "TxtBigCTNCD"                       '大分類コード
                    TxtBigCTNCD.Text = WW_SelectValue
                    LblBigCTNCDName.Text = WW_SelectText
                    TxtBigCTNCD.Focus()
                Case "TxtMiddleCTNCD"                    '中分類コード
                    TxtMiddleCTNCD.Text = WW_SelectValue
                    LblMiddleCTNCDName.Text = WW_SelectText
                    TxtMiddleCTNCD.Focus()
                Case "TxtSprUseFeeRateRound1"            '特例置換項目-使用料率端数整理1
                    TxtSprUseFeeRateRound1.Text = WW_SelectValue
                    LblSprUseFeeRateRound1Name.Text = WW_SelectText
                    TxtSprUseFeeRateRound1.Focus()
                Case "TxtSprUseFeeRateRound2"            '特例置換項目-使用料率端数整理2
                    TxtSprUseFeeRateRound2.Text = WW_SelectValue
                    LblSprUseFeeRateRound2Name.Text = WW_SelectText
                    TxtSprUseFeeRateRound2.Focus()
                Case "TxtSprUseFeeRateAddSubCond1"       '特例置換項目-使用料率加減額端数整理1
                    TxtSprUseFeeRateAddSubCond1.Text = WW_SelectValue
                    LblSprUseFeeRateAddSubCond1Name.Text = WW_SelectText
                    TxtSprUseFeeRateAddSubCond1.Focus()
                Case "TxtSprUseFeeRateAddSubCond2"       '特例置換項目-使用料率加減額端数整理2
                    TxtSprUseFeeRateAddSubCond2.Text = WW_SelectValue
                    LblSprUseFeeRateAddSubCond2Name.Text = WW_SelectText
                    TxtSprUseFeeRateAddSubCond2.Focus()
                Case "TxtSprRoundPointKbn"               '特例置換項目-端数処理時点区分
                    TxtSprRoundPointKbn.Text = WW_SelectValue
                    LblSprRoundPointKbnName.Text = WW_SelectText
                    TxtSprRoundPointKbn.Focus()
                Case "TxtSprUseFreeSpe"                  '特例置換項目-使用料無料特認
                    TxtSprUseFreeSpe.Text = WW_SelectValue
                    LblSprUseFreeSpeName.Text = WW_SelectText
                    TxtSprUseFreeSpe.Focus()
                Case "TxtSprFitKbn"                      '特例置換項目-適合区分
                    TxtSprFitKbn.Text = WW_SelectValue
                    LblSprFitKbnName.Text = WW_SelectText
                    TxtSprFitKbn.Focus()
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
                Case "TxtOrgCode"                        '組織コード
                    TxtOrgCode.Focus()
                Case "TxtBigCTNCD"                       '大分類コード
                    TxtBigCTNCD.Focus()
                Case "TxtMiddleCTNCD"                    '中分類コード
                    TxtMiddleCTNCD.Focus()
                Case "TxtSprUseFeeRateRound1"            '特例置換項目-使用料率端数整理1
                    TxtSprUseFeeRateRound1.Focus()
                Case "TxtSprUseFeeRateRound2"            '特例置換項目-使用料率端数整理2
                    TxtSprUseFeeRateRound2.Focus()
                Case "TxtSprUseFeeRateAddSubCond1"       '特例置換項目-使用料率加減額端数整理1
                    TxtSprUseFeeRateAddSubCond1.Focus()
                Case "TxtSprUseFeeRateAddSubCond2"       '特例置換項目-使用料率加減額端数整理2
                    TxtSprUseFeeRateAddSubCond2.Focus()
                Case "TxtSprRoundPointKbn"               '特例置換項目-端数処理時点区分
                    TxtSprRoundPointKbn.Focus()
                Case "TxtSprUseFreeSpe"                  '特例置換項目-使用料無料特認
                    TxtSprUseFreeSpe.Focus()
                Case "TxtSprFitKbn"                      '特例置換項目-適合区分
                    TxtSprFitKbn.Focus()
            End Select
        End If

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
            WW_CheckMES1 = "・使用料特例マスタ２更新権限なし"
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
            ' 組織コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ORG", LNM0017INProw("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("ORG", LNM0017INProw("ORGCODE"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・組織コード入力エラー"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・組織コード入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 大分類コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIGCTNCD", LNM0017INProw("BIGCTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("BIGCTNCD", LNM0017INProw("BIGCTNCD"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "MIDDLECTNCD", LNM0017INProw("MIDDLECTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("MIDDLECTNCD", LNM0017INProw("MIDDLECTNCD"), WW_Dummy, WW_RtnSW)
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
            ' 使用目的(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PURPOSE", LNM0017INProw("PURPOSE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・使用目的エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-使用料金額(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRUSEFEE", LNM0017INProw("SPRUSEFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-使用料金額エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-使用料率(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRUSEFEERATE", LNM0017INProw("SPRUSEFEERATE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-使用料率エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 入力値チェック(使用料率)
            If String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATE")) OrElse
                LNM0017INProw("SPRUSEFEERATE") = "0" Then
                If String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEE")) OrElse
                    LNM0017INProw("SPRUSEFEE") = "0" Then
                    ' 入力値チェック(使用料金額&使用料率)
                    WW_CheckMES1 = "・特例置換項目-使用料金額・使用料率入力エラーです。"
                    WW_CheckMES2 = "どちらかを入力してください。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                ElseIf Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUB")) AndAlso
                    LNM0017INProw("SPRUSEFEERATEADDSUB") <> "0" Then
                    ' 入力値チェック(使用料率&使用料率加減額)
                    WW_CheckMES1 = "・特例置換項目-使用料率・使用料率加減額入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-使用料率が未入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                ElseIf Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUBCOND1")) OrElse
                    Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUBCOND2")) Then
                    ' 入力値チェック(使用料率&使用料率加減額端数整理)
                    WW_CheckMES1 = "・特例置換項目-使用料率・使用料率加減額端数整理入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-使用料率が未入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-使用料率端数整理(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRUSEFEERATEROUND", LNM0017INProw("SPRUSEFEERATEROUND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEROUND1")) AndAlso
                    Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEROUND2")) Then
                    ' 名称存在チェック
                    CODENAME_get("HASUU1", LNM0017INProw("SPRUSEFEERATEROUND1"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-使用料率端数整理１エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                    ' 名称存在チェック
                    CODENAME_get("HASUU2", LNM0017INProw("SPRUSEFEERATEROUND2"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-使用料率端数整理２エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                ElseIf Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEROUND1")) AndAlso String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEROUND2")) OrElse
                     String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEROUND1")) AndAlso Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEROUND2")) Then
                    WW_CheckMES1 = "・特例置換項目-使用料率端数整理１・使用料率端数整理２エラーです。"
                    WW_CheckMES2 = "両方に入力してください。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-使用料率端数整理エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 入力値チェック(使用料率&使用料率端数整理)
            If Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATE")) AndAlso
                LNM0017INProw("SPRUSEFEERATE") <> 0 AndAlso
                String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEROUND1")) AndAlso
                String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEROUND2")) Then
                WW_CheckMES1 = "・特例置換項目-使用料率&使用料率端数整理入力エラーです。"
                WW_CheckMES2 = "特例置換項目-使用料率端数整理が未入力です。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-使用料率加減額(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRUSEFEERATEADDSUB", LNM0017INProw("SPRUSEFEERATEADDSUB"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-使用料率加減額エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-使用料率加減額端数整理(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRUSEFEERATEADDSUBCOND", LNM0017INProw("SPRUSEFEERATEADDSUBCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUBCOND1")) AndAlso
                    Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUBCOND2")) Then
                    ' 名称存在チェック
                    CODENAME_get("HASUU1", LNM0017INProw("SPRUSEFEERATEADDSUBCOND1"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-使用料率加減額端数整理１エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                    ' 名称存在チェック
                    CODENAME_get("HASUU2", LNM0017INProw("SPRUSEFEERATEADDSUBCOND2"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-使用料率加減額端数整理２エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                ElseIf String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUBCOND1")) AndAlso Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUBCOND")) OrElse
                     Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUBCOND1")) AndAlso String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUBCOND")) Then
                    WW_CheckMES1 = "・特例置換項目-使用料率加減額端数整理１・使用料率加減額端数整理２エラーです。"
                    WW_CheckMES2 = "両方に入力してください。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-使用料率加減額端数整理エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 入力値チェック(使用料率加減額&使用料率加減額端数整理)
            If String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUB")) OrElse
                LNM0017INProw("SPRUSEFEERATEADDSUB") = "0" Then
                If Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUBCOND1")) AndAlso
                    Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUBCOND2")) Then
                    WW_CheckMES1 = "・特例置換項目-使用料率加減額・使用料率加減額端数整理入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-使用料率加減額が未入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            ElseIf Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUB")) AndAlso
                LNM0017INProw("SPRUSEFEERATEADDSUB") <> 0 Then
                If String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUBCOND1")) AndAlso
                    String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUBCOND2")) Then
                    WW_CheckMES1 = "・特例置換項目-使用料率加減額・使用料率加減額端数整理入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-使用料率加減額端数整理が未入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-端数処理時点区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRROUNDPOINTKBN", LNM0017INProw("SPRROUNDPOINTKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0017INProw("SPRROUNDPOINTKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("HASUUPOINTKBN", LNM0017INProw("SPRROUNDPOINTKBN"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-端数処理時点区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-端数処理時点区分です。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 入力値チェック(使用料率&端数処理時点区分) else (使用料率加減額&端数処理時点区分)
            If String.IsNullOrEmpty(LNM0017INProw("SPRROUNDPOINTKBN")) Then
                If Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATE")) OrElse
                    LNM0017INProw("SPRUSEFEERATE") = "0" Then
                    WW_CheckMES1 = "・特例置換項目-使用料率・端数処理時点区分入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-端数処理時点区分が未入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                ElseIf Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFEERATEADDSUB")) OrElse
                   LNM0017INProw("SPRUSEFEERATEADDSUB") = "0" Then
                    WW_CheckMES1 = "・特例置換項目-使用料率加減額・端数処理時点区分入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-使用料率加減額が未入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-使用料無料特認(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRUSEFREESPE", LNM0017INProw("SPRUSEFREESPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0017INProw("SPRUSEFREESPE")) Then
                    ' 名称存在チェック
                    CODENAME_get("USEFREEKBN", LNM0017INProw("SPRUSEFREESPE"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-使用料無料特認エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-使用料無料特認エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-通運負担回送運賃(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRNITTSUFREESENDFEE", LNM0017INProw("SPRNITTSUFREESENDFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-通運負担回送運賃エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-運行管理料(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRMANAGEFEE", LNM0017INProw("SPRMANAGEFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-運行管理料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-荷主負担運賃(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRSHIPBURDENFEE", LNM0017INProw("SPRSHIPBURDENFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-荷主負担運賃エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-発送料(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRSHIPFEE", LNM0017INProw("SPRSHIPFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-発送料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-到着料(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRARRIVEFEE", LNM0017INProw("SPRARRIVEFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-到着料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-集荷料(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRPICKUPFEE", LNM0017INProw("SPRPICKUPFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-集荷料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-配達料(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRDELIVERYFEE", LNM0017INProw("SPRDELIVERYFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-配達料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-その他１(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPROTHER1", LNM0017INProw("SPROTHER1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-その他１エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-その他２(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPROTHER2", LNM0017INProw("SPROTHER2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-その他２エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-適合区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRFITKBN", LNM0017INProw("SPRFITKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0017INProw("SPRFITKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("FITKBN", LNM0017INProw("SPRFITKBN"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-適合区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-使用料無料特認エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 入力値チェック(大分類コード&特例置換項目-適合区分)
            If LNM0017INProw("BIGCTNCD") = "10" AndAlso
                LNM0017INProw("SPRFITKBN") <> "0" AndAlso
                LNM0017INProw("SPRFITKBN") <> "1" AndAlso
                LNM0017INProw("SPRFITKBN") <> "2" OrElse
                LNM0017INProw("BIGCTNCD") <> "10" AndAlso
                LNM0017INProw("SPRFITKBN") <> "0" Then
                WW_CheckMES1 = "・大分類コード・特例置換項目-適合区分入力エラーです。"
                WW_CheckMES2 = "特例置換項目-適合区分が不適切です。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_ORG2.Text) Then  '組織コード
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                    TxtOrgCode.Text, TxtBigCTNCD.Text,
                                    TxtMiddleCTNCD.Text, work.WF_SEL_TIMESTAMP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（組織コード&大分類コード&中分類コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                       "([" & LNM0017INProw("ORGCODE") & "]" &
                                       "([" & LNM0017INProw("BIGCTNCD") & "]" &
                                       "([" & LNM0017INProw("MIDDLECTNCD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNM0017INProw("ORGCODE") = work.WF_SEL_ORG2.Text OrElse            '組織コード
               Not LNM0017INProw("BIGCTNCD") = work.WF_SEL_BIGCTNCD2.Text OrElse      '大分類コード
               Not LNM0017INProw("MIDDLECTNCD") = work.WF_SEL_MIDDLECTNCD2.Text Then  '中分類コード
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（組織コード&大分類コード&中分類コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & LNM0017INProw("ORGCODE") & "]" &
                                       "([" & LNM0017INProw("BIGCTNCD") & "]" &
                                       "([" & LNM0017INProw("MIDDLECTNCD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LineErr = "" Then
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
                If LNM0017row("ORGCODE") = LNM0017INProw("ORGCODE") AndAlso                                       '組織コード
                   LNM0017row("BIGCTNCD") = LNM0017INProw("BIGCTNCD") AndAlso                                     '大分類コード
                   LNM0017row("MIDDLECTNCD") = LNM0017INProw("MIDDLECTNCD") Then                                  '中分類コード
                    ' KEY項目以外の項目の差異をチェック                                                           
                    If LNM0017row("DELFLG") = LNM0017INProw("DELFLG") AndAlso                                     '削除フラグ
                       LNM0017row("PURPOSE") = LNM0017INProw("PURPOSE") AndAlso                                   '使用目的
                        LNM0017row("SPRUSEFEE") = LNM0017INProw("SPRUSEFEE") AndAlso                              '特例置換項目-使用料金額
                        LNM0017row("SPRUSEFEERATE") = LNM0017INProw("SPRUSEFEERATE") AndAlso                      '特例置換項目-使用料率
                        LNM0017row("SPRUSEFEERATEROUND") = LNM0017INProw("SPRUSEFEERATEROUND") AndAlso            '特例置換項目-使用料率端数整理
                        LNM0017row("SPRUSEFEERATEADDSUB") = LNM0017INProw("SPRUSEFEERATEADDSUB") AndAlso          '特例置換項目-使用料率加減額
                        LNM0017row("SPRUSEFEERATEADDSUBCOND") = LNM0017INProw("SPRUSEFEERATEADDSUBCOND") AndAlso  '特例置換項目-使用料率加減額端数整理
                        LNM0017row("SPRROUNDPOINTKBN") = LNM0017INProw("SPRROUNDPOINTKBN") AndAlso                '特例置換項目-端数処理時点区分
                        LNM0017row("SPRUSEFREESPE") = LNM0017INProw("SPRUSEFREESPE") AndAlso                      '特例置換項目-使用料無料特認
                        LNM0017row("SPRNITTSUFREESENDFEE") = LNM0017INProw("SPRNITTSUFREESENDFEE") AndAlso        '特例置換項目-通運負担回送運賃
                        LNM0017row("SPRMANAGEFEE") = LNM0017INProw("SPRMANAGEFEE") AndAlso                        '特例置換項目-運行管理料
                        LNM0017row("SPRSHIPBURDENFEE") = LNM0017INProw("SPRSHIPBURDENFEE") AndAlso                '特例置換項目-荷主負担運賃
                        LNM0017row("SPRSHIPFEE") = LNM0017INProw("SPRSHIPFEE") AndAlso                            '特例置換項目-発送料
                        LNM0017row("SPRARRIVEFEE") = LNM0017INProw("SPRARRIVEFEE") AndAlso                        '特例置換項目-到着料
                        LNM0017row("SPRPICKUPFEE") = LNM0017INProw("SPRPICKUPFEE") AndAlso                        '特例置換項目-集荷料
                        LNM0017row("SPRDELIVERYFEE") = LNM0017INProw("SPRDELIVERYFEE") AndAlso                    '特例置換項目-配達料
                        LNM0017row("SPROTHER1") = LNM0017INProw("SPROTHER1") AndAlso                              '特例置換項目-その他１
                        LNM0017row("SPROTHER2") = LNM0017INProw("SPROTHER2") AndAlso                              '特例置換項目-その他２
                        LNM0017row("SPRFITKBN") = LNM0017INProw("SPRFITKBN") AndAlso                              '特例置換項目-適合区分
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

                '変更チェック
                REST2MEXISTS(SQLcon, WW_MODIFYKBN)
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
        For Each LNM0017INProw As DataRow In LNM0017INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0017row As DataRow In LNM0017tbl.Rows
                ' 同一レコードか判定
                If LNM0017INProw("ORGCODE") = LNM0017row("ORGCODE") AndAlso       '組織コード
                   LNM0017INProw("BIGCTNCD") = LNM0017row("BIGCTNCD") AndAlso     '大分類コード
                   LNM0017INProw("MIDDLECTNCD") = LNM0017row("MIDDLECTNCD") Then  '中分類コード
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
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrEmpty(I_VALUE) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If
        Dim WW_PrmData As New Hashtable

        Try
            Select Case I_FIELD
                Case "ORG"                '組織コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "BIGCTNCD"           '大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS))
                Case "MIDDLECTNCD"        '中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, TxtBigCTNCD.Text))
                Case "HASUU1",            '端数区分１
                     "HASUU2",            '端数区分２
                     "HASUUPOINTKBN",     '端数時点区分
                     "USEFREEKBN",        '使用料無料区分
                     "FITKBN"             '適合区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))

                Case "OUTPUTID"           '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))
                Case "ONOFF"              '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))
                Case "DELFLG"             '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
