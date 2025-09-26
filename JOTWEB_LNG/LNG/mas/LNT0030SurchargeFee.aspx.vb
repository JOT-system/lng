''************************************************************
' サーチャージ料金画面
' 作成日 2025/08/26
' 更新日 
' 作成者 三宅
' 更新者 
'
''************************************************************
Imports MySql.Data.MySqlClient
Imports System.IO
Imports JOTWEB_LNG.GRIS0005LeftBox
Imports GrapeCity.Documents.Excel
Imports System.Drawing

''' <summary>
''' サーチャージ料金登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNT0030SurchargeFee
    Inherits Page

    '○ 検索結果格納Table
    Private LNT0030tbl As DataTable         '一覧格納用テーブル
    Private LNT0030INPtbl As DataTable      '入力格納用テーブル
    Private LNT0030UPDtbl As DataTable      '更新用テーブル
    Private UploadFileTbl As New DataTable    '添付ファイルテーブル
    Private LNT0030Exceltbl As New DataTable  'Excelデータ格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 19                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 19                 'マウススクロール時稼働行数

    '〇 帳票用
    Private Const CONST_COLOR_HATCHING_REQUIRED As String = "#FFFF00" '入力必須網掛け色
    Private Const CONST_COLOR_HATCHING_UNNECESSARY As String = "#BFBFBF" '入力不要網掛け色
    Private Const CONST_COLOR_HATCHING_HEADER As String = "#002060" 'ヘッダ網掛け色
    Private Const CONST_COLOR_FONT_HEADER As String = "#FFFFFF" 'ヘッダフォント色
    Private Const CONST_COLOR_BLACK As String = "#000000" '黒
    Private Const CONST_COLOR_GRAY As String = "#808080" '灰色
    Private Const CONST_HEIGHT_PER_ROW As Integer = 14 'セルのコメントの一行あたりの高さ
    Private Const CONST_DATA_START_ROW As Integer = 3 'データ開始行
    Private Const CONST_PULLDOWNSHEETNAME = "PULLLIST"
    Private Const CONST_INSERT As String = "Insert"                 'データ追加
    Private Const CONST_UPDATE As String = "Update"                 'データ更新

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    '○ 共通処理結果
    Private WW_ErrSW As String = ""
    Private WW_RtnSW As String = ""
    Private WW_Dummy As String = ""
    Private WW_ErrCode As String                                    'サブ用リターンコード

    '〇 共通定数
    Private Const WW_COLUMNCOUNT As Integer = 21                          'スプレッド列数

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
                    Master.RecoverTable(LNT0030tbl)
                    '○ 画面編集データ取得＆保存(サーバー側で設定した内容を取得し保存する。)
                    If CS0013ProfView.SetDispListTextBoxValues(LNT0030tbl, pnlListArea) Then
                        Master.SaveTable(LNT0030tbl)
                    End If

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonINSERT"                      '行追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_Field_DBClick"                     '届先選択ボタン、一覧の選択（カレンダー、虫眼鏡）押下（共通部品（フィールドダブルクリックを利用））
                            WF_FIELD_DBClick()
                        Case "WF_ListChange"                        '届先選択ボタン、一覧の選択（カレンダー、虫眼鏡）押下（共通部品（フィールドダブルクリックを利用））
                            WF_ListChange(WW_ErrSW)
                        Case "WF_ButtonSel"                         '(左ボックス)選択ボタン押下
                            WF_ButtonSel()
                        Case "mspTodokeCodeConfirmAdd"              '届先複数選択ポップアップより追加ボタン押下
                            RowSelected_mspTodokeCodeMulti()
                        Case "mspTodokeCodeRowSelected"             '届先ポップアップより届先選択
                            RowSelected_mspTodokeCodeSingle()
                        Case "mspShukabashoRowSelected"             '出荷場所ポップアップより出荷場所選択
                            RowSelected_mspShukabashoSingle()
                        Case "WF_CheckBoxSELECT"                    '一覧の削除チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click()
                        Case "WF_ButtonALLSELECT"                   '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonALLREJECT"                   '選択解除ボタン押下
                            WF_ButtonALLREJECT_Click()
                        Case "WF_ButtonUPDATE"                      '変更ボタン押下
                            WF_UPDATE_Click(WW_ErrSW)
                            If WW_ErrSW = C_MESSAGE_NO.NORMAL Then
                                GridViewInitialize()
                            Else
                                DisplayGrid()
                            End If
                        Case "WF_ButtonCan"                         '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ButtonDOWNLOAD"                    'ダウンロードボタン押下
                            WF_EXCELPDF(LNT0030WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"                       '一覧印刷ボタン押下
                            WF_EXCELPDF(LNT0030WRKINC.FILETYPE.PDF)
                        Case "WF_ButtonEND", "LNM0019L"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonUPLOAD"                      'ｱｯﾌﾟﾛｰﾄﾞボタン押下
                            WF_ButtonUPLOAD_Click()
                            GridViewInitialize()
                        Case "WF_SelectCALENDARChange"              '請求年月(変更)時
                            GridViewInitialize()
                    End Select

                    '○ 一覧再表示処理
                    If Not WF_ButtonClick.Value = "WF_ButtonUPLOAD" AndAlso
                       Not WF_ButtonClick.Value = "WF_ButtonUPDATE" AndAlso
                       Not WF_ButtonClick.Value = "btnCommonConfirmOk" AndAlso
                       Not WF_ButtonClick.Value = "WF_SelectCALENDARChange" Then
                        DisplayGrid()
                    End If

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

        Finally
            '○ 格納Table Close
            If Not IsNothing(LNT0030tbl) Then
                LNT0030tbl.Clear()
                LNT0030tbl.Dispose()
                LNT0030tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNT0030WRKINC.MAPIDL
        '○ HELP表示有無設定
        Master.dispHelp = False
        '○ D&D有無設定
        Master.eventDrop = True
        '○ Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()
        '○ 入力情報（INPtbl）保存先のファイル名
        WW_CreateXMLSaveFile()

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
        rightview.Initialize("")

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ GridView初期設定
        GridViewInitialize()

        '届先選択ボタン制御
        If work.WF_SEL_SURCHARGEPATTERNCODE.Text = "01" OrElse
           work.WF_SEL_SURCHARGEPATTERNCODE.Text = "05" Then
            'WF_ButtonTODOKE.Disabled = True
        End If

        '〇 更新画面からの遷移もしくは、アップロード完了の場合、更新完了メッセージを出力
        If Not String.IsNullOrEmpty(work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text) Then
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)
            work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""
        End If

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        If String.IsNullOrEmpty(Master.VIEWID) Then
            rightview2.MAPIDS = Master.MAPID
            rightview2.MAPID = Master.MAPID
            rightview2.COMPCODE = Master.USERCAMP
            rightview2.MAPVARI = Master.MAPvariant
            rightview2.PROFID = Master.PROF_VIEW
            rightview2.MENUROLE = Master.ROLE_MENU
            rightview2.MAPROLE = Master.ROLE_MAP
            rightview2.VIEWROLE = Master.ROLE_VIEWPROF
            rightview2.RPRTROLE = Master.ROLE_RPRTPROF
            rightview2.Initialize("画面レイアウト設定", WW_Dummy)
            Master.VIEWID = rightview2.GetViewId(Master.USERCAMP)
        End If

        WF_BILLINGCYCLENAME.Text = work.WF_SEL_BILLINGCYCLENAME.Text
        WF_BILLINGCYCLE.Text = work.WF_SEL_BILLINGCYCLE.Text
        WF_SURCHARGEPATTERNNAME.Text = work.WF_SEL_SURCHARGEPATTERNNAME.Text
        WF_SURCHARGEPATTERNCODE.Text = work.WF_SEL_SURCHARGEPATTERNCODE.Text
        WF_CALCMETHODNAME.Text = work.WF_SEL_CALCMETHODNAME.Text
        WF_CALCMETHOD.Text = work.WF_SEL_CALCMETHOD.Text
        WF_TORINAME.Text = work.WF_SEL_TORINAME.Text
        WF_TORICODE.Text = work.WF_SEL_TORICODE.Text
        WF_ORGNAME.Text = work.WF_SEL_ORGNAME.Text
        WF_ORGCODE.Text = work.WF_SEL_ORGCODE.Text

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()
            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0030tbl)
        Master.SaveTable(LNT0030tbl, work.WF_SEL_INPTBL.Text)

        '〇 一覧ヘッダを設定
        Me.ListCount.Text = "件数：" + LNT0030tbl.Rows.Count.ToString()
        WF_BILLINGCYCLENAME.Text = work.WF_SEL_BILLINGCYCLENAME.Text
        WF_BILLINGCYCLE.Text = work.WF_SEL_BILLINGCYCLE.Text
        WF_SURCHARGEPATTERNNAME.Text = work.WF_SEL_SURCHARGEPATTERNNAME.Text
        WF_SURCHARGEPATTERNCODE.Text = work.WF_SEL_SURCHARGEPATTERNCODE.Text
        WF_CALCMETHODNAME.Text = work.WF_SEL_CALCMETHODNAME.Text
        WF_CALCMETHOD.Text = work.WF_SEL_CALCMETHOD.Text
        WF_TORINAME.Text = work.WF_SEL_TORINAME.Text
        WF_TORICODE.Text = work.WF_SEL_TORICODE.Text
        WF_ORGNAME.Text = work.WF_SEL_ORGNAME.Text
        WF_ORGCODE.Text = work.WF_SEL_ORGCODE.Text

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNT0030tbl)
        Dim WW_RowFilterCMD As New StringBuilder
        WW_RowFilterCMD.Append("LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT)

        TBLview.RowFilter = WW_RowFilterCMD.ToString

        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        'CS0013ProfView.LEVENT = "Onchange"
        'CS0013ProfView.LFUNC = "ListChange"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As MySqlConnection)

        If IsNothing(LNT0030tbl) Then
            LNT0030tbl = New DataTable
        End If

        If LNT0030tbl.Columns.Count <> 0 Then
            LNT0030tbl.Columns.Clear()
        End If

        LNT0030tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをサーチャージ料金から取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                              ")
        SQLStr.AppendLine("     1                                                                      AS 'SELECT'              ")
        SQLStr.AppendLine("   , 0                                                                      AS HIDDEN                ")
        SQLStr.AppendLine("   , 0                                                                      AS LINECNT               ")
        SQLStr.AppendLine("   , ''                                                                     AS OPERATION             ")
        SQLStr.AppendLine("   , ''                                                                     AS OPERATIONCB           ")
        SQLStr.AppendLine("   , LNT0030.UPDTIMSTP                                                      AS UPDTIMSTP             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.SEIKYUYM), '')                                  AS SEIKYUYM              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.SEIKYUBRANCH), '')                              AS SEIKYUBRANCH          ")
        SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(SEIKYUDATEFROM, '%Y/%m/%d'), '')                  AS SEIKYUDATEFROM        ")
        SQLStr.AppendLine("   , COALESCE(DATE_FORMAT(SEIKYUDATETO, '%Y/%m/%d'), '')                    AS SEIKYUDATETO          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.TORICODE), '')                                  AS TORICODE              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.TORINAME), '')                                  AS TORINAME              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.ORGCODE), '')                                   AS ORGCODE               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.ORGNAME), '')                                   AS ORGNAME               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.KASANORGCODE), '')                              AS KASANORGCODE          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.KASANORGNAME), '')                              AS KASANORGNAME          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.PATTERNCODE), '')                               AS PATTERNCODE           ")
        SQLStr.AppendLine("   , ''                                                                     AS PATTERNNAME           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.AVOCADOSHUKABASHO), '')                         AS AVOCADOSHUKABASHO     ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.AVOCADOSHUKANAME), '')                          AS AVOCADOSHUKANAME      ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.AVOCADOTODOKECODE), '')                         AS AVOCADOTODOKECODE     ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.AVOCADOTODOKENAME), '')                         AS AVOCADOTODOKENAME     ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.SHAGATA), '')                                   AS SHAGATA               ")
        SQLStr.AppendLine("   , CASE LNT0030.SHAGATA WHEN '1' THEN '単車' ELSE 'トレーラ' END          AS SHAGATANAME           ")
        SQLStr.AppendLine("   , COALESCE(LNT0030.SHABARA, '0.000')                                     AS SHABARA               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.SHABAN), '')                                    AS SHABAN                ")
        SQLStr.AppendLine("   , COALESCE(LNT0030.DIESELPRICESTANDARD, '0.00')                          AS DIESELPRICESTANDARD   ")
        SQLStr.AppendLine("   , COALESCE(LNT0030.DIESELPRICECURRENT, '0.00')                           AS DIESELPRICECURRENT    ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.CALCMETHOD), '')                                AS CALCMETHOD            ")
        SQLStr.AppendLine("   , ''                                                                     AS CALCMETHODNAME        ")
        SQLStr.AppendLine("   , COALESCE(LNT0030.DISTANCE, '0.00')                                     AS DISTANCE              ")
        SQLStr.AppendLine("   , COALESCE(LNT0030.SHIPPINGCOUNT, '0')                                   AS SHIPPINGCOUNT         ")
        SQLStr.AppendLine("   , COALESCE(LNT0030.NENPI, '0.00')                                        AS NENPI                 ")
        SQLStr.AppendLine("   , COALESCE(LNT0030.FUELBASE, '0.00')                                     AS FUELBASE              ")
        SQLStr.AppendLine("   , COALESCE(LNT0030.FUELRESULT, '0.00')                                   AS FUELRESULT            ")
        SQLStr.AppendLine("   , COALESCE(LNT0030.ADJUSTMENT, '0.00')                                   AS ADJUSTMENT            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.MEMO), '')                                      AS MEMO                  ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNT0030.DELFLG), '')                                    AS DELFLG                ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(LNS0006.VALUE1), '')                                    AS DELFLGNAME            ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.LNT0030_SURCHARGEFEE LNT0030                                                                ")
        SQLStr.AppendLine(" LEFT JOIN                                                                                           ")
        SQLStr.AppendLine("    (                                                                                                ")
        SQLStr.AppendLine("      SELECT                                                                                         ")
        SQLStr.AppendLine("          KEYCODE                                                                                    ")
        SQLStr.AppendLine("         ,VALUE1                                                                                     ")
        SQLStr.AppendLine("      FROM                                                                                           ")
        SQLStr.AppendLine("          COM.LNS0006_FIXVALUE                                                                       ")
        SQLStr.AppendLine("      WHERE                                                                                          ")
        SQLStr.AppendLine("          CAMPCODE = @CAMPCODE                                                                       ")
        SQLStr.AppendLine("      AND CLASS = 'DELFLG'                                                                           ")
        SQLStr.AppendLine("      AND CURDATE() BETWEEN STYMD AND ENDYMD                                                         ")
        SQLStr.AppendLine("      AND DELFLG <> '1'                                                                              ")
        SQLStr.AppendLine("    ) LNS0006                                                                                        ")
        SQLStr.AppendLine("      ON  LNT0030.DELFLG = LNS0006.KEYCODE                                                           ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     LNT0030.SEIKYUYM        like CONCAT(@SEIKYUYM, '%')                                             ")
        SQLStr.AppendLine(" AND LNT0030.TORICODE           = @TORICODE                                                          ")
        SQLStr.AppendLine(" AND LNT0030.ORGCODE            = @ORGCODE                                                           ")
        SQLStr.AppendLine(" AND LNT0030.PATTERNCODE        = @PATTERNCODE                                                       ")
        SQLStr.AppendLine(" AND LNT0030.DELFLG = '0'                                                                            ")
        SQLStr.AppendLine(" ORDER BY                                                                                            ")
        SQLStr.AppendLine("     LNT0030.SEIKYUYM                                                                                ")
        SQLStr.AppendLine("    ,LNT0030.SEIKYUDATEFROM                                                                          ")
        SQLStr.AppendLine("    ,LNT0030.SEIKYUDATETO                                                                            ")
        SQLStr.AppendLine("    ,LNT0030.TORICODE                                                                                ")
        SQLStr.AppendLine("    ,LNT0030.ORGCODE                                                                                 ")
        SQLStr.AppendLine("    ,LNT0030.PATTERNCODE                                                                             ")
        SQLStr.AppendLine("    ,LNT0030.AVOCADOSHUKABASHO                                                                       ")
        SQLStr.AppendLine("    ,LNT0030.AVOCADOTODOKECODE                                                                       ")
        SQLStr.AppendLine("    ,LNT0030.SHAGATA                                                                                 ")
        SQLStr.AppendLine("    ,LNT0030.SHABARA                                                                                 ")
        SQLStr.AppendLine("    ,LNT0030.SHABAN                                                                                  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                '会社
                Dim P_CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar, 20)
                P_CAMPCODE.Value = Master.USERCAMP


                Dim P_SEIKYUYM As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUYM", MySqlDbType.VarChar, 6)           '請求年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)             '部門コード
                Dim P_PATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@PATTERNCODE", MySqlDbType.VarChar, 2)     'パターンコード

                If String.IsNullOrEmpty(WF_SeikyuYm.Value) Then
                    P_SEIKYUYM.Value = ""
                Else
                    P_SEIKYUYM.Value = WF_SeikyuYm.Value.Replace("/", "")
                End If
                P_TORICODE.Value = work.WF_SEL_TORICODE.Text
                P_ORGCODE.Value = work.WF_SEL_ORGCODE.Text
                P_PATTERNCODE.Value = work.WF_SEL_SURCHARGEPATTERNCODE.Text

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0030tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0030tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNT0030row As DataRow In LNT0030tbl.Rows
                    i += 1
                    LNT0030row("LINECNT") = i        'LINECNT
                Next
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0030_SURCHARGEFEE SELECT")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0030_SURCHARGEFEE Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ' ******************************************************************************
    ' ***  ボタン押下処理                                                        ***
    ' ******************************************************************************
    ''' <summary>
    ''' 行追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        Dim LNT0030row As DataRow = LNT0030tbl.NewRow
        LNT0030row("SELECT") = "1"
        LNT0030row("HIDDEN") = "0"
        LNT0030row("LINECNT") = LNT0030tbl.Rows.Count + 1
        LNT0030row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
        LNT0030row("UPDTIMSTP") = Date.Now
        LNT0030row("SEIKYUYM") = ""
        LNT0030row("SEIKYUBRANCH") = ""
        LNT0030row("SEIKYUDATEFROM") = DBNull.Value
        LNT0030row("SEIKYUDATETO") = DBNull.Value
        LNT0030row("TORICODE") = work.WF_SEL_TORICODE.Text
        LNT0030row("TORINAME") = work.WF_SEL_TORINAME.Text
        LNT0030row("ORGCODE") = work.WF_SEL_ORGCODE.Text
        LNT0030row("ORGNAME") = work.WF_SEL_ORGNAME.Text
        LNT0030row("KASANORGCODE") = work.WF_SEL_KASANORGCODE.Text
        LNT0030row("KASANORGNAME") = work.WF_SEL_KASANORGNAME.Text
        LNT0030row("PATTERNCODE") = work.WF_SEL_SURCHARGEPATTERNCODE.Text
        LNT0030row("PATTERNNAME") = work.WF_SEL_SURCHARGEPATTERNNAME.Text
        LNT0030row("AVOCADOSHUKABASHO") = ""
        LNT0030row("AVOCADOSHUKANAME") = ""
        LNT0030row("AVOCADOTODOKECODE") = ""
        LNT0030row("AVOCADOTODOKENAME") = ""
        LNT0030row("SHAGATA") = ""
        LNT0030row("SHAGATANAME") = ""
        LNT0030row("SHABARA") = "0.000"
        LNT0030row("SHABAN") = ""
        LNT0030row("DIESELPRICESTANDARD") = "0.00"
        LNT0030row("DIESELPRICECURRENT") = "0.00"
        LNT0030row("CALCMETHOD") = work.WF_SEL_CALCMETHOD.Text
        LNT0030row("CALCMETHODNAME") = work.WF_SEL_CALCMETHODNAME.Text
        LNT0030row("DISTANCE") = "0.00"
        LNT0030row("SHIPPINGCOUNT") = "0"
        LNT0030row("NENPI") = "0.00"
        LNT0030row("FUELBASE") = "0.00"
        LNT0030row("FUELRESULT") = "0.00"
        LNT0030row("ADJUSTMENT") = "0.00"
        LNT0030row("MEMO") = ""
        LNT0030row("DELFLG") = "0"
        LNT0030row("DELFLGNAME") = "有効"

        LNT0030tbl.Rows.Add(LNT0030row)

        '○ 画面表示データ保存
        Master.SaveTable(LNT0030tbl)
    End Sub
    ''' <summary>
    ''' 更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click(ByRef oRtn As String)

        oRtn = Messages.C_MESSAGE_NO.NORMAL

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNT0030INPtbl(oRtn)
        If Not isNormal(oRtn) Then
            Exit Sub
        End If

        '初期取得データの戻し
        Master.RecoverTable(LNT0030tbl, work.WF_SEL_INPTBL.Text)

        '変更チェック
        '○ 追加変更判定
        For Each LNT0030INProw As DataRow In LNT0030INPtbl.Rows

            ' 既存レコードとの比較
            For Each LNT0030row As DataRow In LNT0030tbl.Rows
                ' KEY項目が等しい時（請求年月は、変更される場合があるため判定せず、行番号で判断する。他のキーは念のためは念のため）
                If LNT0030row("TORICODE") = LNT0030INProw("TORICODE") AndAlso                                           '取引先コード
                    LNT0030row("ORGCODE") = LNT0030INProw("ORGCODE") AndAlso                                            '部門コード
                    LNT0030row("PATTERNCODE") = LNT0030INProw("PATTERNCODE") AndAlso                                    'パターンコード
                    LNT0030row("LINECNT") = LNT0030INProw("LINECNT") Then                                               '行番号
                    ' KEY項目以外の項目の差異をチェック
                    If LNT0030row("DELFLG") = LNT0030INProw("DELFLG") AndAlso
                        LNT0030row("SEIKYUYM") = LNT0030INProw("SEIKYUYM") AndAlso                                      '請求年月
                        LNT0030row("SEIKYUDATEFROM") = LNT0030INProw("SEIKYUDATEFROM") AndAlso                          '請求対象期間From
                        LNT0030row("SEIKYUDATETO") = LNT0030INProw("SEIKYUDATETO") AndAlso                              '請求対象期間To
                        LNT0030row("AVOCADOSHUKABASHO") = LNT0030INProw("AVOCADOSHUKABASHO") AndAlso                    '出荷場所コード
                        LNT0030row("AVOCADOTODOKECODE") = LNT0030INProw("AVOCADOTODOKECODE") AndAlso                    '届先コード
                        LNT0030row("SHAGATA") = LNT0030INProw("SHAGATA") AndAlso                                        '車型
                        LNT0030row("SHABARA") = LNT0030INProw("SHABARA") AndAlso                                        '車腹
                        LNT0030row("SHABAN") = LNT0030INProw("SHABAN") AndAlso                                          '車番
                        LNT0030row("DIESELPRICESTANDARD") = LNT0030INProw("DIESELPRICESTANDARD") AndAlso                '基準単価
                        LNT0030row("DIESELPRICECURRENT") = LNT0030INProw("DIESELPRICECURRENT") AndAlso                  '実勢単価
                        LNT0030row("DISTANCE") = LNT0030INProw("DISTANCE") AndAlso                                      '距離
                        LNT0030row("SHIPPINGCOUNT") = LNT0030INProw("SHIPPINGCOUNT") AndAlso                            '輸送回数
                        LNT0030row("NENPI") = LNT0030INProw("NENPI") AndAlso                                            '燃費
                        LNT0030row("FUELBASE") = LNT0030INProw("FUELBASE") AndAlso                                      '基準燃料使用量
                        LNT0030row("FUELRESULT") = LNT0030INProw("FUELRESULT") AndAlso                                  '燃料使用量
                        LNT0030row("ADJUSTMENT") = LNT0030INProw("ADJUSTMENT") AndAlso                                  '精算調整幅
                        LNT0030row("MEMO") = LNT0030INProw("MEMO") AndAlso                                              '計算式メモ
                        LNT0030row("DELFLG") = LNT0030INProw("DELFLG") Then                                             '削除フラグ

                        ' 変更がない時は「操作」の項目は空白にする
                        LNT0030INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNT0030INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    End If

                    Exit For
                End If
            Next
        Next

        ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
        Dim selStr As String = String.Format("OPERATION='{0}' or OPERATION='{1}'", C_LIST_OPERATION_CODE.UPDATING, C_LIST_OPERATION_CODE.INSERTING)
        Dim selRow() = LNT0030INPtbl.Select(selStr)
        If selRow.Count = 0 Then
            Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
            oRtn = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub
        End If

        '------------------------------------------
        'テーブル更新処理
        '------------------------------------------
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            SQLcon.Open()       'DataBase接続

            TblUpdate(SQLcon, LNT0030INPtbl, oRtn)
        End Using

        '更新結果を画面表示テーブルに反映
        For Each LNT0030INProw As DataRow In LNT0030INPtbl.Rows
            Dim findFlg As Boolean = False
            For Each LNT0030row As DataRow In LNT0030tbl.Rows
                ' KEY項目が等しい時、画面内容に入れ替える
                If LNT0030row("TORICODE") = LNT0030INProw("TORICODE") AndAlso                                           '取引先コード
                    LNT0030row("ORGCODE") = LNT0030INProw("ORGCODE") AndAlso                                            '部門コード
                    LNT0030row("PATTERNCODE") = LNT0030INProw("PATTERNCODE") AndAlso                                    'パターンコード
                    LNT0030row("LINECNT") = LNT0030INProw("LINECNT") Then                                               '行番号
                    LNT0030row.ItemArray = LNT0030INProw.ItemArray
                    findFlg = True
                    Exit For
                End If
            Next
            '存在しない場合、新規追加する
            If findFlg = False Then
                Dim LNT0030row As DataRow = LNT0030tbl.NewRow
                LNT0030row.ItemArray = LNT0030INProw.ItemArray
                LNT0030tbl.Rows.Add(LNT0030row)
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0030tbl, work.WF_SEL_INPTBL.Text)

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()
        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub
    ''' <summary>
    ''' 詳細画面-テーブル退避
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub DetailBoxToLNT0030INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        If IsNothing(LNT0030INPtbl) Then
            LNT0030INPtbl = LNT0030tbl.Clone
        Else
            LNT0030INPtbl.Clear()
        End If

        Dim WW_TEXT As String = ""
        Dim WW_DATATYPE As String = ""
        Dim WW_RESULT As Boolean

        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        For i As Integer = 0 To LNT0030tbl.Rows.Count - 1

            'If LNT0030tbl.Rows(i)("SEIKYUYM") = "" Then
            '    Continue For
            'End If

            Dim LNT0030INProw As DataRow = LNT0030INPtbl.NewRow
            LNT0030INProw.ItemArray = LNT0030tbl.Rows(i).ItemArray

            LNT0030INProw("SELECT") = 1
            LNT0030INProw("HIDDEN") = 0

            '請求年月
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEIKYUYM" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEIKYUYM" & (i + 1))).Replace("/", "")
                WW_DATATYPE = LNT0030INProw("SEIKYUYM").GetType.Name.ToString
                LNT0030INProw("SEIKYUYM") = LNT0030WRKINC.DataConvert("請求年月", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("SEIKYUYM"))
            Else
                LNT0030INProw("SEIKYUYM") = ""
            End If
            '請求対象期間From
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEIKYUDATEFROM" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEIKYUDATEFROM" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("SEIKYUDATEFROM").GetType.Name.ToString
                LNT0030INProw("SEIKYUDATEFROM") = LNT0030WRKINC.DataConvert("対象期間From", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("SEIKYUDATEFROM"))
            Else
                LNT0030INProw("SEIKYUDATEFROM") = ""
            End If
            '請求対象期間To
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEIKYUDATETO" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEIKYUDATETO" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("SEIKYUDATETO").GetType.Name.ToString
                LNT0030INProw("SEIKYUDATETO") = LNT0030WRKINC.DataConvert("対象期間To", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("SEIKYUDATETO"))
            Else
                LNT0030INProw("SEIKYUDATETO") = ""
            End If
            '出荷場所名
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "AVOCADOSHUKANAME" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "AVOCADOSHUKANAME" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("AVOCADOSHUKANAME").GetType.Name.ToString
                LNT0030INProw("AVOCADOSHUKANAME") = LNT0030WRKINC.DataConvert("出荷場所名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("AVOCADOSHUKANAME"))
            Else
                LNT0030INProw("AVOCADOSHUKANAME") = ""
            End If
            '出荷場所コード
            CODENAME_get("SHUKANAME", LNT0030INProw("AVOCADOSHUKANAME"), WW_TEXT, WW_RtnSW)
            LNT0030INProw("AVOCADOSHUKABASHO") = WW_TEXT
            '届先名
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "AVOCADOTODOKENAME" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "AVOCADOTODOKENAME" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("AVOCADOTODOKENAME").GetType.Name.ToString
                LNT0030INProw("AVOCADOTODOKENAME") = LNT0030WRKINC.DataConvert("届先名", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("AVOCADOTODOKENAME"))
            Else
                LNT0030INProw("AVOCADOTODOKENAME") = ""
            End If
            '届先コード
            CODENAME_get("TODOKENAME", LNT0030INProw("AVOCADOTODOKENAME"), WW_TEXT, WW_RtnSW)
            LNT0030INProw("AVOCADOTODOKECODE") = WW_TEXT
            '車型
            If Not IsNothing(Request.Form("ctl00$contents1$lbSHAGATASHAGATA" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("ctl00$contents1$lbSHAGATASHAGATA" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("SHAGATA").GetType.Name.ToString
                LNT0030INProw("SHAGATA") = LNT0030WRKINC.DataConvert("車型", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("SHAGATA"))
            Else
                LNT0030INProw("SHAGATA") = ""
            End If
            CODENAME_get("SHAGATA", LNT0030INProw("SHAGATA"), WW_TEXT, WW_RtnSW)
            LNT0030INProw("SHAGATANAME") = WW_TEXT
            '車腹
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHABARA" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHABARA" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("SHABARA").GetType.Name.ToString
                LNT0030INProw("SHABARA") = LNT0030WRKINC.DataConvert("車腹", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("SHABARA"))
            Else
                LNT0030INProw("SHABARA") = "0.000"
            End If
            '車番
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHABAN" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHABAN" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("SHABAN").GetType.Name.ToString
                LNT0030INProw("SHABAN") = LNT0030WRKINC.DataConvert("車番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("SHABAN"))
            Else
                LNT0030INProw("SHABAN") = ""
            End If
            '基準単価
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "DIESELPRICESTANDARD" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DIESELPRICESTANDARD" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("DIESELPRICESTANDARD").GetType.Name.ToString
                LNT0030INProw("DIESELPRICESTANDARD") = LNT0030WRKINC.DataConvert("基準単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("DIESELPRICESTANDARD"))
            Else
                LNT0030INProw("DIESELPRICESTANDARD") = "0.00"
            End If
            '実勢単価
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "DIESELPRICECURRENT" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DIESELPRICECURRENT" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("DIESELPRICECURRENT").GetType.Name.ToString
                LNT0030INProw("DIESELPRICECURRENT") = LNT0030WRKINC.DataConvert("実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("DIESELPRICECURRENT"))
            Else
                LNT0030INProw("DIESELPRICECURRENT") = "0.00"
            End If
            '距離
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "DISTANCE" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DISTANCE" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("DISTANCE").GetType.Name.ToString
                LNT0030INProw("DISTANCE") = LNT0030WRKINC.DataConvert("距離", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("DISTANCE"))
            Else
                LNT0030INProw("DISTANCE") = "0.00"
            End If
            '輸送回数
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHIPPINGCOUNT" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHIPPINGCOUNT" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("SHIPPINGCOUNT").GetType.Name.ToString
                LNT0030INProw("SHIPPINGCOUNT") = LNT0030WRKINC.DataConvert("輸送回数", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("SHIPPINGCOUNT"))
            Else
                LNT0030INProw("SHIPPINGCOUNT") = "0"
            End If
            '燃費
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "NENPI" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "NENPI" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("NENPI").GetType.Name.ToString
                LNT0030INProw("NENPI") = LNT0030WRKINC.DataConvert("燃費", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("NENPI"))
            Else
                LNT0030INProw("NENPI") = "0.00"
            End If
            '基準燃料使用量
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "FUELBASE" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "FUELBASE" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("FUELBASE").GetType.Name.ToString
                LNT0030INProw("FUELBASE") = LNT0030WRKINC.DataConvert("基準燃料使用量", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("FUELBASE"))
            Else
                LNT0030INProw("FUELBASE") = "0.00"
            End If
            '燃料使用量
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "FUELRESULT" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "FUELRESULT" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("FUELRESULT").GetType.Name.ToString
                LNT0030INProw("FUELRESULT") = LNT0030WRKINC.DataConvert("燃料使用量", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("FUELRESULT"))
            Else
                LNT0030INProw("FUELRESULT") = "0.00"
            End If
            '精算調整幅
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "ADJUSTMENT" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "ADJUSTMENT" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("ADJUSTMENT").GetType.Name.ToString
                LNT0030INProw("ADJUSTMENT") = LNT0030WRKINC.DataConvert("精算調整幅", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("ADJUSTMENT"))
            Else
                LNT0030INProw("ADJUSTMENT") = "0.00"
            End If
            '計算式メモ
            If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "MEMO" & (i + 1))) Then
                WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "MEMO" & (i + 1)))
                WW_DATATYPE = LNT0030INProw("MEMO").GetType.Name.ToString
                LNT0030INProw("MEMO") = LNT0030WRKINC.DataConvert("計算式メモ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(LNT0030INProw("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                Master.EraseCharToIgnore(LNT0030INProw("MEMO"))
            Else
                LNT0030INProw("MEMO") = ""
            End If

            LNT0030INPtbl.Rows.Add(LNT0030INProw)
        Next

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNT0030row As DataRow In LNT0030tbl.Rows
            If LNT0030row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNT0030row("SELECT") = WW_DataCNT
            End If
        Next

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNT0030tbl.Rows.Count.ToString()
        WF_BILLINGCYCLENAME.Text = work.WF_SEL_BILLINGCYCLENAME.Text
        WF_BILLINGCYCLE.Text = work.WF_SEL_BILLINGCYCLE.Text
        WF_SURCHARGEPATTERNNAME.Text = work.WF_SEL_SURCHARGEPATTERNNAME.Text
        WF_SURCHARGEPATTERNCODE.Text = work.WF_SEL_SURCHARGEPATTERNCODE.Text
        WF_CALCMETHODNAME.Text = work.WF_SEL_CALCMETHODNAME.Text
        WF_CALCMETHOD.Text = work.WF_SEL_CALCMETHOD.Text
        WF_TORINAME.Text = work.WF_SEL_TORINAME.Text
        WF_TORICODE.Text = work.WF_SEL_TORICODE.Text
        WF_ORGNAME.Text = work.WF_SEL_ORGNAME.Text
        WF_ORGCODE.Text = work.WF_SEL_ORGCODE.Text

        '○ 表示LINECNT取得
        If String.IsNullOrEmpty(WF_GridPosition.Text) Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○ 表示格納位置決定
        ' 表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If
        ' 表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNT0030tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        'CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.CS0013ProfView()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("SELECT")
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        'Master.TransitionPrevPage()
        Server.Transfer("~/LNG/mas/LNM0019SurchargePatternList.aspx")

    End Sub

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************
    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel()

        Dim WW_GridDBclick As Integer = 1
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_GridDBclick)
        Catch ex As Exception
            WW_GridDBclick = 1
        End Try

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If Not IsNothing(leftview.GetActiveValue) Then
            WW_SelectValue = leftview.GetActiveValue(0)
            WW_SelectText = leftview.GetActiveValue(1)
        End If

        Dim LNT0030row As DataRow = LNT0030tbl(WW_GridDBclick - 1)
        LNT0030row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

        ' フィールドによってパラメータを変える
        Select Case WF_FIELD.Value
            Case "SEIKYUDATEFROM"             '対象期間From
                Dim WW_DATE As Date
                'ポップアップ選択
                Try
                    Date.TryParse(WW_SelectValue, WW_DATE)
                    LNT0030row("SEIKYUDATEFROM") = WW_DATE.ToString("yyyy/MM/dd")
                Catch ex As Exception
                End Try
            Case "SEIKYUDATETO"               '対象期間To
                Dim WW_DATE As Date
                'ポップアップ選択
                Try
                    Date.TryParse(WW_SelectValue, WW_DATE)
                    LNT0030row("SEIKYUDATETO") = WW_DATE.ToString("yyyy/MM/dd")
                Catch ex As Exception
                End Try
            Case "AVOCADOTODOKENAME"    '届先
                'ポップアップ選択
                LNT0030row("AVOCADOTODOKENAME") = WW_SelectText
                LNT0030row("AVOCADOTODOKECODE") = WW_SelectValue
            Case "AVOCADOSHUKANAME"    '出荷場所
                'ポップアップ選択
                LNT0030row("AVOCADOSHUKANAME") = WW_SelectText
                LNT0030row("AVOCADOSHUKABASHO") = WW_SelectValue
        End Select

        '○ 画面表示データ保存
        Master.SaveTable(LNT0030tbl)

        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' リスト変更時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange(ByRef oRtn As String)

        oRtn = Messages.C_MESSAGE_NO.NORMAL

        Dim WW_TEXT As String = ""
        Dim WW_DATATYPE As String = ""
        Dim WW_RESULT As Boolean
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""

        Dim WW_GridDBclick As Integer = 1
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_GridDBclick)
        Catch ex As Exception
            WW_GridDBclick = 1
        End Try

        If WW_GridDBclick = 0 Then
            '一覧なし（データ0件）の場合
            Exit Sub
        End If

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""

        '○ 選択内容を取得
        If Not IsNothing(leftview.GetActiveValue) Then
            WW_SelectValue = leftview.GetActiveValue(0)
            WW_SelectText = leftview.GetActiveValue(1)
        End If

        Dim LNT0030row As DataRow = LNT0030tbl(WW_GridDBclick - 1)
        LNT0030row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING

        '請求年月
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEIKYUYM" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEIKYUYM" & (WW_GridDBclick))).Replace("/", "")
            WW_DATATYPE = LNT0030tbl.Rows(0)("SEIKYUYM").GetType.Name.ToString
            LNT0030row("SEIKYUYM") = LNT0030WRKINC.DataConvert("請求年月", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("SEIKYUYM"))
        Else
            LNT0030row("SEIKYUYM") = ""
        End If

        '対象期間From
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEIKYUDATEFROM" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEIKYUDATEFROM" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("SEIKYUDATEFROM").GetType.Name.ToString
            LNT0030row("SEIKYUDATEFROM") = LNT0030WRKINC.DataConvert("対象期間From", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("SEIKYUDATEFROM"))
        Else
            LNT0030row("SEIKYUDATEFROM") = DBNull.Value
        End If
        '対象期間To
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SEIKYUDATETO" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SEIKYUDATETO" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("SEIKYUDATETO").GetType.Name.ToString
            LNT0030row("SEIKYUDATETO") = LNT0030WRKINC.DataConvert("対象期間To", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("SEIKYUDATETO"))
        Else
            LNT0030row("SEIKYUDATETO") = DBNull.Value
        End If
        '届先
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "AVOCADOTODOKENAME" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "AVOCADOTODOKENAME" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("AVOCADOTODOKENAME").GetType.Name.ToString
            LNT0030row("AVOCADOTODOKENAME") = LNT0030WRKINC.DataConvert("届先", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            Else
                '届先コード取得
                Dim WW_CODE As String = ""
                CODENAME_get("TODOKENAME", Convert.ToString(Request.Form("txt" & pnlListArea.ID & "AVOCADOTODOKENAME" & (WW_GridDBclick))), WW_CODE, WW_RtnSW)
                If WW_CODE = "" Then
                    LNT0030row("AVOCADOTODOKECODE") = ""
                Else
                    LNT0030row("AVOCADOTODOKECODE") = WW_CODE
                End If
            End If
            Master.EraseCharToIgnore(LNT0030row("AVOCADOTODOKENAME"))
        Else
            LNT0030row("AVOCADOTODOKECODE") = ""
            LNT0030row("AVOCADOTODOKENAME") = ""
        End If
        '出荷場所
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "AVOCADOSHUKANAME" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "AVOCADOSHUKANAME" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("AVOCADOSHUKANAME").GetType.Name.ToString
            LNT0030row("AVOCADOSHUKANAME") = LNT0030WRKINC.DataConvert("出荷場所", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            Else
                '出荷場所コード取得
                Dim WW_CODE As String = ""
                CODENAME_get("SHUKANAME", Convert.ToString(Request.Form("txt" & pnlListArea.ID & "AVOCADOSHUKANAME" & (WW_GridDBclick))), WW_CODE, WW_RtnSW)
                If WW_CODE = "" Then
                    LNT0030row("AVOCADOSHUKABASHO") = ""
                Else
                    LNT0030row("AVOCADOSHUKABASHO") = WW_CODE
                End If
            End If
            Master.EraseCharToIgnore(LNT0030row("AVOCADOSHUKANAME"))
        Else
            LNT0030row("AVOCADOTODOKENAME") = ""
            LNT0030row("AVOCADOTODOKECODE") = ""
        End If
        '車型
        If Not IsNothing(Request.Form("ctl00$contents1$lbSHAGATASHAGATA" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("ctl00$contents1$lbSHAGATASHAGATA" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("SHAGATA").GetType.Name.ToString
            LNT0030row("SHAGATA") = LNT0030WRKINC.DataConvert("車型", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("SHAGATA"))
        Else
            LNT0030row("SHAGATA") = ""
        End If
        '車腹
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHABARA" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHABARA" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("SHABARA").GetType.Name.ToString
            LNT0030row("SHABARA") = LNT0030WRKINC.DataConvert("車腹", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("SHABARA"))
        Else
            LNT0030row("SHABARA") = "0.000"
        End If
        '車番
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHABAN" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHABAN" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("SHABAN").GetType.Name.ToString
            LNT0030row("SHABAN") = LNT0030WRKINC.DataConvert("車番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("SHABAN"))
        Else
            LNT0030row("SHABAN") = ""
        End If
        '基準単価
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "DIESELPRICESTANDARD" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DIESELPRICESTANDARD" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("DIESELPRICESTANDARD").GetType.Name.ToString
            LNT0030row("DIESELPRICESTANDARD") = LNT0030WRKINC.DataConvert("基準単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("DIESELPRICESTANDARD"))
        Else
            LNT0030row("DIESELPRICESTANDARD") = "0.00"
        End If
        '実勢単価
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "DIESELPRICECURRENT" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DIESELPRICECURRENT" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("DIESELPRICECURRENT").GetType.Name.ToString
            LNT0030row("DIESELPRICECURRENT") = LNT0030WRKINC.DataConvert("実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("DIESELPRICECURRENT"))
        Else
            LNT0030row("DIESELPRICECURRENT") = "0.00"
        End If
        '距離
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "DISTANCE" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "DISTANCE" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("DISTANCE").GetType.Name.ToString
            LNT0030row("DISTANCE") = LNT0030WRKINC.DataConvert("距離", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("DISTANCE"))
        Else
            LNT0030row("DISTANCE") = "0.00"
        End If
        '輸送回数
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "SHIPPINGCOUNT" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "SHIPPINGCOUNT" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("SHIPPINGCOUNT").GetType.Name.ToString
            LNT0030row("SHIPPINGCOUNT") = LNT0030WRKINC.DataConvert("輸送回数", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("SHIPPINGCOUNT"))
        Else
            LNT0030row("SHIPPINGCOUNT") = "0"
        End If
        '燃費
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "NENPI" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "NENPI" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("NENPI").GetType.Name.ToString
            LNT0030row("NENPI") = LNT0030WRKINC.DataConvert("燃費", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("NENPI"))
        Else
            LNT0030row("NENPI") = "0.00"
        End If
        '基準燃料使用量
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "FUELBASE" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "FUELBASE" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("FUELBASE").GetType.Name.ToString
            LNT0030row("FUELBASE") = LNT0030WRKINC.DataConvert("基準燃料使用量", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("FUELBASE"))
        Else
            LNT0030row("FUELBASE") = "0.00"
        End If
        '燃料使用量
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "FUELRESULT" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "FUELRESULT" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("FUELRESULT").GetType.Name.ToString
            LNT0030row("FUELRESULT") = LNT0030WRKINC.DataConvert("燃料使用量", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("FUELRESULT"))
        Else
            LNT0030row("FUELRESULT") = "0.00"
        End If
        '精算調整幅
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "ADJUSTMENT" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "ADJUSTMENT" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("ADJUSTMENT").GetType.Name.ToString
            LNT0030row("ADJUSTMENT") = LNT0030WRKINC.DataConvert("精算調整幅", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("ADJUSTMENT"))
        Else
            LNT0030row("ADJUSTMENT") = "0.00"
        End If
        '計算式メモ
        If Not IsNothing(Request.Form("txt" & pnlListArea.ID & "MEMO" & (WW_GridDBclick))) Then
            WW_TEXT = Convert.ToString(Request.Form("txt" & pnlListArea.ID & "MEMO" & (WW_GridDBclick)))
            WW_DATATYPE = LNT0030tbl.Rows(0)("MEMO").GetType.Name.ToString
            LNT0030row("MEMO") = LNT0030WRKINC.DataConvert("計算式メモ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(LNT0030row("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                oRtn = "ERR"
            End If
            Master.EraseCharToIgnore(LNT0030row("MEMO"))
        Else
            LNT0030row("MEMO") = ""
        End If
        '削除フラグ
        If LNT0030row("OPERATIONCB") = "on" Then
            LNT0030row("DELFLG") = C_DELETE_FLG.DELETE
        Else
            LNT0030row("DELFLG") = C_DELETE_FLG.ALIVE
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNT0030tbl)

        'WF_FIELD.Value = ""
        'WF_LeftboxOpen.Value = ""
        'WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

        WF_ListChange(WW_ErrSW)

        Dim WW_PrmData As New Hashtable
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            Dim WW_GridDBclick As Integer = 1
            Try
                Integer.TryParse(WF_GridDBclick.Text, WW_GridDBclick)
            Catch ex As Exception
                WW_GridDBclick = 1
            End Try

            leftview.Visible = True
            Select Case WF_LeftMViewChange.Value
                Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                    ' 日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                    Select Case WF_FIELD.Value
                        Case "SEIKYUDATEFROM"           '有効年月日(From)
                            leftview.WF_Calendar.Text = LNT0030tbl.Rows(WW_GridDBclick - 1)("SEIKYUDATEFROM")
                        Case "SEIKYUDATETO"             '有効年月日(To)
                            leftview.WF_Calendar.Text = LNT0030tbl.Rows(WW_GridDBclick - 1)("SEIKYUDATETO")
                    End Select
                    leftview.ActiveCalendar()
                    Exit Sub
            End Select
        End If

        ' フィールドによってパラメータを変える
        Select Case WF_FIELD.Value
            Case "WF_TODOKECODE"       '届先選択ボタン
                leftview.Visible = False
                '検索画面
                DisplayView_mspTodokeCodeMulti()
                '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                WF_LeftboxOpen.Value = ""
            Case "AVOCADOTODOKENAME"    '一覧の届先選択（虫眼鏡）
                leftview.Visible = False
                '検索画面
                DisplayView_mspTodokeCodeSingle()
                '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                WF_LeftboxOpen.Value = ""
            Case "AVOCADOSHUKANAME"    '一覧の出荷場所選択（虫眼鏡）
                leftview.Visible = False
                '検索画面
                DisplayView_mspShukabashoSingle()
                '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                WF_LeftboxOpen.Value = ""
        End Select

    End Sub
    ''' <summary>
    ''' 届先コード検索時処理（複数）
    ''' </summary>
    Protected Sub DisplayView_mspTodokeCodeMulti()

        Me.mspTodokeCode.InitPopUp()
        Me.mspTodokeCode.SelectionMode = ListSelectionMode.Multiple

        Me.mspTodokeCode.SQL = CmnSearchSQL.GetTankaTodokeSQL(prmOrgCode:=work.WF_SEL_ORGCODE.Text, prmToriCode:=work.WF_SEL_TORICODE.Text)

        Me.mspTodokeCode.KeyFieldName = "KEYCODE"
        Me.mspTodokeCode.DispFieldList.AddRange(CmnSearchSQL.GetTankaTodokeTitle)

        Me.mspTodokeCode.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 届先コード検索時処理（一行毎）
    ''' </summary>
    Protected Sub DisplayView_mspTodokeCodeSingle()

        Me.mspTodokeCode.InitPopUp()
        Me.mspTodokeCode.SelectionMode = ListSelectionMode.Single

        Me.mspTodokeCode.SQL = CmnSearchSQL.GetTankaTodokeSQL(prmOrgCode:=work.WF_SEL_ORGCODE.Text, prmToriCode:=work.WF_SEL_TORICODE.Text)

        Me.mspTodokeCode.KeyFieldName = "KEYCODE"
        Me.mspTodokeCode.DispFieldList.AddRange(CmnSearchSQL.GetTankaTodokeTitle)

        Me.mspTodokeCode.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 出荷場所コード検索時処理（一行毎）
    ''' </summary>
    Protected Sub DisplayView_mspShukabashoSingle()

        Me.mspShukabasho.InitPopUp()
        Me.mspShukabasho.SelectionMode = ListSelectionMode.Single

        Me.mspShukabasho.SQL = CmnSearchSQL.GetTankaShukabashoSQL(prmOrgCode:=work.WF_SEL_ORGCODE.Text, prmToriCode:=work.WF_SEL_TORICODE.Text)

        Me.mspShukabasho.KeyFieldName = "KEYCODE"
        Me.mspShukabasho.DispFieldList.AddRange(CmnSearchSQL.GetTankaShukabashoTitle)

        Me.mspShukabasho.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 届先選択ポップアップで複数行選択
    ''' </summary>
    Protected Sub RowSelected_mspTodokeCodeMulti()

        Dim selData = Me.mspTodokeCode.SelectedItems

        '空白行がある場合、空白行を削除行（空白行の半手は、対象年の入力有無で判定）
        Dim TBLview As DataView = New DataView(LNT0030tbl)
        TBLview.RowFilter = "SEIKYUYM<>''"
        LNT0030tbl = TBLview.ToTable

        For i As Integer = 0 To selData.Count - 1
            Dim LNT0030row As DataRow = LNT0030tbl.NewRow
            LNT0030row("SELECT") = "1"
            LNT0030row("HIDDEN") = "0"
            LNT0030row("LINECNT") = LNT0030tbl.Rows.Count + 1
            LNT0030row("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
            LNT0030row("UPDTIMSTP") = Date.Now
            LNT0030row("SEIKYUYM") = ""
            LNT0030row("SEIKYUBRANCH") = ""
            LNT0030row("SEIKYUDATEFROM") = DBNull.Value
            LNT0030row("SEIKYUDATETO") = DBNull.Value
            LNT0030row("TORICODE") = work.WF_SEL_TORICODE.Text
            LNT0030row("TORINAME") = work.WF_SEL_TORINAME.Text
            LNT0030row("ORGCODE") = work.WF_SEL_ORGCODE.Text
            LNT0030row("ORGNAME") = work.WF_SEL_ORGNAME.Text
            LNT0030row("KASANORGCODE") = work.WF_SEL_KASANORGCODE.Text
            LNT0030row("KASANORGNAME") = work.WF_SEL_KASANORGNAME.Text
            LNT0030row("PATTERNCODE") = work.WF_SEL_SURCHARGEPATTERNCODE.Text
            LNT0030row("PATTERNNAME") = work.WF_SEL_SURCHARGEPATTERNNAME.Text
            LNT0030row("AVOCADOSHUKABASHO") = ""
            LNT0030row("AVOCADOSHUKANAME") = ""
            LNT0030row("AVOCADOTODOKECODE") = selData(i)("TODOKECODE")
            LNT0030row("AVOCADOTODOKENAME") = selData(i)("TODOKENAME")
            LNT0030row("SHAGATA") = ""
            LNT0030row("SHAGATANAME") = ""
            LNT0030row("SHABARA") = "0.000"
            LNT0030row("SHABAN") = ""
            LNT0030row("DIESELPRICESTANDARD") = "0.00"
            LNT0030row("DIESELPRICECURRENT") = "0.00"
            LNT0030row("CALCMETHOD") = work.WF_SEL_CALCMETHOD.Text
            LNT0030row("CALCMETHODNAME") = work.WF_SEL_CALCMETHODNAME.Text
            LNT0030row("DISTANCE") = "0.00"
            LNT0030row("SHIPPINGCOUNT") = "0"
            LNT0030row("NENPI") = "0.00"
            LNT0030row("FUELBASE") = "0.00"
            LNT0030row("FUELRESULT") = "0.00"
            LNT0030row("ADJUSTMENT") = "0.00"
            LNT0030row("MEMO") = ""
            LNT0030row("DELFLG") = "0"
            LNT0030row("DELFLGNAME") = ""

            LNT0030tbl.Rows.Add(LNT0030row)

        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0030tbl)

        'ポップアップの非表示
        Me.mspTodokeCode.HidePopUp()

    End Sub

    ''' <summary>
    ''' 届先選択ポップアップで１行選択
    ''' </summary>
    Protected Sub RowSelected_mspTodokeCodeSingle()

        Dim selData = Me.mspTodokeCode.SelectedSingleItem

        '○ 表示LINECNT取得
        Dim WW_GridDBclick As Integer = 1
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_GridDBclick)
        Catch ex As Exception
            WW_GridDBclick = 1
        End Try

        Dim LNT0030row As DataRow = LNT0030tbl(WW_GridDBclick - 1)
        LNT0030row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        LNT0030row("AVOCADOTODOKECODE") = selData("TODOKECODE")
        LNT0030row("AVOCADOTODOKENAME") = selData("TODOKENAME")

        '○ 画面表示データ保存
        Master.SaveTable(LNT0030tbl)

        'ポップアップの非表示
        Me.mspTodokeCode.HidePopUp()

    End Sub

    ''' <summary>
    ''' 出荷場所選択ポップアップで１行選択
    ''' </summary>
    Protected Sub RowSelected_mspShukabashoSingle()

        Dim selData = Me.mspShukabasho.SelectedSingleItem

        '○ 表示LINECNT取得
        Dim WW_GridDBclick As Integer = 1
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_GridDBclick)
        Catch ex As Exception
            WW_GridDBclick = 1
        End Try

        Dim LNT0030row As DataRow = LNT0030tbl(WW_GridDBclick - 1)
        LNT0030row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
        LNT0030row("AVOCADOSHUKABASHO") = selData("SHUKABASHO")
        LNT0030row("AVOCADOSHUKANAME") = selData("SHUKANAME")

        '○ 画面表示データ保存
        Master.SaveTable(LNT0030tbl)

        'ポップアップの非表示
        Me.mspShukabasho.HidePopUp()

    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(LNT0030tbl)

        'チェックボックス判定
        For i As Integer = 0 To LNT0030tbl.Rows.Count - 1
            If LNT0030tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                If LNT0030tbl.Rows(i)("OPERATIONCB") = "on" Then
                    LNT0030tbl.Rows(i)("OPERATIONCB") = ""
                    LNT0030tbl.Rows(i)("DELFLG") = C_DELETE_FLG.ALIVE
                Else
                    LNT0030tbl.Rows(i)("OPERATIONCB") = "on"
                    LNT0030tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0030tbl)

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(LNT0030tbl)

        '全チェックボックスON
        For i As Integer = 0 To LNT0030tbl.Rows.Count - 1
            If LNT0030tbl.Rows(i)("HIDDEN") = "0" Then
                LNT0030tbl.Rows(i)("OPERATIONCB") = "on"
                LNT0030tbl.Rows(i)("DELFLG") = C_DELETE_FLG.DELETE
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0030tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLREJECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(LNT0030tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To LNT0030tbl.Rows.Count - 1
            If LNT0030tbl.Rows(i)("HIDDEN") = "0" Then
                LNT0030tbl.Rows(i)("OPERATIONCB") = ""
                LNT0030tbl.Rows(i)("DELFLG") = C_DELETE_FLG.ALIVE
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0030tbl)

    End Sub

    ''' <summary>
    ''' 退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INITTBL.txt"

    End Sub

    ' ******************************************************************************
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

    ''' <summary>
    ''' 削除、ロックフラグの更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    ''' <remarks></remarks>
    Public Sub FlgUpdate(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByVal WW_NOW As Date, ByVal iCTRL As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ更新
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        SQLStr.Append("     LNG.LNT0030_SURCHARGEFEE             ")
        SQLStr.Append(" SET                                         ")
        If iCTRL = "DEL" Then
            SQLStr.Append("     DELFLG               = @DELFLG      ")
        Else
            SQLStr.Append("     LOCKFLG              = @LOCKFLG     ")
            SQLStr.Append("    ,LOCKYMD              = @LOCKYMD     ")
            SQLStr.Append("    ,LOCKUSER             = @LOCKUSER    ")
        End If
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(DIESELPRICESITEID, '')      = @DIESELPRICESITEID ")
        SQLStr.Append("    AND  COALESCE(DIESELPRICESITEBRANCH, '')  = @DIESELPRICESITEBRANCH ")
        SQLStr.Append("    AND  COALESCE(TARGETYEAR, '')             = @TARGETYEAR ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)            '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)    '実勢軽油価格参照先ID枝番
                Dim P_TARGETYEAR As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYEAR", MySqlDbType.VarChar, 4)                           '対象年
                Dim P_LOCKFLG As MySqlParameter = SQLcmd.Parameters.Add("@LOCKFLG", MySqlDbType.VarChar, 1)                                 'ロックフラグ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                                   '削除フラグ
                Dim P_LOCKYMD As MySqlParameter = SQLcmd.Parameters.Add("@LOCKYMD", MySqlDbType.DateTime)                                   'ロック実行年月日
                Dim P_LOCKUSER As MySqlParameter = SQLcmd.Parameters.Add("@LOCKUSER", MySqlDbType.VarChar, 20)                              'ロック実行者
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                                     '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)                                '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)                            '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)                                '更新プログラムＩＤ

                P_DIESELPRICESITEID.Value = WW_ROW("DIESELPRICESITEID")             '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = WW_ROW("DIESELPRICESITEBRANCH")     '実勢軽油価格参照先ID枝番
                P_TARGETYEAR.Value = WW_ROW("TARGETYEAR")                           '対象年
                P_LOCKFLG.Value = WW_ROW("LOCKFLG")                                 'ロックフラグ
                If WW_ROW("LOCKFLG") = "0" Then
                    P_LOCKYMD.Value = DBNull.Value                      'ロック実行年月日
                    P_LOCKUSER.Value = ""                               'ロック実行者
                Else
                    P_LOCKYMD.Value = Date.Now                          'ロック実行年月日
                    P_LOCKUSER.Value = Master.USERID                    'ロック実行者
                End If
                P_DELFLG.Value = WW_ROW("DELFLG")                                   '削除フラグ

                P_UPDYMD.Value = WW_NOW                              '更新年月日
                P_UPDUSER.Value = Master.USERID                      '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name         '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0030_SURCHARGEFEE UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        Dim O_DUMMY As String = ""
        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrEmpty(I_VALUE) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Try
            Select Case I_FIELD
                Case "ORGCODE"          '組織コード
                    LNT0030WRKINC.getOrgName(I_VALUE, O_TEXT, O_RTN)
                Case "KASANORGCODE"     '加算先部署コード
                    LNT0030WRKINC.getKasanOrgName(I_VALUE, O_DUMMY, O_TEXT, O_RTN)
                Case "KASANORGNAME"     '加算先部署名
                    LNT0030WRKINC.getKasanOrgName(I_VALUE, O_TEXT, O_DUMMY, O_RTN)
                Case "DELFLG", "SHAGATA"     '削除フラグ、車型
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "TORICODE"
                    LNT0030WRKINC.getToriName(I_VALUE, O_TEXT, O_RTN)
                Case "SHUKABASHO"
                    LNT0030WRKINC.getShukaName(work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text, I_VALUE, O_TEXT, O_RTN)
                Case "SHUKANAME"
                    LNT0030WRKINC.getShukaCode(work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text, I_VALUE, O_TEXT, O_RTN)
                Case "TODOKECODE"
                    LNT0030WRKINC.getTodokeName(work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text, I_VALUE, O_TEXT, O_RTN)
                Case "TODOKENAME"
                    LNT0030WRKINC.getTodokeCode(work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text, I_VALUE, O_TEXT, O_RTN)
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

#Region "ﾀﾞｳﾝﾛｰﾄﾞ"

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(Excel出力)ボタン、ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_EXCELPDF(ByVal WW_FILETYPE As Integer)
        'ファイル保存先
        Dim UploadRootPath As String = System.IO.Path.Combine(CS0050SESSION.UPLOAD_PATH,
                                                   "PRINTWORK",
                                                   CS0050SESSION.USERID)
        'ディレクトリが存在しない場合は生成
        If IO.Directory.Exists(UploadRootPath) = False Then
            IO.Directory.CreateDirectory(UploadRootPath)
        End If
        '前日プリフィックスのアップロードファイルが残っていた場合は削除
        Dim targetFiles = IO.Directory.GetFiles(UploadRootPath, "*.*")
        Dim keepFilePrefix As String = Now.ToString("yyyyMMdd")
        For Each targetFile In targetFiles
            Dim targetfileName As String = IO.Path.GetFileName(targetFile)
            '今日の日付が先頭のファイル名の場合は残す
            If targetfileName.StartsWith(keepFilePrefix) Then
                Continue For
            End If
            Try
                IO.File.Delete(targetFile)
            Catch ex As Exception
                '削除時のエラーは無視
            End Try
        Next targetFile

        Dim UrlRoot As String
        'URLのルートを表示
        'UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)
        UrlRoot = String.Format("{0}://{1}/{3}/{2}/", CS0050SESSION.HTTPS_GET, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

        'Excel新規作成
        Dim wb As Workbook = New GrapeCity.Documents.Excel.Workbook

        '最大列(RANGE)を取得
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNT0030WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

        'シート名
        wb.ActiveSheet.Name = "入出力"

        '行幅設定
        SetROWSHEIGHT(wb.ActiveSheet)

        '明細設定
        Dim WW_ACTIVEROW As Integer = 3
        Dim WW_STROW As Integer = 0
        Dim WW_ENDROW As Integer = 0

        WW_STROW = WW_ACTIVEROW
        SetDETAIL(wb, wb.ActiveSheet, WW_ACTIVEROW)
        WW_ENDROW = WW_ACTIVEROW - 1

        'シート全体設定
        SetALL(wb.ActiveSheet)

        'プルダウンリスト作成
        SetPULLDOWNLIST(wb, WW_STROW, WW_ENDROW)

        '入力不可列設定
        SetCOLLOCKED(wb.ActiveSheet, WW_STROW, WW_ENDROW)

        '明細の線を引く
        Dim WW_MAXRANGE As String = wb.ActiveSheet.Cells(WW_ACTIVEROW - 1, WW_MAXCOL).Address
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders.LineStyle = BorderLineStyle.Dotted
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeLeft).LineStyle = BorderLineStyle.Thin
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeRight).LineStyle = BorderLineStyle.Thin

        '入力必須列、入力不要列網掛け設定
        SetREQUNNECEHATCHING(wb.ActiveSheet)

        'ヘッダ設定
        SetHEADER(wb, wb.ActiveSheet, WW_MAXCOL)

        'その他設定
        wb.ActiveSheet.Range("A1").Value = "ID:" + Master.MAPID
        wb.ActiveSheet.Range("A2").Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED)
        wb.ActiveSheet.Range("B2").Value = "は入力必須"
        wb.ActiveSheet.Range("C1").Value = "サーチャージ料金一覧"
        wb.ActiveSheet.Range("C2").Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY)
        wb.ActiveSheet.Range("D2").Value = "は入力不要"

        '列幅自動調整
        wb.ActiveSheet.Range("A3:" + WW_MAXRANGE).EntireColumn.AutoFit()

        '印刷設定
        With wb.ActiveSheet.PageSetup
            .PrintArea = "A1:" + WW_MAXRANGE '印刷範囲
            .PaperSize = PaperSize.A4 '用紙サイズ　
            .Orientation = PageOrientation.Landscape '横向き
            '.Zoom = 80 '倍率
            .IsPercentScale = False 'FalseでFitToPages有効化
            .FitToPagesWide = 1 'すべての列を1ページに印刷
            .FitToPagesTall = 99 '設定しないと全て1ページにされる
            .LeftMargin = 16 '左余白(ポイント)
            .RightMargin = 16 '右余白(ポイント)
            .PrintTitleRows = "$3:$3" 'ページヘッダ
            .RightFooter = "&P / &N" 'ページフッタにページ番号設定
        End With

        Dim FileName As String
        Dim FilePath As String
        Select Case WW_FILETYPE
            Case LNT0030WRKINC.FILETYPE.EXCEL
                FileName = "サーチャージ料金.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNT0030WRKINC.FILETYPE.PDF
                FileName = "サーチャージ料金.pdf"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Pdf)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)
            Case Else
        End Select
    End Sub

    ''' <summary>
    ''' シート全体設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetALL(ByVal sheet As IWorksheet)
        ' ウィンドウ枠を固定
        'sheet.FreezePanes(1, 3)
        sheet.FreezePanes(3, 0)

        ' ワークシートのビューを構成
        Dim sheetView As IWorksheetView = sheet.SheetView
        'sheetView.DisplayFormulas = False
        'sheetView.DisplayRightToLeft = True
        '表示倍率
        sheetView.Zoom = 90

        '列幅
        sheet.Columns.ColumnWidth = 5
        '行幅
        sheet.Rows.RowHeight = 15.75
        'フォント
        With sheet.Columns.Font
            .Color = Color.FromArgb(0, 0, 0)
            .Name = "Meiryo UI"
            .Size = 11
        End With
        '配置
        sheet.Columns.VerticalAlignment = VerticalAlignment.Center
        'sheet.Rows.HorizontalAlignment = HorizontalAlignment.Center
    End Sub

    ''' <summary>
    ''' 入力必須列、入力不要列網掛け設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetREQUNNECEHATCHING(ByVal sheet As IWorksheet)
        '入力必須列網掛け
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.SEIKYUYM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '請求年月
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.SEIKYUDATEFROM).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '請求対象期間From
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.SEIKYUDATETO).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '請求対象期間To
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.TORICODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '取引先コード
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.ORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '部門コード
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.PATTERNCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'パターンコード
        'sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.AVOCADOSHUKABASHO).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '出荷場所コード
        'sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.AVOCADOTODOKECODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '届先コード
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.CALCMETHOD).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '距離計算方式

        '入力不要列網掛け
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.SEIKYUBRANCH).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '請求年月枝番
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.TORINAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '取引先名
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.ORGNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '部門名
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.KASANORGCODE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '加算先部門コード
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.KASANORGNAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '加算先部門名
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.AVOCADOSHUKANAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '出荷場所名
        sheet.Columns(LNT0030WRKINC.INOUTEXCELCOL.AVOCADOTODOKENAME).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_UNNECESSARY) '届先名

        '1,2行の網掛けは消す
        sheet.Rows(0).Interior.ColorIndex = 0
        sheet.Rows(1).Interior.ColorIndex = 0
    End Sub

    ''' <summary>
    ''' 行幅設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetROWSHEIGHT(ByVal sheet As IWorksheet)

    End Sub

    ''' <summary>
    ''' ヘッダ設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetHEADER(ByVal wb As Workbook, ByVal sheet As IWorksheet, ByVal WW_MAXCOL As Integer)
        '行幅
        sheet.Rows(0).RowHeight = 15.75 '１行目
        sheet.Rows(1).RowHeight = 15.75 '２行目
        sheet.Rows(2).RowHeight = 31.5 '３行目

        Dim WW_MAXRANGE As String = sheet.Cells(2, WW_MAXCOL).Address

        '線
        sheet.Range("A3:" + WW_MAXRANGE).Borders.LineStyle = BorderLineStyle.Thin
        sheet.Range("A3:" + WW_MAXRANGE).Borders.Color = ColorTranslator.FromHtml(CONST_COLOR_BLACK)

        '背景色
        sheet.Range("A3:" + WW_MAXRANGE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_HEADER)

        'フォント
        sheet.Range("A3:" + WW_MAXRANGE).Font.Color = ColorTranslator.FromHtml(CONST_COLOR_FONT_HEADER)
        sheet.Range("A3:" + WW_MAXRANGE).Font.Bold = True

        '配置
        sheet.Range("A3:" + WW_MAXRANGE).HorizontalAlignment = HorizontalAlignment.Center

        'オートフィルタ
        sheet.Range("A3:" + WW_MAXRANGE).AutoFilter()

        '折り返して全体を表示
        'sheet.Range("J1:M1").WrapText = True

        '値
        Dim WW_HEADERROW As Integer = 2
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.SEIKYUYM).Value = "（必須）請求年月"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.SEIKYUBRANCH).Value = "請求年月枝番"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.SEIKYUDATEFROM).Value = "（必須）請求対象期間From"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.SEIKYUDATETO).Value = "（必須）請求対象期間To"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.TORICODE).Value = "（必須）取引先コード"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.TORINAME).Value = "取引先名"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.ORGCODE).Value = "（必須）部門コード"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.ORGNAME).Value = "部門名"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = "加算先部門コード"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = "加算先部門名"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.PATTERNCODE).Value = "（必須）パターンコード"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.AVOCADOSHUKABASHO).Value = "出荷場所コード"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.AVOCADOSHUKANAME).Value = "出荷場所名"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.AVOCADOTODOKECODE).Value = "届先コード"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.AVOCADOTODOKENAME).Value = "届先名"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.SHAGATA).Value = "車型"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.SHABARA).Value = "車腹"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.SHABAN).Value = "車番"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.DIESELPRICESTANDARD).Value = "基準単価"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.DIESELPRICECURRENT).Value = "実勢単価"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.CALCMETHOD).Value = "（必須）距離計算方式"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.DISTANCE).Value = "距離"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.SHIPPINGCOUNT).Value = "輸送回数"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.NENPI).Value = "燃費"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.FUELBASE).Value = "基準燃料使用量"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.FUELRESULT).Value = "燃料使用量"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.ADJUSTMENT).Value = "精算調整幅"
        sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.MEMO).Value = "計算式メモ"

        Dim WW_TEXT As String = ""
        Dim WW_TEXTLIST = New StringBuilder
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '削除フラグ
            COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            'パターンコード
            COMMENT_get(SQLcon, "SURCHARGEPATTERN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.PATTERNCODE).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.PATTERNCODE).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '車型
            COMMENT_get(SQLcon, "SHAGATA", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.SHAGATA).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.SHAGATA).Comment.Shape
                    .Width = 70
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '距離計算方式
            COMMENT_get(SQLcon, "CALCMETHOD", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.CALCMETHOD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNT0030WRKINC.INOUTEXCELCOL.CALCMETHOD).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

        End Using

    End Sub

    ''' <summary>
    ''' プルダウンリスト作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetPULLDOWNLIST(ByVal wb As Workbook, ByVal WW_STROW As Integer, ByVal WW_ENDROW As Integer)
        'メインシートを取得
        Dim mainsheet As IWorksheet = wb.ActiveSheet
        'サブシートを作成
        Dim subsheet As IWorksheet = wb.Worksheets.Add()
        subsheet.Name = CONST_PULLDOWNSHEETNAME

        Dim WW_COL As String = ""
        Dim WW_MAIN_STRANGE As IRange
        Dim WW_MAIN_ENDRANGE As IRange
        Dim WW_SUB_STRANGE As IRange
        Dim WW_SUB_ENDRANGE As IRange
        Dim WW_FIXENDROW As Integer = 0
        Dim WW_FORMULA1 As String = ""

        '○入力リスト取得
        '削除フラグ
        SETFIXVALUELIST(subsheet, "DELFLG", LNT0030WRKINC.INOUTEXCELCOL.DELFLG, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNT0030WRKINC.INOUTEXCELCOL.DELFLG)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNT0030WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_STRANGE = subsheet.Cells(0, LNT0030WRKINC.INOUTEXCELCOL.DELFLG)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNT0030WRKINC.INOUTEXCELCOL.DELFLG)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If
        'サーチャージパターンコード
        SETFIXVALUELIST(subsheet, "SURCHARGEPATTERN", LNT0030WRKINC.INOUTEXCELCOL.PATTERNCODE, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNT0030WRKINC.INOUTEXCELCOL.PATTERNCODE)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNT0030WRKINC.INOUTEXCELCOL.PATTERNCODE)
            WW_SUB_STRANGE = subsheet.Cells(0, LNT0030WRKINC.INOUTEXCELCOL.PATTERNCODE)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNT0030WRKINC.INOUTEXCELCOL.PATTERNCODE)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If
        '車型
        SETFIXVALUELIST(subsheet, "SHAGATA", LNT0030WRKINC.INOUTEXCELCOL.SHAGATA, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNT0030WRKINC.INOUTEXCELCOL.SHAGATA)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNT0030WRKINC.INOUTEXCELCOL.SHAGATA)
            WW_SUB_STRANGE = subsheet.Cells(0, LNT0030WRKINC.INOUTEXCELCOL.SHAGATA)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNT0030WRKINC.INOUTEXCELCOL.SHAGATA)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If
        '距離計算方式
        SETFIXVALUELIST(subsheet, "CALCMETHOD", LNT0030WRKINC.INOUTEXCELCOL.CALCMETHOD, WW_FIXENDROW)
        If Not WW_FIXENDROW = -1 Then
            WW_MAIN_STRANGE = mainsheet.Cells(WW_STROW, LNT0030WRKINC.INOUTEXCELCOL.CALCMETHOD)
            WW_MAIN_ENDRANGE = mainsheet.Cells(WW_ENDROW, LNT0030WRKINC.INOUTEXCELCOL.CALCMETHOD)
            WW_SUB_STRANGE = subsheet.Cells(0, LNT0030WRKINC.INOUTEXCELCOL.CALCMETHOD)
            WW_SUB_ENDRANGE = subsheet.Cells(WW_FIXENDROW, LNT0030WRKINC.INOUTEXCELCOL.CALCMETHOD)
            WW_FORMULA1 = "=" & CONST_PULLDOWNSHEETNAME & "!" & WW_SUB_STRANGE.Address & ":" & WW_SUB_ENDRANGE.Address
            With mainsheet.Range(WW_MAIN_STRANGE.Address & ":" & WW_MAIN_ENDRANGE.Address).Validation
                .Add(type:=ValidationType.List, formula1:=WW_FORMULA1)
            End With
        End If

        'メインシートをアクティブにする
        mainsheet.Activate()
        'サブシートを非表示にする
        subsheet.Visible = Visibility.Hidden
    End Sub

    ''' <summary>
    ''' 入力不可列設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetCOLLOCKED(ByVal sheet As IWorksheet, ByVal WW_STROW As Integer, ByVal WW_ENDROW As Integer)
        'Dim WW_STRANGE As IRange
        'Dim WW_ENDRANGE As IRange

        ''シートの保護をかけるとリボンも操作できなくなるため
        ''データの入力規則で対応(該当セルの入力可能文字数を0にする)

        ''枝番
        'WW_STRANGE = sheet.Cells(WW_STROW, LNT0030WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'WW_ENDRANGE = sheet.Cells(WW_ENDROW, LNT0030WRKINC.INOUTEXCELCOL.BRANCHCODE)
        'With sheet.Range(WW_STRANGE.Address & ":" & WW_ENDRANGE.Address).Validation
        '    .Add(type:=ValidationType.TextLength, validationOperator:=ValidationOperator.LessEqual, formula1:=0)
        'End With

    End Sub

    ''' <summary>
    ''' 明細設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDETAIL(ByVal wb As Workbook, ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)

        '数値書式(整数)
        Dim IntStyle As IStyle = wb.Styles.Add("IntStyle")
        IntStyle.NumberFormat = "#,##0_);[Red](#,##0)"

        '数値書式(小数点含む)
        Dim DecStyle As IStyle = wb.Styles.Add("DecStyle")
        DecStyle.NumberFormat = "#,##0.000_);[Red](#,##0.000)"

        '数値書式(小数点含む)
        Dim DecStyle2 As IStyle = wb.Styles.Add("DecStyle2")
        DecStyle2.NumberFormat = "#,##0.00_);[Red](#,##0.00)"

        For Each Row As DataRow In LNT0030tbl.Rows
            '値
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.SEIKYUYM).Value = Row("SEIKYUYM") '請求年月
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.SEIKYUBRANCH).Value = Row("SEIKYUBRANCH") '請求年月枝番
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.SEIKYUDATEFROM).Value = Row("SEIKYUDATEFROM") '請求対象期間From
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.SEIKYUDATETO).Value = Row("SEIKYUDATETO") '請求対象期間To
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.TORINAME).Value = Row("TORINAME") '取引先名
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.ORGCODE).Value = Row("ORGCODE") '部門コード
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.ORGNAME).Value = Row("ORGNAME") '部門名
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.PATTERNCODE).Value = Row("PATTERNCODE") 'パターンコード
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.AVOCADOSHUKABASHO).Value = Row("AVOCADOSHUKABASHO") '出荷場所コード
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.AVOCADOSHUKANAME).Value = Row("AVOCADOSHUKANAME") '出荷場所名
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.AVOCADOTODOKECODE).Value = Row("AVOCADOTODOKECODE") '届先コード
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.AVOCADOTODOKENAME).Value = Row("AVOCADOTODOKENAME") '届先名
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.SHAGATA).Value = Row("SHAGATA") '車型
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.SHABARA).Value = CDbl(Row("SHABARA")) '車腹
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.SHABAN).Value = Row("SHABAN") '車番
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.DIESELPRICESTANDARD).Value = CDbl(Row("DIESELPRICESTANDARD")) '基準単価
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.DIESELPRICECURRENT).Value = CDbl(Row("DIESELPRICECURRENT")) '実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.CALCMETHOD).Value = Row("CALCMETHOD") '距離計算方式
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.DISTANCE).Value = CDbl(Row("DISTANCE")) '距離
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.SHIPPINGCOUNT).Value = CInt(Row("SHIPPINGCOUNT")) '輸送回数
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.NENPI).Value = CDbl(Row("NENPI")) '燃費
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.FUELBASE).Value = CDbl(Row("FUELBASE")) '基準燃料使用量
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.FUELRESULT).Value = CDbl(Row("FUELRESULT")) '燃料使用量
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.ADJUSTMENT).Value = CDbl(Row("ADJUSTMENT")) '精算調整幅
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.MEMO).Value = Row("MEMO") '計算式メモ
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ

            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.SHABARA).Style = DecStyle '車腹
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.DIESELPRICESTANDARD).Style = DecStyle2 '基準単価
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.DIESELPRICECURRENT).Style = DecStyle2 '実勢単価
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.DISTANCE).Style = DecStyle2 '距離
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.SHIPPINGCOUNT).Style = IntStyle '輸送回数
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.NENPI).Style = DecStyle2 '燃費
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.FUELBASE).Style = DecStyle2 '基準燃料使用量
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.FUELRESULT).Style = DecStyle2 '燃料使用量
            sheet.Cells(WW_ACTIVEROW, LNT0030WRKINC.INOUTEXCELCOL.ADJUSTMENT).Style = DecStyle2 '精算調整幅

            WW_ACTIVEROW += 1
        Next
    End Sub

    ''' <summary>
    ''' セル表示用のコメント取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="I_FIELD"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_CNT"></param>
    ''' <remarks></remarks>
    Protected Sub COMMENT_get(ByVal SQLcon As MySqlConnection,
                                   ByVal I_FIELD As String,
                                   ByRef O_TEXT As String,
                                   ByRef O_CNT As Integer)

        O_TEXT = ""
        O_CNT = 0

        Dim WW_PrmData As New Hashtable
        Dim WW_PrmDataList = New StringBuilder
        Dim WW_DUMMY As String = ""
        Dim WW_VALUE As String = ""

        With leftview
            Select Case I_FIELD
                Case "DELFLG", "SURCHARGEPATTERN", "CALCMETHOD", "SHAGATA"   '削除フラグ、サージャージパターンコード、距離計算方式、車型
                    WW_PrmData = work.CreateFIXParam(Master.USERCAMP, I_FIELD)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
            End Select
            .SetListBox(WW_VALUE, WW_DUMMY, WW_PrmData)

            For i As Integer = 0 To .WF_LeftListBox.Items.Count - 1
                If Not Trim(.WF_LeftListBox.Items(i).Text) = "" Then
                    WW_PrmDataList.AppendLine(.WF_LeftListBox.Items(i).Value + "：" + .WF_LeftListBox.Items(i).Text)
                End If
            Next

            O_TEXT = WW_PrmDataList.ToString
            O_CNT = .WF_LeftListBox.Items.Count

        End With
    End Sub

    ''' <summary>
    ''' プルダウンシートにリストを作成
    ''' </summary>
    ''' <param name="sheet"></param>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_COL"></param>
    ''' <remarks></remarks>
    Protected Sub SETFIXVALUELIST(ByVal sheet As IWorksheet, ByVal I_FIELD As String, ByVal I_COL As Integer, ByRef WW_FIXENDROW As Integer)

        Dim WW_PrmData As New Hashtable
        Dim WW_DUMMY As String = ""
        Dim WW_VALUE As String = ""
        Dim WW_ROW As Integer = 0

        With leftview
            Select Case I_FIELD
                Case "DELFLG", "SURCHARGEPATTERN", "CALCMETHOD", "SHAGATA"   '削除フラグ、サーチャージパターン、距離計算方式、車型
                    WW_PrmData = work.CreateFIXParam(Master.USERCAMP, I_FIELD)
                    WW_VALUE = GRIS0005LeftBox.LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
            End Select
            .SetListBox(WW_VALUE, WW_DUMMY, WW_PrmData)

            For i As Integer = 0 To .WF_LeftListBox.Items.Count - 1
                If Not Trim(.WF_LeftListBox.Items(i).Text) = "" Then
                    sheet.Cells(WW_ROW, I_COL).Value = .WF_LeftListBox.Items(i).Value
                    WW_ROW += 1
                End If
            Next

            WW_FIXENDROW = WW_ROW - 1

        End With
    End Sub


#End Region

    ' ******************************************************************************
    ' ***  更新処理                                                              ***
    ' ******************************************************************************
#Region "ｱｯﾌﾟﾛｰﾄﾞ"
    ''' <summary>
    ''' ｱｯﾌﾟﾛｰﾄﾞボタン押下処理
    ''' </summary>
    Protected Sub WF_ButtonUPLOAD_Click()
        '○ 画面操作権限チェック
        ' 権限チェック(操作者に更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "サーチャージ料金の更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNT0030Exceltbl) Then
            LNT0030Exceltbl = New DataTable
        End If
        If LNT0030Exceltbl.Columns.Count <> 0 Then
            LNT0030Exceltbl.Columns.Clear()
        End If
        LNT0030Exceltbl.Clear()

        '添付ファイルテーブルの初期化
        If IsNothing(UploadFileTbl) Then
            UploadFileTbl = New DataTable
        End If
        If UploadFileTbl.Columns.Count <> 0 Then
            UploadFileTbl.Columns.Clear()
        End If
        UploadFileTbl.Clear()

        '添付ファイルテーブル
        UploadFileTbl.Columns.Add("FILENAME", Type.GetType("System.String"))
        UploadFileTbl.Columns.Add("FILEPATH", Type.GetType("System.String"))

        'アップロードファイル名と拡張子を取得する
        Dim fileName As String = ""
        fileName = WF_UPLOAD_BTN.FileName

        Dim fileNameParts = fileName.Split(CType(".", Char()))
        Dim fileExtention = fileNameParts(fileNameParts.Length - 1)

        'アップロードフォルダ作成
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\TANKAEXCEL"
        Dim di As System.IO.DirectoryInfo = System.IO.Directory.CreateDirectory(fileUploadPath)
        Dim dir = New System.IO.DirectoryInfo(fileUploadPath)
        Dim files As IEnumerable(Of System.IO.FileInfo) = dir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
        For Each file As System.IO.FileInfo In files
            Try
                IO.File.Delete(fileUploadPath & "\" & file.Name)
            Catch ex As Exception
            End Try
        Next

        'ファイル名先頭
        Dim fileNameHead As String = "TANKAEXCEL_TMP_"

        'ファイルパスの決定
        Dim newfileName As String = fileNameHead & DateTime.Now.ToString("yyyyMMddHHmmss") & "." & fileExtention
        Dim filePath As String = fileUploadPath & "\" & newfileName
        'ファイルの保存
        WF_UPLOAD_BTN.SaveAs(filePath)

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            SQLcon.Open()       'DataBase接続
            'Excelデータ格納用テーブルに格納する
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("データ変換に失敗したためアップロードを中断しました。")
            SetExceltbl(SQLcon, filePath, WW_ErrSW)
            If WW_ErrSW = "ERR" Then
                WF_RightboxOpen.Value = "Open"
                Exit Sub
            End If

            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("以下のデータが登録されませんでした。")

            'テーブル更新処理
            TblUpdate(SQLcon, LNT0030Exceltbl, WW_ErrSW)

        End Using
    End Sub

    ''' <summary>
    ''' アップロードしたファイルの内容をExcelデータ格納用テーブルに格納する
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="FilePath"></param>
    ''' <remarks></remarks>
    Protected Sub SetExceltbl(ByVal SQLcon As MySqlConnection, ByVal FilePath As String, ByRef O_RTN As String)
        Dim DataTypeHT As Hashtable = New Hashtable

        '○ 登録・更新するテーブルのフィールド名とフィールドの型を取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("   0   AS LINECNT ")
        SQLStr.AppendLine("       , SEIKYUYM  ")
        SQLStr.AppendLine("       , SEIKYUBRANCH  ")
        SQLStr.AppendLine("       , SEIKYUDATEFROM  ")
        SQLStr.AppendLine("       , SEIKYUDATETO  ")
        SQLStr.AppendLine("       , TORICODE  ")
        SQLStr.AppendLine("       , TORINAME  ")
        SQLStr.AppendLine("       , ORGCODE  ")
        SQLStr.AppendLine("       , ORGNAME  ")
        SQLStr.AppendLine("       , KASANORGCODE  ")
        SQLStr.AppendLine("       , KASANORGNAME  ")
        SQLStr.AppendLine("       , PATTERNCODE  ")
        SQLStr.AppendLine("       , AVOCADOSHUKABASHO  ")
        SQLStr.AppendLine("       , AVOCADOSHUKANAME  ")
        SQLStr.AppendLine("       , AVOCADOTODOKECODE  ")
        SQLStr.AppendLine("       , AVOCADOTODOKENAME  ")
        SQLStr.AppendLine("       , SHAGATA  ")
        SQLStr.AppendLine(" ,''  AS SHAGATANAME  ")
        SQLStr.AppendLine("       , SHABARA  ")
        SQLStr.AppendLine("       , SHABAN  ")
        SQLStr.AppendLine("       , DIESELPRICESTANDARD  ")
        SQLStr.AppendLine("       , DIESELPRICECURRENT  ")
        SQLStr.AppendLine("       , CALCMETHOD  ")
        SQLStr.AppendLine("       , DISTANCE  ")
        SQLStr.AppendLine("       , SHIPPINGCOUNT  ")
        SQLStr.AppendLine("       , NENPI  ")
        SQLStr.AppendLine("       , FUELBASE  ")
        SQLStr.AppendLine("       , FUELRESULT  ")
        SQLStr.AppendLine("       , ADJUSTMENT  ")
        SQLStr.AppendLine("       , MEMO  ")
        SQLStr.AppendLine("       , DELFLG  ")
        SQLStr.AppendLine(" ,''  AS DELFLGNAME  ")
        SQLStr.AppendLine(" FROM LNG.LNT0030_SURCHARGEFEE ")
        SQLStr.AppendLine(" LIMIT 0 ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0030Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0030_SURCHARGEFEE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0030_SURCHARGEFEE SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        Try
            'Excelファイルを開く
            Dim fileStream As FileStream
            fileStream = File.OpenRead(FilePath)

            'ファイル内のシート名を取得
            Dim sheetname = GrapeCity.Documents.Excel.Workbook.GetNames(fileStream)

            'データを取得
            Dim WW_EXCELDATA = GrapeCity.Documents.Excel.Workbook.ImportData(fileStream, sheetname(0))

            O_RTN = ""
            Dim WW_TEXT As String = ""
            Dim WW_DATATYPE As String = ""
            Dim WW_RESULT As Boolean

            Dim WW_CheckMES1 As String = ""
            Dim WW_CheckMES2 As String = ""
            Dim WW_CS0024FCHECKERR As String = ""
            Dim WW_CS0024FCHECKREPORT As String = ""

            Dim LNT0030Exceltblrow As DataRow
            Dim WW_LINECNT As Integer

            WW_LINECNT = 1

            For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
                LNT0030Exceltblrow = LNT0030Exceltbl.NewRow

                'LINECNT
                LNT0030Exceltblrow("LINECNT") = WW_LINECNT

                '請求年月
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.SEIKYUYM))
                WW_DATATYPE = DataTypeHT("SEIKYUYM")
                LNT0030Exceltblrow("SEIKYUYM") = LNT0030WRKINC.DataConvert("請求年月", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '請求年月枝番
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.SEIKYUBRANCH))
                WW_DATATYPE = DataTypeHT("SEIKYUBRANCH")
                LNT0030Exceltblrow("SEIKYUBRANCH") = LNT0030WRKINC.DataConvert("請求年月枝番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '請求対象期間From
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.SEIKYUDATEFROM))
                WW_DATATYPE = DataTypeHT("SEIKYUDATEFROM")
                LNT0030Exceltblrow("SEIKYUDATEFROM") = LNT0030WRKINC.DataConvert("請求対象期間From", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '請求対象期間To
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.SEIKYUDATETO))
                WW_DATATYPE = DataTypeHT("SEIKYUDATETO")
                LNT0030Exceltblrow("SEIKYUDATETO") = LNT0030WRKINC.DataConvert("請求対象期間To", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '取引先コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.TORICODE))
                WW_DATATYPE = DataTypeHT("TORICODE")
                LNT0030Exceltblrow("TORICODE") = LNT0030WRKINC.DataConvert("取引先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '取引先名
                CODENAME_get("TORICODE", LNT0030Exceltblrow("TORICODE"), WW_TEXT, WW_RtnSW)
                If String.IsNullOrEmpty(WW_TEXT) Then
                    If Not String.IsNullOrEmpty(LNT0030Exceltblrow("TORICODE")) Then
                        WW_CheckMES1 = "・取引先エラー。"
                        WW_CheckMES2 = "マスターに存在しません"
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                End If
                LNT0030Exceltblrow("TORINAME") = WW_TEXT
                '部門コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.ORGCODE))
                WW_DATATYPE = DataTypeHT("ORGCODE")
                LNT0030Exceltblrow("ORGCODE") = LNT0030WRKINC.DataConvert("部門コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '部門名
                CODENAME_get("ORGCODE", LNT0030Exceltblrow("ORGCODE"), WW_TEXT, WW_RtnSW)
                If String.IsNullOrEmpty(WW_TEXT) Then
                    If Not String.IsNullOrEmpty(LNT0030Exceltblrow("ORGCODE")) Then
                        WW_CheckMES1 = "・部門エラー。"
                        WW_CheckMES2 = "マスターに存在しません"
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                End If
                LNT0030Exceltblrow("ORGNAME") = WW_TEXT
                '加算先部門コード
                CODENAME_get("KASANORGNAME", LNT0030Exceltblrow("ORGCODE"), WW_TEXT, WW_RtnSW)
                LNT0030Exceltblrow("KASANORGCODE") = WW_TEXT
                '加算先部門名
                CODENAME_get("KASANORGCODE", LNT0030Exceltblrow("ORGCODE"), WW_TEXT, WW_RtnSW)
                LNT0030Exceltblrow("KASANORGNAME") = WW_TEXT
                'パターンコード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.PATTERNCODE))
                WW_DATATYPE = DataTypeHT("PATTERNCODE")
                LNT0030Exceltblrow("PATTERNCODE") = LNT0030WRKINC.DataConvert("パターンコード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '出荷場所コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.AVOCADOSHUKABASHO))
                WW_DATATYPE = DataTypeHT("AVOCADOSHUKABASHO")
                LNT0030Exceltblrow("AVOCADOSHUKABASHO") = LNT0030WRKINC.DataConvert("出荷場所コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '出荷場所名
                CODENAME_get("SHUKABASHO", LNT0030Exceltblrow("AVOCADOSHUKABASHO"), WW_TEXT, WW_RtnSW)
                If String.IsNullOrEmpty(WW_TEXT) Then
                    If Not String.IsNullOrEmpty(LNT0030Exceltblrow("AVOCADOSHUKABASHO")) Then
                        WW_CheckMES1 = "・出荷場所エラー。"
                        WW_CheckMES2 = "マスターに存在しません"
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                End If
                LNT0030Exceltblrow("AVOCADOSHUKANAME") = WW_TEXT
                '届先コード
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.AVOCADOTODOKECODE))
                WW_DATATYPE = DataTypeHT("AVOCADOTODOKECODE")
                LNT0030Exceltblrow("AVOCADOTODOKECODE") = LNT0030WRKINC.DataConvert("届先コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '届先名
                CODENAME_get("TODOKECODE", LNT0030Exceltblrow("AVOCADOTODOKECODE"), WW_TEXT, WW_RtnSW)
                If String.IsNullOrEmpty(WW_TEXT) Then
                    If Not String.IsNullOrEmpty(LNT0030Exceltblrow("AVOCADOTODOKECODE")) Then
                        WW_CheckMES1 = "・届先エラー。"
                        WW_CheckMES2 = "マスターに存在しません"
                        WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                        O_RTN = "ERR"
                    End If
                End If
                LNT0030Exceltblrow("AVOCADOTODOKENAME") = WW_TEXT
                '車型
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.SHAGATA))
                WW_DATATYPE = DataTypeHT("SHAGATA")
                LNT0030Exceltblrow("SHAGATA") = LNT0030WRKINC.DataConvert("車型", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                If Not String.IsNullOrEmpty(LNT0030Exceltblrow("SHAGATA")) Then
                    CODENAME_get("SHAGATA", LNT0030Exceltblrow("SHAGATA"), WW_TEXT, WW_RtnSW)
                    If String.IsNullOrEmpty(WW_TEXT) Then
                        If Not String.IsNullOrEmpty(LNT0030Exceltblrow("TORICODE")) Then
                            WW_CheckMES1 = "・車型エラー。"
                            WW_CheckMES2 = "マスターに存在しません"
                            WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                            O_RTN = "ERR"
                        End If
                    End If
                End If
                '車腹
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.SHABARA))
                WW_DATATYPE = DataTypeHT("SHABARA")
                LNT0030Exceltblrow("SHABARA") = LNT0030WRKINC.DataConvert("車腹", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '車番
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.SHABAN))
                WW_DATATYPE = DataTypeHT("SHABAN")
                LNT0030Exceltblrow("SHABAN") = LNT0030WRKINC.DataConvert("車番", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '基準単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.DIESELPRICESTANDARD))
                WW_DATATYPE = DataTypeHT("DIESELPRICESTANDARD")
                LNT0030Exceltblrow("DIESELPRICESTANDARD") = LNT0030WRKINC.DataConvert("基準単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '実勢単価
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.DIESELPRICECURRENT))
                WW_DATATYPE = DataTypeHT("DIESELPRICECURRENT")
                LNT0030Exceltblrow("DIESELPRICECURRENT") = LNT0030WRKINC.DataConvert("実勢単価", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '距離計算方式
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.CALCMETHOD))
                WW_DATATYPE = DataTypeHT("CALCMETHOD")
                LNT0030Exceltblrow("CALCMETHOD") = LNT0030WRKINC.DataConvert("距離計算方式", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '距離
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.DISTANCE))
                WW_DATATYPE = DataTypeHT("DISTANCE")
                LNT0030Exceltblrow("DISTANCE") = LNT0030WRKINC.DataConvert("距離", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '輸送回数
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.SHIPPINGCOUNT))
                WW_DATATYPE = DataTypeHT("SHIPPINGCOUNT")
                LNT0030Exceltblrow("SHIPPINGCOUNT") = LNT0030WRKINC.DataConvert("輸送回数", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '燃費
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.NENPI))
                WW_DATATYPE = DataTypeHT("NENPI")
                LNT0030Exceltblrow("NENPI") = LNT0030WRKINC.DataConvert("燃費", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '基準燃料使用量
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.FUELBASE))
                WW_DATATYPE = DataTypeHT("FUELBASE")
                LNT0030Exceltblrow("FUELBASE") = LNT0030WRKINC.DataConvert("基準燃料使用量", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '燃料使用量
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.FUELRESULT))
                WW_DATATYPE = DataTypeHT("FUELRESULT")
                LNT0030Exceltblrow("FUELRESULT") = LNT0030WRKINC.DataConvert("燃料使用量", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '精算調整幅
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.ADJUSTMENT))
                WW_DATATYPE = DataTypeHT("ADJUSTMENT")
                LNT0030Exceltblrow("ADJUSTMENT") = LNT0030WRKINC.DataConvert("精算調整幅", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '計算式メモ
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.MEMO))
                WW_DATATYPE = DataTypeHT("MEMO")
                LNT0030Exceltblrow("MEMO") = LNT0030WRKINC.DataConvert("計算式メモ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                '削除フラグ
                WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNT0030WRKINC.INOUTEXCELCOL.DELFLG))
                WW_DATATYPE = DataTypeHT("DELFLG")
                LNT0030Exceltblrow("DELFLG") = LNT0030WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
                If WW_RESULT = False Then
                    WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                    O_RTN = "ERR"
                End If
                CODENAME_get("DELFLG", LNT0030Exceltblrow("DELFLG"), WW_TEXT, WW_RtnSW)
                LNT0030Exceltblrow("DELFLGNAME") = WW_TEXT

                '登録
                LNT0030Exceltbl.Rows.Add(LNT0030Exceltblrow)

                WW_LINECNT = WW_LINECNT + 1
            Next
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.OIL_FREE_MESSAGE, C_MESSAGE_TYPE.ERR, "アップロードファイル不正、内容を確認してください。", needsPopUp:=True)

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "アップロードファイル不正、内容を確認してください。"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.OIL_FREE_MESSAGE
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            O_RTN = "ERR"
            Exit Sub

        End Try
    End Sub

    ''' <summary>
    ''' テーブル更新処理
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="iTbl"></param>
    ''' <remarks></remarks>
    Protected Sub TblUpdate(ByVal SQLcon As MySqlConnection, ByVal iTbl As DataTable, ByRef oRtn As String)

        oRtn = C_MESSAGE_NO.NORMAL

        Dim WW_ErrData As Boolean = False
        Dim DATENOW As DateTime = Date.Now

        '件数初期化
        Dim WW_UplInsCnt As Integer = 0                             'アップロード件数(登録)
        Dim WW_UplUpdCnt As Integer = 0                             'アップロード件数(更新)
        Dim WW_UplDelCnt As Integer = 0                             'アップロード件数(削除)
        Dim WW_UplErrCnt As Integer = 0                             'アップロード件数(エラー)
        Dim WW_UplUnnecessaryCnt As Integer = 0                     'アップロード件数(更新不要)
        Dim WW_DBDataCheck As String = ""

        For Each Row As DataRow In iTbl.Rows

            '存在チェック
            If Not SameDataChk(SQLcon, Row) = False Then
                'テーブルに同一データが存在しない場合

                If Row("DELFLG") = "1" Then
                    ''履歴登録(変更前)
                    'InsertHist(SQLcon, Row, C_DELETE_FLG.ALIVE, LNT0031WRKINC.MODIFYKBN.BEFDATA, DATENOW, WW_ErrSW)
                    'If Not isNormal(WW_ErrSW) Then
                    '    Exit Sub
                    'End If
                    '削除フラグのみ更新する
                    SetDelflg(SQLcon, Row, DATENOW)
                    If Not isNormal(WW_ErrSW) Then
                        Exit Sub
                    End If
                    ''履歴登録(変更後)
                    'InsertHist(SQLcon, Row, C_DELETE_FLG.DELETE, LNT0031WRKINC.MODIFYKBN.AFTDATA, DATENOW, WW_ErrSW)
                    'If Not isNormal(WW_ErrSW) Then
                    '    Exit Sub
                    'End If
                    WW_UplDelCnt += 1
                    Continue For

                End If

                '項目チェック
                INPTableCheck(SQLcon, Row, WW_ErrSW)
                If Not isNormal(WW_ErrSW) Then
                    WW_ErrData = True
                    WW_UplErrCnt += 1
                    Continue For
                End If

                Dim WW_MODIFYKBN As String = ""
                Dim WW_BEFDELFLG As String = ""

                '変更チェック
                MASTEREXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ErrSW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNT0030WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    'InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                    'If Not isNormal(WW_ErrSW) Then
                    '    Exit Sub
                    'End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNT0030WRKINC.MODIFYKBN.AFTDATA
                End If


                '件数カウント
                Select Case True
                    Case Row("DELFLG") = "1" '削除の場合
                        WW_UplDelCnt += 1
                    Case WW_MODIFYKBN = LNT0030WRKINC.MODIFYKBN.NEWDATA '新規の場合
                        WW_UplInsCnt += 1
                    Case Else
                        WW_UplUpdCnt += 1
                End Select

                'まずは、対象データを削除
                SetDelflg(SQLcon, Row, DATENOW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If
                '登録、更新する
                InsUpdExcelData(SQLcon, Row, DATENOW)
                If Not isNormal(WW_ErrSW) Then
                    Exit Sub
                End If

                '履歴登録(新規・変更後)
                'InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ErrSW)
                'If Not isNormal(WW_ErrSW) Then
                '    Exit Sub
                'End If

            Else '同一データの場合
                WW_UplUnnecessaryCnt += 1
            End If
        Next

        Dim WW_OutPutCount As String
        WW_OutPutCount = WW_UplInsCnt.ToString + "件登録完了 " _
                       + WW_UplUpdCnt.ToString + "件更新完了 " _
                       + WW_UplDelCnt.ToString + "件削除完了 " _
                       + WW_UplUnnecessaryCnt.ToString + "件更新不要 " _
                       + WW_UplErrCnt.ToString + "件エラーが起きました。"

        Dim WW_GetErrorReport As String = rightview.GetErrorReport()

        'エラーデータが存在した場合
        If WW_ErrData = True Then
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport(WW_OutPutCount)
            rightview.AddErrorReport(WW_GetErrorReport)
            '更新完了メッセージを表示
            Master.Output(C_MESSAGE_NO.BOX_ERROR_EXIST, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
        Else
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport(WW_OutPutCount)
            '更新完了メッセージを表示
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)
        End If

        'Rightboxを表示する
        If WW_UplErrCnt = 0 Then
            'エラーなし
            WF_RightboxOpen.Value = "OpenI"
        Else
            'エラーあり
            WF_RightboxOpen.Value = "Open"
        End If

        oRtn = WW_ErrSW

    End Sub

    '' <summary>
    '' 今回アップロードしたデータと完全一致するデータがあるか確認する
    '' </summary>
    Protected Function SameDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        SameDataChk = False

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    Select")
        SQLStr.AppendLine("        DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNT0030_SURCHARGEFEE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(SEIKYUYM, '')            = @SEIKYUYM ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(SEIKYUDATEFROM, '%Y/%m/%d'), '')      = @SEIKYUDATEFROM ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(SEIKYUDATETO, '%Y/%m/%d'), '')        = @SEIKYUDATETO ")
        SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')            = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')        = @KASANORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOSHUKABASHO, '')   = @AVOCADOSHUKABASHO ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOTODOKECODE, '')   = @AVOCADOTODOKECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SHAGATA, '')             = @SHAGATA ")
        SQLStr.AppendLine("    AND  COALESCE(SHABARA, '0')            = @SHABARA ")
        SQLStr.AppendLine("    AND  COALESCE(SHABAN, '')              = @SHABAN ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICESTANDARD, '0')= @DIESELPRICESTANDARD ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICECURRENT, '0') = @DIESELPRICECURRENT ")
        SQLStr.AppendLine("    AND  COALESCE(DISTANCE, '0')           = @DISTANCE ")
        SQLStr.AppendLine("    AND  COALESCE(SHIPPINGCOUNT, '0')      = @SHIPPINGCOUNT ")
        SQLStr.AppendLine("    AND  COALESCE(NENPI, '0')              = @NENPI ")
        SQLStr.AppendLine("    AND  COALESCE(FUELBASE, '0')           = @FUELBASE ")
        SQLStr.AppendLine("    AND  COALESCE(FUELRESULT, '0')         = @FUELRESULT ")
        SQLStr.AppendLine("    AND  COALESCE(ADJUSTMENT, '0')         = @ADJUSTMENT ")
        SQLStr.AppendLine("    AND  COALESCE(MEMO, '')                = @MEMO ")
        SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')              = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_SEIKYUYM As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUYM", MySqlDbType.VarChar)                          '請求年月
                Dim P_SEIKYUDATEFROM As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUDATEFROM", MySqlDbType.Date)                 '請求対象期間From
                Dim P_SEIKYUDATETO As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUDATETO", MySqlDbType.Date)                     '請求対象期間To
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar)                            '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar)                  '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar)        '出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar)        '届先コード
                Dim P_SHAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SHAGATA", MySqlDbType.VarChar)                            '車型
                Dim P_SHABARA As MySqlParameter = SQLcmd.Parameters.Add("@SHABARA", MySqlDbType.Decimal)                            '車腹
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar)                              '車番
                Dim P_DIESELPRICESTANDARD As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESTANDARD", MySqlDbType.Decimal)    '基準単価
                Dim P_DIESELPRICECURRENT As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICECURRENT", MySqlDbType.Decimal)      '実勢単価
                Dim P_DISTANCE As MySqlParameter = SQLcmd.Parameters.Add("@DISTANCE", MySqlDbType.Decimal)                          '距離
                Dim P_SHIPPINGCOUNT As MySqlParameter = SQLcmd.Parameters.Add("@SHIPPINGCOUNT", MySqlDbType.Decimal)                '輸送回数
                Dim P_NENPI As MySqlParameter = SQLcmd.Parameters.Add("@NENPI", MySqlDbType.Decimal)                                '燃費
                Dim P_FUELBASE As MySqlParameter = SQLcmd.Parameters.Add("@FUELBASE", MySqlDbType.Decimal)                          '基準燃料使用量
                Dim P_FUELRESULT As MySqlParameter = SQLcmd.Parameters.Add("@FUELRESULT", MySqlDbType.Decimal)                      '燃料使用量
                Dim P_ADJUSTMENT As MySqlParameter = SQLcmd.Parameters.Add("@ADJUSTMENT", MySqlDbType.Decimal)                      '精算調整幅
                Dim P_MEMO As MySqlParameter = SQLcmd.Parameters.Add("@MEMO", MySqlDbType.VarChar)                                  '計算式メモ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                           '削除フラグ

                P_SEIKYUYM.Value = WW_ROW("SEIKYUYM")                           '請求年月
                If String.IsNullOrEmpty(WW_ROW("SEIKYUDATEFROM")) Then
                    P_SEIKYUDATEFROM.Value = DBNull.Value                       '請求対象期間From
                Else
                    P_SEIKYUDATEFROM.Value = WW_ROW("SEIKYUDATEFROM")           '請求対象期間From
                End If
                If String.IsNullOrEmpty(WW_ROW("SEIKYUDATETO")) Then
                    P_SEIKYUDATETO.Value = DBNull.Value                         '請求対象期間To
                Else
                    P_SEIKYUDATETO.Value = WW_ROW("SEIKYUDATETO")               '請求対象期間To
                End If
                P_TORICODE.Value = WW_ROW("TORICODE")                           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")                             '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")                   '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO")         '出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE")         '届先コード
                P_SHAGATA.Value = WW_ROW("SHAGATA")                             '車型
                P_SHABARA.Value = WW_ROW("SHABARA")                             '車腹
                P_SHABAN.Value = WW_ROW("SHABAN")                               '車番
                P_DIESELPRICESTANDARD.Value = WW_ROW("DIESELPRICESTANDARD")     '基準単価
                P_DIESELPRICECURRENT.Value = WW_ROW("DIESELPRICECURRENT")       '実勢単価
                P_DISTANCE.Value = WW_ROW("DISTANCE")                           '距離
                P_SHIPPINGCOUNT.Value = WW_ROW("SHIPPINGCOUNT")                 '輸送回数
                P_NENPI.Value = WW_ROW("NENPI")                                 '燃費
                P_FUELBASE.Value = WW_ROW("FUELBASE")                           '基準燃料使用量
                P_FUELRESULT.Value = WW_ROW("FUELRESULT")                       '燃料使用量
                P_ADJUSTMENT.Value = WW_ROW("ADJUSTMENT")                       '精算調整幅
                P_MEMO.Value = WW_ROW("MEMO")                                   '計算式メモ
                P_DELFLG.Value = WW_ROW("DELFLG")                               '削除フラグ

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    Dim WW_Tbl = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count >= 1 Then
                        Exit Function
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0030_SURCHARGEFEE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0030_SURCHARGEFEE SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Function
        End Try
        SameDataChk = True
    End Function

    '' <summary>
    '' ロックフラグが"1"（ロック中）の場合、Trueを返す
    '' </summary>
    Protected Function ValidationLockChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        ValidationLockChk = False

        '更新前の削除フラグを取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        LOCKFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNT0030_SURCHARGEFEE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(DIESELPRICESITEID, '')      = @DIESELPRICESITEID ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICESITEBRANCH, '')  = @DIESELPRICESITEBRANCH ")
        SQLStr.AppendLine("    AND  COALESCE(TARGETYEAR, '')             = @TARGETYEAR ")
        SQLStr.AppendLine("    AND  COALESCE(DELFLG, '')                 = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)             '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID枝番
                Dim P_TARGETYEAR As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYEAR", MySqlDbType.VarChar, 4)                            '対象年
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)                                    '削除フラグ

                P_DIESELPRICESITEID.Value = WW_ROW("DIESELPRICESITEID")             '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = WW_ROW("DIESELPRICESITEBRANCH")     '実勢軽油価格参照先ID枝番
                P_TARGETYEAR.Value = WW_ROW("TARGETYEAR")                           '対象年
                P_DELFLG.Value = C_DELETE_FLG.ALIVE                                 '削除フラグ

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    'データが存在した場合
                    If WW_Tbl.Rows.Count > 0 Then
                        '更新前のロックフラグがロック中の場合
                        If WW_Tbl.Rows(0)("LOCKFLG") = "1" Then
                            ValidationLockChk = True
                            Exit Function
                        End If
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0030_SURCHARGEFEE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0030_SURCHARGEFEE SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            Exit Function
        End Try
    End Function
    ''' <summary>
    ''' 削除フラグ更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Public Sub SetDelflg(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByVal WW_DATENOW As DateTime)

        WW_ErrSW = C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        SQLStr.Append("     LNG.LNT0030_SURCHARGEFEE             ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(SEIKYUYM, '')            = @SEIKYUYM ")
        SQLStr.Append("    AND  COALESCE(SEIKYUBRANCH, '')        = @SEIKYUBRANCH ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(SEIKYUDATEFROM, '%Y/%m/%d'), '')      = @SEIKYUDATEFROM ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(SEIKYUDATETO, '%Y/%m/%d'), '')        = @SEIKYUDATETO ")
        SQLStr.Append("    AND  COALESCE(TORICODE, '')            = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(PATTERNCODE, '')         = @PATTERNCODE ")
        SQLStr.Append("    AND  COALESCE(AVOCADOSHUKABASHO, '')   = @AVOCADOSHUKABASHO ")
        SQLStr.Append("    AND  COALESCE(AVOCADOTODOKECODE, '')   = @AVOCADOTODOKECODE ")
        SQLStr.Append("    AND  COALESCE(SHAGATA, '')             = @SHAGATA ")
        SQLStr.Append("    AND  COALESCE(SHABARA, 0)              = @SHABARA ")
        SQLStr.Append("    AND  COALESCE(SHABAN, '')              = @SHABAN ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_SEIKYUYM As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUYM", MySqlDbType.VarChar)                          '請求年月
                Dim P_SEIKYUBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUBRANCH", MySqlDbType.VarChar)                  '請求年月枝番
                Dim P_SEIKYUDATEFROM As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUDATEFROM", MySqlDbType.Date)                 '請求対象期間From
                Dim P_SEIKYUDATETO As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUDATETO", MySqlDbType.Date)                     '請求対象期間To
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar)                            '部門コード
                Dim P_PATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@PATTERNCODE", MySqlDbType.VarChar)                    'パターンコード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar)        '出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar)        '届先コード
                Dim P_SHAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SHAGATA", MySqlDbType.VarChar)                            '車型
                Dim P_SHABARA As MySqlParameter = SQLcmd.Parameters.Add("@SHABARA", MySqlDbType.Decimal)                            '車腹
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar)                              '車番

                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                                     '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)                                '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)                            '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)                                '更新プログラムＩＤ

                P_SEIKYUYM.Value = WW_ROW("SEIKYUYM")                           '請求年月
                P_SEIKYUBRANCH.Value = WW_ROW("SEIKYUBRANCH")                   '請求年月
                P_SEIKYUDATEFROM.Value = WW_ROW("SEIKYUDATEFROM")               '請求対象期間From
                P_SEIKYUDATETO.Value = WW_ROW("SEIKYUDATETO")                   '請求対象期間To
                P_TORICODE.Value = WW_ROW("TORICODE")                           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")                             '部門コード
                P_PATTERNCODE.Value = WW_ROW("PATTERNCODE")                     'パターンコード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO")         '出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE")         '届先コード
                P_SHAGATA.Value = WW_ROW("SHAGATA")                             '車型
                P_SHABARA.Value = WW_ROW("SHABARA")                             '車腹
                P_SHABAN.Value = WW_ROW("SHABAN")                               '車番
                P_UPDYMD.Value = WW_DATENOW                                     '更新年月日
                P_UPDUSER.Value = Master.USERID                                 '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                           '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name                    '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0030_SURCHARGEFEE UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' Excelデータ登録・更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsUpdExcelData(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByVal WW_DATENOW As DateTime)
        WW_ErrSW = C_MESSAGE_NO.NORMAL

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("  INSERT INTO LNG.LNT0030_SURCHARGEFEE")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("         SEIKYUYM  ")
        SQLStr.AppendLine("       , SEIKYUBRANCH  ")
        SQLStr.AppendLine("       , SEIKYUDATEFROM  ")
        SQLStr.AppendLine("       , SEIKYUDATETO  ")
        SQLStr.AppendLine("       , TORICODE  ")
        SQLStr.AppendLine("       , TORINAME  ")
        SQLStr.AppendLine("       , ORGCODE  ")
        SQLStr.AppendLine("       , ORGNAME  ")
        SQLStr.AppendLine("       , KASANORGCODE  ")
        SQLStr.AppendLine("       , KASANORGNAME  ")
        SQLStr.AppendLine("       , PATTERNCODE  ")
        SQLStr.AppendLine("       , AVOCADOSHUKABASHO  ")
        SQLStr.AppendLine("       , AVOCADOSHUKANAME  ")
        SQLStr.AppendLine("       , AVOCADOTODOKECODE  ")
        SQLStr.AppendLine("       , AVOCADOTODOKENAME  ")
        SQLStr.AppendLine("       , SHAGATA  ")
        SQLStr.AppendLine("       , SHABARA  ")
        SQLStr.AppendLine("       , SHABAN  ")
        SQLStr.AppendLine("       , DIESELPRICESTANDARD  ")
        SQLStr.AppendLine("       , DIESELPRICECURRENT  ")
        SQLStr.AppendLine("       , CALCMETHOD  ")
        SQLStr.AppendLine("       , DISTANCE  ")
        SQLStr.AppendLine("       , SHIPPINGCOUNT  ")
        SQLStr.AppendLine("       , NENPI  ")
        SQLStr.AppendLine("       , FUELBASE  ")
        SQLStr.AppendLine("       , FUELRESULT  ")
        SQLStr.AppendLine("       , ADJUSTMENT  ")
        SQLStr.AppendLine("       , MEMO  ")
        SQLStr.AppendLine("       , DELFLG  ")
        SQLStr.AppendLine("       , INITYMD  ")
        SQLStr.AppendLine("       , INITUSER  ")
        SQLStr.AppendLine("       , INITTERMID  ")
        SQLStr.AppendLine("       , INITPGID  ")
        SQLStr.AppendLine("       , RECEIVEYMD  ")
        SQLStr.AppendLine("   )  ")
        SQLStr.AppendLine("   VALUES  ")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("         @SEIKYUYM  ")
        SQLStr.AppendLine("       , @SEIKYUBRANCH  ")
        SQLStr.AppendLine("       , @SEIKYUDATEFROM   ")
        SQLStr.AppendLine("       , @SEIKYUDATETO  ")
        SQLStr.AppendLine("       , @TORICODE  ")
        SQLStr.AppendLine("       , @TORINAME  ")
        SQLStr.AppendLine("       , @ORGCODE  ")
        SQLStr.AppendLine("       , @ORGNAME  ")
        SQLStr.AppendLine("       , @KASANORGCODE  ")
        SQLStr.AppendLine("       , @KASANORGNAME  ")
        SQLStr.AppendLine("       , @PATTERNCODE  ")
        SQLStr.AppendLine("       , @AVOCADOSHUKABASHO  ")
        SQLStr.AppendLine("       , @AVOCADOSHUKANAME  ")
        SQLStr.AppendLine("       , @AVOCADOTODOKECODE  ")
        SQLStr.AppendLine("       , @AVOCADOTODOKENAME  ")
        SQLStr.AppendLine("       , @SHAGATA  ")
        SQLStr.AppendLine("       , @SHABARA  ")
        SQLStr.AppendLine("       , @SHABAN  ")
        SQLStr.AppendLine("       , @DIESELPRICESTANDARD  ")
        SQLStr.AppendLine("       , @DIESELPRICECURRENT  ")
        SQLStr.AppendLine("       , @CALCMETHOD  ")
        SQLStr.AppendLine("       , @DISTANCE  ")
        SQLStr.AppendLine("       , @SHIPPINGCOUNT  ")
        SQLStr.AppendLine("       , @NENPI  ")
        SQLStr.AppendLine("       , @FUELBASE  ")
        SQLStr.AppendLine("       , @FUELRESULT  ")
        SQLStr.AppendLine("       , @ADJUSTMENT  ")
        SQLStr.AppendLine("       , @MEMO  ")
        SQLStr.AppendLine("       , @DELFLG  ")
        SQLStr.AppendLine("       , @INITYMD  ")
        SQLStr.AppendLine("       , @INITUSER  ")
        SQLStr.AppendLine("       , @INITTERMID  ")
        SQLStr.AppendLine("       , @INITPGID  ")
        SQLStr.AppendLine("       , @RECEIVEYMD  ")
        SQLStr.AppendLine("   )   ")
        SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
        SQLStr.AppendLine("         SEIKYUYM     = @SEIKYUYM  ")
        'SQLStr.AppendLine("       , SEIKYUBRANCH     = @SEIKYUBRANCH  ")
        SQLStr.AppendLine("       , SEIKYUDATEFROM     = @SEIKYUDATEFROM ")
        SQLStr.AppendLine("       , SEIKYUDATETO     = @SEIKYUDATETO  ")
        SQLStr.AppendLine("       , TORICODE     = @TORICODE  ")
        SQLStr.AppendLine("       , TORINAME     = @TORINAME  ")
        SQLStr.AppendLine("       , ORGCODE     = @ORGCODE  ")
        SQLStr.AppendLine("       , ORGNAME     = @ORGNAME  ")
        SQLStr.AppendLine("       , KASANORGCODE     = @KASANORGCODE  ")
        SQLStr.AppendLine("       , KASANORGNAME     = @KASANORGNAME  ")
        SQLStr.AppendLine("       , PATTERNCODE     = @PATTERNCODE  ")
        SQLStr.AppendLine("       , AVOCADOSHUKABASHO     = @AVOCADOSHUKABASHO  ")
        SQLStr.AppendLine("       , AVOCADOSHUKANAME     = @AVOCADOSHUKANAME  ")
        SQLStr.AppendLine("       , AVOCADOTODOKECODE     = @AVOCADOTODOKECODE  ")
        SQLStr.AppendLine("       , AVOCADOTODOKENAME     = @AVOCADOTODOKENAME  ")
        SQLStr.AppendLine("       , SHAGATA     = @SHAGATA  ")
        SQLStr.AppendLine("       , SHABARA     = @SHABARA  ")
        SQLStr.AppendLine("       , SHABAN     = @SHABAN  ")
        SQLStr.AppendLine("       , DIESELPRICESTANDARD     = @DIESELPRICESTANDARD  ")
        SQLStr.AppendLine("       , DIESELPRICECURRENT     = @DIESELPRICECURRENT  ")
        SQLStr.AppendLine("       , CALCMETHOD     = @CALCMETHOD  ")
        SQLStr.AppendLine("       , DISTANCE     = @DISTANCE  ")
        SQLStr.AppendLine("       , SHIPPINGCOUNT     = @SHIPPINGCOUNT  ")
        SQLStr.AppendLine("       , NENPI     = @NENPI  ")
        SQLStr.AppendLine("       , FUELBASE     = @FUELBASE  ")
        SQLStr.AppendLine("       , FUELRESULT     = @FUELRESULT ")
        SQLStr.AppendLine("       , ADJUSTMENT     = @ADJUSTMENT ")
        SQLStr.AppendLine("       , MEMO     = @MEMO ")
        SQLStr.AppendLine("       , DELFLG     = @DELFLG")
        SQLStr.AppendLine("       , UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("       , UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("       , UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("       , UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("       , RECEIVEYMD =  @RECEIVEYMD")
        SQLStr.AppendLine("    ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_SEIKYUYM As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUYM", MySqlDbType.VarChar, 6)     '請求年月
                Dim P_SEIKYUBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUBRANCH", MySqlDbType.VarChar, 8)     '請求年月枝番
                Dim P_SEIKYUDATEFROM As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUDATEFROM", MySqlDbType.Date)     '請求対象期間From
                Dim P_SEIKYUDATETO As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUDATETO", MySqlDbType.Date)     '請求対象期間To
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名
                Dim P_PATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@PATTERNCODE", MySqlDbType.VarChar, 2)     'パターンコード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '出荷場所コード
                Dim P_AVOCADOSHUKANAME As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKANAME", MySqlDbType.VarChar, 20)     '出荷場所名
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                Dim P_AVOCADOTODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKENAME", MySqlDbType.VarChar, 20)     '届先名
                Dim P_SHAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SHAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SHABARA As MySqlParameter = SQLcmd.Parameters.Add("@SHABARA", MySqlDbType.Decimal)     '車腹
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 10)     '車番
                Dim P_DIESELPRICESTANDARD As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESTANDARD", MySqlDbType.Decimal)     '基準単価
                Dim P_DIESELPRICECURRENT As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICECURRENT", MySqlDbType.Decimal)     '実勢単価
                Dim P_CALCMETHOD As MySqlParameter = SQLcmd.Parameters.Add("@CALCMETHOD", MySqlDbType.VarChar, 1)     '距離計算方式
                Dim P_DISTANCE As MySqlParameter = SQLcmd.Parameters.Add("@DISTANCE", MySqlDbType.Decimal)     '距離
                Dim P_SHIPPINGCOUNT As MySqlParameter = SQLcmd.Parameters.Add("@SHIPPINGCOUNT", MySqlDbType.Decimal)     '輸送回数
                Dim P_NENPI As MySqlParameter = SQLcmd.Parameters.Add("@NENPI", MySqlDbType.Decimal)     '燃費
                Dim P_FUELBASE As MySqlParameter = SQLcmd.Parameters.Add("@FUELBASE", MySqlDbType.Decimal)     '基準燃料使用量
                Dim P_FUELRESULT As MySqlParameter = SQLcmd.Parameters.Add("@FUELRESULT", MySqlDbType.Decimal)     '燃料使用量
                Dim P_ADJUSTMENT As MySqlParameter = SQLcmd.Parameters.Add("@ADJUSTMENT", MySqlDbType.Decimal)     '精算調整幅
                Dim P_MEMO As MySqlParameter = SQLcmd.Parameters.Add("@MEMO", MySqlDbType.VarChar, 500)     '計算式メモ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                'DB更新
                P_SEIKYUYM.Value = WW_ROW("SEIKYUYM")           '請求年月

                Dim WW_SEQ As String = ""
                work.GetMaxSEIKYUBRANCH(SQLcon, WW_ROW, WW_RtnSW, WW_SEQ)
                If isNormal(WW_RtnSW) Then
                    WW_ROW("SEIKYUBRANCH") = WW_SEQ                         '請求年月枝番
                Else
                    Exit Sub
                End If

                P_SEIKYUBRANCH.Value = WW_ROW("SEIKYUBRANCH")           '請求年月枝番
                P_SEIKYUDATEFROM.Value = WW_ROW("SEIKYUDATEFROM")           '請求対象期間From
                P_SEIKYUDATETO.Value = WW_ROW("SEIKYUDATETO")           '請求対象期間To
                P_TORICODE.Value = WW_ROW("TORICODE")           '取引先コード
                P_TORINAME.Value = WW_ROW("TORINAME")           '取引先名
                P_ORGCODE.Value = WW_ROW("ORGCODE")           '部門コード
                P_ORGNAME.Value = WW_ROW("ORGNAME")           '部門名
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE")           '加算先部門コード
                P_KASANORGNAME.Value = WW_ROW("KASANORGNAME")           '加算先部門名
                P_PATTERNCODE.Value = WW_ROW("PATTERNCODE")           'パターンコード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO")           '出荷場所コード
                P_AVOCADOSHUKANAME.Value = WW_ROW("AVOCADOSHUKANAME")           '出荷場所名
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE")           '届先コード
                P_AVOCADOTODOKENAME.Value = WW_ROW("AVOCADOTODOKENAME")           '届先名
                P_SHAGATA.Value = WW_ROW("SHAGATA")           '車型
                P_SHABARA.Value = WW_ROW("SHABARA")           '車腹
                P_SHABAN.Value = WW_ROW("SHABAN")           '車番
                P_DIESELPRICESTANDARD.Value = WW_ROW("DIESELPRICESTANDARD")           '基準単価
                P_DIESELPRICECURRENT.Value = WW_ROW("DIESELPRICECURRENT")           '実勢単価
                P_CALCMETHOD.Value = WW_ROW("CALCMETHOD")           '距離計算方式
                P_DISTANCE.Value = WW_ROW("DISTANCE")           '距離
                P_SHIPPINGCOUNT.Value = WW_ROW("SHIPPINGCOUNT")           '輸送回数
                P_NENPI.Value = WW_ROW("NENPI")           '燃費
                P_FUELBASE.Value = WW_ROW("FUELBASE")           '基準燃料使用量
                P_FUELRESULT.Value = WW_ROW("FUELRESULT")           '燃料使用量
                P_ADJUSTMENT.Value = WW_ROW("ADJUSTMENT")           '精算調整幅
                P_MEMO.Value = WW_ROW("MEMO")           '計算式メモ
                P_DELFLG.Value = WW_ROW("DELFLG")           '削除フラグ

                P_INITYMD.Value = WW_DATENOW                        '登録年月日
                P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID              '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                P_UPDYMD.Value = WW_DATENOW                         '更新年月日
                P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0030_SURCHARGEFEE  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0030_SURCHARGEFEE  INSERTUPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 入力値チェック
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub INPTableCheck(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim WW_LineErr As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""
        Dim NowDate As DateTime = Date.Now
        Dim WW_result As Date = Date.Now
        Dim WW_date As String = ""

        WW_LineErr = ""

        ' 請求年月(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SEIKYUYM", WW_ROW("SEIKYUYM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            WW_date = Mid(WW_ROW("SEIKYUYM"), 1, 4) & "/" & Mid(WW_ROW("SEIKYUYM"), 5, 2) & "/01"
            If Not Date.TryParse(WW_date, WW_result) Then
                WW_CheckMES1 = "・請求年月エラーです。"
                WW_CheckMES2 = "年月が正しくありません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                WW_ROW("SEIKYUYM") = WW_result.ToString("yyyyMM")
            End If
        Else
            WW_CheckMES1 = "・請求年月エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 請求年月枝番(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SEIKYUBRANCH", WW_ROW("SEIKYUBRANCH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・請求年月枝番エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        Dim WW_DATE_ERR As Boolean = False
        ' 請求対象期間From(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SEIKYUDATEFROM", WW_ROW("SEIKYUDATEFROM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・請求対象期間Fromエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            WW_DATE_ERR = True
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 請求対象期間To(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SEIKYUDATETO", WW_ROW("SEIKYUDATETO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・請求対象期間Toエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            WW_DATE_ERR = True
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 取引先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORICODE", WW_ROW("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 取引先名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TORINAME", WW_ROW("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・取引先名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 部門コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ORGCODE", WW_ROW("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・部門コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 部門名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ORGNAME", WW_ROW("ORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・部門名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 加算先部門コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KASANORGCODE", WW_ROW("KASANORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・加算先部門コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 加算先部門名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "KASANORGNAME", WW_ROW("KASANORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・加算先部門名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' パターンコード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "PATTERNCODE", WW_ROW("PATTERNCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・パターンコードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 出荷場所名(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "AVOCADOSHUKANAME", WW_ROW("AVOCADOSHUKANAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・出荷場所名エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 出荷場所コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "AVOCADOSHUKABASHO", WW_ROW("AVOCADOSHUKABASHO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("SHUKABASHO", WW_ROW("AVOCADOSHUKABASHO"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・出荷場所エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・出荷場所コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 届先名(バリデーションチェック)
        If WW_ROW("PATTERNCODE") = "02" Then
            If String.IsNullOrEmpty(WW_ROW("AVOCADOTODOKENAME")) Then
                WW_CheckMES1 = "・届先エラーです。"
                WW_CheckMES2 = "必須入力項目です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckField(Master.USERCAMP, "AVOCADOTODOKENAME", WW_ROW("AVOCADOTODOKENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・届先名エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If
        ' 届先コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "AVOCADOTODOKECODE", WW_ROW("AVOCADOTODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("TODOKECODE", WW_ROW("AVOCADOTODOKECODE"), WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・届先入力エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・届先コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 車型(バリデーションチェック)
        If WW_ROW("PATTERNCODE") = "03" Then
            If String.IsNullOrEmpty(WW_ROW("SHAGATA")) OrElse WW_ROW("SHAGATA") = "0" Then
                WW_CheckMES1 = "・車型エラーです。"
                WW_CheckMES2 = "必須入力項目です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckField(Master.USERCAMP, "SHAGATA", WW_ROW("SHAGATA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・車型エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If
        ' 車腹(バリデーションチェック)
        If WW_ROW("PATTERNCODE") = "04" Then
            If String.IsNullOrEmpty(WW_ROW("SHABARA")) Then
                WW_CheckMES1 = "・車腹エラーです。"
                WW_CheckMES2 = "必須入力項目です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckField(Master.USERCAMP, "SHABARA", WW_ROW("SHABARA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    If CDec(WW_ROW("SHABARA")) <= 0 Then
                        WW_CheckMES1 = "・車腹エラーです。"
                        WW_CheckMES2 = "必須入力項目です。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・車腹エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If
        ' 車番(バリデーションチェック)
        If WW_ROW("PATTERNCODE") = "05" Then
            If String.IsNullOrEmpty(WW_ROW("SHABAN")) Then
                WW_CheckMES1 = "・車番エラーです。"
                WW_CheckMES2 = "必須入力項目です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                Master.CheckField(Master.USERCAMP, "SHABAN", WW_ROW("SHABAN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・車番エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If
        ' 基準単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICESTANDARD", WW_ROW("DIESELPRICESTANDARD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・基準単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 実勢単価(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DIESELPRICECURRENT", WW_ROW("DIESELPRICECURRENT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・実勢単価エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 距離計算方式(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CALCMETHOD", WW_ROW("CALCMETHOD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・距離計算方式エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 距離(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DISTANCE", WW_ROW("DISTANCE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If WW_ROW("CALCMETHOD") = "1" AndAlso CDec(WW_ROW("DISTANCE")) < 0 Then
                WW_CheckMES1 = "・距離エラーです。"
                WW_CheckMES2 = "必須入力項目です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・距離エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 輸送回数(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SHIPPINGCOUNT", WW_ROW("SHIPPINGCOUNT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・輸送回数エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 燃費(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "NENPI", WW_ROW("NENPI"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・燃費エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 基準燃料使用量(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "FUELBASE", WW_ROW("FUELBASE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・基準燃料使用量エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 燃料使用量(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "FUELRESULT", WW_ROW("FUELRESULT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・燃料使用量エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 精算調整幅(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ADJUSTMENT", WW_ROW("ADJUSTMENT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・精算調整幅エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 計算式メモ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "MEMO", WW_ROW("MEMO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・計算式メモエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '削除フラグ(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "DELFLG", WW_ROW("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("DELFLG", WW_ROW("DELFLG"), WW_ROW("DELFLGNAME"), WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・削除コード入力エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・削除コードエラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        '-----------------------------------------------------
        '関連チェック
        '-----------------------------------------------------
        '大小チェック
        If WW_DATE_ERR = False Then
            If CDate(WW_ROW("SEIKYUDATEFROM")) > CDate(WW_ROW("SEIKYUDATETO")) Then
                WW_CheckMES1 = "・請求対象期間エラーです。"
                WW_CheckMES2 = "象期間FROM ≦ 象期間TOとしてください"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If

        If WW_ROW("TORICODE") = work.WF_SEL_TORICODE.Text AndAlso
           WW_ROW("ORGCODE") = work.WF_SEL_ORGCODE.Text AndAlso
           WW_ROW("PATTERNCODE") = work.WF_SEL_SURCHARGEPATTERNCODE.Text Then
        Else
            WW_CheckMES1 = "・入力データエラーです。"
            WW_CheckMES2 = "取引先、部門、ｻｰﾊｰｼﾞﾊﾟﾀｰﾝが一致しません"
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

    End Sub

    ''' <summary>
    ''' エラーレポート編集
    ''' </summary>
    ''' <param name="LINECNT"></param>
    ''' <param name="MESSAGE1"></param>
    ''' <param name="MESSAGE2"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckERR(ByVal LINECNT As String, ByVal MESSAGE1 As String, ByVal MESSAGE2 As String)

        Dim WW_ErrMes As String = ""
        WW_ErrMes = "【" + LINECNT + "行目】"
        WW_ErrMes &= vbCr & MESSAGE1
        If Not String.IsNullOrEmpty(MESSAGE2) Then

            WW_ErrMes &= vbCr & "   -->" & MESSAGE2
        End If

        rightview.AddErrorReport(WW_ErrMes)

    End Sub

#Region "変更履歴テーブル登録"
    ''' <summary>
    ''' 変更チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MASTEREXISTS(ByVal SQLcon As MySqlConnection,
                               ByVal WW_ROW As DataRow,
                               ByRef WW_BEFDELFLG As String,
                               ByRef WW_MODIFYKBN As String,
                               ByRef O_RTN As String)

        O_RTN = Messages.C_MESSAGE_NO.NORMAL

        'サーチャージ料金に同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    Select")
        SQLStr.AppendLine("        DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNT0030_SURCHARGEFEE")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(SEIKYUYM, '')            = @SEIKYUYM ")
        SQLStr.AppendLine("    AND  COALESCE(SEIKYUBRANCH, '')        = @SEIKYUBRANCH ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(SEIKYUDATEFROM, '%Y/%m/%d'), '')      = @SEIKYUDATEFROM ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(SEIKYUDATETO, '%Y/%m/%d'), '')        = @SEIKYUDATETO ")
        SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')            = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(PATTERNCODE, '')         = @PATTERNCODE ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOSHUKABASHO, '')   = @AVOCADOSHUKABASHO ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOTODOKECODE, '')   = @AVOCADOTODOKECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SHAGATA, '')             = @SHAGATA ")
        SQLStr.AppendLine("    AND  COALESCE(SHABARA,'0')             = @SHABARA ")
        SQLStr.AppendLine("    AND  COALESCE(SHABAN, '')              = @SHABAN ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_SEIKYUYM As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUYM", MySqlDbType.VarChar)                          '請求年月
                Dim P_SEIKYUBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUBRANCH", MySqlDbType.VarChar)                  '請求年月枝番
                Dim P_SEIKYUDATEFROM As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUDATEFROM", MySqlDbType.Date)                 '請求対象期間From
                Dim P_SEIKYUDATETO As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUDATETO", MySqlDbType.Date)                     '請求対象期間To
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar)                            '部門コード
                Dim P_PATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@PATTERNCODE", MySqlDbType.VarChar)                    'パターンコード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar)        '出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar)        '届先コード
                Dim P_SHAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SHAGATA", MySqlDbType.VarChar)                            '車型
                Dim P_SHABARA As MySqlParameter = SQLcmd.Parameters.Add("@SHABARA", MySqlDbType.Decimal)                            '車腹
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar)                              '車番

                P_SEIKYUYM.Value = WW_ROW("SEIKYUYM")                           '請求年月
                P_SEIKYUBRANCH.Value = WW_ROW("SEIKYUBRANCH")                   '請求年月枝番
                P_SEIKYUDATEFROM.Value = WW_ROW("SEIKYUDATEFROM")               '請求対象期間From
                P_SEIKYUDATETO.Value = WW_ROW("SEIKYUDATETO")                   '請求対象期間To
                P_TORICODE.Value = WW_ROW("TORICODE")                           '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE")                             '部門コード
                P_PATTERNCODE.Value = WW_ROW("PATTERNCODE")                     'パターンコード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO")         '出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE")         '届先コード
                P_SHAGATA.Value = WW_ROW("SHAGATA")                             '車型
                P_SHABARA.Value = WW_ROW("SHABARA")                             '車腹
                P_SHABAN.Value = WW_ROW("SHABAN")                               '車番

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
                        If WW_Tbl.Rows(0)("DELFLG") = C_DELETE_FLG.DELETE Then
                            WW_MODIFYKBN = LNT0030WRKINC.MODIFYKBN.NEWDATA '新規
                        Else
                            WW_MODIFYKBN = LNT0030WRKINC.MODIFYKBN.BEFDATA '変更前
                        End If
                    Else
                        WW_MODIFYKBN = LNT0030WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0030_SURCHARGEFEE SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0030_SURCHARGEFEE SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

#End Region
#End Region

End Class


