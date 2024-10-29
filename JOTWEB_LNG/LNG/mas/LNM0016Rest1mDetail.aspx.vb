''************************************************************
' 使用料特例１マスタメンテ登録画面
' 作成日 2022/02/14
' 更新日 2023/10/02
' 作成者 名取
' 更新者 大浜
'
' 修正履歴 : 2022/02/14 新規作成
'          : 2023/10/02 変更履歴登録機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' 使用料特例１マスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0016Rest1mDetail
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private LNM0016tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0016INPtbl As DataTable                              'チェック用テーブル
    Private LNM0016UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0016tbl, work.WF_SEL_INPTBL.Text)

                    Select Case WF_ButtonClick.Value
                        Case "WF_ButtonUPDATE"          '更新ボタン押下
                            WF_UPDATE_Click()
                        Case "WF_ButtonCLEAR"           '戻るボタン押下
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
                        Case "mspStationSingleRowSelected" '[共通]駅選択ポップアップで行選択
                            RowSelected_mspStationSingle()

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
            If Not IsNothing(LNM0016tbl) Then
                LNM0016tbl.Clear()
                LNM0016tbl.Dispose()
                LNM0016tbl = Nothing
            End If

            If Not IsNothing(LNM0016INPtbl) Then
                LNM0016INPtbl.Clear()
                LNM0016INPtbl.Dispose()
                LNM0016INPtbl = Nothing
            End If

            If Not IsNothing(LNM0016UPDtbl) Then
                LNM0016UPDtbl.Clear()
                LNM0016UPDtbl.Dispose()
                LNM0016UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0016WRKINC.MAPIDD
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

        '○ 画面の値設定
        WW_MAPValueSet()

    End Sub

    ''' <summary>
    ''' 画面初期値設定処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WW_MAPValueSet()

        '○ 検索画面からの遷移
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0016L Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If

        '○ 名称設定処理
        '選択行
        LblSelLineCNT.Text = work.WF_SEL_LINECNT.Text
        '削除
        TxtDelFlg.Text = work.WF_SEL_DELFLG.Text
        CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
        '画面ＩＤ
        TxtMapId.Text = "M00001"
        '組織コード
        TxtOrgCode.Text = work.WF_SEL_ORG2.Text
        CODENAME_get("ORG", TxtOrgCode.Text, LblOrgName.Text, WW_Dummy)
        '大分類コード
        TxtBigCTNCD.Text = work.WF_SEL_BIGCTNCD2.Text
        CODENAME_get("BIGCTNCD", TxtBigCTNCD.Text, LblBigCTNCDName.Text, WW_Dummy)
        '中分類コード
        TxtMiddleCTNCD.Text = work.WF_SEL_MIDDLECTNCD2.Text
        CODENAME_get("MIDDLECTNCD", TxtMiddleCTNCD.Text, LblMiddleCTNCDName.Text, WW_Dummy)
        '発駅コード
        TxtDepStation.Text = work.WF_SEL_DEPSTATION2.Text
        CODENAME_get("STATION", TxtDepStation.Text, LblDepStationName.Text, WW_Dummy)
        '発受託人コード
        TxtDepTrusteeCd.Text = work.WF_SEL_DEPTRUSTEECD2.Text
        CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCd.Text, LblDepTrusteeCdName.Text, WW_Dummy)
        '発受託人サブコード
        TxtDepTrusteeSubCd.Text = work.WF_SEL_DEPTRUSTEESUBCD2.Text
        CODENAME_get("DEPTRUSTEESUBCD", TxtDepTrusteeSubCd.Text, LblDepTrusteeSubCdName.Text, WW_Dummy)
        '優先順位
        TxtPriorityNo.Text = work.WF_SEL_PRIORITYNO.Text
        '使用目的
        TxtPurpose.Text = work.WF_SEL_PURPOSE.Text
        '選択比較項目-コンテナ記号
        TxtCTNType.Text = work.WF_SEL_SLCCTNTYPE.Text
        '選択比較項目-コンテナ番号（開始）
        TxtCTNStNo.Text = work.WF_SEL_SLCCTNSTNO.Text
        '選択比較項目-コンテナ番号（終了）
        TxtCTNEndNo.Text = work.WF_SEL_SLCCTNENDNO.Text
        '選択比較項目-ＪＲ発支社支店コード
        TxtSlcJrDepBranchCd.Text = work.WF_SEL_SLCJRDEPBRANCHCD.Text
        CODENAME_get("JRBRANCHCD", TxtSlcJrDepBranchCd.Text, LblSlcJrDepBranchCdName.Text, WW_Dummy)
        '選択比較項目-発荷主コード１
        TxtSlcDepShipperCd1.Text = work.WF_SEL_SLCDEPSHIPPERCD1.Text
        CODENAME_get("SHIPPER", TxtSlcDepShipperCd1.Text, LblSlcDepShipperCd1Name.Text, WW_Dummy)
        '選択比較項目-発荷主コード２
        TxtSlcDepShipperCd2.Text = work.WF_SEL_SLCDEPSHIPPERCD2.Text
        CODENAME_get("SHIPPER", TxtSlcDepShipperCd2.Text, LblSlcDepShipperCd2Name.Text, WW_Dummy)
        '選択比較項目-発荷主コード３
        TxtSlcDepShipperCd3.Text = work.WF_SEL_SLCDEPSHIPPERCD3.Text
        CODENAME_get("SHIPPER", TxtSlcDepShipperCd3.Text, LblSlcDepShipperCd3Name.Text, WW_Dummy)
        '選択比較項目-発荷主コード４
        TxtSlcDepShipperCd4.Text = work.WF_SEL_SLCDEPSHIPPERCD4.Text
        CODENAME_get("SHIPPER", TxtSlcDepShipperCd4.Text, LblSlcDepShipperCd4Name.Text, WW_Dummy)
        '選択比較項目-発荷主コード５
        TxtSlcDepShipperCd5.Text = work.WF_SEL_SLCDEPSHIPPERCD5.Text
        CODENAME_get("SHIPPER", TxtSlcDepShipperCd5.Text, LblSlcDepShipperCd5Name.Text, WW_Dummy)
        '選択比較項目-発荷主ＣＤ比較条件
        TxtSlcDepShipperCdCond.Text = work.WF_SEL_SLCDEPSHIPPERCDCOND.Text
        CODENAME_get("COMPARECONDKBN", TxtSlcDepShipperCdCond.Text, LblSlcDepShipperCdCondName.Text, WW_Dummy)
        '選択比較項目-ＪＲ着支社支店コード
        TxtSlcJrArrBranchCd.Text = work.WF_SEL_SLCJRARRBRANCHCD.Text
        CODENAME_get("JRBRANCHCD", TxtSlcJrArrBranchCd.Text, LblSlcJrArrBranchCdName.Text, WW_Dummy)
        '選択比較項目-ＪＲ着支社支店ＣＤ比較
        TxtSlcJrArrBranchCdCond.Text = work.WF_SEL_SLCJRARRBRANCHCDCOND.Text
        CODENAME_get("COMPARECONDKBN", TxtSlcJrArrBranchCdCond.Text, LblSlcJrArrBranchCdCondName.Text, WW_Dummy)
        '選択比較項目-ＪＯＴ着組織コード
        TxtSlcJotArrOrgCode.Text = work.WF_SEL_SLCJOTARRORGCODE.Text
        CODENAME_get("ORG", TxtSlcJotArrOrgCode.Text, LblSlcJotArrOrgCodeName.Text, WW_Dummy)
        '選択比較項目-ＪＯＴ着組織ＣＤ比較
        TxtSlcJotArrOrgCodeCond.Text = work.WF_SEL_SLCJOTARRORGCODECOND.Text
        CODENAME_get("COMPARECONDKBN", TxtSlcJotArrOrgCodeCond.Text, LblSlcJotArrOrgCodeCondName.Text, WW_Dummy)
        '選択比較項目-着駅コード１
        TxtSlcArrStation1.Text = work.WF_SEL_SLCARRSTATION1.Text
        CODENAME_get("STATION", TxtSlcArrStation1.Text, LblSlcArrStation1Name.Text, WW_Dummy)
        '選択比較項目-着駅コード２
        TxtSlcArrStation2.Text = work.WF_SEL_SLCARRSTATION2.Text
        CODENAME_get("STATION", TxtSlcArrStation2.Text, LblSlcArrStation2Name.Text, WW_Dummy)
        '選択比較項目-着駅コード３
        TxtSlcArrStation3.Text = work.WF_SEL_SLCARRSTATION3.Text
        CODENAME_get("STATION", TxtSlcArrStation3.Text, LblSlcArrStation3Name.Text, WW_Dummy)
        '選択比較項目-着駅コード４
        TxtSlcArrStation4.Text = work.WF_SEL_SLCARRSTATION4.Text
        CODENAME_get("STATION", TxtSlcArrStation4.Text, LblSlcArrStation4Name.Text, WW_Dummy)
        '選択比較項目-着駅コード５
        TxtSlcArrStation5.Text = work.WF_SEL_SLCARRSTATION5.Text
        CODENAME_get("STATION", TxtSlcArrStation5.Text, LblSlcArrStation5Name.Text, WW_Dummy)
        '選択比較項目-着駅コード６
        TxtSlcArrStation6.Text = work.WF_SEL_SLCARRSTATION6.Text
        CODENAME_get("STATION", TxtSlcArrStation6.Text, LblSlcArrStation6Name.Text, WW_Dummy)
        '選択比較項目-着駅コード７
        TxtSlcArrStation7.Text = work.WF_SEL_SLCARRSTATION7.Text
        CODENAME_get("STATION", TxtSlcArrStation7.Text, LblSlcArrStation7Name.Text, WW_Dummy)
        '選択比較項目-着駅コード８
        TxtSlcArrStation8.Text = work.WF_SEL_SLCARRSTATION8.Text
        CODENAME_get("STATION", TxtSlcArrStation8.Text, LblSlcArrStation8Name.Text, WW_Dummy)
        '選択比較項目-着駅コード９
        TxtSlcArrStation9.Text = work.WF_SEL_SLCARRSTATION9.Text
        CODENAME_get("STATION", TxtSlcArrStation9.Text, LblSlcArrStation9Name.Text, WW_Dummy)
        '選択比較項目-着駅コード１０
        TxtSlcArrStation10.Text = work.WF_SEL_SLCARRSTATION10.Text
        CODENAME_get("STATION", TxtSlcArrStation10.Text, LblSlcArrStation10Name.Text, WW_Dummy)
        '選択比較項目-着駅コード比較条件
        TxtSlcArrStationCond.Text = work.WF_SEL_SLCARRSTATIONCOND.Text
        CODENAME_get("COMPARECONDKBN", TxtSlcArrStationCond.Text, LblSlcArrStationCondName.Text, WW_Dummy)
        '選択比較項目-着受託人コード
        TxtSlcArrTrusteeCd.Text = work.WF_SEL_SLCARRTRUSTEECD.Text
        CODENAME_get("ARRTRUSTEECD", TxtSlcArrTrusteeCd.Text, LblSlcArrTrusteeCdName.Text, WW_Dummy)
        '選択比較項目-着受託人ＣＤ比較条件
        TxtSlcArrTrusteeCdCond.Text = work.WF_SEL_SLCARRTRUSTEECDCOND.Text
        CODENAME_get("COMPARECONDKBN", TxtSlcArrTrusteeCdCond.Text, LblSlcArrTrusteeCdCondName.Text, WW_Dummy)
        '選択比較項目-着受託人サブコード
        TxtSlcArrTrusteeSubCd.Text = work.WF_SEL_SLCARRTRUSTEESUBCD.Text
        CODENAME_get("ARRTRUSTEESUBCD", TxtSlcArrTrusteeSubCd.Text, LblSlcArrTrusteeSubCdName.Text, WW_Dummy)
        '選択比較項目-着受託人サブＣＤ比較
        TxtSlcArrTrusteeSubCdCond.Text = work.WF_SEL_SLCARRTRUSTEESUBCDCOND.Text
        CODENAME_get("COMPARECONDKBN", TxtSlcArrTrusteeSubCdCond.Text, LblSlcArrTrusteeSubCdCondName.Text, WW_Dummy)
        '選択比較項目-開始月日
        TxtSlcStMD.Text = work.WF_SEL_SLCSTMD.Text
        '選択比較項目-終了月日
        TxtSlcEndMD.Text = work.WF_SEL_SLCENDMD.Text
        '選択比較項目-開始発送年月日
        TxtSlcStShipMD.Text = work.WF_SEL_SLCSTSHIPYMD.Text
        '選択比較項目-終了発送年月日
        TxtSlcEndShipMD.Text = work.WF_SEL_SLCENDSHIPYMD.Text
        '選択比較項目-ＪＲ品目コード１
        TxtSlcJrItemCd1.Text = work.WF_SEL_SLCJRITEMCD1.Text
        CODENAME_get("ITEM", TxtSlcJrItemCd1.Text, LblSlcJrItemCd1Name.Text, WW_Dummy)
        '選択比較項目-ＪＲ品目コード２
        TxtSlcJrItemCd2.Text = work.WF_SEL_SLCJRITEMCD2.Text
        CODENAME_get("ITEM", TxtSlcJrItemCd2.Text, LblSlcJrItemCd2Name.Text, WW_Dummy)
        '選択比較項目-ＪＲ品目コード３
        TxtSlcJrItemCd3.Text = work.WF_SEL_SLCJRITEMCD3.Text
        CODENAME_get("ITEM", TxtSlcJrItemCd3.Text, LblSlcJrItemCd3Name.Text, WW_Dummy)
        '選択比較項目-ＪＲ品目コード４
        TxtSlcJrItemCd4.Text = work.WF_SEL_SLCJRITEMCD4.Text
        CODENAME_get("ITEM", TxtSlcJrItemCd4.Text, LblSlcJrItemCd4Name.Text, WW_Dummy)
        '選択比較項目-ＪＲ品目コード５
        TxtSlcJrItemCd5.Text = work.WF_SEL_SLCJRITEMCD5.Text
        CODENAME_get("ITEM", TxtSlcJrItemCd5.Text, LblSlcJrItemCd5Name.Text, WW_Dummy)
        '選択比較項目-ＪＲ品目コード６
        TxtSlcJrItemCd6.Text = work.WF_SEL_SLCJRITEMCD6.Text
        CODENAME_get("ITEM", TxtSlcJrItemCd6.Text, LblSlcJrItemCd6Name.Text, WW_Dummy)
        '選択比較項目-ＪＲ品目コード７
        TxtSlcJrItemCd7.Text = work.WF_SEL_SLCJRITEMCD7.Text
        CODENAME_get("ITEM", TxtSlcJrItemCd7.Text, LblSlcJrItemCd7Name.Text, WW_Dummy)
        '選択比較項目-ＪＲ品目コード８
        TxtSlcJrItemCd8.Text = work.WF_SEL_SLCJRITEMCD8.Text
        CODENAME_get("ITEM", TxtSlcJrItemCd8.Text, LblSlcJrItemCd8Name.Text, WW_Dummy)
        '選択比較項目-ＪＲ品目コード９
        TxtSlcJrItemCd9.Text = work.WF_SEL_SLCJRITEMCD9.Text
        CODENAME_get("ITEM", TxtSlcJrItemCd9.Text, LblSlcJrItemCd9Name.Text, WW_Dummy)
        '選択比較項目-ＪＲ品目コード１０
        TxtSlcJrItemCd10.Text = work.WF_SEL_SLCJRITEMCD10.Text
        CODENAME_get("ITEM", TxtSlcJrItemCd10.Text, LblSlcJrItemCd10Name.Text, WW_Dummy)
        '選択比較項目-ＪＲ品目コード比較
        TxtSlcJrItemCdCond.Text = work.WF_SEL_SLCJRITEMCDCOND.Text
        CODENAME_get("COMPARECONDKBN", TxtSlcJrItemCdCond.Text, LblSlcJrItemCdCondName.Text, WW_Dummy)
        '特例置換項目-使用料金額
        TxtSprUseFee.Text = work.WF_SEL_SPRUSEFEE.Text
        '特例置換項目-使用料率
        TxtSprUseFeeRate.Text = work.WF_SEL_SPRUSEFEERATE.Text
        '特例置換項目-使用料率端数整理
        TxtSprUseFeeRateRound1.Text = work.WF_SEL_SPRUSEFEERATEROUND1.Text
        CODENAME_get("HASUU1", TxtSprUseFeeRateRound1.Text, LblSprUseFeeRateRound1Name.Text, WW_Dummy)
        TxtSprUseFeeRateRound2.Text = work.WF_SEL_SPRUSEFEERATEROUND2.Text
        CODENAME_get("HASUU2", TxtSprUseFeeRateRound2.Text, LblSprUseFeeRateRound2Name.Text, WW_Dummy)
        '特例置換項目-使用料率加減額
        TxtSprUseFeeRateAddSub.Text = work.WF_SEL_SPRUSEFEERATEADDSUB.Text
        '特例置換項目-使用料率加減額端数整理
        TxtSprUseFeeRateAddSubCond1.Text = work.WF_SEL_SPRUSEFEERATEADDSUBCOND1.Text
        CODENAME_get("HASUU1", TxtSprUseFeeRateAddSubCond1.Text, LblSprUseFeeRateAddSubCond1Name.Text, WW_Dummy)
        TxtSprUseFeeRateAddSubCond2.Text = work.WF_SEL_SPRUSEFEERATEADDSUBCOND2.Text
        CODENAME_get("HASUU2", TxtSprUseFeeRateAddSubCond2.Text, LblSprUseFeeRateAddSubCond2Name.Text, WW_Dummy)
        '特例置換項目-端数処理時点区分
        TxtSprRoundPointKbn.Text = work.WF_SEL_SPRROUNDPOINTKBN.Text
        CODENAME_get("HASUUPOINTKBN", TxtSprRoundPointKbn.Text, LblSprRoundPointKbnName.Text, WW_Dummy)
        '特例置換項目-使用料無料特認
        TxtSprUseFreeSpe.Text = work.WF_SEL_SPRUSEFREESPE.Text
        CODENAME_get("USEFREEKBN", TxtSprUseFreeSpe.Text, LblSprUseFreeSpeName.Text, WW_Dummy)
        '特例置換項目-通運負担回送運賃
        TxtSprNittsuFreeSendFee.Text = work.WF_SEL_SPRNITTSUFREESENDFEE.Text
        '特例置換項目-運行管理料
        TxtSprManageFee.Text = work.WF_SEL_SPRMANAGEFEE.Text
        '特例置換項目-荷主負担運賃
        TxtSprShipBurdenFee.Text = work.WF_SEL_SPRSHIPBURDENFEE.Text
        '特例置換項目-発送料
        TxtSprShipFee.Text = work.WF_SEL_SPRSHIPFEE.Text
        '特例置換項目-到着料
        TxtSprArriveFee.Text = work.WF_SEL_SPRARRIVEFEE.Text
        '特例置換項目-集荷料
        TxtSprPickUpFee.Text = work.WF_SEL_SPRPICKUPFEE.Text
        '特例置換項目-配達料
        TxtSprDeliveryFee.Text = work.WF_SEL_SPRDELIVERYFEE.Text
        '特例置換項目-その他１
        TxtSprOther1.Text = work.WF_SEL_SPROTHER1.Text
        '特例置換項目-その他２
        TxtSprOther2.Text = work.WF_SEL_SPROTHER2.Text
        '特例置換項目-適合区分
        TxtSprFitKbn.Text = work.WF_SEL_SPRFITKBN.Text
        CODENAME_get("FITKBN", TxtSprFitKbn.Text, LblSprFitKbnName.Text, WW_Dummy)
        '特例置換項目-契約コード
        TxtSprContractCd.Text = work.WF_SEL_SPRCONTRACTCD.Text

        'Disabled制御項目
        DisabledKeyItem.Value = work.WF_SEL_ORG2.Text

        ' 数値(0～9)のみ入力可能とする。
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"                    '削除フラグ
        Me.TxtOrgCode.Attributes("onkeyPress") = "CheckNum()"                   '組織コード
        Me.TxtBigCTNCD.Attributes("onkeyPress") = "CheckNum()"                  '大分類コード
        Me.TxtMiddleCTNCD.Attributes("onkeyPress") = "CheckNum()"               '中分類コード
        Me.TxtDepStation.Attributes("onkeyPress") = "CheckNum()"                '発駅コード
        Me.TxtDepTrusteeCd.Attributes("onkeyPress") = "CheckNum()"              '発受託人コード
        Me.TxtDepTrusteeCd.Attributes("onkeyPress") = "CheckNum()"              '発受託人コード
        Me.TxtPriorityNo.Attributes("onkeyPress") = "CheckNum()"                '優先順位
        Me.TxtCTNStNo.Attributes("onkeyPress") = "CheckNum()"                   '選択比較項目-コンテナ番号（開始）
        Me.TxtCTNEndNo.Attributes("onkeyPress") = "CheckNum()"                  '選択比較項目-コンテナ番号（終了）
        Me.TxtSlcJrDepBranchCd.Attributes("onkeyPress") = "CheckNum()"          '選択比較項目-ＪＲ発支社支店コード
        Me.TxtSlcDepShipperCd1.Attributes("onkeyPress") = "CheckNum()"          '選択比較項目-発荷主コード１
        Me.TxtSlcDepShipperCd2.Attributes("onkeyPress") = "CheckNum()"          '選択比較項目-発荷主コード２
        Me.TxtSlcDepShipperCd3.Attributes("onkeyPress") = "CheckNum()"          '選択比較項目-発荷主コード３
        Me.TxtSlcDepShipperCd4.Attributes("onkeyPress") = "CheckNum()"          '選択比較項目-発荷主コード４
        Me.TxtSlcDepShipperCd5.Attributes("onkeyPress") = "CheckNum()"          '選択比較項目-発荷主コード５
        Me.TxtSlcDepShipperCdCond.Attributes("onkeyPress") = "CheckNum()"       '選択比較項目-発荷主ＣＤ比較条件
        Me.TxtSlcJrArrBranchCd.Attributes("onkeyPress") = "CheckNum()"          '選択比較項目-ＪＲ着支社支店コード
        Me.TxtSlcJrArrBranchCdCond.Attributes("onkeyPress") = "CheckNum()"      '選択比較項目-ＪＲ着支社支店ＣＤ比較
        Me.TxtSlcJotArrOrgCode.Attributes("onkeyPress") = "CheckNum()"          '選択比較項目-ＪＯＴ着組織コード
        Me.TxtSlcJotArrOrgCodeCond.Attributes("onkeyPress") = "CheckNum()"      '選択比較項目-ＪＯＴ着組織ＣＤ比較
        Me.TxtSlcArrStation1.Attributes("onkeyPress") = "CheckNum()"            '選択比較項目-着駅コード１
        Me.TxtSlcArrStation2.Attributes("onkeyPress") = "CheckNum()"            '選択比較項目-着駅コード２
        Me.TxtSlcArrStation3.Attributes("onkeyPress") = "CheckNum()"            '選択比較項目-着駅コード３
        Me.TxtSlcArrStation4.Attributes("onkeyPress") = "CheckNum()"            '選択比較項目-着駅コード４
        Me.TxtSlcArrStation5.Attributes("onkeyPress") = "CheckNum()"            '選択比較項目-着駅コード５
        Me.TxtSlcArrStation6.Attributes("onkeyPress") = "CheckNum()"            '選択比較項目-着駅コード６
        Me.TxtSlcArrStation7.Attributes("onkeyPress") = "CheckNum()"            '選択比較項目-着駅コード７
        Me.TxtSlcArrStation8.Attributes("onkeyPress") = "CheckNum()"            '選択比較項目-着駅コード８
        Me.TxtSlcArrStation9.Attributes("onkeyPress") = "CheckNum()"            '選択比較項目-着駅コード９
        Me.TxtSlcArrStation10.Attributes("onkeyPress") = "CheckNum()"           '選択比較項目-着駅コード１０
        Me.TxtSlcArrStationCond.Attributes("onkeyPress") = "CheckNum()"         '選択比較項目-着駅コード比較条件
        Me.TxtSlcArrTrusteeCd.Attributes("onkeyPress") = "CheckNum()"           '選択比較項目-着受託人コード
        Me.TxtSlcArrTrusteeCdCond.Attributes("onkeyPress") = "CheckNum()"       '選択比較項目-着受託人ＣＤ比較条件
        Me.TxtSlcArrTrusteeSubCd.Attributes("onkeyPress") = "CheckNum()"        '選択比較項目-着受託人サブコード
        Me.TxtSlcArrTrusteeSubCdCond.Attributes("onkeyPress") = "CheckNum()"    '選択比較項目-着受託人サブＣＤ比較
        Me.TxtSlcStMD.Attributes("onkeyPress") = "CheckNum()"                   '選択比較項目-開始月日
        Me.TxtSlcEndMD.Attributes("onkeyPress") = "CheckNum()"                  '選択比較項目-終了月日
        Me.TxtSlcJrItemCd1.Attributes("onkeyPress") = "CheckNum()"              '選択比較項目-ＪＲ品目コード１
        Me.TxtSlcJrItemCd2.Attributes("onkeyPress") = "CheckNum()"              '選択比較項目-ＪＲ品目コード２
        Me.TxtSlcJrItemCd3.Attributes("onkeyPress") = "CheckNum()"              '選択比較項目-ＪＲ品目コード３
        Me.TxtSlcJrItemCd4.Attributes("onkeyPress") = "CheckNum()"              '選択比較項目-ＪＲ品目コード４
        Me.TxtSlcJrItemCd5.Attributes("onkeyPress") = "CheckNum()"              '選択比較項目-ＪＲ品目コード５
        Me.TxtSlcJrItemCd6.Attributes("onkeyPress") = "CheckNum()"              '選択比較項目-ＪＲ品目コード６
        Me.TxtSlcJrItemCd7.Attributes("onkeyPress") = "CheckNum()"              '選択比較項目-ＪＲ品目コード７
        Me.TxtSlcJrItemCd8.Attributes("onkeyPress") = "CheckNum()"              '選択比較項目-ＪＲ品目コード８
        Me.TxtSlcJrItemCd9.Attributes("onkeyPress") = "CheckNum()"              '選択比較項目-ＪＲ品目コード９
        Me.TxtSlcJrItemCd10.Attributes("onkeyPress") = "CheckNum()"             '選択比較項目-ＪＲ品目コード１０
        Me.TxtSlcJrItemCdCond.Attributes("onkeyPress") = "CheckNum()"           '選択比較項目-ＪＲ品目コード比較
        Me.TxtSprUseFee.Attributes("onkeyPress") = "CheckNum()"                 '特例置換項目-使用料金額
        Me.TxtSprUseFeeRateRound1.Attributes("onkeyPress") = "CheckNum()"       '特例置換項目-使用料率端数整理1
        Me.TxtSprUseFeeRateRound2.Attributes("onkeyPress") = "CheckNum()"       '特例置換項目-使用料率端数整理2
        Me.TxtSprUseFeeRateAddSub.Attributes("onkeyPress") = "CheckNum()"       '特例置換項目-使用料率加減額
        Me.TxtSprUseFeeRateAddSubCond1.Attributes("onkeyPress") = "CheckNum()"  '特例置換項目-使用料率加減額端数整理1
        Me.TxtSprUseFeeRateAddSubCond2.Attributes("onkeyPress") = "CheckNum()"  '特例置換項目-使用料率加減額端数整理2
        Me.TxtSprRoundPointKbn.Attributes("onkeyPress") = "CheckNum()"          '特例置換項目-端数処理時点区分
        Me.TxtSprUseFreeSpe.Attributes("onkeyPress") = "CheckNum()"             '特例置換項目-使用料無料特認
        Me.TxtSprNittsuFreeSendFee.Attributes("onkeyPress") = "CheckNum()"      '特例置換項目-通運負担回送運賃
        Me.TxtSprManageFee.Attributes("onkeyPress") = "CheckNum()"              '特例置換項目-運行管理料
        Me.TxtSprShipBurdenFee.Attributes("onkeyPress") = "CheckNum()"          '特例置換項目-荷主負担運賃
        Me.TxtSprShipFee.Attributes("onkeyPress") = "CheckNum()"                '特例置換項目-発送料
        Me.TxtSprArriveFee.Attributes("onkeyPress") = "CheckNum()"              '特例置換項目-到着料
        Me.TxtSprPickUpFee.Attributes("onkeyPress") = "CheckNum()"              '特例置換項目-集荷料
        Me.TxtSprDeliveryFee.Attributes("onkeyPress") = "CheckNum()"            '特例置換項目-配達料
        Me.TxtSprOther1.Attributes("onkeyPress") = "CheckNum()"                 '特例置換項目-その他１
        Me.TxtSprOther2.Attributes("onkeyPress") = "CheckNum()"                 '特例置換項目-その他２
        Me.TxtSprFitKbn.Attributes("onkeyPress") = "CheckNum()"                 '特例置換項目-適合区分
        Me.TxtSprContractCd.Attributes("onkeyPress") = "CheckNum()"             '特例置換項目-契約コード

        ' 入力するテキストボックスは数値(0～9)＋英字のみ可能とする。
        Me.TxtCTNType.Attributes("onkeyPress") = "CheckNumAZ()"                 '選択比較項目-コンテナ記号

        ' 入力するテキストボックスは数値(0～9)＋記号(.)のみ可能とする。
        Me.TxtSprUseFeeRate.Attributes("onkeyPress") = "CheckDeci()"            '特例置換項目-使用料率

        ' 入力するテキストボックスは数値(0～9)＋記号(/)のみ可能とする。
        Me.TxtSlcStShipMD.Attributes("onkeyPress") = "CheckCalendar()"          '選択比較項目-開始発送年月日
        Me.TxtSlcEndShipMD.Attributes("onkeyPress") = "CheckCalendar()"         '選択比較項目-終了発送年月日


        Me.TxtSprUseFeeRateRound1.Attributes("onMouseOver") = "saveTabScrollPosition()"       '特例置換項目-使用料率端数整理1



    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO"></param>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                         " _
            & "     ORGCODE                    " _
            & "   , BIGCTNCD                   " _
            & "   , MIDDLECTNCD                " _
            & "   , DEPSTATION                 " _
            & "   , DEPTRUSTEECD               " _
            & "   , DEPTRUSTEESUBCD            " _
            & "   , PRIORITYNO                 " _
            & " FROM                           " _
            & "     LNG.LNM0016_REST1M         " _
            & " WHERE                          " _
            & "         ORGCODE          = @P1 " _
            & "     AND BIGCTNCD         = @P2 " _
            & "     AND MIDDLECTNCD      = @P3 " _
            & "     AND DEPSTATION       = @P4 " _
            & "     AND DEPTRUSTEECD     = @P5 " _
            & "     AND DEPTRUSTEESUBCD  = @P6 " _
            & "     AND PRIORITYNO       = @P7 " _
            & "     AND DELFLG          <> @P8 "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 6) '組織コード
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 2) '大分類コード
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 2) '中分類コード
                Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 6) '発駅コード
                Dim PARA5 As MySqlParameter = SQLcmd.Parameters.Add("@P5", MySqlDbType.VarChar, 5) '発受託人コード
                Dim PARA6 As MySqlParameter = SQLcmd.Parameters.Add("@P6", MySqlDbType.VarChar, 3) '発受託人サブコード
                Dim PARA7 As MySqlParameter = SQLcmd.Parameters.Add("@P7", MySqlDbType.VarChar, 5) '優先順位
                Dim PARA8 As MySqlParameter = SQLcmd.Parameters.Add("@P8", MySqlDbType.VarChar, 1) '削除フラグ

                PARA1.Value = TxtOrgCode.Text          '組織コード
                PARA2.Value = TxtBigCTNCD.Text         '大分類コード
                PARA3.Value = TxtMiddleCTNCD.Text      '中分類コード
                PARA4.Value = TxtDepStation.Text       '発駅コード
                PARA5.Value = TxtDepTrusteeCd.Text     '発受託人コード
                PARA6.Value = TxtDepTrusteeSubCd.Text  '発受託人サブコード
                PARA7.Value = TxtPriorityNo.Text       '優先順位
                PARA8.Value = C_DELETE_FLG.DELETE      '削除フラグ

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0016Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0016Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0016Chk.Load(SQLdr)

                    If LNM0016Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0016C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0016C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 使用料特例マスタ１登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(使用料特例マスタ１)
        Dim SQLStr As String =
              " DECLARE @hensuu AS bigint ;                 " _
            & "     SET @hensuu = 0 ;                       " _
            & " DECLARE hensuu CURSOR FOR                   " _
            & "     SELECT                                  " _
            & "         UPDTIMSTP AS hensuu                 " _
            & "     FROM                                    " _
            & "         LNG.LNM0016_REST1M                  " _
            & "     WHERE                                   " _
            & "         ORGCODE         = @P01              " _
            & "     AND BIGCTNCD        = @P02              " _
            & "     AND MIDDLECTNCD     = @P03              " _
            & "     AND DEPSTATION      = @P04              " _
            & "     AND DEPTRUSTEECD    = @P05              " _
            & "     AND DEPTRUSTEESUBCD = @P06              " _
            & "     AND PRIORITYNO      = @P07 ;            " _
            & " OPEN hensuu ;                               " _
            & " FETCH NEXT FROM hensuu INTO @hensuu ;       " _
            & " IF (@@FETCH_STATUS = 0)                     " _
            & "     UPDATE LNG.LNM0016_REST1M               " _
            & "     SET                                     " _
            & "         DELFLG                  = @P00      " _
            & "       , PURPOSE                 = @P08      " _
            & "       , SLCCTNTYPE              = @P09      " _
            & "       , SLCCTNSTNO              = @P10      " _
            & "       , SLCCTNENDNO             = @P11      " _
            & "       , SLCJRDEPBRANCHCD        = @P12      " _
            & "       , SLCDEPSHIPPERCD1        = @P13      " _
            & "       , SLCDEPSHIPPERCD2        = @P14      " _
            & "       , SLCDEPSHIPPERCD3        = @P15      " _
            & "       , SLCDEPSHIPPERCD4        = @P16      " _
            & "       , SLCDEPSHIPPERCD5        = @P17      " _
            & "       , SLCDEPSHIPPERCDCOND     = @P18      " _
            & "       , SLCJRARRBRANCHCD        = @P19      " _
            & "       , SLCJRARRBRANCHCDCOND    = @P20      " _
            & "       , SLCJOTARRORGCODE        = @P21      " _
            & "       , SLCJOTARRORGCODECOND    = @P22      " _
            & "       , SLCARRSTATION1          = @P23      " _
            & "       , SLCARRSTATION2          = @P24      " _
            & "       , SLCARRSTATION3          = @P25      " _
            & "       , SLCARRSTATION4          = @P26      " _
            & "       , SLCARRSTATION5          = @P27      " _
            & "       , SLCARRSTATION6          = @P28      " _
            & "       , SLCARRSTATION7          = @P29      " _
            & "       , SLCARRSTATION8          = @P30      " _
            & "       , SLCARRSTATION9          = @P31      " _
            & "       , SLCARRSTATION10         = @P32      " _
            & "       , SLCARRSTATIONCOND       = @P33      " _
            & "       , SLCARRTRUSTEECD         = @P34      " _
            & "       , SLCARRTRUSTEECDCOND     = @P35      " _
            & "       , SLCARRTRUSTEESUBCD      = @P36      " _
            & "       , SLCARRTRUSTEESUBCDCOND  = @P37      " _
            & "       , SLCSTMD                 = @P38      " _
            & "       , SLCENDMD                = @P39      " _
            & "       , SLCSTSHIPYMD            = @P40      " _
            & "       , SLCENDSHIPYMD           = @P41      " _
            & "       , SLCJRITEMCD1            = @P42      " _
            & "       , SLCJRITEMCD2            = @P43      " _
            & "       , SLCJRITEMCD3            = @P44      " _
            & "       , SLCJRITEMCD4            = @P45      " _
            & "       , SLCJRITEMCD5            = @P46      " _
            & "       , SLCJRITEMCD6            = @P47      " _
            & "       , SLCJRITEMCD7            = @P48      " _
            & "       , SLCJRITEMCD8            = @P49      " _
            & "       , SLCJRITEMCD9            = @P50      " _
            & "       , SLCJRITEMCD10           = @P51      " _
            & "       , SLCJRITEMCDCOND         = @P52      " _
            & "       , SPRUSEFEE               = @P53      " _
            & "       , SPRUSEFEERATE           = @P54      " _
            & "       , SPRUSEFEERATEROUND      = @P55      " _
            & "       , SPRUSEFEERATEADDSUB     = @P56      " _
            & "       , SPRUSEFEERATEADDSUBCOND = @P57      " _
            & "       , SPRROUNDPOINTKBN        = @P58      " _
            & "       , SPRUSEFREESPE           = @P59      " _
            & "       , SPRNITTSUFREESENDFEE    = @P60      " _
            & "       , SPRMANAGEFEE            = @P61      " _
            & "       , SPRSHIPBURDENFEE        = @P62      " _
            & "       , SPRSHIPFEE              = @P63      " _
            & "       , SPRARRIVEFEE            = @P64      " _
            & "       , SPRPICKUPFEE            = @P65      " _
            & "       , SPRDELIVERYFEE          = @P66      " _
            & "       , SPROTHER1               = @P67      " _
            & "       , SPROTHER2               = @P68      " _
            & "       , SPRFITKBN               = @P69      " _
            & "       , SPRCONTRACTCD           = @P70      " _
            & "       , UPDYMD                  = @P76      " _
            & "       , UPDUSER                 = @P77      " _
            & "       , UPDTERMID               = @P78      " _
            & "       , UPDPGID                 = @P79      " _
            & "       , RECEIVEYMD              = @P80      " _
            & "     WHERE                                   " _
            & "         ORGCODE         = @P01              " _
            & "     AND BIGCTNCD        = @P02              " _
            & "     AND MIDDLECTNCD     = @P03              " _
            & "     AND DEPSTATION      = @P04              " _
            & "     AND DEPTRUSTEECD    = @P05              " _
            & "     AND DEPTRUSTEESUBCD = @P06              " _
            & "     AND PRIORITYNO      = @P07 ;            " _
            & " IF (@@FETCH_STATUS <> 0)                    " _
            & "     INSERT INTO LNG.LNM0016_REST1M          " _
            & "        (DELFLG                              " _
            & "       , ORGCODE                             " _
            & "       , BIGCTNCD                            " _
            & "       , MIDDLECTNCD                         " _
            & "       , DEPSTATION                          " _
            & "       , DEPTRUSTEECD                        " _
            & "       , DEPTRUSTEESUBCD                     " _
            & "       , PRIORITYNO                          " _
            & "       , PURPOSE                             " _
            & "       , SLCCTNTYPE                          " _
            & "       , SLCCTNSTNO                          " _
            & "       , SLCCTNENDNO                         " _
            & "       , SLCJRDEPBRANCHCD                    " _
            & "       , SLCDEPSHIPPERCD1                    " _
            & "       , SLCDEPSHIPPERCD2                    " _
            & "       , SLCDEPSHIPPERCD3                    " _
            & "       , SLCDEPSHIPPERCD4                    " _
            & "       , SLCDEPSHIPPERCD5                    " _
            & "       , SLCDEPSHIPPERCDCOND                 " _
            & "       , SLCJRARRBRANCHCD                    " _
            & "       , SLCJRARRBRANCHCDCOND                " _
            & "       , SLCJOTARRORGCODE                    " _
            & "       , SLCJOTARRORGCODECOND                " _
            & "       , SLCARRSTATION1                      " _
            & "       , SLCARRSTATION2                      " _
            & "       , SLCARRSTATION3                      " _
            & "       , SLCARRSTATION4                      " _
            & "       , SLCARRSTATION5                      " _
            & "       , SLCARRSTATION6                      " _
            & "       , SLCARRSTATION7                      " _
            & "       , SLCARRSTATION8                      " _
            & "       , SLCARRSTATION9                      " _
            & "       , SLCARRSTATION10                     " _
            & "       , SLCARRSTATIONCOND                   " _
            & "       , SLCARRTRUSTEECD                     " _
            & "       , SLCARRTRUSTEECDCOND                 " _
            & "       , SLCARRTRUSTEESUBCD                  " _
            & "       , SLCARRTRUSTEESUBCDCOND              " _
            & "       , SLCSTMD                             " _
            & "       , SLCENDMD                            " _
            & "       , SLCSTSHIPYMD                        " _
            & "       , SLCENDSHIPYMD                       " _
            & "       , SLCJRITEMCD1                        " _
            & "       , SLCJRITEMCD2                        " _
            & "       , SLCJRITEMCD3                        " _
            & "       , SLCJRITEMCD4                        " _
            & "       , SLCJRITEMCD5                        " _
            & "       , SLCJRITEMCD6                        " _
            & "       , SLCJRITEMCD7                        " _
            & "       , SLCJRITEMCD8                        " _
            & "       , SLCJRITEMCD9                        " _
            & "       , SLCJRITEMCD10                       " _
            & "       , SLCJRITEMCDCOND                     " _
            & "       , SPRUSEFEE                           " _
            & "       , SPRUSEFEERATE                       " _
            & "       , SPRUSEFEERATEROUND                  " _
            & "       , SPRUSEFEERATEADDSUB                 " _
            & "       , SPRUSEFEERATEADDSUBCOND             " _
            & "       , SPRROUNDPOINTKBN                    " _
            & "       , SPRUSEFREESPE                       " _
            & "       , SPRNITTSUFREESENDFEE                " _
            & "       , SPRMANAGEFEE                        " _
            & "       , SPRSHIPBURDENFEE                    " _
            & "       , SPRSHIPFEE                          " _
            & "       , SPRARRIVEFEE                        " _
            & "       , SPRPICKUPFEE                        " _
            & "       , SPRDELIVERYFEE                      " _
            & "       , SPROTHER1                           " _
            & "       , SPROTHER2                           " _
            & "       , SPRFITKBN                           " _
            & "       , SPRCONTRACTCD                       " _
            & "       , INITYMD                             " _
            & "       , INITUSER                            " _
            & "       , INITTERMID                          " _
            & "       , INITPGID                            " _
            & "       , UPDYMD                              " _
            & "       , UPDUSER                             " _
            & "       , UPDTERMID                           " _
            & "       , UPDPGID                             " _
            & "       , RECEIVEYMD)                         " _
            & "     VALUES                                  " _
            & "        (@P00                                " _
            & "       , @P01                                " _
            & "       , @P02                                " _
            & "       , @P03                                " _
            & "       , @P04                                " _
            & "       , @P05                                " _
            & "       , @P06                                " _
            & "       , @P07                                " _
            & "       , @P08                                " _
            & "       , @P09                                " _
            & "       , @P10                                " _
            & "       , @P11                                " _
            & "       , @P12                                " _
            & "       , @P13                                " _
            & "       , @P14                                " _
            & "       , @P15                                " _
            & "       , @P16                                " _
            & "       , @P17                                " _
            & "       , @P18                                " _
            & "       , @P19                                " _
            & "       , @P20                                " _
            & "       , @P21                                " _
            & "       , @P22                                " _
            & "       , @P23                                " _
            & "       , @P24                                " _
            & "       , @P25                                " _
            & "       , @P26                                " _
            & "       , @P27                                " _
            & "       , @P28                                " _
            & "       , @P29                                " _
            & "       , @P30                                " _
            & "       , @P31                                " _
            & "       , @P32                                " _
            & "       , @P33                                " _
            & "       , @P34                                " _
            & "       , @P35                                " _
            & "       , @P36                                " _
            & "       , @P37                                " _
            & "       , @P38                                " _
            & "       , @P39                                " _
            & "       , @P40                                " _
            & "       , @P41                                " _
            & "       , @P42                                " _
            & "       , @P43                                " _
            & "       , @P44                                " _
            & "       , @P45                                " _
            & "       , @P46                                " _
            & "       , @P47                                " _
            & "       , @P48                                " _
            & "       , @P49                                " _
            & "       , @P50                                " _
            & "       , @P51                                " _
            & "       , @P52                                " _
            & "       , @P53                                " _
            & "       , @P54                                " _
            & "       , @P55                                " _
            & "       , @P56                                " _
            & "       , @P57                                " _
            & "       , @P58                                " _
            & "       , @P59                                " _
            & "       , @P60                                " _
            & "       , @P61                                " _
            & "       , @P62                                " _
            & "       , @P63                                " _
            & "       , @P64                                " _
            & "       , @P65                                " _
            & "       , @P66                                " _
            & "       , @P67                                " _
            & "       , @P68                                " _
            & "       , @P69                                " _
            & "       , @P70                                " _
            & "       , @P72                                " _
            & "       , @P73                                " _
            & "       , @P74                                " _
            & "       , @P75                                " _
            & "       , @P76                                " _
            & "       , @P77                                " _
            & "       , @P78                                " _
            & "       , @P79                                " _
            & "       , @P80) ;                             " _
            & " CLOSE hensuu ;                              " _
            & " DEALLOCATE hensuu ;                         "

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As String =
              " Select                                     " _
            & "    DELFLG                                  " _
            & "  , ORGCODE                                 " _
            & "  , BIGCTNCD                                " _
            & "  , MIDDLECTNCD                             " _
            & "  , DEPSTATION                              " _
            & "  , DEPTRUSTEECD                            " _
            & "  , DEPTRUSTEESUBCD                         " _
            & "  , PRIORITYNO                              " _
            & "  , PURPOSE                                 " _
            & "  , SLCCTNTYPE                              " _
            & "  , SLCCTNSTNO                              " _
            & "  , SLCCTNENDNO                             " _
            & "  , SLCJRDEPBRANCHCD                        " _
            & "  , SLCDEPSHIPPERCD1                        " _
            & "  , SLCDEPSHIPPERCD2                        " _
            & "  , SLCDEPSHIPPERCD3                        " _
            & "  , SLCDEPSHIPPERCD4                        " _
            & "  , SLCDEPSHIPPERCD5                        " _
            & "  , SLCDEPSHIPPERCDCOND                     " _
            & "  , SLCJRARRBRANCHCD                        " _
            & "  , SLCJRARRBRANCHCDCOND                    " _
            & "  , SLCJOTARRORGCODE                        " _
            & "  , SLCJOTARRORGCODECOND                    " _
            & "  , SLCARRSTATION1                          " _
            & "  , SLCARRSTATION2                          " _
            & "  , SLCARRSTATION3                          " _
            & "  , SLCARRSTATION4                          " _
            & "  , SLCARRSTATION5                          " _
            & "  , SLCARRSTATION6                          " _
            & "  , SLCARRSTATION7                          " _
            & "  , SLCARRSTATION8                          " _
            & "  , SLCARRSTATION9                          " _
            & "  , SLCARRSTATION10                         " _
            & "  , SLCARRSTATIONCOND                       " _
            & "  , SLCARRTRUSTEECD                         " _
            & "  , SLCARRTRUSTEECDCOND                     " _
            & "  , SLCARRTRUSTEESUBCD                      " _
            & "  , SLCARRTRUSTEESUBCDCOND                  " _
            & "  , SLCSTMD                                 " _
            & "  , SLCENDMD                                " _
            & "  , SLCSTSHIPYMD                            " _
            & "  , SLCENDSHIPYMD                           " _
            & "  , SLCJRITEMCD1                            " _
            & "  , SLCJRITEMCD2                            " _
            & "  , SLCJRITEMCD3                            " _
            & "  , SLCJRITEMCD4                            " _
            & "  , SLCJRITEMCD5                            " _
            & "  , SLCJRITEMCD6                            " _
            & "  , SLCJRITEMCD7                            " _
            & "  , SLCJRITEMCD8                            " _
            & "  , SLCJRITEMCD9                            " _
            & "  , SLCJRITEMCD10                           " _
            & "  , SLCJRITEMCDCOND                         " _
            & "  , SPRUSEFEE                               " _
            & "  , SPRUSEFEERATE                           " _
            & "  , SPRUSEFEERATEROUND                      " _
            & "  , SPRUSEFEERATEADDSUB                     " _
            & "  , SPRUSEFEERATEADDSUBCOND                 " _
            & "  , SPRROUNDPOINTKBN                        " _
            & "  , SPRUSEFREESPE                           " _
            & "  , SPRNITTSUFREESENDFEE                    " _
            & "  , SPRMANAGEFEE                            " _
            & "  , SPRSHIPBURDENFEE                        " _
            & "  , SPRSHIPFEE                              " _
            & "  , SPRARRIVEFEE                            " _
            & "  , SPRPICKUPFEE                            " _
            & "  , SPRDELIVERYFEE                          " _
            & "  , SPROTHER1                               " _
            & "  , SPROTHER2                               " _
            & "  , SPRFITKBN                               " _
            & "  , SPRCONTRACTCD                           " _
            & "  , INITYMD                                 " _
            & "  , INITUSER                                " _
            & "  , INITTERMID                              " _
            & "  , INITPGID                                " _
            & "  , UPDYMD                                  " _
            & "  , UPDUSER                                 " _
            & "  , UPDTERMID                               " _
            & "  , UPDPGID                                 " _
            & "  , RECEIVEYMD                              " _
            & "  , UPDTIMSTP                               " _
            & " FROM                                       " _
            & "    LNG.LNM0016_REST1M                      " _
            & " WHERE                                      " _
            & "        ORGCODE         = @P01              " _
            & "    AND BIGCTNCD        = @P02              " _
            & "    AND MIDDLECTNCD     = @P03              " _
            & "    AND DEPSTATION      = @P04              " _
            & "    AND DEPTRUSTEECD    = @P05              " _
            & "    AND DEPTRUSTEESUBCD = @P06              " _
            & "    AND PRIORITYNO      = @P07              "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl, SQLcon)
                ' DB更新用パラメータ
                Dim PARA00 As MySqlParameter = SQLcmd.Parameters.Add("@P00", MySqlDbType.VarChar, 1)     '削除フラグ
                Dim PARA01 As MySqlParameter = SQLcmd.Parameters.Add("@P01", MySqlDbType.VarChar, 6)     '組織コード
                Dim PARA02 As MySqlParameter = SQLcmd.Parameters.Add("@P02", MySqlDbType.VarChar, 2)     '大分類コード
                Dim PARA03 As MySqlParameter = SQLcmd.Parameters.Add("@P03", MySqlDbType.VarChar, 2)     '中分類コード
                Dim PARA04 As MySqlParameter = SQLcmd.Parameters.Add("@P04", MySqlDbType.VarChar, 6)     '発駅コード
                Dim PARA05 As MySqlParameter = SQLcmd.Parameters.Add("@P05", MySqlDbType.VarChar, 5)     '発受託人コード
                Dim PARA06 As MySqlParameter = SQLcmd.Parameters.Add("@P06", MySqlDbType.VarChar, 3)     '発受託人サブコード
                Dim PARA07 As MySqlParameter = SQLcmd.Parameters.Add("@P07", MySqlDbType.VarChar, 5)     '優先順位
                Dim PARA08 As MySqlParameter = SQLcmd.Parameters.Add("@P08", MySqlDbType.VarChar, 42)    '使用目的
                Dim PARA09 As MySqlParameter = SQLcmd.Parameters.Add("@P09", MySqlDbType.VarChar, 5)     '選択比較項目-コンテナ記号
                Dim PARA10 As MySqlParameter = SQLcmd.Parameters.Add("@P10", MySqlDbType.VarChar, 8)     '選択比較項目-コンテナ番号（開始）
                Dim PARA11 As MySqlParameter = SQLcmd.Parameters.Add("@P11", MySqlDbType.VarChar, 8)     '選択比較項目-コンテナ番号（終了）
                Dim PARA12 As MySqlParameter = SQLcmd.Parameters.Add("@P12", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ発支社支店コード
                Dim PARA13 As MySqlParameter = SQLcmd.Parameters.Add("@P13", MySqlDbType.VarChar, 6)     '選択比較項目-発荷主コード１
                Dim PARA14 As MySqlParameter = SQLcmd.Parameters.Add("@P14", MySqlDbType.VarChar, 6)     '選択比較項目-発荷主コード２
                Dim PARA15 As MySqlParameter = SQLcmd.Parameters.Add("@P15", MySqlDbType.VarChar, 6)     '選択比較項目-発荷主コード３
                Dim PARA16 As MySqlParameter = SQLcmd.Parameters.Add("@P16", MySqlDbType.VarChar, 6)     '選択比較項目-発荷主コード４
                Dim PARA17 As MySqlParameter = SQLcmd.Parameters.Add("@P17", MySqlDbType.VarChar, 6)     '選択比較項目-発荷主コード５
                Dim PARA18 As MySqlParameter = SQLcmd.Parameters.Add("@P18", MySqlDbType.VarChar, 1)     '選択比較項目-発荷主ＣＤ比較条件
                Dim PARA19 As MySqlParameter = SQLcmd.Parameters.Add("@P19", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ着支社支店コード
                Dim PARA20 As MySqlParameter = SQLcmd.Parameters.Add("@P20", MySqlDbType.VarChar, 1)     '選択比較項目-ＪＲ着支社支店ＣＤ比較
                Dim PARA21 As MySqlParameter = SQLcmd.Parameters.Add("@P21", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＯＴ着組織コード
                Dim PARA22 As MySqlParameter = SQLcmd.Parameters.Add("@P22", MySqlDbType.VarChar, 1)     '選択比較項目-ＪＯＴ着組織ＣＤ比較
                Dim PARA23 As MySqlParameter = SQLcmd.Parameters.Add("@P23", MySqlDbType.VarChar, 6)     '選択比較項目-着駅コード１
                Dim PARA24 As MySqlParameter = SQLcmd.Parameters.Add("@P24", MySqlDbType.VarChar, 6)     '選択比較項目-着駅コード２
                Dim PARA25 As MySqlParameter = SQLcmd.Parameters.Add("@P25", MySqlDbType.VarChar, 6)     '選択比較項目-着駅コード３
                Dim PARA26 As MySqlParameter = SQLcmd.Parameters.Add("@P26", MySqlDbType.VarChar, 6)     '選択比較項目-着駅コード４
                Dim PARA27 As MySqlParameter = SQLcmd.Parameters.Add("@P27", MySqlDbType.VarChar, 6)     '選択比較項目-着駅コード５
                Dim PARA28 As MySqlParameter = SQLcmd.Parameters.Add("@P28", MySqlDbType.VarChar, 6)     '選択比較項目-着駅コード６
                Dim PARA29 As MySqlParameter = SQLcmd.Parameters.Add("@P29", MySqlDbType.VarChar, 6)     '選択比較項目-着駅コード７
                Dim PARA30 As MySqlParameter = SQLcmd.Parameters.Add("@P30", MySqlDbType.VarChar, 6)     '選択比較項目-着駅コード８
                Dim PARA31 As MySqlParameter = SQLcmd.Parameters.Add("@P31", MySqlDbType.VarChar, 6)     '選択比較項目-着駅コード９
                Dim PARA32 As MySqlParameter = SQLcmd.Parameters.Add("@P32", MySqlDbType.VarChar, 6)     '選択比較項目-着駅コード１０
                Dim PARA33 As MySqlParameter = SQLcmd.Parameters.Add("@P33", MySqlDbType.VarChar, 1)     '選択比較項目-着駅コード比較条件
                Dim PARA34 As MySqlParameter = SQLcmd.Parameters.Add("@P34", MySqlDbType.VarChar, 5)     '選択比較項目-着受託人コード
                Dim PARA35 As MySqlParameter = SQLcmd.Parameters.Add("@P35", MySqlDbType.VarChar, 1)     '選択比較項目-着受託人ＣＤ比較条件
                Dim PARA36 As MySqlParameter = SQLcmd.Parameters.Add("@P36", MySqlDbType.VarChar, 3)     '選択比較項目-着受託人サブコード
                Dim PARA37 As MySqlParameter = SQLcmd.Parameters.Add("@P37", MySqlDbType.VarChar, 1)     '選択比較項目-着受託人サブＣＤ比較
                Dim PARA38 As MySqlParameter = SQLcmd.Parameters.Add("@P38", MySqlDbType.VarChar, 4)     '選択比較項目-開始月日
                Dim PARA39 As MySqlParameter = SQLcmd.Parameters.Add("@P39", MySqlDbType.VarChar, 4)     '選択比較項目-終了月日
                Dim PARA40 As MySqlParameter = SQLcmd.Parameters.Add("@P40", MySqlDbType.Date)            '選択比較項目-開始発送年月日
                Dim PARA41 As MySqlParameter = SQLcmd.Parameters.Add("@P41", MySqlDbType.Date)            '選択比較項目-終了発送年月日
                Dim PARA42 As MySqlParameter = SQLcmd.Parameters.Add("@P42", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ品目コード１
                Dim PARA43 As MySqlParameter = SQLcmd.Parameters.Add("@P43", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ品目コード２
                Dim PARA44 As MySqlParameter = SQLcmd.Parameters.Add("@P44", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ品目コード３
                Dim PARA45 As MySqlParameter = SQLcmd.Parameters.Add("@P45", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ品目コード４
                Dim PARA46 As MySqlParameter = SQLcmd.Parameters.Add("@P46", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ品目コード５
                Dim PARA47 As MySqlParameter = SQLcmd.Parameters.Add("@P47", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ品目コード６
                Dim PARA48 As MySqlParameter = SQLcmd.Parameters.Add("@P48", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ品目コード７
                Dim PARA49 As MySqlParameter = SQLcmd.Parameters.Add("@P49", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ品目コード８
                Dim PARA50 As MySqlParameter = SQLcmd.Parameters.Add("@P50", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ品目コード９
                Dim PARA51 As MySqlParameter = SQLcmd.Parameters.Add("@P51", MySqlDbType.VarChar, 6)     '選択比較項目-ＪＲ品目コード１０
                Dim PARA52 As MySqlParameter = SQLcmd.Parameters.Add("@P52", MySqlDbType.VarChar, 1)     '選択比較項目-ＪＲ品目コード比較
                Dim PARA53 As MySqlParameter = SQLcmd.Parameters.Add("@P53", MySqlDbType.VarChar, 7)     '特例置換項目-使用料金額
                Dim PARA54 As MySqlParameter = SQLcmd.Parameters.Add("@P54", MySqlDbType.Decimal, 5, 4)   '特例置換項目-使用料率
                Dim PARA55 As MySqlParameter = SQLcmd.Parameters.Add("@P55", MySqlDbType.VarChar, 2)     '特例置換項目-使用料率端数整理
                Dim PARA56 As MySqlParameter = SQLcmd.Parameters.Add("@P56", MySqlDbType.VarChar, 7)     '特例置換項目-使用料率加減額
                Dim PARA57 As MySqlParameter = SQLcmd.Parameters.Add("@P57", MySqlDbType.VarChar, 2)     '特例置換項目-使用料率加減額端数整理
                Dim PARA58 As MySqlParameter = SQLcmd.Parameters.Add("@P58", MySqlDbType.VarChar, 2)     '特例置換項目-端数処理時点区分
                Dim PARA59 As MySqlParameter = SQLcmd.Parameters.Add("@P59", MySqlDbType.VarChar, 2)     '特例置換項目-使用料無料特認
                Dim PARA60 As MySqlParameter = SQLcmd.Parameters.Add("@P60", MySqlDbType.VarChar, 7)     '特例置換項目-通運負担回送運賃
                Dim PARA61 As MySqlParameter = SQLcmd.Parameters.Add("@P61", MySqlDbType.VarChar, 7)     '特例置換項目-運行管理料
                Dim PARA62 As MySqlParameter = SQLcmd.Parameters.Add("@P62", MySqlDbType.VarChar, 7)     '特例置換項目-荷主負担運賃
                Dim PARA63 As MySqlParameter = SQLcmd.Parameters.Add("@P63", MySqlDbType.VarChar, 7)     '特例置換項目-発送料
                Dim PARA64 As MySqlParameter = SQLcmd.Parameters.Add("@P64", MySqlDbType.VarChar, 7)     '特例置換項目-到着料
                Dim PARA65 As MySqlParameter = SQLcmd.Parameters.Add("@P65", MySqlDbType.VarChar, 7)     '特例置換項目-集荷料
                Dim PARA66 As MySqlParameter = SQLcmd.Parameters.Add("@P66", MySqlDbType.VarChar, 7)     '特例置換項目-配達料
                Dim PARA67 As MySqlParameter = SQLcmd.Parameters.Add("@P67", MySqlDbType.VarChar, 7)     '特例置換項目-その他１
                Dim PARA68 As MySqlParameter = SQLcmd.Parameters.Add("@P68", MySqlDbType.VarChar, 7)     '特例置換項目-その他２
                Dim PARA69 As MySqlParameter = SQLcmd.Parameters.Add("@P69", MySqlDbType.VarChar, 2)     '特例置換項目-適合区分
                Dim PARA70 As MySqlParameter = SQLcmd.Parameters.Add("@P70", MySqlDbType.VarChar, 5)     '特例置換項目-契約コード
                Dim PARA72 As MySqlParameter = SQLcmd.Parameters.Add("@P72", MySqlDbType.DateTime)        '登録年月日
                Dim PARA73 As MySqlParameter = SQLcmd.Parameters.Add("@P73", MySqlDbType.VarChar, 20)    '登録ユーザーＩＤ
                Dim PARA74 As MySqlParameter = SQLcmd.Parameters.Add("@P74", MySqlDbType.VarChar, 20)    '登録端末
                Dim PARA75 As MySqlParameter = SQLcmd.Parameters.Add("@P75", MySqlDbType.VarChar, 40)    '登録プログラムＩＤ
                Dim PARA76 As MySqlParameter = SQLcmd.Parameters.Add("@P76", MySqlDbType.DateTime)        '更新年月日
                Dim PARA77 As MySqlParameter = SQLcmd.Parameters.Add("@P77", MySqlDbType.VarChar, 20)    '更新ユーザーＩＤ
                Dim PARA78 As MySqlParameter = SQLcmd.Parameters.Add("@P78", MySqlDbType.VarChar, 20)    '更新端末
                Dim PARA79 As MySqlParameter = SQLcmd.Parameters.Add("@P79", MySqlDbType.VarChar, 40)    '更新プログラムＩＤ
                Dim PARA80 As MySqlParameter = SQLcmd.Parameters.Add("@P80", MySqlDbType.DateTime)        '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JPARA01 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P01", MySqlDbType.VarChar, 6)  '組織コード
                Dim JPARA02 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P02", MySqlDbType.VarChar, 2)  '大分類コード
                Dim JPARA03 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P03", MySqlDbType.VarChar, 2)  '中分類コード
                Dim JPARA04 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P04", MySqlDbType.VarChar, 6)  '発駅コード
                Dim JPARA05 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P05", MySqlDbType.VarChar, 5)  '発受託人コード
                Dim JPARA06 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P06", MySqlDbType.VarChar, 3)  '発受託人サブコード
                Dim JPARA07 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P07", MySqlDbType.VarChar, 5)  '優先順位

                Dim LNM0016row As DataRow = LNM0016INPtbl.Rows(0)

                Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA00.Value = LNM0016row("DELFLG")                                                '削除フラグ
                PARA01.Value = LNM0016row("ORGCODE")                                               '組織コード
                PARA02.Value = LNM0016row("BIGCTNCD")                                              '大分類コード
                PARA03.Value = LNM0016row("MIDDLECTNCD")                                           '中分類コード
                PARA04.Value = LNM0016row("DEPSTATION")                                            '発駅コード
                PARA05.Value = LNM0016row("DEPTRUSTEECD")                                          '発受託人コード
                PARA06.Value = LNM0016row("DEPTRUSTEESUBCD")                                       '発受託人サブコード
                PARA07.Value = LNM0016row("PRIORITYNO")                                            '優先順位
                PARA08.Value = LNM0016row("PURPOSE")                                               '使用目的
                If String.IsNullOrEmpty(LNM0016row("SLCCTNTYPE")) Then                             '選択比較項目-コンテナ記号
                    PARA09.Value = DBNull.Value
                Else
                    PARA09.Value = LNM0016row("SLCCTNTYPE")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCCTNSTNO")) Then                             '選択比較項目-コンテナ番号（開始）
                    PARA10.Value = DBNull.Value
                Else
                    PARA10.Value = LNM0016row("SLCCTNSTNO")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCCTNENDNO")) Then                            '選択比較項目-コンテナ番号（終了）
                    PARA11.Value = DBNull.Value
                Else
                    PARA11.Value = LNM0016row("SLCCTNENDNO")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRDEPBRANCHCD")) Then                       '選択比較項目-ＪＲ発支社支店コード
                    PARA12.Value = DBNull.Value
                Else
                    PARA12.Value = CInt(LNM0016row("SLCJRDEPBRANCHCD"))
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCDEPSHIPPERCD1")) Then                       '選択比較項目-発荷主コード１
                    PARA13.Value = DBNull.Value
                Else
                    PARA13.Value = LNM0016row("SLCDEPSHIPPERCD1")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCDEPSHIPPERCD2")) Then                       '選択比較項目-発荷主コード２
                    PARA14.Value = DBNull.Value
                Else
                    PARA14.Value = LNM0016row("SLCDEPSHIPPERCD2")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCDEPSHIPPERCD3")) Then                       '選択比較項目-発荷主コード３
                    PARA15.Value = DBNull.Value
                Else
                    PARA15.Value = LNM0016row("SLCDEPSHIPPERCD3")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCDEPSHIPPERCD4")) Then                       '選択比較項目-発荷主コード４
                    PARA16.Value = DBNull.Value
                Else
                    PARA16.Value = LNM0016row("SLCDEPSHIPPERCD4")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCDEPSHIPPERCD5")) Then                       '選択比較項目-発荷主コード５
                    PARA17.Value = DBNull.Value
                Else
                    PARA17.Value = LNM0016row("SLCDEPSHIPPERCD5")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCDEPSHIPPERCDCOND")) Then                    '選択比較項目-発荷主ＣＤ比較条件
                    PARA18.Value = "0"
                Else
                    PARA18.Value = LNM0016row("SLCDEPSHIPPERCDCOND")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRARRBRANCHCD")) Then                       '選択比較項目-ＪＲ着支社支店コード
                    PARA19.Value = DBNull.Value
                Else
                    PARA19.Value = CInt(LNM0016row("SLCJRARRBRANCHCD"))
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRARRBRANCHCDCOND")) Then                   '選択比較項目-ＪＲ着支社支店ＣＤ比較
                    PARA20.Value = "0"
                Else
                    PARA20.Value = LNM0016row("SLCJRARRBRANCHCDCOND")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJOTARRORGCODE")) Then                   '選択比較項目-ＪＯＴ着組織コード
                    PARA21.Value = DBNull.Value
                Else
                    PARA21.Value = LNM0016row("SLCJOTARRORGCODE")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJOTARRORGCODECOND")) Then                   '選択比較項目-ＪＯＴ着組織ＣＤ比較
                    PARA22.Value = "0"
                Else
                    PARA22.Value = LNM0016row("SLCJOTARRORGCODECOND")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRSTATION1")) Then                         '選択比較項目-着駅コード１
                    PARA23.Value = DBNull.Value
                Else
                    PARA23.Value = LNM0016row("SLCARRSTATION1")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRSTATION2")) Then                         '選択比較項目-着駅コード２
                    PARA24.Value = DBNull.Value
                Else
                    PARA24.Value = LNM0016row("SLCARRSTATION2")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRSTATION3")) Then                         '選択比較項目-着駅コード３
                    PARA25.Value = DBNull.Value
                Else
                    PARA25.Value = LNM0016row("SLCARRSTATION3")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRSTATION4")) Then                         '選択比較項目-着駅コード４
                    PARA26.Value = DBNull.Value
                Else
                    PARA26.Value = LNM0016row("SLCARRSTATION4")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRSTATION5")) Then                         '選択比較項目-着駅コード５
                    PARA27.Value = DBNull.Value
                Else
                    PARA27.Value = LNM0016row("SLCARRSTATION5")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRSTATION6")) Then                         '選択比較項目-着駅コード６
                    PARA28.Value = DBNull.Value
                Else
                    PARA28.Value = LNM0016row("SLCARRSTATION6")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRSTATION7")) Then                         '選択比較項目-着駅コード７
                    PARA29.Value = DBNull.Value
                Else
                    PARA29.Value = LNM0016row("SLCARRSTATION7")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRSTATION8")) Then                         '選択比較項目-着駅コード８
                    PARA30.Value = DBNull.Value
                Else
                    PARA30.Value = LNM0016row("SLCARRSTATION8")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRSTATION9")) Then                         '選択比較項目-着駅コード９
                    PARA31.Value = DBNull.Value
                Else
                    PARA31.Value = LNM0016row("SLCARRSTATION9")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRSTATION10")) Then                        '選択比較項目-着駅コード１０
                    PARA32.Value = DBNull.Value
                Else
                    PARA32.Value = LNM0016row("SLCARRSTATION10")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRSTATIONCOND")) Then                      '選択比較項目-着駅コード比較条件
                    PARA33.Value = "0"
                Else
                    PARA33.Value = LNM0016row("SLCARRSTATIONCOND")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRTRUSTEECD")) Then                        '選択比較項目-着受託人コード
                    PARA34.Value = DBNull.Value
                Else
                    PARA34.Value = LNM0016row("SLCARRTRUSTEECD")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRTRUSTEECDCOND")) Then                    '選択比較項目-着受託人ＣＤ比較条件
                    PARA35.Value = "0"
                Else
                    PARA35.Value = LNM0016row("SLCARRTRUSTEECDCOND")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRTRUSTEESUBCD")) Then                     '選択比較項目-着受託人サブコード
                    PARA36.Value = DBNull.Value
                Else
                    PARA36.Value = LNM0016row("SLCARRTRUSTEESUBCD")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCARRTRUSTEESUBCDCOND")) Then                 '選択比較項目-着受託人サブＣＤ比較
                    PARA37.Value = "0"
                Else
                    PARA37.Value = LNM0016row("SLCARRTRUSTEESUBCDCOND")
                End If
                PARA38.Value = LNM0016row("SLCSTMD")                                               '選択比較項目-開始月日
                PARA39.Value = LNM0016row("SLCENDMD")                                              '選択比較項目-終了月日
                If String.IsNullOrEmpty(LNM0016row("SLCSTSHIPYMD")) Then                           '選択比較項目-開始発送年月日
                    PARA40.Value = DBNull.Value
                Else
                    PARA40.Value = LNM0016row("SLCSTSHIPYMD")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCENDSHIPYMD")) Then                          '選択比較項目-終了発送年月日
                    PARA41.Value = DBNull.Value
                Else
                    PARA41.Value = LNM0016row("SLCENDSHIPYMD")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRITEMCD1")) Then                           '選択比較項目-ＪＲ品目コード１
                    PARA42.Value = DBNull.Value
                Else
                    PARA42.Value = LNM0016row("SLCJRITEMCD1")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRITEMCD2")) Then                           '選択比較項目-ＪＲ品目コード２
                    PARA43.Value = DBNull.Value
                Else
                    PARA43.Value = LNM0016row("SLCJRITEMCD2")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRITEMCD3")) Then                           '選択比較項目-ＪＲ品目コード３
                    PARA44.Value = DBNull.Value
                Else
                    PARA44.Value = LNM0016row("SLCJRITEMCD3")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRITEMCD4")) Then                           '選択比較項目-ＪＲ品目コード４
                    PARA45.Value = DBNull.Value
                Else
                    PARA45.Value = LNM0016row("SLCJRITEMCD4")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRITEMCD5")) Then                           '選択比較項目-ＪＲ品目コード５
                    PARA46.Value = DBNull.Value
                Else
                    PARA46.Value = LNM0016row("SLCJRITEMCD5")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRITEMCD6")) Then                           '選択比較項目-ＪＲ品目コード６
                    PARA47.Value = DBNull.Value
                Else
                    PARA47.Value = LNM0016row("SLCJRITEMCD6")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRITEMCD7")) Then                           '選択比較項目-ＪＲ品目コード７
                    PARA48.Value = DBNull.Value
                Else
                    PARA48.Value = LNM0016row("SLCJRITEMCD7")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRITEMCD8")) Then                           '選択比較項目-ＪＲ品目コード８
                    PARA49.Value = DBNull.Value
                Else
                    PARA49.Value = LNM0016row("SLCJRITEMCD8")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRITEMCD9")) Then                           '選択比較項目-ＪＲ品目コード９
                    PARA50.Value = DBNull.Value
                Else
                    PARA50.Value = LNM0016row("SLCJRITEMCD9")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRITEMCD10")) Then                          '選択比較項目-ＪＲ品目コード１０
                    PARA51.Value = DBNull.Value
                Else
                    PARA51.Value = LNM0016row("SLCJRITEMCD10")
                End If
                If String.IsNullOrEmpty(LNM0016row("SLCJRITEMCDCOND")) Then                        '選択比較項目-ＪＲ品目コード比較
                    PARA52.Value = DBNull.Value
                Else
                    PARA52.Value = LNM0016row("SLCJRITEMCDCOND")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRUSEFEE")) Then                              '特例置換項目-使用料金額
                    PARA53.Value = DBNull.Value
                Else
                    PARA53.Value = LNM0016row("SPRUSEFEE")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRUSEFEERATE")) Then                          '特例置換項目-使用料率
                    PARA54.Value = DBNull.Value
                Else
                    PARA54.Value = LNM0016row("SPRUSEFEERATE")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRUSEFEERATEROUND")) Then                     '特例置換項目-使用料率端数整理
                    PARA55.Value = "0"
                Else
                    PARA55.Value = LNM0016row("SPRUSEFEERATEROUND")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRUSEFEERATEADDSUB")) Then                    '特例置換項目-使用料率加減額
                    PARA56.Value = DBNull.Value
                Else
                    PARA56.Value = LNM0016row("SPRUSEFEERATEADDSUB")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRUSEFEERATEADDSUBCOND")) Then                '特例置換項目-使用料率加減額端数整理
                    PARA57.Value = "0"
                Else
                    PARA57.Value = LNM0016row("SPRUSEFEERATEADDSUBCOND")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRROUNDPOINTKBN")) Then                       '特例置換項目-端数処理時点区分
                    PARA58.Value = DBNull.Value
                Else
                    PARA58.Value = LNM0016row("SPRROUNDPOINTKBN")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRUSEFREESPE")) Then                          '特例置換項目-使用料無料特認
                    PARA59.Value = "0"
                Else
                    PARA59.Value = LNM0016row("SPRUSEFREESPE")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRNITTSUFREESENDFEE")) Then                   '特例置換項目-通運負担回送運賃
                    PARA60.Value = DBNull.Value
                Else
                    PARA60.Value = LNM0016row("SPRNITTSUFREESENDFEE")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRMANAGEFEE")) Then                           '特例置換項目-運行管理料
                    PARA61.Value = DBNull.Value
                Else
                    PARA61.Value = LNM0016row("SPRMANAGEFEE")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRSHIPBURDENFEE")) Then                       '特例置換項目-荷主負担運賃
                    PARA62.Value = DBNull.Value
                Else
                    PARA62.Value = LNM0016row("SPRSHIPBURDENFEE")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRSHIPFEE")) Then                             '特例置換項目-発送料
                    PARA63.Value = DBNull.Value
                Else
                    PARA63.Value = LNM0016row("SPRSHIPFEE")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRARRIVEFEE")) Then                           '特例置換項目-到着料
                    PARA64.Value = DBNull.Value
                Else
                    PARA64.Value = LNM0016row("SPRARRIVEFEE")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRPICKUPFEE")) Then                           '特例置換項目-集荷料
                    PARA65.Value = DBNull.Value
                Else
                    PARA65.Value = LNM0016row("SPRPICKUPFEE")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRDELIVERYFEE")) Then                         '特例置換項目-配達料
                    PARA66.Value = DBNull.Value
                Else
                    PARA66.Value = LNM0016row("SPRDELIVERYFEE")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPROTHER1")) Then                              '特例置換項目-その他１
                    PARA67.Value = DBNull.Value
                Else
                    PARA67.Value = LNM0016row("SPROTHER1")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPROTHER2")) Then                              '特例置換項目-その他２
                    PARA68.Value = DBNull.Value
                Else
                    PARA68.Value = LNM0016row("SPROTHER2")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRFITKBN")) Then                              '特例置換項目-適合区分
                    PARA69.Value = DBNull.Value
                Else
                    PARA69.Value = LNM0016row("SPRFITKBN")
                End If
                If String.IsNullOrEmpty(LNM0016row("SPRCONTRACTCD")) Then                              '特例置換項目-契約コード
                    PARA70.Value = DBNull.Value
                Else
                    PARA70.Value = LNM0016row("SPRCONTRACTCD")
                End If





                PARA72.Value = WW_NOW                                                              '登録年月日
                PARA73.Value = Master.USERID                                                       '登録ユーザーＩＤ
                PARA74.Value = Master.USERTERMID                                                   '登録端末
                PARA75.Value = Me.GetType().BaseType.Name                                          '登録プログラムＩＤ
                PARA76.Value = WW_NOW                                                              '更新年月日
                PARA77.Value = Master.USERID                                                       '更新ユーザーＩＤ
                PARA78.Value = Master.USERTERMID                                                   '更新端末
                PARA79.Value = Me.GetType().BaseType.Name                                          '更新プログラムＩＤ
                PARA80.Value = C_DEFAULT_YMD                                                       '集信日時
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA01.Value = LNM0016row("ORGCODE")
                JPARA02.Value = LNM0016row("BIGCTNCD")
                JPARA03.Value = LNM0016row("MIDDLECTNCD")
                JPARA04.Value = LNM0016row("DEPSTATION")
                JPARA05.Value = LNM0016row("DEPTRUSTEECD")
                JPARA06.Value = LNM0016row("DEPTRUSTEESUBCD")
                JPARA07.Value = LNM0016row("PRIORITYNO")

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0016UPDtbl) Then
                        LNM0016UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0016UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0016UPDtbl.Clear()
                    LNM0016UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0016UPDrow As DataRow In LNM0016UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0016C"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0016UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0016C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0016C UPDATE_INSERT"
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
    Protected Sub REST1MEXISTS(ByVal SQLcon As MySqlConnection, ByRef WW_MODIFYKBN As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '使用料特例マスタ１に同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        ORGCODE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0016_REST1M")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        ORGCODE         = @ORGCODE")
        SQLStr.AppendLine("    AND BIGCTNCD        = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD     = @MIDDLECTNCD")
        SQLStr.AppendLine("    AND DEPSTATION      = @DEPSTATION")
        SQLStr.AppendLine("    AND DEPTRUSTEECD    = @DEPTRUSTEECD")
        SQLStr.AppendLine("    AND DEPTRUSTEESUBCD = @DEPTRUSTEESUBCD")
        SQLStr.AppendLine("    AND PRIORITYNO      = @PRIORITYNO")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位

                Dim LNM0016row As DataRow = LNM0016INPtbl.Rows(0)

                P_ORGCODE.Value = LNM0016row("ORGCODE")               '組織コード
                P_BIGCTNCD.Value = LNM0016row("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = LNM0016row("MIDDLECTNCD")               '中分類コード
                P_DEPSTATION.Value = LNM0016row("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = LNM0016row("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = LNM0016row("DEPTRUSTEESUBCD")               '発受託人サブコード
                P_PRIORITYNO.Value = LNM0016row("PRIORITYNO")               '優先順位

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
                        WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0016C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0016C Select"
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
        SQLStr.AppendLine(" INSERT INTO LNG.LNT0095_REST1HIST ")
        SQLStr.AppendLine("  (  ")
        SQLStr.AppendLine("         ORGCODE  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,SLCCTNTYPE  ")
        SQLStr.AppendLine("        ,SLCCTNSTNO  ")
        SQLStr.AppendLine("        ,SLCCTNENDNO  ")
        SQLStr.AppendLine("        ,SLCJRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD1  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD2  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD3  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD4  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD5  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCDCOND  ")
        SQLStr.AppendLine("        ,SLCJRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,SLCJRARRBRANCHCDCOND  ")
        SQLStr.AppendLine("        ,SLCJOTARRORGCODE  ")
        SQLStr.AppendLine("        ,SLCJOTARRORGCODECOND  ")
        SQLStr.AppendLine("        ,SLCARRSTATION1  ")
        SQLStr.AppendLine("        ,SLCARRSTATION2  ")
        SQLStr.AppendLine("        ,SLCARRSTATION3  ")
        SQLStr.AppendLine("        ,SLCARRSTATION4  ")
        SQLStr.AppendLine("        ,SLCARRSTATION5  ")
        SQLStr.AppendLine("        ,SLCARRSTATION6  ")
        SQLStr.AppendLine("        ,SLCARRSTATION7  ")
        SQLStr.AppendLine("        ,SLCARRSTATION8  ")
        SQLStr.AppendLine("        ,SLCARRSTATION9  ")
        SQLStr.AppendLine("        ,SLCARRSTATION10  ")
        SQLStr.AppendLine("        ,SLCARRSTATIONCOND  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECDCOND  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCDCOND  ")
        SQLStr.AppendLine("        ,SLCSTMD  ")
        SQLStr.AppendLine("        ,SLCENDMD  ")
        SQLStr.AppendLine("        ,SLCSTSHIPYMD  ")
        SQLStr.AppendLine("        ,SLCENDSHIPYMD  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD1  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD2  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD3  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD4  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD5  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD6  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD7  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD8  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD9  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD10  ")
        SQLStr.AppendLine("        ,SLCJRITEMCDCOND  ")
        SQLStr.AppendLine("        ,SPRUSEFEE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEROUND  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUB  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUBCOND  ")
        SQLStr.AppendLine("        ,SPRROUNDPOINTKBN  ")
        SQLStr.AppendLine("        ,SPRUSEFREESPE  ")
        SQLStr.AppendLine("        ,SPRNITTSUFREESENDFEE  ")
        SQLStr.AppendLine("        ,SPRMANAGEFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPBURDENFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPFEE  ")
        SQLStr.AppendLine("        ,SPRARRIVEFEE  ")
        SQLStr.AppendLine("        ,SPRPICKUPFEE  ")
        SQLStr.AppendLine("        ,SPRDELIVERYFEE  ")
        SQLStr.AppendLine("        ,SPROTHER1  ")
        SQLStr.AppendLine("        ,SPROTHER2  ")
        SQLStr.AppendLine("        ,SPRFITKBN  ")
        SQLStr.AppendLine("        ,SPRCONTRACTCD  ")
        SQLStr.AppendLine("        ,BEFOREORGCODE  ")
        SQLStr.AppendLine("        ,BEFORESLCJOTARRORGCODE  ")
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
        SQLStr.AppendLine("         ORGCODE  ")
        SQLStr.AppendLine("        ,BIGCTNCD  ")
        SQLStr.AppendLine("        ,MIDDLECTNCD  ")
        SQLStr.AppendLine("        ,DEPSTATION  ")
        SQLStr.AppendLine("        ,DEPTRUSTEECD  ")
        SQLStr.AppendLine("        ,DEPTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,PRIORITYNO  ")
        SQLStr.AppendLine("        ,PURPOSE  ")
        SQLStr.AppendLine("        ,SLCCTNTYPE  ")
        SQLStr.AppendLine("        ,SLCCTNSTNO  ")
        SQLStr.AppendLine("        ,SLCCTNENDNO  ")
        SQLStr.AppendLine("        ,SLCJRDEPBRANCHCD  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD1  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD2  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD3  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD4  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCD5  ")
        SQLStr.AppendLine("        ,SLCDEPSHIPPERCDCOND  ")
        SQLStr.AppendLine("        ,SLCJRARRBRANCHCD  ")
        SQLStr.AppendLine("        ,SLCJRARRBRANCHCDCOND  ")
        SQLStr.AppendLine("        ,SLCJOTARRORGCODE  ")
        SQLStr.AppendLine("        ,SLCJOTARRORGCODECOND  ")
        SQLStr.AppendLine("        ,SLCARRSTATION1  ")
        SQLStr.AppendLine("        ,SLCARRSTATION2  ")
        SQLStr.AppendLine("        ,SLCARRSTATION3  ")
        SQLStr.AppendLine("        ,SLCARRSTATION4  ")
        SQLStr.AppendLine("        ,SLCARRSTATION5  ")
        SQLStr.AppendLine("        ,SLCARRSTATION6  ")
        SQLStr.AppendLine("        ,SLCARRSTATION7  ")
        SQLStr.AppendLine("        ,SLCARRSTATION8  ")
        SQLStr.AppendLine("        ,SLCARRSTATION9  ")
        SQLStr.AppendLine("        ,SLCARRSTATION10  ")
        SQLStr.AppendLine("        ,SLCARRSTATIONCOND  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEECDCOND  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCD  ")
        SQLStr.AppendLine("        ,SLCARRTRUSTEESUBCDCOND  ")
        SQLStr.AppendLine("        ,SLCSTMD  ")
        SQLStr.AppendLine("        ,SLCENDMD  ")
        SQLStr.AppendLine("        ,SLCSTSHIPYMD  ")
        SQLStr.AppendLine("        ,SLCENDSHIPYMD  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD1  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD2  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD3  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD4  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD5  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD6  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD7  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD8  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD9  ")
        SQLStr.AppendLine("        ,SLCJRITEMCD10  ")
        SQLStr.AppendLine("        ,SLCJRITEMCDCOND  ")
        SQLStr.AppendLine("        ,SPRUSEFEE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATE  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEROUND  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUB  ")
        SQLStr.AppendLine("        ,SPRUSEFEERATEADDSUBCOND  ")
        SQLStr.AppendLine("        ,SPRROUNDPOINTKBN  ")
        SQLStr.AppendLine("        ,SPRUSEFREESPE  ")
        SQLStr.AppendLine("        ,SPRNITTSUFREESENDFEE  ")
        SQLStr.AppendLine("        ,SPRMANAGEFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPBURDENFEE  ")
        SQLStr.AppendLine("        ,SPRSHIPFEE  ")
        SQLStr.AppendLine("        ,SPRARRIVEFEE  ")
        SQLStr.AppendLine("        ,SPRPICKUPFEE  ")
        SQLStr.AppendLine("        ,SPRDELIVERYFEE  ")
        SQLStr.AppendLine("        ,SPROTHER1  ")
        SQLStr.AppendLine("        ,SPROTHER2  ")
        SQLStr.AppendLine("        ,SPRFITKBN  ")
        SQLStr.AppendLine("        ,SPRCONTRACTCD  ")
        SQLStr.AppendLine("        ,BEFOREORGCODE  ")
        SQLStr.AppendLine("        ,BEFORESLCJOTARRORGCODE  ")
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
        SQLStr.AppendLine("        LNG.LNM0016_REST1M")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        ORGCODE         = @ORGCODE")
        SQLStr.AppendLine("    AND BIGCTNCD        = @BIGCTNCD")
        SQLStr.AppendLine("    AND MIDDLECTNCD     = @MIDDLECTNCD")
        SQLStr.AppendLine("    AND DEPSTATION      = @DEPSTATION")
        SQLStr.AppendLine("    AND DEPTRUSTEECD    = @DEPTRUSTEECD")
        SQLStr.AppendLine("    AND DEPTRUSTEESUBCD = @DEPTRUSTEESUBCD")
        SQLStr.AppendLine("    AND PRIORITYNO      = @PRIORITYNO")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6)         '組織コード
                Dim P_BIGCTNCD As MySqlParameter = SQLcmd.Parameters.Add("@BIGCTNCD", MySqlDbType.VarChar, 2)         '大分類コード
                Dim P_MIDDLECTNCD As MySqlParameter = SQLcmd.Parameters.Add("@MIDDLECTNCD", MySqlDbType.VarChar, 2)         '中分類コード
                Dim P_DEPSTATION As MySqlParameter = SQLcmd.Parameters.Add("@DEPSTATION", MySqlDbType.VarChar, 6)         '発駅コード
                Dim P_DEPTRUSTEECD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEECD", MySqlDbType.VarChar, 5)         '発受託人コード
                Dim P_DEPTRUSTEESUBCD As MySqlParameter = SQLcmd.Parameters.Add("@DEPTRUSTEESUBCD", MySqlDbType.VarChar, 3)         '発受託人サブコード
                Dim P_PRIORITYNO As MySqlParameter = SQLcmd.Parameters.Add("@PRIORITYNO", MySqlDbType.VarChar, 5)         '優先順位

                Dim P_OPERATEKBN As MySqlParameter = SQLcmd.Parameters.Add("@OPERATEKBN", MySqlDbType.VarChar, 1)       '操作区分
                Dim P_MODIFYKBN As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYKBN", MySqlDbType.VarChar, 1)         '変更区分
                Dim P_MODIFYYMD As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYYMD", MySqlDbType.DateTime)         '変更日時
                Dim P_MODIFYUSER As MySqlParameter = SQLcmd.Parameters.Add("@MODIFYUSER", MySqlDbType.VarChar, 20)         '変更ユーザーＩＤ

                Dim P_INITYMD As MySqlParameter = SQLcmd.Parameters.Add("@INITYMD", MySqlDbType.DateTime)         '登録年月日
                Dim P_INITUSER As MySqlParameter = SQLcmd.Parameters.Add("@INITUSER", MySqlDbType.VarChar, 20)         '登録ユーザーＩＤ
                Dim P_INITTERMID As MySqlParameter = SQLcmd.Parameters.Add("@INITTERMID", MySqlDbType.VarChar, 20)         '登録端末
                Dim P_INITPGID As MySqlParameter = SQLcmd.Parameters.Add("@INITPGID", MySqlDbType.VarChar, 40)         '登録プログラムＩＤ

                Dim LNM0016row As DataRow = LNM0016INPtbl.Rows(0)

                ' DB更新
                P_ORGCODE.Value = LNM0016row("ORGCODE")               '組織コード
                P_BIGCTNCD.Value = LNM0016row("BIGCTNCD")               '大分類コード
                P_MIDDLECTNCD.Value = LNM0016row("MIDDLECTNCD")               '中分類コード
                P_DEPSTATION.Value = LNM0016row("DEPSTATION")               '発駅コード
                P_DEPTRUSTEECD.Value = LNM0016row("DEPTRUSTEECD")               '発受託人コード
                P_DEPTRUSTEESUBCD.Value = LNM0016row("DEPTRUSTEESUBCD")               '発受託人サブコード
                P_PRIORITYNO.Value = LNM0016row("PRIORITYNO")               '優先順位

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0016WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0016tbl.Rows(0)("DELFLG") = "0" And LNM0016row("DELFLG") = "1" Then
                        P_OPERATEKBN.Value = CInt(LNM0016WRKINC.OPERATEKBN.DELDATA).ToString
                    Else
                        P_OPERATEKBN.Value = CInt(LNM0016WRKINC.OPERATEKBN.UPDDATA).ToString
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNT0095_REST1HIST  INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                             'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:" + "LNT0095_REST1HIST  INSERT"
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
    ''' 詳細画面-更新ボタン押下
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_Click()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0016INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0016tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0016tbl, work.WF_SEL_INPTBL.Text)

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
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, "使用料特例１", needsPopUp:=True)
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
    Protected Sub DetailBoxToLNM0016INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)                    '削除フラグ
        Master.EraseCharToIgnore(TxtOrgCode.Text)                   '組織コード
        Master.EraseCharToIgnore(TxtBigCTNCD.Text)                  '大分類コード
        Master.EraseCharToIgnore(TxtMiddleCTNCD.Text)               '中分類コード
        Master.EraseCharToIgnore(TxtDepStation.Text)                '発駅コード
        Master.EraseCharToIgnore(TxtDepTrusteeCd.Text)              '発受託人コード
        Master.EraseCharToIgnore(TxtDepTrusteeSubCd.Text)           '発受託人サブコード
        Master.EraseCharToIgnore(TxtPriorityNo.Text)                '優先順位
        Master.EraseCharToIgnore(TxtPurpose.Text)                   '使用目的
        Master.EraseCharToIgnore(TxtCTNType.Text)                   '選択比較項目-コンテナ記号
        Master.EraseCharToIgnore(TxtCTNStNo.Text)                   '選択比較項目-コンテナ番号（開始）
        Master.EraseCharToIgnore(TxtCTNEndNo.Text)                  '選択比較項目-コンテナ番号（終了）
        Master.EraseCharToIgnore(TxtSlcJrDepBranchCd.Text)          '選択比較項目-ＪＲ発支社支店コード
        Master.EraseCharToIgnore(TxtSlcDepShipperCd1.Text)          '選択比較項目-発荷主コード１
        Master.EraseCharToIgnore(TxtSlcDepShipperCd2.Text)          '選択比較項目-発荷主コード２
        Master.EraseCharToIgnore(TxtSlcDepShipperCd3.Text)          '選択比較項目-発荷主コード３
        Master.EraseCharToIgnore(TxtSlcDepShipperCd4.Text)          '選択比較項目-発荷主コード４
        Master.EraseCharToIgnore(TxtSlcDepShipperCd5.Text)          '選択比較項目-発荷主コード５
        Master.EraseCharToIgnore(TxtSlcDepShipperCdCond.Text)       '選択比較項目-発荷主ＣＤ比較条件
        Master.EraseCharToIgnore(TxtSlcJrArrBranchCd.Text)          '選択比較項目-ＪＲ着支社支店コード
        Master.EraseCharToIgnore(TxtSlcJrArrBranchCdCond.Text)      '選択比較項目-ＪＲ着支社支店ＣＤ比較
        Master.EraseCharToIgnore(TxtSlcJotArrOrgCode.Text)          '選択比較項目-ＪＯＴ着組織コード
        Master.EraseCharToIgnore(TxtSlcJotArrOrgCodeCond.Text)      '選択比較項目-ＪＯＴ着組織ＣＤ比較
        Master.EraseCharToIgnore(TxtSlcArrStation1.Text)            '選択比較項目-着駅コード１
        Master.EraseCharToIgnore(TxtSlcArrStation2.Text)            '選択比較項目-着駅コード２
        Master.EraseCharToIgnore(TxtSlcArrStation3.Text)            '選択比較項目-着駅コード３
        Master.EraseCharToIgnore(TxtSlcArrStation4.Text)            '選択比較項目-着駅コード４
        Master.EraseCharToIgnore(TxtSlcArrStation5.Text)            '選択比較項目-着駅コード５
        Master.EraseCharToIgnore(TxtSlcArrStation6.Text)            '選択比較項目-着駅コード６
        Master.EraseCharToIgnore(TxtSlcArrStation7.Text)            '選択比較項目-着駅コード７
        Master.EraseCharToIgnore(TxtSlcArrStation8.Text)            '選択比較項目-着駅コード８
        Master.EraseCharToIgnore(TxtSlcArrStation9.Text)            '選択比較項目-着駅コード９
        Master.EraseCharToIgnore(TxtSlcArrStation10.Text)           '選択比較項目-着駅コード１０
        Master.EraseCharToIgnore(TxtSlcArrStationCond.Text)         '選択比較項目-着駅コード比較条件
        Master.EraseCharToIgnore(TxtSlcArrTrusteeCd.Text)           '選択比較項目-着受託人コード
        Master.EraseCharToIgnore(TxtSlcArrTrusteeCdCond.Text)       '選択比較項目-着受託人ＣＤ比較条件
        Master.EraseCharToIgnore(TxtSlcArrTrusteeSubCd.Text)        '選択比較項目-着受託人サブコード
        Master.EraseCharToIgnore(TxtSlcArrTrusteeSubCdCond.Text)    '選択比較項目-着受託人サブＣＤ比較
        Master.EraseCharToIgnore(TxtSlcStMD.Text)                   '選択比較項目-開始月日
        Master.EraseCharToIgnore(TxtSlcEndMD.Text)                  '選択比較項目-終了月日
        Master.EraseCharToIgnore(TxtSlcStShipMD.Text)               '選択比較項目-開始発送年月日
        Master.EraseCharToIgnore(TxtSlcEndShipMD.Text)              '選択比較項目-終了発送年月日
        Master.EraseCharToIgnore(TxtSlcJrItemCd1.Text)              '選択比較項目-ＪＲ品目コード１
        Master.EraseCharToIgnore(TxtSlcJrItemCd2.Text)              '選択比較項目-ＪＲ品目コード２
        Master.EraseCharToIgnore(TxtSlcJrItemCd3.Text)              '選択比較項目-ＪＲ品目コード３
        Master.EraseCharToIgnore(TxtSlcJrItemCd4.Text)              '選択比較項目-ＪＲ品目コード４
        Master.EraseCharToIgnore(TxtSlcJrItemCd5.Text)              '選択比較項目-ＪＲ品目コード５
        Master.EraseCharToIgnore(TxtSlcJrItemCd6.Text)              '選択比較項目-ＪＲ品目コード６
        Master.EraseCharToIgnore(TxtSlcJrItemCd7.Text)              '選択比較項目-ＪＲ品目コード７
        Master.EraseCharToIgnore(TxtSlcJrItemCd8.Text)              '選択比較項目-ＪＲ品目コード８
        Master.EraseCharToIgnore(TxtSlcJrItemCd9.Text)              '選択比較項目-ＪＲ品目コード９
        Master.EraseCharToIgnore(TxtSlcJrItemCd10.Text)             '選択比較項目-ＪＲ品目コード１０
        Master.EraseCharToIgnore(TxtSlcJrItemCdCond.Text)           '選択比較項目-ＪＲ品目コード比較
        Master.EraseCharToIgnore(TxtSprUseFee.Text)                 '特例置換項目-使用料金額
        Master.EraseCharToIgnore(TxtSprUseFeeRate.Text)             '特例置換項目-使用料率
        Master.EraseCharToIgnore(TxtSprUseFeeRateRound1.Text)       '特例置換項目-使用料率端数整理1
        Master.EraseCharToIgnore(TxtSprUseFeeRateRound2.Text)       '特例置換項目-使用料率端数整理2
        Master.EraseCharToIgnore(TxtSprUseFeeRateAddSub.Text)       '特例置換項目-使用料率加減額
        Master.EraseCharToIgnore(TxtSprUseFeeRateAddSubCond1.Text)  '特例置換項目-使用料率加減額端数整理1
        Master.EraseCharToIgnore(TxtSprUseFeeRateAddSubCond2.Text)  '特例置換項目-使用料率加減額端数整理2
        Master.EraseCharToIgnore(TxtSprRoundPointKbn.Text)          '特例置換項目-端数処理時点区分
        Master.EraseCharToIgnore(TxtSprUseFreeSpe.Text)             '特例置換項目-使用料無料特認
        Master.EraseCharToIgnore(TxtSprNittsuFreeSendFee.Text)      '特例置換項目-通運負担回送運賃
        Master.EraseCharToIgnore(TxtSprManageFee.Text)              '特例置換項目-運行管理料
        Master.EraseCharToIgnore(TxtSprShipBurdenFee.Text)          '特例置換項目-荷主負担運賃
        Master.EraseCharToIgnore(TxtSprShipFee.Text)                '特例置換項目-発送料
        Master.EraseCharToIgnore(TxtSprArriveFee.Text)              '特例置換項目-到着料
        Master.EraseCharToIgnore(TxtSprPickUpFee.Text)              '特例置換項目-集荷料
        Master.EraseCharToIgnore(TxtSprDeliveryFee.Text)            '特例置換項目-配達料
        Master.EraseCharToIgnore(TxtSprOther1.Text)                 '特例置換項目-その他１
        Master.EraseCharToIgnore(TxtSprOther2.Text)                 '特例置換項目-その他２
        Master.EraseCharToIgnore(TxtSprFitKbn.Text)                 '特例置換項目-適合区分

        '○ GridViewから未選択状態で表更新ボタンを押下時の例外を回避する
        If String.IsNullOrEmpty(LblSelLineCNT.Text) AndAlso
            String.IsNullOrEmpty(TxtDelFlg.Text) Then
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

        Master.CreateEmptyTable(LNM0016INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0016INProw As DataRow = LNM0016INPtbl.NewRow

        ' LINECNT
        If String.IsNullOrEmpty(LblSelLineCNT.Text) Then
            LNM0016INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(LblSelLineCNT.Text, LNM0016INProw("LINECNT"))
            Catch ex As Exception
                LNM0016INProw("LINECNT") = 0
            End Try
        End If

        LNM0016INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0016INProw("UPDTIMSTP") = 0
        LNM0016INProw("SELECT") = 1
        LNM0016INProw("HIDDEN") = 0

        LNM0016INProw("DELFLG") = TxtDelFlg.Text                                        '削除フラグ
        LNM0016INProw("ORGCODE") = TxtOrgCode.Text                                      '組織コード
        LNM0016INProw("BIGCTNCD") = TxtBigCTNCD.Text                                    '大分類コード
        LNM0016INProw("MIDDLECTNCD") = TxtMiddleCTNCD.Text                              '中分類コード
        LNM0016INProw("DEPSTATION") = TxtDepStation.Text                                '発駅コード
        LNM0016INProw("DEPTRUSTEECD") = TxtDepTrusteeCd.Text                            '発受託人コード
        LNM0016INProw("DEPTRUSTEESUBCD") = TxtDepTrusteeSubCd.Text                      '発受託人サブコード
        LNM0016INProw("PRIORITYNO") = TxtPriorityNo.Text                                '優先順位
        LNM0016INProw("PURPOSE") = TxtPurpose.Text                                      '使用目的
        LNM0016INProw("SLCCTNTYPE") = TxtCTNType.Text                                   '選択比較項目-コンテナ記号
        LNM0016INProw("SLCCTNSTNO") = TxtCTNStNo.Text                                   '選択比較項目-コンテナ番号（開始）
        LNM0016INProw("SLCCTNENDNO") = TxtCTNEndNo.Text                                 '選択比較項目-コンテナ番号（終了）
        LNM0016INProw("SLCJRDEPBRANCHCD") = TxtSlcJrDepBranchCd.Text                    '選択比較項目-ＪＲ発支社支店コード
        LNM0016INProw("SLCDEPSHIPPERCD1") = TxtSlcDepShipperCd1.Text                    '選択比較項目-発荷主コード１
        LNM0016INProw("SLCDEPSHIPPERCD2") = TxtSlcDepShipperCd2.Text                    '選択比較項目-発荷主コード２
        LNM0016INProw("SLCDEPSHIPPERCD3") = TxtSlcDepShipperCd3.Text                    '選択比較項目-発荷主コード３
        LNM0016INProw("SLCDEPSHIPPERCD4") = TxtSlcDepShipperCd4.Text                    '選択比較項目-発荷主コード４
        LNM0016INProw("SLCDEPSHIPPERCD5") = TxtSlcDepShipperCd5.Text                    '選択比較項目-発荷主コード５
        LNM0016INProw("SLCDEPSHIPPERCDCOND") = TxtSlcDepShipperCdCond.Text              '選択比較項目-発荷主ＣＤ比較条件
        LNM0016INProw("SLCJRARRBRANCHCD") = TxtSlcJrArrBranchCd.Text                    '選択比較項目-ＪＲ着支社支店コード
        LNM0016INProw("SLCJRARRBRANCHCDCOND") = TxtSlcJrArrBranchCdCond.Text            '選択比較項目-ＪＲ着支社支店ＣＤ比較
        LNM0016INProw("SLCJOTARRORGCODE") = TxtSlcJotArrOrgCode.Text                    '選択比較項目-ＪＯＴ着組織コード
        LNM0016INProw("SLCJOTARRORGCODECOND") = TxtSlcJotArrOrgCodeCond.Text            '選択比較項目-ＪＯＴ着組織ＣＤ比較
        LNM0016INProw("SLCARRSTATION1") = TxtSlcArrStation1.Text                        '選択比較項目-着駅コード１
        LNM0016INProw("SLCARRSTATION2") = TxtSlcArrStation2.Text                        '選択比較項目-着駅コード２
        LNM0016INProw("SLCARRSTATION3") = TxtSlcArrStation3.Text                        '選択比較項目-着駅コード３
        LNM0016INProw("SLCARRSTATION4") = TxtSlcArrStation4.Text                        '選択比較項目-着駅コード４
        LNM0016INProw("SLCARRSTATION5") = TxtSlcArrStation5.Text                        '選択比較項目-着駅コード５
        LNM0016INProw("SLCARRSTATION6") = TxtSlcArrStation6.Text                        '選択比較項目-着駅コード６
        LNM0016INProw("SLCARRSTATION7") = TxtSlcArrStation7.Text                        '選択比較項目-着駅コード７
        LNM0016INProw("SLCARRSTATION8") = TxtSlcArrStation8.Text                        '選択比較項目-着駅コード８
        LNM0016INProw("SLCARRSTATION9") = TxtSlcArrStation9.Text                        '選択比較項目-着駅コード９
        LNM0016INProw("SLCARRSTATION10") = TxtSlcArrStation10.Text                      '選択比較項目-着駅コード１０
        LNM0016INProw("SLCARRSTATIONCOND") = TxtSlcArrStationCond.Text                  '選択比較項目-着駅コード比較条件
        LNM0016INProw("SLCARRTRUSTEECD") = TxtSlcArrTrusteeCd.Text                      '選択比較項目-着受託人コード
        LNM0016INProw("SLCARRTRUSTEECDCOND") = TxtSlcArrTrusteeCdCond.Text              '選択比較項目-着受託人ＣＤ比較条件
        LNM0016INProw("SLCARRTRUSTEESUBCD") = TxtSlcArrTrusteeSubCd.Text                '選択比較項目-着受託人サブコード
        LNM0016INProw("SLCARRTRUSTEESUBCDCOND") = TxtSlcArrTrusteeSubCdCond.Text        '選択比較項目-着受託人サブＣＤ比較
        LNM0016INProw("SLCSTMD") = TxtSlcStMD.Text                                      '選択比較項目-開始月日
        LNM0016INProw("SLCENDMD") = TxtSlcEndMD.Text                                    '選択比較項目-終了月日
        LNM0016INProw("SLCSTSHIPYMD") = TxtSlcStShipMD.Text                             '選択比較項目-開始発送年月日
        LNM0016INProw("SLCENDSHIPYMD") = TxtSlcEndShipMD.Text                           '選択比較項目-終了発送年月日
        LNM0016INProw("SLCJRITEMCD1") = TxtSlcJrItemCd1.Text                            '選択比較項目-ＪＲ品目コード１
        LNM0016INProw("SLCJRITEMCD2") = TxtSlcJrItemCd2.Text                            '選択比較項目-ＪＲ品目コード２
        LNM0016INProw("SLCJRITEMCD3") = TxtSlcJrItemCd3.Text                            '選択比較項目-ＪＲ品目コード３
        LNM0016INProw("SLCJRITEMCD4") = TxtSlcJrItemCd4.Text                            '選択比較項目-ＪＲ品目コード４
        LNM0016INProw("SLCJRITEMCD5") = TxtSlcJrItemCd5.Text                            '選択比較項目-ＪＲ品目コード５
        LNM0016INProw("SLCJRITEMCD6") = TxtSlcJrItemCd6.Text                            '選択比較項目-ＪＲ品目コード６
        LNM0016INProw("SLCJRITEMCD7") = TxtSlcJrItemCd7.Text                            '選択比較項目-ＪＲ品目コード７
        LNM0016INProw("SLCJRITEMCD8") = TxtSlcJrItemCd8.Text                            '選択比較項目-ＪＲ品目コード８
        LNM0016INProw("SLCJRITEMCD9") = TxtSlcJrItemCd9.Text                            '選択比較項目-ＪＲ品目コード９
        LNM0016INProw("SLCJRITEMCD10") = TxtSlcJrItemCd10.Text                          '選択比較項目-ＪＲ品目コード１０
        LNM0016INProw("SLCJRITEMCDCOND") = TxtSlcJrItemCdCond.Text                      '選択比較項目-ＪＲ品目コード比較
        LNM0016INProw("SPRUSEFEE") = TxtSprUseFee.Text                                  '特例置換項目-使用料金額
        LNM0016INProw("SPRUSEFEERATE") = TxtSprUseFeeRate.Text                          '特例置換項目-使用料率
        LNM0016INProw("SPRUSEFEERATEROUND") = TxtSprUseFeeRateRound1.Text &             '特例置換項目-使用料率端数整理
                                              TxtSprUseFeeRateRound2.Text
        LNM0016INProw("SPRUSEFEERATEROUND1") = TxtSprUseFeeRateRound1.Text              '特例置換項目-使用料率端数整理1
        LNM0016INProw("SPRUSEFEERATEROUND2") = TxtSprUseFeeRateRound2.Text              '特例置換項目-使用料率端数整理2
        LNM0016INProw("SPRUSEFEERATEADDSUB") = TxtSprUseFeeRateAddSub.Text              '特例置換項目-使用料率加減額
        LNM0016INProw("SPRUSEFEERATEADDSUBCOND") = TxtSprUseFeeRateAddSubCond1.Text &   '特例置換項目-使用料率加減額端数整理
                                                   TxtSprUseFeeRateAddSubCond2.Text
        LNM0016INProw("SPRUSEFEERATEADDSUBCOND1") = TxtSprUseFeeRateAddSubCond1.Text    '特例置換項目-使用料率加減額端数整理1
        LNM0016INProw("SPRUSEFEERATEADDSUBCOND2") = TxtSprUseFeeRateAddSubCond2.Text    '特例置換項目-使用料率加減額端数整理2
        LNM0016INProw("SPRROUNDPOINTKBN") = TxtSprRoundPointKbn.Text                    '特例置換項目-端数処理時点区分
        LNM0016INProw("SPRUSEFREESPE") = TxtSprUseFreeSpe.Text                          '特例置換項目-使用料無料特認
        LNM0016INProw("SPRNITTSUFREESENDFEE") = TxtSprNittsuFreeSendFee.Text            '特例置換項目-通運負担回送運賃
        LNM0016INProw("SPRMANAGEFEE") = TxtSprManageFee.Text                            '特例置換項目-運行管理料
        LNM0016INProw("SPRSHIPBURDENFEE") = TxtSprShipBurdenFee.Text                    '特例置換項目-荷主負担運賃
        LNM0016INProw("SPRSHIPFEE") = TxtSprShipFee.Text                                '特例置換項目-発送料
        LNM0016INProw("SPRARRIVEFEE") = TxtSprArriveFee.Text                            '特例置換項目-到着料
        LNM0016INProw("SPRPICKUPFEE") = TxtSprPickUpFee.Text                            '特例置換項目-集荷料
        LNM0016INProw("SPRDELIVERYFEE") = TxtSprDeliveryFee.Text                        '特例置換項目-配達料
        LNM0016INProw("SPROTHER1") = TxtSprOther1.Text                                  '特例置換項目-その他１
        LNM0016INProw("SPROTHER2") = TxtSprOther2.Text                                  '特例置換項目-その他２
        LNM0016INProw("SPRFITKBN") = TxtSprFitKbn.Text                                  '特例置換項目-適合区分
        LNM0016INProw("SPRCONTRACTCD") = TxtSprContractCd.Text                          '特例置換項目-契約コード

        '○ チェック用テーブルに登録する
        LNM0016INPtbl.Rows.Add(LNM0016INProw)

    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0016INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0016INProw As DataRow = LNM0016INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0016row As DataRow In LNM0016tbl.Rows
            ' KEY項目が等しい時
            If LNM0016row("ORGCODE") = LNM0016INProw("ORGCODE") AndAlso                                      '組織コード
               LNM0016row("BIGCTNCD") = LNM0016INProw("BIGCTNCD") AndAlso                                    '大分類コード
               LNM0016row("MIDDLECTNCD") = LNM0016INProw("MIDDLECTNCD") AndAlso                              '中分類コード
               LNM0016row("DEPSTATION") = LNM0016INProw("DEPSTATION") AndAlso                                '発駅コード
               LNM0016row("DEPTRUSTEECD") = LNM0016INProw("DEPTRUSTEECD") AndAlso                            '発受託人コード
               LNM0016row("DEPTRUSTEESUBCD") = LNM0016INProw("DEPTRUSTEESUBCD") AndAlso                      '発受託人サブコード
               LNM0016row("PRIORITYNO") = LNM0016INProw("PRIORITYNO") Then                                   '優先順位
                ' KEY項目以外の項目の差異をチェック
                If LNM0016row("DELFLG") = LNM0016INProw("DELFLG") AndAlso                                    '削除フラグ
                   LNM0016row("PURPOSE") = LNM0016INProw("PURPOSE") AndAlso                                  '使用目的
                   LNM0016row("SLCCTNTYPE") = LNM0016INProw("SLCCTNTYPE") AndAlso                            '選択比較項目-コンテナ記号
                   LNM0016row("SLCCTNSTNO") = LNM0016INProw("SLCCTNSTNO") AndAlso                            '選択比較項目-コンテナ番号（開始）
                   LNM0016row("SLCCTNENDNO") = LNM0016INProw("SLCCTNENDNO") AndAlso                          '選択比較項目-コンテナ番号（終了）
                   LNM0016row("SLCJRDEPBRANCHCD") = LNM0016INProw("SLCJRDEPBRANCHCD") AndAlso                '選択比較項目-ＪＲ発支社支店コード
                   LNM0016row("SLCDEPSHIPPERCD1") = LNM0016INProw("SLCDEPSHIPPERCD1") AndAlso                '選択比較項目-発荷主コード１
                   LNM0016row("SLCDEPSHIPPERCD2") = LNM0016INProw("SLCDEPSHIPPERCD2") AndAlso                '選択比較項目-発荷主コード２
                   LNM0016row("SLCDEPSHIPPERCD3") = LNM0016INProw("SLCDEPSHIPPERCD3") AndAlso                '選択比較項目-発荷主コード３
                   LNM0016row("SLCDEPSHIPPERCD4") = LNM0016INProw("SLCDEPSHIPPERCD4") AndAlso                '選択比較項目-発荷主コード４
                   LNM0016row("SLCDEPSHIPPERCD5") = LNM0016INProw("SLCDEPSHIPPERCD5") AndAlso                '選択比較項目-発荷主コード５
                   LNM0016row("SLCDEPSHIPPERCDCOND") = LNM0016INProw("SLCDEPSHIPPERCDCOND") AndAlso          '選択比較項目-発荷主ＣＤ比較条件
                   LNM0016row("SLCJRARRBRANCHCD") = LNM0016INProw("SLCJRARRBRANCHCD") AndAlso                '選択比較項目-ＪＲ着支社支店コード
                   LNM0016row("SLCJRARRBRANCHCDCOND") = LNM0016INProw("SLCJRARRBRANCHCDCOND") AndAlso        '選択比較項目-ＪＲ着支社支店ＣＤ比較
                   LNM0016row("SLCJOTARRORGCODE") = LNM0016INProw("SLCJOTARRORGCODE") AndAlso                '選択比較項目-ＪＯＴ着組織コード
                   LNM0016row("SLCJOTARRORGCODECOND") = LNM0016INProw("SLCJOTARRORGCODECOND") AndAlso        '選択比較項目-ＪＯＴ着組織ＣＤ比較
                   LNM0016row("SLCARRSTATION1") = LNM0016INProw("SLCARRSTATION1") AndAlso                    '選択比較項目-着駅コード１
                   LNM0016row("SLCARRSTATION2") = LNM0016INProw("SLCARRSTATION2") AndAlso                    '選択比較項目-着駅コード２
                   LNM0016row("SLCARRSTATION3") = LNM0016INProw("SLCARRSTATION3") AndAlso                    '選択比較項目-着駅コード３
                   LNM0016row("SLCARRSTATION4") = LNM0016INProw("SLCARRSTATION4") AndAlso                    '選択比較項目-着駅コード４
                   LNM0016row("SLCARRSTATION5") = LNM0016INProw("SLCARRSTATION5") AndAlso                    '選択比較項目-着駅コード５
                   LNM0016row("SLCARRSTATION6") = LNM0016INProw("SLCARRSTATION6") AndAlso                    '選択比較項目-着駅コード６
                   LNM0016row("SLCARRSTATION7") = LNM0016INProw("SLCARRSTATION7") AndAlso                    '選択比較項目-着駅コード７
                   LNM0016row("SLCARRSTATION8") = LNM0016INProw("SLCARRSTATION8") AndAlso                    '選択比較項目-着駅コード８
                   LNM0016row("SLCARRSTATION9") = LNM0016INProw("SLCARRSTATION9") AndAlso                    '選択比較項目-着駅コード９
                   LNM0016row("SLCARRSTATION10") = LNM0016INProw("SLCARRSTATION10") AndAlso                  '選択比較項目-着駅コード１０
                   LNM0016row("SLCARRSTATIONCOND") = LNM0016INProw("SLCARRSTATIONCOND") AndAlso              '選択比較項目-着駅コード比較条件
                   LNM0016row("SLCARRTRUSTEECD") = LNM0016INProw("SLCARRTRUSTEECD") AndAlso                  '選択比較項目-着受託人コード
                   LNM0016row("SLCARRTRUSTEECDCOND") = LNM0016INProw("SLCARRTRUSTEECDCOND") AndAlso          '選択比較項目-着受託人ＣＤ比較条件
                   LNM0016row("SLCARRTRUSTEESUBCD") = LNM0016INProw("SLCARRTRUSTEESUBCD") AndAlso            '選択比較項目-着受託人サブコード
                   LNM0016row("SLCARRTRUSTEESUBCDCOND") = LNM0016INProw("SLCARRTRUSTEESUBCDCOND") AndAlso    '選択比較項目-着受託人サブＣＤ比較
                   LNM0016row("SLCSTMD") = LNM0016INProw("SLCSTMD") AndAlso                                  '選択比較項目-開始月日
                   LNM0016row("SLCENDMD") = LNM0016INProw("SLCENDMD") AndAlso                                '選択比較項目-終了月日
                   LNM0016row("SLCSTSHIPYMD") = LNM0016INProw("SLCSTSHIPYMD") AndAlso                        '選択比較項目-開始発送年月日
                   LNM0016row("SLCENDSHIPYMD") = LNM0016INProw("SLCENDSHIPYMD") AndAlso                      '選択比較項目-終了発送年月日
                   LNM0016row("SLCJRITEMCD1") = LNM0016INProw("SLCJRITEMCD1") AndAlso                        '選択比較項目-ＪＲ品目コード１
                   LNM0016row("SLCJRITEMCD2") = LNM0016INProw("SLCJRITEMCD2") AndAlso                        '選択比較項目-ＪＲ品目コード２
                   LNM0016row("SLCJRITEMCD3") = LNM0016INProw("SLCJRITEMCD3") AndAlso                        '選択比較項目-ＪＲ品目コード３
                   LNM0016row("SLCJRITEMCD4") = LNM0016INProw("SLCJRITEMCD4") AndAlso                        '選択比較項目-ＪＲ品目コード４
                   LNM0016row("SLCJRITEMCD5") = LNM0016INProw("SLCJRITEMCD5") AndAlso                        '選択比較項目-ＪＲ品目コード５
                   LNM0016row("SLCJRITEMCD6") = LNM0016INProw("SLCJRITEMCD6") AndAlso                        '選択比較項目-ＪＲ品目コード６
                   LNM0016row("SLCJRITEMCD7") = LNM0016INProw("SLCJRITEMCD7") AndAlso                        '選択比較項目-ＪＲ品目コード７
                   LNM0016row("SLCJRITEMCD8") = LNM0016INProw("SLCJRITEMCD8") AndAlso                        '選択比較項目-ＪＲ品目コード８
                   LNM0016row("SLCJRITEMCD9") = LNM0016INProw("SLCJRITEMCD9") AndAlso                        '選択比較項目-ＪＲ品目コード９
                   LNM0016row("SLCJRITEMCD10") = LNM0016INProw("SLCJRITEMCD10") AndAlso                      '選択比較項目-ＪＲ品目コード１０
                   LNM0016row("SLCJRITEMCDCOND") = LNM0016INProw("SLCJRITEMCDCOND") AndAlso                  '選択比較項目-ＪＲ品目コード比較
                   LNM0016row("SPRUSEFEE") = LNM0016INProw("SPRUSEFEE") AndAlso                              '特例置換項目-使用料金額
                   LNM0016row("SPRUSEFEERATE") = LNM0016INProw("SPRUSEFEERATE") AndAlso                      '特例置換項目-使用料率
                   LNM0016row("SPRUSEFEERATEROUND") = LNM0016INProw("SPRUSEFEERATEROUND") AndAlso            '特例置換項目-使用料率端数整理
                   LNM0016row("SPRUSEFEERATEADDSUB") = LNM0016INProw("SPRUSEFEERATEADDSUB") AndAlso          '特例置換項目-使用料率加減額
                   LNM0016row("SPRUSEFEERATEADDSUBCOND") = LNM0016INProw("SPRUSEFEERATEADDSUBCOND") AndAlso  '特例置換項目-使用料率加減額端数整理
                   LNM0016row("SPRROUNDPOINTKBN") = LNM0016INProw("SPRROUNDPOINTKBN") AndAlso                '特例置換項目-端数処理時点区分
                   LNM0016row("SPRUSEFREESPE") = LNM0016INProw("SPRUSEFREESPE") AndAlso                      '特例置換項目-使用料無料特認
                   LNM0016row("SPRNITTSUFREESENDFEE") = LNM0016INProw("SPRNITTSUFREESENDFEE") AndAlso        '特例置換項目-通運負担回送運賃
                   LNM0016row("SPRMANAGEFEE") = LNM0016INProw("SPRMANAGEFEE") AndAlso                        '特例置換項目-運行管理料
                   LNM0016row("SPRSHIPBURDENFEE") = LNM0016INProw("SPRSHIPBURDENFEE") AndAlso                '特例置換項目-荷主負担運賃
                   LNM0016row("SPRSHIPFEE") = LNM0016INProw("SPRSHIPFEE") AndAlso                            '特例置換項目-発送料
                   LNM0016row("SPRARRIVEFEE") = LNM0016INProw("SPRARRIVEFEE") AndAlso                        '特例置換項目-到着料
                   LNM0016row("SPRPICKUPFEE") = LNM0016INProw("SPRPICKUPFEE") AndAlso                        '特例置換項目-集荷料
                   LNM0016row("SPRDELIVERYFEE") = LNM0016INProw("SPRDELIVERYFEE") AndAlso                    '特例置換項目-配達料
                   LNM0016row("SPROTHER1") = LNM0016INProw("SPROTHER1") AndAlso                              '特例置換項目-その他１
                   LNM0016row("SPROTHER2") = LNM0016INProw("SPROTHER2") AndAlso                              '特例置換項目-その他２
                   LNM0016row("SPRFITKBN") = LNM0016INProw("SPRFITKBN") AndAlso                              '特例置換項目-適合区分
                   LNM0016row("SPRCONTRACTCD") = LNM0016INProw("SPRCONTRACTCD") Then                         '特例置換項目-契約コード
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
        For Each LNM0016row As DataRow In LNM0016tbl.Rows
            Select Case LNM0016row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0016tbl, work.WF_SEL_INPTBL.Text)

        LblSelLineCNT.Text = ""                'LINECNT
        TxtMapId.Text = "M00001"               '画面ＩＤ
        TxtDelFlg.Text = ""                    '削除フラグ
        TxtOrgCode.Text = ""                   '組織コード
        TxtBigCTNCD.Text = ""                  '大分類コード
        TxtMiddleCTNCD.Text = ""               '中分類コード
        TxtDepStation.Text = ""                '発駅コード
        TxtDepTrusteeCd.Text = ""              '発受託人コード
        TxtDepTrusteeSubCd.Text = ""           '発受託人サブコード
        TxtPriorityNo.Text = ""                '優先順位
        TxtPurpose.Text = ""                   '使用目的
        TxtCTNType.Text = ""                   '選択比較項目-コンテナ記号
        TxtCTNStNo.Text = ""                   '選択比較項目-コンテナ番号（開始）
        TxtCTNEndNo.Text = ""                  '選択比較項目-コンテナ番号（終了）
        TxtSlcJrDepBranchCd.Text = ""          '選択比較項目-ＪＲ発支社支店コード
        TxtSlcDepShipperCd1.Text = ""          '選択比較項目-発荷主コード１
        TxtSlcDepShipperCd2.Text = ""          '選択比較項目-発荷主コード２
        TxtSlcDepShipperCd3.Text = ""          '選択比較項目-発荷主コード３
        TxtSlcDepShipperCd4.Text = ""          '選択比較項目-発荷主コード４
        TxtSlcDepShipperCd5.Text = ""          '選択比較項目-発荷主コード５
        TxtSlcDepShipperCdCond.Text = ""       '選択比較項目-発荷主ＣＤ比較条件
        TxtSlcJrArrBranchCd.Text = ""          '選択比較項目-ＪＲ着支社支店コード
        TxtSlcJrArrBranchCdCond.Text = ""      '選択比較項目-ＪＲ着支社支店ＣＤ比較
        TxtSlcJotArrOrgCode.Text = ""          '選択比較項目-ＪＯＴ着組織コード
        TxtSlcJotArrOrgCodeCond.Text = ""      '選択比較項目-ＪＯＴ着組織ＣＤ比較
        TxtSlcArrStation1.Text = ""            '選択比較項目-着駅コード１
        TxtSlcArrStation2.Text = ""            '選択比較項目-着駅コード２
        TxtSlcArrStation3.Text = ""            '選択比較項目-着駅コード３
        TxtSlcArrStation4.Text = ""            '選択比較項目-着駅コード４
        TxtSlcArrStation5.Text = ""            '選択比較項目-着駅コード５
        TxtSlcArrStation6.Text = ""            '選択比較項目-着駅コード６
        TxtSlcArrStation7.Text = ""            '選択比較項目-着駅コード７
        TxtSlcArrStation8.Text = ""            '選択比較項目-着駅コード８
        TxtSlcArrStation9.Text = ""            '選択比較項目-着駅コード９
        TxtSlcArrStation10.Text = ""           '選択比較項目-着駅コード１０
        TxtSlcArrStationCond.Text = ""         '選択比較項目-着駅コード比較条件
        TxtSlcArrTrusteeCd.Text = ""           '選択比較項目-着受託人コード
        TxtSlcArrTrusteeCdCond.Text = ""       '選択比較項目-着受託人ＣＤ比較条件
        TxtSlcArrTrusteeSubCd.Text = ""        '選択比較項目-着受託人サブコード
        TxtSlcArrTrusteeSubCdCond.Text = ""    '選択比較項目-着受託人サブＣＤ比較
        TxtSlcStMD.Text = ""                   '選択比較項目-開始月日
        TxtSlcEndMD.Text = ""                  '選択比較項目-終了月日
        TxtSlcStShipMD.Text = ""               '選択比較項目-開始発送年月日
        TxtSlcEndShipMD.Text = ""              '選択比較項目-終了発送年月日
        TxtSlcJrItemCd1.Text = ""              '選択比較項目-ＪＲ品目コード１
        TxtSlcJrItemCd2.Text = ""              '選択比較項目-ＪＲ品目コード２
        TxtSlcJrItemCd3.Text = ""              '選択比較項目-ＪＲ品目コード３
        TxtSlcJrItemCd4.Text = ""              '選択比較項目-ＪＲ品目コード４
        TxtSlcJrItemCd5.Text = ""              '選択比較項目-ＪＲ品目コード５
        TxtSlcJrItemCd6.Text = ""              '選択比較項目-ＪＲ品目コード６
        TxtSlcJrItemCd7.Text = ""              '選択比較項目-ＪＲ品目コード７
        TxtSlcJrItemCd8.Text = ""              '選択比較項目-ＪＲ品目コード８
        TxtSlcJrItemCd9.Text = ""              '選択比較項目-ＪＲ品目コード９
        TxtSlcJrItemCd10.Text = ""             '選択比較項目-ＪＲ品目コード１０
        TxtSlcJrItemCdCond.Text = ""           '選択比較項目-ＪＲ品目コード比較
        TxtSprUseFee.Text = ""                 '特例置換項目-使用料金額
        TxtSprUseFeeRate.Text = ""             '特例置換項目-使用料率
        TxtSprUseFeeRateRound1.Text = ""       '特例置換項目-使用料率端数整理
        TxtSprUseFeeRateRound2.Text = ""
        TxtSprUseFeeRateAddSub.Text = ""       '特例置換項目-使用料率加減額
        TxtSprUseFeeRateAddSubCond1.Text = ""  '特例置換項目-使用料率加減額端数整理
        TxtSprUseFeeRateAddSubCond2.Text = ""
        TxtSprRoundPointKbn.Text = ""         '特例置換項目-端数処理時点区分
        TxtSprUseFreeSpe.Text = ""             '特例置換項目-使用料無料特認
        TxtSprNittsuFreeSendFee.Text = ""      '特例置換項目-通運負担回送運賃
        TxtSprManageFee.Text = ""              '特例置換項目-運行管理料
        TxtSprShipBurdenFee.Text = ""          '特例置換項目-荷主負担運賃
        TxtSprShipFee.Text = ""                '特例置換項目-発送料
        TxtSprArriveFee.Text = ""              '特例置換項目-到着料
        TxtSprPickUpFee.Text = ""              '特例置換項目-集荷料
        TxtSprDeliveryFee.Text = ""            '特例置換項目-配達料
        TxtSprOther1.Text = ""                 '特例置換項目-その他１
        TxtSprOther2.Text = ""                 '特例置換項目-その他２
        TxtSprFitKbn.Text = ""                 '特例置換項目-適合区分
        TxtSprContractCd.Text = ""             '特例置換項目-契約コード

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
                .Visible = true
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        ' 日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "TxtSlcStShipMD"                    '選択比較項目-開始発送年月日
                                .WF_Calendar.Text = TxtSlcStShipMD.Text
                            Case "TxtSlcEndShipMD"                   '選択比較項目-終了発送年月日
                                .WF_Calendar.Text = TxtSlcEndShipMD.Text
                        End Select
                        .ActiveCalendar()

                Case Else
                        ' フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "TxtOrgCode",              '組織コード
                                 "TxtSlcJotArrOrgCode"               '選択比較項目-ＪＯＴ着組織コード
                                WW_PrmData = work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP)
                            Case "TxtBigCTNCD"                       '大分類コード
                                WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                            Case "TxtMiddleCTNCD"                    '中分類コード
                                WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, TxtBigCTNCD.Text)
                            Case "TxtDepStation",                    '発駅コード
                                 "TxtSlcArrStation1",                '選択比較項目-着駅コード１
                                 "TxtSlcArrStation2",                '選択比較項目-着駅コード２
                                 "TxtSlcArrStation3",                '選択比較項目-着駅コード３
                                 "TxtSlcArrStation4",                '選択比較項目-着駅コード４
                                 "TxtSlcArrStation5",                '選択比較項目-着駅コード５
                                 "TxtSlcArrStation6",                '選択比較項目-着駅コード６
                                 "TxtSlcArrStation7",                '選択比較項目-着駅コード７
                                 "TxtSlcArrStation8",                '選択比較項目-着駅コード８
                                 "TxtSlcArrStation9",                '選択比較項目-着駅コード９
                                 "TxtSlcArrStation10"                '選択比較項目-着駅コード１０
                                leftview.Visible = False
                                '検索画面
                                DisplayView_mspStationSingle()
                                '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                                WF_LeftboxOpen.Value = ""
                                Exit Sub
                            Case "TxtDepTrusteeCd"                   '発受託人コード
                                WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtDepStation.Text)
                            Case "TxtDepTrusteeSubCd"                '発受託人コード
                                WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtDepStation.Text, TxtDepTrusteeCd.Text)
                            Case "TxtCTNType"                        'コンテナ記号
                                WW_PrmData = work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_TYPE)
                            Case "TxtCTNStNo",                       'コンテナ番号
                                 "TxtCTNEndNo"                       'コンテナ番号
                                WW_PrmData = work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, TxtCTNType.Text)
                            Case "TxtSlcDepShipperCdCond"            '選択比較項目-発荷主ＣＤ比較条件
                            Case "TxtSlcJrArrBranchCdCond"           '選択比較項目-ＪＲ着支社支店ＣＤ比較
                            Case "TxtSlcJotArrOrgCodeCond"           '選択比較項目-ＪＯＴ着組織ＣＤ比較
                            Case "TxtSlcArrStationCond"              '選択比較項目-着駅コード比較条件
                            Case "TxtSlcArrTrusteeCdCond"            '選択比較項目-着受託人ＣＤ比較条件
                            Case "TxtSlcArrTrusteeSubCdCond"         '選択比較項目-着受託人サブＣＤ比較
                            Case "TxtSlcJrItemCdCond"                '選択比較項目-ＪＲ品目コード比較
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "COMPARECONDKBN")
                            Case "TxtSlcArrTrusteeCd"                '選択比較項目-着受託人コード
                                WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtSlcArrStation1.Text)
                            Case "TxtSlcArrTrusteeSubCd"             '選択比較項目-着受託人サブコード
                                WW_PrmData = work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtSlcArrStation1.Text, TxtSlcArrTrusteeCd.Text)
                            Case "TxtSlcJrDepBranchCd",              '選択比較項目-ＪＲ発支社支店コード
                                 "TxtSlcJrArrBranchCd"               '選択比較項目-ＪＲ着支社支店コード
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "JRBRANCHCD")
                            Case "TxtSprUseFeeRateRound1",           '特例置換項目-使用料率端数整理1
                                 "TxtSprUseFeeRateAddSubCond1"       '特例置換項目-使用料率加減額端数整理1
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUU1")
                            Case "TxtSprUseFeeRateRound2",           '特例置換項目-使用料率端数整理2
                                 "TxtSprUseFeeRateAddSubCond2"       '特例置換項目-使用料率加減額端数整理2
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUU2")
                            Case "TxtSprRoundPointKbn"               '特例置換項目-端数処理時点区分
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "HASUUPOINTKBN")
                            Case "TxtSprUseFreeSpe"                  '特例置換項目-使用料無料特認
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "USEFREEKBN")
                            Case "TxtSprFitKbn"                      '特例置換項目-適合区分
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "FITKBN")
                            Case "TxtDelFlg"                         '削除フラグ
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "DELFLG")
                        End Select
                        .SetListBox(WF_LeftMViewChange.Value, WW_Dummy, WW_PrmData)
                        .ActiveListBox()
                End Select
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
            Case "TxtDelFlg"                         '削除フラグ
                CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
                TxtDelFlg.Focus()
            Case "TxtOrgCode"                        '組織コード
                CODENAME_get("ORG", TxtOrgCode.Text, LblOrgName.Text, WW_Dummy)
                TxtOrgCode.Focus()
            Case "TxtBigCTNCD"                       '大分類コード
                CODENAME_get("BIGCTNCD", TxtBigCTNCD.Text, LblBigCTNCDName.Text, WW_Dummy)
                TxtBigCTNCD.Focus()
            Case "TxtMiddleCTNCD"                    '中分類コード
                CODENAME_get("MIDDLECTNCD", TxtMiddleCTNCD.Text, LblMiddleCTNCDName.Text, WW_Dummy)
                TxtMiddleCTNCD.Focus()
            Case "TxtDepStation"                     '発駅コード
                CODENAME_get("STATION", TxtDepStation.Text, LblDepStationName.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblDepStationName.Text) And TxtDepStation.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtDepStation.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtDepStation.Focus()
                End If
            Case "TxtDepTrusteeCd"                   '発受託人コード
                CODENAME_get("DEPTRUSTEECD", TxtDepTrusteeCd.Text, LblDepTrusteeCdName.Text, WW_Dummy)
                TxtDepTrusteeCd.Focus()
            Case "TxtDepTrusteeSubCd"                '発受託人サブコード
                CODENAME_get("DEPTRUSTEESUBCD", TxtDepTrusteeSubCd.Text, LblDepTrusteeSubCdName.Text, WW_Dummy)
                TxtDepTrusteeSubCd.Focus()
            Case "TxtSlcJrDepBranchCd"               '選択比較項目-ＪＲ発支社支店コード
                CODENAME_get("JRBRANCHCD", TxtSlcJrDepBranchCd.Text, LblSlcJrDepBranchCdName.Text, WW_Dummy)
                TxtSlcJrDepBranchCd.Focus()
            Case "TxtSlcDepShipperCd1"               '選択比較項目-発荷主コード１
                CODENAME_get("SHIPPER", TxtSlcDepShipperCd1.Text, LblSlcDepShipperCd1Name.Text, WW_Dummy)
                TxtSlcDepShipperCd1.Focus()
            Case "TxtSlcDepShipperCd2"               '選択比較項目-発荷主コード２
                CODENAME_get("SHIPPER", TxtSlcDepShipperCd2.Text, LblSlcDepShipperCd2Name.Text, WW_Dummy)
                TxtSlcDepShipperCd2.Focus()
            Case "TxtSlcDepShipperCd3"               '選択比較項目-発荷主コード３
                CODENAME_get("SHIPPER", TxtSlcDepShipperCd3.Text, LblSlcDepShipperCd3Name.Text, WW_Dummy)
                TxtSlcDepShipperCd3.Focus()
            Case "TxtSlcDepShipperCd4"               '選択比較項目-発荷主コード４
                CODENAME_get("SHIPPER", TxtSlcDepShipperCd4.Text, LblSlcDepShipperCd4Name.Text, WW_Dummy)
                TxtSlcDepShipperCd4.Focus()
            Case "TxtSlcDepShipperCd5"               '選択比較項目-発荷主コード５
                CODENAME_get("SHIPPER", TxtSlcDepShipperCd5.Text, LblSlcDepShipperCd5Name.Text, WW_Dummy)
                TxtSlcDepShipperCd5.Focus()
            Case "TxtSlcDepShipperCdCond"            '選択比較項目-発荷主ＣＤ比較条件
                CODENAME_get("COMPARECONDKBN", TxtSlcDepShipperCdCond.Text, LblSlcDepShipperCdCondName.Text, WW_Dummy)
                TxtSlcDepShipperCdCond.Focus()
            Case "TxtSlcJrArrBranchCd"               '選択比較項目-ＪＲ着支社支店コード
                CODENAME_get("JRBRANCHCD", TxtSlcJrArrBranchCd.Text, LblSlcJrArrBranchCdName.Text, WW_Dummy)
                TxtSlcJrArrBranchCd.Focus()
            Case "TxtSlcJrArrBranchCdCond"           '選択比較項目-ＪＲ着支社支店ＣＤ比較
                CODENAME_get("COMPARECONDKBN", TxtSlcJrArrBranchCdCond.Text, LblSlcJrArrBranchCdCondName.Text, WW_Dummy)
                TxtSlcJrArrBranchCdCond.Focus()
            Case "TxtSlcJotArrOrgCode"               '選択比較項目-ＪＯＴ着組織コード
                CODENAME_get("ORG", TxtSlcJotArrOrgCode.Text, LblSlcJotArrOrgCodeName.Text, WW_Dummy)
                TxtSlcJotArrOrgCode.Focus()
            Case "TxtSlcJotArrOrgCodeCond"           '選択比較項目-ＪＯＴ着組織ＣＤ比較
                CODENAME_get("COMPARECONDKBN", TxtSlcJotArrOrgCodeCond.Text, LblSlcJotArrOrgCodeCondName.Text, WW_Dummy)
                TxtSlcJotArrOrgCodeCond.Focus()
            Case "TxtSlcArrStation1"                 '選択比較項目-着駅コード１
                CODENAME_get("STATION", TxtSlcArrStation1.Text, LblSlcArrStation1Name.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblSlcArrStation1Name.Text) And TxtSlcArrStation1.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtSlcArrStation1.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtSlcArrStation1.Focus()
                End If
            Case "TxtSlcArrStation2"                 '選択比較項目-着駅コード２
                CODENAME_get("STATION", TxtSlcArrStation2.Text, LblSlcArrStation2Name.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblSlcArrStation2Name.Text) And TxtSlcArrStation2.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtSlcArrStation2.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtSlcArrStation2.Focus()
                End If
            Case "TxtSlcArrStation3"                 '選択比較項目-着駅コード３
                CODENAME_get("STATION", TxtSlcArrStation3.Text, LblSlcArrStation3Name.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblSlcArrStation3Name.Text) And TxtSlcArrStation3.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtSlcArrStation3.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtSlcArrStation3.Focus()
                End If
            Case "TxtSlcArrStation4"                 '選択比較項目-着駅コード４
                CODENAME_get("STATION", TxtSlcArrStation4.Text, LblSlcArrStation4Name.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblSlcArrStation4Name.Text) And TxtSlcArrStation4.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtSlcArrStation4.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtSlcArrStation4.Focus()
                End If
            Case "TxtSlcArrStation5"                 '選択比較項目-着駅コード５
                CODENAME_get("STATION", TxtSlcArrStation5.Text, LblSlcArrStation5Name.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblSlcArrStation5Name.Text) And TxtSlcArrStation5.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtSlcArrStation5.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtSlcArrStation5.Focus()
                End If
            Case "TxtSlcArrStation6"                 '選択比較項目-着駅コード６
                CODENAME_get("STATION", TxtSlcArrStation6.Text, LblSlcArrStation6Name.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblSlcArrStation6Name.Text) And TxtSlcArrStation6.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtSlcArrStation6.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtSlcArrStation6.Focus()
                End If
            Case "TxtSlcArrStation7"                 '選択比較項目-着駅コード７
                CODENAME_get("STATION", TxtSlcArrStation7.Text, LblSlcArrStation7Name.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblSlcArrStation7Name.Text) And TxtSlcArrStation7.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtSlcArrStation7.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtSlcArrStation7.Focus()
                End If
            Case "TxtSlcArrStation8"                 '選択比較項目-着駅コード８
                CODENAME_get("STATION", TxtSlcArrStation8.Text, LblSlcArrStation8Name.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblSlcArrStation8Name.Text) And TxtSlcArrStation8.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtSlcArrStation8.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtSlcArrStation8.Focus()
                End If
            Case "TxtSlcArrStation9"                 '選択比較項目-着駅コード９
                CODENAME_get("STATION", TxtSlcArrStation9.Text, LblSlcArrStation9Name.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblSlcArrStation9Name.Text) And TxtSlcArrStation9.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtSlcArrStation9.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtSlcArrStation9.Focus()
                End If
            Case "TxtSlcArrStation10"                '選択比較項目-着駅コード１０
                CODENAME_get("STATION", TxtSlcArrStation10.Text, LblSlcArrStation10Name.Text, WW_Dummy)
                'データ無しでも、駅コードが入力されている場合、検索画面表示
                If String.IsNullOrEmpty(LblSlcArrStation10Name.Text) And TxtSlcArrStation10.Text <> "" Then
                    '検索画面を表示する
                    leftview.Visible = False
                    '検索画面
                    DisplayView_mspStationSingle(TxtSlcArrStation10.Text)
                    '○ 画面左右ボックス非表示は、画面JavaScript(InitLoad)で実行
                    WF_LeftboxOpen.Value = ""
                    Exit Sub
                Else
                    TxtSlcArrStation10.Focus()
                End If
            Case "TxtSlcArrStationCond"              '選択比較項目-着駅コード比較条件
                CODENAME_get("COMPARECONDKBN", TxtSlcArrStationCond.Text, LblSlcArrStationCondName.Text, WW_Dummy)
                TxtSlcArrStationCond.Focus()
            Case "TxtSlcArrTrusteeCd"                '選択比較項目-着受託人コード
                CODENAME_get("ARRTRUSTEECD", TxtSlcArrTrusteeCd.Text, LblSlcArrTrusteeCdName.Text, WW_Dummy)
                TxtSlcArrTrusteeCd.Focus()
            Case "TxtSlcArrTrusteeCdCond"            '選択比較項目-着受託人ＣＤ比較条件
                CODENAME_get("COMPARECONDKBN", TxtSlcArrTrusteeCdCond.Text, LblSlcArrTrusteeCdCondName.Text, WW_Dummy)
                TxtSlcArrTrusteeCdCond.Focus()
            Case "TxtSlcArrTrusteeSubCd"             '選択比較項目-着受託人サブコード
                CODENAME_get("ARRTRUSTEESUBCD", TxtSlcArrTrusteeSubCd.Text, LblSlcArrTrusteeSubCdName.Text, WW_Dummy)
                TxtSlcArrTrusteeSubCd.Focus()
            Case "TxtSlcArrTrusteeSubCdCond"         '選択比較項目-着受託人サブＣＤ比較
                CODENAME_get("COMPARECONDKBN", TxtSlcArrTrusteeSubCdCond.Text, LblSlcArrTrusteeSubCdCondName.Text, WW_Dummy)
                TxtSlcArrTrusteeSubCdCond.Focus()
            Case "TxtSlcJrItemCd1"                   '選択比較項目-ＪＲ品目コード１
                CODENAME_get("ITEM", TxtSlcJrItemCd1.Text, LblSlcJrItemCd1Name.Text, WW_Dummy)
                TxtSlcJrItemCd1.Focus()
            Case "TxtSlcJrItemCd2"                   '選択比較項目-ＪＲ品目コード２
                CODENAME_get("ITEM", TxtSlcJrItemCd2.Text, LblSlcJrItemCd2Name.Text, WW_Dummy)
                TxtSlcJrItemCd2.Focus()
            Case "TxtSlcJrItemCd3"                   '選択比較項目-ＪＲ品目コード３
                CODENAME_get("ITEM", TxtSlcJrItemCd3.Text, LblSlcJrItemCd3Name.Text, WW_Dummy)
                TxtSlcJrItemCd3.Focus()
            Case "TxtSlcJrItemCd4"                   '選択比較項目-ＪＲ品目コード４
                CODENAME_get("ITEM", TxtSlcJrItemCd4.Text, LblSlcJrItemCd4Name.Text, WW_Dummy)
                TxtSlcJrItemCd4.Focus()
            Case "TxtSlcJrItemCd5"                   '選択比較項目-ＪＲ品目コード５
                CODENAME_get("ITEM", TxtSlcJrItemCd5.Text, LblSlcJrItemCd5Name.Text, WW_Dummy)
                TxtSlcJrItemCd5.Focus()
            Case "TxtSlcJrItemCd6"                   '選択比較項目-ＪＲ品目コード６
                CODENAME_get("ITEM", TxtSlcJrItemCd6.Text, LblSlcJrItemCd6Name.Text, WW_Dummy)
                TxtSlcJrItemCd6.Focus()
            Case "TxtSlcJrItemCd7"                   '選択比較項目-ＪＲ品目コード７
                CODENAME_get("ITEM", TxtSlcJrItemCd7.Text, LblSlcJrItemCd7Name.Text, WW_Dummy)
                TxtSlcJrItemCd7.Focus()
            Case "TxtSlcJrItemCd8"                   '選択比較項目-ＪＲ品目コード８
                CODENAME_get("ITEM", TxtSlcJrItemCd8.Text, LblSlcJrItemCd8Name.Text, WW_Dummy)
                TxtSlcJrItemCd8.Focus()
            Case "TxtSlcJrItemCd9"                   '選択比較項目-ＪＲ品目コード９
                CODENAME_get("ITEM", TxtSlcJrItemCd9.Text, LblSlcJrItemCd9Name.Text, WW_Dummy)
                TxtSlcJrItemCd9.Focus()
            Case "TxtSlcJrItemCd10"                  '選択比較項目-ＪＲ品目コード１０
                CODENAME_get("ITEM", TxtSlcJrItemCd10.Text, LblSlcJrItemCd10Name.Text, WW_Dummy)
                TxtSlcJrItemCd10.Focus()
            Case "TxtSlcJrItemCdCond"                '選択比較項目-ＪＲ品目コード比較
                CODENAME_get("COMPARECONDKBN", TxtSlcJrItemCdCond.Text, LblSlcJrItemCdCondName.Text, WW_Dummy)
                TxtSlcJrItemCdCond.Focus()
            Case "TxtSprUseFeeRateRound1"            '特例置換項目-使用料率端数整理1
                CODENAME_get("HASUU1", TxtSprUseFeeRateRound1.Text, LblSprUseFeeRateRound1Name.Text, WW_Dummy)
                TxtSprUseFeeRateRound1.Focus()
            Case "TxtSprUseFeeRateRound2"            '特例置換項目-使用料率端数整理2
                CODENAME_get("HASUU2", TxtSprUseFeeRateRound2.Text, LblSprUseFeeRateRound2Name.Text, WW_Dummy)
                TxtSprUseFeeRateRound2.Focus()
            Case "TxtSprUseFeeRateAddSubCond1"       '特例置換項目-使用料率加減額端数整理1
                CODENAME_get("HASUU1", TxtSprUseFeeRateAddSubCond1.Text, LblSprUseFeeRateAddSubCond1Name.Text, WW_Dummy)
                TxtSprUseFeeRateAddSubCond1.Focus()
            Case "TxtSprUseFeeRateAddSubCond2"       '特例置換項目-使用料率加減額端数整理2
                CODENAME_get("HASUU2", TxtSprUseFeeRateAddSubCond2.Text, LblSprUseFeeRateAddSubCond2Name.Text, WW_Dummy)
                TxtSprUseFeeRateAddSubCond2.Focus()
            Case "TxtSprRoundPointKbn"               '特例置換項目-端数処理時点区分
                CODENAME_get("HASUUPOINTKBN", TxtSprRoundPointKbn.Text, LblSprRoundPointKbnName.Text, WW_Dummy)
                TxtSprRoundPointKbn.Focus()
            Case "TxtSprUseFreeSpe"                  '特例置換項目-使用料無料特認
                CODENAME_get("USEFREEKBN", TxtSprUseFreeSpe.Text, LblSprUseFreeSpeName.Text, WW_Dummy)
                TxtSprUseFreeSpe.Focus()
            Case "TxtSprFitKbn"                      '特例置換項目-適合区分
                CODENAME_get("FITKBN", TxtSprFitKbn.Text, LblSprFitKbnName.Text, WW_Dummy)
                TxtSprFitKbn.Focus()
        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

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
        Dim WW_Date As Date

        '○ 選択内容を取得
        If leftview.WF_LeftListBox.SelectedIndex >= 0 Then
            WF_SELectedIndex.Value = leftview.WF_LeftListBox.SelectedIndex
            WW_SelectValue = leftview.WF_LeftListBox.Items(WF_SELectedIndex.Value).Value
            WW_SelectText = leftview.WF_LeftListBox.Items(WF_SELectedIndex.Value).Text
        End If

        '○ 選択内容を画面項目へセット
        If String.IsNullOrEmpty(WF_FIELD_REP.Value) Then
            Select Case WF_FIELD.Value
                Case "TxtDelFlg"                         '削除フラグ
                    TxtDelFlg.Text = WW_SelectValue
                    LblDelFlgName.Text = WW_SelectText
                    TxtDelFlg.Focus()
                Case "TxtOrgCode"                        '組織コード
                    TxtOrgCode.Text = WW_SelectValue
                    LblOrgName.Text = WW_SelectText
                    TxtOrgCode.Focus()
                Case "TxtBigCTNCD"                       '大分類コード
                    TxtBigCTNCD.Text = WW_SelectValue
                    LblBigCTNCDName.Text = WW_SelectText
                    TxtBigCTNCD.Focus()
                Case "TxtMiddleCTNCD"                    '中分類コード
                    TxtMiddleCTNCD.Text = WW_SelectValue
                    LblMiddleCTNCDName.Text = WW_SelectText
                    TxtMiddleCTNCD.Focus()
                Case "TxtDepStation"                     '発駅コード
                    TxtDepStation.Text = WW_SelectValue
                    LblDepStationName.Text = WW_SelectText
                    TxtDepStation.Focus()
                Case "TxtDepTrusteeCd"                   '発受託人コード
                    TxtDepTrusteeCd.Text = WW_SelectValue
                    LblDepTrusteeCdName.Text = WW_SelectText
                    TxtDepTrusteeCd.Focus()
                Case "TxtDepTrusteeSubCd"                '発受託人サブコード
                    TxtDepTrusteeSubCd.Text = WW_SelectValue
                    LblDepTrusteeSubCdName.Text = WW_SelectText
                    TxtDepTrusteeSubCd.Focus()
                Case "TxtCTNType"                        '選択比較項目-コンテナ記号
                    TxtCTNType.Text = WW_SelectValue
                    LblCTNTypeName.Text = WW_SelectText
                    TxtCTNType.Focus()
                Case "TxtCTNStNo"                        '選択比較項目-コンテナ番号（開始）
                    TxtCTNStNo.Text = WW_SelectValue
                    LblCTNStNoName.Text = WW_SelectText
                    TxtCTNStNo.Focus()
                Case "TxtCTNEndNo"                       '選択比較項目-コンテナ番号（終了）
                    TxtCTNEndNo.Text = WW_SelectValue
                    LblCTNEndNoName.Text = WW_SelectText
                    TxtCTNEndNo.Focus()
                Case "TxtSlcJrDepBranchCd"               '選択比較項目-ＪＲ発支社支店コード
                    TxtSlcJrDepBranchCd.Text = WW_SelectValue
                    LblSlcJrDepBranchCdName.Text = WW_SelectText
                    TxtSlcJrDepBranchCd.Focus()
                Case "TxtSlcDepShipperCd1"               '選択比較項目-発荷主コード１
                    TxtSlcDepShipperCd1.Text = WW_SelectValue
                    LblSlcDepShipperCd1Name.Text = WW_SelectText
                    TxtSlcDepShipperCd1.Focus()
                Case "TxtSlcDepShipperCd2"               '選択比較項目-発荷主コード２
                    TxtSlcDepShipperCd2.Text = WW_SelectValue
                    LblSlcDepShipperCd2Name.Text = WW_SelectText
                    TxtSlcDepShipperCd2.Focus()
                Case "TxtSlcDepShipperCd3"               '選択比較項目-発荷主コード３
                    TxtSlcDepShipperCd3.Text = WW_SelectValue
                    LblSlcDepShipperCd3Name.Text = WW_SelectText
                    TxtSlcDepShipperCd3.Focus()
                Case "TxtSlcDepShipperCd4"               '選択比較項目-発荷主コード４
                    TxtSlcDepShipperCd4.Text = WW_SelectValue
                    LblSlcDepShipperCd4Name.Text = WW_SelectText
                    TxtSlcDepShipperCd4.Focus()
                Case "TxtSlcDepShipperCd5"               '選択比較項目-発荷主コード５
                    TxtSlcDepShipperCd5.Text = WW_SelectValue
                    LblSlcDepShipperCd5Name.Text = WW_SelectText
                    TxtSlcDepShipperCd5.Focus()
                Case "TxtSlcDepShipperCdCond"            '選択比較項目-発荷主ＣＤ比較条件
                    TxtSlcDepShipperCdCond.Text = WW_SelectValue
                    LblSlcDepShipperCdCondName.Text = WW_SelectText
                    TxtSlcDepShipperCdCond.Focus()
                Case "TxtSlcJrArrBranchCd"               '選択比較項目-ＪＲ着支社支店コード
                    TxtSlcJrArrBranchCd.Text = WW_SelectValue
                    LblSlcJrArrBranchCdName.Text = WW_SelectText
                    TxtSlcJrArrBranchCd.Focus()
                Case "TxtSlcJrArrBranchCdCond"           '選択比較項目-ＪＲ着支社支店ＣＤ比較
                    TxtSlcJrArrBranchCdCond.Text = WW_SelectValue
                    LblSlcJrArrBranchCdCondName.Text = WW_SelectText
                    TxtSlcJrArrBranchCdCond.Focus()
                Case "TxtSlcJotArrOrgCode"               '選択比較項目-ＪＯＴ着組織コード
                    TxtSlcJotArrOrgCode.Text = WW_SelectValue
                    LblSlcJotArrOrgCodeName.Text = WW_SelectText
                    TxtSlcJotArrOrgCode.Focus()
                Case "TxtSlcJotArrOrgCodeCond"           '選択比較項目-ＪＯＴ着組織ＣＤ比較
                    TxtSlcJotArrOrgCodeCond.Text = WW_SelectValue
                    LblSlcJotArrOrgCodeCondName.Text = WW_SelectText
                    TxtSlcJotArrOrgCodeCond.Focus()
                Case "TxtSlcArrStation1"                 '選択比較項目-着駅コード１
                    TxtSlcArrStation1.Text = WW_SelectValue
                    LblSlcArrStation1Name.Text = WW_SelectText
                    TxtSlcArrStation1.Focus()
                Case "TxtSlcArrStation2"                 '選択比較項目-着駅コード２
                    TxtSlcArrStation2.Text = WW_SelectValue
                    LblSlcArrStation2Name.Text = WW_SelectText
                    TxtSlcArrStation2.Focus()
                Case "TxtSlcArrStation3"                 '選択比較項目-着駅コード３
                    TxtSlcArrStation3.Text = WW_SelectValue
                    LblSlcArrStation3Name.Text = WW_SelectText
                    TxtSlcArrStation3.Focus()
                Case "TxtSlcArrStation4"                 '選択比較項目-着駅コード４
                    TxtSlcArrStation4.Text = WW_SelectValue
                    LblSlcArrStation4Name.Text = WW_SelectText
                    TxtSlcArrStation4.Focus()
                Case "TxtSlcArrStation5"                 '選択比較項目-着駅コード５
                    TxtSlcArrStation5.Text = WW_SelectValue
                    LblSlcArrStation5Name.Text = WW_SelectText
                    TxtSlcArrStation5.Focus()
                Case "TxtSlcArrStation6"                 '選択比較項目-着駅コード６
                    TxtSlcArrStation6.Text = WW_SelectValue
                    LblSlcArrStation6Name.Text = WW_SelectText
                    TxtSlcArrStation6.Focus()
                Case "TxtSlcArrStation7"                 '選択比較項目-着駅コード７
                    TxtSlcArrStation7.Text = WW_SelectValue
                    LblSlcArrStation7Name.Text = WW_SelectText
                    TxtSlcArrStation7.Focus()
                Case "TxtSlcArrStation8"                 '選択比較項目-着駅コード８
                    TxtSlcArrStation8.Text = WW_SelectValue
                    LblSlcArrStation8Name.Text = WW_SelectText
                    TxtSlcArrStation8.Focus()
                Case "TxtSlcArrStation9"                 '選択比較項目-着駅コード９
                    TxtSlcArrStation9.Text = WW_SelectValue
                    LblSlcArrStation9Name.Text = WW_SelectText
                    TxtSlcArrStation9.Focus()
                Case "TxtSlcArrStation10"                '選択比較項目-着駅コード１０
                    TxtSlcArrStation10.Text = WW_SelectValue
                    LblSlcArrStation10Name.Text = WW_SelectText
                    TxtSlcArrStation10.Focus()
                Case "TxtSlcArrStationCond"              '選択比較項目-着駅コード比較条件
                    TxtSlcArrStationCond.Text = WW_SelectValue
                    LblSlcArrStationCondName.Text = WW_SelectText
                    TxtSlcArrStationCond.Focus()
                Case "TxtSlcArrTrusteeCd"                '選択比較項目-着受託人コード
                    TxtSlcArrTrusteeCd.Text = WW_SelectValue
                    LblSlcArrTrusteeCdName.Text = WW_SelectText
                    TxtSlcArrTrusteeCd.Focus()
                Case "TxtSlcArrTrusteeCdCond"            '選択比較項目-着受託人ＣＤ比較条件
                    TxtSlcArrTrusteeCdCond.Text = WW_SelectValue
                    LblSlcArrTrusteeCdCondName.Text = WW_SelectText
                    TxtSlcArrTrusteeCdCond.Focus()
                Case "TxtSlcArrTrusteeSubCd"             '選択比較項目-着受託人サブコード
                    TxtSlcArrTrusteeSubCd.Text = WW_SelectValue
                    LblSlcArrTrusteeSubCdName.Text = WW_SelectText
                    TxtSlcArrTrusteeSubCd.Focus()
                Case "TxtSlcArrTrusteeSubCdCond"         '選択比較項目-着受託人サブＣＤ比較
                    TxtSlcArrTrusteeSubCdCond.Text = WW_SelectValue
                    LblSlcArrTrusteeSubCdCondName.Text = WW_SelectText
                    TxtSlcArrTrusteeSubCdCond.Focus()
                Case "TxtSlcStShipMD"                    '選択比較項目-開始発送年月日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtSlcStShipMD.Text = ""
                        Else
                            TxtSlcStShipMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtSlcStShipMD.Focus()
                Case "TxtSlcEndShipMD"                    '選択比較項目-終了発送年月日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtSlcEndShipMD.Text = ""
                        Else
                            TxtSlcEndShipMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtSlcEndShipMD.Focus()
                Case "TxtSlcJrItemCd1"                   '選択比較項目-ＪＲ品目コード１
                    TxtSlcJrItemCd1.Text = WW_SelectValue
                    LblSlcJrItemCd1Name.Text = WW_SelectText
                    TxtSlcJrItemCd1.Focus()
                Case "TxtSlcJrItemCd2"                   '選択比較項目-ＪＲ品目コード２
                    TxtSlcJrItemCd2.Text = WW_SelectValue
                    LblSlcJrItemCd2Name.Text = WW_SelectText
                    TxtSlcJrItemCd2.Focus()
                Case "TxtSlcJrItemCd3"                   '選択比較項目-ＪＲ品目コード３
                    TxtSlcJrItemCd3.Text = WW_SelectValue
                    LblSlcJrItemCd3Name.Text = WW_SelectText
                    TxtSlcJrItemCd3.Focus()
                Case "TxtSlcJrItemCd4"                   '選択比較項目-ＪＲ品目コード４
                    TxtSlcJrItemCd4.Text = WW_SelectValue
                    LblSlcJrItemCd4Name.Text = WW_SelectText
                    TxtSlcJrItemCd4.Focus()
                Case "TxtSlcJrItemCd5"                   '選択比較項目-ＪＲ品目コード５
                    TxtSlcJrItemCd5.Text = WW_SelectValue
                    LblSlcJrItemCd5Name.Text = WW_SelectText
                    TxtSlcJrItemCd5.Focus()
                Case "TxtSlcJrItemCd6"                   '選択比較項目-ＪＲ品目コード６
                    TxtSlcJrItemCd6.Text = WW_SelectValue
                    LblSlcJrItemCd6Name.Text = WW_SelectText
                    TxtSlcJrItemCd6.Focus()
                Case "TxtSlcJrItemCd7"                   '選択比較項目-ＪＲ品目コード７
                    TxtSlcJrItemCd7.Text = WW_SelectValue
                    LblSlcJrItemCd7Name.Text = WW_SelectText
                    TxtSlcJrItemCd7.Focus()
                Case "TxtSlcJrItemCd8"                   '選択比較項目-ＪＲ品目コード８
                    TxtSlcJrItemCd8.Text = WW_SelectValue
                    LblSlcJrItemCd8Name.Text = WW_SelectText
                    TxtSlcJrItemCd8.Focus()
                Case "TxtSlcJrItemCd9"                   '選択比較項目-ＪＲ品目コード９
                    TxtSlcJrItemCd9.Text = WW_SelectValue
                    LblSlcJrItemCd9Name.Text = WW_SelectText
                    TxtSlcJrItemCd9.Focus()
                Case "TxtSlcJrItemCd10"                  '選択比較項目-ＪＲ品目コード１０
                    TxtSlcJrItemCd10.Text = WW_SelectValue
                    LblSlcJrItemCd10Name.Text = WW_SelectText
                    TxtSlcJrItemCd10.Focus()
                Case "TxtSlcJrItemCdCond"                '選択比較項目-ＪＲ品目コード比較
                    TxtSlcJrItemCdCond.Text = WW_SelectValue
                    LblSlcJrItemCdCondName.Text = WW_SelectText
                    TxtSlcJrItemCdCond.Focus()
                Case "TxtSprUseFeeRateRound1"            '特例置換項目-使用料率端数整理1
                    TxtSprUseFeeRateRound1.Text = WW_SelectValue
                    LblSprUseFeeRateRound1Name.Text = WW_SelectText
                    TxtSprUseFeeRateRound1.Focus()
                Case "TxtSprUseFeeRateRound2"            '特例置換項目-使用料率端数整理2
                    TxtSprUseFeeRateRound2.Text = WW_SelectValue
                    LblSprUseFeeRateRound2Name.Text = WW_SelectText
                    TxtSprUseFeeRateRound2.Focus()
                Case "TxtSprUseFeeRateAddSubCond1"       '特例置換項目-使用料率加減額端数整理1
                    TxtSprUseFeeRateAddSubCond1.Text = WW_SelectValue
                    LblSprUseFeeRateAddSubCond1Name.Text = WW_SelectText
                    TxtSprUseFeeRateAddSubCond1.Focus()
                Case "TxtSprUseFeeRateAddSubCond2"       '特例置換項目-使用料率加減額端数整理2
                    TxtSprUseFeeRateAddSubCond2.Text = WW_SelectValue
                    LblSprUseFeeRateAddSubCond2Name.Text = WW_SelectText
                    TxtSprUseFeeRateAddSubCond2.Focus()
                Case "TxtSprRoundPointKbn"               '特例置換項目-端数処理時点区分
                    TxtSprRoundPointKbn.Text = WW_SelectValue
                    LblSprRoundPointKbnName.Text = WW_SelectText
                    TxtSprRoundPointKbn.Focus()
                Case "TxtSprUseFreeSpe"                  '特例置換項目-使用料無料特認
                    TxtSprUseFreeSpe.Text = WW_SelectValue
                    LblSprUseFreeSpeName.Text = WW_SelectText
                    TxtSprUseFreeSpe.Focus()
                Case "TxtSprFitKbn"                      '特例置換項目-適合区分
                    TxtSprFitKbn.Text = WW_SelectValue
                    LblSprFitKbnName.Text = WW_SelectText
                    TxtSprFitKbn.Focus()
            End Select
        End If

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
                Case "TxtDelFlg"                         '削除フラグ
                    TxtDelFlg.Focus()
                Case "TxtOrgCode"                        '組織コード
                    TxtOrgCode.Focus()
                Case "TxtBigCTNCD"                       '大分類コード
                    TxtBigCTNCD.Focus()
                Case "TxtMiddleCTNCD"                    '中分類コード
                    TxtMiddleCTNCD.Focus()
                Case "TxtDepStation"                     '発駅コード
                    TxtDepStation.Focus()
                Case "TxtDepTrusteeCd"                   '発受託人コード
                    TxtDepTrusteeCd.Focus()
                Case "TxtDepTrusteeSubCd"                '発受託人サブコード
                    TxtDepTrusteeSubCd.Focus()
                Case "TxtCTNType"                        '選択比較項目-コンテナ記号
                    TxtCTNType.Focus()
                Case "TxtCTNStNo"                        '選択比較項目-コンテナ番号（開始）
                    TxtCTNStNo.Focus()
                Case "TxtCTNEndNo"                       '選択比較項目-コンテナ番号（終了）
                    TxtCTNEndNo.Focus()
                Case "TxtSlcJrDepBranchCd"               '選択比較項目-ＪＲ発支社支店コード
                    TxtSlcJrDepBranchCd.Focus()
                Case "TxtSlcDepShipperCd1"               '選択比較項目-発荷主コード１
                    TxtSlcDepShipperCd1.Focus()
                Case "TxtSlcDepShipperCd2"               '選択比較項目-発荷主コード２
                    TxtSlcDepShipperCd2.Focus()
                Case "TxtSlcDepShipperCd3"               '選択比較項目-発荷主コード３
                    TxtSlcDepShipperCd3.Focus()
                Case "TxtSlcDepShipperCd4"               '選択比較項目-発荷主コード４
                    TxtSlcDepShipperCd4.Focus()
                Case "TxtSlcDepShipperCd5"               '選択比較項目-発荷主コード５
                    TxtSlcDepShipperCd5.Focus()
                Case "TxtSlcDepShipperCdCond"            '選択比較項目-発荷主ＣＤ比較条件
                    TxtSlcDepShipperCdCond.Focus()
                Case "TxtSlcJrArrBranchCd"               '選択比較項目-ＪＲ着支社支店コード
                    TxtSlcJrArrBranchCd.Focus()
                Case "TxtSlcJrArrBranchCdCond"           '選択比較項目-ＪＲ着支社支店ＣＤ比較
                    TxtSlcJrArrBranchCdCond.Focus()
                Case "TxtSlcJotArrOrgCode"               '選択比較項目-ＪＯＴ着組織コード
                    TxtSlcJotArrOrgCode.Focus()
                Case "TxtSlcJotArrOrgCodeCond"           '選択比較項目-ＪＯＴ着組織ＣＤ比較
                    TxtSlcJotArrOrgCodeCond.Focus()
                Case "TxtSlcArrStation1"                 '選択比較項目-着駅コード１
                    TxtSlcArrStation1.Focus()
                Case "TxtSlcArrStation2"                 '選択比較項目-着駅コード２
                    TxtSlcArrStation2.Focus()
                Case "TxtSlcArrStation3"                 '選択比較項目-着駅コード３
                    TxtSlcArrStation3.Focus()
                Case "TxtSlcArrStation4"                 '選択比較項目-着駅コード４
                    TxtSlcArrStation4.Focus()
                Case "TxtSlcArrStation5"                 '選択比較項目-着駅コード５
                    TxtSlcArrStation5.Focus()
                Case "TxtSlcArrStation6"                 '選択比較項目-着駅コード６
                    TxtSlcArrStation6.Focus()
                Case "TxtSlcArrStation7"                 '選択比較項目-着駅コード７
                    TxtSlcArrStation7.Focus()
                Case "TxtSlcArrStation8"                 '選択比較項目-着駅コード８
                    TxtSlcArrStation8.Focus()
                Case "TxtSlcArrStation9"                 '選択比較項目-着駅コード９
                    TxtSlcArrStation9.Focus()
                Case "TxtSlcArrStation10"                '選択比較項目-着駅コード１０
                    TxtSlcArrStation10.Focus()
                Case "TxtSlcArrStationCond"              '選択比較項目-着駅コード比較条件
                    TxtSlcArrStationCond.Focus()
                Case "TxtSlcArrTrusteeCd"                '選択比較項目-着受託人コード
                    TxtSlcArrTrusteeCd.Focus()
                Case "TxtSlcArrTrusteeCdCond"            '選択比較項目-着受託人ＣＤ比較条件
                    TxtSlcArrTrusteeCdCond.Focus()
                Case "TxtSlcArrTrusteeSubCd"             '選択比較項目-着受託人サブコード
                    TxtSlcArrTrusteeSubCd.Focus()
                Case "TxtSlcArrTrusteeSubCdCond"         '選択比較項目-着受託人サブＣＤ比較
                    TxtSlcArrTrusteeSubCdCond.Focus()
                Case "TxtSlcStShipMD"                    '選択比較項目-開始発送年月日
                    TxtSlcStShipMD.Focus()
                Case "TxtSlcEndShipMD"                   '選択比較項目-終了発送年月日
                    TxtSlcEndShipMD.Focus()
                Case "TxtSlcJrItemCd1"                   '選択比較項目-ＪＲ品目コード１
                    TxtSlcJrItemCd1.Focus()
                Case "TxtSlcJrItemCd2"                   '選択比較項目-ＪＲ品目コード２
                    TxtSlcJrItemCd2.Focus()
                Case "TxtSlcJrItemCd3"                   '選択比較項目-ＪＲ品目コード３
                    TxtSlcJrItemCd3.Focus()
                Case "TxtSlcJrItemCd4"                   '選択比較項目-ＪＲ品目コード４
                    TxtSlcJrItemCd4.Focus()
                Case "TxtSlcJrItemCd5"                   '選択比較項目-ＪＲ品目コード５
                    TxtSlcJrItemCd5.Focus()
                Case "TxtSlcJrItemCd6"                   '選択比較項目-ＪＲ品目コード６
                    TxtSlcJrItemCd6.Focus()
                Case "TxtSlcJrItemCd7"                   '選択比較項目-ＪＲ品目コード７
                    TxtSlcJrItemCd7.Focus()
                Case "TxtSlcJrItemCd8"                   '選択比較項目-ＪＲ品目コード８
                    TxtSlcJrItemCd8.Focus()
                Case "TxtSlcJrItemCd9"                   '選択比較項目-ＪＲ品目コード９
                    TxtSlcJrItemCd9.Focus()
                Case "TxtSlcJrItemCd10"                  '選択比較項目-ＪＲ品目コード１０
                    TxtSlcJrItemCd10.Focus()
                Case "TxtSlcJrItemCdCond"                '選択比較項目-ＪＲ品目コード比較
                    TxtSlcJrItemCdCond.Focus()
                Case "TxtSprUseFeeRateRound1"            '特例置換項目-使用料率端数整理1
                    TxtSprUseFeeRateRound1.Focus()
                Case "TxtSprUseFeeRateRound2"            '特例置換項目-使用料率端数整理2
                    TxtSprUseFeeRateRound2.Focus()
                Case "TxtSprUseFeeRateAddSubCond1"       '特例置換項目-使用料率加減額端数整理1
                    TxtSprUseFeeRateAddSubCond1.Focus()
                Case "TxtSprUseFeeRateAddSubCond2"       '特例置換項目-使用料率加減額端数整理2
                    TxtSprUseFeeRateAddSubCond2.Focus()
                Case "TxtSprRoundPointKbn"               '特例置換項目-端数処理時点区分
                    TxtSprRoundPointKbn.Focus()
                Case "TxtSprUseFreeSpe"                  '特例置換項目-使用料無料特認
                    TxtSprUseFreeSpe.Focus()
                Case "TxtSprFitKbn"                      '特例置換項目-適合区分
                    TxtSprFitKbn.Focus()
            End Select
        End If

        WF_FIELD.Value = ""
        WF_FIELD_REP.Value = ""
        WF_LeftboxOpen.Value = ""
        WF_RightboxOpen.Value = ""

    End Sub

    ''' <summary>
    ''' 駅検索時処理
    ''' </summary>
    Protected Sub DisplayView_mspStationSingle(Optional ByVal prmKey As String = "")

        Me.mspStationSingle.InitPopUp()
        Me.mspStationSingle.SelectionMode = ListSelectionMode.Single
        Me.mspStationSingle.SQL = CmnSearchSQL.GetStationSQL(work.WF_SEL_CAMPCODE.Text)

        Me.mspStationSingle.KeyFieldName = "KEYCODE"
        Me.mspStationSingle.DispFieldList.AddRange(CmnSearchSQL.GetStationTitle)

        Me.mspStationSingle.ShowPopUpList(prmKey)

    End Sub

    ''' <summary>
    ''' 駅選択ポップアップで行選択
    ''' </summary>
    Protected Sub RowSelected_mspStationSingle()

        Dim selData = Me.mspStationSingle.SelectedSingleItem

        '○ 変更した項目の名称をセット
        Select Case WF_FIELD.Value

            Case TxtDepStation.ID                       '発駅コード
                Me.TxtDepStation.Text = selData("STATION").ToString
                Me.LblDepStationName.Text = selData("NAMES").ToString
                Me.TxtDepStation.Focus()

            Case TxtSlcArrStation1.ID                   '選択比較項目-着駅コード１
                Me.TxtSlcArrStation1.Text = selData("STATION").ToString
                Me.LblSlcArrStation1Name.Text = selData("NAMES").ToString
                Me.TxtSlcArrStation1.Focus()

            Case TxtSlcArrStation2.ID                   '選択比較項目-着駅コード２
                Me.TxtSlcArrStation2.Text = selData("STATION").ToString
                Me.LblSlcArrStation2Name.Text = selData("NAMES").ToString
                Me.TxtSlcArrStation2.Focus()

            Case TxtSlcArrStation3.ID                   '選択比較項目-着駅コード３
                Me.TxtSlcArrStation3.Text = selData("STATION").ToString
                Me.LblSlcArrStation3Name.Text = selData("NAMES").ToString
                Me.TxtSlcArrStation3.Focus()

            Case TxtSlcArrStation4.ID                   '選択比較項目-着駅コード４
                Me.TxtSlcArrStation4.Text = selData("STATION").ToString
                Me.LblSlcArrStation4Name.Text = selData("NAMES").ToString
                Me.TxtSlcArrStation4.Focus()

            Case TxtSlcArrStation5.ID                   '選択比較項目-着駅コード５
                Me.TxtSlcArrStation5.Text = selData("STATION").ToString
                Me.LblSlcArrStation5Name.Text = selData("NAMES").ToString
                Me.TxtSlcArrStation5.Focus()

            Case TxtSlcArrStation6.ID                   '選択比較項目-着駅コード６
                Me.TxtSlcArrStation6.Text = selData("STATION").ToString
                Me.LblSlcArrStation6Name.Text = selData("NAMES").ToString
                Me.TxtSlcArrStation6.Focus()

            Case TxtSlcArrStation7.ID                   '選択比較項目-着駅コード７
                Me.TxtSlcArrStation7.Text = selData("STATION").ToString
                Me.LblSlcArrStation7Name.Text = selData("NAMES").ToString
                Me.TxtSlcArrStation7.Focus()

            Case TxtSlcArrStation8.ID                   '選択比較項目-着駅コード８
                Me.TxtSlcArrStation8.Text = selData("STATION").ToString
                Me.LblSlcArrStation8Name.Text = selData("NAMES").ToString
                Me.TxtSlcArrStation8.Focus()

            Case TxtSlcArrStation9.ID                   '選択比較項目-着駅コード９
                Me.TxtSlcArrStation9.Text = selData("STATION").ToString
                Me.LblSlcArrStation9Name.Text = selData("NAMES").ToString
                Me.TxtSlcArrStation9.Focus()

            Case TxtSlcArrStation10.ID                  '選択比較項目-着駅コード１０
                Me.TxtSlcArrStation10.Text = selData("STATION").ToString
                Me.LblSlcArrStation10Name.Text = selData("NAMES").ToString
                Me.TxtSlcArrStation10.Focus()

        End Select

        'ポップアップの非表示
        Me.mspStationSingle.HidePopUp()

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
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""
        Dim WW_DBDataCheck As String = ""
        Dim WW_SlcStMD As String = ""
        Dim WW_SlcEndMD As String = ""
        Dim WW_SLCSTSHIPYMDNormal As Boolean = True
        Dim WW_SLCENDSHIPYMDNormal As Boolean = True

        '○ 画面操作権限チェック
        ' 権限チェック(操作者に更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            WW_CheckMES1 = "・使用料特例マスタ１更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0016INProw As DataRow In LNM0016INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0016INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0016INProw("DELFLG"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・削除コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・削除コードエラーです"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 組織コード(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "ORG", LNM0016INProw("ORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("ORG", LNM0016INProw("ORGCODE"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・組織コード入力エラー"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・組織コード入力エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 大分類コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIGCTNCD", LNM0016INProw("BIGCTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("BIGCTNCD", LNM0016INProw("BIGCTNCD"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・大分類コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・大分類コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 中分類コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "MIDDLECTNCD", LNM0016INProw("MIDDLECTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("MIDDLECTNCD", LNM0016INProw("MIDDLECTNCD"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・中分類コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・中分類コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 発駅コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPSTATION", LNM0016INProw("DEPSTATION"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("STATION", LNM0016INProw("DEPSTATION"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・発駅コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・発駅コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 発受託人コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEECD", LNM0016INProw("DEPTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DEPTRUSTEECD", LNM0016INProw("DEPTRUSTEECD"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・発受託人コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・発受託人コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 発受託人サブコード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DEPTRUSTEESUBCD", LNM0016INProw("DEPTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DEPTRUSTEESUBCD", LNM0016INProw("DEPTRUSTEESUBCD"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・発受託人サブコードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・発受託人サブコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 優先順位(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PRIORITYNO", LNM0016INProw("PRIORITYNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・優先順位エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 使用目的(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "PURPOSE", LNM0016INProw("PURPOSE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・使用目的エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-コンテナ記号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCCTNTYPE", LNM0016INProw("SLCCTNTYPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCCTNTYPE")) Then
                    ' 名称存在チェック
                    CODENAME_get("CTNTYPE", LNM0016INProw("SLCCTNTYPE"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-コンテナ記号エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-コンテナ記号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-コンテナ番号（開始）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCCTNSTNO", LNM0016INProw("SLCCTNSTNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCCTNSTNO")) Then
                    ' 名称存在チェック
                    CODENAME_get("CTNNO", LNM0016INProw("SLCCTNSTNO"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-コンテナ番号（開始）エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-コンテナ番号（開始）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-コンテナ番号（終了）(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCCTNENDNO", LNM0016INProw("SLCCTNENDNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCCTNENDNO")) Then
                    ' 名称存在チェック
                    CODENAME_get("CTNNO", LNM0016INProw("SLCCTNENDNO"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-コンテナ番号（終了）エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-コンテナ番号（終了）エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' コンテナ番号大小チェック(コンテナ番号（開始）・コンテナ番号（終了）)
            If Not String.IsNullOrEmpty(LNM0016INProw("SLCCTNSTNO")) AndAlso
                Not String.IsNullOrEmpty(LNM0016INProw("SLCCTNENDNO")) Then
                If CInt(LNM0016INProw("SLCCTNSTNO")) > CInt(LNM0016INProw("SLCCTNENDNO")) Then
                    WW_CheckMES1 = "・選択比較項目-コンテナ番号(開始)＆選択比較項目-コンテナ番号(終了)エラー"
                    WW_CheckMES2 = "コンテナ番号大小入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 選択比較項目-ＪＲ発支社支店コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRDEPBRANCHCD", LNM0016INProw("SLCJRDEPBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRDEPBRANCHCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("JRBRANCHCD", LNM0016INProw("SLCJRDEPBRANCHCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ発支社支店コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ発支社支店コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-発荷主コード１(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCD1", LNM0016INProw("SLCDEPSHIPPERCD1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCDEPSHIPPERCD1")) Then
                    ' 名称存在チェック
                    CODENAME_get("SHIPPER", LNM0016INProw("SLCDEPSHIPPERCD1"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発荷主コード１エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発荷主コード１エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-発荷主コード２(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCD2", LNM0016INProw("SLCDEPSHIPPERCD2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCDEPSHIPPERCD2")) Then
                    ' 名称存在チェック
                    CODENAME_get("SHIPPER", LNM0016INProw("SLCDEPSHIPPERCD2"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発荷主コード２エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発荷主コード２エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-発荷主コード３(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCD3", LNM0016INProw("SLCDEPSHIPPERCD3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCDEPSHIPPERCD3")) Then
                    ' 名称存在チェック
                    CODENAME_get("SHIPPER", LNM0016INProw("SLCDEPSHIPPERCD3"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発荷主コード３エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発荷主コード３エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-発荷主コード４(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCD4", LNM0016INProw("SLCDEPSHIPPERCD4"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCDEPSHIPPERCD4")) Then
                    ' 名称存在チェック
                    CODENAME_get("SHIPPER", LNM0016INProw("SLCDEPSHIPPERCD4"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発荷主コード４エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発荷主コード４エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-発荷主コード５(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCD5", LNM0016INProw("SLCDEPSHIPPERCD5"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCDEPSHIPPERCD5")) Then
                    ' 名称存在チェック
                    CODENAME_get("SHIPPER", LNM0016INProw("SLCDEPSHIPPERCD5"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発荷主コード５エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発荷主コード５エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-発荷主ＣＤ比較条件(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCDEPSHIPPERCDCOND", LNM0016INProw("SLCDEPSHIPPERCDCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCDEPSHIPPERCDCOND")) Then
                    ' 名称存在チェック
                    CODENAME_get("COMPARECONDKBN", LNM0016INProw("SLCDEPSHIPPERCDCOND"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-発荷主ＣＤ比較条件エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-発荷主ＣＤ比較条件エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ着支社支店コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRARRBRANCHCD", LNM0016INProw("SLCJRARRBRANCHCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRARRBRANCHCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("JRBRANCHCD", LNM0016INProw("SLCJRARRBRANCHCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ着支社支店コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ着支社支店コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ着支社支店ＣＤ比較(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRARRBRANCHCDCOND", LNM0016INProw("SLCJRARRBRANCHCDCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRARRBRANCHCDCOND")) Then
                    ' 名称存在チェック
                    CODENAME_get("COMPARECONDKBN", LNM0016INProw("SLCJRARRBRANCHCDCOND"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ着支社支店ＣＤ比較エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ着支社支店ＣＤ比較エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＯＴ着組織コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJOTARRORGCODE", LNM0016INProw("SLCJOTARRORGCODE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJOTARRORGCODE")) Then
                    ' 名称存在チェック
                    CODENAME_get("ORG", LNM0016INProw("SLCJOTARRORGCODE"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＯＴ着組織コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＯＴ着組織コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＯＴ着組織ＣＤ比較(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJOTARRORGCODECOND", LNM0016INProw("SLCJOTARRORGCODECOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJOTARRORGCODECOND")) Then
                    ' 名称存在チェック
                    CODENAME_get("COMPARECONDKBN", LNM0016INProw("SLCJOTARRORGCODECOND"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＯＴ着組織ＣＤ比較エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＯＴ着組織ＣＤ比較エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着駅１コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION1", LNM0016INProw("SLCARRSTATION1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRSTATION1")) Then
                    ' 名称存在チェック
                    CODENAME_get("STATION", LNM0016INProw("SLCARRSTATION1"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着駅コード１エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード１エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着駅２コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION2", LNM0016INProw("SLCARRSTATION2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRSTATION2")) Then
                    ' 名称存在チェック
                    CODENAME_get("STATION", LNM0016INProw("SLCARRSTATION2"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着駅コード２エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード２エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着駅３コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION3", LNM0016INProw("SLCARRSTATION3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRSTATION3")) Then
                    ' 名称存在チェック
                    CODENAME_get("STATION", LNM0016INProw("SLCARRSTATION3"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着駅コード３エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード３エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着駅４コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION4", LNM0016INProw("SLCARRSTATION4"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRSTATION4")) Then
                    ' 名称存在チェック
                    CODENAME_get("STATION", LNM0016INProw("SLCARRSTATION4"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着駅コード４エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード４エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着駅５コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION5", LNM0016INProw("SLCARRSTATION5"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRSTATION5")) Then
                    ' 名称存在チェック
                    CODENAME_get("STATION", LNM0016INProw("SLCARRSTATION5"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着駅コード５エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード５エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着駅６コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION6", LNM0016INProw("SLCARRSTATION6"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRSTATION6")) Then
                    ' 名称存在チェック
                    CODENAME_get("STATION", LNM0016INProw("SLCARRSTATION6"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着駅コード６エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード６エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着駅７コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION7", LNM0016INProw("SLCARRSTATION7"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRSTATION7")) Then
                    ' 名称存在チェック
                    CODENAME_get("STATION", LNM0016INProw("SLCARRSTATION7"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着駅コード７エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード７エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着駅８コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION8", LNM0016INProw("SLCARRSTATION8"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRSTATION8")) Then
                    ' 名称存在チェック
                    CODENAME_get("STATION", LNM0016INProw("SLCARRSTATION8"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着駅コード８エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード８エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着駅９コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION9", LNM0016INProw("SLCARRSTATION9"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRSTATION9")) Then
                    ' 名称存在チェック
                    CODENAME_get("STATION", LNM0016INProw("SLCARRSTATION9"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着駅コード９エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード９エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着駅１０コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRSTATION10", LNM0016INProw("SLCARRSTATION10"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRSTATION10")) Then
                    ' 名称存在チェック
                    CODENAME_get("STATION", LNM0016INProw("SLCARRSTATION10"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着駅コード１０エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード１０エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着駅コード比較条件(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRSTATIONCOND", LNM0016INProw("SLCARRSTATIONCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRSTATIONCOND")) Then
                    ' 名称存在チェック
                    CODENAME_get("COMPARECONDKBN", LNM0016INProw("SLCARRSTATIONCOND"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着駅コード比較条件エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着駅コード比較条件エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着受託人コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRTRUSTEECD", LNM0016INProw("SLCARRTRUSTEECD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRTRUSTEECD")) Then
                    ' 名称存在チェック
                    CODENAME_get("ARRTRUSTEECD", LNM0016INProw("SLCARRTRUSTEECD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着受託人コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着受託人コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着受託人ＣＤ比較条件(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRTRUSTEECDCOND", LNM0016INProw("SLCARRTRUSTEECDCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRTRUSTEECDCOND")) Then
                    ' 名称存在チェック
                    CODENAME_get("COMPARECONDKBN", LNM0016INProw("SLCARRTRUSTEECDCOND"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着受託人ＣＤ比較条件エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着受託人ＣＤ比較条件エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着受託人サブコード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRTRUSTEESUBCD", LNM0016INProw("SLCARRTRUSTEESUBCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRTRUSTEESUBCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("ARRTRUSTEESUBCD", LNM0016INProw("SLCARRTRUSTEESUBCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着受託人サブコードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着受託人サブコードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-着受託人サブＣＤ比較(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCARRTRUSTEESUBCDCOND", LNM0016INProw("SLCARRTRUSTEESUBCDCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCARRTRUSTEESUBCDCOND")) Then
                    ' 名称存在チェック
                    CODENAME_get("COMPARECONDKBN", LNM0016INProw("SLCARRTRUSTEESUBCDCOND"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-着受託人サブＣＤ比較エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-着受託人サブＣＤ比較エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-開始月日を年月日(YYYY/MM/DD)に変更(2000年指定)
            If Not String.IsNullOrEmpty(LNM0016INProw("SLCSTMD")) Then
                If LNM0016INProw("SLCSTMD").ToString.Length = 3 Then
                    WW_SlcStMD = DateTime.ParseExact(String.Concat("20000", LNM0016INProw("SLCSTMD")), "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None).ToString("yyyy/MM/dd")
                ElseIf LNM0016INProw("SLCSTMD").ToString.Length = 4 Then
                    WW_SlcStMD = DateTime.ParseExact(String.Concat("2000", LNM0016INProw("SLCSTMD")), "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None).ToString("yyyy/MM/dd")
                Else
                    WW_CheckMES1 = "・選択比較項目-開始月日エラーです。"
                    WW_CheckMES2 = "月日ではありません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 選択比較項目-開始月日(バリデーションチェック)
            If Not String.IsNullOrEmpty(WW_SlcStMD) Then
                Master.CheckField(Master.USERCAMP, "SLCSTMD", WW_SlcStMD, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・選択比較項目-開始月日エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 選択比較項目-終了月日を年月日(YYYY/MM/DD)に変更(2000年指定)
            If Not String.IsNullOrEmpty(LNM0016INProw("SLCENDMD")) Then
                If LNM0016INProw("SLCENDMD").ToString.Length = 3 Then
                    WW_SlcEndMD = DateTime.ParseExact(String.Concat("20000", LNM0016INProw("SLCENDMD")), "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None).ToString("yyyy/MM/dd")
                ElseIf LNM0016INProw("SLCENDMD").ToString.Length = 4 Then
                    WW_SlcEndMD = DateTime.ParseExact(String.Concat("2000", LNM0016INProw("SLCENDMD")), "yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo, System.Globalization.DateTimeStyles.None).ToString("yyyy/MM/dd")
                Else
                    WW_CheckMES1 = "・選択比較項目-終了月日エラーです。"
                    WW_CheckMES2 = "月日ではありません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 選択比較項目-終了月日(バリデーションチェック)
            If Not String.IsNullOrEmpty(WW_SlcEndMD) Then
                Master.CheckField(Master.USERCAMP, "SLCENDMD", WW_SlcStMD, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
                If Not isNormal(WW_CS0024FCheckerr) Then
                    WW_CheckMES1 = "・選択比較項目-終了月日エラーです。"
                    WW_CheckMES2 = WW_CS0024FCheckReport
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 選択比較項目-開始発送年月日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCSTSHIPYMD", LNM0016INProw("SLCSTSHIPYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCSTSHIPYMD")) Then
                    LNM0016INProw("SLCSTSHIPYMD") = CDate(LNM0016INProw("SLCSTSHIPYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-開始発送年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_SLCSTSHIPYMDNormal = False
            End If
            ' 選択比較項目-終了発送年月日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCENDSHIPYMD", LNM0016INProw("SLCENDSHIPYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCENDSHIPYMD")) Then
                    LNM0016INProw("SLCENDSHIPYMD") = CDate(LNM0016INProw("SLCENDSHIPYMD")).ToString("yyyy/MM/dd")
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-終了発送年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                WW_SLCENDSHIPYMDNormal = False
            End If
            ' 日付大小チェック(選択比較項目-開始発送年月日・終了発送年月日)
            If Not String.IsNullOrEmpty(LNM0016INProw("SLCSTSHIPYMD")) AndAlso
                Not String.IsNullOrEmpty(LNM0016INProw("SLCENDSHIPYMD")) AndAlso
                  WW_SLCSTSHIPYMDNormal = True AndAlso
                  WW_SLCENDSHIPYMDNormal = True Then
                If CDate(LNM0016INProw("SLCSTSHIPYMD")) > CDate(LNM0016INProw("SLCENDSHIPYMD")) Then
                    WW_CheckMES1 = "・選択比較項目-開始発送年月日&終了発送年月日エラー"
                    WW_CheckMES2 = "日付大小入力エラー"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 選択比較項目-ＪＲ品目コード１(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD1", LNM0016INProw("SLCJRITEMCD1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRITEMCD1")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", LNM0016INProw("SLCJRITEMCD1"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード１エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード１エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ品目コード２(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD2", LNM0016INProw("SLCJRITEMCD2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRITEMCD2")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", LNM0016INProw("SLCJRITEMCD2"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード２エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード２エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ品目コード３(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD3", LNM0016INProw("SLCJRITEMCD3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRITEMCD3")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", LNM0016INProw("SLCJRITEMCD3"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード３エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード３エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ品目コード４(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD4", LNM0016INProw("SLCJRITEMCD4"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRITEMCD4")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", LNM0016INProw("SLCJRITEMCD4"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード４エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード４エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ品目コード５(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD5", LNM0016INProw("SLCJRITEMCD5"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRITEMCD5")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", LNM0016INProw("SLCJRITEMCD5"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード５エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード５エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ品目コード６(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD6", LNM0016INProw("SLCJRITEMCD6"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRITEMCD6")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", LNM0016INProw("SLCJRITEMCD6"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード６エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード６エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ品目コード７(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD7", LNM0016INProw("SLCJRITEMCD7"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRITEMCD7")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", LNM0016INProw("SLCJRITEMCD7"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード７エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード７エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ品目コード８(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD8", LNM0016INProw("SLCJRITEMCD8"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRITEMCD8")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", LNM0016INProw("SLCJRITEMCD8"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード８エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード８エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ品目コード９(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD9", LNM0016INProw("SLCJRITEMCD9"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRITEMCD9")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", LNM0016INProw("SLCJRITEMCD9"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード９エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード９エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ品目コード１０(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCD10", LNM0016INProw("SLCJRITEMCD10"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRITEMCD10")) Then
                    ' 名称存在チェック
                    CODENAME_get("ITEM", LNM0016INProw("SLCJRITEMCD10"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード１０エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード１０エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 選択比較項目-ＪＲ品目コード比較(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SLCJRITEMCDCOND", LNM0016INProw("SLCJRITEMCDCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SLCJRITEMCDCOND")) Then
                    ' 名称存在チェック
                    CODENAME_get("COMPARECONDKBN", LNM0016INProw("SLCJRITEMCDCOND"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード比較エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・選択比較項目-ＪＲ品目コード比較エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-使用料金額(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRUSEFEE", LNM0016INProw("SPRUSEFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-使用料金額エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-使用料率(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRUSEFEERATE", LNM0016INProw("SPRUSEFEERATE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-使用料率エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 入力値チェック(使用料率)
            If String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATE")) OrElse
                LNM0016INProw("SPRUSEFEERATE") = "0" Then
                If String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEE")) OrElse
                    LNM0016INProw("SPRUSEFEE") = "0" Then
                    ' 入力値チェック(使用料金額&使用料率)
                    WW_CheckMES1 = "・特例置換項目-使用料金額・使用料率入力エラーです。"
                    WW_CheckMES2 = "どちらかを入力してください。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                ElseIf Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUB")) AndAlso
                    LNM0016INProw("SPRUSEFEERATEADDSUB") <> "0" Then
                    ' 入力値チェック(使用料率&使用料率加減額)
                    WW_CheckMES1 = "・特例置換項目-使用料率・使用料率加減額入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-使用料率が未入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                ElseIf Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUBCOND1")) OrElse
                    Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUBCOND2")) Then
                    ' 入力値チェック(使用料率&使用料率加減額端数整理)
                    WW_CheckMES1 = "・特例置換項目-使用料率・使用料率加減額端数整理入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-使用料率が未入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-使用料率端数整理(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRUSEFEERATEROUND", LNM0016INProw("SPRUSEFEERATEROUND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEROUND1")) AndAlso
                    Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEROUND2")) Then
                    ' 名称存在チェック
                    CODENAME_get("HASUU1", LNM0016INProw("SPRUSEFEERATEROUND1"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-使用料率端数整理１エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                    ' 名称存在チェック
                    CODENAME_get("HASUU2", LNM0016INProw("SPRUSEFEERATEROUND2"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-使用料率端数整理２エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                ElseIf Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEROUND1")) AndAlso String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEROUND2")) OrElse
                     String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEROUND1")) AndAlso Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEROUND2")) Then
                    WW_CheckMES1 = "・特例置換項目-使用料率端数整理１・使用料率端数整理２エラーです。"
                    WW_CheckMES2 = "両方に入力してください。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-使用料率端数整理エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 入力値チェック(使用料率&使用料率端数整理)
            If Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATE")) AndAlso
                LNM0016INProw("SPRUSEFEERATE") <> 0 AndAlso
                String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEROUND1")) AndAlso
                String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEROUND2")) Then
                WW_CheckMES1 = "・特例置換項目-使用料率&使用料率端数整理入力エラーです。"
                WW_CheckMES2 = "特例置換項目-使用料率端数整理が未入力です。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-使用料率加減額(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRUSEFEERATEADDSUB", LNM0016INProw("SPRUSEFEERATEADDSUB"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-使用料率加減額エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-使用料率加減額端数整理(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRUSEFEERATEADDSUBCOND", LNM0016INProw("SPRUSEFEERATEADDSUBCOND"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUBCOND1")) AndAlso
                    Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUBCOND2")) Then
                    ' 名称存在チェック
                    CODENAME_get("HASUU1", LNM0016INProw("SPRUSEFEERATEADDSUBCOND1"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-使用料率加減額端数整理１エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                    ' 名称存在チェック
                    CODENAME_get("HASUU2", LNM0016INProw("SPRUSEFEERATEADDSUBCOND2"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-使用料率加減額端数整理２エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                ElseIf String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUBCOND1")) AndAlso Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUBCOND")) OrElse
                     Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUBCOND1")) AndAlso String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUBCOND")) Then
                    WW_CheckMES1 = "・特例置換項目-使用料率加減額端数整理１・使用料率加減額端数整理２エラーです。"
                    WW_CheckMES2 = "両方に入力してください。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-使用料率加減額端数整理エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 入力値チェック(使用料率加減額&使用料率加減額端数整理)
            If String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUB")) OrElse
                LNM0016INProw("SPRUSEFEERATEADDSUB") = "0" Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUBCOND1")) AndAlso
                    Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUBCOND2")) Then
                    WW_CheckMES1 = "・特例置換項目-使用料率加減額・使用料率加減額端数整理入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-使用料率加減額が未入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            ElseIf Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUB")) AndAlso
                LNM0016INProw("SPRUSEFEERATEADDSUB") <> 0 Then
                If String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUBCOND1")) AndAlso
                    String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUBCOND2")) Then
                    WW_CheckMES1 = "・特例置換項目-使用料率加減額・使用料率加減額端数整理入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-使用料率加減額端数整理が未入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-端数処理時点区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRROUNDPOINTKBN", LNM0016INProw("SPRROUNDPOINTKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SPRROUNDPOINTKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("HASUUPOINTKBN", LNM0016INProw("SPRROUNDPOINTKBN"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-端数処理時点区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-端数処理時点区分です。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 入力値チェック(使用料率&端数処理時点区分) else (使用料率加減額&端数処理時点区分)
            If String.IsNullOrEmpty(LNM0016INProw("SPRROUNDPOINTKBN")) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATE")) OrElse
                    LNM0016INProw("SPRUSEFEERATE") = "0" Then
                    WW_CheckMES1 = "・特例置換項目-使用料率・端数処理時点区分入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-端数処理時点区分が未入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                ElseIf Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFEERATEADDSUB")) OrElse
                   LNM0016INProw("SPRUSEFEERATEADDSUB") = "0" Then
                    WW_CheckMES1 = "・特例置換項目-使用料率加減額・端数処理時点区分入力エラーです。"
                    WW_CheckMES2 = "特例置換項目-使用料率加減額が未入力です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            End If
            ' 特例置換項目-使用料無料特認(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRUSEFREESPE", LNM0016INProw("SPRUSEFREESPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SPRUSEFREESPE")) Then
                    ' 名称存在チェック
                    CODENAME_get("USEFREEKBN", LNM0016INProw("SPRUSEFREESPE"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-使用料無料特認エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-使用料無料特認エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-通運負担回送運賃(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRNITTSUFREESENDFEE", LNM0016INProw("SPRNITTSUFREESENDFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-通運負担回送運賃エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-運行管理料(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRMANAGEFEE", LNM0016INProw("SPRMANAGEFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-運行管理料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-荷主負担運賃(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRSHIPBURDENFEE", LNM0016INProw("SPRSHIPBURDENFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-荷主負担運賃エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-発送料(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRSHIPFEE", LNM0016INProw("SPRSHIPFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-発送料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-到着料(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRARRIVEFEE", LNM0016INProw("SPRARRIVEFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-到着料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-集荷料(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRPICKUPFEE", LNM0016INProw("SPRPICKUPFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-集荷料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-配達料(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRDELIVERYFEE", LNM0016INProw("SPRDELIVERYFEE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-配達料エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-その他１(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPROTHER1", LNM0016INProw("SPROTHER1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-その他１エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-その他２(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPROTHER2", LNM0016INProw("SPROTHER2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-その他２エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-適合区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRFITKBN", LNM0016INProw("SPRFITKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0016INProw("SPRFITKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("FITKBN", LNM0016INProw("SPRFITKBN"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・特例置換項目-適合区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・特例置換項目-使用料無料特認エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 入力値チェック(大分類コード&特例置換項目-適合区分)
            If LNM0016INProw("BIGCTNCD") = "10" AndAlso
                LNM0016INProw("SPRFITKBN") <> "0" AndAlso
                LNM0016INProw("SPRFITKBN") <> "1" AndAlso
                LNM0016INProw("SPRFITKBN") <> "2" OrElse
                LNM0016INProw("BIGCTNCD") <> "10" AndAlso
                LNM0016INProw("SPRFITKBN") <> "0" Then
                WW_CheckMES1 = "・大分類コード・特例置換項目-適合区分入力エラーです。"
                WW_CheckMES2 = "特例置換項目-適合区分が不適切です。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 特例置換項目-契約コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPRCONTRACTCD", LNM0016INProw("SPRCONTRACTCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・特例置換項目-契約コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_ORG2.Text) Then  '組織コード
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck,
                                    TxtOrgCode.Text, TxtBigCTNCD.Text,
                                    TxtMiddleCTNCD.Text, TxtDepStation.Text,
                                    TxtDepTrusteeCd.Text, TxtDepTrusteeSubCd.Text,
                                    TxtPriorityNo.Text, work.WF_SEL_TIMESTAMP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（組織コード&大分類コード&中分類コード&発駅コード&発受託人コード&優先順位）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                       "([" & LNM0016INProw("ORGCODE") & "]" &
                                       "([" & LNM0016INProw("BIGCTNCD") & "]" &
                                       "([" & LNM0016INProw("MIDDLECTNCD") & "]" &
                                       "([" & LNM0016INProw("DEPSTATION") & "]" &
                                       "([" & LNM0016INProw("DEPTRUSTEECD") & "]" &
                                       "([" & LNM0016INProw("DEPTRUSTEESUBCD") & "]" &
                                       " [" & LNM0016INProw("PRIORITYNO") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNM0016INProw("ORGCODE") = work.WF_SEL_ORG2.Text OrElse                      '組織コード
               Not LNM0016INProw("BIGCTNCD") = work.WF_SEL_BIGCTNCD2.Text OrElse                '大分類コード
               Not LNM0016INProw("MIDDLECTNCD") = work.WF_SEL_MIDDLECTNCD2.Text OrElse          '中分類コード
               Not LNM0016INProw("DEPSTATION") = work.WF_SEL_DEPSTATION2.Text OrElse            '発駅コード
               Not LNM0016INProw("DEPTRUSTEECD") = work.WF_SEL_DEPTRUSTEECD2.Text OrElse        '発受託人コード
               Not LNM0016INProw("DEPTRUSTEESUBCD") = work.WF_SEL_DEPTRUSTEESUBCD2.Text OrElse  '発受託人コード
               Not LNM0016INProw("PRIORITYNO") = work.WF_SEL_PRIORITYNO.Text Then               '優先順位
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（組織コード&大分類コード&中分類コード&発駅コード&発受託人コード&優先順位）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & LNM0016INProw("ORGCODE") & "]" &
                                       "([" & LNM0016INProw("BIGCTNCD") & "]" &
                                       "([" & LNM0016INProw("MIDDLECTNCD") & "]" &
                                       "([" & LNM0016INProw("DEPSTATION") & "]" &
                                       "([" & LNM0016INProw("DEPTRUSTEECD") & "]" &
                                       "([" & LNM0016INProw("DEPTRUSTEESUBCD") & "]" &
                                       " [" & LNM0016INProw("PRIORITYNO") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If WW_LineErr = "" Then
                If LNM0016INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0016INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0016INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0016INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
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
    ''' LNM0016tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0016tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0016row As DataRow In LNM0016tbl.Rows
            Select Case LNM0016row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0016row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0016INProw As DataRow In LNM0016INPtbl.Rows
            ' エラーレコード読み飛ばし
            If LNM0016INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0016INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0016row As DataRow In LNM0016tbl.Rows
                ' KEY項目が等しい時
                If LNM0016row("ORGCODE") = LNM0016INProw("ORGCODE") AndAlso                                '組織コード
                   LNM0016row("BIGCTNCD") = LNM0016INProw("BIGCTNCD") AndAlso                              '大分類コード
                   LNM0016row("MIDDLECTNCD") = LNM0016INProw("MIDDLECTNCD") AndAlso                        '中分類コード
                   LNM0016row("DEPSTATION") = LNM0016INProw("DEPSTATION") AndAlso                          '発駅コード
                   LNM0016row("DEPTRUSTEECD") = LNM0016INProw("DEPTRUSTEECD") AndAlso                      '発受託人コード
                   LNM0016row("DEPTRUSTEESUBCD") = LNM0016INProw("DEPTRUSTEESUBCD") AndAlso                '発受託人サブコード
                   LNM0016row("PRIORITYNO") = LNM0016INProw("PRIORITYNO") Then                             '優先順位
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0016row("DELFLG") = LNM0016INProw("DELFLG") AndAlso                              '削除フラグ
                       LNM0016row("PURPOSE") = LNM0016INProw("PURPOSE") AndAlso                            '使用目的
                        LNM0016row("SLCCTNTYPE") = LNM0016INProw("SLCCTNTYPE") AndAlso                            '選択比較項目-コンテナ記号
                        LNM0016row("SLCCTNSTNO") = LNM0016INProw("SLCCTNSTNO") AndAlso                            '選択比較項目-コンテナ番号（開始）
                        LNM0016row("SLCCTNENDNO") = LNM0016INProw("SLCCTNENDNO") AndAlso                          '選択比較項目-コンテナ番号（終了）
                        LNM0016row("SLCJRDEPBRANCHCD") = LNM0016INProw("SLCJRDEPBRANCHCD") AndAlso                '選択比較項目-ＪＲ発支社支店コード
                        LNM0016row("SLCDEPSHIPPERCD1") = LNM0016INProw("SLCDEPSHIPPERCD1") AndAlso                '選択比較項目-発荷主コード１
                        LNM0016row("SLCDEPSHIPPERCD2") = LNM0016INProw("SLCDEPSHIPPERCD2") AndAlso                '選択比較項目-発荷主コード２
                        LNM0016row("SLCDEPSHIPPERCD3") = LNM0016INProw("SLCDEPSHIPPERCD3") AndAlso                '選択比較項目-発荷主コード３
                        LNM0016row("SLCDEPSHIPPERCD4") = LNM0016INProw("SLCDEPSHIPPERCD4") AndAlso                '選択比較項目-発荷主コード４
                        LNM0016row("SLCDEPSHIPPERCD5") = LNM0016INProw("SLCDEPSHIPPERCD5") AndAlso                '選択比較項目-発荷主コード５
                        LNM0016row("SLCDEPSHIPPERCDCOND") = LNM0016INProw("SLCDEPSHIPPERCDCOND") AndAlso          '選択比較項目-発荷主ＣＤ比較条件
                        LNM0016row("SLCJRARRBRANCHCD") = LNM0016INProw("SLCJRARRBRANCHCD") AndAlso                '選択比較項目-ＪＲ着支社支店コード
                        LNM0016row("SLCJRARRBRANCHCDCOND") = LNM0016INProw("SLCJRARRBRANCHCDCOND") AndAlso        '選択比較項目-ＪＲ着支社支店ＣＤ比較
                        LNM0016row("SLCJOTARRORGCODE") = LNM0016INProw("SLCJOTARRORGCODE") AndAlso                '選択比較項目-ＪＯＴ着組織コード
                        LNM0016row("SLCJOTARRORGCODECOND") = LNM0016INProw("SLCJOTARRORGCODECOND") AndAlso        '選択比較項目-ＪＯＴ着組織ＣＤ比較
                        LNM0016row("SLCARRSTATION1") = LNM0016INProw("SLCARRSTATION1") AndAlso                    '選択比較項目-着駅コード１
                        LNM0016row("SLCARRSTATION2") = LNM0016INProw("SLCARRSTATION2") AndAlso                    '選択比較項目-着駅コード２
                        LNM0016row("SLCARRSTATION3") = LNM0016INProw("SLCARRSTATION3") AndAlso                    '選択比較項目-着駅コード３
                        LNM0016row("SLCARRSTATION4") = LNM0016INProw("SLCARRSTATION4") AndAlso                    '選択比較項目-着駅コード４
                        LNM0016row("SLCARRSTATION5") = LNM0016INProw("SLCARRSTATION5") AndAlso                    '選択比較項目-着駅コード５
                        LNM0016row("SLCARRSTATION6") = LNM0016INProw("SLCARRSTATION6") AndAlso                    '選択比較項目-着駅コード６
                        LNM0016row("SLCARRSTATION7") = LNM0016INProw("SLCARRSTATION7") AndAlso                    '選択比較項目-着駅コード７
                        LNM0016row("SLCARRSTATION8") = LNM0016INProw("SLCARRSTATION8") AndAlso                    '選択比較項目-着駅コード８
                        LNM0016row("SLCARRSTATION9") = LNM0016INProw("SLCARRSTATION9") AndAlso                    '選択比較項目-着駅コード９
                        LNM0016row("SLCARRSTATION10") = LNM0016INProw("SLCARRSTATION10") AndAlso                  '選択比較項目-着駅コード１０
                        LNM0016row("SLCARRSTATIONCOND") = LNM0016INProw("SLCARRSTATIONCOND") AndAlso              '選択比較項目-着駅コード比較条件
                        LNM0016row("SLCARRTRUSTEECD") = LNM0016INProw("SLCARRTRUSTEECD") AndAlso                  '選択比較項目-着受託人コード
                        LNM0016row("SLCARRTRUSTEECDCOND") = LNM0016INProw("SLCARRTRUSTEECDCOND") AndAlso          '選択比較項目-着受託人ＣＤ比較条件
                        LNM0016row("SLCARRTRUSTEESUBCD") = LNM0016INProw("SLCARRTRUSTEESUBCD") AndAlso            '選択比較項目-着受託人サブコード
                        LNM0016row("SLCARRTRUSTEESUBCDCOND") = LNM0016INProw("SLCARRTRUSTEESUBCDCOND") AndAlso    '選択比較項目-着受託人サブＣＤ比較
                        LNM0016row("SLCSTMD") = LNM0016INProw("SLCSTMD") AndAlso                                  '選択比較項目-開始月日
                        LNM0016row("SLCENDMD") = LNM0016INProw("SLCENDMD") AndAlso                                '選択比較項目-終了月日
                        LNM0016row("SLCSTSHIPYMD") = LNM0016INProw("SLCSTSHIPYMD") AndAlso                        '選択比較項目-開始発送年月日
                        LNM0016row("SLCENDSHIPYMD") = LNM0016INProw("SLCENDSHIPYMD") AndAlso                      '選択比較項目-終了発送年月日
                        LNM0016row("SLCJRITEMCD1") = LNM0016INProw("SLCJRITEMCD1") AndAlso                        '選択比較項目-ＪＲ品目コード１
                        LNM0016row("SLCJRITEMCD2") = LNM0016INProw("SLCJRITEMCD2") AndAlso                        '選択比較項目-ＪＲ品目コード２
                        LNM0016row("SLCJRITEMCD3") = LNM0016INProw("SLCJRITEMCD3") AndAlso                        '選択比較項目-ＪＲ品目コード３
                        LNM0016row("SLCJRITEMCD4") = LNM0016INProw("SLCJRITEMCD4") AndAlso                        '選択比較項目-ＪＲ品目コード４
                        LNM0016row("SLCJRITEMCD5") = LNM0016INProw("SLCJRITEMCD5") AndAlso                        '選択比較項目-ＪＲ品目コード５
                        LNM0016row("SLCJRITEMCD6") = LNM0016INProw("SLCJRITEMCD6") AndAlso                        '選択比較項目-ＪＲ品目コード６
                        LNM0016row("SLCJRITEMCD7") = LNM0016INProw("SLCJRITEMCD7") AndAlso                        '選択比較項目-ＪＲ品目コード７
                        LNM0016row("SLCJRITEMCD8") = LNM0016INProw("SLCJRITEMCD8") AndAlso                        '選択比較項目-ＪＲ品目コード８
                        LNM0016row("SLCJRITEMCD9") = LNM0016INProw("SLCJRITEMCD9") AndAlso                        '選択比較項目-ＪＲ品目コード９
                        LNM0016row("SLCJRITEMCD10") = LNM0016INProw("SLCJRITEMCD10") AndAlso                      '選択比較項目-ＪＲ品目コード１０
                        LNM0016row("SLCJRITEMCDCOND") = LNM0016INProw("SLCJRITEMCDCOND") AndAlso                  '選択比較項目-ＪＲ品目コード比較
                        LNM0016row("SPRUSEFEE") = LNM0016INProw("SPRUSEFEE") AndAlso                              '特例置換項目-使用料金額
                        LNM0016row("SPRUSEFEERATE") = LNM0016INProw("SPRUSEFEERATE") AndAlso                      '特例置換項目-使用料率
                        LNM0016row("SPRUSEFEERATEROUND") = LNM0016INProw("SPRUSEFEERATEROUND") AndAlso            '特例置換項目-使用料率端数整理
                        LNM0016row("SPRUSEFEERATEADDSUB") = LNM0016INProw("SPRUSEFEERATEADDSUB") AndAlso          '特例置換項目-使用料率加減額
                        LNM0016row("SPRUSEFEERATEADDSUBCOND") = LNM0016INProw("SPRUSEFEERATEADDSUBCOND") AndAlso  '特例置換項目-使用料率加減額端数整理
                        LNM0016row("SPRROUNDPOINTKBN") = LNM0016INProw("SPRROUNDPOINTKBN") AndAlso                '特例置換項目-端数処理時点区分
                        LNM0016row("SPRUSEFREESPE") = LNM0016INProw("SPRUSEFREESPE") AndAlso                      '特例置換項目-使用料無料特認
                        LNM0016row("SPRNITTSUFREESENDFEE") = LNM0016INProw("SPRNITTSUFREESENDFEE") AndAlso        '特例置換項目-通運負担回送運賃
                        LNM0016row("SPRMANAGEFEE") = LNM0016INProw("SPRMANAGEFEE") AndAlso                        '特例置換項目-運行管理料
                        LNM0016row("SPRSHIPBURDENFEE") = LNM0016INProw("SPRSHIPBURDENFEE") AndAlso                '特例置換項目-荷主負担運賃
                        LNM0016row("SPRSHIPFEE") = LNM0016INProw("SPRSHIPFEE") AndAlso                            '特例置換項目-発送料
                        LNM0016row("SPRARRIVEFEE") = LNM0016INProw("SPRARRIVEFEE") AndAlso                        '特例置換項目-到着料
                        LNM0016row("SPRPICKUPFEE") = LNM0016INProw("SPRPICKUPFEE") AndAlso                        '特例置換項目-集荷料
                        LNM0016row("SPRDELIVERYFEE") = LNM0016INProw("SPRDELIVERYFEE") AndAlso                    '特例置換項目-配達料
                        LNM0016row("SPROTHER1") = LNM0016INProw("SPROTHER1") AndAlso                              '特例置換項目-その他１
                        LNM0016row("SPROTHER2") = LNM0016INProw("SPROTHER2") AndAlso                              '特例置換項目-その他２
                        LNM0016row("SPRFITKBN") = LNM0016INProw("SPRFITKBN") AndAlso                              '特例置換項目-適合区分
                        LNM0016row("SPRCONTRACTCD") = LNM0016INProw("SPRCONTRACTCD") AndAlso                      '特例置換項目-契約コード
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0016row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0016INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0016INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        ' 更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0016INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0016INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0016INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now

                '変更チェック
                REST1MEXISTS(SQLcon, WW_MODIFYKBN)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0016WRKINC.MODIFYKBN.AFTDATA
                End If

                ' マスタ更新
                UpdateMaster(SQLcon, WW_DATE)
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
        For Each LNM0016INProw As DataRow In LNM0016INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0016row As DataRow In LNM0016tbl.Rows
                ' 同一レコードか判定
                If LNM0016INProw("ORGCODE") = LNM0016row("ORGCODE") AndAlso                                '組織コード
                   LNM0016INProw("BIGCTNCD") = LNM0016row("BIGCTNCD") AndAlso                              '大分類コード
                   LNM0016INProw("MIDDLECTNCD") = LNM0016row("MIDDLECTNCD") AndAlso                        '中分類コード
                   LNM0016INProw("DEPSTATION") = LNM0016row("DEPSTATION") AndAlso                          '発駅コード
                   LNM0016INProw("DEPTRUSTEECD") = LNM0016row("DEPTRUSTEECD") AndAlso                      '発受託人コード
                   LNM0016INProw("DEPTRUSTEESUBCD") = LNM0016row("DEPTRUSTEESUBCD") AndAlso                '発受託人サブコード
                   LNM0016INProw("PRIORITYNO") = LNM0016row("PRIORITYNO") Then                             '優先順位
                    ' 画面入力テーブル項目設定
                    LNM0016INProw("LINECNT") = LNM0016row("LINECNT")
                    LNM0016INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0016INProw("UPDTIMSTP") = LNM0016row("UPDTIMSTP")
                    LNM0016INProw("SELECT") = 0
                    LNM0016INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0016row.ItemArray = LNM0016INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0016tbl.NewRow
                WW_NRow.ItemArray = LNM0016INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0016tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0016tbl.Rows.Add(WW_NRow)
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
    Protected Sub CODENAME_get(ByVal I_FIELD As String, ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String)

        O_TEXT = ""
        O_RTN = ""

        If String.IsNullOrEmpty(I_VALUE) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Exit Sub
        End If

        Try
            Select Case I_FIELD
                Case "ORG"                '組織コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ORG, I_VALUE, O_TEXT, O_RTN, work.CreateORGParam(GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY, Master.USERCAMP))
                Case "BIGCTNCD"           '大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS))
                Case "MIDDLECTNCD"        '中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, TxtBigCTNCD.Text))
                Case "STATION"            '発駅コード・着駅コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_STATION, I_VALUE, O_TEXT, O_RTN, work.CreateStationParam(Master.USERCAMP))
                Case "DEPTRUSTEECD"       '発受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtDepStation.Text))
                Case "DEPTRUSTEESUBCD"    '発受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtDepStation.Text, TxtDepTrusteeCd.Text))
                Case "CTNTYPE"            'コンテナ記号
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_TYPE))
                Case "CTNNO"              'コンテナ番号（開始/終了）
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, TxtCTNType.Text))
                Case "SHIPPER"            '荷主コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_SHIPPER, I_VALUE, O_TEXT, O_RTN)
                Case "ARRTRUSTEECD"       '着受託人コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD, TxtSlcArrStation1.Text))
                Case "ARRTRUSTEESUBCD"    '着受託人サブコード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_REKEJM, I_VALUE, O_TEXT, O_RTN, work.CreateTrusteeCdParam(GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_SUBCD, TxtSlcArrStation1.Text, TxtSlcArrTrusteeCd.Text))
                Case "ITEM"               '品目コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_ITEM, I_VALUE, O_TEXT, O_RTN)
                Case "JRBRANCHCD",        'JR支社支店コード
                     "COMPARECONDKBN",    '比較条件区分
                     "HASUU1",            '端数区分１
                     "HASUU2",            '端数区分２
                     "HASUUPOINTKBN",     '端数時点区分
                     "USEFREEKBN",        '使用料無料区分
                     "FITKBN"             '適合区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))

                Case "OUTPUTID"           '情報出力ID
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "PANEID"))
                Case "ONOFF"              '表示フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, "VISIBLEFLG"))
                Case "DELFLG"             '削除フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
            End Select
        Catch ex As Exception
            O_RTN = C_MESSAGE_NO.FILE_NOT_EXISTS_ERROR
            Exit Sub
        End Try

    End Sub

End Class
