''************************************************************
' コンテナマスタメンテ変更履歴画面
' 作成日 2024/01/10
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2024/01/10 新規作成
'          : 
''************************************************************
Imports MySQL.Data.MySqlClient
Imports System.Drawing
Imports GrapeCity.Documents.Excel

''' <summary>
''' コンテナマスタ変更履歴
''' </summary>
''' <remarks></remarks>
Public Class LNM0002ReconmHistory
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0002tbl As DataTable                                  '一覧格納用テーブル

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
                    Master.RecoverTable(LNM0002tbl)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonDOWNLOAD"        'ダウンロードボタン押下
                            WF_EXCELPDF(LNM0002WRKINC.FILETYPE.EXCEL)
                        Case "WF_ButtonPRINT"           '一覧印刷ボタン押下
                            WF_EXCELPDF(LNM0002WRKINC.FILETYPE.PDF)
                        Case "WF_ButtonEND"             '戻るボタン押下
                            WF_ButtonEND_Click()
                        Case "WF_ButtonFIRST"           '先頭頁ボタン押下
                            WF_ButtonFIRST_Click()
                        Case "WF_ButtonLAST"            '最終頁ボタン押下
                            WF_ButtonLAST_Click()
                        Case "WF_SelectMODIFYYMChange"  '変更年月フィールドチェンジ
                            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                                SQLcon.Open()       'DataBase接続
                                '変更日取得
                                MODIFYDDGet(SQLcon)
                                '変更ユーザ取得
                                MODIFYUSERGet(SQLcon)
                            End Using
                        Case "WF_SelectMODIFYDDChange"  '変更日フィールドチェンジ
                            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                                SQLcon.Open()       'DataBase接続
                                '変更ユーザ取得
                                MODIFYUSERGet(SQLcon)
                            End Using
                        Case "WF_ButtonMODIFYVIEW"  '表示するボタン押下
                            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                                SQLcon.Open()  ' DataBase接続
                                MAPDataGet(SQLcon)
                                Master.SaveTable(LNM0002tbl)
                                '〇 一覧の件数を取得
                                Me.ListCount.Text = "件数：" + LNM0002tbl.Rows.Count.ToString()
                            End Using
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
            If Not IsNothing(LNM0002tbl) Then
                LNM0002tbl.Clear()
                LNM0002tbl.Dispose()
                LNM0002tbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0002WRKINC.MAPIDH
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
        Master.SaveTable(LNM0002tbl)

        '〇 一覧の件数を取得
        Me.ListCount.Text = "件数：" + LNM0002tbl.Rows.Count.ToString()

        '○ 一覧表示データ編集(性能対策)
        Dim TBLview As DataView = New DataView(LNM0002tbl)

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

        If IsNothing(LNM0002tbl) Then
            LNM0002tbl = New DataTable
        End If

        If LNM0002tbl.Columns.Count <> 0 Then
            LNM0002tbl.Columns.Clear()
        End If

        LNM0002tbl.Clear()

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをコンテナマスタ変更履歴テーブルから取得する
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select                                                                                               ")
        SQLStr.AppendLine("     1                                                                    AS 'SELECT'                 ")
        SQLStr.AppendLine("   , 0                                                                    AS HIDDEN                   ")
        SQLStr.AppendLine("   , 0                                                                    AS LINECNT                  ")
        SQLStr.AppendLine("   , ''                                                                   AS OPERATION                ")
        SQLStr.AppendLine("   , coalesce(RTRIM(DELFLG), '')                              AS DELFLG               ")
        SQLStr.AppendLine("   , coalesce(RTRIM(CTNTYPE), '')                             AS CTNTYPE              ")
        SQLStr.AppendLine("   , coalesce(RTRIM(CTNNO), '')                               AS CTNNO                ")
        SQLStr.AppendLine("   , coalesce(RTRIM(JURISDICTIONCD), '')                      AS JURISDICTIONCD       ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ACCOUNTINGASSETSCD), '')                  AS ACCOUNTINGASSETSCD   ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ACCOUNTINGASSETSKBN), '')                 AS ACCOUNTINGASSETSKBN  ")
        SQLStr.AppendLine("   , coalesce(RTRIM(DUMMYKBN), '')                            AS DUMMYKBN             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(SPOTKBN), '')                             AS SPOTKBN              ")
        SQLStr.AppendLine("   , coalesce(FORMAT(SPOTSTYMD, 'yyyy/MM/dd'), '')            AS SPOTSTYMD            ")
        SQLStr.AppendLine("   , coalesce(FORMAT(SPOTENDYMD, 'yyyy/MM/dd'), '')           AS SPOTENDYMD           ")
        SQLStr.AppendLine("   , coalesce(RTRIM(BIGCTNCD), '')                            AS BIGCTNCD             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(MIDDLECTNCD), '')                         AS MIDDLECTNCD          ")
        SQLStr.AppendLine("   , coalesce(RTRIM(SMALLCTNCD), '')                          AS SMALLCTNCD           ")
        SQLStr.AppendLine("   , coalesce(RTRIM(CONSTRUCTIONYM), '')                      AS CONSTRUCTIONYM       ")
        SQLStr.AppendLine("   , coalesce(RTRIM(CTNMAKER), '')                            AS CTNMAKER             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(FROZENMAKER), '')                         AS FROZENMAKER          ")
        SQLStr.AppendLine("   , coalesce(RTRIM(GROSSWEIGHT), '')                         AS GROSSWEIGHT          ")
        SQLStr.AppendLine("   , coalesce(RTRIM(CARGOWEIGHT), '')                         AS CARGOWEIGHT          ")
        SQLStr.AppendLine("   , coalesce(RTRIM(MYWEIGHT), '')                            AS MYWEIGHT             ")
        SQLStr.AppendLine("   , RTRIM(CONVERT(NUMERIC ,coalesce(BOOKVALUE, 0)))          AS BOOKVALUE            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(OUTHEIGHT), '')                           AS OUTHEIGHT            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(OUTWIDTH), '')                            AS OUTWIDTH             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(OUTLENGTH), '')                           AS OUTLENGTH            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(INHEIGHT), '')                            AS INHEIGHT             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(INWIDTH), '')                             AS INWIDTH              ")
        SQLStr.AppendLine("   , coalesce(RTRIM(INLENGTH), '')                            AS INLENGTH             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(WIFEHEIGHT), '')                          AS WIFEHEIGHT           ")
        SQLStr.AppendLine("   , coalesce(RTRIM(WIFEWIDTH), '')                           AS WIFEWIDTH            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(SIDEHEIGHT), '')                          AS SIDEHEIGHT           ")
        SQLStr.AppendLine("   , coalesce(RTRIM(SIDEWIDTH), '')                           AS SIDEWIDTH            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(FLOORAREA), '')                           AS FLOORAREA            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(INVOLUMEMARKING), '')                     AS INVOLUMEMARKING      ")
        SQLStr.AppendLine("   , coalesce(RTRIM(INVOLUMEACTUA), '')                       AS INVOLUMEACTUA        ")
        SQLStr.AppendLine("   , coalesce(RTRIM(TRAINSCYCLEDAYS), '')                     AS TRAINSCYCLEDAYS      ")
        SQLStr.AppendLine("   , coalesce(FORMAT(TRAINSBEFORERUNYMD, 'yyyy/MM/dd'), '')   AS TRAINSBEFORERUNYMD   ")
        SQLStr.AppendLine("   , coalesce(FORMAT(TRAINSNEXTRUNYMD, 'yyyy/MM/dd'), '')     AS TRAINSNEXTRUNYMD     ")
        SQLStr.AppendLine("   , coalesce(RTRIM(REGINSCYCLEDAYS), '')                     AS REGINSCYCLEDAYS      ")
        SQLStr.AppendLine("   , coalesce(RTRIM(REGINSCYCLEHOURMETER), '')                AS REGINSCYCLEHOURMETER ")
        SQLStr.AppendLine("   , coalesce(FORMAT(REGINSBEFORERUNYMD, 'yyyy/MM/dd'), '')   AS REGINSBEFORERUNYMD   ")
        SQLStr.AppendLine("   , coalesce(FORMAT(REGINSNEXTRUNYMD, 'yyyy/MM/dd'), '')     AS REGINSNEXTRUNYMD     ")
        SQLStr.AppendLine("   , coalesce(FORMAT(REGINSHOURMETERYMD, 'yyyy/MM/dd'), '')   AS REGINSHOURMETERYMD   ")
        SQLStr.AppendLine("   , coalesce(RTRIM(REGINSHOURMETERTIME), '')                 AS REGINSHOURMETERTIME  ")
        SQLStr.AppendLine("   , coalesce(RTRIM(REGINSHOURMETERDSP), '')                  AS REGINSHOURMETERDSP   ")
        SQLStr.AppendLine("   , coalesce(FORMAT(OPERATIONSTYMD, 'yyyy/MM/dd'), '')       AS OPERATIONSTYMD       ")
        SQLStr.AppendLine("   , coalesce(FORMAT(OPERATIONENDYMD, 'yyyy/MM/dd'), '')      AS OPERATIONENDYMD      ")
        SQLStr.AppendLine("   , coalesce(FORMAT(RETIRMENTYMD, 'yyyy/MM/dd'), '')         AS RETIRMENTYMD         ")
        SQLStr.AppendLine("   , coalesce(RTRIM(COMPKANKBN), '')                          AS COMPKANKBN           ")
        SQLStr.AppendLine("   , coalesce(RTRIM(SUPPLYFLG), '')                           AS SUPPLYFLG            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM1), '')                            AS ADDITEM1             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM2), '')                            AS ADDITEM2             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM3), '')                            AS ADDITEM3             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM4), '')                            AS ADDITEM4             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM5), '')                            AS ADDITEM5             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM6), '')                            AS ADDITEM6             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM7), '')                            AS ADDITEM7             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM8), '')                            AS ADDITEM8             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM9), '')                            AS ADDITEM9             ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM10), '')                           AS ADDITEM10            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM11), '')                           AS ADDITEM11            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM12), '')                           AS ADDITEM12            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM13), '')                           AS ADDITEM13            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM14), '')                           AS ADDITEM14            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM15), '')                           AS ADDITEM15            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM16), '')                           AS ADDITEM16            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM17), '')                           AS ADDITEM17            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM18), '')                           AS ADDITEM18            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM19), '')                           AS ADDITEM19            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM20), '')                           AS ADDITEM20            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM21), '')                           AS ADDITEM21            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM22), '')                           AS ADDITEM22            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM23), '')                           AS ADDITEM23            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM24), '')                           AS ADDITEM24            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM25), '')                           AS ADDITEM25            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM26), '')                           AS ADDITEM26            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM27), '')                           AS ADDITEM27            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM28), '')                           AS ADDITEM28            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM29), '')                           AS ADDITEM29            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM30), '')                           AS ADDITEM30            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM31), '')                           AS ADDITEM31            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM32), '')                           AS ADDITEM32            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM33), '')                           AS ADDITEM33            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM34), '')                           AS ADDITEM34            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM35), '')                           AS ADDITEM35            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM36), '')                           AS ADDITEM36            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM37), '')                           AS ADDITEM37            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM38), '')                           AS ADDITEM38            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM39), '')                           AS ADDITEM39            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM40), '')                           AS ADDITEM40            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM41), '')                           AS ADDITEM41            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM42), '')                           AS ADDITEM42            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM43), '')                           AS ADDITEM43            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM44), '')                           AS ADDITEM44            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM45), '')                           AS ADDITEM45            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM46), '')                           AS ADDITEM46            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM47), '')                           AS ADDITEM47            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM48), '')                           AS ADDITEM48            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM49), '')                           AS ADDITEM49            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(ADDITEM50), '')                           AS ADDITEM50            ")
        SQLStr.AppendLine("   , coalesce(RTRIM(FLOORMATERIAL), '')                       AS FLOORMATERIAL        ")
        SQLStr.AppendLine("   , coalesce(RTRIM(OPERATEKBN), '')                                        AS OPERATEKBN               ")
        SQLStr.AppendLine("   , CASE                 ")
        SQLStr.AppendLine("      WHEN coalesce(RTRIM(OPERATEKBN), '') ='2' AND coalesce(RTRIM(MODIFYKBN), '') ='2' THEN '変更前 更新' ")
        SQLStr.AppendLine("      WHEN coalesce(RTRIM(OPERATEKBN), '') ='2' AND coalesce(RTRIM(MODIFYKBN), '') ='3' THEN '変更後 更新' ")
        SQLStr.AppendLine("      WHEN coalesce(RTRIM(OPERATEKBN), '') ='3' AND coalesce(RTRIM(MODIFYKBN), '') ='2' THEN '変更前 削除' ")
        SQLStr.AppendLine("      WHEN coalesce(RTRIM(OPERATEKBN), '') ='3' AND coalesce(RTRIM(MODIFYKBN), '') ='3' THEN '変更後 削除' ")
        SQLStr.AppendLine("      ELSE ''                                                                                          ")
        SQLStr.AppendLine("    END AS OPERATEKBNNAME                                                                              ")
        SQLStr.AppendLine("   , coalesce(RTRIM(MODIFYKBN), '')                                        AS MODIFYKBN                　")
        SQLStr.AppendLine("   ,CASE coalesce(RTRIM(MODIFYKBN), '')                                                                  ")
        SQLStr.AppendLine("      WHEN '1' THEN '新規'                                                                             ")
        SQLStr.AppendLine("      WHEN '2' THEN '変更前'                                                                           ")
        SQLStr.AppendLine("      WHEN '3' THEN '変更後'                                                                           ")
        SQLStr.AppendLine("      ELSE ''                                                                                          ")
        SQLStr.AppendLine("    END AS MODIFYKBNNAME                                                                               ")
        SQLStr.AppendLine("   , FORMAT(MODIFYYMD, 'yyyy/MM/dd HH:mm:ss')                              AS MODIFYYMD                ")
        SQLStr.AppendLine("   , coalesce(RTRIM(MODIFYUSER), '')                                         AS MODIFYUSER               ")
        SQLStr.AppendLine(" FROM                                                                                                  ")
        SQLStr.AppendLine("     LNG.LNT0115_RECONHIST                                                                            ")
        SQLStr.AppendLine(" WHERE                                                                                                 ")
        '変更日が指定されている場合
        If Not WF_DDL_MODIFYDD.SelectedValue = "" Then
            SQLStr.AppendLine("    FORMAT(MODIFYYMD, 'yyyy/MM/dd')  = @MODIFYYMD                                                  ")
        Else
            SQLStr.AppendLine("    FORMAT(MODIFYYMD, 'yyyy/MM/01')  = @MODIFYYMD                                                  ")
        End If
        '変更ユーザが指定されている場合
        If Not WF_DDL_MODIFYUSER.SelectedValue = "" Then
            SQLStr.AppendLine(" AND coalesce(RTRIM(MODIFYUSER), '')  =  @MODIFYUSER               　　　　　　　　　　　　　　　　  ")
        End If
        SQLStr.AppendLine(" ORDER BY                                                                                              ")
        SQLStr.AppendLine("    MODIFYYMD DESC                                                                                     ")
        SQLStr.AppendLine("   ,CTNTYPE                                                                                            ")
        SQLStr.AppendLine("   ,CTNNO                                                                                              ")
        SQLStr.AppendLine("   ,MODIFYKBN                                                                                          ")

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
                        LNM0002tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0002tbl.Load(SQLdr)
                End Using

                Dim i As Integer = 0
                For Each LNM0002row As DataRow In LNM0002tbl.Rows
                    i += 1
                    LNM0002row("LINECNT") = i        'LINECNT
                Next
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0002H SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0002H Select"
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
        SQLStr.AppendLine("     FORMAT(MODIFYYMD, 'yyyy/MM') AS MODIFYYM ")
        SQLStr.AppendLine(" FROM lng.LNT0115_RECONHIST ")
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0115_RECONHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0115_RECONHIST Select"
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
        SQLStr.AppendLine("     FORMAT(MODIFYYMD, 'dd') AS MODIFYDD ")
        SQLStr.AppendLine(" FROM lng.LNT0115_RECONHIST ")
        SQLStr.AppendLine(" WHERE                                                                                                 ")
        SQLStr.AppendLine("    FORMAT(MODIFYYMD, 'yyyy/MM')  = @MODIFYYM                                                         ")
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0115_RECONHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0115_RECONHIST Select"
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
        SQLStr.AppendLine(" FROM lng.LNT0115_RECONHIST ")
        SQLStr.AppendLine(" WHERE                                                                                                 ")
        '変更日が指定されている場合
        If Not WF_DDL_MODIFYDD.SelectedValue = "" Then
            SQLStr.AppendLine("    FORMAT(MODIFYYMD, 'yyyy/MM/dd')  = @MODIFYYMD                                                  ")
        Else
            SQLStr.AppendLine("    FORMAT(MODIFYYMD, 'yyyy/MM/01')  = @MODIFYYMD                                                  ")
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0115_RECONHIST SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0115_RECONHIST Select"
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
        For Each LNM0002row As DataRow In LNM0002tbl.Rows
            If LNM0002row("HIDDEN") = 0 Then
                WW_DataCNT += 1
                ' 行(LINECNT)を再設定する。既存項目(SELECT)を利用
                LNM0002row("SELECT") = WW_DataCNT
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
        Dim TBLview As DataView = New DataView(LNM0002tbl)

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
        Dim TBLview As New DataView(LNM0002tbl)
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
        UrlRoot = String.Format("{0}://{1}/{3}/{2}/", HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Host, CS0050SESSION.USERID, CS0050SESSION.PRINT_ROOT_URL_NAME)

        'Excel新規作成
        Dim wb As Workbook = New GrapeCity.Documents.Excel.Workbook

        '最大列(RANGE)を取得
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0002WRKINC.HISTORYEXCELCOL)).Cast(Of Integer)().Max()

        'シート名
        wb.ActiveSheet.Name = Left(WF_DDL_MODIFYYM.SelectedValue, 4) + "年" + Right(WF_DDL_MODIFYYM.SelectedValue, 2) + "月"

        'シート全体設定
        SetALL(wb.ActiveSheet)

        '行幅設定
        SetROWSHEIGHT(wb.ActiveSheet)

        '明細設定
        Dim WW_ACTIVEROW As Integer = 3
        SetDETAIL(wb.ActiveSheet, WW_ACTIVEROW)

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
        wb.ActiveSheet.Range("C1").Value = "コンテナマスタ変更履歴一覧"

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
            Case LNM0002WRKINC.FILETYPE.EXCEL
                FileName = "コンテナマスタ変更履歴.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNM0002WRKINC.FILETYPE.PDF
                FileName = "コンテナマスタ変更履歴.pdf"
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
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.OPERATEKBNNAME).Value = "操作区分"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.MODIFYKBNNAME).Value = "変更区分"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.MODIFYYMD).Value = "変更日時"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.MODIFYUSER).Value = "変更USER"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.DELFLG).Value = "削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.CTNTYPE).Value = "コンテナ記号"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.CTNNO).Value = "コンテナ番号"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.JURISDICTIONCD).Value = "所管部コード"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ACCOUNTINGASSETSCD).Value = "経理資産コード"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ACCOUNTINGASSETSKBN).Value = "経理資産区分"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.DUMMYKBN).Value = "ダミー区分"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.SPOTKBN).Value = "スポット区分"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.SPOTSTYMD).Value = "スポット区分　開始年月日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.SPOTENDYMD).Value = "スポット区分　終了年月日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.BIGCTNCD).Value = "大分類コード"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.MIDDLECTNCD).Value = "中分類コード"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.SMALLCTNCD).Value = "小分類コード"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.CONSTRUCTIONYM).Value = "建造年月"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.CTNMAKER).Value = "コンテナメーカー"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.FROZENMAKER).Value = "冷凍機メーカー"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.GROSSWEIGHT).Value = "総重量"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.CARGOWEIGHT).Value = "荷重"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.MYWEIGHT).Value = "自重"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.BOOKVALUE).Value = "簿価商品価格"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.OUTHEIGHT).Value = "外寸・高さ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.OUTWIDTH).Value = "外寸・幅"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.OUTLENGTH).Value = "外寸・長さ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.INHEIGHT).Value = "内寸・高さ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.INWIDTH).Value = "内寸・幅"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.INLENGTH).Value = "内寸・長さ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.WIFEHEIGHT).Value = "妻入口・高さ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.WIFEWIDTH).Value = "妻入口・幅"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.SIDEHEIGHT).Value = "側入口・高さ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.SIDEWIDTH).Value = "側入口・幅"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.FLOORAREA).Value = "床面積"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.INVOLUMEMARKING).Value = "内容積・標記"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.INVOLUMEACTUA).Value = "内容積・実寸"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.TRAINSCYCLEDAYS).Value = "交番検査・ｻｲｸﾙ日数"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.TRAINSBEFORERUNYMD).Value = "交番検査・前回実施日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.TRAINSNEXTRUNYMD).Value = "交番検査・次回実施日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSCYCLEDAYS).Value = "定期検査・ｻｲｸﾙ月数"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSCYCLEHOURMETER).Value = "定期検査・ｻｲｸﾙｱﾜﾒｰﾀ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSBEFORERUNYMD).Value = "定期検査・前回実施日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSNEXTRUNYMD).Value = "定期検査・次回実施日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSHOURMETERYMD).Value = "定期検査・ｱﾜﾒｰﾀ記載日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSHOURMETERTIME).Value = "定期検査・ｱﾜﾒｰﾀ時間"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSHOURMETERDSP).Value = "定期検査・ｱﾜﾒｰﾀ表示桁"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.OPERATIONSTYMD).Value = "運用開始年月日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.OPERATIONENDYMD).Value = "運用除外年月日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.RETIRMENTYMD).Value = "除却年月日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.COMPKANKBN).Value = "複合一貫区分"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.SUPPLYFLG).Value = "調達フラグ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM1).Value = "付帯項目１"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM2).Value = "付帯項目２"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM3).Value = "付帯項目３"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM4).Value = "付帯項目４"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM5).Value = "付帯項目５"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM6).Value = "付帯項目６"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM7).Value = "付帯項目７"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM8).Value = "付帯項目８"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM9).Value = "付帯項目９"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM10).Value = "付帯項目１０"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM11).Value = "付帯項目１１"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM12).Value = "付帯項目１２"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM13).Value = "付帯項目１３"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM14).Value = "付帯項目１４"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM15).Value = "付帯項目１５"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM16).Value = "付帯項目１６"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM17).Value = "付帯項目１７"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM18).Value = "付帯項目１８"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM19).Value = "付帯項目１９"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM20).Value = "付帯項目２０"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM21).Value = "付帯項目２１"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM22).Value = "付帯項目２２"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM23).Value = "付帯項目２３"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM24).Value = "付帯項目２４"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM25).Value = "付帯項目２５"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM26).Value = "付帯項目２５"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM27).Value = "付帯項目２７"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM28).Value = "付帯項目２８"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM29).Value = "付帯項目２９"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM30).Value = "付帯項目３０"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM31).Value = "付帯項目３１"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM32).Value = "付帯項目３２"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM33).Value = "付帯項目３３"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM34).Value = "付帯項目３４"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM35).Value = "付帯項目３５"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM36).Value = "付帯項目３６"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM37).Value = "付帯項目３７"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM38).Value = "付帯項目３８"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM39).Value = "付帯項目３９"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM40).Value = "付帯項目４０"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM41).Value = "付帯項目４１"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM42).Value = "付帯項目４２"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM43).Value = "付帯項目４３"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM44).Value = "付帯項目４４"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM45).Value = "付帯項目４５"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM46).Value = "付帯項目４６"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM47).Value = "付帯項目４７"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM48).Value = "付帯項目４８"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM49).Value = "付帯項目４９"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM50).Value = "付帯項目５０"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.HISTORYEXCELCOL.FLOORMATERIAL).Value = "床材質コード"

    End Sub

    ''' <summary>
    ''' 明細設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDETAIL(ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)

        For Each Row As DataRow In LNM0002tbl.Rows
            '値
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.OPERATEKBNNAME).Value = Row("OPERATEKBNNAME") '操作区分
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.MODIFYKBNNAME).Value = Row("MODIFYKBNNAME") '変更区分
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.MODIFYYMD).Value = Row("MODIFYYMD") '変更日時
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.MODIFYUSER).Value = Row("MODIFYUSER") '変更USER
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.CTNTYPE).Value = Row("CTNTYPE") 'コンテナ記号
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.CTNNO).Value = Row("CTNNO") 'コンテナ番号
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.JURISDICTIONCD).Value = Row("JURISDICTIONCD") '所管部コード
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ACCOUNTINGASSETSCD).Value = Row("ACCOUNTINGASSETSCD") '経理資産コード
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ACCOUNTINGASSETSKBN).Value = Row("ACCOUNTINGASSETSKBN") '経理資産区分
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.DUMMYKBN).Value = Row("DUMMYKBN") 'ダミー区分
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.SPOTKBN).Value = Row("SPOTKBN") 'スポット区分
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.SPOTSTYMD).Value = Row("SPOTSTYMD") 'スポット区分　開始年月日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.SPOTENDYMD).Value = Row("SPOTENDYMD") 'スポット区分　終了年月日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.BIGCTNCD).Value = Row("BIGCTNCD") '大分類コード
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.MIDDLECTNCD).Value = Row("MIDDLECTNCD") '中分類コード
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.SMALLCTNCD).Value = Row("SMALLCTNCD") '小分類コード
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.CONSTRUCTIONYM).Value = Row("CONSTRUCTIONYM") '建造年月
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.CTNMAKER).Value = Row("CTNMAKER") 'コンテナメーカー
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.FROZENMAKER).Value = Row("FROZENMAKER") '冷凍機メーカー
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.GROSSWEIGHT).Value = Row("GROSSWEIGHT") '総重量
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.CARGOWEIGHT).Value = Row("CARGOWEIGHT") '荷重
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.MYWEIGHT).Value = Row("MYWEIGHT") '自重
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.BOOKVALUE).Value = Row("BOOKVALUE") '簿価商品価格
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.OUTHEIGHT).Value = Row("OUTHEIGHT") '外寸・高さ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.OUTWIDTH).Value = Row("OUTWIDTH") '外寸・幅
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.OUTLENGTH).Value = Row("OUTLENGTH") '外寸・長さ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.INHEIGHT).Value = Row("INHEIGHT") '内寸・高さ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.INWIDTH).Value = Row("INWIDTH") '内寸・幅
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.INLENGTH).Value = Row("INLENGTH") '内寸・長さ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.WIFEHEIGHT).Value = Row("WIFEHEIGHT") '妻入口・高さ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.WIFEWIDTH).Value = Row("WIFEWIDTH") '妻入口・幅
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.SIDEHEIGHT).Value = Row("SIDEHEIGHT") '側入口・高さ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.SIDEWIDTH).Value = Row("SIDEWIDTH") '側入口・幅
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.FLOORAREA).Value = Row("FLOORAREA") '床面積
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.INVOLUMEMARKING).Value = Row("INVOLUMEMARKING") '内容積・標記
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.INVOLUMEACTUA).Value = Row("INVOLUMEACTUA") '内容積・実寸
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.TRAINSCYCLEDAYS).Value = Row("TRAINSCYCLEDAYS") '交番検査・ｻｲｸﾙ日数
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.TRAINSBEFORERUNYMD).Value = Row("TRAINSBEFORERUNYMD") '交番検査・前回実施日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.TRAINSNEXTRUNYMD).Value = Row("TRAINSNEXTRUNYMD") '交番検査・次回実施日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSCYCLEDAYS).Value = Row("REGINSCYCLEDAYS") '定期検査・ｻｲｸﾙ月数
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSCYCLEHOURMETER).Value = Row("REGINSCYCLEHOURMETER") '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSBEFORERUNYMD).Value = Row("REGINSBEFORERUNYMD") '定期検査・前回実施日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSNEXTRUNYMD).Value = Row("REGINSNEXTRUNYMD") '定期検査・次回実施日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSHOURMETERYMD).Value = Row("REGINSHOURMETERYMD") '定期検査・ｱﾜﾒｰﾀ記載日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSHOURMETERTIME).Value = Row("REGINSHOURMETERTIME") '定期検査・ｱﾜﾒｰﾀ時間
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.REGINSHOURMETERDSP).Value = Row("REGINSHOURMETERDSP") '定期検査・ｱﾜﾒｰﾀ表示桁
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.OPERATIONSTYMD).Value = Row("OPERATIONSTYMD") '運用開始年月日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.OPERATIONENDYMD).Value = Row("OPERATIONENDYMD") '運用除外年月日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.RETIRMENTYMD).Value = Row("RETIRMENTYMD") '除却年月日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.COMPKANKBN).Value = Row("COMPKANKBN") '複合一貫区分
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.SUPPLYFLG).Value = Row("SUPPLYFLG") '調達フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM1).Value = Row("ADDITEM1") '付帯項目１
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM2).Value = Row("ADDITEM2") '付帯項目２
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM3).Value = Row("ADDITEM3") '付帯項目３
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM4).Value = Row("ADDITEM4") '付帯項目４
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM5).Value = Row("ADDITEM5") '付帯項目５
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM6).Value = Row("ADDITEM6") '付帯項目６
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM7).Value = Row("ADDITEM7") '付帯項目７
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM8).Value = Row("ADDITEM8") '付帯項目８
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM9).Value = Row("ADDITEM9") '付帯項目９
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM10).Value = Row("ADDITEM10") '付帯項目１０
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM11).Value = Row("ADDITEM11") '付帯項目１１
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM12).Value = Row("ADDITEM12") '付帯項目１２
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM13).Value = Row("ADDITEM13") '付帯項目１３
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM14).Value = Row("ADDITEM14") '付帯項目１４
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM15).Value = Row("ADDITEM15") '付帯項目１５
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM16).Value = Row("ADDITEM16") '付帯項目１６
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM17).Value = Row("ADDITEM17") '付帯項目１７
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM18).Value = Row("ADDITEM18") '付帯項目１８
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM19).Value = Row("ADDITEM19") '付帯項目１９
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM20).Value = Row("ADDITEM20") '付帯項目２０
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM21).Value = Row("ADDITEM21") '付帯項目２１
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM22).Value = Row("ADDITEM22") '付帯項目２２
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM23).Value = Row("ADDITEM23") '付帯項目２３
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM24).Value = Row("ADDITEM24") '付帯項目２４
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM25).Value = Row("ADDITEM25") '付帯項目２５
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM26).Value = Row("ADDITEM26") '付帯項目２５
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM27).Value = Row("ADDITEM27") '付帯項目２７
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM28).Value = Row("ADDITEM28") '付帯項目２８
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM29).Value = Row("ADDITEM29") '付帯項目２９
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM30).Value = Row("ADDITEM30") '付帯項目３０
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM31).Value = Row("ADDITEM31") '付帯項目３１
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM32).Value = Row("ADDITEM32") '付帯項目３２
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM33).Value = Row("ADDITEM33") '付帯項目３３
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM34).Value = Row("ADDITEM34") '付帯項目３４
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM35).Value = Row("ADDITEM35") '付帯項目３５
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM36).Value = Row("ADDITEM36") '付帯項目３６
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM37).Value = Row("ADDITEM37") '付帯項目３７
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM38).Value = Row("ADDITEM38") '付帯項目３８
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM39).Value = Row("ADDITEM39") '付帯項目３９
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM40).Value = Row("ADDITEM40") '付帯項目４０
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM41).Value = Row("ADDITEM41") '付帯項目４１
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM42).Value = Row("ADDITEM42") '付帯項目４２
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM43).Value = Row("ADDITEM43") '付帯項目４３
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM44).Value = Row("ADDITEM44") '付帯項目４４
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM45).Value = Row("ADDITEM45") '付帯項目４５
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM46).Value = Row("ADDITEM46") '付帯項目４６
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM47).Value = Row("ADDITEM47") '付帯項目４７
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM48).Value = Row("ADDITEM48") '付帯項目４８
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM49).Value = Row("ADDITEM49") '付帯項目４９
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.ADDITEM50).Value = Row("ADDITEM50") '付帯項目５０
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.HISTORYEXCELCOL.FLOORMATERIAL).Value = Row("FLOORMATERIAL") '床材質コード

            '変更区分が変更後の行の場合
            If Row("MODIFYKBN") = LNM0002WRKINC.MODIFYKBN.AFTDATA Then
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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0002WRKINC.HISTORYEXCELCOL)).Cast(Of Integer)().Max()

        '変更チェック開始列を取得
        Dim WW_STCOL As Integer = LNM0002WRKINC.HISTORYEXCELCOL.DELFLG   '削除フラグ

        '開始列から最大列まで変更前後の値を確認
        For index As Integer = WW_STCOL To WW_MAXCOL
            '変更前と変更後が不一致の場合
            If Not sheet.Cells(WW_ACTIVEROW - 1, index).Value = sheet.Cells(WW_ACTIVEROW, index).Value Then

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

