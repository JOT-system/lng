''************************************************************
' 単価マスタメンテ登録画面
' 作成日 2024/12/16
' 更新日 
' 作成者 大浜
' 更新者 
'
' 修正履歴 : 2024/12/16 新規作成
'          : 2025/05/23 統合版に変更
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 単価マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0006TankaDetail
    Inherits Page

    '○ 検索結果格納Table
    Private LNM0006tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0006INPtbl As DataTable                              'チェック用テーブル
    Private LNM0006UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0006tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_ButtonCLEAR", "LNM0006L", "LNM0006S"  '戻るボタン押下（LNM0006L、LNM0006Sは、パンくずより）
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
                        Case "WF_TORIChange"            '取引先名チェンジ
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
                        Case "WF_ORGChange"             '部門コードチェンジ
                            'WF_ORGCODE_TEXT.Text = WF_ORG.SelectedValue
                            WF_SelectFIELD_CHANGE(WF_ButtonClick.Value)
                        Case "WF_KASANORGChange"        '加算先部門コードチェンジ
                            'WF_KASANORGCODE_TEXT.Text = WF_KASANORG.SelectedValue
                            WF_SelectFIELD_CHANGE(WF_ButtonClick.Value)
                        Case "WF_AVOCADOSHUKAChange"    '出荷場所名チェンジ
                            Dim WW_HT As New Hashtable
                            For index As Integer = 0 To WF_AVOCADOSHUKA.Items.Count - 1
                                WW_HT.Add(WF_AVOCADOSHUKA.Items(index).Text, WF_AVOCADOSHUKA.Items(index).Value)
                                If WF_AVOCADOSHUKA.Items(index).Text = WF_AVOCADOSHUKANAME.Text Then
                                    WF_AVOCADOSHUKA.SelectedValue = WF_AVOCADOSHUKA.Items(index).Value
                                    WF_AVOCADOSHUKA.SelectedIndex = index
                                End If
                            Next

                            If WW_HT.ContainsKey(WF_AVOCADOSHUKANAME.Text) Then
                                WF_AVOCADOSHUKABASHO_TEXT.Text = WW_HT(WF_AVOCADOSHUKANAME.Text)
                            Else
                                WF_AVOCADOSHUKABASHO_TEXT.Text = ""
                            End If
                            WF_SelectFIELD_CHANGE(WF_ButtonClick.Value)
                        Case "WF_AVOCADOTODOKEChange"    '実績届先名チェンジ
                            Dim WW_HT As New Hashtable
                            For index As Integer = 0 To WF_AVOCADOTODOKE.Items.Count - 1
                                Try
                                    WW_HT.Add(WF_AVOCADOTODOKE.Items(index).Text, WF_AVOCADOTODOKE.Items(index).Value)
                                    If WF_AVOCADOTODOKE.Items(index).Text = WF_AVOCADOTODOKENAME.Text Then
                                        WF_AVOCADOTODOKE.SelectedValue = WF_AVOCADOTODOKE.Items(index).Value
                                        WF_AVOCADOTODOKE.SelectedIndex = index
                                    End If
                                Catch ex As Exception
                                End Try
                            Next

                            If WW_HT.ContainsKey(WF_AVOCADOTODOKENAME.Text) Then
                                WF_AVOCADOTODOKECODE_TEXT.Text = WW_HT(WF_AVOCADOTODOKENAME.Text)
                            Else
                                WF_AVOCADOTODOKECODE_TEXT.Text = ""
                            End If
                        Case "WF_SelectCALENDARChange" 'カレンダーチェンジ
                            WF_ACCOUNTCODE_TEXT.Text = ""
                            WF_SEGMENTCODE_TEXT.Text = ""
                            '勘定科目
                            Me.WF_ACCOUNT.Items.Clear()
                            Me.WF_ACCOUNT.Items.Add("")
                            Dim retAccountList As New DropDownList
                            retAccountList = LNM0006WRKINC.getDowpDownAccountList(WF_StYMD.Value)
                            For index As Integer = 0 To retAccountList.Items.Count - 1
                                WF_ACCOUNT.Items.Add(New ListItem(retAccountList.Items(index).Text, retAccountList.Items(index).Value))
                            Next

                            'セグメント
                            Me.WF_SEGMENT.Items.Clear()
                        Case "WF_ACCOUNTChange" '勘定科目チェンジ
                            WF_ACCOUNTCODE_TEXT.Text = WF_ACCOUNT.SelectedValue

                            'セグメント
                            Me.WF_SEGMENT.Items.Clear()
                            WF_SEGMENTCODE_TEXT.Text = ""
                            Dim retSegmentList As New DropDownList
                            retSegmentList = LNM0006WRKINC.getDowpDownSegmentList(WF_StYMD.Value, WF_ACCOUNT.SelectedValue)

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
            If Not IsNothing(LNM0006tbl) Then
                LNM0006tbl.Clear()
                LNM0006tbl.Dispose()
                LNM0006tbl = Nothing
            End If

            If Not IsNothing(LNM0006INPtbl) Then
                LNM0006INPtbl.Clear()
                LNM0006INPtbl.Dispose()
                LNM0006INPtbl = Nothing
            End If

            If Not IsNothing(LNM0006UPDtbl) Then
                LNM0006UPDtbl.Clear()
                LNM0006UPDtbl.Dispose()
                LNM0006UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0006WRKINC.MAPIDD
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
        retToriList = LNM0006WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG, work.WF_SEL_TARGETYMD_L.Text, I_CREATEFLG:=True)
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
        Dim retOrgList As New DropDownList
        retOrgList = LNM0006WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG, work.WF_SEL_TARGETYMD_L.Text, I_CREATEFLG:=True)

        If retOrgList.Items.Count > 0 Then
            '情シス、高圧ガス以外
            If LNM0006WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
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
        retKasanOrgList = LNM0006WRKINC.getDowpDownKasanOrgList(Master.MAPID, Master.ROLE_ORG, I_CREATEFLG:=True)
        For index As Integer = 0 To retKasanOrgList.Items.Count - 1
            WF_KASANORG.Items.Add(New ListItem(retKasanOrgList.Items(index).Text, retKasanOrgList.Items(index).Value))
        Next

        '出荷場所
        Me.WF_AVOCADOSHUKA.Items.Clear()
        Me.WF_AVOCADOSHUKA.Items.Add("")

        Dim retAvocadoshukaiList As New DropDownList
        retAvocadoshukaiList = LNM0006WRKINC.getDowpDownAvocadoshukaList(Master.MAPID, Master.ROLE_ORG, work.WF_SEL_TARGETYMD_L.Text, I_CREATEFLG:=True)
        For index As Integer = 0 To retAvocadoshukaiList.Items.Count - 1
            WF_AVOCADOSHUKA.Items.Add(New ListItem(retAvocadoshukaiList.Items(index).Text, retAvocadoshukaiList.Items(index).Value))
        Next

        'コンボボックス化
        Dim WF_AVOCADOSHUKA_OPTIONS As String = ""
        For index As Integer = 0 To retAvocadoshukaiList.Items.Count - 1
            WF_AVOCADOSHUKA_OPTIONS += "<option>" + retAvocadoshukaiList.Items(index).Text + "</option>"
        Next
        WF_AVOCADOSHUKA_DL.InnerHtml = WF_AVOCADOSHUKA_OPTIONS
        Me.WF_AVOCADOSHUKANAME.Attributes("list") = Me.WF_AVOCADOSHUKA_DL.ClientID

        '実績届先
        Me.WF_AVOCADOTODOKE.Items.Clear()
        Me.WF_AVOCADOTODOKE.Items.Add("")

        Dim retAvocadotodokeList As New DropDownList
        retAvocadotodokeList = LNM0006WRKINC.getDowpDownAvocadotodokeList(Master.MAPID, Master.ROLE_ORG, work.WF_SEL_TARGETYMD_L.Text, I_CREATEFLG:=True)
        For index As Integer = 0 To retAvocadotodokeList.Items.Count - 1
            WF_AVOCADOTODOKE.Items.Add(New ListItem(retAvocadotodokeList.Items(index).Text, retAvocadotodokeList.Items(index).Value))
        Next

        'コンボボックス化
        Dim WF_AVOCADOTODOKE_OPTIONS As String = ""
        For index As Integer = 0 To retAvocadotodokeList.Items.Count - 1
            WF_AVOCADOTODOKE_OPTIONS += "<option>" + retAvocadotodokeList.Items(index).Text + "</option>"
        Next
        WF_AVOCADOTODOKE_DL.InnerHtml = WF_AVOCADOTODOKE_OPTIONS
        Me.WF_AVOCADOTODOKENAME.Attributes("list") = Me.WF_AVOCADOTODOKE_DL.ClientID

        '勘定科目
        Me.WF_ACCOUNT.Items.Clear()
        Me.WF_ACCOUNT.Items.Add("")
        Dim retAccountList As New DropDownList
        retAccountList = LNM0006WRKINC.getDowpDownAccountList(work.WF_SEL_STYMD.Text)
        For index As Integer = 0 To retAccountList.Items.Count - 1
            WF_ACCOUNT.Items.Add(New ListItem(retAccountList.Items(index).Text, retAccountList.Items(index).Value))
        Next

        'セグメント
        Me.WF_SEGMENT.Items.Clear()
        Dim retSegmentList As New DropDownList
        retSegmentList = LNM0006WRKINC.getDowpDownSegmentList(work.WF_SEL_STYMD.Text, work.WF_SEL_ACCOUNTCODE.Text)

        If retSegmentList.Items.Count > 1 Then
            Me.WF_SEGMENT.Items.Add("")
        End If

        For index As Integer = 0 To retSegmentList.Items.Count - 1
            WF_SEGMENT.Items.Add(New ListItem(retSegmentList.Items(index).Text, retSegmentList.Items(index).Value))
        Next

        '計算区分ドロップダウンのクリア
        Me.ddlSelectCALCKBN.Items.Clear()
        Me.ddlSelectCALCKBN.Items.Add("")

        '計算区分ドロップダウンの生成
        Dim retCALCKBNList As DropDownList = CmnLng.getDowpDownFixedList(Master.USERCAMP, "CALCKBNDROP")
        If retCALCKBNList.Items.Count > 0 Then
            For index As Integer = 0 To retCALCKBNList.Items.Count - 1
                ddlSelectCALCKBN.Items.Add(New ListItem(retCALCKBNList.Items(index).Text, retCALCKBNList.Items(index).Value))
            Next
        End If

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0006L Then
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
        WF_KASANORG_SAVE.Value = work.WF_SEL_KASANORGCODE.Text

        '実績出荷場所コード、名称
        WF_AVOCADOSHUKABASHO_TEXT.Text = work.WF_SEL_AVOCADOSHUKABASHO.Text
        WF_AVOCADOSHUKANAME.Text = work.WF_SEL_AVOCADOSHUKANAME.Text
        WF_AVOCADOSHUKABASHO_TEXT_SAVE.Value = work.WF_SEL_AVOCADOSHUKABASHO.Text
        WF_AVOCADOSHUKANAME_SAVE.Value = work.WF_SEL_AVOCADOSHUKANAME.Text

        '変換後出荷場所コード
        TxtSHUKABASHO.Text = work.WF_SEL_SHUKABASHO.Text
        '変換後出荷場所名称
        TxtSHUKANAME.Text = work.WF_SEL_SHUKANAME.Text

        '実績届先コード、名称
        WF_AVOCADOTODOKECODE_TEXT.Text = work.WF_SEL_AVOCADOTODOKECODE.Text
        WF_AVOCADOTODOKENAME.Text = work.WF_SEL_AVOCADOTODOKENAME.Text
        WF_AVOCADOTODOKECODE_TEXT_SAVE.Value = work.WF_SEL_AVOCADOTODOKECODE.Text
        WF_AVOCADOTODOKENAME_SAVE.Value = work.WF_SEL_AVOCADOTODOKENAME.Text

        '変換後届先コード
        TxtTODOKECODE.Text = work.WF_SEL_TODOKECODE.Text
        '変換後届先名称
        TxtTODOKENAME.Text = work.WF_SEL_TODOKENAME.Text
        '陸事番号
        TxtTANKNUMBER.Text = work.WF_SEL_TANKNUMBER.Text
        '車番
        TxtSHABAN.Text = work.WF_SEL_SHABAN.Text
        WF_SHABAN_SAVE.Value = work.WF_SEL_SHABAN.Text
        '有効開始日
        WF_StYMD.Value = work.WF_SEL_STYMD.Text
        'WF_STYMD_SAVE.Value = work.WF_SEL_STYMD.Text

        '有効終了日
        WF_EndYMD.Value = work.WF_SEL_ENDYMD.Text
        '枝番
        TxtBRANCHCODE.Text = work.WF_SEL_BRANCHCODE.Text
        WF_BRANCHCODE_SAVE.Value = work.WF_SEL_BRANCHCODE.Text
        '単価区分
        RadioTANKAKBN.SelectedValue = work.WF_SEL_TANKAKBN.Text
        '単価用途
        TxtMEMO.Text = work.WF_SEL_MEMO.Text
        '単価
        TxtTANKA.Text = work.WF_SEL_TANKA.Text
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

        '計算区分(KEYは固定値マスタのCALCKBNに合わせる)
        Select Case work.WF_SEL_CALCKBN.Text
            Case "トン" : ddlSelectCALCKBN.SelectedValue = "1"
            Case "回" : ddlSelectCALCKBN.SelectedValue = "2"
            Case "距離" : ddlSelectCALCKBN.SelectedValue = "3"
            Case "定数" : ddlSelectCALCKBN.SelectedValue = "9"
            Case Else : ddlSelectCALCKBN.SelectedValue = ""
        End Select

        '往復距離
        TxtROUNDTRIP.Text = work.WF_SEL_ROUNDTRIP.Text
        '通行料
        TxtTOLLFEE.Text = work.WF_SEL_TOLLFEE.Text
        '車型
        WF_SYAGATA.SelectedValue = work.WF_SEL_SYAGATA.Text
        WF_SYAGATA_CODE_TEXT.Text = work.WF_SEL_SYAGATA.Text
        WF_SYAGATA_SAVE.Value = work.WF_SEL_SYAGATA.Text

        '車腹
        TxtSYABARA.Text = work.WF_SEL_SYABARA.Text
        WF_SYABARA_SAVE.Value = work.WF_SEL_SYABARA.Text
        '備考1
        TxtBIKOU1.Text = work.WF_SEL_BIKOU1.Text
        '備考2
        TxtBIKOU2.Text = work.WF_SEL_BIKOU2.Text
        '備考3
        TxtBIKOU3.Text = work.WF_SEL_BIKOU3.Text

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_AVOCADOSHUKABASHO.Text

        '表示制御項目
        '情シス、高圧ガス以外の場合
        If LNM0006WRKINC.AdminCheck(Master.ROLE_ORG) = False Then
            VisibleKeyOrgCode.Value = ""
        Else
            VisibleKeyOrgCode.Value = Master.ROLE_ORG
        End If

        'チェックボックス状態(変換後出荷場所)
        If Not work.WF_SEL_SHUKABASHO.Text = "" Or Not work.WF_SEL_SHUKANAME.Text = "" Then
            WF_SHUKACHKSTATUS.Value = "true"
        End If
        'チェックボックス状態(変換後届先)
        If Not work.WF_SEL_TODOKECODE.Text = "" Or Not work.WF_SEL_TODOKENAME.Text = "" Then
            WF_TODOKECHKSTATUS.Value = "true"
        End If

        ' 取引先コード・実績出荷場所コード・変換後出荷場所コード・実績届先コード・変換後届先コード
        ' 枝番・単価を入力するテキストボックスは数値(0～9)のみ可能とする。
        Me.WF_TORICODE_TEXT.Attributes("onkeyPress") = "CheckNum()"
        Me.WF_AVOCADOSHUKABASHO_TEXT.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtSHUKABASHO.Attributes("onkeyPress") = "CheckNum()"
        Me.WF_AVOCADOTODOKECODE_TEXT.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTODOKECODE.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtBRANCHCODE.Attributes("onkeyPress") = "CheckNum()"
        Me.TxtTANKA.Attributes("onkeyPress") = "CheckNum()"

        ' 有効開始日・有効終了日を入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.WF_StYMD.Attributes("onkeyPress") = "CheckCalendar()"
        Me.WF_EndYMD.Attributes("onkeyPress") = "CheckCalendar()"

        ' 入力するテキストボックスは数値(0～9)＋記号(.)のみ可能とする。
        Me.TxtROUNDTRIP.Attributes("onkeyPress") = "CheckDeci()"             '往復距離
        Me.TxtTOLLFEE.Attributes("onkeyPress") = "CheckDeci()"             '通行料
        Me.TxtSYABARA.Attributes("onkeyPress") = "CheckDeci()"             '車腹
        Me.TxtJOTPERCENTAGE.Attributes("onkeyPress") = "CheckDeci()"       '割合JOT
        Me.TxtENEXPERCENTAGE.Attributes("onkeyPress") = "CheckDeci()"      '割合ENEX

        '○ サイドメニューへの値設定
        leftmenu.COMPCODE = Master.USERCAMP
        leftmenu.ROLEMENU = Master.ROLE_MENU
    End Sub

    ''' <summary>
    ''' 単価マスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(単価マスタ)
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("     INSERT INTO LNG.LNM0006_NEWTANKA        ")
        SQLStr.AppendLine("        (                                    ")
        SQLStr.AppendLine("         DELFLG                              ")
        SQLStr.AppendLine("       , TORICODE                            ")
        SQLStr.AppendLine("       , TORINAME                            ")
        SQLStr.AppendLine("       , ORGCODE                             ")
        SQLStr.AppendLine("       , ORGNAME                             ")
        SQLStr.AppendLine("       , KASANORGCODE                        ")
        SQLStr.AppendLine("       , KASANORGNAME                        ")
        SQLStr.AppendLine("       , AVOCADOSHUKABASHO                   ")
        SQLStr.AppendLine("       , AVOCADOSHUKANAME                    ")
        SQLStr.AppendLine("       , SHUKABASHO                          ")
        SQLStr.AppendLine("       , SHUKANAME                           ")
        SQLStr.AppendLine("       , AVOCADOTODOKECODE                   ")
        SQLStr.AppendLine("       , AVOCADOTODOKENAME                   ")
        SQLStr.AppendLine("       , TODOKECODE                          ")
        SQLStr.AppendLine("       , TODOKENAME                          ")
        SQLStr.AppendLine("       , TANKNUMBER                          ")
        SQLStr.AppendLine("       , SHABAN                              ")
        SQLStr.AppendLine("       , STYMD                               ")
        SQLStr.AppendLine("       , ENDYMD                              ")
        SQLStr.AppendLine("       , BRANCHCODE                          ")
        SQLStr.AppendLine("       , TANKAKBN                            ")
        SQLStr.AppendLine("       , MEMO                                ")
        SQLStr.AppendLine("       , TANKA                               ")
        SQLStr.AppendLine("       , ACCOUNTCODE                         ")
        SQLStr.AppendLine("       , ACCOUNTNAME                         ")
        SQLStr.AppendLine("       , SEGMENTCODE                         ")
        SQLStr.AppendLine("       , SEGMENTNAME                         ")
        SQLStr.AppendLine("       , JOTPERCENTAGE                       ")
        SQLStr.AppendLine("       , ENEXPERCENTAGE                      ")
        SQLStr.AppendLine("       , CALCKBN                             ")
        SQLStr.AppendLine("       , ROUNDTRIP                           ")
        SQLStr.AppendLine("       , TOLLFEE                             ")
        SQLStr.AppendLine("       , SYAGATA                             ")
        SQLStr.AppendLine("       , SYAGATANAME                         ")
        SQLStr.AppendLine("       , SYABARA                             ")
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
        SQLStr.AppendLine("       , @AVOCADOSHUKABASHO                  ")
        SQLStr.AppendLine("       , @AVOCADOSHUKANAME                   ")
        SQLStr.AppendLine("       , @SHUKABASHO                         ")
        SQLStr.AppendLine("       , @SHUKANAME                          ")
        SQLStr.AppendLine("       , @AVOCADOTODOKECODE                  ")
        SQLStr.AppendLine("       , @AVOCADOTODOKENAME                  ")
        SQLStr.AppendLine("       , @TODOKECODE                         ")
        SQLStr.AppendLine("       , @TODOKENAME                         ")
        SQLStr.AppendLine("       , @TANKNUMBER                         ")
        SQLStr.AppendLine("       , @SHABAN                             ")
        SQLStr.AppendLine("       , @STYMD                              ")
        SQLStr.AppendLine("       , @ENDYMD                             ")
        SQLStr.AppendLine("       , @BRANCHCODE                         ")
        SQLStr.AppendLine("       , @TANKAKBN                           ")
        SQLStr.AppendLine("       , @MEMO                               ")
        SQLStr.AppendLine("       , @TANKA                              ")
        SQLStr.AppendLine("       , @ACCOUNTCODE                        ")
        SQLStr.AppendLine("       , @ACCOUNTNAME                        ")
        SQLStr.AppendLine("       , @SEGMENTCODE                        ")
        SQLStr.AppendLine("       , @SEGMENTNAME                        ")
        SQLStr.AppendLine("       , @JOTPERCENTAGE                      ")
        SQLStr.AppendLine("       , @ENEXPERCENTAGE                     ")
        SQLStr.AppendLine("       , @CALCKBN                            ")
        SQLStr.AppendLine("       , @ROUNDTRIP                          ")
        SQLStr.AppendLine("       , @TOLLFEE                            ")
        SQLStr.AppendLine("       , @SYAGATA                            ")
        SQLStr.AppendLine("       , @SYAGATANAME                        ")
        SQLStr.AppendLine("       , @SYABARA                            ")
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
        SQLStr.AppendLine("       , TORICODE     = @TORICODE                            ")
        SQLStr.AppendLine("       , TORINAME     = @TORINAME                            ")
        SQLStr.AppendLine("       , ORGCODE     = @ORGCODE                            ")
        SQLStr.AppendLine("       , ORGNAME     = @ORGNAME                            ")
        SQLStr.AppendLine("       , KASANORGCODE     = @KASANORGCODE                            ")
        SQLStr.AppendLine("       , KASANORGNAME     = @KASANORGNAME                            ")
        SQLStr.AppendLine("       , AVOCADOSHUKABASHO     = @AVOCADOSHUKABASHO                            ")
        SQLStr.AppendLine("       , AVOCADOSHUKANAME     = @AVOCADOSHUKANAME                            ")
        SQLStr.AppendLine("       , SHUKABASHO     = @SHUKABASHO                            ")
        SQLStr.AppendLine("       , SHUKANAME     = @SHUKANAME                            ")
        SQLStr.AppendLine("       , AVOCADOTODOKECODE     = @AVOCADOTODOKECODE                            ")
        SQLStr.AppendLine("       , AVOCADOTODOKENAME     = @AVOCADOTODOKENAME                            ")
        SQLStr.AppendLine("       , TODOKECODE     = @TODOKECODE                            ")
        SQLStr.AppendLine("       , TODOKENAME     = @TODOKENAME                            ")
        SQLStr.AppendLine("       , TANKNUMBER     = @TANKNUMBER                            ")
        SQLStr.AppendLine("       , SHABAN     = @SHABAN                            ")
        SQLStr.AppendLine("       , STYMD     = @STYMD                            ")
        SQLStr.AppendLine("       , ENDYMD     = @ENDYMD                            ")
        SQLStr.AppendLine("       , BRANCHCODE     = @BRANCHCODE                            ")
        SQLStr.AppendLine("       , TANKAKBN     = @TANKAKBN                            ")
        SQLStr.AppendLine("       , MEMO     = @MEMO                            ")
        SQLStr.AppendLine("       , TANKA     = @TANKA                            ")
        SQLStr.AppendLine("       , ACCOUNTCODE =  @ACCOUNTCODE")
        SQLStr.AppendLine("       , ACCOUNTNAME =  @ACCOUNTNAME")
        SQLStr.AppendLine("       , SEGMENTCODE =  @SEGMENTCODE")
        SQLStr.AppendLine("       , SEGMENTNAME =  @SEGMENTNAME")
        SQLStr.AppendLine("       , JOTPERCENTAGE =  @JOTPERCENTAGE")
        SQLStr.AppendLine("       , ENEXPERCENTAGE =  @ENEXPERCENTAGE")
        SQLStr.AppendLine("       , CALCKBN     = @CALCKBN                            ")
        SQLStr.AppendLine("       , ROUNDTRIP     = @ROUNDTRIP                            ")
        SQLStr.AppendLine("       , TOLLFEE     = @TOLLFEE                            ")
        SQLStr.AppendLine("       , SYAGATA     = @SYAGATA                            ")
        SQLStr.AppendLine("       , SYAGATANAME     = @SYAGATANAME                            ")
        SQLStr.AppendLine("       , SYABARA     = @SYABARA                            ")
        SQLStr.AppendLine("       , BIKOU1     = @BIKOU1                            ")
        SQLStr.AppendLine("       , BIKOU2     = @BIKOU2                            ")
        SQLStr.AppendLine("       , BIKOU3     = @BIKOU3                            ")
        SQLStr.AppendLine("       , UPDYMD         = @UPDYMD            ")
        SQLStr.AppendLine("       , UPDUSER        = @UPDUSER           ")
        SQLStr.AppendLine("       , UPDTERMID      = @UPDTERMID         ")
        SQLStr.AppendLine("       , UPDPGID        = @UPDPGID           ")
        SQLStr.AppendLine("       , RECEIVEYMD     = @RECEIVEYMD        ")

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl = New StringBuilder
        SQLJnl.AppendLine(" SELECT                                     ")
        SQLJnl.AppendLine("     DELFLG                                 ")
        SQLJnl.AppendLine("   , TORICODE                              ")
        SQLJnl.AppendLine("   , TORINAME                              ")
        SQLJnl.AppendLine("   , ORGCODE                              ")
        SQLJnl.AppendLine("   , ORGNAME                              ")
        SQLJnl.AppendLine("   , KASANORGCODE                              ")
        SQLJnl.AppendLine("   , KASANORGNAME                              ")
        SQLJnl.AppendLine("   , AVOCADOSHUKABASHO                              ")
        SQLJnl.AppendLine("   , AVOCADOSHUKANAME                              ")
        SQLJnl.AppendLine("   , SHUKABASHO                              ")
        SQLJnl.AppendLine("   , SHUKANAME                              ")
        SQLJnl.AppendLine("   , AVOCADOTODOKECODE                              ")
        SQLJnl.AppendLine("   , AVOCADOTODOKENAME                              ")
        SQLJnl.AppendLine("   , TODOKECODE                              ")
        SQLJnl.AppendLine("   , TODOKENAME                              ")
        SQLJnl.AppendLine("   , TANKNUMBER                              ")
        SQLJnl.AppendLine("   , SHABAN                              ")
        SQLJnl.AppendLine("   , STYMD                              ")
        SQLJnl.AppendLine("   , ENDYMD                              ")
        SQLJnl.AppendLine("   , BRANCHCODE                              ")
        SQLJnl.AppendLine("   , TANKAKBN                              ")
        SQLJnl.AppendLine("   , MEMO                              ")
        SQLJnl.AppendLine("   , TANKA                              ")
        SQLJnl.AppendLine("   , ACCOUNTCODE                         ")
        SQLJnl.AppendLine("   , ACCOUNTNAME                         ")
        SQLJnl.AppendLine("   , SEGMENTCODE                         ")
        SQLJnl.AppendLine("   , SEGMENTNAME                         ")
        SQLJnl.AppendLine("   , JOTPERCENTAGE                       ")
        SQLJnl.AppendLine("   , ENEXPERCENTAGE                      ")
        SQLJnl.AppendLine("   , CALCKBN                              ")
        SQLJnl.AppendLine("   , ROUNDTRIP                              ")
        SQLJnl.AppendLine("   , TOLLFEE                              ")
        SQLJnl.AppendLine("   , SYAGATA                              ")
        SQLJnl.AppendLine("   , SYAGATANAME                              ")
        SQLJnl.AppendLine("   , SYABARA                              ")
        SQLJnl.AppendLine("   , BIKOU1                              ")
        SQLJnl.AppendLine("   , BIKOU2                              ")
        SQLJnl.AppendLine("   , BIKOU3                              ")
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
        SQLJnl.AppendLine("     LNG.LNM0006_NEWTANKA                      ")
        SQLJnl.AppendLine(" WHERE                                      ")
        SQLJnl.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLJnl.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLJnl.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLJnl.AppendLine("    AND  COALESCE(AVOCADOSHUKABASHO, '')             = @AVOCADOSHUKABASHO ")
        SQLJnl.AppendLine("    AND  COALESCE(AVOCADOTODOKECODE, '')             = @AVOCADOTODOKECODE ")
        SQLJnl.AppendLine("    AND  COALESCE(SHABAN, '')             = @SHABAN ")
        SQLJnl.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLJnl.AppendLine("    AND  COALESCE(BRANCHCODE, '0')             = @BRANCHCODE ")
        SQLJnl.AppendLine("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        SQLJnl.AppendLine("    AND  COALESCE(SYABARA, '0')             = @SYABARA ")

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
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOSHUKANAME As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKANAME", MySqlDbType.VarChar, 20)     '実績出荷場所名称
                Dim P_SHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@SHUKABASHO", MySqlDbType.VarChar, 6)     '変換後出荷場所コード
                Dim P_SHUKANAME As MySqlParameter = SQLcmd.Parameters.Add("@SHUKANAME", MySqlDbType.VarChar, 20)     '変換後出荷場所名称
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_AVOCADOTODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKENAME", MySqlDbType.VarChar, 20)     '実績届先名称
                Dim P_TODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@TODOKECODE", MySqlDbType.VarChar, 6)     '変換後届先コード
                Dim P_TODOKENAME As MySqlParameter = SQLcmd.Parameters.Add("@TODOKENAME", MySqlDbType.VarChar, 20)     '変換後届先名称
                Dim P_TANKNUMBER As MySqlParameter = SQLcmd.Parameters.Add("@TANKNUMBER", MySqlDbType.VarChar, 20)     '陸事番号
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2)     '枝番
                Dim P_TANKAKBN As MySqlParameter = SQLcmd.Parameters.Add("@TANKAKBN", MySqlDbType.VarChar, 1)     '単価区分
                Dim P_MEMO As MySqlParameter = SQLcmd.Parameters.Add("@MEMO", MySqlDbType.VarChar, 50)     '単価用途
                Dim P_TANKA As MySqlParameter = SQLcmd.Parameters.Add("@TANKA", MySqlDbType.Decimal)         '単価
                Dim P_ACCOUNTCODE As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTCODE", MySqlDbType.Decimal, 8)     '勘定科目コード
                Dim P_ACCOUNTNAME As MySqlParameter = SQLcmd.Parameters.Add("@ACCOUNTNAME", MySqlDbType.VarChar, 100)     '勘定科目名
                Dim P_SEGMENTCODE As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTCODE", MySqlDbType.Decimal, 5)     'セグメントコード
                Dim P_SEGMENTNAME As MySqlParameter = SQLcmd.Parameters.Add("@SEGMENTNAME", MySqlDbType.VarChar, 100)     'セグメント名
                Dim P_JOTPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@JOTPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合JOT
                Dim P_ENEXPERCENTAGE As MySqlParameter = SQLcmd.Parameters.Add("@ENEXPERCENTAGE", MySqlDbType.Decimal, 5, 2)     '割合ENEX
                Dim P_CALCKBN As MySqlParameter = SQLcmd.Parameters.Add("@CALCKBN", MySqlDbType.VarChar, 2)     '計算区分
                Dim P_ROUNDTRIP As MySqlParameter = SQLcmd.Parameters.Add("@ROUNDTRIP", MySqlDbType.Decimal, 5, 3)    '往復距離
                Dim P_TOLLFEE As MySqlParameter = SQLcmd.Parameters.Add("@TOLLFEE", MySqlDbType.Decimal, 8, 3)        '通行料
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYAGATANAME As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATANAME", MySqlDbType.VarChar, 50)     '車型名
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
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
                Dim JP_TORICODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim JP_ORGCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim JP_KASANORGCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim JP_AVOCADOSHUKABASHO As MySqlParameter = SQLcmdJnl.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim JP_AVOCADOTODOKECODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim JP_SHABAN As MySqlParameter = SQLcmdJnl.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim JP_STYMD As MySqlParameter = SQLcmdJnl.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim JP_BRANCHCODE As MySqlParameter = SQLcmdJnl.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim JP_SYAGATA As MySqlParameter = SQLcmdJnl.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim JP_SYABARA As MySqlParameter = SQLcmdJnl.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹

                Dim LNM0006row As DataRow = LNM0006INPtbl.Rows(0)

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                P_DELFLG.Value = LNM0006row("DELFLG")               '削除フラグ

                P_TORICODE.Value = LNM0006row("TORICODE")           '取引先コード
                P_TORINAME.Value = LNM0006row("TORINAME")           '取引先名称
                P_ORGCODE.Value = LNM0006row("ORGCODE")           '部門コード
                P_ORGNAME.Value = LNM0006row("ORGNAME")           '部門名称
                P_KASANORGCODE.Value = LNM0006row("KASANORGCODE")           '加算先部門コード
                P_KASANORGNAME.Value = LNM0006row("KASANORGNAME")           '加算先部門名称
                P_AVOCADOSHUKABASHO.Value = LNM0006row("AVOCADOSHUKABASHO")           '実績出荷場所コード
                P_AVOCADOSHUKANAME.Value = LNM0006row("AVOCADOSHUKANAME")           '実績出荷場所名称
                P_SHUKABASHO.Value = LNM0006row("SHUKABASHO")           '変換後出荷場所コード
                P_SHUKANAME.Value = LNM0006row("SHUKANAME")           '変換後出荷場所名称
                P_AVOCADOTODOKECODE.Value = LNM0006row("AVOCADOTODOKECODE")           '実績届先コード
                P_AVOCADOTODOKENAME.Value = LNM0006row("AVOCADOTODOKENAME")           '実績届先名称
                P_TODOKECODE.Value = LNM0006row("TODOKECODE")           '変換後届先コード
                P_TODOKENAME.Value = LNM0006row("TODOKENAME")           '変換後届先名称
                P_TANKNUMBER.Value = LNM0006row("TANKNUMBER")           '陸事番号
                P_SHABAN.Value = LNM0006row("SHABAN")           '車番
                P_STYMD.Value = LNM0006row("STYMD")           '有効開始日

                '有効終了日(画面入力済みの場合画面入力を優先)
                If Not WF_EndYMD.Value = "" Then
                    P_ENDYMD.Value = LNM0006row("ENDYMD")
                Else
                    P_ENDYMD.Value = WF_AUTOENDYMD.Value
                End If

                P_BRANCHCODE.Value = LNM0006row("BRANCHCODE")           '枝番
                P_TANKAKBN.Value = LNM0006row("TANKAKBN")           '単価区分
                P_MEMO.Value = LNM0006row("MEMO")           '単価用途

                '単価
                If LNM0006row("TANKA").ToString = "" Then
                    P_TANKA.Value = "0"
                Else
                    P_TANKA.Value = LNM0006row("TANKA")
                End If

                '勘定科目コード
                If LNM0006row("ACCOUNTCODE").ToString = "" Then
                    P_ACCOUNTCODE.Value = DBNull.Value
                Else
                    P_ACCOUNTCODE.Value = LNM0006row("ACCOUNTCODE")
                End If

                P_ACCOUNTNAME.Value = LNM0006row("ACCOUNTNAME")           '勘定科目名

                'セグメントコード
                If LNM0006row("SEGMENTCODE").ToString = "" Then
                    P_SEGMENTCODE.Value = DBNull.Value
                Else
                    P_SEGMENTCODE.Value = LNM0006row("SEGMENTCODE")
                End If

                P_SEGMENTNAME.Value = LNM0006row("SEGMENTNAME")           'セグメント名

                '割合JOT
                If LNM0006row("JOTPERCENTAGE").ToString = "" Then
                    P_JOTPERCENTAGE.Value = DBNull.Value
                Else
                    P_JOTPERCENTAGE.Value = LNM0006row("JOTPERCENTAGE")
                End If

                '割合ENEX
                If LNM0006row("ENEXPERCENTAGE").ToString = "" Then
                    P_ENEXPERCENTAGE.Value = DBNull.Value
                Else
                    P_ENEXPERCENTAGE.Value = LNM0006row("ENEXPERCENTAGE")
                End If

                '計算区分
                Select Case LNM0006row("CALCKBN").ToString
                    Case "1" : P_CALCKBN.Value = "トン"
                    Case "2" : P_CALCKBN.Value = "回"
                    Case "3" : P_CALCKBN.Value = "距離"
                    Case "9" : P_CALCKBN.Value = "定数"
                    Case Else : P_CALCKBN.Value = ""
                End Select

                '往復距離
                If LNM0006row("ROUNDTRIP").ToString = "0" Or LNM0006row("ROUNDTRIP").ToString = "" Then
                    P_ROUNDTRIP.Value = DBNull.Value
                Else
                    P_ROUNDTRIP.Value = LNM0006row("ROUNDTRIP")
                End If

                '通行料
                If LNM0006row("TOLLFEE").ToString = "0" Or LNM0006row("TOLLFEE").ToString = "" Then
                    P_TOLLFEE.Value = DBNull.Value
                Else
                    P_TOLLFEE.Value = LNM0006row("TOLLFEE")
                End If

                P_SYAGATA.Value = LNM0006row("SYAGATA")           '車型
                P_SYAGATANAME.Value = LNM0006row("SYAGATANAME")            '車型名

                '車腹
                If LNM0006row("SYABARA").ToString = "" Then
                    P_SYABARA.Value = "0"
                Else
                    P_SYABARA.Value = LNM0006row("SYABARA")
                End If

                P_BIKOU1.Value = LNM0006row("BIKOU1")           '備考1
                P_BIKOU2.Value = LNM0006row("BIKOU2")           '備考2
                P_BIKOU3.Value = LNM0006row("BIKOU3")           '備考3

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
                JP_TORICODE.Value = LNM0006row("TORICODE") '取引先コード
                JP_ORGCODE.Value = LNM0006row("ORGCODE") '部門コード
                JP_KASANORGCODE.Value = LNM0006row("KASANORGCODE") '加算先部門コード
                JP_AVOCADOSHUKABASHO.Value = LNM0006row("AVOCADOSHUKABASHO") '実績出荷場所コード
                JP_AVOCADOTODOKECODE.Value = LNM0006row("AVOCADOTODOKECODE") '実績届先コード
                JP_SHABAN.Value = LNM0006row("SHABAN") '車番
                JP_STYMD.Value = LNM0006row("STYMD") '有効開始日
                JP_BRANCHCODE.Value = LNM0006row("BRANCHCODE") '枝番
                JP_SYAGATA.Value = LNM0006row("SYAGATA") '車型
                JP_SYABARA.Value = LNM0006row("SYABARA") '車腹

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0006UPDtbl) Then
                        LNM0006UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0006UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0006UPDtbl.Clear()
                    LNM0006UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0006UPDrow As DataRow In LNM0006UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0006D"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0006UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0006D UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0006D UPDATE_INSERT"
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

        '単価マスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        TORICODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0006_NEWTANKA")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOSHUKABASHO, '')             = @AVOCADOSHUKABASHO ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOTODOKECODE, '')             = @AVOCADOTODOKECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SHABAN, '')             = @SHABAN ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.AppendLine("    AND  COALESCE(BRANCHCODE, '0')             = @BRANCHCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        SQLStr.AppendLine("    AND  COALESCE(SYABARA, '0')             = @SYABARA ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹

                Dim LNM0006row As DataRow = LNM0006INPtbl.Rows(0)

                P_TORICODE.Value = LNM0006row("TORICODE") '取引先コード
                P_ORGCODE.Value = LNM0006row("ORGCODE") '部門コード
                P_KASANORGCODE.Value = LNM0006row("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = LNM0006row("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = LNM0006row("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = LNM0006row("SHABAN") '車番
                P_STYMD.Value = LNM0006row("STYMD") '有効開始日
                P_BRANCHCODE.Value = LNM0006row("BRANCHCODE") '枝番
                P_SYAGATA.Value = LNM0006row("SYAGATA") '車型
                P_SYABARA.Value = LNM0006row("SYABARA") '車腹

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
                        WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0006C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0006C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0005_NEWTANKAHIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("      TORICODE  ")
        SQLStr.AppendLine("     ,TORINAME  ")
        SQLStr.AppendLine("     ,ORGCODE  ")
        SQLStr.AppendLine("     ,ORGNAME  ")
        SQLStr.AppendLine("     ,KASANORGCODE  ")
        SQLStr.AppendLine("     ,KASANORGNAME  ")
        SQLStr.AppendLine("     ,AVOCADOSHUKABASHO  ")
        SQLStr.AppendLine("     ,AVOCADOSHUKANAME  ")
        SQLStr.AppendLine("     ,SHUKABASHO  ")
        SQLStr.AppendLine("     ,SHUKANAME  ")
        SQLStr.AppendLine("     ,AVOCADOTODOKECODE  ")
        SQLStr.AppendLine("     ,AVOCADOTODOKENAME  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKENAME  ")
        SQLStr.AppendLine("     ,TANKNUMBER  ")
        SQLStr.AppendLine("     ,SHABAN  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,BRANCHCODE  ")
        SQLStr.AppendLine("     ,TANKAKBN  ")
        SQLStr.AppendLine("     ,MEMO  ")
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,ACCOUNTCODE  ")
        SQLStr.AppendLine("     ,ACCOUNTNAME  ")
        SQLStr.AppendLine("     ,SEGMENTCODE  ")
        SQLStr.AppendLine("     ,SEGMENTNAME  ")
        SQLStr.AppendLine("     ,JOTPERCENTAGE  ")
        SQLStr.AppendLine("     ,ENEXPERCENTAGE  ")
        SQLStr.AppendLine("     ,CALCKBN  ")
        SQLStr.AppendLine("     ,ROUNDTRIP  ")
        SQLStr.AppendLine("     ,TOLLFEE  ")
        SQLStr.AppendLine("     ,SYAGATA  ")
        SQLStr.AppendLine("     ,SYAGATANAME  ")
        SQLStr.AppendLine("     ,SYABARA  ")
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
        SQLStr.AppendLine("     ,AVOCADOSHUKABASHO  ")
        SQLStr.AppendLine("     ,AVOCADOSHUKANAME  ")
        SQLStr.AppendLine("     ,SHUKABASHO  ")
        SQLStr.AppendLine("     ,SHUKANAME  ")
        SQLStr.AppendLine("     ,AVOCADOTODOKECODE  ")
        SQLStr.AppendLine("     ,AVOCADOTODOKENAME  ")
        SQLStr.AppendLine("     ,TODOKECODE  ")
        SQLStr.AppendLine("     ,TODOKENAME  ")
        SQLStr.AppendLine("     ,TANKNUMBER  ")
        SQLStr.AppendLine("     ,SHABAN  ")
        SQLStr.AppendLine("     ,STYMD  ")
        SQLStr.AppendLine("     ,ENDYMD  ")
        SQLStr.AppendLine("     ,BRANCHCODE  ")
        SQLStr.AppendLine("     ,TANKAKBN  ")
        SQLStr.AppendLine("     ,MEMO  ")
        SQLStr.AppendLine("     ,TANKA  ")
        SQLStr.AppendLine("     ,ACCOUNTCODE  ")
        SQLStr.AppendLine("     ,ACCOUNTNAME  ")
        SQLStr.AppendLine("     ,SEGMENTCODE  ")
        SQLStr.AppendLine("     ,SEGMENTNAME  ")
        SQLStr.AppendLine("     ,JOTPERCENTAGE  ")
        SQLStr.AppendLine("     ,ENEXPERCENTAGE  ")
        SQLStr.AppendLine("     ,CALCKBN  ")
        SQLStr.AppendLine("     ,ROUNDTRIP  ")
        SQLStr.AppendLine("     ,TOLLFEE  ")
        SQLStr.AppendLine("     ,SYAGATA  ")
        SQLStr.AppendLine("     ,SYAGATANAME  ")
        SQLStr.AppendLine("     ,SYABARA  ")
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
        SQLStr.AppendLine("        LNG.LNM0006_NEWTANKA")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOSHUKABASHO, '')             = @AVOCADOSHUKABASHO ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOTODOKECODE, '')             = @AVOCADOTODOKECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SHABAN, '')             = @SHABAN ")
        SQLStr.AppendLine("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.AppendLine("    AND  COALESCE(BRANCHCODE, '0')             = @BRANCHCODE ")
        SQLStr.AppendLine("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        SQLStr.AppendLine("    AND  COALESCE(SYABARA, '0')             = @SYABARA ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0006row As DataRow = LNM0006INPtbl.Rows(0)

                ' DB更新
                P_TORICODE.Value = LNM0006row("TORICODE") '取引先コード
                P_ORGCODE.Value = LNM0006row("ORGCODE") '部門コード
                P_KASANORGCODE.Value = LNM0006row("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = LNM0006row("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = LNM0006row("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = LNM0006row("SHABAN") '車番
                P_STYMD.Value = LNM0006row("STYMD") '有効開始日
                P_BRANCHCODE.Value = LNM0006row("BRANCHCODE") '枝番
                P_SYAGATA.Value = LNM0006row("SYAGATA") '車型
                P_SYABARA.Value = LNM0006row("SYABARA") '車腹

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0006WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0006tbl.Rows(0)("DELFLG") = "0" And LNM0006row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0006WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0006WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0005_NEWTANKAHIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0005_NEWTANKAHIST  INSERT"
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
        SQLStr.Append("     LNG.LNM0006_NEWTANKA                    ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     ENDYMD               = @ENDYMD          ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.Append("    AND  COALESCE(AVOCADOSHUKABASHO, '')             = @AVOCADOSHUKABASHO ")
        SQLStr.Append("    AND  COALESCE(AVOCADOTODOKECODE, '')             = @AVOCADOTODOKECODE ")
        SQLStr.Append("    AND  COALESCE(SHABAN, '')             = @SHABAN ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.Append("    AND  COALESCE(BRANCHCODE, '0')             = @BRANCHCODE ")
        SQLStr.Append("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        SQLStr.Append("    AND  COALESCE(SYABARA, '0')             = @SYABARA ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_ENDYMD As MySqlParameter = SQLcmd.Parameters.Add("@ENDYMD", MySqlDbType.Date)     '有効終了日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                P_TORICODE.Value = WW_ROW("TORICODE") '取引先コード
                P_ORGCODE.Value = WW_ROW("ORGCODE") '部門コード
                P_KASANORGCODE.Value = WW_ROW("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = WW_ROW("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = WW_ROW("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = WW_ROW("SHABAN") '車番
                P_STYMD.Value = WW_ROW("STYMD") '有効開始日
                P_ENDYMD.Value = WW_ROW("ENDYMD") '有効終了日
                P_BRANCHCODE.Value = WW_ROW("BRANCHCODE") '枝番
                P_SYAGATA.Value = WW_ROW("SYAGATA") '車型
                P_SYABARA.Value = WW_ROW("SYABARA") '車腹
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0006_NEWTANKA UPDATE"
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
        DetailBoxToLNM0006INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0006tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0006tbl, work.WF_SEL_INPTBL.Text)

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
    Protected Sub DetailBoxToLNM0006INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(RadioDELFLG.SelectedValue)      '削除フラグ
        Master.EraseCharToIgnore(TxtSHUKABASHO.Text)  '変換後出荷場所コード
        Master.EraseCharToIgnore(TxtSHUKANAME.Text)  '変換後出荷場所名称
        Master.EraseCharToIgnore(TxtTODOKECODE.Text)  '変換後届先コード
        Master.EraseCharToIgnore(TxtTODOKENAME.Text)  '変換後届先名称
        Master.EraseCharToIgnore(TxtTANKNUMBER.Text)  '陸事番号
        Master.EraseCharToIgnore(TxtSHABAN.Text)  '車番
        Master.EraseCharToIgnore(TxtBRANCHCODE.Text)  '枝番
        Master.EraseCharToIgnore(TxtMEMO.Text)  '単価用途
        Master.EraseCharToIgnore(TxtTANKA.Text)  '単価
        Master.EraseCharToIgnore(TxtROUNDTRIP.Text)  '往復距離
        Master.EraseCharToIgnore(TxtTOLLFEE.Text)  '通行料
        Master.EraseCharToIgnore(TxtSYABARA.Text)  '車腹
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

        Master.CreateEmptyTable(LNM0006INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0006INProw As DataRow = LNM0006INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(TxtSelLineCNT.Text) Then
            LNM0006INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(TxtSelLineCNT.Text, LNM0006INProw("LINECNT"))
            Catch ex As Exception
                LNM0006INProw("LINECNT") = 0
            End Try
        End If

        LNM0006INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0006INProw("UPDTIMSTP") = 0
        LNM0006INProw("SELECT") = 1
        LNM0006INProw("HIDDEN") = 0

        LNM0006INProw("DELFLG") = RadioDELFLG.SelectedValue             '削除フラグ

        '更新の場合
        If Not DisabledKeyItem.Value = "" Then
            LNM0006INProw("TORICODE") = work.WF_SEL_TORICODE.Text           '取引先コード
            LNM0006INProw("TORINAME") = work.WF_SEL_TORINAME.Text             '取引先名称
            LNM0006INProw("ORGCODE") = work.WF_SEL_ORGCODE.Text          '部門コード
            LNM0006INProw("ORGNAME") = work.WF_SEL_ORGNAME.Text           '部門名称
            LNM0006INProw("KASANORGCODE") = work.WF_SEL_KASANORGCODE.Text           '加算先部門コード
            LNM0006INProw("KASANORGNAME") = work.WF_SEL_KASANORGNAME.Text            '加算先部門名称
            LNM0006INProw("AVOCADOSHUKABASHO") = work.WF_SEL_AVOCADOSHUKABASHO.Text            '実績出荷場所コード
            LNM0006INProw("AVOCADOSHUKANAME") = work.WF_SEL_AVOCADOSHUKANAME.Text           '実績出荷場所名称
            LNM0006INProw("AVOCADOTODOKECODE") = work.WF_SEL_AVOCADOTODOKECODE.Text           '実績届先コード
            LNM0006INProw("AVOCADOTODOKENAME") = work.WF_SEL_AVOCADOTODOKENAME.Text           '実績届先名称
            LNM0006INProw("SHABAN") = work.WF_SEL_SHABAN.Text           '車番
            'LNM0006INProw("STYMD") = work.WF_SEL_STYMD.Text          '有効開始日
            LNM0006INProw("BRANCHCODE") = work.WF_SEL_BRANCHCODE.Text         '枝番
            LNM0006INProw("SYAGATA") = work.WF_SEL_SYAGATA.Text           '車型
            LNM0006INProw("SYAGATANAME") = work.WF_SEL_SYAGATANAME.Text          '車型名
            LNM0006INProw("SYABARA") = work.WF_SEL_SYABARA.Text           '車腹
        Else
            LNM0006INProw("TORICODE") = WF_TORICODE_TEXT.Text           '取引先コード
            LNM0006INProw("TORINAME") = WF_TORINAME.Text              '取引先名称
            LNM0006INProw("ORGCODE") = WF_ORG.SelectedValue           '部門コード
            LNM0006INProw("ORGNAME") = WF_ORG.SelectedItem           '部門名称
            LNM0006INProw("KASANORGCODE") = WF_KASANORG.SelectedValue            '加算先部門コード
            LNM0006INProw("KASANORGNAME") = WF_KASANORG.SelectedItem            '加算先部門名称
            LNM0006INProw("AVOCADOSHUKABASHO") = WF_AVOCADOSHUKABASHO_TEXT.Text            '実績出荷場所コード
            LNM0006INProw("AVOCADOSHUKANAME") = WF_AVOCADOSHUKANAME.Text           '実績出荷場所名称
            LNM0006INProw("AVOCADOTODOKECODE") = WF_AVOCADOTODOKECODE_TEXT.Text            '実績届先コード
            LNM0006INProw("AVOCADOTODOKENAME") = WF_AVOCADOTODOKENAME.Text            '実績届先名称
            LNM0006INProw("SHABAN") = TxtSHABAN.Text            '車番
            'LNM0006INProw("STYMD") = WF_StYMD.Value            '有効開始日
            LNM0006INProw("BRANCHCODE") = TxtBRANCHCODE.Text            '枝番
            LNM0006INProw("SYAGATA") = WF_SYAGATA.SelectedValue            '車型
            LNM0006INProw("SYAGATANAME") = WF_SYAGATA.SelectedItem            '車型名
            LNM0006INProw("SYABARA") = TxtSYABARA.Text            '車腹
        End If

        LNM0006INProw("STYMD") = WF_StYMD.Value            '有効開始日
        LNM0006INProw("SHUKABASHO") = TxtSHUKABASHO.Text            '変換後出荷場所コード
        LNM0006INProw("SHUKANAME") = TxtSHUKANAME.Text            '変換後出荷場所名称
        LNM0006INProw("TODOKECODE") = TxtTODOKECODE.Text            '変換後届先コード
        LNM0006INProw("TODOKENAME") = TxtTODOKENAME.Text            '変換後届先名称
        LNM0006INProw("TANKNUMBER") = TxtTANKNUMBER.Text            '陸事番号

        LNM0006INProw("ENDYMD") = WF_EndYMD.Value            '有効終了日

        LNM0006INProw("TANKAKBN") = RadioTANKAKBN.SelectedValue          '単価区分
        LNM0006INProw("MEMO") = TxtMEMO.Text            '単価用途
        LNM0006INProw("TANKA") = TxtTANKA.Text            '単価

        LNM0006INProw("ACCOUNTCODE") = WF_ACCOUNT.SelectedValue           '勘定科目コード
        LNM0006INProw("ACCOUNTNAME") = WF_ACCOUNT.SelectedItem            '勘定科目名

        If Not WF_ACCOUNT.SelectedValue = "" Then
            LNM0006INProw("SEGMENTCODE") = WF_SEGMENT.SelectedValue           'セグメントコード
            LNM0006INProw("SEGMENTNAME") = WF_SEGMENT.SelectedItem            'セグメント名
        Else
            LNM0006INProw("SEGMENTCODE") = ""           'セグメントコード
            LNM0006INProw("SEGMENTNAME") = ""            'セグメント名
        End If

        LNM0006INProw("JOTPERCENTAGE") = TxtJOTPERCENTAGE.Text            '割合JOT
        LNM0006INProw("ENEXPERCENTAGE") = TxtENEXPERCENTAGE.Text            '割合ENEX

        LNM0006INProw("CALCKBN") = ddlSelectCALCKBN.SelectedValue            '計算区分
        LNM0006INProw("ROUNDTRIP") = TxtROUNDTRIP.Text            '往復距離
        LNM0006INProw("TOLLFEE") = TxtTOLLFEE.Text            '通行料

        LNM0006INProw("BIKOU1") = TxtBIKOU1.Text            '備考1
        LNM0006INProw("BIKOU2") = TxtBIKOU2.Text            '備考2
        LNM0006INProw("BIKOU3") = TxtBIKOU3.Text            '備考3

        '○ チェック用テーブルに登録する
        LNM0006INPtbl.Rows.Add(LNM0006INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0006INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0006INProw As DataRow = LNM0006INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0006row As DataRow In LNM0006tbl.Rows
            ' KEY項目が等しい時
            If LNM0006row("TORICODE") = LNM0006INProw("TORICODE") AndAlso
                LNM0006row("ORGCODE") = LNM0006INProw("ORGCODE") AndAlso                                '部門コード
                LNM0006row("KASANORGCODE") = LNM0006INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                LNM0006row("AVOCADOSHUKABASHO") = LNM0006INProw("AVOCADOSHUKABASHO") AndAlso                                '実績出荷場所コード
                LNM0006row("AVOCADOTODOKECODE") = LNM0006INProw("AVOCADOTODOKECODE") AndAlso                                '実績届先コード
                LNM0006row("SHABAN") = LNM0006INProw("SHABAN") AndAlso                                '車番
                LNM0006row("STYMD") = LNM0006INProw("STYMD") AndAlso                                '有効開始日
                LNM0006row("BRANCHCODE") = LNM0006INProw("BRANCHCODE") AndAlso                                '枝番
                LNM0006row("SYAGATA") = LNM0006INProw("SYAGATA") AndAlso                                '車型
                LNM0006row("SYABARA") = LNM0006INProw("SYABARA") Then
                ' KEY項目以外の項目の差異をチェック
                If LNM0006row("DELFLG") = LNM0006INProw("DELFLG") AndAlso
                    LNM0006row("TORINAME") = LNM0006INProw("TORINAME") AndAlso                                '取引先名称
                    LNM0006row("ORGNAME") = LNM0006INProw("ORGNAME") AndAlso                                '部門名称
                    LNM0006row("KASANORGNAME") = LNM0006INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                    LNM0006row("AVOCADOSHUKANAME") = LNM0006INProw("AVOCADOSHUKANAME") AndAlso                                '実績出荷場所名称
                    LNM0006row("SHUKABASHO") = LNM0006INProw("SHUKABASHO") AndAlso                                '変換後出荷場所コード
                    LNM0006row("SHUKANAME") = LNM0006INProw("SHUKANAME") AndAlso                                '変換後出荷場所名称
                    LNM0006row("AVOCADOTODOKENAME") = LNM0006INProw("AVOCADOTODOKENAME") AndAlso                                '実績届先名称
                    LNM0006row("TODOKECODE") = LNM0006INProw("TODOKECODE") AndAlso                                '変換後届先コード
                    LNM0006row("TODOKENAME") = LNM0006INProw("TODOKENAME") AndAlso                                '変換後届先名称
                    LNM0006row("TANKNUMBER") = LNM0006INProw("TANKNUMBER") AndAlso                                '陸事番号
                    LNM0006row("ENDYMD") = LNM0006INProw("ENDYMD") AndAlso                                '有効終了日
                    LNM0006row("TANKAKBN") = LNM0006INProw("TANKAKBN") AndAlso                                '単価区分
                    LNM0006row("MEMO") = LNM0006INProw("MEMO") AndAlso                                '単価用途
                    LNM0006row("TANKA") = LNM0006INProw("TANKA") AndAlso                                '単価
                    LNM0006row("ACCOUNTCODE") = LNM0006INProw("ACCOUNTCODE") AndAlso                                '勘定科目コード
                    LNM0006row("ACCOUNTNAME") = LNM0006INProw("ACCOUNTNAME") AndAlso                                '勘定科目名
                    LNM0006row("SEGMENTCODE") = LNM0006INProw("SEGMENTCODE") AndAlso                                'セグメントコード
                    LNM0006row("SEGMENTNAME") = LNM0006INProw("SEGMENTNAME") AndAlso                                'セグメント名
                    LNM0006row("JOTPERCENTAGE") = LNM0006INProw("JOTPERCENTAGE") AndAlso                                '割合JOT
                    LNM0006row("ENEXPERCENTAGE") = LNM0006INProw("ENEXPERCENTAGE") AndAlso                                '割合ENEX
                    LNM0006row("CALCKBN") = LNM0006INProw("CALCKBN") AndAlso                                '計算区分
                    LNM0006row("ROUNDTRIP") = LNM0006INProw("ROUNDTRIP") AndAlso                                '往復距離
                    LNM0006row("TOLLFEE") = LNM0006INProw("TOLLFEE") AndAlso                                '通行料
                    LNM0006row("SYAGATANAME") = LNM0006INProw("SYAGATANAME") AndAlso                                '車型名
                    LNM0006row("BIKOU1") = LNM0006INProw("BIKOU1") AndAlso                                '備考1
                    LNM0006row("BIKOU2") = LNM0006INProw("BIKOU2") AndAlso                                '備考2
                    LNM0006row("BIKOU3") = LNM0006INProw("BIKOU3") Then
                    ' 変更がない時は、入力変更フラグをOFFにする
                    WW_InputChangeFlg = False
                End If

                Exit For

            End If
        Next

        'パンくずから検索を選択した場合
        If WF_ButtonClick.Value = "LNM0006S" Then
            WF_BeforeMAPID.Value = LNM0006WRKINC.MAPIDL
        Else
            WF_BeforeMAPID.Value = LNM0006WRKINC.MAPIDD
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

    ' ******************************************************************************
    ' ***  フィールド変更処理                                                    ***
    ' ******************************************************************************
    ''' <summary>
    ''' フィールド(変更)時処理
    ''' </summary>
    ''' <param name="resVal">取引先(変更)時(WF_SelectTORIChange),部門(変更)時(WF_SelectORGChange),加算先部門(変更)時(WF_SelectKASANORGChange)</param>
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
        '■出荷(情報)取得
        Dim selectSHUKA As String = WF_AVOCADOSHUKABASHO_TEXT.Text
        'Dim selectSHUKA As String = WF_AVOCADOSHUKA.SelectedValue
        Dim selectindexSHUKA As Integer = WF_AVOCADOSHUKA.SelectedIndex
        '■届先(情報)取得
        Dim selectTODOKE As String = WF_AVOCADOTODOKECODE_TEXT.Text
        'Dim selectTODOKE As String = WF_AVOCADOTODOKE.SelectedValue
        Dim selectindexTODOKE As Integer = WF_AVOCADOTODOKE.SelectedIndex

        '〇フィールド(変更)ボタン
        Select Case resVal
            '取引先(変更)時
            Case "WF_TORIChange"
                If selectTORI = "" Then
                    selectORG = ""              '-- 部門(表示)初期化
                    selectindexORG = 0          '-- 部門(INDEX)初期化
                    selectKASANORG = ""         '-- 加算先部門(表示)初期化
                    selectindexKASANORG = 0     '-- 加算先部門(INDEX)初期化
                    selectSHUKA = ""            '-- 出荷(表示)初期化
                    selectindexSHUKA = 0        '-- 出荷(INDEX)初期化
                    selectTODOKE = ""           '-- 届先(表示)初期化
                    selectindexTODOKE = 0       '-- 届先(INDEX)初期化
                End If
            '部門(変更)時
            Case "WF_ORGChange"
                selectKASANORG = ""         '-- 加算先部門(表示)初期化
                selectindexKASANORG = 0     '-- 加算先部門(INDEX)初期化
                selectSHUKA = ""            '-- 出荷(表示)初期化
                selectindexSHUKA = 0        '-- 出荷(INDEX)初期化
                'selectTODOKE = ""           '-- 届先(表示)初期化
                'selectindexTODOKE = 0       '-- 届先(INDEX)初期化
            '加算先部門(変更)時
            Case "WF_KASANORGChange"
                selectSHUKA = ""            '-- 出荷(表示)初期化
                selectindexSHUKA = 0        '-- 出荷(INDEX)初期化
                'selectTODOKE = ""           '-- 届先(表示)初期化
                'selectindexTODOKE = 0       '-- 届先(INDEX)初期化
            '出荷(変更)時
            Case "WF_AVOCADOSHUKAChange"
                'selectTODOKE = ""           '-- 届先(表示)初期化
                'selectindexTODOKE = 0       '-- 届先(INDEX)初期化
            '届先(変更)時
            Case "WF_AVOCADOTODOKEChange"
        End Select

        '〇取引先
        Me.WF_TORI.Items.Clear()
        Dim retToriList As New DropDownList
        retToriList = LNM0006WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG, work.WF_SEL_TARGETYMD_L.Text, I_CREATEFLG:=True)
        'retToriList = LNM0007WRKINC.getDowpDownToriList(Master.MAPID, Master.ROLE_ORG, work.WF_SEL_TARGETYMD_L.Text, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG, I_CREATEFLG:=True)
        WF_TORI.Items.Add(New ListItem("", ""))
        '★ドロップダウンリスト選択(取引先)の場合
        If retToriList.Items.Count = 1 Then
            selectindexTORI = 1
        End If
        '★ドロップダウンリスト再作成(取引先)
        For index As Integer = 0 To retToriList.Items.Count - 1
            WF_TORI.Items.Add(New ListItem(retToriList.Items(index).Text, retToriList.Items(index).Value))
        Next
        Try
            WF_TORI.SelectedIndex = selectindexTORI
        Catch ex As Exception
            WF_TORI.SelectedIndex = 0
        End Try
        WF_TORINAME.Text = WF_TORI.Items(Integer.Parse(selectindexTORI)).Text
        WF_TORICODE_TEXT.Text = WF_TORI.Items(Integer.Parse(selectindexTORI)).Value
        'WF_TORICODE_TEXT.Text = WF_TORI.SelectedValue

        '〇部門
        Me.WF_ORG.Items.Clear()
        Dim retOrgList As New DropDownList
        retOrgList = LNM0006WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG, work.WF_SEL_TARGETYMD_L.Text, I_TORICODE:=selectTORI, I_CREATEFLG:=True)
        'retOrgList = LNM0006WRKINC.getDowpDownOrgList(Master.MAPID, Master.ROLE_ORG, work.WF_SEL_TARGETYMD_L.Text, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG, I_SHUKABASHO:=selectSHUKA, I_CREATEFLG:=True)
        If selectTORI <> "" AndAlso retOrgList.Items.Count = 0 Then
            selectORG = ""              '-- 部門(表示)初期化
            selectindexORG = 0          '-- 部門(INDEX)初期化
            selectKASANORG = ""         '-- 加算先部門(表示)初期化
            selectindexKASANORG = 0     '-- 加算先部門(INDEX)初期化
            selectSHUKA = ""            '-- 出荷(表示)初期化
            selectindexSHUKA = 0        '-- 出荷(INDEX)初期化
            selectTODOKE = ""           '-- 届先(表示)初期化
            selectindexTODOKE = 0       '-- 届先(INDEX)初期化
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
        Try
            WF_ORG.SelectedIndex = selectindexORG
        Catch ex As Exception
            WF_ORG.SelectedIndex = 0
        End Try
        WF_ORGCODE_TEXT.Text = WF_ORG.SelectedValue

        '〇加算先部門
        Me.WF_KASANORG.Items.Clear()
        Dim retKASANOrgList As New DropDownList
        retKASANOrgList = LNM0006WRKINC.getDowpDownKasanOrgList(Master.MAPID, Master.ROLE_ORG, work.WF_SEL_TARGETYMD_L.Text, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG, I_SHUKABASHO:=selectSHUKA, I_CREATEFLG:=True)
        If selectTORI <> "" AndAlso retKASANOrgList.Items.Count = 0 Then
            selectKASANORG = ""         '-- 加算先部門(表示)初期化
            selectindexKASANORG = 0     '-- 加算先部門(INDEX)初期化
            selectSHUKA = ""            '-- 出荷(表示)初期化
            selectindexSHUKA = 0        '-- 出荷(INDEX)初期化
            selectTODOKE = ""           '-- 届先(表示)初期化
            selectindexTODOKE = 0       '-- 届先(INDEX)初期化
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
        Try
            WF_KASANORG.SelectedIndex = selectindexKASANORG
        Catch ex As Exception
            WF_KASANORG.SelectedIndex = 0
        End Try
        WF_KASANORGCODE_TEXT.Text = WF_KASANORG.SelectedValue

        '〇出荷
        Me.WF_AVOCADOSHUKA.Items.Clear()
        Dim retShukaList As New DropDownList
        retShukaList = LNM0006WRKINC.getDowpDownAvocadoshukaList(Master.MAPID, Master.ROLE_ORG, work.WF_SEL_TARGETYMD_L.Text, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG, I_CREATEFLG:=True)
        WF_AVOCADOSHUKA.Items.Add(New ListItem("", ""))
        '★ドロップダウンリスト選択(出荷)の場合
        If retShukaList.Items.Count = 1 Then
            selectindexSHUKA = 1
        End If
        '★ドロップダウンリスト再作成(出荷)
        For index As Integer = 0 To retShukaList.Items.Count - 1
            WF_AVOCADOSHUKA.Items.Add(New ListItem(retShukaList.Items(index).Text, retShukaList.Items(index).Value))
        Next
        Try
            WF_AVOCADOSHUKA.SelectedIndex = selectindexSHUKA
            WF_AVOCADOSHUKANAME.Text = WF_AVOCADOSHUKA.Items(Integer.Parse(selectindexSHUKA)).Text
            WF_AVOCADOSHUKABASHO_TEXT.Text = WF_AVOCADOSHUKA.Items(Integer.Parse(selectindexSHUKA)).Value
            'WF_AVOCADOSHUKABASHO_TEXT.Text = WF_AVOCADOSHUKA.SelectedValue
        Catch ex As Exception
            WF_AVOCADOSHUKA.SelectedIndex = 0
        End Try

        'コンボボックス化
        Dim WF_AVOCADOSHUKA_OPTIONS As String = ""
        For index As Integer = 0 To retShukaList.Items.Count - 1
            WF_AVOCADOSHUKA_OPTIONS += "<option>" + retShukaList.Items(index).Text + "</option>"
        Next
        WF_AVOCADOSHUKA_DL.InnerHtml = WF_AVOCADOSHUKA_OPTIONS
        Me.WF_AVOCADOSHUKANAME.Attributes("list") = Me.WF_AVOCADOSHUKA_DL.ClientID

        '〇届先
        Me.WF_AVOCADOTODOKE.Items.Clear()
        Dim retTodokeList As New DropDownList
        retTodokeList = LNM0006WRKINC.getDowpDownAvocadotodokeList(Master.MAPID, Master.ROLE_ORG, work.WF_SEL_TARGETYMD_L.Text, I_TORICODE:=selectTORI, I_ORGCODE:=selectORG, I_KASANORGCODE:=selectKASANORG, I_CREATEFLG:=True)
        WF_AVOCADOTODOKE.Items.Add(New ListItem("", ""))
        '★ドロップダウンリスト選択(届先)の場合
        If retTodokeList.Items.Count = 1 Then
            selectindexTODOKE = 1
        End If
        '★ドロップダウンリスト再作成(届先)
        For index As Integer = 0 To retTodokeList.Items.Count - 1
            WF_AVOCADOTODOKE.Items.Add(New ListItem(retTodokeList.Items(index).Text, retTodokeList.Items(index).Value))
        Next
        Try
            '取引先(変更)時
            If resVal = "WF_TORIChange" Then
                WF_AVOCADOTODOKE.SelectedIndex = selectindexTODOKE
                WF_AVOCADOTODOKENAME.Text = WF_AVOCADOTODOKE.Items(Integer.Parse(selectindexTODOKE)).Text
                WF_AVOCADOTODOKECODE_TEXT.Text = WF_AVOCADOTODOKE.Items(Integer.Parse(selectindexTODOKE)).Value
                'WF_AVOCADOTODOKECODE_TEXT.Text = WF_AVOCADOTODOKE.SelectedValue
            End If
        Catch ex As Exception
            WF_AVOCADOTODOKE.SelectedIndex = 0
        End Try

        'コンボボックス化
        Dim WF_AVOCADOTODOKE_OPTIONS As String = ""
        For index As Integer = 0 To retTodokeList.Items.Count - 1
            WF_AVOCADOTODOKE_OPTIONS += "<option>" + retTodokeList.Items(index).Text + "</option>"
        Next
        WF_AVOCADOTODOKE_DL.InnerHtml = WF_AVOCADOTODOKE_OPTIONS
        Me.WF_AVOCADOTODOKENAME.Attributes("list") = Me.WF_AVOCADOTODOKE_DL.ClientID

    End Sub

    ''' <summary>
    ''' 詳細画面初期化
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub DetailBoxClear()

        '○ 状態をクリア
        For Each LNM0006row As DataRow In LNM0006tbl.Rows
            Select Case LNM0006row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0006tbl, work.WF_SEL_INPTBL.Text)

        TxtSelLineCNT.Text = ""              'LINECNT
        TxtMapId.Text = "M00001"             '画面ＩＤ
        RadioDELFLG.SelectedValue = ""                  '削除フラグ
        TxtSHUKABASHO.Text = ""                    '変換後出荷場所コード
        TxtSHUKANAME.Text = ""                    '変換後出荷場所名称
        WF_AVOCADOTODOKENAME.Text = ""                    '実績届先名称
        TxtTODOKECODE.Text = ""                    '変換後届先コード
        TxtTODOKENAME.Text = ""                    '変換後届先名称
        TxtTANKNUMBER.Text = ""                    '陸事番号
        TxtSHABAN.Text = ""                    '車番
        TxtBRANCHCODE.Text = ""                    '枝番
        TxtMEMO.Text = ""                    '単価用途
        TxtTANKA.Text = ""                    '単価
        TxtJOTPERCENTAGE.Text = ""                    '割合JOT
        TxtENEXPERCENTAGE.Text = ""                    '割合ENEX
        TxtROUNDTRIP.Text = ""                    '往復距離
        TxtTOLLFEE.Text = ""                    '通行料
        TxtSYABARA.Text = ""                    '車腹
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
            Case "WF_AVOCADOTODOKENAME"
                CODENAME_get("AVOCADOTODOKECODE", WF_AVOCADOTODOKECODE_TEXT.Text, WF_AVOCADOTODOKENAME.Text, WW_RtnSW)  '実績届先コード
                WF_AVOCADOTODOKENAME.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 単価マスタ更新(削除フラグのみ)
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub UpdateMasterDelflgOnly()
        Dim WW_MODIFYKBN As String = ""
        Dim WW_DATE As Date = Date.Now

        '初期化
        LNM0006INPtbl = New DataTable
        LNM0006INPtbl.Columns.Add("TORICODE")
        LNM0006INPtbl.Columns.Add("ORGCODE")
        LNM0006INPtbl.Columns.Add("KASANORGCODE")
        LNM0006INPtbl.Columns.Add("AVOCADOSHUKABASHO")
        LNM0006INPtbl.Columns.Add("AVOCADOTODOKECODE")
        LNM0006INPtbl.Columns.Add("SHABAN")
        LNM0006INPtbl.Columns.Add("STYMD")
        LNM0006INPtbl.Columns.Add("BRANCHCODE")
        LNM0006INPtbl.Columns.Add("SYAGATA")
        LNM0006INPtbl.Columns.Add("SYABARA")
        LNM0006INPtbl.Columns.Add("DELFLG")

        Dim row As DataRow
        row = LNM0006INPtbl.NewRow
        row("TORICODE") = work.WF_SEL_TORICODE.Text
        row("ORGCODE") = work.WF_SEL_ORGCODE.Text
        row("KASANORGCODE") = work.WF_SEL_KASANORGCODE.Text
        row("AVOCADOSHUKABASHO") = work.WF_SEL_AVOCADOSHUKABASHO.Text
        row("AVOCADOTODOKECODE") = work.WF_SEL_AVOCADOTODOKECODE.Text
        row("SHABAN") = work.WF_SEL_SHABAN.Text
        row("STYMD") = work.WF_SEL_STYMD.Text
        row("BRANCHCODE") = work.WF_SEL_BRANCHCODE.Text
        row("SYAGATA") = work.WF_SEL_SYAGATA.Text
        row("SYABARA") = work.WF_SEL_SYABARA.Text
        row("DELFLG") = C_DELETE_FLG.DELETE
        LNM0006INPtbl.Rows.Add(row)

        ' DB更新処理
        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            ' DataBase接続
            SQLcon.Open()

            '履歴テーブルに変更前データを登録
            InsertHist(SQLcon, LNM0006WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '削除フラグ更新
            SetDelflg(SQLcon, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

            '履歴テーブルに変更後データを登録
            InsertHist(SQLcon, LNM0006WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                Exit Sub
            End If

        End Using

        '○ 入力値反映
        For Each LNM0006INProw As DataRow In LNM0006INPtbl.Rows
            For Each LNM0006row As DataRow In LNM0006tbl.Rows
                If LNM0006INProw("TORICODE") = LNM0006row("TORICODE") AndAlso
                    LNM0006INProw("ORGCODE") = LNM0006row("ORGCODE") AndAlso                                '部門コード
                    LNM0006INProw("KASANORGCODE") = LNM0006row("KASANORGCODE") AndAlso                                '加算先部門コード
                    LNM0006INProw("AVOCADOSHUKABASHO") = LNM0006row("AVOCADOSHUKABASHO") AndAlso                                '実績出荷場所コード
                    LNM0006INProw("AVOCADOTODOKECODE") = LNM0006row("AVOCADOTODOKECODE") AndAlso                                '実績届先コード
                    LNM0006INProw("SHABAN") = LNM0006row("SHABAN") AndAlso                                '車番
                    LNM0006INProw("STYMD") = LNM0006row("STYMD") AndAlso                                '有効開始日
                    LNM0006INProw("BRANCHCODE") = LNM0006row("BRANCHCODE") AndAlso                                '枝番
                    LNM0006INProw("SYAGATA") = LNM0006row("SYAGATA") AndAlso                                '車型
                    LNM0006INProw("SYABARA") = LNM0006row("SYABARA") Then '車腹
                    ' 画面入力テーブル項目設定              
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0006row("DELFLG") = LNM0006INProw("DELFLG")
                    LNM0006row("SELECT") = 0
                    LNM0006row("HIDDEN") = 0
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
        SQLStr.Append("     LNG.LNM0006_NEWTANKA                    ")
        SQLStr.Append(" SET                                         ")
        SQLStr.Append("     DELFLG               = '1'              ")
        SQLStr.Append("   , UPDYMD               = @UPDYMD          ")
        SQLStr.Append("   , UPDUSER              = @UPDUSER         ")
        SQLStr.Append("   , UPDTERMID            = @UPDTERMID       ")
        SQLStr.Append("   , UPDPGID              = @UPDPGID         ")
        SQLStr.Append(" WHERE                                       ")
        SQLStr.Append("         COALESCE(TORICODE, '')             = @TORICODE ")
        SQLStr.Append("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.Append("    AND  COALESCE(KASANORGCODE, '')             = @KASANORGCODE ")
        SQLStr.Append("    AND  COALESCE(AVOCADOSHUKABASHO, '')             = @AVOCADOSHUKABASHO ")
        SQLStr.Append("    AND  COALESCE(AVOCADOTODOKECODE, '')             = @AVOCADOTODOKECODE ")
        SQLStr.Append("    AND  COALESCE(SHABAN, '')             = @SHABAN ")
        SQLStr.Append("    AND  COALESCE(DATE_FORMAT(STYMD, '%Y/%m/%d'), '') = COALESCE(DATE_FORMAT(@STYMD, '%Y/%m/%d'), '') ")
        SQLStr.Append("    AND  COALESCE(BRANCHCODE, '0')             = @BRANCHCODE ")
        SQLStr.Append("    AND  COALESCE(SYAGATA, '')             = @SYAGATA ")
        SQLStr.Append("    AND  COALESCE(SYABARA, '0')             = @SYABARA ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_KASANORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@KASANORGCODE", MySqlDbType.VarChar, 6) '加算先部門コード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar, 6)     '実績出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar, 6)     '実績届先コード
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar, 20)     '車番
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日
                Dim P_BRANCHCODE As MySqlParameter = SQLcmd.Parameters.Add("@BRANCHCODE", MySqlDbType.VarChar, 2) '枝番
                Dim P_SYAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SYAGATA", MySqlDbType.VarChar, 1)     '車型
                Dim P_SYABARA As MySqlParameter = SQLcmd.Parameters.Add("@SYABARA", MySqlDbType.Decimal, 10, 3)     '車腹
                Dim P_UPDYMD As MySqlParameter = SQLcmd.Parameters.Add("@UPDYMD", MySqlDbType.DateTime)         '更新年月日
                Dim P_UPDUSER As MySqlParameter = SQLcmd.Parameters.Add("@UPDUSER", MySqlDbType.VarChar, 20)         '更新ユーザーＩＤ
                Dim P_UPDTERMID As MySqlParameter = SQLcmd.Parameters.Add("@UPDTERMID", MySqlDbType.VarChar, 20)         '更新端末
                Dim P_UPDPGID As MySqlParameter = SQLcmd.Parameters.Add("@UPDPGID", MySqlDbType.VarChar, 40)         '更新プログラムＩＤ

                Dim LNM0006row As DataRow = LNM0006INPtbl.Rows(0)
                P_TORICODE.Value = LNM0006row("TORICODE") '取引先コード
                P_ORGCODE.Value = LNM0006row("ORGCODE") '部門コード
                P_KASANORGCODE.Value = LNM0006row("KASANORGCODE") '加算先部門コード
                P_AVOCADOSHUKABASHO.Value = LNM0006row("AVOCADOSHUKABASHO") '実績出荷場所コード
                P_AVOCADOTODOKECODE.Value = LNM0006row("AVOCADOTODOKECODE") '実績届先コード
                P_SHABAN.Value = LNM0006row("SHABAN") '車番
                P_STYMD.Value = LNM0006row("STYMD") '有効開始日
                P_BRANCHCODE.Value = LNM0006row("BRANCHCODE") '枝番
                P_SYAGATA.Value = LNM0006row("SYAGATA") '車型
                P_SYABARA.Value = LNM0006row("SYABARA") '車腹
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0006C UPDATE"
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
            WW_CheckMES1 = "・単価マスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0006INProw As DataRow In LNM0006INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0006INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0006INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "TORICODE", LNM0006INProw("TORICODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not LNM0006INProw("TORICODE").ToString.Length = LNM0006WRKINC.REQUIREDDIGITS_TORICODE Then
                    WW_CheckMES1 = "・取引先コードエラーです。"
                    WW_CheckMES2 = "桁数エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・取引先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 取引先名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TORINAME", LNM0006INProw("TORINAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・取引先名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 部門コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ORGCODE", LNM0006INProw("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・部門コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 部門名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ORGNAME", LNM0006INProw("ORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・部門名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 加算先部門コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KASANORGCODE", LNM0006INProw("KASANORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・加算先部門コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 加算先部門名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "KASANORGNAME", LNM0006INProw("KASANORGNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・加算先部門名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 実績出荷場所コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "AVOCADOSHUKABASHO", LNM0006INProw("AVOCADOSHUKABASHO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not LNM0006INProw("AVOCADOSHUKABASHO").ToString.Length = LNM0006WRKINC.REQUIREDDIGITS_AVOCADOSHUKABASHO Then
                    WW_CheckMES1 = "・実績出荷場所コードエラーです。"
                    WW_CheckMES2 = "桁数エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・実績出荷場所コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 実績出荷場所名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "AVOCADOSHUKANAME", LNM0006INProw("AVOCADOSHUKANAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・実績出荷場所名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 変換後出荷場所コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SHUKABASHO", LNM0006INProw("SHUKABASHO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・変換後出荷場所コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 変換後出荷場所名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SHUKANAME", LNM0006INProw("SHUKANAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・変換後出荷場所名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 実績届先コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "AVOCADOTODOKECODE", LNM0006INProw("AVOCADOTODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not LNM0006INProw("AVOCADOTODOKECODE").ToString.Length = LNM0006WRKINC.REQUIREDDIGITS_AVOCADOTODOKECODE Then
                    WW_CheckMES1 = "・実績届先コードエラーです。"
                    WW_CheckMES2 = "桁数エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・実績届先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 実績届先名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "AVOCADOTODOKENAME", LNM0006INProw("AVOCADOTODOKENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・実績届先名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 変換後届先コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TODOKECODE", LNM0006INProw("TODOKECODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・変換後届先コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 変換後届先名称(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TODOKENAME", LNM0006INProw("TODOKENAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・変換後届先名称エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 陸事番号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TANKNUMBER", LNM0006INProw("TANKNUMBER"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・陸事番号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 車番(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SHABAN", LNM0006INProw("SHABAN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・車番エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 有効開始日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "STYMD", LNM0006INProw("STYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                LNM0006INProw("STYMD") = CDate(LNM0006INProw("STYMD")).ToString("yyyy/MM/dd")
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
                Master.CheckField(Master.USERCAMP, "ENDYMD", LNM0006INProw("ENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If isNormal(WW_CS0024FCheckerr) Then
                    LNM0006INProw("ENDYMD") = CDate(LNM0006INProw("ENDYMD")).ToString("yyyy/MM/dd")
                Else
                    WW_CheckMES1 = "・有効終了日エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 枝番(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BRANCHCODE", LNM0006INProw("BRANCHCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・枝番エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 単価区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TANKAKBN", LNM0006INProw("TANKAKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・単価区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 単価用途(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "MEMO", LNM0006INProw("MEMO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・単価用途エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 単価(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TANKA", LNM0006INProw("TANKA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・単価エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 勘定科目コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ACCOUNTCODE", LNM0006INProw("ACCOUNTCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・勘定科目コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 勘定科目名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ACCOUNTNAME", LNM0006INProw("ACCOUNTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・勘定科目名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' セグメントコード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SEGMENTCODE", LNM0006INProw("SEGMENTCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・セグメントコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' セグメント名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SEGMENTNAME", LNM0006INProw("SEGMENTNAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・セグメント名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 割合JOT(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "JOTPERCENTAGE", LNM0006INProw("JOTPERCENTAGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・割合JOTエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 割合ENEX(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ENEXPERCENTAGE", LNM0006INProw("ENEXPERCENTAGE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・割合ENEXエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 計算区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CALCKBN", LNM0006INProw("CALCKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・計算区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 往復距離(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ROUNDTRIP", LNM0006INProw("ROUNDTRIP"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・往復距離エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 通行料(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TOLLFEE", LNM0006INProw("TOLLFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・通行料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 車型(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SYAGATA", LNM0006INProw("SYAGATA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・車型エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 車型名(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SYAGATANAME", LNM0006INProw("SYAGATANAME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・車型名エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 車腹(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SYABARA", LNM0006INProw("SYABARA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・車腹エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 備考1(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIKOU1", LNM0006INProw("BIKOU1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・備考1エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 備考2(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIKOU2", LNM0006INProw("BIKOU2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・備考2エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 備考3(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIKOU3", LNM0006INProw("BIKOU3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・備考3エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            '画面で入力済みの場合のみ
            If Not WF_EndYMD.Value = "" Then
                ' 日付大小チェック
                If Not String.IsNullOrEmpty(LNM0006INProw("STYMD")) AndAlso
                        Not String.IsNullOrEmpty(LNM0006INProw("ENDYMD")) Then
                    If CDate(LNM0006INProw("STYMD")) > CDate(LNM0006INProw("ENDYMD")) Then
                        WW_CheckMES1 = "・有効開始日＆有効終了日エラーです。"
                        WW_CheckMES2 = "日付大小入力エラー"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            End If

            '割合JOT、割合ENEX合計値チェック
            If Not String.IsNullOrEmpty(LNM0006INProw("JOTPERCENTAGE")) Or
                    Not String.IsNullOrEmpty(LNM0006INProw("ENEXPERCENTAGE")) Then
                Dim WW_Decimal As Decimal
                Dim WW_JOTPERCENTAGE As Double
                Dim WW_ENEXPERCENTAGE As Double
                Dim WW_TOTALPERCENTAGE As Double

                If Decimal.TryParse(LNM0006INProw("JOTPERCENTAGE").ToString, WW_Decimal) Then
                    WW_JOTPERCENTAGE = WW_Decimal
                Else
                    WW_JOTPERCENTAGE = 0
                End If
                If Decimal.TryParse(LNM0006INProw("ENEXPERCENTAGE").ToString, WW_Decimal) Then
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
            If Not String.IsNullOrEmpty(work.WF_SEL_TORICODE.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_TIMESTAMP.Text,
                                              work.WF_SEL_TORICODE.Text, work.WF_SEL_ORGCODE.Text, work.WF_SEL_KASANORGCODE.Text,
                                              work.WF_SEL_AVOCADOSHUKABASHO.Text, work.WF_SEL_AVOCADOTODOKECODE.Text, work.WF_SEL_SHABAN.Text,
                                              work.WF_SEL_STYMD.Text, work.WF_SEL_BRANCHCODE.Text,
                                              work.WF_SEL_SYAGATA.Text, work.WF_SEL_SYABARA.Text)


                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（取引先コード & 部門コード & 加算先部門コード & 実績出荷場所コード & 実績届先コード & 車番 & 有効開始日 & 枝番 & 車型 & 車腹）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0006INProw("TORICODE") & "]" &
                                           "([" & LNM0006INProw("ORGCODE") & "]" &
                                           "([" & LNM0006INProw("KASANORGCODE") & "]" &
                                           "([" & LNM0006INProw("AVOCADOSHUKABASHO") & "]" &
                                           "([" & LNM0006INProw("AVOCADOTODOKECODE") & "]" &
                                           "([" & LNM0006INProw("SHABAN") & "]" &
                                           "([" & LNM0006INProw("STYMD") & "]" &
                                           "([" & LNM0006INProw("BRANCHCODE") & "]" &
                                           "([" & LNM0006INProw("SYAGATA") & "]" &
                                           " [" & LNM0006INProw("SYABARA") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If

            If String.IsNullOrEmpty(WW_LineErr) Then
                If LNM0006INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0006INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0006INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0006INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0006tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0006tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0006row As DataRow In LNM0006tbl.Rows
            Select Case LNM0006row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0006row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0006INProw As DataRow In LNM0006INPtbl.Rows
            'エラーレコード読み飛ばし
            If LNM0006INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0006INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0006row As DataRow In LNM0006tbl.Rows
                ' KEY項目が等しい時
                If LNM0006row("TORICODE") = LNM0006INProw("TORICODE") AndAlso'取引先コード
                    LNM0006row("ORGCODE") = LNM0006INProw("ORGCODE") AndAlso                                '部門コード
                    LNM0006row("KASANORGCODE") = LNM0006INProw("KASANORGCODE") AndAlso                                '加算先部門コード
                    LNM0006row("AVOCADOSHUKABASHO") = LNM0006INProw("AVOCADOSHUKABASHO") AndAlso                                '実績出荷場所コード
                    LNM0006row("AVOCADOTODOKECODE") = LNM0006INProw("AVOCADOTODOKECODE") AndAlso                                '実績届先コード
                    LNM0006row("SHABAN") = LNM0006INProw("SHABAN") AndAlso                                '車番
                    LNM0006row("STYMD") = LNM0006INProw("STYMD") AndAlso                                '有効開始日
                    LNM0006row("BRANCHCODE") = LNM0006INProw("BRANCHCODE") AndAlso                                '枝番
                    LNM0006row("SYAGATA") = LNM0006INProw("SYAGATA") AndAlso                                '車型
                    LNM0006row("SYABARA") = LNM0006INProw("SYABARA") Then '車腹
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0006row("DELFLG") = LNM0006INProw("DELFLG") AndAlso
                        LNM0006row("TORINAME") = LNM0006INProw("TORINAME") AndAlso                                '取引先名称
                        LNM0006row("ORGNAME") = LNM0006INProw("ORGNAME") AndAlso                                '部門名称
                        LNM0006row("KASANORGNAME") = LNM0006INProw("KASANORGNAME") AndAlso                                '加算先部門名称
                        LNM0006row("AVOCADOSHUKANAME") = LNM0006INProw("AVOCADOSHUKANAME") AndAlso                                '実績出荷場所名称
                        LNM0006row("SHUKABASHO") = LNM0006INProw("SHUKABASHO") AndAlso                                '変換後出荷場所コード
                        LNM0006row("SHUKANAME") = LNM0006INProw("SHUKANAME") AndAlso                                '変換後出荷場所名称
                        LNM0006row("AVOCADOTODOKENAME") = LNM0006INProw("AVOCADOTODOKENAME") AndAlso                                '実績届先名称
                        LNM0006row("TODOKECODE") = LNM0006INProw("TODOKECODE") AndAlso                                '変換後届先コード
                        LNM0006row("TODOKENAME") = LNM0006INProw("TODOKENAME") AndAlso                                '変換後届先名称
                        LNM0006row("TANKNUMBER") = LNM0006INProw("TANKNUMBER") AndAlso                                '陸事番号
                        LNM0006row("ENDYMD") = LNM0006INProw("ENDYMD") AndAlso                                '有効終了日
                        LNM0006row("TANKAKBN") = LNM0006INProw("TANKAKBN") AndAlso                                '単価区分
                        LNM0006row("MEMO") = LNM0006INProw("MEMO") AndAlso                                '単価用途
                        LNM0006row("TANKA") = LNM0006INProw("TANKA") AndAlso                                '単価
                        LNM0006row("ACCOUNTCODE") = LNM0006INProw("ACCOUNTCODE") AndAlso                                '勘定科目コード
                        LNM0006row("ACCOUNTNAME") = LNM0006INProw("ACCOUNTNAME") AndAlso                                '勘定科目名
                        LNM0006row("SEGMENTCODE") = LNM0006INProw("SEGMENTCODE") AndAlso                                'セグメントコード
                        LNM0006row("SEGMENTNAME") = LNM0006INProw("SEGMENTNAME") AndAlso                                'セグメント名
                        LNM0006row("JOTPERCENTAGE") = LNM0006INProw("JOTPERCENTAGE") AndAlso                                '割合JOT
                        LNM0006row("ENEXPERCENTAGE") = LNM0006INProw("ENEXPERCENTAGE") AndAlso                                '割合ENEX
                        LNM0006row("CALCKBN") = LNM0006INProw("CALCKBN") AndAlso                                '計算区分
                        LNM0006row("ROUNDTRIP") = LNM0006INProw("ROUNDTRIP") AndAlso                                '往復距離
                        LNM0006row("TOLLFEE") = LNM0006INProw("TOLLFEE") AndAlso                                '通行料
                        LNM0006row("SYAGATANAME") = LNM0006INProw("SYAGATANAME") AndAlso                                '車型名
                        LNM0006row("BIKOU1") = LNM0006INProw("BIKOU1") AndAlso                                '備考1
                        LNM0006row("BIKOU2") = LNM0006INProw("BIKOU2") AndAlso                                '備考2
                        LNM0006row("BIKOU3") = LNM0006INProw("BIKOU3") AndAlso                                '備考3
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0006row("OPERATION")) Then

                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0006INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0006INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        '更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0006INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0006INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0006INPtbl.Rows(0)("OPERATION")) Then
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

                '枝番が新規、有効開始日が変更されたときの対応
                If LNM0006INPtbl.Rows(0)("BRANCHCODE").ToString = "" Then '枝番なし(新規の場合)
                    '枝番を生成
                    LNM0006INPtbl.Rows(0)("BRANCHCODE") = LNM0006WRKINC.GenerateBranchCode(SQLcon, LNM0006INPtbl.Rows(0), WW_DBDataCheck)
                    If Not isNormal(WW_DBDataCheck) Then
                        Exit Sub
                    End If
                    WF_AUTOENDYMD.Value = LNM0006WRKINC.MAX_ENDYMD
                Else
                    '更新前の最大有効開始日取得
                    WW_BeforeMAXSTYMD = LNM0006WRKINC.GetSTYMD(SQLcon, LNM0006INPtbl.Rows(0), WW_DBDataCheck)
                    If Not isNormal(WW_DBDataCheck) Then
                        Exit Sub
                    End If

                    Select Case True
                        'DBに登録されている有効開始日が無かった場合
                        Case WW_BeforeMAXSTYMD = ""
                            WF_AUTOENDYMD.Value = LNM0006WRKINC.MAX_ENDYMD
                            '同一の場合
                        Case WW_BeforeMAXSTYMD = CDate(LNM0006INPtbl.Rows(0)("STYMD")).ToString("yyyy/MM/dd")
                            WF_AUTOENDYMD.Value = LNM0006WRKINC.MAX_ENDYMD
                        '更新前有効開始日 <　入力有効開始日(DBに登録されている有効開始日よりも登録しようとしている有効開始日が大きい場合)
                        Case WW_BeforeMAXSTYMD < CDate(LNM0006INPtbl.Rows(0)("STYMD")).ToString("yyyy/MM/dd")
                            'DBに登録されている有効開始日の有効終了日を登録しようとしている有効開始日-1にする

                            '変更後の有効開始日退避
                            WW_STYMD_SAVE = LNM0006INPtbl.Rows(0)("STYMD")
                            '変更後の有効終了日退避
                            WW_ENDYMD_SAVE = LNM0006INPtbl.Rows(0)("ENDYMD")

                            '変更後テーブルに変更前の有効開始日格納
                            LNM0006INPtbl.Rows(0)("STYMD") = WW_BeforeMAXSTYMD
                            '変更後テーブルに更新用の有効終了日格納
                            LNM0006INPtbl.Rows(0)("ENDYMD") = DateTime.Parse(WW_STYMD_SAVE).AddDays(-1).ToString("yyyy/MM/dd")
                            '履歴テーブルに変更前データを登録
                            InsertHist(SQLcon, LNM0006WRKINC.MODIFYKBN.BEFDATA, WW_DATE)
                            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                Exit Sub
                            End If
                            '変更前の有効終了日更新
                            UpdateENDYMD(SQLcon, LNM0006INPtbl.Rows(0), WW_DBDataCheck, WW_DATE)
                            If Not isNormal(WW_DBDataCheck) Then
                                Exit Sub
                            End If
                            '履歴テーブルに変更後データを登録
                            InsertHist(SQLcon, LNM0006WRKINC.MODIFYKBN.AFTDATA, WW_DATE)
                            If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                                Exit Sub
                            End If
                            '退避した有効開始日を元に戻す
                            LNM0006INPtbl.Rows(0)("STYMD") = WW_STYMD_SAVE
                            '退避した有効終了日を元に戻す
                            LNM0006INPtbl.Rows(0)("ENDYMD") = WW_ENDYMD_SAVE
                            '有効終了日に最大値を入れる
                            WF_AUTOENDYMD.Value = LNM0006WRKINC.MAX_ENDYMD
                        Case Else
                            '有効終了日に有効開始日の月の末日を入れる
                            Dim WW_NEXT_YM As String = DateTime.Parse(LNM0006INPtbl.Rows(0)("STYMD")).AddMonths(1).ToString("yyyy/MM")
                            WF_AUTOENDYMD.Value = DateTime.Parse(WW_NEXT_YM & "/01").AddDays(-1).ToString("yyyy/MM/dd")
                    End Select
                End If

                '変更チェック
                MASTEREXISTS(SQLcon, WW_MODIFYKBN)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0006WRKINC.MODIFYKBN.AFTDATA
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
        For Each LNM0006INProw As DataRow In LNM0006INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0006row As DataRow In LNM0006tbl.Rows
                ' 同一レコードか判定
                If LNM0006INProw("TORICODE") = LNM0006row("TORICODE") AndAlso
                    LNM0006INProw("ORGCODE") = LNM0006row("ORGCODE") AndAlso                                '部門コード
                    LNM0006INProw("KASANORGCODE") = LNM0006row("KASANORGCODE") AndAlso                                '加算先部門コード
                    LNM0006INProw("AVOCADOSHUKABASHO") = LNM0006row("AVOCADOSHUKABASHO") AndAlso                                '実績出荷場所コード
                    LNM0006INProw("AVOCADOTODOKECODE") = LNM0006row("AVOCADOTODOKECODE") AndAlso                                '実績届先コード
                    LNM0006INProw("SHABAN") = LNM0006row("SHABAN") AndAlso                                '車番
                    LNM0006INProw("STYMD") = LNM0006row("STYMD") AndAlso                                '有効開始日
                    LNM0006INProw("BRANCHCODE") = LNM0006row("BRANCHCODE") AndAlso                                '枝番
                    LNM0006INProw("SYAGATA") = LNM0006row("SYAGATA") AndAlso                                '車型
                    LNM0006INProw("SYABARA") = LNM0006row("SYABARA") Then '車腹
                    ' 画面入力テーブル項目設定
                    LNM0006INProw("LINECNT") = LNM0006row("LINECNT")
                    LNM0006INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0006INProw("UPDTIMSTP") = LNM0006row("UPDTIMSTP")
                    LNM0006INProw("SELECT") = 0
                    LNM0006INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0006row.ItemArray = LNM0006INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0006tbl.NewRow
                WW_NRow.ItemArray = LNM0006INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0006tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0006tbl.Rows.Add(WW_NRow)
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
                    work.CODENAMEGetAVOCADOTODOKE(SQLcon, WW_NAMEht)
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
