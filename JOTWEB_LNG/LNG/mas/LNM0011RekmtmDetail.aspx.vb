''************************************************************
' キロ程マスタメンテ登録画面
' 作成日 2023/10/25
' 更新日
' 作成者 大浜
' 更新者 
'
' 修正履歴:2023/10/25 新規作成
'         :
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' キロ程マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0011RekmtmDetail
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private LNM0011tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0011INPtbl As DataTable                              'チェック用テーブル
    Private LNM0011UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0011tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(LNM0011tbl) Then
                LNM0011tbl.Clear()
                LNM0011tbl.Dispose()
                LNM0011tbl = Nothing
            End If

            If Not IsNothing(LNM0011INPtbl) Then
                LNM0011INPtbl.Clear()
                LNM0011INPtbl.Dispose()
                LNM0011INPtbl = Nothing
            End If

            If Not IsNothing(LNM0011UPDtbl) Then
                LNM0011UPDtbl.Clear()
                LNM0011UPDtbl.Dispose()
                LNM0011UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0011WRKINC.MAPIDD
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0011L Then
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
        TxtDepStation.Text = work.WF_SEL_DEPSTATION.Text
        CODENAME_get("STATION", TxtDepStation.Text, LblDepStationName.Text, WW_Dummy)
        '着駅コード
        TxtArrStation.Text = work.WF_SEL_ARRSTATION.Text
        CODENAME_get("STATION", TxtArrStation.Text, LblArrStationName.Text, WW_Dummy)
        '摘要年月日
        If work.WF_SEL_FROMYMD.Text = "" Then
            TxtFromYmd.Text = Date.Now.ToString("yyyy-MM-dd")
        Else
            TxtFromYmd.Text = Replace(work.WF_SEL_FROMYMD.Text, "/", "-")
        End If
        'キロ程
        TxtKiro.Text = work.WF_SEL_KIRO.Text

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_DEPSTATION.Text

        ' 数値(0～9)のみ入力可能とする。
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"                    '削除フラグ
        Me.TxtDepStation.Attributes("onkeyPress") = "CheckNum()"                '発駅コード
        Me.TxtArrStation.Attributes("onkeyPress") = "CheckNum()"                '着駅コード

        ' 入力するテキストボックスは数値(0～9)＋記号(.)のみ可能とする。
        Me.TxtKiro.Attributes("onkeyPress") = "CheckDeci()"             'キロ程

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
        SQLStr.AppendLine("     DEPSTATION             ")
        SQLStr.AppendLine("   , ARRSTATION             ")
        SQLStr.AppendLine("   , FROMYMD                ")
        SQLStr.AppendLine(" FROM                       ")
        SQLStr.AppendLine("     LNG.LNM0011_REKMTM     ")
        SQLStr.AppendLine(" WHERE                      ")
        SQLStr.AppendLine("         DEPSTATION      = @DEPSTATION ")
        SQLStr.AppendLine("     AND ARRSTATION     = @ARRSTATION ")
        SQLStr.AppendLine("     AND FORMAT(FROMYMD, 'yyyyMMdd') = FORMAT(@FROMYMD, 'yyyyMMdd') ")
        SQLStr.AppendLine("     AND DELFLG      <> @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6) '発駅コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6) '着駅コード
                Dim P_FROMYMD As MySqlParameter = SQLcmd.Parameters.Add("@FROMYMD", MySqlDbType.Date)              '摘要年月日
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_DEPSTATION.Value = TxtDepStation.Text   '発駅コード
                P_ARRSTATION.Value = TxtArrStation.Text   '着駅コード
                P_FROMYMD.Value = CDate(TxtFromYmd.Text)  '摘要年月日
                P_DELFLG.Value = C_DELETE_FLG.DELETE      '削除フラグ

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0011Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0011Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0011Chk.Load(SQLdr)

                    If LNM0011Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0011C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' キロ程マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(キロ程マスタ)
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" DECLARE @hensuu AS bigint ;                                        ")
        SQLStr.AppendLine("     SET @hensuu = 0 ;                                              ")
        SQLStr.AppendLine(" DECLARE hensuu CURSOR FOR                                          ")
        SQLStr.AppendLine("     SELECT                                                         ")
        SQLStr.AppendLine("         UPDTIMSTP AS hensuu                                        ")
        SQLStr.AppendLine("     FROM                                                           ")
        SQLStr.AppendLine("         LNG.LNM0011_REKMTM                                         ")
        SQLStr.AppendLine("     WHERE                                                          ")
        SQLStr.AppendLine("         DEPSTATION         = @DEPSTATION                           ")
        SQLStr.AppendLine("     AND ARRSTATION        = @ARRSTATION                            ")
        SQLStr.AppendLine("     AND FORMAT(FROMYMD, 'yyyyMMdd') = FORMAT(@FROMYMD, 'yyyyMMdd') ")
        SQLStr.AppendLine(" OPEN hensuu ;                                                      ")
        SQLStr.AppendLine(" FETCH NEXT FROM hensuu INTO @hensuu ;                              ")
        SQLStr.AppendLine(" IF (@@FETCH_STATUS = 0)                                            ")
        SQLStr.AppendLine("     UPDATE LNG.LNM0011_REKMTM                                      ")
        SQLStr.AppendLine("     SET                                                            ")
        SQLStr.AppendLine("         DELFLG                  = @DELFLG                          ")
        SQLStr.AppendLine("       , KIRO                    = @KIRO                            ")
        SQLStr.AppendLine("       , UPDYMD                  = @UPDYMD                          ")
        SQLStr.AppendLine("       , UPDUSER                 = @UPDUSER                         ")
        SQLStr.AppendLine("       , UPDTERMID               = @UPDTERMID                       ")
        SQLStr.AppendLine("       , UPDPGID                 = @UPDPGID                         ")
        SQLStr.AppendLine("       , RECEIVEYMD              = @RECEIVEYMD                      ")
        SQLStr.AppendLine("     WHERE                                                          ")
        SQLStr.AppendLine("         DEPSTATION        = @DEPSTATION                            ")
        SQLStr.AppendLine("     AND ARRSTATION        = @ARRSTATION                            ")
        SQLStr.AppendLine("     AND FORMAT(FROMYMD, 'yyyyMMdd') = FORMAT(@FROMYMD, 'yyyyMMdd') ")
        SQLStr.AppendLine(" IF (@@FETCH_STATUS <> 0)                                           ")
        SQLStr.AppendLine("     INSERT INTO LNG.LNM0011_REKMTM                                 ")
        SQLStr.AppendLine("        (DELFLG                                                     ")
        SQLStr.AppendLine("       , DEPSTATION                                                 ")
        SQLStr.AppendLine("       , ARRSTATION                                                 ")
        SQLStr.AppendLine("       , FROMYMD                                                    ")
        SQLStr.AppendLine("       , KIRO                                                       ")
        SQLStr.AppendLine("       , INITYMD                                                    ")
        SQLStr.AppendLine("       , INITUSER                                                   ")
        SQLStr.AppendLine("       , INITTERMID                                                 ")
        SQLStr.AppendLine("       , INITPGID                                                   ")
        SQLStr.AppendLine("       , RECEIVEYMD)                                                ")
        SQLStr.AppendLine("     VALUES                                                         ")
        SQLStr.AppendLine("        (@DELFLG                                                    ")
        SQLStr.AppendLine("       , @DEPSTATION                                                ")
        SQLStr.AppendLine("       , @ARRSTATION                                                ")
        SQLStr.AppendLine("       , @FROMYMD                                                   ")
        SQLStr.AppendLine("       , @KIRO                                                      ")
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
        SQLJnl.AppendLine("  , DEPSTATION                              ")
        SQLJnl.AppendLine("  , ARRSTATION                              ")
        SQLJnl.AppendLine("  , FROMYMD                                 ")
        SQLJnl.AppendLine("  , KIRO                                    ")
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
        SQLJnl.AppendLine("    LNG.LNM0011_REKMTM                      ")
        SQLJnl.AppendLine(" WHERE                                      ")
        SQLJnl.AppendLine("        DEPSTATION        = @DEPSTATION     ")
        SQLJnl.AppendLine("    AND ARRSTATION        = @ARRSTATION     ")
        SQLJnl.AppendLine("    AND FORMAT(FROMYMD, 'yyyyMMdd') = FORMAT(@FROMYMD, 'yyyyMMdd') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl.ToString, SQLcon)
                ' DB更新用パラメータ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)            '削除フラグ
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)     '発駅コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)     '着駅コード
                Dim P_FROMYMD As MySqlParameter = SQLcmd.Parameters.Add("@FROMYMD", MySqlDbType.Date)                  '摘要年月日
                Dim P_KIRO As MySqlParameter = SQLcmd.Parameters.Add("@KIRO", MySqlDbType.Decimal, 7, 1)               'キロ程
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
                Dim JP_DEPSTATION As MySqlParameter = SQLcmdJnl.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)  '発駅コード
                Dim JP_ARRSTATION As MySqlParameter = SQLcmdJnl.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)  '着駅コード
                Dim JP_FROMYMD As MySqlParameter = SQLcmdJnl.Parameters.Add("@FROMYMD", MySqlDbType.Date)        '摘要年月日

                Dim LNM0011row As DataRow = LNM0011INPtbl.Rows(0)

                ' DB更新
                P_DELFLG.Value = LNM0011row("DELFLG")                   '削除フラグ
                P_DEPSTATION.Value = LNM0011row("DEPSTATION")           '発駅コード
                P_ARRSTATION.Value = LNM0011row("ARRSTATION")           '着駅コード
                P_FROMYMD.Value = CDate(LNM0011row("FROMYMD"))          '摘要年月日
                P_KIRO.Value = LNM0011row("KIRO")                       'キロ程

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
                JP_DEPSTATION.Value = LNM0011row("DEPSTATION")
                JP_ARRSTATION.Value = LNM0011row("ARRSTATION")
                JP_FROMYMD.Value = LNM0011row("FROMYMD")

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0011UPDtbl) Then
                        LNM0011UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0011UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0011UPDtbl.Clear()
                    LNM0011UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0011UPDrow As DataRow In LNM0011UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0011C"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0011UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0011C UPDATE_INSERT"
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

        'キロ程マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DEPSTATION")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0011_REKMTM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        DEPSTATION         = @DEPSTATION")
        SQLStr.AppendLine("    AND ARRSTATION        = @ARRSTATION")
        SQLStr.AppendLine("    AND FORMAT(FROMYMD, 'yyyyMMdd') = FORMAT(@FROMYMD, 'yyyyMMdd') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)     '発駅コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)     '着駅コード
                Dim P_FROMYMD As MySqlParameter = SQLcmd.Parameters.Add("@FROMYMD", MySqlDbType.Date)                  '摘要年月日

                Dim LNM0011row As DataRow = LNM0011INPtbl.Rows(0)

                P_DEPSTATION.Value = LNM0011row("DEPSTATION")           '発駅コード
                P_ARRSTATION.Value = LNM0011row("ARRSTATION")           '着駅コード
                P_FROMYMD.Value = CDate(LNM0011row("FROMYMD"))          '摘要年月日

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
                        WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0011C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0011C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0098_REKMTHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         DEPSTATION  ")
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,FROMYMD  ")
        SQLStr.AppendLine("        ,KIRO  ")
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
        SQLStr.AppendLine("        ,ARRSTATION  ")
        SQLStr.AppendLine("        ,FROMYMD  ")
        SQLStr.AppendLine("        ,KIRO  ")
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
        SQLStr.AppendLine("        LNG.LNM0011_REKMTM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        DEPSTATION         = @DEPSTATION")
        SQLStr.AppendLine("    AND ARRSTATION        = @ARRSTATION")
        SQLStr.AppendLine("    AND FORMAT(FROMYMD, 'yyyyMMdd') = FORMAT(@FROMYMD, 'yyyyMMdd') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)     '発駅コード
                Dim P_ARRSTATION As MySqlParameter = SQLcmd.Parameters.Add("@ARRSTATION", MySqlDbType.VarChar, 6)     '着駅コード
                Dim P_FROMYMD As MySqlParameter = SQLcmd.Parameters.Add("@FROMYMD", MySqlDbType.Date)                  '摘要年月日

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0011row As DataRow = LNM0011INPtbl.Rows(0)

                ' DB更新
                P_DEPSTATION.Value = LNM0011row("DEPSTATION")           '発駅コード
                P_ARRSTATION.Value = LNM0011row("ARRSTATION")           '着駅コード
                P_FROMYMD.Value = CDate(LNM0011row("FROMYMD"))          '摘要年月日

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0011WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0011tbl.Rows(0)("DELFLG") = "0" And LNM0011row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0011WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0011WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0098_REKMTHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0098_REKMTHIST  INSERT"
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
        DetailBoxToLNM0011INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0011tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0011tbl, work.WF_SEL_INPTBL.Text)

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
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, "キロ程", needsPopUp:=True)
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
    Protected Sub DetailBoxToLNM0011INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)                    '削除フラグ
        Master.EraseCharToIgnore(TxtDepStation.Text)                '発駅コード
        Master.EraseCharToIgnore(TxtArrStation.Text)                '着駅コード
        Master.EraseCharToIgnore(TxtFromYmd.Text)                   '摘要年月日
        Master.EraseCharToIgnore(TxtKiro.Text)                      'キロ程

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

        Master.CreateEmptyTable(LNM0011INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0011INProw As DataRow = LNM0011INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(LblSelLineCNT.Text) Then
            LNM0011INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(LblSelLineCNT.Text, LNM0011INProw("LINECNT"))
            Catch ex As Exception
                LNM0011INProw("LINECNT") = 0
            End Try
        End If

        LNM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0011INProw("UPDTIMSTP") = 0
        LNM0011INProw("SELECT") = 1
        LNM0011INProw("HIDDEN") = 0

        LNM0011INProw("DELFLG") = TxtDelFlg.Text                                        '削除フラグ
        LNM0011INProw("DEPSTATION") = TxtDepStation.Text                                '発駅コード
        LNM0011INProw("ARRSTATION") = TxtArrStation.Text                                '着駅コード
        LNM0011INProw("FROMYMD") = TxtFromYmd.Text                                      '摘要年月日
        LNM0011INProw("KIRO") = TxtKiro.Text                                            'キロ程

        '○ チェック用テーブルに登録する
        LNM0011INPtbl.Rows.Add(LNM0011INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0011INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0011INProw As DataRow = LNM0011INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0011row As DataRow In LNM0011tbl.Rows
            ' KEY項目が等しい時
            If LNM0011row("DEPSTATION") = LNM0011INProw("DEPSTATION") AndAlso                                '発駅コード
               LNM0011row("ARRSTATION") = LNM0011INProw("ARRSTATION") AndAlso                                '着駅コード
               LNM0011row("FROMYMD") = LNM0011INProw("FROMYMD") Then                                         '摘要年月日
                ' KEY項目以外の項目の差異をチェック
                If LNM0011row("DELFLG") = LNM0011INProw("DELFLG") AndAlso                                    '削除フラグ
                   LNM0011row("KIRO") = LNM0011INProw("KIRO") Then                                           'キロ程
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
        For Each LNM0011row As DataRow In LNM0011tbl.Rows
            Select Case LNM0011row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0011tbl, work.WF_SEL_INPTBL.Text)

        LblSelLineCNT.Text = ""                'LINECNT
        TxtMapId.Text = "M00001"               '画面ＩＤ
        TxtDelFlg.Text = ""                    '削除フラグ
        TxtDepStation.Text = ""                '発駅コード
        TxtArrStation.Text = ""                '着駅コード
        TxtFromYmd.Text = ""                   '摘要年月日
        TxtKiro.Text = ""                      'キロ程

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
                    Case "TxtDepStation",       '発駅コード
                         "TxtArrStation"        '着駅コード
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspStationSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub

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
            Case "TxtArrStation"               '着駅コード
                CODENAME_get("STATION", TxtArrStation.Text, LblArrStationName.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblArrStationName.Text) And TxtArrStation.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtArrStation.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtArrStation.Focus()
                End If
                TxtArrStation.Focus()
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
                Case "TxtDepStation"               '発駅コード
                    TxtDepStation.Text = WW_SelectValue
                    LblDepStationName.Text = WW_SelectText
                    TxtDepStation.Focus()
                Case "TxtArrStation"               '着駅コード
                    TxtArrStation.Text = WW_SelectValue
                    LblArrStationName.Text = WW_SelectText
                    TxtArrStation.Focus()
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
                Case "TxtDepStation"               '発駅コード
                    TxtDepStation.Focus()
                Case "TxtArrStation"               '着駅コード
                    TxtArrStation.Focus()
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
    ''' 駅選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspStationSingle()

        Dim selData = Me.mspStationSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtDepStation.ID
                Me.TxtDepStation.Text = selData("STATION").ToString
                Me.LblDepStationName.Text = selData("NAMES").ToString
                Me.TxtDepStation.Focus()

            Case TxtArrStation.ID
                Me.TxtArrStation.Text = selData("STATION").ToString
                Me.LblArrStationName.Text = selData("NAMES").ToString
                Me.TxtArrStation.Focus()
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
            WW_CheckMES1 = "・キロ程マスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0011INProw As DataRow In LNM0011INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0011INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0011INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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
            ' 発駅コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DEPSTATION", LNM0011INProw("DEPSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("STATION", LNM0011INProw("DEPSTATION"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・発駅コード入力エラー"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・発駅コード入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 着駅コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ARRSTATION", LNM0011INProw("ARRSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("STATION", LNM0011INProw("ARRSTATION"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・着駅コード入力エラー"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・着駅コード入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 摘要年月日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "FROMYMD", LNM0011INProw("FROMYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0011INProw("FROMYMD")) Then
                    LNM0011INProw("FROMYMD") = CDate(LNM0011INProw("FROMYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・摘要年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' キロ程(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KIRO", LNM0011INProw("KIRO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・キロ程エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_DEPSTATION.Text) Then  '発駅コード
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                    TxtDepStation.Text, TxtArrStation.Text,
                                    TxtFromYmd.Text, work.WF_SEL_TIMESTAMP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（発駅コード&着駅コード&摘要年月日）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                       "([" & LNM0011INProw("DEPSTATION") & "]" &
                                       "([" & LNM0011INProw("ARRSTATION") & "]" &
                                       "([" & LNM0011INProw("FROMYMD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNM0011INProw("DEPSTATION") = work.WF_SEL_DEPSTATION.Text OrElse     '発駅コード
               Not LNM0011INProw("ARRSTATION") = work.WF_SEL_ARRSTATION.Text OrElse     '着駅コード
               Not LNM0011INProw("FROMYMD") = work.WF_SEL_FROMYMD.Text Then             '摘要年月日
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（発駅コード&着駅コード&摘要年月日）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & LNM0011INProw("DEPSTATION") & "]" &
                                       "([" & LNM0011INProw("ARRSTATION") & "]" &
                                       "([" & LNM0011INProw("FROMYMD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LineErr = "" Then
                If LNM0011INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0011INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0011INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0011tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0011tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0011row As DataRow In LNM0011tbl.Rows
            Select Case LNM0011row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0011row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0011INProw As DataRow In LNM0011INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0011INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0011INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0011row As DataRow In LNM0011tbl.Rows
                ' KEY項目が等しい時
                If LNM0011row("DEPSTATION") = LNM0011INProw("DEPSTATION") AndAlso                         '発駅コード
                   LNM0011row("ARRSTATION") = LNM0011INProw("ARRSTATION") AndAlso                         '着駅コード
                   LNM0011row("FROMYMD") = LNM0011INProw("FROMYMD") Then                                  '摘要年月日
                    ' KEY項目以外の項目の差異をチェック                                                           
                    If LNM0011row("DELFLG") = LNM0011INProw("DELFLG") AndAlso                             '削除フラグ
                        LNM0011row("KIRO") = LNM0011INProw("KIRO") AndAlso                                'キロ程
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0011row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0011INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0011INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0011INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0011INPtbl.Rows(0)("OPERATION")) Then
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
                If WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0011WRKINC.MODIFYKBN.AFTDATA
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
        For Each LNM0011INProw As DataRow In LNM0011INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0011row As DataRow In LNM0011tbl.Rows
                ' 同一レコードか判定
                If LNM0011INProw("DEPSTATION") = LNM0011row("DEPSTATION") AndAlso       '発駅コード
                   LNM0011INProw("ARRSTATION") = LNM0011row("ARRSTATION") AndAlso       '着駅コード
                   LNM0011INProw("FROMYMD") = LNM0011row("FROMYMD") Then                '摘要年月日
                    ' 画面入力テーブル項目設定
                    LNM0011INProw("LINECNT") = LNM0011row("LINECNT")
                    LNM0011INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0011INProw("UPDTIMSTP") = LNM0011row("UPDTIMSTP")
                    LNM0011INProw("SELECT") = 0
                    LNM0011INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0011row.ItemArray = LNM0011INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0011tbl.NewRow
                WW_NRow.ItemArray = LNM0011INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0011tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0011tbl.Rows.Add(WW_NRow)
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
                Case "STATION"            '発駅コード・着駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "DELFLG"             '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
