''************************************************************
' 品目マスタメンテ登録画面
' 作成日 2022/03/07
' 更新日 
' 作成者 名取
' 更新者 
'
' 修正履歴:2022/03/07 新規作成
'         :
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 組織マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0021ItemDetail
    Inherits Page

    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private LNM0021tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0021INPtbl As DataTable                              'チェック用テーブル
    Private LNM0021UPDtbl As DataTable                              '更新用テーブル

    '○ データOPERATION用
    Private Const CONST_INSERT As String = "Insert"                 'データ追加
    Private Const CONST_UPDATE As String = "Update"                 'データ更新
    Private Const CONST_PATTERNERR As String = "PATTEN ERR"         '関連チェックエラー

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
                    Master.RecoverTable(LNM0021tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_UPDATE"                '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_CLEAR"                 '戻るボタン押下
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
                        Case "btnClearConfirmOk"        '戻るボタン押下後の確認ダイアログでOK押下
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
            If Not IsNothing(LNM0021tbl) Then
                LNM0021tbl.Clear()
                LNM0021tbl.Dispose()
                LNM0021tbl = Nothing
            End If

            If Not IsNothing(LNM0021INPtbl) Then
                LNM0021INPtbl.Clear()
                LNM0021INPtbl.Dispose()
                LNM0021INPtbl = Nothing
            End If

            If Not IsNothing(LNM0021UPDtbl) Then
                LNM0021UPDtbl.Clear()
                LNM0021UPDtbl.Dispose()
                LNM0021UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = LNM0021WRKINC.MAPIDD
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True

        '○初期値設定
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        rightview.ResetIndex()
        leftview.ActiveListBox()

        '右Boxへの値設定
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
        '選択行
        LblSelLineCNT.Text = work.WF_SEL_LINECNT.Text
        '削除フラグ
        TxtDelFlg.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_RtnSW)
        '品目コード
        TxtItemCd.Text = work.WF_SEL_ITEMCD2.Text
        '品目名
        TxtName.Text = work.WF_SEL_NAME.Text
        '品目名（短）
        TxtNames.Text = work.WF_SEL_NAMES.Text
        '品目名カナ
        TxtNameKana.Text = work.WF_SEL_NAMEKANA.Text
        '品目名カナ（短）
        TxtNameKanas.Text = work.WF_SEL_NAMEKANAS.Text
        '特大分類コード
        TxtSpBigCategCd.Text = work.WF_SEL_SPBIGCATEGCD.Text
        CODENAME_get("SPBIGCATEGCD", TxtSpBigCategCd.Text, LblSpBigCategCdName.Text, WW_RtnSW)
        '大分類コード
        TxtBigCategCd.Text = work.WF_SEL_BIGCATEGCD.Text
        CODENAME_get("BIGCATEGCD", TxtBigCategCd.Text, LblBigCategCdName.Text, WW_RtnSW)
        '中分類コード
        TxtMiddleCategCd.Text = work.WF_SEL_MIDDLECATEGCD.Text
        CODENAME_get("MIDDLECATEGCD", TxtMiddleCategCd.Text, LblMiddleCategCdName.Text, WW_RtnSW)
        '小分類コード
        TxtSmallCategCd.Text = work.WF_SEL_SMALLCATEGCD.Text
        CODENAME_get("SMALLCATEGCD", TxtSmallCategCd.Text, LblSmallCategCdName.Text, WW_RtnSW)
        '危険品区分
        TxtDangerKbn.Text = work.WF_SEL_DANGERKBN.Text
        CODENAME_get("DANGERKBN", TxtDangerKbn.Text, LblDangerKbnName.Text, WW_RtnSW)
        '軽量品区分
        TxtLightWtKbn.Text = work.WF_SEL_LIGHTWTKBN.Text
        CODENAME_get("LIGHTWTKBN", TxtLightWtKbn.Text, LblLightWtKbnName.Text, WW_RtnSW)
        '貴重品区分
        TxtValuableKbn.Text = work.WF_SEL_VALUABLEKBN.Text
        CODENAME_get("VALUABLEKBN", TxtValuableKbn.Text, LblValuableKbnName.Text, WW_RtnSW)
        '冷蔵適合フラグ
        TxtRefrigerationFlg.Text = work.WF_SEL_REFRIGERATIONFLG.Text
        CODENAME_get("REFRIGERATIONFLG", TxtRefrigerationFlg.Text, LblRefrigerationFlgName.Text, WW_RtnSW)
        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_ITEMCD2.Text

        ' テキストボックスは数値(0～9)のみ可能とする。
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"            '削除フラグ
        Me.TxtItemCd.Attributes("onkeyPress") = "CheckNum()"            '品目コード
        Me.TxtSpBigCategCd.Attributes("onkeyPress") = "CheckNum()"      '特大分類コード
        Me.TxtBigCategCd.Attributes("onkeyPress") = "CheckNum()"        '大分類コード
        Me.TxtMiddleCategCd.Attributes("onkeyPress") = "CheckNum()"     '中分類コード
        Me.TxtSmallCategCd.Attributes("onkeyPress") = "CheckNum()"      '小分類コード
        Me.TxtDangerKbn.Attributes("onkeyPress") = "CheckNum()"         '危険品区分
        Me.TxtLightWtKbn.Attributes("onkeyPress") = "CheckNum()"        '軽量品区分
        Me.TxtValuableKbn.Attributes("onkeyPress") = "CheckNum()"       '貴重品区分
        Me.TxtRefrigerationFlg.Attributes("onkeyPress") = "CheckNum()"  '冷蔵適合フラグ

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO"></param>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                " _
            & "     ITEMCD            " _
            & " FROM                  " _
            & "     LNG.LNM0021_ITEM  " _
            & " WHERE                 " _
            & "         ITEMCD  = @P1 " _
            & "     AND DELFLG <> @P2 "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 6) '品目コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 1) '削除フラグ

                PARA1.Value = TxtItemCd.Text
                PARA2.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0021Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0021Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0021Chk.Load(SQLdr)

                    If LNM0021Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0021C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0021C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 品目マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = C_MESSAGE_NO.NORMAL

        '○ ＤＢ更新
        'Dim SQLStr As String =
        '      " DECLARE @hensuu AS bigint ;                 " _
        '    & "     SET @hensuu = 0 ;                       " _
        '    & " DECLARE hensuu CURSOR FOR                   " _
        '    & "     SELECT                                  " _
        '    & "         UPDTIMSTP AS hensuu                 " _
        '    & "     FROM                                    " _
        '    & "         LNG.LNM0021_ITEM                    " _
        '    & "     WHERE                                   " _
        '    & "         ITEMCD = @P01 ;                     " _
        '    & " OPEN hensuu ;                               " _
        '    & " FETCH NEXT FROM hensuu INTO @hensuu ;       " _
        '    & " IF (@@FETCH_STATUS = 0)                     " _
        '    & "     UPDATE LNG.LNM0021_ITEM                 " _
        '    & "     SET                                     " _
        '    & "         DELFLG            = @P00            " _
        '    & "       , NAME              = @P02            " _
        '    & "       , NAMES             = @P03            " _
        '    & "       , NAMEKANA          = @P04            " _
        '    & "       , NAMEKANAS         = @P05            " _
        '    & "       , SPBIGCATEGCD      = @P06            " _
        '    & "       , BIGCATEGCD        = @P07            " _
        '    & "       , MIDDLECATEGCD     = @P08            " _
        '    & "       , SMALLCATEGCD      = @P09            " _
        '    & "       , DANGERKBN         = @P10            " _
        '    & "       , LIGHTWTKBN        = @P11            " _
        '    & "       , VALUABLEKBN       = @P12            " _
        '    & "       , REFRIGERATIONFLG  = @P13            " _
        '    & "       , UPDYMD            = @P19            " _
        '    & "       , UPDUSER           = @P20            " _
        '    & "       , UPDTERMID         = @P21            " _
        '    & "       , UPDPGID           = @P22            " _
        '    & "       , RECEIVEYMD        = @P23            " _
        '    & "     WHERE                                   " _
        '    & "        ITEMCD = @P01 ;                      " _
        '    & " IF (@@FETCH_STATUS <> 0)                    " _
        '    & "     INSERT INTO LNG.LNM0021_ITEM            " _
        '    & "        (DELFLG                              " _
        '    & "       , ITEMCD                              " _
        '    & "       , NAME                                " _
        '    & "       , NAMES                               " _
        '    & "       , NAMEKANA                            " _
        '    & "       , NAMEKANAS                           " _
        '    & "       , SPBIGCATEGCD                        " _
        '    & "       , BIGCATEGCD                          " _
        '    & "       , MIDDLECATEGCD                       " _
        '    & "       , SMALLCATEGCD                        " _
        '    & "       , DANGERKBN                           " _
        '    & "       , LIGHTWTKBN                          " _
        '    & "       , VALUABLEKBN                         " _
        '    & "       , REFRIGERATIONFLG                    " _
        '    & "       , INITYMD                             " _
        '    & "       , INITUSER                            " _
        '    & "       , INITTERMID                          " _
        '    & "       , INITPGID                            " _
        '    & "       , UPDYMD                              " _
        '    & "       , UPDUSER                             " _
        '    & "       , UPDTERMID                           " _
        '    & "       , UPDPGID                             " _
        '    & "       , RECEIVEYMD)                         " _
        '    & "     VALUES                                  " _
        '    & "        (@P00                                " _
        '    & "       , @P01                                " _
        '    & "       , @P02                                " _
        '    & "       , @P03                                " _
        '    & "       , @P04                                " _
        '    & "       , @P05                                " _
        '    & "       , @P06                                " _
        '    & "       , @P07                                " _
        '    & "       , @P08                                " _
        '    & "       , @P09                                " _
        '    & "       , @P10                                " _
        '    & "       , @P11                                " _
        '    & "       , @P12                                " _
        '    & "       , @P13                                " _
        '    & "       , @P15                                " _
        '    & "       , @P16                                " _
        '    & "       , @P17                                " _
        '    & "       , @P18                                " _
        '    & "       , @P19                                " _
        '    & "       , @P20                                " _
        '    & "       , @P21                                " _
        '    & "       , @P22                                " _
        '    & "       , @P23) ;                             " _
        '    & " CLOSE hensuu ;                              " _
        '    & " DEALLOCATE hensuu ;                         "
        Dim SQLStr As String =
              "     INSERT INTO LNG.LNM0021_ITEM            " _
            & "        (DELFLG                              " _
            & "       , ITEMCD                              " _
            & "       , NAME                                " _
            & "       , NAMES                               " _
            & "       , NAMEKANA                            " _
            & "       , NAMEKANAS                           " _
            & "       , SPBIGCATEGCD                        " _
            & "       , BIGCATEGCD                          " _
            & "       , MIDDLECATEGCD                       " _
            & "       , SMALLCATEGCD                        " _
            & "       , DANGERKBN                           " _
            & "       , LIGHTWTKBN                          " _
            & "       , VALUABLEKBN                         " _
            & "       , REFRIGERATIONFLG                    " _
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
            & "       , @P15                                " _
            & "       , @P16                                " _
            & "       , @P17                                " _
            & "       , @P18                                " _
            & "       , @P19                                " _
            & "       , @P20                                " _
            & "       , @P21                                " _
            & "       , @P22                                " _
            & "       , @P23)                               " _
            & "     ON DUPLICATE KEY UPDATE                 " _
            & "         DELFLG            = @P00            " _
            & "       , NAME              = @P02            " _
            & "       , NAMES             = @P03            " _
            & "       , NAMEKANA          = @P04            " _
            & "       , NAMEKANAS         = @P05            " _
            & "       , SPBIGCATEGCD      = @P06            " _
            & "       , BIGCATEGCD        = @P07            " _
            & "       , MIDDLECATEGCD     = @P08            " _
            & "       , SMALLCATEGCD      = @P09            " _
            & "       , DANGERKBN         = @P10            " _
            & "       , LIGHTWTKBN        = @P11            " _
            & "       , VALUABLEKBN       = @P12            " _
            & "       , REFRIGERATIONFLG  = @P13            " _
            & "       , UPDYMD            = @P19            " _
            & "       , UPDUSER           = @P20            " _
            & "       , UPDTERMID         = @P21            " _
            & "       , UPDPGID           = @P22            " _
            & "       , RECEIVEYMD        = @P23            " _

        '○ 更新ジャーナル出力
        Dim SQLJnl As String =
              " SELECT                                  " _
            & "    DELFLG                               " _
            & "  , ITEMCD                               " _
            & "  , NAME                                 " _
            & "  , NAMES                                " _
            & "  , NAMEKANA                             " _
            & "  , NAMEKANAS                            " _
            & "  , SPBIGCATEGCD                         " _
            & "  , BIGCATEGCD                           " _
            & "  , MIDDLECATEGCD                        " _
            & "  , SMALLCATEGCD                         " _
            & "  , DANGERKBN                            " _
            & "  , LIGHTWTKBN                           " _
            & "  , VALUABLEKBN                          " _
            & "  , REFRIGERATIONFLG                     " _
            & "  , INITYMD                              " _
            & "  , INITUSER                             " _
            & "  , INITTERMID                           " _
            & "  , INITPGID                             " _
            & "  , UPDYMD                               " _
            & "  , UPDUSER                              " _
            & "  , UPDTERMID                            " _
            & "  , UPDPGID                              " _
            & "  , RECEIVEYMD                           " _
            & "  , UPDTIMSTP                            " _
            & " FROM                                    " _
            & "    LNG.LNM0021_ITEM                     " _
            & " WHERE                                   " _
            & "        ITEMCD = @P01                    "
        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                Dim PARA00 As MySqlParameter = SQLcmd.Parameters.Add("@P00", MySqlDbType.VarChar, 1)          '削除フラグ
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.Int32, 6)               '品目コード
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 50)         '品目名称
                Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 20)         '品目名称(短)
                Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar, 50)         '品目カナ名称
                Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar, 20)         '品目カナ名称(短)
                Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar, 2)          '特大分類コード
                Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar, 2)          '大分類コード
                Dim PARA08 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.VarChar, 2)          '中大分類コード
                Dim PARA09 As MySqlParameter = SQLcmd.Parameters.Add("@P09", MySqlDbType.VarChar, 2)          '小大分類コード
                Dim PARA10 As MySqlParameter = SQLcmd.Parameters.Add("@P10", MySqlDbType.VarChar, 1)          '危険品区分
                Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.VarChar, 1)          '軽量品区分
                Dim PARA12 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.VarChar, 1)          '貴重品区分
                Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar, 1)          '冷蔵適合フラグ
                Dim PARA15 As MySqlParameter = SQLcmd.Parameters.Add("@P15", MySqlDbType.DateTime)             '登録年月日
                Dim PARA16 As MySqlParameter = SQLcmd.Parameters.Add("@P16", MySqlDbType.VarChar, 20)         '登録ユーザーID
                Dim PARA17 As MySqlParameter = SQLcmd.Parameters.Add("@P17", MySqlDbType.VarChar, 20)         '登録端末
                Dim PARA18 As MySqlParameter = SQLcmd.Parameters.Add("@P18", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ
                Dim PARA19 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.DateTime)             '更新年月日
                Dim PARA20 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.VarChar, 20)         '更新ユーザーID
                Dim PARA21 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.VarChar, 20)         '更新端末
                Dim PARA22 As MySqlParameter = SQLcmd.Parameters.Add("@P22", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ
                Dim PARA23 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.DateTime)             '集信日時

                Dim JPARA1 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.Int32, 6)             '品目コード

                Dim LNM0021row As DataRow = LNM0021INPtbl.Rows(0)

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA00.Value = LNM0021row("DELFLG")                          '削除フラグ
                PARA01.Value = LNM0021row("ITEMCD")                          '品目コード
                PARA02.Value = LNM0021row("NAME")                            '品目名称
                PARA03.Value = LNM0021row("NAMES")                           '品目名称(短)
                PARA04.Value = LNM0021row("NAMEKANA")                        '品目カナ名称
                PARA05.Value = LNM0021row("NAMEKANAS")                       '品目カナ名称(短)
                PARA06.Value = LNM0021row("SPBIGCATEGCD")                    '特大分類コード
                PARA07.Value = LNM0021row("BIGCATEGCD")                      '大分類コード
                PARA08.Value = LNM0021row("MIDDLECATEGCD")                   '中大分類コード
                PARA09.Value = LNM0021row("SMALLCATEGCD")                    '小大分類コード
                PARA10.Value = LNM0021row("DANGERKBN")                       '危険品区分
                PARA11.Value = LNM0021row("LIGHTWTKBN")                      '軽量品区分
                PARA12.Value = LNM0021row("VALUABLEKBN")                     '貴重品区分
                If String.IsNullOrEmpty(LNM0021row("REFRIGERATIONFLG")) Then '冷蔵適合フラグ
                    PARA13.Value = 0
                Else
                    PARA13.Value = LNM0021row("REFRIGERATIONFLG")
                End If
                PARA15.Value = WW_DateNow                                    '登録年月日
                PARA16.Value = Master.USERID                                 '登録ユーザーＩＤ
                PARA17.Value = Master.USERTERMID                             '登録端末
                PARA18.Value = Me.GetType().BaseType.Name                    '登録プログラムＩＤ
                PARA19.Value = WW_DateNow                                    '更新年月日
                PARA20.Value = Master.USERID                                 '更新ユーザーＩＤ
                PARA21.Value = Master.USERTERMID                             '更新端末
                PARA22.Value = Me.GetType().BaseType.Name                    '更新プログラムＩＤ
                PARA23.Value = C_DEFAULT_YMD                                 '集信日時

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA1.Value = LNM0021row("ITEMCD")

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0021UPDtbl) Then
                        LNM0021UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0021UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0021UPDtbl.Clear()
                    LNM0021UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0021UPDrow As DataRow In LNM0021UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0021C"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0021UPDrow
                    CS0020JOURNAL.CS0020JOURNAL()
                    If Not isNormal(CS0020JOURNAL.ERR) Then
                        Master.Output(CS0020JOURNAL.ERR, C_MESSAGE_TYPE.ABORT, "CS0020JOURNAL JOURNAL")

                        CS0011LOGWrite.INFSUBCLASS = "MAIN"                     'SUBクラス名
                        CS0011LOGWrite.INFPOSI = "CS0020JOURNAL JOURNAL"
                        CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
                        CS0011LOGWrite.TEXT = "CS0020JOURNAL Call Err!"
                        CS0011LOGWrite.MESSAGENO = CS0020JOURNAL.ERR
                        CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

                        rightview.AddErrorReport("DB更新ジャーナル出力エラーが発生しました。システム管理者にお問い合わせ下さい。")
                        WW_ErrSW = CS0020JOURNAL.ERR
                        Exit Sub
                    End If
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0021L UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0021L UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-表更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0021INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0021tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0021tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        ' 右BOXクローズ
        WF_RightboxOpen.Value = ""
        If String.IsNullOrEmpty(WW_ErrSW) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ErrSW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            ElseIf WW_ErrSW = C_MESSAGE_NO.PRIMARYKEY_REPEAT_ERROR Then
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, "品目", needsPopUp:=True)
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
            Master.TransitionPrevPage(Master.USERCAMP)
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToLNM0021INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)            '削除
        Master.EraseCharToIgnore(TxtItemCd.Text)            '削除
        Master.EraseCharToIgnore(TxtName.Text)              '品目名称
        Master.EraseCharToIgnore(TxtNames.Text)             '品目名称（短）
        Master.EraseCharToIgnore(TxtNameKana.Text)          '品目カナ名称
        Master.EraseCharToIgnore(TxtNameKanas.Text)         '品目カナ名称（短）
        Master.EraseCharToIgnore(TxtSpBigCategCd.Text)      '特大分類コード
        Master.EraseCharToIgnore(TxtBigCategCd.Text)        '大分類コード
        Master.EraseCharToIgnore(TxtMiddleCategCd.Text)     '中大分類コード
        Master.EraseCharToIgnore(TxtSmallCategCd.Text)      '小大分類コード
        Master.EraseCharToIgnore(TxtDangerKbn.Text)         '危険品区分
        Master.EraseCharToIgnore(TxtLightWtKbn.Text)        '軽量品区分
        Master.EraseCharToIgnore(TxtValuableKbn.Text)       '貴重品区分
        Master.EraseCharToIgnore(TxtRefrigerationFlg.Text)  '冷蔵適合フラグ

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(LblSelLineCNT.Text) AndAlso
            String.IsNullOrEmpty(TxtDelFlg.Text) Then
            Master.Output(C_MESSAGE_NO.INVALID_PROCCESS_ERROR, C_MESSAGE_TYPE.ERR, "no Detail", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "DetailBoxToINPtbl"        'SUBクラス名
            CS0011LOGWrite.INFPOSI = "non Detail"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ERR
            CS0011LOGWrite.TEXT = "non Detail"
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                         'ログ出力

            O_RTN = C_MESSAGE_NO.INVALID_PROCCESS_ERROR
            Exit Sub
        End If

        Master.CreateEmptyTable(LNM0021INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0021INProw As DataRow = LNM0021INPtbl.NewRow

        ' LINECNT
        If String.IsNullOrEmpty(LblSelLineCNT.Text) Then
            LNM0021INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(LblSelLineCNT.Text, LNM0021INProw("LINECNT"))
            Catch ex As Exception
                LNM0021INProw("LINECNT") = 0
            End Try
        End If

        LNM0021INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0021INProw("UPDTIMSTP") = 0
        LNM0021INProw("SELECT") = 1
        LNM0021INProw("HIDDEN") = 0

        LNM0021INProw("ITEMCD") = TxtItemCd.Text                      '品目コード
        LNM0021INProw("NAME") = TxtName.Text                          '品目名
        LNM0021INProw("NAMES") = TxtNames.Text                        '品目名（短）
        LNM0021INProw("NAMEKANA") = TxtNameKana.Text                  '品目名カナ
        LNM0021INProw("NAMEKANAS") = TxtNameKanas.Text                '品目名カナ（短）
        LNM0021INProw("SPBIGCATEGCD") = TxtSpBigCategCd.Text          '特大分類コード
        LNM0021INProw("BIGCATEGCD") = TxtBigCategCd.Text              '大分類コード
        LNM0021INProw("MIDDLECATEGCD") = TxtMiddleCategCd.Text        '中大分類コード
        LNM0021INProw("SMALLCATEGCD") = TxtSmallCategCd.Text          '小大分類コード
        LNM0021INProw("DANGERKBN") = TxtDangerKbn.Text                '危険品区分
        LNM0021INProw("LIGHTWTKBN") = TxtLightWtKbn.Text              '軽量品区分
        LNM0021INProw("VALUABLEKBN") = TxtValuableKbn.Text            '貴重品区分
        LNM0021INProw("REFRIGERATIONFLG") = TxtRefrigerationFlg.Text  '冷蔵適合フラグ
        LNM0021INProw("DELFLG") = TxtDelFlg.Text                      '削除

        '○ チェック用テーブルに登録する
        LNM0021INPtbl.Rows.Add(LNM0021INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()

        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0021INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim inputChangeFlg As Boolean = True
        Dim LNM0021INProw As DataRow = LNM0021INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0021row As DataRow In LNM0021tbl.Rows
            ' KEY項目が等しい時
            If LNM0021row("ITEMCD") = LNM0021INProw("ITEMCD") Then                           '品目コード
                ' KEY項目以外の項目の差異をチェック                                          
                If LNM0021row("DELFLG") = LNM0021INProw("DELFLG") AndAlso                    '削除フラグ
                    LNM0021row("NAME") = LNM0021INProw("NAME") AndAlso                       '品目名称
                    LNM0021row("NAMES") = LNM0021INProw("NAMES") AndAlso                     '品目名称(短)
                    LNM0021row("NAMEKANA") = LNM0021INProw("NAMEKANA") AndAlso               '品目カナ名称
                    LNM0021row("NAMEKANAS") = LNM0021INProw("NAMEKANAS") AndAlso             '品目カナ名称(短)
                    LNM0021row("SPBIGCATEGCD") = LNM0021INProw("SPBIGCATEGCD") AndAlso       '特大分類コード
                    LNM0021row("BIGCATEGCD") = LNM0021INProw("BIGCATEGCD") AndAlso           '大分類コード
                    LNM0021row("MIDDLECATEGCD") = LNM0021INProw("MIDDLECATEGCD") AndAlso     '中大分類コード
                    LNM0021row("SMALLCATEGCD") = LNM0021INProw("SMALLCATEGCD") AndAlso       '小大分類コード
                    LNM0021row("DANGERKBN") = LNM0021INProw("DANGERKBN") AndAlso             '危険品区分
                    LNM0021row("LIGHTWTKBN") = LNM0021INProw("LIGHTWTKBN") AndAlso           '軽量品区分
                    LNM0021row("VALUABLEKBN") = LNM0021INProw("VALUABLEKBN") AndAlso         '貴重品区分
                    LNM0021row("REFRIGERATIONFLG") = LNM0021INProw("REFRIGERATIONFLG") Then  '冷蔵適合フラグ
                    ' 変更がない場合、入力変更フラグをOFFにする
                    inputChangeFlg = False
                End If

                Exit For
            End If
        Next

        If inputChangeFlg Then
            ' 変更がある場合は、確認ダイアログを表示
            Master.Output(C_MESSAGE_NO.UPDATE_CANCEL_CONFIRM, C_MESSAGE_TYPE.QUES, I_PARA02:="W",
                needsPopUp:=True, messageBoxTitle:="確認", IsConfirm:=True, YesButtonId:="btnClearConfirmOk")
        Else
            ' 変更がない場合は、確認ダイアログを表示せずに、前画面に戻る
            WF_CLEAR_ConfirmOkClick()
        End If

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時、確認ダイアログOKボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_ConfirmOkClick()

        '○ 詳細画面初期化
        DetailBoxClear()

        '○ メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_CLEAR_SUCCESSFUL, C_MESSAGE_TYPE.INF)

        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""

        Master.TransitionPrevPage(Master.USERCAMP)

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each LNM0021row As DataRow In LNM0021tbl.Rows
            Select Case LNM0021row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0021row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0021row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0021row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0021row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0021row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0021tbl, work.WF_SEL_INPTBL.Text)

        LblSelLineCNT.Text = ""            'LINECNT
        TxtItemCd.Text = ""                 '品目コード
        TxtName.Text = ""                   '品目名
        TxtNames.Text = ""                  '品目名（短）
        TxtNameKana.Text = ""               '品目名カナ
        TxtNameKanas.Text = ""              '品目名カナ（短）
        TxtSpBigCategCd.Text = ""           '特大分類コード
        TxtBigCategCd.Text = ""             '大分類コード
        TxtMiddleCategCd.Text = ""          '中大分類コード
        TxtSmallCategCd.Text = ""           '小大分類コード
        TxtDangerKbn.Text = ""              '危険品区分
        TxtLightWtKbn.Text = ""             '軽量品区分
        TxtValuableKbn.Text = ""            '貴重品区分
        TxtRefrigerationFlg.Text = ""       '冷蔵適合フラグ
        TxtDelFlg.Text = ""                 '削除

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    Protected Sub WF_FIELD_DBClick()

        Dim WW_PrmData As New Hashtable

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Select Case WF_FIELD.Value
                    Case "TxtDelFlg"           '削除フラグ
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                    Case "TxtSpBigCategCd"     '特大分類コード
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "SPBIGCATEGCD")
                    Case "TxtBigCategCd"       '大分類コード
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "BIGCATEGCD")
                    Case "TxtMiddleCategCd"    '中分類コード
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "MIDDLECATEGCD")
                    Case "TxtSmallCategCd"     '小分類コード
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "SMALLCATEGCD")
                    Case "TxtDangerKbn"        '危険品区分
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "DANGERKBN")
                    Case "TxtLightWtKbn"       '軽量品区分
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "LIGHTWTKBN")
                    Case "TxtValuableKbn"      '貴重品区分
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "VALUABLEKBN")
                    Case "TxtRefrigerationFlg"  '冷蔵適合フラグ
                        WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "REFRIGERATIONFLG")
                End Select
                .SetListBox(WF_LeftMViewChange.Value, WW_Dummy, WW_PrmData)
                .ActiveListBox()

            End With
        End If

    End Sub

    ''' <summary>
    ''' フィールドチェンジ時処理
    ''' </summary>
    Protected Sub WF_FIELD_Change()
        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value
            Case "TxtDelFlg"            '削除フラグ
                CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_RtnSW)
                TxtDelFlg.Focus()
            Case "TxtSpBigCategCd"      '特大分類コード
                CODENAME_get("SPBIGCATEGCD", TxtSpBigCategCd.Text, LblSpBigCategCdName.Text, WW_RtnSW)
                TxtSpBigCategCd.Focus()
            Case "TxtBigCategCd"        '大分類コード
                CODENAME_get("BIGCATEGCD", TxtBigCategCd.Text, LblBigCategCdName.Text, WW_RtnSW)
                TxtBigCategCd.Focus()
            Case "TxtMiddleCategCd"     '中分類コード
                CODENAME_get("MIDDLECATEGCD", TxtMiddleCategCd.Text, LblMiddleCategCdName.Text, WW_RtnSW)
                TxtMiddleCategCd.Focus()
            Case "TxtSmallCategCd"      '小分類コード
                CODENAME_get("SMALLCATEGCD", TxtSmallCategCd.Text, LblSmallCategCdName.Text, WW_RtnSW)
                TxtSmallCategCd.Focus()
            Case "TxtDangerKbn"         '危険品区分
                CODENAME_get("DANGERKBN", TxtDangerKbn.Text, LblDangerKbnName.Text, WW_RtnSW)
                TxtDangerKbn.Focus()
            Case "TxtLightWtKbn"        '軽量品区分
                CODENAME_get("LIGHTWTKBN", TxtLightWtKbn.Text, LblLightWtKbnName.Text, WW_RtnSW)
                TxtLightWtKbn.Focus()
            Case "TxtValuableKbn"       '貴重品区分
                CODENAME_get("VALUABLEKBN", TxtValuableKbn.Text, LblValuableKbnName.Text, WW_RtnSW)
                TxtValuableKbn.Focus()
            Case "TxtRefrigerationFlg"  '冷蔵適合フラグ
                CODENAME_get("REFRIGERATIONFLG", TxtRefrigerationFlg.Text, LblRefrigerationFlgName.Text, WW_RtnSW)
                TxtRefrigerationFlg.Focus()

        End Select

    End Sub

    ' ******************************************************************************
    ' ***  leftBOX関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
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
                Case "TxtDelFlg"                               '削除フラグ
                    TxtDelFlg.Text = WW_SelectValue
                    LblDelFlgName.Text = WW_SelectText
                    TxtDelFlg.Focus()
                Case "TxtSpBigCategCd"                         '特大分類コード
                    TxtSpBigCategCd.Text = WW_SelectValue
                    LblSpBigCategCdName.Text = WW_SelectText
                    TxtSpBigCategCd.Focus()
                Case "TxtBigCategCd"                           '大分類コード
                    TxtBigCategCd.Text = WW_SelectValue
                    LblBigCategCdName.Text = WW_SelectText
                    TxtBigCategCd.Focus()
                Case "TxtMiddleCategCd"                        '中分類コード
                    TxtMiddleCategCd.Text = WW_SelectValue
                    LblMiddleCategCdName.Text = WW_SelectText
                    TxtMiddleCategCd.Focus()
                Case "TxtSmallCategCd"                         '小分類コード
                    TxtSmallCategCd.Text = WW_SelectValue
                    LblSmallCategCdName.Text = WW_SelectText
                    TxtSmallCategCd.Focus()
                Case "TxtDangerKbn"                            '危険品区分
                    TxtDangerKbn.Text = WW_SelectValue
                    LblDangerKbnName.Text = WW_SelectText
                    TxtDangerKbn.Focus()
                Case "TxtLightWtKbn"                           '軽量品区分
                    TxtLightWtKbn.Text = WW_SelectValue
                    LblLightWtKbnName.Text = WW_SelectText
                    TxtLightWtKbn.Focus()
                Case "TxtValuableKbn"                          '貴重品区分
                    TxtValuableKbn.Text = WW_SelectValue
                    LblValuableKbnName.Text = WW_SelectText
                    TxtValuableKbn.Focus()
                Case "TxtRefrigerationFlg"                     '冷蔵適合フラグ
                    TxtRefrigerationFlg.Text = WW_SelectValue
                    LblRefrigerationFlgName.Text = WW_SelectText
                    TxtRefrigerationFlg.Focus()
            End Select
        Else
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
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        If String.IsNullOrEmpty(WF_FIELD_REP.Value) Then
            Select Case WF_FIELD.Value
                Case "TxtDelFlg"             '削除フラグ
                    TxtDelFlg.Focus()
                Case "TxtSpBigCategCd"       '特大分類コード
                    TxtSpBigCategCd.Focus()
                Case "TxtBigCategCd"         '大分類コード
                    TxtBigCategCd.Focus()
                Case "TxtMiddleCategCd"      '中分類コード
                    TxtMiddleCategCd.Focus()
                Case "TxtSmallCategCd"       '小分類コード
                    TxtSmallCategCd.Focus()
                Case "TxtDangerKbn"          '危険品区分
                    TxtDangerKbn.Focus()
                Case "TxtLightWtKbn"         '軽量品区分
                    TxtLightWtKbn.Focus()
                Case "TxtValuableKbn"        '貴重品区分
                    TxtValuableKbn.Focus()
                Case "TxtRefrigerationFlg"   '冷蔵適合フラグ
                    TxtRefrigerationFlg.Focus()
            End Select
        Else
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
    Protected Sub INPTableCheck(ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LineErr As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""
        Dim WW_DBDataCheck As String = ""

        '○ 画面操作権限チェック
        ' 権限チェック(操作者がデータ内USERの更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            WW_CheckMES1 = "・品目マスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0021INProw As DataRow In LNM0021INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0021INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0021INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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
            ' 品目コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ITEMCD", LNM0021INProw("ITEMCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・品目コードエラーです"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 品目名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "NAME", LNM0021INProw("NAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・品目名称入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 品目名称（短）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "NAMES", LNM0021INProw("NAMES"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・品目名称（短）入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 品目カナ名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "NAMEKANA", LNM0021INProw("NAMEKANA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・品目カナ名称入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 品目カナ名称（短）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "NAMEKANAS", LNM0021INProw("NAMEKANAS"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・品目カナ名称（短）入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特大分類コード(バリデーションチェック）
            'Master.CheckField(Master.USERCAMP, "SPBIGCATEGCD", LNM0021INProw("SPBIGCATEGCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If isNormal(WW_CS0024FCheckerr) Then
            '    If String.IsNullOrEmpty(LNM0021INProw("SPBIGCATEGCD")) Then
            '        ' 名称存在チェック
            '        CODENAME_get("SPBIGCATEGCD", LNM0021INProw("SPBIGCATEGCD"), WW_Dummy, WW_RtnSW)
            '        If Not isNormal(WW_RtnSW) Then
            '            WW_CheckMES1 = "・特大分類コードエラーです。"
            '            WW_CheckMES2 = "マスタに存在しません。"
            '            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '            WW_LineErr = "ERR"
            '            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '        End If
            '    End If
            'Else
            '    WW_CheckMES1 = "・特大分類コードエラーです"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            ' 大分類コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "BIGCATEGCD", LNM0021INProw("BIGCATEGCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If String.IsNullOrEmpty(LNM0021INProw("BIGCATEGCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("BIGCATEGCD", LNM0021INProw("BIGCATEGCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・大分類コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・大分類コードエラーです"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 中分類コード(バリデーションチェック）
            'Master.CheckField(Master.USERCAMP, "MIDDLECATEGCD", LNM0021INProw("MIDDLECATEGCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If isNormal(WW_CS0024FCheckerr) Then
            '    If String.IsNullOrEmpty(LNM0021INProw("MIDDLECATEGCD")) Then
            '        ' 名称存在チェック
            '        CODENAME_get("MIDDLECATEGCD", LNM0021INProw("MIDDLECATEGCD"), WW_Dummy, WW_RtnSW)
            '        If Not isNormal(WW_RtnSW) Then
            '            WW_CheckMES1 = "・中分類コードエラーです。"
            '            WW_CheckMES2 = "マスタに存在しません。"
            '            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '            WW_LineErr = "ERR"
            '            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '        End If
            '    End If
            'Else
            '    WW_CheckMES1 = "・中分類コードエラーです"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            ' 小分類コード(バリデーションチェック）
            'Master.CheckField(Master.USERCAMP, "SMALLCATEGCD", LNM0021INProw("SMALLCATEGCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If isNormal(WW_CS0024FCheckerr) Then
            '    If String.IsNullOrEmpty(LNM0021INProw("SMALLCATEGCD")) Then
            '        ' 名称存在チェック
            '        CODENAME_get("SMALLCATEGCD", LNM0021INProw("SMALLCATEGCD"), WW_Dummy, WW_RtnSW)
            '        If Not isNormal(WW_RtnSW) Then
            '            WW_CheckMES1 = "・小分類コードエラーです。"
            '            WW_CheckMES2 = "マスタに存在しません。"
            '            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '            WW_LineErr = "ERR"
            '            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            '        End If
            '    End If
            'Else
            '    WW_CheckMES1 = "・小分類コードエラーです"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            ' 危険品区分(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DANGERKBN", LNM0021INProw("DANGERKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If String.IsNullOrEmpty(LNM0021INProw("DANGERKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("DANGERKBN", LNM0021INProw("DANGERKBN"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・危険品区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・危険品区分エラーです"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 軽量品区分(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "LIGHTWTKBN", LNM0021INProw("LIGHTWTKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If String.IsNullOrEmpty(LNM0021INProw("LIGHTWTKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("LIGHTWTKBN", LNM0021INProw("LIGHTWTKBN"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・軽量品区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・軽量品区分エラーです"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 貴重品区分(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "VALUABLEKBN", LNM0021INProw("VALUABLEKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If String.IsNullOrEmpty(LNM0021INProw("VALUABLEKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("VALUABLEKBN", LNM0021INProw("VALUABLEKBN"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・貴重品区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・貴重品区分エラーです"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 冷蔵適合フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "REFRIGERATIONFLG", LNM0021INProw("REFRIGERATIONFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If String.IsNullOrEmpty(LNM0021INProw("REFRIGERATIONFLG")) Then
                    ' 名称存在チェック
                    CODENAME_get("REFRIGERATIONFLG", LNM0021INProw("REFRIGERATIONFLG"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・冷蔵適合フラグエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・冷蔵適合フラグエラーです"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_ITEMCD2.Text) Then  '品目コード
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck, TxtItemCd.Text, work.WF_SEL_TIMESTAMP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（品目コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                       "([" & LNM0021INProw("ITEMCD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNM0021INProw("ITEMCD") = work.WF_SEL_ITEMCD2.Text Then  '品目コード
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（品目コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & LNM0021INProw("ITEMCD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If String.IsNullOrEmpty(WW_LineErr) Then
                If LNM0021INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0021INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0021INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0021INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    Protected Sub WW_CheckERR(ByVal MESSAGE1 As String, ByVal MESSAGE2 As String)

        Dim WW_ErrMes As String = ""
        WW_ErrMes = MESSAGE1
        If Not String.IsNullOrEmpty(MESSAGE2) Then
            WW_ErrMes &= vbCr & "   -->" & MESSAGE2
        End If

        rightview.AddErrorReport(WW_ErrMes)

    End Sub

    ''' <summary>
    ''' LNM0021tbl更新
    ''' </summary>
    Protected Sub LNM0021tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0021row As DataRow In LNM0021tbl.Rows
            Select Case LNM0021row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0021row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0021row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0021row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0021row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0021row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0021INProw As DataRow In LNM0021INPtbl.Rows

            'エラーレコード読み飛ばし
            If LNM0021INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0021INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0021row As DataRow In LNM0021tbl.Rows
                ' KEY項目が等しい時
                If LNM0021row("ITEMCD") = LNM0021INProw("ITEMCD") Then                           '品目コード
                    ' KEY項目以外の項目の差異をチェック                                          
                    If LNM0021row("DELFLG") = LNM0021INProw("DELFLG") AndAlso                    '削除フラグ
                        LNM0021INProw("NAME") = LNM0021row("NAME") AndAlso                       '品目名称
                        LNM0021INProw("NAMES") = LNM0021row("NAMES") AndAlso                     '品目名称（短）
                        LNM0021INProw("NAMEKANA") = LNM0021row("NAMEKANA") AndAlso               '品目カナ名称
                        LNM0021INProw("NAMEKANAS") = LNM0021row("NAMEKANAS") AndAlso             '品目カナ名称（短）
                        LNM0021INProw("SPBIGCATEGCD") = LNM0021row("SPBIGCATEGCD") AndAlso       '特大分類コード
                        LNM0021INProw("BIGCATEGCD") = LNM0021row("BIGCATEGCD") AndAlso           '大分類コード
                        LNM0021INProw("MIDDLECATEGCD") = LNM0021row("MIDDLECATEGCD") AndAlso     '中大分類コード
                        LNM0021INProw("SMALLCATEGCD") = LNM0021row("SMALLCATEGCD") AndAlso       '小大分類コード
                        LNM0021INProw("DANGERKBN") = LNM0021row("DANGERKBN") AndAlso             '危険品区分
                        LNM0021INProw("LIGHTWTKBN") = LNM0021row("LIGHTWTKBN") AndAlso           '軽量品区分
                        LNM0021INProw("VALUABLEKBN") = LNM0021row("VALUABLEKBN") AndAlso         '貴重品区分
                        LNM0021INProw("REFRIGERATIONFLG") = LNM0021row("REFRIGERATIONFLG") Then  '冷蔵適合フラグ
                        ' 変更がないときは「操作」の項目は空白にする
                        LNM0021INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0021INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For

                End If
            Next
        Next

        ' 更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0021INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub
        ElseIf CONST_UPDATE.Equals(LNM0021INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0021INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' マスタ更新
                UpdateMaster(SQLcon)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = "Update Success!!"
            End Using
        End If

        '○ 変更有無判定 & 入力値反映
        For Each LNM0021INProw As DataRow In LNM0021INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0021row As DataRow In LNM0021tbl.Rows

                ' 同一レコードか判定
                If LNM0021INProw("ITEMCD") = LNM0021row("ITEMCD") Then  '品目コード
                    ' 画面入力テーブル項目設定
                    LNM0021INProw("LINECNT") = LNM0021row("LINECNT")
                    LNM0021INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0021INProw("UPDTIMSTP") = LNM0021row("UPDTIMSTP")
                    LNM0021INProw("SELECT") = 0
                    LNM0021INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0021row.ItemArray = LNM0021INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0021tbl.NewRow
                WW_NRow.ItemArray = LNM0021INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0021tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0021tbl.Rows.Add(WW_NRow)
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
                Case "SPBIGCATEGCD",     '特大分類コード
                     "BIGCATEGCD",       '大分類コード
                     "MIDDLECATEGCD",    '中大分類コード
                     "SMALLCATEGCD",     '小大分類コード
                     "DANGERKBN",        '危険品区分
                     "LIGHTWTKBN",       '軽量品区分
                     "VALUABLEKBN",      '貴重品区分
                     "REFRIGERATIONFLG"  '冷蔵適合フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))

                Case "OUTPUTID"         '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))
                Case "ONOFF"            '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))
                Case "DELFLG"           '削除
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_DELFLG, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DELFLG"))

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
