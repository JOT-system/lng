''************************************************************
' 固定費マスタメンテ登録画面
' 作成日 2025/01/20
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2025/01/20 新規作成
'          : 2025/05/15 統合版に変更
''************************************************************
Imports MySql.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 固定費マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0007KoteihiDetail
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0007tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0007INPtbl As DataTable                              'チェック用テーブル
    Private LNM0007UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0007tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_ButtonCLEAR", "LNM0007L"  '戻るボタン押下（LNM0007Lは、パンくずより）
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
                        Case "WF_TORIChange"    '取引先コードチェンジ
                            Dim WW_HT As New Hashtable
                            For index As Integer = 0 To WF_TORI.Items.Count - 1
                                WW_HT.Add(WF_TORI.Items(index).Text, WF_TORI.Items(index).Value)
                                If WF_TORI.Items(index).Text = WF_TORINAME.Text Then
                                    WF_TORI.SelectedValue = WF_TORI.Items(index).Value
                                    WF_TORI.SelectedIndex = index
                                End If
                            Next

                            If WW_HT.ContainsKey(WF_TORINAME.Text) Then
                                WF_TORICODE_TEXT.Text = WW_HT(WF_TORINAME.Text)
                            Else
                                WF_TORICODE_TEXT.Text = ""
                            End If
                            WF_SelectFIELD_CHANGE(WF_ButtonClick.Value)
                        Case "WF_ORGChange"    '部門コードチェンジ
                            'WF_ORGCODE_TEXT.Text = WF_ORG.SelectedValue
                            WF_SelectFIELD_CHANGE(WF_ButtonClick.Value)
                        Case "WF_KASANORGChange"    '加算先部門コードチェンジ
                            WF_KASANORGCODE_TEXT.Text = WF_KASANORG.SelectedValue
                            WF_SelectFIELD_CHANGE(WF_ButtonClick.Value)
                        Case "WF_SelectCALENDARChange" 'カレンダーチェンジ
                            WF_ACCOUNTCODE_TEXT.Text = ""
                            WF_SEGMENTCODE_TEXT.Text = ""

                            '勘定科目
                            Me.WF_ACCOUNT.Items.Clear()
                            Me.WF_ACCOUNT.Items.Add("")
                            Dim retAccountList As New DropDownList
                            retAccountList = LNM0007WRKINC.getDowpDownAccountList(WF_TARGETYM.Value)
                            For index As Integer = 0 To retAccountList.Items.Count - 1
                                WF_ACCOUNT.Items.Add(New ListItem(retAccountList.Items(index).Text, retAccountList.Items(index).Value))
                            Next

                            'セグメント
                            Me.WF_SEGMENT.Items.Clear()
                        Case "WF_ACCOUNTChange" '勘定科目チェンジ
                            WF_ACCOUNTCODE_TEXT.Text = WF_ACCOUNT.SelectedValue

                            Dim WK_TARGETYM As String = Replace(work.WF_SEL_TARGETYM.Text, "/", "")
                            Dim WW_YM As String = ""
                            '更新の場合
                            If Not DisabledKeyItem.Value = "" Then
                                WW_YM = WK_TARGETYM.Substring(0, 4) & "/" & WK_TARGETYM.Substring(4, 2)
                            Else
                                WW_YM = WF_TARGETYM.Value
                            End If

                            'セグメント
                            Me.WF_SEGMENT.Items.Clear()
                            WF_SEGMENTCODE_TEXT.Text = ""
                            Dim retSegmentList As New DropDownList
                            retSegmentList = LNM0007WRKINC.getDowpDownSegmentList(WW_YM, WF_ACCOUNT.SelectedValue)

                            If retSegmentList.Items.Count > 1 Then
                                Me.WF_SEGMENT.Items.Add("")
                            End If

                            For index As Integer = 0 To retSegmentList.Items.Count - 1
                                WF_SEGMENT.Items.Add(New ListItem(retSegmentList.Items(index).Text, retSegmentList.Items(index).Value))
                            Next

                            If WF_SEGMENT.Items.Count = 1 Then
                                WF_SEGMENTCODE_TEXT.Text = WF_SEGMENT.SelectedValue
                            End If

                        Case "WF_SEGMENTChange" 'セグメントチェンジ
                            WF_SEGMENTCODE_TEXT.Text = WF_SEGMENT.SelectedValue
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
            If Not IsNothing(LNM0007tbl) Then
                LNM0007tbl.Clear()
                LNM0007tbl.Dispose()
                LNM0007tbl = Nothing
            End If

            If Not IsNothing(LNM0007INPtbl) Then
                LNM0007INPtbl.Clear()
                LNM0007INPtbl.Dispose()
                LNM0007INPtbl = Nothing
            End If

            If Not IsNothing(LNM0007UPDtbl) Then
                LNM0007UPDtbl.Clear()
                LNM0007UPDtbl.Dispose()
                LNM0007UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0007WRKINC.MAPIDD
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

        '○ ドロップダウンリスト生成
        createListBox()

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' ドロップダウン生成処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub createListBox()
        '荷主
        Me.WF_TORI.Items.Clear()
        Me.WF_TORI.Items.Add("")
        'Dim retToriList As New DropDownList
        'retToriList = LNM0007WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG)
        Dim retToriList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "FIXEDTORI")
        For index As Integer = 0 To retToriList.Items.Count - 1
            WF_TORI.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
        Next

        'コンボボックス化
        Dim WW_TORI_OPTIONS As String = ""
        For index As Integer = 0 To retToriList.Items.Count - 1
            WW_TORI_OPTIONS += "<option>" + retToriList.Items(index).Text + "</option>"
        Next
        WF_TORI_DL.InnerHtml = WW_TORI_OPTIONS
        Me.WF_TORINAME.Attributes("list") = Me.WF_TORI_DL.ClientID

        '部門
        Me.WF_ORG.Items.Clear()
        Me.WF_ORG.Items.Add("")
        'Dim retOrgList As New DropDownList
        'retOrgList = LNM0007WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG)
        'Dim retOrgList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "FIXEDORG")
        'For index As Integer = 0 To retOrgList.Items.Count - 1
        '    WF_ORG.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
        'Next

        Dim retOfficeList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "FIXEDORG")
        If retOfficeList.Items.Count > 0 Then
            '情シス、高圧ガス以外
            If LNM0007WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
                Dim WW_OrgPermitHt As New Hashtable
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()  ' DataBase接続
                    work.GetPermitOrg(SQLcon, Master.USERCAMP, Master.ROLE_ORG, WW_OrgPermitHt)
                    For index As Integer = 0 To retOfficeList.Items.Count - 1
                        If WW_OrgPermitHt.ContainsKey(retOfficeList.Items(index).Value) = True Then
                            WF_ORG.Items.Add(New ListItem(retOfficeList.Items(index).Text, retOfficeList.Items(index).Value))
                        End If
                    Next
                End Using
            Else
                For index As Integer = 0 To retOfficeList.Items.Count - 1
                    WF_ORG.Items.Add(New ListItem(retOfficeList.Items(index).Text, retOfficeList.Items(index).Value))
                Next
            End If
        End If

        '加算先部門
        Me.WF_KASANORG.Items.Clear()
        Me.WF_KASANORG.Items.Add("")
        'Dim retKasanOrgList As New DropDownList
        'retKasanOrgList = LNM0007WRKINC.getDowpDownKasanOrgList(Master.MAPID, Master.ROLE_ORG)
        Dim retKasanOrgList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "FIXEDKASANORG")
        For index As Integer = 0 To retKasanOrgList.Items.Count - 1
            WF_KASANORG.Items.Add(New ListItem(retKasanOrgList.Items(index).Text, retKasanOrgList.Items(index).Value))
        Next

        Dim WK_TARGETYM As String = Replace(work.WF_SEL_TARGETYM.Text, "/", "")
        Dim WW_YM As String = ""
        If Not WK_TARGETYM = "" Then
            WW_YM = WK_TARGETYM.Substring(0, 4) & "/" & WK_TARGETYM.Substring(4, 2)
        Else
            WW_YM = work.WF_SEL_TARGETYM.Text
        End If

        '勘定科目
        Me.WF_ACCOUNT.Items.Clear()
        Me.WF_ACCOUNT.Items.Add("")
        Dim retAccountList As New DropDownList
        retAccountList = LNM0007WRKINC.getDowpDownAccountList(WW_YM)
        For index As Integer = 0 To retAccountList.Items.Count - 1
            WF_ACCOUNT.Items.Add(New ListItem(retAccountList.Items(index).Text, retAccountList.Items(index).Value))
        Next

        'セグメント
        Me.WF_SEGMENT.Items.Clear()
        Dim retSegmentList As New DropDownList
        retSegmentList = LNM0007WRKINC.getDowpDownSegmentList(WW_YM, work.WF_SEL_ACCOUNTCODE.Text)

        If retSegmentList.Items.Count > 1 Then
            Me.WF_SEGMENT.Items.Add("")
        End If

        For index As Integer = 0 To retSegmentList.Items.Count - 1
            WF_SEGMENT.Items.Add(New ListItem(retSegmentList.Items(index).Text, retSegmentList.Items(index).Value))
        Next

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0007L Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        TxtSelLineCNT.Text = work.WF_SEL_LINECNT.Text
        '削除
        RadioDELFLG.SelectedValue = work.WF_SEL_DELFLG.Text
        '画面ＩＤ
        TxtMapId.Text = "M00001"
        '会社コード
        TxtCampCode.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", TxtCampCode.Text, LblCampCodeName.Text, WW_RtnSW)
        '取引先コード、名称
        WF_TORICODE_TEXT.Text = work.WF_SEL_TORICODE.Text
        WF_TORINAME.Text = work.WF_SEL_TORINAME.Text
        WF_TORICODE_TEXT_SAVE.Value = work.WF_SEL_TORICODE.Text
        WF_TORINAME_SAVE.Value = work.WF_SEL_TORINAME.Text

        '部門コード、名称
        WF_ORG.SelectedValue = work.WF_SEL_ORGCODE.Text
        WF_ORGCODE_TEXT.Text = work.WF_SEL_ORGCODE.Text
        WF_ORG_SAVE.Value = work.WF_SEL_ORGCODE.Text

        '加算先部門コード、名称
        WF_KASANORG.SelectedValue = work.WF_SEL_KASANORGCODE.Text
        WF_KASANORGCODE_TEXT.Text = work.WF_SEL_KASANORGCODE.Text
        '対象年月
        Dim WK_TARGETYM As String = Replace(work.WF_SEL_TARGETYM.Text, "/", "")
        If Not WK_TARGETYM = "" Then
            WF_TARGETYM.Value = WK_TARGETYM.Substring(0, 4) & "/" & WK_TARGETYM.Substring(4, 2)
        Else
            WF_TARGETYM.Value = work.WF_SEL_TARGETYM.Text
        End If
        WF_TARGETYM_SAVE.Value = WF_TARGETYM.Value

        '車番
        TxtSYABAN.Text = work.WF_SEL_SYABAN.Text
        '陸事番号
        TxtRIKUBAN.Text = work.WF_SEL_RIKUBAN.Text
        '車型
        WF_SYAGATA.SelectedValue = work.WF_SEL_SYAGATA.Text
        '車型コード
        WF_SYAGATA_CODE_TEXT.Text = work.WF_SEL_SYAGATA.Text
        '車腹
        TxtSYABARA.Text = work.WF_SEL_SYABARA.Text
        '季節料金判定区分
        WF_SEASONKBN.SelectedValue = work.WF_SEL_SEASONKBN.Text
        WF_SEASONKBN_SAVE.Value = work.WF_SEL_SEASONKBN.Text
        '季節料金判定開始月日
        TxtSEASONSTART.Text = work.WF_SEL_SEASONSTART.Text
        '季節料金判定終了月日
        TxtSEASONEND.Text = work.WF_SEL_SEASONEND.Text
        '固定費(月額)
        TxtKOTEIHIM.Text = work.WF_SEL_KOTEIHIM.Text
        '固定費(日額)
        TxtKOTEIHID.Text = work.WF_SEL_KOTEIHID.Text
        '回数
        TxtKAISU.Text = work.WF_SEL_KAISU.Text
        '減額費用
        TxtGENGAKU.Text = work.WF_SEL_GENGAKU.Text
        '請求額
        TxtAMOUNT.Text = work.WF_SEL_AMOUNT.Text
        '勘定科目
        WF_ACCOUNT.SelectedValue = work.WF_SEL_ACCOUNTCODE.Text
        WF_ACCOUNTCODE_TEXT.Text = work.WF_SEL_ACCOUNTCODE.Text
        'セグメント
        WF_SEGMENT.SelectedValue = work.WF_SEL_SEGMENTCODE.Text
        WF_SEGMENTCODE_TEXT.Text = work.WF_SEL_SEGMENTCODE.Text
        '割合JOT
        TxtJOTPERCENTAGE.Text = work.WF_SEL_JOTPERCENTAGE.Text
        '割合ENEX
        TxtENEXPERCENTAGE.Text = work.WF_SEL_ENEXPERCENTAGE.Text
        '備考1
        TxtBIKOU1.Text = work.WF_SEL_BIKOU1.Text
        '備考2
        TxtBIKOU2.Text = work.WF_SEL_BIKOU2.Text
        '備考3
        TxtBIKOU3.Text = work.WF_SEL_BIKOU3.Text

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_SYABAN.Text

        ' 季節料金開始年月・季節料金終了年月・固定費(月額)・固定費(日額)を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.TxtSEASONSTART.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtSEASONEND.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtKOTEIHIM.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtKOTEIHID.Attributes("onkeyPress") = "CheckNum()"

        ' 有効開始日・有効終了日・対象年月を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        'Me.WF_StYMD.Attributes("onkeyPress") = "CheckCalendar()"
        'Me.WF_EndYMD.Attributes("onkeyPress") = "CheckCalendar()"

        ' 入力するテキストボックスは数値(0～9)＋記号(.)のみ可能とする。
        Me.TxtSYABARA.Attributes("onkeyPress") = "CheckDeci()"             '車腹
        Me.TxtJOTPERCENTAGE.Attributes("onkeyPress") = "CheckDeci()"       '割合JOT
        Me.TxtENEXPERCENTAGE.Attributes("onkeyPress") = "CheckDeci()"      '割合ENEX

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU
    End Sub

    ''' <summary>
    ''' 固定費マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(特別料金マスタ)
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("     INSERT INTO LNG.LNM0007_FIXED           ")
        SQLStr.AppendLine("        (                                    ")
        SQLStr.AppendLine("         DELFLG                              ")
        SQLStr.AppendLine("       , TORICODE                            ")
        SQLStr.AppendLine("       , TORINAME                            ")
        SQLStr.AppendLine("       , ORGCODE                             ")
        SQLStr.AppendLine("       , ORGNAME                             ")
        SQLStr.AppendLine("       , KASANORGCODE                        ")
        SQLStr.AppendLine("       , KASANORGNAME                        ")
        SQLStr.AppendLine("       , TARGETYM                            ")
        SQLStr.AppendLine("       , SYABAN                              ")
        SQLStr.AppendLine("       , RIKUBAN                             ")
        SQLStr.AppendLine("       , SYAGATA                             ")
        SQLStr.AppendLine("       , SYAGATANAME                         ")
        SQLStr.AppendLine("       , SYABARA                             ")
        SQLStr.AppendLine("       , SEASONKBN                           ")
        SQLStr.AppendLine("       , SEASONSTART                         ")
        SQLStr.AppendLine("       , SEASONEND                           ")
        SQLStr.AppendLine("       , KOTEIHIM                            ")
        SQLStr.AppendLine("       , KOTEIHID                            ")
        SQLStr.AppendLine("       , KAISU                               ")
        SQLStr.AppendLine("       , GENGAKU                             ")
        SQLStr.AppendLine("       , AMOUNT                              ")
        SQLStr.AppendLine("       , ACCOUNTCODE                         ")
        SQLStr.AppendLine("       , ACCOUNTNAME                         ")
        SQLStr.AppendLine("       , SEGMENTCODE                         ")
        SQLStr.AppendLine("       , SEGMENTNAME                         ")
        SQLStr.AppendLine("       , JOTPERCENTAGE                       ")
        SQLStr.AppendLine("       , ENEXPERCENTAGE                      ")
        SQLStr.AppendLine("       , BIKOU1                              ")
        SQLStr.AppendLine("       , BIKOU2                              ")
        SQLStr.AppendLine("       , BIKOU3                              ")
        SQLStr.AppendLine("       , INITYMD                             ")
        SQLStr.AppendLine("       , INITUSER                            ")
        SQLStr.AppendLine("       , INITTERMID                          ")
        SQLStr.AppendLine("       , INITPGID                            ")
        SQLStr.AppendLine("       , RECEIVEYMD                          ")
        SQLStr.AppendLine("        )                                    ")
        SQLStr.AppendLine("     VALUES                                  ")
        SQLStr.AppendLine("        (                                    ")
        SQLStr.AppendLine("         @DELFLG                             ")
        SQLStr.AppendLine("       , @TORICODE                           ")
        SQLStr.AppendLine("       , @TORINAME                           ")
        SQLStr.AppendLine("       , @ORGCODE                            ")
        SQLStr.AppendLine("       , @ORGNAME                            ")
        SQLStr.AppendLine("       , @KASANORGCODE                       ")
        SQLStr.AppendLine("       , @KASANORGNAME                       ")
        SQLStr.AppendLine("       , @TARGETYM                           ")
        SQLStr.AppendLine("       , @SYABAN                             ")
        SQLStr.AppendLine("       , @RIKUBAN                            ")
        SQLStr.AppendLine("       , @SYAGATA                            ")
        SQLStr.AppendLine("       , @SYAGATANAME                        ")
        SQLStr.AppendLine("       , @SYABARA                            ")
        SQLStr.AppendLine("       , @SEASONKBN                          ")
        SQLStr.AppendLine("       , @SEASONSTART                        ")
        SQLStr.AppendLine("       , @SEASONEND                          ")
        SQLStr.AppendLine("       , @KOTEIHIM                           ")
        SQLStr.AppendLine("       , @KOTEIHID                           ")
        SQLStr.AppendLine("       , @KAISU                              ")
        SQLStr.AppendLine("       , @GENGAKU                            ")
        SQLStr.AppendLine("       , @AMOUNT                             ")
        SQLStr.AppendLine("       , @ACCOUNTCODE                        ")
        SQLStr.AppendLine("       , @ACCOUNTNAME                        ")
        SQLStr.AppendLine("       , @SEGMENTCODE                        ")
        SQLStr.AppendLine("       , @SEGMENTNAME                        ")
        SQLStr.AppendLine("       , @JOTPERCENTAGE                      ")
        SQLStr.AppendLine("       , @ENEXPERCENTAGE                     ")
        SQLStr.AppendLine("       , @BIKOU1                             ")
        SQLStr.AppendLine("       , @BIKOU2                             ")
        SQLStr.AppendLine("       , @BIKOU3                             ")
        SQLStr.AppendLine("       , @INITYMD                            ")
        SQLStr.AppendLine("       , @INITUSER                           ")
        SQLStr.AppendLine("       , @INITTERMID                         ")
        SQLStr.AppendLine("       , @INITPGID                           ")
        SQLStr.AppendLine("       , @RECEIVEYMD                         ")
        SQLStr.AppendLine("        )                                    ")
        SQLStr.AppendLine("     ON DUPLICATE KEY UPDATE                 ")
        SQLStr.AppendLine("         DELFLG         = @DELFLG            ")
        SQLStr.AppendLine("       , TORICODE     = @TORICODE            ")
        SQLStr.AppendLine("       , TORINAME     = @TORINAME            ")
        SQLStr.AppendLine("       , ORGCODE     = @ORGCODE              ")
        SQLStr.AppendLine("       , ORGNAME     = @ORGNAME              ")
        SQLStr.AppendLine("       , KASANORGCODE     = @KASANORGCODE    ")
        SQLStr.AppendLine("       , KASANORGNAME     = @KASANORGNAME    ")
        SQLStr.AppendLine("       , TARGETYM     = @TARGETYM            ")
        SQLStr.AppendLine("       , SYABAN     = @SYABAN                ")
        SQLStr.AppendLine("       , RIKUBAN     = @RIKUBAN              ")
        SQLStr.AppendLine("       , SYAGATA     = @SYAGATA              ")
        SQLStr.AppendLine("       , SYAGATANAME     = @SYAGATANAME      ")
        SQLStr.AppendLine("       , SYABARA     = @SYABARA              ")
        SQLStr.AppendLine("       , SEASONKBN     = @SEASONKBN          ")
        SQLStr.AppendLine("       , SEASONSTART     = @SEASONSTART      ")
        SQLStr.AppendLine("       , SEASONEND     = @SEASONEND          ")
        SQLStr.AppendLine("       , KOTEIHIM     = @KOTEIHIM            ")
        SQLStr.AppendLine("       , KOTEIHID     = @KOTEIHID            ")
        SQLStr.AppendLine("       , KAISU     = @KAISU                  ")
        SQLStr.AppendLine("       , GENGAKU     = @GENGAKU              ")
        SQLStr.AppendLine("       , AMOUNT     = @AMOUNT                ")
        SQLStr.AppendLine("       , ACCOUNTCODE =  @ACCOUNTCODE")
        SQLStr.AppendLine("       , ACCOUNTNAME =  @ACCOUNTNAME")
        SQLStr.AppendLine("       , SEGMENTCODE =  @SEGMENTCODE")
        SQLStr.AppendLine("       , SEGMENTNAME =  @SEGMENTNAME")
        SQLStr.AppendLine("       , JOTPERCENTAGE =  @JOTPERCENTAGE")
        SQLStr.AppendLine("       , ENEXPERCENTAGE =  @ENEXPERCENTAGE")
        SQLStr.AppendLine("       , BIKOU1     = @BIKOU1                ")
        SQLStr.AppendLine("       , BIKOU2     = @BIKOU2                ")
        SQLStr.AppendLine("       , BIKOU3     = @BIKOU3                ")
        SQLStr.AppendLine("       , UPDYMD         = @UPDYMD            ")
        SQLStr.AppendLine("       , UPDUSER        = @UPDUSER           ")
        SQLStr.AppendLine("       , UPDTERMID      = @UPDTERMID         ")
        SQLStr.AppendLine("       , UPDPGID        = @UPDPGID           ")
        SQLStr.AppendLine("       , RECEIVEYMD     = @RECEIVEYMD        ")

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl = New StringBuilder
        SQLJnl.AppendLine(" SELECT                                     ")
        SQLJnl.AppendLine("     DELFLG                                 ")
        SQLJnl.AppendLine("   , TORICODE                               ")
        SQLJnl.AppendLine("   , TORINAME                               ")
        SQLJnl.AppendLine("   , ORGCODE                                ")
        SQLJnl.AppendLine("   , ORGNAME                                ")
        SQLJnl.AppendLine("   , KASANORGCODE                           ")
        SQLJnl.AppendLine("   , KASANORGNAME                           ")
        SQLJnl.AppendLine("   , TARGETYM                               ")
        SQLJnl.AppendLine("   , SYABAN                                 ")
        SQLJnl.AppendLine("   , RIKUBAN                                ")
        SQLJnl.AppendLine("   , SYAGATA                                ")
        SQLJnl.AppendLine("   , SYAGATANAME                            ")
        SQLJnl.AppendLine("   , SYABARA                                ")
        SQLJnl.AppendLine("   , SEASONKBN                              ")
        SQLJnl.AppendLine("   , SEASONSTART                            ")
        SQLJnl.AppendLine("   , SEASONEND                              ")
        SQLJnl.AppendLine("   , KOTEIHIM                               ")
        SQLJnl.AppendLine("   , KOTEIHID                               ")
        SQLJnl.AppendLine("   , KAISU                                  ")
        SQLJnl.AppendLine("   , GENGAKU                                ")
        SQLJnl.AppendLine("   , AMOUNT                                 ")
        SQLJnl.AppendLine("   , ACCOUNTCODE                         ")
        SQLJnl.AppendLine("   , ACCOUNTNAME                         ")
        SQLJnl.AppendLine("   , SEGMENTCODE                         ")
        SQLJnl.AppendLine("   , SEGMENTNAME                         ")
        SQLJnl.AppendLine("   , JOTPERCENTAGE                       ")
        SQLJnl.AppendLine("   , ENEXPERCENTAGE                      ")
        SQLJnl.AppendLine("   , BIKOU1                                 ")
        SQLJnl.AppendLine("   , BIKOU2                                 ")
        SQLJnl.AppendLine("   , BIKOU3                                 ")
        SQLJnl.AppendLine("   , INITYMD                                ")
        SQLJnl.AppendLine("   , INITUSER                               ")
        SQLJnl.AppendLine("   , INITTERMID                             ")
        SQLJnl.AppendLine("   , INITPGID                               ")
        SQLJnl.AppendLine("   , UPDYMD                                 ")
        SQLJnl.AppendLine("   , UPDUSER                                ")
        SQLJnl.AppendLine("   , UPDTERMID                              ")
        SQLJnl.AppendLine("   , UPDPGID                                ")
        SQLJnl.AppendLine("   , RECEIVEYMD                             ")
        SQLJnl.AppendLine("   , UPDTIMSTP                              ")
        SQLJnl.AppendLine(" FROM                                       ")
        SQLJnl.AppendLine("     LNG.LNM0007_FIXED                      ")
        SQLJnl.AppendLine(" WHERE                                      ")
        SQLJnl.AppendLine("         COALESCE(TORICODE, '')   = @TORICODE ")
        SQLJnl.AppendLine("    AND  COALESCE(ORGCODE, '')    = @ORGCODE ")
        SQLJnl.AppendLine("    AND  COALESCE(TARGETYM, '')   = @TARGETYM ")
        SQLJnl.AppendLine("    AND  COALESCE(SYABAN, '')     = @SYABAN ")
        SQLJnl.AppendLine("    AND  COALESCE(SEASONKBN, '')    = @SEASONKBN ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl.ToString, SQLcon)
                ' DB更新用パラメータ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_RIKUBAN As MySqlParameter = SQLcmd.Parameters.Add("@RIKUBAN", MySqlDbType.VarChar, 20)     '陸事番号
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYAGATANAME As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATANAME", MySqlDbType.VarChar, 50)     '車型名
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分
                Dim P_SEASONSTART As MySqlParameter = SQLcmd.Parameters.Add("@SEASONSTART", MySqlDbType.VarChar, 4)     '季節料金判定開始月日
                Dim P_SEASONEND As MySqlParameter = SQLcmd.Parameters.Add("@SEASONEND", MySqlDbType.VarChar, 4)     '季節料金判定終了月日
                Dim P_KOTEIHIM As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHIM", MySqlDbType.Decimal, 10)     '固定費(月額)
                Dim P_KOTEIHID As MySqlParameter = SQLcmd.Parameters.Add("@KOTEIHID", MySqlDbType.Decimal, 10)     '固定費(日額)
                Dim P_KAISU As MySqlParameter = SQLcmd.Parameters.Add("@KAISU", MySqlDbType.Decimal, 3)     '回数
                Dim P_GENGAKU As MySqlParameter = SQLcmd.Parameters.Add("@GENGAKU", MySqlDbType.Decimal, 10)     '減額費用
                Dim P_AMOUNT As MySqlParameter = SQLcmd.Parameters.Add("@AMOUNT", MySqlDbType.Decimal, 10)     '請求額
                Dim P_ACCOUNTCODE As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTCODE", MySqlDbType.Decimal, 8)     '勘定科目コード
                Dim P_ACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNAME", MySqlDbType.VarChar, 100)     '勘定科目名
                Dim P_SEGMENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTCODE", MySqlDbType.Decimal, 5)     'セグメントコード
                Dim P_SEGMENTNAME As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTNAME", MySqlDbType.VarChar, 100)     'セグメント名
                Dim P_JOTPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@JOTPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合JOT
                Dim P_ENEXPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@ENEXPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合ENEX
                Dim P_BIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU1", MySqlDbType.VarChar, 50)     '備考1
                Dim P_BIKOU2 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU2", MySqlDbType.VarChar, 50)     '備考2
                Dim P_BIKOU3 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU3", MySqlDbType.VarChar, 50)     '備考3
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JP_TORICODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim JP_ORGCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim JP_TARGETYM As MySqlParameter = SQLcmdJnl.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim JP_SYABAN As MySqlParameter = SQLcmdJnl.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim JP_SEASONKBN As MySqlParameter = SQLcmdJnl.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分

                Dim LNM0007row As DataRow = LNM0007INPtbl.Rows(0)

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                P_DELFLG.Value = LNM0007row("DELFLG")               '削除フラグ
                P_TORICODE.Value = LNM0007row("TORICODE")           '取引先コード
                P_TORINAME.Value = LNM0007row("TORINAME")           '取引先名称
                P_ORGCODE.Value = LNM0007row("ORGCODE")           '部門コード
                P_ORGNAME.Value = LNM0007row("ORGNAME")           '部門名称
                P_KASANORGCODE.Value = LNM0007row("KASANORGCODE")           '加算先部門コード
                P_KASANORGNAME.Value = LNM0007row("KASANORGNAME")           '加算先部門名称
                P_TARGETYM.Value = LNM0007row("TARGETYM")           '対象年月
                P_SYABAN.Value = LNM0007row("SYABAN")           '車番
                P_RIKUBAN.Value = LNM0007row("RIKUBAN")           '陸事番号
                P_SYAGATA.Value = LNM0007row("SYAGATA")           '車型
                P_SYAGATANAME.Value = LNM0007row("SYAGATANAME")           '車型名

                '車腹
                If LNM0007row("SYABARA").ToString = "0" Or LNM0007row("SYABARA").ToString = "" Then
                    P_SYABARA.Value = DBNull.Value
                Else
                    P_SYABARA.Value = LNM0007row("SYABARA")
                End If

                P_SEASONKBN.Value = LNM0007row("SEASONKBN")           '季節料金判定区分
                P_SEASONSTART.Value = LNM0007row("SEASONSTART")           '季節料金判定開始月日
                P_SEASONEND.Value = LNM0007row("SEASONEND")           '季節料金判定終了月日

                '固定費(月額)
                If LNM0007row("KOTEIHIM").ToString = "0" Or LNM0007row("KOTEIHIM").ToString = "" Then
                    P_KOTEIHIM.Value = DBNull.Value
                Else
                    P_KOTEIHIM.Value = LNM0007row("KOTEIHIM")
                End If

                '固定費(日額)
                If LNM0007row("KOTEIHID").ToString = "0" Or LNM0007row("KOTEIHID").ToString = "" Then
                    P_KOTEIHID.Value = DBNull.Value
                Else
                    P_KOTEIHID.Value = LNM0007row("KOTEIHID")
                End If

                '回数
                If LNM0007row("KAISU").ToString = "0" Or LNM0007row("KAISU").ToString = "" Then
                    P_KAISU.Value = DBNull.Value
                Else
                    P_KAISU.Value = LNM0007row("KAISU")
                End If

                '減額費用
                If LNM0007row("GENGAKU").ToString = "0" Or LNM0007row("GENGAKU").ToString = "" Then
                    P_GENGAKU.Value = DBNull.Value
                Else
                    P_GENGAKU.Value = LNM0007row("GENGAKU")
                End If

                '請求額
                If LNM0007row("AMOUNT").ToString = "0" Or LNM0007row("AMOUNT").ToString = "" Then
                    P_AMOUNT.Value = DBNull.Value
                Else
                    P_AMOUNT.Value = LNM0007row("AMOUNT")
                End If

                '勘定科目コード
                If LNM0007row("ACCOUNTCODE").ToString = "" Then
                    P_ACCOUNTCODE.Value = DBNull.Value
                Else
                    P_ACCOUNTCODE.Value = LNM0007row("ACCOUNTCODE")
                End If

                P_ACCOUNTNAME.Value = LNM0007row("ACCOUNTNAME")           '勘定科目名

                'セグメントコード
                If LNM0007row("SEGMENTCODE").ToString = "" Then
                    P_SEGMENTCODE.Value = DBNull.Value
                Else
                    P_SEGMENTCODE.Value = LNM0007row("SEGMENTCODE")
                End If

                P_SEGMENTNAME.Value = LNM0007row("SEGMENTNAME")           'セグメント名

                '割合JOT
                If LNM0007row("JOTPERCENTAGE").ToString = "" Then
                    P_JOTPERCENTAGE.Value = DBNull.Value
                Else
                    P_JOTPERCENTAGE.Value = LNM0007row("JOTPERCENTAGE")
                End If

                '割合ENEX
                If LNM0007row("ENEXPERCENTAGE").ToString = "" Then
                    P_ENEXPERCENTAGE.Value = DBNull.Value
                Else
                    P_ENEXPERCENTAGE.Value = LNM0007row("ENEXPERCENTAGE")
                End If

                P_BIKOU1.Value = LNM0007row("BIKOU1")           '備考1
                P_BIKOU2.Value = LNM0007row("BIKOU2")           '備考2
                P_BIKOU3.Value = LNM0007row("BIKOU3")           '備考3

                P_INITYMD.Value = WW_DateNow                        '登録年月日
                P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID              '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                P_UPDYMD.Value = WW_DateNow                         '更新年月日
                P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JP_TORICODE.Value = LNM0007row("TORICODE")           '取引先コード
                JP_ORGCODE.Value = LNM0007row("ORGCODE")           '部門コード
                JP_TARGETYM.Value = LNM0007row("TARGETYM")           '対象年月
                JP_SYABAN.Value = LNM0007row("SYABAN")           '車番
                JP_SEASONKBN.Value = LNM0007row("SEASONKBN")           '季節料金判定区分

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0007UPDtbl) Then
                        LNM0007UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0007UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0007UPDtbl.Clear()
                    LNM0007UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0007UPDrow As DataRow In LNM0007UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0007D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0007UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007D UPDATE_INSERT"
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

        '固定費マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0007_FIXED")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(TARGETYM, '')             = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
        SQLStr.AppendLine("    AND  COALESCE(SEASONKBN, '')             = @SEASONKBN ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分

                Dim LNM0007row As DataRow = LNM0007INPtbl.Rows(0)

                P_TORICODE.Value = LNM0007row("TORICODE")           '取引先コード
                P_ORGCODE.Value = LNM0007row("ORGCODE")           '部門コード
                P_TARGETYM.Value = LNM0007row("TARGETYM")           '対象年月
                P_SYABAN.Value = LNM0007row("SYABAN")           '車番
                P_SEASONKBN.Value = LNM0007row("SEASONKBN")           '季節料金判定区分

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
                        WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0007C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0006_FIXEDHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,TARGETYM  ")
        SQLStr.AppendLine("     ,SYABAN  ")
        SQLStr.AppendLine("     ,RIKUBAN  ")
        SQLStr.AppendLine("     ,SYAGATA  ")
        SQLStr.AppendLine("     ,SYAGATANAME  ")
        SQLStr.AppendLine("     ,SYABARA  ")
        SQLStr.AppendLine("     ,SEASONKBN  ")
        SQLStr.AppendLine("     ,SEASONSTART  ")
        SQLStr.AppendLine("     ,SEASONEND  ")
        SQLStr.AppendLine("     ,KOTEIHIM  ")
        SQLStr.AppendLine("     ,KOTEIHID  ")
        SQLStr.AppendLine("     ,KAISU  ")
        SQLStr.AppendLine("     ,GENGAKU  ")
        SQLStr.AppendLine("     ,AMOUNT  ")
        SQLStr.AppendLine("     ,ACCOUNTCODE  ")
        SQLStr.AppendLine("     ,ACCOUNTNAME  ")
        SQLStr.AppendLine("     ,SEGMENTCODE  ")
        SQLStr.AppendLine("     ,SEGMENTNAME  ")
        SQLStr.AppendLine("     ,JOTPERCENTAGE  ")
        SQLStr.AppendLine("     ,ENEXPERCENTAGE  ")
        SQLStr.AppendLine("     ,BIKOU1  ")
        SQLStr.AppendLine("     ,BIKOU2  ")
        SQLStr.AppendLine("     ,BIKOU3  ")
        SQLStr.AppendLine("     ,OPERATEKBN  ")
        SQLStr.AppendLine("     ,MODIFYKBN  ")
        SQLStr.AppendLine("     ,MODIFYYMD  ")
        SQLStr.AppendLine("     ,MODIFYUSER  ")
        SQLStr.AppendLine("     ,DELFLG  ")
        SQLStr.AppendLine("     ,INITYMD  ")
        SQLStr.AppendLine("     ,INITUSER  ")
        SQLStr.AppendLine("     ,INITTERMID  ")
        SQLStr.AppendLine("     ,INITPGID  ")
        SQLStr.AppendLine("  )  ")
        SQLStr.AppendLine("  SELECT  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,TARGETYM  ")
        SQLStr.AppendLine("     ,SYABAN  ")
        SQLStr.AppendLine("     ,RIKUBAN  ")
        SQLStr.AppendLine("     ,SYAGATA  ")
        SQLStr.AppendLine("     ,SYAGATANAME  ")
        SQLStr.AppendLine("     ,SYABARA  ")
        SQLStr.AppendLine("     ,SEASONKBN  ")
        SQLStr.AppendLine("     ,SEASONSTART  ")
        SQLStr.AppendLine("     ,SEASONEND  ")
        SQLStr.AppendLine("     ,KOTEIHIM  ")
        SQLStr.AppendLine("     ,KOTEIHID  ")
        SQLStr.AppendLine("     ,KAISU  ")
        SQLStr.AppendLine("     ,GENGAKU  ")
        SQLStr.AppendLine("     ,AMOUNT  ")
        SQLStr.AppendLine("     ,ACCOUNTCODE  ")
        SQLStr.AppendLine("     ,ACCOUNTNAME  ")
        SQLStr.AppendLine("     ,SEGMENTCODE  ")
        SQLStr.AppendLine("     ,SEGMENTNAME  ")
        SQLStr.AppendLine("     ,JOTPERCENTAGE  ")
        SQLStr.AppendLine("     ,ENEXPERCENTAGE  ")
        SQLStr.AppendLine("     ,BIKOU1  ")
        SQLStr.AppendLine("     ,BIKOU2  ")
        SQLStr.AppendLine("     ,BIKOU3  ")
        SQLStr.AppendLine("     ,@OPERATEKBN AS OPERATEKBN ")
        SQLStr.AppendLine("     ,@MODIFYKBN AS MODIFYKBN ")
        SQLStr.AppendLine("     ,@MODIFYYMD AS MODIFYYMD ")
        SQLStr.AppendLine("     ,@MODIFYUSER AS MODIFYUSER ")
        SQLStr.AppendLine("     ,DELFLG ")
        SQLStr.AppendLine("     ,@INITYMD AS INITYMD ")
        SQLStr.AppendLine("     ,@INITUSER AS INITUSER ")
        SQLStr.AppendLine("     ,@INITTERMID AS INITTERMID ")
        SQLStr.AppendLine("     ,@INITPGID AS INITPGID ")
        SQLStr.AppendLine("  FROM   ")
        SQLStr.AppendLine("        LNG.LNM0007_FIXED")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(TARGETYM, '')             = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
        SQLStr.AppendLine("    AND  COALESCE(SEASONKBN, '')             = @SEASONKBN ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0007row As DataRow = LNM0007INPtbl.Rows(0)

                ' DB更新
                P_TORICODE.Value = LNM0007row("TORICODE")           '取引先コード
                P_ORGCODE.Value = LNM0007row("ORGCODE")           '部門コード
                P_TARGETYM.Value = LNM0007row("TARGETYM")           '対象年月
                P_SYABAN.Value = LNM0007row("SYABAN")           '車番
                P_SEASONKBN.Value = LNM0007row("SEASONKBN")           '季節料金判定区分

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0007WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0007tbl.Rows(0)("DELFLG") = "0" And LNM0007row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0007WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0007WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0006_FIXEDHIST INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0006_FIXEDHIST INSERT"
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
    ''' 詳細画面-更新ボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '論理削除の場合は入力チェックを省略、削除フラグのみ更新
        If Not DisabledKeyItem.Value = "" And
            work.WF_SEL_DELFLG.Text = C_DELETE_FLG.ALIVE And
            RadioDELFLG.SelectedValue = C_DELETE_FLG.DELETE Then

            ' マスタ更新(削除フラグのみ)
            UpdateMasterDelflgOnly()
            If Not isNormal(WW_ErrSW) Then
                Exit Sub
            End If
            work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = "Update Success!!"
            ' 前ページ遷移
            Master.TransitionPrevPage()
            Exit Sub
        End If

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0007INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0007tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0007tbl, work.WF_SEL_INPTBL.Text)

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
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, "ユーザー", needsPopUp:=True)
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
    Protected Sub DetailBoxToLNM0007INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(RadioDELFLG.SelectedValue)      '削除フラグ
        Master.EraseCharToIgnore(WF_TARGETYM.Value)  '対象年月
        Master.EraseCharToIgnore(TxtSYABAN.Text)  '車番
        Master.EraseCharToIgnore(TxtRIKUBAN.Text)  '陸事番号
        Master.EraseCharToIgnore(TxtSYABARA.Text)  '車腹
        Master.EraseCharToIgnore(TxtSEASONSTART.Text)  '季節料金判定開始月日
        Master.EraseCharToIgnore(TxtSEASONEND.Text)  '季節料金判定終了月日
        Master.EraseCharToIgnore(TxtKOTEIHIM.Text)  '固定費(月額)
        Master.EraseCharToIgnore(TxtKOTEIHID.Text)  '固定費(日額)
        Master.EraseCharToIgnore(TxtKAISU.Text)  '回数
        Master.EraseCharToIgnore(TxtGENGAKU.Text)  '減額費用
        Master.EraseCharToIgnore(TxtAMOUNT.Text)  '請求額
        Master.EraseCharToIgnore(TxtBIKOU1.Text)  '備考1
        Master.EraseCharToIgnore(TxtBIKOU2.Text)  '備考2
        Master.EraseCharToIgnore(TxtBIKOU3.Text)  '備考3


        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) AndAlso
            String.IsNullOrEmpty(RadioDELFLG.SelectedValue) Then
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

        Master.CreateEmptyTable(LNM0007INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0007INProw As DataRow = LNM0007INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) Then
            LNM0007INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(TxtSelLineCNT.Text, LNM0007INProw("LINECNT"))
            Catch ex As Exception
                LNM0007INProw("LINECNT") = 0
            End Try
        End If

        LNM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0007INProw("UPDTIMSTP") = 0
        LNM0007INProw("SELECT") = 1
        LNM0007INProw("HIDDEN") = 0

        LNM0007INProw("DELFLG") = RadioDELFLG.SelectedValue             '削除フラグ

        '更新の場合
        If Not DisabledKeyItem.Value = "" Then
            LNM0007INProw("TORICODE") = work.WF_SEL_TORICODE.Text     '取引先コード
            LNM0007INProw("TORINAME") = work.WF_SEL_TORINAME.Text      '取引先名称
            LNM0007INProw("ORGCODE") = work.WF_SEL_ORGCODE.Text          '部門コード
            LNM0007INProw("ORGNAME") = work.WF_SEL_ORGNAME.Text           '部門名称
            LNM0007INProw("TARGETYM") = work.WF_SEL_TARGETYM.Text         '対象年月
            LNM0007INProw("SEASONKBN") = work.WF_SEL_SEASONKBN.Text           '季節料金判定区分
        Else
            LNM0007INProw("TORICODE") = WF_TORICODE_TEXT.Text            '取引先コード
            LNM0007INProw("TORINAME") = WF_TORINAME.Text            '取引先名称
            LNM0007INProw("ORGCODE") = WF_ORG.SelectedValue           '部門コード
            LNM0007INProw("ORGNAME") = WF_ORG.SelectedItem           '部門名称

            '対象年月
            If Not WF_TARGETYM.Value = "" Then
                LNM0007INProw("TARGETYM") = Replace(WF_TARGETYM.Value, "/", "")
            Else
                LNM0007INProw("TARGETYM") = WF_TARGETYM.Value
            End If

            LNM0007INProw("SEASONKBN") = WF_SEASONKBN.SelectedValue            '季節料金判定区分
        End If

        LNM0007INProw("KASANORGCODE") = WF_KASANORG.SelectedValue            '加算先部門コード
        LNM0007INProw("KASANORGNAME") = WF_KASANORG.SelectedItem            '加算先部門名称

        LNM0007INProw("SYABAN") = TxtSYABAN.Text            '車番
        LNM0007INProw("RIKUBAN") = TxtRIKUBAN.Text            '陸事番号
        LNM0007INProw("SYAGATA") = WF_SYAGATA.SelectedValue           '車型
        LNM0007INProw("SYAGATANAME") = WF_SYAGATA.SelectedItem            '車型名
        LNM0007INProw("SYABARA") = TxtSYABARA.Text            '車腹


        '季節料金判定開始月日
        If Not TxtSEASONSTART.Text = "" Then
            LNM0007INProw("SEASONSTART") = Replace(TxtSEASONSTART.Text, "/", "")
        Else
            LNM0007INProw("SEASONSTART") = TxtSEASONSTART.Text
        End If

        '季節料金判定終了月日
        If Not TxtSEASONEND.Text = "" Then
            LNM0007INProw("SEASONEND") = Replace(TxtSEASONEND.Text, "/", "")
        Else
            LNM0007INProw("SEASONEND") = TxtSEASONEND.Text
        End If

        LNM0007INProw("KOTEIHIM") = TxtKOTEIHIM.Text            '固定費(月額)
        LNM0007INProw("KOTEIHID") = TxtKOTEIHID.Text            '固定費(日額)
        LNM0007INProw("KAISU") = TxtKAISU.Text            '回数
        LNM0007INProw("GENGAKU") = TxtGENGAKU.Text            '減額費用
        LNM0007INProw("AMOUNT") = TxtAMOUNT.Text            '請求額

        LNM0007INProw("ACCOUNTCODE") = WF_ACCOUNT.SelectedValue           '勘定科目コード
        LNM0007INProw("ACCOUNTNAME") = WF_ACCOUNT.SelectedItem            '勘定科目名

        If Not WF_ACCOUNT.SelectedValue = "" Then
            LNM0007INProw("SEGMENTCODE") = WF_SEGMENT.SelectedValue           'セグメントコード
            LNM0007INProw("SEGMENTNAME") = WF_SEGMENT.SelectedItem            'セグメント名
        Else
            LNM0007INProw("SEGMENTCODE") = ""           'セグメントコード
            LNM0007INProw("SEGMENTNAME") = ""            'セグメント名
        End If

        LNM0007INProw("JOTPERCENTAGE") = TxtJOTPERCENTAGE.Text            '割合JOT
        LNM0007INProw("ENEXPERCENTAGE") = TxtENEXPERCENTAGE.Text            '割合ENEX


        LNM0007INProw("BIKOU1") = TxtBIKOU1.Text            '備考1
        LNM0007INProw("BIKOU2") = TxtBIKOU2.Text            '備考2
        LNM0007INProw("BIKOU3") = TxtBIKOU3.Text            '備考3


        '○ チェック用テーブルに登録する
        LNM0007INPtbl.Rows.Add(LNM0007INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0007INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0007INProw As DataRow = LNM0007INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0007row As DataRow In LNM0007tbl.Rows
            ' KEY項目が等しい時
            If LNM0007row("TORICODE") = LNM0007INProw("TORICODE") AndAlso                       '取引先コード
                LNM0007row("ORGCODE") = LNM0007INProw("ORGCODE") AndAlso                        '部門コード
                LNM0007row("TARGETYM") = LNM0007INProw("TARGETYM") AndAlso                      '対象年月
                LNM0007row("SYABAN") = LNM0007INProw("SYABAN") AndAlso                          '車番
                LNM0007row("SYABARA") = LNM0007INProw("SYABARA") AndAlso                        '車腹
                LNM0007row("SEASONKBN") = LNM0007INProw("SEASONKBN") Then                       '季節料金判定区分
                ' KEY項目以外の項目の差異をチェック
                If LNM0007row("DELFLG") = LNM0007INProw("DELFLG") AndAlso
                    LNM0007row("TORINAME") = LNM0007INProw("TORINAME") AndAlso                                '取引先名称
                    LNM0007row("ORGNAME") = LNM0007INProw("ORGNAME") AndAlso                                '部門名称
                    LNM0007row("KASANORGCODE") = LNM0007INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                    LNM0007row("KASANORGNAME") = LNM0007INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                    LNM0007row("SYABAN") = LNM0007INProw("SYABAN") AndAlso                                '車番
                    LNM0007row("RIKUBAN") = LNM0007INProw("RIKUBAN") AndAlso                                '陸事番号
                    LNM0007row("SYAGATA") = LNM0007INProw("SYAGATA") AndAlso                                '車型
                    LNM0007row("SYAGATANAME") = LNM0007INProw("SYAGATANAME") AndAlso                                '車型名
                    LNM0007row("SEASONSTART") = LNM0007INProw("SEASONSTART") AndAlso                                '季節料金判定開始月日
                    LNM0007row("SEASONEND") = LNM0007INProw("SEASONEND") AndAlso                                '季節料金判定終了月日
                    LNM0007row("KOTEIHIM") = LNM0007INProw("KOTEIHIM") AndAlso                                '固定費(月額)
                    LNM0007row("KOTEIHID") = LNM0007INProw("KOTEIHID") AndAlso                                '固定費(日額)
                    LNM0007row("KAISU") = LNM0007INProw("KAISU") AndAlso                                '回数
                    LNM0007row("GENGAKU") = LNM0007INProw("GENGAKU") AndAlso                                '減額費用
                    LNM0007row("AMOUNT") = LNM0007INProw("AMOUNT") AndAlso                                '請求額
                    LNM0007row("ACCOUNTCODE") = LNM0007INProw("ACCOUNTCODE") AndAlso                                '勘定科目コード
                    LNM0007row("ACCOUNTNAME") = LNM0007INProw("ACCOUNTNAME") AndAlso                                '勘定科目名
                    LNM0007row("SEGMENTCODE") = LNM0007INProw("SEGMENTCODE") AndAlso                                'セグメントコード
                    LNM0007row("SEGMENTNAME") = LNM0007INProw("SEGMENTNAME") AndAlso                                'セグメント名
                    LNM0007row("JOTPERCENTAGE") = LNM0007INProw("JOTPERCENTAGE") AndAlso                                '割合JOT
                    LNM0007row("ENEXPERCENTAGE") = LNM0007INProw("ENEXPERCENTAGE") AndAlso                                '割合ENEX
                    LNM0007row("BIKOU1") = LNM0007INProw("BIKOU1") AndAlso                                '備考1
                    LNM0007row("BIKOU2") = LNM0007INProw("BIKOU2") AndAlso                                '備考2
                    LNM0007row("BIKOU3") = LNM0007INProw("BIKOU3") Then                                '備考3

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
        For Each LNM0007row As DataRow In LNM0007tbl.Rows
            Select Case LNM0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0007tbl, work.WF_SEL_INPTBL.Text)

        TxtSelLineCNT.Text = ""              'LINECNT
        TxtMapId.Text = "M00001"             '画面ＩＤ
        RadioDELFLG.SelectedValue = ""                  '削除フラグ
        WF_TARGETYM.Value = ""                    '対象年月
        TxtSYABAN.Text = ""                    '車番
        TxtRIKUBAN.Text = ""                    '陸事番号
        TxtSYABARA.Text = ""                    '車腹
        TxtSEASONSTART.Text = ""                    '季節料金判定開始月日
        TxtSEASONEND.Text = ""                    '季節料金判定終了月日
        TxtKOTEIHIM.Text = ""                    '固定費(月額)
        TxtKOTEIHID.Text = ""                    '固定費(日額)
        TxtKAISU.Text = ""                    '回数
        TxtGENGAKU.Text = ""                    '減額費用
        TxtAMOUNT.Text = ""                    '請求額
        TxtJOTPERCENTAGE.Text = ""                    '割合JOT
        TxtENEXPERCENTAGE.Text = ""                    '割合ENEX
        TxtBIKOU1.Text = ""                    '備考1
        TxtBIKOU2.Text = ""                    '備考2
        TxtBIKOU3.Text = ""                    '備考3

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
                    Case "TxtDelFlg"
                        leftview.Visible = True
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
            'Case "TxtDelFlg"      '削除フラグ
            '    CODENAME_get("DELFLG", RadioDELFLG.SelectedValue, LblDelFlgName.Text, WW_Dummy)
            '    TxtDelFlg.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 固定費マスタ更新(削除フラグのみ)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UpdateMasterDelflgOnly()
        Dim WW_MODIFYKBN As String = ""
        Dim WW_DATE As Date = Date.Now

        '初期化
        LNM0007INPtbl = New DataTable
        LNM0007INPtbl.Columns.Add("TORICODE")
        LNM0007INPtbl.Columns.Add("ORGCODE")
        LNM0007INPtbl.Columns.Add("TARGETYM")
        LNM0007INPtbl.Columns.Add("SYABAN")
        LNM0007INPtbl.Columns.Add("SEASONKBN")
        LNM0007INPtbl.Columns.Add("DELFLG")

        Dim row As DataRow
        row = LNM0007INPtbl.NewRow
        row("TORICODE") = work.WF_SEL_TORICODE.Text
        row("ORGCODE") = work.WF_SEL_ORGCODE.Text
        row("TARGETYM") = work.WF_SEL_TARGETYM.Text
        row("SYABAN") = work.WF_SEL_SYABAN.Text
        row("SEASONKBN") = work.WF_SEL_SEASONKBN.Text
        row("DELFLG") = C_DELETE_FLG.DELETE
        LNM0007INPtbl.Rows.Add(row)

        ' DB更新処理
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()

            '履歴テーブルに変更前データを登録
            InsertHist(SQLcon, LNM0007WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '削除フラグ更新
            SetDelflg(SQLcon, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '履歴テーブルに変更後データを登録
            InsertHist(SQLcon, LNM0007WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

        End Using

        '○ 入力値反映
        For Each LNM0007INProw As DataRow In LNM0007INPtbl.Rows
            For Each LNM0007row As DataRow In LNM0007tbl.Rows
                If LNM0007INProw("TORICODE") = LNM0007row("TORICODE") AndAlso
                            LNM0007INProw("ORGCODE") = LNM0007row("ORGCODE") AndAlso
                            LNM0007INProw("TARGETYM") = LNM0007row("TARGETYM") AndAlso
                            LNM0007INProw("SYABAN") = LNM0007row("SYABAN") AndAlso
                            LNM0007INProw("SEASONKBN") = LNM0007row("SEASONKBN") Then
                    ' 画面入力テーブル項目設定              
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0007row("DELFLG") = LNM0007INProw("DELFLG")
                    LNM0007row("SELECT") = 0
                    LNM0007row("HIDDEN") = 0
                    Exit For
                End If
            Next
        Next

    End Sub

    ''' <summary>
    ''' 削除フラグ更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_NOW"></param>
    ''' <remarks></remarks>
    Public Sub SetDelflg(ByVal SQLcon As MySqlConnection, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ更新
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        SQLStr.Append("     LNG.LNM0007_FIXED                     ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(TARGETYM, '')             = @TARGETYM ")
        SQLStr.Append("    AND  COALESCE(SYABAN, '')             = @SYABAN ")
        SQLStr.Append("    AND  COALESCE(SEASONKBN, '')             = @SEASONKBN ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_SYABAN As MySqlParameter = SQLcmd.Parameters.Add("@SYABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_SEASONKBN As MySqlParameter = SQLcmd.Parameters.Add("@SEASONKBN", MySqlDbType.VarChar, 1)     '季節料金判定区分
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                Dim LNM0007row As DataRow = LNM0007INPtbl.Rows(0)
                P_TORICODE.Value = LNM0007row("TORICODE")           '取引先コード
                P_ORGCODE.Value = LNM0007row("ORGCODE")           '部門コード
                P_TARGETYM.Value = LNM0007row("TARGETYM")           '対象年月
                P_SYABAN.Value = LNM0007row("SYABAN")           '車番
                P_SEASONKBN.Value = LNM0007row("SEASONKBN")           '季節料金判定区分
                P_UPDYMD.Value = WW_NOW                '更新年月日
                P_UPDUSER.Value = Master.USERID                '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID                '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name          '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0007C UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

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
                'Case "TxtDelFlg"      '削除フラグ
                '    RadioDELFLG.SelectedValue = WW_SelectValue
                '    LblDelFlgName.Text = WW_SelectText
                '    TxtDelFlg.Focus()
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

        '○ フォーカスセット
        If String.IsNullOrEmpty(WF_FIELD_REP.Value) Then
            Select Case WF_FIELD.Value
                'Case "TxtDelFlg"            '削除フラグ
                '    TxtDelFlg.Focus()
            End Select
        End If

        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub

    ' ******************************************************************************
    ' ***  フィールド変更処理                                                    ***
    ' ******************************************************************************
    ''' <summary>
    ''' フィールド(変更)時処理
    ''' </summary>
    ''' <param name="resVal">取引(変更)時(WF_SelectTORIChange),部門(変更)時(WF_SelectORGChange),加算先部門(変更)時(WF_SelectKASANORGChange)</param>
    ''' <remarks></remarks>
    Protected Sub WF_SelectFIELD_CHANGE(ByVal resVal As String)
        '■取引先(情報)取得
        Dim selectTORI As String = WF_TORICODE_TEXT.Text
        'Dim selectTORI As String = WF_TORI.SelectedValue
        Dim selectindexTORI As Integer = WF_TORI.SelectedIndex
        '■部門(情報)取得
        Dim selectORG As String = WF_ORG.SelectedValue
        Dim selectindexORG As Integer = WF_ORG.SelectedIndex
        '■加算先部門(情報)取得
        Dim selectKASANORG As String = WF_KASANORG.SelectedValue
        Dim selectindexKASANORG As Integer = WF_KASANORG.SelectedIndex

        '〇フィールド(変更)ボタン
        Select Case resVal
            '取引先(変更)時
            Case "WF_TORIChange"
                If selectTORI = "" Then
                    selectORG = ""              '-- 部門(表示)初期化
                    selectindexORG = 0          '-- 部門(INDEX)初期化
                    selectKASANORG = ""         '-- 加算先部門(表示)初期化
                    selectindexKASANORG = 0     '-- 加算先部門(INDEX)初期化
                End If
            '部門(変更)時
            Case "WF_ORGChange"
                selectKASANORG = ""         '-- 加算先部門(表示)初期化
                selectindexKASANORG = 0     '-- 加算先部門(INDEX)初期化
            '加算先部門(変更)時
            Case "WF_KASANORGChange"
        End Select

        '〇取引先
        Me.WF_TORI.Items.Clear()
        Dim retToriList As New DropDownList
        retToriList = LNM0007WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG, I_CREATEFLG:=True)
        'retToriList = LNM0007WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG, I_CREATEFLG:=True)
        WF_TORI.Items.Add(New ListItem("", ""))
        '★ドロップダウンリスト選択(取引先)の場合
        If retToriList.Items.Count = 1 Then
            selectindexTORI = 1
        End If
        '★ドロップダウンリスト再作成(取引先)
        For index As Integer = 0 To retToriList.Items.Count - 1
            WF_TORI.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
        Next
        WF_TORI.SelectedIndex = selectindexTORI
        WF_TORINAME.Text = WF_TORI.Items(Integer.Parse(selectindexTORI)).Text
        WF_TORICODE_TEXT.Text = WF_TORI.Items(Integer.Parse(selectindexTORI)).Value
        'WF_TORICODE_TEXT.Text = WF_TORI.SelectedValue

        '〇部門
        Me.WF_ORG.Items.Clear()
        Dim retOrgList As New DropDownList
        retOrgList = LNM0007WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG, I_CREATEFLG:=True)
        If selectTORI <> "" AndAlso retOrgList.Items.Count = 0 Then
            selectORG = ""              '-- 部門(表示)初期化
            selectindexORG = 0          '-- 部門(INDEX)初期化
            selectKASANORG = ""         '-- 加算先部門(表示)初期化
            selectindexKASANORG = 0     '-- 加算先部門(INDEX)初期化
            retOrgList = LNM0007WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG, I_CREATEFLG:=True)
        End If
        WF_ORG.Items.Add(New ListItem("", ""))
        '★ドロップダウンリスト選択(部門)の場合
        If retOrgList.Items.Count = 1 Then
            selectindexORG = 1
        End If
        '★ドロップダウンリスト再作成(部門)
        For index As Integer = 0 To retOrgList.Items.Count - 1
            WF_ORG.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
        Next
        WF_ORG.SelectedIndex = selectindexORG
        WF_ORGCODE_TEXT.Text = WF_ORG.SelectedValue

        '〇加算先部門
        Me.WF_KASANORG.Items.Clear()
        Dim retKASANOrgList As New DropDownList
        retKASANOrgList = LNM0007WRKINC.getDowpDownKasanOrgList(Master.MAPID, Master.ROLE_ORG, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG, I_CREATEFLG:=True)
        If selectTORI <> "" AndAlso retKASANOrgList.Items.Count = 0 Then
            selectKASANORG = ""         '-- 加算先部門(表示)初期化
            selectindexKASANORG = 0     '-- 加算先部門(INDEX)初期化
            retOrgList = LNM0007WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG, I_CREATEFLG:=True)
        End If
        WF_KASANORG.Items.Add(New ListItem("", ""))
        '★ドロップダウンリスト選択(加算先部門)の場合
        If retKASANOrgList.Items.Count = 1 Then
            selectindexKASANORG = 1
        End If
        '★ドロップダウンリスト再作成(加算先部門)
        For index As Integer = 0 To retKASANOrgList.Items.Count - 1
            WF_KASANORG.Items.Add(New ListItem(retKASANOrgList.Items(index).Text, retKASANOrgList.Items(index).Value))
        Next
        WF_KASANORG.SelectedIndex = selectindexKASANORG
        WF_KASANORGCODE_TEXT.Text = WF_KASANORG.SelectedValue

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
        Dim WW_StyDateFlag As String = ""
        Dim WW_NewPassEndDate As String = ""
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""
        Dim WW_DBDataCheck As String = ""
        Dim NowDate As DateTime = Date.Now

        '○ 画面操作権限チェック
        ' 権限チェック(操作者に更新権限があるかチェック)
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            WW_CheckMES1 = "・固定費マスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0007INProw As DataRow In LNM0007INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0007INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0007INProw("DELFLG"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・削除コード入力エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・削除コードエラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 取引先コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORICODE", LNM0007INProw("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 取引先名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORINAME", LNM0007INProw("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 部門コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ORGCODE", LNM0007INProw("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・部門コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 部門名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ORGNAME", LNM0007INProw("ORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・部門名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 加算先部門コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KASANORGCODE", LNM0007INProw("KASANORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・加算先部門コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 加算先部門名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KASANORGNAME", LNM0007INProw("KASANORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・加算先部門名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 対象年月(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TARGETYM", LNM0007INProw("TARGETYM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・対象年月エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 車番(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SYABAN", LNM0007INProw("SYABAN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・車番エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 陸事番号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "RIKUBAN", LNM0007INProw("RIKUBAN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・陸事番号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 車型(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SYAGATA", LNM0007INProw("SYAGATA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・車型エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 車型名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SYAGATANAME", LNM0007INProw("SYAGATANAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・車型名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 車腹(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SYABARA", LNM0007INProw("SYABARA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・車腹エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 季節料金判定区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SEASONKBN", LNM0007INProw("SEASONKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・季節料金判定区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            '' 季節料金判定開始月日(バリデーションチェック)
            'Master.CheckField(Master.USERCAMP, "SEASONSTART", LNM0007INProw("SEASONSTART"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・季節料金判定開始月日エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            '' 季節料金判定終了月日(バリデーションチェック)
            'Master.CheckField(Master.USERCAMP, "SEASONEND", LNM0007INProw("SEASONEND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・季節料金判定終了月日エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            ' 固定費(月額)(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KOTEIHIM", LNM0007INProw("KOTEIHIM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・固定費(月額)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 固定費(日額)(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KOTEIHID", LNM0007INProw("KOTEIHID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・固定費(日額)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 回数(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KAISU", LNM0007INProw("KAISU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・回数エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 減額費用(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "GENGAKU", LNM0007INProw("GENGAKU"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・減額費用エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 請求額(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "AMOUNT", LNM0007INProw("AMOUNT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・請求額エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 勘定科目コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ACCOUNTCODE", LNM0007INProw("ACCOUNTCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・勘定科目コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 勘定科目名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ACCOUNTNAME", LNM0007INProw("ACCOUNTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・勘定科目名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' セグメントコード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SEGMENTCODE", LNM0007INProw("SEGMENTCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・セグメントコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' セグメント名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SEGMENTNAME", LNM0007INProw("SEGMENTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・セグメント名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 割合JOT(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "JOTPERCENTAGE", LNM0007INProw("JOTPERCENTAGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・割合JOTエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 割合ENEX(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ENEXPERCENTAGE", LNM0007INProw("ENEXPERCENTAGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・割合ENEXエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 備考1(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIKOU1", LNM0007INProw("BIKOU1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・備考1エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 備考2(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIKOU2", LNM0007INProw("BIKOU2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・備考2エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 備考3(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIKOU3", LNM0007INProw("BIKOU3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・備考3エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '季節料金判定開始月日、季節料金判定終了月日チェック
            Dim dt As DateTime
            '通年以外の場合
            If Not LNM0007INProw("SEASONKBN") = "0" Then
                '季節料金判定開始月日(バリデーションチェック)
                If Not LNM0007INProw("SEASONSTART").ToString.Length = 4 OrElse DateTime.TryParse("2099/" &
                                  LNM0007INProw("SEASONSTART").ToString.Substring(0, 2) & "/" &
                                  LNM0007INProw("SEASONSTART").ToString.Substring(2, 2), dt) = False Then
                    WW_CheckMES1 = "・季節料金判定開始月日エラーです。"
                    WW_CheckMES2 = "日付入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
                '季節料金判定終了月日(バリデーションチェック)
                If Not LNM0007INProw("SEASONEND").ToString.Length = 4 OrElse DateTime.TryParse("2099/" &
                                  LNM0007INProw("SEASONEND").ToString.Substring(0, 2) & "/" &
                                  LNM0007INProw("SEASONEND").ToString.Substring(2, 2), dt) = False Then
                    WW_CheckMES1 = "・季節料金判定終了月日エラーです。"
                    WW_CheckMES2 = "日付入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            '割合JOT、割合ENEX合計値チェック
            If Not String.IsNullOrEmpty(LNM0007INProw("JOTPERCENTAGE")) Or
                    Not String.IsNullOrEmpty(LNM0007INProw("ENEXPERCENTAGE")) Then
                Dim WW_Decimal As Decimal
                Dim WW_JOTPERCENTAGE As Double
                Dim WW_ENEXPERCENTAGE As Double
                Dim WW_TOTALPERCENTAGE As Double

                If Decimal.TryParse(LNM0007INProw("JOTPERCENTAGE").ToString, WW_Decimal) Then
                    WW_JOTPERCENTAGE = WW_Decimal
                Else
                    WW_JOTPERCENTAGE = 0
                End If
                If Decimal.TryParse(LNM0007INProw("ENEXPERCENTAGE").ToString, WW_Decimal) Then
                    WW_ENEXPERCENTAGE = WW_Decimal
                Else
                    WW_ENEXPERCENTAGE = 0
                End If

                WW_TOTALPERCENTAGE = WW_JOTPERCENTAGE + WW_ENEXPERCENTAGE

                If WW_TOTALPERCENTAGE > 100.0 Then
                    WW_CheckMES1 = "・割合JOT＆割合ENEXエラーです。"
                    WW_CheckMES2 = "割合合計エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_SYABAN.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()

                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                                    work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text, work.WF_SEL_TARGETYM.Text,
                                    work.WF_SEL_SYABAN.Text, work.WF_SEL_SEASONKBN.Text)

                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（取引先コード & 部門コード & 対象年月 & 車番 & 季節料金判定区分）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0007INProw("TORICODE") & "]" &
                                           "([" & LNM0007INProw("ORGCODE") & "]" &
                                           "([" & LNM0007INProw("TARGETYM") & "]" &
                                           "([" & LNM0007INProw("SYABAN") & "]" &
                                           " [" & LNM0007INProw("SEASONKBN") & "])"

                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If

            If String.IsNullOrEmpty(WW_LineErr) Then
                If LNM0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0007INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0007INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0007tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0007tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0007row As DataRow In LNM0007tbl.Rows
            Select Case LNM0007row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0007row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0007INProw As DataRow In LNM0007INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0007INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0007INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0007row As DataRow In LNM0007tbl.Rows
                ' KEY項目が等しい時
                If LNM0007row("TORICODE") = LNM0007INProw("TORICODE") AndAlso                                '取引先コード
                    LNM0007row("ORGCODE") = LNM0007INProw("ORGCODE") AndAlso                                '部門コード
                    LNM0007row("TARGETYM") = LNM0007INProw("TARGETYM") AndAlso                                '対象年月
                    LNM0007row("SYABARA") = LNM0007INProw("SYABARA") AndAlso                                '車腹
                    LNM0007row("SEASONKBN") = LNM0007INProw("SEASONKBN") Then                                '季節料金判定区分
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0007row("DELFLG") = LNM0007INProw("DELFLG") AndAlso
                                LNM0007row("TORINAME") = LNM0007INProw("TORINAME") AndAlso                                '取引先名称
                                LNM0007row("ORGNAME") = LNM0007INProw("ORGNAME") AndAlso                                '部門名称
                                LNM0007row("KASANORGCODE") = LNM0007INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                                LNM0007row("KASANORGNAME") = LNM0007INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                                LNM0007row("SYABAN") = LNM0007INProw("SYABAN") AndAlso                                '車番
                                LNM0007row("RIKUBAN") = LNM0007INProw("RIKUBAN") AndAlso                                '陸事番号
                                LNM0007row("SYAGATA") = LNM0007INProw("SYAGATA") AndAlso                                '車型
                                LNM0007row("SYAGATANAME") = LNM0007INProw("SYAGATANAME") AndAlso                                '車型名
                                LNM0007row("SEASONSTART") = LNM0007INProw("SEASONSTART") AndAlso                                '季節料金判定開始月日
                                LNM0007row("SEASONEND") = LNM0007INProw("SEASONEND") AndAlso                                '季節料金判定終了月日
                                LNM0007row("KOTEIHIM") = LNM0007INProw("KOTEIHIM") AndAlso                                '固定費(月額)
                                LNM0007row("KOTEIHID") = LNM0007INProw("KOTEIHID") AndAlso                                '固定費(日額)
                                LNM0007row("KAISU") = LNM0007INProw("KAISU") AndAlso                                '回数
                                LNM0007row("GENGAKU") = LNM0007INProw("GENGAKU") AndAlso                                '減額費用
                                LNM0007row("AMOUNT") = LNM0007INProw("AMOUNT") AndAlso                                '請求額
                                LNM0007row("ACCOUNTCODE") = LNM0007INProw("ACCOUNTCODE") AndAlso                                '勘定科目コード
                                LNM0007row("ACCOUNTNAME") = LNM0007INProw("ACCOUNTNAME") AndAlso                                '勘定科目名
                                LNM0007row("SEGMENTCODE") = LNM0007INProw("SEGMENTCODE") AndAlso                                'セグメントコード
                                LNM0007row("SEGMENTNAME") = LNM0007INProw("SEGMENTNAME") AndAlso                                'セグメント名
                                LNM0007row("JOTPERCENTAGE") = LNM0007INProw("JOTPERCENTAGE") AndAlso                                '割合JOT
                                LNM0007row("ENEXPERCENTAGE") = LNM0007INProw("ENEXPERCENTAGE") AndAlso                                '割合ENEX
                                LNM0007row("BIKOU1") = LNM0007INProw("BIKOU1") AndAlso                                '備考1
                                LNM0007row("BIKOU2") = LNM0007INProw("BIKOU2") AndAlso                                '備考2
                                LNM0007row("BIKOU3") = LNM0007INProw("BIKOU3") AndAlso                                '備考3
                                Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0007row("OPERATION")) Then

                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0007INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0007INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0007INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0007INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now
                Dim WW_DBDataCheck As String = ""

                '変更チェック
                MASTEREXISTS(SQLcon, WW_MODIFYKBN)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0007WRKINC.MODIFYKBN.AFTDATA
                End If

                ' マスタ更新
                UpdateMaster(SQLcon)
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
        For Each LNM0007INProw As DataRow In LNM0007INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0007row As DataRow In LNM0007tbl.Rows
                ' 同一レコードか判定
                If LNM0007row("TORICODE") = LNM0007INProw("TORICODE") AndAlso                                '取引先コード
                    LNM0007row("ORGCODE") = LNM0007INProw("ORGCODE") AndAlso                                '部門コード
                    LNM0007row("TARGETYM") = LNM0007INProw("TARGETYM") AndAlso                                '対象年月
                    LNM0007row("SYABARA") = LNM0007INProw("SYABARA") AndAlso                                '車腹
                    LNM0007row("SEASONKBN") = LNM0007INProw("SEASONKBN") Then                                '季節料金判定区分
                    ' 画面入力テーブル項目設定
                    LNM0007INProw("LINECNT") = LNM0007row("LINECNT")
                    LNM0007INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0007INProw("UPDTIMSTP") = LNM0007row("UPDTIMSTP")
                    LNM0007INProw("SELECT") = 0
                    LNM0007INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0007row.ItemArray = LNM0007INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0007tbl.NewRow
                WW_NRow.ItemArray = LNM0007INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0007tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0007tbl.Rows.Add(WW_NRow)
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
    ''' <remarks></remarks>
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrEmpty(I_VALUE) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        '名称取得
        Dim WW_NAMEht = New Hashtable '名称格納HT
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続
            Select Case I_FIELD
                Case "TORICODE"             '取引先コード
                    work.CODENAMEGetTORI(SQLcon, WW_NAMEht)
                Case "KASANORGCODE"        '加算先部門コード
                    work.CODENAMEGetKASANORG(SQLcon, WW_NAMEht)
            End Select
        End Using

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ALL, Master.USERCAMP))
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DELFLG"))
                Case "TORICODE"              '取引先コード
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
                Case "KASANORGCODE"         '加算先部門コード
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
