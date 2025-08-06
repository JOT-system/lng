''************************************************************
' 統合版特別料金マスタメンテ変更履歴画面
' 作成日 2025/03/18
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2025/03/18 新規作成
'          : 
''************************************************************
Imports MySql.Data.MySqlClient
Imports System.Drawing
Imports GrapeCity.Documents.Excel

''' <summary>
''' 特別料金マスタ変更履歴
''' </summary>
''' <remarks></remarks>
Public Class LNM0014SprateHistory
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0014tbl As DataTable                                  '一覧格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 19                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 19                 'マウススクロール時稼働行数

    '〇 帳票用
    Private Const CONST_COLOR_HATCHING_MODIFY As String = "#FFFF00" '変更項目強調表示色(黄)
    Private Const CONST_COLOR_HATCHING_HEADER As String = "#002060" 'ヘッダ網掛け色
    Private Const CONST_COLOR_FONT_HEADER As String = "#FFFFFF" 'ヘッダフォント色
    Private Const CONST_COLOR_BLACK As String = "#000000" '黒
    Private Const CONST_COLOR_RED As String = "#FF0000" '赤

    Private Const CONST_HEIGHT_PER_ROW As Integer = 15 'セルのコメントの一行あたりの高さ
    Private Const CONST_DATA_START_ROW As Integer = 3 'データ開始行

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

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
                    Master.RecoverTable(LNM0014tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0014WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0014WRKINC.FILETYPE.PDF)
                        Case "WF_ButtonEND", "LNM0014L" '戻るボタン押下（LNS0001Lは、パンくずより）
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_SelectMODIFYYMChange"  '変更年月フィールドチェンジ
                            WF_SelectMODIFYYM_CHANGE()
                        Case "WF_SelectMODIFYDDChange"  '変更日フィールドチェンジ
                            WF_SelectMODIFYDD_CHANGE()
                        Case "WF_SelectMODIFYUSERChange",   '変更ユーザーフィールドチェンジ
                             "WF_ButtonMODIFYVIEW"          '表示するボタン押下
                            WF_WF_ButtonMODIFY_VIEW()
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
            If Not IsNothing(LNM0014tbl) Then
                LNM0014tbl.Clear()
                LNM0014tbl.Dispose()
                LNM0014tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0014WRKINC.MAPIDH
        '○ HELP表示有無設定
        Master.dispHelp = False
        '○ D&D有無設定
        Master.eventDrop = True
        '○ Grid情報保存先のファイル名
        Master.CreateXMLSaveFile()

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

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '変更年月取得
            MODIFYYMGet(SQLcon)
            '変更日取得
            MODIFYDDGet(SQLcon)
            '変更ユーザ取得
            MODIFYUSERGet(SQLcon)
        End Using

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
            SQLcon.Open()  ' DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNM0014tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0014tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0014tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        CS0013ProfView.CAMPCODE = Master.USERCAMP
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

        '変更箇所を強調表示
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ModifyHatching();", True)

    End Sub

    ''' <summary>
    ''' 画面表示データ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MAPDataGet(ByVal SQLcon As MySqlConnection)

        If IsNothing(LNM0014tbl) Then
            LNM0014tbl = New DataTable
        End If

        If LNM0014tbl.Columns.Count <> 0 Then
            LNM0014tbl.Columns.Clear()
        End If

        LNM0014tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを予算分類変更履歴から取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                      ")
        SQLStr.AppendLine("     1                                                                AS 'SELECT'            ")
        SQLStr.AppendLine("   , 0                                                                AS HIDDEN              ")
        SQLStr.AppendLine("   , 0                                                                AS LINECNT             ")
        SQLStr.AppendLine("   , ''                                                               AS OPERATION           ")
        SQLStr.AppendLine("   , UPDTIMSTP                                                        AS UPDTIMSTP           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(DELFLG), '')                                      AS DELFLG              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(TARGETYM), '')                                    AS TARGETYM            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(TORICODE), '')                                    AS TORICODE            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(TORINAME), '')                                    AS TORINAME            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ORGCODE), '')                                     AS ORGCODE             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ORGNAME), '')                                     AS ORGNAME             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(KASANORGCODE), '')                                AS KASANORGCODE        ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(KASANORGNAME), '')                                AS KASANORGNAME        ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(BIGCATECODE), '')                                 AS BIGCATECODE         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(BIGCATENAME), '')                                 AS BIGCATENAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(MIDCATECODE), '')                                 AS MIDCATECODE         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(MIDCATENAME), '')                                 AS MIDCATENAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SMALLCATECODE), '')                               AS SMALLCATECODE       ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SMALLCATENAME), '')                               AS SMALLCATENAME       ")
#Region "コメント-2025/08/04(分類追加対応のため)"
        'SQLStr.AppendLine("   , COALESCE(RTRIM(TODOKECODE), '')                                  AS TODOKECODE          ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(TODOKENAME), '')                                  AS TODOKENAME          ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(GROUPSORTNO), '')                                 AS GROUPSORTNO         ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(GROUPID), '')                                     AS GROUPID             ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(GROUPNAME), '')                                   AS GROUPNAME           ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(DETAILSORTNO), '')                                AS DETAILSORTNO        ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(DETAILID), '')                                    AS DETAILID            ")
        'SQLStr.AppendLine("   , COALESCE(RTRIM(DETAILNAME), '')                                  AS DETAILNAME          ")
#End Region
        SQLStr.AppendLine("   , COALESCE(RTRIM(TANKA), '')                                       AS TANKA               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(QUANTITY), '')                                    AS QUANTITY            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(CALCUNIT), '')                                    AS CALCUNIT            ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(DEPARTURE), '')                                   AS DEPARTURE           ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(MILEAGE), '')                                     AS MILEAGE             ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SHIPPINGCOUNT), '')                               AS SHIPPINGCOUNT       ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(NENPI), '')                                       AS NENPI               ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(DIESELPRICECURRENT), '')                          AS DIESELPRICECURRENT  ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(DIESELPRICESTANDARD), '')                         AS DIESELPRICESTANDARD ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(DIESELCONSUMPTION), '')                           AS DIESELCONSUMPTION   ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(DISPLAYFLG), '')                                  AS DISPLAYFLG          ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ASSESSMENTFLG), '')                               AS ASSESSMENTFLG       ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ATENACOMPANYNAME), '')                            AS ATENACOMPANYNAME    ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ATENACOMPANYDEVNAME), '')                         AS ATENACOMPANYDEVNAME ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(FROMORGNAME), '')                                 AS FROMORGNAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(MEISAICATEGORYID), '')                            AS MEISAICATEGORYID    ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ACCOUNTCODE), '')                                 AS ACCOUNTCODE         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ACCOUNTNAME), '')                                 AS ACCOUNTNAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SEGMENTCODE), '')                                 AS SEGMENTCODE         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(SEGMENTNAME), '')                                 AS SEGMENTNAME         ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(JOTPERCENTAGE), '')                               AS JOTPERCENTAGE       ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(ENEXPERCENTAGE), '')                              AS ENEXPERCENTAGE      ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(BIKOU1), '')                                      AS BIKOU1              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(BIKOU2), '')                                      AS BIKOU2              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(BIKOU3), '')                                      AS BIKOU3              ")

        '画面表示用
        '大分類コード
        SQLStr.AppendLine("   , CASE                                                                                    ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(BIGCATECODE), '') = '' THEN ''                                     ")
        SQLStr.AppendLine("      ELSE  FORMAT(BIGCATECODE,0)                                                            ")
        SQLStr.AppendLine("     END AS SCRBIGCATECODE                                                                   ")
        '中分類コード
        SQLStr.AppendLine("   , CASE                                                                                    ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(MIDCATECODE), '') = '' THEN ''                                     ")
        SQLStr.AppendLine("      ELSE  FORMAT(MIDCATECODE,0)                                                            ")
        SQLStr.AppendLine("     END AS SCRMIDCATECODE                                                                   ")
        '小分類コード
        SQLStr.AppendLine("   , CASE                                                                                    ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(SMALLCATECODE), '') = '' THEN ''                                   ")
        SQLStr.AppendLine("      ELSE  FORMAT(SMALLCATECODE,0)                                                          ")
        SQLStr.AppendLine("     END AS SCRSMALLCATECODE                                                                 ")
#Region "コメント-2025/08/04(分類追加対応のため)"
        ''グループソート順
        'SQLStr.AppendLine("   , CASE                                                                                            ")
        'SQLStr.AppendLine("      WHEN COALESCE(RTRIM(GROUPSORTNO), '') = '' THEN ''                                                   ")
        'SQLStr.AppendLine("      ELSE  FORMAT(GROUPSORTNO,0)                                                                          ")
        'SQLStr.AppendLine("     END AS SCRGROUPSORTNO                                                                                 ")
        ''グループID
        'SQLStr.AppendLine("   , CASE                                                                                            ")
        'SQLStr.AppendLine("      WHEN COALESCE(RTRIM(GROUPID), '') = '' THEN ''                                                   ")
        'SQLStr.AppendLine("      ELSE  FORMAT(GROUPID,0)                                                                          ")
        'SQLStr.AppendLine("     END AS SCRGROUPID                                                                                 ")
        ''明細ソート順
        'SQLStr.AppendLine("   , CASE                                                                                            ")
        'SQLStr.AppendLine("      WHEN COALESCE(RTRIM(DETAILSORTNO), '') = '' THEN ''                                                   ")
        'SQLStr.AppendLine("      ELSE  FORMAT(DETAILSORTNO,0)                                                                          ")
        'SQLStr.AppendLine("     END AS SCRDETAILSORTNO                                                                                 ")
        ''明細ID
        'SQLStr.AppendLine("   , CASE                                                                                            ")
        'SQLStr.AppendLine("      WHEN COALESCE(RTRIM(DETAILID), '') = '' THEN ''                                                   ")
        'SQLStr.AppendLine("      ELSE  FORMAT(DETAILID,0)                                                                          ")
        'SQLStr.AppendLine("     END AS SCRDETAILID                                                                                 ")
#End Region
        '単価
        SQLStr.AppendLine("   , CASE                                                                                    ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(TANKA), '') = '' THEN ''                                           ")
        SQLStr.AppendLine("      ELSE  FORMAT(TANKA,2)                                                                  ")
        SQLStr.AppendLine("     END AS SCRTANKA                                                                         ")
        '数量
        SQLStr.AppendLine("   , CASE                                                                                    ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(QUANTITY), '') = '' THEN ''                                        ")
        SQLStr.AppendLine("      ELSE  FORMAT(QUANTITY,2)                                                               ")
        SQLStr.AppendLine("     END AS SCRQUANTITY                                                                      ")
        '走行距離
        SQLStr.AppendLine("   , CASE                                                                                    ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(MILEAGE), '') = '' THEN ''                                         ")
        SQLStr.AppendLine("      ELSE  FORMAT(MILEAGE,2)                                                                ")
        SQLStr.AppendLine("     END AS SCRMILEAGE                                                                       ")
        '輸送回数
        SQLStr.AppendLine("   , CASE                                                                                    ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(SHIPPINGCOUNT), '') = '' THEN ''                                   ")
        SQLStr.AppendLine("      ELSE  FORMAT(SHIPPINGCOUNT,0)                                                          ")
        SQLStr.AppendLine("     END AS SCRSHIPPINGCOUNT                                                                 ")
        '燃費
        SQLStr.AppendLine("   , CASE                                                                                    ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(NENPI), '') = '' THEN ''                                           ")
        SQLStr.AppendLine("      ELSE  FORMAT(NENPI,2)                                                                  ")
        SQLStr.AppendLine("     END AS SCRNENPI                                                                         ")
        '実勢軽油価格
        SQLStr.AppendLine("   , CASE                                                                                    ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(DIESELPRICECURRENT), '') = '' THEN ''                              ")
        SQLStr.AppendLine("      ELSE  FORMAT(DIESELPRICECURRENT,2)                                                     ")
        SQLStr.AppendLine("     END AS SCRDIESELPRICECURRENT                                                            ")
        '基準経由価格
        SQLStr.AppendLine("   , CASE                                                                                    ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(DIESELPRICESTANDARD), '') = '' THEN ''                             ")
        SQLStr.AppendLine("      ELSE  FORMAT(DIESELPRICESTANDARD,2)                                                    ")
        SQLStr.AppendLine("     END AS SCRDIESELPRICESTANDARD                                                           ")
        '燃料使用量
        SQLStr.AppendLine("   , CASE                                                                                    ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(DIESELCONSUMPTION), '') = '' THEN ''                               ")
        SQLStr.AppendLine("      ELSE  FORMAT(DIESELCONSUMPTION,2)                                                      ")
        SQLStr.AppendLine("     END AS SCRDIESELCONSUMPTION                                                             ")

        '表示フラグ
        SQLStr.AppendLine("   , ''                                                                       AS SCRDISPLAYFLG        ")
        '鑑分けフラグ
        SQLStr.AppendLine("   , ''                                                                       AS SCRASSESSMENTFLG     ")
        '明細区分
        SQLStr.AppendLine("   , ''                                                                       AS SCRMEISAICATEGORYID  ")
        '割合JOT
        SQLStr.AppendLine("   , ''                                                                       AS SCRJOTPERCENTAGE   ")
        '割合ENEX
        SQLStr.AppendLine("   , ''                                                                       AS SCRENEXPERCENTAGE  ")

        SQLStr.AppendLine("   , Case                 ")
        SQLStr.AppendLine("      When COALESCE(RTRIM(OPERATEKBN), '') ='2' AND COALESCE(RTRIM(MODIFYKBN), '') ='2' THEN '変更前 更新' ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(OPERATEKBN), '') ='2' AND COALESCE(RTRIM(MODIFYKBN), '') ='3' THEN '変更後 更新' ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(OPERATEKBN), '') ='3' AND COALESCE(RTRIM(MODIFYKBN), '') ='2' THEN '変更前 削除' ")
        SQLStr.AppendLine("      WHEN COALESCE(RTRIM(OPERATEKBN), '') ='3' AND COALESCE(RTRIM(MODIFYKBN), '') ='3' THEN '変更後 削除' ")
        SQLStr.AppendLine("      ELSE ''                                                                                          ")
        SQLStr.AppendLine("    END AS OPERATEKBNNAME                                                                              ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(MODIFYKBN), '')                                                 AS MODIFYKBN                 ")
        SQLStr.AppendLine("   ,CASE COALESCE(RTRIM(MODIFYKBN), '')                                                                          ")
        SQLStr.AppendLine("      WHEN '1' THEN '新規'                                                                                     ")
        SQLStr.AppendLine("      WHEN '2' THEN '変更前'                                                                                   ")
        SQLStr.AppendLine("      WHEN '3' THEN '変更後'                                                                                   ")
        SQLStr.AppendLine("      ELSE ''                                                                                                  ")
        SQLStr.AppendLine("    END AS MODIFYKBNNAME                                                                                       ")
        'SQLStr.AppendLine("   , FORMAT(MODIFYYMD, 'yyyy/MM/dd HH:mm:ss')                                     AS MODIFYYMD                 ")
        SQLStr.AppendLine("   , DATE_FORMAT(MODIFYYMD, '%Y/%m/%d %T')                                     AS MODIFYYMD                 ")
        SQLStr.AppendLine("   , COALESCE(RTRIM(MODIFYUSER), '')                                           AS MODIFYUSER                ")
        SQLStr.AppendLine(" FROM                                                                                                          ")
        SQLStr.AppendLine("     LNG.LNT0015_SPRATEHIST2                                                                                    ")
        SQLStr.AppendLine(" WHERE                                                                                                 ")
        '変更日が指定されている場合
        If Not WF_DDL_MODIFYDD.SelectedValue = "" Then
            SQLStr.AppendLine("    DATE_FORMAT(MODIFYYMD,'%Y/%m/%d')  = @MODIFYYMD                                                  ")
        Else
            SQLStr.AppendLine("    DATE_FORMAT(MODIFYYMD,'%Y/%m/01')  = @MODIFYYMD                                                  ")
        End If
        '変更ユーザが指定されている場合
        If Not WF_DDL_MODIFYUSER.SelectedValue = "" Then
            SQLStr.AppendLine(" AND COALESCE(RTRIM(MODIFYUSER), '')  =  @MODIFYUSER ")
        End If
        SQLStr.AppendLine(" ORDER BY                                                                                              ")
        SQLStr.AppendLine("    MODIFYYMD DESC                                                                                     ")
        SQLStr.AppendLine("    ,TARGETYM                                                           ")
        SQLStr.AppendLine("    ,TORICODE                                                           ")
        SQLStr.AppendLine("    ,ORGCODE                                                            ")
        SQLStr.AppendLine("    ,BIGCATECODE                                                        ")
        SQLStr.AppendLine("    ,MIDCATECODE                                                        ")
        SQLStr.AppendLine("    ,SMALLCATECODE                                                      ")
#Region "コメント-2025/08/04(分類追加対応のため)"
        'SQLStr.AppendLine("    ,GROUPID                                                            ")
        'SQLStr.AppendLine("    ,DETAILID                                                           ")
#End Region
        SQLStr.AppendLine("    ,MODIFYKBN                                                          ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.VarChar, 10)         '変更年月
                '変更日が指定されている場合
                If Not WF_DDL_MODIFYDD.SelectedValue = "" Then
                    P_MODIFYYMD.Value = WF_DDL_MODIFYYM.SelectedValue + "/" + WF_DDL_MODIFYDD.SelectedValue
                Else
                    P_MODIFYYMD.Value = WF_DDL_MODIFYYM.SelectedValue + "/01"
                End If

                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザ
                '変更ユーザが指定されている場合
                If Not WF_DDL_MODIFYUSER.SelectedValue = "" Then
                    P_MODIFYUSER.Value = WF_DDL_MODIFYUSER.SelectedValue
                Else
                    P_MODIFYUSER.Value = ""
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0014tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0014tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0014row As DataRow In LNM0014tbl.Rows
                    i += 1
                    LNM0014row("LINECNT") = i        'LINECNT

                    '表示フラグ
                    Select Case LNM0014row("DISPLAYFLG").ToString
                        Case "0" : LNM0014row("SCRDISPLAYFLG") = "表示しない"
                        Case "1" : LNM0014row("SCRDISPLAYFLG") = "表示する"
                        Case Else : LNM0014row("SCRDISPLAYFLG") = ""
                    End Select

                    '鑑分けフラグ
                    Select Case LNM0014row("ASSESSMENTFLG").ToString
                        Case "0" : LNM0014row("SCRASSESSMENTFLG") = "鑑分けしない"
                        Case "1" : LNM0014row("SCRASSESSMENTFLG") = "鑑分けする"
                        Case Else : LNM0014row("SCRASSESSMENTFLG") = ""
                    End Select

                    '明細区分
                    Select Case LNM0014row("MEISAICATEGORYID").ToString
                        Case "1" : LNM0014row("SCRMEISAICATEGORYID") = "請求追加明細(特別料金)"
                        Case "2" : LNM0014row("SCRMEISAICATEGORYID") = "サーチャージ"
                        Case Else : LNM0014row("SCRMEISAICATEGORYID") = ""
                    End Select

                    '割合JOT
                    Select Case LNM0014row("JOTPERCENTAGE").ToString
                        Case "" : LNM0014row("SCRJOTPERCENTAGE") = ""
                        Case Else : LNM0014row("SCRJOTPERCENTAGE") = LNM0014row("JOTPERCENTAGE").ToString & "%"
                    End Select

                    '割合ENEX
                    Select Case LNM0014row("ENEXPERCENTAGE").ToString
                        Case "" : LNM0014row("SCRENEXPERCENTAGE") = ""
                        Case Else : LNM0014row("SCRENEXPERCENTAGE") = LNM0014row("ENEXPERCENTAGE").ToString & "%"
                    End Select

                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014H SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014H Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 変更年月取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MODIFYYMGet(ByVal SQLcon As MySqlConnection)

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT ")
        'SQLStr.AppendLine("     FORMAT(MODIFYYMD, 'yyyy/MM') AS MODIFYYM ")
        SQLStr.AppendLine("     DATE_FORMAT(MODIFYYMD, '%Y/%m') AS MODIFYYM ")
        SQLStr.AppendLine(" FROM LNG.LNT0015_SPRATEHIST2")
        SQLStr.AppendLine(" ORDER BY MODIFYYM DESC ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable
                    Dim WW_CURRENTMONTH As String = Date.Now.ToString("yyyy/MM")

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)

                    WF_DDL_MODIFYYM.Items.Clear()
                    'WF_DDL_MODIFYYM.Items.Add("")
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        WF_DDL_MODIFYYM.Items.Add(WW_ROW("MODIFYYM"))
                    Next

                    '当月がドロップダウンリストに存在しない場合追加
                    If WF_DDL_MODIFYYM.Items.FindByValue(WW_CURRENTMONTH) Is Nothing Then
                        WF_DDL_MODIFYYM.Items.Insert(0, WW_CURRENTMONTH)
                    End If

                    WF_DDL_MODIFYYM.SelectedValue = WW_CURRENTMONTH

                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0015_SPRATEHIST2 SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0015_SPRATEHIST2 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 変更日取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MODIFYDDGet(ByVal SQLcon As MySqlConnection)

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT ")
        'SQLStr.AppendLine("     FORMAT(MODIFYYMD, 'dd') AS MODIFYDD ")
        SQLStr.AppendLine("     DATE_FORMAT(MODIFYYMD, '%d') AS MODIFYDD ")
        SQLStr.AppendLine(" FROM LNG.LNT0015_SPRATEHIST2")
        SQLStr.AppendLine(" WHERE                                                                                                 ")
        'SQLStr.AppendLine("    FORMAT(MODIFYYMD, 'yyyy/MM')  = @MODIFYYM                                                         ")
        SQLStr.AppendLine("    DATE_FORMAT(MODIFYYMD, '%Y/%m')  = @MODIFYYM                                                         ")
        SQLStr.AppendLine(" ORDER BY MODIFYDD ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_MODIFYYM As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYM", MySqlDbType.VarChar, 7)         '変更年月
                P_MODIFYYM.Value = WF_DDL_MODIFYYM.SelectedValue

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)

                    WF_DDL_MODIFYDD.Items.Clear()
                    WF_DDL_MODIFYDD.Items.Add("")
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        WF_DDL_MODIFYDD.Items.Add(WW_ROW("MODIFYDD"))
                    Next

                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0015_SPRATEHIST2 SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0015_SPRATEHIST2 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 変更ユーザ取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub MODIFYUSERGet(ByVal SQLcon As MySqlConnection)

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT ")
        SQLStr.AppendLine("     MODIFYUSER ")
        SQLStr.AppendLine(" FROM LNG.LNT0015_SPRATEHIST2 ")
        SQLStr.AppendLine(" WHERE                                                                                                 ")
        '変更日が指定されている場合
        If Not WF_DDL_MODIFYDD.SelectedValue = "" Then
            'SQLStr.AppendLine("    FORMAT(MODIFYYMD, 'yyyy/MM/dd')  = @MODIFYYMD                                                  ")
            SQLStr.AppendLine("    DATE_FORMAT(MODIFYYMD,'%Y/%m/%d')  = @MODIFYYMD                                                  ")
        Else
            'SQLStr.AppendLine("    FORMAT(MODIFYYMD, 'yyyy/MM/01')  = @MODIFYYMD                                                  ")
            SQLStr.AppendLine("    DATE_FORMAT(MODIFYYMD,'%Y/%m/01')  = @MODIFYYMD                                                  ")
        End If

        SQLStr.AppendLine(" ORDER BY MODIFYUSER  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.VarChar, 10)         '変更年月日
                '変更日が指定されている場合
                If Not WF_DDL_MODIFYDD.SelectedValue = "" Then
                    P_MODIFYYMD.Value = WF_DDL_MODIFYYM.SelectedValue + "/" + WF_DDL_MODIFYDD.SelectedValue
                Else
                    P_MODIFYYMD.Value = WF_DDL_MODIFYYM.SelectedValue + "/01"
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)

                    WF_DDL_MODIFYUSER.Items.Clear()
                    WF_DDL_MODIFYUSER.Items.Add("")
                    For Each WW_ROW As DataRow In WW_Tbl.Rows
                        WF_DDL_MODIFYUSER.Items.Add(WW_ROW("MODIFYUSER"))
                    Next

                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0015_SPRATEHIST2 SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0015_SPRATEHIST2 Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNM0014row As DataRow In LNM0014tbl.Rows
            If LNM0014row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0014row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(LNM0014tbl)

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

        '変更箇所を強調表示
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ModifyHatching();", True)

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()
        'パンくずから検索を選択した場合
        If WF_ButtonClick.Value = "LNM0014S" Then
            Master.MAPID = LNM0014WRKINC.MAPIDL
        End If

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 先頭頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonFIRST_Click()

        '○ 先頭頁に移動
        WF_GridPosition.Text = "1"

    End Sub

    ''' <summary>
    ''' 最終頁ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonLAST_Click()

        '○ ソート
        Dim TBLview As New DataView(LNM0014tbl)
        TBLview.RowFilter = "HIDDEN = 0"

        '○ 最終頁に移動
        If TBLview.Count Mod 10 = 0 Then
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10)
        Else
            WF_GridPosition.Text = TBLview.Count - (TBLview.Count Mod 10) + 1
        End If

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' 変更年月フィールドチェンジ
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_SelectMODIFYYM_CHANGE()
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続
            '変更日取得
            MODIFYDDGet(SQLcon)
            '変更ユーザ取得
            MODIFYUSERGet(SQLcon)
        End Using

        '〇表示するボタン押下時処理
        WF_WF_ButtonMODIFY_VIEW()

    End Sub

    ''' <summary>
    ''' 変更年月フィールドチェンジ
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_SelectMODIFYDD_CHANGE()
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続
            '変更ユーザ取得
            MODIFYUSERGet(SQLcon)
        End Using

        '〇表示するボタン押下時処理
        WF_WF_ButtonMODIFY_VIEW()

    End Sub

    ''' <summary>
    ''' 表示するボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_WF_ButtonMODIFY_VIEW()
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            MAPDataGet(SQLcon)
            Master.SaveTable(LNM0014tbl)
            '〇 一覧の件数を取得
            Me.ListCount.Text = "件数：" + LNM0014tbl.Rows.Count.ToString()
        End Using
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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0014WRKINC.HISTORYEXCELCOL)).Cast(Of Integer)().Max()

        'シート名
        wb.ActiveSheet.Name = Left(WF_DDL_MODIFYYM.SelectedValue, 4) + "年" + Right(WF_DDL_MODIFYYM.SelectedValue, 2) + "月"

        '行幅設定
        SetROWSHEIGHT(wb.ActiveSheet)

        '明細設定
        Dim WW_ACTIVEROW As Integer = 3
        SetDETAIL(wb, wb.ActiveSheet, WW_ACTIVEROW)

        'シート全体設定
        SetALL(wb.ActiveSheet)

        '明細の線を引く
        Dim WW_MAXRANGE As String = wb.ActiveSheet.Cells(WW_ACTIVEROW - 1, WW_MAXCOL).Address
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders.LineStyle = BorderLineStyle.Dotted
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeTop).LineStyle = BorderLineStyle.Thin
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeBottom).LineStyle = BorderLineStyle.Thin
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeLeft).LineStyle = BorderLineStyle.Thin
        wb.ActiveSheet.Range("A4:" + WW_MAXRANGE).Borders(BordersIndex.EdgeRight).LineStyle = BorderLineStyle.Thin

        'ヘッダ設定
        SetHEADER(wb.ActiveSheet, WW_MAXCOL)

        'その他設定
        wb.ActiveSheet.Range("A1").Value = "ID:" + Master.MAPID
        wb.ActiveSheet.Range("A2").Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_MODIFY)
        wb.ActiveSheet.Range("B2").Value = "は変更項目"
        wb.ActiveSheet.Range("C1").Value = "特別料金マスタ変更履歴一覧"

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
            Case LNM0014WRKINC.FILETYPE.EXCEL
                FileName = "特別料金マスタ変更履歴.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNM0014WRKINC.FILETYPE.PDF
                FileName = "特別料金マスタ変更履歴.pdf"
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
            '.Color = Color.FromArgb(0, 0, 0)
            .Name = "Meiryo UI"
            .Size = 11
        End With
        '配置
        sheet.Columns.VerticalAlignment = VerticalAlignment.Center
        'sheet.Rows.HorizontalAlignment = HorizontalAlignment.Center
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
    Public Sub SetHEADER(ByVal sheet As IWorksheet, ByVal WW_MAXCOL As Integer)
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
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.OPERATEKBNNAME).Value = "操作区分"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.MODIFYKBNNAME).Value = "変更区分"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.MODIFYYMD).Value = "変更日時"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.MODIFYUSER).Value = "変更USER"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.DELFLG).Value = "削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.TARGETYM).Value = "対象年月"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.TORICODE).Value = "取引先コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.TORINAME).Value = "取引先名称"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.ORGCODE).Value = "部門コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.ORGNAME).Value = "部門名称"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.KASANORGCODE).Value = "加算先部門コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.KASANORGNAME).Value = "加算先部門名称"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.BIGCATECODE).Value = "大分類コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.BIGCATENAME).Value = "大分類名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.MIDCATECODE).Value = "中分類コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.MIDCATENAME).Value = "中分類名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.SMALLCATECODE).Value = "小分類コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.SMALLCATENAME).Value = "小分類名"
#Region "コメント-2025/08/04(分類追加対応のため)"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.TODOKECODE).Value = "届先コード"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.TODOKENAME).Value = "届先名称"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.GROUPSORTNO).Value = "グループソート順"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.GROUPID).Value = "グループID"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.GROUPNAME).Value = "グループ名"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.DETAILSORTNO).Value = "明細ソート順"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.DETAILID).Value = "明細ID"
        'sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.DETAILNAME).Value = "明細名"
#End Region
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.TANKA).Value = "単価"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.QUANTITY).Value = "数量"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.CALCUNIT).Value = "計算単位"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.DEPARTURE).Value = "出荷地"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.MILEAGE).Value = "走行距離"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.SHIPPINGCOUNT).Value = "輸送回数"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.NENPI).Value = "燃費"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.DIESELPRICECURRENT).Value = "実勢軽油価格"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.DIESELPRICESTANDARD).Value = "基準経由価格"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.DIESELCONSUMPTION).Value = "燃料使用量"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.DISPLAYFLG).Value = "表示フラグ"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.ASSESSMENTFLG).Value = "鑑分けフラグ"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.ATENACOMPANYNAME).Value = "宛名会社名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.ATENACOMPANYDEVNAME).Value = "宛名会社部門名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.FROMORGNAME).Value = "請求書発行部店名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.MEISAICATEGORYID).Value = "明細区分"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.ACCOUNTCODE).Value = "勘定科目コード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.ACCOUNTNAME).Value = "勘定科目名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.SEGMENTCODE).Value = "セグメントコード"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.SEGMENTNAME).Value = "セグメント名"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.JOTPERCENTAGE).Value = "割合JOT"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.ENEXPERCENTAGE).Value = "割合ENEX"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.BIKOU1).Value = "備考1"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.BIKOU2).Value = "備考2"
        sheet.Cells(WW_HEADERROW, LNM0014WRKINC.HISTORYEXCELCOL.BIKOU3).Value = "備考3"

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
        DecStyle.NumberFormat = "#,##0.00_);[Red](#,##0.00)"

        For Each Row As DataRow In LNM0014tbl.Rows
            '値
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.OPERATEKBNNAME).Value = Row("OPERATEKBNNAME") '操作区分
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.MODIFYKBNNAME).Value = Row("MODIFYKBNNAME") '変更区分
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.MODIFYYMD).Value = Row("MODIFYYMD") '変更日時
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.MODIFYUSER).Value = Row("MODIFYUSER") '変更USER
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.TARGETYM).Value = Row("TARGETYM") '対象年月
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.TORICODE).Value = Row("TORICODE") '取引先コード
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.TORINAME).Value = Row("TORINAME") '取引先名称
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.ORGCODE).Value = Row("ORGCODE") '部門コード
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.ORGNAME).Value = Row("ORGNAME") '部門名称
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.KASANORGCODE).Value = Row("KASANORGCODE") '加算先部門コード
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.KASANORGNAME).Value = Row("KASANORGNAME") '加算先部門名称
            '大分類コード
            If Row("BIGCATECODE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.BIGCATECODE).Value = Row("BIGCATECODE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.BIGCATECODE).Value = CDbl(Row("BIGCATECODE"))
            End If
            '大分類名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.BIGCATENAME).Value = Row("BIGCATENAME")

            '中分類コード
            If Row("MIDCATECODE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.MIDCATECODE).Value = Row("MIDCATECODE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.MIDCATECODE).Value = CDbl(Row("MIDCATECODE"))
            End If
            '中分類名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.MIDCATENAME).Value = Row("MIDCATENAME")

            '小分類コード
            If Row("SMALLCATECODE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.SMALLCATECODE).Value = Row("SMALLCATECODE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.SMALLCATECODE).Value = CDbl(Row("SMALLCATECODE"))
            End If
            '小分類名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.SMALLCATENAME).Value = Row("SMALLCATENAME")
#Region "コメント-2025/08/04(分類追加対応のため)"
            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.TODOKECODE).Value = Row("TODOKECODE") '届先コード
            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.TODOKENAME).Value = Row("TODOKENAME") '届先名称

            ''グループソート順
            'If Row("GROUPSORTNO") = "" Then
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.GROUPSORTNO).Value = Row("GROUPSORTNO")
            'Else
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.GROUPSORTNO).Value = CDbl(Row("GROUPSORTNO"))
            'End If

            ''グループID
            'If Row("GROUPID") = "" Then
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.GROUPID).Value = Row("GROUPID")
            'Else
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.GROUPID).Value = CDbl(Row("GROUPID"))
            'End If

            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.GROUPNAME).Value = Row("GROUPNAME") 'グループ名

            ''明細ソート順
            'If Row("DETAILSORTNO") = "" Then
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DETAILSORTNO).Value = Row("DETAILSORTNO")
            'Else
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DETAILSORTNO).Value = CDbl(Row("DETAILSORTNO"))
            'End If

            ''明細ID
            'If Row("DETAILID") = "" Then
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DETAILID).Value = Row("DETAILID")
            'Else
            '    sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DETAILID).Value = CDbl(Row("DETAILID"))
            'End If

            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DETAILNAME).Value = Row("DETAILNAME") '明細名
#End Region
            '単価
            If Row("TANKA") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.TANKA).Value = Row("TANKA")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.TANKA).Value = CDbl(Row("TANKA"))
            End If

            '数量
            If Row("QUANTITY") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.QUANTITY).Value = Row("QUANTITY")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.QUANTITY).Value = CDbl(Row("QUANTITY"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.CALCUNIT).Value = Row("CALCUNIT") '計算単位
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DEPARTURE).Value = Row("DEPARTURE") '出荷地

            '走行距離
            If Row("MILEAGE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.MILEAGE).Value = Row("MILEAGE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.MILEAGE).Value = CDbl(Row("MILEAGE"))
            End If

            '輸送回数
            If Row("SHIPPINGCOUNT") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.SHIPPINGCOUNT).Value = Row("SHIPPINGCOUNT")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.SHIPPINGCOUNT).Value = CDbl(Row("SHIPPINGCOUNT"))
            End If

            '燃費
            If Row("NENPI") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.NENPI).Value = Row("NENPI")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.NENPI).Value = CDbl(Row("NENPI"))
            End If

            '実勢軽油価格
            If Row("DIESELPRICECURRENT") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DIESELPRICECURRENT).Value = Row("DIESELPRICECURRENT")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DIESELPRICECURRENT).Value = CDbl(Row("DIESELPRICECURRENT"))
            End If

            '基準経由価格
            If Row("DIESELPRICESTANDARD") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DIESELPRICESTANDARD).Value = Row("DIESELPRICESTANDARD")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DIESELPRICESTANDARD).Value = CDbl(Row("DIESELPRICESTANDARD"))
            End If

            '燃料使用量
            If Row("DIESELCONSUMPTION") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DIESELCONSUMPTION).Value = Row("DIESELCONSUMPTION")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DIESELCONSUMPTION).Value = CDbl(Row("DIESELCONSUMPTION"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DISPLAYFLG).Value = Row("DISPLAYFLG") '表示フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.ASSESSMENTFLG).Value = Row("ASSESSMENTFLG") '鑑分けフラグ
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.ATENACOMPANYNAME).Value = Row("ATENACOMPANYNAME") '宛名会社名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.ATENACOMPANYDEVNAME).Value = Row("ATENACOMPANYDEVNAME") '宛名会社部門名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.FROMORGNAME).Value = Row("FROMORGNAME") '請求書発行部店名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.MEISAICATEGORYID).Value = Row("MEISAICATEGORYID") '明細区分

            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.ACCOUNTCODE).Value = Row("ACCOUNTCODE") '勘定科目コード
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.ACCOUNTNAME).Value = Row("ACCOUNTNAME") '勘定科目名
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.SEGMENTCODE).Value = Row("SEGMENTCODE") 'セグメントコード
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.SEGMENTNAME).Value = Row("SEGMENTNAME") 'セグメント名

            '割合JOT
            If Row("JOTPERCENTAGE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.JOTPERCENTAGE).Value = Row("JOTPERCENTAGE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.JOTPERCENTAGE).Value = CDbl(Row("JOTPERCENTAGE"))
            End If

            '割合ENEX
            If Row("ENEXPERCENTAGE") = "" Then
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.ENEXPERCENTAGE).Value = Row("ENEXPERCENTAGE")
            Else
                sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.ENEXPERCENTAGE).Value = CDbl(Row("ENEXPERCENTAGE"))
            End If

            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.BIKOU1).Value = Row("BIKOU1") '備考1
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.BIKOU2).Value = Row("BIKOU2") '備考2
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.BIKOU3).Value = Row("BIKOU3") '備考3

            '数値形式に変更
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.BIGCATECODE).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.MIDCATECODE).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.SMALLCATECODE).Style = IntStyle
#Region "コメント-2025/08/04(分類追加対応のため)"
            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.GROUPSORTNO).Style = IntStyle
            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.GROUPID).Style = IntStyle
            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DETAILSORTNO).Style = IntStyle
            'sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DETAILID).Style = IntStyle
#End Region
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.TANKA).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.QUANTITY).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.MILEAGE).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.SHIPPINGCOUNT).Style = IntStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.NENPI).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DIESELPRICECURRENT).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DIESELPRICESTANDARD).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.DIESELCONSUMPTION).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.JOTPERCENTAGE).Style = DecStyle
            sheet.Cells(WW_ACTIVEROW, LNM0014WRKINC.HISTORYEXCELCOL.ENEXPERCENTAGE).Style = DecStyle

            '変更区分が変更後の行の場合
            If Row("MODIFYKBN") = LNM0014WRKINC.MODIFYKBN.AFTDATA Then
                '変更箇所を塗りつぶし
                SetMODIFYHATCHING(sheet, WW_ACTIVEROW)
            End If

            WW_ACTIVEROW += 1
        Next
    End Sub

    ''' <summary>
    ''' 変更箇所を塗りつぶし
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetMODIFYHATCHING(ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)
        '最大列(RANGE)を取得
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0014WRKINC.HISTORYEXCELCOL)).Cast(Of Integer)().Max()

        '変更チェック開始列を取得
        Dim WW_STCOL As Integer = LNM0014WRKINC.HISTORYEXCELCOL.DELFLG   '削除フラグ

        '開始列から最大列まで変更前後の値を確認
        For index As Integer = WW_STCOL To WW_MAXCOL
            '変更前と変更後が不一致の場合
            If Not Convert.ToString(sheet.Cells(WW_ACTIVEROW - 1, index).Value) = Convert.ToString(sheet.Cells(WW_ACTIVEROW, index).Value) Then

                '変更後の背景色を塗りつぶし
                sheet.Cells(WW_ACTIVEROW, index).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_MODIFY)

                '変更後のフォント色を変える
                sheet.Cells(WW_ACTIVEROW, index).Font.Color = ColorTranslator.FromHtml(CONST_COLOR_RED)
                sheet.Cells(WW_ACTIVEROW, index).Font.Bold = True

            End If
        Next
    End Sub
#End Region

End Class

