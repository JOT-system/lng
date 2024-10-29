'Option Strict On
'Option Explicit On

Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 受注一覧画面
''' </summary>
''' <remarks></remarks>
Public Class LNT0001OrderList
    Inherits System.Web.UI.Page

    '定数
    Private Const CONST_DISPROWCOUNT As Integer = 36                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 18                 'マウススクロール時稼働行数

    '○ 検索結果格納Table
    Private LNT0001tbl As DataTable                                 '一覧格納用テーブル

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

    '○ 共通処理結果
    Private WW_DUMMY As String = ""
    Private WW_ORDERSTATUS As String = ""                           '受注進行ステータス

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Try
            If IsPostBack Then
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then
                    '○ 画面表示データ復元
                    Master.RecoverTable(LNT0001tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_CheckBoxSELECT"        'チェックボックス(選択)クリック
                            WF_CheckBoxSELECT_Click()
                        Case "WF_ButtonALLSELECT"       '全選択ボタン押下
                            WF_ButtonALLSELECT_Click()
                        Case "WF_ButtonSELECT_LIFTED"   '選択解除ボタン押下
                            WF_ButtonSELECT_LIFTED_Click()
                        Case "WF_ButtonORDER_CANCEL"    'キャンセルボタン押下
                            WF_ButtonORDER_CANCEL_Click()
                        Case "WF_ButtonDetailDownload"  'ダウンロードボタン押下
                            WF_ButtonDownload_Click()
                        Case "WF_ButtonINSERT"          '受注新規作成ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_MouseWheelUp"          'マウスホイール(Up)
                            WF_Grid_Scroll()
                        Case "WF_MouseWheelDown"        'マウスホイール(Down)
                            WF_Grid_Scroll()
                        Case "WF_EXCEL_UPLOAD"          'ファイルアップロード
                            'WF_FILEUPLOAD()
                        Case "btnCommonConfirmOk"       '確認メッセージOK
                            WW_UpdateOrderStatusCancel()
                    End Select

                    '○ 一覧再表示処理
                    DisplayGrid()
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
            If Not IsNothing(LNT0001tbl) Then
                LNT0001tbl.Clear()
                LNT0001tbl.Dispose()
                LNT0001tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○画面ID設定
        Master.MAPID = LNT0001WRKINC.MAPIDL
        '○HELP表示有無設定
        Master.dispHelp = False
        '○D&D有無設定
        Master.eventDrop = True
        '○Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

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
        rightview.COMPCODE = work.WF_SEL_CAMPCODE.Text
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_DUMMY)

        '○ 画面の値設定
        WW_MAPValueSet()

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0001S Then
            'Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0001D Then
            Master.RecoverTable(LNT0001tbl, work.WF_SEL_INPTBL.Text)
        End If

    End Sub

    ''' <summary>
    ''' GridViewデータ設定
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GridViewInitialize()

        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

        '件数表示
        Me.WF_SEL_CNT.Text = LNT0001tbl.Rows.Count

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNT0001tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
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

        If IsNothing(LNT0001tbl) Then
            LNT0001tbl = New DataTable
        End If

        If LNT0001tbl.Columns.Count <> 0 Then
            LNT0001tbl.Columns.Clear()
        End If

        LNT0001tbl.Clear()

        '○ 一覧表示用検索SQL
        '　一覧説明
        '     条件指定に従い該当データを受注テーブルから取得する
        Dim SQLStr As String = ""
        '抽出項目
        SQLStr &= " SELECT"
        SQLStr &= "   1                                             AS 'SELECT'"             'SELECT
        SQLStr &= " , 0                                             AS HIDDEN"               'HIDDEN
        SQLStr &= " , ROW_NUMBER() OVER(ORDER BY LNT0005.ORDERNO, LNT0005.SAMEDAYCNT ASC) AS LINECNT"  '行No
        SQLStr &= " , ''                                            AS OPERATION"            '選択
        SQLStr &= " , LNT0005.ORDERNO                               AS ORDERNO"              '受注№
        SQLStr &= " , LNT0005.SAMEDAYCNT                            AS DETAILNO"             '受注明細№
        SQLStr &= " , LNT0004.STATUS                                AS ORDERSTATUS"          '受注状態
        SQLStr &= " , coalesce(RTRIM(CTS0006_STATUS.VALUE1     ), '') AS ORDERSTATUSNM"        '受注状態
        SQLStr &= " , CONVERT(VARCHAR,LNT0006.SHIPYMD ,111)         AS SHIPYMD"              '発送日
        SQLStr &= " , coalesce(RTRIM(LNT0004.CTNTYPE), '')            AS CTNTYPE"              'コンテナ記号
        SQLStr &= " , RIGHT('00000000' + convert(varchar, coalesce(LNT0004.CTNNO, 0)), 8) AS CTNNO"  'コンテナ番号
        SQLStr &= " , coalesce(RTRIM(LNT0005.ITEMCD), '')             AS ITEMCD"               '品目コード
        SQLStr &= " , coalesce(RTRIM(LNT0005.ITEMNM), '')             AS JRITEMNM"             'JR品目名
        SQLStr &= " , coalesce(RTRIM(LNM0021.NAME), '')               AS ITEMNM"               '品目名
        SQLStr &= " , coalesce(RTRIM(LNT0006.DEPSTATION        ), '') AS DEPSTATION"           '発駅コード
        SQLStr &= " , coalesce(RTRIM(CTS0020_DEP.NAMES         ), '') AS DEPSTATIONNM"         '発駅名
        SQLStr &= " , coalesce(RTRIM(LNT0006.ARRSTATION        ), '') AS ARRSTATION"           '着駅コード
        SQLStr &= " , coalesce(RTRIM(CTS0020_ARR.NAMES         ), '') AS ARRSTATIONNM"         '着駅名
        SQLStr &= " , coalesce(RTRIM(LNT0005.RAILDEPSTATION    ), '') AS RAILDEPSTATION"       '鉄道発駅コード
        SQLStr &= " , coalesce(RTRIM(CTS0020_RAILDEP.NAMES     ), '') AS RAILDEPSTATIONNM"     '鉄道発駅名
        SQLStr &= " , coalesce(RTRIM(LNT0005.RAILARRSTATION    ), '') AS RAILARRSTATION"       '鉄道着駅コード
        SQLStr &= " , coalesce(RTRIM(CTS0020_RAILARR.NAMES     ), '') AS RAILARRSTATIONNM"     '鉄道着駅名
        SQLStr &= " , coalesce(RTRIM(LNT0005.RAWDEPSTATION     ), '') AS RAWDEPSTATION"        '原発駅コード
        SQLStr &= " , coalesce(RTRIM(CTS0020_RAWDEP.NAMES      ), '') AS RAWDEPSTATIONNM"      '原発駅名
        SQLStr &= " , coalesce(RTRIM(LNT0005.RAWARRSTATION     ), '') AS RAWARRSTATION"        '原着駅コード
        SQLStr &= " , coalesce(RTRIM(CTS0020_RAWARR.NAMES      ), '') AS RAWARRSTATIONNM"      '原着駅名
        SQLStr &= " , coalesce(RTRIM(LNT0005.DEPTRUSTEECD      ), '') AS DEPTRUSTEECD"         '発受託人コード
        SQLStr &= " , coalesce(RTRIM(LNM0003_DEP.DEPTRUSTEENM  ), '') AS DEPTRUSTEENM"         '発受託人
        SQLStr &= " , coalesce(RTRIM(LNT0005.DEPPICKDELTRADERCD), '') AS DEPPICKDELTRADERCD"   '発集配業者コード
        SQLStr &= " , coalesce(RTRIM(LNM0003_DEP.DEPTRUSTEESUBNM),'') AS DEPPICKDELTRADERNM"   '発集配業者
        SQLStr &= " , coalesce(RTRIM(LNT0005.ARRTRUSTEECD      ), '') AS ARRTRUSTEECD"         '着受託人コード
        SQLStr &= " , coalesce(RTRIM(LNM0003_ARR.DEPTRUSTEENM  ), '') AS ARRTRUSTEENM"         '着受託人
        SQLStr &= " , coalesce(RTRIM(LNT0005.ARRPICKDELTRADERCD), '') AS ARRPICKDELTRADERCD"   '着集配業者コード
        SQLStr &= " , coalesce(RTRIM(LNM0003_ARR.DEPTRUSTEESUBNM),'') AS ARRPICKDELTRADERNM"   '着集配業者
        SQLStr &= " , coalesce(RTRIM(LNT0005.DEPTRAINNO        ), '') AS DEPTRAINNO"           '発列車番号
        SQLStr &= " , coalesce(RTRIM(LNT0005.ARRTRAINNO        ), '') AS ARRTRAINNO"           '着列車番号
        SQLStr &= " , (CASE WHEN LNT0005.PLANARRYMD IS NULL THEN '' ELSE FORMAT(LNT0005.PLANARRYMD, 'yyyy/MM/dd HH:mm:ss') END) AS PLANARRYMD"        '到着予定日
        SQLStr &= " , (CASE WHEN LNT0005.RESULTARRYMD IS NULL THEN '' ELSE FORMAT(LNT0005.RESULTARRYMD, 'yyyy/MM/dd HH:mm:ss') END) AS RESULTARRYMD"  '到着実績日
        SQLStr &= " , coalesce(RTRIM(LNT0005.STACKFREEKBN      ), '') AS STACKFREEKBNCD"       '積空区分コード
        SQLStr &= " , coalesce(RTRIM(CTS0006.VALUE1            ), '') AS STACKFREEKBNNM"       '積空区分名
        SQLStr &= " , coalesce(RTRIM(LNT0005.SHIPPERCD         ), '') AS SHIPPERCD"            '荷送人コード
        SQLStr &= " , coalesce(RTRIM(LNT0005.CONSIGNEENM       ), '') AS JRSHIPPERNM"          'JR荷送人
        SQLStr &= " , coalesce(RTRIM(LNM0019_SHIP.KANJI1       ), '') AS SHIPPERNM"            '荷送人
        SQLStr &= " , coalesce(RTRIM(LNT0005.SLCPICKUPTEL      ), '') AS SLCPICKUPTEL"         '集荷先電話番号
        SQLStr &= " , coalesce(RTRIM(LNT0005.OTHERFEE          ), '') AS OTHERFEE"             'その他料金
        SQLStr &= " , coalesce(RTRIM(LNT0005.DELFLG            ), '') AS DELFLG"               '削除フラグ
        '受注データ（ヘッダ）
        SQLStr &= " FROM LNG.LNT0004_ORDERHEAD LNT0004 "
        '受注データ（明細データ）
        SQLStr &= " LEFT JOIN LNG.LNT0005_ORDERDATA LNT0005 "
        SQLStr &= "      ON LNT0004.ORDERNO = LNT0005.ORDERNO "
        SQLStr &= "     AND LNT0005.DELFLG <> @P01"
        '受注データ（精算予定ファイル）
        SQLStr &= " LEFT JOIN LNG.LNT0006_PAYPLANF LNT0006 "
        SQLStr &= "      ON LNT0005.ORDERNO = LNT0006.ORDERNO "
        SQLStr &= "     AND LNT0005.SAMEDAYCNT = LNT0006.SAMEDAYCNT "
        SQLStr &= "     AND LNT0006.DELFLG <> @P01"
        '品目マスタ 品目名
        SQLStr &= " LEFT JOIN LNG.LNM0021_ITEM LNM0021 "
        SQLStr &= "      ON LNT0005.ITEMCD = LNM0021.ITEMCD"
        SQLStr &= "     AND LNM0021.DELFLG <> @P01"
        '駅マスタ 発駅名
        SQLStr &= " LEFT JOIN COM.LNS0020_STATION CTS0020_DEP "
        SQLStr &= "      ON LNT0006.DEPSTATION = CTS0020_DEP.STATION"
        SQLStr &= "     AND CTS0020_DEP.DELFLG <> @P01"
        '駅マスタ 着駅名
        SQLStr &= " LEFT JOIN COM.LNS0020_STATION CTS0020_ARR "
        SQLStr &= "      ON LNT0006.ARRSTATION = CTS0020_ARR.STATION"
        SQLStr &= "     AND CTS0020_ARR.DELFLG <> @P01"
        '駅マスタ 鉄道発駅名
        SQLStr &= " LEFT JOIN COM.LNS0020_STATION CTS0020_RAILDEP "
        SQLStr &= "      ON LNT0005.RAILDEPSTATION = CTS0020_RAILDEP.STATION"
        SQLStr &= "     AND CTS0020_RAILDEP.DELFLG <> @P01"
        '駅マスタ 鉄道着駅名
        SQLStr &= " LEFT JOIN COM.LNS0020_STATION CTS0020_RAILARR "
        SQLStr &= "      ON LNT0005.RAILARRSTATION = CTS0020_RAILARR.STATION"
        SQLStr &= "     AND CTS0020_RAILARR.DELFLG <> @P01"
        '駅マスタ 原発駅名
        SQLStr &= " LEFT JOIN COM.LNS0020_STATION CTS0020_RAWDEP "
        SQLStr &= "      ON LNT0005.RAWDEPSTATION = CTS0020_RAWDEP.STATION"
        SQLStr &= "     AND CTS0020_RAWDEP.DELFLG <> @P01"
        '駅マスタ 原着駅名
        SQLStr &= " LEFT JOIN COM.LNS0020_STATION CTS0020_RAWARR "
        SQLStr &= "      ON LNT0005.RAWARRSTATION = CTS0020_RAWARR.STATION"
        SQLStr &= "     AND CTS0020_RAWARR.DELFLG <> @P01"
        'コンテナ決済マスタ(発駅)
        SQLStr &= " LEFT JOIN LNG.LNM0003_REKEJM LNM0003_DEP "
        SQLStr &= "      ON LNT0005.RAILDEPSTATION = LNM0003_DEP.DEPSTATION"
        SQLStr &= "     AND LNT0005.DEPTRUSTEECD = LNM0003_DEP.DEPTRUSTEECD"
        SQLStr &= "     AND LNT0005.DEPPICKDELTRADERCD = LNM0003_DEP.DEPTRUSTEESUBCD"
        SQLStr &= "     AND LNM0003_DEP.DELFLG <> @P01"
        'コンテナ決済マスタ(着駅)
        SQLStr &= " LEFT JOIN LNG.LNM0003_REKEJM LNM0003_ARR "
        SQLStr &= "      ON LNT0005.RAILARRSTATION = LNM0003_ARR.DEPSTATION"
        SQLStr &= "     AND LNT0005.ARRTRUSTEECD = LNM0003_ARR.DEPTRUSTEECD"
        SQLStr &= "     AND LNT0005.ARRPICKDELTRADERCD = LNM0003_ARR.DEPTRUSTEESUBCD"
        SQLStr &= "     AND LNM0003_ARR.DELFLG <> @P01"
        '固定値マスタ 積空区分
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006 "
        SQLStr &= "      ON LNT0005.STACKFREEKBN = CTS0006.KEYCODE"
        SQLStr &= "     AND CTS0006.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006.CLASS = 'STACKFREEKBN'"
        SQLStr &= "     AND CTS0006.DELFLG <> @P01"
        '固定値マスタ 受注状態
        SQLStr &= " LEFT JOIN COM.LNS0006_FIXVALUE CTS0006_STATUS "
        SQLStr &= "      ON LNT0004.STATUS = CTS0006_STATUS.KEYCODE"
        SQLStr &= "     AND CTS0006_STATUS.CAMPCODE = '01'"
        SQLStr &= "     AND CTS0006_STATUS.CLASS = 'ORDERSTATUS'"
        SQLStr &= "     AND CTS0006_STATUS.DELFLG <> @P01"
        '名称マスタ 荷送人
        SQLStr &= " LEFT JOIN LNG.LNM0019_MMASADJ LNM0019_SHIP "
        SQLStr &= "      ON LNT0005.SHIPPERCD = LNM0019_SHIP.KEY4"
        SQLStr &= "     AND LNM0019_SHIP.KEY1 = 2160"
        SQLStr &= "     AND LNM0019_SHIP.DELFLG <> @P01"
        '条件
        SQLStr &= " WHERE LNT0004.DELFLG <> @P01"
        SQLStr &= " AND LNT0006.SHIPYMD >= @P02"

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        '発車予定日To
        If Not String.IsNullOrEmpty(work.WF_SEL_DATE_TO.Text) Then
            SQLStr &= String.Format("    AND LNT0006.SHIPYMD <= '{0}'", work.WF_SEL_DATE_TO.Text)
        End If
        '所管部
        If Not String.IsNullOrEmpty(work.WF_SEL_JURISDICTIONCD.Text) Then
            SQLStr &= String.Format("    AND LNT0006.JURISDICTIONCD = {0}", work.WF_SEL_JURISDICTIONCD.Text)
        End If
        'JOT発店所
        If Not String.IsNullOrEmpty(work.WF_SEL_JOTDEPBRANCHCD.Text) Then
            SQLStr &= String.Format("    AND LNT0006.JOTDEPBRANCHCD = {0}", work.WF_SEL_JOTDEPBRANCHCD.Text)
        End If
        '積空区分
        If Not String.IsNullOrEmpty(work.WF_SEL_STACKFREEKBNCD.Text) Then
            SQLStr &= String.Format("    AND LNT0006.STACKFREEKBN = {0}", work.WF_SEL_STACKFREEKBNCD.Text)
        End If
        '発駅コード
        If Not String.IsNullOrEmpty(work.WF_SEL_DEPSTATION.Text) Then
            SQLStr &= String.Format("    AND LNT0006.DEPSTATION = {0}", work.WF_SEL_DEPSTATION.Text)
        End If
        '発受託人
        If Not String.IsNullOrEmpty(work.WF_SEL_DEPTRUSTEECD.Text) Then
            SQLStr &= String.Format("    AND LNT0006.DEPTRUSTEECD = {0}", work.WF_SEL_DEPTRUSTEECD.Text)
        End If
        'コンテナ記号
        If Not String.IsNullOrEmpty(work.WF_SEL_CTNTYPE.Text) Then
            SQLStr &= String.Format("    AND LNT0004.CTNTYPE = '{0}'", work.WF_SEL_CTNTYPE.Text)
        End If
        'コンテナ番号
        If Not String.IsNullOrEmpty(work.WF_SEL_CTNNO.Text) Then
            SQLStr &= String.Format("    AND LNT0004.CTNNO = '{0}'", work.WF_SEL_CTNNO.Text)
        End If
        '状態(受注進行ステータス)
        If Not String.IsNullOrEmpty(work.WF_SEL_STATUSCODE.Text) Then
            SQLStr &= String.Format("    AND LNT0004.STATUS = '{0}'", work.WF_SEL_STATUSCODE.Text)
        End If
        '受注キャンセルフラグ
        If work.WF_SEL_ORDERCANCELFLG.Text = "0" Then
            SQLStr &= String.Format("    AND LNT0004.STATUS <> '{0}'", BaseDllConst.CONST_ORDERSTATUS_900)
        End If
        '対象外フラグ
        If work.WF_SEL_NOTSELFLG.Text = "0" Then
            SQLStr &= "    AND LNT0005.SKIPFLG = 0"
        End If

        'ソート順
        SQLStr &= " ORDER BY"
        SQLStr &= "     LNT0005.ORDERNO, LNT0005.SAMEDAYCNT"

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 1)  '削除フラグ
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.Date)     '積込日(開始)

                PARA1.Value = C_DELETE_FLG.DELETE
                PARA2.Value = work.WF_SEL_DATE.Text

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0001tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0001tbl.Load(SQLdr)
                End Using

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001L Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' チェックボックス(選択)クリック処理
    ''' </summary>
    Protected Sub WF_CheckBoxSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(LNT0001tbl)

        'チェックボックス判定
        For i As Integer = 0 To LNT0001tbl.Rows.Count - 1
            If LNT0001tbl.Rows(i)("LINECNT") = WF_SelectedIndex.Value Then
                If LNT0001tbl.Rows(i)("OPERATION") = "" Then
                    LNT0001tbl.Rows(i)("OPERATION") = "on"
                Else
                    LNT0001tbl.Rows(i)("OPERATION") = ""
                End If
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

    End Sub

    ''' <summary>
    ''' 全選択ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonALLSELECT_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(LNT0001tbl)

        '全チェックボックスON
        For i As Integer = 0 To LNT0001tbl.Rows.Count - 1
            If LNT0001tbl.Rows(i)("HIDDEN") = "0" AndAlso LNT0001tbl.Rows(i)("ORDERSTATUS") <> BaseDllConst.CONST_ORDERSTATUS_900 Then
                LNT0001tbl.Rows(i)("OPERATION") = "on"
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

    End Sub

    ''' <summary>
    ''' 全解除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonSELECT_LIFTED_Click()

        '○ 画面表示データ復元
        Master.RecoverTable(LNT0001tbl)

        '全チェックボックスOFF
        For i As Integer = 0 To LNT0001tbl.Rows.Count - 1
            If LNT0001tbl.Rows(i)("HIDDEN") = "0" Then
                LNT0001tbl.Rows(i)("OPERATION") = ""
            End If
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

    End Sub

    ''' <summary>
    ''' キャンセルボタン押下時処理
    ''' </summary>
    Protected Sub WF_ButtonORDER_CANCEL_Click()

        Dim SelectChk As Boolean = False
        Dim intTblCnt As Integer = 0

        '件数を取得
        intTblCnt = LNT0001tbl.Rows.Count

        '行が選択されているかチェック
        For Each OIT0003UPDrow In LNT0001tbl.Rows
            If OIT0003UPDrow("OPERATION") = "on" Then
                If OIT0003UPDrow("ORDERSTATUS") <> BaseDllConst.CONST_ORDERSTATUS_900 Then
                    SelectChk = True
                End If
            End If
        Next

        '○メッセージ表示
        '一覧件数が０件の時のキャンセルの場合
        If intTblCnt = 0 Then
            Master.Output(C_MESSAGE_NO.CTN_CANCELDATA_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub

            '一覧件数が１件以上で未選択によるキャンセルの場合
        ElseIf SelectChk = False Then
            Master.Output(C_MESSAGE_NO.CTN_CANCELLINE_NOTFOUND, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '◯確認メッセージ(受注キャンセルの確認)
        Master.Output(C_MESSAGE_NO.CTN_CONFIRM_CANCEL_ORDER,
                      C_MESSAGE_TYPE.QUES,
                      needsPopUp:=True,
                      messageBoxTitle:="",
                      IsConfirm:=True)

    End Sub

#Region "帳票処理"

    ''' <summary>
    ''' ダウンロード
    ''' </summary>
    Protected Sub WF_ButtonDownload_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = work.WF_SEL_CAMPCODE.Text       '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightview.GetReportId()         '帳票ID
        CS0030REPORT.FILEtyp = "XLSX"                           '出力ファイル形式
        CS0030REPORT.TBLDATA = LNT0001tbl                        'データ参照  Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でExcelを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)

    End Sub

#End Region

    ''' <summary>
    ''' 受注新規作成ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()

        '選択行
        work.WF_SELROW_LINECNT.Text = ""
        '受注№ 
        work.WF_SELROW_ORDERNO.Text = ""
        '受注明細№
        work.WF_SELROW_DETAILNO.Text = ""
        '受注進行ステータス(コード)
        work.WF_SELROW_ORDERSTATUS.Text = BaseDllConst.CONST_ORDERSTATUS_100
        '受注進行ステータス(名)
        CODENAME_get("STATUS", BaseDllConst.CONST_ORDERSTATUS_100, work.WF_SELROW_ORDERSTATUSNM.Text, WW_DUMMY)
        '品目コード
        work.WF_SELROW_ITEMCD.Text = ""
        '品目名
        work.WF_SELROW_ITEMNM.Text = ""
        '鉄道発駅コード
        work.WF_SELROW_RAILDEPSTATION.Text = ""
        '鉄道発駅名
        work.WF_SELROW_RAILDEPSTATIONNM.Text = ""
        '鉄道着駅コード
        work.WF_SELROW_RAILARRSTATION.Text = ""
        '鉄道着駅名
        work.WF_SELROW_RAILARRSTATIONNM.Text = ""
        '原発駅コード
        work.WF_SELROW_RAWDEPSTATION.Text = ""
        '原発駅名
        work.WF_SELROW_RAWDEPSTATIONNM.Text = ""
        '原着駅コード
        work.WF_SELROW_RAWARRSTATION.Text = ""
        '原着駅名
        work.WF_SELROW_RAWARRSTATIONNM.Text = ""
        '発受託人コード
        work.WF_SELROW_DEPTRUSTEECD.Text = ""
        '発受託人
        work.WF_SELROW_DEPTRUSTEENM.Text = ""
        '発集配業者コード
        work.WF_SELROW_DEPPICKDELTRADERCD.Text = ""
        '発集配業者
        work.WF_SELROW_DEPPICKDELTRADERNM.Text = ""
        '着受託人コード
        work.WF_SELROW_ARRTRUSTEECD.Text = ""
        '着受託人
        work.WF_SELROW_ARRTRUSTEENM.Text = ""
        '着集配業者コード
        work.WF_SELROW_ARRPICKDELTRADERCD.Text = ""
        '着集配業者
        work.WF_SELROW_ARRPICKDELTRADERNM.Text = ""
        '発列車番号
        work.WF_SELROW_DEPTRAINNO.Text = ""
        '着列車番号
        work.WF_SELROW_ARRTRAINNO.Text = ""
        '到着予定日
        work.WF_SELROW_PLANARRYMD.Text = ""
        '到着実績日
        work.WF_SELROW_RESULTARRYMD.Text = ""
        '積空区分コード
        work.WF_SELROW_STACKFREEKBNCD.Text = ""
        '積空区分名
        work.WF_SELROW_STACKFREEKBNNM.Text = ""
        '荷送人コード
        work.WF_SELROW_SHIPPERCD.Text = ""
        '荷送人
        work.WF_SELROW_SHIPPERNM.Text = ""
        '集荷先電話番号
        work.WF_SELROW_SLCPICKUPTEL.Text = ""
        'その他料金
        work.WF_SELROW_OTHERFEE.Text = "0"

        '削除フラグ
        work.WF_SELROW_DELFLG.Text = "0"
        '作成フラグ(1：新規登録, 2：更新)
        work.WF_SEL_CREATEFLG.Text = "1"

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text)

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()

    End Sub

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBClick()

        Dim WW_LINECNT As Integer = 0

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LINECNT)
            WW_LINECNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        '〇 受注進行ステータスが"900(受注キャンセル)"の場合は何もしない
        WW_ORDERSTATUS = LNT0001tbl.Rows(WW_LINECNT)("ORDERSTATUS")
        If WW_ORDERSTATUS = BaseDllConst.CONST_ORDERSTATUS_900 Then
            Master.Output(C_MESSAGE_NO.CTN_CANCEL_ENTRY_ORDER, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Exit Sub
        End If

        '選択行
        work.WF_SELROW_LINECNT.Text = LNT0001tbl.Rows(WW_LINECNT)("LINECNT")
        '受注№
        work.WF_SELROW_ORDERNO.Text = LNT0001tbl.Rows(WW_LINECNT)("ORDERNO")
        '受注明細№
        work.WF_SELROW_DETAILNO.Text = LNT0001tbl.Rows(WW_LINECNT)("DETAILNO")
        '受注進行ステータス(コード)
        work.WF_SELROW_ORDERSTATUS.Text = LNT0001tbl.Rows(WW_LINECNT)("ORDERSTATUS")
        '受注進行ステータス(名)
        work.WF_SELROW_ORDERSTATUSNM.Text = LNT0001tbl.Rows(WW_LINECNT)("ORDERSTATUSNM")
        '品目コード
        work.WF_SELROW_ITEMCD.Text = LNT0001tbl.Rows(WW_LINECNT)("ITEMCD")
        '品目名
        work.WF_SELROW_ITEMNM.Text = Regex.Replace(LNT0001tbl.Rows(WW_LINECNT)("ITEMNM"), "<[^>]*?>", "")
        '鉄道発駅コード
        work.WF_SELROW_RAILDEPSTATION.Text = LNT0001tbl.Rows(WW_LINECNT)("RAILDEPSTATION")
        '鉄道発駅名
        work.WF_SELROW_RAILDEPSTATIONNM.Text = Regex.Replace(LNT0001tbl.Rows(WW_LINECNT)("RAILDEPSTATIONNM"), "<[^>]*?>", "")
        '鉄道着駅コード
        work.WF_SELROW_RAILARRSTATION.Text = LNT0001tbl.Rows(WW_LINECNT)("RAILARRSTATION")
        '鉄道着駅名
        work.WF_SELROW_RAILARRSTATIONNM.Text = Regex.Replace(LNT0001tbl.Rows(WW_LINECNT)("RAILARRSTATIONNM"), "<[^>]*?>", "")
        '原発駅コード
        work.WF_SELROW_RAWDEPSTATION.Text = LNT0001tbl.Rows(WW_LINECNT)("RAWDEPSTATION")
        '原発駅名
        work.WF_SELROW_RAWDEPSTATIONNM.Text = Regex.Replace(LNT0001tbl.Rows(WW_LINECNT)("RAWDEPSTATIONNM"), "<[^>]*?>", "")
        '原着駅コード
        work.WF_SELROW_RAWARRSTATION.Text = LNT0001tbl.Rows(WW_LINECNT)("RAWARRSTATION")
        '原着駅名
        work.WF_SELROW_RAWARRSTATIONNM.Text = Regex.Replace(LNT0001tbl.Rows(WW_LINECNT)("RAWARRSTATIONNM"), "<[^>]*?>", "")
        '発受託人コード
        work.WF_SELROW_DEPTRUSTEECD.Text = LNT0001tbl.Rows(WW_LINECNT)("DEPTRUSTEECD")
        '発受託人 
        work.WF_SELROW_DEPTRUSTEENM.Text = Regex.Replace(LNT0001tbl.Rows(WW_LINECNT)("DEPTRUSTEENM"), "<[^>]*?>", "")
        '発集配業者コード
        work.WF_SELROW_DEPPICKDELTRADERCD.Text = LNT0001tbl.Rows(WW_LINECNT)("DEPPICKDELTRADERCD")
        '発集配業者
        work.WF_SELROW_DEPPICKDELTRADERNM.Text = Regex.Replace(LNT0001tbl.Rows(WW_LINECNT)("DEPPICKDELTRADERNM"), "<[^>]*?>", "")
        '着受託人コード
        work.WF_SELROW_ARRTRUSTEECD.Text = LNT0001tbl.Rows(WW_LINECNT)("ARRTRUSTEECD")
        '着受託人
        work.WF_SELROW_ARRTRUSTEENM.Text = Regex.Replace(LNT0001tbl.Rows(WW_LINECNT)("ARRTRUSTEENM"), "<[^>]*?>", "")
        '着集配業者コード
        work.WF_SELROW_ARRPICKDELTRADERCD.Text = LNT0001tbl.Rows(WW_LINECNT)("ARRPICKDELTRADERCD")
        '着集配業者
        work.WF_SELROW_ARRPICKDELTRADERNM.Text = Regex.Replace(LNT0001tbl.Rows(WW_LINECNT)("ARRPICKDELTRADERNM"), "<[^>]*?>", "")
        '発列車番号
        work.WF_SELROW_DEPTRAINNO.Text = LNT0001tbl.Rows(WW_LINECNT)("DEPTRAINNO")
        '着列車番号
        work.WF_SELROW_ARRTRAINNO.Text = LNT0001tbl.Rows(WW_LINECNT)("ARRTRAINNO")
        '到着予定日
        work.WF_SELROW_PLANARRYMD.Text = LNT0001tbl.Rows(WW_LINECNT)("PLANARRYMD")
        '到着実績日
        work.WF_SELROW_RESULTARRYMD.Text = LNT0001tbl.Rows(WW_LINECNT)("RESULTARRYMD")
        '積空区分コード
        work.WF_SELROW_STACKFREEKBNCD.Text = LNT0001tbl.Rows(WW_LINECNT)("STACKFREEKBNCD")
        '積空区分名
        work.WF_SELROW_STACKFREEKBNNM.Text = Regex.Replace(LNT0001tbl.Rows(WW_LINECNT)("STACKFREEKBNNM"), "<[^>]*?>", "")
        '荷送人コード
        work.WF_SELROW_SHIPPERCD.Text = LNT0001tbl.Rows(WW_LINECNT)("SHIPPERCD")
        '荷送人
        work.WF_SELROW_SHIPPERNM.Text = Regex.Replace(LNT0001tbl.Rows(WW_LINECNT)("SHIPPERNM"), "<[^>]*?>", "")
        '集荷先電話番号
        work.WF_SELROW_SLCPICKUPTEL.Text = LNT0001tbl.Rows(WW_LINECNT)("SLCPICKUPTEL")
        'その他料金
        work.WF_SELROW_OTHERFEE.Text = LNT0001tbl.Rows(WW_LINECNT)("OTHERFEE")

        '削除フラグ
        work.WF_SELROW_DELFLG.Text = LNT0001tbl.Rows(WW_LINECNT)("DELFLG")
        '作成フラグ(1：新規登録, 2：更新)
        work.WF_SEL_CREATEFLG.Text = "2"

        '○ 状態をクリア
        For Each OIT0003row As DataRow In LNT0001tbl.Rows
            Select Case OIT0003row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    OIT0003row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select

        Next

        '○ 選択明細の状態を設定
        Select Case LNT0001tbl.Rows(WW_LINECNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                LNT0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                LNT0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                LNT0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                LNT0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                LNT0001tbl.Rows(WW_LINECNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

        WF_GridDBclick.Text = ""

        '遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNT0001tbl, work.WF_SEL_INPTBL.Text)

        '受注明細画面ページへ遷移
        Master.TransitionPage(work.WF_SEL_CAMPCODE.Text)

    End Sub

    ''' <summary>
    ''' 一覧画面-マウスホイール時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_Scroll()

    End Sub

#Region "Excelアップロード"
    ''' <summary>
    ''' ファイルアップロード時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FILEUPLOAD()

    End Sub

#End Region

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each OIT0003row As DataRow In LNT0001tbl.Rows
            If OIT0003row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                '行(LINECNT)を再設定する。既存項目(SELECT)を利用
                OIT0003row("SELECT") = WW_DataCNT
            End If
        Next

        '○ 表示LINECNT取得
        If WF_GridPosition.Text = "" Then
            WW_GridPosition = 1
        Else
            Try
                Integer.TryParse(WF_GridPosition.Text, WW_GridPosition)
            Catch ex As Exception
                WW_GridPosition = 1
            End Try
        End If

        '○ 表示格納位置決定

        '表示開始_格納位置決定(次頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelUp" Then
            If (WW_GridPosition + CONST_SCROLLCOUNT) <= WW_DataCNT Then
                WW_GridPosition += CONST_SCROLLCOUNT
            End If
        End If

        '表示開始_格納位置決定(前頁スクロール)
        If WF_ButtonClick.Value = "WF_MouseWheelDown" Then
            If (WW_GridPosition - CONST_SCROLLCOUNT) > 0 Then
                WW_GridPosition -= CONST_SCROLLCOUNT
            Else
                WW_GridPosition = 1
            End If
        End If

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNT0001tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        CS0013ProfView.CAMPCODE = work.WF_SEL_CAMPCODE.Text
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        CS0013ProfView.LFUNC = "ListDbClick"
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
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String,
                               Optional ByVal I_OFFICECODE As String = Nothing)

        O_TEXT = ""
        O_RTN = ""

        If I_VALUE = "" Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If
        Dim prmData As New Hashtable

        Try
            Select Case I_FIELD
                Case "STATUS"           '状態
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORDERSTATUS, I_VALUE, O_TEXT, O_RTN, prmData)

            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' (受注TBL)受注進行ステータス(受注キャンセル)更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_UpdateOrderStatusCancel()

        Dim StatusChk As Boolean = False

        '○ 画面表示データ復元
        Master.RecoverTable(LNT0001tbl)

        Try
            'DataBase接続文字
            Dim SQLcon = CS0050SESSION.getConnection
            SQLcon.Open() 'DataBase接続(Open)

            Using tran = SQLcon.BeginTransaction

                Dim intOrderNo As Integer
                Dim WW_DATENOW As DateTime = Date.Now

                '選択されている行の受注進行ステータスを「900:受注キャンセル」に更新
                For Each LNT0001UPDrow In LNT0001tbl.Rows
                    '選択されている場合
                    If LNT0001UPDrow("OPERATION") = "on" Then

                        Dim htHeadDataParm As New Hashtable

                        '受注No取得
                        intOrderNo = CInt(LNT0001UPDrow("ORDERNO"))
                        LNT0001UPDrow("ORDERSTATUS") = BaseDllConst.CONST_ORDERSTATUS_900

                        '■受注データ（ヘッダ） パラメータ設定処理
                        htHeadDataParm = SetOrderHeadParam(intOrderNo, LNT0001UPDrow("ORDERSTATUS").ToString, WW_DATENOW)
                        '■受注データ(ヘッダ) ステータス更新処理
                        EntryOrderData.UpdateOrderHeadStatus(SQLcon, tran, htHeadDataParm)
                    End If
                Next

                'トランザクションコミット
                tran.Commit()
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0001D DELETE")
            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0001D DELETE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

        '○メッセージ表示
        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 受注データ（ヘッダ） パラメータ設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Private Function SetOrderHeadParam(ByVal intPrmOrderNo As Integer, ByVal strStatus As String,
                                       ByVal dtSysDateTime As DateTime) As Hashtable

        Dim htHeadDataTbl As New Hashtable

        htHeadDataTbl(C_HEADPARAM.HP_ORDERNO) = intPrmOrderNo                  'オーダーNo
        htHeadDataTbl(C_HEADPARAM.HP_STATUS) = strStatus                       '状態
        htHeadDataTbl(C_HEADPARAM.HP_INITYMD) = dtSysDateTime                  '更新年月日
        htHeadDataTbl(C_HEADPARAM.HP_INITUSER) = Master.USERID                 '更新ユーザーＩＤ
        htHeadDataTbl(C_HEADPARAM.HP_INITTERMID) = Master.USERTERMID           '更新端末
        htHeadDataTbl(C_HEADPARAM.HP_INITPGID) = Me.GetType().BaseType.Name    '更新プログラムＩＤ

        Return htHeadDataTbl

    End Function

    ''' <summary>
    ''' 遷移先(登録画面)退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

    End Sub

End Class