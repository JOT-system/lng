''************************************************************
' サーチャージ定義マスタタメンテ登録画面
' 作成日 2024/12/16
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2024/12/16 新規作成
'          : 2025/05/23 統合版に変更
''************************************************************
Imports MySql.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' サーチャージ定義マスタタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0019SurchargePatternDetail
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0019tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0019INPtbl As DataTable                              'チェック用テーブル
    Private LNM0019UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0019tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"                          '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_ButtonCLEAR", "LNM0019L", "LNM0019S"   '戻るボタン押下（LNM0019L、LNM0019Sは、パンくずより）
                            WF_CLEAR_Click()
                        Case "WF_Field_DBClick"                         'フィールドダブルクリック
                            WF_FIELD_DBClick()
                        Case "WF_LeftBoxSelectClick"                    'フィールドチェンジ
                            WF_FIELD_Change()
                        Case "WF_ButtonSel"                             '(左ボックス)選択ボタン押下
                            WF_ButtonSel_Click()
                        Case "WF_ButtonCan"                             '(左ボックス)キャンセルボタン押下
                            WF_ButtonCan_Click()
                        Case "WF_ListboxDBclick"                        '左ボックスダブルクリック
                            WF_ButtonSel_Click()
                        Case "btnClearConfirmOK"                        '戻るボタン押下後の確認ダイアログでOK押下
                            WF_CLEAR_ConfirmOkClick()
                        Case "WF_TORINAMEChange"                        '取引先名チェンジ
                            WF_TORICODE.Text = WF_TORINAME.SelectedValue
                            createListBox("ORG")
                        Case "WF_ORGNAMEChange"                         '部門コードチェンジ
                            WF_ORGCODE.Text = WF_ORGNAME.SelectedValue
                            createListBox("KASANORG")
                        Case "WF_KASANORGNAMEChange"                    '加算先部門コードチェンジ
                            WF_KASANORGCODE.Text = WF_KASANORGNAME.SelectedValue
                        Case "WF_BILLINGCYCLECNAMEhange"                '請求サイクルチェンジ
                            WF_BILLINGCYCLE.Text = WF_BILLINGCYCLENAME.SelectedValue
                        Case "WF_SURCHARGEPATTERNNAMEChange"            'サーチャージパターンコードチェンジ
                            WF_SURCHARGEPATTERNCODE.Text = WF_SURCHARGEPATTERNNAME.SelectedValue
                        Case "WF_CALCMETHODNAMEChange"                  '距離算定方式チェンジ
                            WF_CALCMETHOD.Text = WF_CALCMETHODNAME.SelectedValue
                        Case "WF_DISPLAYNAMEChange"                     '実勢価格参照先チェンジ
                            WF_DIESELPRICESITEID.Text = Left(WF_DISPLAYNAME.SelectedValue, 2)
                            WF_DIESELPRICESITEBRANCH.Text = Right(WF_KASANORGNAME.SelectedValue,2)
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
            If Not IsNothing(LNM0019tbl) Then
                LNM0019tbl.Clear()
                LNM0019tbl.Dispose()
                LNM0019tbl = Nothing
            End If

            If Not IsNothing(LNM0019INPtbl) Then
                LNM0019INPtbl.Clear()
                LNM0019INPtbl.Dispose()
                LNM0019INPtbl = Nothing
            End If

            If Not IsNothing(LNM0019UPDtbl) Then
                LNM0019UPDtbl.Clear()
                LNM0019UPDtbl.Dispose()
                LNM0019UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0019WRKINC.MAPIDD
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
    Protected Sub createListBox(Optional I_KBN As String = "INIT")

        '荷主
        If I_KBN = "INIT" OrElse I_KBN = "TORI" Then
            Me.WF_TORINAME.Items.Clear()
            Me.WF_TORINAME.Items.Add("")

            Dim retToriList As New DropDownList
            retToriList = LNM0019WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG)
            For index As Integer = 0 To retToriList.Items.Count - 1
                WF_TORINAME.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
            Next
        End If

        '部門
        If I_KBN = "INIT" OrElse I_KBN = "ORG" Then
            Me.WF_ORGNAME.Items.Clear()
            Me.WF_ORGNAME.Items.Add("")
            Dim retOrgList As New DropDownList
            retOrgList = LNM0019WRKINC.getDowpDownOrgList(Master.MAPID, WF_TORICODE.Text, Master.ROLE_ORG)

            If retOrgList.Items.Count > 0 Then
                '情シス、高圧ガス以外
                If LNM0019WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
                    Dim WW_OrgPermitHt As New Hashtable
                    Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                        SQLcon.Open()  ' DataBase接続
                        work.GetPermitOrg(SQLcon, Master.USERCAMP, Master.ROLE_ORG, WW_OrgPermitHt)
                        For index As Integer = 0 To retOrgList.Items.Count - 1
                            If WW_OrgPermitHt.ContainsKey(retOrgList.Items(index).Value) = True Then
                                WF_ORGNAME.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
                            End If
                        Next
                    End Using
                Else
                    For index As Integer = 0 To retOrgList.Items.Count - 1
                        WF_ORGNAME.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
                    Next
                End If
            End If
        End If

        '加算先部門
        If I_KBN = "INIT" OrElse I_KBN = "KASANORG" Then
            Me.WF_KASANORGNAME.Items.Clear()
            Me.WF_KASANORGNAME.Items.Add("")
            Dim retKasanOrgList As New DropDownList
            retKasanOrgList = LNM0019WRKINC.getDowpDownKasanOrgList(WF_ORGCODE.Text)
            For index As Integer = 0 To retKasanOrgList.Items.Count - 1
                WF_KASANORGNAME.Items.Add(New ListItem(retKasanOrgList.Items(index).Text, retKasanOrgList.Items(index).Value))
            Next
        End If

        '請求サイクル
        If I_KBN = "INIT" OrElse I_KBN = "BILLINGCYCLE" Then
            Me.WF_BILLINGCYCLENAME.Items.Clear()
            Me.WF_BILLINGCYCLENAME.Items.Add("")
            Dim retBillingCycleList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "BILLINGCYCLE")
            For index As Integer = 0 To retBillingCycleList.Items.Count - 1
                WF_BILLINGCYCLENAME.Items.Add(New ListItem(retBillingCycleList.Items(index).Text, retBillingCycleList.Items(index).Value))
            Next
        End If

        'サーチャージパターンコード
        If I_KBN = "INIT" OrElse I_KBN = "SURCHARGEPATTERN" Then
            Me.WF_SURCHARGEPATTERNNAME.Items.Clear()
            Me.WF_SURCHARGEPATTERNNAME.Items.Add("")
            Dim retCurchargePatternList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "SURCHARGEPATTERN")
            For index As Integer = 0 To retCurchargePatternList.Items.Count - 1
                WF_SURCHARGEPATTERNNAME.Items.Add(New ListItem(retCurchargePatternList.Items(index).Text, retCurchargePatternList.Items(index).Value))
            Next
        End If

        '距離算定方式
        If I_KBN = "INIT" OrElse I_KBN = "CALCMETHOD" Then
            Me.WF_CALCMETHODNAME.Items.Clear()
            Me.WF_CALCMETHODNAME.Items.Add("")
            Dim retCalcMethodList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "CALCMETHOD")
            For index As Integer = 0 To retCalcMethodList.Items.Count - 1
                WF_CALCMETHODNAME.Items.Add(New ListItem(retCalcMethodList.Items(index).Text, retCalcMethodList.Items(index).Value))
            Next
        End If

        '実勢軽油価格参照先名
        If I_KBN = "INIT" OrElse I_KBN = "DISPLAYNAME" Then
            Me.WF_DISPLAYNAME.Items.Clear()
            Me.WF_DISPLAYNAME.Items.Add("")
            Dim retDieselPriceSiteList As New DropDownList
            retDieselPriceSiteList = LNM0019WRKINC.getDowpDownDieselPriceSiteList()
            For index As Integer = 0 To retDieselPriceSiteList.Items.Count - 1
                WF_DISPLAYNAME.Items.Add(New ListItem(retDieselPriceSiteList.Items(index).Text, retDieselPriceSiteList.Items(index).Value))
            Next
        End If

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0019L Then
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
        WF_TORINAME.SelectedValue = work.WF_SEL_TORICODE.Text
        WF_TORICODE.Text = work.WF_SEL_TORICODE.Text
        WF_TORICODE_SAVE.Value = work.WF_SEL_TORICODE.Text
        WF_TORINAME_SAVE.Value = work.WF_SEL_TORINAME.Text

        '部門コード、名称
        WF_ORGNAME.SelectedValue = work.WF_SEL_ORGCODE.Text
        WF_ORGCODE.Text = work.WF_SEL_ORGCODE.Text
        WF_ORG_SAVE.Value = work.WF_SEL_ORGCODE.Text
        WF_ORGNAME_SAVE.Value = work.WF_SEL_ORGNAME.Text

        '加算先部門コード、名称
        WF_KASANORGNAME.SelectedValue = work.WF_SEL_KASANORGCODE.Text
        WF_KASANORGCODE.Text = work.WF_SEL_KASANORGCODE.Text
        WF_KASANORG_SAVE.Value = work.WF_SEL_KASANORGCODE.Text
        WF_KASANORGNAME_SAVE.Value = work.WF_SEL_KASANORGNAME.Text

        '請求サイクル
        WF_BILLINGCYCLENAME.SelectedValue = work.WF_SEL_BILLINGCYCLE.Text
        WF_BILLINGCYCLE.Text = work.WF_SEL_BILLINGCYCLE.Text
        WF_BILLINGCYCLE_SAVE.Value = work.WF_SEL_BILLINGCYCLE.Text
        WF_BILLINGCYCLENAME_SAVE.Value = work.WF_SEL_BILLINGCYCLENAME.Text

        'サーチャージパターンコード
        WF_SURCHARGEPATTERNNAME.SelectedValue = work.WF_SEL_SURCHARGEPATTERNCODE.Text
        WF_SURCHARGEPATTERNCODE.Text = work.WF_SEL_SURCHARGEPATTERNCODE.Text
        WF_SURCHARGEPATTERNCODE_SAVE.Value = work.WF_SEL_SURCHARGEPATTERNCODE.Text
        WF_SURCHARGEPATTERNNAME_SAVE.Value = work.WF_SEL_SURCHARGEPATTERNNAME.Text

        '距離算定方式
        WF_CALCMETHODNAME.SelectedValue = work.WF_SEL_CALCMETHOD.Text
        WF_CALCMETHOD.Text = work.WF_SEL_CALCMETHOD.Text

        '実勢軽油価格参照先名
        WF_DISPLAYNAME.SelectedValue = work.WF_SEL_DIESELPRICESITEID.Text & work.WF_SEL_DIESELPRICESITEBRANCH.Text
        WF_DIESELPRICESITEID.Text = work.WF_SEL_DIESELPRICESITEID.Text
        WF_DIESELPRICESITENAME.Text = work.WF_SEL_DIESELPRICESITENAME.Text
        WF_DIESELPRICESITEBRANCH.Text = work.WF_SEL_DIESELPRICESITEBRANCH.Text
        WF_DIESELPRICESITEKBNNAME.Text = work.WF_SEL_DIESELPRICESITEKBNNAME.Text

        '有効開始日
        WF_StYMD.Value = work.WF_SEL_STYMD.Text
        'WF_STYMD_SAVE.Value = work.WF_SEL_STYMD.Text

        '有効終了日
        WF_EndYMD.Value = work.WF_SEL_ENDYMD.Text

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_TORICODE.Text

        '表示制御項目
        '情シス、高圧ガス以外の場合
        If LNM0019WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            VisibleKeyOrgCode.Value = ""
        Else
            VisibleKeyOrgCode.Value = Master.ROLE_ORG
        End If

        ' 取引先コード・実績出荷場所コード・変換後出荷場所コード・実績届先コード・変換後届先コード
        ' 枝番・単価を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.WF_TORICODE.Attributes("onkeyPress") = "CheckNum()"

        ' 有効開始日・有効終了日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.WF_StYMD.Attributes("onkeyPress") = "CheckCalendar()"
        Me.WF_EndYMD.Attributes("onkeyPress") = "CheckCalendar()"

        ' 入力するテキストボックスは数値(0～9)＋記号(.)のみ可能とする。
        'Me.TxtROUNDTRIP.Attributes("onkeyPress") = "CheckDeci()"             '往復距離
        'Me.TxtTOLLFEE.Attributes("onkeyPress") = "CheckDeci()"             '通行料
        'Me.TxtSYABARA.Attributes("onkeyPress") = "CheckDeci()"             '車腹
        'Me.TxtJOTPERCENTAGE.Attributes("onkeyPress") = "CheckDeci()"       '割合JOT
        'Me.TxtENEXPERCENTAGE.Attributes("onkeyPress") = "CheckDeci()"      '割合ENEX

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU
    End Sub

    ''' <summary>
    ''' サーチャージ定義マスタタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(サーチャージ定義マスタタ)
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("  INSERT INTO LNG.LNM0019_SURCHARGEPATTERN")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNCODE  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNNAME  ")
        SQLStr.AppendLine("     ,BILLINGCYCLE  ")
        SQLStr.AppendLine("     ,CALCMETHOD  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH  ")
        SQLStr.AppendLine("     ,DELFLG  ")
        SQLStr.AppendLine("     ,INITYMD  ")
        SQLStr.AppendLine("     ,INITUSER  ")
        SQLStr.AppendLine("     ,INITTERMID  ")
        SQLStr.AppendLine("     ,INITPGID  ")
        SQLStr.AppendLine("     ,RECEIVEYMD  ")
        SQLStr.AppendLine("   )  ")
        SQLStr.AppendLine("   VALUES  ")
        SQLStr.AppendLine("   (  ")
        SQLStr.AppendLine("      @TORICODE  ")
        SQLStr.AppendLine("     ,@TORINAME  ")
        SQLStr.AppendLine("     ,@ORGCODE  ")
        SQLStr.AppendLine("     ,@ORGNAME  ")
        SQLStr.AppendLine("     ,@KASANORGCODE  ")
        SQLStr.AppendLine("     ,@KASANORGNAME  ")
        SQLStr.AppendLine("     ,@SURCHARGEPATTERNCODE  ")
        SQLStr.AppendLine("     ,@SURCHARGEPATTERNNAME  ")
        SQLStr.AppendLine("     ,@BILLINGCYCLE  ")
        SQLStr.AppendLine("     ,@CALCMETHOD  ")
        SQLStr.AppendLine("     ,@STYMD  ")
        SQLStr.AppendLine("     ,@ENDYMD  ")
        SQLStr.AppendLine("     ,@DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,@DIESELPRICESITEBRANCH  ")
        SQLStr.AppendLine("     ,@DELFLG  ")
        SQLStr.AppendLine("     ,@INITYMD  ")
        SQLStr.AppendLine("     ,@INITUSER  ")
        SQLStr.AppendLine("     ,@INITTERMID  ")
        SQLStr.AppendLine("     ,@INITPGID  ")
        SQLStr.AppendLine("     ,@RECEIVEYMD  ")
        SQLStr.AppendLine("   )   ")
        SQLStr.AppendLine("  ON DUPLICATE KEY UPDATE  ")
        SQLStr.AppendLine("      TORICODE =  @TORICODE")
        SQLStr.AppendLine("     ,TORINAME =  @TORINAME")
        SQLStr.AppendLine("     ,ORGCODE =  @ORGCODE")
        SQLStr.AppendLine("     ,ORGNAME =  @ORGNAME")
        SQLStr.AppendLine("     ,KASANORGCODE =  @KASANORGCODE")
        SQLStr.AppendLine("     ,KASANORGNAME =  @KASANORGNAME")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNCODE =  @SURCHARGEPATTERNCODE")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNNAME =  @SURCHARGEPATTERNNAME")
        SQLStr.AppendLine("     ,BILLINGCYCLE =  @BILLINGCYCLE")
        SQLStr.AppendLine("     ,CALCMETHOD =  @CALCMETHOD")
        SQLStr.AppendLine("     ,STYMD =  @STYMD")
        SQLStr.AppendLine("     ,ENDYMD =  @ENDYMD")
        SQLStr.AppendLine("     ,DIESELPRICESITEID =  @DIESELPRICESITEID")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH =  @DIESELPRICESITEBRANCH")
        SQLStr.AppendLine("     ,DELFLG =  @DELFLG")
        SQLStr.AppendLine("     ,UPDYMD =  @UPDYMD")
        SQLStr.AppendLine("     ,UPDUSER =  @UPDUSER")
        SQLStr.AppendLine("     ,UPDTERMID =  @UPDTERMID")
        SQLStr.AppendLine("     ,UPDPGID =  @UPDPGID")
        SQLStr.AppendLine("    ;  ")

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl = New StringBuilder
        SQLJnl.AppendLine(" SELECT         ")
        SQLJnl.AppendLine("      TORICODE  ")
        SQLJnl.AppendLine("     ,TORINAME  ")
        SQLJnl.AppendLine("     ,ORGCODE   ")
        SQLJnl.AppendLine("     ,ORGNAME   ")
        SQLJnl.AppendLine("     ,KASANORGCODE  ")
        SQLJnl.AppendLine("     ,KASANORGNAME  ")
        SQLJnl.AppendLine("     ,SURCHARGEPATTERNCODE  ")
        SQLJnl.AppendLine("     ,SURCHARGEPATTERNNAME  ")
        SQLJnl.AppendLine("     ,BILLINGCYCLE  ")
        SQLJnl.AppendLine("     ,CALCMETHOD  ")
        SQLJnl.AppendLine("     ,STYMD   ")
        SQLJnl.AppendLine("     ,ENDYMD  ")
        SQLJnl.AppendLine("     ,DIESELPRICESITEID  ")
        SQLJnl.AppendLine("     ,DIESELPRICESITEBRANCH  ")
        SQLJnl.AppendLine("     ,DELFLG   ")
        SQLJnl.AppendLine("     ,INITYMD  ")
        SQLJnl.AppendLine("     ,INITUSER  ")
        SQLJnl.AppendLine("     ,INITTERMID  ")
        SQLJnl.AppendLine("     ,INITPGID  ")
        SQLJnl.AppendLine("     ,UPDYMD  ")
        SQLJnl.AppendLine("     ,UPDUSER  ")
        SQLJnl.AppendLine("     ,UPDTERMID  ")
        SQLJnl.AppendLine("     ,UPDPGID  ")
        SQLJnl.AppendLine("     ,RECEIVEYMD  ")
        SQLJnl.AppendLine("     ,UPDTIMSTP   ")
        SQLJnl.AppendLine(" FROM                                       ")
        SQLJnl.AppendLine("     LNG.LNM0019_SURCHARGEPATTERN                      ")
        SQLJnl.AppendLine(" WHERE                                      ")
        SQLJnl.AppendLine("         COALESCE(TORICODE, '')              = @TORICODE ")
        SQLJnl.AppendLine("    AND  COALESCE(ORGCODE, '')               = @ORGCODE ")
        SQLJnl.AppendLine("    AND  COALESCE(SURCHARGEPATTERNCODE, '')  = @SURCHARGEPATTERNCODE ")
        SQLJnl.AppendLine("    AND  COALESCE(BILLINGCYCLE, '')          = @BILLINGCYCLE ")
        SQLJnl.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl.ToString, SQLcon)
                ' DB更新用パラメータ
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 20)     '取引先名
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 20)     '部門コード
                Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 20)     '加算先部門コード
                Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_SURCHARGEPATTERNNAME As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNNAME", MySqlDbType.VarChar, 20)     'サーチャージパターン名
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_CALCMETHOD As MySqlParameter = SQLcmd.Parameters.Add("@CALCMETHOD", MySqlDbType.VarChar, 1)     '距離算定方式
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)     '実勢軽油価格参照先ID枝番
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

                ' 更新ジャーナル出力用パラメータ
                Dim JP_TORICODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim JP_ORGCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 20)     '部門コード
                Dim JP_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim JP_BILLINGCYCLE As MySqlParameter = SQLcmdJnl.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim JP_STYMD As MySqlParameter = SQLcmdJnl.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                Dim LNM0019row As DataRow = LNM0019INPtbl.Rows(0)

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                P_TORICODE.Value = LNM0019row("TORICODE")           '取引先コード
                P_TORINAME.Value = LNM0019row("TORINAME")           '取引先名
                P_ORGCODE.Value = LNM0019row("ORGCODE")           '部門コード
                P_ORGNAME.Value = LNM0019row("ORGNAME")           '部門名
                P_KASANORGCODE.Value = LNM0019row("KASANORGCODE")           '加算先部門コード
                P_KASANORGNAME.Value = LNM0019row("KASANORGNAME")           '加算先部門名
                P_SURCHARGEPATTERNCODE.Value = LNM0019row("SURCHARGEPATTERNCODE")           'サーチャージパターンコード
                P_SURCHARGEPATTERNNAME.Value = LNM0019row("SURCHARGEPATTERNNAME")           'サーチャージパターン名
                P_BILLINGCYCLE.Value = LNM0019row("BILLINGCYCLE")           '請求サイクル
                P_CALCMETHOD.Value = LNM0019row("CALCMETHOD")           '距離算定方式
                P_STYMD.Value = LNM0019row("STYMD")           '有効開始日
                P_DIESELPRICESITEID.Value = LNM0019row("DIESELPRICESITEID")           '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = LNM0019row("DIESELPRICESITEBRANCH")           '実勢軽油価格参照先ID枝番
                P_DELFLG.Value = LNM0019row("DELFLG")           '削除フラグ

                '有効終了日(画面入力済みの場合画面入力を優先)
                If Not WF_EndYMD.Value = "" Then
                    P_ENDYMD.Value = LNM0019row("ENDYMD")
                Else
                    P_ENDYMD.Value = WF_AUTOENDYMD.Value
                End If

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
                JP_TORICODE.Value = LNM0019row("TORICODE") '取引先コード
                JP_ORGCODE.Value = LNM0019row("ORGCODE") '部門コード
                JP_SURCHARGEPATTERNCODE.Value = LNM0019row("SURCHARGEPATTERNCODE")           'サーチャージパターンコード
                JP_BILLINGCYCLE.Value = LNM0019row("BILLINGCYCLE")           '請求サイクル
                JP_STYMD.Value = LNM0019row("STYMD")           '有効開始日

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0019UPDtbl) Then
                        LNM0019UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0019UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0019UPDtbl.Clear()
                    LNM0019UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0019UPDrow As DataRow In LNM0019UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0019_SURCHARGEPATTERN"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0019UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0019_SURCHARGEPATTERN UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN UPDATE_INSERT"
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

        'サーチャージ定義マスタタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("       ,DELFLG")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0019_SURCHARGEPATTERN")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SURCHARGEPATTERNCODE, '')             = @SURCHARGEPATTERNCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BILLINGCYCLE, '')             = @BILLINGCYCLE ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                Dim LNM0019row As DataRow = LNM0019INPtbl.Rows(0)

                P_TORICODE.Value = LNM0019row("TORICODE") '取引先コード
                P_ORGCODE.Value = LNM0019row("ORGCODE") '部門コード
                P_SURCHARGEPATTERNCODE.Value = LNM0019row("SURCHARGEPATTERNCODE") 'サーチャージパターンコード
                P_BILLINGCYCLE.Value = LNM0019row("BILLINGCYCLE") '請求サイクル
                P_STYMD.Value = LNM0019row("STYMD") '有効開始日

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
                        WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0019_SURCHARGEPATTERN SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0032_SURCHARGEPATTERNHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNCODE  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNNAME  ")
        SQLStr.AppendLine("     ,BILLINGCYCLE  ")
        SQLStr.AppendLine("     ,CALCMETHOD  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH  ")
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
        SQLStr.AppendLine("     ,SURCHARGEPATTERNCODE  ")
        SQLStr.AppendLine("     ,SURCHARGEPATTERNNAME  ")
        SQLStr.AppendLine("     ,BILLINGCYCLE  ")
        SQLStr.AppendLine("     ,CALCMETHOD  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEID  ")
        SQLStr.AppendLine("     ,DIESELPRICESITEBRANCH  ")
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
        SQLStr.AppendLine("        LNG.LNM0019_SURCHARGEPATTERN")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SURCHARGEPATTERNCODE, '')             = @SURCHARGEPATTERNCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BILLINGCYCLE, '')             = @BILLINGCYCLE ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 20)     '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0019row As DataRow = LNM0019INPtbl.Rows(0)

                ' DB更新
                P_TORICODE.Value = LNM0019row("TORICODE")           '取引先コード
                P_ORGCODE.Value = LNM0019row("ORGCODE")           '部門コード
                P_SURCHARGEPATTERNCODE.Value = LNM0019row("SURCHARGEPATTERNCODE")           'サーチャージパターンコード
                P_BILLINGCYCLE.Value = LNM0019row("BILLINGCYCLE")           '請求サイクル
                P_STYMD.Value = LNM0019row("STYMD")           '有効開始日

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0019WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0019tbl.Rows(0)("DELFLG") = "0" And LNM0019row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0019WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0019WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0032_SURCHARGEPATTERNHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0032_SURCHARGEPATTERNHIST  INSERT"
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

    ''' <summary>
    ''' 有効終了日更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="WW_ROW"></param>
    Public Sub UpdateENDYMD(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow,
                            ByRef O_MESSAGENO As String, ByVal WW_NOW As String)


        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ更新
        Dim SQLStr As New StringBuilder
        SQLStr.Append(" UPDATE                                      ")
        SQLStr.Append("     LNG.LNM0019_SURCHARGEPATTERN                    ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     ENDYMD               = @ENDYMD          ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(SURCHARGEPATTERNCODE, '')             = @SURCHARGEPATTERNCODE ")
        SQLStr.Append("    AND  COALESCE(BILLINGCYCLE, '')             = @BILLINGCYCLE ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 6) 'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 6)     '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日

                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_SURCHARGEPATTERNCODE.Value = WW_ROW("SURCHARGEPATTERNCODE") 'サーチャージパターンコード
                P_BILLINGCYCLE.Value = WW_ROW("BILLINGCYCLE") '請求サイクル
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日

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
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN UPDATE"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try
    End Sub

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
        DetailBoxToLNM0019INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0019tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0019tbl, work.WF_SEL_INPTBL.Text)

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
    Protected Sub DetailBoxToLNM0019INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(RadioDELFLG.SelectedValue)      '削除フラグ


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

        Master.CreateEmptyTable(LNM0019INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0019INProw As DataRow = LNM0019INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) Then
            LNM0019INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(TxtSelLineCNT.Text, LNM0019INProw("LINECNT"))
            Catch ex As Exception
                LNM0019INProw("LINECNT") = 0
            End Try
        End If

        LNM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0019INProw("UPDTIMSTP") = 0
        LNM0019INProw("SELECT") = 1
        LNM0019INProw("HIDDEN") = 0

        LNM0019INProw("DELFLG") = RadioDELFLG.SelectedValue             '削除フラグ

        '更新の場合
        If Not DisabledKeyItem.Value = "" Then
            LNM0019INProw("TORICODE") = work.WF_SEL_TORICODE.Text                                   '取引先コード
            LNM0019INProw("TORINAME") = work.WF_SEL_TORINAME.Text                                   '取引先名称
            LNM0019INProw("ORGCODE") = work.WF_SEL_ORGCODE.Text                                     '部門コード
            LNM0019INProw("ORGNAME") = work.WF_SEL_ORGNAME.Text                                     '部門名称
            LNM0019INProw("KASANORGCODE") = work.WF_SEL_KASANORGCODE.Text                           '加算先部門コード
            LNM0019INProw("KASANORGNAME") = work.WF_SEL_KASANORGNAME.Text                           '加算先部門名称
            LNM0019INProw("SURCHARGEPATTERNCODE") = work.WF_SEL_SURCHARGEPATTERNCODE.Text           'サーチャージパターンコード
            LNM0019INProw("SURCHARGEPATTERNNAME") = work.WF_SEL_SURCHARGEPATTERNNAME.Text           'サーチャージパターン名
            LNM0019INProw("BILLINGCYCLE") = work.WF_SEL_BILLINGCYCLE.Text                           '請求サイクル
            LNM0019INProw("BILLINGCYCLENAME") = work.WF_SEL_BILLINGCYCLENAME.Text                   '請求サイクル名
            LNM0019INProw("CALCMETHOD") = WF_CALCMETHODNAME.SelectedValue                           '距離算定方式
            LNM0019INProw("CALCMETHODNAME") = WF_CALCMETHODNAME.SelectedItem                        '距離算定方式名
            LNM0019INProw("DIESELPRICESITEID") = Left(WF_DISPLAYNAME.SelectedValue, 2)              '実勢軽油価格参照先ID
            LNM0019INProw("DIESELPRICESITENAME") = WF_DISPLAYNAME.SelectedItem                      '実勢軽油価格参照先名
            LNM0019INProw("DIESELPRICESITEBRANCH") = Right(WF_DISPLAYNAME.SelectedValue, 2)         '実勢軽油価格参照先ID枝番
            LNM0019INProw("DIESELPRICESITEKBNNAME") = WF_DISPLAYNAME.SelectedItem                   '実勢軽油価格参照先区分名
            LNM0019INProw("DISPLAYNAME") = WF_DISPLAYNAME.SelectedItem                              '実勢軽油価格参照先画面表示名称
        Else
            LNM0019INProw("TORICODE") = WF_TORINAME.SelectedValue                                   '取引先コード
            LNM0019INProw("TORINAME") = WF_TORINAME.SelectedItem                                    '取引先名称
            LNM0019INProw("ORGCODE") = WF_ORGNAME.SelectedValue                                     '部門コード
            LNM0019INProw("ORGNAME") = WF_ORGNAME.SelectedItem                                      '部門名称
            LNM0019INProw("KASANORGCODE") = WF_KASANORGNAME.SelectedValue                           '加算先部門コード
            LNM0019INProw("KASANORGNAME") = WF_KASANORGNAME.SelectedItem                            '加算先部門名称
            LNM0019INProw("SURCHARGEPATTERNCODE") = WF_SURCHARGEPATTERNNAME.SelectedValue           'サーチャージパターンコード
            LNM0019INProw("SURCHARGEPATTERNNAME") = WF_SURCHARGEPATTERNNAME.SelectedItem            'サーチャージパターン名
            LNM0019INProw("BILLINGCYCLE") = WF_BILLINGCYCLENAME.SelectedValue                       '請求サイクル
            LNM0019INProw("BILLINGCYCLENAME") = WF_BILLINGCYCLENAME.SelectedItem                    '請求サイクル名
            LNM0019INProw("CALCMETHOD") = WF_CALCMETHODNAME.SelectedValue                           '距離算定方式
            LNM0019INProw("CALCMETHODNAME") = WF_CALCMETHODNAME.SelectedItem                        '距離算定方式名
            LNM0019INProw("DIESELPRICESITEID") = Left(WF_DISPLAYNAME.SelectedValue, 2)              '実勢軽油価格参照先ID
            LNM0019INProw("DIESELPRICESITENAME") = WF_DISPLAYNAME.SelectedItem                      '実勢軽油価格参照先名
            LNM0019INProw("DIESELPRICESITEBRANCH") = Right(WF_DISPLAYNAME.SelectedValue, 2)         '実勢軽油価格参照先ID枝番
            LNM0019INProw("DIESELPRICESITEKBNNAME") = WF_DISPLAYNAME.SelectedItem                   '実勢軽油価格参照先区分名
            LNM0019INProw("DISPLAYNAME") = WF_DISPLAYNAME.SelectedItem                              '実勢軽油価格参照先画面表示名称
        End If

        LNM0019INProw("STYMD") = WF_StYMD.Value            '有効開始日
        LNM0019INProw("ENDYMD") = WF_EndYMD.Value            '有効終了日

        '○ チェック用テーブルに登録する
        LNM0019INPtbl.Rows.Add(LNM0019INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0019INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0019INProw As DataRow = LNM0019INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0019row As DataRow In LNM0019tbl.Rows
            ' KEY項目が等しい時
            If LNM0019row("TORICODE") = LNM0019INProw("TORICODE") AndAlso
                LNM0019row("ORGCODE") = LNM0019INProw("ORGCODE") AndAlso                                    '部門コード
                LNM0019row("SURCHARGEPATTERNCODE") = LNM0019INProw("SURCHARGEPATTERNCODE") AndAlso          'サーチャージパターンコード
                LNM0019row("BILLINGCYCLE") = LNM0019INProw("BILLINGCYCLE") AndAlso                          '請求サイクル
                LNM0019row("STYMD") = LNM0019INProw("STYMD") Then                                           '有効開始日
                ' KEY項目以外の項目の差異をチェック
                If LNM0019row("DELFLG") = LNM0019INProw("DELFLG") AndAlso
                    LNM0019row("TORINAME") = LNM0019INProw("TORINAME") AndAlso                              '取引先名称
                    LNM0019row("ORGNAME") = LNM0019INProw("ORGNAME") AndAlso                                '部門名称
                    LNM0019row("KASANORGCODE") = LNM0019INProw("KASANORGCODE") AndAlso                      '加算先部門コード
                    LNM0019row("KASANORGNAME") = LNM0019INProw("KASANORGNAME") AndAlso                      '加算先部門名称
                    LNM0019row("SURCHARGEPATTERNNAME") = LNM0019INProw("SURCHARGEPATTERNNAME") AndAlso      'サーチャージパターン名
                    LNM0019row("CALCMETHOD") = LNM0019INProw("CALCMETHOD") AndAlso                          '距離算定方式
                    LNM0019row("ENDYMD") = LNM0019INProw("ENDYMD") AndAlso                                  '有効終了日
                    LNM0019row("DIESELPRICESITEID") = LNM0019INProw("DIESELPRICESITEID") AndAlso            '実勢軽油価格参照先ID
                    LNM0019row("DIESELPRICESITEBRANCH") = LNM0019INProw("DIESELPRICESITEBRANCH") Then       '実勢軽油価格参照先ID枝番
                    ' 変更がない時は、入力変更フラグをOFFにする
                    WW_InputChangeFlg = False
                End If

                Exit For

            End If
        Next

        'パンくずから検索を選択した場合
        If WF_ButtonClick.Value = "LNM0019S" Then
            WF_BeforeMAPID.Value = LNM0019WRKINC.MAPIDL
        Else
            WF_BeforeMAPID.Value = LNM0019WRKINC.MAPIDD
        End If

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

        Master.MAPID = WF_BeforeMAPID.Value
        Master.TransitionPrevPage()

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each LNM0019row As DataRow In LNM0019tbl.Rows
            Select Case LNM0019row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0019row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0019row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0019row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0019tbl, work.WF_SEL_INPTBL.Text)

        TxtSelLineCNT.Text = ""              'LINECNT
        TxtMapId.Text = "M00001"             '画面ＩＤ
        RadioDELFLG.SelectedValue = ""                  '削除フラグ

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
            Case "WF_AVOCADOTODOKENAME"
                'CODENAME_get("AVOCADOTODOKECODE", WF_AVOCADOTODOKECODE.Text, WF_AVOCADOTODOKENAME.Text, WW_RtnSW)  '実績届先コード
                'WF_AVOCADOTODOKENAME.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' サーチャージ定義マスタタ更新(削除フラグのみ)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UpdateMasterDelflgOnly()
        Dim WW_MODIFYKBN As String = ""
        Dim WW_DATE As Date = Date.Now

        '初期化
        LNM0019INPtbl = New DataTable
        LNM0019INPtbl.Columns.Add("TORICODE")
        LNM0019INPtbl.Columns.Add("ORGCODE")
        LNM0019INPtbl.Columns.Add("KASANORGCODE")
        LNM0019INPtbl.Columns.Add("SURCHARGEPATTERNCODE")
        LNM0019INPtbl.Columns.Add("BILLINGCYCLE")
        LNM0019INPtbl.Columns.Add("STYMD")
        LNM0019INPtbl.Columns.Add("DELFLG")

        Dim row As DataRow
        row = LNM0019INPtbl.NewRow
        row("TORICODE") = work.WF_SEL_TORICODE.Text
        row("ORGCODE") = work.WF_SEL_ORGCODE.Text
        row("KASANORGCODE") = work.WF_SEL_KASANORGCODE.Text
        row("SURCHARGEPATTERNCODE") = work.WF_SEL_SURCHARGEPATTERNCODE.Text
        row("BILLINGCYCLE") = work.WF_SEL_BILLINGCYCLE.Text
        row("STYMD") = work.WF_SEL_STYMD.Text
        row("DELFLG") = C_DELETE_FLG.DELETE
        LNM0019INPtbl.Rows.Add(row)

        ' DB更新処理
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()

            '履歴テーブルに変更前データを登録
            InsertHist(SQLcon, LNM0019WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '削除フラグ更新
            SetDelflg(SQLcon, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '履歴テーブルに変更後データを登録
            InsertHist(SQLcon, LNM0019WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

        End Using

        '○ 入力値反映
        For Each LNM0019INProw As DataRow In LNM0019INPtbl.Rows
            For Each LNM0019row As DataRow In LNM0019tbl.Rows
                If LNM0019INProw("TORICODE") = LNM0019row("TORICODE") AndAlso
                    LNM0019INProw("ORGCODE") = LNM0019row("ORGCODE") AndAlso                                            '部門コード
                    LNM0019INProw("SURCHARGEPATTERNCODE") = LNM0019row("SURCHARGEPATTERNCODE") AndAlso                  'サーチャージパターンコード
                    LNM0019INProw("BILLINGCYCLE") = LNM0019row("BILLINGCYCLE") AndAlso                                  '請求サイクル
                    LNM0019INProw("STYMD") = LNM0019row("STYMD") Then                                                   '有効開始日
                    ' 画面入力テーブル項目設定              
                    LNM0019row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0019row("DELFLG") = LNM0019INProw("DELFLG")
                    LNM0019row("SELECT") = 0
                    LNM0019row("HIDDEN") = 0
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
        SQLStr.Append("     LNG.LNM0019_SURCHARGEPATTERN            ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(SURCHARGEPATTERNCODE, '')             = @SURCHARGEPATTERNCODE ")
        SQLStr.Append("    AND  COALESCE(BILLINGCYCLE, '')             = @BILLINGCYCLE ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 20)     '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 20)     '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)     'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)     '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                Dim LNM0019row As DataRow = LNM0019INPtbl.Rows(0)
                P_TORICODE.Value = LNM0019row("TORICODE")           '取引先コード
                P_ORGCODE.Value = LNM0019row("ORGCODE")           '部門コード
                P_SURCHARGEPATTERNCODE.Value = LNM0019row("SURCHARGEPATTERNCODE")           'サーチャージパターンコード
                P_BILLINGCYCLE.Value = LNM0019row("BILLINGCYCLE")           '請求サイクル
                P_STYMD.Value = LNM0019row("STYMD")           '有効開始日
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN UPDATE"
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
            WW_CheckMES1 = "・サーチャージ定義マスタタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0019INProw As DataRow In LNM0019INPtbl.Rows

            WW_LineErr = ""
            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0019INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0019INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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

            '取引先コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORICODE", LNM0019INProw("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '取引先名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORINAME", LNM0019INProw("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 部門コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ORGCODE", LNM0019INProw("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・部門コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 部門名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ORGNAME", LNM0019INProw("ORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・部門名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 加算先部門コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KASANORGCODE", LNM0019INProw("KASANORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・加算先部門コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 加算先部門名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KASANORGNAME", LNM0019INProw("KASANORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・加算先部門名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' サーチャージパターンコード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SURCHARGEPATTERNCODE", LNM0019INProw("SURCHARGEPATTERNCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・サーチャージパターンコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' サーチャージパターン名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SURCHARGEPATTERNNAME", LNM0019INProw("SURCHARGEPATTERNNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・サーチャージパターン名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 請求サイクル(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BILLINGCYCLE", LNM0019INProw("BILLINGCYCLE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・請求サイクルエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 距離算定方式(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CALCMETHOD", LNM0019INProw("CALCMETHOD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・距離算定方式エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 有効開始日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "STYMD", LNM0019INProw("STYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                LNM0019INProw("STYMD") = CDate(LNM0019INProw("STYMD")).ToString("yyyy/MM/dd")
            Else
                WW_CheckMES1 = "・有効開始日エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面で入力済みの場合のみ
            If Not WF_EndYMD.Value = "" Then
                ' 有効終了日(バリデーションチェック)
                Master.CheckField(Master.USERCAMP, "ENDYMD", LNM0019INProw("ENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    LNM0019INProw("ENDYMD") = CDate(LNM0019INProw("ENDYMD")).ToString("yyyy/MM/dd")
                Else
                    WW_CheckMES1 = "・有効終了日エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If

            ' 実勢軽油価格参照先ID(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DIESELPRICESITEID", LNM0019INProw("DIESELPRICESITEID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・実勢軽油価格参照先IDエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 実勢軽油価格参照先ID枝番(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DIESELPRICESITEBRANCH", LNM0019INProw("DIESELPRICESITEBRANCH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・実勢軽油価格参照先ID枝番エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面で入力済みの場合のみ
            If Not WF_EndYMD.Value = "" Then
                ' 日付大小チェック
                If Not String.IsNullOrEmpty(LNM0019INProw("STYMD")) AndAlso
                        Not String.IsNullOrEmpty(LNM0019INProw("ENDYMD")) Then
                    If CDate(LNM0019INProw("STYMD")) > CDate(LNM0019INProw("ENDYMD")) Then
                        WW_CheckMES1 = "・有効開始日＆有効終了日エラーです。"
                        WW_CheckMES2 = "日付大小入力エラー"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_TORICODE.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                                              work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text, work.WF_SEL_SURCHARGEPATTERNCODE.Text,
                                              work.WF_SEL_BILLINGCYCLE.Text, work.WF_SEL_STYMD.Text)

                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（取引先コード & 部門コード & サーチャージパターンコード & 請求サイクル & 有効開始日）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0019INProw("TORICODE") & "]" &
                                           "[" & LNM0019INProw("ORGCODE") & "]" &
                                           "[" & LNM0019INProw("SURCHARGEPATTERNCODE") & "]" &
                                           "[" & LNM0019INProw("BILLINGCYCLE") & "]" &
                                           "[" & LNM0019INProw("STYMD") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If

            If String.IsNullOrEmpty(WW_LineErr) Then
                If LNM0019INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0019INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0019INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0019tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0019tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0019row As DataRow In LNM0019tbl.Rows
            Select Case LNM0019row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0019row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0019row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0019row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0019INProw As DataRow In LNM0019INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0019INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0019INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0019row As DataRow In LNM0019tbl.Rows
                ' KEY項目が等しい時
                If LNM0019row("TORICODE") = LNM0019INProw("TORICODE") AndAlso                                   '取引先コード
                    LNM0019row("ORGCODE") = LNM0019INProw("ORGCODE") AndAlso                                    '部門コード
                    LNM0019row("SURCHARGEPATTERNCODE") = LNM0019INProw("SURCHARGEPATTERNCODE") AndAlso          'サーチャージパターンコード
                    LNM0019row("BILLINGCYCLE") = LNM0019INProw("BILLINGCYCLE") AndAlso                          '請求サイクル
                    LNM0019row("STYMD") = LNM0019INProw("STYMD") Then                                           '有効開始日
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0019row("DELFLG") = LNM0019INProw("DELFLG") AndAlso
                        LNM0019row("TORINAME") = LNM0019INProw("TORINAME") AndAlso                              '取引先名称
                        LNM0019row("ORGNAME") = LNM0019INProw("ORGNAME") AndAlso                                '部門名称
                        LNM0019row("KASANORGCODE") = LNM0019INProw("KASANORGCODE") AndAlso                      '加算先部門コード
                        LNM0019row("KASANORGNAME") = LNM0019INProw("KASANORGNAME") AndAlso                      '加算先部門名称
                        LNM0019row("SURCHARGEPATTERNNAME") = LNM0019INProw("SURCHARGEPATTERNNAME") AndAlso      'サーチャージパターン名
                        LNM0019row("CALCMETHOD") = LNM0019INProw("CALCMETHOD") AndAlso                          '距離算定方式
                        LNM0019row("ENDYMD") = LNM0019INProw("ENDYMD") AndAlso                                  '有効終了日
                        LNM0019row("DIESELPRICESITEID") = LNM0019INProw("DIESELPRICESITEID") AndAlso            '実勢軽油価格参照先ID
                        LNM0019row("DIESELPRICESITEBRANCH") = LNM0019INProw("DIESELPRICESITEBRANCH") AndAlso    '実勢軽油価格参照先ID枝番
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0019row("OPERATION")) Then

                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0019INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0019INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0019INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0019INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now
                Dim WW_DBDataCheck As String = ""
                Dim WW_BeforeMAXSTYMD As String = ""
                Dim WW_STYMD_SAVE As String = ""
                Dim WW_ENDYMD_SAVE As String = ""

                WF_AUTOENDYMD.Value = ""

                '新規、有効開始日が変更されたときの対応
                If work.AddDataChk(SQLcon, LNM0019INPtbl.Rows(0)) = True Then '新規の場合
                    WF_AUTOENDYMD.Value = LNM0019WRKINC.MAX_ENDYMD
                Else
                    '更新前の最大有効開始日取得
                    WW_BeforeMAXSTYMD = LNM0019WRKINC.GetSTYMD(SQLcon, LNM0019INPtbl.Rows(0), WW_DBDataCheck)
                    If Not isNormal(WW_DBDataCheck) Then
                        Exit Sub
                    End If

                    Select Case True
                        'DBに登録されている有効開始日が無かった場合
                        Case WW_BeforeMAXSTYMD = ""
                            WF_AUTOENDYMD.Value = LNM0019WRKINC.MAX_ENDYMD
                            '同一の場合
                        Case WW_BeforeMAXSTYMD = CDate(LNM0019INPtbl.Rows(0)("STYMD")).ToString("yyyy/MM/dd")
                            WF_AUTOENDYMD.Value = LNM0019WRKINC.MAX_ENDYMD
                        '更新前有効開始日 <　入力有効開始日(DBに登録されている有効開始日よりも登録しようとしている有効開始日が大きい場合)
                        Case WW_BeforeMAXSTYMD < CDate(LNM0019INPtbl.Rows(0)("STYMD")).ToString("yyyy/MM/dd")
                            'DBに登録されている有効開始日の有効終了日を登録しようとしている有効開始日-1にする

                            '変更後の有効開始日退避
                            WW_STYMD_SAVE = LNM0019INPtbl.Rows(0)("STYMD")
                            '変更後の有効終了日退避
                            WW_ENDYMD_SAVE = LNM0019INPtbl.Rows(0)("ENDYMD")

                            '変更後テーブルに変更前の有効開始日格納
                            LNM0019INPtbl.Rows(0)("STYMD") = WW_BeforeMAXSTYMD
                            '変更後テーブルに更新用の有効終了日格納
                            LNM0019INPtbl.Rows(0)("ENDYMD") = DateTime.Parse(WW_STYMD_SAVE).AddDays(-1).ToString("yyyy/MM/dd")
                            '履歴テーブルに変更前データを登録
                            InsertHist(SQLcon, LNM0019WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
                            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                Exit Sub
                            End If
                            '変更前の有効終了日更新
                            UpdateENDYMD(SQLcon, LNM0019INPtbl.Rows(0), WW_DBDataCheck, WW_DATE)
                            If Not isNormal(WW_DBDataCheck) Then
                                Exit Sub
                            End If
                            '履歴テーブルに変更後データを登録
                            InsertHist(SQLcon, LNM0019WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
                            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                Exit Sub
                            End If
                            '退避した有効開始日を元に戻す
                            LNM0019INPtbl.Rows(0)("STYMD") = WW_STYMD_SAVE
                            '退避した有効終了日を元に戻す
                            LNM0019INPtbl.Rows(0)("ENDYMD") = WW_ENDYMD_SAVE
                            '有効終了日に最大値を入れる
                            WF_AUTOENDYMD.Value = LNM0019WRKINC.MAX_ENDYMD
                        Case Else
                            '有効終了日に有効開始日の月の末日を入れる
                            Dim WW_NEXT_YM As String = DateTime.Parse(LNM0019INPtbl.Rows(0)("STYMD")).AddMonths(1).ToString("yyyy/MM")
                            WF_AUTOENDYMD.Value = DateTime.Parse(WW_NEXT_YM & "/01").AddDays(-1).ToString("yyyy/MM/dd")
                    End Select
                End If

                '変更チェック
                MASTEREXISTS(SQLcon, WW_MODIFYKBN)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0019WRKINC.MODIFYKBN.AFTDATA
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
        For Each LNM0019INProw As DataRow In LNM0019INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0019row As DataRow In LNM0019tbl.Rows
                ' 同一レコードか判定
                If LNM0019row("TORICODE") = LNM0019INProw("TORICODE") AndAlso                                   '取引先コード
                   LNM0019row("ORGCODE") = LNM0019INProw("ORGCODE") AndAlso                                    '部門コード
                   LNM0019row("SURCHARGEPATTERNCODE") = LNM0019INProw("SURCHARGEPATTERNCODE") AndAlso          'サーチャージパターンコード
                   LNM0019row("BILLINGCYCLE") = LNM0019INProw("BILLINGCYCLE") AndAlso                          '請求サイクル
                   LNM0019row("STYMD") = LNM0019INProw("STYMD") Then                                           '有効開始日
                    ' 画面入力テーブル項目設定
                    LNM0019INProw("LINECNT") = LNM0019row("LINECNT")
                    LNM0019INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0019INProw("UPDTIMSTP") = LNM0019row("UPDTIMSTP")
                    LNM0019INProw("SELECT") = 0
                    LNM0019INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0019row.ItemArray = LNM0019INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0019tbl.NewRow
                WW_NRow.ItemArray = LNM0019INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0019tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                WW_NRow("UPDTIMSTP") = Date.Now
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0019tbl.Rows.Add(WW_NRow)
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
                Case "AVOCADOTODOKECODE"        '実績届先コード
                    'work.CODENAMEGetAVOCADOTODOKE(SQLcon, WW_NAMEht)
            End Select
        End Using

        Try
            Select Case I_FIELD
                Case "CAMPCODE"         '会社コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_COMPANY, I_VALUE, O_TEXT, O_RTN, work.CreateCOMPANYParam(GL0001CompList.LC_COMPANY_TYPE.ALL, Master.USERCAMP))
                Case "DELFLG"           '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "DELFLG"))
                Case "AVOCADOTODOKECODE"         '実績届先コード
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
