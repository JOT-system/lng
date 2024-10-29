''************************************************************
' コード変換特例１マスタメンテ登録画面
' 作成日 2022/02/14
' 更新日 2023/12/21
' 作成者 名取
' 更新者 大浜
'
' 修正履歴 : 2022/02/14 新規作成
'          : 2023/12/21 変更履歴登録機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' コード変換特例１マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0007Rect1mDetail
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private LNM0007tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0007INPtbl As DataTable                              'チェック用テーブル
    Private LNM0007UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0007tbl, work.WF_SEL_INPTBL.Text)

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
                        Case "mspStationSingleRowSelected"  '[共通]駅選択ポップアップで行選択
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
            If Not IsNothing(LNM0007tbl) Then
                LNM0007tbl.Clear()
                LNM0007tbl.Dispose()
                LNM0007tbl = Nothing
            End If

            If Not IsNothing(LNM0007INPtbl) Then
                LNM0007INPtbl.Clear()
                LNM0007INPtbl.Dispose()
                LNM0007INPtbl = Nothing
            End If

            If Not IsNothing(LNM0007UPDtbl) Then
                LNM0007UPDtbl.Clear()
                LNM0007UPDtbl.Dispose()
                LNM0007UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0007WRKINC.MAPIDD
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0007L Then
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
        '発駅コード
        TxtDepStation.Text = work.WF_SEL_DEPSTATION2.Text
        CODENAME_get("STATION", TxtDepStation.Text, LblDepStationName.Text, WW_Dummy)
        '発受託人コード
        TxtDepTrusteeCd.Text = work.WF_SEL_DEPTRUSTEECD2.Text
        CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCd.Text, LblDepTrusteeCdName.Text, WW_Dummy)
        '優先順位
        TxtPriorityNo.Text = work.WF_SEL_PRIORITYNO.Text
        '使用目的
        TxtPurpose.Text = work.WF_SEL_PURPOSE.Text
        '選択比較項目-小分類コード
        TxtSmallCTNCD.Text = work.WF_SEL_SMALLCTNCD.Text
        CODENAME_get("SMALLCTNCD", TxtSmallCTNCD.Text, LblSmallCTNCDName.Text, WW_Dummy)
        '選択比較項目-コンテナ記号
        TxtCTNType.Text = work.WF_SEL_CTNTYPE.Text
        CODENAME_get("CTNTYPE", TxtCTNType.Text, LblCTNTypeName.Text, WW_Dummy)
        '選択比較項目-コンテナ番号（開始）
        TxtCTNStNo.Text = work.WF_SEL_CTNSTNO.Text
        '選択比較項目-コンテナ番号（終了）
        TxtCTNEndNo.Text = work.WF_SEL_CTNENDNO.Text
        '選択比較項目-積空区分
        TxtSlcStackFreeKbn.Text = work.WF_SEL_SLCSTACKFREEKBN.Text
        CODENAME_get("STACKFREEKBN", TxtSlcStackFreeKbn.Text, LblSlcStackFreeKbnName.Text, WW_Dummy)
        '選択比較項目-状態区分
        TxtSlcStatusKbn.Text = work.WF_SEL_SLCSTATUSKBN.Text
        CODENAME_get("OPERATIONKBN", TxtSlcStatusKbn.Text, LblSlcStatusKbnName.Text, WW_Dummy)
        '選択比較項目-発受託人サブコード
        TxtSlcDepTrusteeSubCd.Text = work.WF_SEL_SLCDEPTRUSTEESUBCD.Text
        CODENAME_get("DEPTRUSTEESUBCD", TxtSlcDepTrusteeSubCd.Text, LblSlcDepTrusteeSubCdName.Text, WW_Dummy)
        '選択比較項目-発荷主コード
        TxtSlcDepShipperCd.Text = work.WF_SEL_SLCDEPSHIPPERCD.Text
        CODENAME_get("SHIPPER", TxtSlcDepShipperCd.Text, LblSlcDepShipperCdName.Text, WW_Dummy)
        '選択比較項目-着駅コード
        TxtSlcArrStation.Text = work.WF_SEL_SLCARRSTATION.Text
        CODENAME_get("STATION", TxtSlcArrStation.Text, LblSlcArrStationName.Text, WW_Dummy)
        '選択比較項目-着受託人コード
        TxtSlcArrTrusteeCd.Text = work.WF_SEL_SLCARRTRUSTEECD.Text
        CODENAME_get("ARRTRUSTEECD", TxtSlcArrTrusteeCd.Text, LblSlcArrTrusteeCdName.Text, WW_Dummy)
        '選択比較項目-着受託人サブコード
        TxtSlcArrTrusteeSubCd.Text = work.WF_SEL_SLCARRTRUSTEESUBCD.Text
        CODENAME_get("ARRTRUSTEESUBCD", TxtSlcArrTrusteeSubCd.Text, LblSlcArrTrusteeSubCdName.Text, WW_Dummy)
        '選択比較項目-ＪＲ品目コード
        TxtSlcJRItemCd.Text = work.WF_SEL_SLCJRITEMCD.Text
        CODENAME_get("ITEM", TxtSlcJRItemCd.Text, LblSlcJRItemCdName.Text, WW_Dummy)
        '選択比較項目-集荷先電話番号
        TxtSlcPickUpTel.Text = work.WF_SEL_SLCPICKUPTEL.Text
        '特例置換項目-発受託人コード
        TxtSprDepTrusteeCd.Text = work.WF_SEL_SPRDEPTRUSTEECD.Text
        CODENAME_get("DEPTRUSTEECD", TxtSprDepTrusteeCd.Text, LblSprDepTrusteeCdName.Text, WW_Dummy)
        '特例置換項目-発受託人サブコード
        TxtSprDepTrusteeSubCd.Text = work.WF_SEL_SPRDEPTRUSTEESUBCD.Text
        CODENAME_get("DEPTRUSTEESUBCD", TxtSprDepTrusteeSubCd.Text, LblSprDepTrusteeSubCdName.Text, WW_Dummy)
        '特例置換項目-発受託人サブゼロ変換区分
        TxtSprDerTrusteeSubZKbn.Text = work.WF_SEL_SPRDEPTRUSTEESUBZKBN.Text
        '特例置換項目-発荷主コード
        TxtSprDepShipperCd.Text = work.WF_SEL_SPRDEPSHIPPERCD.Text
        CODENAME_get("SHIPPER", TxtSprDepShipperCd.Text, LblSprDepShipperCdName.Text, WW_Dummy)
        '選択比較項目-着受託人コード
        TxtSprArrTrusteeCd.Text = work.WF_SEL_SPRARRTRUSTEECD.Text
        CODENAME_get("ARRTRUSTEECD", TxtSprArrTrusteeCd.Text, LblSprArrTrusteeCdName.Text, WW_Dummy)
        '選択比較項目-着受託人サブコード
        TxtSprArrTrusteeSubCd.Text = work.WF_SEL_SPRARRTRUSTEESUBCD.Text
        CODENAME_get("ARRTRUSTEESUBCD", TxtSprArrTrusteeSubCd.Text, LblSprArrTrusteeSubCdName.Text, WW_Dummy)
        '特例置換項目-発受託人サブゼロ変換区分
        TxtSprArrTrusteeSubZKbn.Text = work.WF_SEL_SPRARRTRUSTEESUBZKBN.Text
        '選択比較項目-ＪＲ品目コード
        TxtSprJRItemCd.Text = work.WF_SEL_SPRJRITEMCD.Text
        CODENAME_get("ITEM", TxtSprJRItemCd.Text, LblSprJRItemCdName.Text, WW_Dummy)
        '特例置換項目-積空区分
        TxtSprStackFreeKbn.Text = work.WF_SEL_SPRSTACKFREEKBN.Text
        CODENAME_get("STACKFREEKBN", TxtSprStackFreeKbn.Text, LblSprStackFreeKbnName.Text, WW_Dummy)
        '特例置換項目-状態区分
        TxtSprStatusKbn.Text = work.WF_SEL_SPRSTATUSKBN.Text
        CODENAME_get("OPERATIONKBN", TxtSprStatusKbn.Text, LblSprStatusKbnName.Text, WW_Dummy)
        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_ORG2.Text

        ' 数値(0～9)のみ入力可能とする。
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"                   '削除フラグ
        Me.TxtOrgCode.Attributes("onkeyPress") = "CheckNum()"                  '組織コード
        Me.TxtBigCTNCD.Attributes("onkeyPress") = "CheckNum()"                 '大分類コード
        Me.TxtMiddleCTNCD.Attributes("onkeyPress") = "CheckNum()"              '中分類コード
        Me.TxtDepStation.Attributes("onkeyPress") = "CheckNum()"               '発駅コード
        Me.TxtDepTrusteeCd.Attributes("onkeyPress") = "CheckNum()"             '発受託人コード
        Me.TxtPriorityNo.Attributes("onkeyPress") = "CheckNum()"               '優先順位
        Me.TxtSmallCTNCD.Attributes("onkeyPress") = "CheckNum()"               '選択比較項目-小分類コード
        Me.TxtCTNStNo.Attributes("onkeyPress") = "CheckNum()"                  '選択比較項目-コンテナ番号（開始）
        Me.TxtCTNEndNo.Attributes("onkeyPress") = "CheckNum()"                 '選択比較項目-コンテナ番号（終了）
        Me.TxtSlcStackFreeKbn.Attributes("onkeyPress") = "CheckNum()"          '選択比較項目-積空区分
        Me.TxtSlcStatusKbn.Attributes("onkeyPress") = "CheckNum()"             '選択比較項目-状態区分
        Me.TxtSlcDepTrusteeSubCd.Attributes("onkeyPress") = "CheckNum()"       '選択比較項目-発受託人サブコード
        Me.TxtSlcDepShipperCd.Attributes("onkeyPress") = "CheckNum()"          '選択比較項目-発荷主コード
        Me.TxtSlcArrStation.Attributes("onkeyPress") = "CheckNum()"            '選択比較項目-着駅コード
        Me.TxtSlcArrTrusteeCd.Attributes("onkeyPress") = "CheckNum()"          '選択比較項目-着受託人コード
        Me.TxtSlcArrTrusteeSubCd.Attributes("onkeyPress") = "CheckNum()"       '選択比較項目-着受託人サブコード
        Me.TxtSlcJRItemCd.Attributes("onkeyPress") = "CheckNum()"              '選択比較項目-ＪＲ品目コード
        Me.TxtSprDepTrusteeCd.Attributes("onkeyPress") = "CheckNum()"          '特例置換項目-発受託人コード
        Me.TxtSprDepTrusteeSubCd.Attributes("onkeyPress") = "CheckNum()"       '特例置換項目-発受託人サブコード
        Me.TxtSprDerTrusteeSubZKbn.Attributes("onkeyPress") = "CheckNum()"     '特例置換項目-発受託人サブゼロ変換区分
        Me.TxtSprDepShipperCd.Attributes("onkeyPress") = "CheckNum()"          '特例置換項目-発荷主コード
        Me.TxtSprArrTrusteeCd.Attributes("onkeyPress") = "CheckNum()"          '特例置換項目-着受託人コード
        Me.TxtSprArrTrusteeSubCd.Attributes("onkeyPress") = "CheckNum()"       '特例置換項目-着受託人サブコード
        Me.TxtSprArrTrusteeSubZKbn.Attributes("onkeyPress") = "CheckNum()"     '特例置換項目-着受託人サブゼロ変換区分
        Me.TxtSprJRItemCd.Attributes("onkeyPress") = "CheckNum()"              '特例置換項目-ＪＲ品目コード
        Me.TxtSprStackFreeKbn.Attributes("onkeyPress") = "CheckNum()"          '特例置換項目-積空区分
        Me.TxtSprStatusKbn.Attributes("onkeyPress") = "CheckNum()"             '特例置換項目-状態区分

        ' 入力するテキストボックスは数値(0～9)＋英字のみ可能とする。
        Me.TxtCTNType.Attributes("onkeyPress") = "CheckNumAZ()"                '選択比較項目-コンテナ記号

        ' 数値(0～9)とハイフン(-)のみ入力可能とする。
        Me.TxtSlcPickUpTel.Attributes("onkeyPress") = "CheckTel()"             '選択比較項目-集荷先電話番号


    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO"></param>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                      " _
            & "     ORGCODE                 " _
            & "   , BIGCTNCD                " _
            & "   , MIDDLECTNCD             " _
            & "   , DEPSTATION              " _
            & "   , DEPTRUSTEECD            " _
            & "   , PRIORITYNO              " _
            & " FROM                        " _
            & "     LNG.LNM0007_RECT1M      " _
            & " WHERE                       " _
            & "         ORGCODE       = @P1 " _
            & "     AND BIGCTNCD      = @P2 " _
            & "     AND MIDDLECTNCD   = @P3 " _
            & "     AND DEPSTATION    = @P4 " _
            & "     AND DEPTRUSTEECD  = @P5 " _
            & "     AND PRIORITYNO    = @P6 " _
            & "     AND DELFLG       <> @P7 "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 6) '組織コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 2) '大分類コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 2) '中分類コード
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 6) '発駅コード
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.VarChar, 5) '発受託人コード
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@P6", MySqlDbType.VarChar, 5) '優先順位
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@P7", MySqlDbType.VarChar, 1) '削除フラグ

                PARA1.Value = TxtOrgCode.Text       '組織コード
                PARA2.Value = TxtBigCTNCD.Text      '大分類コード
                PARA3.Value = TxtMiddleCTNCD.Text   '中分類コード
                PARA4.Value = TxtDepStation.Text    '発駅コード
                PARA5.Value = TxtDepTrusteeCd.Text  '発受託人コード
                PARA6.Value = TxtPriorityNo.Text    '優先順位
                PARA7.Value = C_DELETE_FLG.DELETE   '削除フラグ

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0007Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0007Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0007Chk.Load(SQLdr)

                    If LNM0007Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' コード変換特例マスタ１登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(コード変換特例マスタ１)
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;                 " _
            & "     SET @hensuu = 0 ;                       " _
            & " DECLARE hensuu CURSOR FOR                   " _
            & "     SELECT                                  " _
            & "         UPDTIMSTP AS hensuu                 " _
            & "     FROM                                    " _
            & "         LNG.LNM0007_RECT1M                  " _
            & "     WHERE                                   " _
            & "         ORGCODE      = @P01                 " _
            & "     AND BIGCTNCD     = @P02                 " _
            & "     AND MIDDLECTNCD  = @P03                 " _
            & "     AND DEPSTATION   = @P04                 " _
            & "     AND DEPTRUSTEECD = @P05                 " _
            & "     AND PRIORITYNO   = @P06 ;               " _
            & " OPEN hensuu ;                               " _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;       " _
            & " IF (@@FETCH_STATUS = 0)                     " _
            & "     UPDATE LNG.LNM0007_RECT1M               " _
            & "     SET                                     " _
            & "         DELFLG               = @P00         " _
            & "       , PURPOSE              = @P07         " _
            & "       , SMALLCTNCD           = @P08         " _
            & "       , CTNTYPE              = @P09         " _
            & "       , CTNSTNO              = @P10         " _
            & "       , CTNENDNO             = @P11         " _
            & "       , SLCSTACKFREEKBN      = @P12         " _
            & "       , SLCSTATUSKBN         = @P13         " _
            & "       , SLCDEPTRUSTEESUBCD   = @P14         " _
            & "       , SLCDEPSHIPPERCD      = @P15         " _
            & "       , SLCARRSTATION        = @P16         " _
            & "       , SLCARRTRUSTEECD      = @P17         " _
            & "       , SLCARRTRUSTEESUBCD   = @P18         " _
            & "       , SLCJRITEMCD          = @P19         " _
            & "       , SLCPICKUPTEL         = @P20         " _
            & "       , SPRDEPTRUSTEECD      = @P21         " _
            & "       , SPRDEPTRUSTEESUBCD   = @P22         " _
            & "       , SPRDEPTRUSTEESUBZKBN = @P23         " _
            & "       , SPRDEPSHIPPERCD      = @P24         " _
            & "       , SPRARRTRUSTEECD      = @P25         " _
            & "       , SPRARRTRUSTEESUBCD   = @P26         " _
            & "       , SPRARRTRUSTEESUBZKBN = @P27         " _
            & "       , SPRJRITEMCD          = @P28         " _
            & "       , SPRSTACKFREEKBN      = @P29         " _
            & "       , SPRSTATUSKBN         = @P30         " _
            & "       , UPDYMD               = @P36         " _
            & "       , UPDUSER              = @P37         " _
            & "       , UPDTERMID            = @P38         " _
            & "       , UPDPGID              = @P39         " _
            & "     WHERE                                   " _
            & "         ORGCODE      = @P01                 " _
            & "     AND BIGCTNCD     = @P02                 " _
            & "     AND MIDDLECTNCD  = @P03                 " _
            & "     AND DEPSTATION   = @P04                 " _
            & "     AND DEPTRUSTEECD = @P05                 " _
            & "     AND PRIORITYNO   = @P06 ;               " _
            & " IF (@@FETCH_STATUS <> 0)                    " _
            & "     INSERT INTO LNG.LNM0007_RECT1M          " _
            & "        (DELFLG                              " _
            & "       , ORGCODE                             " _
            & "       , BIGCTNCD                            " _
            & "       , MIDDLECTNCD                         " _
            & "       , DEPSTATION                          " _
            & "       , DEPTRUSTEECD                        " _
            & "       , PRIORITYNO                          " _
            & "       , PURPOSE                             " _
            & "       , SMALLCTNCD                          " _
            & "       , CTNTYPE                             " _
            & "       , CTNSTNO                             " _
            & "       , CTNENDNO                            " _
            & "       , SLCSTACKFREEKBN                     " _
            & "       , SLCSTATUSKBN                        " _
            & "       , SLCDEPTRUSTEESUBCD                  " _
            & "       , SLCDEPSHIPPERCD                     " _
            & "       , SLCARRSTATION                       " _
            & "       , SLCARRTRUSTEECD                     " _
            & "       , SLCARRTRUSTEESUBCD                  " _
            & "       , SLCJRITEMCD                         " _
            & "       , SLCPICKUPTEL                        " _
            & "       , SPRDEPTRUSTEECD                     " _
            & "       , SPRDEPTRUSTEESUBCD                  " _
            & "       , SPRDEPTRUSTEESUBZKBN                " _
            & "       , SPRDEPSHIPPERCD                     " _
            & "       , SPRARRTRUSTEECD                     " _
            & "       , SPRARRTRUSTEESUBCD                  " _
            & "       , SPRARRTRUSTEESUBZKBN                " _
            & "       , SPRJRITEMCD                         " _
            & "       , SPRSTACKFREEKBN                     " _
            & "       , SPRSTATUSKBN                        " _
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
            & "       , @P24                                " _
            & "       , @P25                                " _
            & "       , @P26                                " _
            & "       , @P27                                " _
            & "       , @P28                                " _
            & "       , @P29                                " _
            & "       , @P30                                " _
            & "       , @P32                                " _
            & "       , @P33                                " _
            & "       , @P34                                " _
            & "       , @P35) ;                             " _
            & " CLOSE hensuu ;                              " _
            & " DEALLOCATE hensuu ;                         "

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                     " _
            & "    DELFLG                                  " _
            & "  , ORGCODE                                 " _
            & "  , BIGCTNCD                                " _
            & "  , MIDDLECTNCD                             " _
            & "  , DEPSTATION                              " _
            & "  , DEPTRUSTEECD                            " _
            & "  , PRIORITYNO                              " _
            & "  , PURPOSE                                 " _
            & "  , SMALLCTNCD                              " _
            & "  , CTNTYPE                                 " _
            & "  , CTNSTNO                                 " _
            & "  , CTNENDNO                                " _
            & "  , SLCSTACKFREEKBN                         " _
            & "  , SLCSTATUSKBN                            " _
            & "  , SLCDEPTRUSTEESUBCD                      " _
            & "  , SLCDEPSHIPPERCD                         " _
            & "  , SLCARRSTATION                           " _
            & "  , SLCARRTRUSTEECD                         " _
            & "  , SLCARRTRUSTEESUBCD                      " _
            & "  , SLCJRITEMCD                             " _
            & "  , SLCPICKUPTEL                            " _
            & "  , SPRDEPTRUSTEECD                         " _
            & "  , SPRDEPTRUSTEESUBCD                      " _
            & "  , SPRDEPTRUSTEESUBZKBN                    " _
            & "  , SPRDEPSHIPPERCD                         " _
            & "  , SPRARRTRUSTEECD                         " _
            & "  , SPRARRTRUSTEESUBCD                      " _
            & "  , SPRARRTRUSTEESUBZKBN                    " _
            & "  , SPRJRITEMCD                             " _
            & "  , SPRSTACKFREEKBN                         " _
            & "  , SPRSTATUSKBN                            " _
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
            & "    LNG.LNM0007_RECT1M                      " _
            & " WHERE                                      " _
            & "        ORGCODE      = @P01                 " _
            & "    AND BIGCTNCD     = @P02                 " _
            & "    AND MIDDLECTNCD  = @P03                 " _
            & "    AND DEPSTATION   = @P04                 " _
            & "    AND DEPTRUSTEECD = @P05                 " _
            & "    AND PRIORITYNO   = @P06                 "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                ' DB更新用パラメータ
                Dim PARA00 As MySqlParameter = SQLcmd.Parameters.Add("@P00", MySqlDbType.VarChar, 1)     '削除フラグ
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 6)     '組織コード
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 2)     '大分類コード
                Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 2)     '中分類コード
                Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar, 6)     '発駅コード
                Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar, 5)     '発受託人コード
                Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar, 5)     '優先順位
                Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar, 42)    '使用目的
                Dim PARA08 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.VarChar, 2)     '選択比較項目-小分類コード
                Dim PARA09 As MySqlParameter = SQLcmd.Parameters.Add("@P09", MySqlDbType.VarChar, 5)     '選択比較項目-コンテナ記号
                Dim PARA10 As MySqlParameter = SQLcmd.Parameters.Add("@P10", MySqlDbType.VarChar, 8)     '選択比較項目-コンテナ番号（開始）
                Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.VarChar, 8)     '選択比較項目-コンテナ番号（終了）
                Dim PARA12 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.VarChar, 1)     '選択比較項目-積空区分
                Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar, 2)     '選択比較項目-状態区分
                Dim PARA14 As MySqlParameter = SQLcmd.Parameters.Add("@P14", MySqlDbType.VarChar, 3)     '選択比較項目-発受託人サブコード
                Dim PARA15 As MySqlParameter = SQLcmd.Parameters.Add("@P15", MySqlDbType.VarChar, 6)     '選択比較項目-発荷主コード
                Dim PARA16 As MySqlParameter = SQLcmd.Parameters.Add("@P16", MySqlDbType.VarChar, 6)     '選択比較項目-着駅コード
                Dim PARA17 As MySqlParameter = SQLcmd.Parameters.Add("@P17", MySqlDbType.VarChar, 5)     '選択比較項目-着受託人コード
                Dim PARA18 As MySqlParameter = SQLcmd.Parameters.Add("@P18", MySqlDbType.VarChar, 3)     '選択比較項目-着受託人サブコード
                Dim PARA19 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ品目コード
                Dim PARA20 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.VarChar, 12)    '選択比較項目-集荷先電話番号
                Dim PARA21 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.VarChar, 5)     '特例置換項目-発受託人コード
                Dim PARA22 As MySqlParameter = SQLcmd.Parameters.Add("@P22", MySqlDbType.VarChar, 3)     '特例置換項目-発受託人サブコード
                Dim PARA23 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.VarChar, 1)     '特例置換項目-発受託人サブゼロ変換区分
                Dim PARA24 As MySqlParameter = SQLcmd.Parameters.Add("@P24", MySqlDbType.VarChar, 6)     '特例置換項目-発荷主コード
                Dim PARA25 As MySqlParameter = SQLcmd.Parameters.Add("@P25", MySqlDbType.VarChar, 5)     '特例置換項目-着受託人コード
                Dim PARA26 As MySqlParameter = SQLcmd.Parameters.Add("@P26", MySqlDbType.VarChar, 3)     '特例置換項目-着受託人サブコード
                Dim PARA27 As MySqlParameter = SQLcmd.Parameters.Add("@P27", MySqlDbType.VarChar, 1)     '特例置換項目-着受託人サブゼロ変換区分
                Dim PARA28 As MySqlParameter = SQLcmd.Parameters.Add("@P28", MySqlDbType.VarChar, 6)     '特例置換項目-ＪＲ品目コード
                Dim PARA29 As MySqlParameter = SQLcmd.Parameters.Add("@P29", MySqlDbType.VarChar, 1)     '特例置換項目-積空区分
                Dim PARA30 As MySqlParameter = SQLcmd.Parameters.Add("@P30", MySqlDbType.VarChar, 2)     '特例置換項目-状態区分
                Dim PARA32 As MySqlParameter = SQLcmd.Parameters.Add("@P32", MySqlDbType.DateTime)        '登録年月日
                Dim PARA33 As MySqlParameter = SQLcmd.Parameters.Add("@P33", MySqlDbType.VarChar, 20)    '登録ユーザーＩＤ
                Dim PARA34 As MySqlParameter = SQLcmd.Parameters.Add("@P34", MySqlDbType.VarChar, 20)    '登録端末
                Dim PARA35 As MySqlParameter = SQLcmd.Parameters.Add("@P35", MySqlDbType.VarChar, 40)    '登録プログラムＩＤ
                Dim PARA36 As MySqlParameter = SQLcmd.Parameters.Add("@P36", MySqlDbType.DateTime)        '更新年月日
                Dim PARA37 As MySqlParameter = SQLcmd.Parameters.Add("@P37", MySqlDbType.VarChar, 20)    '更新ユーザーＩＤ
                Dim PARA38 As MySqlParameter = SQLcmd.Parameters.Add("@P38", MySqlDbType.VarChar, 20)    '更新端末
                Dim PARA39 As MySqlParameter = SQLcmd.Parameters.Add("@P39", MySqlDbType.VarChar, 40)    '更新プログラムＩＤ

                ' 更新ジャーナル出力用パラメータ
                Dim JPARA01 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 6)  '組織コード
                Dim JPARA02 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P02", MySqlDbType.VarChar, 2)  '大分類コード
                Dim JPARA03 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P03", MySqlDbType.VarChar, 2)  '中分類コード
                Dim JPARA04 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P04", MySqlDbType.VarChar, 6)  '発駅コード
                Dim JPARA05 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P05", MySqlDbType.VarChar, 5)  '発受託人コード
                Dim JPARA06 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P06", MySqlDbType.VarChar, 5)  '優先順位

                Dim LNM0007row As DataRow = LNM0007INPtbl.Rows(0)

                'Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA00.Value = LNM0007row("DELFLG")                             '削除フラグ
                PARA01.Value = LNM0007row("ORGCODE")                            '組織コード
                PARA02.Value = LNM0007row("BIGCTNCD")                           '大分類コード
                PARA03.Value = LNM0007row("MIDDLECTNCD")                        '中分類コード
                PARA04.Value = LNM0007row("DEPSTATION")                         '発駅コード
                PARA05.Value = LNM0007row("DEPTRUSTEECD")                       '発受託人コード
                PARA06.Value = LNM0007row("PRIORITYNO")                         '優先順位
                PARA07.Value = LNM0007row("PURPOSE")                            '使用目的
                If String.IsNullOrEmpty(LNM0007row("SMALLCTNCD")) Then          '選択比較項目-小分類コード
                    PARA08.Value = DBNull.Value
                Else
                    PARA08.Value = LNM0007row("SMALLCTNCD")
                End If
                If String.IsNullOrEmpty(LNM0007row("CTNTYPE")) Then             '選択比較項目-コンテナ記号
                    PARA09.Value = DBNull.Value
                Else
                    PARA09.Value = LNM0007row("CTNTYPE")
                End If
                If String.IsNullOrEmpty(LNM0007row("CTNSTNO")) Then             '選択比較項目-コンテナ番号（開始）
                    PARA10.Value = DBNull.Value
                Else
                    PARA10.Value = LNM0007row("CTNSTNO")
                End If
                If String.IsNullOrEmpty(LNM0007row("CTNENDNO")) Then            '選択比較項目-コンテナ番号（終了）
                    PARA11.Value = DBNull.Value
                Else
                    PARA11.Value = LNM0007row("CTNENDNO")
                End If
                If String.IsNullOrEmpty(LNM0007row("SLCSTACKFREEKBN")) Then            '選択比較項目-積空区分
                    PARA12.Value = DBNull.Value
                Else
                    PARA12.Value = LNM0007row("SLCSTACKFREEKBN")
                End If
                If String.IsNullOrEmpty(LNM0007row("SLCSTATUSKBN")) Then            '選択比較項目-状態区分
                    PARA13.Value = DBNull.Value
                Else
                    PARA13.Value = LNM0007row("SLCSTATUSKBN")
                End If
                If String.IsNullOrEmpty(LNM0007row("SLCDEPTRUSTEESUBCD")) Then  '選択比較項目-発受託人サブコード
                    PARA14.Value = DBNull.Value
                Else
                    PARA14.Value = LNM0007row("SLCDEPTRUSTEESUBCD")
                End If
                If String.IsNullOrEmpty(LNM0007row("SLCDEPSHIPPERCD")) Then     '選択比較項目-発荷主コード
                    PARA15.Value = DBNull.Value
                Else
                    PARA15.Value = LNM0007row("SLCDEPSHIPPERCD")
                End If
                If String.IsNullOrEmpty(LNM0007row("SLCARRSTATION")) Then       '選択比較項目-着駅コード
                    PARA16.Value = DBNull.Value
                Else
                    PARA16.Value = LNM0007row("SLCARRSTATION")
                End If
                If String.IsNullOrEmpty(LNM0007row("SLCARRTRUSTEECD")) Then     '選択比較項目-着受託人コード
                    PARA17.Value = DBNull.Value
                Else
                    PARA17.Value = LNM0007row("SLCARRTRUSTEECD")
                End If
                If String.IsNullOrEmpty(LNM0007row("SLCARRTRUSTEESUBCD")) Then  '選択比較項目-着受託人サブコード
                    PARA18.Value = DBNull.Value
                Else
                    PARA18.Value = LNM0007row("SLCARRTRUSTEESUBCD")
                End If
                If String.IsNullOrEmpty(LNM0007row("SLCJRITEMCD")) Then         '選択比較項目-ＪＲ品目コード
                    PARA19.Value = DBNull.Value
                Else
                    PARA19.Value = LNM0007row("SLCJRITEMCD")
                End If
                If String.IsNullOrEmpty(LNM0007row("SLCPICKUPTEL")) Then         '選択比較項目-集荷先電話番号
                    PARA20.Value = DBNull.Value
                Else
                    PARA20.Value = LNM0007row("SLCPICKUPTEL")
                End If
                If String.IsNullOrEmpty(LNM0007row("SPRDEPTRUSTEECD")) Then     '特例置換項目-発受託人コード
                    PARA21.Value = "0"
                Else
                    PARA21.Value = LNM0007row("SPRDEPTRUSTEECD")
                End If
                If String.IsNullOrEmpty(LNM0007row("SPRDEPTRUSTEESUBCD")) Then  '特例置換項目-発受託人サブコード
                    PARA22.Value = "0"
                Else
                    PARA22.Value = LNM0007row("SPRDEPTRUSTEESUBCD")
                End If
                PARA23.Value = LNM0007row("SPRDEPTRUSTEESUBZKBN")               '特例置換項目-発受託人サブゼロ変換区分 
                If String.IsNullOrEmpty(LNM0007row("SPRDEPSHIPPERCD")) Then     '特例置換項目-発荷主コード
                    PARA24.Value = "0"
                Else
                    PARA24.Value = LNM0007row("SPRDEPSHIPPERCD")
                End If
                If String.IsNullOrEmpty(LNM0007row("SPRARRTRUSTEECD")) Then     '特例置換項目-着受託人コード
                    PARA25.Value = "0"
                Else
                    PARA25.Value = LNM0007row("SPRARRTRUSTEECD")
                End If
                If String.IsNullOrEmpty(LNM0007row("SPRARRTRUSTEESUBCD")) Then  '特例置換項目-着受託人サブコード
                    PARA26.Value = "0"
                Else
                    PARA26.Value = LNM0007row("SPRARRTRUSTEESUBCD")
                End If
                PARA27.Value = LNM0007row("SPRARRTRUSTEESUBZKBN")               '特例置換項目-着受託人サブゼロ変換区分
                If String.IsNullOrEmpty(LNM0007row("SPRJRITEMCD")) Then         '特例置換項目-ＪＲ品目コード
                    PARA28.Value = "0"
                Else
                    PARA28.Value = LNM0007row("SPRJRITEMCD")
                End If
                If String.IsNullOrEmpty(LNM0007row("SPRSTACKFREEKBN")) Then     '特例置換項目-積空区分
                    PARA29.Value = "0"
                Else
                    PARA29.Value = LNM0007row("SPRSTACKFREEKBN")
                End If
                If String.IsNullOrEmpty(LNM0007row("SPRSTATUSKBN")) Then        '特例置換項目-状態区分
                    PARA30.Value = "0"
                Else
                    PARA30.Value = LNM0007row("SPRSTATUSKBN")
                End If
                PARA32.Value = WW_NOW                                           '登録年月日
                PARA33.Value = Master.USERID                                    '登録ユーザーＩＤ
                PARA34.Value = Master.USERTERMID                                '登録端末
                PARA35.Value = Me.GetType().BaseType.Name                       '登録プログラムＩＤ
                PARA36.Value = WW_NOW                                           '更新年月日
                PARA37.Value = Master.USERID                                    '更新ユーザーＩＤ
                PARA38.Value = Master.USERTERMID                                '更新端末
                PARA39.Value = Me.GetType().BaseType.Name                       '更新プログラムＩＤ
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA01.Value = LNM0007row("ORGCODE")
                JPARA02.Value = LNM0007row("BIGCTNCD")
                JPARA03.Value = LNM0007row("MIDDLECTNCD")
                JPARA04.Value = LNM0007row("DEPSTATION")
                JPARA05.Value = LNM0007row("DEPTRUSTEECD")
                JPARA06.Value = LNM0007row("PRIORITYNO")

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0007UPDtbl) Then
                        LNM0007UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0007UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0007UPDtbl.Clear()
                    LNM0007UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0007UPDrow As DataRow In LNM0007UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0007C"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0007UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007C UPDATE_INSERT"
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
    Protected Sub RECT1MEXISTS(ByVal SQLcon As MySqlConnection, ByRef WW_MODIFYKBN As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        'コード変換特例マスタ１に同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        ORGCODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0007_RECT1M")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        ORGCODE         = @ORGCODE")
        SQLStr.AppendLine("    AND BIGCTNCD        = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD     = @MIDDLECTNCD")
        SQLStr.AppendLine("    AND DEPSTATION      = @DEPSTATION")
        SQLStr.AppendLine("    AND DEPTRUSTEECD    = @DEPTRUSTEECD")
        SQLStr.AppendLine("    AND PRIORITYNO      = @PRIORITYNO")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位

                Dim LNM0007row As DataRow = LNM0007INPtbl.Rows(0)

                P_ORGCODE.Value = LNM0007row("ORGCODE")               '組織コード
                P_BIGCTNCD.Value = LNM0007row("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = LNM0007row("MIDDLECTNCD")               '中分類コード
                P_DEPSTATION.Value = LNM0007row("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = LNM0007row("DEPTRUSTEECD")               '発受託人コード
                P_PRIORITYNO.Value = LNM0007row("PRIORITYNO")               '優先順位

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
                        WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0112_RECT1HIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         ORGCODE  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,SMALLCTNCD  ")
        SQLStr.AppendLine("        ,CTNTYPE  ")
        SQLStr.AppendLine("        ,CTNSTNO  ")
        SQLStr.AppendLine("        ,CTNENDNO  ")
        SQLStr.AppendLine("        ,SLCSTACKFREEKBN  ")
        SQLStr.AppendLine("        ,SLCSTATUSKBN  ")
        SQLStr.AppendLine("        ,SLCDEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD  ")
        SQLStr.AppendLine("        ,SLCARRSTATION  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD  ")
        SQLStr.AppendLine("        ,SLCPICKUPTEL  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,SPRDEPSHIPPERCD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,SPRJRITEMCD  ")
        SQLStr.AppendLine("        ,SPRSTACKFREEKBN  ")
        SQLStr.AppendLine("        ,SPRSTATUSKBN  ")
        SQLStr.AppendLine("        ,BEFOREORGCODE  ")
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
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,SMALLCTNCD  ")
        SQLStr.AppendLine("        ,CTNTYPE  ")
        SQLStr.AppendLine("        ,CTNSTNO  ")
        SQLStr.AppendLine("        ,CTNENDNO  ")
        SQLStr.AppendLine("        ,SLCSTACKFREEKBN  ")
        SQLStr.AppendLine("        ,SLCSTATUSKBN  ")
        SQLStr.AppendLine("        ,SLCDEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD  ")
        SQLStr.AppendLine("        ,SLCARRSTATION  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD  ")
        SQLStr.AppendLine("        ,SLCPICKUPTEL  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRDEPTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,SPRDEPSHIPPERCD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SPRARRTRUSTEESUBZKBN  ")
        SQLStr.AppendLine("        ,SPRJRITEMCD  ")
        SQLStr.AppendLine("        ,SPRSTACKFREEKBN  ")
        SQLStr.AppendLine("        ,SPRSTATUSKBN  ")
        SQLStr.AppendLine("        ,BEFOREORGCODE  ")
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
        SQLStr.AppendLine("        LNG.LNM0007_RECT1M")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        ORGCODE         = @ORGCODE")
        SQLStr.AppendLine("    AND BIGCTNCD        = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD     = @MIDDLECTNCD")
        SQLStr.AppendLine("    AND DEPSTATION      = @DEPSTATION")
        SQLStr.AppendLine("    AND DEPTRUSTEECD    = @DEPTRUSTEECD")
        SQLStr.AppendLine("    AND PRIORITYNO      = @PRIORITYNO")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード             
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0007row As DataRow = LNM0007INPtbl.Rows(0)

                ' DB更新
                P_ORGCODE.Value = LNM0007row("ORGCODE")               '組織コード
                P_BIGCTNCD.Value = LNM0007row("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = LNM0007row("MIDDLECTNCD")               '中分類コード
                P_DEPSTATION.Value = LNM0007row("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = LNM0007row("DEPTRUSTEECD")               '発受託人コード
                P_PRIORITYNO.Value = LNM0007row("PRIORITYNO")               '優先順位

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0007WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0007tbl.Rows(0)("DELFLG") = "0" And LNM0007row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0007WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0007WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0112_RECT1HIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0112_RECT1HIST  INSERT"
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
        DetailBoxToLNM0007INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0007tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0007tbl, work.WF_SEL_INPTBL.Text)

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
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, "コード変換特例１", needsPopUp:=True)
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
    Protected Sub DetailBoxToLNM0007INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)                '削除フラグ
        Master.EraseCharToIgnore(TxtOrgCode.Text)               '組織コード
        Master.EraseCharToIgnore(TxtBigCTNCD.Text)              '大分類コード
        Master.EraseCharToIgnore(TxtMiddleCTNCD.Text)           '中分類コード
        Master.EraseCharToIgnore(TxtDepStation.Text)            '発駅コード
        Master.EraseCharToIgnore(TxtDepTrusteeCd.Text)          '発受託人コード
        Master.EraseCharToIgnore(TxtPriorityNo.Text)            '優先順位
        Master.EraseCharToIgnore(TxtPurpose.Text)               '使用目的
        Master.EraseCharToIgnore(TxtSmallCTNCD.Text)            '選択比較項目-小分類コード
        Master.EraseCharToIgnore(TxtCTNType.Text)               '選択比較項目-コンテナ記号
        Master.EraseCharToIgnore(TxtCTNStNo.Text)               '選択比較項目-コンテナ番号（開始）
        Master.EraseCharToIgnore(TxtCTNEndNo.Text)              '選択比較項目-コンテナ番号（終了）
        Master.EraseCharToIgnore(TxtSlcStackFreeKbn.Text)       '選択比較項目-積空区分
        Master.EraseCharToIgnore(TxtSlcStatusKbn.Text)          '選択比較項目-状態区分
        Master.EraseCharToIgnore(TxtSlcDepTrusteeSubCd.Text)    '選択比較項目-発受託人サブコード
        Master.EraseCharToIgnore(TxtSlcDepShipperCd.Text)       '選択比較項目-発荷主コード
        Master.EraseCharToIgnore(TxtSlcArrStation.Text)         '選択比較項目-着駅コード
        Master.EraseCharToIgnore(TxtSlcArrTrusteeCd.Text)       '選択比較項目-着受託人コード
        Master.EraseCharToIgnore(TxtSlcArrTrusteeSubCd.Text)    '選択比較項目-着受託人サブコード
        Master.EraseCharToIgnore(TxtSlcJRItemCd.Text)           '選択比較項目-ＪＲ品目コード
        Master.EraseCharToIgnore(TxtSlcPickUpTel.Text)          '選択比較項目-集荷先電話番号
        Master.EraseCharToIgnore(TxtSprDepTrusteeCd.Text)       '特例置換項目-発受託人コード
        Master.EraseCharToIgnore(TxtSprDepTrusteeSubCd.Text)    '特例置換項目-発受託人サブコード
        Master.EraseCharToIgnore(TxtSprDerTrusteeSubZKbn.Text)  '特例置換項目-発受託人サブゼロ変換区分
        Master.EraseCharToIgnore(TxtSprDepShipperCd.Text)       '特例置換項目-発荷主コード
        Master.EraseCharToIgnore(TxtSprArrTrusteeCd.Text)       '特例置換項目-着受託人コード
        Master.EraseCharToIgnore(TxtSprArrTrusteeSubCd.Text)    '特例置換項目-着受託人サブコード
        Master.EraseCharToIgnore(TxtSprArrTrusteeSubZKbn.Text)  '特例置換項目-着受託人サブゼロ変換区分
        Master.EraseCharToIgnore(TxtSprJRItemCd.Text)           '特例置換項目-ＪＲ品目コード
        Master.EraseCharToIgnore(TxtSprStackFreeKbn.Text)       '特例置換項目-積空区分
        Master.EraseCharToIgnore(TxtSprStatusKbn.Text)          '特例置換項目-状態区分

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

        Master.CreateEmptyTable(LNM0007INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0007INProw As DataRow = LNM0007INPtbl.NewRow

        ' LINECNT
        If String.IsNullOrEmpty(LblSelLineCNT.Text) Then
            LNM0007INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(LblSelLineCNT.Text, LNM0007INProw("LINECNT"))
            Catch ex As Exception
                LNM0007INProw("LINECNT") = 0
            End Try
        End If

        LNM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0007INProw("UPDTIMSTP") = 0
        LNM0007INProw("SELECT") = 1
        LNM0007INProw("HIDDEN") = 0

        LNM0007INProw("DELFLG") = TxtDelFlg.Text                              '削除フラグ
        LNM0007INProw("ORGCODE") = TxtOrgCode.Text                            '組織コード
        LNM0007INProw("BIGCTNCD") = TxtBigCTNCD.Text                          '大分類コード
        LNM0007INProw("MIDDLECTNCD") = TxtMiddleCTNCD.Text                    '中分類コード
        LNM0007INProw("DEPSTATION") = TxtDepStation.Text                      '発駅コード
        LNM0007INProw("DEPTRUSTEECD") = TxtDepTrusteeCd.Text                  '発受託人コード
        LNM0007INProw("PRIORITYNO") = TxtPriorityNo.Text                      '優先順位
        LNM0007INProw("PURPOSE") = TxtPurpose.Text                            '使用目的
        LNM0007INProw("SMALLCTNCD") = TxtSmallCTNCD.Text                      '選択比較項目-小分類コード
        LNM0007INProw("CTNTYPE") = TxtCTNType.Text                            '選択比較項目-コンテナ記号
        LNM0007INProw("CTNSTNO") = TxtCTNStNo.Text                            '選択比較項目-コンテナ番号（開始）
        LNM0007INProw("CTNENDNO") = TxtCTNEndNo.Text                          '選択比較項目-コンテナ番号（終了）
        LNM0007INProw("SLCSTACKFREEKBN") = TxtSlcStackFreeKbn.Text            '選択比較項目-積空区分
        LNM0007INProw("SLCSTATUSKBN") = TxtSlcStatusKbn.Text                  '選択比較項目-状態区分
        LNM0007INProw("SLCDEPTRUSTEESUBCD") = TxtSlcDepTrusteeSubCd.Text      '選択比較項目-発受託人サブコード
        LNM0007INProw("SLCDEPSHIPPERCD") = TxtSlcDepShipperCd.Text            '選択比較項目-発荷主コード
        LNM0007INProw("SLCARRSTATION") = TxtSlcArrStation.Text                '選択比較項目-着駅コード
        LNM0007INProw("SLCARRTRUSTEECD") = TxtSlcArrTrusteeCd.Text            '選択比較項目-着受託人コード
        LNM0007INProw("SLCARRTRUSTEESUBCD") = TxtSlcArrTrusteeSubCd.Text      '選択比較項目-着受託人サブコード
        LNM0007INProw("SLCJRITEMCD") = TxtSlcJRItemCd.Text                    '選択比較項目-ＪＲ品目コード
        LNM0007INProw("SLCPICKUPTEL") = TxtSlcPickUpTel.Text                  '選択比較項目-集荷先電話番号
        LNM0007INProw("SPRDEPTRUSTEECD") = TxtSprDepTrusteeCd.Text            '特例置換項目-発受託人コード
        LNM0007INProw("SPRDEPTRUSTEESUBCD") = TxtSprDepTrusteeSubCd.Text      '特例置換項目-発受託人サブコード
        LNM0007INProw("SPRDEPTRUSTEESUBZKBN") = TxtSprDerTrusteeSubZKbn.Text  '特例置換項目-発受託人サブゼロ変換区分
        LNM0007INProw("SPRDEPSHIPPERCD") = TxtSprDepShipperCd.Text            '特例置換項目-発荷主コード
        LNM0007INProw("SPRARRTRUSTEECD") = TxtSprArrTrusteeCd.Text            '特例置換項目-着受託人コード
        LNM0007INProw("SPRARRTRUSTEESUBCD") = TxtSprArrTrusteeSubCd.Text      '特例置換項目-着受託人サブコード
        LNM0007INProw("SPRARRTRUSTEESUBZKBN") = TxtSprArrTrusteeSubZKbn.Text  '特例置換項目-着受託人サブゼロ変換区分
        LNM0007INProw("SPRJRITEMCD") = TxtSprJRItemCd.Text                    '特例置換項目-ＪＲ品目コード
        LNM0007INProw("SPRSTACKFREEKBN") = TxtSprStackFreeKbn.Text            '特例置換項目-積空区分
        LNM0007INProw("SPRSTATUSKBN") = TxtSprStatusKbn.Text                  '特例置換項目-状態区分

        '○ チェック用テーブルに登録する
        LNM0007INPtbl.Rows.Add(LNM0007INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0007INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0007INProw As DataRow = LNM0007INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0007row As DataRow In LNM0007tbl.Rows
            ' KEY項目が等しい時
            If LNM0007row("ORGCODE") = LNM0007INProw("ORGCODE") AndAlso                                '組織コード
               LNM0007row("BIGCTNCD") = LNM0007INProw("BIGCTNCD") AndAlso                              '大分類コード
               LNM0007row("MIDDLECTNCD") = LNM0007INProw("MIDDLECTNCD") AndAlso                        '中分類コード
               LNM0007row("DEPSTATION") = LNM0007INProw("DEPSTATION") AndAlso                          '発駅コード
               LNM0007row("DEPTRUSTEECD") = LNM0007INProw("DEPTRUSTEECD") AndAlso                      '発受託人コード
               LNM0007row("PRIORITYNO") = LNM0007INProw("PRIORITYNO") Then                             '優先順位
                ' KEY項目以外の項目の差異をチェック
                If LNM0007row("DELFLG") = LNM0007INProw("DELFLG") AndAlso                              '削除フラグ
                    LNM0007row("PURPOSE") = LNM0007INProw("PURPOSE") AndAlso                           '使用目的
                   LNM0007row("SMALLCTNCD") = LNM0007INProw("SMALLCTNCD") AndAlso                      '選択比較項目-小分類コード
                   LNM0007row("CTNTYPE") = LNM0007INProw("CTNTYPE") AndAlso                            '選択比較項目-コンテナ記号
                   LNM0007row("CTNSTNO") = LNM0007INProw("CTNSTNO") AndAlso                            '選択比較項目-コンテナ番号（開始）
                   LNM0007row("CTNENDNO") = LNM0007INProw("CTNENDNO") AndAlso                          '選択比較項目-コンテナ番号（終了）
                   LNM0007row("SLCSTACKFREEKBN") = LNM0007INProw("SLCSTACKFREEKBN") AndAlso            '選択比較項目-積空区分
                   LNM0007row("SLCSTATUSKBN") = LNM0007INProw("SLCSTATUSKBN") AndAlso                  '選択比較項目-状態区分
                   LNM0007row("SLCDEPTRUSTEESUBCD") = LNM0007INProw("SLCDEPTRUSTEESUBCD") AndAlso      '選択比較項目-発受託人サブコード
                   LNM0007row("SLCDEPSHIPPERCD") = LNM0007INProw("SLCDEPSHIPPERCD") AndAlso            '選択比較項目-発荷主コード
                   LNM0007row("SLCARRSTATION") = LNM0007INProw("SLCARRSTATION") AndAlso                '選択比較項目-着駅コード
                   LNM0007row("SLCARRTRUSTEECD") = LNM0007INProw("SLCARRTRUSTEECD") AndAlso            '選択比較項目-着受託人コード
                   LNM0007row("SLCARRTRUSTEESUBCD") = LNM0007INProw("SLCARRTRUSTEESUBCD") AndAlso      '選択比較項目-着受託人サブコード
                   LNM0007row("SLCJRITEMCD") = LNM0007INProw("SLCJRITEMCD") AndAlso                    '選択比較項目-ＪＲ品目コード
                   LNM0007row("SLCPICKUPTEL") = LNM0007INProw("SLCPICKUPTEL") AndAlso                  '選択比較項目-集荷先電話番号
                   LNM0007row("SPRDEPTRUSTEECD") = LNM0007INProw("SPRDEPTRUSTEECD") AndAlso            '特例置換項目-発受託人コード
                   LNM0007row("SPRDEPTRUSTEESUBCD") = LNM0007INProw("SPRDEPTRUSTEESUBCD") AndAlso      '特例置換項目-発受託人サブコード
                   LNM0007row("SPRDEPTRUSTEESUBZKBN") = LNM0007INProw("SPRDEPTRUSTEESUBZKBN") AndAlso  '特例置換項目-発受託人サブゼロ変換区分
                   LNM0007row("SPRDEPSHIPPERCD") = LNM0007INProw("SPRDEPSHIPPERCD") AndAlso            '特例置換項目-発荷主コード
                   LNM0007row("SPRARRTRUSTEECD") = LNM0007INProw("SPRARRTRUSTEECD") AndAlso            '特例置換項目-着受託人コード
                   LNM0007row("SPRARRTRUSTEESUBCD") = LNM0007INProw("SPRARRTRUSTEESUBCD") AndAlso      '特例置換項目-着受託人サブコード
                   LNM0007row("SPRARRTRUSTEESUBZKBN") = LNM0007INProw("SPRARRTRUSTEESUBZKBN") AndAlso  '特例置換項目-着受託人サブゼロ変換区分
                   LNM0007row("SPRJRITEMCD") = LNM0007INProw("SPRJRITEMCD") AndAlso                    '特例置換項目-ＪＲ品目コード
                   LNM0007row("SPRSTACKFREEKBN") = LNM0007INProw("SPRSTACKFREEKBN") AndAlso            '特例置換項目-積空区分
                   LNM0007row("SPRSTATUSKBN") = LNM0007INProw("SPRSTATUSKBN") Then                     '特例置換項目-状態区分
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
        For Each LNM0007row As DataRow In LNM0007tbl.Rows
            Select Case LNM0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0007tbl, work.WF_SEL_INPTBL.Text)

        LblSelLineCNT.Text = ""               'LINECNT
        TxtMapId.Text = "M00001"              '画面ＩＤ
        TxtDelFlg.Text = ""                   '削除フラグ
        TxtOrgCode.Text = ""                  '組織コード
        TxtBigCTNCD.Text = ""                 '大分類コード
        TxtMiddleCTNCD.Text = ""              '中分類コード
        TxtDepStation.Text = ""               '発駅コード
        TxtDepTrusteeCd.Text = ""             '発受託人コード
        TxtPriorityNo.Text = ""               '優先順位
        TxtPurpose.Text = ""                  '使用目的
        TxtSmallCTNCD.Text = ""               '選択比較項目-小分類コード
        TxtCTNType.Text = ""                  '選択比較項目-コンテナ記号
        TxtCTNStNo.Text = ""                  '選択比較項目-コンテナ番号（開始）
        TxtCTNEndNo.Text = ""                 '選択比較項目-コンテナ番号（終了）
        TxtSlcStackFreeKbn.Text = ""          '選択比較項目-積空区分
        TxtSlcStatusKbn.Text = ""             '選択比較項目-状態区分
        TxtSlcDepTrusteeSubCd.Text = ""       '選択比較項目-発受託人サブコード
        TxtSlcDepShipperCd.Text = ""          '選択比較項目-発荷主コード
        TxtSlcArrStation.Text = ""            '選択比較項目-着駅コード
        TxtSlcArrTrusteeCd.Text = ""          '選択比較項目-着受託人コード
        TxtSlcArrTrusteeSubCd.Text = ""       '選択比較項目-着受託人サブコード
        TxtSlcJRItemCd.Text = ""              '選択比較項目-ＪＲ品目コード
        TxtSlcPickUpTel.Text = ""             '選択比較項目-集荷先電話番号
        TxtSprDepTrusteeCd.Text = ""          '特例置換項目-発受託人コード
        TxtSprDepTrusteeSubCd.Text = ""       '特例置換項目-発受託人サブコード
        TxtSprDerTrusteeSubZKbn.Text = ""     '特例置換項目-発受託人サブゼロ変換区分
        TxtSprDepShipperCd.Text = ""          '特例置換項目-発荷主コード
        TxtSprArrTrusteeCd.Text = ""          '特例置換項目-着受託人コード
        TxtSprArrTrusteeSubCd.Text = ""       '特例置換項目-着受託人サブコード
        TxtSprArrTrusteeSubZKbn.Text = ""     '特例置換項目-着受託人サブゼロ変換区分
        TxtSprJRItemCd.Text = ""              '特例置換項目-ＪＲ品目コード
        TxtSprStackFreeKbn.Text = ""          '特例置換項目-積空区分
        TxtSprStatusKbn.Text = ""             '特例置換項目-状態区分

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
                    Case "TxtOrgCode"              '組織コード
                        WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP)
                    Case "TxtBigCTNCD"             '大分類コード
                        WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                    Case "TxtMiddleCTNCD"          '中分類コード
                        WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, TxtBigCTNCD.Text)
                    Case "TxtDepStation",          '発駅コード
                         "TxtSlcArrStation"        '選択比較項目-着駅コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspStationSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub

                    Case "TxtDepTrusteeCd",        '発受託人コード
                         "TxtSprDepTrusteeCd"      '特例置換項目-発受託人コード
                        WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtDepStation.Text)
                    Case "TxtSmallCTNCD"           '小分類コード
                        WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.SMALL_CLASS, TxtBigCTNCD.Text, TxtMiddleCTNCD.Text)
                    Case "TxtCTNType"              'コンテナ記号
                        WW_PrmData = work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_TYPE)
                    Case "TxtCTNStNo",             'コンテナ番号
                         "TxtCTNEndNo"             'コンテナ番号
                        WW_PrmData = work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, TxtCTNType.Text)
                    Case "TxtSlcStackFreeKbn",     '選択比較項目-積空区分
                         "TxtSprStackFreeKbn"      '特例置換項目-積空区分
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "STACKFREEKBN")
                    Case "TxtSlcStatusKbn",        '選択比較項目-状態区分
                         "TxtSprStatusKbn"         '特例置換項目-状態区分
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "OPERATIONKBN")
                    Case "TxtSlcDepTrusteeSubCd",  '発受託人サブコード
                         "TxtSprDepTrusteeSubCd"   '特例置換項目-発受託人サブコード
                        WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtDepStation.Text, TxtDepTrusteeCd.Text)
                    Case "TxtSlcArrTrusteeCd",     '選択比較項目-着受託人コード
                         "TxtSprArrTrusteeCd"      '特例置換項目-着受託人コード
                        WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtSlcArrStation.Text)
                    Case "TxtSlcArrTrusteeSubCd",  '選択比較項目-着受託人サブコード
                         "TxtSprArrTrusteeSubCd"   '特例置換項目-着受託人サブコード
                        WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtSlcArrStation.Text, TxtSlcArrTrusteeCd.Text)
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
            Case "TxtDepStation"               '発駅コード
                CODENAME_get("STATION", TxtDepStation.Text, LblDepStationName.Text, WW_Dummy)
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
            Case "TxtDepTrusteeCd"             '発受託人コード
                CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCd.Text, LblDepTrusteeCdName.Text, WW_Dummy)
                TxtDepTrusteeCd.Focus()
            Case "TxtSmallCTNCD"               '選択比較項目-小分類コード
                CODENAME_get("SMALLCTNCD", TxtSmallCTNCD.Text, LblSmallCTNCDName.Text, WW_Dummy)
                TxtSmallCTNCD.Focus()
            Case "TxtCTNType"                  '選択比較項目-コンテナ記号
                CODENAME_get("CTNTYPE", TxtCTNType.Text, LblCTNTypeName.Text, WW_Dummy)
                TxtCTNType.Focus()
            Case "TxtSlcStackFreeKbn"          '選択比較項目-積空区分
                CODENAME_get("STACKFREEKBN", TxtSlcStackFreeKbn.Text, LblSlcStackFreeKbnName.Text, WW_Dummy)
                TxtSlcStackFreeKbn.Focus()
            Case "TxtSlcStatusKbn"             '選択比較項目-状態区分
                CODENAME_get("OPERATIONKBN", TxtSlcStatusKbn.Text, LblSlcStatusKbnName.Text, WW_Dummy)
                TxtSlcStatusKbn.Focus()
            Case "TxtSlcDepTrusteeSubCd"       '選択比較項目-発受託人サブコード
                CODENAME_get("DEPTRUSTEESUBCD", TxtSlcDepTrusteeSubCd.Text, LblSlcDepTrusteeSubCdName.Text, WW_Dummy)
                TxtSlcDepTrusteeSubCd.Focus()
            Case "TxtSlcDepShipperCd"          '選択比較項目-発荷主コード
                CODENAME_get("SHIPPER", TxtSlcDepShipperCd.Text, LblSlcDepShipperCdName.Text, WW_Dummy)
                TxtSlcDepShipperCd.Focus()
            Case "TxtSlcArrStation"            '選択比較項目-着駅コード
                CODENAME_get("STATION", TxtSlcArrStation.Text, LblSlcArrStationName.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblSlcArrStationName.Text) And TxtSlcArrStation.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtSlcArrStation.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtSlcArrStation.Focus()
                End If
            Case "TxtSlcArrTrusteeCd"          '選択比較項目-着受託人コード
                CODENAME_get("ARRTRUSTEECD", TxtSlcArrTrusteeCd.Text, LblSlcArrTrusteeCdName.Text, WW_Dummy)
                TxtSlcArrTrusteeCd.Focus()
            Case "TxtSlcArrTrusteeSubCd"       '選択比較項目-着受託人サブコード
                CODENAME_get("ARRTRUSTEESUBCD", TxtSlcArrTrusteeSubCd.Text, LblSlcArrTrusteeSubCdName.Text, WW_Dummy)
                TxtSlcArrTrusteeSubCd.Focus()
            Case "TxtSlcJRItemCd"              '選択比較項目-ＪＲ品目コード
                CODENAME_get("ITEM", TxtSlcJRItemCd.Text, LblSlcJRItemCdName.Text, WW_Dummy)
                TxtSlcJRItemCd.Focus()
            Case "TxtSprDepTrusteeCd"          '特例置換項目-発受託人コード
                CODENAME_get("DEPTRUSTEECD", TxtSprDepTrusteeCd.Text, LblSprDepTrusteeCdName.Text, WW_Dummy)
                TxtSprDepTrusteeCd.Focus()
            Case "TxtSprDepTrusteeSubCd"       '特例置換項目-発受託人サブコード
                CODENAME_get("DEPTRUSTEESUBCD", TxtSprDepTrusteeSubCd.Text, LblSprDepTrusteeSubCdName.Text, WW_Dummy)
                TxtSprDepTrusteeSubCd.Focus()
            Case "TxtSprDepShipperCd"          '特例置換項目-発荷主コード
                CODENAME_get("SHIPPER", TxtSprDepShipperCd.Text, LblSprDepShipperCdName.Text, WW_Dummy)
                TxtSprDepShipperCd.Focus()
            Case "TxtSprArrTrusteeCd"          '特例置換項目-着受託人コード
                CODENAME_get("ARRTRUSTEECD", TxtSprArrTrusteeCd.Text, LblSprArrTrusteeCdName.Text, WW_Dummy)
                TxtSprArrTrusteeCd.Focus()
            Case "TxtSprArrTrusteeSubCd"       '特例置換項目-着受託人サブコード
                CODENAME_get("ARRTRUSTEESUBCD", TxtSprArrTrusteeSubCd.Text, LblSprArrTrusteeSubCdName.Text, WW_Dummy)
                TxtSprArrTrusteeSubCd.Focus()
            Case "TxtSprJRItemCd"              '特例置換項目-ＪＲ品目コード
                CODENAME_get("ITEM", TxtSprJRItemCd.Text, LblSprJRItemCdName.Text, WW_Dummy)
                TxtSprJRItemCd.Focus()
            Case "TxtSprStackFreeKbn"          '特例置換項目-積空区分
                CODENAME_get("STACKFREEKBN", TxtSprStackFreeKbn.Text, LblSprStackFreeKbnName.Text, WW_Dummy)
                TxtSprStackFreeKbn.Focus()
            Case "TxtSprStatusKbn"             '特例置換項目-状態区分
                CODENAME_get("OPERATIONKBN", TxtSprStatusKbn.Text, LblSprStatusKbnName.Text, WW_Dummy)
                TxtSprStatusKbn.Focus()
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
                Case "TxtDelFlg"                   '削除フラグ
                    TxtDelFlg.Text = WW_SelectValue
                    LblDelFlgName.Text = WW_SelectText
                    TxtDelFlg.Focus()
                Case "TxtOrgCode"                  '組織コード
                    TxtOrgCode.Text = WW_SelectValue
                    LblOrgName.Text = WW_SelectText
                    TxtOrgCode.Focus()
                Case "TxtBigCTNCD"                 '大分類コード
                    TxtBigCTNCD.Text = WW_SelectValue
                    LblBigCTNCDName.Text = WW_SelectText
                    TxtBigCTNCD.Focus()
                Case "TxtMiddleCTNCD"              '中分類コード
                    TxtMiddleCTNCD.Text = WW_SelectValue
                    LblMiddleCTNCDName.Text = WW_SelectText
                    TxtMiddleCTNCD.Focus()
                Case "TxtDepStation"               '発駅コード
                    TxtDepStation.Text = WW_SelectValue
                    LblDepStationName.Text = WW_SelectText
                    TxtDepStation.Focus()
                Case "TxtDepTrusteeCd"             '発受託人コード
                    TxtDepTrusteeCd.Text = WW_SelectValue
                    LblDepTrusteeCdName.Text = WW_SelectText
                    TxtDepTrusteeCd.Focus()
                Case "TxtSmallCTNCD"               '選択比較項目-小分類コード
                    TxtSmallCTNCD.Text = WW_SelectValue
                    LblSmallCTNCDName.Text = WW_SelectText
                    TxtSmallCTNCD.Focus()
                Case "TxtCTNType"                  '選択比較項目-コンテナ記号
                    TxtCTNType.Text = WW_SelectValue
                    LblCTNTypeName.Text = WW_SelectText
                    TxtCTNType.Focus()
                Case "TxtCTNStNo"                  '選択比較項目-コンテナ番号（開始）
                    TxtCTNStNo.Text = WW_SelectValue
                    LblCTNStNoName.Text = WW_SelectText
                    TxtCTNStNo.Focus()
                Case "TxtCTNEndNo"                 '選択比較項目-コンテナ番号（終了）
                    TxtDepTrusteeCd.Text = WW_SelectValue
                    LblDepTrusteeCdName.Text = WW_SelectText
                    TxtCTNEndNo.Focus()
                Case "TxtSlcStackFreeKbn"          '選択比較項目-積空区分
                    TxtSlcStackFreeKbn.Text = WW_SelectValue
                    LblSlcStackFreeKbnName.Text = WW_SelectText
                    TxtSlcStackFreeKbn.Focus()
                Case "TxtSlcStatusKbn"             '選択比較項目-状態区分
                    TxtSlcStatusKbn.Text = WW_SelectValue
                    LblSlcStatusKbnName.Text = WW_SelectText
                    TxtSlcStatusKbn.Focus()
                Case "TxtSlcDepTrusteeSubCd"       '発受託人サブコード
                    TxtSlcDepTrusteeSubCd.Text = WW_SelectValue
                    LblSlcDepTrusteeSubCdName.Text = WW_SelectText
                    TxtSlcDepTrusteeSubCd.Focus()
                Case "TxtSlcDepShipperCd"          '選択比較項目-発荷主コード
                    TxtSlcDepShipperCd.Text = WW_SelectValue
                    LblSlcDepShipperCdName.Text = WW_SelectText
                    TxtSlcDepShipperCd.Focus()
                Case "TxtSlcArrStation"            '選択比較項目-着駅コード
                    TxtSlcArrStation.Text = WW_SelectValue
                    LblSlcArrStationName.Text = WW_SelectText
                    TxtSlcArrStation.Focus()
                Case "TxtSlcArrTrusteeCd"          '選択比較項目-着受託人コード
                    TxtSlcArrTrusteeCd.Text = WW_SelectValue
                    LblSlcArrTrusteeCdName.Text = WW_SelectText
                    TxtSlcArrTrusteeCd.Focus()
                Case "TxtSlcArrTrusteeSubCd"       '選択比較項目-着受託人サブコード
                    TxtSlcArrTrusteeSubCd.Text = WW_SelectValue
                    LblSlcArrTrusteeSubCdName.Text = WW_SelectText
                    TxtSlcArrTrusteeSubCd.Focus()
                Case "TxtSlcJRItemCd"              '選択比較項目-ＪＲ品目コード
                    TxtSlcJRItemCd.Text = WW_SelectValue
                    LblSlcJRItemCdName.Text = WW_SelectText
                    TxtSlcJRItemCd.Focus()
                Case "TxtSprDepTrusteeCd"          '特例置換項目-発受託人コード
                    TxtSprDepTrusteeCd.Text = WW_SelectValue
                    LblSprDepTrusteeCdName.Text = WW_SelectText
                    TxtSprDepTrusteeCd.Focus()
                Case "TxtSprDepTrusteeSubCd"       '特例置換項目-発受託人サブコード
                    TxtSprDepTrusteeSubCd.Text = WW_SelectValue
                    LblSprDepTrusteeSubCdName.Text = WW_SelectText
                    TxtSprDepTrusteeSubCd.Focus()
                Case "TxtSprDepShipperCd"          '特例置換項目-発荷主コード
                    TxtSprDepShipperCd.Text = WW_SelectValue
                    LblSprDepShipperCdName.Text = WW_SelectText
                    TxtSprDepShipperCd.Focus()
                Case "TxtSprArrTrusteeCd"          '特例置換項目-着受託人コード
                    TxtSprArrTrusteeCd.Text = WW_SelectValue
                    LblSprArrTrusteeCdName.Text = WW_SelectText
                    TxtSprArrTrusteeCd.Focus()
                Case "TxtSprArrTrusteeSubCd"       '特例置換項目-着受託人サブコード
                    TxtSprArrTrusteeSubCd.Text = WW_SelectValue
                    LblSprArrTrusteeSubCdName.Text = WW_SelectText
                    TxtSprArrTrusteeSubCd.Focus()
                Case "TxtSprJRItemCd"              '特例置換項目-ＪＲ品目コード
                    TxtSprJRItemCd.Text = WW_SelectValue
                    LblSprJRItemCdName.Text = WW_SelectText
                    TxtSprJRItemCd.Focus()
                Case "TxtSprStackFreeKbn"          '特例置換項目-積空区分
                    TxtSprStackFreeKbn.Text = WW_SelectValue
                    LblSprStackFreeKbnName.Text = WW_SelectText
                    TxtSprStackFreeKbn.Focus()
                Case "TxtSprStatusKbn"             '特例置換項目-状態区分
                    TxtSprStatusKbn.Text = WW_SelectValue
                    LblSprStatusKbnName.Text = WW_SelectText
                    TxtSprStatusKbn.Focus()
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
                Case "TxtDelFlg"                   '削除フラグ
                    TxtDelFlg.Focus()
                Case "TxtOrgCode"                  '組織コード
                    TxtOrgCode.Focus()
                Case "TxtBigCTNCD"                 '大分類コード
                    TxtBigCTNCD.Focus()
                Case "TxtMiddleCTNCD"              '中分類コード
                    TxtMiddleCTNCD.Focus()
                Case "TxtDepStation"               '発駅コード
                    TxtDepStation.Focus()
                Case "TxtDepTrusteeCd"             '発受託人コード
                    TxtDepTrusteeCd.Focus()
                Case "TxtSmallCTNCD"               '選択比較項目-小分類コード
                    TxtSmallCTNCD.Focus()
                Case "TxtCTNType"                  '選択比較項目-コンテナ記号
                    TxtCTNType.Focus()
                Case "TxtCTNStNo"                  '選択比較項目-コンテナ番号（開始）
                    TxtCTNStNo.Focus()
                Case "TxtCTNEndNo"                 '選択比較項目-コンテナ番号（終了）
                    TxtCTNEndNo.Focus()
                Case "TxtSlcStackFreeKbn"          '選択比較項目-積空区分
                    TxtSlcStackFreeKbn.Focus()
                Case "TxtSlcStatusKbn"             '選択比較項目-状態区分
                    TxtSlcStatusKbn.Focus()
                Case "TxtSlcDepTrusteeSubCd"       '選択比較項目-発受託人サブコード
                    TxtSlcDepTrusteeSubCd.Focus()
                Case "TxtSlcDepShipperCd"          '選択比較項目-発荷主コード
                    TxtSlcDepShipperCd.Focus()
                Case "TxtSlcArrStation"            '選択比較項目-着駅コード
                    TxtSlcArrStation.Focus()
                Case "TxtSlcArrTrusteeCd"          '選択比較項目-着受託人コード
                    TxtSlcArrTrusteeCd.Focus()
                Case "TxtSlcArrTrusteeSubCd"       '選択比較項目-着受託人サブコード
                    TxtSlcArrTrusteeSubCd.Focus()
                Case "TxtSlcJRItemCd"              '選択比較項目-ＪＲ品目コード
                    TxtSlcJRItemCd.Focus()
                Case "TxtSprDepTrusteeCd"          '特例置換項目-発受託人コード
                    TxtSprDepTrusteeCd.Focus()
                Case "TxtSprDepTrusteeSubCd"       '特例置換項目-発受託人サブコード
                    TxtSprDepTrusteeSubCd.Focus()
                Case "TxtSprDepShipperCd"          '特例置換項目-発荷主コード
                    TxtSprDepShipperCd.Focus()
                Case "TxtSprArrTrusteeCd"          '特例置換項目-着受託人コード
                    TxtSprArrTrusteeCd.Focus()
                Case "TxtSprArrTrusteeSubCd"       '特例置換項目-着受託人サブコード
                    TxtSprArrTrusteeSubCd.Focus()
                Case "TxtSprJRItemCd"              '特例置換項目-ＪＲ品目コード
                    TxtSprJRItemCd.Focus()
                Case "TxtSprStackFreeKbn"          '特例置換項目-積空区分
                    TxtSprStackFreeKbn.Focus()
                Case "TxtSprStatusKbn"             '特例置換項目-状態区分
                    TxtSprStatusKbn.Focus()
            End Select
        End If

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
    ''' 取引先選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspStationSingle()

        Dim selData = Me.mspStationSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtDepStation.ID
                Me.TxtDepStation.Text = selData("STATION").ToString
                Me.LblDepStationName.Text = selData("NAMES").ToString
                Me.TxtDepStation.Focus()

            Case TxtSlcArrStation.ID
                Me.TxtSlcArrStation.Text = selData("STATION").ToString
                Me.LblSlcArrStationName.Text = selData("NAMES").ToString
                Me.TxtSlcArrStation.Focus()
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

        '○ 画面操作権限チェック
        ' 権限チェック(操作者に更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            WW_CheckMES1 = "・コード変換特例マスタ１更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0007INProw As DataRow In LNM0007INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0007INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0007INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "ORG", LNM0007INProw("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("ORG", LNM0007INProw("ORGCODE"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "BIGCTNCD", LNM0007INProw("BIGCTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("BIGCTNCD", LNM0007INProw("BIGCTNCD"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "MIDDLECTNCD", LNM0007INProw("MIDDLECTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("MIDDLECTNCD", LNM0007INProw("MIDDLECTNCD"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "DEPSTATION", LNM0007INProw("DEPSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("STATION", LNM0007INProw("DEPSTATION"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEECD", LNM0007INProw("DEPTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DEPTRUSTEECD", LNM0007INProw("DEPTRUSTEECD"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・発受託人コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・発受託人コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 優先順位(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PRIORITYNO", LNM0007INProw("PRIORITYNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・優先順位エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 使用目的(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PURPOSE", LNM0007INProw("PURPOSE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・使用目的エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-小分類コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SMALLCTNCD", LNM0007INProw("SMALLCTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0007INProw("SMALLCTNCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("SMALLCTNCD", LNM0007INProw("SMALLCTNCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-小分類コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-小分類コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-コンテナ記号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CTNTYPE", LNM0007INProw("CTNTYPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0007INProw("CTNTYPE")) Then
                    ' 名称存在チェック
                    CODENAME_get("CTNTYPE", LNM0007INProw("CTNTYPE"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-コンテナ記号エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-コンテナ記号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-コンテナ番号（開始）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CTNSTNO", LNM0007INProw("CTNSTNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0007INProw("CTNSTNO")) Then
                    ' 名称存在チェック
                    CODENAME_get("CTNNO", LNM0007INProw("CTNSTNO"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-コンテナ番号（開始）エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-コンテナ番号（開始）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-コンテナ番号（終了）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CTNENDNO", LNM0007INProw("CTNENDNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0007INProw("CTNENDNO")) Then
                    ' 名称存在チェック
                    CODENAME_get("CTNNO", LNM0007INProw("CTNENDNO"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-コンテナ番号（終了）エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-コンテナ番号（終了）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' コンテナ番号大小チェック(コンテナ番号（開始）・コンテナ番号（終了）)
            If Not String.IsNullOrEmpty(LNM0007INProw("CTNSTNO")) AndAlso
                Not String.IsNullOrEmpty(LNM0007INProw("CTNENDNO")) Then
                If CInt(LNM0007INProw("CTNSTNO")) > CInt(LNM0007INProw("CTNENDNO")) Then
                    WW_CheckMES1 = "・選択比較項目-コンテナ番号(開始)＆選択比較項目-コンテナ番号(終了)エラー"
                    WW_CheckMES2 = "コンテナ番号大小入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 選択比較項目-積空区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCSTACKFREEKBN", LNM0007INProw("SLCSTACKFREEKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0007INProw("SLCSTACKFREEKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("STACKFREEKBN", LNM0007INProw("SLCSTACKFREEKBN"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-積空区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-積空区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-状態区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCSTATUSKBN", LNM0007INProw("SLCSTATUSKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0007INProw("SLCSTATUSKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("OPERATIONKBN", LNM0007INProw("SLCSTATUSKBN"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-状態区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-状態区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-発受託人サブコード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCDEPTRUSTEESUBCD", LNM0007INProw("SLCDEPTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0007INProw("SLCDEPTRUSTEESUBCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("DEPTRUSTEESUBCD", LNM0007INProw("SLCDEPTRUSTEESUBCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発受託人サブコードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発受託人サブコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-発荷主コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCD", LNM0007INProw("SLCDEPSHIPPERCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0007INProw("SLCDEPSHIPPERCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("SHIPPER", LNM0007INProw("SLCDEPSHIPPERCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発荷主コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発荷主コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着駅コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION", LNM0007INProw("SLCARRSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0007INProw("SLCARRSTATION")) Then
                    ' 名称存在チェック
                    CODENAME_get("STATION", LNM0007INProw("SLCARRSTATION"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着駅コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着受託人コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRTRUSTEECD", LNM0007INProw("SLCARRTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0007INProw("SLCARRTRUSTEECD")) Then
                    ' 名称存在チェック
                    CODENAME_get("ARRTRUSTEECD", LNM0007INProw("SLCARRTRUSTEECD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着受託人コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着受託人コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着受託人サブコード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRTRUSTEESUBCD", LNM0007INProw("SLCARRTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0007INProw("SLCARRTRUSTEESUBCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("ARRTRUSTEESUBCD", LNM0007INProw("SLCARRTRUSTEESUBCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着受託人サブコードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着受託人サブコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ品目コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD", LNM0007INProw("SLCJRITEMCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0007INProw("SLCJRITEMCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", LNM0007INProw("SLCJRITEMCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-集荷先電話番号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCPICKUPTEL", LNM0007INProw("SLCPICKUPTEL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・選択比較項目-集荷先電話番号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-発受託人コード(バリデーションチェック)
            If Not LNM0007INProw("SPRDEPTRUSTEECD") = "0" Then
                Master.CheckField(Master.USERCAMP, "SPRDEPTRUSTEECD", LNM0007INProw("SPRDEPTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0007INProw("SPRDEPTRUSTEECD")) Then
                        ' 名称存在チェック
                        CODENAME_get("DEPTRUSTEECD", LNM0007INProw("SPRDEPTRUSTEECD"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・特例置換項目-発受託人コードエラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・特例置換項目-発受託人コードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-発受託人サブコード(バリデーションチェック)
            If Not LNM0007INProw("SPRDEPTRUSTEESUBCD") = "0" Then
                Master.CheckField(Master.USERCAMP, "SPRDEPTRUSTEESUBCD", LNM0007INProw("SPRDEPTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0007INProw("SPRDEPTRUSTEESUBCD")) Then
                        ' 名称存在チェック
                        CODENAME_get("DEPTRUSTEESUBCD", LNM0007INProw("SPRDEPTRUSTEESUBCD"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・特例置換項目-発受託人サブコードエラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・特例置換項目-発受託人サブコードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-発受託人サブゼロ変換区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRDEPTRUSTEESUBZKBN", LNM0007INProw("SPRDEPTRUSTEESUBZKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) OrElse CInt(LNM0007INProw("SPRDEPTRUSTEESUBZKBN")) > 1 Then
                WW_CheckMES1 = "・特例置換項目-発受託人サブゼロ変換区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-発荷主コード(バリデーションチェック)
            If Not LNM0007INProw("SPRDEPSHIPPERCD") = "0" Then
                Master.CheckField(Master.USERCAMP, "SPRDEPSHIPPERCD", LNM0007INProw("SPRDEPSHIPPERCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0007INProw("SPRDEPSHIPPERCD")) Then
                        ' 名称存在チェック
                        CODENAME_get("SHIPPER", LNM0007INProw("SPRDEPSHIPPERCD"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・特例置換項目-発荷主コードエラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・特例置換項目-発荷主コードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-着受託人コード(バリデーションチェック)
            If Not LNM0007INProw("SPRARRTRUSTEECD") = "0" Then
                Master.CheckField(Master.USERCAMP, "SPRARRTRUSTEECD", LNM0007INProw("SPRARRTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0007INProw("SPRARRTRUSTEECD")) Then
                        ' 名称存在チェック
                        CODENAME_get("ARRTRUSTEECD", LNM0007INProw("SPRARRTRUSTEECD"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・特例置換項目-着受託人コードエラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・特例置換項目-着受託人コードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-着受託人サブコード(バリデーションチェック)
            If Not LNM0007INProw("SPRARRTRUSTEESUBCD") = "0" Then
                Master.CheckField(Master.USERCAMP, "SPRARRTRUSTEESUBCD", LNM0007INProw("SPRARRTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0007INProw("SPRARRTRUSTEESUBCD")) Then
                        ' 名称存在チェック
                        CODENAME_get("ARRTRUSTEESUBCD", LNM0007INProw("SPRARRTRUSTEESUBCD"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・特例置換項目-着受託人サブコードエラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・特例置換項目-着受託人サブコードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-着受託人サブゼロ変換区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRARRTRUSTEESUBZKBN", LNM0007INProw("SPRARRTRUSTEESUBZKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) OrElse CInt(LNM0007INProw("SPRARRTRUSTEESUBZKBN")) > 1 Then
                WW_CheckMES1 = "・特例置換項目-着受託人サブゼロ変換区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-ＪＲ品目コード(バリデーションチェック)
            If Not LNM0007INProw("SPRJRITEMCD") = "0" Then
                Master.CheckField(Master.USERCAMP, "SPRJRITEMCD", LNM0007INProw("SPRJRITEMCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0007INProw("SPRJRITEMCD")) Then
                        ' 名称存在チェック
                        CODENAME_get("ITEM", LNM0007INProw("SPRJRITEMCD"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・特例置換項目-ＪＲ品目コードエラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・特例置換項目-ＪＲ品目コードエラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-積空区分(バリデーションチェック)
            If Not LNM0007INProw("SPRSTACKFREEKBN") = "0" Then
                Master.CheckField(Master.USERCAMP, "SPRSTACKFREEKBN", LNM0007INProw("SPRSTACKFREEKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0007INProw("SPRSTACKFREEKBN")) Then
                        ' 名称存在チェック
                        CODENAME_get("STACKFREEKBN", LNM0007INProw("SPRSTACKFREEKBN"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・特例置換項目-積空区分エラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・特例置換項目-積空区分エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-状態区分(バリデーションチェック)
            If Not LNM0007INProw("SPRSTATUSKBN") = "0" Then
                Master.CheckField(Master.USERCAMP, "SPRSTATUSKBN", LNM0007INProw("SPRSTATUSKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If Not String.IsNullOrEmpty(LNM0007INProw("SPRSTATUSKBN")) Then
                        ' 名称存在チェック
                        CODENAME_get("OPERATIONKBN", LNM0007INProw("SPRSTATUSKBN"), WW_Dummy, WW_RtnSW)
                        If Not isNormal(WW_RtnSW) Then
                            WW_CheckMES1 = "・特例置換項目-状態区分エラーです。"
                            WW_CheckMES2 = "マスタに存在しません。"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        End If
                    End If
                Else
                    WW_CheckMES1 = "・特例置換項目-状態区分エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_ORG2.Text) Then  '組織コード
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                    TxtOrgCode.Text, TxtBigCTNCD.Text,
                                    TxtMiddleCTNCD.Text, TxtDepStation.Text,
                                    TxtDepTrusteeCd.Text, TxtPriorityNo.Text,
                                    work.WF_SEL_TIMESTAMP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（組織コード&大分類コード&中分類コード&発駅コード&発受託人コード&優先順位）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                       "([" & LNM0007INProw("ORGCODE") & "]" &
                                       "([" & LNM0007INProw("BIGCTNCD") & "]" &
                                       "([" & LNM0007INProw("MIDDLECTNCD") & "]" &
                                       "([" & LNM0007INProw("DEPSTATION") & "]" &
                                       "([" & LNM0007INProw("DEPTRUSTEECD") & "]" &
                                       " [" & LNM0007INProw("PRIORITYNO") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNM0007INProw("ORGCODE") = work.WF_SEL_ORG2.Text OrElse                '組織コード
               Not LNM0007INProw("BIGCTNCD") = work.WF_SEL_BIGCTNCD2.Text OrElse          '大分類コード
               Not LNM0007INProw("MIDDLECTNCD") = work.WF_SEL_MIDDLECTNCD2.Text OrElse    '中分類コード
               Not LNM0007INProw("DEPSTATION") = work.WF_SEL_DEPSTATION2.Text OrElse      '発駅コード
               Not LNM0007INProw("DEPTRUSTEECD") = work.WF_SEL_DEPTRUSTEECD2.Text OrElse  '発受託人コード
               Not LNM0007INProw("PRIORITYNO") = work.WF_SEL_PRIORITYNO.Text Then         '優先順位
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（組織コード&大分類コード&中分類コード&発駅コード&発受託人コード&優先順位）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & LNM0007INProw("ORGCODE") & "]" &
                                       "([" & LNM0007INProw("BIGCTNCD") & "]" &
                                       "([" & LNM0007INProw("MIDDLECTNCD") & "]" &
                                       "([" & LNM0007INProw("DEPSTATION") & "]" &
                                       "([" & LNM0007INProw("DEPTRUSTEECD") & "]" &
                                       " [" & LNM0007INProw("PRIORITYNO") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LineErr = "" Then
                If LNM0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0007INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0007INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0007tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0007tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0007row As DataRow In LNM0007tbl.Rows
            Select Case LNM0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0007INProw As DataRow In LNM0007INPtbl.Rows
            ' エラーレコード読み飛ばし
            If LNM0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0007INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0007row As DataRow In LNM0007tbl.Rows
                ' KEY項目が等しい時
                If LNM0007row("ORGCODE") = LNM0007INProw("ORGCODE") AndAlso                                '組織コード
                   LNM0007row("BIGCTNCD") = LNM0007INProw("BIGCTNCD") AndAlso                              '大分類コード
                   LNM0007row("MIDDLECTNCD") = LNM0007INProw("MIDDLECTNCD") AndAlso                        '中分類コード
                   LNM0007row("DEPSTATION") = LNM0007INProw("DEPSTATION") AndAlso                          '発駅コード
                   LNM0007row("DEPTRUSTEECD") = LNM0007INProw("DEPTRUSTEECD") AndAlso                      '発受託人コード
                   LNM0007row("PRIORITYNO") = LNM0007INProw("PRIORITYNO") Then                             '優先順位
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0007row("DELFLG") = LNM0007INProw("DELFLG") AndAlso                              '削除フラグ
                       LNM0007row("PURPOSE") = LNM0007INProw("PURPOSE") AndAlso                            '使用目的
                       LNM0007row("SMALLCTNCD") = LNM0007INProw("SMALLCTNCD") AndAlso                      '選択比較項目-小分類コード
                       LNM0007row("CTNTYPE") = LNM0007INProw("CTNTYPE") AndAlso                            '選択比較項目-コンテナ記号
                       LNM0007row("CTNSTNO") = LNM0007INProw("CTNSTNO") AndAlso                            '選択比較項目-コンテナ番号（開始）
                       LNM0007row("CTNENDNO") = LNM0007INProw("CTNENDNO") AndAlso                          '選択比較項目-コンテナ番号（終了）
                       LNM0007row("SLCSTACKFREEKBN") = LNM0007INProw("SLCSTACKFREEKBN") AndAlso            '選択比較項目-積空区分
                       LNM0007row("SLCSTATUSKBN") = LNM0007INProw("SLCSTATUSKBN") AndAlso                  '選択比較項目-状態区分
                       LNM0007row("SLCDEPTRUSTEESUBCD") = LNM0007INProw("SLCDEPTRUSTEESUBCD") AndAlso      '選択比較項目-発受託人サブコード
                       LNM0007row("SLCDEPSHIPPERCD") = LNM0007INProw("SLCDEPSHIPPERCD") AndAlso            '選択比較項目-発荷主コード
                       LNM0007row("SLCARRSTATION") = LNM0007INProw("SLCARRSTATION") AndAlso                '選択比較項目-着駅コード
                       LNM0007row("SLCARRTRUSTEECD") = LNM0007INProw("SLCARRTRUSTEECD") AndAlso            '選択比較項目-着受託人コード
                       LNM0007row("SLCARRTRUSTEESUBCD") = LNM0007INProw("SLCARRTRUSTEESUBCD") AndAlso      '選択比較項目-着受託人サブコード
                       LNM0007row("SLCJRITEMCD") = LNM0007INProw("SLCJRITEMCD") AndAlso                    '選択比較項目-ＪＲ品目コード
                       LNM0007row("SLCPICKUPTEL") = LNM0007INProw("SLCPICKUPTEL") AndAlso                  '選択比較項目-集荷先電話番号
                       LNM0007row("SPRDEPTRUSTEECD") = LNM0007INProw("SPRDEPTRUSTEECD") AndAlso            '特例置換項目-発受託人コード
                       LNM0007row("SPRDEPTRUSTEESUBCD") = LNM0007INProw("SPRDEPTRUSTEESUBCD") AndAlso      '特例置換項目-発受託人サブコード
                       LNM0007row("SPRDEPTRUSTEESUBZKBN") = LNM0007INProw("SPRDEPTRUSTEESUBZKBN") AndAlso  '特例置換項目-発受託人サブゼロ変換区分
                       LNM0007row("SPRDEPSHIPPERCD") = LNM0007INProw("SPRDEPSHIPPERCD") AndAlso            '特例置換項目-発荷主コード
                       LNM0007row("SPRARRTRUSTEECD") = LNM0007INProw("SPRARRTRUSTEECD") AndAlso            '特例置換項目-着受託人コード
                       LNM0007row("SPRARRTRUSTEESUBCD") = LNM0007INProw("SPRARRTRUSTEESUBCD") AndAlso      '特例置換項目-着受託人サブコード
                       LNM0007row("SPRARRTRUSTEESUBZKBN") = LNM0007INProw("SPRARRTRUSTEESUBZKBN") AndAlso  '特例置換項目-着受託人サブゼロ変換区分
                       LNM0007row("SPRJRITEMCD") = LNM0007INProw("SPRJRITEMCD") AndAlso                    '特例置換項目-ＪＲ品目コード
                       LNM0007row("SPRSTACKFREEKBN") = LNM0007INProw("SPRSTACKFREEKBN") AndAlso            '特例置換項目-積空区分
                       LNM0007row("SPRSTATUSKBN") = LNM0007INProw("SPRSTATUSKBN") AndAlso                  '特例置換項目-状態区分
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0007row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0007INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        ' 更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0007INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0007INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0007INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now

                '変更チェック
                RECT1MEXISTS(SQLcon, WW_MODIFYKBN)
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
        For Each LNM0007INProw As DataRow In LNM0007INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0007row As DataRow In LNM0007tbl.Rows
                ' 同一レコードか判定
                If LNM0007INProw("ORGCODE") = LNM0007row("ORGCODE") AndAlso                                '組織コード
                   LNM0007INProw("BIGCTNCD") = LNM0007row("BIGCTNCD") AndAlso                              '大分類コード
                   LNM0007INProw("MIDDLECTNCD") = LNM0007row("MIDDLECTNCD") AndAlso                        '中分類コード
                   LNM0007INProw("DEPSTATION") = LNM0007row("DEPSTATION") AndAlso                          '発駅コード
                   LNM0007INProw("DEPTRUSTEECD") = LNM0007row("DEPTRUSTEECD") AndAlso                      '発受託人コード
                   LNM0007INProw("PRIORITYNO") = LNM0007row("PRIORITYNO") Then                             '優先順位
                    ' 画面入力テーブル項目設定
                    LNM0007INProw("LINECNT") = LNM0007row("LINECNT")
                    LNM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0007INProw("UPDTIMSTP") = LNM0007row("UPDTIMSTP")
                    LNM0007INProw("SELECT") = 0
                    LNM0007INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0007row.ItemArray = LNM0007INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0007tbl.NewRow
                WW_NRow.ItemArray = LNM0007INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0007tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0007tbl.Rows.Add(WW_NRow)
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

        Try
            Select Case I_FIELD
                Case "ORG"                '組織コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "BIGCTNCD"           '大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS))
                Case "MIDDLECTNCD"        '中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, TxtBigCTNCD.Text))
                Case "STATION"            '発駅コード・着駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "DEPTRUSTEECD"       '発受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtDepStation.Text))
                Case "SMALLCTNCD"         '小分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.SMALL_CLASS, TxtBigCTNCD.Text, TxtMiddleCTNCD.Text))
                Case "CTNTYPE"            'コンテナ記号
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_TYPE))
                Case "CTNNO"              'コンテナ番号（開始/終了）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, TxtCTNType.Text))
                Case "STACKFREEKBN",      '積空区分
                     "OPERATIONKBN"       '状態区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "DEPTRUSTEESUBCD"    '発受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtDepStation.Text, TxtDepTrusteeCd.Text))
                Case "SHIPPER"            '荷主コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SHIPPER, I_VALUE, O_TEXT, O_RTN)
                Case "ARRTRUSTEECD"       '着受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtSlcArrStation.Text))
                Case "ARRTRUSTEESUBCD"    '着受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtSlcArrStation.Text, TxtSlcArrTrusteeCd.Text))
                Case "ITEM"               '品目コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ITEM, I_VALUE, O_TEXT, O_RTN)

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
