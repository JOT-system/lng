''************************************************************
' コンテナマスタメンテ一覧画面
' 作成日 2022/01/14
' 更新日 2024/01/10
' 作成者 名取
' 更新者 大浜
'
' 修正履歴 : 2021/02/14 新規作成
'          : 2024/01/10 変更履歴画面、UL/DL機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports System.Drawing
Imports System.IO
Imports GrapeCity.Documents.Excel
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' コンテナマスタ登録（一覧）
''' </summary>
''' <remarks></remarks>
Public Class LNM0002ReconmList
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0002tbl As DataTable                                  '一覧格納用テーブル
    Private UploadFileTbl As New DataTable                           '添付ファイルテーブル
    Private LNM0002Exceltbl As New DataTable                         'Excelデータ格納用テーブル

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

    '○ 共通関数宣言(BASEDLL)
    Private CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
    Private CS0013ProfView As New CS0013ProfView                    'Tableオブジェクト展開
    Private CS0020JOURNAL As New CS0020JOURNAL                      '更新ジャーナル出力
    Private CS0030REPORT As New CS0030REPORT                        '帳票出力
    Private CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理
    Private CS0025AUTHORget As New CS0025AUTHORget                  '権限チェック(マスタチェック)

    '○ 共通処理結果
    Private WW_ERR_SW As String = ""
    Private WW_RtnSW As String = ""
    Private WW_Dummy As String = ""
    Private WW_ErrCode As String                                    'サブ用リターンコード
    Private WW_Kbn01 As String = "01"                               '区分判定(01)
    Private WW_Kbn02 As String = "02"                               '区分判定(02)
    Private WW_DefaultReginsHourMeterDsp As String = "4"            'デフォルトアワメータ表示桁数
    Private WW_CntKey As String = "KOBAN"                           'コントロールＫＥＹ(交番検査)

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
                        Case "WF_ButtonINSERT"          '追加ボタン押下
                            WF_ButtonINSERT_Click()
                        Case "WF_ButtonHISTORY"         '変更履歴ボタン押下
                            WF_ButtonHISTORY_Click()
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
                        Case "WF_GridDBclick"           'GridViewダブルクリック
                            WF_Grid_DBClick()
                        Case "WF_ButtonUPLOAD"          'ｱｯﾌﾟﾛｰﾄﾞボタン押下
                            WF_ButtonUPLOAD_Click()
                        Case "WF_ButtonDebug"           'デバッグボタン押下
                            WF_ButtonDEBUG_Click()
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
        Master.MAPID = LNM0002WRKINC.MAPIDL
        '○ HELP表示有無設定
        Master.dispHelp = False
        '○ D&D有無設定
        Master.eventDrop = False
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

        '〇 更新画面からの遷移の場合、更新完了メッセージを出力
        If Not String.IsNullOrEmpty(work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text) Then
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)
            work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""
        End If

        '○ GridView初期設定
        GridViewInitialize()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0002S Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0002D Then
            Master.RecoverTable(LNM0002tbl, work.WF_SEL_INPTBL.Text)
        End If

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
        '     条件指定に従い該当データをコンテナマスタから取得する
        Dim SQLStr As String =
              " Select                                                                                     " _
            & "     1                                                              AS 'SELECT'             " _
            & "   , 0                                                              AS HIDDEN               " _
            & "   , 0                                                              AS LINECNT              " _
            & "   , ''                                                             AS OPERATION            " _
            & "   , LNM0002.UPDTIMSTP                                              AS UPDTIMSTP            " _
            & "   , coalesce(RTRIM(LNM0002.DELFLG), '')                              AS DELFLG               " _
            & "   , coalesce(RTRIM(LNM0002.CTNTYPE), '')                             AS CTNTYPE              " _
            & "   , coalesce(RTRIM(LNM0002.CTNNO), '')                               AS CTNNO                " _
            & "   , coalesce(RTRIM(LNM0002.JURISDICTIONCD), '')                      AS JURISDICTIONCD       " _
            & "   , coalesce(RTRIM(LNM0002.ACCOUNTINGASSETSCD), '')                  AS ACCOUNTINGASSETSCD   " _
            & "   , coalesce(RTRIM(LNM0002.ACCOUNTINGASSETSKBN), '')                 AS ACCOUNTINGASSETSKBN  " _
            & "   , coalesce(RTRIM(LNM0002.DUMMYKBN), '')                            AS DUMMYKBN             " _
            & "   , coalesce(RTRIM(LNM0002.SPOTKBN), '')                             AS SPOTKBN              " _
            & "   , coalesce(FORMAT(LNM0002.SPOTSTYMD, 'yyyy/MM/dd'), '')            AS SPOTSTYMD            " _
            & "   , coalesce(FORMAT(LNM0002.SPOTENDYMD, 'yyyy/MM/dd'), '')           AS SPOTENDYMD           " _
            & "   , coalesce(RTRIM(LNM0002.BIGCTNCD), '')                            AS BIGCTNCD             " _
            & "   , coalesce(RTRIM(LNM0002.MIDDLECTNCD), '')                         AS MIDDLECTNCD          " _
            & "   , coalesce(RTRIM(LNM0002.SMALLCTNCD), '')                          AS SMALLCTNCD           " _
            & "   , coalesce(RTRIM(LNM0002.CONSTRUCTIONYM), '')                      AS CONSTRUCTIONYM       " _
            & "   , coalesce(RTRIM(LNM0002.CTNMAKER), '')                            AS CTNMAKER             " _
            & "   , coalesce(RTRIM(LNM0002.FROZENMAKER), '')                         AS FROZENMAKER          " _
            & "   , coalesce(RTRIM(LNM0002.GROSSWEIGHT), '')                         AS GROSSWEIGHT          " _
            & "   , coalesce(RTRIM(LNM0002.CARGOWEIGHT), '')                         AS CARGOWEIGHT          " _
            & "   , coalesce(RTRIM(LNM0002.MYWEIGHT), '')                            AS MYWEIGHT             " _
            & "   , RTRIM(CONVERT(NUMERIC ,coalesce(LNM0002.BOOKVALUE, 0)))          AS BOOKVALUE            " _
            & "   , coalesce(RTRIM(LNM0002.OUTHEIGHT), '')                           AS OUTHEIGHT            " _
            & "   , coalesce(RTRIM(LNM0002.OUTWIDTH), '')                            AS OUTWIDTH             " _
            & "   , coalesce(RTRIM(LNM0002.OUTLENGTH), '')                           AS OUTLENGTH            " _
            & "   , coalesce(RTRIM(LNM0002.INHEIGHT), '')                            AS INHEIGHT             " _
            & "   , coalesce(RTRIM(LNM0002.INWIDTH), '')                             AS INWIDTH              " _
            & "   , coalesce(RTRIM(LNM0002.INLENGTH), '')                            AS INLENGTH             " _
            & "   , coalesce(RTRIM(LNM0002.WIFEHEIGHT), '')                          AS WIFEHEIGHT           " _
            & "   , coalesce(RTRIM(LNM0002.WIFEWIDTH), '')                           AS WIFEWIDTH            " _
            & "   , coalesce(RTRIM(LNM0002.SIDEHEIGHT), '')                          AS SIDEHEIGHT           " _
            & "   , coalesce(RTRIM(LNM0002.SIDEWIDTH), '')                           AS SIDEWIDTH            " _
            & "   , coalesce(RTRIM(LNM0002.FLOORAREA), '')                           AS FLOORAREA            " _
            & "   , coalesce(RTRIM(LNM0002.INVOLUMEMARKING), '')                     AS INVOLUMEMARKING      " _
            & "   , coalesce(RTRIM(LNM0002.INVOLUMEACTUA), '')                       AS INVOLUMEACTUA        " _
            & "   , coalesce(RTRIM(LNM0002.TRAINSCYCLEDAYS), '')                     AS TRAINSCYCLEDAYS      " _
            & "   , coalesce(FORMAT(LNM0002.TRAINSBEFORERUNYMD, 'yyyy/MM/dd'), '')   AS TRAINSBEFORERUNYMD   " _
            & "   , coalesce(FORMAT(LNM0002.TRAINSNEXTRUNYMD, 'yyyy/MM/dd'), '')     AS TRAINSNEXTRUNYMD     " _
            & "   , coalesce(RTRIM(LNM0002.REGINSCYCLEDAYS), '')                     AS REGINSCYCLEDAYS      " _
            & "   , coalesce(RTRIM(LNM0002.REGINSCYCLEHOURMETER), '')                AS REGINSCYCLEHOURMETER " _
            & "   , coalesce(FORMAT(LNM0002.REGINSBEFORERUNYMD, 'yyyy/MM/dd'), '')   AS REGINSBEFORERUNYMD   " _
            & "   , coalesce(FORMAT(LNM0002.REGINSNEXTRUNYMD, 'yyyy/MM/dd'), '')     AS REGINSNEXTRUNYMD     " _
            & "   , coalesce(FORMAT(LNM0002.REGINSHOURMETERYMD, 'yyyy/MM/dd'), '')   AS REGINSHOURMETERYMD   " _
            & "   , coalesce(RTRIM(LNM0002.REGINSHOURMETERTIME), '')                 AS REGINSHOURMETERTIME  " _
            & "   , coalesce(RTRIM(LNM0002.REGINSHOURMETERDSP), '')                  AS REGINSHOURMETERDSP   " _
            & "   , coalesce(FORMAT(LNM0002.OPERATIONSTYMD, 'yyyy/MM/dd'), '')       AS OPERATIONSTYMD       " _
            & "   , coalesce(FORMAT(LNM0002.OPERATIONENDYMD, 'yyyy/MM/dd'), '')      AS OPERATIONENDYMD      " _
            & "   , coalesce(FORMAT(LNM0002.RETIRMENTYMD, 'yyyy/MM/dd'), '')         AS RETIRMENTYMD         " _
            & "   , coalesce(RTRIM(LNM0002.COMPKANKBN), '')                          AS COMPKANKBN           " _
            & "   , coalesce(RTRIM(LNM0002.SUPPLYFLG), '')                           AS SUPPLYFLG            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM1), '')                            AS ADDITEM1             " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM2), '')                            AS ADDITEM2             " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM3), '')                            AS ADDITEM3             " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM4), '')                            AS ADDITEM4             " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM5), '')                            AS ADDITEM5             " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM6), '')                            AS ADDITEM6             " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM7), '')                            AS ADDITEM7             " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM8), '')                            AS ADDITEM8             " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM9), '')                            AS ADDITEM9             " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM10), '')                           AS ADDITEM10            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM11), '')                           AS ADDITEM11            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM12), '')                           AS ADDITEM12            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM13), '')                           AS ADDITEM13            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM14), '')                           AS ADDITEM14            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM15), '')                           AS ADDITEM15            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM16), '')                           AS ADDITEM16            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM17), '')                           AS ADDITEM17            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM18), '')                           AS ADDITEM18            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM19), '')                           AS ADDITEM19            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM20), '')                           AS ADDITEM20            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM21), '')                           AS ADDITEM21            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM22), '')                           AS ADDITEM22            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM23), '')                           AS ADDITEM23            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM24), '')                           AS ADDITEM24            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM25), '')                           AS ADDITEM25            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM26), '')                           AS ADDITEM26            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM27), '')                           AS ADDITEM27            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM28), '')                           AS ADDITEM28            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM29), '')                           AS ADDITEM29            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM30), '')                           AS ADDITEM30            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM31), '')                           AS ADDITEM31            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM32), '')                           AS ADDITEM32            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM33), '')                           AS ADDITEM33            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM34), '')                           AS ADDITEM34            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM35), '')                           AS ADDITEM35            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM36), '')                           AS ADDITEM36            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM37), '')                           AS ADDITEM37            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM38), '')                           AS ADDITEM38            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM39), '')                           AS ADDITEM39            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM40), '')                           AS ADDITEM40            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM41), '')                           AS ADDITEM41            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM42), '')                           AS ADDITEM42            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM43), '')                           AS ADDITEM43            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM44), '')                           AS ADDITEM44            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM45), '')                           AS ADDITEM45            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM46), '')                           AS ADDITEM46            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM47), '')                           AS ADDITEM47            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM48), '')                           AS ADDITEM48            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM49), '')                           AS ADDITEM49            " _
            & "   , coalesce(RTRIM(LNM0002.ADDITEM50), '')                           AS ADDITEM50            " _
            & "   , coalesce(RTRIM(LNM0002.FLOORMATERIAL), '')                       AS FLOORMATERIAL        " _
            & " FROM                                                                                       " _
            & "     LNG.LNM0002_RECONM LNM0002                                                             "

        '○ 条件指定で指定されたものでSQLで可能なものを追加する
        Dim SQLWhereStr As String = ""
        ' コンテナ記号
        If Not String.IsNullOrEmpty(work.WF_SEL_CTNTYPE.Text) Then
            SQLWhereStr = " WHERE                       " _
                        & "     LNM0002.CTNTYPE = @P1 "
        End If
        ' コンテナ番号
        If Not String.IsNullOrEmpty(work.WF_SEL_CTNNO.Text) Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                   " _
                            & "     LNM0002.CTNNO = @P2 "
            Else
                SQLWhereStr &= "    AND LNM0002.CTNNO = @P2 "
            End If
        End If
        ' 論理削除フラグ
        If work.WF_SEL_DELDATAFLG.Text = "0" Then
            If String.IsNullOrEmpty(SQLWhereStr) Then
                SQLWhereStr = " WHERE                  " _
                            & "     LNM0002.DELFLG = 0 "
            Else
                SQLWhereStr &= "    AND LNM0002.DELFLG = 0 "
            End If
        End If

        SQLStr &= SQLWhereStr

        SQLStr &=
              " ORDER BY" _
            & "    LNM0002.CTNTYPE" _
            & "  , LNM0002.CTNNO"

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                If Not String.IsNullOrEmpty(work.WF_SEL_CTNTYPE.Text) Then
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 5)  'コンテナ記号
                    PARA1.Value = work.WF_SEL_CTNTYPE.Text
                End If
                If Not String.IsNullOrEmpty(work.WF_SEL_CTNNO.Text) Then
                    Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 8)  'コンテナ番号
                    PARA2.Value = work.WF_SEL_CTNNO.Text
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0002L SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0002L Select"
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

    End Sub

    ''' <summary>
    ''' 追加ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonINSERT_Click()


        work.WF_SEL_LINECNT.Text = ""                                                                     '選択行
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_DELFLG.Text)                            '削除
        work.WF_SEL_CTNTYPE2.Text = ""                                                                    'コンテナ記号
        work.WF_SEL_CTNNO2.Text = ""                                                                      'コンテナ番号
        Master.GetFirstValue(Master.USERCAMP, "JURISDICTIONCD", work.WF_SEL_JURISDICTIONCD.Text)          '所管部コード
        work.WF_SEL_ACCOUNTINGASSETSCD.Text = ""                                                          '経理資産コード
        work.WF_SEL_ACCOUNTINGASSETSKBN.Text = ""                                                         '経理資産区分
        work.WF_SEL_DUMMYKBN.Text = ""                                                                    'ダミー区分
        work.WF_SEL_SPOTKBN.Text = ""                                                                     'スポット区分
        work.WF_SEL_SPOTSTYMD.Text = ""                                                                   'スポット区分　開始年月日
        work.WF_SEL_SPOTENDYMD.Text = ""                                                                  'スポット区分　終了年月日
        work.WF_SEL_BIGCTNCD.Text = ""                                                                    '大分類コード
        work.WF_SEL_MIDDLECTNCD.Text = ""                                                                 '中分類コード
        work.WF_SEL_SMALLCTNCD.Text = ""                                                                  '小分類コード
        work.WF_SEL_CONSTRUCTIONYM.Text = ""                                                              '建造年月
        work.WF_SEL_CTNMAKER.Text = ""                                                                    'コンテナメーカー
        work.WF_SEL_FROZENMAKER.Text = ""                                                                 '冷凍機メーカー
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_GROSSWEIGHT.Text)                       '総重量
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_CARGOWEIGHT.Text)                       '荷重
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_MYWEIGHT.Text)                          '自重
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_BOOKVALUE.Text)                         '簿価商品価格
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_OUTHEIGHT.Text)                         '外寸・高さ
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_OUTWIDTH.Text)                          '外寸・幅
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_OUTLENGTH.Text)                         '外寸・長さ
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_INHEIGHT.Text)                          '内寸・高さ
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_INWIDTH.Text)                           '内寸・幅
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_INLENGTH.Text)                          '内寸・長さ
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_WIFEHEIGHT.Text)                        '妻入口・高さ
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_WIFEWIDTH.Text)                         '妻入口・幅
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SIDEHEIGHT.Text)                        '側入口・高さ
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_SIDEWIDTH.Text)                         '側入口・幅
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_FLOORAREA.Text)                         '床面積
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_INVOLUMEMARKING.Text)                   '内容積・標記
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_INVOLUMEACTUA.Text)                     '内容積・実寸
        Master.GetFirstValue(Master.USERCAMP, "TRAINSCYCLEDAYS", work.WF_SEL_TRAINSCYCLEDAYS.Text)        '交番検査・ｻｲｸﾙ日数
        work.WF_SEL_TRAINSBEFORERUNYMD.Text = ""                                                          '交番検査・前回実施日
        work.WF_SEL_TRAINSNEXTRUNYMD.Text = ""                                                            '交番検査・次回実施日
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_REGINSCYCLEDAYS.Text)                   '定期検査・ｻｲｸﾙ月数
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_REGINSCYCLEHOURMETER.Text)              '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
        work.WF_SEL_REGINSBEFORERUNYMD.Text = ""                                                          '定期検査・前回実施日
        work.WF_SEL_REGINSNEXTRUNYMD.Text = ""                                                            '定期検査・次回実施日
        work.WF_SEL_REGINSHOURMETERYMD.Text = ""                                                          '定期検査・ｱﾜﾒｰﾀ記載日
        Master.GetFirstValue(Master.USERCAMP, "ZERO", work.WF_SEL_REGINSHOURMETERTIME.Text)               '定期検査・ｱﾜﾒｰﾀ時間
        Master.GetFirstValue(Master.USERCAMP, "REGINSHOURMETERDSP", work.WF_SEL_REGINSHOURMETERDSP.Text)  '定期検査・ｱﾜﾒｰﾀ表示桁
        work.WF_SEL_OPERATIONSTYMD.Text = ""                                                              '運用開始年月日
        work.WF_SEL_OPERATIONENDYMD.Text = ""                                                             '運用除外年月日
        work.WF_SEL_RETIRMENTYMD.Text = ""                                                                '除却年月日
        work.WF_SEL_COMPKANKBN.Text = ""                                                                  '複合一貫区分
        work.WF_SEL_SUPPLYFLG.Text = ""                                                                   '調達フラグ
        work.WF_SEL_ADDITEM1.Text = ""                                                                    '付帯項目１
        work.WF_SEL_ADDITEM2.Text = ""                                                                    '付帯項目２
        work.WF_SEL_ADDITEM3.Text = ""                                                                    '付帯項目３
        work.WF_SEL_ADDITEM4.Text = ""                                                                    '付帯項目４
        work.WF_SEL_ADDITEM5.Text = ""                                                                    '付帯項目５
        work.WF_SEL_ADDITEM6.Text = ""                                                                    '付帯項目６
        work.WF_SEL_ADDITEM7.Text = ""                                                                    '付帯項目７
        work.WF_SEL_ADDITEM8.Text = ""                                                                    '付帯項目８
        work.WF_SEL_ADDITEM9.Text = ""                                                                    '付帯項目９
        work.WF_SEL_ADDITEM10.Text = ""                                                                   '付帯項目１０
        work.WF_SEL_ADDITEM11.Text = ""                                                                   '付帯項目１１
        work.WF_SEL_ADDITEM12.Text = ""                                                                   '付帯項目１２
        work.WF_SEL_ADDITEM13.Text = ""                                                                   '付帯項目１３
        work.WF_SEL_ADDITEM14.Text = ""                                                                   '付帯項目１４
        work.WF_SEL_ADDITEM15.Text = ""                                                                   '付帯項目１５
        work.WF_SEL_ADDITEM16.Text = ""                                                                   '付帯項目１６
        work.WF_SEL_ADDITEM17.Text = ""                                                                   '付帯項目１７
        work.WF_SEL_ADDITEM18.Text = ""                                                                   '付帯項目１８
        work.WF_SEL_ADDITEM19.Text = ""                                                                   '付帯項目１９
        work.WF_SEL_ADDITEM20.Text = ""                                                                   '付帯項目２０
        work.WF_SEL_ADDITEM21.Text = ""                                                                   '付帯項目２１
        work.WF_SEL_ADDITEM22.Text = ""                                                                   '付帯項目２２
        work.WF_SEL_ADDITEM23.Text = ""                                                                   '付帯項目２３
        work.WF_SEL_ADDITEM24.Text = ""                                                                   '付帯項目２４
        work.WF_SEL_ADDITEM25.Text = ""                                                                   '付帯項目２５
        work.WF_SEL_ADDITEM26.Text = ""                                                                   '付帯項目２６
        work.WF_SEL_ADDITEM27.Text = ""                                                                   '付帯項目２７
        work.WF_SEL_ADDITEM28.Text = ""                                                                   '付帯項目２８
        work.WF_SEL_ADDITEM29.Text = ""                                                                   '付帯項目２９
        work.WF_SEL_ADDITEM30.Text = ""                                                                   '付帯項目３０
        work.WF_SEL_ADDITEM31.Text = ""                                                                   '付帯項目３１
        work.WF_SEL_ADDITEM32.Text = ""                                                                   '付帯項目３２
        work.WF_SEL_ADDITEM33.Text = ""                                                                   '付帯項目３３
        work.WF_SEL_ADDITEM34.Text = ""                                                                   '付帯項目３４
        work.WF_SEL_ADDITEM35.Text = ""                                                                   '付帯項目３５
        work.WF_SEL_ADDITEM36.Text = ""                                                                   '付帯項目３６
        work.WF_SEL_ADDITEM37.Text = ""                                                                   '付帯項目３７
        work.WF_SEL_ADDITEM38.Text = ""                                                                   '付帯項目３８
        work.WF_SEL_ADDITEM39.Text = ""                                                                   '付帯項目３９
        work.WF_SEL_ADDITEM40.Text = ""                                                                   '付帯項目４０
        work.WF_SEL_ADDITEM41.Text = ""                                                                   '付帯項目４１
        work.WF_SEL_ADDITEM42.Text = ""                                                                   '付帯項目４２
        work.WF_SEL_ADDITEM43.Text = ""                                                                   '付帯項目４３
        work.WF_SEL_ADDITEM44.Text = ""                                                                   '付帯項目４４
        work.WF_SEL_ADDITEM45.Text = ""                                                                   '付帯項目４５
        work.WF_SEL_ADDITEM46.Text = ""                                                                   '付帯項目４６
        work.WF_SEL_ADDITEM47.Text = ""                                                                   '付帯項目４７
        work.WF_SEL_ADDITEM48.Text = ""                                                                   '付帯項目４８
        work.WF_SEL_ADDITEM49.Text = ""                                                                   '付帯項目４９
        work.WF_SEL_ADDITEM50.Text = ""                                                                   '付帯項目５０
        work.WF_SEL_FLOORMATERIAL.Text = ""                                                               '床材質コード
        work.WF_SEL_TIMESTAMP.Text = ""         　                                                        'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                                       '詳細画面更新メッセージ
        work.WF_SEL_DETAIL_DECISION.Text = "0"                                                            '追加ボタンから詳細画面へ

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0002tbl)

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存
        Master.SaveTable(LNM0002tbl, work.WF_SEL_INPTBL.Text)

        '○ 次ページ遷移
        Master.TransitionPage()

    End Sub

    ''' <summary>
    ''' 変更履歴ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonHISTORY_Click()
        Server.Transfer("~/LNG/mas/LNM0002ReconmHistory.aspx")
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

    ' ******************************************************************************
    ' ***  一覧表示(GridView)関連操作                                            ***
    ' ******************************************************************************

    ''' <summary>
    ''' 一覧画面-明細行ダブルクリック時処理 (GridView ---> detailbox)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_Grid_DBClick()

        Dim WW_DBDataCheck As String = ""
        Dim WW_LineCNT As Integer = 0

        '○ LINECNT取得
        Try
            Integer.TryParse(WF_GridDBclick.Text, WW_LineCNT)
            WW_LineCNT -= 1
        Catch ex As Exception
            Exit Sub
        End Try

        work.WF_SEL_LINECNT.Text = LNM0002tbl.Rows(WW_LineCNT)("LINECNT")                            '選択行
        work.WF_SEL_DELFLG.Text = LNM0002tbl.Rows(WW_LineCNT)("DELFLG")                              '削除フラグ
        work.WF_SEL_CTNTYPE2.Text = LNM0002tbl.Rows(WW_LineCNT)("CTNTYPE")                           'コンテナ記号
        work.WF_SEL_CTNNO2.Text = LNM0002tbl.Rows(WW_LineCNT)("CTNNO")                               'コンテナ番号
        work.WF_SEL_JURISDICTIONCD.Text = LNM0002tbl.Rows(WW_LineCNT)("JURISDICTIONCD")              '所管部コード
        work.WF_SEL_ACCOUNTINGASSETSCD.Text = LNM0002tbl.Rows(WW_LineCNT)("ACCOUNTINGASSETSCD")      '経理資産コード
        work.WF_SEL_ACCOUNTINGASSETSKBN.Text = LNM0002tbl.Rows(WW_LineCNT)("ACCOUNTINGASSETSKBN")    '経理資産区分
        work.WF_SEL_DUMMYKBN.Text = LNM0002tbl.Rows(WW_LineCNT)("DUMMYKBN")                          'ダミー区分
        work.WF_SEL_SPOTKBN.Text = LNM0002tbl.Rows(WW_LineCNT)("SPOTKBN")                            'スポット区分
        work.WF_SEL_SPOTSTYMD.Text = LNM0002tbl.Rows(WW_LineCNT)("SPOTSTYMD")                        'スポット区分　開始年月日
        work.WF_SEL_SPOTENDYMD.Text = LNM0002tbl.Rows(WW_LineCNT)("SPOTENDYMD")                      'スポット区分　終了年月日
        work.WF_SEL_BIGCTNCD.Text = LNM0002tbl.Rows(WW_LineCNT)("BIGCTNCD")                          '大分類コード
        work.WF_SEL_MIDDLECTNCD.Text = LNM0002tbl.Rows(WW_LineCNT)("MIDDLECTNCD")                    '中分類コード
        work.WF_SEL_SMALLCTNCD.Text = LNM0002tbl.Rows(WW_LineCNT)("SMALLCTNCD")                      '小分類コード
        work.WF_SEL_CONSTRUCTIONYM.Text = LNM0002tbl.Rows(WW_LineCNT)("CONSTRUCTIONYM")              '建造年月
        work.WF_SEL_CTNMAKER.Text = LNM0002tbl.Rows(WW_LineCNT)("CTNMAKER")                          'コンテナメーカー
        work.WF_SEL_FROZENMAKER.Text = LNM0002tbl.Rows(WW_LineCNT)("FROZENMAKER")                    '冷凍機メーカー
        work.WF_SEL_GROSSWEIGHT.Text = LNM0002tbl.Rows(WW_LineCNT)("GROSSWEIGHT")                    '総重量
        work.WF_SEL_CARGOWEIGHT.Text = LNM0002tbl.Rows(WW_LineCNT)("CARGOWEIGHT")                    '荷重
        work.WF_SEL_MYWEIGHT.Text = LNM0002tbl.Rows(WW_LineCNT)("MYWEIGHT")                          '自重
        work.WF_SEL_BOOKVALUE.Text = Val(LNM0002tbl.Rows(WW_LineCNT)("BOOKVALUE").ToString)          '簿価商品価格
        work.WF_SEL_OUTHEIGHT.Text = LNM0002tbl.Rows(WW_LineCNT)("OUTHEIGHT")                        '外寸・高さ
        work.WF_SEL_OUTWIDTH.Text = LNM0002tbl.Rows(WW_LineCNT)("OUTWIDTH")                          '外寸・幅
        work.WF_SEL_OUTLENGTH.Text = LNM0002tbl.Rows(WW_LineCNT)("OUTLENGTH")                        '外寸・長さ
        work.WF_SEL_INHEIGHT.Text = LNM0002tbl.Rows(WW_LineCNT)("INHEIGHT")                          '内寸・高さ
        work.WF_SEL_INWIDTH.Text = LNM0002tbl.Rows(WW_LineCNT)("INWIDTH")                            '内寸・幅
        work.WF_SEL_INLENGTH.Text = LNM0002tbl.Rows(WW_LineCNT)("INLENGTH")                          '内寸・長さ
        work.WF_SEL_WIFEHEIGHT.Text = LNM0002tbl.Rows(WW_LineCNT)("WIFEHEIGHT")                      '妻入口・高さ
        work.WF_SEL_WIFEWIDTH.Text = LNM0002tbl.Rows(WW_LineCNT)("WIFEWIDTH")                        '妻入口・幅
        work.WF_SEL_SIDEHEIGHT.Text = LNM0002tbl.Rows(WW_LineCNT)("SIDEHEIGHT")                      '側入口・高さ
        work.WF_SEL_SIDEWIDTH.Text = LNM0002tbl.Rows(WW_LineCNT)("SIDEWIDTH")                        '側入口・幅
        work.WF_SEL_FLOORAREA.Text = LNM0002tbl.Rows(WW_LineCNT)("FLOORAREA")                        '床面積
        work.WF_SEL_INVOLUMEMARKING.Text = LNM0002tbl.Rows(WW_LineCNT)("INVOLUMEMARKING")            '内容積・標記
        work.WF_SEL_INVOLUMEACTUA.Text = LNM0002tbl.Rows(WW_LineCNT)("INVOLUMEACTUA")                '内容積・実寸
        work.WF_SEL_TRAINSCYCLEDAYS.Text = LNM0002tbl.Rows(WW_LineCNT)("TRAINSCYCLEDAYS")            '交番検査・ｻｲｸﾙ日数
        work.WF_SEL_TRAINSBEFORERUNYMD.Text = LNM0002tbl.Rows(WW_LineCNT)("TRAINSBEFORERUNYMD")      '交番検査・前回実施日
        work.WF_SEL_TRAINSNEXTRUNYMD.Text = LNM0002tbl.Rows(WW_LineCNT)("TRAINSNEXTRUNYMD")          '交番検査・次回実施日
        work.WF_SEL_REGINSCYCLEDAYS.Text = LNM0002tbl.Rows(WW_LineCNT)("REGINSCYCLEDAYS")            '定期検査・ｻｲｸﾙ月数
        work.WF_SEL_REGINSCYCLEHOURMETER.Text = LNM0002tbl.Rows(WW_LineCNT)("REGINSCYCLEHOURMETER")  '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
        work.WF_SEL_REGINSBEFORERUNYMD.Text = LNM0002tbl.Rows(WW_LineCNT)("REGINSBEFORERUNYMD")      '定期検査・前回実施日
        work.WF_SEL_REGINSNEXTRUNYMD.Text = LNM0002tbl.Rows(WW_LineCNT)("REGINSNEXTRUNYMD")          '定期検査・次回実施日
        work.WF_SEL_REGINSHOURMETERYMD.Text = LNM0002tbl.Rows(WW_LineCNT)("REGINSHOURMETERYMD")      '定期検査・ｱﾜﾒｰﾀ記載日
        work.WF_SEL_REGINSHOURMETERTIME.Text = LNM0002tbl.Rows(WW_LineCNT)("REGINSHOURMETERTIME")    '定期検査・ｱﾜﾒｰﾀ時間
        work.WF_SEL_REGINSHOURMETERDSP.Text = LNM0002tbl.Rows(WW_LineCNT)("REGINSHOURMETERDSP")      '定期検査・ｱﾜﾒｰﾀ表示桁
        work.WF_SEL_OPERATIONSTYMD.Text = LNM0002tbl.Rows(WW_LineCNT)("OPERATIONSTYMD")              '運用開始年月日
        work.WF_SEL_OPERATIONENDYMD.Text = LNM0002tbl.Rows(WW_LineCNT)("OPERATIONENDYMD")            '運用除外年月日
        work.WF_SEL_RETIRMENTYMD.Text = LNM0002tbl.Rows(WW_LineCNT)("RETIRMENTYMD")                  '除却年月日
        work.WF_SEL_COMPKANKBN.Text = LNM0002tbl.Rows(WW_LineCNT)("COMPKANKBN")                      '複合一貫区分
        work.WF_SEL_SUPPLYFLG.Text = LNM0002tbl.Rows(WW_LineCNT)("SUPPLYFLG")                        '調達フラグ
        work.WF_SEL_ADDITEM1.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM1")                          '付帯項目１
        work.WF_SEL_ADDITEM2.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM2")                          '付帯項目２
        work.WF_SEL_ADDITEM3.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM3")                          '付帯項目３
        work.WF_SEL_ADDITEM4.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM4")                          '付帯項目４
        work.WF_SEL_ADDITEM5.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM5")                          '付帯項目５
        work.WF_SEL_ADDITEM6.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM6")                          '付帯項目６
        work.WF_SEL_ADDITEM7.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM7")                          '付帯項目７
        work.WF_SEL_ADDITEM8.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM8")                          '付帯項目８
        work.WF_SEL_ADDITEM9.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM9")                          '付帯項目９
        work.WF_SEL_ADDITEM10.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM10")                        '付帯項目１０
        work.WF_SEL_ADDITEM11.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM11")                        '付帯項目１１
        work.WF_SEL_ADDITEM12.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM12")                        '付帯項目１２
        work.WF_SEL_ADDITEM13.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM13")                        '付帯項目１３
        work.WF_SEL_ADDITEM14.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM14")                        '付帯項目１４
        work.WF_SEL_ADDITEM15.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM15")                        '付帯項目１５
        work.WF_SEL_ADDITEM16.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM16")                        '付帯項目１６
        work.WF_SEL_ADDITEM17.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM17")                        '付帯項目１７
        work.WF_SEL_ADDITEM18.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM18")                        '付帯項目１８
        work.WF_SEL_ADDITEM19.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM19")                        '付帯項目１９
        work.WF_SEL_ADDITEM20.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM20")                        '付帯項目２０
        work.WF_SEL_ADDITEM21.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM21")                        '付帯項目２１
        work.WF_SEL_ADDITEM22.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM22")                        '付帯項目２２
        work.WF_SEL_ADDITEM23.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM23")                        '付帯項目２３
        work.WF_SEL_ADDITEM24.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM24")                        '付帯項目２４
        work.WF_SEL_ADDITEM25.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM25")                        '付帯項目２５
        work.WF_SEL_ADDITEM26.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM26")                        '付帯項目２６
        work.WF_SEL_ADDITEM27.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM27")                        '付帯項目２７
        work.WF_SEL_ADDITEM28.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM28")                        '付帯項目２８
        work.WF_SEL_ADDITEM29.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM29")                        '付帯項目２９
        work.WF_SEL_ADDITEM30.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM30")                        '付帯項目３０
        work.WF_SEL_ADDITEM31.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM31")                        '付帯項目３１
        work.WF_SEL_ADDITEM32.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM32")                        '付帯項目３２
        work.WF_SEL_ADDITEM33.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM33")                        '付帯項目３３
        work.WF_SEL_ADDITEM34.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM34")                        '付帯項目３４
        work.WF_SEL_ADDITEM35.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM35")                        '付帯項目３５
        work.WF_SEL_ADDITEM36.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM36")                        '付帯項目３６
        work.WF_SEL_ADDITEM37.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM37")                        '付帯項目３７
        work.WF_SEL_ADDITEM38.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM38")                        '付帯項目３８
        work.WF_SEL_ADDITEM39.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM39")                        '付帯項目３９
        work.WF_SEL_ADDITEM40.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM40")                        '付帯項目４０
        work.WF_SEL_ADDITEM41.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM41")                        '付帯項目４１
        work.WF_SEL_ADDITEM42.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM42")                        '付帯項目４２
        work.WF_SEL_ADDITEM43.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM43")                        '付帯項目４３
        work.WF_SEL_ADDITEM44.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM44")                        '付帯項目４４
        work.WF_SEL_ADDITEM45.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM45")                        '付帯項目４５
        work.WF_SEL_ADDITEM46.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM46")                        '付帯項目４６
        work.WF_SEL_ADDITEM47.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM47")                        '付帯項目４７
        work.WF_SEL_ADDITEM48.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM48")                        '付帯項目４８
        work.WF_SEL_ADDITEM49.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM49")                        '付帯項目４９
        work.WF_SEL_ADDITEM50.Text = LNM0002tbl.Rows(WW_LineCNT)("ADDITEM50")                        '付帯項目５０
        work.WF_SEL_FLOORMATERIAL.Text = LNM0002tbl.Rows(WW_LineCNT)("FLOORMATERIAL")                '床材質コード
        work.WF_SEL_TIMESTAMP.Text = LNM0002tbl.Rows(WW_LineCNT)("UPDTIMSTP")                        'タイムスタンプ
        work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = ""                                                  '詳細画面更新メッセージ
        work.WF_SEL_DETAIL_DECISION.Text = "1"                                                       'ダブルクリックから詳細画面へ

        '○ 状態をクリア
        For Each LNM0002row As DataRow In LNM0002tbl.Rows
            Select Case LNM0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 選択明細の状態を設定
        Select Case LNM0002tbl.Rows(WW_LineCNT)("OPERATION")
            Case C_LIST_OPERATION_CODE.NODATA
                LNM0002tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.NODISP
                LNM0002tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.SELECTED
                LNM0002tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
            Case C_LIST_OPERATION_CODE.UPDATING
                LNM0002tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
            Case C_LIST_OPERATION_CODE.ERRORED
                LNM0002tbl.Rows(WW_LineCNT)("OPERATION") = C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
        End Select

        '○画面切替設定
        WF_BOXChange.Value = "detailbox"

        '○ 画面表示データ保存
        Master.SaveTable(LNM0002tbl)

        WF_GridDBclick.Text = ""

        '○ 遷移先(登録画面)退避データ保存先の作成
        WW_CreateXMLSaveFile()

        '○ 画面表示データ保存(遷移先(登録画面)向け)
        Master.SaveTable(LNM0002tbl, work.WF_SEL_INPTBL.Text)

        '〇 排他チェック
        If Not String.IsNullOrEmpty(work.WF_SEL_CTNTYPE2.Text) Then
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' 排他チェック
                work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_CTNTYPE2.Text,
                                                        work.WF_SEL_CTNNO2.Text,
                                                        work.WF_SEL_TIMESTAMP.Text)
            End Using

            If Not isNormal(WW_DBDataCheck) Then
                Master.Output(C_MESSAGE_NO.CTN_HAITA_DATA_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 登録画面ページへ遷移
        Master.TransitionPage(Master.USERCAMP)

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
        Dim WW_MAXCOL As Integer = [Enum].GetValues(GetType(LNM0002WRKINC.INOUTEXCELCOL)).Cast(Of Integer)().Max()

        'シート名
        wb.ActiveSheet.Name = "入出力"

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

        '入力必須列、入力不要列網掛け設定
        SetREQUNNECEHATCHING(wb.ActiveSheet)

        'ヘッダ設定
        SetHEADER(wb, wb.ActiveSheet, WW_MAXCOL)

        'その他設定
        wb.ActiveSheet.Range("A1").Value = "ID:" + Master.MAPID
        wb.ActiveSheet.Range("A2").Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED)
        wb.ActiveSheet.Range("B2").Value = "は入力必須"
        wb.ActiveSheet.Range("C1").Value = "コンテナマスタ一覧"
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
            Case LNM0002WRKINC.FILETYPE.EXCEL
                FileName = "コンテナマスタ.xlsx"
                FilePath = IO.Path.Combine(UploadRootPath, FileName)

                '保存
                wb.Save(FilePath, SaveFileFormat.Xlsx)

                'ダウンロード
                WF_PrintURL.Value = UrlRoot & FileName
                ClientScript.RegisterStartupScript(Me.GetType(), "key", "f_ExcelPrint();", True)
            Case LNM0002WRKINC.FILETYPE.PDF
                FileName = "コンテナマスタ.pdf"
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
        sheet.Columns(LNM0002WRKINC.INOUTEXCELCOL.DELFLG).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) '削除フラグ
        sheet.Columns(LNM0002WRKINC.INOUTEXCELCOL.CTNTYPE).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'コンテナ記号
        sheet.Columns(LNM0002WRKINC.INOUTEXCELCOL.CTNNO).Interior.Color = ColorTranslator.FromHtml(CONST_COLOR_HATCHING_REQUIRED) 'コンテナ番号

        '入力不要列網掛け

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
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.DELFLG).Value = "（必須）削除フラグ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.CTNTYPE).Value = "（必須）コンテナ記号"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.CTNNO).Value = "（必須）コンテナ番号"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.JURISDICTIONCD).Value = "所管部コード"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ACCOUNTINGASSETSCD).Value = "経理資産コード"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ACCOUNTINGASSETSKBN).Value = "経理資産区分"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.DUMMYKBN).Value = "ダミー区分"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SPOTKBN).Value = "スポット区分"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SPOTSTYMD).Value = "スポット区分　開始年月日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SPOTENDYMD).Value = "スポット区分　終了年月日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.BIGCTNCD).Value = "大分類コード"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Value = "中分類コード"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SMALLCTNCD).Value = "小分類コード"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.CONSTRUCTIONYM).Value = "建造年月"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.CTNMAKER).Value = "コンテナメーカー"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.FROZENMAKER).Value = "冷凍機メーカー"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.GROSSWEIGHT).Value = "総重量"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.CARGOWEIGHT).Value = "荷重"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.MYWEIGHT).Value = "自重"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.BOOKVALUE).Value = "簿価商品価格"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.OUTHEIGHT).Value = "外寸・高さ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.OUTWIDTH).Value = "外寸・幅"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.OUTLENGTH).Value = "外寸・長さ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.INHEIGHT).Value = "内寸・高さ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.INWIDTH).Value = "内寸・幅"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.INLENGTH).Value = "内寸・長さ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.WIFEHEIGHT).Value = "妻入口・高さ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.WIFEWIDTH).Value = "妻入口・幅"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SIDEHEIGHT).Value = "側入口・高さ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SIDEWIDTH).Value = "側入口・幅"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.FLOORAREA).Value = "床面積"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.INVOLUMEMARKING).Value = "内容積・標記"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.INVOLUMEACTUA).Value = "内容積・実寸"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.TRAINSCYCLEDAYS).Value = "交番検査・ｻｲｸﾙ日数"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.TRAINSBEFORERUNYMD).Value = "交番検査・前回実施日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.TRAINSNEXTRUNYMD).Value = "交番検査・次回実施日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSCYCLEDAYS).Value = "定期検査・ｻｲｸﾙ月数"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSCYCLEHOURMETER).Value = "定期検査・ｻｲｸﾙｱﾜﾒｰﾀ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSBEFORERUNYMD).Value = "定期検査・前回実施日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSNEXTRUNYMD).Value = "定期検査・次回実施日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSHOURMETERYMD).Value = "定期検査・ｱﾜﾒｰﾀ記載日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSHOURMETERTIME).Value = "定期検査・ｱﾜﾒｰﾀ時間"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSHOURMETERDSP).Value = "定期検査・ｱﾜﾒｰﾀ表示桁"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.OPERATIONSTYMD).Value = "運用開始年月日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.OPERATIONENDYMD).Value = "運用除外年月日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.RETIRMENTYMD).Value = "除却年月日"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.COMPKANKBN).Value = "複合一貫区分"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SUPPLYFLG).Value = "調達フラグ"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM1).Value = "付帯項目１"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM2).Value = "付帯項目２"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM3).Value = "付帯項目３"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM4).Value = "付帯項目４"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM5).Value = "付帯項目５"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM6).Value = "付帯項目６"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM7).Value = "付帯項目７"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM8).Value = "付帯項目８"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM9).Value = "付帯項目９"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM10).Value = "付帯項目１０"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM11).Value = "付帯項目１１"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM12).Value = "付帯項目１２"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM13).Value = "付帯項目１３"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM14).Value = "付帯項目１４"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM15).Value = "付帯項目１５"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM16).Value = "付帯項目１６"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM17).Value = "付帯項目１７"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM18).Value = "付帯項目１８"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM19).Value = "付帯項目１９"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM20).Value = "付帯項目２０"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM21).Value = "付帯項目２１"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM22).Value = "付帯項目２２"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM23).Value = "付帯項目２３"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM24).Value = "付帯項目２４"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM25).Value = "付帯項目２５"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM26).Value = "付帯項目２５"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM27).Value = "付帯項目２７"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM28).Value = "付帯項目２８"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM29).Value = "付帯項目２９"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM30).Value = "付帯項目３０"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM31).Value = "付帯項目３１"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM32).Value = "付帯項目３２"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM33).Value = "付帯項目３３"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM34).Value = "付帯項目３４"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM35).Value = "付帯項目３５"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM36).Value = "付帯項目３６"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM37).Value = "付帯項目３７"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM38).Value = "付帯項目３８"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM39).Value = "付帯項目３９"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM40).Value = "付帯項目４０"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM41).Value = "付帯項目４１"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM42).Value = "付帯項目４２"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM43).Value = "付帯項目４３"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM44).Value = "付帯項目４４"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM45).Value = "付帯項目４５"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM46).Value = "付帯項目４６"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM47).Value = "付帯項目４７"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM48).Value = "付帯項目４８"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM49).Value = "付帯項目４９"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM50).Value = "付帯項目５０"
        sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.FLOORMATERIAL).Value = "床材質コード"

        Dim WW_TEXT As String = ""
        Dim WW_CNT As Integer = 0
        Dim WW_HT As New Hashtable

        '○ コメント取得
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続

            '削除フラグ
            COMMENT_get(SQLcon, "DELFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.DELFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.DELFLG).Comment.Shape
                    .Width = 50
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '所管部コード
            COMMENT_get(SQLcon, "JURISDICTIONCD", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.JURISDICTIONCD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.JURISDICTIONCD).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '経理資産コード
            COMMENT_get(SQLcon, "ACCOUNTINGASSETSCD", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ACCOUNTINGASSETSCD).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ACCOUNTINGASSETSCD).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '経理資産区分
            COMMENT_get(SQLcon, "ACCOUNTINGASSETSKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ACCOUNTINGASSETSKBN).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ACCOUNTINGASSETSKBN).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            'ダミー区分
            COMMENT_get(SQLcon, "DUMMYKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.DUMMYKBN).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.DUMMYKBN).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            ''大分類コード
            'COMMENT_get(SQLcon, "BIGCTNCD", WW_TEXT, WW_CNT)
            'If Not WW_CNT = 0 Then
            '    sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.BIGCTNCD).AddComment(WW_TEXT)
            '    With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.BIGCTNCD).Comment.Shape
            '        .Width = 100
            '        .Height = CONST_HEIGHT_PER_ROW * WW_CNT
            '    End With
            'End If
            ''中分類コード
            'COMMENT_get(SQLcon, "MIDDLECTNCD", WW_TEXT, WW_CNT)
            'If Not WW_CNT = 0 Then
            '    sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.MIDDLECTNCD).AddComment(WW_TEXT)
            '    With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Comment.Shape
            '        .Width = 100
            '        .Height = CONST_HEIGHT_PER_ROW * WW_CNT
            '    End With
            'End If
            ''小分類コード
            'COMMENT_get(SQLcon, "SMALLCTNCD", WW_TEXT, WW_CNT)
            'If Not WW_CNT = 0 Then
            '    sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SMALLCTNCD).AddComment(WW_TEXT)
            '    With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SMALLCTNCD).Comment.Shape
            '        .Width = 100
            '        .Height = CONST_HEIGHT_PER_ROW * WW_CNT
            '    End With
            'End If
            'コンテナメーカー
            COMMENT_get(SQLcon, "CTNMAKER", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.CTNMAKER).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.CTNMAKER).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '冷凍機メーカー
            COMMENT_get(SQLcon, "FROZENMAKER", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.FROZENMAKER).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.FROZENMAKER).Comment.Shape
                    .Width = 200
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '複合一貫区分
            COMMENT_get(SQLcon, "COMPKANKBN", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.COMPKANKBN).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.COMPKANKBN).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '調達フラグ
            COMMENT_get(SQLcon, "SUPPLYFLG", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SUPPLYFLG).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SUPPLYFLG).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目１
            COMMENT_get(SQLcon, "ADDITEM1", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM1).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM1).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目２
            COMMENT_get(SQLcon, "ADDITEM2", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM2).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM2).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目３
            COMMENT_get(SQLcon, "ADDITEM3", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM3).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM3).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目４
            COMMENT_get(SQLcon, "ADDITEM4", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM4).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM4).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目５
            COMMENT_get(SQLcon, "ADDITEM5", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM5).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM5).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目６
            COMMENT_get(SQLcon, "ADDITEM6", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM6).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM6).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目７
            COMMENT_get(SQLcon, "ADDITEM7", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM7).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM7).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目８
            COMMENT_get(SQLcon, "ADDITEM8", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM8).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM8).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目９
            COMMENT_get(SQLcon, "ADDITEM9", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM9).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM9).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目１０
            COMMENT_get(SQLcon, "ADDITEM10", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM10).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM10).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目１１
            COMMENT_get(SQLcon, "ADDITEM11", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM11).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM11).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目１２
            COMMENT_get(SQLcon, "ADDITEM12", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM12).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM12).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目１３
            COMMENT_get(SQLcon, "ADDITEM13", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM13).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM13).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目１４
            COMMENT_get(SQLcon, "ADDITEM14", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM14).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM14).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目１５
            COMMENT_get(SQLcon, "ADDITEM15", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM15).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM15).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目１６
            COMMENT_get(SQLcon, "ADDITEM16", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM16).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM16).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目１７
            COMMENT_get(SQLcon, "ADDITEM17", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM17).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM17).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目１８
            COMMENT_get(SQLcon, "ADDITEM18", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM18).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM18).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目１９
            COMMENT_get(SQLcon, "ADDITEM19", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM19).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM19).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目２０
            COMMENT_get(SQLcon, "ADDITEM20", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM20).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM20).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目２１
            COMMENT_get(SQLcon, "ADDITEM21", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM21).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM21).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目２２
            COMMENT_get(SQLcon, "ADDITEM22", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM22).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM22).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目２３
            COMMENT_get(SQLcon, "ADDITEM23", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM23).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM23).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目２４
            COMMENT_get(SQLcon, "ADDITEM24", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM24).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM24).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目２５
            COMMENT_get(SQLcon, "ADDITEM25", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM25).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM25).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目２５
            COMMENT_get(SQLcon, "ADDITEM26", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM26).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM26).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目２７
            COMMENT_get(SQLcon, "ADDITEM27", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM27).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM27).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目２８
            COMMENT_get(SQLcon, "ADDITEM28", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM28).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM28).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目２９
            COMMENT_get(SQLcon, "ADDITEM29", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM29).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM29).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目３０
            COMMENT_get(SQLcon, "ADDITEM30", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM30).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM30).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目３１
            COMMENT_get(SQLcon, "ADDITEM31", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM31).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM31).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目３２
            COMMENT_get(SQLcon, "ADDITEM32", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM32).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM32).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目３３
            COMMENT_get(SQLcon, "ADDITEM33", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM33).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM33).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目３４
            COMMENT_get(SQLcon, "ADDITEM34", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM34).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM34).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目３５
            COMMENT_get(SQLcon, "ADDITEM35", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM35).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM35).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目３６
            COMMENT_get(SQLcon, "ADDITEM36", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM36).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM36).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目３７
            COMMENT_get(SQLcon, "ADDITEM37", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM37).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM37).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目３８
            COMMENT_get(SQLcon, "ADDITEM38", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM38).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM38).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目３９
            COMMENT_get(SQLcon, "ADDITEM39", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM39).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM39).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目４０
            COMMENT_get(SQLcon, "ADDITEM40", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM40).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM40).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目４１
            COMMENT_get(SQLcon, "ADDITEM41", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM41).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM41).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目４２
            COMMENT_get(SQLcon, "ADDITEM42", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM42).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM42).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目４３
            COMMENT_get(SQLcon, "ADDITEM43", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM43).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM43).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目４４
            COMMENT_get(SQLcon, "ADDITEM44", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM44).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM44).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目４５
            COMMENT_get(SQLcon, "ADDITEM45", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM45).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM45).Comment.Shape
                    .Width = 120
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目４６
            COMMENT_get(SQLcon, "ADDITEM46", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM46).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM46).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目４７
            COMMENT_get(SQLcon, "ADDITEM47", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM47).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM47).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目４８
            COMMENT_get(SQLcon, "ADDITEM48", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM48).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM48).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目４９
            COMMENT_get(SQLcon, "ADDITEM49", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM49).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM49).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '付帯項目５０
            COMMENT_get(SQLcon, "ADDITEM50", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM50).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM50).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If
            '床材質コード
            COMMENT_get(SQLcon, "FLOORMATERIAL", WW_TEXT, WW_CNT)
            If Not WW_CNT = 0 Then
                sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.FLOORMATERIAL).AddComment(WW_TEXT)
                With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.FLOORMATERIAL).Comment.Shape
                    .Width = 100
                    .Height = CONST_HEIGHT_PER_ROW * WW_CNT
                End With
            End If

            '○ コメントに表示が難しいデータは別シートに作成
            WW_TEXT = "シート:大中小分類一覧参照"
            SETSUBSHEET(wb, "CTNCD")
            '大分類コード
            sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.BIGCTNCD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.BIGCTNCD).Comment.Shape
                .Width = 150
                .Height = 30
            End With
            '中分類コード
            sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.MIDDLECTNCD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Comment.Shape
                .Width = 150
                .Height = 30
            End With
            '選択比較項目-小分類コード
            sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SMALLCTNCD).AddComment(WW_TEXT)
            With sheet.Cells(WW_HEADERROW, LNM0002WRKINC.INOUTEXCELCOL.SMALLCTNCD).Comment.Shape
                .Width = 150
                .Height = 30
            End With

        End Using

    End Sub

    ''' <summary>
    ''' 明細設定
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub SetDETAIL(ByVal sheet As IWorksheet, ByRef WW_ACTIVEROW As Integer)


        For Each Row As DataRow In LNM0002tbl.Rows
            '値
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.DELFLG).Value = Row("DELFLG") '削除フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.CTNTYPE).Value = Row("CTNTYPE") 'コンテナ記号
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.CTNNO).Value = Row("CTNNO") 'コンテナ番号
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.JURISDICTIONCD).Value = Row("JURISDICTIONCD") '所管部コード
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ACCOUNTINGASSETSCD).Value = Row("ACCOUNTINGASSETSCD") '経理資産コード
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ACCOUNTINGASSETSKBN).Value = Row("ACCOUNTINGASSETSKBN") '経理資産区分
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.DUMMYKBN).Value = Row("DUMMYKBN") 'ダミー区分
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.SPOTKBN).Value = Row("SPOTKBN") 'スポット区分
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.SPOTSTYMD).Value = Row("SPOTSTYMD") 'スポット区分　開始年月日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.SPOTENDYMD).Value = Row("SPOTENDYMD") 'スポット区分　終了年月日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.BIGCTNCD).Value = Row("BIGCTNCD") '大分類コード
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.MIDDLECTNCD).Value = Row("MIDDLECTNCD") '中分類コード
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.SMALLCTNCD).Value = Row("SMALLCTNCD") '小分類コード
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.CONSTRUCTIONYM).Value = Row("CONSTRUCTIONYM") '建造年月
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.CTNMAKER).Value = Row("CTNMAKER") 'コンテナメーカー
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.FROZENMAKER).Value = Row("FROZENMAKER") '冷凍機メーカー
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.GROSSWEIGHT).Value = Row("GROSSWEIGHT") '総重量
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.CARGOWEIGHT).Value = Row("CARGOWEIGHT") '荷重
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.MYWEIGHT).Value = Row("MYWEIGHT") '自重
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.BOOKVALUE).Value = Row("BOOKVALUE") '簿価商品価格
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.OUTHEIGHT).Value = Row("OUTHEIGHT") '外寸・高さ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.OUTWIDTH).Value = Row("OUTWIDTH") '外寸・幅
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.OUTLENGTH).Value = Row("OUTLENGTH") '外寸・長さ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.INHEIGHT).Value = Row("INHEIGHT") '内寸・高さ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.INWIDTH).Value = Row("INWIDTH") '内寸・幅
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.INLENGTH).Value = Row("INLENGTH") '内寸・長さ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.WIFEHEIGHT).Value = Row("WIFEHEIGHT") '妻入口・高さ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.WIFEWIDTH).Value = Row("WIFEWIDTH") '妻入口・幅
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.SIDEHEIGHT).Value = Row("SIDEHEIGHT") '側入口・高さ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.SIDEWIDTH).Value = Row("SIDEWIDTH") '側入口・幅
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.FLOORAREA).Value = Row("FLOORAREA") '床面積
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.INVOLUMEMARKING).Value = Row("INVOLUMEMARKING") '内容積・標記
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.INVOLUMEACTUA).Value = Row("INVOLUMEACTUA") '内容積・実寸
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.TRAINSCYCLEDAYS).Value = Row("TRAINSCYCLEDAYS") '交番検査・ｻｲｸﾙ日数
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.TRAINSBEFORERUNYMD).Value = Row("TRAINSBEFORERUNYMD") '交番検査・前回実施日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.TRAINSNEXTRUNYMD).Value = Row("TRAINSNEXTRUNYMD") '交番検査・次回実施日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSCYCLEDAYS).Value = Row("REGINSCYCLEDAYS") '定期検査・ｻｲｸﾙ月数
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSCYCLEHOURMETER).Value = Row("REGINSCYCLEHOURMETER") '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSBEFORERUNYMD).Value = Row("REGINSBEFORERUNYMD") '定期検査・前回実施日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSNEXTRUNYMD).Value = Row("REGINSNEXTRUNYMD") '定期検査・次回実施日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSHOURMETERYMD).Value = Row("REGINSHOURMETERYMD") '定期検査・ｱﾜﾒｰﾀ記載日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSHOURMETERTIME).Value = Row("REGINSHOURMETERTIME") '定期検査・ｱﾜﾒｰﾀ時間
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSHOURMETERDSP).Value = Row("REGINSHOURMETERDSP") '定期検査・ｱﾜﾒｰﾀ表示桁
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.OPERATIONSTYMD).Value = Row("OPERATIONSTYMD") '運用開始年月日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.OPERATIONENDYMD).Value = Row("OPERATIONENDYMD") '運用除外年月日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.RETIRMENTYMD).Value = Row("RETIRMENTYMD") '除却年月日
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.COMPKANKBN).Value = Row("COMPKANKBN") '複合一貫区分
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.SUPPLYFLG).Value = Row("SUPPLYFLG") '調達フラグ
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM1).Value = Row("ADDITEM1") '付帯項目１
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM2).Value = Row("ADDITEM2") '付帯項目２
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM3).Value = Row("ADDITEM3") '付帯項目３
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM4).Value = Row("ADDITEM4") '付帯項目４
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM5).Value = Row("ADDITEM5") '付帯項目５
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM6).Value = Row("ADDITEM6") '付帯項目６
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM7).Value = Row("ADDITEM7") '付帯項目７
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM8).Value = Row("ADDITEM8") '付帯項目８
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM9).Value = Row("ADDITEM9") '付帯項目９
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM10).Value = Row("ADDITEM10") '付帯項目１０
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM11).Value = Row("ADDITEM11") '付帯項目１１
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM12).Value = Row("ADDITEM12") '付帯項目１２
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM13).Value = Row("ADDITEM13") '付帯項目１３
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM14).Value = Row("ADDITEM14") '付帯項目１４
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM15).Value = Row("ADDITEM15") '付帯項目１５
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM16).Value = Row("ADDITEM16") '付帯項目１６
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM17).Value = Row("ADDITEM17") '付帯項目１７
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM18).Value = Row("ADDITEM18") '付帯項目１８
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM19).Value = Row("ADDITEM19") '付帯項目１９
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM20).Value = Row("ADDITEM20") '付帯項目２０
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM21).Value = Row("ADDITEM21") '付帯項目２１
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM22).Value = Row("ADDITEM22") '付帯項目２２
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM23).Value = Row("ADDITEM23") '付帯項目２３
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM24).Value = Row("ADDITEM24") '付帯項目２４
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM25).Value = Row("ADDITEM25") '付帯項目２５
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM26).Value = Row("ADDITEM26") '付帯項目２５
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM27).Value = Row("ADDITEM27") '付帯項目２７
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM28).Value = Row("ADDITEM28") '付帯項目２８
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM29).Value = Row("ADDITEM29") '付帯項目２９
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM30).Value = Row("ADDITEM30") '付帯項目３０
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM31).Value = Row("ADDITEM31") '付帯項目３１
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM32).Value = Row("ADDITEM32") '付帯項目３２
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM33).Value = Row("ADDITEM33") '付帯項目３３
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM34).Value = Row("ADDITEM34") '付帯項目３４
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM35).Value = Row("ADDITEM35") '付帯項目３５
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM36).Value = Row("ADDITEM36") '付帯項目３６
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM37).Value = Row("ADDITEM37") '付帯項目３７
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM38).Value = Row("ADDITEM38") '付帯項目３８
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM39).Value = Row("ADDITEM39") '付帯項目３９
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM40).Value = Row("ADDITEM40") '付帯項目４０
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM41).Value = Row("ADDITEM41") '付帯項目４１
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM42).Value = Row("ADDITEM42") '付帯項目４２
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM43).Value = Row("ADDITEM43") '付帯項目４３
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM44).Value = Row("ADDITEM44") '付帯項目４４
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM45).Value = Row("ADDITEM45") '付帯項目４５
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM46).Value = Row("ADDITEM46") '付帯項目４６
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM47).Value = Row("ADDITEM47") '付帯項目４７
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM48).Value = Row("ADDITEM48") '付帯項目４８
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM49).Value = Row("ADDITEM49") '付帯項目４９
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM50).Value = Row("ADDITEM50") '付帯項目５０
            sheet.Cells(WW_ACTIVEROW, LNM0002WRKINC.INOUTEXCELCOL.FLOORMATERIAL).Value = Row("FLOORMATERIAL") '床材質コード

            WW_ACTIVEROW += 1
        Next
    End Sub

    Public Sub SETSUBSHEET(ByVal wb As Workbook, ByVal I_FIELD As String)
        'メインシートを取得
        Dim mainsheet As IWorksheet = wb.ActiveSheet
        'サブシートを作成
        Dim subsheet As IWorksheet = wb.Worksheets.Add()
        subsheet.FreezePanes(1, 0)
        subsheet.TabColor = ColorTranslator.FromHtml(CONST_COLOR_GRAY)

        Dim WW_PrmData As New Hashtable
        Dim WW_PrmDataList = New StringBuilder
        Dim WW_DUMMY As String = ""
        Dim WW_VALUE As String = ""
        Dim WW_ROW As Integer = 0

        With leftview
            '○入力リスト取得
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open()       'DataBase接続

                Select Case I_FIELD
                    Case "CTNCD"
                        subsheet.Name = "大中小分類一覧"
                        SETCTNCDLIST(SQLcon, subsheet)
                End Select

            End Using
        End With

        'サブシートの列幅自動調整
        subsheet.Cells.EntireColumn.AutoFit()

        'メインシートをアクティブにする
        mainsheet.Activate()

    End Sub

    ''' <summary>
    ''' 入力一覧作成(大中小分類一覧)
    ''' </summary>
    Protected Sub SETCTNCDLIST(ByVal SQLcon As MySqlConnection,
                                   ByVal WW_SHEET As IWorksheet)

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT ")
        SQLStr.AppendLine("    RTRIM(BIGCTNCD) AS BIGCTNCD ")
        SQLStr.AppendLine("   ,RTRIM(MIDDLECTNCD) AS MIDDLECTNCD ")
        SQLStr.AppendLine("   ,RTRIM(SMALLCTNCD) AS SMALLCTNCD ")
        SQLStr.AppendLine("   ,RTRIM(KANJI1) AS BIGCTNNAME ")
        SQLStr.AppendLine("   ,RTRIM(KANJI2) AS MIDDLECTNNAME ")
        SQLStr.AppendLine("   ,RTRIM(KANJI3) AS SMALLCTNNAME ")
        SQLStr.AppendLine(" FROM LNG.LNM0022_CLASS ")
        SQLStr.AppendLine(" WHERE DELFLG <> @DELFLG ")
        SQLStr.AppendLine(" ORDER BY")
        SQLStr.AppendLine("      BIGCTNCD")
        SQLStr.AppendLine("     ,MIDDLECTNCD")
        SQLStr.AppendLine("     ,SMALLCTNCD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)

                '削除フラグ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)
                P_DELFLG.Value = C_DELETE_FLG.DELETE

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim WW_Tbl = New DataTable
                    Dim WW_ROW As Integer = 0
                    Dim prmDataList = New StringBuilder
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    WW_Tbl.Load(SQLdr)

                    If WW_Tbl.Rows.Count > 0 Then
                        WW_SHEET.Cells(WW_ROW, 0).Value = "大分類コード" '1列目
                        WW_SHEET.Cells(WW_ROW, 1).Value = "中分類コード" '2列目
                        WW_SHEET.Cells(WW_ROW, 2).Value = "小分類コード" '3列目
                        WW_SHEET.Cells(WW_ROW, 3).Value = "大分類名称" '4列目
                        WW_SHEET.Cells(WW_ROW, 4).Value = "中分類名称" '5列目
                        WW_SHEET.Cells(WW_ROW, 5).Value = "小分類名称" '6列目
                        WW_ROW += 1
                        For Each Row As DataRow In WW_Tbl.Rows
                            WW_SHEET.Cells(WW_ROW, 0).Value = Row("BIGCTNCD") '1列目
                            WW_SHEET.Cells(WW_ROW, 1).Value = Row("MIDDLECTNCD") '2列目
                            WW_SHEET.Cells(WW_ROW, 2).Value = Row("SMALLCTNCD") '3列目
                            WW_SHEET.Cells(WW_ROW, 3).Value = Row("BIGCTNNAME") '4列目
                            WW_SHEET.Cells(WW_ROW, 4).Value = Row("MIDDLECTNNAME") '5列目
                            WW_SHEET.Cells(WW_ROW, 5).Value = Row("SMALLCTNNAME") '6列目

                            WW_ROW += 1
                        Next
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0022_CLASS SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0022_CLASS Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
        End Try
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
                'Case "BIGCTNCD"                   '大分類コード
                '    WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                '    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_CLASS
                'Case "MIDDLECTNCD"                '中分類コード
                '    WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS)
                '    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_CLASS
                'Case "SMALLCTNCD"                 '小分類コード
                '    WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.SMALL_CLASS)
                '    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_CLASS
                Case "JURISDICTIONCD",            '所管部コード
                     "ACCOUNTINGASSETSCD",        '経理資産コード
                     "ACCOUNTINGASSETSKBN",       '経理資産区分
                     "DUMMYKBN",                  'ダミー区分
                     "SPOTKBN",                   'スポット区分
                     "CTNMAKER",                　'コンテナメーカー
                     "FROZENMAKER",               '冷凍機メーカー
                     "COMPKANKBN",                '複合一貫区分
                     "ADDITEM1",                  '付帯項目１
                     "ADDITEM2",                  '付帯項目２
                     "ADDITEM3",                  '付帯項目３
                     "ADDITEM4",                  '付帯項目４
                     "ADDITEM5",                  '付帯項目５
                     "ADDITEM6",                  '付帯項目６
                     "ADDITEM7",                  '付帯項目７
                     "ADDITEM8",                  '付帯項目８
                     "ADDITEM9",                  '付帯項目９
                     "ADDITEM10",                 '付帯項目１０
                     "ADDITEM11",                 '付帯項目１１
                     "ADDITEM12",                 '付帯項目１２
                     "ADDITEM13",                 '付帯項目１３
                     "ADDITEM14",                 '付帯項目１４
                     "ADDITEM15",                 '付帯項目１５
                     "ADDITEM16",                 '付帯項目１６
                     "ADDITEM17",                 '付帯項目１７
                     "ADDITEM18",                 '付帯項目１８
                     "ADDITEM19",                 '付帯項目１９
                     "ADDITEM20",                 '付帯項目２０
                     "ADDITEM21",                 '付帯項目２１
                     "ADDITEM22",                 '付帯項目２２
                     "ADDITEM23",                 '付帯項目２３
                     "ADDITEM24",                 '付帯項目２４
                     "ADDITEM25",                 '付帯項目２５
                     "ADDITEM26",                 '付帯項目２６
                     "ADDITEM27",                 '付帯項目２７
                     "ADDITEM28",                 '付帯項目２８
                     "ADDITEM29",                 '付帯項目２９
                     "ADDITEM30",                 '付帯項目３０
                     "ADDITEM31",                 '付帯項目３１
                     "ADDITEM32",                 '付帯項目３２
                     "ADDITEM33",                 '付帯項目３３
                     "ADDITEM34",                 '付帯項目３４
                     "ADDITEM35",                 '付帯項目３５
                     "ADDITEM36",                 '付帯項目３６
                     "ADDITEM37",                 '付帯項目３７
                     "ADDITEM38",                 '付帯項目３８
                     "ADDITEM39",                 '付帯項目３９
                     "ADDITEM40",                 '付帯項目４０
                     "ADDITEM41",                 '付帯項目４１
                     "ADDITEM42",                 '付帯項目４２
                     "ADDITEM43",                 '付帯項目４３
                     "ADDITEM44",                 '付帯項目４４
                     "ADDITEM45",                 '付帯項目４５
                     "ADDITEM46",                 '付帯項目４６
                     "ADDITEM47",                 '付帯項目４７
                     "ADDITEM48",                 '付帯項目４８
                     "ADDITEM49",                 '付帯項目４９
                     "ADDITEM50",                 '付帯項目５０
                     "FLOORMATERIAL",             '床材質コード
                     "SUPPLYFLG"                  '調達フラグ

                    WW_PrmData = work.CreateFIXParam(Master.USERCAMP, I_FIELD)
                    WW_VALUE = LIST_BOX_CLASSIFICATION.LC_FIX_VALUE

                Case "DELFLG"            '削除フラグ
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
#End Region

#Region "ｱｯﾌﾟﾛｰﾄﾞ"
    ''' <summary>
    ''' デバッグ
    ''' </summary>
    Protected Sub WF_ButtonDEBUG_Click()
        Dim filePath As String
        filePath = "D:\コンテナマスタ一括アップロードテスト.xlsx"

        Dim DATENOW As DateTime
        Dim WW_ErrData As Boolean = False

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()       'DataBase接続
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("以下のデータ変換に失敗したためアップロードを中断しました。")
            SetExceltbl(SQLcon, filePath, WW_ERR_SW)
            If WW_ERR_SW = "ERR" Then
                WF_RightboxOpen.Value = "Open"
                Exit Sub
            End If

            DATENOW = Date.Now
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("以下のデータが登録されませんでした。")
            '空白の際の初期値取得
            Dim strTRAINSCYCLEDAYS As String = ""     '交番検査・ｻｲｸﾙ日数
            Dim strREGINSHOURMETERDSP As String = ""  '定期検査・ｱﾜﾒｰﾀ表示桁
            Master.GetFirstValue(Master.USERCAMP, "TRAINSCYCLEDAYS", strTRAINSCYCLEDAYS)        '交番検査・ｻｲｸﾙ日数
            Master.GetFirstValue(Master.USERCAMP, "REGINSHOURMETERDSP", strREGINSHOURMETERDSP)  '定期検査・ｱﾜﾒｰﾀ表示桁

            For Each Row As DataRow In LNM0002Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    INPTableCheck(SQLcon, Row, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        WW_ErrData = True
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    RECONMEXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '登録、更新する
                    InsUpdExcelData(SQLcon, Row, DATENOW, strTRAINSCYCLEDAYS, strREGINSHOURMETERDSP)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    Dim zaikokbn As String = LNM0002WRKINC.GetZaikoUpdateHantei(Row)
                    '在庫更新判定処理
                    If zaikokbn <> "" Then
                        ' 在庫更新 
                        UpdateZaiko(SQLcon, Row, zaikokbn, DATENOW)
                    End If

                    '履歴登録(新規・変更後)
                    InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                End If
            Next

            'エラーデータが存在した場合Rightboxを表示する
            If WW_ErrData = True Then
                WF_RightboxOpen.Value = "Open"
            Else
                rightview.InitMemoErrList(WW_Dummy)
            End If

            '更新完了メッセージを表示
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)

        End Using
    End Sub

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
            Master.Output(C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR, C_MESSAGE_TYPE.WAR, "コンテナマスタの更新権限がありません")
            Exit Sub
        End If

        'エクセルデータ格納用テーブルの初期化
        If IsNothing(LNM0002Exceltbl) Then
            LNM0002Exceltbl = New DataTable
        End If
        If LNM0002Exceltbl.Columns.Count <> 0 Then
            LNM0002Exceltbl.Columns.Clear()
        End If
        LNM0002Exceltbl.Clear()

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
        Dim fileUploadPath As String = CS0050SESSION.UPLOAD_PATH & "\UPLOAD\RECONMEXCEL"
        Dim di As System.IO.DirectoryInfo = System.IO.Directory.CreateDirectory(fileUploadPath)
        Dim dir = New System.IO.DirectoryInfo(fileUploadPath)
        Dim files As IEnumerable(Of System.IO.FileInfo) = dir.EnumerateFiles("*", System.IO.SearchOption.AllDirectories)
        For Each file As System.IO.FileInfo In files
            IO.File.Delete(fileUploadPath & "\" & file.Name)
        Next

        'ファイル名先頭
        Dim fileNameHead As String = "RECONMEXCEL_TMP_"

        'ファイルパスの決定
        Dim newfileName As String = fileNameHead & DateTime.Now.ToString("yyyyMMddHHmmss") & "." & fileExtention
        Dim filePath As String = fileUploadPath & "\" & newfileName
        'ファイルの保存
        WF_UPLOAD_BTN.SaveAs(filePath)

        Dim DATENOW As DateTime
        Dim WW_ErrData As Boolean = False

        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection

            SQLcon.Open()       'DataBase接続
            'Excelデータ格納用テーブルに格納する
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("以下のデータ変換に失敗したためアップロードを中断しました。")
            SetExceltbl(SQLcon, filePath, WW_ERR_SW)
            If WW_ERR_SW = "ERR" Then
                WF_RightboxOpen.Value = "Open"
                Exit Sub
            End If

            DATENOW = Date.Now
            rightview.InitMemoErrList(WW_Dummy)
            rightview.AddErrorReport("以下のデータが登録されませんでした。")
            '空白の際の初期値取得
            Dim strTRAINSCYCLEDAYS As String = ""     '交番検査・ｻｲｸﾙ日数
            Dim strREGINSHOURMETERDSP As String = ""  '定期検査・ｱﾜﾒｰﾀ表示桁
            Master.GetFirstValue(Master.USERCAMP, "TRAINSCYCLEDAYS", strTRAINSCYCLEDAYS)        '交番検査・ｻｲｸﾙ日数
            Master.GetFirstValue(Master.USERCAMP, "REGINSHOURMETERDSP", strREGINSHOURMETERDSP)  '定期検査・ｱﾜﾒｰﾀ表示桁

            '件数初期化
            Dim WW_UplInsCnt As Integer = 0                             'アップロード件数(登録)
            Dim WW_UplUpdCnt As Integer = 0                             'アップロード件数(更新)
            Dim WW_UplDelCnt As Integer = 0                             'アップロード件数(削除)
            Dim WW_UplErrCnt As Integer = 0                             'アップロード件数(エラー)
            Dim WW_UplUnnecessaryCnt As Integer = 0                     'アップロード件数(更新不要)

            For Each Row As DataRow In LNM0002Exceltbl.Rows
                'テーブルに同一データが存在しない場合
                If Not SameDataChk(SQLcon, Row) = False Then
                    '項目チェック
                    INPTableCheck(SQLcon, Row, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        WW_ErrData = True
                        WW_UplErrCnt += 1
                        Continue For
                    End If

                    Dim WW_MODIFYKBN As String = ""
                    Dim WW_BEFDELFLG As String = ""

                    '変更チェック
                    RECONMEXISTS(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    '変更がある場合履歴テーブルに変更前データを登録
                    If WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.BEFDATA Then
                        '履歴登録(変更前)
                        InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                        If Not isNormal(WW_ERR_SW) Then
                            Exit Sub
                        End If
                        '登録後変更区分を変更後にする
                        WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.AFTDATA
                    End If

                    '件数カウント
                    Select Case True
                        Case Row("DELFLG") = "1" '削除の場合
                            WW_UplDelCnt += 1
                        Case WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.NEWDATA '新規の場合
                            WW_UplInsCnt += 1
                        Case Else
                            WW_UplUpdCnt += 1
                    End Select

                    '登録、更新する
                    InsUpdExcelData(SQLcon, Row, DATENOW, strTRAINSCYCLEDAYS, strREGINSHOURMETERDSP)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If

                    Dim zaikokbn As String = LNM0002WRKINC.GetZaikoUpdateHantei(Row)
                    '在庫更新判定処理
                    If zaikokbn <> "" Then
                        ' 在庫更新 
                        UpdateZaiko(SQLcon, Row, zaikokbn, DATENOW)
                    End If

                    '履歴登録(新規・変更後)
                    InsertHist(SQLcon, Row, WW_BEFDELFLG, WW_MODIFYKBN, DATENOW, WW_ERR_SW)
                    If Not isNormal(WW_ERR_SW) Then
                        Exit Sub
                    End If
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
            Else
                rightview.InitMemoErrList(WW_Dummy)
                rightview.AddErrorReport(WW_OutPutCount)
            End If

            'Rightboxを表示する
            WF_RightboxOpen.Value = "Open"

            '更新完了メッセージを表示
            Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF, needsPopUp:=True)

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
        SQLStr.AppendLine(" Select TOP 0")
        SQLStr.AppendLine("   0   As LINECNT ")
        SQLStr.AppendLine("        , CTNTYPE  ")
        SQLStr.AppendLine("        , CTNNO  ")
        SQLStr.AppendLine("        , JURISDICTIONCD  ")
        SQLStr.AppendLine("        , ACCOUNTINGASSETSCD  ")
        SQLStr.AppendLine("        , ACCOUNTINGASSETSKBN  ")
        SQLStr.AppendLine("        , DUMMYKBN  ")
        SQLStr.AppendLine("        , SPOTKBN  ")
        SQLStr.AppendLine("        , SPOTSTYMD  ")
        SQLStr.AppendLine("        , SPOTENDYMD  ")
        SQLStr.AppendLine("        , BIGCTNCD  ")
        SQLStr.AppendLine("        , MIDDLECTNCD  ")
        SQLStr.AppendLine("        , SMALLCTNCD  ")
        SQLStr.AppendLine("        , CONSTRUCTIONYM  ")
        SQLStr.AppendLine("        , CTNMAKER  ")
        SQLStr.AppendLine("        , FROZENMAKER  ")
        SQLStr.AppendLine("        , GROSSWEIGHT  ")
        SQLStr.AppendLine("        , CARGOWEIGHT  ")
        SQLStr.AppendLine("        , MYWEIGHT  ")
        SQLStr.AppendLine("        , BOOKVALUE  ")
        SQLStr.AppendLine("        , OUTHEIGHT  ")
        SQLStr.AppendLine("        , OUTWIDTH  ")
        SQLStr.AppendLine("        , OUTLENGTH  ")
        SQLStr.AppendLine("        , INHEIGHT  ")
        SQLStr.AppendLine("        , INWIDTH  ")
        SQLStr.AppendLine("        , INLENGTH  ")
        SQLStr.AppendLine("        , WIFEHEIGHT  ")
        SQLStr.AppendLine("        , WIFEWIDTH  ")
        SQLStr.AppendLine("        , SIDEHEIGHT  ")
        SQLStr.AppendLine("        , SIDEWIDTH  ")
        SQLStr.AppendLine("        , FLOORAREA  ")
        SQLStr.AppendLine("        , INVOLUMEMARKING  ")
        SQLStr.AppendLine("        , INVOLUMEACTUA  ")
        SQLStr.AppendLine("        , TRAINSCYCLEDAYS  ")
        SQLStr.AppendLine("        , TRAINSBEFORERUNYMD  ")
        SQLStr.AppendLine("        , TRAINSNEXTRUNYMD  ")
        SQLStr.AppendLine("        , REGINSCYCLEDAYS  ")
        SQLStr.AppendLine("        , REGINSCYCLEHOURMETER  ")
        SQLStr.AppendLine("        , REGINSBEFORERUNYMD  ")
        SQLStr.AppendLine("        , REGINSNEXTRUNYMD  ")
        SQLStr.AppendLine("        , REGINSHOURMETERYMD  ")
        SQLStr.AppendLine("        , REGINSHOURMETERTIME  ")
        SQLStr.AppendLine("        , REGINSHOURMETERDSP  ")
        SQLStr.AppendLine("        , OPERATIONSTYMD  ")
        SQLStr.AppendLine("        , OPERATIONENDYMD  ")
        SQLStr.AppendLine("        , RETIRMENTYMD  ")
        SQLStr.AppendLine("        , COMPKANKBN  ")
        SQLStr.AppendLine("        , SUPPLYFLG  ")
        SQLStr.AppendLine("        , ADDITEM1  ")
        SQLStr.AppendLine("        , ADDITEM2  ")
        SQLStr.AppendLine("        , ADDITEM3  ")
        SQLStr.AppendLine("        , ADDITEM4  ")
        SQLStr.AppendLine("        , ADDITEM5  ")
        SQLStr.AppendLine("        , ADDITEM6  ")
        SQLStr.AppendLine("        , ADDITEM7  ")
        SQLStr.AppendLine("        , ADDITEM8  ")
        SQLStr.AppendLine("        , ADDITEM9  ")
        SQLStr.AppendLine("        , ADDITEM10  ")
        SQLStr.AppendLine("        , ADDITEM11  ")
        SQLStr.AppendLine("        , ADDITEM12  ")
        SQLStr.AppendLine("        , ADDITEM13  ")
        SQLStr.AppendLine("        , ADDITEM14  ")
        SQLStr.AppendLine("        , ADDITEM15  ")
        SQLStr.AppendLine("        , ADDITEM16  ")
        SQLStr.AppendLine("        , ADDITEM17  ")
        SQLStr.AppendLine("        , ADDITEM18  ")
        SQLStr.AppendLine("        , ADDITEM19  ")
        SQLStr.AppendLine("        , ADDITEM20  ")
        SQLStr.AppendLine("        , ADDITEM21  ")
        SQLStr.AppendLine("        , ADDITEM22  ")
        SQLStr.AppendLine("        , ADDITEM23  ")
        SQLStr.AppendLine("        , ADDITEM24  ")
        SQLStr.AppendLine("        , ADDITEM25  ")
        SQLStr.AppendLine("        , ADDITEM26  ")
        SQLStr.AppendLine("        , ADDITEM27  ")
        SQLStr.AppendLine("        , ADDITEM28  ")
        SQLStr.AppendLine("        , ADDITEM29  ")
        SQLStr.AppendLine("        , ADDITEM30  ")
        SQLStr.AppendLine("        , ADDITEM31  ")
        SQLStr.AppendLine("        , ADDITEM32  ")
        SQLStr.AppendLine("        , ADDITEM33  ")
        SQLStr.AppendLine("        , ADDITEM34  ")
        SQLStr.AppendLine("        , ADDITEM35  ")
        SQLStr.AppendLine("        , ADDITEM36  ")
        SQLStr.AppendLine("        , ADDITEM37  ")
        SQLStr.AppendLine("        , ADDITEM38  ")
        SQLStr.AppendLine("        , ADDITEM39  ")
        SQLStr.AppendLine("        , ADDITEM40  ")
        SQLStr.AppendLine("        , ADDITEM41  ")
        SQLStr.AppendLine("        , ADDITEM42  ")
        SQLStr.AppendLine("        , ADDITEM43  ")
        SQLStr.AppendLine("        , ADDITEM44  ")
        SQLStr.AppendLine("        , ADDITEM45  ")
        SQLStr.AppendLine("        , ADDITEM46  ")
        SQLStr.AppendLine("        , ADDITEM47  ")
        SQLStr.AppendLine("        , ADDITEM48  ")
        SQLStr.AppendLine("        , ADDITEM49  ")
        SQLStr.AppendLine("        , ADDITEM50  ")
        SQLStr.AppendLine("        , FLOORMATERIAL  ")
        SQLStr.AppendLine("        , DELFLG  ")
        SQLStr.AppendLine(" FROM LNG.LNM0002_RECONM ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0002Exceltbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        DataTypeHT.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index).Name)
                    Next
                End Using
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0002_RECONM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB: LNM0002_RECONM Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

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

        Dim LNM0002Exceltblrow As DataRow
        Dim WW_LINECNT As Integer

        WW_LINECNT = 1

        For WW_ROW As Integer = CONST_DATA_START_ROW To WW_EXCELDATA.GetLength(0) - 1
            LNM0002Exceltblrow = LNM0002Exceltbl.NewRow

            'LINECNT
            LNM0002Exceltblrow("LINECNT") = WW_LINECNT
            WW_LINECNT = WW_LINECNT + 1

            '◆データセット
            'コンテナ記号
            If Not Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.CTNTYPE)) = "" Then
                WW_TEXT = Strings.StrConv(Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.CTNTYPE)), VbStrConv.Narrow)
            Else
                WW_TEXT = ""
            End If
            WW_DATATYPE = DataTypeHT("CTNTYPE")
            LNM0002Exceltblrow("CTNTYPE") = LNM0002WRKINC.DataConvert("コンテナ記号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'コンテナ番号
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.CTNNO))
            WW_DATATYPE = DataTypeHT("CTNNO")
            LNM0002Exceltblrow("CTNNO") = LNM0002WRKINC.DataConvert("コンテナ番号", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '所管部コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.JURISDICTIONCD))
            WW_DATATYPE = DataTypeHT("JURISDICTIONCD")
            LNM0002Exceltblrow("JURISDICTIONCD") = LNM0002WRKINC.DataConvert("所管部コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '経理資産コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ACCOUNTINGASSETSCD))
            WW_DATATYPE = DataTypeHT("ACCOUNTINGASSETSCD")
            LNM0002Exceltblrow("ACCOUNTINGASSETSCD") = LNM0002WRKINC.DataConvert("経理資産コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '経理資産区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ACCOUNTINGASSETSKBN))
            WW_DATATYPE = DataTypeHT("ACCOUNTINGASSETSKBN")
            LNM0002Exceltblrow("ACCOUNTINGASSETSKBN") = LNM0002WRKINC.DataConvert("経理資産区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'ダミー区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.DUMMYKBN))
            WW_DATATYPE = DataTypeHT("DUMMYKBN")
            LNM0002Exceltblrow("DUMMYKBN") = LNM0002WRKINC.DataConvert("ダミー区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'スポット区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.SPOTKBN))
            WW_DATATYPE = DataTypeHT("SPOTKBN")
            LNM0002Exceltblrow("SPOTKBN") = LNM0002WRKINC.DataConvert("スポット区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'スポット区分　開始年月日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.SPOTSTYMD))
            WW_DATATYPE = DataTypeHT("SPOTSTYMD")
            LNM0002Exceltblrow("SPOTSTYMD") = LNM0002WRKINC.DataConvert("スポット区分　開始年月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'スポット区分　終了年月日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.SPOTENDYMD))
            WW_DATATYPE = DataTypeHT("SPOTENDYMD")
            LNM0002Exceltblrow("SPOTENDYMD") = LNM0002WRKINC.DataConvert("スポット区分　終了年月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '大分類コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.BIGCTNCD))
            WW_DATATYPE = DataTypeHT("BIGCTNCD")
            LNM0002Exceltblrow("BIGCTNCD") = LNM0002WRKINC.DataConvert("大分類コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '中分類コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.MIDDLECTNCD))
            WW_DATATYPE = DataTypeHT("MIDDLECTNCD")
            LNM0002Exceltblrow("MIDDLECTNCD") = LNM0002WRKINC.DataConvert("中分類コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '小分類コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.SMALLCTNCD))
            WW_DATATYPE = DataTypeHT("SMALLCTNCD")
            LNM0002Exceltblrow("SMALLCTNCD") = LNM0002WRKINC.DataConvert("小分類コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '建造年月
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.CONSTRUCTIONYM))
            WW_DATATYPE = DataTypeHT("CONSTRUCTIONYM")
            LNM0002Exceltblrow("CONSTRUCTIONYM") = LNM0002WRKINC.DataConvert("建造年月", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            'コンテナメーカー
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.CTNMAKER))
            WW_DATATYPE = DataTypeHT("CTNMAKER")
            LNM0002Exceltblrow("CTNMAKER") = LNM0002WRKINC.DataConvert("コンテナメーカー", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '冷凍機メーカー
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.FROZENMAKER))
            WW_DATATYPE = DataTypeHT("FROZENMAKER")
            LNM0002Exceltblrow("FROZENMAKER") = LNM0002WRKINC.DataConvert("冷凍機メーカー", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '総重量
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.GROSSWEIGHT))
            WW_DATATYPE = DataTypeHT("GROSSWEIGHT")
            LNM0002Exceltblrow("GROSSWEIGHT") = LNM0002WRKINC.DataConvert("総重量", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '荷重
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.CARGOWEIGHT))
            WW_DATATYPE = DataTypeHT("CARGOWEIGHT")
            LNM0002Exceltblrow("CARGOWEIGHT") = LNM0002WRKINC.DataConvert("荷重", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '自重
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.MYWEIGHT))
            WW_DATATYPE = DataTypeHT("MYWEIGHT")
            LNM0002Exceltblrow("MYWEIGHT") = LNM0002WRKINC.DataConvert("自重", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '簿価商品価格
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.BOOKVALUE))
            WW_DATATYPE = DataTypeHT("BOOKVALUE")
            LNM0002Exceltblrow("BOOKVALUE") = LNM0002WRKINC.DataConvert("簿価商品価格", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '外寸・高さ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.OUTHEIGHT))
            WW_DATATYPE = DataTypeHT("OUTHEIGHT")
            LNM0002Exceltblrow("OUTHEIGHT") = LNM0002WRKINC.DataConvert("外寸・高さ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '外寸・幅
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.OUTWIDTH))
            WW_DATATYPE = DataTypeHT("OUTWIDTH")
            LNM0002Exceltblrow("OUTWIDTH") = LNM0002WRKINC.DataConvert("外寸・幅", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '外寸・長さ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.OUTLENGTH))
            WW_DATATYPE = DataTypeHT("OUTLENGTH")
            LNM0002Exceltblrow("OUTLENGTH") = LNM0002WRKINC.DataConvert("外寸・長さ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '内寸・高さ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.INHEIGHT))
            WW_DATATYPE = DataTypeHT("INHEIGHT")
            LNM0002Exceltblrow("INHEIGHT") = LNM0002WRKINC.DataConvert("内寸・高さ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '内寸・幅
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.INWIDTH))
            WW_DATATYPE = DataTypeHT("INWIDTH")
            LNM0002Exceltblrow("INWIDTH") = LNM0002WRKINC.DataConvert("内寸・幅", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '内寸・長さ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.INLENGTH))
            WW_DATATYPE = DataTypeHT("INLENGTH")
            LNM0002Exceltblrow("INLENGTH") = LNM0002WRKINC.DataConvert("内寸・長さ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '妻入口・高さ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.WIFEHEIGHT))
            WW_DATATYPE = DataTypeHT("WIFEHEIGHT")
            LNM0002Exceltblrow("WIFEHEIGHT") = LNM0002WRKINC.DataConvert("妻入口・高さ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '妻入口・幅
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.WIFEWIDTH))
            WW_DATATYPE = DataTypeHT("WIFEWIDTH")
            LNM0002Exceltblrow("WIFEWIDTH") = LNM0002WRKINC.DataConvert("妻入口・幅", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '側入口・高さ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.SIDEHEIGHT))
            WW_DATATYPE = DataTypeHT("SIDEHEIGHT")
            LNM0002Exceltblrow("SIDEHEIGHT") = LNM0002WRKINC.DataConvert("側入口・高さ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '側入口・幅
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.SIDEWIDTH))
            WW_DATATYPE = DataTypeHT("SIDEWIDTH")
            LNM0002Exceltblrow("SIDEWIDTH") = LNM0002WRKINC.DataConvert("側入口・幅", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '床面積
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.FLOORAREA))
            WW_DATATYPE = DataTypeHT("FLOORAREA")
            LNM0002Exceltblrow("FLOORAREA") = LNM0002WRKINC.DataConvert("床面積", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '内容積・標記
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.INVOLUMEMARKING))
            WW_DATATYPE = DataTypeHT("INVOLUMEMARKING")
            LNM0002Exceltblrow("INVOLUMEMARKING") = LNM0002WRKINC.DataConvert("内容積・標記", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '内容積・実寸
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.INVOLUMEACTUA))
            WW_DATATYPE = DataTypeHT("INVOLUMEACTUA")
            LNM0002Exceltblrow("INVOLUMEACTUA") = LNM0002WRKINC.DataConvert("内容積・実寸", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '交番検査・ｻｲｸﾙ日数
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.TRAINSCYCLEDAYS))
            WW_DATATYPE = DataTypeHT("TRAINSCYCLEDAYS")
            LNM0002Exceltblrow("TRAINSCYCLEDAYS") = LNM0002WRKINC.DataConvert("交番検査・ｻｲｸﾙ日数", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '交番検査・前回実施日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.TRAINSBEFORERUNYMD))
            WW_DATATYPE = DataTypeHT("TRAINSBEFORERUNYMD")
            LNM0002Exceltblrow("TRAINSBEFORERUNYMD") = LNM0002WRKINC.DataConvert("交番検査・前回実施日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '交番検査・次回実施日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.TRAINSNEXTRUNYMD))
            WW_DATATYPE = DataTypeHT("TRAINSNEXTRUNYMD")
            LNM0002Exceltblrow("TRAINSNEXTRUNYMD") = LNM0002WRKINC.DataConvert("交番検査・次回実施日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '定期検査・ｻｲｸﾙ月数
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSCYCLEDAYS))
            WW_DATATYPE = DataTypeHT("REGINSCYCLEDAYS")
            LNM0002Exceltblrow("REGINSCYCLEDAYS") = LNM0002WRKINC.DataConvert("定期検査・ｻｲｸﾙ月数", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSCYCLEHOURMETER))
            WW_DATATYPE = DataTypeHT("REGINSCYCLEHOURMETER")
            LNM0002Exceltblrow("REGINSCYCLEHOURMETER") = LNM0002WRKINC.DataConvert("定期検査・ｻｲｸﾙｱﾜﾒｰﾀ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '定期検査・前回実施日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSBEFORERUNYMD))
            WW_DATATYPE = DataTypeHT("REGINSBEFORERUNYMD")
            LNM0002Exceltblrow("REGINSBEFORERUNYMD") = LNM0002WRKINC.DataConvert("定期検査・前回実施日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '定期検査・次回実施日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSNEXTRUNYMD))
            WW_DATATYPE = DataTypeHT("REGINSNEXTRUNYMD")
            LNM0002Exceltblrow("REGINSNEXTRUNYMD") = LNM0002WRKINC.DataConvert("定期検査・次回実施日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '定期検査・ｱﾜﾒｰﾀ記載日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSHOURMETERYMD))
            WW_DATATYPE = DataTypeHT("REGINSHOURMETERYMD")
            LNM0002Exceltblrow("REGINSHOURMETERYMD") = LNM0002WRKINC.DataConvert("定期検査・ｱﾜﾒｰﾀ記載日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '定期検査・ｱﾜﾒｰﾀ時間
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSHOURMETERTIME))
            WW_DATATYPE = DataTypeHT("REGINSHOURMETERTIME")
            LNM0002Exceltblrow("REGINSHOURMETERTIME") = LNM0002WRKINC.DataConvert("定期検査・ｱﾜﾒｰﾀ時間", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '定期検査・ｱﾜﾒｰﾀ表示桁
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.REGINSHOURMETERDSP))
            WW_DATATYPE = DataTypeHT("REGINSHOURMETERDSP")
            LNM0002Exceltblrow("REGINSHOURMETERDSP") = LNM0002WRKINC.DataConvert("定期検査・ｱﾜﾒｰﾀ表示桁", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '運用開始年月日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.OPERATIONSTYMD))
            WW_DATATYPE = DataTypeHT("OPERATIONSTYMD")
            LNM0002Exceltblrow("OPERATIONSTYMD") = LNM0002WRKINC.DataConvert("運用開始年月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '運用除外年月日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.OPERATIONENDYMD))
            WW_DATATYPE = DataTypeHT("OPERATIONENDYMD")
            LNM0002Exceltblrow("OPERATIONENDYMD") = LNM0002WRKINC.DataConvert("運用除外年月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '除却年月日
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.RETIRMENTYMD))
            WW_DATATYPE = DataTypeHT("RETIRMENTYMD")
            LNM0002Exceltblrow("RETIRMENTYMD") = LNM0002WRKINC.DataConvert("除却年月日", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '複合一貫区分
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.COMPKANKBN))
            WW_DATATYPE = DataTypeHT("COMPKANKBN")
            LNM0002Exceltblrow("COMPKANKBN") = LNM0002WRKINC.DataConvert("複合一貫区分", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '調達フラグ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.SUPPLYFLG))
            WW_DATATYPE = DataTypeHT("SUPPLYFLG")
            LNM0002Exceltblrow("SUPPLYFLG") = LNM0002WRKINC.DataConvert("調達フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目１
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM1))
            WW_DATATYPE = DataTypeHT("ADDITEM1")
            LNM0002Exceltblrow("ADDITEM1") = LNM0002WRKINC.DataConvert("付帯項目１", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目２
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM2))
            WW_DATATYPE = DataTypeHT("ADDITEM2")
            LNM0002Exceltblrow("ADDITEM2") = LNM0002WRKINC.DataConvert("付帯項目２", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目３
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM3))
            WW_DATATYPE = DataTypeHT("ADDITEM3")
            LNM0002Exceltblrow("ADDITEM3") = LNM0002WRKINC.DataConvert("付帯項目３", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目４
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM4))
            WW_DATATYPE = DataTypeHT("ADDITEM4")
            LNM0002Exceltblrow("ADDITEM4") = LNM0002WRKINC.DataConvert("付帯項目４", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目５
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM5))
            WW_DATATYPE = DataTypeHT("ADDITEM5")
            LNM0002Exceltblrow("ADDITEM5") = LNM0002WRKINC.DataConvert("付帯項目５", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目６
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM6))
            WW_DATATYPE = DataTypeHT("ADDITEM6")
            LNM0002Exceltblrow("ADDITEM6") = LNM0002WRKINC.DataConvert("付帯項目６", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目７
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM7))
            WW_DATATYPE = DataTypeHT("ADDITEM7")
            LNM0002Exceltblrow("ADDITEM7") = LNM0002WRKINC.DataConvert("付帯項目７", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目８
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM8))
            WW_DATATYPE = DataTypeHT("ADDITEM8")
            LNM0002Exceltblrow("ADDITEM8") = LNM0002WRKINC.DataConvert("付帯項目８", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目９
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM9))
            WW_DATATYPE = DataTypeHT("ADDITEM9")
            LNM0002Exceltblrow("ADDITEM9") = LNM0002WRKINC.DataConvert("付帯項目９", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目１０
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM10))
            WW_DATATYPE = DataTypeHT("ADDITEM10")
            LNM0002Exceltblrow("ADDITEM10") = LNM0002WRKINC.DataConvert("付帯項目１０", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目１１
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM11))
            WW_DATATYPE = DataTypeHT("ADDITEM11")
            LNM0002Exceltblrow("ADDITEM11") = LNM0002WRKINC.DataConvert("付帯項目１１", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目１２
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM12))
            WW_DATATYPE = DataTypeHT("ADDITEM12")
            LNM0002Exceltblrow("ADDITEM12") = LNM0002WRKINC.DataConvert("付帯項目１２", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目１３
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM13))
            WW_DATATYPE = DataTypeHT("ADDITEM13")
            LNM0002Exceltblrow("ADDITEM13") = LNM0002WRKINC.DataConvert("付帯項目１３", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目１４
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM14))
            WW_DATATYPE = DataTypeHT("ADDITEM14")
            LNM0002Exceltblrow("ADDITEM14") = LNM0002WRKINC.DataConvert("付帯項目１４", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目１５
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM15))
            WW_DATATYPE = DataTypeHT("ADDITEM15")
            LNM0002Exceltblrow("ADDITEM15") = LNM0002WRKINC.DataConvert("付帯項目１５", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目１６
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM16))
            WW_DATATYPE = DataTypeHT("ADDITEM16")
            LNM0002Exceltblrow("ADDITEM16") = LNM0002WRKINC.DataConvert("付帯項目１６", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目１７
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM17))
            WW_DATATYPE = DataTypeHT("ADDITEM17")
            LNM0002Exceltblrow("ADDITEM17") = LNM0002WRKINC.DataConvert("付帯項目１７", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目１８
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM18))
            WW_DATATYPE = DataTypeHT("ADDITEM18")
            LNM0002Exceltblrow("ADDITEM18") = LNM0002WRKINC.DataConvert("付帯項目１８", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目１９
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM19))
            WW_DATATYPE = DataTypeHT("ADDITEM19")
            LNM0002Exceltblrow("ADDITEM19") = LNM0002WRKINC.DataConvert("付帯項目１９", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目２０
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM20))
            WW_DATATYPE = DataTypeHT("ADDITEM20")
            LNM0002Exceltblrow("ADDITEM20") = LNM0002WRKINC.DataConvert("付帯項目２０", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目２１
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM21))
            WW_DATATYPE = DataTypeHT("ADDITEM21")
            LNM0002Exceltblrow("ADDITEM21") = LNM0002WRKINC.DataConvert("付帯項目２１", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目２２
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM22))
            WW_DATATYPE = DataTypeHT("ADDITEM22")
            LNM0002Exceltblrow("ADDITEM22") = LNM0002WRKINC.DataConvert("付帯項目２２", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目２３
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM23))
            WW_DATATYPE = DataTypeHT("ADDITEM23")
            LNM0002Exceltblrow("ADDITEM23") = LNM0002WRKINC.DataConvert("付帯項目２３", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目２４
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM24))
            WW_DATATYPE = DataTypeHT("ADDITEM24")
            LNM0002Exceltblrow("ADDITEM24") = LNM0002WRKINC.DataConvert("付帯項目２４", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目２５
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM25))
            WW_DATATYPE = DataTypeHT("ADDITEM25")
            LNM0002Exceltblrow("ADDITEM25") = LNM0002WRKINC.DataConvert("付帯項目２５", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目２６
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM26))
            WW_DATATYPE = DataTypeHT("ADDITEM26")
            LNM0002Exceltblrow("ADDITEM26") = LNM0002WRKINC.DataConvert("付帯項目２６", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目２７
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM27))
            WW_DATATYPE = DataTypeHT("ADDITEM27")
            LNM0002Exceltblrow("ADDITEM27") = LNM0002WRKINC.DataConvert("付帯項目２７", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目２８
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM28))
            WW_DATATYPE = DataTypeHT("ADDITEM28")
            LNM0002Exceltblrow("ADDITEM28") = LNM0002WRKINC.DataConvert("付帯項目２８", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目２９
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM29))
            WW_DATATYPE = DataTypeHT("ADDITEM29")
            LNM0002Exceltblrow("ADDITEM29") = LNM0002WRKINC.DataConvert("付帯項目２９", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目３０
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM30))
            WW_DATATYPE = DataTypeHT("ADDITEM30")
            LNM0002Exceltblrow("ADDITEM30") = LNM0002WRKINC.DataConvert("付帯項目３０", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目３１
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM31))
            WW_DATATYPE = DataTypeHT("ADDITEM31")
            LNM0002Exceltblrow("ADDITEM31") = LNM0002WRKINC.DataConvert("付帯項目３１", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目３２
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM32))
            WW_DATATYPE = DataTypeHT("ADDITEM32")
            LNM0002Exceltblrow("ADDITEM32") = LNM0002WRKINC.DataConvert("付帯項目３２", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目３３
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM33))
            WW_DATATYPE = DataTypeHT("ADDITEM33")
            LNM0002Exceltblrow("ADDITEM33") = LNM0002WRKINC.DataConvert("付帯項目３３", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目３４
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM34))
            WW_DATATYPE = DataTypeHT("ADDITEM34")
            LNM0002Exceltblrow("ADDITEM34") = LNM0002WRKINC.DataConvert("付帯項目３４", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目３５
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM35))
            WW_DATATYPE = DataTypeHT("ADDITEM35")
            LNM0002Exceltblrow("ADDITEM35") = LNM0002WRKINC.DataConvert("付帯項目３５", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目３６
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM36))
            WW_DATATYPE = DataTypeHT("ADDITEM36")
            LNM0002Exceltblrow("ADDITEM36") = LNM0002WRKINC.DataConvert("付帯項目３６", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目３７
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM37))
            WW_DATATYPE = DataTypeHT("ADDITEM37")
            LNM0002Exceltblrow("ADDITEM37") = LNM0002WRKINC.DataConvert("付帯項目３７", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目３８
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM38))
            WW_DATATYPE = DataTypeHT("ADDITEM38")
            LNM0002Exceltblrow("ADDITEM38") = LNM0002WRKINC.DataConvert("付帯項目３８", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目３９
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM39))
            WW_DATATYPE = DataTypeHT("ADDITEM39")
            LNM0002Exceltblrow("ADDITEM39") = LNM0002WRKINC.DataConvert("付帯項目３９", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目４０
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM40))
            WW_DATATYPE = DataTypeHT("ADDITEM40")
            LNM0002Exceltblrow("ADDITEM40") = LNM0002WRKINC.DataConvert("付帯項目４０", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目４１
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM41))
            WW_DATATYPE = DataTypeHT("ADDITEM41")
            LNM0002Exceltblrow("ADDITEM41") = LNM0002WRKINC.DataConvert("付帯項目４１", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目４２
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM42))
            WW_DATATYPE = DataTypeHT("ADDITEM42")
            LNM0002Exceltblrow("ADDITEM42") = LNM0002WRKINC.DataConvert("付帯項目４２", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目４３
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM43))
            WW_DATATYPE = DataTypeHT("ADDITEM43")
            LNM0002Exceltblrow("ADDITEM43") = LNM0002WRKINC.DataConvert("付帯項目４３", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目４４
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM44))
            WW_DATATYPE = DataTypeHT("ADDITEM44")
            LNM0002Exceltblrow("ADDITEM44") = LNM0002WRKINC.DataConvert("付帯項目４４", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目４５
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM45))
            WW_DATATYPE = DataTypeHT("ADDITEM45")
            LNM0002Exceltblrow("ADDITEM45") = LNM0002WRKINC.DataConvert("付帯項目４５", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目４６
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM46))
            WW_DATATYPE = DataTypeHT("ADDITEM46")
            LNM0002Exceltblrow("ADDITEM46") = LNM0002WRKINC.DataConvert("付帯項目４６", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目４７
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM47))
            WW_DATATYPE = DataTypeHT("ADDITEM47")
            LNM0002Exceltblrow("ADDITEM47") = LNM0002WRKINC.DataConvert("付帯項目４７", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目４８
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM48))
            WW_DATATYPE = DataTypeHT("ADDITEM48")
            LNM0002Exceltblrow("ADDITEM48") = LNM0002WRKINC.DataConvert("付帯項目４８", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目４９
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM49))
            WW_DATATYPE = DataTypeHT("ADDITEM49")
            LNM0002Exceltblrow("ADDITEM49") = LNM0002WRKINC.DataConvert("付帯項目４９", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '付帯項目５０
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.ADDITEM50))
            WW_DATATYPE = DataTypeHT("ADDITEM50")
            LNM0002Exceltblrow("ADDITEM50") = LNM0002WRKINC.DataConvert("付帯項目５０", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '床材質コード
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.FLOORMATERIAL))
            WW_DATATYPE = DataTypeHT("FLOORMATERIAL")
            LNM0002Exceltblrow("FLOORMATERIAL") = LNM0002WRKINC.DataConvert("床材質コード", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If
            '削除フラグ
            WW_TEXT = Convert.ToString(WW_EXCELDATA(WW_ROW, LNM0002WRKINC.INOUTEXCELCOL.DELFLG))
            WW_DATATYPE = DataTypeHT("DELFLG")
            LNM0002Exceltblrow("DELFLG") = LNM0002WRKINC.DataConvert("削除フラグ", WW_TEXT, WW_DATATYPE, WW_RESULT, WW_CheckMES1, WW_CheckMES2)
            If WW_RESULT = False Then
                WW_CheckERR(WW_LINECNT, WW_CheckMES1, WW_CheckMES2)
                O_RTN = "ERR"
            End If

            '登録
            LNM0002Exceltbl.Rows.Add(LNM0002Exceltblrow)

        Next
    End Sub

    '' <summary>
    '' 今回アップロードしたデータと完全一致するデータがあるか確認する
    '' </summary>
    Protected Function SameDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        SameDataChk = False

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        CTNTYPE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0002_RECONM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         coalesce(CTNTYPE, '')             = @CTNTYPE ")
        SQLStr.AppendLine("    AND  coalesce(CTNNO, '0')             = @CTNNO ")
        SQLStr.AppendLine("    AND  coalesce(JURISDICTIONCD, '')             = @JURISDICTIONCD ")
        SQLStr.AppendLine("    AND  coalesce(ACCOUNTINGASSETSCD, '')             = @ACCOUNTINGASSETSCD ")
        SQLStr.AppendLine("    AND  coalesce(ACCOUNTINGASSETSKBN, '')             = @ACCOUNTINGASSETSKBN ")
        SQLStr.AppendLine("    AND  coalesce(DUMMYKBN, '')             = @DUMMYKBN ")
        SQLStr.AppendLine("    AND  coalesce(SPOTKBN, '')             = @SPOTKBN ")
        SQLStr.AppendLine("    AND  coalesce(FORMAT(SPOTSTYMD, 'yyyyMMdd'), '')   = coalesce(FORMAT(@SPOTSTYMD, 'yyyyMMdd'), '') ")
        SQLStr.AppendLine("    AND  coalesce(FORMAT(SPOTENDYMD, 'yyyyMMdd'), '')   = coalesce(FORMAT(@SPOTENDYMD, 'yyyyMMdd'), '') ")
        SQLStr.AppendLine("    AND  coalesce(BIGCTNCD, '')             = @BIGCTNCD ")
        SQLStr.AppendLine("    AND  coalesce(MIDDLECTNCD, '')             = @MIDDLECTNCD ")
        SQLStr.AppendLine("    AND  coalesce(SMALLCTNCD, '')             = @SMALLCTNCD ")
        SQLStr.AppendLine("    AND  coalesce(CONSTRUCTIONYM, '')             = @CONSTRUCTIONYM ")
        SQLStr.AppendLine("    AND  coalesce(CTNMAKER, '')             = @CTNMAKER ")
        SQLStr.AppendLine("    AND  coalesce(FROZENMAKER, '')             = @FROZENMAKER ")
        SQLStr.AppendLine("    AND  coalesce(GROSSWEIGHT, '0')             = @GROSSWEIGHT ")
        SQLStr.AppendLine("    AND  coalesce(CARGOWEIGHT, '0')             = @CARGOWEIGHT ")
        SQLStr.AppendLine("    AND  coalesce(MYWEIGHT, '0')             = @MYWEIGHT ")
        SQLStr.AppendLine("    AND  coalesce(BOOKVALUE, '')             = @BOOKVALUE ")
        SQLStr.AppendLine("    AND  coalesce(OUTHEIGHT, '0')             = @OUTHEIGHT ")
        SQLStr.AppendLine("    AND  coalesce(OUTWIDTH, '0')             = @OUTWIDTH ")
        SQLStr.AppendLine("    AND  coalesce(OUTLENGTH, '0')             = @OUTLENGTH ")
        SQLStr.AppendLine("    AND  coalesce(INHEIGHT, '0')             = @INHEIGHT ")
        SQLStr.AppendLine("    AND  coalesce(INWIDTH, '0')             = @INWIDTH ")
        SQLStr.AppendLine("    AND  coalesce(INLENGTH, '0')             = @INLENGTH ")
        SQLStr.AppendLine("    AND  coalesce(WIFEHEIGHT, '0')             = @WIFEHEIGHT ")
        SQLStr.AppendLine("    AND  coalesce(WIFEWIDTH, '0')             = @WIFEWIDTH ")
        SQLStr.AppendLine("    AND  coalesce(SIDEHEIGHT, '0')             = @SIDEHEIGHT ")
        SQLStr.AppendLine("    AND  coalesce(SIDEWIDTH, '0')             = @SIDEWIDTH ")
        SQLStr.AppendLine("    AND  coalesce(FLOORAREA, '0')             = @FLOORAREA ")
        SQLStr.AppendLine("    AND  coalesce(INVOLUMEMARKING, '0')             = @INVOLUMEMARKING ")
        SQLStr.AppendLine("    AND  coalesce(INVOLUMEACTUA, '0')             = @INVOLUMEACTUA ")
        SQLStr.AppendLine("    AND  coalesce(TRAINSCYCLEDAYS, '0')             = @TRAINSCYCLEDAYS ")
        SQLStr.AppendLine("    AND  coalesce(FORMAT(TRAINSBEFORERUNYMD, 'yyyyMMdd'), '')   = coalesce(FORMAT(@TRAINSBEFORERUNYMD, 'yyyyMMdd'), '') ")
        SQLStr.AppendLine("    AND  coalesce(FORMAT(TRAINSNEXTRUNYMD, 'yyyyMMdd'), '')   = coalesce(FORMAT(@TRAINSNEXTRUNYMD, 'yyyyMMdd'), '') ")
        SQLStr.AppendLine("    AND  coalesce(TRAINSNEXTRUNYMD, '0')             = @TRAINSNEXTRUNYMD ")
        SQLStr.AppendLine("    AND  coalesce(REGINSCYCLEDAYS, '0')             = @REGINSCYCLEDAYS ")
        SQLStr.AppendLine("    AND  coalesce(REGINSCYCLEHOURMETER, '0')             = @REGINSCYCLEHOURMETER ")
        SQLStr.AppendLine("    AND  coalesce(FORMAT(REGINSBEFORERUNYMD, 'yyyyMMdd'), '')   = coalesce(FORMAT(@REGINSBEFORERUNYMD, 'yyyyMMdd'), '') ")
        SQLStr.AppendLine("    AND  coalesce(FORMAT(REGINSNEXTRUNYMD, 'yyyyMMdd'), '')   = coalesce(FORMAT(@REGINSNEXTRUNYMD, 'yyyyMMdd'), '') ")
        SQLStr.AppendLine("    AND  coalesce(FORMAT(REGINSHOURMETERYMD, 'yyyyMMdd'), '')   = coalesce(FORMAT(@REGINSHOURMETERYMD, 'yyyyMMdd'), '') ")
        SQLStr.AppendLine("    AND  coalesce(REGINSHOURMETERTIME, '0')             = @REGINSHOURMETERTIME ")
        SQLStr.AppendLine("    AND  coalesce(REGINSHOURMETERDSP, '0')             = @REGINSHOURMETERDSP ")
        SQLStr.AppendLine("    AND  coalesce(FORMAT(OPERATIONSTYMD, 'yyyyMMdd'), '')   = coalesce(FORMAT(@OPERATIONSTYMD, 'yyyyMMdd'), '') ")
        SQLStr.AppendLine("    AND  coalesce(FORMAT(OPERATIONENDYMD, 'yyyyMMdd'), '')   = coalesce(FORMAT(@OPERATIONENDYMD, 'yyyyMMdd'), '') ")
        SQLStr.AppendLine("    AND  coalesce(FORMAT(RETIRMENTYMD, 'yyyyMMdd'), '')   = coalesce(FORMAT(@RETIRMENTYMD, 'yyyyMMdd'), '') ")
        SQLStr.AppendLine("    AND  coalesce(COMPKANKBN, '')             = @COMPKANKBN ")
        SQLStr.AppendLine("    AND  coalesce(SUPPLYFLG, '')             = @SUPPLYFLG ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM1, '')             = @ADDITEM1 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM2, '')             = @ADDITEM2 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM3, '')             = @ADDITEM3 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM4, '')             = @ADDITEM4 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM5, '')             = @ADDITEM5 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM6, '')             = @ADDITEM6 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM7, '')             = @ADDITEM7 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM8, '')             = @ADDITEM8 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM9, '')             = @ADDITEM9 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM10, '')             = @ADDITEM10 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM11, '')             = @ADDITEM11 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM12, '')             = @ADDITEM12 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM13, '')             = @ADDITEM13 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM14, '')             = @ADDITEM14 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM15, '')             = @ADDITEM15 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM16, '')             = @ADDITEM16 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM17, '')             = @ADDITEM17 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM18, '')             = @ADDITEM18 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM19, '')             = @ADDITEM19 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM20, '')             = @ADDITEM20 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM21, '')             = @ADDITEM21 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM22, '')             = @ADDITEM22 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM23, '')             = @ADDITEM23 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM24, '')             = @ADDITEM24 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM25, '')             = @ADDITEM25 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM26, '')             = @ADDITEM26 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM27, '')             = @ADDITEM27 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM28, '')             = @ADDITEM28 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM29, '')             = @ADDITEM29 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM30, '')             = @ADDITEM30 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM31, '')             = @ADDITEM31 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM32, '')             = @ADDITEM32 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM33, '')             = @ADDITEM33 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM34, '')             = @ADDITEM34 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM35, '')             = @ADDITEM35 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM36, '')             = @ADDITEM36 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM37, '')             = @ADDITEM37 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM38, '')             = @ADDITEM38 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM39, '')             = @ADDITEM39 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM40, '')             = @ADDITEM40 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM41, '')             = @ADDITEM41 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM42, '')             = @ADDITEM42 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM43, '')             = @ADDITEM43 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM44, '')             = @ADDITEM44 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM45, '')             = @ADDITEM45 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM46, '')             = @ADDITEM46 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM47, '')             = @ADDITEM47 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM48, '')             = @ADDITEM48 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM49, '')             = @ADDITEM49 ")
        SQLStr.AppendLine("    AND  coalesce(ADDITEM50, '')             = @ADDITEM50 ")
        SQLStr.AppendLine("    AND  coalesce(FLOORMATERIAL, '')             = @FLOORMATERIAL ")
        SQLStr.AppendLine("    AND  coalesce(DELFLG, '')             = @DELFLG ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_CTNTYPE As MySqlParameter = SQLcmd.Parameters.Add("@CTNTYPE", MySqlDbType.VarChar, 5)         'コンテナ記号
                Dim P_CTNNO As MySqlParameter = SQLcmd.Parameters.Add("@CTNNO", MySqlDbType.VarChar, 8)         'コンテナ番号
                Dim P_JURISDICTIONCD As MySqlParameter = SQLcmd.Parameters.Add("@JURISDICTIONCD", MySqlDbType.VarChar, 2)         '所管部コード
                Dim P_ACCOUNTINGASSETSCD As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTINGASSETSCD", MySqlDbType.VarChar, 4)         '経理資産コード
                Dim P_ACCOUNTINGASSETSKBN As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTINGASSETSKBN", MySqlDbType.VarChar, 2)         '経理資産区分
                Dim P_DUMMYKBN As MySqlParameter = SQLcmd.Parameters.Add("@DUMMYKBN", MySqlDbType.VarChar, 2)         'ダミー区分
                Dim P_SPOTKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPOTKBN", MySqlDbType.VarChar, 2)         'スポット区分
                Dim P_SPOTSTYMD As MySqlParameter = SQLcmd.Parameters.Add("@SPOTSTYMD", MySqlDbType.Date)         'スポット区分　開始年月日
                Dim P_SPOTENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@SPOTENDYMD", MySqlDbType.Date)         'スポット区分　終了年月日
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_SMALLCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCTNCD", MySqlDbType.VarChar, 2)         '小分類コード
                Dim P_CONSTRUCTIONYM As MySqlParameter = SQLcmd.Parameters.Add("@CONSTRUCTIONYM", MySqlDbType.VarChar, 6)         '建造年月
                Dim P_CTNMAKER As MySqlParameter = SQLcmd.Parameters.Add("@CTNMAKER", MySqlDbType.VarChar, 3)         'コンテナメーカー
                Dim P_FROZENMAKER As MySqlParameter = SQLcmd.Parameters.Add("@FROZENMAKER", MySqlDbType.VarChar, 3)         '冷凍機メーカー
                Dim P_GROSSWEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@GROSSWEIGHT", MySqlDbType.Decimal, 4, 1)       '総重量
                Dim P_CARGOWEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@CARGOWEIGHT", MySqlDbType.Decimal, 6, 1)       '荷重
                Dim P_MYWEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@MYWEIGHT", MySqlDbType.Decimal, 4, 1)       '自重
                Dim P_BOOKVALUE As MySqlParameter = SQLcmd.Parameters.Add("@BOOKVALUE", MySqlDbType.Decimal)         '簿価商品価格
                Dim P_OUTHEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@OUTHEIGHT", MySqlDbType.VarChar, 4)         '外寸・高さ
                Dim P_OUTWIDTH As MySqlParameter = SQLcmd.Parameters.Add("@OUTWIDTH", MySqlDbType.VarChar, 4)         '外寸・幅
                Dim P_OUTLENGTH As MySqlParameter = SQLcmd.Parameters.Add("@OUTLENGTH", MySqlDbType.VarChar, 4)         '外寸・長さ
                Dim P_INHEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@INHEIGHT", MySqlDbType.VarChar, 4)         '内寸・高さ
                Dim P_INWIDTH As MySqlParameter = SQLcmd.Parameters.Add("@INWIDTH", MySqlDbType.VarChar, 4)         '内寸・幅
                Dim P_INLENGTH As MySqlParameter = SQLcmd.Parameters.Add("@INLENGTH", MySqlDbType.VarChar, 4)         '内寸・長さ
                Dim P_WIFEHEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@WIFEHEIGHT", MySqlDbType.VarChar, 4)         '妻入口・高さ
                Dim P_WIFEWIDTH As MySqlParameter = SQLcmd.Parameters.Add("@WIFEWIDTH", MySqlDbType.VarChar, 4)         '妻入口・幅
                Dim P_SIDEHEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@SIDEHEIGHT", MySqlDbType.VarChar, 4)         '側入口・高さ
                Dim P_SIDEWIDTH As MySqlParameter = SQLcmd.Parameters.Add("@SIDEWIDTH", MySqlDbType.VarChar, 4)         '側入口・幅
                Dim P_FLOORAREA As MySqlParameter = SQLcmd.Parameters.Add("@FLOORAREA", MySqlDbType.Decimal, 6, 2)       '床面積
                Dim P_INVOLUMEMARKING As MySqlParameter = SQLcmd.Parameters.Add("@INVOLUMEMARKING", MySqlDbType.VarChar, 4)         '内容積・標記
                Dim P_INVOLUMEACTUA As MySqlParameter = SQLcmd.Parameters.Add("@INVOLUMEACTUA", MySqlDbType.Decimal, 6, 2)       '内容積・実寸
                Dim P_TRAINSCYCLEDAYS As MySqlParameter = SQLcmd.Parameters.Add("@TRAINSCYCLEDAYS", MySqlDbType.VarChar, 3)         '交番検査・ｻｲｸﾙ日数
                Dim P_TRAINSBEFORERUNYMD As MySqlParameter = SQLcmd.Parameters.Add("@TRAINSBEFORERUNYMD", MySqlDbType.Date)         '交番検査・前回実施日
                Dim P_TRAINSNEXTRUNYMD As MySqlParameter = SQLcmd.Parameters.Add("@TRAINSNEXTRUNYMD", MySqlDbType.Date)         '交番検査・次回実施日
                Dim P_REGINSCYCLEDAYS As MySqlParameter = SQLcmd.Parameters.Add("@REGINSCYCLEDAYS", MySqlDbType.VarChar, 2)         '定期検査・ｻｲｸﾙ月数
                Dim P_REGINSCYCLEHOURMETER As MySqlParameter = SQLcmd.Parameters.Add("@REGINSCYCLEHOURMETER", MySqlDbType.VarChar, 3)         '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
                Dim P_REGINSBEFORERUNYMD As MySqlParameter = SQLcmd.Parameters.Add("@REGINSBEFORERUNYMD", MySqlDbType.Date)         '定期検査・前回実施日
                Dim P_REGINSNEXTRUNYMD As MySqlParameter = SQLcmd.Parameters.Add("@REGINSNEXTRUNYMD", MySqlDbType.Date)         '定期検査・次回実施日
                Dim P_REGINSHOURMETERYMD As MySqlParameter = SQLcmd.Parameters.Add("@REGINSHOURMETERYMD", MySqlDbType.Date)         '定期検査・ｱﾜﾒｰﾀ記載日
                Dim P_REGINSHOURMETERTIME As MySqlParameter = SQLcmd.Parameters.Add("@REGINSHOURMETERTIME", MySqlDbType.VarChar, 5)         '定期検査・ｱﾜﾒｰﾀ時間
                Dim P_REGINSHOURMETERDSP As MySqlParameter = SQLcmd.Parameters.Add("@REGINSHOURMETERDSP", MySqlDbType.VarChar, 1)         '定期検査・ｱﾜﾒｰﾀ表示桁
                Dim P_OPERATIONSTYMD As MySqlParameter = SQLcmd.Parameters.Add("@OPERATIONSTYMD", MySqlDbType.Date)         '運用開始年月日
                Dim P_OPERATIONENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@OPERATIONENDYMD", MySqlDbType.Date)         '運用除外年月日
                Dim P_RETIRMENTYMD As MySqlParameter = SQLcmd.Parameters.Add("@RETIRMENTYMD", MySqlDbType.Date)         '除却年月日
                Dim P_COMPKANKBN As MySqlParameter = SQLcmd.Parameters.Add("@COMPKANKBN", MySqlDbType.VarChar, 2)         '複合一貫区分
                Dim P_SUPPLYFLG As MySqlParameter = SQLcmd.Parameters.Add("@SUPPLYFLG", MySqlDbType.VarChar, 1)         '調達フラグ
                Dim P_ADDITEM1 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM1", MySqlDbType.VarChar, 4)         '付帯項目１
                Dim P_ADDITEM2 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM2", MySqlDbType.VarChar, 4)         '付帯項目２
                Dim P_ADDITEM3 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM3", MySqlDbType.VarChar, 4)         '付帯項目３
                Dim P_ADDITEM4 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM4", MySqlDbType.VarChar, 4)         '付帯項目４
                Dim P_ADDITEM5 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM5", MySqlDbType.VarChar, 4)         '付帯項目５
                Dim P_ADDITEM6 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM6", MySqlDbType.VarChar, 4)         '付帯項目６
                Dim P_ADDITEM7 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM7", MySqlDbType.VarChar, 4)         '付帯項目７
                Dim P_ADDITEM8 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM8", MySqlDbType.VarChar, 4)         '付帯項目８
                Dim P_ADDITEM9 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM9", MySqlDbType.VarChar, 4)         '付帯項目９
                Dim P_ADDITEM10 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM10", MySqlDbType.VarChar, 4)         '付帯項目１０
                Dim P_ADDITEM11 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM11", MySqlDbType.VarChar, 4)         '付帯項目１１
                Dim P_ADDITEM12 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM12", MySqlDbType.VarChar, 4)         '付帯項目１２
                Dim P_ADDITEM13 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM13", MySqlDbType.VarChar, 4)         '付帯項目１３
                Dim P_ADDITEM14 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM14", MySqlDbType.VarChar, 4)         '付帯項目１４
                Dim P_ADDITEM15 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM15", MySqlDbType.VarChar, 4)         '付帯項目１５
                Dim P_ADDITEM16 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM16", MySqlDbType.VarChar, 4)         '付帯項目１６
                Dim P_ADDITEM17 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM17", MySqlDbType.VarChar, 4)         '付帯項目１７
                Dim P_ADDITEM18 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM18", MySqlDbType.VarChar, 4)         '付帯項目１８
                Dim P_ADDITEM19 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM19", MySqlDbType.VarChar, 4)         '付帯項目１９
                Dim P_ADDITEM20 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM20", MySqlDbType.VarChar, 4)         '付帯項目２０
                Dim P_ADDITEM21 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM21", MySqlDbType.VarChar, 4)         '付帯項目２１
                Dim P_ADDITEM22 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM22", MySqlDbType.VarChar, 4)         '付帯項目２２
                Dim P_ADDITEM23 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM23", MySqlDbType.VarChar, 4)         '付帯項目２３
                Dim P_ADDITEM24 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM24", MySqlDbType.VarChar, 4)         '付帯項目２４
                Dim P_ADDITEM25 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM25", MySqlDbType.VarChar, 4)         '付帯項目２５
                Dim P_ADDITEM26 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM26", MySqlDbType.VarChar, 4)         '付帯項目２６
                Dim P_ADDITEM27 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM27", MySqlDbType.VarChar, 4)         '付帯項目２７
                Dim P_ADDITEM28 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM28", MySqlDbType.VarChar, 4)         '付帯項目２８
                Dim P_ADDITEM29 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM29", MySqlDbType.VarChar, 4)         '付帯項目２９
                Dim P_ADDITEM30 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM30", MySqlDbType.VarChar, 4)         '付帯項目３０
                Dim P_ADDITEM31 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM31", MySqlDbType.VarChar, 4)         '付帯項目３１
                Dim P_ADDITEM32 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM32", MySqlDbType.VarChar, 4)         '付帯項目３２
                Dim P_ADDITEM33 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM33", MySqlDbType.VarChar, 4)         '付帯項目３３
                Dim P_ADDITEM34 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM34", MySqlDbType.VarChar, 4)         '付帯項目３４
                Dim P_ADDITEM35 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM35", MySqlDbType.VarChar, 4)         '付帯項目３５
                Dim P_ADDITEM36 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM36", MySqlDbType.VarChar, 4)         '付帯項目３６
                Dim P_ADDITEM37 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM37", MySqlDbType.VarChar, 4)         '付帯項目３７
                Dim P_ADDITEM38 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM38", MySqlDbType.VarChar, 4)         '付帯項目３８
                Dim P_ADDITEM39 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM39", MySqlDbType.VarChar, 4)         '付帯項目３９
                Dim P_ADDITEM40 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM40", MySqlDbType.VarChar, 4)         '付帯項目４０
                Dim P_ADDITEM41 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM41", MySqlDbType.VarChar, 4)         '付帯項目４１
                Dim P_ADDITEM42 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM42", MySqlDbType.VarChar, 4)         '付帯項目４２
                Dim P_ADDITEM43 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM43", MySqlDbType.VarChar, 4)         '付帯項目４３
                Dim P_ADDITEM44 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM44", MySqlDbType.VarChar, 4)         '付帯項目４４
                Dim P_ADDITEM45 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM45", MySqlDbType.VarChar, 4)         '付帯項目４５
                Dim P_ADDITEM46 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM46", MySqlDbType.VarChar, 4)         '付帯項目４６
                Dim P_ADDITEM47 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM47", MySqlDbType.VarChar, 4)         '付帯項目４７
                Dim P_ADDITEM48 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM48", MySqlDbType.VarChar, 4)         '付帯項目４８
                Dim P_ADDITEM49 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM49", MySqlDbType.VarChar, 4)         '付帯項目４９
                Dim P_ADDITEM50 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM50", MySqlDbType.VarChar, 4)         '付帯項目５０
                Dim P_FLOORMATERIAL As MySqlParameter = SQLcmd.Parameters.Add("@FLOORMATERIAL", MySqlDbType.VarChar, 1)         '床材質コード
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ

                P_CTNTYPE.Value = WW_ROW("CTNTYPE")               'コンテナ記号
                P_CTNNO.Value = WW_ROW("CTNNO")               'コンテナ番号
                P_JURISDICTIONCD.Value = WW_ROW("JURISDICTIONCD")               '所管部コード
                P_ACCOUNTINGASSETSCD.Value = WW_ROW("ACCOUNTINGASSETSCD")               '経理資産コード
                P_ACCOUNTINGASSETSKBN.Value = WW_ROW("ACCOUNTINGASSETSKBN")               '経理資産区分
                P_DUMMYKBN.Value = WW_ROW("DUMMYKBN")               'ダミー区分
                P_SPOTKBN.Value = WW_ROW("SPOTKBN")               'スポット区分
                'スポット区分　開始年月日
                If Not WW_ROW("SPOTSTYMD") = Date.MinValue Then
                    P_SPOTSTYMD.Value = WW_ROW("SPOTSTYMD")
                Else
                    P_SPOTSTYMD.Value = DBNull.Value
                End If
                'スポット区分　終了年月日
                If Not WW_ROW("SPOTENDYMD") = Date.MinValue Then
                    P_SPOTENDYMD.Value = WW_ROW("SPOTENDYMD")
                Else
                    P_SPOTENDYMD.Value = DBNull.Value
                End If
                P_BIGCTNCD.Value = WW_ROW("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = WW_ROW("MIDDLECTNCD")               '中分類コード
                P_SMALLCTNCD.Value = WW_ROW("SMALLCTNCD")               '小分類コード
                P_CONSTRUCTIONYM.Value = WW_ROW("CONSTRUCTIONYM")               '建造年月
                P_CTNMAKER.Value = WW_ROW("CTNMAKER")               'コンテナメーカー
                P_FROZENMAKER.Value = WW_ROW("FROZENMAKER")               '冷凍機メーカー
                P_GROSSWEIGHT.Value = WW_ROW("GROSSWEIGHT")               '総重量
                P_CARGOWEIGHT.Value = WW_ROW("CARGOWEIGHT")               '荷重
                P_MYWEIGHT.Value = WW_ROW("MYWEIGHT")               '自重
                P_BOOKVALUE.Value = WW_ROW("BOOKVALUE")               '簿価商品価格
                P_OUTHEIGHT.Value = WW_ROW("OUTHEIGHT")               '外寸・高さ
                P_OUTWIDTH.Value = WW_ROW("OUTWIDTH")               '外寸・幅
                P_OUTLENGTH.Value = WW_ROW("OUTLENGTH")               '外寸・長さ
                P_INHEIGHT.Value = WW_ROW("INHEIGHT")               '内寸・高さ
                P_INWIDTH.Value = WW_ROW("INWIDTH")               '内寸・幅
                P_INLENGTH.Value = WW_ROW("INLENGTH")               '内寸・長さ
                P_WIFEHEIGHT.Value = WW_ROW("WIFEHEIGHT")               '妻入口・高さ
                P_WIFEWIDTH.Value = WW_ROW("WIFEWIDTH")               '妻入口・幅
                P_SIDEHEIGHT.Value = WW_ROW("SIDEHEIGHT")               '側入口・高さ
                P_SIDEWIDTH.Value = WW_ROW("SIDEWIDTH")               '側入口・幅
                P_FLOORAREA.Value = WW_ROW("FLOORAREA")               '床面積
                P_INVOLUMEMARKING.Value = WW_ROW("INVOLUMEMARKING")               '内容積・標記
                P_INVOLUMEACTUA.Value = WW_ROW("INVOLUMEACTUA")               '内容積・実寸
                P_TRAINSCYCLEDAYS.Value = WW_ROW("TRAINSCYCLEDAYS")               '交番検査・ｻｲｸﾙ日数
                '交番検査・前回実施日
                If Not WW_ROW("TRAINSBEFORERUNYMD") = Date.MinValue Then
                    P_TRAINSBEFORERUNYMD.Value = WW_ROW("TRAINSBEFORERUNYMD")
                Else
                    P_TRAINSBEFORERUNYMD.Value = DBNull.Value
                End If
                '交番検査・次回実施日
                If Not WW_ROW("TRAINSNEXTRUNYMD") = Date.MinValue Then
                    P_TRAINSNEXTRUNYMD.Value = WW_ROW("TRAINSNEXTRUNYMD")
                Else
                    P_TRAINSNEXTRUNYMD.Value = DBNull.Value
                End If
                P_REGINSCYCLEDAYS.Value = WW_ROW("REGINSCYCLEDAYS")               '定期検査・ｻｲｸﾙ月数
                P_REGINSCYCLEHOURMETER.Value = WW_ROW("REGINSCYCLEHOURMETER")               '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
                '定期検査・前回実施日
                If Not WW_ROW("REGINSBEFORERUNYMD") = Date.MinValue Then
                    P_REGINSBEFORERUNYMD.Value = WW_ROW("REGINSBEFORERUNYMD")
                Else
                    P_REGINSBEFORERUNYMD.Value = DBNull.Value
                End If
                '定期検査・次回実施日
                If Not WW_ROW("REGINSNEXTRUNYMD") = Date.MinValue Then
                    P_REGINSNEXTRUNYMD.Value = WW_ROW("REGINSNEXTRUNYMD")
                Else
                    P_REGINSNEXTRUNYMD.Value = DBNull.Value
                End If
                '定期検査・ｱﾜﾒｰﾀ記載日
                If Not WW_ROW("REGINSHOURMETERYMD") = Date.MinValue Then
                    P_REGINSHOURMETERYMD.Value = WW_ROW("REGINSHOURMETERYMD")
                Else
                    P_REGINSHOURMETERYMD.Value = DBNull.Value
                End If
                P_REGINSHOURMETERTIME.Value = WW_ROW("REGINSHOURMETERTIME")               '定期検査・ｱﾜﾒｰﾀ時間
                P_REGINSHOURMETERDSP.Value = WW_ROW("REGINSHOURMETERDSP")               '定期検査・ｱﾜﾒｰﾀ表示桁
                '運用開始年月日
                If Not WW_ROW("OPERATIONSTYMD") = Date.MinValue Then
                    P_OPERATIONSTYMD.Value = WW_ROW("OPERATIONSTYMD")
                Else
                    P_OPERATIONSTYMD.Value = DBNull.Value
                End If
                '運用除外年月日
                If Not WW_ROW("OPERATIONENDYMD") = Date.MinValue Then
                    P_OPERATIONENDYMD.Value = WW_ROW("OPERATIONENDYMD")
                Else
                    P_OPERATIONENDYMD.Value = DBNull.Value
                End If
                '除却年月日
                If Not WW_ROW("RETIRMENTYMD") = Date.MinValue Then
                    P_RETIRMENTYMD.Value = WW_ROW("RETIRMENTYMD")
                Else
                    P_RETIRMENTYMD.Value = DBNull.Value
                End If
                P_COMPKANKBN.Value = WW_ROW("COMPKANKBN")               '複合一貫区分
                P_SUPPLYFLG.Value = WW_ROW("SUPPLYFLG")               '調達フラグ
                P_ADDITEM1.Value = WW_ROW("ADDITEM1")               '付帯項目１
                P_ADDITEM2.Value = WW_ROW("ADDITEM2")               '付帯項目２
                P_ADDITEM3.Value = WW_ROW("ADDITEM3")               '付帯項目３
                P_ADDITEM4.Value = WW_ROW("ADDITEM4")               '付帯項目４
                P_ADDITEM5.Value = WW_ROW("ADDITEM5")               '付帯項目５
                P_ADDITEM6.Value = WW_ROW("ADDITEM6")               '付帯項目６
                P_ADDITEM7.Value = WW_ROW("ADDITEM7")               '付帯項目７
                P_ADDITEM8.Value = WW_ROW("ADDITEM8")               '付帯項目８
                P_ADDITEM9.Value = WW_ROW("ADDITEM9")               '付帯項目９
                P_ADDITEM10.Value = WW_ROW("ADDITEM10")               '付帯項目１０
                P_ADDITEM11.Value = WW_ROW("ADDITEM11")               '付帯項目１１
                P_ADDITEM12.Value = WW_ROW("ADDITEM12")               '付帯項目１２
                P_ADDITEM13.Value = WW_ROW("ADDITEM13")               '付帯項目１３
                P_ADDITEM14.Value = WW_ROW("ADDITEM14")               '付帯項目１４
                P_ADDITEM15.Value = WW_ROW("ADDITEM15")               '付帯項目１５
                P_ADDITEM16.Value = WW_ROW("ADDITEM16")               '付帯項目１６
                P_ADDITEM17.Value = WW_ROW("ADDITEM17")               '付帯項目１７
                P_ADDITEM18.Value = WW_ROW("ADDITEM18")               '付帯項目１８
                P_ADDITEM19.Value = WW_ROW("ADDITEM19")               '付帯項目１９
                P_ADDITEM20.Value = WW_ROW("ADDITEM20")               '付帯項目２０
                P_ADDITEM21.Value = WW_ROW("ADDITEM21")               '付帯項目２１
                P_ADDITEM22.Value = WW_ROW("ADDITEM22")               '付帯項目２２
                P_ADDITEM23.Value = WW_ROW("ADDITEM23")               '付帯項目２３
                P_ADDITEM24.Value = WW_ROW("ADDITEM24")               '付帯項目２４
                P_ADDITEM25.Value = WW_ROW("ADDITEM25")               '付帯項目２５
                P_ADDITEM26.Value = WW_ROW("ADDITEM26")               '付帯項目２６
                P_ADDITEM27.Value = WW_ROW("ADDITEM27")               '付帯項目２７
                P_ADDITEM28.Value = WW_ROW("ADDITEM28")               '付帯項目２８
                P_ADDITEM29.Value = WW_ROW("ADDITEM29")               '付帯項目２９
                P_ADDITEM30.Value = WW_ROW("ADDITEM30")               '付帯項目３０
                P_ADDITEM31.Value = WW_ROW("ADDITEM31")               '付帯項目３１
                P_ADDITEM32.Value = WW_ROW("ADDITEM32")               '付帯項目３２
                P_ADDITEM33.Value = WW_ROW("ADDITEM33")               '付帯項目３３
                P_ADDITEM34.Value = WW_ROW("ADDITEM34")               '付帯項目３４
                P_ADDITEM35.Value = WW_ROW("ADDITEM35")               '付帯項目３５
                P_ADDITEM36.Value = WW_ROW("ADDITEM36")               '付帯項目３６
                P_ADDITEM37.Value = WW_ROW("ADDITEM37")               '付帯項目３７
                P_ADDITEM38.Value = WW_ROW("ADDITEM38")               '付帯項目３８
                P_ADDITEM39.Value = WW_ROW("ADDITEM39")               '付帯項目３９
                P_ADDITEM40.Value = WW_ROW("ADDITEM40")               '付帯項目４０
                P_ADDITEM41.Value = WW_ROW("ADDITEM41")               '付帯項目４１
                P_ADDITEM42.Value = WW_ROW("ADDITEM42")               '付帯項目４２
                P_ADDITEM43.Value = WW_ROW("ADDITEM43")               '付帯項目４３
                P_ADDITEM44.Value = WW_ROW("ADDITEM44")               '付帯項目４４
                P_ADDITEM45.Value = WW_ROW("ADDITEM45")               '付帯項目４５
                P_ADDITEM46.Value = WW_ROW("ADDITEM46")               '付帯項目４６
                P_ADDITEM47.Value = WW_ROW("ADDITEM47")               '付帯項目４７
                P_ADDITEM48.Value = WW_ROW("ADDITEM48")               '付帯項目４８
                P_ADDITEM49.Value = WW_ROW("ADDITEM49")               '付帯項目４９
                P_ADDITEM50.Value = WW_ROW("ADDITEM50")               '付帯項目５０
                P_FLOORMATERIAL.Value = WW_ROW("FLOORMATERIAL")               '床材質コード
                P_DELFLG.Value = WW_ROW("DELFLG")               '削除フラグ

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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0002_RECONM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0002_RECONM SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力
            Exit Function
        End Try
        SameDataChk = True
    End Function

    ''' <summary>
    ''' Excelデータ登録・更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsUpdExcelData(ByVal SQLcon As MySqlConnection,
                                  ByVal WW_ROW As DataRow,
                                  ByVal WW_DATENOW As DateTime,
                                  ByVal prmTRAINSCYCLEDAYS As String,
                                  ByVal prmREGINSHOURMETERDSP As String)

        WW_ERR_SW = C_MESSAGE_NO.NORMAL

        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" MERGE INTO LNG.LNM0002_RECONM LNM0002")
        SQLStr.AppendLine("     USING ( ")
        SQLStr.AppendLine("             SELECT ")
        SQLStr.AppendLine("              @CTNTYPE AS CTNTYPE ")
        SQLStr.AppendLine("             ,@CTNNO AS CTNNO ")
        SQLStr.AppendLine("             ,@JURISDICTIONCD AS JURISDICTIONCD ")
        SQLStr.AppendLine("             ,@ACCOUNTINGASSETSCD AS ACCOUNTINGASSETSCD ")
        SQLStr.AppendLine("             ,@ACCOUNTINGASSETSKBN AS ACCOUNTINGASSETSKBN ")
        SQLStr.AppendLine("             ,@DUMMYKBN AS DUMMYKBN ")
        SQLStr.AppendLine("             ,@SPOTKBN AS SPOTKBN ")
        SQLStr.AppendLine("             ,@SPOTSTYMD AS SPOTSTYMD ")
        SQLStr.AppendLine("             ,@SPOTENDYMD AS SPOTENDYMD ")
        SQLStr.AppendLine("             ,@BIGCTNCD AS BIGCTNCD ")
        SQLStr.AppendLine("             ,@MIDDLECTNCD AS MIDDLECTNCD ")
        SQLStr.AppendLine("             ,@SMALLCTNCD AS SMALLCTNCD ")
        SQLStr.AppendLine("             ,@CONSTRUCTIONYM AS CONSTRUCTIONYM ")
        SQLStr.AppendLine("             ,@CTNMAKER AS CTNMAKER ")
        SQLStr.AppendLine("             ,@FROZENMAKER AS FROZENMAKER ")
        SQLStr.AppendLine("             ,@GROSSWEIGHT AS GROSSWEIGHT ")
        SQLStr.AppendLine("             ,@CARGOWEIGHT AS CARGOWEIGHT ")
        SQLStr.AppendLine("             ,@MYWEIGHT AS MYWEIGHT ")
        SQLStr.AppendLine("             ,@BOOKVALUE AS BOOKVALUE ")
        SQLStr.AppendLine("             ,@OUTHEIGHT AS OUTHEIGHT ")
        SQLStr.AppendLine("             ,@OUTWIDTH AS OUTWIDTH ")
        SQLStr.AppendLine("             ,@OUTLENGTH AS OUTLENGTH ")
        SQLStr.AppendLine("             ,@INHEIGHT AS INHEIGHT ")
        SQLStr.AppendLine("             ,@INWIDTH AS INWIDTH ")
        SQLStr.AppendLine("             ,@INLENGTH AS INLENGTH ")
        SQLStr.AppendLine("             ,@WIFEHEIGHT AS WIFEHEIGHT ")
        SQLStr.AppendLine("             ,@WIFEWIDTH AS WIFEWIDTH ")
        SQLStr.AppendLine("             ,@SIDEHEIGHT AS SIDEHEIGHT ")
        SQLStr.AppendLine("             ,@SIDEWIDTH AS SIDEWIDTH ")
        SQLStr.AppendLine("             ,@FLOORAREA AS FLOORAREA ")
        SQLStr.AppendLine("             ,@INVOLUMEMARKING AS INVOLUMEMARKING ")
        SQLStr.AppendLine("             ,@INVOLUMEACTUA AS INVOLUMEACTUA ")
        SQLStr.AppendLine("             ,@TRAINSCYCLEDAYS AS TRAINSCYCLEDAYS ")
        SQLStr.AppendLine("             ,@TRAINSBEFORERUNYMD AS TRAINSBEFORERUNYMD ")
        SQLStr.AppendLine("             ,@TRAINSNEXTRUNYMD AS TRAINSNEXTRUNYMD ")
        SQLStr.AppendLine("             ,@REGINSCYCLEDAYS AS REGINSCYCLEDAYS ")
        SQLStr.AppendLine("             ,@REGINSCYCLEHOURMETER AS REGINSCYCLEHOURMETER ")
        SQLStr.AppendLine("             ,@REGINSBEFORERUNYMD AS REGINSBEFORERUNYMD ")
        SQLStr.AppendLine("             ,@REGINSNEXTRUNYMD AS REGINSNEXTRUNYMD ")
        SQLStr.AppendLine("             ,@REGINSHOURMETERYMD AS REGINSHOURMETERYMD ")
        SQLStr.AppendLine("             ,@REGINSHOURMETERTIME AS REGINSHOURMETERTIME ")
        SQLStr.AppendLine("             ,@REGINSHOURMETERDSP AS REGINSHOURMETERDSP ")
        SQLStr.AppendLine("             ,@OPERATIONSTYMD AS OPERATIONSTYMD ")
        SQLStr.AppendLine("             ,@OPERATIONENDYMD AS OPERATIONENDYMD ")
        SQLStr.AppendLine("             ,@RETIRMENTYMD AS RETIRMENTYMD ")
        SQLStr.AppendLine("             ,@COMPKANKBN AS COMPKANKBN ")
        SQLStr.AppendLine("             ,@SUPPLYFLG AS SUPPLYFLG ")
        SQLStr.AppendLine("             ,@ADDITEM1 AS ADDITEM1 ")
        SQLStr.AppendLine("             ,@ADDITEM2 AS ADDITEM2 ")
        SQLStr.AppendLine("             ,@ADDITEM3 AS ADDITEM3 ")
        SQLStr.AppendLine("             ,@ADDITEM4 AS ADDITEM4 ")
        SQLStr.AppendLine("             ,@ADDITEM5 AS ADDITEM5 ")
        SQLStr.AppendLine("             ,@ADDITEM6 AS ADDITEM6 ")
        SQLStr.AppendLine("             ,@ADDITEM7 AS ADDITEM7 ")
        SQLStr.AppendLine("             ,@ADDITEM8 AS ADDITEM8 ")
        SQLStr.AppendLine("             ,@ADDITEM9 AS ADDITEM9 ")
        SQLStr.AppendLine("             ,@ADDITEM10 AS ADDITEM10 ")
        SQLStr.AppendLine("             ,@ADDITEM11 AS ADDITEM11 ")
        SQLStr.AppendLine("             ,@ADDITEM12 AS ADDITEM12 ")
        SQLStr.AppendLine("             ,@ADDITEM13 AS ADDITEM13 ")
        SQLStr.AppendLine("             ,@ADDITEM14 AS ADDITEM14 ")
        SQLStr.AppendLine("             ,@ADDITEM15 AS ADDITEM15 ")
        SQLStr.AppendLine("             ,@ADDITEM16 AS ADDITEM16 ")
        SQLStr.AppendLine("             ,@ADDITEM17 AS ADDITEM17 ")
        SQLStr.AppendLine("             ,@ADDITEM18 AS ADDITEM18 ")
        SQLStr.AppendLine("             ,@ADDITEM19 AS ADDITEM19 ")
        SQLStr.AppendLine("             ,@ADDITEM20 AS ADDITEM20 ")
        SQLStr.AppendLine("             ,@ADDITEM21 AS ADDITEM21 ")
        SQLStr.AppendLine("             ,@ADDITEM22 AS ADDITEM22 ")
        SQLStr.AppendLine("             ,@ADDITEM23 AS ADDITEM23 ")
        SQLStr.AppendLine("             ,@ADDITEM24 AS ADDITEM24 ")
        SQLStr.AppendLine("             ,@ADDITEM25 AS ADDITEM25 ")
        SQLStr.AppendLine("             ,@ADDITEM26 AS ADDITEM26 ")
        SQLStr.AppendLine("             ,@ADDITEM27 AS ADDITEM27 ")
        SQLStr.AppendLine("             ,@ADDITEM28 AS ADDITEM28 ")
        SQLStr.AppendLine("             ,@ADDITEM29 AS ADDITEM29 ")
        SQLStr.AppendLine("             ,@ADDITEM30 AS ADDITEM30 ")
        SQLStr.AppendLine("             ,@ADDITEM31 AS ADDITEM31 ")
        SQLStr.AppendLine("             ,@ADDITEM32 AS ADDITEM32 ")
        SQLStr.AppendLine("             ,@ADDITEM33 AS ADDITEM33 ")
        SQLStr.AppendLine("             ,@ADDITEM34 AS ADDITEM34 ")
        SQLStr.AppendLine("             ,@ADDITEM35 AS ADDITEM35 ")
        SQLStr.AppendLine("             ,@ADDITEM36 AS ADDITEM36 ")
        SQLStr.AppendLine("             ,@ADDITEM37 AS ADDITEM37 ")
        SQLStr.AppendLine("             ,@ADDITEM38 AS ADDITEM38 ")
        SQLStr.AppendLine("             ,@ADDITEM39 AS ADDITEM39 ")
        SQLStr.AppendLine("             ,@ADDITEM40 AS ADDITEM40 ")
        SQLStr.AppendLine("             ,@ADDITEM41 AS ADDITEM41 ")
        SQLStr.AppendLine("             ,@ADDITEM42 AS ADDITEM42 ")
        SQLStr.AppendLine("             ,@ADDITEM43 AS ADDITEM43 ")
        SQLStr.AppendLine("             ,@ADDITEM44 AS ADDITEM44 ")
        SQLStr.AppendLine("             ,@ADDITEM45 AS ADDITEM45 ")
        SQLStr.AppendLine("             ,@ADDITEM46 AS ADDITEM46 ")
        SQLStr.AppendLine("             ,@ADDITEM47 AS ADDITEM47 ")
        SQLStr.AppendLine("             ,@ADDITEM48 AS ADDITEM48 ")
        SQLStr.AppendLine("             ,@ADDITEM49 AS ADDITEM49 ")
        SQLStr.AppendLine("             ,@ADDITEM50 AS ADDITEM50 ")
        SQLStr.AppendLine("             ,@FLOORMATERIAL AS FLOORMATERIAL ")
        SQLStr.AppendLine("             ,@DELFLG AS DELFLG ")
        SQLStr.AppendLine("            ) EXCEL")
        SQLStr.AppendLine("    ON ( ")
        SQLStr.AppendLine("             LNM0002.CTNTYPE = EXCEL.CTNTYPE ")
        SQLStr.AppendLine("         AND LNM0002.CTNNO = EXCEL.CTNNO ")
        SQLStr.AppendLine("       ) ")
        SQLStr.AppendLine("    WHEN MATCHED THEN ")
        SQLStr.AppendLine("     UPDATE SET ")
        SQLStr.AppendLine("          LNM0002.JURISDICTIONCD =  EXCEL.JURISDICTIONCD")
        SQLStr.AppendLine("         ,LNM0002.ACCOUNTINGASSETSCD =  EXCEL.ACCOUNTINGASSETSCD")
        SQLStr.AppendLine("         ,LNM0002.ACCOUNTINGASSETSKBN =  EXCEL.ACCOUNTINGASSETSKBN")
        SQLStr.AppendLine("         ,LNM0002.DUMMYKBN =  EXCEL.DUMMYKBN")
        SQLStr.AppendLine("         ,LNM0002.SPOTKBN =  EXCEL.SPOTKBN")
        SQLStr.AppendLine("         ,LNM0002.SPOTSTYMD =  EXCEL.SPOTSTYMD")
        SQLStr.AppendLine("         ,LNM0002.SPOTENDYMD =  EXCEL.SPOTENDYMD")
        SQLStr.AppendLine("         ,LNM0002.BIGCTNCD =  EXCEL.BIGCTNCD")
        SQLStr.AppendLine("         ,LNM0002.MIDDLECTNCD =  EXCEL.MIDDLECTNCD")
        SQLStr.AppendLine("         ,LNM0002.SMALLCTNCD =  EXCEL.SMALLCTNCD")
        SQLStr.AppendLine("         ,LNM0002.CONSTRUCTIONYM =  EXCEL.CONSTRUCTIONYM")
        SQLStr.AppendLine("         ,LNM0002.CTNMAKER =  EXCEL.CTNMAKER")
        SQLStr.AppendLine("         ,LNM0002.FROZENMAKER =  EXCEL.FROZENMAKER")
        SQLStr.AppendLine("         ,LNM0002.GROSSWEIGHT =  EXCEL.GROSSWEIGHT")
        SQLStr.AppendLine("         ,LNM0002.CARGOWEIGHT =  EXCEL.CARGOWEIGHT")
        SQLStr.AppendLine("         ,LNM0002.MYWEIGHT =  EXCEL.MYWEIGHT")
        SQLStr.AppendLine("         ,LNM0002.BOOKVALUE =  EXCEL.BOOKVALUE")
        SQLStr.AppendLine("         ,LNM0002.OUTHEIGHT =  EXCEL.OUTHEIGHT")
        SQLStr.AppendLine("         ,LNM0002.OUTWIDTH =  EXCEL.OUTWIDTH")
        SQLStr.AppendLine("         ,LNM0002.OUTLENGTH =  EXCEL.OUTLENGTH")
        SQLStr.AppendLine("         ,LNM0002.INHEIGHT =  EXCEL.INHEIGHT")
        SQLStr.AppendLine("         ,LNM0002.INWIDTH =  EXCEL.INWIDTH")
        SQLStr.AppendLine("         ,LNM0002.INLENGTH =  EXCEL.INLENGTH")
        SQLStr.AppendLine("         ,LNM0002.WIFEHEIGHT =  EXCEL.WIFEHEIGHT")
        SQLStr.AppendLine("         ,LNM0002.WIFEWIDTH =  EXCEL.WIFEWIDTH")
        SQLStr.AppendLine("         ,LNM0002.SIDEHEIGHT =  EXCEL.SIDEHEIGHT")
        SQLStr.AppendLine("         ,LNM0002.SIDEWIDTH =  EXCEL.SIDEWIDTH")
        SQLStr.AppendLine("         ,LNM0002.FLOORAREA =  EXCEL.FLOORAREA")
        SQLStr.AppendLine("         ,LNM0002.INVOLUMEMARKING =  EXCEL.INVOLUMEMARKING")
        SQLStr.AppendLine("         ,LNM0002.INVOLUMEACTUA =  EXCEL.INVOLUMEACTUA")
        SQLStr.AppendLine("         ,LNM0002.TRAINSCYCLEDAYS =  EXCEL.TRAINSCYCLEDAYS")
        SQLStr.AppendLine("         ,LNM0002.TRAINSBEFORERUNYMD =  EXCEL.TRAINSBEFORERUNYMD")
        SQLStr.AppendLine("         ,LNM0002.TRAINSNEXTRUNYMD =  EXCEL.TRAINSNEXTRUNYMD")
        SQLStr.AppendLine("         ,LNM0002.REGINSCYCLEDAYS =  EXCEL.REGINSCYCLEDAYS")
        SQLStr.AppendLine("         ,LNM0002.REGINSCYCLEHOURMETER =  EXCEL.REGINSCYCLEHOURMETER")
        SQLStr.AppendLine("         ,LNM0002.REGINSBEFORERUNYMD =  EXCEL.REGINSBEFORERUNYMD")
        SQLStr.AppendLine("         ,LNM0002.REGINSNEXTRUNYMD =  EXCEL.REGINSNEXTRUNYMD")
        SQLStr.AppendLine("         ,LNM0002.REGINSHOURMETERYMD =  EXCEL.REGINSHOURMETERYMD")
        SQLStr.AppendLine("         ,LNM0002.REGINSHOURMETERTIME =  EXCEL.REGINSHOURMETERTIME")
        SQLStr.AppendLine("         ,LNM0002.REGINSHOURMETERDSP =  EXCEL.REGINSHOURMETERDSP")
        SQLStr.AppendLine("         ,LNM0002.OPERATIONSTYMD =  EXCEL.OPERATIONSTYMD")
        SQLStr.AppendLine("         ,LNM0002.OPERATIONENDYMD =  EXCEL.OPERATIONENDYMD")
        SQLStr.AppendLine("         ,LNM0002.RETIRMENTYMD =  EXCEL.RETIRMENTYMD")
        SQLStr.AppendLine("         ,LNM0002.COMPKANKBN =  EXCEL.COMPKANKBN")
        SQLStr.AppendLine("         ,LNM0002.SUPPLYFLG =  EXCEL.SUPPLYFLG")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM1 =  EXCEL.ADDITEM1")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM2 =  EXCEL.ADDITEM2")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM3 =  EXCEL.ADDITEM3")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM4 =  EXCEL.ADDITEM4")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM5 =  EXCEL.ADDITEM5")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM6 =  EXCEL.ADDITEM6")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM7 =  EXCEL.ADDITEM7")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM8 =  EXCEL.ADDITEM8")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM9 =  EXCEL.ADDITEM9")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM10 =  EXCEL.ADDITEM10")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM11 =  EXCEL.ADDITEM11")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM12 =  EXCEL.ADDITEM12")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM13 =  EXCEL.ADDITEM13")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM14 =  EXCEL.ADDITEM14")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM15 =  EXCEL.ADDITEM15")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM16 =  EXCEL.ADDITEM16")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM17 =  EXCEL.ADDITEM17")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM18 =  EXCEL.ADDITEM18")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM19 =  EXCEL.ADDITEM19")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM20 =  EXCEL.ADDITEM20")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM21 =  EXCEL.ADDITEM21")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM22 =  EXCEL.ADDITEM22")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM23 =  EXCEL.ADDITEM23")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM24 =  EXCEL.ADDITEM24")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM25 =  EXCEL.ADDITEM25")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM26 =  EXCEL.ADDITEM26")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM27 =  EXCEL.ADDITEM27")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM28 =  EXCEL.ADDITEM28")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM29 =  EXCEL.ADDITEM29")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM30 =  EXCEL.ADDITEM30")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM31 =  EXCEL.ADDITEM31")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM32 =  EXCEL.ADDITEM32")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM33 =  EXCEL.ADDITEM33")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM34 =  EXCEL.ADDITEM34")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM35 =  EXCEL.ADDITEM35")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM36 =  EXCEL.ADDITEM36")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM37 =  EXCEL.ADDITEM37")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM38 =  EXCEL.ADDITEM38")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM39 =  EXCEL.ADDITEM39")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM40 =  EXCEL.ADDITEM40")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM41 =  EXCEL.ADDITEM41")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM42 =  EXCEL.ADDITEM42")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM43 =  EXCEL.ADDITEM43")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM44 =  EXCEL.ADDITEM44")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM45 =  EXCEL.ADDITEM45")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM46 =  EXCEL.ADDITEM46")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM47 =  EXCEL.ADDITEM47")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM48 =  EXCEL.ADDITEM48")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM49 =  EXCEL.ADDITEM49")
        SQLStr.AppendLine("         ,LNM0002.ADDITEM50 =  EXCEL.ADDITEM50")
        SQLStr.AppendLine("         ,LNM0002.FLOORMATERIAL =  EXCEL.FLOORMATERIAL")
        SQLStr.AppendLine("         ,LNM0002.DELFLG =  EXCEL.DELFLG")
        SQLStr.AppendLine("         ,LNM0002.UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("         ,LNM0002.UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("         ,LNM0002.UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("         ,LNM0002.UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("    WHEN NOT MATCHED THEN ")
        SQLStr.AppendLine("     INSERT ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         CTNTYPE  ")
        SQLStr.AppendLine("        ,CTNNO  ")
        SQLStr.AppendLine("        ,JURISDICTIONCD  ")
        SQLStr.AppendLine("        ,ACCOUNTINGASSETSCD  ")
        SQLStr.AppendLine("        ,ACCOUNTINGASSETSKBN  ")
        SQLStr.AppendLine("        ,DUMMYKBN  ")
        SQLStr.AppendLine("        ,SPOTKBN  ")
        SQLStr.AppendLine("        ,SPOTSTYMD  ")
        SQLStr.AppendLine("        ,SPOTENDYMD  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,SMALLCTNCD  ")
        SQLStr.AppendLine("        ,CONSTRUCTIONYM  ")
        SQLStr.AppendLine("        ,CTNMAKER  ")
        SQLStr.AppendLine("        ,FROZENMAKER  ")
        SQLStr.AppendLine("        ,GROSSWEIGHT  ")
        SQLStr.AppendLine("        ,CARGOWEIGHT  ")
        SQLStr.AppendLine("        ,MYWEIGHT  ")
        SQLStr.AppendLine("        ,BOOKVALUE  ")
        SQLStr.AppendLine("        ,OUTHEIGHT  ")
        SQLStr.AppendLine("        ,OUTWIDTH  ")
        SQLStr.AppendLine("        ,OUTLENGTH  ")
        SQLStr.AppendLine("        ,INHEIGHT  ")
        SQLStr.AppendLine("        ,INWIDTH  ")
        SQLStr.AppendLine("        ,INLENGTH  ")
        SQLStr.AppendLine("        ,WIFEHEIGHT  ")
        SQLStr.AppendLine("        ,WIFEWIDTH  ")
        SQLStr.AppendLine("        ,SIDEHEIGHT  ")
        SQLStr.AppendLine("        ,SIDEWIDTH  ")
        SQLStr.AppendLine("        ,FLOORAREA  ")
        SQLStr.AppendLine("        ,INVOLUMEMARKING  ")
        SQLStr.AppendLine("        ,INVOLUMEACTUA  ")
        SQLStr.AppendLine("        ,TRAINSCYCLEDAYS  ")
        SQLStr.AppendLine("        ,TRAINSBEFORERUNYMD  ")
        SQLStr.AppendLine("        ,TRAINSNEXTRUNYMD  ")
        SQLStr.AppendLine("        ,REGINSCYCLEDAYS  ")
        SQLStr.AppendLine("        ,REGINSCYCLEHOURMETER  ")
        SQLStr.AppendLine("        ,REGINSBEFORERUNYMD  ")
        SQLStr.AppendLine("        ,REGINSNEXTRUNYMD  ")
        SQLStr.AppendLine("        ,REGINSHOURMETERYMD  ")
        SQLStr.AppendLine("        ,REGINSHOURMETERTIME  ")
        SQLStr.AppendLine("        ,REGINSHOURMETERDSP  ")
        SQLStr.AppendLine("        ,OPERATIONSTYMD  ")
        SQLStr.AppendLine("        ,OPERATIONENDYMD  ")
        SQLStr.AppendLine("        ,RETIRMENTYMD  ")
        SQLStr.AppendLine("        ,COMPKANKBN  ")
        SQLStr.AppendLine("        ,SUPPLYFLG  ")
        SQLStr.AppendLine("        ,ADDITEM1  ")
        SQLStr.AppendLine("        ,ADDITEM2  ")
        SQLStr.AppendLine("        ,ADDITEM3  ")
        SQLStr.AppendLine("        ,ADDITEM4  ")
        SQLStr.AppendLine("        ,ADDITEM5  ")
        SQLStr.AppendLine("        ,ADDITEM6  ")
        SQLStr.AppendLine("        ,ADDITEM7  ")
        SQLStr.AppendLine("        ,ADDITEM8  ")
        SQLStr.AppendLine("        ,ADDITEM9  ")
        SQLStr.AppendLine("        ,ADDITEM10  ")
        SQLStr.AppendLine("        ,ADDITEM11  ")
        SQLStr.AppendLine("        ,ADDITEM12  ")
        SQLStr.AppendLine("        ,ADDITEM13  ")
        SQLStr.AppendLine("        ,ADDITEM14  ")
        SQLStr.AppendLine("        ,ADDITEM15  ")
        SQLStr.AppendLine("        ,ADDITEM16  ")
        SQLStr.AppendLine("        ,ADDITEM17  ")
        SQLStr.AppendLine("        ,ADDITEM18  ")
        SQLStr.AppendLine("        ,ADDITEM19  ")
        SQLStr.AppendLine("        ,ADDITEM20  ")
        SQLStr.AppendLine("        ,ADDITEM21  ")
        SQLStr.AppendLine("        ,ADDITEM22  ")
        SQLStr.AppendLine("        ,ADDITEM23  ")
        SQLStr.AppendLine("        ,ADDITEM24  ")
        SQLStr.AppendLine("        ,ADDITEM25  ")
        SQLStr.AppendLine("        ,ADDITEM26  ")
        SQLStr.AppendLine("        ,ADDITEM27  ")
        SQLStr.AppendLine("        ,ADDITEM28  ")
        SQLStr.AppendLine("        ,ADDITEM29  ")
        SQLStr.AppendLine("        ,ADDITEM30  ")
        SQLStr.AppendLine("        ,ADDITEM31  ")
        SQLStr.AppendLine("        ,ADDITEM32  ")
        SQLStr.AppendLine("        ,ADDITEM33  ")
        SQLStr.AppendLine("        ,ADDITEM34  ")
        SQLStr.AppendLine("        ,ADDITEM35  ")
        SQLStr.AppendLine("        ,ADDITEM36  ")
        SQLStr.AppendLine("        ,ADDITEM37  ")
        SQLStr.AppendLine("        ,ADDITEM38  ")
        SQLStr.AppendLine("        ,ADDITEM39  ")
        SQLStr.AppendLine("        ,ADDITEM40  ")
        SQLStr.AppendLine("        ,ADDITEM41  ")
        SQLStr.AppendLine("        ,ADDITEM42  ")
        SQLStr.AppendLine("        ,ADDITEM43  ")
        SQLStr.AppendLine("        ,ADDITEM44  ")
        SQLStr.AppendLine("        ,ADDITEM45  ")
        SQLStr.AppendLine("        ,ADDITEM46  ")
        SQLStr.AppendLine("        ,ADDITEM47  ")
        SQLStr.AppendLine("        ,ADDITEM48  ")
        SQLStr.AppendLine("        ,ADDITEM49  ")
        SQLStr.AppendLine("        ,ADDITEM50  ")
        SQLStr.AppendLine("        ,FLOORMATERIAL  ")
        SQLStr.AppendLine("        ,DELFLG  ")
        SQLStr.AppendLine("        ,INITYMD  ")
        SQLStr.AppendLine("        ,INITUSER  ")
        SQLStr.AppendLine("        ,INITTERMID  ")
        SQLStr.AppendLine("        ,INITPGID  ")
        SQLStr.AppendLine("      )  ")
        SQLStr.AppendLine("      VALUES  ")
        SQLStr.AppendLine("      (  ")
        SQLStr.AppendLine("         @CTNTYPE  ")
        SQLStr.AppendLine("        ,@CTNNO  ")
        SQLStr.AppendLine("        ,@JURISDICTIONCD  ")
        SQLStr.AppendLine("        ,@ACCOUNTINGASSETSCD  ")
        SQLStr.AppendLine("        ,@ACCOUNTINGASSETSKBN  ")
        SQLStr.AppendLine("        ,@DUMMYKBN  ")
        SQLStr.AppendLine("        ,@SPOTKBN  ")
        SQLStr.AppendLine("        ,@SPOTSTYMD  ")
        SQLStr.AppendLine("        ,@SPOTENDYMD  ")
        SQLStr.AppendLine("        ,@BIGCTNCD  ")
        SQLStr.AppendLine("        ,@MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,@SMALLCTNCD  ")
        SQLStr.AppendLine("        ,@CONSTRUCTIONYM  ")
        SQLStr.AppendLine("        ,@CTNMAKER  ")
        SQLStr.AppendLine("        ,@FROZENMAKER  ")
        SQLStr.AppendLine("        ,@GROSSWEIGHT  ")
        SQLStr.AppendLine("        ,@CARGOWEIGHT  ")
        SQLStr.AppendLine("        ,@MYWEIGHT  ")
        SQLStr.AppendLine("        ,@BOOKVALUE  ")
        SQLStr.AppendLine("        ,@OUTHEIGHT  ")
        SQLStr.AppendLine("        ,@OUTWIDTH  ")
        SQLStr.AppendLine("        ,@OUTLENGTH  ")
        SQLStr.AppendLine("        ,@INHEIGHT  ")
        SQLStr.AppendLine("        ,@INWIDTH  ")
        SQLStr.AppendLine("        ,@INLENGTH  ")
        SQLStr.AppendLine("        ,@WIFEHEIGHT  ")
        SQLStr.AppendLine("        ,@WIFEWIDTH  ")
        SQLStr.AppendLine("        ,@SIDEHEIGHT  ")
        SQLStr.AppendLine("        ,@SIDEWIDTH  ")
        SQLStr.AppendLine("        ,@FLOORAREA  ")
        SQLStr.AppendLine("        ,@INVOLUMEMARKING  ")
        SQLStr.AppendLine("        ,@INVOLUMEACTUA  ")
        SQLStr.AppendLine("        ,@TRAINSCYCLEDAYS  ")
        SQLStr.AppendLine("        ,@TRAINSBEFORERUNYMD  ")
        SQLStr.AppendLine("        ,@TRAINSNEXTRUNYMD  ")
        SQLStr.AppendLine("        ,@REGINSCYCLEDAYS  ")
        SQLStr.AppendLine("        ,@REGINSCYCLEHOURMETER  ")
        SQLStr.AppendLine("        ,@REGINSBEFORERUNYMD  ")
        SQLStr.AppendLine("        ,@REGINSNEXTRUNYMD  ")
        SQLStr.AppendLine("        ,@REGINSHOURMETERYMD  ")
        SQLStr.AppendLine("        ,@REGINSHOURMETERTIME  ")
        SQLStr.AppendLine("        ,@REGINSHOURMETERDSP  ")
        SQLStr.AppendLine("        ,@OPERATIONSTYMD  ")
        SQLStr.AppendLine("        ,@OPERATIONENDYMD  ")
        SQLStr.AppendLine("        ,@RETIRMENTYMD  ")
        SQLStr.AppendLine("        ,@COMPKANKBN  ")
        SQLStr.AppendLine("        ,@SUPPLYFLG  ")
        SQLStr.AppendLine("        ,@ADDITEM1  ")
        SQLStr.AppendLine("        ,@ADDITEM2  ")
        SQLStr.AppendLine("        ,@ADDITEM3  ")
        SQLStr.AppendLine("        ,@ADDITEM4  ")
        SQLStr.AppendLine("        ,@ADDITEM5  ")
        SQLStr.AppendLine("        ,@ADDITEM6  ")
        SQLStr.AppendLine("        ,@ADDITEM7  ")
        SQLStr.AppendLine("        ,@ADDITEM8  ")
        SQLStr.AppendLine("        ,@ADDITEM9  ")
        SQLStr.AppendLine("        ,@ADDITEM10  ")
        SQLStr.AppendLine("        ,@ADDITEM11  ")
        SQLStr.AppendLine("        ,@ADDITEM12  ")
        SQLStr.AppendLine("        ,@ADDITEM13  ")
        SQLStr.AppendLine("        ,@ADDITEM14  ")
        SQLStr.AppendLine("        ,@ADDITEM15  ")
        SQLStr.AppendLine("        ,@ADDITEM16  ")
        SQLStr.AppendLine("        ,@ADDITEM17  ")
        SQLStr.AppendLine("        ,@ADDITEM18  ")
        SQLStr.AppendLine("        ,@ADDITEM19  ")
        SQLStr.AppendLine("        ,@ADDITEM20  ")
        SQLStr.AppendLine("        ,@ADDITEM21  ")
        SQLStr.AppendLine("        ,@ADDITEM22  ")
        SQLStr.AppendLine("        ,@ADDITEM23  ")
        SQLStr.AppendLine("        ,@ADDITEM24  ")
        SQLStr.AppendLine("        ,@ADDITEM25  ")
        SQLStr.AppendLine("        ,@ADDITEM26  ")
        SQLStr.AppendLine("        ,@ADDITEM27  ")
        SQLStr.AppendLine("        ,@ADDITEM28  ")
        SQLStr.AppendLine("        ,@ADDITEM29  ")
        SQLStr.AppendLine("        ,@ADDITEM30  ")
        SQLStr.AppendLine("        ,@ADDITEM31  ")
        SQLStr.AppendLine("        ,@ADDITEM32  ")
        SQLStr.AppendLine("        ,@ADDITEM33  ")
        SQLStr.AppendLine("        ,@ADDITEM34  ")
        SQLStr.AppendLine("        ,@ADDITEM35  ")
        SQLStr.AppendLine("        ,@ADDITEM36  ")
        SQLStr.AppendLine("        ,@ADDITEM37  ")
        SQLStr.AppendLine("        ,@ADDITEM38  ")
        SQLStr.AppendLine("        ,@ADDITEM39  ")
        SQLStr.AppendLine("        ,@ADDITEM40  ")
        SQLStr.AppendLine("        ,@ADDITEM41  ")
        SQLStr.AppendLine("        ,@ADDITEM42  ")
        SQLStr.AppendLine("        ,@ADDITEM43  ")
        SQLStr.AppendLine("        ,@ADDITEM44  ")
        SQLStr.AppendLine("        ,@ADDITEM45  ")
        SQLStr.AppendLine("        ,@ADDITEM46  ")
        SQLStr.AppendLine("        ,@ADDITEM47  ")
        SQLStr.AppendLine("        ,@ADDITEM48  ")
        SQLStr.AppendLine("        ,@ADDITEM49  ")
        SQLStr.AppendLine("        ,@ADDITEM50  ")
        SQLStr.AppendLine("        ,@FLOORMATERIAL  ")
        SQLStr.AppendLine("        ,@DELFLG  ")
        SQLStr.AppendLine("        ,@INITYMD  ")
        SQLStr.AppendLine("        ,@INITUSER  ")
        SQLStr.AppendLine("        ,@INITTERMID  ")
        SQLStr.AppendLine("        ,@INITPGID  ")
        SQLStr.AppendLine("      ) ;  ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_CTNTYPE As MySqlParameter = SQLcmd.Parameters.Add("@CTNTYPE", MySqlDbType.VarChar, 5)         'コンテナ記号
                Dim P_CTNNO As MySqlParameter = SQLcmd.Parameters.Add("@CTNNO", MySqlDbType.VarChar, 8)         'コンテナ番号
                Dim P_JURISDICTIONCD As MySqlParameter = SQLcmd.Parameters.Add("@JURISDICTIONCD", MySqlDbType.VarChar, 2)         '所管部コード
                Dim P_ACCOUNTINGASSETSCD As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTINGASSETSCD", MySqlDbType.VarChar, 4)         '経理資産コード
                Dim P_ACCOUNTINGASSETSKBN As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTINGASSETSKBN", MySqlDbType.VarChar, 2)         '経理資産区分
                Dim P_DUMMYKBN As MySqlParameter = SQLcmd.Parameters.Add("@DUMMYKBN", MySqlDbType.VarChar, 2)         'ダミー区分
                Dim P_SPOTKBN As MySqlParameter = SQLcmd.Parameters.Add("@SPOTKBN", MySqlDbType.VarChar, 2)         'スポット区分
                Dim P_SPOTSTYMD As MySqlParameter = SQLcmd.Parameters.Add("@SPOTSTYMD", MySqlDbType.Date)         'スポット区分　開始年月日
                Dim P_SPOTENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@SPOTENDYMD", MySqlDbType.Date)         'スポット区分　終了年月日
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_SMALLCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCTNCD", MySqlDbType.VarChar, 2)         '小分類コード
                Dim P_CONSTRUCTIONYM As MySqlParameter = SQLcmd.Parameters.Add("@CONSTRUCTIONYM", MySqlDbType.VarChar, 6)         '建造年月
                Dim P_CTNMAKER As MySqlParameter = SQLcmd.Parameters.Add("@CTNMAKER", MySqlDbType.VarChar, 3)         'コンテナメーカー
                Dim P_FROZENMAKER As MySqlParameter = SQLcmd.Parameters.Add("@FROZENMAKER", MySqlDbType.VarChar, 3)         '冷凍機メーカー
                Dim P_GROSSWEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@GROSSWEIGHT", MySqlDbType.Decimal, 4, 1)       '総重量
                Dim P_CARGOWEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@CARGOWEIGHT", MySqlDbType.Decimal, 6, 1)       '荷重
                Dim P_MYWEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@MYWEIGHT", MySqlDbType.Decimal, 4, 1)       '自重
                Dim P_BOOKVALUE As MySqlParameter = SQLcmd.Parameters.Add("@BOOKVALUE", MySqlDbType.Decimal)         '簿価商品価格
                Dim P_OUTHEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@OUTHEIGHT", MySqlDbType.VarChar, 4)         '外寸・高さ
                Dim P_OUTWIDTH As MySqlParameter = SQLcmd.Parameters.Add("@OUTWIDTH", MySqlDbType.VarChar, 4)         '外寸・幅
                Dim P_OUTLENGTH As MySqlParameter = SQLcmd.Parameters.Add("@OUTLENGTH", MySqlDbType.VarChar, 4)         '外寸・長さ
                Dim P_INHEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@INHEIGHT", MySqlDbType.VarChar, 4)         '内寸・高さ
                Dim P_INWIDTH As MySqlParameter = SQLcmd.Parameters.Add("@INWIDTH", MySqlDbType.VarChar, 4)         '内寸・幅
                Dim P_INLENGTH As MySqlParameter = SQLcmd.Parameters.Add("@INLENGTH", MySqlDbType.VarChar, 4)         '内寸・長さ
                Dim P_WIFEHEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@WIFEHEIGHT", MySqlDbType.VarChar, 4)         '妻入口・高さ
                Dim P_WIFEWIDTH As MySqlParameter = SQLcmd.Parameters.Add("@WIFEWIDTH", MySqlDbType.VarChar, 4)         '妻入口・幅
                Dim P_SIDEHEIGHT As MySqlParameter = SQLcmd.Parameters.Add("@SIDEHEIGHT", MySqlDbType.VarChar, 4)         '側入口・高さ
                Dim P_SIDEWIDTH As MySqlParameter = SQLcmd.Parameters.Add("@SIDEWIDTH", MySqlDbType.VarChar, 4)         '側入口・幅
                Dim P_FLOORAREA As MySqlParameter = SQLcmd.Parameters.Add("@FLOORAREA", MySqlDbType.Decimal, 6, 2)       '床面積
                Dim P_INVOLUMEMARKING As MySqlParameter = SQLcmd.Parameters.Add("@INVOLUMEMARKING", MySqlDbType.VarChar, 4)         '内容積・標記
                Dim P_INVOLUMEACTUA As MySqlParameter = SQLcmd.Parameters.Add("@INVOLUMEACTUA", MySqlDbType.Decimal, 6, 2)       '内容積・実寸
                Dim P_TRAINSCYCLEDAYS As MySqlParameter = SQLcmd.Parameters.Add("@TRAINSCYCLEDAYS", MySqlDbType.VarChar, 3)         '交番検査・ｻｲｸﾙ日数
                Dim P_TRAINSBEFORERUNYMD As MySqlParameter = SQLcmd.Parameters.Add("@TRAINSBEFORERUNYMD", MySqlDbType.Date)         '交番検査・前回実施日
                Dim P_TRAINSNEXTRUNYMD As MySqlParameter = SQLcmd.Parameters.Add("@TRAINSNEXTRUNYMD", MySqlDbType.Date)         '交番検査・次回実施日
                Dim P_REGINSCYCLEDAYS As MySqlParameter = SQLcmd.Parameters.Add("@REGINSCYCLEDAYS", MySqlDbType.VarChar, 2)         '定期検査・ｻｲｸﾙ月数
                Dim P_REGINSCYCLEHOURMETER As MySqlParameter = SQLcmd.Parameters.Add("@REGINSCYCLEHOURMETER", MySqlDbType.VarChar, 3)         '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
                Dim P_REGINSBEFORERUNYMD As MySqlParameter = SQLcmd.Parameters.Add("@REGINSBEFORERUNYMD", MySqlDbType.Date)         '定期検査・前回実施日
                Dim P_REGINSNEXTRUNYMD As MySqlParameter = SQLcmd.Parameters.Add("@REGINSNEXTRUNYMD", MySqlDbType.Date)         '定期検査・次回実施日
                Dim P_REGINSHOURMETERYMD As MySqlParameter = SQLcmd.Parameters.Add("@REGINSHOURMETERYMD", MySqlDbType.Date)         '定期検査・ｱﾜﾒｰﾀ記載日
                Dim P_REGINSHOURMETERTIME As MySqlParameter = SQLcmd.Parameters.Add("@REGINSHOURMETERTIME", MySqlDbType.VarChar, 5)         '定期検査・ｱﾜﾒｰﾀ時間
                Dim P_REGINSHOURMETERDSP As MySqlParameter = SQLcmd.Parameters.Add("@REGINSHOURMETERDSP", MySqlDbType.VarChar, 1)         '定期検査・ｱﾜﾒｰﾀ表示桁
                Dim P_OPERATIONSTYMD As MySqlParameter = SQLcmd.Parameters.Add("@OPERATIONSTYMD", MySqlDbType.Date)         '運用開始年月日
                Dim P_OPERATIONENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@OPERATIONENDYMD", MySqlDbType.Date)         '運用除外年月日
                Dim P_RETIRMENTYMD As MySqlParameter = SQLcmd.Parameters.Add("@RETIRMENTYMD", MySqlDbType.Date)         '除却年月日
                Dim P_COMPKANKBN As MySqlParameter = SQLcmd.Parameters.Add("@COMPKANKBN", MySqlDbType.VarChar, 2)         '複合一貫区分
                Dim P_SUPPLYFLG As MySqlParameter = SQLcmd.Parameters.Add("@SUPPLYFLG", MySqlDbType.VarChar, 1)         '調達フラグ
                Dim P_ADDITEM1 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM1", MySqlDbType.VarChar, 4)         '付帯項目１
                Dim P_ADDITEM2 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM2", MySqlDbType.VarChar, 4)         '付帯項目２
                Dim P_ADDITEM3 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM3", MySqlDbType.VarChar, 4)         '付帯項目３
                Dim P_ADDITEM4 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM4", MySqlDbType.VarChar, 4)         '付帯項目４
                Dim P_ADDITEM5 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM5", MySqlDbType.VarChar, 4)         '付帯項目５
                Dim P_ADDITEM6 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM6", MySqlDbType.VarChar, 4)         '付帯項目６
                Dim P_ADDITEM7 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM7", MySqlDbType.VarChar, 4)         '付帯項目７
                Dim P_ADDITEM8 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM8", MySqlDbType.VarChar, 4)         '付帯項目８
                Dim P_ADDITEM9 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM9", MySqlDbType.VarChar, 4)         '付帯項目９
                Dim P_ADDITEM10 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM10", MySqlDbType.VarChar, 4)         '付帯項目１０
                Dim P_ADDITEM11 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM11", MySqlDbType.VarChar, 4)         '付帯項目１１
                Dim P_ADDITEM12 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM12", MySqlDbType.VarChar, 4)         '付帯項目１２
                Dim P_ADDITEM13 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM13", MySqlDbType.VarChar, 4)         '付帯項目１３
                Dim P_ADDITEM14 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM14", MySqlDbType.VarChar, 4)         '付帯項目１４
                Dim P_ADDITEM15 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM15", MySqlDbType.VarChar, 4)         '付帯項目１５
                Dim P_ADDITEM16 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM16", MySqlDbType.VarChar, 4)         '付帯項目１６
                Dim P_ADDITEM17 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM17", MySqlDbType.VarChar, 4)         '付帯項目１７
                Dim P_ADDITEM18 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM18", MySqlDbType.VarChar, 4)         '付帯項目１８
                Dim P_ADDITEM19 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM19", MySqlDbType.VarChar, 4)         '付帯項目１９
                Dim P_ADDITEM20 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM20", MySqlDbType.VarChar, 4)         '付帯項目２０
                Dim P_ADDITEM21 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM21", MySqlDbType.VarChar, 4)         '付帯項目２１
                Dim P_ADDITEM22 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM22", MySqlDbType.VarChar, 4)         '付帯項目２２
                Dim P_ADDITEM23 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM23", MySqlDbType.VarChar, 4)         '付帯項目２３
                Dim P_ADDITEM24 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM24", MySqlDbType.VarChar, 4)         '付帯項目２４
                Dim P_ADDITEM25 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM25", MySqlDbType.VarChar, 4)         '付帯項目２５
                Dim P_ADDITEM26 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM26", MySqlDbType.VarChar, 4)         '付帯項目２６
                Dim P_ADDITEM27 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM27", MySqlDbType.VarChar, 4)         '付帯項目２７
                Dim P_ADDITEM28 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM28", MySqlDbType.VarChar, 4)         '付帯項目２８
                Dim P_ADDITEM29 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM29", MySqlDbType.VarChar, 4)         '付帯項目２９
                Dim P_ADDITEM30 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM30", MySqlDbType.VarChar, 4)         '付帯項目３０
                Dim P_ADDITEM31 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM31", MySqlDbType.VarChar, 4)         '付帯項目３１
                Dim P_ADDITEM32 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM32", MySqlDbType.VarChar, 4)         '付帯項目３２
                Dim P_ADDITEM33 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM33", MySqlDbType.VarChar, 4)         '付帯項目３３
                Dim P_ADDITEM34 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM34", MySqlDbType.VarChar, 4)         '付帯項目３４
                Dim P_ADDITEM35 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM35", MySqlDbType.VarChar, 4)         '付帯項目３５
                Dim P_ADDITEM36 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM36", MySqlDbType.VarChar, 4)         '付帯項目３６
                Dim P_ADDITEM37 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM37", MySqlDbType.VarChar, 4)         '付帯項目３７
                Dim P_ADDITEM38 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM38", MySqlDbType.VarChar, 4)         '付帯項目３８
                Dim P_ADDITEM39 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM39", MySqlDbType.VarChar, 4)         '付帯項目３９
                Dim P_ADDITEM40 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM40", MySqlDbType.VarChar, 4)         '付帯項目４０
                Dim P_ADDITEM41 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM41", MySqlDbType.VarChar, 4)         '付帯項目４１
                Dim P_ADDITEM42 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM42", MySqlDbType.VarChar, 4)         '付帯項目４２
                Dim P_ADDITEM43 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM43", MySqlDbType.VarChar, 4)         '付帯項目４３
                Dim P_ADDITEM44 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM44", MySqlDbType.VarChar, 4)         '付帯項目４４
                Dim P_ADDITEM45 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM45", MySqlDbType.VarChar, 4)         '付帯項目４５
                Dim P_ADDITEM46 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM46", MySqlDbType.VarChar, 4)         '付帯項目４６
                Dim P_ADDITEM47 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM47", MySqlDbType.VarChar, 4)         '付帯項目４７
                Dim P_ADDITEM48 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM48", MySqlDbType.VarChar, 4)         '付帯項目４８
                Dim P_ADDITEM49 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM49", MySqlDbType.VarChar, 4)         '付帯項目４９
                Dim P_ADDITEM50 As MySqlParameter = SQLcmd.Parameters.Add("@ADDITEM50", MySqlDbType.VarChar, 4)         '付帯項目５０
                Dim P_FLOORMATERIAL As MySqlParameter = SQLcmd.Parameters.Add("@FLOORMATERIAL", MySqlDbType.VarChar, 1)         '床材質コード
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                'DB更新
                P_CTNTYPE.Value = WW_ROW("CTNTYPE")                                  'コンテナ記号
                P_CTNNO.Value = WW_ROW("CTNNO")                                    'コンテナ番号
                If String.IsNullOrEmpty(WW_ROW("JURISDICTIONCD").ToString) Then    '所管部コード
                    P_JURISDICTIONCD.Value = "14"
                Else
                    P_JURISDICTIONCD.Value = WW_ROW("JURISDICTIONCD")
                End If
                P_ACCOUNTINGASSETSCD.Value = WW_ROW("ACCOUNTINGASSETSCD")                       '経理資産コード
                P_ACCOUNTINGASSETSKBN.Value = WW_ROW("ACCOUNTINGASSETSKBN")                      '経理資産区分
                If String.IsNullOrEmpty(WW_ROW("DUMMYKBN").ToString) Then          'ダミー区分
                    P_DUMMYKBN.Value = "00"
                Else
                    P_DUMMYKBN.Value = WW_ROW("DUMMYKBN")
                End If
                If String.IsNullOrEmpty(WW_ROW("SPOTKBN").ToString) Then           'スポット区分
                    P_SPOTKBN.Value = "00"
                Else
                    P_SPOTKBN.Value = WW_ROW("SPOTKBN")
                End If
                'スポット区分　開始年月日
                If Not WW_ROW("SPOTSTYMD") = Date.MinValue Then
                    P_SPOTSTYMD.Value = WW_ROW("SPOTSTYMD")
                Else
                    P_SPOTSTYMD.Value = DBNull.Value
                End If
                'スポット区分　終了年月日
                If Not WW_ROW("SPOTENDYMD") = Date.MinValue Then
                    P_SPOTENDYMD.Value = WW_ROW("SPOTENDYMD")
                Else
                    P_SPOTENDYMD.Value = DBNull.Value
                End If
                P_BIGCTNCD.Value = WW_ROW("BIGCTNCD")                                 '大分類コード
                P_MIDDLECTNCD.Value = WW_ROW("MIDDLECTNCD")                              '中分類コード
                P_SMALLCTNCD.Value = WW_ROW("SMALLCTNCD")                               '小分類コード
                P_CONSTRUCTIONYM.Value = WW_ROW("CONSTRUCTIONYM")                           '建造年月
                P_CTNMAKER.Value = WW_ROW("CTNMAKER")                                 'コンテナメーカー
                If String.IsNullOrEmpty(WW_ROW("FROZENMAKER").ToString) Then       '冷凍機メーカー
                    P_FROZENMAKER.Value = "000"
                Else
                    P_FROZENMAKER.Value = WW_ROW("FROZENMAKER")
                End If
                If String.IsNullOrEmpty(WW_ROW("GROSSWEIGHT").ToString) Then       '総重量
                    P_GROSSWEIGHT.Value = 0
                Else
                    P_GROSSWEIGHT.Value = WW_ROW("GROSSWEIGHT")
                End If
                If String.IsNullOrEmpty(WW_ROW("CARGOWEIGHT").ToString) Then       '荷重
                    P_CARGOWEIGHT.Value = 0
                Else
                    P_CARGOWEIGHT.Value = WW_ROW("CARGOWEIGHT")
                End If
                If String.IsNullOrEmpty(WW_ROW("MYWEIGHT").ToString) Then          '自重
                    P_MYWEIGHT.Value = 0
                Else
                    P_MYWEIGHT.Value = WW_ROW("MYWEIGHT")
                End If
                If String.IsNullOrEmpty(WW_ROW("BOOKVALUE").ToString) Then             '簿価商品価格
                    P_BOOKVALUE.Value = 0
                Else
                    P_BOOKVALUE.Value = Val(WW_ROW("BOOKVALUE").ToString)
                End If
                If String.IsNullOrEmpty(WW_ROW("OUTHEIGHT").ToString) Then         '外寸・高さ
                    P_OUTHEIGHT.Value = "0"
                Else
                    P_OUTHEIGHT.Value = WW_ROW("OUTHEIGHT")
                End If
                If String.IsNullOrEmpty(WW_ROW("OUTWIDTH").ToString) Then          '外寸・幅
                    P_OUTWIDTH.Value = "0"
                Else
                    P_OUTWIDTH.Value = WW_ROW("OUTWIDTH")
                End If
                If String.IsNullOrEmpty(WW_ROW("OUTLENGTH").ToString) Then         '外寸・長さ
                    P_OUTLENGTH.Value = "0"
                Else
                    P_OUTLENGTH.Value = WW_ROW("OUTLENGTH")
                End If
                If String.IsNullOrEmpty(WW_ROW("INHEIGHT").ToString) Then          '内寸・高さ
                    P_INHEIGHT.Value = "0"
                Else
                    P_INHEIGHT.Value = WW_ROW("INHEIGHT")
                End If
                If String.IsNullOrEmpty(WW_ROW("INWIDTH").ToString) Then           '内寸・幅
                    P_INWIDTH.Value = "0"
                Else
                    P_INWIDTH.Value = WW_ROW("INWIDTH")
                End If
                If String.IsNullOrEmpty(WW_ROW("INLENGTH").ToString) Then          '内寸・長さ
                    P_INLENGTH.Value = "0"
                Else
                    P_INLENGTH.Value = WW_ROW("INLENGTH")
                End If
                If String.IsNullOrEmpty(WW_ROW("WIFEHEIGHT").ToString) Then        '妻入口・高さ
                    P_WIFEHEIGHT.Value = "0"
                Else
                    P_WIFEHEIGHT.Value = WW_ROW("WIFEHEIGHT")
                End If
                If String.IsNullOrEmpty(WW_ROW("WIFEWIDTH").ToString) Then         '妻入口・幅
                    P_WIFEWIDTH.Value = "0"
                Else
                    P_WIFEWIDTH.Value = WW_ROW("WIFEWIDTH")
                End If
                If String.IsNullOrEmpty(WW_ROW("SIDEHEIGHT").ToString) Then        '側入口・高さ
                    P_SIDEHEIGHT.Value = "0"
                Else
                    P_SIDEHEIGHT.Value = WW_ROW("SIDEHEIGHT")
                End If
                If String.IsNullOrEmpty(WW_ROW("SIDEWIDTH").ToString) Then         '側入口・幅
                    P_SIDEWIDTH.Value = "0"
                Else
                    P_SIDEWIDTH.Value = WW_ROW("SIDEWIDTH")
                End If
                If String.IsNullOrEmpty(WW_ROW("FLOORAREA").ToString) Then         '床面積
                    P_FLOORAREA.Value = "0"
                Else
                    P_FLOORAREA.Value = WW_ROW("FLOORAREA")
                End If
                If String.IsNullOrEmpty(WW_ROW("INVOLUMEMARKING").ToString) Then   '内容積・標記
                    P_INVOLUMEMARKING.Value = "0"
                Else
                    P_INVOLUMEMARKING.Value = WW_ROW("INVOLUMEMARKING")
                End If
                If String.IsNullOrEmpty(WW_ROW("INVOLUMEACTUA").ToString) Then     '内容積・実寸
                    P_INVOLUMEACTUA.Value = 0
                Else
                    P_INVOLUMEACTUA.Value = WW_ROW("INVOLUMEACTUA")
                End If
                If String.IsNullOrEmpty(WW_ROW("TRAINSCYCLEDAYS").ToString) Then   '交番検査・サイクル日数
                    P_TRAINSCYCLEDAYS.Value = prmTRAINSCYCLEDAYS
                Else
                    P_TRAINSCYCLEDAYS.Value = WW_ROW("TRAINSCYCLEDAYS")
                End If
                '交番検査・前回実施日
                If Not WW_ROW("TRAINSBEFORERUNYMD") = Date.MinValue Then
                    P_TRAINSBEFORERUNYMD.Value = WW_ROW("TRAINSBEFORERUNYMD")
                Else
                    P_TRAINSBEFORERUNYMD.Value = DBNull.Value
                End If
                '交番検査・次回実施日
                If Not WW_ROW("TRAINSNEXTRUNYMD") = Date.MinValue Then
                    P_TRAINSNEXTRUNYMD.Value = WW_ROW("TRAINSNEXTRUNYMD")
                Else
                    P_TRAINSNEXTRUNYMD.Value = DBNull.Value
                End If
                If String.IsNullOrEmpty(WW_ROW("REGINSCYCLEDAYS").ToString) Then            '定期検査・ｻｲｸﾙ月数
                    P_REGINSCYCLEDAYS.Value = "0"
                Else
                    P_REGINSCYCLEDAYS.Value = WW_ROW("REGINSCYCLEDAYS")
                End If
                If String.IsNullOrEmpty(WW_ROW("REGINSCYCLEHOURMETER").ToString) Then       '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
                    P_REGINSCYCLEHOURMETER.Value = "0"
                Else
                    P_REGINSCYCLEHOURMETER.Value = WW_ROW("REGINSCYCLEHOURMETER")
                End If
                '定期検査・前回実施日
                If Not WW_ROW("REGINSBEFORERUNYMD") = Date.MinValue Then
                    P_REGINSBEFORERUNYMD.Value = WW_ROW("REGINSBEFORERUNYMD")
                Else
                    P_REGINSBEFORERUNYMD.Value = DBNull.Value
                End If
                '定期検査・次回実施日
                If Not WW_ROW("REGINSNEXTRUNYMD") = Date.MinValue Then
                    P_REGINSNEXTRUNYMD.Value = WW_ROW("REGINSNEXTRUNYMD")
                Else
                    P_REGINSNEXTRUNYMD.Value = DBNull.Value
                End If
                '定期検査・ｱﾜﾒｰﾀ記載日
                If Not WW_ROW("REGINSHOURMETERYMD") = Date.MinValue Then
                    P_REGINSHOURMETERYMD.Value = WW_ROW("REGINSHOURMETERYMD")
                Else
                    P_REGINSHOURMETERYMD.Value = DBNull.Value
                End If
                If String.IsNullOrEmpty(WW_ROW("REGINSHOURMETERTIME").ToString) Then        '定期検査・ｱﾜﾒｰﾀ時間
                    P_REGINSHOURMETERTIME.Value = "0"
                Else
                    P_REGINSHOURMETERTIME.Value = WW_ROW("REGINSHOURMETERTIME")
                End If
                If String.IsNullOrEmpty(WW_ROW("REGINSHOURMETERDSP").ToString) Then         '定期検査・ｱﾜﾒｰﾀ表示桁
                    P_REGINSHOURMETERDSP.Value = prmREGINSHOURMETERDSP
                Else
                    P_REGINSHOURMETERDSP.Value = WW_ROW("REGINSHOURMETERDSP")
                End If
                '運用開始年月日
                If Not WW_ROW("OPERATIONSTYMD") = Date.MinValue Then
                    P_OPERATIONSTYMD.Value = WW_ROW("OPERATIONSTYMD")
                Else
                    P_OPERATIONSTYMD.Value = DBNull.Value
                End If
                '運用除外年月日
                If Not WW_ROW("OPERATIONENDYMD") = Date.MinValue Then
                    P_OPERATIONENDYMD.Value = WW_ROW("OPERATIONENDYMD")
                Else
                    P_OPERATIONENDYMD.Value = DBNull.Value
                End If
                '除却年月日
                If Not WW_ROW("RETIRMENTYMD") = Date.MinValue Then
                    P_RETIRMENTYMD.Value = WW_ROW("RETIRMENTYMD")
                Else
                    P_RETIRMENTYMD.Value = DBNull.Value
                End If
                If String.IsNullOrEmpty(WW_ROW("COMPKANKBN").ToString) Then    '複合一貫区分
                    P_COMPKANKBN.Value = "00"
                Else
                    P_COMPKANKBN.Value = WW_ROW("COMPKANKBN")
                End If
                If String.IsNullOrEmpty(WW_ROW("SUPPLYFLG").ToString) Then             '調達フラグ
                    P_SUPPLYFLG.Value = "0"
                Else
                    P_SUPPLYFLG.Value = WW_ROW("SUPPLYFLG").ToString
                End If

                ' 付帯項目１～５０(初期値セット)
                For intItemCnt As Integer = 1 To 50
                    Dim strItemName As String = "ADDITEM" & CStr(intItemCnt)

                    '付帯項目
                    If String.IsNullOrEmpty(WW_ROW(strItemName).ToString) Then
                        WW_ROW(strItemName) = "00"
                        If intItemCnt = 11 OrElse intItemCnt = 34 Then
                            WW_ROW(strItemName) = "000"
                        End If
                    Else
                        WW_ROW(strItemName) = WW_ROW(strItemName).ToString
                    End If
                Next

                P_ADDITEM1.Value = WW_ROW("ADDITEM1")               '付帯項目１
                P_ADDITEM2.Value = WW_ROW("ADDITEM2")               '付帯項目２
                P_ADDITEM3.Value = WW_ROW("ADDITEM3")               '付帯項目３
                P_ADDITEM4.Value = WW_ROW("ADDITEM4")               '付帯項目４
                P_ADDITEM5.Value = WW_ROW("ADDITEM5")               '付帯項目５
                P_ADDITEM6.Value = WW_ROW("ADDITEM6")               '付帯項目６
                P_ADDITEM7.Value = WW_ROW("ADDITEM7")               '付帯項目７
                P_ADDITEM8.Value = WW_ROW("ADDITEM8")               '付帯項目８
                P_ADDITEM9.Value = WW_ROW("ADDITEM9")               '付帯項目９
                P_ADDITEM10.Value = WW_ROW("ADDITEM10")               '付帯項目１０
                P_ADDITEM11.Value = WW_ROW("ADDITEM11")               '付帯項目１１
                P_ADDITEM12.Value = WW_ROW("ADDITEM12")               '付帯項目１２
                P_ADDITEM13.Value = WW_ROW("ADDITEM13")               '付帯項目１３
                P_ADDITEM14.Value = WW_ROW("ADDITEM14")               '付帯項目１４
                P_ADDITEM15.Value = WW_ROW("ADDITEM15")               '付帯項目１５
                P_ADDITEM16.Value = WW_ROW("ADDITEM16")               '付帯項目１６
                P_ADDITEM17.Value = WW_ROW("ADDITEM17")               '付帯項目１７
                P_ADDITEM18.Value = WW_ROW("ADDITEM18")               '付帯項目１８
                P_ADDITEM19.Value = WW_ROW("ADDITEM19")               '付帯項目１９
                P_ADDITEM20.Value = WW_ROW("ADDITEM20")               '付帯項目２０
                P_ADDITEM21.Value = WW_ROW("ADDITEM21")               '付帯項目２１
                P_ADDITEM22.Value = WW_ROW("ADDITEM22")               '付帯項目２２
                P_ADDITEM23.Value = WW_ROW("ADDITEM23")               '付帯項目２３
                P_ADDITEM24.Value = WW_ROW("ADDITEM24")               '付帯項目２４
                P_ADDITEM25.Value = WW_ROW("ADDITEM25")               '付帯項目２５
                P_ADDITEM26.Value = WW_ROW("ADDITEM26")               '付帯項目２６
                P_ADDITEM27.Value = WW_ROW("ADDITEM27")               '付帯項目２７
                P_ADDITEM28.Value = WW_ROW("ADDITEM28")               '付帯項目２８
                P_ADDITEM29.Value = WW_ROW("ADDITEM29")               '付帯項目２９
                P_ADDITEM30.Value = WW_ROW("ADDITEM30")               '付帯項目３０
                P_ADDITEM31.Value = WW_ROW("ADDITEM31")               '付帯項目３１
                P_ADDITEM32.Value = WW_ROW("ADDITEM32")               '付帯項目３２
                P_ADDITEM33.Value = WW_ROW("ADDITEM33")               '付帯項目３３
                P_ADDITEM34.Value = WW_ROW("ADDITEM34")               '付帯項目３４
                P_ADDITEM35.Value = WW_ROW("ADDITEM35")               '付帯項目３５
                P_ADDITEM36.Value = WW_ROW("ADDITEM36")               '付帯項目３６
                P_ADDITEM37.Value = WW_ROW("ADDITEM37")               '付帯項目３７
                P_ADDITEM38.Value = WW_ROW("ADDITEM38")               '付帯項目３８
                P_ADDITEM39.Value = WW_ROW("ADDITEM39")               '付帯項目３９
                P_ADDITEM40.Value = WW_ROW("ADDITEM40")               '付帯項目４０
                P_ADDITEM41.Value = WW_ROW("ADDITEM41")               '付帯項目４１
                P_ADDITEM42.Value = WW_ROW("ADDITEM42")               '付帯項目４２
                P_ADDITEM43.Value = WW_ROW("ADDITEM43")               '付帯項目４３
                P_ADDITEM44.Value = WW_ROW("ADDITEM44")               '付帯項目４４
                P_ADDITEM45.Value = WW_ROW("ADDITEM45")               '付帯項目４５
                P_ADDITEM46.Value = WW_ROW("ADDITEM46")               '付帯項目４６
                P_ADDITEM47.Value = WW_ROW("ADDITEM47")               '付帯項目４７
                P_ADDITEM48.Value = WW_ROW("ADDITEM48")               '付帯項目４８
                P_ADDITEM49.Value = WW_ROW("ADDITEM49")               '付帯項目４９
                P_ADDITEM50.Value = WW_ROW("ADDITEM50")               '付帯項目５０
                If String.IsNullOrEmpty(WW_ROW("FLOORMATERIAL").ToString) Then         '床材質コード
                    P_FLOORMATERIAL.Value = DBNull.Value
                Else
                    P_FLOORMATERIAL.Value = WW_ROW("FLOORMATERIAL").ToString
                End If
                If String.IsNullOrEmpty(WW_ROW("DELFLG").ToString) Then            '削除フラグ
                    P_DELFLG.Value = C_DELETE_FLG.ALIVE
                Else
                    P_DELFLG.Value = WW_ROW("DELFLG")
                End If

                P_INITYMD.Value = WW_DATENOW                '登録年月日
                P_INITUSER.Value = Master.USERID               '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID               '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name          '登録プログラムＩＤ
                P_UPDYMD.Value = WW_DATENOW                '更新年月日
                P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0002_RECONM  INSERTUPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNM0002_RECONM  INSERTUPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            WW_ERR_SW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' コンテナマスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Private Function UpdateZaiko(ByVal SQLcon As MySqlConnection, ByVal LNM0002row As DataRow, ByVal zaikokbn As String, ByVal WW_DATENOW As DateTime) As String

        UpdateZaiko = Messages.C_MESSAGE_NO.NORMAL

        'Dim WW_DATENOW As DateTime = Date.Now
        Dim dtKeijyo As DateTime
        Dim htDetailDataWKParm As New Hashtable
        Dim strKeijyoYM As String = ""
        Dim strCtnType As String = ""
        Dim strCtnNo As String = ""
        Dim strStationCd As String = ""
        Dim strOrgCd As String = ""
        Dim strGovernOrgCd As String = ""
        Dim strCtnStatus As String = ""
        Dim strCONFIRMFLG As String = ""
        Dim strINVOICEORGCODE As String = ""
        Dim strOPERATIONENDYMD As String = ""
        Dim strRefDISPOSALFLG As String = ""
        Dim strDISPOSALFLG As String = ""
        Dim strBefCtnStatus As String = ""
        Dim intInsCnt As Integer = 0
        Dim intGenkyouCnt As Integer = 0

        Try
            '原価確定状態テーブル 計上年月取得処理
            LNM0002WRKINC.GetKeijyoYYYYMM(SQLcon, Nothing, "0", strKeijyoYM)
            If strKeijyoYM.Trim = "" Then
                LNM0002WRKINC.GetKeijyoYYYYMM(SQLcon, Nothing, "1", strKeijyoYM)
                If strKeijyoYM.Trim = "" Then
                    strKeijyoYM = WW_DATENOW.ToString("yyyyMM")
                Else
                    dtKeijyo = DateTime.ParseExact(strKeijyoYM & "01", "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None)
                    dtKeijyo = dtKeijyo.AddMonths(1)
                    strKeijyoYM = dtKeijyo.ToString("yyyyMM")
                End If
            End If

            strCtnType = LNM0002row("CTNTYPE").ToString
            strCtnNo = LNM0002row("CTNNO").ToString
            strOPERATIONENDYMD = LNM0002row("OPERATIONENDYMD").ToString
            If zaikokbn = "1" Then
                strCtnStatus = C_CONTSTATUSKBN.KBN_HIKIAI_WAIT
            ElseIf zaikokbn = "2" Then
                strCtnStatus = C_CONTSTATUSKBN.KBN_EIGYOGAI_HIKIAI_WAIT
            End If
            strDISPOSALFLG = "0"

            '駅コード取得処理(現況表)
            LNM0002WRKINC.GetStation(SQLcon, Nothing, strCtnType, strCtnNo, intGenkyouCnt, strStationCd, strBefCtnStatus)

            '計上支店管轄支店取得処理
            LNM0002WRKINC.GetOrgGovernCode(SQLcon, Nothing, strStationCd, strOrgCd, strGovernOrgCd)

            'コンテナ在庫テーブル 件数取得処理
            LNM0002WRKINC.GetCtnStockCnt(SQLcon, Nothing, strKeijyoYM, strCtnType, strCtnNo,
                                            intInsCnt, strCONFIRMFLG, strINVOICEORGCODE, strRefDISPOSALFLG)

            '■在庫テーブル パラメータ設定処理
            htDetailDataWKParm = SetCtnStockParam(WW_DATENOW, strKeijyoYM, strCtnType, strCtnNo,
                                                       strOrgCd, strGovernOrgCd, strStationCd,
                                                       strCtnStatus, strOPERATIONENDYMD, strDISPOSALFLG)
            '現況表が存在し、在庫が存在しない場合
            If intGenkyouCnt > 0 AndAlso intInsCnt = 0 Then
                Using tran = SQLcon.BeginTransaction
                    'コンテナ状態に変更がある場合
                    If strBefCtnStatus <> strCtnStatus Then
                        '■現況表テーブル 更新処理
                        LNM0002WRKINC.UpdatePresenttateData(SQLcon, tran, htDetailDataWKParm)
                        '■コンテナステータス履歴ファイル 登録処理
                        LNM0002WRKINC.InsertCtnStatusData(SQLcon, tran, htDetailDataWKParm)
                    End If

                    '■在庫テーブル 削除処理
                    LNM0002WRKINC.DeleteCtnStockData(SQLcon, tran, htDetailDataWKParm)
                    '■在庫テーブル 登録処理
                    LNM0002WRKINC.InsertCtnStockData(SQLcon, tran, htDetailDataWKParm)

                    'トランザクションコミット
                    tran.Commit()
                End Using
            End If

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0002L UPDATE_ZAIKO")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0002L UPDATE_ZAIKO"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            UpdateZaiko = C_MESSAGE_NO.DB_ERROR
        End Try

    End Function

    ''' <summary>
    ''' 在庫テーブル パラメータ設定処理
    ''' </summary>
    ''' <param name="dtSysDateTime">システム日付</param>
    ''' <param name="prmCtnType">コンテナ形式</param>
    ''' <param name="prmCtnNo">コンテナ番号</param>
    ''' <param name="prmKeijyoOrg">計上支店</param>
    ''' <param name="prmBRANCHCODE">管轄支店</param>
    ''' <param name="prmSTATIONCODE">現在駅</param>
    ''' <param name="prmSTOCKSTATUS">在庫状態</param>
    ''' <param name="prmEXCEPTIONDATE">運用除外日</param>
    ''' <param name="prmDISPOSALFLG">在庫処分フラグ</param>
    ''' <returns>在庫テーブル 設定したパラメータ</returns>
    ''' <remarks></remarks>
    Private Function SetCtnStockParam(ByVal dtSysDateTime As DateTime,
                                       ByVal prmKeijoYM As String,
                                       ByVal prmCtnType As String,
                                       ByVal prmCtnNo As String,
                                       ByVal prmKeijyoOrg As String,
                                       ByVal prmBRANCHCODE As String,
                                       ByVal prmSTATIONCODE As String,
                                       ByVal prmSTOCKSTATUS As String,
                                       ByVal prmEXCEPTIONDATE As String,
                                       ByVal prmDISPOSALFLG As String) As Hashtable

        Dim htWKTbl As New Hashtable
        Dim htZeritData As New Hashtable
        Dim strTaxRate As String = ""
        Dim strAutoUpd As String = ""
        Dim strContraLNType As String = ""

        htWKTbl(ZAIKO_DP.CS_KEIJOYM) = prmKeijoYM                   '計上年月
        htWKTbl(ZAIKO_DP.CS_CTNTYPE) = prmCtnType                   'コンテナ形式
        htWKTbl(ZAIKO_DP.CS_CTNNO) = prmCtnNo                       'コンテナ番号
        htWKTbl(ZAIKO_DP.CS_INVOICEKEIJYOBRANCHCODE) = prmKeijyoOrg '計上支店
        htWKTbl(ZAIKO_DP.CS_STATIONCODE) = prmSTATIONCODE           '現在駅
        htWKTbl(ZAIKO_DP.CS_STOCKSTATUS) = prmSTOCKSTATUS           '在庫状態
        htWKTbl(ZAIKO_DP.CS_STOCKREGISTRATIONDATE) = dtSysDateTime.ToString("yyyy/MM/dd") '在庫登録日
        htWKTbl(ZAIKO_DP.CS_EXCEPTIONDATE) = prmEXCEPTIONDATE       '運用除外日
        htWKTbl(ZAIKO_DP.CS_STOCKREGISTRATID) = Master.USERID       '在庫登録者
        htWKTbl(ZAIKO_DP.CS_DISPOSALFLG) = prmDISPOSALFLG           '在庫処分フラグ

        '登録ユーザ、作成年月日
        htWKTbl(ZAIKO_DP.CS_INITYMD) = dtSysDateTime                 '登録年月日
        htWKTbl(ZAIKO_DP.CS_INITUSER) = Master.USERID                '登録ユーザーＩＤ
        htWKTbl(ZAIKO_DP.CS_INITTERMID) = Master.USERTERMID          '登録端末
        htWKTbl(ZAIKO_DP.CS_INITPGID) = Me.GetType().BaseType.Name   '登録プログラムＩＤ
        htWKTbl(ZAIKO_DP.CS_UPDYMD) = dtSysDateTime                  '更新年月日
        htWKTbl(ZAIKO_DP.CS_UPDUSER) = Master.USERID                 '更新ユーザーＩＤ
        htWKTbl(ZAIKO_DP.CS_UPDTERMID) = Master.USERTERMID           '更新端末
        htWKTbl(ZAIKO_DP.CS_UPDPGID) = Me.GetType().BaseType.Name    '更新プログラムＩＤ

        Return htWKTbl

    End Function

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
        Dim WW_DBDataCheck As String = ""
        Dim WW_KobanErrSW As String

        WW_LineErr = ""

        Dim WW_ConstructionYMD As String = ""

        ' 削除フラグ(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "DELFLG", WW_ROW("DELFLG").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("DELFLG", WW_ROW("DELFLG").ToString, WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・削除コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・削除コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' コンテナ記号(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CTNTYPE", WW_ROW("CTNTYPE").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・コンテナ記号エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' コンテナ番号(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CTNNO", WW_ROW("CTNNO").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・コンテナ番号エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 所管部コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "JURISDICTIONCD", WW_ROW("JURISDICTIONCD").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("JURISDICTION", WW_ROW("JURISDICTIONCD").ToString, WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・所管部コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・所管部コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 経理資産コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ACCOUNTINGASSETSCD", WW_ROW("ACCOUNTINGASSETSCD").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("ACCOUNTINGASSETSCD").ToString) Then
                ' 名称存在チェック
                CODENAME_get("ACCOUNTINGASSETSCD", WW_ROW("ACCOUNTINGASSETSCD").ToString, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・経理資産コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・経理資産コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 経理資産区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "ACCOUNTINGASSETSKBN", WW_ROW("ACCOUNTINGASSETSKBN").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("ACCOUNTINGASSETSKBN", WW_ROW("ACCOUNTINGASSETSKBN").ToString, WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・経理資産区分エラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・経理資産区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' ダミー区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "DUMMYKBN", WW_ROW("DUMMYKBN").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("DUMMYKBN").ToString) Then
                ' 名称存在チェック
                CODENAME_get("DUMMYKBN", WW_ROW("DUMMYKBN").ToString, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・ダミー区分エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・ダミー区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' スポット区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SPOTKBN", WW_ROW("SPOTKBN").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SPOTKBN").ToString) Then
                ' 名称存在チェック
                CODENAME_get("SPOTKBN", WW_ROW("SPOTKBN").ToString, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・スポット区分エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・スポット区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 入力値チェック(経理資産区分・スポット区分)
        If WW_ROW("ACCOUNTINGASSETSKBN").ToString = WW_Kbn01 AndAlso WW_ROW("SPOTKBN").ToString = WW_Kbn02 OrElse
             WW_ROW("ACCOUNTINGASSETSKBN").ToString = WW_Kbn02 AndAlso WW_ROW("SPOTKBN").ToString = WW_Kbn01 Then
            WW_CheckMES1 = "・経理資産区分＆スポット区分エラー"
            WW_CheckMES2 = "同じ区分は入力出来ません。"
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        Dim blnSpotStart As Boolean = False
        Dim blnSpotEnd As Boolean = False
        ' スポット区分　開始年月日(バリデーションチェック)
        If Not WW_ROW("SPOTSTYMD") = Date.MinValue Then
            Master.CheckField(Master.USERCAMP, "SPOTSTYMD", WW_ROW("SPOTSTYMD").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' スポット区分入力時、必須チェック
                If Not String.IsNullOrEmpty(WW_ROW("SPOTKBN").ToString) AndAlso WW_ROW("SPOTKBN").ToString <> "00" Then
                    If String.IsNullOrEmpty(WW_ROW("SPOTSTYMD").ToString) Then
                        WW_CheckMES1 = "・スポット区分　開始年月日エラー"
                        WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        blnSpotStart = True
                    End If
                Else
                    If Not String.IsNullOrEmpty(WW_ROW("SPOTSTYMD").ToString) Then
                        WW_ROW("SPOTSTYMD") = CDate(WW_ROW("SPOTSTYMD")).ToString("yyyy/MM/dd")
                    End If
                    blnSpotStart = True
                End If
            Else
                WW_CheckMES1 = "・スポット区分　開始年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' スポット区分　終了年月日(バリデーションチェック)
        If Not WW_ROW("SPOTENDYMD") = Date.MinValue Then
            Master.CheckField(Master.USERCAMP, "SPOTENDYMD", WW_ROW("SPOTENDYMD").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' スポット区分入力時、必須チェック
                If Not String.IsNullOrEmpty(WW_ROW("SPOTKBN").ToString) AndAlso WW_ROW("SPOTKBN").ToString <> "00" Then
                    If String.IsNullOrEmpty(WW_ROW("SPOTENDYMD").ToString) Then
                        WW_CheckMES1 = "・スポット区分　終了年月日エラー"
                        WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        blnSpotEnd = True
                    End If
                Else
                    If Not String.IsNullOrEmpty(WW_ROW("SPOTENDYMD").ToString) Then
                        ' 過去日入力チェック
                        If Date.Now > CDate(WW_ROW("SPOTENDYMD").ToString) And WW_ROW("SPOTENDYMD").ToString <> work.WF_SEL_SPOTENDYMD.Text Then
                            WW_CheckMES1 = "・スポット区分　終了年月日エラー"
                            WW_CheckMES2 = "過去日入力エラー"
                            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        Else
                            WW_ROW("SPOTENDYMD") = CDate(WW_ROW("SPOTENDYMD")).ToString("yyyy/MM/dd")
                            blnSpotEnd = True
                        End If
                    End If
                End If
            Else
                WW_CheckMES1 = "・スポット区分　終了年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        If blnSpotStart = True AndAlso blnSpotEnd = True Then
            ' 日付大小チェック(スポット区分　開始年月日・スポット区分　終了年月日)
            If Not String.IsNullOrEmpty(WW_ROW("SPOTSTYMD").ToString) AndAlso Not String.IsNullOrEmpty(WW_ROW("SPOTENDYMD").ToString) Then
                If CDate(WW_ROW("SPOTSTYMD").ToString) > CDate(WW_ROW("SPOTENDYMD").ToString) Then
                    WW_CheckMES1 = "・スポット区分　開始年月日＆スポット区分　終了年月日エラー"
                    WW_CheckMES2 = "日付大小入力エラー"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If

        ' 大分類コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "BIGCTNCD", WW_ROW("BIGCTNCD").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("BIGCTNCD", WW_ROW("BIGCTNCD").ToString, WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・大分類コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・大分類コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 中分類コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "MIDDLECTNCD", WW_ROW("MIDDLECTNCD").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("MIDDLECTNCD", WW_ROW("MIDDLECTNCD").ToString, WW_Dummy, WW_RtnSW, WW_ROW("BIGCTNCD").ToString)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・中分類コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・中分類コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 小分類コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SMALLCTNCD", WW_ROW("SMALLCTNCD").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("SMALLCTNCD", WW_ROW("SMALLCTNCD").ToString, WW_Dummy, WW_RtnSW, WW_ROW("BIGCTNCD").ToString, WW_ROW("MIDDLECTNCD").ToString)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・小分類コードエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・小分類コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        Dim blnCONSTRUCTIONYM As Boolean = False
        ' 建造年月(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CONSTRUCTIONYM", WW_ROW("CONSTRUCTIONYM").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 建造年月を年月日(YYYY/MM/DD)に変更(月初指定)
            If WW_ROW("CONSTRUCTIONYM").ToString.Length = 6 Then
                Dim strDateYmd As String = WW_ROW("CONSTRUCTIONYM").ToString
                strDateYmd = Left(strDateYmd, 4) & "/" & Right(strDateYmd, 2) & "/01"
                Dim dt As DateTime
                If DateTime.TryParse(strDateYmd, dt) Then
                    '変換出来たら、OK
                    WW_ConstructionYMD = strDateYmd
                    blnCONSTRUCTIONYM = True
                Else
                    WW_CheckMES1 = "・建造年月エラーです。"
                    WW_CheckMES2 = "入力値が不正です。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・建造年月エラーです。"
                WW_CheckMES2 = "入力値が不正です。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・建造年月エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' コンテナメーカー(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "CTNMAKER", WW_ROW("CTNMAKER").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            ' 名称存在チェック
            CODENAME_get("CTNMAKER", WW_ROW("CTNMAKER").ToString, WW_Dummy, WW_RtnSW)
            If Not isNormal(WW_RtnSW) Then
                WW_CheckMES1 = "・コンテナメーカーエラーです。"
                WW_CheckMES2 = "マスタに存在しません。"
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Else
            WW_CheckMES1 = "・コンテナメーカーエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 冷凍機メーカー(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "FROZENMAKER", WW_ROW("FROZENMAKER").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("FROZENMAKER").ToString) Then
                ' 名称存在チェック
                CODENAME_get("FROZENMAKER", WW_ROW("FROZENMAKER").ToString, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・冷凍機メーカーエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・冷凍機メーカーエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 総重量(バリデーションチェック)
        Dim blnGROSSWEIGHTErr As Boolean = False
        Master.CheckField(Master.USERCAMP, "GROSSWEIGHT", WW_ROW("GROSSWEIGHT").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・総重量エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            blnGROSSWEIGHTErr = True
        End If
        ' 荷重(バリデーションチェック)
        Dim blnCARGOWEIGHTErr As Boolean = False
        Master.CheckField(Master.USERCAMP, "CARGOWEIGHT", WW_ROW("CARGOWEIGHT").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・荷重エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            blnCARGOWEIGHTErr = True
        End If
        ' 自重(バリデーションチェック)
        Dim blnMYWEIGHTErr As Boolean = False
        Master.CheckField(Master.USERCAMP, "MYWEIGHT", WW_ROW("MYWEIGHT").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・自重エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            blnMYWEIGHTErr = True
        End If
        ' 重量大小チェック(荷重・自重)
        If blnCARGOWEIGHTErr = False AndAlso blnMYWEIGHTErr = False Then
            If Not String.IsNullOrEmpty(WW_ROW("CARGOWEIGHT").ToString) AndAlso Not String.IsNullOrEmpty(WW_ROW("MYWEIGHT").ToString) Then
                If CDbl(WW_ROW("CARGOWEIGHT")) < CDbl(WW_ROW("MYWEIGHT")) Then
                    WW_CheckMES1 = "・荷重＆自重エラー"
                    WW_CheckMES2 = "重量大小入力エラー"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If
        ' 重量大小チェック(総重量・荷重・自重)
        If blnGROSSWEIGHTErr = False AndAlso blnCARGOWEIGHTErr = False AndAlso blnMYWEIGHTErr = False Then
            If Not String.IsNullOrEmpty(WW_ROW("CARGOWEIGHT").ToString) Then
                If CDbl(WW_ROW("GROSSWEIGHT")) < (CDbl(WW_ROW("CARGOWEIGHT")) + CDbl(WW_ROW("MYWEIGHT"))) Then
                    WW_CheckMES1 = "・総重量＆荷重＋自重エラー"
                    WW_CheckMES2 = "重量大小入力エラー"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If
        ' 簿価商品価格(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "BOOKVALUE", WW_ROW("BOOKVALUE").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・簿価商品価格エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 外寸・高さ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "OUTHEIGHT", WW_ROW("OUTHEIGHT").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・外寸・高さエラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 外寸・幅(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "OUTWIDTH", WW_ROW("OUTWIDTH").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・外寸・幅エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 外寸・長さ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "OUTLENGTH", WW_ROW("OUTLENGTH").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・外寸・長さエラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 内寸・高さ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "INHEIGHT", WW_ROW("INHEIGHT").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・内寸・高さエラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 内寸・幅(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "INWIDTH", WW_ROW("INWIDTH").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・内寸・幅エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 内寸・長さ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "INLENGTH", WW_ROW("INLENGTH").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・内寸・長さエラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 妻入口・高さ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "WIFEHEIGHT", WW_ROW("WIFEHEIGHT").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・妻入口・高さエラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 妻入口・幅(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "WIFEWIDTH", WW_ROW("WIFEWIDTH").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・妻入口・幅エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 側入口・高さ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SIDEHEIGHT", WW_ROW("SIDEHEIGHT").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・側入口・高さエラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 側入口・幅(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SIDEWIDTH", WW_ROW("SIDEWIDTH").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・側入口・幅エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 床面積(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "FLOORAREA", WW_ROW("FLOORAREA").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・床面積エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 内容積・標記(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "INVOLUMEMARKING", WW_ROW("INVOLUMEMARKING").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・内容積・標記エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 内容積・実寸(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "INVOLUMEACTUA", WW_ROW("INVOLUMEACTUA").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・内容積・実寸エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 交番検査・ｻｲｸﾙ日数(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "TRAINSCYCLEDAYS", WW_ROW("TRAINSCYCLEDAYS").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・交番検査・ｻｲｸﾙ日数エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 定期検査・ｻｲｸﾙ月数(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "REGINSCYCLEDAYS", WW_ROW("REGINSCYCLEDAYS").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・定期検査・ｻｲｸﾙ月数エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 定期検査・ｻｲｸﾙｱﾜﾒｰﾀ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "REGINSCYCLEHOURMETER", WW_ROW("REGINSCYCLEHOURMETER").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・定期検査・ｻｲｸﾙｱﾜﾒｰﾀエラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 大分類コード＝"15"(冷蔵)時、入力値チェック(定期検査・ｻｲｸﾙ月数・定期検査・ｻｲｸﾙｱﾜﾒｰﾀ)
        If WW_ROW("BIGCTNCD").ToString = "15" AndAlso
                String.IsNullOrEmpty(WW_ROW("REGINSCYCLEDAYS")) AndAlso
                String.IsNullOrEmpty(WW_ROW("REGINSCYCLEHOURMETER")) Then
            WW_CheckMES1 = "・定期検査・ｻｲｸﾙ月数＆定期検査・ｻｲｸﾙｱﾜﾒｰﾀエラー"
            WW_CheckMES2 = "どちらかを入力してください。"
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 定期検査・ｱﾜﾒｰﾀ記載日(バリデーションチェック)
        If Not WW_ROW("REGINSHOURMETERYMD") = Date.MinValue Then
            Master.CheckField(Master.USERCAMP, "REGINSHOURMETERYMD", WW_ROW("REGINSHOURMETERYMD").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・定期検査・ｱﾜﾒｰﾀ記載日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 定期検査・ｱﾜﾒｰﾀ時間(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "REGINSHOURMETERTIME", WW_ROW("REGINSHOURMETERTIME").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・定期検査・ｱﾜﾒｰﾀ時間エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 定期検査・ｱﾜﾒｰﾀ表示桁(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "REGINSHOURMETERDSP", WW_ROW("REGINSHOURMETERDSP").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・定期検査・ｱﾜﾒｰﾀ表示桁エラー"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        Else
            ' 入力値チェック
            If WW_ROW("REGINSHOURMETERDSP").ToString = "0" OrElse
                String.IsNullOrEmpty(WW_ROW("REGINSHOURMETERDSP").ToString) Then
                WW_ROW("REGINSHOURMETERDSP") = WW_DefaultReginsHourMeterDsp
            End If
        End If
        ' 運用開始年月日(バリデーションチェック)
        Dim blnOperationStart As Boolean = False
        If Not WW_ROW("OPERATIONSTYMD") = Date.MinValue Then
            Master.CheckField(Master.USERCAMP, "OPERATIONSTYMD", WW_ROW("OPERATIONSTYMD").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("OPERATIONSTYMD").ToString) Then
                    WW_ROW("OPERATIONSTYMD") = CDate(WW_ROW("OPERATIONSTYMD")).ToString("yyyy/MM/dd")
                End If
                blnOperationStart = True
            Else
                WW_CheckMES1 = "・運用開始年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 日付大小チェック(建造年月(01日)・運用開始年月日)
        If Not String.IsNullOrEmpty(WW_ROW("CONSTRUCTIONYM").ToString) AndAlso Not String.IsNullOrEmpty(WW_ROW("OPERATIONSTYMD").ToString) Then
            If blnCONSTRUCTIONYM = True AndAlso blnOperationStart = True Then
                If CDate(WW_ConstructionYMD) > CDate(WW_ROW("OPERATIONSTYMD")) Then
                    WW_CheckMES1 = "・建造年月＆運用開始年月日エラー"
                    WW_CheckMES2 = "日付大小入力エラー"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If
        ' 運用除外年月日(バリデーションチェック)
        Dim blnOperationEnd As Boolean = False
        If Not WW_ROW("OPERATIONENDYMD") = Date.MinValue Then
            Master.CheckField(Master.USERCAMP, "OPERATIONENDYMD", WW_ROW("OPERATIONENDYMD").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("OPERATIONENDYMD").ToString) Then
                    WW_ROW("OPERATIONENDYMD") = CDate(WW_ROW("OPERATIONENDYMD")).ToString("yyyy/MM/dd")
                End If
                blnOperationEnd = True
            Else
                WW_CheckMES1 = "・運用除外年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 日付大小チェック(運用開始年月日・運用除外年月日)
        'If Not String.IsNullOrEmpty(WW_ROW("OPERATIONSTYMD").ToString) AndAlso Not String.IsNullOrEmpty(WW_ROW("OPERATIONENDYMD").ToString) Then
        If Not WW_ROW("OPERATIONSTYMD") = Date.MinValue AndAlso Not WW_ROW("OPERATIONENDYMD") = Date.MinValue Then
            If blnOperationStart = True AndAlso blnOperationEnd = True Then
                If CDate(WW_ROW("OPERATIONSTYMD")) > CDate(WW_ROW("OPERATIONENDYMD")) Then
                    WW_CheckMES1 = "・運用開始年月日＆運用除外年月日エラー"
                    WW_CheckMES2 = "日付大小入力エラー"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If
        ' 除却年月日(バリデーションチェック)
        Dim blnRetirment As Boolean = False
        If Not WW_ROW("RETIRMENTYMD") = Date.MinValue Then
            Master.CheckField(Master.USERCAMP, "RETIRMENTYMD", WW_ROW("RETIRMENTYMD").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW("RETIRMENTYMD").ToString) Then
                    WW_ROW("RETIRMENTYMD") = CDate(WW_ROW("RETIRMENTYMD")).ToString("yyyy/MM/dd")
                End If
                blnRetirment = True
            Else
                WW_CheckMES1 = "・除却年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        End If
        ' 日付大小チェック(運用除外年月日・除却年月日)
        'If Not String.IsNullOrEmpty(WW_ROW("OPERATIONENDYMD").ToString) AndAlso Not String.IsNullOrEmpty(WW_ROW("RETIRMENTYMD").ToString) Then
        If Not WW_ROW("OPERATIONENDYMD") = Date.MinValue AndAlso Not WW_ROW("RETIRMENTYMD") = Date.MinValue Then
            If blnOperationEnd = True AndAlso blnRetirment = True Then
                If CDate(WW_ROW("OPERATIONENDYMD")) > CDate(WW_ROW("RETIRMENTYMD")) Then
                    WW_CheckMES1 = "・運用除外年月日＆除却年月日エラー"
                    WW_CheckMES2 = "日付大小入力エラー"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        End If
        ' 複合一貫区分(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "COMPKANKBN", WW_ROW("COMPKANKBN").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("COMPKANKBN").ToString) Then
                ' 名称存在チェック
                CODENAME_get("COMPKANKBN", WW_ROW("COMPKANKBN").ToString, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・複合一貫区分エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・複合一貫区分エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If
        ' 調達フラグ(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "SUPPLYFLG", WW_ROW("SUPPLYFLG").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("SUPPLYFLG").ToString) Then
                ' 名称存在チェック
                CODENAME_get("SUPPLYFLG", WW_ROW("SUPPLYFLG").ToString, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・調達フラグエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・調達フラグエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 付帯項目１～５０(バリデーションチェック)
        For intItemCnt As Integer = 1 To 50
            Dim strItemName As String = "ADDITEM" & CStr(intItemCnt)
            Dim strZenkaku As String = StrConv(CStr(intItemCnt), VbStrConv.Wide)
            Dim strItemMsg As String = "・付帯項目" & strZenkaku & "エラーです。"

            ' 付帯項目(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, strItemName, WW_ROW(strItemName).ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(WW_ROW(strItemName).ToString) Then
                    ' 名称存在チェック
                    CODENAME_get(strItemName, WW_ROW(strItemName).ToString, WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = strItemMsg
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = strItemMsg
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Next

        ' 床材質コード(バリデーションチェック)
        Master.CheckField(Master.USERCAMP, "FLOORMATERIAL", WW_ROW("FLOORMATERIAL").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If isNormal(WW_CS0024FCheckerr) Then
            If Not String.IsNullOrEmpty(WW_ROW("FLOORMATERIAL").ToString) Then
                ' 名称存在チェック
                CODENAME_get("FLOORMATERIAL", WW_ROW("FLOORMATERIAL").ToString, WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・床材質コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
        Else
            WW_CheckMES1 = "・床材質コードエラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        ' 交番検査・ｻｲｸﾙ日数 自動計算値同値チェック
        WW_KobanErrSW = ""
        KobanCheck(SQLcon, WW_ROW("TRAINSCYCLEDAYS").ToString, WW_KobanErrSW)
        If Not WW_KobanErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
            WW_CheckMES1 = "・交番検査エラーです。"
            WW_CheckMES2 = "規定値未入力エラー"
            WW_CheckERR(WW_ROW("LINECNT"), WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

    End Sub

    ''' <summary>
    ''' 自動計算値同値チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO"></param>
    Protected Sub KobanCheck(ByVal SQLcon As MySqlConnection, ByVal WW_TrainsCycleDays As String, ByRef O_MESSAGENO As String)

        Dim AutoValue As Integer = 0

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                 " _
            & "    CURSETVAL1          " _
            & "  , NEXTFROMYMD         " _
            & "  , NEXTSETVAL1         " _
            & " FROM                   " _
            & "     LNG.LNM0001_RECNTM " _
            & " WHERE                  " _
            & "         CNTKEY  = @P1  " _
            & "     And DELFLG  <> @P2 "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 5)  'コントロールキー(ＫＥＹ(交番検査))
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 1)  '削除フラグ

                PARA1.Value = WW_CntKey            'コントロールキー(ＫＥＹ(交番検査))
                PARA2.Value = C_DELETE_FLG.DELETE  '削除フラグ

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0002Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0002Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    LNM0002Chk.Load(SQLdr)

                    For Each LNM0002row As DataRow In LNM0002Chk.Rows
                        If LNM0002row("NEXTFROMYMD").Equals(DBNull.Value) OrElse DateTime.Parse(LNM0002row("NEXTFROMYMD")) > DateTime.Parse(Date.Now) Then
                            ' 次期　適用年月日対象外
                            AutoValue = LNM0002row("CURSETVAL1")   '現行　設定値１
                        Else
                            ' 次期　適用年月日対象
                            AutoValue = LNM0002row("NEXTSETVAL1")  '次期　設定値１
                        End If
                    Next
                End Using

                If Integer.Parse(WW_TrainsCycleDays) = AutoValue Then
                    ' 正常終了時
                    O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                Else
                    ' 自動計算値と異なる場合
                    O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_KOBANCYCLE_ERR
                End If


            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0002C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB: LNM0002C TRAINSCYCLEDAYS_CHECK"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

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

    ''' <summary>
    ''' 名称取得
    ''' </summary>
    ''' <param name="I_FIELD"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_TEXT"></param>
    ''' <param name="O_RTN"></param>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal param1 As String = "", Optional ByVal param2 As String = "")

        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrEmpty(I_VALUE) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Try
            Select Case I_FIELD
                Case "BIGCTNCD"                   '大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS))
                Case "MIDDLECTNCD"                '中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, param1))
                Case "SMALLCTNCD"                 '小分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.SMALL_CLASS, param1, param2))
                Case "JURISDICTION",              '所管部コード
                     "ACCOUNTINGASSETSCD",        '経理資産コード
                     "ACCOUNTINGASSETSKBN",       '経理資産区分
                     "DUMMYKBN",                  'ダミー区分
                     "SPOTKBN",                   'スポット区分
                     "CTNMAKER",                　'コンテナメーカー
                     "FROZENMAKER",               '冷凍機メーカー
                     "COMPKANKBN",                '複合一貫区分
                     "ADDITEM1",                  '付帯項目１
                     "ADDITEM2",                  '付帯項目２
                     "ADDITEM3",                  '付帯項目３
                     "ADDITEM4",                  '付帯項目４
                     "ADDITEM5",                  '付帯項目５
                     "ADDITEM6",                  '付帯項目６
                     "ADDITEM7",                  '付帯項目７
                     "ADDITEM8",                  '付帯項目８
                     "ADDITEM9",                  '付帯項目９
                     "ADDITEM10",                 '付帯項目１０
                     "ADDITEM11",                 '付帯項目１１
                     "ADDITEM12",                 '付帯項目１２
                     "ADDITEM13",                 '付帯項目１３
                     "ADDITEM14",                 '付帯項目１４
                     "ADDITEM15",                 '付帯項目１５
                     "ADDITEM16",                 '付帯項目１６
                     "ADDITEM17",                 '付帯項目１７
                     "ADDITEM18",                 '付帯項目１８
                     "ADDITEM19",                 '付帯項目１９
                     "ADDITEM20",                 '付帯項目２０
                     "ADDITEM21",                 '付帯項目２１
                     "ADDITEM22",                 '付帯項目２２
                     "ADDITEM23",                 '付帯項目２３
                     "ADDITEM24",                 '付帯項目２４
                     "ADDITEM25",                 '付帯項目２５
                     "ADDITEM26",                 '付帯項目２６
                     "ADDITEM27",                 '付帯項目２７
                     "ADDITEM28",                 '付帯項目２８
                     "ADDITEM29",                 '付帯項目２９
                     "ADDITEM30",                 '付帯項目３０
                     "ADDITEM31",                 '付帯項目３１
                     "ADDITEM32",                 '付帯項目３２
                     "ADDITEM33",                 '付帯項目３３
                     "ADDITEM34",                 '付帯項目３４
                     "ADDITEM35",                 '付帯項目３５
                     "ADDITEM36",                 '付帯項目３６
                     "ADDITEM37",                 '付帯項目３７
                     "ADDITEM38",                 '付帯項目３８
                     "ADDITEM39",                 '付帯項目３９
                     "ADDITEM40",                 '付帯項目４０
                     "ADDITEM41",                 '付帯項目４１
                     "ADDITEM42",                 '付帯項目４２
                     "ADDITEM43",                 '付帯項目４３
                     "ADDITEM44",                 '付帯項目４４
                     "ADDITEM45",                 '付帯項目４５
                     "ADDITEM46",                 '付帯項目４６
                     "ADDITEM47",                 '付帯項目４７
                     "ADDITEM48",                 '付帯項目４８
                     "ADDITEM49",                 '付帯項目４９
                     "ADDITEM50",                 '付帯項目５０
                     "FLOORMATERIAL",             '床材質コード
                     "SUPPLYFLG"                  '調達フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))

                Case "OUTPUTID"                   '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))
                Case "ONOFF"                      '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))
                Case "DELFLG"                     '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

#Region "変更履歴テーブル登録"
    ''' <summary>
    ''' 変更チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub RECONMEXISTS(ByVal SQLcon As MySqlConnection,
                               ByVal WW_ROW As DataRow,
                               ByRef WW_BEFDELFLG As String,
                               ByRef WW_MODIFYKBN As String,
                               ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        'コンテナマスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        CTNTYPE")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0002_RECONM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        CTNTYPE         = @CTNTYPE")
        SQLStr.AppendLine("    AND CTNNO        = @CTNNO")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_CTNTYPE As MySqlParameter = SQLcmd.Parameters.Add("@CTNTYPE", MySqlDbType.VarChar, 5)         'コンテナ記号
                Dim P_CTNNO As MySqlParameter = SQLcmd.Parameters.Add("@CTNNO", MySqlDbType.VarChar, 8)         'コンテナ番号

                P_CTNTYPE.Value = WW_ROW("CTNTYPE")                                  'コンテナ記号
                P_CTNNO.Value = WW_ROW("CTNNO")                                    'コンテナ番号

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    '更新の場合(データが存在した場合)は変更区分に変更前をセット、更新前の削除フラグを取得する
                    If WW_Tbl.Rows.Count > 0 Then
                        WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.BEFDATA '変更前
                        WW_BEFDELFLG = WW_Tbl.Rows(0)("DELFLG")
                    Else
                        WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0002_RECONM SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0002_RECONM Select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 履歴テーブル登録
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub InsertHist(ByVal SQLcon As MySqlConnection,
                             ByVal WW_ROW As DataRow,
                             ByVal WW_BEFDELFLG As String,
                             ByVal WW_MODIFYKBN As String,
                             ByVal WW_NOW As Date,
                             ByRef O_RTN As String)

        O_RTN = Messages.C_MESSAGE_NO.NORMAL

        '○ ＤＢ更新
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0115_RECONHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         CTNTYPE  ")
        SQLStr.AppendLine("        ,CTNNO  ")
        SQLStr.AppendLine("        ,JURISDICTIONCD  ")
        SQLStr.AppendLine("        ,ACCOUNTINGASSETSCD  ")
        SQLStr.AppendLine("        ,ACCOUNTINGASSETSKBN  ")
        SQLStr.AppendLine("        ,DUMMYKBN  ")
        SQLStr.AppendLine("        ,SPOTKBN  ")
        SQLStr.AppendLine("        ,SPOTSTYMD  ")
        SQLStr.AppendLine("        ,SPOTENDYMD  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,SMALLCTNCD  ")
        SQLStr.AppendLine("        ,CONSTRUCTIONYM  ")
        SQLStr.AppendLine("        ,CTNMAKER  ")
        SQLStr.AppendLine("        ,FROZENMAKER  ")
        SQLStr.AppendLine("        ,GROSSWEIGHT  ")
        SQLStr.AppendLine("        ,CARGOWEIGHT  ")
        SQLStr.AppendLine("        ,MYWEIGHT  ")
        SQLStr.AppendLine("        ,BOOKVALUE  ")
        SQLStr.AppendLine("        ,OUTHEIGHT  ")
        SQLStr.AppendLine("        ,OUTWIDTH  ")
        SQLStr.AppendLine("        ,OUTLENGTH  ")
        SQLStr.AppendLine("        ,INHEIGHT  ")
        SQLStr.AppendLine("        ,INWIDTH  ")
        SQLStr.AppendLine("        ,INLENGTH  ")
        SQLStr.AppendLine("        ,WIFEHEIGHT  ")
        SQLStr.AppendLine("        ,WIFEWIDTH  ")
        SQLStr.AppendLine("        ,SIDEHEIGHT  ")
        SQLStr.AppendLine("        ,SIDEWIDTH  ")
        SQLStr.AppendLine("        ,FLOORAREA  ")
        SQLStr.AppendLine("        ,INVOLUMEMARKING  ")
        SQLStr.AppendLine("        ,INVOLUMEACTUA  ")
        SQLStr.AppendLine("        ,TRAINSCYCLEDAYS  ")
        SQLStr.AppendLine("        ,TRAINSBEFORERUNYMD  ")
        SQLStr.AppendLine("        ,TRAINSNEXTRUNYMD  ")
        SQLStr.AppendLine("        ,REGINSCYCLEDAYS  ")
        SQLStr.AppendLine("        ,REGINSCYCLEHOURMETER  ")
        SQLStr.AppendLine("        ,REGINSBEFORERUNYMD  ")
        SQLStr.AppendLine("        ,REGINSNEXTRUNYMD  ")
        SQLStr.AppendLine("        ,REGINSHOURMETERYMD  ")
        SQLStr.AppendLine("        ,REGINSHOURMETERTIME  ")
        SQLStr.AppendLine("        ,REGINSHOURMETERDSP  ")
        SQLStr.AppendLine("        ,OPERATIONSTYMD  ")
        SQLStr.AppendLine("        ,OPERATIONENDYMD  ")
        SQLStr.AppendLine("        ,RETIRMENTYMD  ")
        SQLStr.AppendLine("        ,COMPKANKBN  ")
        SQLStr.AppendLine("        ,SUPPLYFLG  ")
        SQLStr.AppendLine("        ,ADDITEM1  ")
        SQLStr.AppendLine("        ,ADDITEM2  ")
        SQLStr.AppendLine("        ,ADDITEM3  ")
        SQLStr.AppendLine("        ,ADDITEM4  ")
        SQLStr.AppendLine("        ,ADDITEM5  ")
        SQLStr.AppendLine("        ,ADDITEM6  ")
        SQLStr.AppendLine("        ,ADDITEM7  ")
        SQLStr.AppendLine("        ,ADDITEM8  ")
        SQLStr.AppendLine("        ,ADDITEM9  ")
        SQLStr.AppendLine("        ,ADDITEM10  ")
        SQLStr.AppendLine("        ,ADDITEM11  ")
        SQLStr.AppendLine("        ,ADDITEM12  ")
        SQLStr.AppendLine("        ,ADDITEM13  ")
        SQLStr.AppendLine("        ,ADDITEM14  ")
        SQLStr.AppendLine("        ,ADDITEM15  ")
        SQLStr.AppendLine("        ,ADDITEM16  ")
        SQLStr.AppendLine("        ,ADDITEM17  ")
        SQLStr.AppendLine("        ,ADDITEM18  ")
        SQLStr.AppendLine("        ,ADDITEM19  ")
        SQLStr.AppendLine("        ,ADDITEM20  ")
        SQLStr.AppendLine("        ,ADDITEM21  ")
        SQLStr.AppendLine("        ,ADDITEM22  ")
        SQLStr.AppendLine("        ,ADDITEM23  ")
        SQLStr.AppendLine("        ,ADDITEM24  ")
        SQLStr.AppendLine("        ,ADDITEM25  ")
        SQLStr.AppendLine("        ,ADDITEM26  ")
        SQLStr.AppendLine("        ,ADDITEM27  ")
        SQLStr.AppendLine("        ,ADDITEM28  ")
        SQLStr.AppendLine("        ,ADDITEM29  ")
        SQLStr.AppendLine("        ,ADDITEM30  ")
        SQLStr.AppendLine("        ,ADDITEM31  ")
        SQLStr.AppendLine("        ,ADDITEM32  ")
        SQLStr.AppendLine("        ,ADDITEM33  ")
        SQLStr.AppendLine("        ,ADDITEM34  ")
        SQLStr.AppendLine("        ,ADDITEM35  ")
        SQLStr.AppendLine("        ,ADDITEM36  ")
        SQLStr.AppendLine("        ,ADDITEM37  ")
        SQLStr.AppendLine("        ,ADDITEM38  ")
        SQLStr.AppendLine("        ,ADDITEM39  ")
        SQLStr.AppendLine("        ,ADDITEM40  ")
        SQLStr.AppendLine("        ,ADDITEM41  ")
        SQLStr.AppendLine("        ,ADDITEM42  ")
        SQLStr.AppendLine("        ,ADDITEM43  ")
        SQLStr.AppendLine("        ,ADDITEM44  ")
        SQLStr.AppendLine("        ,ADDITEM45  ")
        SQLStr.AppendLine("        ,ADDITEM46  ")
        SQLStr.AppendLine("        ,ADDITEM47  ")
        SQLStr.AppendLine("        ,ADDITEM48  ")
        SQLStr.AppendLine("        ,ADDITEM49  ")
        SQLStr.AppendLine("        ,ADDITEM50  ")
        SQLStr.AppendLine("        ,FLOORMATERIAL  ")
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
        SQLStr.AppendLine("         CTNTYPE  ")
        SQLStr.AppendLine("        ,CTNNO  ")
        SQLStr.AppendLine("        ,JURISDICTIONCD  ")
        SQLStr.AppendLine("        ,ACCOUNTINGASSETSCD  ")
        SQLStr.AppendLine("        ,ACCOUNTINGASSETSKBN  ")
        SQLStr.AppendLine("        ,DUMMYKBN  ")
        SQLStr.AppendLine("        ,SPOTKBN  ")
        SQLStr.AppendLine("        ,SPOTSTYMD  ")
        SQLStr.AppendLine("        ,SPOTENDYMD  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,SMALLCTNCD  ")
        SQLStr.AppendLine("        ,CONSTRUCTIONYM  ")
        SQLStr.AppendLine("        ,CTNMAKER  ")
        SQLStr.AppendLine("        ,FROZENMAKER  ")
        SQLStr.AppendLine("        ,GROSSWEIGHT  ")
        SQLStr.AppendLine("        ,CARGOWEIGHT  ")
        SQLStr.AppendLine("        ,MYWEIGHT  ")
        SQLStr.AppendLine("        ,BOOKVALUE  ")
        SQLStr.AppendLine("        ,OUTHEIGHT  ")
        SQLStr.AppendLine("        ,OUTWIDTH  ")
        SQLStr.AppendLine("        ,OUTLENGTH  ")
        SQLStr.AppendLine("        ,INHEIGHT  ")
        SQLStr.AppendLine("        ,INWIDTH  ")
        SQLStr.AppendLine("        ,INLENGTH  ")
        SQLStr.AppendLine("        ,WIFEHEIGHT  ")
        SQLStr.AppendLine("        ,WIFEWIDTH  ")
        SQLStr.AppendLine("        ,SIDEHEIGHT  ")
        SQLStr.AppendLine("        ,SIDEWIDTH  ")
        SQLStr.AppendLine("        ,FLOORAREA  ")
        SQLStr.AppendLine("        ,INVOLUMEMARKING  ")
        SQLStr.AppendLine("        ,INVOLUMEACTUA  ")
        SQLStr.AppendLine("        ,TRAINSCYCLEDAYS  ")
        SQLStr.AppendLine("        ,TRAINSBEFORERUNYMD  ")
        SQLStr.AppendLine("        ,TRAINSNEXTRUNYMD  ")
        SQLStr.AppendLine("        ,REGINSCYCLEDAYS  ")
        SQLStr.AppendLine("        ,REGINSCYCLEHOURMETER  ")
        SQLStr.AppendLine("        ,REGINSBEFORERUNYMD  ")
        SQLStr.AppendLine("        ,REGINSNEXTRUNYMD  ")
        SQLStr.AppendLine("        ,REGINSHOURMETERYMD  ")
        SQLStr.AppendLine("        ,REGINSHOURMETERTIME  ")
        SQLStr.AppendLine("        ,REGINSHOURMETERDSP  ")
        SQLStr.AppendLine("        ,OPERATIONSTYMD  ")
        SQLStr.AppendLine("        ,OPERATIONENDYMD  ")
        SQLStr.AppendLine("        ,RETIRMENTYMD  ")
        SQLStr.AppendLine("        ,COMPKANKBN  ")
        SQLStr.AppendLine("        ,SUPPLYFLG  ")
        SQLStr.AppendLine("        ,ADDITEM1  ")
        SQLStr.AppendLine("        ,ADDITEM2  ")
        SQLStr.AppendLine("        ,ADDITEM3  ")
        SQLStr.AppendLine("        ,ADDITEM4  ")
        SQLStr.AppendLine("        ,ADDITEM5  ")
        SQLStr.AppendLine("        ,ADDITEM6  ")
        SQLStr.AppendLine("        ,ADDITEM7  ")
        SQLStr.AppendLine("        ,ADDITEM8  ")
        SQLStr.AppendLine("        ,ADDITEM9  ")
        SQLStr.AppendLine("        ,ADDITEM10  ")
        SQLStr.AppendLine("        ,ADDITEM11  ")
        SQLStr.AppendLine("        ,ADDITEM12  ")
        SQLStr.AppendLine("        ,ADDITEM13  ")
        SQLStr.AppendLine("        ,ADDITEM14  ")
        SQLStr.AppendLine("        ,ADDITEM15  ")
        SQLStr.AppendLine("        ,ADDITEM16  ")
        SQLStr.AppendLine("        ,ADDITEM17  ")
        SQLStr.AppendLine("        ,ADDITEM18  ")
        SQLStr.AppendLine("        ,ADDITEM19  ")
        SQLStr.AppendLine("        ,ADDITEM20  ")
        SQLStr.AppendLine("        ,ADDITEM21  ")
        SQLStr.AppendLine("        ,ADDITEM22  ")
        SQLStr.AppendLine("        ,ADDITEM23  ")
        SQLStr.AppendLine("        ,ADDITEM24  ")
        SQLStr.AppendLine("        ,ADDITEM25  ")
        SQLStr.AppendLine("        ,ADDITEM26  ")
        SQLStr.AppendLine("        ,ADDITEM27  ")
        SQLStr.AppendLine("        ,ADDITEM28  ")
        SQLStr.AppendLine("        ,ADDITEM29  ")
        SQLStr.AppendLine("        ,ADDITEM30  ")
        SQLStr.AppendLine("        ,ADDITEM31  ")
        SQLStr.AppendLine("        ,ADDITEM32  ")
        SQLStr.AppendLine("        ,ADDITEM33  ")
        SQLStr.AppendLine("        ,ADDITEM34  ")
        SQLStr.AppendLine("        ,ADDITEM35  ")
        SQLStr.AppendLine("        ,ADDITEM36  ")
        SQLStr.AppendLine("        ,ADDITEM37  ")
        SQLStr.AppendLine("        ,ADDITEM38  ")
        SQLStr.AppendLine("        ,ADDITEM39  ")
        SQLStr.AppendLine("        ,ADDITEM40  ")
        SQLStr.AppendLine("        ,ADDITEM41  ")
        SQLStr.AppendLine("        ,ADDITEM42  ")
        SQLStr.AppendLine("        ,ADDITEM43  ")
        SQLStr.AppendLine("        ,ADDITEM44  ")
        SQLStr.AppendLine("        ,ADDITEM45  ")
        SQLStr.AppendLine("        ,ADDITEM46  ")
        SQLStr.AppendLine("        ,ADDITEM47  ")
        SQLStr.AppendLine("        ,ADDITEM48  ")
        SQLStr.AppendLine("        ,ADDITEM49  ")
        SQLStr.AppendLine("        ,ADDITEM50  ")
        SQLStr.AppendLine("        ,FLOORMATERIAL  ")
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
        SQLStr.AppendLine("        LNG.LNM0002_RECONM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        CTNTYPE      = @CTNTYPE")
        SQLStr.AppendLine("    AND CTNNO        = @CTNNO")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_CTNTYPE As MySqlParameter = SQLcmd.Parameters.Add("@CTNTYPE", MySqlDbType.VarChar, 5)         'コンテナ記号
                Dim P_CTNNO As MySqlParameter = SQLcmd.Parameters.Add("@CTNNO", MySqlDbType.VarChar, 8)         'コンテナ番号

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                ' DB更新
                P_CTNTYPE.Value = WW_ROW("CTNTYPE")               'コンテナ記号
                P_CTNNO.Value = WW_ROW("CTNNO")               'コンテナ番号

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0002WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If WW_BEFDELFLG = "0" And WW_ROW("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0002WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0002WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0115_RECONHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0115_RECONHIST  INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                                 'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            O_RTN = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

#End Region

#End Region

End Class

