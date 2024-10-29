''************************************************************
' ボリュームインセンティブマスタメンテ登録画面
' 作成日 2022/05/23
' 更新日 
' 作成者 瀬口
' 更新者 
'
' 修正履歴 : 2022/06/02 新規作成
'          : 
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' ボリュームインセンティブマスタメンテ（詳細）
''' </summary>
''' <remarks></remarks>
Public Class LNM0026IncentiveDetail
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private LNM0026tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0026INPtbl As DataTable                              'チェック用テーブル
    Private LNM0026UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0026tbl, work.WF_SEL_INPTBL.Text)

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
            If Not IsNothing(LNM0026tbl) Then
                LNM0026tbl.Clear()
                LNM0026tbl.Dispose()
                LNM0026tbl = Nothing
            End If

            If Not IsNothing(LNM0026INPtbl) Then
                LNM0026INPtbl.Clear()
                LNM0026INPtbl.Dispose()
                LNM0026INPtbl = Nothing
            End If

            If Not IsNothing(LNM0026UPDtbl) Then
                LNM0026UPDtbl.Clear()
                LNM0026UPDtbl.Dispose()
                LNM0026UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0026WRKINC.MAPIDD
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0026L Then
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
        txtMapId.Text = "LNM0026D"

        '取引先コード
        txtToriCode.Text = work.WF_SEL_TORICODE.Text
        CODENAME_get("TORICODE", txtToriCode.Text, lblToriCodeName.Text, WW_Dummy)

        '発駅コード
        txtDepStation.Text = work.WF_SEL_DEPSTATION.Text
        CODENAME_get("DEPSTATION", txtDepStation.Text, lblDepStationName.Text, WW_Dummy)

        txtVolIncentAmo.Text = work.WF_SEL_VOLINCENTAMO.Text

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_TORICODE.Text

        ' 数値(0～9)のみ入力可能とする。
        Me.txtDelFlg.Attributes("onkeyPress") = "CheckNum()"                   '削除フラグ
        Me.txtToriCode.Attributes("onkeyPress") = "CheckNum()"                 '取引先コード
        Me.txtDepStation.Attributes("onkeyPress") = "CheckNum()"               '発駅コード
        Me.txtVolIncentAmo.Attributes("onkeyPress") = "CheckNum()"             'ボリュームインセンティブ料金

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
            & "   , DEPSTATION                 " _
            & " FROM                           " _
            & "     LNG.LNM0026_INCENTIVE      " _
            & " WHERE                          " _
            & "         TORICODE        = @P1  " _
            & "     AND DEPSTATION      = @P2  " _
            & "     AND DELFLG         <> @P3  "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 5) '取引先コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 6) '発駅コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 1) '削除フラグ

                PARA1.Value = txtToriCode.Text
                PARA2.Value = txtDepStation.Text
                PARA3.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0026Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0026Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0026Chk.Load(SQLdr)

                    If LNM0026Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0026D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0026D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub


    ''' <summary>
    ''' 排他チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO"></param>
    Protected Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String)

        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                                      " _
            & "     TORICODE                                " _
            & "   , DEPSTATION                              " _
            & "   , UPDTIMSTP                               " _
            & " FROM                                        " _
            & "     LNG.LNM0026_INCENTIVE                   " _
            & " WHERE                                       " _
            & "         TORICODE        = @P1               " _
            & "     AND DEPSTATION      = @P2               "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 10) '取引先コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 6) '発駅コード

                PARA1.Value = txtToriCode.Text
                PARA2.Value = txtDepStation.Text

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0026Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0026Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0026Chk.Load(SQLdr)

                    If LNM0026Chk.Rows.Count > 0 Then
                        Dim LNM0026row As DataRow
                        LNM0026row = LNM0026Chk.Rows(0)
                        If Not LNM0026row("UPDTIMSTP").ToString = "" Then                                 'タイムスタンプ
                            If LNM0026row("UPDTIMSTP").ToString <> work.WF_SEL_UPDTIMSTP.Text Then
                                ' 排他エラー
                                O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                            End If
                        End If
                    Else
                        ' 排他エラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0026D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0026D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' ボリュームインセンティブマスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(ボリュームインセンティブマスタ)
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;                 " _
            & "     SET @hensuu = 0 ;                       " _
            & " DECLARE hensuu CURSOR FOR                   " _
            & "     SELECT                                  " _
            & "         UPDTIMSTP AS hensuu                 " _
            & "     FROM                                    " _
            & "         LNG.LNM0026_INCENTIVE               " _
            & "     WHERE                                   " _
            & "             TORICODE           = @P01       " _
            & "         AND DEPSTATION         = @P02 ;     " _
            & " OPEN hensuu ;                               " _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;       " _
            & " IF (@@FETCH_STATUS = 0)                     " _
            & "     UPDATE LNG.LNM0026_INCENTIVE            " _
            & "     SET                                     " _
            & "         VOLINCENTAMO           = @P03       " _
            & "       , DELFLG                 = @P04       " _
            & "       , UPDYMD                 = @P05       " _
            & "       , UPDUSER                = @P06       " _
            & "       , UPDTERMID              = @P07       " _
            & "       , UPDPGID                = @P08       " _
            & "       , RECEIVEYMD             = @P09       " _
            & "     WHERE                                   " _
            & "             TORICODE           = @P01       " _
            & "         AND DEPSTATION         = @P02 ;     " _
            & " IF (@@FETCH_STATUS <> 0)                    " _
            & "     INSERT INTO LNG.LNM0026_INCENTIVE       " _
            & "        (TORICODE                            " _
            & "       , DEPSTATION                          " _
            & "       , VOLINCENTAMO                        " _
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
            & "       , @P13) ;                             " _
            & " CLOSE hensuu ;                              " _
            & " DEALLOCATE hensuu ;                         "

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                      " _
            & "     DELFLG                                  " _
            & "   , TORICODE                                " _
            & "   , DEPSTATION                              " _
            & "   , VOLINCENTAMO                            " _
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
            & "     LNG.LNM0026_INCENTIVE                   " _
            & " WHERE                                       " _
            & "             TORICODE              = @P01    " _
            & "         AND DEPSTATION            = @P02    "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                ' DB更新用パラメータ
                Dim PARA001 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 10)        '取引先コード
                Dim PARA002 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 6)         '発駅コード
                Dim PARA003 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.Decimal, 9)          'ボリュームインセンティブ料金
                Dim PARA004 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim PARA005 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.DateTime)            '登録年月日
                Dim PARA006 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar, 20)        '登録ユーザーＩＤ
                Dim PARA007 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar, 20)        '登録端末
                Dim PARA008 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.VarChar, 40)        '登録プログラムＩＤ
                Dim PARA009 As MySqlParameter = SQLcmd.Parameters.Add("@P09", MySqlDbType.DateTime)            '更新年月日
                Dim PARA010 As MySqlParameter = SQLcmd.Parameters.Add("@P10", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim PARA011 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.VarChar, 20)        '更新端末
                Dim PARA012 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ
                Dim PARA013 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.DateTime)            '集信日時


                ' 更新ジャーナル出力用パラメータ
                Dim JPARA001 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 10)    '取引先コード
                Dim JPARA002 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P02", MySqlDbType.VarChar, 6)     '発駅コード

                Dim LNM0026row As DataRow = LNM0026INPtbl.Rows(0)

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA001.Value = LNM0026row("TORICODE")                                           '取引先コード
                PARA002.Value = LNM0026row("DEPSTATION")                                         '発駅コード

                If String.IsNullOrEmpty(LNM0026row("VOLINCENTAMO")) Then
                    PARA003.Value = DBNull.Value                                                 'ボリュームインセンティブ料金
                Else
                    PARA003.Value = LNM0026row("VOLINCENTAMO")                                   'ボリュームインセンティブ料金
                End If

                PARA004.Value = LNM0026row("DELFLG")                                             '削除フラグ                   
                PARA005.Value = WW_DateNow                                                       '登録年月日                   
                PARA006.Value = Master.USERID                                                    '登録ユーザーＩＤ             
                PARA007.Value = Master.USERTERMID                                                '登録端末                     
                PARA008.Value = Me.GetType().BaseType.Name                                       '登録プログラムＩＤ           
                PARA009.Value = WW_DateNow                                                       '更新年月日                   
                PARA010.Value = Master.USERID                                                    '更新ユーザーＩＤ            
                PARA011.Value = Master.USERTERMID                                                '更新端末                    
                PARA012.Value = Me.GetType().BaseType.Name                                       '更新プログラムＩＤ          
                PARA013.Value = C_DEFAULT_YMD                                                    '集信日時                     
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA001.Value = LNM0026row("TORICODE")
                JPARA002.Value = LNM0026row("DEPSTATION")


                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0026UPDtbl) Then
                        LNM0026UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0026UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0026UPDtbl.Clear()
                    LNM0026UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0026UPDrow As DataRow In LNM0026UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0026D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0026UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0026D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0026D UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

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
        DetailBoxToLNM0026INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0026tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0026tbl, work.WF_SEL_INPTBL.Text)

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
    Protected Sub DetailBoxToLNM0026INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(txtDelFlg.Text)                      '削除フラグ
        Master.EraseCharToIgnore(txtToriCode.Text)                    '取引先コード
        Master.EraseCharToIgnore(txtDepStation.Text)                  '発駅コード
        Master.EraseCharToIgnore(txtVolIncentAmo.Text)                'ボリュームインセンティブ料金


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

        Master.CreateEmptyTable(LNM0026INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0026INProw As DataRow = LNM0026INPtbl.NewRow

        'LINECNT
        If lblSelLineCNT.Text = "" Then
            LNM0026INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(lblSelLineCNT.Text, LNM0026INProw("LINECNT"))
            Catch ex As Exception
                LNM0026INProw("LINECNT") = 0
            End Try
        End If

        LNM0026INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0026INProw("UPDTIMSTP") = 0
        LNM0026INProw("SELECT") = 1
        LNM0026INProw("HIDDEN") = 0

        LNM0026INProw("TORICODE") = txtToriCode.Text                                '取引先コード
        LNM0026INProw("DEPSTATION") = txtDepStation.Text                            '発駅コード
        LNM0026INProw("VOLINCENTAMO") = txtVolIncentAmo.Text                        'ボリュームインセンティブ料金

        LNM0026INProw("DELFLG") = txtDelFlg.Text                                    '削除フラグ
        LNM0026INProw("UPDYMD") = Date.Now                                          '更新日付

        '○ チェック用テーブルに登録する
        LNM0026INPtbl.Rows.Add(LNM0026INProw)

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
        DetailBoxToLNM0026INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0026INProw As DataRow = LNM0026INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0026row As DataRow In LNM0026tbl.Rows
            ' KEY項目が等しい時
            If LNM0026row("TORICODE") = LNM0026INProw("TORICODE") AndAlso
                LNM0026row("DEPSTATION") = LNM0026INProw("DEPSTATION") Then
                ' KEY項目以外の項目の差異をチェック
                If LNM0026row("VOLINCENTAMO") = LNM0026INProw("VOLINCENTAMO") AndAlso
                    LNM0026row("DELFLG") = LNM0026INProw("DELFLG") Then
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
        For Each LNM0026row As DataRow In LNM0026tbl.Rows
            Select Case LNM0026row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0026row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0026row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0026row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0026row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0026row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0026tbl, work.WF_SEL_INPTBL.Text)

        lblSelLineCNT.Text = ""               'LINECNT
        txtMapId.Text = "M00001"              '画面ＩＤ

        txtToriCode.Text = ""                 '取引先コード
        txtDepStation.Text = ""               '発駅コード
        txtVolIncentAmo.Text = ""             'ボリュームインセンティブ料金

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
                ' フィールドによってパラメータを変える
                Select Case WF_FIELD.Value
                    Case "txtToriCode"               '取引先コード
                        WW_PrmData = work.CreateKekkjmParam(GL0018InvKesaiKbnList.LS_INVOICE_WITH.TORICODE)
                    Case "txtDepSation"          '発駅コード
                        WW_PrmData = work.CreateStationParam(Master.USERCAMP)
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
            Case "txtDepStation"         '請求書提出部店
                CODENAME_get("INVFILINGDEPT", txtDepStation.Text, lblDepStationName.Text, WW_Dummy)
                txtDepStation.Focus()
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
                Case "txtDepStation"                             '請求書提出部店
                    txtDepStation.Text = WW_SelectValue
                    lblDepStationName.Text = WW_SelectText
                    txtDepStation.Focus()

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
                Case "txtDepStation"                   '発駅コード
                    txtDepStation.Focus()
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


        For Each LNM0026INProw As DataRow In LNM0026INPtbl.Rows
            '○ 単項目チェック

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0026INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 値存在チェック
                CODENAME_get("DELFLG", LNM0026INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "TORICODE", LNM0026INProw("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 値存在チェック
                CODENAME_get("TORICODE", LNM0026INProw("TORICODE"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・取引先コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・取引先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 発駅コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPSTATION", LNM0026INProw("DEPSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 値存在チェック
                CODENAME_get("DEPSTATION", LNM0026INProw("DEPSTATION"), WW_Dummy, WW_RtnSW)
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




            ' 排他チェック
            If Not work.WF_SEL_TORICODE.Text = "" Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    HaitaCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（取引先コード & 発駅コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0026INProw("TORICODE") & "]" &
                                           " [" & LNM0026INProw("DEPSTATION") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNM0026INProw("TORICODE") = work.WF_SEL_TORICODE.Text OrElse
               Not LNM0026INProw("DEPSTATION") = work.WF_SEL_DEPSTATION.Text Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（取引先コード & 発駅コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                           "([" & LNM0026INProw("TORICODE") & "]" &
                                           " [" & LNM0026INProw("DEPSTATION") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LineErr = "" Then
                If LNM0026INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0026INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0026INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0026INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0026tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0026tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0026row As DataRow In LNM0026tbl.Rows
            Select Case LNM0026row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0026row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0026row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0026row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0026row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0026row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0026INProw As DataRow In LNM0026INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0026INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0026INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0026row As DataRow In LNM0026tbl.Rows
                ' KEY項目が等しい時
                If LNM0026row("TORICODE") = LNM0026INProw("TORICODE") AndAlso
                   LNM0026row("DEPSTATION") = LNM0026INProw("DEPSTATION") Then
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0026row("VOLINCENTAMO") = LNM0026INProw("VOLINCENTAMO") AndAlso
                        LNM0026row("DELFLG") = LNM0026INProw("DELFLG") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0026row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0026INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0026INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0026INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0026INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0026INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' マスタ更新
                UpdateMaster(SQLcon)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If
                work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = "Update Success!!"
            End Using
        End If

        '○ 変更有無判定 & 入力値反映
        For Each LNM0026INProw As DataRow In LNM0026INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0026row As DataRow In LNM0026tbl.Rows
                ' 同一レコードか判定
                If LNM0026INProw("TORICODE") = LNM0026row("TORICODE") AndAlso
                    LNM0026INProw("DEPSTATION") = LNM0026row("DEPSTATION") Then
                    ' 画面入力テーブル項目設定
                    LNM0026INProw("LINECNT") = LNM0026row("LINECNT")
                    LNM0026INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0026INProw("UPDTIMSTP") = LNM0026row("UPDTIMSTP")
                    LNM0026INProw("SELECT") = 0
                    LNM0026INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0026row.ItemArray = LNM0026INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0026tbl.NewRow
                WW_NRow.ItemArray = LNM0026INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0026tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0026tbl.Rows.Add(WW_NRow)
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
                Case "DEPSTATION"             '発駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
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
