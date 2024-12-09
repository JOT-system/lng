''************************************************************
' ガイダンスマスタメンテ登録画面
' 作成日 2022/03/01
' 更新日 
' 作成者 名取
' 更新者 
'
' 修正履歴 : 2022/03/01 新規作成
'          : 
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox
Imports System.IO

''' <summary>
''' ガイダンスマスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNS0008GuidanceDetail
    Inherits System.Web.UI.Page

    '○ 検索結果格納Table
    Private LNS0008tbl As DataTable                                 '一覧格納用テーブル

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

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        Try
            If IsPostBack Then
                ' 添付ファイルアップロード処理
                If Not String.IsNullOrEmpty(WF_FILENAMELIST.Value) Then
                    Dim WW_ReturnMessage = UploadAttachments()
                    If WW_ReturnMessage.MessageNo <> C_MESSAGE_NO.NORMAL Then
                        Master.Output(WW_ReturnMessage.MessageNo, C_MESSAGE_TYPE.ERR, WW_ReturnMessage.Pram01, needsPopUp:=True)
                    End If
                    WF_FILENAMELIST.Value = ""
                End If
                '○ 各ボタン押下処理
                If Not String.IsNullOrEmpty(WF_ButtonClick.Value) Then

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_ButtonCLEAR"           '戻るボタン押下
                            WF_CLEAR_Click()
                        Case "WF_ButtonDELETE"          '削除ボタン押下(添付ファイル)
                            If WF_ButtonUPDATE.Visible = True Then
                                WF_DELETE_Click()
                            End If
                        Case "WF_Field_DBClick"         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_ButtonSel"             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
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
            If Not IsNothing(LNS0008tbl) Then
                LNS0008tbl.Clear()
                LNS0008tbl.Dispose()
                LNS0008tbl = Nothing
            End If

        End Try
    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNS0008WRKINC.MAPIDC
        '○ HELP表示有無設定
        Master.dispHelp = False
        '○ D&D有無設定
        ' 共通のD&Dは使わない
        Master.eventDrop = False

        '○ 初期値設定
        WF_FIELD.Value = ""
        WF_ButtonClick.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""
        rightview.ResetIndex()
        leftview.ActiveListBox()

        ' 右Boxへの値設定
        rightview.MAPID = Master.MAPID
        rightview.MAPVARI = Master.MAPvariant
        rightview.COMPCODE = Master.USERCAMP
        rightview.PROFID = Master.PROF_REPORT
        rightview.Initialize(WW_Dummy)

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        ' ガイダンスマスタ項目を定義
        Dim WW_DispVal As LNS0008WRKINC.GuidanceItemClass = Nothing
        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNS0008L Then
            ' 一覧画面にて選択したレコード情報の取得(キー:ガイダンス№)
            WW_DispVal = GetGuidance(work.WF_SEL_GUIDANCENO2.Text)
            WF_ButtonUPDATE.Visible = True
            WF_ButtonCLEAR.Visible = True
            WF_ButtonBackToMenu.Visible = False
        ElseIf Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.MENU Then
            Dim prev As M00001MENU = DirectCast(Me.PreviousPage, M00001MENU)
            WW_DispVal = GetGuidance(prev.SelectedGuidanceNo)
            Form.Attributes.Add("REFONLY", "1")
            TxtEndYmd.Enabled = False
            TxtFromYmd.Enabled = False
            TxtTitle.Enabled = False
            TxtNaiyou.Enabled = False
            ChklFlags.Enabled = False
            RblType.Enabled = False
            WF_ButtonUPDATE.Visible = False
            WF_ButtonCLEAR.Visible = False
            WF_ButtonBackToMenu.Visible = True
        End If
        ' 添付ファイル作業フォルダの生成
        CreateInitDir(WW_DispVal)

        TxtFromYmd.Text = WW_DispVal.FromYmd                              '掲載開始日
        TxtEndYmd.Text = WW_DispVal.EndYmd                                '掲載終了日

        '〇 選択肢初期値設定(種類)
        RblType.Items.Add(New ListItem("障害", "E"))                      '種類
        RblType.Items.Add(New ListItem("インフォメーション", "I"))
        RblType.Items.Add(New ListItem("注意", "W"))
        '○ 名称設定処理
        LblGuidanceEntryDate.Text = WW_DispVal.InitYmd
        If RblType.Items.FindByValue(WW_DispVal.Type) IsNot Nothing Then
            RblType.SelectedValue = WW_DispVal.Type
        End If

        TxtTitle.Text = WW_DispVal.Title                                  'タイトル
        ChklFlags.DataSource = WW_DispVal.DispFlags                       '各対象フラグ
        ChklFlags.DataTextField = "DispName"
        ChklFlags.DataValueField = "FieldName"
        ChklFlags.DataBind()
        TxtNaiyou.Text = WW_DispVal.Naiyo                                 '内容
        RepAttachments.DataSource = WW_DispVal.Attachments                '各添付ファイル
        RepAttachments.DataBind()
        ViewState("DISPVALUE") = WW_DispVal                               '画面表示情報
    End Sub

    ''' <summary>
    ''' ガイダンスマスタよりデータ取得
    ''' </summary>
    ''' <param name="WW_WorkGuidance"></param>
    ''' <returns></returns>
    Private Function GetGuidance(WW_WorkGuidance As String) As LNS0008WRKINC.GuidanceItemClass

        ' ガイダンスマスタ項目を定義
        Dim WW_RetVal As New LNS0008WRKINC.GuidanceItemClass
        ' ガイダンス番号が無い場合は新規作成扱い
        If String.IsNullOrEmpty(WW_WorkGuidance) Then
            Return GetNewGuidanceItem()
        End If

        '○ 検索SQL
        '　検索説明
        '     条件指定に従い該当データをガイダンスマスタから取得する
        Dim SQLStr As String =
              " Select                                                                                " _
            & "     coalesce(RTRIM(LNS0008.DELFLG), '')                               AS DELFLG         " _
            & "   , coalesce(RTRIM(LNS0008.GUIDANCENO), '')                           AS GUIDANCENO     " _
            & "   , coalesce(DATE_FORMAT(LNS0008.FROMYMD, '%Y/%m/%d'), '')            AS FROMYMD        " _
            & "   , coalesce(DATE_FORMAT(LNS0008.ENDYMD, '%Y/%m/%d'), '')             AS ENDYMD         " _
            & "   , coalesce(RTRIM(LNS0008.TYPE), '')                                 AS TYPE           " _
            & "   , coalesce(RTRIM(LNS0008.TITLE), '')                                AS TITLE          " _
            & "   , coalesce(RTRIM(LNS0008.OUTFLG), '')                               AS OUTFLG         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG1), '')                               AS INFLG1         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG2), '')                               AS INFLG2         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG3), '')                               AS INFLG3         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG4), '')                               AS INFLG4         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG5), '')                               AS INFLG5         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG6), '')                               AS INFLG6         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG7), '')                               AS INFLG7         " _
            & "   , coalesce(RTRIM(LNS0008.INFLG8), '')                               AS INFLG8         " _
            & "   , coalesce(RTRIM(LNS0008.NAIYOU), '')                               AS NAIYOU         " _
            & "   , coalesce(RTRIM(LNS0008.FILE1), '')                                AS FILE1          " _
            & "   , coalesce(RTRIM(LNS0008.FILE2), '')                                AS FILE2          " _
            & "   , coalesce(RTRIM(LNS0008.FILE3), '')                                AS FILE3          " _
            & "   , coalesce(RTRIM(LNS0008.FILE4), '')                                AS FILE4          " _
            & "   , coalesce(RTRIM(LNS0008.FILE5), '')                                AS FILE5          " _
            & "   , coalesce(DATE_FORMAT(LNS0008.INITYMD, '%Y/%m/%d %H:%i:%s'), '')   AS INITYMD        " _
            & "   , coalesce(DATE_FORMAT(LNS0008.UPDYMD, '%Y/%m/%d %H:%i:%s'), '')    AS UPDYMD         " _
            & " FROM                                                                                  " _
            & "     COM.LNS0008_GUIDANCE LNS0008                                                      " _
            & " WHERE                                                                                 " _
            & "     LNS0008.GUIDANCENO = @GUIDANCENO                                                  "
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection,
            sqlCmd As New MySqlCommand(SQLStr, SQLcon)

            SQLcon.Open()
            sqlCmd.CommandTimeout = 300
            ' 検索パラメータ設定
            sqlCmd.Parameters.Add("@GUIDANCENO", MySqlDbType.VarChar).Value = WW_WorkGuidance

            Using SQLdr As MySqlDataReader = sqlCmd.ExecuteReader()
                If SQLdr.HasRows = False Then
                    Return GetNewGuidanceItem()
                End If
                SQLdr.Read()
                WW_RetVal.DelFlg = Convert.ToString(SQLdr("DELFLG"))          '削除フラグ
                WW_RetVal.GuidanceNo = Convert.ToString(SQLdr("GUIDANCENO"))  'ガイダンス№
                WW_RetVal.FromYmd = Convert.ToString(SQLdr("FROMYMD"))        '掲載開始日
                WW_RetVal.EndYmd = Convert.ToString(SQLdr("ENDYMD"))          '掲載終了日
                WW_RetVal.Type = Convert.ToString(SQLdr("TYPE"))              '種類
                WW_RetVal.Title = Convert.ToString(SQLdr("TITLE"))            'タイトル

                ' チェックリスト(対象フラグ)の初期値を設定
                WW_RetVal.DispFlags = LNS0008WRKINC.GetNewDisplayFlags()      '各対象フラグ
                ' 各対象フラグ項目の項目IDをリストに格納
                Dim WW_KeyValues As New List(Of String) From {
                    "OUTFLG", "INFLG1", "INFLG2", "INFLG3", "INFLG4", "INFLG5", "INFLG6", "INFLG7", "INFLG8"}
                ' フラグの初期値設定
                Dim WW_StringVal As String = ""

                For Each keyVal In WW_KeyValues
                    ' 項目IDに当てはまるDB登録情報を変数へ
                    WW_StringVal = Convert.ToString(SQLdr(keyVal))
                    ' 登録情報 = "1"の項目のチェックをTrue
                    Dim item = From dispFlg In WW_RetVal.DispFlags Where dispFlg.FieldName = keyVal
                    If item.Any Then
                        Dim fstItem = item.FirstOrDefault
                        If WW_StringVal = "1" Then
                            fstItem.Checked = True
                        End If
                    End If
                Next

                WW_RetVal.Naiyo = Convert.ToString(SQLdr("NAIYOU"))           '内容

                ' 各添付ファイル項目の項目IDをリストに格納
                WW_KeyValues = New List(Of String) From {
                    "FILE1", "FILE2", "FILE3", "FILE4", "FILE5"}              '各添付ファイル
                For Each keyVal In WW_KeyValues
                    ' 項目IDに当てはまるDB登録情報を変数へ
                    WW_StringVal = Convert.ToString(SQLdr(keyVal))
                    ' 登録情報 <> ""の項目の場合、添付ファイル設定
                    If Not String.IsNullOrEmpty(WW_StringVal) Then
                        Dim fileInf As New LNS0008WRKINC.FileItemClass
                        fileInf.FileName = WW_StringVal
                        WW_RetVal.Attachments.Add(fileInf)
                    End If
                Next

                WW_RetVal.InitYmd = Convert.ToString(SQLdr("INITYMD"))        '登録年月日

            End Using
        End Using
        Return WW_RetVal
    End Function

    ''' <summary>
    ''' 新規ガイダンス情報の作成
    ''' </summary>
    ''' <returns></returns>
    Private Function GetNewGuidanceItem() As LNS0008WRKINC.GuidanceItemClass

        Dim WW_RetVal As New LNS0008WRKINC.GuidanceItemClass
        ' 初期値設定
        WW_RetVal.DelFlg = C_DELETE_FLG.ALIVE                                '削除フラグ
        WW_RetVal.GuidanceNo = ""                                            'ガイダンス№
        Master.GetFirstValue(Master.USERCAMP, "FROMYMD", WW_RetVal.FromYmd)  '掲載開始日
        Master.GetFirstValue(Master.USERCAMP, "ENDYMD", WW_RetVal.EndYmd)    '掲載終了日
        Master.GetFirstValue(Master.USERCAMP, "TYPE", WW_RetVal.Type)        '種類
        WW_RetVal.Title = ""                                                 'タイトル
        WW_RetVal.DispFlags = LNS0008WRKINC.GetNewDisplayFlags()             '各対象フラグ
        WW_RetVal.Naiyo = ""                                                 '内容

        Return WW_RetVal
    End Function

    ''' <summary>
    ''' 入力チェック
    ''' </summary>
    ''' <param name="WW_DispVal"></param>
    ''' <param name="O_RTN"></param>
    Protected Sub INPCheck(WW_DispVal As LNS0008WRKINC.GuidanceItemClass, ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL
        Dim WW_LineErr As String = ""
        Dim WW_CheckMES1 As String = ""
        Dim WW_CheckMES2 As String = ""
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""
        Dim WW_DBDataCheck As String = ""

        ' 掲載開始日(バリデーションチェック）
        Master.CheckField(Master.USERCAMP, "FROMYMD", WW_DispVal.FromYmd, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・掲載開始日入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 掲載終了日(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ENDYMD", WW_DispVal.EndYmd, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・掲載終了日入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 日付大小チェック
            If Not String.IsNullOrEmpty(WW_DispVal.FromYmd) AndAlso
                Not String.IsNullOrEmpty(WW_DispVal.EndYmd) Then
                If CDate(WW_DispVal.FromYmd) > CDate(WW_DispVal.EndYmd) Then
                    WW_CheckMES1 = "・掲載開始日＆掲載終了日エラーです。"
                    WW_CheckMES2 = "日付大小入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 種類(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "TYPE", WW_DispVal.Type, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・種類選択エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' タイトル(バリデーションチェック
            Master.CheckField(Master.USERCAMP, "TITLE", WW_DispVal.Title, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・タイトル入力エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 内容(バリデーションチェック
            Master.CheckField(Master.USERCAMP, "NAIYO", WW_DispVal.Naiyo, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
        If Not isNormal(WW_CS0024FCheckerr) Then
            WW_CheckMES1 = "・内容入力エラーです。"
            WW_CheckMES2 = WW_CS0024FCheckReport
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
        End If

        For Each fileItm In WW_DispVal.Attachments
            Master.CheckField(Master.USERCAMP, "FILE", fileItm.FileName, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・" & String.Format("ファイル名({0})", fileItm.FileName) & "エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
        Next

        ' 排他チェック
        If Not String.IsNullOrEmpty(work.WF_SEL_GUIDANCENO2.Text) Then  'ガイダンスNo.
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' 排他チェック
                work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                    work.WF_SEL_GUIDANCENO2.Text, work.WF_SEL_TIMESTAMP.Text)
            End Using

            If Not isNormal(WW_DBDataCheck) Then
                WW_CheckMES1 = "・排他エラー（ガイダンスNo.）"
                WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                       "([" & WW_DispVal.GuidanceNo & "]"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
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
        If Not String.IsNullOrEmpty(MESSAGE2) Then
            WW_ErrMes &= vbCr & "   -->" & MESSAGE2
        End If

        rightview.AddErrorReport(WW_ErrMes)

    End Sub

    ''' <summary>
    ''' ガイダンステーブル更新処理
    ''' </summary>
    ''' <param name="WW_DispVal"></param>
    ''' <param name="SQLcon"></param>
    ''' <param name="sqlTran"></param>
    Private Sub UpdateGuidance(WW_DispVal As LNS0008WRKINC.GuidanceItemClass, ByVal SQLcon As MySqlConnection, sqlTran As MySqlTransaction)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(ガイダンスマスタ)
        Dim SQLStr As String =
              " UPDATE COM.LNS0008_GUIDANCE  " _
            & " SET                          " _
            & "     DELFLG     = @P00        " _
            & "   , FROMYMD    = @P02        " _
            & "   , ENDYMD     = @P03        " _
            & "   , TYPE       = @P04        " _
            & "   , TITLE      = @P05        " _
            & "   , OUTFLG     = @P06        " _
            & "   , INFLG1     = @P07        " _
            & "   , INFLG2     = @P08        " _
            & "   , INFLG3     = @P09        " _
            & "   , INFLG4     = @P10        " _
            & "   , INFLG5     = @P11        " _
            & "   , INFLG6     = @P12        " _
            & "   , INFLG7     = @P13        " _
            & "   , INFLG8     = @P14        " _
            & "   , NAIYOU     = @P15        " _
            & "   , FILE1      = @P16        " _
            & "   , FILE2      = @P17        " _
            & "   , FILE3      = @P18        " _
            & "   , FILE4      = @P19        " _
            & "   , FILE5      = @P20        " _
            & "   , UPDYMD     = @P26        " _
            & "   , UPDUSER    = @P27        " _
            & "   , UPDTERMID  = @P28        " _
            & "   , UPDPGID    = @P29        " _
            & "   , RECEIVEYMD = @P30        " _
            & " WHERE                        " _
            & "     GUIDANCENO = @P01        "

        Try
            Using sqlCmd As New MySqlCommand(SQLStr, SQLcon, sqlTran)
                Dim PARA00 As MySqlParameter = sqlCmd.Parameters.Add("@P00", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim PARA01 As MySqlParameter = sqlCmd.Parameters.Add("@P01", MySqlDbType.VarChar, 12)        'ガイダンス№
                Dim PARA02 As MySqlParameter = sqlCmd.Parameters.Add("@P02", MySqlDbType.Date)                '掲載開始日
                Dim PARA03 As MySqlParameter = sqlCmd.Parameters.Add("@P03", MySqlDbType.Date)                '掲載終了日
                Dim PARA04 As MySqlParameter = sqlCmd.Parameters.Add("@P04", MySqlDbType.VarChar, 1)         '種類
                Dim PARA05 As MySqlParameter = sqlCmd.Parameters.Add("@P05", MySqlDbType.VarChar, 100)       'タイトル
                Dim PARA06 As MySqlParameter = sqlCmd.Parameters.Add("@P06", MySqlDbType.VarChar, 1)         '対象フラグ　社外
                Dim PARA07 As MySqlParameter = sqlCmd.Parameters.Add("@P07", MySqlDbType.VarChar, 1)         '対象フラグ　コンテナ部
                Dim PARA08 As MySqlParameter = sqlCmd.Parameters.Add("@P08", MySqlDbType.VarChar, 1)         '対象フラグ　北海道支店
                Dim PARA09 As MySqlParameter = sqlCmd.Parameters.Add("@P09", MySqlDbType.VarChar, 1)         '対象フラグ　東北支店
                Dim PARA10 As MySqlParameter = sqlCmd.Parameters.Add("@P10", MySqlDbType.VarChar, 1)         '対象フラグ　関東支店
                Dim PARA11 As MySqlParameter = sqlCmd.Parameters.Add("@P11", MySqlDbType.VarChar, 1)         '対象フラグ　新潟事業所
                Dim PARA12 As MySqlParameter = sqlCmd.Parameters.Add("@P12", MySqlDbType.VarChar, 1)         '対象フラグ　中部支店
                Dim PARA13 As MySqlParameter = sqlCmd.Parameters.Add("@P13", MySqlDbType.VarChar, 1)         '対象フラグ　関西支店
                Dim PARA14 As MySqlParameter = sqlCmd.Parameters.Add("@P14", MySqlDbType.VarChar, 1)         '対象フラグ　九州支店
                Dim PARA15 As MySqlParameter = sqlCmd.Parameters.Add("@P15", MySqlDbType.VarChar, 500)       '内容
                Dim PARA16 As MySqlParameter = sqlCmd.Parameters.Add("@P16", MySqlDbType.VarChar, 100)       '添付ファイル名１
                Dim PARA17 As MySqlParameter = sqlCmd.Parameters.Add("@P17", MySqlDbType.VarChar, 100)       '添付ファイル名２
                Dim PARA18 As MySqlParameter = sqlCmd.Parameters.Add("@P18", MySqlDbType.VarChar, 100)       '添付ファイル名３
                Dim PARA19 As MySqlParameter = sqlCmd.Parameters.Add("@P19", MySqlDbType.VarChar, 100)       '添付ファイル名４
                Dim PARA20 As MySqlParameter = sqlCmd.Parameters.Add("@P20", MySqlDbType.VarChar, 100)       '添付ファイル名５
                Dim PARA26 As MySqlParameter = sqlCmd.Parameters.Add("@P26", MySqlDbType.DateTime)            '更新年月日
                Dim PARA27 As MySqlParameter = sqlCmd.Parameters.Add("@P27", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim PARA28 As MySqlParameter = sqlCmd.Parameters.Add("@P28", MySqlDbType.VarChar, 20)        '更新端末
                Dim PARA29 As MySqlParameter = sqlCmd.Parameters.Add("@P29", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ
                Dim PARA30 As MySqlParameter = sqlCmd.Parameters.Add("@P30", MySqlDbType.DateTime)            '集信日時

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA00.Value = WW_DispVal.DelFlg
                PARA01.Value = WW_DispVal.GuidanceNo
                PARA02.Value = WW_DispVal.FromYmd
                PARA03.Value = WW_DispVal.EndYmd
                PARA04.Value = WW_DispVal.Type
                PARA05.Value = WW_DispVal.Title
                Dim WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "OUTFLG" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA06.Value = "1"
                Else
                    PARA06.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG1" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA07.Value = "1"
                Else
                    PARA07.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG2" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA08.Value = "1"
                Else
                    PARA08.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG3" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA09.Value = "1"
                Else
                    PARA09.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG4" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA10.Value = "1"
                Else
                    PARA10.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG5" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA11.Value = "1"
                Else
                    PARA11.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG6" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA12.Value = "1"
                Else
                    PARA12.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG7" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA13.Value = "1"
                Else
                    PARA13.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG8" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA14.Value = "1"
                Else
                    PARA14.Value = "0"
                End If
                PARA15.Value = WW_DispVal.Naiyo
                Dim WW_FileNo As Integer = 0
                For Each attachItm In WW_DispVal.Attachments
                    If WW_FileNo >= 5 Then
                        Exit For
                    End If
                    WW_FileNo = WW_FileNo + 1
                    If WW_FileNo = 1 Then
                        PARA16.Value = attachItm.FileName
                    ElseIf WW_FileNo = 2 Then
                        PARA17.Value = attachItm.FileName
                    ElseIf WW_FileNo = 3 Then
                        PARA18.Value = attachItm.FileName
                    ElseIf WW_FileNo = 4 Then
                        PARA19.Value = attachItm.FileName
                    ElseIf WW_FileNo = 5 Then
                        PARA20.Value = attachItm.FileName
                    End If
                Next

                If WW_FileNo < 5 Then
                    WW_FileNo = WW_FileNo + 1
                    For i = WW_FileNo To 5
                        If i = 1 Then
                            PARA16.Value = ""
                        ElseIf i = 2 Then
                            PARA17.Value = ""
                        ElseIf i = 3 Then
                            PARA18.Value = ""
                        ElseIf i = 4 Then
                            PARA19.Value = ""
                        ElseIf i = 5 Then
                            PARA20.Value = ""
                        End If
                    Next
                End If
                PARA26.Value = WW_DateNow                                          '更新年月日
                PARA27.Value = Master.USERID                                       '更新ユーザーＩＤ
                PARA28.Value = Master.USERTERMID                                   '更新端末
                PARA29.Value = Me.GetType().BaseType.Name                          '更新プログラムＩＤ
                PARA30.Value = C_DEFAULT_YMD                                       '集信日時
                sqlCmd.CommandTimeout = 300
                sqlCmd.ExecuteNonQuery()
            End Using

        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0008D UPDATE")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0008D UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

            rightview.AddErrorReport("DB更新処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' ガイダンステーブル追加処理
    ''' </summary>
    ''' <param name="WW_DispVal"></param>
    ''' <param name="SQLcon"></param>
    ''' <param name="sqlTran"></param>
    Private Sub InsertGuidance(WW_DispVal As LNS0008WRKINC.GuidanceItemClass, SQLcon As MySqlConnection, sqlTran As MySqlTransaction, ByRef WW_NewGuidanceNo As String, ByRef WW_NowDate As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        ' ガイダンス番号の自動採番
        WW_NewGuidanceNo = ""
        WW_NowDate = Now.ToString("yyyyMMdd")
        Dim SQLStrGetWW_NewGuidanceNo As String =
          " SELECT                                                                                             " _
        & "     @YMD + FORMAT(coalesce(MAX(CONVERT(int,REPLACE(LNS0008.GUIDANCENO,@YMD,''))),0) + 1,'0000') NGD  " _
        & " FROM                                                                                               " _
        & "     COM.LNS0008_GUIDANCE LNS0008                                                                   " _
        & " WHERE                                                                                              " _
        & " LNS0008.GUIDANCENO LIKE @YMD + '%'                                                                 "
        Using sqlCmd As New MySqlCommand(SQLStrGetWW_NewGuidanceNo, SQLcon, sqlTran)
            sqlCmd.Parameters.Add("@YMD", MySqlDbType.VarChar).Value = WW_NowDate

            WW_NewGuidanceNo = Convert.ToString(sqlCmd.ExecuteScalar())
        End Using

        '○ DB登録SQL(ガイダンスマスタ)
        Dim SQLStr As String =
              " INSERT INTO COM.LNS0008_GUIDANCE  " _
            & "    (DELFLG                        " _
            & "   , GUIDANCENO                    " _
            & "   , FROMYMD                       " _
            & "   , ENDYMD                        " _
            & "   , TYPE                          " _
            & "   , TITLE                         " _
            & "   , OUTFLG                        " _
            & "   , INFLG1                        " _
            & "   , INFLG2                        " _
            & "   , INFLG3                        " _
            & "   , INFLG4                        " _
            & "   , INFLG5                        " _
            & "   , INFLG6                        " _
            & "   , INFLG7                        " _
            & "   , INFLG8                        " _
            & "   , NAIYOU                        " _
            & "   , FILE1                         " _
            & "   , FILE2                         " _
            & "   , FILE3                         " _
            & "   , FILE4                         " _
            & "   , FILE5                         " _
            & "   , INITYMD                       " _
            & "   , INITUSER                      " _
            & "   , INITTERMID                    " _
            & "   , INITPGID                      " _
            & "   , UPDYMD                        " _
            & "   , UPDUSER                       " _
            & "   , UPDTERMID                     " _
            & "   , UPDPGID                       " _
            & "   , RECEIVEYMD)                   " _
            & " VALUES                            " _
            & "    (@P00                          " _
            & "   , @P01                          " _
            & "   , @P02                          " _
            & "   , @P03                          " _
            & "   , @P04                          " _
            & "   , @P05                          " _
            & "   , @P06                          " _
            & "   , @P07                          " _
            & "   , @P08                          " _
            & "   , @P09                          " _
            & "   , @P10                          " _
            & "   , @P11                          " _
            & "   , @P12                          " _
            & "   , @P13                          " _
            & "   , @P14                          " _
            & "   , @P15                          " _
            & "   , @P16                          " _
            & "   , @P17                          " _
            & "   , @P18                          " _
            & "   , @P19                          " _
            & "   , @P20                          " _
            & "   , @P22                          " _
            & "   , @P23                          " _
            & "   , @P24                          " _
            & "   , @P25                          " _
            & "   , @P26                          " _
            & "   , @P27                          " _
            & "   , @P28                          " _
            & "   , @P29                          " _
            & "   , @P30) ;                       "

        Try
            Using sqlCmd As New MySqlCommand(SQLStr, SQLcon, sqlTran)
                Dim PARA00 As MySqlParameter = sqlCmd.Parameters.Add("@P00", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim PARA01 As MySqlParameter = sqlCmd.Parameters.Add("@P01", MySqlDbType.VarChar, 12)        'ガイダンス№
                Dim PARA02 As MySqlParameter = sqlCmd.Parameters.Add("@P02", MySqlDbType.Date)                '掲載開始日
                Dim PARA03 As MySqlParameter = sqlCmd.Parameters.Add("@P03", MySqlDbType.Date)                '掲載終了日
                Dim PARA04 As MySqlParameter = sqlCmd.Parameters.Add("@P04", MySqlDbType.VarChar, 1)         '種類
                Dim PARA05 As MySqlParameter = sqlCmd.Parameters.Add("@P05", MySqlDbType.VarChar, 100)       'タイトル
                Dim PARA06 As MySqlParameter = sqlCmd.Parameters.Add("@P06", MySqlDbType.VarChar, 1)         '対象フラグ　社外
                Dim PARA07 As MySqlParameter = sqlCmd.Parameters.Add("@P07", MySqlDbType.VarChar, 1)         '対象フラグ　コンテナ部
                Dim PARA08 As MySqlParameter = sqlCmd.Parameters.Add("@P08", MySqlDbType.VarChar, 1)         '対象フラグ　北海道支店
                Dim PARA09 As MySqlParameter = sqlCmd.Parameters.Add("@P09", MySqlDbType.VarChar, 1)         '対象フラグ　東北支店
                Dim PARA10 As MySqlParameter = sqlCmd.Parameters.Add("@P10", MySqlDbType.VarChar, 1)         '対象フラグ　関東支店
                Dim PARA11 As MySqlParameter = sqlCmd.Parameters.Add("@P11", MySqlDbType.VarChar, 1)         '対象フラグ　新潟事業所
                Dim PARA12 As MySqlParameter = sqlCmd.Parameters.Add("@P12", MySqlDbType.VarChar, 1)         '対象フラグ　中部支店
                Dim PARA13 As MySqlParameter = sqlCmd.Parameters.Add("@P13", MySqlDbType.VarChar, 1)         '対象フラグ　関西支店
                Dim PARA14 As MySqlParameter = sqlCmd.Parameters.Add("@P14", MySqlDbType.VarChar, 1)         '対象フラグ　九州支店
                Dim PARA15 As MySqlParameter = sqlCmd.Parameters.Add("@P15", MySqlDbType.VarChar, 500)       '内容
                Dim PARA16 As MySqlParameter = sqlCmd.Parameters.Add("@P16", MySqlDbType.VarChar, 100)       '添付ファイル名１
                Dim PARA17 As MySqlParameter = sqlCmd.Parameters.Add("@P17", MySqlDbType.VarChar, 100)       '添付ファイル名２
                Dim PARA18 As MySqlParameter = sqlCmd.Parameters.Add("@P18", MySqlDbType.VarChar, 100)       '添付ファイル名３
                Dim PARA19 As MySqlParameter = sqlCmd.Parameters.Add("@P19", MySqlDbType.VarChar, 100)       '添付ファイル名４
                Dim PARA20 As MySqlParameter = sqlCmd.Parameters.Add("@P20", MySqlDbType.VarChar, 100)       '添付ファイル名５
                Dim PARA22 As MySqlParameter = sqlCmd.Parameters.Add("@P22", MySqlDbType.DateTime)            '登録年月日
                Dim PARA23 As MySqlParameter = sqlCmd.Parameters.Add("@P23", MySqlDbType.VarChar, 20)        '登録ユーザーＩＤ
                Dim PARA24 As MySqlParameter = sqlCmd.Parameters.Add("@P24", MySqlDbType.VarChar, 20)        '登録端末
                Dim PARA25 As MySqlParameter = sqlCmd.Parameters.Add("@P25", MySqlDbType.VarChar, 40)        '登録プログラムＩＤ
                Dim PARA26 As MySqlParameter = sqlCmd.Parameters.Add("@P26", MySqlDbType.DateTime)            '更新年月日
                Dim PARA27 As MySqlParameter = sqlCmd.Parameters.Add("@P27", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim PARA28 As MySqlParameter = sqlCmd.Parameters.Add("@P28", MySqlDbType.VarChar, 20)        '更新端末
                Dim PARA29 As MySqlParameter = sqlCmd.Parameters.Add("@P29", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ
                Dim PARA30 As MySqlParameter = sqlCmd.Parameters.Add("@P30", MySqlDbType.DateTime)            '集信日時

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA00.Value = WW_DispVal.DelFlg
                PARA01.Value = WW_NewGuidanceNo
                PARA02.Value = WW_DispVal.FromYmd
                PARA03.Value = WW_DispVal.EndYmd
                PARA04.Value = WW_DispVal.Type
                PARA05.Value = WW_DispVal.Title
                Dim WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "OUTFLG" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA06.Value = "1"
                Else
                    PARA06.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG1" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA07.Value = "1"
                Else
                    PARA07.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG2" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA08.Value = "1"
                Else
                    PARA08.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG3" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA09.Value = "1"
                Else
                    PARA09.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG4" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA10.Value = "1"
                Else
                    PARA10.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG5" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA11.Value = "1"
                Else
                    PARA11.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG6" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA12.Value = "1"
                Else
                    PARA12.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG7" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA13.Value = "1"
                Else
                    PARA13.Value = "0"
                End If
                WW_FindFlag = From flagitm In WW_DispVal.DispFlags Where flagitm.FieldName = "INFLG8" AndAlso flagitm.Checked
                If WW_FindFlag.Any Then
                    PARA14.Value = "1"
                Else
                    PARA14.Value = "0"
                End If
                PARA15.Value = WW_DispVal.Naiyo
                Dim WW_FileNo As Integer = 0
                For Each attachItm In WW_DispVal.Attachments
                    If WW_FileNo >= 5 Then
                        Exit For
                    End If
                    WW_FileNo = WW_FileNo + 1
                    If WW_FileNo = 1 Then
                        PARA16.Value = attachItm.FileName
                    ElseIf WW_FileNo = 2 Then
                        PARA17.Value = attachItm.FileName
                    ElseIf WW_FileNo = 3 Then
                        PARA18.Value = attachItm.FileName
                    ElseIf WW_FileNo = 4 Then
                        PARA19.Value = attachItm.FileName
                    ElseIf WW_FileNo = 5 Then
                        PARA20.Value = attachItm.FileName
                    End If
                Next

                If WW_FileNo < 5 Then
                    WW_FileNo = WW_FileNo + 1
                    For i = WW_FileNo To 5
                        If i = 1 Then
                            PARA16.Value = ""
                        ElseIf i = 2 Then
                            PARA17.Value = ""
                        ElseIf i = 3 Then
                            PARA18.Value = ""
                        ElseIf i = 4 Then
                            PARA19.Value = ""
                        ElseIf i = 5 Then
                            PARA20.Value = ""
                        End If
                    Next
                End If
                PARA22.Value = WW_DateNow                                          '登録年月日
                PARA23.Value = Master.USERID                                       '登録ユーザーＩＤ
                PARA24.Value = Master.USERTERMID                                   '登録端末
                PARA25.Value = Me.GetType().BaseType.Name                          '登録プログラムＩＤ
                PARA26.Value = WW_DateNow                                          '更新年月日
                PARA27.Value = Master.USERID                                       '更新ユーザーＩＤ
                PARA28.Value = Master.USERTERMID                                   '更新端末
                PARA29.Value = Me.GetType().BaseType.Name                          '更新プログラムＩＤ
                PARA30.Value = C_DEFAULT_YMD                                       '集信日時
                sqlCmd.CommandTimeout = 300
                sqlCmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNS0008D INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNS0008D INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力

            rightview.AddErrorReport("DB登録処理で例外エラーが発生しました。システム管理者にお問い合わせ下さい。")
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try
    End Sub

    ''' <summary>
    ''' 更新ボタン押下時処理
    ''' </summary>
    Protected Sub WF_UPDATE_Click()

        Dim WW_NewGuidanceNo As String = ""
        Dim WW_WorkGuidance As String = ""
        Dim WW_NowDate As String = ""

        ' 左ボックスの選択解除
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        ' 画面の入力値収集
        Dim WW_DispVal = CollectDispValue()

        ' 入力チェック
        INPCheck(WW_DispVal, WW_ErrSW)

        If isNormal(WW_ErrSW) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                Using SQLtrn = SQLcon.BeginTransaction
                    If Not String.IsNullOrEmpty(WW_DispVal.GuidanceNo) Then
                        ' 更新処理
                        UpdateGuidance(WW_DispVal, SQLcon, SQLtrn)
                        WW_WorkGuidance = WW_DispVal.GuidanceNo
                    Else
                        ' 登録処理
                        InsertGuidance(WW_DispVal, SQLcon, SQLtrn, WW_NewGuidanceNo, WW_NowDate)
                        WW_WorkGuidance = WW_NewGuidanceNo
                    End If
                    ' ジャーナル生成
                    SaveJournal(WW_WorkGuidance, SQLcon, SQLtrn)

                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    ' ファイル移動
                    MoveAttachments(WW_WorkGuidance, WW_DispVal)
                    ' トランザクションコミット
                    SQLtrn.Commit()

                End Using
            End Using
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNS0008tbl, work.WF_SEL_INPTBL.Text)

        '○ メッセージ表示
        ' 右BOXクローズ
        WF_RightboxOpen.Value = ""
        If String.IsNullOrEmpty(WW_ErrSW) Then
            Master.Output(C_MESSAGE_NO.NORMAL, C_MESSAGE_TYPE.INF)
        Else
            If isNormal(WW_ErrSW) Then
                Master.Output(C_MESSAGE_NO.TABLE_ADDION_SUCCESSFUL, C_MESSAGE_TYPE.INF)
            Else
                If WW_ErrSW = "20028" Then
                    ' 排他エラー
                    Master.Output(C_MESSAGE_NO.CTN_HAITA_DATA_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                Else
                    ' その他エラー
                    Master.Output(C_MESSAGE_NO.INVALID_UPDATE_RECORD_ERROR, C_MESSAGE_TYPE.ERR, needsPopUp:=True)
                End If
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
    ''' 画面入力値収集
    ''' </summary>
    ''' <returns></returns>
    Public Function CollectDispValue() As LNS0008WRKINC.GuidanceItemClass

        Dim WW_DispVal = DirectCast(ViewState("DISPVALUE"), LNS0008WRKINC.GuidanceItemClass)

        WW_DispVal.FromYmd = TxtFromYmd.Text                          '掲載開始日
        WW_DispVal.EndYmd = TxtEndYmd.Text                            '掲載終了日
        WW_DispVal.Title = TxtTitle.Text                              '種類
        WW_DispVal.Naiyo = TxtNaiyou.Text                             'タイトル

        If RblType.SelectedItem IsNot Nothing Then                    '種類
            WW_DispVal.Type = RblType.SelectedValue
        Else
            WW_DispVal.Type = ""
        End If

        For Each flag In WW_DispVal.DispFlags                         '各対象フラグ
            Dim chkObj = ChklFlags.Items.FindByValue(flag.FieldName)
            If chkObj IsNot Nothing AndAlso chkObj.Selected Then
                flag.Checked = True
            Else
                flag.Checked = False
            End If
        Next
        Return WW_DispVal
    End Function

    ''' <summary>
    ''' 戻るボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_CLEAR_Click()
        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""

        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 添付ファイル削除ボタン押下時処理
    ''' </summary>
    Protected Sub WF_DELETE_Click()
        Dim WW_DispVal = DirectCast(ViewState("DISPVALUE"), LNS0008WRKINC.GuidanceItemClass)
        ' ガイダンス用作業フォルダ
        Dim WW_GuidanceWorkDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, LNS0008WRKINC.GUIDANCEROOT, "USERWORKS", CS0050SESSION.USERID)
        If Not Directory.Exists(WW_GuidanceWorkDir) Then
            Directory.CreateDirectory(WW_GuidanceWorkDir)
        End If
        Dim WW_DeleteFileName As String = WF_DELETEFILENAME.Value
        ' 画面の添付ファイルリストから対象のファイルを削除
        For i = WW_DispVal.Attachments.Count - 1 To 0 Step -1
            If WW_DispVal.Attachments(i).FileName = WW_DeleteFileName Then
                WW_DispVal.Attachments.RemoveAt(i)
                Exit For
            End If
        Next
        Dim WW_DelFilePath As String = IO.Path.Combine(WW_GuidanceWorkDir, WW_DeleteFileName)
        ' ファイル保管フォルダから削除
        If IO.File.Exists(WW_DelFilePath) Then
            Try
                IO.File.Delete(WW_DelFilePath)
            Catch ex As Exception
            End Try
        End If
        ' 画面情報を書き換え
        RepAttachments.DataSource = WW_DispVal.Attachments
        RepAttachments.DataBind()
        ViewState("DISPVALUE") = WW_DispVal
    End Sub

    ''' <summary>
    ''' フィールドダブルクリック時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_FIELD_DBClick()
        If Not String.IsNullOrEmpty(WF_LeftMViewChange.Value) Then
            Try
                Integer.Parse(WF_LeftMViewChange.Value)
            Catch ex As Exception
                Exit Sub
            End Try
            With leftview
                ' 入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                Select Case WF_FIELD.Value
                    Case "TxtFromYmd"  '掲載開始日
                        .WF_Calendar.Text = TxtFromYmd.Text
                    Case "TxtEndYmd"   '掲載終了日
                        .WF_Calendar.Text = TxtEndYmd.Text
                End Select
                .ActiveCalendar()
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

        Dim WW_Date As Date

        '○ 選択内容を画面項目へセット
        Select Case WF_FIELD.Value
            Case "TxtFromYmd"  '掲載開始日
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                    If WW_Date < CDate(C_DEFAULT_YMD) Then
                        TxtFromYmd.Text = ""
                    Else
                        TxtFromYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                TxtFromYmd.Focus()
            Case "TxtEndYmd"   '掲載終了日
                Try
                    Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                    If WW_Date < CDate(C_DEFAULT_YMD) Then
                        TxtEndYmd.Text = ""
                    Else
                        TxtEndYmd.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                    End If
                Catch ex As Exception
                End Try
                TxtEndYmd.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

    ''' <summary>
    ''' LeftBoxキャンセルボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_ButtonCan_Click()
        '○ フォーカスセット
        Select Case WF_FIELD.Value
            Case "WF_FROMYMD"  '掲載開始日
                TxtFromYmd.Focus()
            Case "WF_ENDYMD"   '掲載終了日
                TxtEndYmd.Focus()
        End Select

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_LeftboxOpen.Value = ""
    End Sub

    ''' <summary>
    ''' チェックボックスデータバインド時イベント
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks>チェックの状態を設定する</remarks>
    Private Sub ChklFlags_DataBound(sender As Object, e As EventArgs) Handles ChklFlags.DataBound
        Dim WW_ChklObj As CheckBoxList = DirectCast(sender, CheckBoxList)
        Dim WW_ChkBindItm As List(Of LNS0008WRKINC.DisplayFlag) = DirectCast(WW_ChklObj.DataSource, List(Of LNS0008WRKINC.DisplayFlag))
        ' 対象の各項目にチェックを付ける
        For i = 0 To WW_ChklObj.Items.Count - 1 Step 1
            WW_ChklObj.Items(i).Selected = WW_ChkBindItm(i).Checked
        Next
    End Sub

    ''' <summary>
    ''' ガイダンス処理の作業フォルダを作成する
    ''' </summary>
    ''' <param name="WW_GuidanceItem"></param>
    Private Sub CreateInitDir(WW_GuidanceItem As LNS0008WRKINC.GuidanceItemClass)

        ' 実体保存フォルダよりファイルのコピーを行う
        If String.IsNullOrEmpty(WW_GuidanceItem.GuidanceNo) Then
            ' ガイダンスNoが無い場合は既登録の添付ファイルはない前提なのでここで終了
            Return
        End If

        ' ガイダンス用作業フォルダ
        Dim WW_GuidanceWorkDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, LNS0008WRKINC.GUIDANCEROOT, "USERWORKS", CS0050SESSION.USERID)
        If Not Directory.Exists(WW_GuidanceWorkDir) Then
            Directory.CreateDirectory(WW_GuidanceWorkDir)
        End If

        ' ファイルパスからファイル名を取得して削除
        For Each tempFile As String In Directory.GetFiles(WW_GuidanceWorkDir, "*.*")
            Try
                File.Delete(tempFile)
            Catch ex As Exception
            End Try
        Next

        ' ファイル保管フォルダ
        Dim guidanceDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, LNS0008WRKINC.GUIDANCEROOT, WW_GuidanceItem.GuidanceNo)

        ' 既存ファイルを作業フォルダにコピー
        If IO.Directory.Exists(guidanceDir) = True Then
            Dim WW_FileNames = IO.Directory.GetFiles(guidanceDir)
            For Each filePath In WW_FileNames
                Dim WW_FileName As String = IO.Path.GetFileName(filePath)
                If String.IsNullOrEmpty(WW_FileName) Then
                    Continue For
                End If
                Dim WW_TargetFile As String = IO.Path.Combine(guidanceDir, WW_FileName)
                Dim WW_CopyPath As String = IO.Path.Combine(WW_GuidanceWorkDir, WW_FileName)
                Try
                    System.IO.File.Copy(WW_TargetFile, WW_CopyPath, True)
                Catch ex As Exception
                End Try
                ' テーブルに登録した情報と比較、実体があり、テーブルにない場合は画面に追加
                If (From gitm In WW_GuidanceItem.Attachments Where gitm.FileName = WW_FileName).Any = False Then
                    WW_GuidanceItem.Attachments.Add(New LNS0008WRKINC.FileItemClass With {.FileName = WW_FileName})
                End If
            Next filePath
            ' テーブルにあり実体がない場合は画面から消去
            If WW_FileNames IsNot Nothing OrElse WW_FileNames.Count > 0 Then
                Dim WW_FileNameList = (From filItm In WW_FileNames Select IO.Path.GetFileName(filItm)).ToList
                For i = WW_GuidanceItem.Attachments.Count - 1 To 0 Step -1

                    If WW_FileNameList.Contains(WW_GuidanceItem.Attachments(i).FileName) = False Then
                        WW_GuidanceItem.Attachments.RemoveAt(i)
                    End If
                Next
            Else
                WW_GuidanceItem.Attachments = New List(Of LNS0008WRKINC.FileItemClass)
            End If
        Else
            WW_GuidanceItem.Attachments = New List(Of LNS0008WRKINC.FileItemClass)
        End If
    End Sub

    ''' <summary>
    ''' ファイルアップロード処理
    ''' </summary>
    ''' <remarks>LNS0008FILEUPLOADの処理が完了後にこちらの処理が実行されます。</remarks>
    Private Function UploadAttachments() As PropMes

        Dim WW_ReturnMessage = New PropMes With {.MessageNo = C_MESSAGE_NO.NORMAL}
        Dim WW_FileType As Type = GetType(List(Of AttachmentFile))
        Dim WW_Serializer As New Runtime.Serialization.Json.DataContractJsonSerializer(WW_FileType)
        Dim WW_UploadFiles As New List(Of AttachmentFile)
        Dim WW_DispVal = DirectCast(ViewState("DISPVALUE"), LNS0008WRKINC.GuidanceItemClass)

        Try
            Using stream As New IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes(WF_FILENAMELIST.Value))
                WW_UploadFiles = DirectCast(WW_Serializer.ReadObject(stream), List(Of AttachmentFile))
            End Using
        Catch ex As Exception
            Return WW_ReturnMessage
        End Try

        ' ガイダンス用作業フォルダ
        Dim WW_GuidanceWorkDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, LNS0008WRKINC.GUIDANCEROOT, "USERWORKS", CS0050SESSION.USERID)
        If Not Directory.Exists(WW_GuidanceWorkDir) Then
            Directory.CreateDirectory(WW_GuidanceWorkDir)
        End If

        ' アップロードワークフォルダ
        Dim WW_UploadWorkDir = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, "UPLOAD_TMP", CS0050SESSION.USERID)
        If Not Directory.Exists(WW_UploadWorkDir) Then
            Return WW_ReturnMessage
        End If

        ' アップロードしたファイルと現在画面にあるファイルをファイル名重複なしてマージ
        Dim WW_FileNames As List(Of String) = (From itm In WW_DispVal.Attachments Select itm.FileName).ToList
        Dim WW_AddedFileList As New List(Of LNS0008WRKINC.FileItemClass)
        For Each uploadFile In WW_UploadFiles
            If Not WW_FileNames.Contains(uploadFile.FileName) Then
                WW_FileNames.Add(uploadFile.FileName)
                WW_AddedFileList.Add(New LNS0008WRKINC.FileItemClass With {.FileName = uploadFile.FileName})
            End If
        Next

        ' ファイル数が5を超えた場合はアップさせずにエラー
        If WW_FileNames.Count > 5 Then
            WW_ReturnMessage.MessageNo = C_MESSAGE_NO.ATTACHMENT_COUNTOVER
            WW_ReturnMessage.Pram01 = "5"
            Return WW_ReturnMessage
        End If

        ' ガイダンスファイル作業フォルダにコピー
        For Each uploadFile In WW_UploadFiles
            Dim WW_TargetFile As String = IO.Path.Combine(WW_UploadWorkDir, uploadFile.FileName)
            Dim WW_CopyPath As String = IO.Path.Combine(WW_GuidanceWorkDir, uploadFile.FileName)
            Try
                System.IO.File.Copy(WW_TargetFile, WW_CopyPath, True)
            Catch ex As Exception
            End Try
        Next

        ' 画面の添付ファイルリストの末尾に追加
        If WW_AddedFileList.Count > 0 Then
            WW_DispVal.Attachments.AddRange(WW_AddedFileList)
        End If

        ' 画面情報を書き換え
        RepAttachments.DataSource = WW_DispVal.Attachments
        RepAttachments.DataBind()
        ViewState("DISPVALUE") = WW_DispVal
        Return WW_ReturnMessage
    End Function

    ''' <summary>
    ''' ガイダンス作業フォルダから実体保存フォルダにコピー
    ''' </summary>
    ''' <param name="WW_TargetGuiganceNo">実際に移動するガイダンス番号、下の画面クラスには新規作成の場合振られていないのでこちらを利用</param>
    ''' <param name="WW_GuidanceItem">画面情報クラス</param>
    Private Sub MoveAttachments(WW_TargetGuiganceNo As String, WW_GuidanceItem As LNS0008WRKINC.GuidanceItemClass)

        ' ガイダンス用作業フォルダ
        Dim WW_GuidanceWorkDir As String = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, LNS0008WRKINC.GUIDANCEROOT, "USERWORKS", CS0050SESSION.USERID)
        If Not Directory.Exists(WW_GuidanceWorkDir) Then
            Directory.CreateDirectory(WW_GuidanceWorkDir)
        End If
        ' ガイダンス実体保存フォルダ
        Dim WW_GuidanceSaveDir = IO.Path.Combine(CS0050SESSION.UPLOAD_PATH, LNS0008WRKINC.GUIDANCEROOT, WW_TargetGuiganceNo)
        If Not Directory.Exists(WW_GuidanceSaveDir) Then
            Directory.CreateDirectory(WW_GuidanceSaveDir)
        Else
            ' 既保存ファイルを削除
            For Each tempFile As String In Directory.GetFiles(WW_GuidanceSaveDir, "*.*")
                ' ファイルパスからファイル名を取得
                Try
                    File.Delete(tempFile)
                Catch ex As Exception
                End Try
            Next
        End If

        ' ガイダンス添付ファイルを作業から実体フォルダにコピー
        Dim WW_UploadFiles = (From attItm In WW_GuidanceItem.Attachments).ToList
        For Each uploadFile In WW_UploadFiles
            Dim WW_TargetFile As String = IO.Path.Combine(WW_GuidanceWorkDir, uploadFile.FileName)
            Dim WW_CopyPath As String = IO.Path.Combine(WW_GuidanceSaveDir, uploadFile.FileName)
            Try
                System.IO.File.Copy(WW_TargetFile, WW_CopyPath, True)
            Catch ex As Exception
            End Try
        Next

    End Sub

    ''' <summary>
    ''' ジャーナル保存
    ''' </summary>
    ''' <param name="WW_WorkGuidance"></param>
    ''' <param name="SQLcon"></param>
    ''' <param name="sqlTran"></param>
    Private Sub SaveJournal(WW_WorkGuidance As String, SQLcon As MySqlConnection, sqlTran As MySqlTransaction)

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                     " _
            & "     DELFLG                                 " _
            & "   , GUIDANCENO                             " _
            & "   , FROMYMD                                " _
            & "   , ENDYMD                                 " _
            & "   , TYPE                                   " _
            & "   , TITLE                                  " _
            & "   , OUTFLG                                 " _
            & "   , INFLG1                                 " _
            & "   , INFLG2                                 " _
            & "   , INFLG3                                 " _
            & "   , INFLG4                                 " _
            & "   , INFLG5                                 " _
            & "   , INFLG6                                 " _
            & "   , INFLG7                                 " _
            & "   , INFLG8                                 " _
            & "   , NAIYOU                                 " _
            & "   , FILE1                                  " _
            & "   , FILE2                                  " _
            & "   , FILE3                                  " _
            & "   , FILE4                                  " _
            & "   , FILE5                                  " _
            & "   , INITYMD                                " _
            & "   , INITUSER                               " _
            & "   , INITTERMID                             " _
            & "   , INITPGID                               " _
            & "   , UPDYMD                                 " _
            & "   , UPDUSER                                " _
            & "   , UPDTERMID                              " _
            & "   , UPDPGID                                " _
            & "   , RECEIVEYMD                             " _
            & "   , UPDTIMSTP                              " _
            & " FROM                                       " _
            & "     COM.LNS0008_GUIDANCE                   " _
            & " WHERE                                      " _
            & "     GUIDANCENO = @P01                      "

        ' トランザクションしない場合は「SQLcon.BeginTransaction」→「nothing」
        Using SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon, sqlTran)
            SQLcmdJnl.CommandTimeout = 300
            ' 更新ジャーナル出力用パラメータ
            Dim JPARA01 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 20)    'ユーザID

            JPARA01.Value = WW_WorkGuidance

            Using LNS0008UPDtbl As New DataTable, SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                For index As Integer = 0 To SQLdr.FieldCount - 1
                    LNS0008UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                Next

                LNS0008UPDtbl.Load(SQLdr)

                CS0020JOURNAL.TABLENM = "LNS0008D"
                CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                CS0020JOURNAL.ROW = LNS0008UPDtbl.Rows(0)
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
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' ファイル情報クラス
    ''' </summary>
    <System.Runtime.Serialization.DataContract()>
    Public Class AttachmentFile
        <System.Runtime.Serialization.DataMember()>
        Public Property FileName As String
    End Class

    Public Class PropMes
        Public Property MessageNo As String = ""
        Public Property Pram01 As String = ""
    End Class
End Class