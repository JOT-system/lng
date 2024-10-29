''************************************************************
' 使用料率マスタメンテ登録画面
' 作成日 2023/11/07
' 更新日
' 作成者 大浜
' 更新者 
'
' 修正履歴:2023/11/07 新規作成
'         :
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 使用料率マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0015ResrtmDetail
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private LNM0015tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0015INPtbl As DataTable                              'チェック用テーブル
    Private LNM0015UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0015tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(LNM0015tbl) Then
                LNM0015tbl.Clear()
                LNM0015tbl.Dispose()
                LNM0015tbl = Nothing
            End If

            If Not IsNothing(LNM0015INPtbl) Then
                LNM0015INPtbl.Clear()
                LNM0015INPtbl.Dispose()
                LNM0015INPtbl = Nothing
            End If

            If Not IsNothing(LNM0015UPDtbl) Then
                LNM0015UPDtbl.Clear()
                LNM0015UPDtbl.Dispose()
                LNM0015UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0015WRKINC.MAPIDD
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0015L Then
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
        '発組織コード
        TxtJRDepBranchCode.Text = work.WF_SEL_JRDEPBRANCHCD.Text
        CODENAME_get("ORG", TxtJRDepBranchCode.Text, LblJRDepBranchName.Text, WW_Dummy)
        '着組織コード
        TxtJRArrBranchCode.Text = work.WF_SEL_JRARRBRANCHCD.Text
        CODENAME_get("ORG", TxtJRArrBranchCode.Text, LblJRArrBranchName.Text, WW_Dummy)
        '使用料率
        TxtUsefeerate.Text = work.WF_SEL_USEFEERATE.Text

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_JRDEPBRANCHCD.Text

        ' 数値(0～9)のみ入力可能とする。
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"                    '削除フラグ
        Me.TxtJRDepBranchCode.Attributes("onkeyPress") = "CheckNum()"           '発組織コード
        Me.TxtJRArrBranchCode.Attributes("onkeyPress") = "CheckNum()"           '着組織コード

        ' 入力するテキストボックスは数値(0～9)＋記号(.)のみ可能とする。
        Me.TxtUsefeerate.Attributes("onkeyPress") = "CheckDeci()"             '使用料率

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
        SQLStr.AppendLine("     JRDEPBRANCHCD          ")
        SQLStr.AppendLine("   , JRARRBRANCHCD          ")
        SQLStr.AppendLine(" FROM                       ")
        SQLStr.AppendLine("     LNG.LNM0015_RESRTM     ")
        SQLStr.AppendLine(" WHERE                      ")
        SQLStr.AppendLine("         JRDEPBRANCHCD      = @JRDEPBRANCHCD ")
        SQLStr.AppendLine("     AND JRARRBRANCHCD     = @JRARRBRANCHCD ")
        SQLStr.AppendLine("     AND DELFLG      <> @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_JRDEPBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRDEPBRANCHCD", MySqlDbType.VarChar, 6) '発組織コード
                Dim P_JRARRBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRARRBRANCHCD", MySqlDbType.VarChar, 6) '着組織コード
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_JRDEPBRANCHCD.Value = TxtJRDepBranchCode.Text   '発駅コード
                P_JRARRBRANCHCD.Value = TxtJRArrBranchCode.Text   '着駅コード
                P_DELFLG.Value = C_DELETE_FLG.DELETE      '削除フラグ

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0015Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0015Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0015Chk.Load(SQLdr)

                    If LNM0015Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0015C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0015C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 使用料率マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(使用料率マスタ)
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" DECLARE @hensuu AS bigint ;                                        ")
        SQLStr.AppendLine("     SET @hensuu = 0 ;                                              ")
        SQLStr.AppendLine(" DECLARE hensuu CURSOR FOR                                          ")
        SQLStr.AppendLine("     SELECT                                                         ")
        SQLStr.AppendLine("         UPDTIMSTP                 AS hensuu                        ")
        SQLStr.AppendLine("     FROM                                                           ")
        SQLStr.AppendLine("         LNG.LNM0015_RESRTM                                         ")
        SQLStr.AppendLine("     WHERE                                                          ")
        SQLStr.AppendLine("         JRDEPBRANCHCD         = @JRDEPBRANCHCD                     ")
        SQLStr.AppendLine("     AND JRARRBRANCHCD        = @JRARRBRANCHCD                      ")
        SQLStr.AppendLine(" OPEN hensuu ;                                                      ")
        SQLStr.AppendLine(" FETCH NEXT FROM hensuu INTO @hensuu ;                              ")
        SQLStr.AppendLine(" IF (@@FETCH_STATUS = 0)                                            ")
        SQLStr.AppendLine("     UPDATE LNG.LNM0015_RESRTM                                      ")
        SQLStr.AppendLine("     SET                                                            ")
        SQLStr.AppendLine("         DELFLG                  = @DELFLG                          ")
        SQLStr.AppendLine("       , USEFEERATE              = @USEFEERATE                      ")
        SQLStr.AppendLine("       , UPDYMD                  = @UPDYMD                          ")
        SQLStr.AppendLine("       , UPDUSER                 = @UPDUSER                         ")
        SQLStr.AppendLine("       , UPDTERMID               = @UPDTERMID                       ")
        SQLStr.AppendLine("       , UPDPGID                 = @UPDPGID                         ")
        SQLStr.AppendLine("       , RECEIVEYMD              = @RECEIVEYMD                      ")
        SQLStr.AppendLine("     WHERE                                                          ")
        SQLStr.AppendLine("         JRDEPBRANCHCD        = @JRDEPBRANCHCD                      ")
        SQLStr.AppendLine("     AND JRARRBRANCHCD        = @JRARRBRANCHCD                      ")
        SQLStr.AppendLine(" IF (@@FETCH_STATUS <> 0)                                           ")
        SQLStr.AppendLine("     INSERT INTO LNG.LNM0015_RESRTM                                 ")
        SQLStr.AppendLine("        (DELFLG                                                     ")
        SQLStr.AppendLine("       , JRDEPBRANCHCD                                              ")
        SQLStr.AppendLine("       , JRARRBRANCHCD                                              ")
        SQLStr.AppendLine("       , USEFEERATE                                                 ")
        SQLStr.AppendLine("       , INITYMD                                                    ")
        SQLStr.AppendLine("       , INITUSER                                                   ")
        SQLStr.AppendLine("       , INITTERMID                                                 ")
        SQLStr.AppendLine("       , INITPGID                                                   ")
        SQLStr.AppendLine("       , RECEIVEYMD)                                                ")
        SQLStr.AppendLine("     VALUES                                                         ")
        SQLStr.AppendLine("        (@DELFLG                                                    ")
        SQLStr.AppendLine("       , @JRDEPBRANCHCD                                             ")
        SQLStr.AppendLine("       , @JRARRBRANCHCD                                             ")
        SQLStr.AppendLine("       , @USEFEERATE                                                ")
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
        SQLJnl.AppendLine("  , JRDEPBRANCHCD                           ")
        SQLJnl.AppendLine("  , JRARRBRANCHCD                           ")
        SQLJnl.AppendLine("  , USEFEERATE                              ")
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
        SQLJnl.AppendLine("    LNG.LNM0015_RESRTM                      ")
        SQLJnl.AppendLine(" WHERE                                      ")
        SQLJnl.AppendLine("        JRDEPBRANCHCD  = @JRDEPBRANCHCD     ")
        SQLJnl.AppendLine("    AND JRARRBRANCHCD  = @JRARRBRANCHCD     ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl.ToString, SQLcon)
                ' DB更新用パラメータ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                '削除フラグ
                Dim P_JRDEPBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRDEPBRANCHCD", MySqlDbType.VarChar, 6)  '発組織コード
                Dim P_JRARRBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRARRBRANCHCD", MySqlDbType.VarChar, 6)  '着組織コード
                Dim P_USEFEERATE As MySqlParameter = SQLcmd.Parameters.Add("@USEFEERATE", MySqlDbType.Decimal, 5, 4)      '使用料率
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)                 '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)           '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)       '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)           '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                   '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)             '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)             '更新プログラムＩＤ
                Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)           '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JP_JRDEPBRANCHCD As MySqlParameter = SQLcmdJnl.Parameters.Add("@JRDEPBRANCHCD", MySqlDbType.VarChar, 6)  '発組織コード
                Dim JP_JRARRBRANCHCD As MySqlParameter = SQLcmdJnl.Parameters.Add("@JRARRBRANCHCD", MySqlDbType.VarChar, 6)  '着組織コード

                Dim LNM0015row As DataRow = LNM0015INPtbl.Rows(0)

                ' DB更新
                P_DELFLG.Value = LNM0015row("DELFLG")                   '削除フラグ
                P_JRDEPBRANCHCD.Value = LNM0015row("JRDEPBRANCHCD")     '発組織コード
                P_JRARRBRANCHCD.Value = LNM0015row("JRARRBRANCHCD")     '着組織コード
                P_USEFEERATE.Value = LNM0015row("USEFEERATE")           '使用料率

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
                JP_JRDEPBRANCHCD.Value = LNM0015row("JRDEPBRANCHCD")
                JP_JRARRBRANCHCD.Value = LNM0015row("JRARRBRANCHCD")

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0015UPDtbl) Then
                        LNM0015UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0015UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0015UPDtbl.Clear()
                    LNM0015UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0015UPDrow As DataRow In LNM0015UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0015C"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0015UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0015C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0015C UPDATE_INSERT"
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

        '使用料率マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        JRDEPBRANCHCD")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0015_RESRTM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        JRDEPBRANCHCD         = @JRDEPBRANCHCD")
        SQLStr.AppendLine("    AND JRARRBRANCHCD        = @JRARRBRANCHCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_JRDEPBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRDEPBRANCHCD", MySqlDbType.VarChar, 6)  '発組織コード
                Dim P_JRARRBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRARRBRANCHCD", MySqlDbType.VarChar, 6)  '着組織コード

                Dim LNM0015row As DataRow = LNM0015INPtbl.Rows(0)

                P_JRDEPBRANCHCD.Value = LNM0015row("JRDEPBRANCHCD")     '発組織コード
                P_JRARRBRANCHCD.Value = LNM0015row("JRARRBRANCHCD")     '着組織コード

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
                        WW_MODIFYKBN = LNM0015WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0015WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0015C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0015C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0099_RESRTHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         JRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,JRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,USEFEERATE  ")
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
        SQLStr.AppendLine("         JRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,JRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,USEFEERATE  ")
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
        SQLStr.AppendLine("        LNG.LNM0015_RESRTM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        JRDEPBRANCHCD         = @JRDEPBRANCHCD")
        SQLStr.AppendLine("    AND JRARRBRANCHCD        = @JRARRBRANCHCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_JRDEPBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRDEPBRANCHCD", MySqlDbType.VarChar, 6)  '発組織コード
                Dim P_JRARRBRANCHCD As MySqlParameter = SQLcmd.Parameters.Add("@JRARRBRANCHCD", MySqlDbType.VarChar, 6)  '着組織コード

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0015row As DataRow = LNM0015INPtbl.Rows(0)

                ' DB更新
                P_JRDEPBRANCHCD.Value = LNM0015row("JRDEPBRANCHCD")     '発組織コード
                P_JRARRBRANCHCD.Value = LNM0015row("JRARRBRANCHCD")     '着組織コード

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0015WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0015WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0015tbl.Rows(0)("DELFLG") = "0" And LNM0015row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0015WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0015WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0099_RESRTHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0099_RESRTHIST  INSERT"
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
        DetailBoxToLNM0015INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0015tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0015tbl, work.WF_SEL_INPTBL.Text)

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
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, "使用料率", needsPopUp:=True)
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
    Protected Sub DetailBoxToLNM0015INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)                    '削除フラグ
        Master.EraseCharToIgnore(TxtJRDepBranchCode.Text)           '発組織コード
        Master.EraseCharToIgnore(TxtJRArrBranchCode.Text)           '着組織コード
        Master.EraseCharToIgnore(TxtUsefeerate.Text)                '使用料率

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

        Master.CreateEmptyTable(LNM0015INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0015INProw As DataRow = LNM0015INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(LblSelLineCNT.Text) Then
            LNM0015INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(LblSelLineCNT.Text, LNM0015INProw("LINECNT"))
            Catch ex As Exception
                LNM0015INProw("LINECNT") = 0
            End Try
        End If

        LNM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0015INProw("UPDTIMSTP") = 0
        LNM0015INProw("SELECT") = 1
        LNM0015INProw("HIDDEN") = 0

        LNM0015INProw("DELFLG") = TxtDelFlg.Text                                        '削除フラグ
        LNM0015INProw("JRDEPBRANCHCD") = TxtJRDepBranchCode.Text                        '発組織コード
        LNM0015INProw("JRARRBRANCHCD") = TxtJRArrBranchCode.Text                        '着組織コード
        LNM0015INProw("USEFEERATE") = TxtUsefeerate.Text                                '使用料率

        '○ チェック用テーブルに登録する
        LNM0015INPtbl.Rows.Add(LNM0015INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0015INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0015INProw As DataRow = LNM0015INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0015row As DataRow In LNM0015tbl.Rows
            ' KEY項目が等しい時
            If LNM0015row("JRDEPBRANCHCD") = LNM0015INProw("JRDEPBRANCHCD") AndAlso                          '発組織コード
               LNM0015row("JRARRBRANCHCD") = LNM0015INProw("JRARRBRANCHCD") Then                             '着組織コード
                ' KEY項目以外の項目の差異をチェック
                If LNM0015row("DELFLG") = LNM0015INProw("DELFLG") AndAlso                                    '削除フラグ
                   LNM0015row("USEFEERATE") = LNM0015INProw("USEFEERATE") Then                               '使用料率
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
        For Each LNM0015row As DataRow In LNM0015tbl.Rows
            Select Case LNM0015row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0015row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0015row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0015tbl, work.WF_SEL_INPTBL.Text)

        LblSelLineCNT.Text = ""                'LINECNT
        TxtMapId.Text = "M00001"               '画面ＩＤ
        TxtDelFlg.Text = ""                    '削除フラグ
        TxtJRDepBranchCode.Text = ""           '発組織コード
        TxtJRArrBranchCode.Text = ""           '着組織コード
        TxtUsefeerate.Text = ""                '使用料率

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
                    Case "TxtJRDepBranchCode",       '発組織コード
                         "TxtJRArrBranchCode"        '着組織コード
                        WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP)
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
            Case "TxtJRDepBranchCode"               '発組織コード
                CODENAME_get("ORG", TxtJRDepBranchCode.Text, LblJRDepBranchName.Text, WW_Dummy)
                TxtJRDepBranchCode.Focus()
            Case "TxtJRArrBranchCode"               '着組織コード
                CODENAME_get("ORG", TxtJRArrBranchCode.Text, LblJRArrBranchName.Text, WW_Dummy)
                TxtJRArrBranchCode.Focus()
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
                Case "TxtJRDepBranchCode"               '発組織コード
                    TxtJRDepBranchCode.Text = WW_SelectValue
                    LblJRDepBranchName.Text = WW_SelectText
                    TxtJRDepBranchCode.Focus()
                Case "TxtJRArrBranchCode"               '着組織コード
                    TxtJRArrBranchCode.Text = WW_SelectValue
                    LblJRArrBranchName.Text = WW_SelectText
                    TxtJRArrBranchCode.Focus()
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
                Case "TxtDepStation"               '発組織コード
                    TxtJRDepBranchCode.Focus()
                Case "TxtArrStation"               '着組織コード
                    TxtJRArrBranchCode.Focus()
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
            WW_CheckMES1 = "・使用料率マスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0015INProw As DataRow In LNM0015INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0015INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0015INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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
            ' 発組織コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "JRDEPBRANCHCD", LNM0015INProw("JRDEPBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("ORG", LNM0015INProw("JRDEPBRANCHCD"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・発組織コード入力エラー"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・発組織コード入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 着組織コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "JRARRBRANCHCD", LNM0015INProw("JRARRBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("ORG", LNM0015INProw("JRARRBRANCHCD"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・着組織コード入力エラー"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・着組織コード入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 使用料率(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "USEFEERATE", LNM0015INProw("USEFEERATE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・使用料率エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_JRDEPBRANCHCD.Text) Then  '発組織コード
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                    TxtJRDepBranchCode.Text, TxtJRArrBranchCode.Text,
                                    work.WF_SEL_TIMESTAMP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（発組織コード&着組織コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                       "([" & LNM0015INProw("JRDEPBRANCHCD") & "]" &
                                       "([" & LNM0015INProw("JRARRBRANCHCD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNM0015INProw("JRDEPBRANCHCD") = work.WF_SEL_JRDEPBRANCHCD.Text OrElse     '発組織コード
               Not LNM0015INProw("JRARRBRANCHCD") = work.WF_SEL_JRARRBRANCHCD.Text Then       '着組織コード
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（発組織コード&着組織コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & LNM0015INProw("JRDEPBRANCHCD") & "]" &
                                       "([" & LNM0015INProw("JRARRBRANCHCD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LineErr = "" Then
                If LNM0015INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0015INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0015INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0015tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0015tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0015row As DataRow In LNM0015tbl.Rows
            Select Case LNM0015row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0015row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0015row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0015row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0015INProw As DataRow In LNM0015INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0015INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0015INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0015row As DataRow In LNM0015tbl.Rows
                ' KEY項目が等しい時
                If LNM0015row("JRDEPBRANCHCD") = LNM0015INProw("JRDEPBRANCHCD") AndAlso                   '発組織コード
                   LNM0015row("JRARRBRANCHCD") = LNM0015INProw("JRARRBRANCHCD") Then                      '着組織コード
                    ' KEY項目以外の項目の差異をチェック                                                           
                    If LNM0015row("DELFLG") = LNM0015INProw("DELFLG") AndAlso                             '削除フラグ
                        LNM0015row("USEFEERATE") = LNM0015INProw("USEFEERATE") AndAlso                    '使用料率
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0015row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0015INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0015INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0015INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0015INPtbl.Rows(0)("OPERATION")) Then
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
                If WW_MODIFYKBN = LNM0015WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0015WRKINC.MODIFYKBN.AFTDATA
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
        For Each LNM0015INProw As DataRow In LNM0015INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0015row As DataRow In LNM0015tbl.Rows
                ' 同一レコードか判定
                If LNM0015INProw("JRDEPBRANCHCD") = LNM0015row("JRDEPBRANCHCD") AndAlso       '発組織コード
                   LNM0015INProw("JRARRBRANCHCD") = LNM0015row("JRARRBRANCHCD") Then          '着組織コード
                    ' 画面入力テーブル項目設定
                    LNM0015INProw("LINECNT") = LNM0015row("LINECNT")
                    LNM0015INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0015INProw("UPDTIMSTP") = LNM0015row("UPDTIMSTP")
                    LNM0015INProw("SELECT") = 0
                    LNM0015INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0015row.ItemArray = LNM0015INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0015tbl.NewRow
                WW_NRow.ItemArray = LNM0015INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0015tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0015tbl.Rows.Add(WW_NRow)
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
                Case "ORG"                '発組織コード・着組織コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "DELFLG"             '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
