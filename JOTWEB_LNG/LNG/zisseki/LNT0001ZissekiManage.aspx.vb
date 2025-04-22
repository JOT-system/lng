'************************************************************
' 実績管理
' 作成日 2024/12/01
' 更新日 
' 作成者 
' 更新者 
'
' 修正履歴 
'************************************************************

Imports GrapeCity.Documents.Excel
Imports Newtonsoft.Json
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0001ZissekiManage
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private LNT0001tbl As DataTable                                  '一覧格納用テーブル

    ''' <summary>
    ''' 定数
    ''' </summary>
    Private Const CONST_DISPROWCOUNT As Integer = 16                '1画面表示用
    Private Const CONST_SCROLLCOUNT As Integer = 16                 'マウススクロール時稼働行数

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private GS0007FIXVALUElst As New GS0007FIXVALUElst              '固定値マスタ

    Private WW_Dummy As String

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
                    Master.RecoverTable(LNT0001tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonExtract"         '検索ボタンクリック
                            WF_ButtonExtract_Click()
                        Case "WF_ButtonZisseki"         '実績取込ボタンクリック
                            WF_ButtonZisseki_Click()
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FiledDBClick()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ButtonInvoice"         '請求書出力ボタン押下
                            WF_ButtonInvoice_Click()
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_ButtonPRINT_Click()
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_TORI"                  'リスト変更
                            WF_ListChange(WF_ButtonClick.Value)
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

        '○ 画面ID設定
        Master.MAPID = LNT0001WRKINC.MAPIDL
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
        rightviewR.ResetIndex()
        leftview.ActiveListBox()

        '○ 右Boxへの値設定
        rightviewR.MAPID = Master.MAPID
        rightviewR.MAPVARI = Master.MAPvariant
        rightviewR.COMPCODE = Master.USERCAMP
        rightviewR.PROFID = Master.PROF_REPORT
        rightviewR.Initialize("")

        '○ RightBox情報設定
        rightviewD.MAPIDS = Master.MAPID
        rightviewD.MAPID = Master.MAPID
        rightviewD.COMPCODE = Master.USERCAMP
        rightviewD.MAPVARI = Master.MAPvariant
        rightviewD.PROFID = Master.PROF_VIEW
        rightviewD.MENUROLE = Master.ROLE_MENU
        rightviewD.MAPROLE = Master.ROLE_MAP
        rightviewD.VIEWROLE = Master.ROLE_VIEWPROF
        rightviewD.RPRTROLE = Master.ROLE_RPRTPROF

        rightviewD.Initialize("画面レイアウト設定", WW_dummy)

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU

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
        ' ドロップダウンリスト（荷主）作成
        Dim toriList As New ListBox
        GS0007FIXVALUElst.CAMPCODE = Master.USERCAMP
        GS0007FIXVALUElst.CLAS = "TORICODEDROP"
        GS0007FIXVALUElst.LISTBOX1 = toriList
        GS0007FIXVALUElst.ADDITIONAL_SORT_ORDER = ""
        GS0007FIXVALUElst.GS0007FIXVALUElst()
        If Not isNormal(GS0007FIXVALUElst.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "固定値取得エラー")
            Exit Sub
        End If

        'ログインユーザーと指定された荷主より操作可能なアボカド接続情報（営業所毎）取得
        Dim ApiInfo = work.GetAvocadoInfo(Master.USERCAMP, Master.ROLE_ORG, "")
        WF_TORI.Items.Clear()
        WF_TORI.Items.Add(New ListItem("選択してください", ""))
        For i As Integer = 0 To toriList.Items.Count - 1
            'ApiInfo(リスト）中に指定された取引先が存在した場合、ドロップダウンリストを作成する
            Dim toriLike As String = "*" & toriList.Items(i).Value & "*"
            Dim exists As Boolean = ApiInfo.Any(Function(p) p.Tori Like toriLike)
            If exists Then
                WF_TORI.Items.Add(New ListItem(toriList.Items(i).Text, toriList.Items(i).Value))
            End If
        Next

        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.SUBMENU Then
            ' メニューからの画面遷移
            ' 画面間の情報クリア
            work.Initialize()

            ' 初期変数設定処理
            WF_TaishoYm.Value = Date.Now.ToString("yyyy/MM")
            work.WF_SEL_TORIINDEX.Text = WF_TORI.SelectedIndex
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0001D OrElse
               Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0001I OrElse
               Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNT0001Z Then
            ' 実行画面からの遷移
            WF_TaishoYm.Value = work.WF_SEL_TAISHOYM.Text
            Try
                WF_TORI.SelectedIndex = work.WF_SEL_TORIINDEX.Text
            Catch ex As Exception
            End Try
        Else
            ' 再度メニューからの遷移
            WF_TaishoYm.Value = Date.Now.ToString("yyyy/MM")
        End If

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
        Master.SaveTable(LNT0001tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNT0001tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNT0001tbl)

        TBLview.RowFilter = "LINECNT >= 1 and LINECNT <= " & CONST_DISPROWCOUNT

        If String.IsNullOrEmpty(Master.VIEWID) Then
            Master.VIEWID = rightviewD.GetViewId(Master.USERCAMP)
        End If
        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()
        If Not isNormal(CS0013ProfView.ERR) Then
            Master.Output(CS0013ProfView.ERR, C_MESSAGE_TYPE.ABORT, "一覧設定エラー")
            Exit Sub
        End If

        '○ 先頭行に合わせる
        WF_GridPosition.Text = "1"

        work.WF_SEL_TAISHOYM.Text = WF_TaishoYm.Value
        work.WF_SEL_TORICODE.Text = WF_TORI.SelectedValue

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

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データを荷主マスタから取得する
        Dim SQLStr As String =
              " Select                                                                              " _
            & "      1                                                    AS 'SELECT'               " _
            & "     ,0                                                    AS HIDDEN                 " _
            & "     ,0                                                    AS LINECNT                " _
            & "     ,''                                                   AS OPERATION              " _
            & "     ,coalesce(LT1.RECONO, '')                             AS RECONO			        " _
            & "     ,coalesce(LT1.LOADUNLOTYPE, '')                       AS LOADUNLOTYPE		    " _
            & "     ,coalesce(LT1.STACKINGTYPE, '')                       AS STACKINGTYPE		    " _
            & "     ,coalesce(LT1.HSETID, '')                             AS HSETID			        " _
            & "     ,coalesce(LT1.ORDERORGSELECT, '')                     AS ORDERORGSELECT	        " _
            & "     ,coalesce(LT1.ORDERORGNAME, '')                       AS ORDERORGNAME		    " _
            & "     ,coalesce(LT1.ORDERORGCODE, '')                       AS ORDERORGCODE		    " _
            & "     ,coalesce(LT1.ORDERORGNAMES, '')                      AS ORDERORGNAMES	        " _
            & "     ,coalesce(LT1.KASANAMEORDERORG, '')                   AS KASANAMEORDERORG	    " _
            & "     ,coalesce(LT1.KASANCODEORDERORG, '')                  AS KASANCODEORDERORG	    " _
            & "     ,coalesce(LT1.KASANAMESORDERORG, '')                  AS KASANAMESORDERORG	    " _
            & "     ,coalesce(LT1.ORDERORG, '')                           AS ORDERORG				" _
            & "     ,coalesce(LT1.KASANORDERORG, '')                      AS KASANORDERORG		    " _
            & "     ,coalesce(LT1.PRODUCTSLCT, '')                        AS PRODUCTSLCT			" _
            & "     ,coalesce(LT1.PRODUCTSYOSAI, '')                      AS PRODUCTSYOSAI		    " _
            & "     ,coalesce(LT1.PRODUCT2NAME, '')                       AS PRODUCT2NAME			" _
            & "     ,coalesce(LT1.PRODUCT2, '')                           AS PRODUCT2				" _
            & "     ,coalesce(LT1.PRODUCT1NAME, '')                       AS PRODUCT1NAME			" _
            & "     ,coalesce(LT1.PRODUCT1, '')                           AS PRODUCT1				" _
            & "     ,coalesce(LT1.OILNAME, '')                            AS OILNAME				" _
            & "     ,coalesce(LT1.OILTYPE, '')                            AS OILTYPE				" _
            & "     ,coalesce(LT1.TODOKESLCT, '')                         AS TODOKESLCT			    " _
            & "     ,coalesce(LT1.TODOKECODE, '')                         AS TODOKECODE			    " _
            & "     ,coalesce(LT1.TODOKENAME, '')                         AS TODOKENAME			    " _
            & "     ,coalesce(LT1.TODOKENAMES, '')                        AS TODOKENAMES			" _
            & "     ,coalesce(LT1.TORICODE, '')                           AS TORICODE				" _
            & "     ,coalesce(LT1.TORINAME, '')                           AS TORINAME				" _
            & "     ,coalesce(LT1.TODOKEADDR, '')                         AS TODOKEADDR			    " _
            & "     ,coalesce(LT1.TODOKETEL, '')                          AS TODOKETEL			    " _
            & "     ,coalesce(LT1.TODOKEMAP, '')                          AS TODOKEMAP			    " _
            & "     ,coalesce(LT1.TODOKEIDO, '')                          AS TODOKEIDO			    " _
            & "     ,coalesce(LT1.TODOKEKEIDO, '')                        AS TODOKEKEIDO			" _
            & "     ,coalesce(LT1.TODOKEBIKO1, '')                        AS TODOKEBIKO1			" _
            & "     ,coalesce(LT1.TODOKEBIKO2, '')                        AS TODOKEBIKO2			" _
            & "     ,coalesce(LT1.TODOKEBIKO3, '')                        AS TODOKEBIKO3			" _
            & "     ,coalesce(LT1.TODOKECOLOR1, '')                       AS TODOKECOLOR1			" _
            & "     ,coalesce(LT1.TODOKECOLOR2, '')                       AS TODOKECOLOR2			" _
            & "     ,coalesce(LT1.TODOKECOLOR3, '')                       AS TODOKECOLOR3			" _
            & "     ,coalesce(LT1.SHUKASLCT, '')                          AS SHUKASLCT			    " _
            & "     ,coalesce(LT1.SHUKABASHO, '')                         AS SHUKABASHO			    " _
            & "     ,coalesce(LT1.SHUKANAME, '')                          AS SHUKANAME			    " _
            & "     ,coalesce(LT1.SHUKANAMES, '')                         AS SHUKANAMES			    " _
            & "     ,coalesce(LT1.SHUKATORICODE, '')                      AS SHUKATORICODE		    " _
            & "     ,coalesce(LT1.SHUKATORINAME, '')                      AS SHUKATORINAME		    " _
            & "     ,coalesce(LT1.SHUKAADDR, '')                          AS SHUKAADDR			    " _
            & "     ,coalesce(LT1.SHUKAADDRTEL, '')                       AS SHUKAADDRTEL			" _
            & "     ,coalesce(LT1.SHUKAMAP, '')                           AS SHUKAMAP				" _
            & "     ,coalesce(LT1.SHUKAIDO, '')                           AS SHUKAIDO				" _
            & "     ,coalesce(LT1.SHUKAKEIDO, '')                         AS SHUKAKEIDO			    " _
            & "     ,coalesce(LT1.SHUKABIKOU1, '')                        AS SHUKABIKOU1			" _
            & "     ,coalesce(LT1.SHUKABIKOU2, '')                        AS SHUKABIKOU2			" _
            & "     ,coalesce(LT1.SHUKABIKOU3, '')                        AS SHUKABIKOU3			" _
            & "     ,coalesce(LT1.SHUKACOLOR1, '')                        AS SHUKACOLOR1			" _
            & "     ,coalesce(LT1.SHUKACOLOR2, '')                        AS SHUKACOLOR2			" _
            & "     ,coalesce(LT1.SHUKACOLOR3, '')                        AS SHUKACOLOR3			" _
            & "     ,coalesce(LT1.SHUKADATE, '')                          AS SHUKADATE			    " _
            & "     ,coalesce(LT1.LOADTIME, '')                           AS LOADTIME				" _
            & "     ,coalesce(LT1.LOADTIMEIN, '')                         AS LOADTIMEIN			    " _
            & "     ,coalesce(LT1.LOADTIMES, '')                          AS LOADTIMES			    " _
            & "     ,coalesce(LT1.TODOKEDATE, '')                         AS TODOKEDATE			    " _
            & "     ,coalesce(LT1.SHITEITIME, '')                         AS SHITEITIME			    " _
            & "     ,coalesce(LT1.SHITEITIMEIN, '')                       AS SHITEITIMEIN			" _
            & "     ,coalesce(LT1.SHITEITIMES, '')                        AS SHITEITIMES			" _
            & "     ,coalesce(LT1.ZYUTYU, '')                             AS ZYUTYU				    " _
            & "     ,coalesce(LT1.ZISSEKI, '')                            AS ZISSEKI				" _
            & "     ,coalesce(LT1.TANNI, '')                              AS TANNI				    " _
            & "     ,coalesce(LT1.GYOUMUSIZI1, '')                        AS GYOUMUSIZI1			" _
            & "     ,coalesce(LT1.GYOUMUSIZI2, '')                        AS GYOUMUSIZI2			" _
            & "     ,coalesce(LT1.GYOUMUSIZI3, '')                        AS GYOUMUSIZI3			" _
            & "     ,coalesce(LT1.NINUSHIBIKOU, '')                       AS NINUSHIBIKOU			" _
            & "     ,coalesce(LT1.GYOMUSYABAN, '')                        AS GYOMUSYABAN			" _
            & "     ,coalesce(LT1.SHIPORGNAME, '')                        AS SHIPORGNAME			" _
            & "     ,coalesce(LT1.SHIPORG, '')                            AS SHIPORG				" _
            & "     ,coalesce(LT1.SHIPORGNAMES, '')                       AS SHIPORGNAMES			" _
            & "     ,coalesce(LT1.KASANSHIPORGNAME, '')                   AS KASANSHIPORGNAME	    " _
            & "     ,coalesce(LT1.KASANSHIPORG, '')                       AS KASANSHIPORG			" _
            & "     ,coalesce(LT1.KASANSHIPORGNAMES, '')                  AS KASANSHIPORGNAMES	    " _
            & "     ,coalesce(LT1.TANKNUM, '')                            AS TANKNUM				" _
            & "     ,coalesce(LT1.TANKNUMBER, '')                         AS TANKNUMBER			    " _
            & "     ,coalesce(LT1.SYAGATA, '')                            AS SYAGATA				" _
            & "     ,coalesce(LT1.SYABARA, '')                            AS SYABARA				" _
            & "     ,coalesce(LT1.NINUSHINAME, '')                        AS NINUSHINAME			" _
            & "     ,coalesce(LT1.CONTYPE, '')                            AS CONTYPE				" _
            & "     ,coalesce(LT1.PRO1SYARYOU, '')                        AS PRO1SYARYOU			" _
            & "     ,coalesce(LT1.TANKMEMO, '')                           AS TANKMEMO				" _
            & "     ,coalesce(LT1.TANKBIKOU1, '')                         AS TANKBIKOU1			    " _
            & "     ,coalesce(LT1.TANKBIKOU2, '')                         AS TANKBIKOU2			    " _
            & "     ,coalesce(LT1.TANKBIKOU3, '')                         AS TANKBIKOU3			    " _
            & "     ,coalesce(LT1.TRACTORNUM, '')                         AS TRACTORNUM			    " _
            & "     ,coalesce(LT1.TRACTORNUMBER, '')                      AS TRACTORNUMBER		    " _
            & "     ,coalesce(LT1.TRIP, '')                               AS TRIP					" _
            & "     ,coalesce(LT1.DRP, '')                                AS DRP					" _
            & "     ,coalesce(LT1.UNKOUMEMO, '')                          AS UNKOUMEMO			    " _
            & "     ,coalesce(LT1.SHUKKINTIME, '')                        AS SHUKKINTIME			" _
            & "     ,coalesce(LT1.STAFFSLCT, '')                          AS STAFFSLCT			    " _
            & "     ,coalesce(LT1.STAFFNAME, '')                          AS STAFFNAME			    " _
            & "     ,coalesce(LT1.STAFFCODE, '')                          AS STAFFCODE			    " _
            & "     ,coalesce(LT1.SUBSTAFFSLCT, '')                       AS SUBSTAFFSLCT			" _
            & "     ,coalesce(LT1.SUBSTAFFNAME, '')                       AS SUBSTAFFNAME			" _
            & "     ,coalesce(LT1.SUBSTAFFNUM, '')                        AS SUBSTAFFNUM			" _
            & "     ,coalesce(LT1.CALENDERMEMO1, '')                      AS CALENDERMEMO1		    " _
            & "     ,coalesce(LT1.CALENDERMEMO2, '')                      AS CALENDERMEMO2		    " _
            & "     ,coalesce(LT1.CALENDERMEMO3, '')                      AS CALENDERMEMO3		    " _
            & "     ,coalesce(LT1.CALENDERMEMO4, '')                      AS CALENDERMEMO4		    " _
            & "     ,coalesce(LT1.CALENDERMEMO5, '')                      AS CALENDERMEMO5		    " _
            & "     ,coalesce(LT1.CALENDERMEMO6, '')                      AS CALENDERMEMO6		    " _
            & "     ,coalesce(LT1.CALENDERMEMO7, '')                      AS CALENDERMEMO7		    " _
            & "     ,coalesce(LT1.CALENDERMEMO8, '')                      AS CALENDERMEMO8		    " _
            & "     ,coalesce(LT1.CALENDERMEMO9, '')                      AS CALENDERMEMO9		    " _
            & "     ,coalesce(LT1.CALENDERMEMO10, '')                     AS CALENDERMEMO10		    " _
            & "     ,coalesce(LT1.GYOMUTANKNUM, '')                       AS GYOMUTANKNUM			" _
            & "     ,coalesce(LT1.YOUSYA, '')                             AS YOUSYA				    " _
            & "     ,coalesce(LT1.RECOTITLE, '')                          AS RECOTITLE			    " _
            & "     ,coalesce(LT1.SHUKODATE, '')                          AS SHUKODATE			    " _
            & "     ,coalesce(LT1.KIKODATE, '')                           AS KIKODATE				" _
            & "     ,coalesce(LT1.KIKOTIME, '')                           AS KIKOTIME				" _
            & "     ,coalesce(LT1.CREWBIKOU1, '')                         AS CREWBIKOU1			    " _
            & "     ,coalesce(LT1.CREWBIKOU2, '')                         AS CREWBIKOU2			    " _
            & "     ,coalesce(LT1.SUBCREWBIKOU1, '')                      AS SUBCREWBIKOU1		    " _
            & "     ,coalesce(LT1.SUBCREWBIKOU2, '')                      AS SUBCREWBIKOU2		    " _
            & "     ,coalesce(LT1.SUBSHUKKINTIME, '')                     AS SUBSHUKKINTIME		    " _
            & "     ,coalesce(LT1.CALENDERMEMO11, '')                     AS CALENDERMEMO11		    " _
            & "     ,coalesce(LT1.CALENDERMEMO12, '')                     AS CALENDERMEMO12		    " _
            & "     ,coalesce(LT1.CALENDERMEMO13, '')                     AS CALENDERMEMO13		    " _
            & "     ,coalesce(LT1.SYABARATANNI, '')                       AS SYABARATANNI			" _
            & "     ,coalesce(LT1.TAIKINTIME, '')                         AS TAIKINTIME			    " _
            & "     ,coalesce(LT1.SUBTIKINTIME, '')                       AS SUBTIKINTIME			" _
            & "     ,coalesce(LT1.KVTITLE, '')                            AS KVTITLE				" _
            & "     ,coalesce(LT1.KVZYUTYU, '')                           AS KVZYUTYU				" _
            & "     ,coalesce(LT1.KVZISSEKI, '')                          AS KVZISSEKI			    " _
            & "     ,coalesce(LT1.KVCREW, '')                             AS KVCREW				    " _
            & "     ,coalesce(LT1.CREWCODE, '')                           AS CREWCODE				" _
            & "     ,coalesce(LT1.SUBCREWCODE, '')                        AS SUBCREWCODE			" _
            & "     ,coalesce(LT1.KVSUBCREW, '')                          AS KVSUBCREW			    " _
            & "     ,coalesce(LT1.ORDERHENKO, '')                         AS ORDERHENKO			    " _
            & "     ,coalesce(LT1.RIKUUNKYOKU, '')                        AS RIKUUNKYOKU			" _
            & "     ,coalesce(LT1.BUNRUINUMBER, '')                       AS BUNRUINUMBER			" _
            & "     ,coalesce(LT1.HIRAGANA, '')                           AS HIRAGANA				" _
            & "     ,coalesce(LT1.ITIRENNUM, '')                          AS ITIRENNUM			    " _
            & "     ,coalesce(LT1.TRACTER1, '')                           AS TRACTER1				" _
            & "     ,coalesce(LT1.TRACTER2, '')                           AS TRACTER2				" _
            & "     ,coalesce(LT1.TRACTER3, '')                           AS TRACTER3				" _
            & "     ,coalesce(LT1.TRACTER4, '')                           AS TRACTER4				" _
            & "     ,coalesce(LT1.TRACTER5, '')                           AS TRACTER5				" _
            & "     ,coalesce(LT1.TRACTER6, '')                           AS TRACTER6				" _
            & "     ,coalesce(LT1.TRACTER7, '')                           AS TRACTER7				" _
            & "     ,coalesce(LT1.HAISYAHUKA, '')                         AS HAISYAHUKA			    " _
            & "     ,coalesce(LT1.HYOZIZYUNT, '')                         AS HYOZIZYUNT			    " _
            & "     ,coalesce(LT1.HYOZIZYUNH, '')                         AS HYOZIZYUNH			    " _
            & "     ,coalesce(LT1.HONTRACTER1, '')                        AS HONTRACTER1			" _
            & "     ,coalesce(LT1.HONTRACTER2, '')                        AS HONTRACTER2			" _
            & "     ,coalesce(LT1.HONTRACTER3, '')                        AS HONTRACTER3			" _
            & "     ,coalesce(LT1.HONTRACTER4, '')                        AS HONTRACTER4			" _
            & "     ,coalesce(LT1.HONTRACTER5, '')                        AS HONTRACTER5			" _
            & "     ,coalesce(LT1.HONTRACTER6, '')                        AS HONTRACTER6			" _
            & "     ,coalesce(LT1.HONTRACTER7, '')                        AS HONTRACTER7			" _
            & "     ,coalesce(LT1.HONTRACTER8, '')                        AS HONTRACTER8			" _
            & "     ,coalesce(LT1.HONTRACTER9, '')                        AS HONTRACTER9			" _
            & "     ,coalesce(LT1.HONTRACTER10, '')                       AS HONTRACTER10			" _
            & "     ,coalesce(LT1.HONTRACTER11, '')                       AS HONTRACTER11			" _
            & "     ,coalesce(LT1.HONTRACTER12, '')                       AS HONTRACTER12			" _
            & "     ,coalesce(LT1.HONTRACTER13, '')                       AS HONTRACTER13			" _
            & "     ,coalesce(LT1.HONTRACTER14, '')                       AS HONTRACTER14			" _
            & "     ,coalesce(LT1.HONTRACTER15, '')                       AS HONTRACTER15			" _
            & "     ,coalesce(LT1.HONTRACTER16, '')                       AS HONTRACTER16			" _
            & "     ,coalesce(LT1.HONTRACTER17, '')                       AS HONTRACTER17			" _
            & "     ,coalesce(LT1.HONTRACTER18, '')                       AS HONTRACTER18			" _
            & "     ,coalesce(LT1.HONTRACTER19, '')                       AS HONTRACTER19			" _
            & "     ,coalesce(LT1.HONTRACTER20, '')                       AS HONTRACTER20			" _
            & "     ,coalesce(LT1.HONTRACTER21, '')                       AS HONTRACTER21			" _
            & "     ,coalesce(LT1.HONTRACTER22, '')                       AS HONTRACTER22			" _
            & "     ,coalesce(LT1.HONTRACTER23, '')                       AS HONTRACTER23			" _
            & "     ,coalesce(LT1.HONTRACTER24, '')                       AS HONTRACTER24			" _
            & "     ,coalesce(LT1.HONTRACTER25, '')                       AS HONTRACTER25			" _
            & "     ,coalesce(LT1.CALENDERMEMO14, '')                     AS CALENDERMEMO14		    " _
            & "     ,coalesce(LT1.CALENDERMEMO15, '')                     AS CALENDERMEMO15		    " _
            & "     ,coalesce(LT1.CALENDERMEMO16, '')                     AS CALENDERMEMO16		    " _
            & "     ,coalesce(LT1.CALENDERMEMO17, '')                     AS CALENDERMEMO17		    " _
            & "     ,coalesce(LT1.CALENDERMEMO18, '')                     AS CALENDERMEMO18		    " _
            & "     ,coalesce(LT1.CALENDERMEMO19, '')                     AS CALENDERMEMO19		    " _
            & "     ,coalesce(LT1.CALENDERMEMO20, '')                     AS CALENDERMEMO20		    " _
            & "     ,coalesce(LT1.CALENDERMEMO21 , '')                    AS CALENDERMEMO21		    " _
            & "     ,coalesce(LT1.CALENDERMEMO22, '')                     AS CALENDERMEMO22		    " _
            & "     ,coalesce(LT1.CALENDERMEMO23, '')                     AS CALENDERMEMO23		    " _
            & "     ,coalesce(LT1.CALENDERMEMO24, '')                     AS CALENDERMEMO24		    " _
            & "     ,coalesce(LT1.CALENDERMEMO25, '')                     AS CALENDERMEMO25		    " _
            & "     ,coalesce(LT1.CALENDERMEMO26, '')                     AS CALENDERMEMO26		    " _
            & "     ,coalesce(LT1.CALENDERMEMO27, '')                     AS CALENDERMEMO27		    " _
            & "     ,coalesce(LT1.UPDATEUSER, '')                         AS UPDATEUSER			    " _
            & "     ,coalesce(LT1.CREATEUSER, '')                         AS CREATEUSER			    " _
            & "     ,coalesce(LT1.UPDATEYMD, '')                          AS UPDATEYMD			    " _
            & "     ,coalesce(LT1.CREATEYMD, '')                          AS CREATEYMD			    " _
            & "     ,coalesce(LT1.DELFLG, '')                             AS DELFLG				    " _
            & "     ,coalesce(LT1.INITYMD, '')                            AS INITYMD				" _
            & "     ,coalesce(LT1.INITUSER, '')                           AS INITUSER				" _
            & "     ,coalesce(LT1.INITTERMID, '')                         AS INITTERMID			    " _
            & "     ,coalesce(LT1.INITPGID, '')                           AS INITPGID				" _
            & "     ,coalesce(LT1.UPDYMD, '')                             AS UPDYMD				    " _
            & "     ,coalesce(LT1.UPDUSER, '')                            AS UPDUSER				" _
            & "     ,coalesce(LT1.UPDTERMID, '')                          AS UPDTERMID			    " _
            & "     ,coalesce(LT1.UPDPGID, '')                            AS UPDPGID				" _
            & "     ,coalesce(LT1.RECEIVEYMD, '')                         AS RECEIVEYMD			    " _
            & "     ,coalesce(LT1.UPDTIMSTP, '')                          AS UPDTIMSTP			    " _
            & " FROM                                                                                " _
            & "     LNG.LNT0001_ZISSEKI LT1                                                         " _
            & " WHERE                                                                               " _
            & "     date_format(LT1.TODOKEDATE, '%Y/%m/%d') >= @P1                                  " _
            & " AND date_format(LT1.TODOKEDATE, '%Y/%m/%d') <= @P2                                  " _

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        ' 取引先
        If WF_TORI.SelectedIndex = 0 Then
            If WF_TORI.Items.Count - 1 > 0 Then
                SQLStr += " AND LT1.TORICODE in ("
                'WF_TORIの先頭に"選択してください"があるためj=1とする
                For j As Integer = 1 To WF_TORI.Items.Count - 1
                    SQLStr += "'"
                    SQLStr += WF_TORI.Items(j).Value
                    SQLStr += "'"
                    If j < WF_TORI.Items.Count - 1 Then
                        SQLStr += ","
                    Else
                        SQLStr += ")"
                    End If
                Next
            End If
        Else
            SQLStr += " AND LT1.TORICODE = '" & WF_TORI.SelectedValue & "'"
        End If

        '部署
        Dim ApiInfo = work.GetAvocadoInfo(Master.USERCAMP, Master.ROLE_ORG, WF_TORI.SelectedValue)
        If ApiInfo.Count > 0 Then
            SQLStr += " AND LT1.ORDERORG in ("
            For j As Integer = 0 To ApiInfo.Count - 1
                SQLStr += "'"
                SQLStr += ApiInfo(j).Org
                SQLStr += "'"
                If j < ApiInfo.Count - 1 Then
                    SQLStr += ","
                Else
                    SQLStr += ")"
                End If
            Next
        End If

        SQLStr += " AND LT1.DELFLG = '0'                                                " _
                & " ORDER BY                                                            " _
                & "     LT1.ORDERORGCODE, LT1.TORICODE, LT1.SHUKADATE, LT1.TODOKEDATE, LT1.STAFFCODE  "


        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.Date)  '届日FROM
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.Date)  '届日TO
                If Not String.IsNullOrEmpty(WF_TaishoYm.Value) AndAlso IsDate(WF_TaishoYm.Value & "/01") Then
                    PARA1.Value = WF_TaishoYm.Value & "/01"
                    PARA2.Value = WF_TaishoYm.Value & DateTime.DaysInMonth(CDate(WF_TaishoYm.Value).Year, CDate(WF_TaishoYm.Value).Month).ToString("/00")
                Else
                    PARA1.Value = Date.Now.ToString("yyyy/MM") & "/01"
                    PARA2.Value = Date.Now.ToString("yyyy/MM") & DateTime.DaysInMonth(Date.Now.Year, Date.Now.Month).ToString("/00")
                End If

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNT0001tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNT0001tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNT0001row As DataRow In LNT0001tbl.Rows
                    i += 1
                    LNT0001row("LINECNT") = i        'LINECNT
                Next
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
    ''' 一覧再表示処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DisplayGrid()

        Dim WW_GridPosition As Integer          '表示位置(開始)
        Dim WW_DataCNT As Integer = 0           '(絞り込み後)有効Data数

        '○ 表示対象行カウント(絞り込み対象)
        For Each LNM0023row As DataRow In LNT0001tbl.Rows
            If LNM0023row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0023row("SELECT") = WW_DataCNT
            End If
        Next

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

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNT0001tbl.Rows.Count.ToString()

        '○ 画面(GridView)表示
        Dim TBLview As DataView = New DataView(LNT0001tbl)

        '○ ソート
        TBLview.Sort = "LINECNT"
        TBLview.RowFilter = "HIDDEN = 0 and SELECT >= " & WW_GridPosition.ToString() & " and SELECT < " & (WW_GridPosition + CONST_DISPROWCOUNT).ToString()

        '○ 一覧作成
        If String.IsNullOrEmpty(Master.VIEWID) Then
            Master.VIEWID = rightviewD.GetViewId(Master.USERCAMP)
        End If
        CS0013ProfView.CAMPCODE = Master.USERCAMP
        CS0013ProfView.PROFID = Master.PROF_VIEW
        CS0013ProfView.MAPID = Master.MAPID
        CS0013ProfView.VARI = Master.VIEWID
        CS0013ProfView.SRCDATA = TBLview.ToTable
        CS0013ProfView.TBLOBJ = pnlListArea
        'CS0013ProfView.HIDENOOPT = True
        CS0013ProfView.HIDEOPERATIONOPT = True
        CS0013ProfView.SCROLLTYPE = CS0013ProfView.SCROLLTYPE_ENUM.Both
        CS0013ProfView.LEVENT = "ondblclick"
        'CS0013ProfView.LFUNC = "ListDbClick"
        CS0013ProfView.TITLEOPT = True
        CS0013ProfView.CS0013ProfView()

        '○ クリア
        If TBLview.Count = 0 Then
            WF_GridPosition.Text = "1"
        Else
            WF_GridPosition.Text = TBLview.Item(0)("SELECT")
        End If

        work.WF_SEL_YM.Text = WF_TaishoYm.Value
        work.WF_SEL_TORICODE.Text = WF_TORI.SelectedValue

        TBLview.Dispose()
        TBLview = Nothing

    End Sub

    ''' <summary>
    ''' リスト変更処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ListChange(ByVal chkFieldName As String)

        Select Case chkFieldName
            '〇荷主リスト変更
            Case "WF_TORI"
                work.WF_SEL_TORIINDEX.Text = Me.WF_TORI.SelectedIndex
        End Select

    End Sub
    ''' <summary>
    ''' 絞り込みボタン押下
    ''' </summary>
    Private Sub WF_ButtonExtract_Click()
        '○ 画面表示データ取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続

            MAPDataGet(SQLcon)
        End Using

        '○ 画面表示データ保存
        Master.SaveTable(LNT0001tbl)

        WF_GridPosition.Text = "1"
    End Sub

    ''' <summary>
    ''' 実績取込画面遷移
    ''' </summary>
    Private Sub WF_ButtonZisseki_Click()
        work.WF_SEL_TAISHOYM.Text = WF_TaishoYm.Value

        '○ 画面レイアウト設定
        If String.IsNullOrEmpty(Master.VIEWID) Then
            Master.VIEWID = rightviewD.GetViewId(Master.USERCAMP)
        End If

        Master.CheckParmissionCode(Master.USERCAMP)
        If Not Master.MAPpermitcode = C_PERMISSION.INVALID Then
            ' 画面遷移
            Master.TransitionPage()
        End If
    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FiledDBClick()

        Dim WW_PrmData As New Hashtable

        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.TryParse(WF_LeftMViewChange.Value, WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try

            With leftview
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        ' 日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "WF_TaishoYm"         '作成日時
                                .WF_Calendar.Text = WF_TaishoYm.Value
                        End Select
                        .ActiveCalendar()
                End Select
            End With
        End If

    End Sub

    ' ******************************************************************************
    ' ***  LeftBox関連操作                                                       ***
    ' ******************************************************************************

    ''' <summary>
    ''' LeftBox選択時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonSel_Click()

        Dim WW_SelectValue As String = ""
        Dim WW_SelectText As String = ""
        Dim WW_SelectDate As Date

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SelectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SelectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "WF_TaishoYm"             '作成日時
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_SelectDate)
                    If WW_SelectDate < C_DEFAULT_YMD Then
                        WF_TaishoYm.Value = ""
                    Else
                        WF_TaishoYm.Value = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM")
                    End If
                Catch ex As Exception
                End Try
                WF_TaishoYm.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()

        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_TaishoYm"             '作成日時
                WF_TaishoYm.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_LeftMViewChange.Value = ""

    End Sub
    ''' <summary>
    ''' 請求書l出力ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonInvoice_Click()

        work.WF_SEL_TAISHOYM.Text = WF_TaishoYm.Value

        Dim WW_URL As String = ""
        work.GetURL(LNT0001WRKINC.MAPIDI, WW_URL)
        Server.Transfer(WW_URL)

    End Sub

    ''' <summary>
    ''' ﾀﾞｳﾝﾛｰﾄﾞ(PDF出力)・一覧印刷ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonPRINT_Click()

        '○ 帳票出力
        CS0030REPORT.CAMPCODE = Master.USERCAMP                 '会社コード
        CS0030REPORT.PROFID = Master.PROF_REPORT                'プロファイルID
        CS0030REPORT.MAPID = Master.MAPID                       '画面ID
        CS0030REPORT.REPORTID = rightviewR.GetReportId()        '帳票ID
        CS0030REPORT.FILEtyp = "pdf"                            '出力ファイル形式
        CS0030REPORT.TBLDATA = LNT0001tbl                       'データ参照Table
        CS0030REPORT.CS0030REPORT()
        If Not isNormal(CS0030REPORT.ERR) Then
            If CS0030REPORT.ERR = C_MESSAGE_NO.REPORT_EXCEL_NOT_FOUND_ERROR Then
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
            Else
                Master.Output(CS0030REPORT.ERR, C_MESSAGE_TYPE.ABORT, "CS0030REPORT")
            End If
            Exit Sub
        End If

        '○ 別画面でPDFを表示
        WF_PrintURL.Value = CS0030REPORT.URL
        ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_PDFPrint();", True)

    End Sub

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonEND_Click()

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
        Dim TBLview As New DataView(LNT0001tbl)
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

    ' ******************************************************************************
    ' ***  共通処理                                                              ***
    ' ******************************************************************************

    ''' <summary>
    ''' 遷移先(登録画面)退避データ保存先の作成
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub WW_CreateXMLSaveFile()
        work.WF_SEL_INPTBL.Text = CS0050SESSION.UPLOAD_PATH & "\XML_TMP\" & Date.Now.ToString("yyyyMMdd") & "-" &
            Master.USERID & "-" & Master.MAPID & "-" & CS0050SESSION.VIEW_MAP_VARIANT & "-" & Date.Now.ToString("HHmmss") & "INPTBL.txt"

    End Sub

End Class