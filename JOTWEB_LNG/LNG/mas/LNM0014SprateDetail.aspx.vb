''************************************************************
' 統合版特別料金マスタメンテ登録画面
' 作成日 2025/03/18
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2025/03/18 新規作成
'          : 
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 特別料金マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0014SprateDetail
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0014tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0014INPtbl As DataTable                              'チェック用テーブル
    Private LNM0014UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0014tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_ButtonCLEAR", "LNM0014L"  '戻るボタン押下（LNM0014Lは、パンくずより）
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
                        Case "mspTodokeCodeSingleRowSelected"       '[共通]届先コード選択ポップアップで行選択
                            RowSelected_mspTodokeCodeSingle()
                        Case "mspBigcateCodeSingleRowSelected"      '[共通]大分類コード選択ポップアップで行選択
                            RowSelected_mspBigcateCodeSingle()
                            'RowSelected_mspGroupIdSingle()
                        Case "mspMidcateCodeSingleRowSelected"      '[共通]中分類コード選択ポップアップで行選択
                            RowSelected_mspMidCateCodeSingle()

                        Case "mspSmallcateCodeSingleRowSelected"    '[共通]小分類コード選択ポップアップで行選択
                            RowSelected_mspSmallCateCodeSingle()
                            'RowSelected_mspDetailIdSingle()
                        Case "WF_TORIChange"    '取引先コードチェンジ
                            WF_TORICODE_TEXT.Text = WF_TORI.SelectedValue
                        Case "WF_ORGChange"    '部門コードチェンジ
                            WF_ORGCODE_TEXT.Text = WF_ORG.SelectedValue
                        Case "WF_KASANORGChange"    '加算先部門コードチェンジ
                            WF_KASANORGCODE_TEXT.Text = WF_KASANORG.SelectedValue
                        Case "WF_SelectCALENDARChange" 'カレンダーチェンジ
                            WF_ACCOUNTCODE_TEXT.Text = ""
                            WF_SEGMENTCODE_TEXT.Text = ""
                            '勘定科目
                            Me.WF_ACCOUNT.Items.Clear()
                            Me.WF_ACCOUNT.Items.Add("")
                            Dim retAccountList As New DropDownList
                            retAccountList = LNM0014WRKINC.getDowpDownAccountList(WF_TARGETYM.Value)
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
                            retSegmentList = LNM0014WRKINC.getDowpDownSegmentList(WW_YM, WF_ACCOUNT.SelectedValue)

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
            If Not IsNothing(LNM0014tbl) Then
                LNM0014tbl.Clear()
                LNM0014tbl.Dispose()
                LNM0014tbl = Nothing
            End If

            If Not IsNothing(LNM0014INPtbl) Then
                LNM0014INPtbl.Clear()
                LNM0014INPtbl.Dispose()
                LNM0014INPtbl = Nothing
            End If

            If Not IsNothing(LNM0014UPDtbl) Then
                LNM0014UPDtbl.Clear()
                LNM0014UPDtbl.Dispose()
                LNM0014UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0014WRKINC.MAPIDD
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
        Dim retToriList As New DropDownList
        retToriList = LNM0014WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG)
        For index As Integer = 0 To retToriList.Items.Count - 1
            WF_TORI.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
        Next

        '部門
        Me.WF_ORG.Items.Clear()
        Me.WF_ORG.Items.Add("")
        Dim retOrgList As New DropDownList
        retOrgList = LNM0014WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG)

        If retOrgList.Items.Count > 0 Then
            '情シス、高圧ガス以外
            If LNM0014WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
                Dim WW_OrgPermitHt As New Hashtable
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    SQLcon.Open()  ' DataBase接続
                    work.GetPermitOrg(SQLcon, Master.USERCAMP, Master.ROLE_ORG, WW_OrgPermitHt)
                    For index As Integer = 0 To retOrgList.Items.Count - 1
                        If WW_OrgPermitHt.ContainsKey(retOrgList.Items(index).Value) = True Then
                            WF_ORG.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
                        End If
                    Next
                End Using
            Else
                For index As Integer = 0 To retOrgList.Items.Count - 1
                    WF_ORG.Items.Add(New ListItem(retOrgList.Items(index).Text, retOrgList.Items(index).Value))
                Next
            End If
        End If

        '加算先部門
        Me.WF_KASANORG.Items.Clear()
        Me.WF_KASANORG.Items.Add("")
        Dim retKasanOrgList As New DropDownList
        retKasanOrgList = LNM0014WRKINC.getDowpDownKasanOrgList(Master.MAPID, Master.ROLE_ORG)
        For index As Integer = 0 To retKasanOrgList.Items.Count - 1
            WF_KASANORG.Items.Add(New ListItem(retKasanOrgList.Items(index).Text, retKasanOrgList.Items(index).Value))
        Next

        '計算単位ドロップダウンのクリア
        Me.ddlSelectCALCUNIT.Items.Clear()
        Me.ddlSelectCALCUNIT.Items.Add("")

        '計算単位ドロップダウンの生成
        Dim retCALCUNITList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "CALCUNITDROP")
        If retCALCUNITList.Items.Count > 0 Then
            For index As Integer = 0 To retCALCUNITList.Items.Count - 1
                ddlSelectCALCUNIT.Items.Add(New ListItem(retCALCUNITList.Items(index).Text, retCALCUNITList.Items(index).Value))
            Next
        End If

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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0014L Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        TxtSelLineCNT.Text = work.WF_SEL_LINECNT.Text
        '削除
        RadioDELFLG.SelectedValue = work.WF_SEL_DELFLG.Text
        'CODENAME_get("DELFLG", ddlDELFLG.SelectedValue, LblDelFlgName.Text, WW_Dummy)
        '画面ＩＤ
        TxtMapId.Text = "M00001"
        '会社コード
        TxtCampCode.Text = work.WF_SEL_CAMPCODE.Text
        CODENAME_get("CAMPCODE", TxtCampCode.Text, LblCampCodeName.Text, WW_RtnSW)
        '対象年月
        Dim WK_TARGETYM As String = Replace(work.WF_SEL_TARGETYM.Text, "/", "")
        If Not WK_TARGETYM = "" Then
            WF_TARGETYM.Value = WK_TARGETYM.Substring(0, 4) & "/" & WK_TARGETYM.Substring(4, 2)
        Else
            WF_TARGETYM.Value = work.WF_SEL_TARGETYM.Text
        End If
        WF_TARGETYM_SAVE.Value = WF_TARGETYM.Value

        'WF_TARGETYM.Value = work.WF_SEL_TARGETYM.Text
        '取引先コード、名称
        WF_TORI.SelectedValue = work.WF_SEL_TORICODE.Text
        WF_TORICODE_TEXT.Text = work.WF_SEL_TORICODE.Text
        WF_TORI_SAVE.Value = work.WF_SEL_TORICODE.Text

        '部門コード、名称
        WF_ORG.SelectedValue = work.WF_SEL_ORGCODE.Text
        WF_ORGCODE_TEXT.Text = work.WF_SEL_ORGCODE.Text
        WF_ORG_SAVE.Value = work.WF_SEL_ORGCODE.Text

        '加算先部門コード、名称
        WF_KASANORG.SelectedValue = work.WF_SEL_KASANORGCODE.Text
        WF_KASANORGCODE_TEXT.Text = work.WF_SEL_KASANORGCODE.Text

        '大分類コード
        TxtBIGCATECODE.Text = work.WF_SEL_BIGCATECODE.Text
        '大分類名
        TxtBIGCATENAME.Text = work.WF_SEL_BIGCATENAME.Text
        WF_BIGCATENAME_SAVE.Value = work.WF_SEL_BIGCATENAME.Text
        '中分類コード
        TxtMIDCATECODE.Text = work.WF_SEL_MIDCATECODE.Text
        '中分類名
        TxtMIDCATENAME.Text = work.WF_SEL_MIDCATENAME.Text
        WF_MIDCATENAME_SAVE.Value = work.WF_SEL_MIDCATENAME.Text
        '小分類コード
        TxtSMALLCATECODE.Text = work.WF_SEL_SMALLCATECODE.Text
        '小分類名
        TxtSMALLCATENAME.Text = work.WF_SEL_SMALLCATENAME.Text
        WF_SMALLCATENAME_SAVE.Value = work.WF_SEL_SMALLCATENAME.Text

#Region "コメント-2025/07/30(分類追加対応のため)"
        ''届先コード
        'TxtTODOKECODE.Text = work.WF_SEL_TODOKECODE.Text
        ''届先名称
        'TxtTODOKENAME.Text = work.WF_SEL_TODOKENAME.Text
        ''グループソート順
        'TxtGROUPSORTNO.Text = work.WF_SEL_GROUPSORTNO.Text
        ''グループID
        'TxtGROUPID.Text = work.WF_SEL_GROUPID.Text
        ''グループ名
        'TxtGROUPNAME.Text = work.WF_SEL_GROUPNAME.Text
        'WF_GROUPNAME_SAVE.Value = work.WF_SEL_GROUPNAME.Text
        ''明細ソート順
        'TxtDETAILSORTNO.Text = work.WF_SEL_DETAILSORTNO.Text
        ''明細ID
        'TxtDETAILID.Text = work.WF_SEL_DETAILID.Text
        ''明細名
        'TxtDETAILNAME.Text = work.WF_SEL_DETAILNAME.Text
        'WF_DETAILNAME_SAVE.Value = work.WF_SEL_DETAILNAME.Text
#End Region

        '単価
        TxtTANKA.Text = work.WF_SEL_TANKA.Text
        '数量
        TxtQUANTITY.Text = work.WF_SEL_QUANTITY.Text

        '計算単位(KEYは固定値マスタのCALCUNITDROPに合わせる)
        Select Case work.WF_SEL_CALCUNIT.Text
            Case "t", "トン単価" : ddlSelectCALCUNIT.SelectedValue = "1"
            Case "回", "回数" : ddlSelectCALCUNIT.SelectedValue = "2"
            Case "人", "人数" : ddlSelectCALCUNIT.SelectedValue = "3"
            Case "台", "台数" : ddlSelectCALCUNIT.SelectedValue = "4"
            Case "式", "定数" : ddlSelectCALCUNIT.SelectedValue = "9"
            Case Else : ddlSelectCALCUNIT.SelectedValue = ""
        End Select

        '出荷地
        TxtDEPARTURE.Text = work.WF_SEL_DEPARTURE.Text
        '走行距離
        TxtMILEAGE.Text = work.WF_SEL_MILEAGE.Text
        '輸送回数
        TxtSHIPPINGCOUNT.Text = work.WF_SEL_SHIPPINGCOUNT.Text
        '燃費
        TxtNENPI.Text = work.WF_SEL_NENPI.Text
        '実勢軽油価格
        TxtDIESELPRICECURRENT.Text = work.WF_SEL_DIESELPRICECURRENT.Text
        '基準経由価格
        TxtDIESELPRICESTANDARD.Text = work.WF_SEL_DIESELPRICESTANDARD.Text
        '燃料使用量
        TxtDIESELCONSUMPTION.Text = work.WF_SEL_DIESELCONSUMPTION.Text
        '表示フラグ
        RadioDISPLAYFLG.SelectedValue = work.WF_SEL_DISPLAYFLG.Text
        '鑑分けフラグ
        RadioASSESSMENTFLG.SelectedValue = work.WF_SEL_ASSESSMENTFLG.Text
        '宛名会社名
        TxtATENACOMPANYNAME.Text = work.WF_SEL_ATENACOMPANYNAME.Text
        '宛名会社部門名
        TxtATENACOMPANYDEVNAME.Text = work.WF_SEL_ATENACOMPANYDEVNAME.Text
        '請求書発行部店名
        TxtFROMORGNAME.Text = work.WF_SEL_FROMORGNAME.Text
        '明細区分
        ddlMEISAICATEGORYID.SelectedValue = work.WF_SEL_MEISAICATEGORYID.Text
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
        'DisabledKeyItem.Value = work.WF_SEL_TORICODE.Text
#Region "コメント-2025/07/30(分類追加対応のため)"
        'DisabledKeyItem.Value = work.WF_SEL_GROUPID.Text
#End Region
        DisabledKeyItem.Value = work.WF_SEL_BIGCATECODE.Text

        '表示制御項目
        '情シス、高圧ガス以外の場合
        If LNM0014WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            VisibleKeyOrgCode.Value = ""
        Else
            VisibleKeyOrgCode.Value = Master.ROLE_ORG
        End If

        ' 大分類コード、中分類コード、小分類コード、輸送回数を入力するテキストボックスは数値(0～9)のみ可能とする。
#Region "コメント-2025/07/30(分類追加対応のため)"
        'Me.TxtGROUPSORTNO.Attributes("onkeyPress") = "CheckNum()"
        'Me.TxtDETAILSORTNO.Attributes("onkeyPress") = "CheckNum()"
#End Region
        Me.TxtBIGCATECODE.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtMIDCATECODE.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtSMALLCATECODE.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtSHIPPINGCOUNT.Attributes("onkeyPress") = "CheckNum()"

        ' 入力するテキストボックスは数値(0～9)＋記号(-)のみ可能とする。
        Me.TxtTANKA.Attributes("onkeyPress") = "CheckTel()"             '単価

        ' 入力するテキストボックスは数値(0～9)＋記号(.)のみ可能とする。
        Me.TxtQUANTITY.Attributes("onkeyPress") = "CheckDeci()"             '数量
        Me.TxtMILEAGE.Attributes("onkeyPress") = "CheckDeci()"             '走行距離
        Me.TxtNENPI.Attributes("onkeyPress") = "CheckDeci()"             '燃費
        Me.TxtDIESELPRICECURRENT.Attributes("onkeyPress") = "CheckDeci()"             '実勢軽油価格
        Me.TxtDIESELPRICESTANDARD.Attributes("onkeyPress") = "CheckDeci()"             '基準経由価格
        Me.TxtDIESELCONSUMPTION.Attributes("onkeyPress") = "CheckDeci()"             '燃料使用量
        Me.TxtJOTPERCENTAGE.Attributes("onkeyPress") = "CheckDeci()"       '割合JOT
        Me.TxtENEXPERCENTAGE.Attributes("onkeyPress") = "CheckDeci()"      '割合ENEX

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU
    End Sub

    ''' <summary>
    ''' 取引先件数取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Function GetToriCnt(ByVal SQLcon As MySqlConnection, ByVal WW_ORGCODE As String) As Integer
        GetToriCnt = 0

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT DISTINCT ")
        SQLStr.AppendLine("       TORICODE")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     LNG.LNM0014_SPRATE2 ")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("        ORGCODE  = @ORGCODE                   ")
        SQLStr.AppendLine("   AND  DELFLG  = '0'                         ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード

                P_ORGCODE.Value = WW_ORGCODE '部門コード

                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)
                End Using

                GetToriCnt = WW_Tbl.Rows.Count
            End Using
        Catch ex As Exception
        End Try
    End Function

    ''' <summary>
    ''' 特別料金マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(特別料金マスタ)
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("     INSERT INTO LNG.LNM0014_SPRATE2         ")
        SQLStr.AppendLine("        (                                    ")
        SQLStr.AppendLine("         DELFLG                              ")
        SQLStr.AppendLine("       , TARGETYM                            ")
        SQLStr.AppendLine("       , TORICODE                            ")
        SQLStr.AppendLine("       , TORINAME                            ")
        SQLStr.AppendLine("       , ORGCODE                             ")
        SQLStr.AppendLine("       , ORGNAME                             ")
        SQLStr.AppendLine("       , KASANORGCODE                        ")
        SQLStr.AppendLine("       , KASANORGNAME                        ")
        SQLStr.AppendLine("       , BIGCATECODE                         ")
        SQLStr.AppendLine("       , BIGCATENAME                         ")
        SQLStr.AppendLine("       , MIDCATECODE                         ")
        SQLStr.AppendLine("       , MIDCATENAME                         ")
        SQLStr.AppendLine("       , SMALLCATECODE                       ")
        SQLStr.AppendLine("       , SMALLCATENAME                       ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("       , TODOKECODE                          ")
        'SQLStr.AppendLine("       , TODOKENAME                          ")
        'SQLStr.AppendLine("       , GROUPSORTNO                         ")
        'SQLStr.AppendLine("       , GROUPID                             ")
        'SQLStr.AppendLine("       , GROUPNAME                           ")
        'SQLStr.AppendLine("       , DETAILSORTNO                        ")
        'SQLStr.AppendLine("       , DETAILID                            ")
        'SQLStr.AppendLine("       , DETAILNAME                          ")
#End Region
        SQLStr.AppendLine("       , TANKA                               ")
        SQLStr.AppendLine("       , QUANTITY                            ")
        SQLStr.AppendLine("       , CALCUNIT                            ")
        SQLStr.AppendLine("       , DEPARTURE                           ")
        SQLStr.AppendLine("       , MILEAGE                             ")
        SQLStr.AppendLine("       , SHIPPINGCOUNT                       ")
        SQLStr.AppendLine("       , NENPI                               ")
        SQLStr.AppendLine("       , DIESELPRICECURRENT                  ")
        SQLStr.AppendLine("       , DIESELPRICESTANDARD                 ")
        SQLStr.AppendLine("       , DIESELCONSUMPTION                   ")
        SQLStr.AppendLine("       , DISPLAYFLG                          ")
        SQLStr.AppendLine("       , ASSESSMENTFLG                       ")
        SQLStr.AppendLine("       , ATENACOMPANYNAME                    ")
        SQLStr.AppendLine("       , ATENACOMPANYDEVNAME                 ")
        SQLStr.AppendLine("       , FROMORGNAME                         ")
        SQLStr.AppendLine("       , MEISAICATEGORYID                    ")
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
        SQLStr.AppendLine("       , UPDTIMSTP                           ")
        SQLStr.AppendLine("        )                                    ")
        SQLStr.AppendLine("     VALUES                                  ")
        SQLStr.AppendLine("        (                                    ")
        SQLStr.AppendLine("         @DELFLG                             ")
        SQLStr.AppendLine("        , @TARGETYM                          ")
        SQLStr.AppendLine("        , @TORICODE                          ")
        SQLStr.AppendLine("        , @TORINAME                          ")
        SQLStr.AppendLine("        , @ORGCODE                           ")
        SQLStr.AppendLine("        , @ORGNAME                           ")
        SQLStr.AppendLine("        , @KASANORGCODE                      ")
        SQLStr.AppendLine("        , @KASANORGNAME                      ")
        SQLStr.AppendLine("        , @BIGCATECODE                       ")
        SQLStr.AppendLine("        , @BIGCATENAME                       ")
        SQLStr.AppendLine("        , @MIDCATECODE                       ")
        SQLStr.AppendLine("        , @MIDCATENAME                       ")
        SQLStr.AppendLine("        , @SMALLCATECODE                     ")
        SQLStr.AppendLine("        , @SMALLCATENAME                     ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("        , @TODOKECODE                        ")
        'SQLStr.AppendLine("        , @TODOKENAME                        ")
        'SQLStr.AppendLine("        , @GROUPSORTNO                       ")
        'SQLStr.AppendLine("        , @GROUPID                           ")
        'SQLStr.AppendLine("        , @GROUPNAME                         ")
        'SQLStr.AppendLine("        , @DETAILSORTNO                      ")
        'SQLStr.AppendLine("        , @DETAILID                          ")
        'SQLStr.AppendLine("        , @DETAILNAME                        ")
#End Region
        SQLStr.AppendLine("        , @TANKA                             ")
        SQLStr.AppendLine("        , @QUANTITY                          ")
        SQLStr.AppendLine("        , @CALCUNIT                          ")
        SQLStr.AppendLine("        , @DEPARTURE                         ")
        SQLStr.AppendLine("        , @MILEAGE                           ")
        SQLStr.AppendLine("        , @SHIPPINGCOUNT                     ")
        SQLStr.AppendLine("        , @NENPI                             ")
        SQLStr.AppendLine("        , @DIESELPRICECURRENT                ")
        SQLStr.AppendLine("        , @DIESELPRICESTANDARD               ")
        SQLStr.AppendLine("        , @DIESELCONSUMPTION                 ")
        SQLStr.AppendLine("        , @DISPLAYFLG                        ")
        SQLStr.AppendLine("        , @ASSESSMENTFLG                     ")
        SQLStr.AppendLine("        , @ATENACOMPANYNAME                  ")
        SQLStr.AppendLine("        , @ATENACOMPANYDEVNAME               ")
        SQLStr.AppendLine("        , @FROMORGNAME                       ")
        SQLStr.AppendLine("        , @MEISAICATEGORYID                  ")
        SQLStr.AppendLine("       , @ACCOUNTCODE                        ")
        SQLStr.AppendLine("       , @ACCOUNTNAME                        ")
        SQLStr.AppendLine("       , @SEGMENTCODE                        ")
        SQLStr.AppendLine("       , @SEGMENTNAME                        ")
        SQLStr.AppendLine("       , @JOTPERCENTAGE                      ")
        SQLStr.AppendLine("       , @ENEXPERCENTAGE                     ")
        SQLStr.AppendLine("        , @BIKOU1                            ")
        SQLStr.AppendLine("        , @BIKOU2                            ")
        SQLStr.AppendLine("        , @BIKOU3                            ")
        SQLStr.AppendLine("       , @INITYMD                            ")
        SQLStr.AppendLine("       , @INITUSER                           ")
        SQLStr.AppendLine("       , @INITTERMID                         ")
        SQLStr.AppendLine("       , @INITPGID                           ")
        SQLStr.AppendLine("       , @RECEIVEYMD                         ")
        SQLStr.AppendLine("       , @UPDTIMSTP                          ")
        SQLStr.AppendLine("        )                                    ")
        SQLStr.AppendLine("     ON DUPLICATE KEY UPDATE                 ")
        SQLStr.AppendLine("         DELFLG         = @DELFLG            ")
        SQLStr.AppendLine("       , TARGETYM     = @TARGETYM            ")
        SQLStr.AppendLine("       , TORICODE     = @TORICODE            ")
        SQLStr.AppendLine("       , TORINAME     = @TORINAME            ")
        SQLStr.AppendLine("       , ORGCODE     = @ORGCODE              ")
        SQLStr.AppendLine("       , ORGNAME     = @ORGNAME              ")
        SQLStr.AppendLine("       , KASANORGCODE     = @KASANORGCODE    ")
        SQLStr.AppendLine("       , KASANORGNAME     = @KASANORGNAME    ")
        SQLStr.AppendLine("       , BIGCATECODE     = @BIGCATECODE      ")
        SQLStr.AppendLine("       , BIGCATENAME     = @BIGCATENAME      ")
        SQLStr.AppendLine("       , MIDCATECODE     = @MIDCATECODE      ")
        SQLStr.AppendLine("       , MIDCATENAME     = @MIDCATENAME      ")
        SQLStr.AppendLine("       , SMALLCATECODE   = @SMALLCATECODE    ")
        SQLStr.AppendLine("       , SMALLCATENAME   = @SMALLCATENAME    ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("       , TODOKECODE     = @TODOKECODE        ")
        'SQLStr.AppendLine("       , TODOKENAME     = @TODOKENAME        ")
        'SQLStr.AppendLine("       , GROUPSORTNO     = @GROUPSORTNO      ")
        'SQLStr.AppendLine("       , GROUPID     = @GROUPID              ")
        'SQLStr.AppendLine("       , GROUPNAME     = @GROUPNAME          ")
        'SQLStr.AppendLine("       , DETAILSORTNO     = @DETAILSORTNO    ")
        'SQLStr.AppendLine("       , DETAILID     = @DETAILID            ")
        'SQLStr.AppendLine("       , DETAILNAME     = @DETAILNAME        ")
#End Region
        SQLStr.AppendLine("       , TANKA     = @TANKA                              ")
        SQLStr.AppendLine("       , QUANTITY     = @QUANTITY                        ")
        SQLStr.AppendLine("       , CALCUNIT     = @CALCUNIT                        ")
        SQLStr.AppendLine("       , DEPARTURE     = @DEPARTURE                      ")
        SQLStr.AppendLine("       , MILEAGE     = @MILEAGE                          ")
        SQLStr.AppendLine("       , SHIPPINGCOUNT     = @SHIPPINGCOUNT              ")
        SQLStr.AppendLine("       , NENPI     = @NENPI                              ")
        SQLStr.AppendLine("       , DIESELPRICECURRENT     = @DIESELPRICECURRENT    ")
        SQLStr.AppendLine("       , DIESELPRICESTANDARD     = @DIESELPRICESTANDARD  ")
        SQLStr.AppendLine("       , DIESELCONSUMPTION     = @DIESELCONSUMPTION      ")
        SQLStr.AppendLine("       , DISPLAYFLG     = @DISPLAYFLG                    ")
        SQLStr.AppendLine("       , ASSESSMENTFLG     = @ASSESSMENTFLG              ")
        SQLStr.AppendLine("       , ATENACOMPANYNAME     = @ATENACOMPANYNAME        ")
        SQLStr.AppendLine("       , ATENACOMPANYDEVNAME     = @ATENACOMPANYDEVNAME  ")
        SQLStr.AppendLine("       , FROMORGNAME     = @FROMORGNAME                  ")
        SQLStr.AppendLine("       , MEISAICATEGORYID     = @MEISAICATEGORYID        ")
        SQLStr.AppendLine("       , ACCOUNTCODE =  @ACCOUNTCODE")
        SQLStr.AppendLine("       , ACCOUNTNAME =  @ACCOUNTNAME")
        SQLStr.AppendLine("       , SEGMENTCODE =  @SEGMENTCODE")
        SQLStr.AppendLine("       , SEGMENTNAME =  @SEGMENTNAME")
        SQLStr.AppendLine("       , JOTPERCENTAGE =  @JOTPERCENTAGE")
        SQLStr.AppendLine("       , ENEXPERCENTAGE =  @ENEXPERCENTAGE")
        SQLStr.AppendLine("       , BIKOU1     = @BIKOU1                            ")
        SQLStr.AppendLine("       , BIKOU2     = @BIKOU2                            ")
        SQLStr.AppendLine("       , BIKOU3     = @BIKOU3                            ")
        SQLStr.AppendLine("       , UPDYMD         = @UPDYMD            ")
        SQLStr.AppendLine("       , UPDUSER        = @UPDUSER           ")
        SQLStr.AppendLine("       , UPDTERMID      = @UPDTERMID         ")
        SQLStr.AppendLine("       , UPDPGID        = @UPDPGID           ")
        SQLStr.AppendLine("       , RECEIVEYMD     = @RECEIVEYMD        ")
        SQLStr.AppendLine("       , UPDTIMSTP      = @UPDTIMSTP         ")

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl = New StringBuilder
        SQLJnl.AppendLine(" SELECT                                     ")
        SQLJnl.AppendLine("     DELFLG                                 ")
        SQLJnl.AppendLine("   , TARGETYM                               ")
        SQLJnl.AppendLine("   , TORICODE                               ")
        SQLJnl.AppendLine("   , TORINAME                               ")
        SQLJnl.AppendLine("   , ORGCODE                                ")
        SQLJnl.AppendLine("   , ORGNAME                                ")
        SQLJnl.AppendLine("   , KASANORGCODE                           ")
        SQLJnl.AppendLine("   , KASANORGNAME                           ")
        SQLJnl.AppendLine("   , BIGCATECODE                            ")
        SQLJnl.AppendLine("   , BIGCATENAME                            ")
        SQLJnl.AppendLine("   , MIDCATECODE                            ")
        SQLJnl.AppendLine("   , MIDCATENAME                            ")
        SQLJnl.AppendLine("   , SMALLCATECODE                          ")
        SQLJnl.AppendLine("   , SMALLCATENAME                          ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLJnl.AppendLine("   , TODOKECODE                             ")
        'SQLJnl.AppendLine("   , TODOKENAME                             ")
        'SQLJnl.AppendLine("   , GROUPSORTNO                            ")
        'SQLJnl.AppendLine("   , GROUPID                                ")
        'SQLJnl.AppendLine("   , GROUPNAME                              ")
        'SQLJnl.AppendLine("   , DETAILSORTNO                           ")
        'SQLJnl.AppendLine("   , DETAILID                               ")
        'SQLJnl.AppendLine("   , DETAILNAME                             ")
#End Region
        SQLJnl.AppendLine("   , TANKA                                  ")
        SQLJnl.AppendLine("   , QUANTITY                               ")
        SQLJnl.AppendLine("   , CALCUNIT                               ")
        SQLJnl.AppendLine("   , DEPARTURE                              ")
        SQLJnl.AppendLine("   , MILEAGE                                ")
        SQLJnl.AppendLine("   , SHIPPINGCOUNT                          ")
        SQLJnl.AppendLine("   , NENPI                                  ")
        SQLJnl.AppendLine("   , DIESELPRICECURRENT                     ")
        SQLJnl.AppendLine("   , DIESELPRICESTANDARD                    ")
        SQLJnl.AppendLine("   , DIESELCONSUMPTION                      ")
        SQLJnl.AppendLine("   , DISPLAYFLG                             ")
        SQLJnl.AppendLine("   , ASSESSMENTFLG                          ")
        SQLJnl.AppendLine("   , ATENACOMPANYNAME                       ")
        SQLJnl.AppendLine("   , ATENACOMPANYDEVNAME                    ")
        SQLJnl.AppendLine("   , FROMORGNAME                            ")
        SQLJnl.AppendLine("   , MEISAICATEGORYID                       ")
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
        SQLJnl.AppendLine("     LNG.LNM0014_SPRATE2                    ")
        SQLJnl.AppendLine(" WHERE                                      ")
        SQLJnl.AppendLine("         COALESCE(TARGETYM, '')       = @TARGETYM ")
        SQLJnl.AppendLine("    AND  COALESCE(TORICODE, '')       = @TORICODE ")
        SQLJnl.AppendLine("    AND  COALESCE(ORGCODE, '')        = @ORGCODE ")
        SQLJnl.AppendLine("    AND  COALESCE(BIGCATECODE, '0')   = @BIGCATECODE ")
        SQLJnl.AppendLine("    AND  COALESCE(MIDCATECODE, '0')   = @MIDCATECODE ")
        SQLJnl.AppendLine("    AND  COALESCE(SMALLCATECODE, '0') = @SMALLCATECODE ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLJnl.AppendLine("    AND  COALESCE(GROUPID, '0')             = @GROUPID ")
        'SQLJnl.AppendLine("    AND  COALESCE(DETAILID, '0')             = @DETAILID ")
#End Region

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl.ToString, SQLcon)
                ' DB更新用パラメータ
                Dim P_DELFLG As MySqlParameter = SQLcmd.Parameters.Add("@DELFLG", MySqlDbType.VarChar, 1)     '削除フラグ
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)     '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)     '取引先コード
                Dim P_TORINAME As MySqlParameter = SQLcmd.Parameters.Add("@TORINAME", MySqlDbType.VarChar, 50)     '取引先名称
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)     '部門コード
                Dim P_ORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@ORGNAME", MySqlDbType.VarChar, 20)     '部門名称
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6)     '加算先部門コード
                Dim P_KASANORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGNAME", MySqlDbType.VarChar, 20)     '加算先部門名称
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)         '大分類コード
                Dim P_BIGCATENAME As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATENAME", MySqlDbType.VarChar, 100)       '大分類名
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)         '中分類コード
                Dim P_MIDCATENAME As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATENAME", MySqlDbType.VarChar, 100)       '中分類名
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2)     '小分類コード
                Dim P_SMALLCATENAME As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATENAME", MySqlDbType.VarChar, 100)   '小分類名
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '届先コード
                'Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '届先名称
                'Dim P_GROUPSORTNO As MySqlParameter = SQLcmd.Parameters.Add("@GROUPSORTNO", MySqlDbType.Decimal, 2)     'グループソート順
                'Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim P_GROUPNAME As MySqlParameter = SQLcmd.Parameters.Add("@GROUPNAME", MySqlDbType.VarChar, 100)     'グループ名
                'Dim P_DETAILSORTNO As MySqlParameter = SQLcmd.Parameters.Add("@DETAILSORTNO", MySqlDbType.Decimal, 2)     '明細ソート順
                'Dim P_DETAILID As MySqlParameter = SQLcmd.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
                'Dim P_DETAILNAME As MySqlParameter = SQLcmd.Parameters.Add("@DETAILNAME", MySqlDbType.VarChar, 100)     '明細名
#End Region
                Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal, 10, 2)     '単価
                Dim P_QUANTITY As MySqlParameter = SQLcmd.Parameters.Add("@QUANTITY", MySqlDbType.Decimal, 10, 2)     '数量
                Dim P_CALCUNIT As MySqlParameter = SQLcmd.Parameters.Add("@CALCUNIT", MySqlDbType.VarChar, 20)     '計算単位
                Dim P_DEPARTURE As MySqlParameter = SQLcmd.Parameters.Add("@DEPARTURE", MySqlDbType.VarChar, 50)     '出荷地
                Dim P_MILEAGE As MySqlParameter = SQLcmd.Parameters.Add("@MILEAGE", MySqlDbType.Decimal, 10, 2)     '走行距離
                Dim P_SHIPPINGCOUNT As MySqlParameter = SQLcmd.Parameters.Add("@SHIPPINGCOUNT", MySqlDbType.Decimal, 3)     '輸送回数
                Dim P_NENPI As MySqlParameter = SQLcmd.Parameters.Add("@NENPI", MySqlDbType.Decimal, 5, 2)     '燃費
                Dim P_DIESELPRICECURRENT As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICECURRENT", MySqlDbType.Decimal, 5, 2)     '実勢軽油価格
                Dim P_DIESELPRICESTANDARD As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESTANDARD", MySqlDbType.Decimal, 5, 2)     '基準経由価格
                Dim P_DIESELCONSUMPTION As MySqlParameter = SQLcmd.Parameters.Add("@DIESELCONSUMPTION", MySqlDbType.Decimal, 10, 2)     '燃料使用量
                Dim P_DISPLAYFLG As MySqlParameter = SQLcmd.Parameters.Add("@DISPLAYFLG", MySqlDbType.VarChar, 1)     '表示フラグ
                Dim P_ASSESSMENTFLG As MySqlParameter = SQLcmd.Parameters.Add("@ASSESSMENTFLG", MySqlDbType.VarChar, 1)     '鑑分けフラグ
                Dim P_ATENACOMPANYNAME As MySqlParameter = SQLcmd.Parameters.Add("@ATENACOMPANYNAME", MySqlDbType.VarChar, 50)     '宛名会社名
                Dim P_ATENACOMPANYDEVNAME As MySqlParameter = SQLcmd.Parameters.Add("@ATENACOMPANYDEVNAME", MySqlDbType.VarChar, 50)     '宛名会社部門名
                Dim P_FROMORGNAME As MySqlParameter = SQLcmd.Parameters.Add("@FROMORGNAME", MySqlDbType.VarChar, 50)     '請求書発行部店名
                Dim P_MEISAICATEGORYID As MySqlParameter = SQLcmd.Parameters.Add("@MEISAICATEGORYID", MySqlDbType.VarChar, 1)     '明細区分
                Dim P_ACCOUNTCODE As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTCODE", MySqlDbType.Decimal, 8)     '勘定科目コード
                Dim P_ACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNAME", MySqlDbType.VarChar, 100)     '勘定科目名
                Dim P_SEGMENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTCODE", MySqlDbType.Decimal, 5)     'セグメントコード
                Dim P_SEGMENTNAME As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTNAME", MySqlDbType.VarChar, 100)     'セグメント名
                Dim P_JOTPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@JOTPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合JOT
                Dim P_ENEXPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@ENEXPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合ENEX
                Dim P_BIKOU1 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU1", MySqlDbType.VarChar, 100)     '備考1
                Dim P_BIKOU2 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU2", MySqlDbType.VarChar, 100)     '備考2
                Dim P_BIKOU3 As MySqlParameter = SQLcmd.Parameters.Add("@BIKOU3", MySqlDbType.VarChar, 100)     '備考3
                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)     '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)     '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)     '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)     '登録プログラムＩＤ
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)     '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)     '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)     '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)     '更新プログラムＩＤ
                Dim P_RECEIVEYMD As MySqlParameter = SQLcmd.Parameters.Add("@RECEIVEYMD", MySqlDbType.DateTime)     '集信日時
                Dim P_UPDTIMSTP As MySqlParameter = SQLcmd.Parameters.Add("@UPDTIMSTP", MySqlDbType.DateTime)     'タイムスタンプ

                ' 更新ジャーナル出力用パラメータ
                Dim JP_TARGETYM As MySqlParameter = SQLcmdJnl.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)           '対象年月
                Dim JP_TORICODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                Dim JP_ORGCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)             '部門コード
                Dim JP_BIGCATECODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)     '大分類コード
                Dim JP_MIDCATECODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)     '中分類コード
                Dim JP_SMALLCATECODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2) '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim JP_GROUPID As MySqlParameter = SQLcmdJnl.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim JP_DETAILID As MySqlParameter = SQLcmdJnl.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
#End Region
                Dim LNM0014row As DataRow = LNM0014INPtbl.Rows(0)
                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                P_DELFLG.Value = LNM0014row("DELFLG")                   '削除フラグ
                P_TARGETYM.Value = LNM0014row("TARGETYM")               '対象年月
                P_TORICODE.Value = LNM0014row("TORICODE")               '取引先コード
                P_TORINAME.Value = LNM0014row("TORINAME")               '取引先名称
                P_ORGCODE.Value = LNM0014row("ORGCODE")                 '部門コード
                P_ORGNAME.Value = LNM0014row("ORGNAME")                 '部門名称
                P_KASANORGCODE.Value = LNM0014row("KASANORGCODE")       '加算先部門コード
                P_KASANORGNAME.Value = LNM0014row("KASANORGNAME")       '加算先部門名称

#Region "コメント-2025/07/30(分類追加対応のため)"
                'P_TODOKECODE.Value = LNM0014row("TODOKECODE")           '届先コード
                'P_TODOKENAME.Value = LNM0014row("TODOKENAME")           '届先名称
                ''グループソート順が空(新規)の場合グループIDを入れる
                'If LNM0014row("GROUPSORTNO").ToString = "" Then
                '    P_GROUPSORTNO.Value = LNM0014row("GROUPID")
                'Else
                '    P_GROUPSORTNO.Value = LNM0014row("GROUPSORTNO")
                'End If
#End Region

                P_BIGCATECODE.Value = LNM0014row("BIGCATECODE")         '大分類コード
                P_BIGCATENAME.Value = LNM0014row("BIGCATENAME")         '大分類名
                P_MIDCATECODE.Value = LNM0014row("MIDCATECODE")         '中分類コード
                P_MIDCATENAME.Value = LNM0014row("MIDCATENAME")         '中分類名

#Region "コメント-2025/07/30(分類追加対応のため)"
                ''明細ソート順が空(新規)の場合明細IDを入れる
                'If LNM0014row("DETAILSORTNO").ToString = "" Then
                '    P_DETAILSORTNO.Value = LNM0014row("DETAILID")
                'Else
                '    P_DETAILSORTNO.Value = LNM0014row("DETAILSORTNO")
                'End If
#End Region

                P_SMALLCATECODE.Value = LNM0014row("SMALLCATECODE")     '小分類コード
                P_SMALLCATENAME.Value = LNM0014row("SMALLCATENAME")     '小分類名

                '単価
                If LNM0014row("TANKA").ToString = "0" Or LNM0014row("TANKA").ToString = "" Then
                    P_TANKA.Value = DBNull.Value
                Else
                    P_TANKA.Value = LNM0014row("TANKA")
                End If

                '数量
                If LNM0014row("QUANTITY").ToString = "0" Or LNM0014row("QUANTITY").ToString = "" Then
                    P_QUANTITY.Value = 0.00
                    'P_QUANTITY.Value = DBNull.Value
                Else
                    P_QUANTITY.Value = LNM0014row("QUANTITY")
                End If

                '計算単位
                P_CALCUNIT.Value = LNM0014row("CALCUNIT")
                'Select Case LNM0014row("CALCUNIT").ToString
                '    Case "1" : P_CALCUNIT.Value = "t"
                '    Case "2" : P_CALCUNIT.Value = "回"
                '    Case "3" : P_CALCUNIT.Value = "人"
                '    Case "4" : P_CALCUNIT.Value = "台"
                '    Case "9" : P_CALCUNIT.Value = "式"
                '    Case Else : P_CALCUNIT.Value = ""
                'End Select

                '出荷地
                P_DEPARTURE.Value = LNM0014row("DEPARTURE")

                '走行距離
                If LNM0014row("MILEAGE").ToString = "0" Or LNM0014row("MILEAGE").ToString = "" Then
                    P_MILEAGE.Value = DBNull.Value
                Else
                    P_MILEAGE.Value = LNM0014row("MILEAGE")
                End If

                '輸送回数
                If LNM0014row("SHIPPINGCOUNT").ToString = "0" Or LNM0014row("SHIPPINGCOUNT").ToString = "" Then
                    P_SHIPPINGCOUNT.Value = DBNull.Value
                Else
                    P_SHIPPINGCOUNT.Value = LNM0014row("SHIPPINGCOUNT")
                End If

                '燃費
                If LNM0014row("NENPI").ToString = "0" Or LNM0014row("NENPI").ToString = "" Then
                    P_NENPI.Value = DBNull.Value
                Else
                    P_NENPI.Value = LNM0014row("NENPI")
                End If

                '実勢軽油価格
                If LNM0014row("DIESELPRICECURRENT").ToString = "0" Or LNM0014row("DIESELPRICECURRENT").ToString = "" Then
                    P_DIESELPRICECURRENT.Value = DBNull.Value
                Else
                    P_DIESELPRICECURRENT.Value = LNM0014row("DIESELPRICECURRENT")
                End If

                '基準経由価格
                If LNM0014row("DIESELPRICESTANDARD").ToString = "0" Or LNM0014row("DIESELPRICESTANDARD").ToString = "" Then
                    P_DIESELPRICESTANDARD.Value = DBNull.Value
                Else
                    P_DIESELPRICESTANDARD.Value = LNM0014row("DIESELPRICESTANDARD")
                End If

                '燃料使用量
                If LNM0014row("DIESELCONSUMPTION").ToString = "0" Or LNM0014row("DIESELCONSUMPTION").ToString = "" Then
                    P_DIESELCONSUMPTION.Value = DBNull.Value
                Else
                    P_DIESELCONSUMPTION.Value = LNM0014row("DIESELCONSUMPTION")
                End If

                P_DISPLAYFLG.Value = LNM0014row("DISPLAYFLG")           '表示フラグ
                P_ASSESSMENTFLG.Value = LNM0014row("ASSESSMENTFLG")           '鑑分けフラグ

                '宛名会社名
                '鑑分けフラグ1かつ宛名会社名が未設定の場合
                If LNM0014row("ASSESSMENTFLG").ToString = "1" And LNM0014row("ATENACOMPANYNAME").ToString = "" Then
                    P_ATENACOMPANYNAME.Value = LNM0014row("TORINAME")
                Else
                    P_ATENACOMPANYNAME.Value = LNM0014row("ATENACOMPANYNAME")
                End If

                '宛名会社部門名
                '鑑分けフラグ1かつ宛名会社部門名が未設定の場合
                If LNM0014row("ASSESSMENTFLG").ToString = "1" And LNM0014row("ATENACOMPANYDEVNAME").ToString = "" Then
                    P_ATENACOMPANYDEVNAME.Value = ""
                Else
                    P_ATENACOMPANYDEVNAME.Value = LNM0014row("ATENACOMPANYDEVNAME")
                End If

                '請求書発行部店名
                '鑑分けフラグ1かつ本項目が請求書発行部店名の場合
                If LNM0014row("ASSESSMENTFLG").ToString = "1" And LNM0014row("FROMORGNAME").ToString = "" Then
                    P_FROMORGNAME.Value = LNM0014WRKINC.DEFAULT_FROMORGNAME
                Else
                    P_FROMORGNAME.Value = LNM0014row("FROMORGNAME")           '請求書発行部店名
                End If

                P_MEISAICATEGORYID.Value = LNM0014row("MEISAICATEGORYID")           '明細区分

                '勘定科目コード
                If LNM0014row("ACCOUNTCODE").ToString = "" Then
                    P_ACCOUNTCODE.Value = DBNull.Value
                Else
                    P_ACCOUNTCODE.Value = LNM0014row("ACCOUNTCODE")
                End If

                P_ACCOUNTNAME.Value = LNM0014row("ACCOUNTNAME")           '勘定科目名

                'セグメントコード
                If LNM0014row("SEGMENTCODE").ToString = "" Then
                    P_SEGMENTCODE.Value = DBNull.Value
                Else
                    P_SEGMENTCODE.Value = LNM0014row("SEGMENTCODE")
                End If

                P_SEGMENTNAME.Value = LNM0014row("SEGMENTNAME")           'セグメント名

                '割合JOT
                If LNM0014row("JOTPERCENTAGE").ToString = "" Then
                    P_JOTPERCENTAGE.Value = DBNull.Value
                Else
                    P_JOTPERCENTAGE.Value = LNM0014row("JOTPERCENTAGE")
                End If

                '割合ENEX
                If LNM0014row("ENEXPERCENTAGE").ToString = "" Then
                    P_ENEXPERCENTAGE.Value = DBNull.Value
                Else
                    P_ENEXPERCENTAGE.Value = LNM0014row("ENEXPERCENTAGE")
                End If

                P_BIKOU1.Value = LNM0014row("BIKOU1")           '備考1
                P_BIKOU2.Value = LNM0014row("BIKOU2")           '備考2
                P_BIKOU3.Value = LNM0014row("BIKOU3")           '備考3

                P_INITYMD.Value = WW_DateNow                        '登録年月日
                P_INITUSER.Value = Master.USERID                    '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID              '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name       '登録プログラムＩＤ
                P_UPDYMD.Value = WW_DateNow                         '更新年月日
                P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ
                P_RECEIVEYMD.Value = C_DEFAULT_YMD                  '集信日時
                P_UPDTIMSTP.Value = WW_DateNow

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JP_TARGETYM.Value = LNM0014row("TARGETYM")              '対象年月
                JP_TORICODE.Value = LNM0014row("TORICODE")              '取引先コード
                JP_ORGCODE.Value = LNM0014row("ORGCODE")                '部門コード
                JP_BIGCATECODE.Value = LNM0014row("BIGCATECODE")        '大分類コード
                JP_MIDCATECODE.Value = LNM0014row("MIDCATECODE")        '中分類コード
                JP_SMALLCATECODE.Value = LNM0014row("SMALLCATECODE")    '小分類コード

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0014UPDtbl) Then
                        LNM0014UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0014UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0014UPDtbl.Clear()
                    LNM0014UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0014UPDrow As DataRow In LNM0014UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0014D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0014UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014D UPDATE_INSERT"
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

        '特別料金マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0014_SPRATE2 ")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TARGETYM, '')       = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')       = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')        = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BIGCATECODE, '0')   = @BIGCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(MIDCATECODE, '0')   = @MIDCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SMALLCATECODE, '0') = @SMALLCATECODE ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("    AND  COALESCE(GROUPID, '0')             = @GROUPID ")
        'SQLStr.AppendLine("    AND  COALESCE(DETAILID, '0')             = @DETAILID ")
#End Region

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)           '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)             '部門コード
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)     '大分類コード
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)     '中分類コード
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2) '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim P_DETAILID As MySqlParameter = SQLcmd.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
#End Region

                Dim LNM0014row As DataRow = LNM0014INPtbl.Rows(0)

                P_TARGETYM.Value = LNM0014row("TARGETYM")           '対象年月
                P_TORICODE.Value = LNM0014row("TORICODE")           '取引先コード
                P_ORGCODE.Value = LNM0014row("ORGCODE")             '部門コード
                P_BIGCATECODE.Value = LNM0014row("BIGCATECODE")     '大分類コード
                P_MIDCATECODE.Value = LNM0014row("MIDCATECODE")     '中分類コード
                P_SMALLCATECODE.Value = LNM0014row("SMALLCATECODE") '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'P_BIGCATECODE.Value = LNM0014row("GROUPID")           'グループID
                'P_SMALLCATECODE.Value = LNM0014row("DETAILID")           '明細ID
#End Region

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
                        WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0014C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0015_SPRATEHIST2 ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      TARGETYM  ")
        SQLStr.AppendLine("     ,TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,BIGCATECODE  ")
        SQLStr.AppendLine("     ,BIGCATENAME  ")
        SQLStr.AppendLine("     ,MIDCATECODE  ")
        SQLStr.AppendLine("     ,MIDCATENAME  ")
        SQLStr.AppendLine("     ,SMALLCATECODE  ")
        SQLStr.AppendLine("     ,SMALLCATENAME  ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("     ,TODOKECODE  ")
        'SQLStr.AppendLine("     ,TODOKENAME  ")
        'SQLStr.AppendLine("     ,GROUPSORTNO  ")
        'SQLStr.AppendLine("     ,GROUPID  ")
        'SQLStr.AppendLine("     ,GROUPNAME  ")
        'SQLStr.AppendLine("     ,DETAILSORTNO  ")
        'SQLStr.AppendLine("     ,DETAILID  ")
        'SQLStr.AppendLine("     ,DETAILNAME  ")
#End Region
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,QUANTITY  ")
        SQLStr.AppendLine("     ,CALCUNIT  ")
        SQLStr.AppendLine("     ,DEPARTURE  ")
        SQLStr.AppendLine("     ,MILEAGE  ")
        SQLStr.AppendLine("     ,SHIPPINGCOUNT  ")
        SQLStr.AppendLine("     ,NENPI  ")
        SQLStr.AppendLine("     ,DIESELPRICECURRENT  ")
        SQLStr.AppendLine("     ,DIESELPRICESTANDARD  ")
        SQLStr.AppendLine("     ,DIESELCONSUMPTION  ")
        SQLStr.AppendLine("     ,DISPLAYFLG  ")
        SQLStr.AppendLine("     ,ASSESSMENTFLG  ")
        SQLStr.AppendLine("     ,ATENACOMPANYNAME  ")
        SQLStr.AppendLine("     ,ATENACOMPANYDEVNAME  ")
        SQLStr.AppendLine("     ,FROMORGNAME  ")
        SQLStr.AppendLine("     ,MEISAICATEGORYID  ")
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
        SQLStr.AppendLine("      TARGETYM  ")
        SQLStr.AppendLine("     ,TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,BIGCATECODE  ")
        SQLStr.AppendLine("     ,BIGCATENAME  ")
        SQLStr.AppendLine("     ,MIDCATECODE  ")
        SQLStr.AppendLine("     ,MIDCATENAME  ")
        SQLStr.AppendLine("     ,SMALLCATECODE  ")
        SQLStr.AppendLine("     ,SMALLCATENAME  ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("     ,TODOKECODE  ")
        'SQLStr.AppendLine("     ,TODOKENAME  ")
        'SQLStr.AppendLine("     ,GROUPSORTNO  ")
        'SQLStr.AppendLine("     ,GROUPID  ")
        'SQLStr.AppendLine("     ,GROUPNAME  ")
        'SQLStr.AppendLine("     ,DETAILSORTNO  ")
        'SQLStr.AppendLine("     ,DETAILID  ")
        'SQLStr.AppendLine("     ,DETAILNAME  ")
#End Region
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,QUANTITY  ")
        SQLStr.AppendLine("     ,CALCUNIT  ")
        SQLStr.AppendLine("     ,DEPARTURE  ")
        SQLStr.AppendLine("     ,MILEAGE  ")
        SQLStr.AppendLine("     ,SHIPPINGCOUNT  ")
        SQLStr.AppendLine("     ,NENPI  ")
        SQLStr.AppendLine("     ,DIESELPRICECURRENT  ")
        SQLStr.AppendLine("     ,DIESELPRICESTANDARD  ")
        SQLStr.AppendLine("     ,DIESELCONSUMPTION  ")
        SQLStr.AppendLine("     ,DISPLAYFLG  ")
        SQLStr.AppendLine("     ,ASSESSMENTFLG  ")
        SQLStr.AppendLine("     ,ATENACOMPANYNAME  ")
        SQLStr.AppendLine("     ,ATENACOMPANYDEVNAME  ")
        SQLStr.AppendLine("     ,FROMORGNAME  ")
        SQLStr.AppendLine("     ,MEISAICATEGORYID  ")
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
        SQLStr.AppendLine("        LNG.LNM0014_SPRATE2 ")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TARGETYM, '')       = @TARGETYM ")
        SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')       = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')        = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(BIGCATECODE, '0')   = @BIGCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(MIDCATECODE, '0')   = @MIDCATECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SMALLCATECODE, '0') = @SMALLCATECODE ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.AppendLine("    AND  COALESCE(GROUPID, '0')             = @GROUPID ")
        'SQLStr.AppendLine("    AND  COALESCE(DETAILID, '0')             = @DETAILID ")
#End Region

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)           '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)             '部門コード
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)     '大分類コード
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)     '中分類コード
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2) '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim P_DETAILID As MySqlParameter = SQLcmd.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
#End Region
                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0014row As DataRow = LNM0014INPtbl.Rows(0)

                ' DB更新
                P_TARGETYM.Value = LNM0014row("TARGETYM")           '対象年月
                P_TORICODE.Value = LNM0014row("TORICODE")           '取引先コード
                P_ORGCODE.Value = LNM0014row("ORGCODE")             '部門コード
                P_BIGCATECODE.Value = LNM0014row("BIGCATECODE")     '大分類コード
                P_MIDCATECODE.Value = LNM0014row("MIDCATECODE")     '中分類コード
                P_SMALLCATECODE.Value = LNM0014row("SMALLCATECODE") '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'P_BIGCATECODE.Value = LNM0014row("GROUPID")           'グループID
                'P_SMALLCATECODE.Value = LNM0014row("DETAILID")           '明細ID
#End Region

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0014WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0014tbl.Rows(0)("DELFLG") = "0" And LNM0014row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0014WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0014WRKINC.OPERATEKBN.UPDDATA).ToString
                    End If
                End If

                P_MODIFYKBN.Value = WW_MODIFYKBN                '変更区分
                P_MODIFYYMD.Value = WW_NOW                      '変更日時
                P_MODIFYUSER.Value = Master.USERID              '変更ユーザーＩＤ

                P_INITYMD.Value = WW_NOW                        '登録年月日
                P_INITUSER.Value = Master.USERID                '登録ユーザーＩＤ
                P_INITTERMID.Value = Master.USERTERMID          '登録端末
                P_INITPGID.Value = Me.GetType().BaseType.Name   '登録プログラムＩＤ

                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0015_SPRATEHIST2  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0015_SPRATEHIST2  INSERT"
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
        DetailBoxToLNM0014INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0014tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0014tbl, work.WF_SEL_INPTBL.Text)

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
    Protected Sub DetailBoxToLNM0014INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(RadioDELFLG.SelectedValue)      '削除フラグ
        Master.EraseCharToIgnore(WF_TARGETYM.Value)  '対象年月
        Master.EraseCharToIgnore(TxtBIGCATECODE.Text)  '大分類コード
        Master.EraseCharToIgnore(TxtBIGCATENAME.Text)  '大分類名
        Master.EraseCharToIgnore(TxtMIDCATECODE.Text)  '中分類コード
        Master.EraseCharToIgnore(TxtMIDCATENAME.Text)  '中分類名
        Master.EraseCharToIgnore(TxtSMALLCATECODE.Text)  '小分類コード
        Master.EraseCharToIgnore(TxtSMALLCATENAME.Text)  '小分類名
#Region "コメント-2025/07/30(分類追加対応のため)"
        'Master.EraseCharToIgnore(TxtTODOKECODE.Text)  '届先コード
        'Master.EraseCharToIgnore(TxtTODOKENAME.Text)  '届先名称
        'Master.EraseCharToIgnore(TxtGROUPSORTNO.Text)  'グループソート順
        'Master.EraseCharToIgnore(TxtGROUPID.Text)  'グループID
        'Master.EraseCharToIgnore(TxtGROUPNAME.Text)  'グループ名
        'Master.EraseCharToIgnore(TxtDETAILSORTNO.Text)  '明細ソート順
        'Master.EraseCharToIgnore(TxtDETAILID.Text)  '明細ID
        'Master.EraseCharToIgnore(TxtDETAILNAME.Text)  '明細名
#End Region
        Master.EraseCharToIgnore(TxtTANKA.Text)  '単価
        Master.EraseCharToIgnore(TxtQUANTITY.Text)  '数量
        Master.EraseCharToIgnore(TxtDEPARTURE.Text)  '出荷地
        Master.EraseCharToIgnore(TxtMILEAGE.Text)  '走行距離
        Master.EraseCharToIgnore(TxtSHIPPINGCOUNT.Text)  '輸送回数
        Master.EraseCharToIgnore(TxtNENPI.Text)  '燃費
        Master.EraseCharToIgnore(TxtDIESELPRICECURRENT.Text)  '実勢軽油価格
        Master.EraseCharToIgnore(TxtDIESELPRICESTANDARD.Text)  '基準経由価格
        Master.EraseCharToIgnore(TxtDIESELCONSUMPTION.Text)  '燃料使用量
        Master.EraseCharToIgnore(TxtATENACOMPANYNAME.Text)  '宛名会社名
        Master.EraseCharToIgnore(TxtATENACOMPANYDEVNAME.Text)  '宛名会社部門名
        Master.EraseCharToIgnore(TxtFROMORGNAME.Text)  '請求書発行部店名
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

        Master.CreateEmptyTable(LNM0014INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0014INProw As DataRow = LNM0014INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) Then
            LNM0014INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(TxtSelLineCNT.Text, LNM0014INProw("LINECNT"))
            Catch ex As Exception
                LNM0014INProw("LINECNT") = 0
            End Try
        End If

        LNM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0014INProw("UPDTIMSTP") = 0
        LNM0014INProw("SELECT") = 1
        LNM0014INProw("HIDDEN") = 0

        LNM0014INProw("DELFLG") = RadioDELFLG.SelectedValue             '削除フラグ

        '更新の場合
        If Not DisabledKeyItem.Value = "" Then
            LNM0014INProw("TARGETYM") = work.WF_SEL_TARGETYM.Text           '対象年月
            LNM0014INProw("TORICODE") = work.WF_SEL_TORICODE.Text           '取引先コード
            LNM0014INProw("TORINAME") = work.WF_SEL_TORINAME.Text           '取引先名称
            LNM0014INProw("ORGCODE") = work.WF_SEL_ORGCODE.Text             '部門コード
            LNM0014INProw("ORGNAME") = work.WF_SEL_ORGNAME.Text             '部門名称
            LNM0014INProw("BIGCATECODE") = work.WF_SEL_BIGCATECODE.Text     '大分類コード
            LNM0014INProw("BIGCATENAME") = work.WF_SEL_BIGCATENAME.Text     '大分類名
            LNM0014INProw("MIDCATECODE") = work.WF_SEL_MIDCATECODE.Text     '中分類コード
            LNM0014INProw("MIDCATENAME") = work.WF_SEL_MIDCATENAME.Text     '中分類名
            LNM0014INProw("SMALLCATECODE") = work.WF_SEL_SMALLCATECODE.Text '小分類コード
            LNM0014INProw("SMALLCATENAME") = work.WF_SEL_SMALLCATENAME.Text '小分類名
#Region "コメント-2025/07/30(分類追加対応のため)"
            'LNM0014INProw("GROUPID") = work.WF_SEL_GROUPID.Text             'グループID
            'LNM0014INProw("GROUPNAME") = work.WF_SEL_GROUPNAME.Text         'グループ名
            'LNM0014INProw("DETAILID") = work.WF_SEL_DETAILID.Text           '明細ID
            'LNM0014INProw("DETAILNAME") = work.WF_SEL_DETAILNAME.Text       '明細名
#End Region
        Else
            '対象年月
            If Not WF_TARGETYM.Value = "" Then
                LNM0014INProw("TARGETYM") = Replace(WF_TARGETYM.Value, "/", "")
            Else
                LNM0014INProw("TARGETYM") = WF_TARGETYM.Value
            End If
            LNM0014INProw("TORICODE") = WF_TORI.SelectedValue       '取引先コード
            LNM0014INProw("TORINAME") = WF_TORI.SelectedItem        '取引先名称
            LNM0014INProw("ORGCODE") = WF_ORG.SelectedValue         '部門コード
            LNM0014INProw("ORGNAME") = WF_ORG.SelectedItem          '部門名称
            LNM0014INProw("BIGCATECODE") = TxtBIGCATECODE.Text      '大分類コード
            LNM0014INProw("BIGCATENAME") = TxtBIGCATENAME.Text      '大分類名
            LNM0014INProw("MIDCATECODE") = TxtMIDCATECODE.Text      '中分類コード
            LNM0014INProw("MIDCATENAME") = TxtMIDCATENAME.Text      '中分類名
            LNM0014INProw("SMALLCATECODE") = TxtSMALLCATECODE.Text  '小分類コード
            LNM0014INProw("SMALLCATENAME") = TxtSMALLCATENAME.Text  '小分類名
#Region "コメント-2025/07/30(分類追加対応のため)"
            'LNM0014INProw("GROUPID") = TxtGROUPID.Text              'グループID
            'LNM0014INProw("GROUPNAME") = TxtGROUPNAME.Text          'グループ名
            'LNM0014INProw("DETAILID") = TxtDETAILID.Text            '明細ID
            'LNM0014INProw("DETAILNAME") = TxtDETAILNAME.Text        '明細名
#End Region
        End If

        LNM0014INProw("KASANORGCODE") = WF_KASANORG.SelectedValue   '加算先部門コード
        LNM0014INProw("KASANORGNAME") = WF_KASANORG.SelectedItem    '加算先部門名称
#Region "コメント-2025/07/30(分類追加対応のため)"
        'LNM0014INProw("TODOKECODE") = TxtTODOKECODE.Text            '届先コード
        'LNM0014INProw("TODOKENAME") = TxtTODOKENAME.Text            '届先名称
        'LNM0014INProw("GROUPSORTNO") = TxtGROUPSORTNO.Text          'グループソート順
        'LNM0014INProw("DETAILSORTNO") = TxtDETAILSORTNO.Text        '明細ソート順
#End Region

        '単価
        If TxtTANKA.Text = "" Then
            LNM0014INProw("TANKA") = "0"
        Else
            LNM0014INProw("TANKA") = TxtTANKA.Text
        End If
        '数量
        If TxtQUANTITY.Text = "" Then
            LNM0014INProw("QUANTITY") = "0"
        Else
            LNM0014INProw("QUANTITY") = TxtQUANTITY.Text
        End If

        '計算単位
        LNM0014INProw("CALCUNIT") = ddlSelectCALCUNIT.SelectedItem.Text
        'LNM0014INProw("CALCUNIT") = ddlSelectCALCUNIT.SelectedValue

        '出荷地
        LNM0014INProw("DEPARTURE") = TxtDEPARTURE.Text

        '走行距離
        If TxtMILEAGE.Text = "" Then
            LNM0014INProw("MILEAGE") = "0"
        Else
            LNM0014INProw("MILEAGE") = TxtMILEAGE.Text
        End If

        '輸送回数
        If TxtSHIPPINGCOUNT.Text = "" Then
            LNM0014INProw("SHIPPINGCOUNT") = "0"
        Else
            LNM0014INProw("SHIPPINGCOUNT") = TxtSHIPPINGCOUNT.Text
        End If

        '燃費
        If TxtNENPI.Text = "" Then
            LNM0014INProw("NENPI") = "0"
        Else
            LNM0014INProw("NENPI") = TxtNENPI.Text
        End If

        '実勢軽油価格
        If TxtDIESELPRICECURRENT.Text = "" Then
            LNM0014INProw("DIESELPRICECURRENT") = "0"
        Else
            LNM0014INProw("DIESELPRICECURRENT") = TxtDIESELPRICECURRENT.Text
        End If

        '基準経由価格
        If TxtDIESELPRICESTANDARD.Text = "" Then
            LNM0014INProw("DIESELPRICESTANDARD") = "0"
        Else
            LNM0014INProw("DIESELPRICESTANDARD") = TxtDIESELPRICESTANDARD.Text
        End If

        '燃料使用量
        If TxtDIESELCONSUMPTION.Text = "" Then
            LNM0014INProw("DIESELCONSUMPTION") = "0"
        Else
            LNM0014INProw("DIESELCONSUMPTION") = TxtDIESELCONSUMPTION.Text
        End If

        LNM0014INProw("DISPLAYFLG") = RadioDISPLAYFLG.SelectedValue            '表示フラグ
        LNM0014INProw("ASSESSMENTFLG") = RadioASSESSMENTFLG.SelectedValue            '鑑分けフラグ

        '宛名会社名
        If Not TxtATENACOMPANYNAME.Text = "" And Not WF_ATENALISTSELECT.Value = "" Then
            '既に「株式会社」がついている場合除去
            TxtATENACOMPANYNAME.Text = Replace(TxtATENACOMPANYNAME.Text, LNM0014WRKINC.KABUSHIKIKAISHA, "")
            Select Case WF_ATENALISTSELECT.Value
                Case "MAE" : LNM0014INProw("ATENACOMPANYNAME") = LNM0014WRKINC.KABUSHIKIKAISHA & TxtATENACOMPANYNAME.Text
                Case "ATO" : LNM0014INProw("ATENACOMPANYNAME") = TxtATENACOMPANYNAME.Text & LNM0014WRKINC.KABUSHIKIKAISHA
            End Select
        Else
            LNM0014INProw("ATENACOMPANYNAME") = TxtATENACOMPANYNAME.Text
        End If

        LNM0014INProw("ATENACOMPANYDEVNAME") = TxtATENACOMPANYDEVNAME.Text            '宛名会社部門名
        LNM0014INProw("FROMORGNAME") = TxtFROMORGNAME.Text            '請求書発行部店名
        LNM0014INProw("MEISAICATEGORYID") = ddlMEISAICATEGORYID.SelectedValue            '明細区分

        LNM0014INProw("ACCOUNTCODE") = WF_ACCOUNT.SelectedValue           '勘定科目コード
        LNM0014INProw("ACCOUNTNAME") = WF_ACCOUNT.SelectedItem            '勘定科目名

        If Not WF_ACCOUNT.SelectedValue = "" Then
            LNM0014INProw("SEGMENTCODE") = WF_SEGMENT.SelectedValue           'セグメントコード
            LNM0014INProw("SEGMENTNAME") = WF_SEGMENT.SelectedItem            'セグメント名
        Else
            LNM0014INProw("SEGMENTCODE") = ""           'セグメントコード
            LNM0014INProw("SEGMENTNAME") = ""            'セグメント名
        End If

        LNM0014INProw("JOTPERCENTAGE") = TxtJOTPERCENTAGE.Text            '割合JOT
        LNM0014INProw("ENEXPERCENTAGE") = TxtENEXPERCENTAGE.Text            '割合ENEX

        LNM0014INProw("BIKOU1") = TxtBIKOU1.Text            '備考1
        LNM0014INProw("BIKOU2") = TxtBIKOU2.Text            '備考2
        LNM0014INProw("BIKOU3") = TxtBIKOU3.Text            '備考3

        '○ チェック用テーブルに登録する
        LNM0014INPtbl.Rows.Add(LNM0014INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0014INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0014INProw As DataRow = LNM0014INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0014row As DataRow In LNM0014tbl.Rows
            ' KEY項目が等しい時
            If LNM0014row("TARGETYM") = LNM0014INProw("TARGETYM") AndAlso
                LNM0014row("TORICODE") = LNM0014INProw("TORICODE") AndAlso
                LNM0014row("ORGCODE") = LNM0014INProw("ORGCODE") AndAlso
                LNM0014row("BIGCATECODE") = LNM0014INProw("BIGCATECODE") AndAlso
                LNM0014row("MIDCATECODE") = LNM0014INProw("MIDCATECODE") AndAlso
                LNM0014row("SMALLCATECODE") = LNM0014INProw("SMALLCATECODE") Then
                ' KEY項目以外の項目の差異をチェック
                If LNM0014row("DELFLG") = LNM0014INProw("DELFLG") AndAlso
                    LNM0014row("TORINAME") = LNM0014INProw("TORINAME") AndAlso
                    LNM0014row("ORGNAME") = LNM0014INProw("ORGNAME") AndAlso
                    LNM0014row("KASANORGCODE") = LNM0014INProw("KASANORGCODE") AndAlso
                    LNM0014row("KASANORGNAME") = LNM0014INProw("KASANORGNAME") AndAlso
                    LNM0014row("BIGCATENAME") = LNM0014INProw("BIGCATENAME") AndAlso
                    LNM0014row("MIDCATENAME") = LNM0014INProw("MIDCATENAME") AndAlso
                    LNM0014row("SMALLCATENAME") = LNM0014INProw("SMALLCATENAME") AndAlso
                    LNM0014row("TANKA") = LNM0014INProw("TANKA") AndAlso
                    LNM0014row("QUANTITY") = LNM0014INProw("QUANTITY") AndAlso
                    LNM0014row("CALCUNIT") = LNM0014INProw("CALCUNIT") AndAlso
                    LNM0014row("DEPARTURE") = LNM0014INProw("DEPARTURE") AndAlso
                    LNM0014row("MILEAGE") = LNM0014INProw("MILEAGE") AndAlso
                    LNM0014row("SHIPPINGCOUNT") = LNM0014INProw("SHIPPINGCOUNT") AndAlso
                    LNM0014row("NENPI") = LNM0014INProw("NENPI") AndAlso
                    LNM0014row("DIESELPRICECURRENT") = LNM0014INProw("DIESELPRICECURRENT") AndAlso
                    LNM0014row("DIESELPRICESTANDARD") = LNM0014INProw("DIESELPRICESTANDARD") AndAlso
                    LNM0014row("DIESELCONSUMPTION") = LNM0014INProw("DIESELCONSUMPTION") AndAlso
                    LNM0014row("DISPLAYFLG") = LNM0014INProw("DISPLAYFLG") AndAlso
                    LNM0014row("ASSESSMENTFLG") = LNM0014INProw("ASSESSMENTFLG") AndAlso
                    LNM0014row("ATENACOMPANYNAME") = LNM0014INProw("ATENACOMPANYNAME") AndAlso
                    LNM0014row("ATENACOMPANYDEVNAME") = LNM0014INProw("ATENACOMPANYDEVNAME") AndAlso
                    LNM0014row("FROMORGNAME") = LNM0014INProw("FROMORGNAME") AndAlso
                    LNM0014row("MEISAICATEGORYID") = LNM0014INProw("MEISAICATEGORYID") AndAlso
                    LNM0014row("ACCOUNTCODE") = LNM0014INProw("ACCOUNTCODE") AndAlso                                '勘定科目コード
                    LNM0014row("ACCOUNTNAME") = LNM0014INProw("ACCOUNTNAME") AndAlso                                '勘定科目名
                    LNM0014row("SEGMENTCODE") = LNM0014INProw("SEGMENTCODE") AndAlso                                'セグメントコード
                    LNM0014row("SEGMENTNAME") = LNM0014INProw("SEGMENTNAME") AndAlso                                'セグメント名
                    LNM0014row("JOTPERCENTAGE") = LNM0014INProw("JOTPERCENTAGE") AndAlso                                '割合JOT
                    LNM0014row("ENEXPERCENTAGE") = LNM0014INProw("ENEXPERCENTAGE") AndAlso                                '割合ENEX
                    LNM0014row("BIKOU1") = LNM0014INProw("BIKOU1") AndAlso
                    LNM0014row("BIKOU2") = LNM0014INProw("BIKOU2") AndAlso
                    LNM0014row("BIKOU3") = LNM0014INProw("BIKOU3") Then
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
        For Each LNM0014row As DataRow In LNM0014tbl.Rows
            Select Case LNM0014row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0014tbl, work.WF_SEL_INPTBL.Text)

        TxtSelLineCNT.Text = ""                 'LINECNT
        TxtMapId.Text = "M00001"                '画面ＩＤ
        WF_TARGETYM.Value = ""                  '対象年月
        TxtBIGCATECODE.Text = ""                '大分類コード
        TxtBIGCATENAME.Text = ""                '大分類名
        TxtMIDCATECODE.Text = ""                '中分類コード
        TxtMIDCATENAME.Text = ""                '中分類名
        TxtSMALLCATECODE.Text = ""              '小分類コード
        TxtSMALLCATENAME.Text = ""              '小分類名
#Region "コメント-2025/07/30(分類追加対応のため)"
        'TxtTODOKECODE.Text = ""                 '届先コード
        'TxtTODOKENAME.Text = ""                 '届先名称
        'TxtGROUPSORTNO.Text = ""                'グループソート順
        'TxtGROUPID.Text = ""                    'グループID
        'TxtGROUPNAME.Text = ""                  'グループ名
        'TxtDETAILSORTNO.Text = ""               '明細ソート順
        'TxtDETAILID.Text = ""                   '明細ID
        'TxtDETAILNAME.Text = ""                 '明細名
#End Region
        TxtTANKA.Text = ""                      '単価
        TxtQUANTITY.Text = ""                   '数量
        TxtDEPARTURE.Text = ""                  '出荷地
        TxtMILEAGE.Text = ""                    '走行距離
        TxtSHIPPINGCOUNT.Text = ""              '輸送回数
        TxtNENPI.Text = ""                      '燃費
        TxtDIESELPRICECURRENT.Text = ""         '実勢軽油価格
        TxtDIESELPRICESTANDARD.Text = ""        '基準経由価格
        TxtDIESELCONSUMPTION.Text = ""          '燃料使用量
        TxtATENACOMPANYNAME.Text = ""           '宛名会社名
        TxtATENACOMPANYDEVNAME.Text = ""        '宛名会社部門名
        TxtFROMORGNAME.Text = ""                '請求書発行部店名
        TxtJOTPERCENTAGE.Text = ""              '割合JOT
        TxtENEXPERCENTAGE.Text = ""             '割合ENEX
        TxtBIKOU1.Text = ""                     '備考1
        TxtBIKOU2.Text = ""                     '備考2
        TxtBIKOU3.Text = ""                     '備考3

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
#Region "コメント-2025/07/30(分類追加対応のため)"
                    'Case "TxtTODOKECODE"       '届先コード
                    '    leftview.Visible = False
                    '    '検索画面
                    '    DisplayView_mspTodokeCodeSingle()
                    '    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    '    WF_LeftboxOpen.Value = ""
                    '    Exit Sub
#End Region
                    Case "TxtBIGCATENAME"       '大分類名
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspBigcateCodeSingle()
                        'DisplayView_mspGroupIdSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    Case "TxtMIDCATENAME"       '中分類名
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspMidCateCodeSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
                    Case "TxtSMALLCATENAME"     '小分類名
                        leftview.Visible = False
                        '検索画面
                        DisplayView_mspSmallCateCodeSingle()
                        '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                        WF_LeftboxOpen.Value = ""
                        Exit Sub
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
            '    CODENAME_get("DELFLG", ddlDELFLG.SelectedValue, LblDelFlgName.Text, WW_Dummy)
            '    TxtDelFlg.Focus()
            'Case "TxtTORICODE"
            '    CODENAME_get("TORICODE", TxtTORICODE.Text, TxtTORINAME.Text, WW_RtnSW)  '取引先コード
            '    TxtTORICODE.Focus()
            'Case "TxtKASANORGCODE"
            '    CODENAME_get("KASANORGCODE", TxtKASANORGCODE.Text, TxtKASANORGNAME.Text, WW_RtnSW)  '加算先部門コード
            '    TxtKASANORGCODE.Focus()
#Region "コメント-2025/07/30(分類追加対応のため)"
            'Case "TxtTODOKECODE"
            '    'CODENAME_get("TODOKECODE", TxtTODOKECODE.Text, TxtTODOKENAME.Text, WW_RtnSW)  '届先コード
            '    CODENAME_get("TODOKECODE", TxtTODOKENAME.Text, TxtTODOKECODE.Text, WW_RtnSW)  '届先コード
            '    TxtTODOKECODE.Focus()
#End Region
            Case "TxtBIGCATENAME"
                CODENAME_get("BIGCATENAME", TxtBIGCATENAME.Text, TxtBIGCATECODE.Text, WW_RtnSW)         '大分類名
                TxtBIGCATENAME.Focus()
            Case "TxtMIDCATENAME"
                CODENAME_get("MIDCATENAME", TxtMIDCATENAME.Text, TxtMIDCATECODE.Text, WW_RtnSW)         '中分類名
                TxtMIDCATENAME.Focus()
            Case "TxtSMALLCATENAME"
                CODENAME_get("SMALLCATENAME", TxtSMALLCATENAME.Text, TxtSMALLCATECODE.Text, WW_RtnSW)   '小分類名
                TxtSMALLCATENAME.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 特別料金マスタ更新(削除フラグのみ)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UpdateMasterDelflgOnly()
        Dim WW_MODIFYKBN As String = ""
        Dim WW_DATE As Date = Date.Now

        '初期化
        LNM0014INPtbl = New DataTable
        LNM0014INPtbl.Columns.Add("TARGETYM")
        LNM0014INPtbl.Columns.Add("TORICODE")
        LNM0014INPtbl.Columns.Add("ORGCODE")
        LNM0014INPtbl.Columns.Add("BIGCATECODE")
        LNM0014INPtbl.Columns.Add("MIDCATECODE")
        LNM0014INPtbl.Columns.Add("SMALLCATECODE")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'LNM0014INPtbl.Columns.Add("GROUPID")
        'LNM0014INPtbl.Columns.Add("DETAILID")
#End Region
        LNM0014INPtbl.Columns.Add("DELFLG")

        Dim row As DataRow
        row = LNM0014INPtbl.NewRow
        row("TARGETYM") = work.WF_SEL_TARGETYM.Text
        row("TORICODE") = work.WF_SEL_TORICODE.Text
        row("ORGCODE") = work.WF_SEL_ORGCODE.Text
        row("BIGCATECODE") = work.WF_SEL_BIGCATECODE.Text
        row("MIDCATECODE") = work.WF_SEL_MIDCATECODE.Text
        row("SMALLCATECODE") = work.WF_SEL_SMALLCATECODE.Text
#Region "コメント-2025/07/30(分類追加対応のため)"
        'row("GROUPID") = work.WF_SEL_GROUPID.Text
        'row("DETAILID") = work.WF_SEL_DETAILID.Text
#End Region
        row("DELFLG") = C_DELETE_FLG.DELETE
        LNM0014INPtbl.Rows.Add(row)

        ' DB更新処理
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()

            '履歴テーブルに変更前データを登録
            InsertHist(SQLcon, LNM0014WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '削除フラグ更新
            SetDelflg(SQLcon, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '履歴テーブルに変更後データを登録
            InsertHist(SQLcon, LNM0014WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

        End Using

        '○ 入力値反映
        For Each LNM0014INProw As DataRow In LNM0014INPtbl.Rows
            For Each LNM0014row As DataRow In LNM0014tbl.Rows
                If LNM0014INProw("TARGETYM") = LNM0014row("TARGETYM") AndAlso
                    LNM0014INProw("TORICODE") = LNM0014row("TORICODE") AndAlso
                    LNM0014INProw("ORGCODE") = LNM0014row("ORGCODE") AndAlso
                    LNM0014INProw("BIGCATECODE") = LNM0014row("BIGCATECODE") AndAlso
                    LNM0014INProw("MIDCATECODE") = LNM0014row("MIDCATECODE") AndAlso
                    LNM0014INProw("SMALLCATECODE") = LNM0014row("SMALLCATECODE") Then
                    ' 画面入力テーブル項目設定              
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0014row("DELFLG") = LNM0014INProw("DELFLG")
                    LNM0014row("SELECT") = 0
                    LNM0014row("HIDDEN") = 0
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
        SQLStr.Append("     LNG.LNM0014_SPRATE2                     ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TARGETYM, '')       = @TARGETYM ")
        SQLStr.Append("    AND  COALESCE(TORICODE, '')       = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')        = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(BIGCATECODE, '0')   = @BIGCATECODE ")
        SQLStr.Append("    AND  COALESCE(MIDCATECODE, '0')   = @MIDCATECODE ")
        SQLStr.Append("    AND  COALESCE(SMALLCATECODE, '0') = @SMALLCATECODE ")
#Region "コメント-2025/07/30(分類追加対応のため)"
        'SQLStr.Append("    AND  COALESCE(GROUPID, '0')             = @GROUPID ")
        'SQLStr.Append("    AND  COALESCE(DETAILID, '0')             = @DETAILID ")
#End Region

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TARGETYM As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYM", MySqlDbType.VarChar, 6)           '対象年月
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10)          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)             '部門コード
                Dim P_BIGCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@BIGCATECODE", MySqlDbType.Decimal, 2)     '大分類コード
                Dim P_MIDCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@MIDCATECODE", MySqlDbType.Decimal, 2)     '中分類コード
                Dim P_SMALLCATECODE As MySqlParameter = SQLcmd.Parameters.Add("@SMALLCATECODE", MySqlDbType.Decimal, 2) '小分類コード
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Dim P_GROUPID As MySqlParameter = SQLcmd.Parameters.Add("@GROUPID", MySqlDbType.Decimal, 2)     'グループID
                'Dim P_DETAILID As MySqlParameter = SQLcmd.Parameters.Add("@DETAILID", MySqlDbType.Decimal, 2)     '明細ID
#End Region
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)                 '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)            '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)        '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)            '更新プログラムＩＤ

                Dim LNM0014row As DataRow = LNM0014INPtbl.Rows(0)
                P_TARGETYM.Value = LNM0014row("TARGETYM")           '対象年月
                P_TORICODE.Value = LNM0014row("TORICODE")           '取引先コード
                P_ORGCODE.Value = LNM0014row("ORGCODE")             '部門コード
                P_BIGCATECODE.Value = LNM0014row("BIGCATECODE")     '大分類コード
                P_MIDCATECODE.Value = LNM0014row("MIDCATECODE")     '中分類コード
                P_SMALLCATECODE.Value = LNM0014row("SMALLCATECODE") '小分類コード
                P_UPDYMD.Value = WW_NOW                             '更新年月日
                P_UPDUSER.Value = Master.USERID                     '更新ユーザーＩＤ
                P_UPDTERMID.Value = Master.USERTERMID               '更新端末
                P_UPDPGID.Value = Me.GetType().BaseType.Name        '更新プログラムＩＤ

                '登録
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0014C UPDATE"
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
                '    ddlDELFLG.SelectedValue = WW_SelectValue
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

    ''' <summary>
    ''' 届先コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspTodokeCodeSingle()

        Me.mspTodokeCodeSingle.InitPopUp()
        Me.mspTodokeCodeSingle.SelectionMode = ListSelectionMode.Single

        Me.mspTodokeCodeSingle.SQL = CmnSearchSQL.GetSprateTodokeSQL(WF_TORI.SelectedValue, WF_ORG.SelectedValue)

        Me.mspTodokeCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspTodokeCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetSprateTodokeTitle)

        Me.mspTodokeCodeSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 届先選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspTodokeCodeSingle()
#Region "コメント-2025/07/30(分類追加対応のため)"
        'Dim selData = Me.mspTodokeCodeSingle.SelectedSingleItem

        ''○ 変更した項目の名称をセット
        'Select Case WF_FIELD.Value

        '    Case TxtTODOKECODE.ID
        '        Me.TxtTODOKECODE.Text = selData("TODOKECODE").ToString '届先コード
        '        Me.TxtTODOKENAME.Text = selData("TODOKENAME").ToString '届先名
        '        Me.TxtTODOKECODE.Focus()
        'End Select

        ''ポップアップの非表示
        'Me.mspTodokeCodeSingle.HidePopUp()
#End Region
    End Sub

    ''' <summary>
    ''' 大分類コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspBigcateCodeSingle()

        Me.mspBigcateCodeSingle.InitPopUp()
        Me.mspBigcateCodeSingle.SelectionMode = ListSelectionMode.Single

        Me.mspBigcateCodeSingle.SQL = CmnSearchSQL.GetSprateBigcateSQL(WF_TORI.SelectedValue, WF_ORG.SelectedValue, TxtBIGCATENAME.Text)

        Me.mspBigcateCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspBigcateCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetSprateBigcateTitle)

        Me.mspBigcateCodeSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 大分類コード選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspBigcateCodeSingle()

        Dim selData = Me.mspBigcateCodeSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtBIGCATENAME.ID
                Me.TxtBIGCATECODE.Text = selData("BIGCATECODE").ToString    '大分類コード
                Me.TxtBIGCATENAME.Text = selData("BIGCATENAME").ToString    '大分類名
                Me.TxtBIGCATENAME.Focus()
        End Select

        'ポップアップの非表示
        Me.mspBigcateCodeSingle.HidePopUp()

    End Sub

    ''' <summary>
    ''' 中分類コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspMidCateCodeSingle()

        Me.mspMidcateCodeSingle.InitPopUp()
        Me.mspMidcateCodeSingle.SelectionMode = ListSelectionMode.Single

        Me.mspMidcateCodeSingle.SQL = CmnSearchSQL.GetSprateMidcateSQL(WF_TORI.SelectedValue, WF_ORG.SelectedValue, TxtBIGCATECODE.Text, TxtMIDCATENAME.Text)

        Me.mspMidcateCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspMidcateCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetSprateMidCateTitle)

        Me.mspMidcateCodeSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 中分類コード選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspMidCateCodeSingle()

        Dim selData = Me.mspMidcateCodeSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtMIDCATENAME.ID
                Me.TxtMIDCATECODE.Text = selData("MIDCATECODE").ToString '中分類コード
                Me.TxtMIDCATENAME.Text = selData("MIDCATENAME").ToString '中分類名
                Me.TxtMIDCATENAME.Focus()
        End Select

        'ポップアップの非表示
        Me.mspMidcateCodeSingle.HidePopUp()

    End Sub

    ''' <summary>
    ''' 小分類コード検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspSmallCateCodeSingle()

        Me.mspSmallcateCodeSingle.InitPopUp()
        Me.mspSmallcateCodeSingle.SelectionMode = ListSelectionMode.Single

        Me.mspSmallcateCodeSingle.SQL = CmnSearchSQL.GetSprateSmallcateSQL(WF_TORI.SelectedValue, WF_ORG.SelectedValue, TxtBIGCATECODE.Text, TxtMIDCATECODE.Text, prmSmallCateName:=TxtSMALLCATENAME.Text)

        Me.mspSmallcateCodeSingle.KeyFieldName = "KEYCODE"
        Me.mspSmallcateCodeSingle.DispFieldList.AddRange(CmnSearchSQL.GetSprateSmallCateTitle)

        Me.mspSmallcateCodeSingle.ShowPopUpList()

    End Sub

    ''' <summary>
    ''' 小分類コード選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspSmallCateCodeSingle()

        Dim selData = Me.mspSmallcateCodeSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtSMALLCATENAME.ID
                Me.TxtSMALLCATECODE.Text = selData("SMALLCATECODE").ToString '小分類コード
                Me.TxtSMALLCATENAME.Text = selData("SMALLCATENAME").ToString '小分類名
                Me.TxtSMALLCATENAME.Focus()
        End Select

        'ポップアップの非表示
        Me.mspSmallcateCodeSingle.HidePopUp()

    End Sub

#Region "コメント-2025/07/30(分類追加対応のため)"
    '''' <summary>
    '''' グループID検索時処理
    '''' </summary>
    'Protected Sub DisplayView_mspGroupIdSingle()

    '    Me.mspGroupIdSingle.InitPopUp()
    '    Me.mspGroupIdSingle.SelectionMode = ListSelectionMode.Single

    '    Me.mspGroupIdSingle.SQL = CmnSearchSQL.GetSprateGroupSQL(WF_TORI.SelectedValue, WF_ORG.SelectedValue, TxtGROUPNAME.Text)

    '    Me.mspGroupIdSingle.KeyFieldName = "KEYCODE"
    '    Me.mspGroupIdSingle.DispFieldList.AddRange(CmnSearchSQL.GetSprateGroupTitle)

    '    Me.mspGroupIdSingle.ShowPopUpList()

    'End Sub

    '''' <summary>
    '''' グループID選択ポップアップで行選択
    '''' </summary>
    'Protected Sub RowSelected_mspGroupIdSingle()

    '    Dim selData = Me.mspGroupIdSingle.SelectedSingleItem

    '    '○ 変更した項目の名称をセット
    '    Select Case WF_FIELD.Value

    '        Case TxtGROUPNAME.ID
    '            Me.TxtGROUPID.Text = selData("GROUPID").ToString 'グループID
    '            Me.TxtGROUPNAME.Text = selData("GROUPNAME").ToString 'グループ名
    '            Me.TxtGROUPSORTNO.Text = selData("GROUPSORTNO").ToString 'グループソート順
    '            Me.TxtGROUPNAME.Focus()
    '    End Select

    '    'ポップアップの非表示
    '    Me.mspGroupIdSingle.HidePopUp()

    'End Sub

    '''' <summary>
    '''' 明細ID検索時処理
    '''' </summary>
    'Protected Sub DisplayView_mspDetailIdSingle()

    '    Me.mspDetailIdSingle.InitPopUp()
    '    Me.mspDetailIdSingle.SelectionMode = ListSelectionMode.Single

    '    Me.mspDetailIdSingle.SQL = CmnSearchSQL.GetSprateDetailSQL(WF_TORI.SelectedValue, WF_ORG.SelectedValue, TxtGROUPID.Text, TxtDETAILNAME.Text)

    '    Me.mspDetailIdSingle.KeyFieldName = "KEYCODE"
    '    Me.mspDetailIdSingle.DispFieldList.AddRange(CmnSearchSQL.GetSprateDetailTitle)

    '    Me.mspDetailIdSingle.ShowPopUpList()

    'End Sub

    '''' <summary>
    '''' 明細ID選択ポップアップで行選択
    '''' </summary>
    'Protected Sub RowSelected_mspDetailIdSingle()

    '    Dim selData = Me.mspDetailIdSingle.SelectedSingleItem

    '    '○ 変更した項目の名称をセット
    '    Select Case WF_FIELD.Value

    '        Case TxtDETAILNAME.ID
    '            Me.TxtDETAILID.Text = selData("DETAILID").ToString 'グループID
    '            Me.TxtDETAILNAME.Text = selData("DETAILNAME").ToString 'グループ名
    '            Me.TxtDETAILSORTNO.Text = selData("DETAILSORTNO").ToString 'グループソート順
    '            Me.TxtDETAILNAME.Focus()
    '    End Select

    '    'ポップアップの非表示
    '    Me.mspDetailIdSingle.HidePopUp()

    'End Sub
#End Region

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
            WW_CheckMES1 = "・特別料金マスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0014INProw As DataRow In LNM0014INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0014INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0014INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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

            ' 対象年月(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TARGETYM", LNM0014INProw("TARGETYM"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・対象年月エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 取引先コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORICODE", LNM0014INProw("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 取引先名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORINAME", LNM0014INProw("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 部門コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ORGCODE", LNM0014INProw("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・部門コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 部門名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ORGNAME", LNM0014INProw("ORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・部門名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 加算先部門コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KASANORGCODE", LNM0014INProw("KASANORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・加算先部門コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 加算先部門名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KASANORGNAME", LNM0014INProw("KASANORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・加算先部門名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 大分類名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIGCATENAME", LNM0014INProw("BIGCATENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・大分類名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 中分類名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "MIDCATENAME", LNM0014INProw("MIDCATENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・中分類名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 小分類名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SMALLCATENAME", LNM0014INProw("SMALLCATENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・小分類名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
#Region "コメント-2025/07/30(分類追加対応のため)"
            '' 届先コード(バリデーションチェック)
            'Master.CheckField(Master.USERCAMP, "TODOKECODE", LNM0014INProw("TODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・届先コードエラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            '' 届先名称(バリデーションチェック)
            'Master.CheckField(Master.USERCAMP, "TODOKENAME", LNM0014INProw("TODOKENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・届先名称エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            ''' グループソート順(バリデーションチェック)
            ''Master.CheckField(Master.USERCAMP, "GROUPSORTNO", LNM0014INProw("GROUPSORTNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            ''If Not isNormal(WW_CS0024FCheckerr) Then
            ''    WW_CheckMES1 = "・グループソート順エラーです。"
            ''    WW_CheckMES2 = WW_CS0024FCheckReport
            ''    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            ''    WW_LineErr = "ERR"
            ''    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            ''End If
            ''' グループID(バリデーションチェック)
            ''Master.CheckField(Master.USERCAMP, "GROUPID", LNM0014INProw("GROUPID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            ''If Not isNormal(WW_CS0024FCheckerr) Then
            ''    WW_CheckMES1 = "・グループIDエラーです。"
            ''    WW_CheckMES2 = WW_CS0024FCheckReport
            ''    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            ''    WW_LineErr = "ERR"
            ''    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            ''End If
            '' グループ名(バリデーションチェック)
            'Master.CheckField(Master.USERCAMP, "GROUPNAME", LNM0014INProw("GROUPNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・グループ名エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
            ''' 明細ソート順(バリデーションチェック)
            ''Master.CheckField(Master.USERCAMP, "DETAILSORTNO", LNM0014INProw("DETAILSORTNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            ''If Not isNormal(WW_CS0024FCheckerr) Then
            ''    WW_CheckMES1 = "・明細ソート順エラーです。"
            ''    WW_CheckMES2 = WW_CS0024FCheckReport
            ''    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            ''    WW_LineErr = "ERR"
            ''    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            ''End If
            ''' 明細ID(バリデーションチェック)
            ''Master.CheckField(Master.USERCAMP, "DETAILID", LNM0014INProw("DETAILID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            ''If Not isNormal(WW_CS0024FCheckerr) Then
            ''    WW_CheckMES1 = "・明細IDエラーです。"
            ''    WW_CheckMES2 = WW_CS0024FCheckReport
            ''    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            ''    WW_LineErr = "ERR"
            ''    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            ''End If
            '' 明細名(バリデーションチェック)
            'Master.CheckField(Master.USERCAMP, "DETAILNAME", LNM0014INProw("DETAILNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            'If Not isNormal(WW_CS0024FCheckerr) Then
            '    WW_CheckMES1 = "・明細名エラーです。"
            '    WW_CheckMES2 = WW_CS0024FCheckReport
            '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            '    WW_LineErr = "ERR"
            '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            'End If
#End Region
            ' 単価(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TANKA", LNM0014INProw("TANKA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・単価エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 数量(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "QUANTITY", LNM0014INProw("QUANTITY"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・数量エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 計算単位(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CALCUNIT", LNM0014INProw("CALCUNIT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・計算単位エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 出荷地(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPARTURE", LNM0014INProw("DEPARTURE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・出荷地エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 走行距離(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "MILEAGE", LNM0014INProw("MILEAGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・走行距離エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 輸送回数(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SHIPPINGCOUNT", LNM0014INProw("SHIPPINGCOUNT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・輸送回数エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 燃費(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "NENPI", LNM0014INProw("NENPI"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・燃費エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 実勢軽油価格(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DIESELPRICECURRENT", LNM0014INProw("DIESELPRICECURRENT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・実勢軽油価格エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 基準経由価格(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DIESELPRICESTANDARD", LNM0014INProw("DIESELPRICESTANDARD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・基準経由価格エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 燃料使用量(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DIESELCONSUMPTION", LNM0014INProw("DIESELCONSUMPTION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・燃料使用量エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 表示フラグ(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DISPLAYFLG", LNM0014INProw("DISPLAYFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・表示フラグエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 鑑分けフラグ(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ASSESSMENTFLG", LNM0014INProw("ASSESSMENTFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・鑑分けフラグエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 宛名会社名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ATENACOMPANYNAME", LNM0014INProw("ATENACOMPANYNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・宛名会社名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 宛名会社部門名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ATENACOMPANYDEVNAME", LNM0014INProw("ATENACOMPANYDEVNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・宛名会社部門名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 請求書発行部店名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "FROMORGNAME", LNM0014INProw("FROMORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・請求書発行部店名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 明細区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "MEISAICATEGORYID", LNM0014INProw("MEISAICATEGORYID"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・明細区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 勘定科目コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ACCOUNTCODE", LNM0014INProw("ACCOUNTCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・勘定科目コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 勘定科目名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ACCOUNTNAME", LNM0014INProw("ACCOUNTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・勘定科目名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' セグメントコード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SEGMENTCODE", LNM0014INProw("SEGMENTCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・セグメントコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' セグメント名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SEGMENTNAME", LNM0014INProw("SEGMENTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・セグメント名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 割合JOT(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "JOTPERCENTAGE", LNM0014INProw("JOTPERCENTAGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・割合JOTエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 割合ENEX(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ENEXPERCENTAGE", LNM0014INProw("ENEXPERCENTAGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・割合ENEXエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 備考1(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIKOU1", LNM0014INProw("BIKOU1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・備考1エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 備考2(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIKOU2", LNM0014INProw("BIKOU2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・備考2エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 備考3(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIKOU3", LNM0014INProw("BIKOU3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・備考3エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '割合JOT、割合ENEX合計値チェック
            If Not String.IsNullOrEmpty(LNM0014INProw("JOTPERCENTAGE")) Or
                    Not String.IsNullOrEmpty(LNM0014INProw("ENEXPERCENTAGE")) Then
                Dim WW_Decimal As Decimal
                Dim WW_JOTPERCENTAGE As Double
                Dim WW_ENEXPERCENTAGE As Double
                Dim WW_TOTALPERCENTAGE As Double

                If Decimal.TryParse(LNM0014INProw("JOTPERCENTAGE").ToString, WW_Decimal) Then
                    WW_JOTPERCENTAGE = WW_Decimal
                Else
                    WW_JOTPERCENTAGE = 0
                End If
                If Decimal.TryParse(LNM0014INProw("ENEXPERCENTAGE").ToString, WW_Decimal) Then
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
            If Not String.IsNullOrEmpty(work.WF_SEL_BIGCATECODE.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                                    work.WF_SEL_TARGETYM.Text, work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text,
                                    work.WF_SEL_BIGCATECODE.Text, work.WF_SEL_MIDCATECODE.Text, work.WF_SEL_SMALLCATECODE.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（対象年月 & 取引先コード & 部門コード & 大分類コード & 中分類コード & 小分類コード）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0014INProw("TARGETYM") & "]" &
                                           "([" & LNM0014INProw("TORICODE") & "]" &
                                           "([" & LNM0014INProw("ORGCODE") & "]" &
                                           "([" & LNM0014INProw("BIGCATECODE") & "]" &
                                           "([" & LNM0014INProw("MIDCATECODE") & "]" &
                                           " [" & LNM0014INProw("SMALLCATECODE") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If

            If String.IsNullOrEmpty(WW_LineErr) Then
                If LNM0014INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0014INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0014INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0014tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0014tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0014row As DataRow In LNM0014tbl.Rows
            Select Case LNM0014row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0014row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0014INProw As DataRow In LNM0014INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0014INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0014INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0014row As DataRow In LNM0014tbl.Rows
                ' KEY項目が等しい時
                If LNM0014row("TARGETYM") = LNM0014INProw("TARGETYM") AndAlso
                    LNM0014row("TORICODE") = LNM0014INProw("TORICODE") AndAlso
                    LNM0014row("ORGCODE") = LNM0014INProw("ORGCODE") AndAlso
                    LNM0014row("BIGCATECODE") = LNM0014INProw("BIGCATECODE") AndAlso
                    LNM0014row("MIDCATECODE") = LNM0014INProw("MIDCATECODE") AndAlso
                    LNM0014row("SMALLCATECODE") = LNM0014INProw("SMALLCATECODE") Then
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0014row("DELFLG") = LNM0014INProw("DELFLG") AndAlso
                        LNM0014row("TORINAME") = LNM0014INProw("TORINAME") AndAlso
                        LNM0014row("ORGNAME") = LNM0014INProw("ORGNAME") AndAlso
                        LNM0014row("KASANORGCODE") = LNM0014INProw("KASANORGCODE") AndAlso
                        LNM0014row("KASANORGNAME") = LNM0014INProw("KASANORGNAME") AndAlso
                        LNM0014row("BIGCATENAME") = LNM0014INProw("BIGCATENAME") AndAlso
                        LNM0014row("MIDCATENAME") = LNM0014INProw("MIDCATENAME") AndAlso
                        LNM0014row("SMALLCATENAME") = LNM0014INProw("SMALLCATENAME") AndAlso
                        LNM0014row("TANKA") = LNM0014INProw("TANKA") AndAlso
                        LNM0014row("QUANTITY") = LNM0014INProw("QUANTITY") AndAlso
                        LNM0014row("CALCUNIT") = LNM0014INProw("CALCUNIT") AndAlso
                        LNM0014row("DEPARTURE") = LNM0014INProw("DEPARTURE") AndAlso
                        LNM0014row("MILEAGE") = LNM0014INProw("MILEAGE") AndAlso
                        LNM0014row("SHIPPINGCOUNT") = LNM0014INProw("SHIPPINGCOUNT") AndAlso
                        LNM0014row("NENPI") = LNM0014INProw("NENPI") AndAlso
                        LNM0014row("DIESELPRICECURRENT") = LNM0014INProw("DIESELPRICECURRENT") AndAlso
                        LNM0014row("DIESELPRICESTANDARD") = LNM0014INProw("DIESELPRICESTANDARD") AndAlso
                        LNM0014row("DIESELCONSUMPTION") = LNM0014INProw("DIESELCONSUMPTION") AndAlso
                        LNM0014row("DISPLAYFLG") = LNM0014INProw("DISPLAYFLG") AndAlso
                        LNM0014row("ASSESSMENTFLG") = LNM0014INProw("ASSESSMENTFLG") AndAlso
                        LNM0014row("ATENACOMPANYNAME") = LNM0014INProw("ATENACOMPANYNAME") AndAlso
                        LNM0014row("ATENACOMPANYDEVNAME") = LNM0014INProw("ATENACOMPANYDEVNAME") AndAlso
                        LNM0014row("FROMORGNAME") = LNM0014INProw("FROMORGNAME") AndAlso
                        LNM0014row("MEISAICATEGORYID") = LNM0014INProw("MEISAICATEGORYID") AndAlso
                        LNM0014row("ACCOUNTCODE") = LNM0014INProw("ACCOUNTCODE") AndAlso                                '勘定科目コード
                        LNM0014row("ACCOUNTNAME") = LNM0014INProw("ACCOUNTNAME") AndAlso                                '勘定科目名
                        LNM0014row("SEGMENTCODE") = LNM0014INProw("SEGMENTCODE") AndAlso                                'セグメントコード
                        LNM0014row("SEGMENTNAME") = LNM0014INProw("SEGMENTNAME") AndAlso                                'セグメント名
                        LNM0014row("JOTPERCENTAGE") = LNM0014INProw("JOTPERCENTAGE") AndAlso                                '割合JOT
                        LNM0014row("ENEXPERCENTAGE") = LNM0014INProw("ENEXPERCENTAGE") AndAlso                                '割合ENEX
                        LNM0014row("BIKOU1") = LNM0014INProw("BIKOU1") AndAlso
                        LNM0014row("BIKOU2") = LNM0014INProw("BIKOU2") AndAlso
                        LNM0014row("BIKOU3") = LNM0014INProw("BIKOU3") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0014row("OPERATION")) Then

                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0014INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0014INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0014INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0014INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now
                Dim WW_DBDataCheck As String = ""

                Select Case True
                    Case LNM0014INPtbl.Rows(0)("BIGCATECODE").ToString = ""     '大分類コードが無い場合
                        '大分類コードを生成
                        LNM0014INPtbl.Rows(0)("BIGCATECODE") = LNM0014WRKINC.GenerateBigcateCode(SQLcon, LNM0014INPtbl.Rows(0), WW_DBDataCheck)
                        LNM0014INPtbl.Rows(0)("MIDCATECODE") = "1"
                        LNM0014INPtbl.Rows(0)("SMALLCATECODE") = "1"
                    Case LNM0014INPtbl.Rows(0)("MIDCATECODE").ToString = ""     '中分類コードが無い場合
                        '中分類コードを生成
                        LNM0014INPtbl.Rows(0)("MIDCATECODE") = LNM0014WRKINC.GenerateMidcateCode(SQLcon, LNM0014INPtbl.Rows(0), WW_DBDataCheck)
                    Case LNM0014INPtbl.Rows(0)("SMALLCATECODE").ToString = ""   '小分類コードが無い場合
                        '小分類コードを生成
                        LNM0014INPtbl.Rows(0)("SMALLCATECODE") = LNM0014WRKINC.GenerateSmallcateCode(SQLcon, LNM0014INPtbl.Rows(0), WW_DBDataCheck)
                    Case Else '大分類コード、中分類コード、小分類コードが設定されている場合は何もしない
                End Select

                '変更チェック
                MASTEREXISTS(SQLcon, WW_MODIFYKBN)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0014WRKINC.MODIFYKBN.AFTDATA
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
        For Each LNM0014INProw As DataRow In LNM0014INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0014row As DataRow In LNM0014tbl.Rows
                ' 同一レコードか判定
                If LNM0014INProw("TARGETYM") = LNM0014row("TARGETYM") AndAlso
                    LNM0014INProw("TORICODE") = LNM0014row("TORICODE") AndAlso
                    LNM0014INProw("ORGCODE") = LNM0014row("ORGCODE") AndAlso
                    LNM0014INProw("BIGCATECODE") = LNM0014row("BIGCATECODE") AndAlso
                    LNM0014INProw("MIDCATECODE") = LNM0014row("MIDCATECODE") AndAlso
                    LNM0014INProw("SMALLCATECODE") = LNM0014row("SMALLCATECODE") Then
                    ' 画面入力テーブル項目設定
                    LNM0014INProw("LINECNT") = LNM0014row("LINECNT")
                    LNM0014INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0014INProw("UPDTIMSTP") = LNM0014row("UPDTIMSTP")
                    LNM0014INProw("SELECT") = 0
                    LNM0014INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0014row.ItemArray = LNM0014INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0014tbl.NewRow
                WW_NRow.ItemArray = LNM0014INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0014tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0014tbl.Rows.Add(WW_NRow)
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
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Case "TODOKECODE"        '届先コード
                '    work.CODENAMEGetTODOKE(SQLcon, WW_NAMEht)
#End Region
                Case "BIGCATENAME"          '大分類名
                    work.IDGetBIGCATE(SQLcon, WW_NAMEht)
                Case "MIDCATENAME"          '中分類名
                    work.IDGetMIDCATE(SQLcon, WW_NAMEht)
                Case "SMALLCATENAME"        '小分類名
                    work.IDGetSMALLCATE(SQLcon, WW_NAMEht)
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
#Region "コメント-2025/07/30(分類追加対応のため)"
                'Case "TODOKECODE"         '届先コード
                '    If WW_NAMEht.ContainsKey(I_VALUE) Then
                '        O_TEXT = WW_NAMEht(I_VALUE)
                '    End If
#End Region
                Case "BIGCATENAME"        '大分類コード
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
                Case "MIDCATENAME"        '中分類コード
                    If WW_NAMEht.ContainsKey(I_VALUE) Then
                        O_TEXT = WW_NAMEht(I_VALUE)
                    End If
                Case "SMALLCATENAME"      '小分類コード
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
