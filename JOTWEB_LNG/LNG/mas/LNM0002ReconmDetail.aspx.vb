''************************************************************
' コンテナマスタメンテ登録画面
' 作成日 2022/01/18
' 更新日 2024/01/10
' 作成者 名取
' 更新者 大浜
'
' 修正履歴 : 2022/01/18 新規作成
'          : 2024/01/10 変更履歴登録機能追加
''************************************************************
Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

''' <summary>
''' コンテナマスタ登録（実行）
''' </summary>
''' <remarks></remarks>
Public Class LNM0002ReconmDetail
    Inherits Page

    ''' <summary>
    ''' ユーザ情報取得
    ''' </summary>
    Private CS0051UserInfo As New CS0051UserInfo                    'ユーザ情報取得

    '○ 検索結果格納Table
    Private LNM0002tbl As DataTable                                 '一覧格納用テーブル
    Private LNM0002INPtbl As DataTable                              'チェック用テーブル
    Private LNM0002UPDtbl As DataTable                              '更新用テーブル

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
                    Master.RecoverTable(LNM0002tbl, work.WF_SEL_INPTBL.Text)

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
                        Case "btnUpdateConfirmOK"        '戻るボタン押下後の確認ダイアログでOK押下
                            WF_UPDATE_ConfirmOkClick()
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
            If Not IsNothing(LNM0002tbl) Then
                LNM0002tbl.Clear()
                LNM0002tbl.Dispose()
                LNM0002tbl = Nothing
            End If

            If Not IsNothing(LNM0002INPtbl) Then
                LNM0002INPtbl.Clear()
                LNM0002INPtbl.Dispose()
                LNM0002INPtbl = Nothing
            End If

            If Not IsNothing(LNM0002UPDtbl) Then
                LNM0002UPDtbl.Clear()
                LNM0002UPDtbl.Dispose()
                LNM0002UPDtbl = Nothing
            End If
        End Try

    End Sub

    ''' <summary>
    ''' 初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub Initialize()

        '○ 画面ID設定
        Master.MAPID = LNM0002WRKINC.MAPIDD
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
        If Context.Handler.ToString().ToUpper() = C_PREV_MAP_LIST.LNM0002L Then
            ' Grid情報保存先のファイル名
            Master.CreateXMLSaveFile()
        End If
        'ダブルクリックから遷移
        If work.WF_SEL_DETAIL_DECISION.Text = "1" Then
            '○ 名称設定処理
            '選択行
            LblSelLineCNT.Text = work.WF_SEL_LINECNT.Text
            '削除
            TxtDelFlg.Text = work.WF_SEL_DELFLG.Text
            CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
            '画面ＩＤ
            TxtMapId.Text = "M00001"
            'コンテナ記号
            TxtCTNType.Text = work.WF_SEL_CTNTYPE2.Text
            CODENAME_get("CTNTYPE", TxtCTNType.Text, LblCTNTypeName.Text, WW_Dummy)
            'コンテナ番号
            TxtCTNNo.Text = work.WF_SEL_CTNNO2.Text
            '所管部コード
            TxtJurisdictionCD.Text = work.WF_SEL_JURISDICTIONCD.Text
            CODENAME_get("JURISDICTION", TxtJurisdictionCD.Text, LblJurisdictionCDName.Text, WW_Dummy)
            '経理資産コード
            TxtAccountingAsSetCD.Text = work.WF_SEL_ACCOUNTINGASSETSCD.Text
            CODENAME_get("ACCOUNTINGASSETSCD", TxtAccountingAsSetCD.Text, LblAccountingAsSetCDName.Text, WW_Dummy)
            '経理資産区分
            TxtAccountingAsSetKbn.Text = work.WF_SEL_ACCOUNTINGASSETSKBN.Text
            CODENAME_get("ACCOUNTINGASSETSKBN", TxtAccountingAsSetKbn.Text, LblAccountingAsSetKbnName.Text, WW_Dummy)
            'ダミー区分
            TxtDummyKbn.Text = work.WF_SEL_DUMMYKBN.Text
            CODENAME_get("DUMMYKBN", TxtDummyKbn.Text, LblDummyKbnName.Text, WW_Dummy)
            'スポット区分
            TxtSpotKbn.Text = work.WF_SEL_SPOTKBN.Text
            CODENAME_get("SPOTKBN", TxtSpotKbn.Text, LblSpotKbnName.Text, WW_Dummy)
            'スポット区分　開始年月日
            TxtSpotStYMD.Text = work.WF_SEL_SPOTSTYMD.Text
            'スポット区分　終了年月日
            TxtSpotEndYMD.Text = work.WF_SEL_SPOTENDYMD.Text
            '大分類コード
            TxtBigCTNCD.Text = work.WF_SEL_BIGCTNCD.Text
            CODENAME_get("BIGCTNCD", TxtBigCTNCD.Text, LblBigCTNCDName.Text, WW_Dummy)
            '中分類コード
            TxtMiddleCTNCD.Text = work.WF_SEL_MIDDLECTNCD.Text
            CODENAME_get("MIDDLECTNCD", TxtMiddleCTNCD.Text, LblMiddleCTNCDName.Text, WW_Dummy)
            '小分類コード
            TxtSmallCTNCD.Text = work.WF_SEL_SMALLCTNCD.Text
            CODENAME_get("SMALLCTNCD", TxtSmallCTNCD.Text, LblSmallCTNCDName.Text, WW_Dummy)
            '建造年月
            TxtConstructionYM.Text = work.WF_SEL_CONSTRUCTIONYM.Text
            'コンテナメーカー
            TxtCTNMaker.Text = work.WF_SEL_CTNMAKER.Text
            CODENAME_get("CTNMAKER", TxtCTNMaker.Text, LblCTNMakerName.Text, WW_Dummy)
            '冷凍機メーカー
            TxtFrozenMaker.Text = work.WF_SEL_FROZENMAKER.Text
            CODENAME_get("FROZENMAKER", TxtFrozenMaker.Text, LblFrozenMakerName.Text, WW_Dummy)
            '総重量
            TxtGrossWeight.Text = work.WF_SEL_GROSSWEIGHT.Text
            '荷重
            TxtCargoWeight.Text = work.WF_SEL_CARGOWEIGHT.Text
            '自重
            TxtMyWeight.Text = work.WF_SEL_MYWEIGHT.Text
            '簿価商品価格
            TxtBookValue.Text = work.WF_SEL_BOOKVALUE.Text
            '外寸・高さ
            TxtOutHeight.Text = work.WF_SEL_OUTHEIGHT.Text
            '外寸・幅
            TxtOutWidth.Text = work.WF_SEL_OUTWIDTH.Text
            '外寸・長さ
            TxtOutLength.Text = work.WF_SEL_OUTLENGTH.Text
            '内寸・高さ
            TxtInHeight.Text = work.WF_SEL_INHEIGHT.Text
            '内寸・幅
            TxtInWidth.Text = work.WF_SEL_INWIDTH.Text
            '内寸・長さ
            TxtInLength.Text = work.WF_SEL_INLENGTH.Text
            '妻入口・高さ
            TxtWifeHeight.Text = work.WF_SEL_WIFEHEIGHT.Text
            '妻入口・幅
            TxtWifeWidth.Text = work.WF_SEL_WIFEWIDTH.Text
            '側入口・高さ
            TxtSideHeight.Text = work.WF_SEL_SIDEHEIGHT.Text
            '側入口・幅
            TxtSideWidth.Text = work.WF_SEL_SIDEWIDTH.Text
            '床面積
            TxtFloorArea.Text = work.WF_SEL_FLOORAREA.Text
            '内容積・標記
            TxtInVolumeMarking.Text = work.WF_SEL_INVOLUMEMARKING.Text
            '内容積・実寸
            TxtInVolumeActua.Text = work.WF_SEL_INVOLUMEACTUA.Text
            '交番検査・ｻｲｸﾙ日数
            TxtTrainsCycleDays.Text = work.WF_SEL_TRAINSCYCLEDAYS.Text
            '交番検査・前回実施日
            TxtTrainsBeforeRunYMD.Text = work.WF_SEL_TRAINSBEFORERUNYMD.Text
            '交番検査・次回実施日
            TxtTrainsNextRunYMD.Text = work.WF_SEL_TRAINSNEXTRUNYMD.Text
            '定期検査・ｻｲｸﾙ月数
            TxtReginsCycleDays.Text = work.WF_SEL_REGINSCYCLEDAYS.Text
            '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
            TxtReginsCycleHourMeter.Text = work.WF_SEL_REGINSCYCLEHOURMETER.Text
            '定期検査・前回実施日
            TxtReginsBeforeRunYMD.Text = work.WF_SEL_REGINSBEFORERUNYMD.Text
            '定期検査・次回実施日
            TxtReginsNextRunYMD.Text = work.WF_SEL_REGINSNEXTRUNYMD.Text
            '定期検査・ｱﾜﾒｰﾀ記載日
            TxtReginsHourMeterYMD.Text = work.WF_SEL_REGINSHOURMETERYMD.Text
            '定期検査・ｱﾜﾒｰﾀ時間
            TxtReginsHourMeterTime.Text = work.WF_SEL_REGINSHOURMETERTIME.Text
            '定期検査・ｱﾜﾒｰﾀ表示桁
            TxtReginsHourMeterDSP.Text = work.WF_SEL_REGINSHOURMETERDSP.Text
            '運用開始年月日
            TxtOperationStYMD.Text = work.WF_SEL_OPERATIONSTYMD.Text
            '運用除外年月日
            TxtOperationEndYMD.Text = work.WF_SEL_OPERATIONENDYMD.Text
            '除却年月日
            TxtRetirmentYMD.Text = work.WF_SEL_RETIRMENTYMD.Text
            '複合一貫区分
            TxtCompKanKbn.Text = work.WF_SEL_COMPKANKBN.Text
            CODENAME_get("COMPKANKBN", TxtCompKanKbn.Text, LblCompKanKbnName.Text, WW_Dummy)
            '調達フラグ
            TxtSupplyFLG.Text = work.WF_SEL_SUPPLYFLG.Text
            CODENAME_get("SUPPLYFLG", TxtSupplyFLG.Text, LblSupplyFLGName.Text, WW_Dummy)
            '付帯項目１
            TxtAddItem1.Text = work.WF_SEL_ADDITEM1.Text
            CODENAME_get("ADDITEM1", TxtAddItem1.Text, LblAddItem1Name.Text, WW_Dummy)
            '付帯項目２
            TxtAddItem2.Text = work.WF_SEL_ADDITEM2.Text
            CODENAME_get("ADDITEM2", TxtAddItem2.Text, LblAddItem2Name.Text, WW_Dummy)
            '付帯項目３
            TxtAddItem3.Text = work.WF_SEL_ADDITEM3.Text
            CODENAME_get("ADDITEM3", TxtAddItem3.Text, LblAddItem3Name.Text, WW_Dummy)
            '付帯項目４
            TxtAddItem4.Text = work.WF_SEL_ADDITEM4.Text
            CODENAME_get("ADDITEM4", TxtAddItem4.Text, LblAddItem4Name.Text, WW_Dummy)
            '付帯項目５
            TxtAddItem5.Text = work.WF_SEL_ADDITEM5.Text
            CODENAME_get("ADDITEM5", TxtAddItem5.Text, LblAddItem5Name.Text, WW_Dummy)
            '付帯項目６
            TxtAddItem6.Text = work.WF_SEL_ADDITEM6.Text
            CODENAME_get("ADDITEM6", TxtAddItem6.Text, LblAddItem6Name.Text, WW_Dummy)
            '付帯項目７
            TxtAddItem7.Text = work.WF_SEL_ADDITEM7.Text
            CODENAME_get("ADDITEM7", TxtAddItem7.Text, LblAddItem7Name.Text, WW_Dummy)
            '付帯項目８
            TxtAddItem8.Text = work.WF_SEL_ADDITEM8.Text
            CODENAME_get("ADDITEM8", TxtAddItem8.Text, LblAddItem8Name.Text, WW_Dummy)
            '付帯項目９
            TxtAddItem9.Text = work.WF_SEL_ADDITEM9.Text
            CODENAME_get("ADDITEM9", TxtAddItem9.Text, LblAddItem9Name.Text, WW_Dummy)
            '付帯項目１０
            TxtAddItem10.Text = work.WF_SEL_ADDITEM10.Text
            CODENAME_get("ADDITEM10", TxtAddItem10.Text, LblAddItem10Name.Text, WW_Dummy)
            '付帯項目１１
            TxtAddItem11.Text = work.WF_SEL_ADDITEM11.Text
            CODENAME_get("ADDITEM11", TxtAddItem11.Text, LblAddItem11Name.Text, WW_Dummy)
            '付帯項目１２
            TxtAddItem12.Text = work.WF_SEL_ADDITEM12.Text
            CODENAME_get("ADDITEM12", TxtAddItem12.Text, LblAddItem12Name.Text, WW_Dummy)
            '付帯項目１３
            TxtAddItem13.Text = work.WF_SEL_ADDITEM13.Text
            CODENAME_get("ADDITEM13", TxtAddItem13.Text, LblAddItem13Name.Text, WW_Dummy)
            '付帯項目１４
            TxtAddItem14.Text = work.WF_SEL_ADDITEM14.Text
            CODENAME_get("ADDITEM14", TxtAddItem14.Text, LblAddItem14Name.Text, WW_Dummy)
            '付帯項目１５
            TxtAddItem15.Text = work.WF_SEL_ADDITEM15.Text
            CODENAME_get("ADDITEM15", TxtAddItem15.Text, LblAddItem15Name.Text, WW_Dummy)
            '付帯項目１６
            TxtAddItem16.Text = work.WF_SEL_ADDITEM16.Text
            CODENAME_get("ADDITEM16", TxtAddItem16.Text, LblAddItem16Name.Text, WW_Dummy)
            '付帯項目１７
            TxtAddItem17.Text = work.WF_SEL_ADDITEM17.Text
            CODENAME_get("ADDITEM17", TxtAddItem17.Text, LblAddItem17Name.Text, WW_Dummy)
            '付帯項目１８
            TxtAddItem18.Text = work.WF_SEL_ADDITEM18.Text
            CODENAME_get("ADDITEM18", TxtAddItem18.Text, LblAddItem18Name.Text, WW_Dummy)
            '付帯項目１９
            TxtAddItem19.Text = work.WF_SEL_ADDITEM19.Text
            CODENAME_get("ADDITEM19", TxtAddItem19.Text, LblAddItem19Name.Text, WW_Dummy)
            '付帯項目２０
            TxtAddItem20.Text = work.WF_SEL_ADDITEM20.Text
            CODENAME_get("ADDITEM20", TxtAddItem20.Text, LblAddItem20Name.Text, WW_Dummy)
            '付帯項目２１
            TxtAddItem21.Text = work.WF_SEL_ADDITEM21.Text
            CODENAME_get("ADDITEM21", TxtAddItem21.Text, LblAddItem21Name.Text, WW_Dummy)
            '付帯項目２２
            TxtAddItem22.Text = work.WF_SEL_ADDITEM22.Text
            CODENAME_get("ADDITEM22", TxtAddItem22.Text, LblAddItem22Name.Text, WW_Dummy)
            '付帯項目２３
            TxtAddItem23.Text = work.WF_SEL_ADDITEM23.Text
            CODENAME_get("ADDITEM23", TxtAddItem23.Text, LblAddItem23Name.Text, WW_Dummy)
            '付帯項目２４
            TxtAddItem24.Text = work.WF_SEL_ADDITEM24.Text
            CODENAME_get("ADDITEM24", TxtAddItem24.Text, LblAddItem24Name.Text, WW_Dummy)
            '付帯項目２５
            TxtAddItem25.Text = work.WF_SEL_ADDITEM25.Text
            CODENAME_get("ADDITEM25", TxtAddItem25.Text, LblAddItem25Name.Text, WW_Dummy)
            '付帯項目２６
            TxtAddItem26.Text = work.WF_SEL_ADDITEM26.Text
            CODENAME_get("ADDITEM26", TxtAddItem26.Text, LblAddItem26Name.Text, WW_Dummy)
            '付帯項目２７
            TxtAddItem27.Text = work.WF_SEL_ADDITEM27.Text
            CODENAME_get("ADDITEM27", TxtAddItem27.Text, LblAddItem27Name.Text, WW_Dummy)
            '付帯項目２８
            TxtAddItem28.Text = work.WF_SEL_ADDITEM28.Text
            CODENAME_get("ADDITEM28", TxtAddItem28.Text, LblAddItem28Name.Text, WW_Dummy)
            '付帯項目２９
            TxtAddItem29.Text = work.WF_SEL_ADDITEM29.Text
            CODENAME_get("ADDITEM29", TxtAddItem29.Text, LblAddItem29Name.Text, WW_Dummy)
            '付帯項目３０
            TxtAddItem30.Text = work.WF_SEL_ADDITEM30.Text
            CODENAME_get("ADDITEM30", TxtAddItem30.Text, LblAddItem30Name.Text, WW_Dummy)
            '付帯項目３１
            TxtAddItem31.Text = work.WF_SEL_ADDITEM31.Text
            CODENAME_get("ADDITEM31", TxtAddItem31.Text, LblAddItem31Name.Text, WW_Dummy)
            '付帯項目３２
            TxtAddItem32.Text = work.WF_SEL_ADDITEM32.Text
            CODENAME_get("ADDITEM32", TxtAddItem32.Text, LblAddItem32Name.Text, WW_Dummy)
            '付帯項目３３
            TxtAddItem33.Text = work.WF_SEL_ADDITEM33.Text
            CODENAME_get("ADDITEM33", TxtAddItem33.Text, LblAddItem33Name.Text, WW_Dummy)
            '付帯項目３４
            TxtAddItem34.Text = work.WF_SEL_ADDITEM34.Text
            CODENAME_get("ADDITEM34", TxtAddItem34.Text, LblAddItem34Name.Text, WW_Dummy)
            '付帯項目３５
            TxtAddItem35.Text = work.WF_SEL_ADDITEM35.Text
            CODENAME_get("ADDITEM35", TxtAddItem35.Text, LblAddItem35Name.Text, WW_Dummy)
            '付帯項目３６
            TxtAddItem36.Text = work.WF_SEL_ADDITEM36.Text
            CODENAME_get("ADDITEM36", TxtAddItem36.Text, LblAddItem36Name.Text, WW_Dummy)
            '付帯項目３７
            TxtAddItem37.Text = work.WF_SEL_ADDITEM37.Text
            CODENAME_get("ADDITEM37", TxtAddItem37.Text, LblAddItem37Name.Text, WW_Dummy)
            '付帯項目３８
            TxtAddItem38.Text = work.WF_SEL_ADDITEM38.Text
            CODENAME_get("ADDITEM38", TxtAddItem38.Text, LblAddItem38Name.Text, WW_Dummy)
            '付帯項目３９
            TxtAddItem39.Text = work.WF_SEL_ADDITEM39.Text
            CODENAME_get("ADDITEM39", TxtAddItem39.Text, LblAddItem39Name.Text, WW_Dummy)
            '付帯項目４０
            TxtAddItem40.Text = work.WF_SEL_ADDITEM40.Text
            CODENAME_get("ADDITEM40", TxtAddItem40.Text, LblAddItem40Name.Text, WW_Dummy)
            '付帯項目４１
            TxtAddItem41.Text = work.WF_SEL_ADDITEM41.Text
            CODENAME_get("ADDITEM41", TxtAddItem41.Text, LblAddItem41Name.Text, WW_Dummy)
            '付帯項目４２
            TxtAddItem42.Text = work.WF_SEL_ADDITEM42.Text
            CODENAME_get("ADDITEM42", TxtAddItem42.Text, LblAddItem42Name.Text, WW_Dummy)
            '付帯項目４３
            TxtAddItem43.Text = work.WF_SEL_ADDITEM43.Text
            CODENAME_get("ADDITEM43", TxtAddItem43.Text, LblAddItem43Name.Text, WW_Dummy)
            '付帯項目４４
            TxtAddItem44.Text = work.WF_SEL_ADDITEM44.Text
            CODENAME_get("ADDITEM44", TxtAddItem44.Text, LblAddItem44Name.Text, WW_Dummy)
            '付帯項目４５
            TxtAddItem45.Text = work.WF_SEL_ADDITEM45.Text
            CODENAME_get("ADDITEM45", TxtAddItem45.Text, LblAddItem45Name.Text, WW_Dummy)
            '付帯項目４６
            TxtAddItem46.Text = work.WF_SEL_ADDITEM46.Text
            CODENAME_get("ADDITEM46", TxtAddItem46.Text, LblAddItem46Name.Text, WW_Dummy)
            '付帯項目４７
            TxtAddItem47.Text = work.WF_SEL_ADDITEM47.Text
            CODENAME_get("ADDITEM47", TxtAddItem47.Text, LblAddItem47Name.Text, WW_Dummy)
            '付帯項目４８
            TxtAddItem48.Text = work.WF_SEL_ADDITEM48.Text
            CODENAME_get("ADDITEM48", TxtAddItem48.Text, LblAddItem48Name.Text, WW_Dummy)
            '付帯項目４９
            TxtAddItem49.Text = work.WF_SEL_ADDITEM49.Text
            CODENAME_get("ADDITEM49", TxtAddItem49.Text, LblAddItem49Name.Text, WW_Dummy)
            '付帯項目５０
            TxtAddItem50.Text = work.WF_SEL_ADDITEM50.Text
            CODENAME_get("ADDITEM50", TxtAddItem50.Text, LblAddItem50Name.Text, WW_Dummy)
            '床材質コード
            TxtFloorMaterial.Text = work.WF_SEL_FLOORMATERIAL.Text
            CODENAME_get("FLOORMATERIAL", TxtFloorMaterial.Text, LblFloorMaterialName.Text, WW_Dummy)
            'Disabled制御項目
            DisabledKeyItem.Value = work.WF_SEL_CTNTYPE2.Text

        ElseIf work.WF_SEL_DETAIL_DECISION.Text = "0" Then
            '○ 名称設定処理
            '選択行
            LblSelLineCNT.Text = work.WF_SEL_LINECNT.Text
            '削除
            TxtDelFlg.Text = work.WF_SEL_DELFLG.Text
            CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
            '画面ＩＤ
            TxtMapId.Text = "M00001"
            'コンテナ記号
            TxtCTNType.Text = work.WF_SEL_CTNTYPE2.Text
            CODENAME_get("CTNTYPE", TxtCTNType.Text, LblCTNTypeName.Text, WW_Dummy)
            'コンテナ番号
            TxtCTNNo.Text = work.WF_SEL_CTNNO2.Text
            '所管部コード
            TxtJurisdictionCD.Text = work.WF_SEL_JURISDICTIONCD.Text
            CODENAME_get("JURISDICTION", TxtJurisdictionCD.Text, LblJurisdictionCDName.Text, WW_Dummy)
            '経理資産コード
            TxtAccountingAsSetCD.Text = work.WF_SEL_ACCOUNTINGASSETSCD.Text
            CODENAME_get("ACCOUNTINGASSETSCD", TxtAccountingAsSetCD.Text, LblAccountingAsSetCDName.Text, WW_Dummy)
            '経理資産区分
            TxtAccountingAsSetKbn.Text = work.WF_SEL_ACCOUNTINGASSETSKBN.Text
            CODENAME_get("ACCOUNTINGASSETSKBN", TxtAccountingAsSetKbn.Text, LblAccountingAsSetKbnName.Text, WW_Dummy)
            'ダミー区分
            TxtDummyKbn.Text = "00"
            CODENAME_get("DUMMYKBN", TxtDummyKbn.Text, LblDummyKbnName.Text, WW_Dummy)
            'スポット区分
            TxtSpotKbn.Text = "00"
            CODENAME_get("SPOTKBN", TxtSpotKbn.Text, LblSpotKbnName.Text, WW_Dummy)
            'スポット区分　開始年月日
            TxtSpotStYMD.Text = work.WF_SEL_SPOTSTYMD.Text
            'スポット区分　終了年月日
            TxtSpotEndYMD.Text = work.WF_SEL_SPOTENDYMD.Text
            '大分類コード
            TxtBigCTNCD.Text = work.WF_SEL_BIGCTNCD.Text
            CODENAME_get("BIGCTNCD", TxtBigCTNCD.Text, LblBigCTNCDName.Text, WW_Dummy)
            '中分類コード
            TxtMiddleCTNCD.Text = work.WF_SEL_MIDDLECTNCD.Text
            CODENAME_get("MIDDLECTNCD", TxtMiddleCTNCD.Text, LblMiddleCTNCDName.Text, WW_Dummy)
            '小分類コード
            TxtSmallCTNCD.Text = work.WF_SEL_SMALLCTNCD.Text
            CODENAME_get("SMALLCTNCD", TxtSmallCTNCD.Text, LblSmallCTNCDName.Text, WW_Dummy)
            '建造年月
            TxtConstructionYM.Text = work.WF_SEL_CONSTRUCTIONYM.Text
            'コンテナメーカー
            TxtCTNMaker.Text = work.WF_SEL_CTNMAKER.Text
            CODENAME_get("CTNMAKER", TxtCTNMaker.Text, LblCTNMakerName.Text, WW_Dummy)
            '冷凍機メーカー
            TxtFrozenMaker.Text = work.WF_SEL_FROZENMAKER.Text
            CODENAME_get("FROZENMAKER", TxtFrozenMaker.Text, LblFrozenMakerName.Text, WW_Dummy)
            '総重量
            TxtGrossWeight.Text = work.WF_SEL_GROSSWEIGHT.Text
            '荷重
            TxtCargoWeight.Text = work.WF_SEL_CARGOWEIGHT.Text
            '自重
            TxtMyWeight.Text = work.WF_SEL_MYWEIGHT.Text
            '簿価商品価格
            TxtBookValue.Text = work.WF_SEL_BOOKVALUE.Text
            '外寸・高さ
            TxtOutHeight.Text = work.WF_SEL_OUTHEIGHT.Text
            '外寸・幅
            TxtOutWidth.Text = work.WF_SEL_OUTWIDTH.Text
            '外寸・長さ
            TxtOutLength.Text = work.WF_SEL_OUTLENGTH.Text
            '内寸・高さ
            TxtInHeight.Text = work.WF_SEL_INHEIGHT.Text
            '内寸・幅
            TxtInWidth.Text = work.WF_SEL_INWIDTH.Text
            '内寸・長さ
            TxtInLength.Text = work.WF_SEL_INLENGTH.Text
            '妻入口・高さ
            TxtWifeHeight.Text = work.WF_SEL_WIFEHEIGHT.Text
            '妻入口・幅
            TxtWifeWidth.Text = work.WF_SEL_WIFEWIDTH.Text
            '側入口・高さ
            TxtSideHeight.Text = work.WF_SEL_SIDEHEIGHT.Text
            '側入口・幅
            TxtSideWidth.Text = work.WF_SEL_SIDEWIDTH.Text
            '床面積
            TxtFloorArea.Text = work.WF_SEL_FLOORAREA.Text
            '内容積・標記
            TxtInVolumeMarking.Text = work.WF_SEL_INVOLUMEMARKING.Text
            '内容積・実寸
            TxtInVolumeActua.Text = work.WF_SEL_INVOLUMEACTUA.Text
            '交番検査・ｻｲｸﾙ日数
            TxtTrainsCycleDays.Text = work.WF_SEL_TRAINSCYCLEDAYS.Text
            '交番検査・前回実施日
            TxtTrainsBeforeRunYMD.Text = work.WF_SEL_TRAINSBEFORERUNYMD.Text
            '交番検査・次回実施日
            TxtTrainsNextRunYMD.Text = work.WF_SEL_TRAINSNEXTRUNYMD.Text
            '定期検査・ｻｲｸﾙ月数
            TxtReginsCycleDays.Text = work.WF_SEL_REGINSCYCLEDAYS.Text
            '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
            TxtReginsCycleHourMeter.Text = work.WF_SEL_REGINSCYCLEHOURMETER.Text
            '定期検査・前回実施日
            TxtReginsBeforeRunYMD.Text = work.WF_SEL_REGINSBEFORERUNYMD.Text
            '定期検査・次回実施日
            TxtReginsNextRunYMD.Text = work.WF_SEL_REGINSNEXTRUNYMD.Text
            '定期検査・ｱﾜﾒｰﾀ記載日
            TxtReginsHourMeterYMD.Text = work.WF_SEL_REGINSHOURMETERYMD.Text
            '定期検査・ｱﾜﾒｰﾀ時間
            TxtReginsHourMeterTime.Text = work.WF_SEL_REGINSHOURMETERTIME.Text
            '定期検査・ｱﾜﾒｰﾀ表示桁
            TxtReginsHourMeterDSP.Text = work.WF_SEL_REGINSHOURMETERDSP.Text
            '運用開始年月日
            TxtOperationStYMD.Text = work.WF_SEL_OPERATIONSTYMD.Text
            '運用除外年月日
            TxtOperationEndYMD.Text = work.WF_SEL_OPERATIONENDYMD.Text
            '除却年月日
            TxtRetirmentYMD.Text = work.WF_SEL_RETIRMENTYMD.Text
            '複合一貫区分
            TxtCompKanKbn.Text = work.WF_SEL_COMPKANKBN.Text
            CODENAME_get("COMPKANKBN", TxtCompKanKbn.Text, LblCompKanKbnName.Text, WW_Dummy)
            '調達フラグ
            TxtSupplyFLG.Text = "0"
            CODENAME_get("SUPPLYFLG", TxtSupplyFLG.Text, LblSupplyFLGName.Text, WW_Dummy)
            '付帯項目１
            TxtAddItem1.Text = "00"
            CODENAME_get("ADDITEM1", TxtAddItem1.Text, LblAddItem1Name.Text, WW_Dummy)
            '付帯項目２
            TxtAddItem2.Text = "00"
            CODENAME_get("ADDITEM2", TxtAddItem2.Text, LblAddItem2Name.Text, WW_Dummy)
            '付帯項目３
            TxtAddItem3.Text = "00"
            CODENAME_get("ADDITEM3", TxtAddItem3.Text, LblAddItem3Name.Text, WW_Dummy)
            '付帯項目４
            TxtAddItem4.Text = "00"
            CODENAME_get("ADDITEM4", TxtAddItem4.Text, LblAddItem4Name.Text, WW_Dummy)
            '付帯項目５
            TxtAddItem5.Text = "00"
            CODENAME_get("ADDITEM5", TxtAddItem5.Text, LblAddItem5Name.Text, WW_Dummy)
            '付帯項目６
            TxtAddItem6.Text = "00"
            CODENAME_get("ADDITEM6", TxtAddItem6.Text, LblAddItem6Name.Text, WW_Dummy)
            '付帯項目７
            TxtAddItem7.Text = "00"
            CODENAME_get("ADDITEM7", TxtAddItem7.Text, LblAddItem7Name.Text, WW_Dummy)
            '付帯項目８
            TxtAddItem8.Text = "00"
            CODENAME_get("ADDITEM8", TxtAddItem8.Text, LblAddItem8Name.Text, WW_Dummy)
            '付帯項目９
            TxtAddItem9.Text = "00"
            CODENAME_get("ADDITEM9", TxtAddItem9.Text, LblAddItem9Name.Text, WW_Dummy)
            '付帯項目１０
            TxtAddItem10.Text = "00"
            CODENAME_get("ADDITEM10", TxtAddItem10.Text, LblAddItem10Name.Text, WW_Dummy)
            '付帯項目１１
            TxtAddItem11.Text = "000"
            CODENAME_get("ADDITEM11", TxtAddItem11.Text, LblAddItem11Name.Text, WW_Dummy)
            '付帯項目１２
            TxtAddItem12.Text = "00"
            CODENAME_get("ADDITEM12", TxtAddItem12.Text, LblAddItem12Name.Text, WW_Dummy)
            '付帯項目１３
            TxtAddItem13.Text = "00"
            CODENAME_get("ADDITEM13", TxtAddItem13.Text, LblAddItem13Name.Text, WW_Dummy)
            '付帯項目１４
            TxtAddItem14.Text = "00"
            CODENAME_get("ADDITEM14", TxtAddItem14.Text, LblAddItem14Name.Text, WW_Dummy)
            '付帯項目１５
            TxtAddItem15.Text = "00"
            CODENAME_get("ADDITEM15", TxtAddItem15.Text, LblAddItem15Name.Text, WW_Dummy)
            '付帯項目１６
            TxtAddItem16.Text = "00"
            CODENAME_get("ADDITEM16", TxtAddItem16.Text, LblAddItem16Name.Text, WW_Dummy)
            '付帯項目１７
            TxtAddItem17.Text = "00"
            CODENAME_get("ADDITEM17", TxtAddItem17.Text, LblAddItem17Name.Text, WW_Dummy)
            '付帯項目１８
            TxtAddItem18.Text = "00"
            CODENAME_get("ADDITEM18", TxtAddItem18.Text, LblAddItem18Name.Text, WW_Dummy)
            '付帯項目１９
            TxtAddItem19.Text = "00"
            CODENAME_get("ADDITEM19", TxtAddItem19.Text, LblAddItem19Name.Text, WW_Dummy)
            '付帯項目２０
            TxtAddItem20.Text = "00"
            CODENAME_get("ADDITEM20", TxtAddItem20.Text, LblAddItem20Name.Text, WW_Dummy)
            '付帯項目２１
            TxtAddItem21.Text = "00"
            CODENAME_get("ADDITEM21", TxtAddItem21.Text, LblAddItem21Name.Text, WW_Dummy)
            '付帯項目２２
            TxtAddItem22.Text = "00"
            CODENAME_get("ADDITEM22", TxtAddItem22.Text, LblAddItem22Name.Text, WW_Dummy)
            '付帯項目２３
            TxtAddItem23.Text = "00"
            CODENAME_get("ADDITEM23", TxtAddItem23.Text, LblAddItem23Name.Text, WW_Dummy)
            '付帯項目２４
            TxtAddItem24.Text = "00"
            CODENAME_get("ADDITEM24", TxtAddItem24.Text, LblAddItem24Name.Text, WW_Dummy)
            '付帯項目２５
            TxtAddItem25.Text = "00"
            CODENAME_get("ADDITEM25", TxtAddItem25.Text, LblAddItem25Name.Text, WW_Dummy)
            '付帯項目２６
            TxtAddItem26.Text = "00"
            CODENAME_get("ADDITEM26", TxtAddItem26.Text, LblAddItem26Name.Text, WW_Dummy)
            '付帯項目２７
            TxtAddItem27.Text = "00"
            CODENAME_get("ADDITEM27", TxtAddItem27.Text, LblAddItem27Name.Text, WW_Dummy)
            '付帯項目２８
            TxtAddItem28.Text = "00"
            CODENAME_get("ADDITEM28", TxtAddItem28.Text, LblAddItem28Name.Text, WW_Dummy)
            '付帯項目２９
            TxtAddItem29.Text = "00"
            CODENAME_get("ADDITEM29", TxtAddItem29.Text, LblAddItem29Name.Text, WW_Dummy)
            '付帯項目３０
            TxtAddItem30.Text = "00"
            CODENAME_get("ADDITEM30", TxtAddItem30.Text, LblAddItem30Name.Text, WW_Dummy)
            '付帯項目３１
            TxtAddItem31.Text = "00"
            CODENAME_get("ADDITEM31", TxtAddItem31.Text, LblAddItem31Name.Text, WW_Dummy)
            '付帯項目３２
            TxtAddItem32.Text = "00"
            CODENAME_get("ADDITEM32", TxtAddItem32.Text, LblAddItem32Name.Text, WW_Dummy)
            '付帯項目３３
            TxtAddItem33.Text = "00"
            CODENAME_get("ADDITEM33", TxtAddItem33.Text, LblAddItem33Name.Text, WW_Dummy)
            '付帯項目３４
            TxtAddItem34.Text = "000"
            CODENAME_get("ADDITEM34", TxtAddItem34.Text, LblAddItem34Name.Text, WW_Dummy)
            '付帯項目３５
            TxtAddItem35.Text = "00"
            CODENAME_get("ADDITEM35", TxtAddItem35.Text, LblAddItem35Name.Text, WW_Dummy)
            '付帯項目３６
            TxtAddItem36.Text = "00"
            CODENAME_get("ADDITEM36", TxtAddItem36.Text, LblAddItem36Name.Text, WW_Dummy)
            '付帯項目３７
            TxtAddItem37.Text = "00"
            CODENAME_get("ADDITEM37", TxtAddItem37.Text, LblAddItem37Name.Text, WW_Dummy)
            '付帯項目３８
            TxtAddItem38.Text = "00"
            CODENAME_get("ADDITEM38", TxtAddItem38.Text, LblAddItem38Name.Text, WW_Dummy)
            '付帯項目３９
            TxtAddItem39.Text = "00"
            CODENAME_get("ADDITEM39", TxtAddItem39.Text, LblAddItem39Name.Text, WW_Dummy)
            '付帯項目４０
            TxtAddItem40.Text = "00"
            CODENAME_get("ADDITEM40", TxtAddItem40.Text, LblAddItem40Name.Text, WW_Dummy)
            '付帯項目４１
            TxtAddItem41.Text = "00"
            CODENAME_get("ADDITEM41", TxtAddItem41.Text, LblAddItem41Name.Text, WW_Dummy)
            '付帯項目４２
            TxtAddItem42.Text = "00"
            CODENAME_get("ADDITEM42", TxtAddItem42.Text, LblAddItem42Name.Text, WW_Dummy)
            '付帯項目４３
            TxtAddItem43.Text = "00"
            CODENAME_get("ADDITEM43", TxtAddItem43.Text, LblAddItem43Name.Text, WW_Dummy)
            '付帯項目４４
            TxtAddItem44.Text = "00"
            CODENAME_get("ADDITEM44", TxtAddItem44.Text, LblAddItem44Name.Text, WW_Dummy)
            '付帯項目４５
            TxtAddItem45.Text = "00"
            CODENAME_get("ADDITEM45", TxtAddItem45.Text, LblAddItem45Name.Text, WW_Dummy)
            '付帯項目４６
            TxtAddItem46.Text = "00"
            CODENAME_get("ADDITEM46", TxtAddItem46.Text, LblAddItem46Name.Text, WW_Dummy)
            '付帯項目４７
            TxtAddItem47.Text = "00"
            CODENAME_get("ADDITEM47", TxtAddItem47.Text, LblAddItem47Name.Text, WW_Dummy)
            '付帯項目４８
            TxtAddItem48.Text = "00"
            CODENAME_get("ADDITEM48", TxtAddItem48.Text, LblAddItem48Name.Text, WW_Dummy)
            '付帯項目４９
            TxtAddItem49.Text = "00"
            CODENAME_get("ADDITEM49", TxtAddItem49.Text, LblAddItem49Name.Text, WW_Dummy)
            '付帯項目５０
            TxtAddItem50.Text = "00"
            CODENAME_get("ADDITEM50", TxtAddItem50.Text, LblAddItem50Name.Text, WW_Dummy)
            '床材質コード
            TxtFloorMaterial.Text = work.WF_SEL_FLOORMATERIAL.Text
            CODENAME_get("FLOORMATERIAL", TxtFloorMaterial.Text, LblFloorMaterialName.Text, WW_Dummy)
            'Disabled制御項目
            DisabledKeyItem.Value = work.WF_SEL_CTNTYPE2.Text
        End If

        ' 数値(0～9)のみ入力可能とする。
        Me.TxtDelFlg.Attributes("onkeyPress") = "CheckNum()"                   '削除フラグ
        Me.TxtCTNNo.Attributes("onkeyPress") = "CheckNum()"                    'コンテナ番号
        Me.TxtJurisdictionCD.Attributes("onkeyPress") = "CheckNum()"           '所管部コード
        Me.TxtAccountingAsSetCD.Attributes("onkeyPress") = "CheckNum()"        '経理資産コード
        Me.TxtAccountingAsSetKbn.Attributes("onkeyPress") = "CheckNum()"       '経理資産区分
        Me.TxtDummyKbn.Attributes("onkeyPress") = "CheckNum()"                 'ダミー区分
        Me.TxtSpotKbn.Attributes("onkeyPress") = "CheckNum()"                  'スポット区分
        Me.TxtBigCTNCD.Attributes("onkeyPress") = "CheckNum()"                 '大分類コード
        Me.TxtMiddleCTNCD.Attributes("onkeyPress") = "CheckNum()"              '中分類コード
        Me.TxtSmallCTNCD.Attributes("onkeyPress") = "CheckNum()"               '小分類コード
        Me.TxtConstructionYM.Attributes("onkeyPress") = "CheckNum()"           '建造年月
        Me.TxtCTNMaker.Attributes("onkeyPress") = "CheckNum()"                 'コンテナメーカー
        Me.TxtFrozenMaker.Attributes("onkeyPress") = "CheckNum()"              '冷凍機メーカー
        Me.TxtBookValue.Attributes("onkeyPress") = "CheckNum()"                '簿価商品価格
        Me.TxtOutHeight.Attributes("onkeyPress") = "CheckNum()"                '外寸・高さ
        Me.TxtOutWidth.Attributes("onkeyPress") = "CheckNum()"                 '外寸・幅
        Me.TxtOutLength.Attributes("onkeyPress") = "CheckNum()"                '外寸・長さ
        Me.TxtInHeight.Attributes("onkeyPress") = "CheckNum()"                 '内寸・高さ
        Me.TxtInWidth.Attributes("onkeyPress") = "CheckNum()"                  '内寸・幅
        Me.TxtInLength.Attributes("onkeyPress") = "CheckNum()"                 '内寸・長さ
        Me.TxtWifeHeight.Attributes("onkeyPress") = "CheckNum()"               '妻入口・高さ
        Me.TxtWifeWidth.Attributes("onkeyPress") = "CheckNum()"                '妻入口・幅
        Me.TxtSideHeight.Attributes("onkeyPress") = "CheckNum()"               '側入口・高さ
        Me.TxtSideWidth.Attributes("onkeyPress") = "CheckNum()"                '側入口・幅
        Me.TxtInVolumeMarking.Attributes("onkeyPress") = "CheckNum()"          '内容積・標記
        Me.TxtTrainsCycleDays.Attributes("onkeyPress") = "CheckNum()"          '交番検査・ｻｲｸﾙ日数
        Me.TxtReginsCycleDays.Attributes("onkeyPress") = "CheckNum()"          '定期検査・ｻｲｸﾙ月数
        Me.TxtReginsCycleHourMeter.Attributes("onkeyPress") = "CheckNum()"     '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
        Me.TxtReginsHourMeterTime.Attributes("onkeyPress") = "CheckNum()"      '定期検査・ｱﾜﾒｰﾀ時間
        Me.TxtReginsHourMeterDSP.Attributes("onkeyPress") = "CheckNum()"       '定期検査・ｱﾜﾒｰﾀ表示桁
        Me.TxtCompKanKbn.Attributes("onkeyPress") = "CheckNum()"               '複合一貫区分
        Me.TxtSupplyFLG.Attributes("onkeyPress") = "CheckNum()"                '調達フラグ
        Me.TxtAddItem1.Attributes("onkeyPress") = "CheckNum()"                 '付帯項目１
        Me.TxtAddItem2.Attributes("onkeyPress") = "CheckNum()"                 '付帯項目２
        Me.TxtAddItem3.Attributes("onkeyPress") = "CheckNum()"                 '付帯項目３
        Me.TxtAddItem4.Attributes("onkeyPress") = "CheckNum()"                 '付帯項目４
        Me.TxtAddItem5.Attributes("onkeyPress") = "CheckNum()"                 '付帯項目５
        Me.TxtAddItem6.Attributes("onkeyPress") = "CheckNum()"                 '付帯項目６
        Me.TxtAddItem7.Attributes("onkeyPress") = "CheckNum()"                 '付帯項目７
        Me.TxtAddItem8.Attributes("onkeyPress") = "CheckNum()"                 '付帯項目８
        Me.TxtAddItem9.Attributes("onkeyPress") = "CheckNum()"                 '付帯項目９
        Me.TxtAddItem10.Attributes("onkeyPress") = "CheckNum()"                '付帯項目１０
        Me.TxtAddItem11.Attributes("onkeyPress") = "CheckNum()"                '付帯項目１１
        Me.TxtAddItem12.Attributes("onkeyPress") = "CheckNum()"                '付帯項目１２
        Me.TxtAddItem13.Attributes("onkeyPress") = "CheckNum()"                '付帯項目１３
        Me.TxtAddItem14.Attributes("onkeyPress") = "CheckNum()"                '付帯項目１４
        Me.TxtAddItem15.Attributes("onkeyPress") = "CheckNum()"                '付帯項目１５
        Me.TxtAddItem16.Attributes("onkeyPress") = "CheckNum()"                '付帯項目１６
        Me.TxtAddItem17.Attributes("onkeyPress") = "CheckNum()"                '付帯項目１７
        Me.TxtAddItem18.Attributes("onkeyPress") = "CheckNum()"                '付帯項目１８
        Me.TxtAddItem19.Attributes("onkeyPress") = "CheckNum()"                '付帯項目１９
        Me.TxtAddItem20.Attributes("onkeyPress") = "CheckNum()"                '付帯項目２０
        Me.TxtAddItem21.Attributes("onkeyPress") = "CheckNum()"                '付帯項目２１
        Me.TxtAddItem22.Attributes("onkeyPress") = "CheckNum()"                '付帯項目２２
        Me.TxtAddItem23.Attributes("onkeyPress") = "CheckNum()"                '付帯項目２３
        Me.TxtAddItem24.Attributes("onkeyPress") = "CheckNum()"                '付帯項目２４
        Me.TxtAddItem25.Attributes("onkeyPress") = "CheckNum()"                '付帯項目２５
        Me.TxtAddItem26.Attributes("onkeyPress") = "CheckNum()"                '付帯項目２６
        Me.TxtAddItem27.Attributes("onkeyPress") = "CheckNum()"                '付帯項目２７
        Me.TxtAddItem28.Attributes("onkeyPress") = "CheckNum()"                '付帯項目２８
        Me.TxtAddItem29.Attributes("onkeyPress") = "CheckNum()"                '付帯項目２９
        Me.TxtAddItem30.Attributes("onkeyPress") = "CheckNum()"                '付帯項目３０
        Me.TxtAddItem31.Attributes("onkeyPress") = "CheckNum()"                '付帯項目３１
        Me.TxtAddItem32.Attributes("onkeyPress") = "CheckNum()"                '付帯項目３２
        Me.TxtAddItem33.Attributes("onkeyPress") = "CheckNum()"                '付帯項目３３
        Me.TxtAddItem34.Attributes("onkeyPress") = "CheckNum()"                '付帯項目３４
        Me.TxtAddItem35.Attributes("onkeyPress") = "CheckNum()"                '付帯項目３５
        Me.TxtAddItem36.Attributes("onkeyPress") = "CheckNum()"                '付帯項目３６
        Me.TxtAddItem37.Attributes("onkeyPress") = "CheckNum()"                '付帯項目３７
        Me.TxtAddItem38.Attributes("onkeyPress") = "CheckNum()"                '付帯項目３８
        Me.TxtAddItem39.Attributes("onkeyPress") = "CheckNum()"                '付帯項目３９
        Me.TxtAddItem40.Attributes("onkeyPress") = "CheckNum()"                '付帯項目４０
        Me.TxtAddItem41.Attributes("onkeyPress") = "CheckNum()"                '付帯項目４１
        Me.TxtAddItem42.Attributes("onkeyPress") = "CheckNum()"                '付帯項目４２
        Me.TxtAddItem43.Attributes("onkeyPress") = "CheckNum()"                '付帯項目４３
        Me.TxtAddItem44.Attributes("onkeyPress") = "CheckNum()"                '付帯項目４４
        Me.TxtAddItem45.Attributes("onkeyPress") = "CheckNum()"                '付帯項目４５
        Me.TxtAddItem46.Attributes("onkeyPress") = "CheckNum()"                '付帯項目４６
        Me.TxtAddItem47.Attributes("onkeyPress") = "CheckNum()"                '付帯項目４７
        Me.TxtAddItem48.Attributes("onkeyPress") = "CheckNum()"                '付帯項目４８
        Me.TxtAddItem49.Attributes("onkeyPress") = "CheckNum()"                '付帯項目４９
        Me.TxtAddItem50.Attributes("onkeyPress") = "CheckNum()"                '付帯項目５０
        Me.TxtFloorMaterial.Attributes("onkeyPress") = "CheckNum()"            '床材質コード

        ' 入力するテキストボックスは数値(0～9)＋英字のみ可能とする。
        Me.TxtCTNType.Attributes("onkeyPress") = "CheckNumAZ()"                'コンテナ記号

        ' 入力するテキストボックスは数値(0～9)＋記号(.)のみ可能とする。
        Me.TxtGrossWeight.Attributes("onkeyPress") = "CheckDeci()"             '総重量
        Me.TxtCargoWeight.Attributes("onkeyPress") = "CheckDeci()"             '荷重
        Me.TxtMyWeight.Attributes("onkeyPress") = "CheckDeci()"                '自重
        Me.TxtFloorArea.Attributes("onkeyPress") = "CheckDeci()"               '床面積
        Me.TxtInVolumeActua.Attributes("onkeyPress") = "CheckDeci()"           '内容積・実寸

        ' 数値(0～9)＋記号(/)のみ入力可能とする。
        Me.TxtSpotStYMD.Attributes("onkeyPress") = "CheckCalendar()"           'スポット区分　開始年月日
        Me.TxtSpotEndYMD.Attributes("onkeyPress") = "CheckCalendar()"          'スポット区分　終了年月日
        Me.TxtTrainsBeforeRunYMD.Attributes("onkeyPress") = "CheckCalendar()"  '交番検査・前回実施日
        Me.TxtTrainsNextRunYMD.Attributes("onkeyPress") = "CheckCalendar()"    '交番検査・次回実施日
        Me.TxtReginsBeforeRunYMD.Attributes("onkeyPress") = "CheckCalendar()"  '定期検査・前回実施日
        Me.TxtReginsNextRunYMD.Attributes("onkeyPress") = "CheckCalendar()"    '定期検査・次回実施日
        Me.TxtReginsHourMeterYMD.Attributes("onkeyPress") = "CheckCalendar()"  '定期検査・ｱﾜﾒｰﾀ記載日
        Me.TxtOperationStYMD.Attributes("onkeyPress") = "CheckCalendar()"      '運用開始年月日
        Me.TxtOperationEndYMD.Attributes("onkeyPress") = "CheckCalendar()"     '運用除外年月日
        Me.TxtRetirmentYMD.Attributes("onkeyPress") = "CheckCalendar()"        '除却年月日

    End Sub

    ''' <summary>
    ''' 一意制約チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO"></param>
    Protected Sub UniqueKeyCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String)

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                 " _
            & "     CTNTYPE            " _
            & "   , CTNNO              " _
            & " FROM                   " _
            & "     LNG.LNM0002_RECONM " _
            & " WHERE                  " _
            & "         CTNTYPE = @P1  " _
            & "     AND CTNNO   = @P2  " _
            & "     AND DELFLG <> @P3  "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 5)  'コンテナ記号
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 8)  'コンテナ番号
                Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.VarChar, 1)  '削除フラグ

                PARA1.Value = TxtCTNType.Text      'コンテナ記号
                PARA2.Value = TxtCTNNo.Text        'コンテナ番号
                PARA3.Value = C_DELETE_FLG.DELETE  '削除フラグ

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0002Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0002Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0002Chk.Load(SQLdr)

                    If LNM0002Chk.Rows.Count > 0 Then
                        ' 重複データエラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                    Else
                        ' 正常終了時
                        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0002C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0002C UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' 自動計算値同値チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO"></param>
    Protected Sub KobanCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String)

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
            & "         CNTKEY   = @P1 " _
            & "     AND DELFLG  <> @P2 "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 5)  'コントロールキー(ＫＥＹ(交番検査))
                Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.VarChar, 1)  '削除フラグ

                PARA1.Value = WW_CntKey               'コントロールキー(ＫＥＹ(交番検査))
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

                If Integer.Parse(TxtTrainsCycleDays.Text) = AutoValue Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0002C TRAINSCYCLEDAYS_CHECK"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

        Master.Output(C_MESSAGE_NO.DATA_UPDATE_SUCCESSFUL, C_MESSAGE_TYPE.INF)

    End Sub

    ''' <summary>
    ''' コンテナマスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Protected Sub UpdateMaster(ByVal SQLcon As MySqlConnection, ByVal WW_NOW As Date)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        '○ DB更新SQL(コンテナマスタ)
        Dim sqlDetailStat As New StringBuilder
        sqlDetailStat.AppendLine(" DECLARE @hensuu AS bigint ;                 ")
        sqlDetailStat.AppendLine("     SET @hensuu = 0 ;                       ")
        sqlDetailStat.AppendLine(" DECLARE hensuu CURSOR FOR                   ")
        sqlDetailStat.AppendLine("     SELECT                                  ")
        sqlDetailStat.AppendLine("         UPDTIMSTP AS hensuu                 ")
        sqlDetailStat.AppendLine("     FROM                                    ")
        sqlDetailStat.AppendLine("         LNG.LNM0002_RECONM                  ")
        sqlDetailStat.AppendLine("     WHERE                                   ")
        sqlDetailStat.AppendLine("         CTNTYPE   = @P001                   ")
        sqlDetailStat.AppendLine("         AND CTNNO = @P002 ;                 ")
        sqlDetailStat.AppendLine(" OPEN hensuu ;                               ")
        sqlDetailStat.AppendLine(" FETCH NEXT FROM hensuu INTO @hensuu ;       ")
        sqlDetailStat.AppendLine(" IF (@@FETCH_STATUS = 0)                     ")
        sqlDetailStat.AppendLine("     UPDATE LNG.LNM0002_RECONM               ")
        sqlDetailStat.AppendLine("     SET                                     ")
        sqlDetailStat.AppendLine("         DELFLG               = @P000        ")
        sqlDetailStat.AppendLine("       , JURISDICTIONCD       = @P003        ")
        sqlDetailStat.AppendLine("       , ACCOUNTINGASSETSCD   = @P004        ")
        sqlDetailStat.AppendLine("       , ACCOUNTINGASSETSKBN  = @P005        ")
        sqlDetailStat.AppendLine("       , DUMMYKBN             = @P006        ")
        sqlDetailStat.AppendLine("       , SPOTKBN              = @P007        ")
        sqlDetailStat.AppendLine("       , SPOTSTYMD            = @P008        ")
        sqlDetailStat.AppendLine("       , SPOTENDYMD           = @P009        ")
        sqlDetailStat.AppendLine("       , BIGCTNCD             = @P010        ")
        sqlDetailStat.AppendLine("       , MIDDLECTNCD          = @P011        ")
        sqlDetailStat.AppendLine("       , SMALLCTNCD           = @P012        ")
        sqlDetailStat.AppendLine("       , CONSTRUCTIONYM       = @P013        ")
        sqlDetailStat.AppendLine("       , CTNMAKER             = @P014        ")
        sqlDetailStat.AppendLine("       , FROZENMAKER          = @P015        ")
        sqlDetailStat.AppendLine("       , GROSSWEIGHT          = @P016        ")
        sqlDetailStat.AppendLine("       , CARGOWEIGHT          = @P017        ")
        sqlDetailStat.AppendLine("       , MYWEIGHT             = @P018        ")
        sqlDetailStat.AppendLine("       , BOOKVALUE            = @P107        ")
        sqlDetailStat.AppendLine("       , OUTHEIGHT            = @P019        ")
        sqlDetailStat.AppendLine("       , OUTWIDTH             = @P020        ")
        sqlDetailStat.AppendLine("       , OUTLENGTH            = @P021        ")
        sqlDetailStat.AppendLine("       , INHEIGHT             = @P022        ")
        sqlDetailStat.AppendLine("       , INWIDTH              = @P023        ")
        sqlDetailStat.AppendLine("       , INLENGTH             = @P024        ")
        sqlDetailStat.AppendLine("       , WIFEHEIGHT           = @P025        ")
        sqlDetailStat.AppendLine("       , WIFEWIDTH            = @P026        ")
        sqlDetailStat.AppendLine("       , SIDEHEIGHT           = @P027        ")
        sqlDetailStat.AppendLine("       , SIDEWIDTH            = @P028        ")
        sqlDetailStat.AppendLine("       , FLOORAREA            = @P029        ")
        sqlDetailStat.AppendLine("       , INVOLUMEMARKING      = @P030        ")
        sqlDetailStat.AppendLine("       , INVOLUMEACTUA        = @P031        ")
        sqlDetailStat.AppendLine("       , TRAINSCYCLEDAYS      = @P032        ")
        sqlDetailStat.AppendLine("       , TRAINSBEFORERUNYMD   = @P033        ")
        sqlDetailStat.AppendLine("       , TRAINSNEXTRUNYMD     = @P034        ")
        sqlDetailStat.AppendLine("       , REGINSCYCLEDAYS      = @P035        ")
        sqlDetailStat.AppendLine("       , REGINSCYCLEHOURMETER = @P036        ")
        sqlDetailStat.AppendLine("       , REGINSBEFORERUNYMD   = @P037        ")
        sqlDetailStat.AppendLine("       , REGINSNEXTRUNYMD     = @P038        ")
        sqlDetailStat.AppendLine("       , REGINSHOURMETERYMD   = @P039        ")
        sqlDetailStat.AppendLine("       , REGINSHOURMETERTIME  = @P040        ")
        sqlDetailStat.AppendLine("       , REGINSHOURMETERDSP   = @P041        ")
        sqlDetailStat.AppendLine("       , OPERATIONSTYMD       = @P042        ")
        sqlDetailStat.AppendLine("       , OPERATIONENDYMD      = @P043        ")
        sqlDetailStat.AppendLine("       , RETIRMENTYMD         = @P044        ")
        sqlDetailStat.AppendLine("       , COMPKANKBN           = @P045        ")
        sqlDetailStat.AppendLine("       , SUPPLYFLG            = @P108        ")
        sqlDetailStat.AppendLine("       , ADDITEM1             = @P046        ")
        sqlDetailStat.AppendLine("       , ADDITEM2             = @P047        ")
        sqlDetailStat.AppendLine("       , ADDITEM3             = @P048        ")
        sqlDetailStat.AppendLine("       , ADDITEM4             = @P049        ")
        sqlDetailStat.AppendLine("       , ADDITEM5             = @P050        ")
        sqlDetailStat.AppendLine("       , ADDITEM6             = @P051        ")
        sqlDetailStat.AppendLine("       , ADDITEM7             = @P052        ")
        sqlDetailStat.AppendLine("       , ADDITEM8             = @P053        ")
        sqlDetailStat.AppendLine("       , ADDITEM9             = @P054        ")
        sqlDetailStat.AppendLine("       , ADDITEM10            = @P055        ")
        sqlDetailStat.AppendLine("       , ADDITEM11            = @P056        ")
        sqlDetailStat.AppendLine("       , ADDITEM12            = @P057        ")
        sqlDetailStat.AppendLine("       , ADDITEM13            = @P058        ")
        sqlDetailStat.AppendLine("       , ADDITEM14            = @P059        ")
        sqlDetailStat.AppendLine("       , ADDITEM15            = @P060        ")
        sqlDetailStat.AppendLine("       , ADDITEM16            = @P061        ")
        sqlDetailStat.AppendLine("       , ADDITEM17            = @P062        ")
        sqlDetailStat.AppendLine("       , ADDITEM18            = @P063        ")
        sqlDetailStat.AppendLine("       , ADDITEM19            = @P064        ")
        sqlDetailStat.AppendLine("       , ADDITEM20            = @P065        ")
        sqlDetailStat.AppendLine("       , ADDITEM21            = @P066        ")
        sqlDetailStat.AppendLine("       , ADDITEM22            = @P067        ")
        sqlDetailStat.AppendLine("       , ADDITEM23            = @P068        ")
        sqlDetailStat.AppendLine("       , ADDITEM24            = @P069        ")
        sqlDetailStat.AppendLine("       , ADDITEM25            = @P070        ")
        sqlDetailStat.AppendLine("       , ADDITEM26            = @P071        ")
        sqlDetailStat.AppendLine("       , ADDITEM27            = @P072        ")
        sqlDetailStat.AppendLine("       , ADDITEM28            = @P073        ")
        sqlDetailStat.AppendLine("       , ADDITEM29            = @P074        ")
        sqlDetailStat.AppendLine("       , ADDITEM30            = @P075        ")
        sqlDetailStat.AppendLine("       , ADDITEM31            = @P076        ")
        sqlDetailStat.AppendLine("       , ADDITEM32            = @P077        ")
        sqlDetailStat.AppendLine("       , ADDITEM33            = @P078        ")
        sqlDetailStat.AppendLine("       , ADDITEM34            = @P079        ")
        sqlDetailStat.AppendLine("       , ADDITEM35            = @P080        ")
        sqlDetailStat.AppendLine("       , ADDITEM36            = @P081        ")
        sqlDetailStat.AppendLine("       , ADDITEM37            = @P082        ")
        sqlDetailStat.AppendLine("       , ADDITEM38            = @P083        ")
        sqlDetailStat.AppendLine("       , ADDITEM39            = @P084        ")
        sqlDetailStat.AppendLine("       , ADDITEM40            = @P085        ")
        sqlDetailStat.AppendLine("       , ADDITEM41            = @P086        ")
        sqlDetailStat.AppendLine("       , ADDITEM42            = @P087        ")
        sqlDetailStat.AppendLine("       , ADDITEM43            = @P088        ")
        sqlDetailStat.AppendLine("       , ADDITEM44            = @P089        ")
        sqlDetailStat.AppendLine("       , ADDITEM45            = @P090        ")
        sqlDetailStat.AppendLine("       , ADDITEM46            = @P091        ")
        sqlDetailStat.AppendLine("       , ADDITEM47            = @P092        ")
        sqlDetailStat.AppendLine("       , ADDITEM48            = @P093        ")
        sqlDetailStat.AppendLine("       , ADDITEM49            = @P094        ")
        sqlDetailStat.AppendLine("       , ADDITEM50            = @P095        ")
        sqlDetailStat.AppendLine("       , FLOORMATERIAL        = @P096        ")
        sqlDetailStat.AppendLine("       , UPDYMD               = @P102        ")
        sqlDetailStat.AppendLine("       , UPDUSER              = @P103        ")
        sqlDetailStat.AppendLine("       , UPDTERMID            = @P104        ")
        sqlDetailStat.AppendLine("       , UPDPGID              = @P105        ")
        sqlDetailStat.AppendLine("       , RECEIVEYMD           = @P106        ")
        sqlDetailStat.AppendLine("     WHERE                                   ")
        sqlDetailStat.AppendLine("             CTNTYPE = @P001                 ")
        sqlDetailStat.AppendLine("         AND CTNNO   = @P002 ;               ")
        sqlDetailStat.AppendLine(" IF (@@FETCH_STATUS <> 0)                    ")
        sqlDetailStat.AppendLine("     INSERT INTO LNG.LNM0002_RECONM          ")
        sqlDetailStat.AppendLine("        (DELFLG                              ")
        sqlDetailStat.AppendLine("       , CTNTYPE                             ")
        sqlDetailStat.AppendLine("       , CTNNO                               ")
        sqlDetailStat.AppendLine("       , JURISDICTIONCD                      ")
        sqlDetailStat.AppendLine("       , ACCOUNTINGASSETSCD                  ")
        sqlDetailStat.AppendLine("       , ACCOUNTINGASSETSKBN                 ")
        sqlDetailStat.AppendLine("       , DUMMYKBN                            ")
        sqlDetailStat.AppendLine("       , SPOTKBN                             ")
        sqlDetailStat.AppendLine("       , SPOTSTYMD                           ")
        sqlDetailStat.AppendLine("       , SPOTENDYMD                          ")
        sqlDetailStat.AppendLine("       , BIGCTNCD                            ")
        sqlDetailStat.AppendLine("       , MIDDLECTNCD                         ")
        sqlDetailStat.AppendLine("       , SMALLCTNCD                          ")
        sqlDetailStat.AppendLine("       , CONSTRUCTIONYM                      ")
        sqlDetailStat.AppendLine("       , CTNMAKER                            ")
        sqlDetailStat.AppendLine("       , FROZENMAKER                         ")
        sqlDetailStat.AppendLine("       , GROSSWEIGHT                         ")
        sqlDetailStat.AppendLine("       , CARGOWEIGHT                         ")
        sqlDetailStat.AppendLine("       , MYWEIGHT                            ")
        sqlDetailStat.AppendLine("       , BOOKVALUE                           ")
        sqlDetailStat.AppendLine("       , OUTHEIGHT                           ")
        sqlDetailStat.AppendLine("       , OUTWIDTH                            ")
        sqlDetailStat.AppendLine("       , OUTLENGTH                           ")
        sqlDetailStat.AppendLine("       , INHEIGHT                            ")
        sqlDetailStat.AppendLine("       , INWIDTH                             ")
        sqlDetailStat.AppendLine("       , INLENGTH                            ")
        sqlDetailStat.AppendLine("       , WIFEHEIGHT                          ")
        sqlDetailStat.AppendLine("       , WIFEWIDTH                           ")
        sqlDetailStat.AppendLine("       , SIDEHEIGHT                          ")
        sqlDetailStat.AppendLine("       , SIDEWIDTH                           ")
        sqlDetailStat.AppendLine("       , FLOORAREA                           ")
        sqlDetailStat.AppendLine("       , INVOLUMEMARKING                     ")
        sqlDetailStat.AppendLine("       , INVOLUMEACTUA                       ")
        sqlDetailStat.AppendLine("       , TRAINSCYCLEDAYS                     ")
        sqlDetailStat.AppendLine("       , TRAINSBEFORERUNYMD                  ")
        sqlDetailStat.AppendLine("       , TRAINSNEXTRUNYMD                    ")
        sqlDetailStat.AppendLine("       , REGINSCYCLEDAYS                     ")
        sqlDetailStat.AppendLine("       , REGINSCYCLEHOURMETER                ")
        sqlDetailStat.AppendLine("       , REGINSBEFORERUNYMD                  ")
        sqlDetailStat.AppendLine("       , REGINSNEXTRUNYMD                    ")
        sqlDetailStat.AppendLine("       , REGINSHOURMETERYMD                  ")
        sqlDetailStat.AppendLine("       , REGINSHOURMETERTIME                 ")
        sqlDetailStat.AppendLine("       , REGINSHOURMETERDSP                  ")
        sqlDetailStat.AppendLine("       , OPERATIONSTYMD                      ")
        sqlDetailStat.AppendLine("       , OPERATIONENDYMD                     ")
        sqlDetailStat.AppendLine("       , RETIRMENTYMD                        ")
        sqlDetailStat.AppendLine("       , COMPKANKBN                          ")
        sqlDetailStat.AppendLine("       , SUPPLYFLG                           ")
        sqlDetailStat.AppendLine("       , ADDITEM1                            ")
        sqlDetailStat.AppendLine("       , ADDITEM2                            ")
        sqlDetailStat.AppendLine("       , ADDITEM3                            ")
        sqlDetailStat.AppendLine("       , ADDITEM4                            ")
        sqlDetailStat.AppendLine("       , ADDITEM5                            ")
        sqlDetailStat.AppendLine("       , ADDITEM6                            ")
        sqlDetailStat.AppendLine("       , ADDITEM7                            ")
        sqlDetailStat.AppendLine("       , ADDITEM8                            ")
        sqlDetailStat.AppendLine("       , ADDITEM9                            ")
        sqlDetailStat.AppendLine("       , ADDITEM10                           ")
        sqlDetailStat.AppendLine("       , ADDITEM11                           ")
        sqlDetailStat.AppendLine("       , ADDITEM12                           ")
        sqlDetailStat.AppendLine("       , ADDITEM13                           ")
        sqlDetailStat.AppendLine("       , ADDITEM14                           ")
        sqlDetailStat.AppendLine("       , ADDITEM15                           ")
        sqlDetailStat.AppendLine("       , ADDITEM16                           ")
        sqlDetailStat.AppendLine("       , ADDITEM17                           ")
        sqlDetailStat.AppendLine("       , ADDITEM18                           ")
        sqlDetailStat.AppendLine("       , ADDITEM19                           ")
        sqlDetailStat.AppendLine("       , ADDITEM20                           ")
        sqlDetailStat.AppendLine("       , ADDITEM21                           ")
        sqlDetailStat.AppendLine("       , ADDITEM22                           ")
        sqlDetailStat.AppendLine("       , ADDITEM23                           ")
        sqlDetailStat.AppendLine("       , ADDITEM24                           ")
        sqlDetailStat.AppendLine("       , ADDITEM25                           ")
        sqlDetailStat.AppendLine("       , ADDITEM26                           ")
        sqlDetailStat.AppendLine("       , ADDITEM27                           ")
        sqlDetailStat.AppendLine("       , ADDITEM28                           ")
        sqlDetailStat.AppendLine("       , ADDITEM29                           ")
        sqlDetailStat.AppendLine("       , ADDITEM30                           ")
        sqlDetailStat.AppendLine("       , ADDITEM31                           ")
        sqlDetailStat.AppendLine("       , ADDITEM32                           ")
        sqlDetailStat.AppendLine("       , ADDITEM33                           ")
        sqlDetailStat.AppendLine("       , ADDITEM34                           ")
        sqlDetailStat.AppendLine("       , ADDITEM35                           ")
        sqlDetailStat.AppendLine("       , ADDITEM36                           ")
        sqlDetailStat.AppendLine("       , ADDITEM37                           ")
        sqlDetailStat.AppendLine("       , ADDITEM38                           ")
        sqlDetailStat.AppendLine("       , ADDITEM39                           ")
        sqlDetailStat.AppendLine("       , ADDITEM40                           ")
        sqlDetailStat.AppendLine("       , ADDITEM41                           ")
        sqlDetailStat.AppendLine("       , ADDITEM42                           ")
        sqlDetailStat.AppendLine("       , ADDITEM43                           ")
        sqlDetailStat.AppendLine("       , ADDITEM44                           ")
        sqlDetailStat.AppendLine("       , ADDITEM45                           ")
        sqlDetailStat.AppendLine("       , ADDITEM46                           ")
        sqlDetailStat.AppendLine("       , ADDITEM47                           ")
        sqlDetailStat.AppendLine("       , ADDITEM48                           ")
        sqlDetailStat.AppendLine("       , ADDITEM49                           ")
        sqlDetailStat.AppendLine("       , ADDITEM50                           ")
        sqlDetailStat.AppendLine("       , FLOORMATERIAL                       ")
        sqlDetailStat.AppendLine("       , INITYMD                             ")
        sqlDetailStat.AppendLine("       , INITUSER                            ")
        sqlDetailStat.AppendLine("       , INITTERMID                          ")
        sqlDetailStat.AppendLine("       , INITPGID                            ")
        sqlDetailStat.AppendLine("       )                                     ")
        sqlDetailStat.AppendLine("     VALUES                                  ")
        sqlDetailStat.AppendLine("        (@P000                               ")
        sqlDetailStat.AppendLine("       , @P001                               ")
        sqlDetailStat.AppendLine("       , @P002                               ")
        sqlDetailStat.AppendLine("       , @P003                               ")
        sqlDetailStat.AppendLine("       , @P004                               ")
        sqlDetailStat.AppendLine("       , @P005                               ")
        sqlDetailStat.AppendLine("       , @P006                               ")
        sqlDetailStat.AppendLine("       , @P007                               ")
        sqlDetailStat.AppendLine("       , @P008                               ")
        sqlDetailStat.AppendLine("       , @P009                               ")
        sqlDetailStat.AppendLine("       , @P010                               ")
        sqlDetailStat.AppendLine("       , @P011                               ")
        sqlDetailStat.AppendLine("       , @P012                               ")
        sqlDetailStat.AppendLine("       , @P013                               ")
        sqlDetailStat.AppendLine("       , @P014                               ")
        sqlDetailStat.AppendLine("       , @P015                               ")
        sqlDetailStat.AppendLine("       , @P016                               ")
        sqlDetailStat.AppendLine("       , @P017                               ")
        sqlDetailStat.AppendLine("       , @P018                               ")
        sqlDetailStat.AppendLine("       , @P107                               ")
        sqlDetailStat.AppendLine("       , @P019                               ")
        sqlDetailStat.AppendLine("       , @P020                               ")
        sqlDetailStat.AppendLine("       , @P021                               ")
        sqlDetailStat.AppendLine("       , @P022                               ")
        sqlDetailStat.AppendLine("       , @P023                               ")
        sqlDetailStat.AppendLine("       , @P024                               ")
        sqlDetailStat.AppendLine("       , @P025                               ")
        sqlDetailStat.AppendLine("       , @P026                               ")
        sqlDetailStat.AppendLine("       , @P027                               ")
        sqlDetailStat.AppendLine("       , @P028                               ")
        sqlDetailStat.AppendLine("       , @P029                               ")
        sqlDetailStat.AppendLine("       , @P030                               ")
        sqlDetailStat.AppendLine("       , @P031                               ")
        sqlDetailStat.AppendLine("       , @P032                               ")
        sqlDetailStat.AppendLine("       , @P033                               ")
        sqlDetailStat.AppendLine("       , @P034                               ")
        sqlDetailStat.AppendLine("       , @P035                               ")
        sqlDetailStat.AppendLine("       , @P036                               ")
        sqlDetailStat.AppendLine("       , @P037                               ")
        sqlDetailStat.AppendLine("       , @P038                               ")
        sqlDetailStat.AppendLine("       , @P039                               ")
        sqlDetailStat.AppendLine("       , @P040                               ")
        sqlDetailStat.AppendLine("       , @P041                               ")
        sqlDetailStat.AppendLine("       , @P042                               ")
        sqlDetailStat.AppendLine("       , @P043                               ")
        sqlDetailStat.AppendLine("       , @P044                               ")
        sqlDetailStat.AppendLine("       , @P045                               ")
        sqlDetailStat.AppendLine("       , @P108                               ")
        sqlDetailStat.AppendLine("       , @P046                               ")
        sqlDetailStat.AppendLine("       , @P047                               ")
        sqlDetailStat.AppendLine("       , @P048                               ")
        sqlDetailStat.AppendLine("       , @P049                               ")
        sqlDetailStat.AppendLine("       , @P050                               ")
        sqlDetailStat.AppendLine("       , @P051                               ")
        sqlDetailStat.AppendLine("       , @P052                               ")
        sqlDetailStat.AppendLine("       , @P053                               ")
        sqlDetailStat.AppendLine("       , @P054                               ")
        sqlDetailStat.AppendLine("       , @P055                               ")
        sqlDetailStat.AppendLine("       , @P056                               ")
        sqlDetailStat.AppendLine("       , @P057                               ")
        sqlDetailStat.AppendLine("       , @P058                               ")
        sqlDetailStat.AppendLine("       , @P059                               ")
        sqlDetailStat.AppendLine("       , @P060                               ")
        sqlDetailStat.AppendLine("       , @P061                               ")
        sqlDetailStat.AppendLine("       , @P062                               ")
        sqlDetailStat.AppendLine("       , @P063                               ")
        sqlDetailStat.AppendLine("       , @P064                               ")
        sqlDetailStat.AppendLine("       , @P065                               ")
        sqlDetailStat.AppendLine("       , @P066                               ")
        sqlDetailStat.AppendLine("       , @P067                               ")
        sqlDetailStat.AppendLine("       , @P068                               ")
        sqlDetailStat.AppendLine("       , @P069                               ")
        sqlDetailStat.AppendLine("       , @P070                               ")
        sqlDetailStat.AppendLine("       , @P071                               ")
        sqlDetailStat.AppendLine("       , @P072                               ")
        sqlDetailStat.AppendLine("       , @P073                               ")
        sqlDetailStat.AppendLine("       , @P074                               ")
        sqlDetailStat.AppendLine("       , @P075                               ")
        sqlDetailStat.AppendLine("       , @P076                               ")
        sqlDetailStat.AppendLine("       , @P077                               ")
        sqlDetailStat.AppendLine("       , @P078                               ")
        sqlDetailStat.AppendLine("       , @P079                               ")
        sqlDetailStat.AppendLine("       , @P080                               ")
        sqlDetailStat.AppendLine("       , @P081                               ")
        sqlDetailStat.AppendLine("       , @P082                               ")
        sqlDetailStat.AppendLine("       , @P083                               ")
        sqlDetailStat.AppendLine("       , @P084                               ")
        sqlDetailStat.AppendLine("       , @P085                               ")
        sqlDetailStat.AppendLine("       , @P086                               ")
        sqlDetailStat.AppendLine("       , @P087                               ")
        sqlDetailStat.AppendLine("       , @P088                               ")
        sqlDetailStat.AppendLine("       , @P089                               ")
        sqlDetailStat.AppendLine("       , @P090                               ")
        sqlDetailStat.AppendLine("       , @P091                               ")
        sqlDetailStat.AppendLine("       , @P092                               ")
        sqlDetailStat.AppendLine("       , @P093                               ")
        sqlDetailStat.AppendLine("       , @P094                               ")
        sqlDetailStat.AppendLine("       , @P095                               ")
        sqlDetailStat.AppendLine("       , @P096                               ")
        sqlDetailStat.AppendLine("       , @P098                               ")
        sqlDetailStat.AppendLine("       , @P099                               ")
        sqlDetailStat.AppendLine("       , @P100                               ")
        sqlDetailStat.AppendLine("       , @P101                               ")
        sqlDetailStat.AppendLine("       ) ;                                   ")
        sqlDetailStat.AppendLine(" CLOSE hensuu ;                              ")
        sqlDetailStat.AppendLine(" DEALLOCATE hensuu ;                         ")

        '○ 更新ジャーナル出力SQL
        Dim SQLJnl As New StringBuilder
        SQLJnl.AppendLine(" Select                                      ")
        SQLJnl.AppendLine("     DELFLG                                 ")
        SQLJnl.AppendLine("   , CTNTYPE                                ")
        SQLJnl.AppendLine("   , CTNNO                                  ")
        SQLJnl.AppendLine("   , JURISDICTIONCD                         ")
        SQLJnl.AppendLine("   , ACCOUNTINGASSETSCD                     ")
        SQLJnl.AppendLine("   , ACCOUNTINGASSETSKBN                    ")
        SQLJnl.AppendLine("   , DUMMYKBN                               ")
        SQLJnl.AppendLine("   , SPOTKBN                                ")
        SQLJnl.AppendLine("   , SPOTSTYMD                              ")
        SQLJnl.AppendLine("   , SPOTENDYMD                             ")
        SQLJnl.AppendLine("   , BIGCTNCD                               ")
        SQLJnl.AppendLine("   , MIDDLECTNCD                            ")
        SQLJnl.AppendLine("   , SMALLCTNCD                             ")
        SQLJnl.AppendLine("   , CONSTRUCTIONYM                         ")
        SQLJnl.AppendLine("   , CTNMAKER                               ")
        SQLJnl.AppendLine("   , FROZENMAKER                            ")
        SQLJnl.AppendLine("   , GROSSWEIGHT                            ")
        SQLJnl.AppendLine("   , CARGOWEIGHT                            ")
        SQLJnl.AppendLine("   , MYWEIGHT                               ")
        SQLJnl.AppendLine("   , BOOKVALUE                              ")
        SQLJnl.AppendLine("   , OUTHEIGHT                              ")
        SQLJnl.AppendLine("   , OUTWIDTH                               ")
        SQLJnl.AppendLine("   , OUTLENGTH                              ")
        SQLJnl.AppendLine("   , INHEIGHT                               ")
        SQLJnl.AppendLine("   , INWIDTH                                ")
        SQLJnl.AppendLine("   , INLENGTH                               ")
        SQLJnl.AppendLine("   , WIFEHEIGHT                             ")
        SQLJnl.AppendLine("   , WIFEWIDTH                              ")
        SQLJnl.AppendLine("   , SIDEHEIGHT                             ")
        SQLJnl.AppendLine("   , SIDEWIDTH                              ")
        SQLJnl.AppendLine("   , FLOORAREA                              ")
        SQLJnl.AppendLine("   , INVOLUMEMARKING                        ")
        SQLJnl.AppendLine("   , INVOLUMEACTUA                          ")
        SQLJnl.AppendLine("   , TRAINSCYCLEDAYS                        ")
        SQLJnl.AppendLine("   , TRAINSBEFORERUNYMD                     ")
        SQLJnl.AppendLine("   , TRAINSNEXTRUNYMD                       ")
        SQLJnl.AppendLine("   , REGINSCYCLEDAYS                        ")
        SQLJnl.AppendLine("   , REGINSCYCLEHOURMETER                   ")
        SQLJnl.AppendLine("   , REGINSBEFORERUNYMD                     ")
        SQLJnl.AppendLine("   , REGINSNEXTRUNYMD                       ")
        SQLJnl.AppendLine("   , REGINSHOURMETERYMD                     ")
        SQLJnl.AppendLine("   , REGINSHOURMETERTIME                    ")
        SQLJnl.AppendLine("   , REGINSHOURMETERDSP                     ")
        SQLJnl.AppendLine("   , OPERATIONSTYMD                         ")
        SQLJnl.AppendLine("   , OPERATIONENDYMD                        ")
        SQLJnl.AppendLine("   , RETIRMENTYMD                           ")
        SQLJnl.AppendLine("   , COMPKANKBN                             ")
        SQLJnl.AppendLine("   , SUPPLYFLG                              ")
        SQLJnl.AppendLine("   , ADDITEM1                               ")
        SQLJnl.AppendLine("   , ADDITEM2                               ")
        SQLJnl.AppendLine("   , ADDITEM3                               ")
        SQLJnl.AppendLine("   , ADDITEM4                               ")
        SQLJnl.AppendLine("   , ADDITEM5                               ")
        SQLJnl.AppendLine("   , ADDITEM6                               ")
        SQLJnl.AppendLine("   , ADDITEM7                               ")
        SQLJnl.AppendLine("   , ADDITEM8                               ")
        SQLJnl.AppendLine("   , ADDITEM9                               ")
        SQLJnl.AppendLine("   , ADDITEM10                              ")
        SQLJnl.AppendLine("   , ADDITEM11                              ")
        SQLJnl.AppendLine("   , ADDITEM12                              ")
        SQLJnl.AppendLine("   , ADDITEM13                              ")
        SQLJnl.AppendLine("   , ADDITEM14                              ")
        SQLJnl.AppendLine("   , ADDITEM15                              ")
        SQLJnl.AppendLine("   , ADDITEM16                              ")
        SQLJnl.AppendLine("   , ADDITEM17                              ")
        SQLJnl.AppendLine("   , ADDITEM18                              ")
        SQLJnl.AppendLine("   , ADDITEM19                              ")
        SQLJnl.AppendLine("   , ADDITEM20                              ")
        SQLJnl.AppendLine("   , ADDITEM21                              ")
        SQLJnl.AppendLine("   , ADDITEM22                              ")
        SQLJnl.AppendLine("   , ADDITEM23                              ")
        SQLJnl.AppendLine("   , ADDITEM24                              ")
        SQLJnl.AppendLine("   , ADDITEM25                              ")
        SQLJnl.AppendLine("   , ADDITEM26                              ")
        SQLJnl.AppendLine("   , ADDITEM27                              ")
        SQLJnl.AppendLine("   , ADDITEM28                              ")
        SQLJnl.AppendLine("   , ADDITEM29                              ")
        SQLJnl.AppendLine("   , ADDITEM30                              ")
        SQLJnl.AppendLine("   , ADDITEM31                              ")
        SQLJnl.AppendLine("   , ADDITEM32                              ")
        SQLJnl.AppendLine("   , ADDITEM33                              ")
        SQLJnl.AppendLine("   , ADDITEM34                              ")
        SQLJnl.AppendLine("   , ADDITEM35                              ")
        SQLJnl.AppendLine("   , ADDITEM36                              ")
        SQLJnl.AppendLine("   , ADDITEM37                              ")
        SQLJnl.AppendLine("   , ADDITEM38                              ")
        SQLJnl.AppendLine("   , ADDITEM39                              ")
        SQLJnl.AppendLine("   , ADDITEM40                              ")
        SQLJnl.AppendLine("   , ADDITEM41                              ")
        SQLJnl.AppendLine("   , ADDITEM42                              ")
        SQLJnl.AppendLine("   , ADDITEM43                              ")
        SQLJnl.AppendLine("   , ADDITEM44                              ")
        SQLJnl.AppendLine("   , ADDITEM45                              ")
        SQLJnl.AppendLine("   , ADDITEM46                              ")
        SQLJnl.AppendLine("   , ADDITEM47                              ")
        SQLJnl.AppendLine("   , ADDITEM48                              ")
        SQLJnl.AppendLine("   , ADDITEM49                              ")
        SQLJnl.AppendLine("   , ADDITEM50                              ")
        SQLJnl.AppendLine("   , FLOORMATERIAL                          ")
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
        SQLJnl.AppendLine("     LNG.LNM0002_RECONM                     ")
        SQLJnl.AppendLine(" WHERE                                      ")
        SQLJnl.AppendLine("         CTNTYPE = @P001                    ")
        SQLJnl.AppendLine("     AND CTNNO   = @P002                    ")

        Try
            Using SQLcmd As New MySqlCommand(sqlDetailStat.ToString, SQLcon), SQLcmdJnl As New MySqlCommand(SQLJnl.ToString, SQLcon)
                ' DB更新用パラメータ
                Dim PARA000 As MySqlParameter = SQLcmd.Parameters.Add("@P000", MySqlDbType.VarChar, 1)         '削除フラグ
                Dim PARA001 As MySqlParameter = SQLcmd.Parameters.Add("@P001", MySqlDbType.VarChar, 5)         'コンテナ記号
                Dim PARA002 As MySqlParameter = SQLcmd.Parameters.Add("@P002", MySqlDbType.VarChar, 8)         'コンテナ番号
                Dim PARA003 As MySqlParameter = SQLcmd.Parameters.Add("@P003", MySqlDbType.VarChar, 2)         '所管部コード
                Dim PARA004 As MySqlParameter = SQLcmd.Parameters.Add("@P004", MySqlDbType.VarChar, 4)         '経理資産コード
                Dim PARA005 As MySqlParameter = SQLcmd.Parameters.Add("@P005", MySqlDbType.VarChar, 2)         '経理資産区分
                Dim PARA006 As MySqlParameter = SQLcmd.Parameters.Add("@P006", MySqlDbType.VarChar, 2)         'ダミー区分
                Dim PARA007 As MySqlParameter = SQLcmd.Parameters.Add("@P007", MySqlDbType.VarChar, 2)         'スポット区分
                Dim PARA008 As MySqlParameter = SQLcmd.Parameters.Add("@P008", MySqlDbType.Date)                'スポット区分　開始年月日
                Dim PARA009 As MySqlParameter = SQLcmd.Parameters.Add("@P009", MySqlDbType.Date)                'スポット区分　終了年月日
                Dim PARA010 As MySqlParameter = SQLcmd.Parameters.Add("@P010", MySqlDbType.VarChar, 2)         '大分類コード
                Dim PARA011 As MySqlParameter = SQLcmd.Parameters.Add("@P011", MySqlDbType.VarChar, 2)         '中分類コード
                Dim PARA012 As MySqlParameter = SQLcmd.Parameters.Add("@P012", MySqlDbType.VarChar, 2)         '小分類コード
                Dim PARA013 As MySqlParameter = SQLcmd.Parameters.Add("@P013", MySqlDbType.VarChar, 6)         '建造年月
                Dim PARA014 As MySqlParameter = SQLcmd.Parameters.Add("@P014", MySqlDbType.VarChar, 3)         'コンテナメーカー
                Dim PARA015 As MySqlParameter = SQLcmd.Parameters.Add("@P015", MySqlDbType.VarChar, 3)         '冷凍機メーカー
                Dim PARA016 As MySqlParameter = SQLcmd.Parameters.Add("@P016", MySqlDbType.Decimal, 4, 1)       '総重量
                Dim PARA017 As MySqlParameter = SQLcmd.Parameters.Add("@P017", MySqlDbType.Decimal, 6, 1)       '荷重
                Dim PARA018 As MySqlParameter = SQLcmd.Parameters.Add("@P018", MySqlDbType.Decimal, 4, 1)       '自重
                Dim PARA107 As MySqlParameter = SQLcmd.Parameters.Add("@P107", MySqlDbType.Decimal)               '簿価商品価格
                Dim PARA019 As MySqlParameter = SQLcmd.Parameters.Add("@P019", MySqlDbType.VarChar, 4)         '外寸・高さ
                Dim PARA020 As MySqlParameter = SQLcmd.Parameters.Add("@P020", MySqlDbType.VarChar, 4)         '外寸・幅
                Dim PARA021 As MySqlParameter = SQLcmd.Parameters.Add("@P021", MySqlDbType.VarChar, 4)         '外寸・長さ
                Dim PARA022 As MySqlParameter = SQLcmd.Parameters.Add("@P022", MySqlDbType.VarChar, 4)         '内寸・高さ
                Dim PARA023 As MySqlParameter = SQLcmd.Parameters.Add("@P023", MySqlDbType.VarChar, 4)         '内寸・幅
                Dim PARA024 As MySqlParameter = SQLcmd.Parameters.Add("@P024", MySqlDbType.VarChar, 4)         '内寸・長さ
                Dim PARA025 As MySqlParameter = SQLcmd.Parameters.Add("@P025", MySqlDbType.VarChar, 4)         '妻入口・高さ
                Dim PARA026 As MySqlParameter = SQLcmd.Parameters.Add("@P026", MySqlDbType.VarChar, 4)         '妻入口・幅
                Dim PARA027 As MySqlParameter = SQLcmd.Parameters.Add("@P027", MySqlDbType.VarChar, 4)         '側入口・高さ
                Dim PARA028 As MySqlParameter = SQLcmd.Parameters.Add("@P028", MySqlDbType.VarChar, 4)         '側入口・幅
                Dim PARA029 As MySqlParameter = SQLcmd.Parameters.Add("@P029", MySqlDbType.Decimal, 6, 2)       '床面積
                Dim PARA030 As MySqlParameter = SQLcmd.Parameters.Add("@P030", MySqlDbType.VarChar, 4)         '内容積・標記
                Dim PARA031 As MySqlParameter = SQLcmd.Parameters.Add("@P031", MySqlDbType.Decimal, 6, 2)       '内容積・実寸
                Dim PARA032 As MySqlParameter = SQLcmd.Parameters.Add("@P032", MySqlDbType.VarChar, 3)         '交番検査・ｻｲｸﾙ日数
                Dim PARA033 As MySqlParameter = SQLcmd.Parameters.Add("@P033", MySqlDbType.Date)                '交番検査・前回実施日
                Dim PARA034 As MySqlParameter = SQLcmd.Parameters.Add("@P034", MySqlDbType.Date)                '交番検査・次回実施日
                Dim PARA035 As MySqlParameter = SQLcmd.Parameters.Add("@P035", MySqlDbType.VarChar, 2)         '定期検査・ｻｲｸﾙ月数
                Dim PARA036 As MySqlParameter = SQLcmd.Parameters.Add("@P036", MySqlDbType.VarChar, 3)         '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
                Dim PARA037 As MySqlParameter = SQLcmd.Parameters.Add("@P037", MySqlDbType.Date)                '定期検査・前回実施日
                Dim PARA038 As MySqlParameter = SQLcmd.Parameters.Add("@P038", MySqlDbType.Date)                '定期検査・次回実施日
                Dim PARA039 As MySqlParameter = SQLcmd.Parameters.Add("@P039", MySqlDbType.Date)                '定期検査・ｱﾜﾒｰﾀ記載日
                Dim PARA040 As MySqlParameter = SQLcmd.Parameters.Add("@P040", MySqlDbType.VarChar, 5)         '定期検査・ｱﾜﾒｰﾀ時間
                Dim PARA041 As MySqlParameter = SQLcmd.Parameters.Add("@P041", MySqlDbType.VarChar, 1)         '定期検査・ｱﾜﾒｰﾀ表示桁
                Dim PARA042 As MySqlParameter = SQLcmd.Parameters.Add("@P042", MySqlDbType.Date)                '運用開始年月日
                Dim PARA043 As MySqlParameter = SQLcmd.Parameters.Add("@P043", MySqlDbType.Date)                '運用除外年月日
                Dim PARA044 As MySqlParameter = SQLcmd.Parameters.Add("@P044", MySqlDbType.Date)                '除却年月日
                Dim PARA045 As MySqlParameter = SQLcmd.Parameters.Add("@P045", MySqlDbType.VarChar, 2)         '複合一貫区分
                Dim PARA108 As MySqlParameter = SQLcmd.Parameters.Add("@P108", MySqlDbType.VarChar, 1)         '調達フラグ
                Dim PARA046 As MySqlParameter = SQLcmd.Parameters.Add("@P046", MySqlDbType.VarChar, 4)         '付帯項目１
                Dim PARA047 As MySqlParameter = SQLcmd.Parameters.Add("@P047", MySqlDbType.VarChar, 4)         '付帯項目２
                Dim PARA048 As MySqlParameter = SQLcmd.Parameters.Add("@P048", MySqlDbType.VarChar, 4)         '付帯項目３
                Dim PARA049 As MySqlParameter = SQLcmd.Parameters.Add("@P049", MySqlDbType.VarChar, 4)         '付帯項目４
                Dim PARA050 As MySqlParameter = SQLcmd.Parameters.Add("@P050", MySqlDbType.VarChar, 4)         '付帯項目５
                Dim PARA051 As MySqlParameter = SQLcmd.Parameters.Add("@P051", MySqlDbType.VarChar, 4)         '付帯項目６
                Dim PARA052 As MySqlParameter = SQLcmd.Parameters.Add("@P052", MySqlDbType.VarChar, 4)         '付帯項目７
                Dim PARA053 As MySqlParameter = SQLcmd.Parameters.Add("@P053", MySqlDbType.VarChar, 4)         '付帯項目８
                Dim PARA054 As MySqlParameter = SQLcmd.Parameters.Add("@P054", MySqlDbType.VarChar, 4)         '付帯項目９
                Dim PARA055 As MySqlParameter = SQLcmd.Parameters.Add("@P055", MySqlDbType.VarChar, 4)         '付帯項目１０
                Dim PARA056 As MySqlParameter = SQLcmd.Parameters.Add("@P056", MySqlDbType.VarChar, 4)         '付帯項目１１
                Dim PARA057 As MySqlParameter = SQLcmd.Parameters.Add("@P057", MySqlDbType.VarChar, 4)         '付帯項目１２
                Dim PARA058 As MySqlParameter = SQLcmd.Parameters.Add("@P058", MySqlDbType.VarChar, 4)         '付帯項目１３
                Dim PARA059 As MySqlParameter = SQLcmd.Parameters.Add("@P059", MySqlDbType.VarChar, 4)         '付帯項目１４
                Dim PARA060 As MySqlParameter = SQLcmd.Parameters.Add("@P060", MySqlDbType.VarChar, 4)         '付帯項目１５
                Dim PARA061 As MySqlParameter = SQLcmd.Parameters.Add("@P061", MySqlDbType.VarChar, 4)         '付帯項目１６
                Dim PARA062 As MySqlParameter = SQLcmd.Parameters.Add("@P062", MySqlDbType.VarChar, 4)         '付帯項目１７
                Dim PARA063 As MySqlParameter = SQLcmd.Parameters.Add("@P063", MySqlDbType.VarChar, 4)         '付帯項目１８
                Dim PARA064 As MySqlParameter = SQLcmd.Parameters.Add("@P064", MySqlDbType.VarChar, 4)         '付帯項目１９
                Dim PARA065 As MySqlParameter = SQLcmd.Parameters.Add("@P065", MySqlDbType.VarChar, 4)         '付帯項目２０
                Dim PARA066 As MySqlParameter = SQLcmd.Parameters.Add("@P066", MySqlDbType.VarChar, 4)         '付帯項目２１
                Dim PARA067 As MySqlParameter = SQLcmd.Parameters.Add("@P067", MySqlDbType.VarChar, 4)         '付帯項目２２
                Dim PARA068 As MySqlParameter = SQLcmd.Parameters.Add("@P068", MySqlDbType.VarChar, 4)         '付帯項目２３
                Dim PARA069 As MySqlParameter = SQLcmd.Parameters.Add("@P069", MySqlDbType.VarChar, 4)         '付帯項目２４
                Dim PARA070 As MySqlParameter = SQLcmd.Parameters.Add("@P070", MySqlDbType.VarChar, 4)         '付帯項目２５
                Dim PARA071 As MySqlParameter = SQLcmd.Parameters.Add("@P071", MySqlDbType.VarChar, 4)         '付帯項目２６
                Dim PARA072 As MySqlParameter = SQLcmd.Parameters.Add("@P072", MySqlDbType.VarChar, 4)         '付帯項目２７
                Dim PARA073 As MySqlParameter = SQLcmd.Parameters.Add("@P073", MySqlDbType.VarChar, 4)         '付帯項目２８
                Dim PARA074 As MySqlParameter = SQLcmd.Parameters.Add("@P074", MySqlDbType.VarChar, 4)         '付帯項目２９
                Dim PARA075 As MySqlParameter = SQLcmd.Parameters.Add("@P075", MySqlDbType.VarChar, 4)         '付帯項目３０
                Dim PARA076 As MySqlParameter = SQLcmd.Parameters.Add("@P076", MySqlDbType.VarChar, 4)         '付帯項目３１
                Dim PARA077 As MySqlParameter = SQLcmd.Parameters.Add("@P077", MySqlDbType.VarChar, 4)         '付帯項目３２
                Dim PARA078 As MySqlParameter = SQLcmd.Parameters.Add("@P078", MySqlDbType.VarChar, 4)         '付帯項目３３
                Dim PARA079 As MySqlParameter = SQLcmd.Parameters.Add("@P079", MySqlDbType.VarChar, 4)         '付帯項目３４
                Dim PARA080 As MySqlParameter = SQLcmd.Parameters.Add("@P080", MySqlDbType.VarChar, 4)         '付帯項目３５
                Dim PARA081 As MySqlParameter = SQLcmd.Parameters.Add("@P081", MySqlDbType.VarChar, 4)         '付帯項目３６
                Dim PARA082 As MySqlParameter = SQLcmd.Parameters.Add("@P082", MySqlDbType.VarChar, 4)         '付帯項目３７
                Dim PARA083 As MySqlParameter = SQLcmd.Parameters.Add("@P083", MySqlDbType.VarChar, 4)         '付帯項目３８
                Dim PARA084 As MySqlParameter = SQLcmd.Parameters.Add("@P084", MySqlDbType.VarChar, 4)         '付帯項目３９
                Dim PARA085 As MySqlParameter = SQLcmd.Parameters.Add("@P085", MySqlDbType.VarChar, 4)         '付帯項目４０
                Dim PARA086 As MySqlParameter = SQLcmd.Parameters.Add("@P086", MySqlDbType.VarChar, 4)         '付帯項目４１
                Dim PARA087 As MySqlParameter = SQLcmd.Parameters.Add("@P087", MySqlDbType.VarChar, 4)         '付帯項目４２
                Dim PARA088 As MySqlParameter = SQLcmd.Parameters.Add("@P088", MySqlDbType.VarChar, 4)         '付帯項目４３
                Dim PARA089 As MySqlParameter = SQLcmd.Parameters.Add("@P089", MySqlDbType.VarChar, 4)         '付帯項目４４
                Dim PARA090 As MySqlParameter = SQLcmd.Parameters.Add("@P090", MySqlDbType.VarChar, 4)         '付帯項目４５
                Dim PARA091 As MySqlParameter = SQLcmd.Parameters.Add("@P091", MySqlDbType.VarChar, 4)         '付帯項目４６
                Dim PARA092 As MySqlParameter = SQLcmd.Parameters.Add("@P092", MySqlDbType.VarChar, 4)         '付帯項目４７
                Dim PARA093 As MySqlParameter = SQLcmd.Parameters.Add("@P093", MySqlDbType.VarChar, 4)         '付帯項目４８
                Dim PARA094 As MySqlParameter = SQLcmd.Parameters.Add("@P094", MySqlDbType.VarChar, 4)         '付帯項目４９
                Dim PARA095 As MySqlParameter = SQLcmd.Parameters.Add("@P095", MySqlDbType.VarChar, 4)         '付帯項目５０
                Dim PARA096 As MySqlParameter = SQLcmd.Parameters.Add("@P096", MySqlDbType.VarChar, 1)         '床材質コード
                Dim PARA098 As MySqlParameter = SQLcmd.Parameters.Add("@P098", MySqlDbType.DateTime)            '登録年月日
                Dim PARA099 As MySqlParameter = SQLcmd.Parameters.Add("@P099", MySqlDbType.VarChar, 20)        '登録ユーザーＩＤ
                Dim PARA100 As MySqlParameter = SQLcmd.Parameters.Add("@P100", MySqlDbType.VarChar, 20)        '登録端末
                Dim PARA101 As MySqlParameter = SQLcmd.Parameters.Add("@P101", MySqlDbType.VarChar, 40)        '登録プログラムＩＤ
                Dim PARA102 As MySqlParameter = SQLcmd.Parameters.Add("@P102", MySqlDbType.DateTime)            '更新年月日
                Dim PARA103 As MySqlParameter = SQLcmd.Parameters.Add("@P103", MySqlDbType.VarChar, 20)        '更新ユーザーＩＤ
                Dim PARA104 As MySqlParameter = SQLcmd.Parameters.Add("@P104", MySqlDbType.VarChar, 20)        '更新端末
                Dim PARA105 As MySqlParameter = SQLcmd.Parameters.Add("@P105", MySqlDbType.VarChar, 40)        '更新プログラムＩＤ
                Dim PARA106 As MySqlParameter = SQLcmd.Parameters.Add("@P106", MySqlDbType.DateTime)            '集信日時

                ' 更新ジャーナル出力用パラメータ
                Dim JPARA001 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P001", MySqlDbType.VarChar, 5)     'コンテナ記号
                Dim JPARA002 As MySqlParameter = SQLcmdJnl.Parameters.Add("@P002", MySqlDbType.VarChar, 8)     'コンテナ番号

                Dim LNM0002row As DataRow = LNM0002INPtbl.Rows(0)

                'Dim WW_DateNow As DateTime = Date.Now

                ' DB更新
                PARA000.Value = LNM0002row("DELFLG")                                   '削除フラグ
                PARA001.Value = LNM0002row("CTNTYPE")                                  'コンテナ記号
                PARA002.Value = LNM0002row("CTNNO")                                    'コンテナ番号
                PARA003.Value = LNM0002row("JURISDICTIONCD")                           '所管部コード
                PARA004.Value = LNM0002row("ACCOUNTINGASSETSCD")                       '経理資産コード
                PARA005.Value = LNM0002row("ACCOUNTINGASSETSKBN")                      '経理資産区分
                PARA006.Value = LNM0002row("DUMMYKBN")                                 'ダミー区分
                PARA007.Value = LNM0002row("SPOTKBN")                                  'スポット区分
                If String.IsNullOrEmpty(RTrim(LNM0002row("SPOTSTYMD"))) Then           'スポット区分　開始年月日
                    PARA008.Value = DBNull.Value
                Else
                    PARA008.Value = RTrim(LNM0002row("SPOTSTYMD"))
                End If
                If String.IsNullOrEmpty(RTrim(LNM0002row("SPOTENDYMD"))) Then          'スポット区分　終了年月日
                    PARA009.Value = DBNull.Value
                Else
                    PARA009.Value = RTrim(LNM0002row("SPOTENDYMD"))
                End If
                PARA010.Value = LNM0002row("BIGCTNCD")                                 '大分類コード
                PARA011.Value = LNM0002row("MIDDLECTNCD")                              '中分類コード
                PARA012.Value = LNM0002row("SMALLCTNCD")                               '小分類コード
                PARA013.Value = LNM0002row("CONSTRUCTIONYM")                           '建造年月
                PARA014.Value = LNM0002row("CTNMAKER")                                 'コンテナメーカー
                PARA015.Value = LNM0002row("FROZENMAKER")                              '冷凍機メーカー
                PARA016.Value = LNM0002row("GROSSWEIGHT")                              '総重量
                PARA017.Value = LNM0002row("CARGOWEIGHT")                              '荷重
                PARA018.Value = LNM0002row("MYWEIGHT")                                 '自重
                PARA107.Value = LNM0002row("BOOKVALUE")                                '簿価商品価格
                If String.IsNullOrEmpty(LNM0002row("OUTHEIGHT")) Then                  '外寸・高さ
                    PARA019.Value = DBNull.Value
                Else
                    PARA019.Value = LNM0002row("OUTHEIGHT")
                End If
                If String.IsNullOrEmpty(LNM0002row("OUTWIDTH")) Then                   '外寸・幅
                    PARA020.Value = DBNull.Value
                Else
                    PARA020.Value = LNM0002row("OUTWIDTH")
                End If
                If String.IsNullOrEmpty(LNM0002row("OUTLENGTH")) Then                  '外寸・長さ
                    PARA021.Value = DBNull.Value
                Else
                    PARA021.Value = LNM0002row("OUTLENGTH")
                End If
                If String.IsNullOrEmpty(LNM0002row("INHEIGHT")) Then                   '内寸・高さ
                    PARA022.Value = DBNull.Value
                Else
                    PARA022.Value = LNM0002row("INHEIGHT")
                End If
                If String.IsNullOrEmpty(LNM0002row("INWIDTH")) Then                    '内寸・幅
                    PARA023.Value = DBNull.Value
                Else
                    PARA023.Value = LNM0002row("INWIDTH")
                End If
                If String.IsNullOrEmpty(LNM0002row("INLENGTH")) Then                   '内寸・長さ
                    PARA024.Value = DBNull.Value
                Else
                    PARA024.Value = LNM0002row("INLENGTH")
                End If
                If String.IsNullOrEmpty(LNM0002row("WIFEHEIGHT")) Then                 '妻入口・高さ
                    PARA025.Value = DBNull.Value
                Else
                    PARA025.Value = LNM0002row("WIFEHEIGHT")
                End If
                If String.IsNullOrEmpty(LNM0002row("WIFEWIDTH")) Then                  '妻入口・幅
                    PARA026.Value = DBNull.Value
                Else
                    PARA026.Value = LNM0002row("WIFEWIDTH")
                End If
                If String.IsNullOrEmpty(LNM0002row("SIDEHEIGHT")) Then                 '側入口・高さ
                    PARA027.Value = DBNull.Value
                Else
                    PARA027.Value = LNM0002row("SIDEHEIGHT")
                End If
                If String.IsNullOrEmpty(LNM0002row("SIDEWIDTH")) Then                  '側入口・幅
                    PARA028.Value = DBNull.Value
                Else
                    PARA028.Value = LNM0002row("SIDEWIDTH")
                End If
                If String.IsNullOrEmpty(LNM0002row("FLOORAREA")) Then                  '床面積
                    PARA029.Value = DBNull.Value
                Else
                    PARA029.Value = LNM0002row("FLOORAREA")
                End If
                If String.IsNullOrEmpty(LNM0002row("INVOLUMEMARKING")) Then            '内容積・標記
                    PARA030.Value = DBNull.Value
                Else
                    PARA030.Value = LNM0002row("INVOLUMEMARKING")
                End If
                If String.IsNullOrEmpty(LNM0002row("INVOLUMEACTUA")) Then              '内容積・実寸
                    PARA031.Value = DBNull.Value
                Else
                    PARA031.Value = LNM0002row("INVOLUMEACTUA")
                End If
                If String.IsNullOrEmpty(LNM0002row("TRAINSCYCLEDAYS")) Then            '交番検査・交番検査
                    PARA032.Value = DBNull.Value
                Else
                    PARA032.Value = LNM0002row("TRAINSCYCLEDAYS")
                End If
                If String.IsNullOrEmpty(RTrim(LNM0002row("TRAINSBEFORERUNYMD"))) Then  '交番検査・前回実施日
                    PARA033.Value = DBNull.Value
                Else
                    PARA033.Value = RTrim(LNM0002row("TRAINSBEFORERUNYMD"))
                End If
                If String.IsNullOrEmpty(RTrim(LNM0002row("TRAINSNEXTRUNYMD"))) Then    '交番検査・次回実施日
                    PARA034.Value = DBNull.Value
                Else
                    PARA034.Value = RTrim(LNM0002row("TRAINSNEXTRUNYMD"))
                End If
                If String.IsNullOrEmpty(LNM0002row("REGINSCYCLEDAYS")) Then            '定期検査・ｻｲｸﾙ月数
                    PARA035.Value = DBNull.Value
                Else
                    PARA035.Value = LNM0002row("REGINSCYCLEDAYS")
                End If
                If String.IsNullOrEmpty(LNM0002row("REGINSCYCLEHOURMETER")) Then       '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
                    PARA036.Value = DBNull.Value
                Else
                    PARA036.Value = LNM0002row("REGINSCYCLEHOURMETER")
                End If
                If String.IsNullOrEmpty(RTrim(LNM0002row("REGINSBEFORERUNYMD"))) Then  '定期検査・前回実施日
                    PARA037.Value = DBNull.Value
                Else
                    PARA037.Value = RTrim(LNM0002row("REGINSBEFORERUNYMD"))
                End If
                If String.IsNullOrEmpty(RTrim(LNM0002row("REGINSNEXTRUNYMD"))) Then    '定期検査・次回実施日
                    PARA038.Value = DBNull.Value
                Else
                    PARA038.Value = RTrim(LNM0002row("REGINSNEXTRUNYMD"))
                End If
                If String.IsNullOrEmpty(RTrim(LNM0002row("REGINSHOURMETERYMD"))) Then  '定期検査・ｱﾜﾒｰﾀ記載日
                    PARA039.Value = DBNull.Value
                Else
                    PARA039.Value = RTrim(LNM0002row("REGINSHOURMETERYMD"))
                End If
                If String.IsNullOrEmpty(LNM0002row("REGINSHOURMETERTIME")) Then        '定期検査・ｱﾜﾒｰﾀ時間
                    PARA040.Value = DBNull.Value
                Else
                    PARA040.Value = LNM0002row("REGINSHOURMETERTIME")
                End If
                If String.IsNullOrEmpty(LNM0002row("REGINSHOURMETERDSP")) Then         '定期検査・ｱﾜﾒｰﾀ表示桁
                    PARA041.Value = DBNull.Value
                Else
                    PARA041.Value = LNM0002row("REGINSHOURMETERDSP")
                End If
                If String.IsNullOrEmpty(RTrim(LNM0002row("OPERATIONSTYMD"))) Then      '運用開始年月日
                    PARA042.Value = DBNull.Value
                Else
                    PARA042.Value = RTrim(LNM0002row("OPERATIONSTYMD"))
                End If
                If String.IsNullOrEmpty(RTrim(LNM0002row("OPERATIONENDYMD"))) Then     '運用除外年月日
                    PARA043.Value = DBNull.Value
                Else
                    PARA043.Value = RTrim(LNM0002row("OPERATIONENDYMD"))
                End If
                If String.IsNullOrEmpty(RTrim(LNM0002row("RETIRMENTYMD"))) Then        '除却年月日
                    PARA044.Value = DBNull.Value
                Else
                    PARA044.Value = RTrim(LNM0002row("RETIRMENTYMD"))
                End If
                PARA045.Value = LNM0002row("COMPKANKBN")                                   '複合一貫区分
                PARA108.Value = LNM0002row("SUPPLYFLG")                                    '調達フラグ
                PARA046.Value = LNM0002row("ADDITEM1")                                     '付帯項目１
                PARA047.Value = LNM0002row("ADDITEM2")                                     '付帯項目２
                PARA048.Value = LNM0002row("ADDITEM3")                                     '付帯項目３
                PARA049.Value = LNM0002row("ADDITEM4")                                     '付帯項目４
                PARA050.Value = LNM0002row("ADDITEM5")                                     '付帯項目５
                PARA051.Value = LNM0002row("ADDITEM6")                                     '付帯項目６
                PARA052.Value = LNM0002row("ADDITEM7")                                     '付帯項目７
                PARA053.Value = LNM0002row("ADDITEM8")                                     '付帯項目８
                PARA054.Value = LNM0002row("ADDITEM9")                                     '付帯項目９
                PARA055.Value = LNM0002row("ADDITEM10")                                    '付帯項目１０
                PARA056.Value = LNM0002row("ADDITEM11")                                    '付帯項目１１
                PARA057.Value = LNM0002row("ADDITEM12")                                    '付帯項目１２
                PARA058.Value = LNM0002row("ADDITEM13")                                    '付帯項目１３
                PARA059.Value = LNM0002row("ADDITEM14")                                    '付帯項目１４
                PARA060.Value = LNM0002row("ADDITEM15")                                    '付帯項目１５
                PARA061.Value = LNM0002row("ADDITEM16")                                    '付帯項目１６
                PARA062.Value = LNM0002row("ADDITEM17")                                    '付帯項目１７
                PARA063.Value = LNM0002row("ADDITEM18")                                    '付帯項目１８
                PARA064.Value = LNM0002row("ADDITEM19")                                    '付帯項目１９
                PARA065.Value = LNM0002row("ADDITEM20")                                    '付帯項目２０
                PARA066.Value = LNM0002row("ADDITEM21")                                    '付帯項目２１
                PARA067.Value = LNM0002row("ADDITEM22")                                    '付帯項目２２
                PARA068.Value = LNM0002row("ADDITEM23")                                    '付帯項目２３
                PARA069.Value = LNM0002row("ADDITEM24")                                    '付帯項目２４
                PARA070.Value = LNM0002row("ADDITEM25")                                    '付帯項目２５
                PARA071.Value = LNM0002row("ADDITEM26")                                    '付帯項目２６
                PARA072.Value = LNM0002row("ADDITEM27")                                    '付帯項目２７
                PARA073.Value = LNM0002row("ADDITEM28")                                    '付帯項目２８
                PARA074.Value = LNM0002row("ADDITEM29")                                    '付帯項目２９
                PARA075.Value = LNM0002row("ADDITEM30")                                    '付帯項目３０
                PARA076.Value = LNM0002row("ADDITEM31")                                    '付帯項目３１
                PARA077.Value = LNM0002row("ADDITEM32")                                    '付帯項目３２
                PARA078.Value = LNM0002row("ADDITEM33")                                    '付帯項目３３
                PARA079.Value = LNM0002row("ADDITEM34")                                    '付帯項目３４
                PARA080.Value = LNM0002row("ADDITEM35")                                    '付帯項目３５
                PARA081.Value = LNM0002row("ADDITEM36")                                    '付帯項目３６
                PARA082.Value = LNM0002row("ADDITEM37")                                    '付帯項目３７
                PARA083.Value = LNM0002row("ADDITEM38")                                    '付帯項目３８
                PARA084.Value = LNM0002row("ADDITEM39")                                    '付帯項目３９
                PARA085.Value = LNM0002row("ADDITEM40")                                    '付帯項目４０
                PARA086.Value = LNM0002row("ADDITEM41")                                    '付帯項目４１
                PARA087.Value = LNM0002row("ADDITEM42")                                    '付帯項目４２
                PARA088.Value = LNM0002row("ADDITEM43")                                    '付帯項目４３
                PARA089.Value = LNM0002row("ADDITEM44")                                    '付帯項目４４
                PARA090.Value = LNM0002row("ADDITEM45")                                    '付帯項目４５
                PARA091.Value = LNM0002row("ADDITEM46")                                    '付帯項目４６
                PARA092.Value = LNM0002row("ADDITEM47")                                    '付帯項目４７
                PARA093.Value = LNM0002row("ADDITEM48")                                    '付帯項目４８
                PARA094.Value = LNM0002row("ADDITEM49")                                    '付帯項目４９
                PARA095.Value = LNM0002row("ADDITEM50")                                    '付帯項目５０
                If String.IsNullOrEmpty(LNM0002row("FLOORMATERIAL").ToString) Then         '床材質コード
                    PARA096.Value = DBNull.Value
                Else
                    PARA096.Value = LNM0002row("FLOORMATERIAL")
                End If
                PARA098.Value = WW_NOW                                                     '登録年月日
                PARA099.Value = Master.USERID                                              '登録ユーザーＩＤ
                PARA100.Value = Master.USERTERMID                                          '登録端末
                PARA101.Value = Me.GetType().BaseType.Name                                 '登録プログラムＩＤ
                PARA102.Value = WW_NOW                                                     '更新年月日
                PARA103.Value = Master.USERID                                              '更新ユーザーＩＤ
                PARA104.Value = Master.USERTERMID                                          '更新端末
                PARA105.Value = Me.GetType().BaseType.Name                                 '更新プログラムＩＤ
                PARA106.Value = C_DEFAULT_YMD                                              '集信日時
                SQLcmd.CommandTimeout = 300
                SQLcmd.ExecuteNonQuery()

                ' 更新ジャーナル出力
                JPARA001.Value = LNM0002row("CTNTYPE")
                JPARA002.Value = LNM0002row("CTNNO")

                Using SQLdr As MySqlDataReader = SQLcmdJnl.ExecuteReader()
                    If IsNothing(LNM0002UPDtbl) Then
                        LNM0002UPDtbl = New DataTable

                        For index As Integer = 0 To SQLdr.FieldCount - 1
                            LNM0002UPDtbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                        Next
                    End If

                    LNM0002UPDtbl.Clear()
                    LNM0002UPDtbl.Load(SQLdr)
                End Using

                For Each LNM0002UPDrow As DataRow In LNM0002UPDtbl.Rows
                    CS0020JOURNAL.TABLENM = "LNM0002C"
                    CS0020JOURNAL.ACTION = "UPDATE_INSERT"
                    CS0020JOURNAL.ROW = LNM0002UPDrow
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0002C UPDATE_INSERT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0002C UPDATE_INSERT"
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
    Protected Sub RECONMEXISTS(ByVal SQLcon As MySqlConnection, ByRef WW_MODIFYKBN As String)

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        'コンテナマスタに同一キーのデータが存在するか確認する。
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        CTNTYPE")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNM0002_RECONM")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("        CTNTYPE      = @CTNTYPE")
        SQLStr.AppendLine("    AND CTNNO        = @CTNNO")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_CTNTYPE As MySqlParameter = SQLcmd.Parameters.Add("@CTNTYPE", MySqlDbType.VarChar, 5)         'コンテナ記号
                Dim P_CTNNO As MySqlParameter = SQLcmd.Parameters.Add("@CTNNO", MySqlDbType.VarChar, 8)         'コンテナ番号

                Dim LNM0002row As DataRow = LNM0002INPtbl.Rows(0)

                P_CTNTYPE.Value = LNM0002row("CTNTYPE")               'コンテナ記号
                P_CTNNO.Value = LNM0002row("CTNNO")               'コンテナ番号

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
                        WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.BEFDATA '変更前
                    Else
                        WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.NEWDATA '新規
                    End If
                End Using
            End Using
        Catch ex As Exception
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0002C SELECT")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0002C Select"
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

                Dim LNM0002row As DataRow = LNM0002INPtbl.Rows(0)

                ' DB更新
                P_CTNTYPE.Value = LNM0002row("CTNTYPE")               'コンテナ記号
                P_CTNNO.Value = LNM0002row("CTNNO")               'コンテナ番号

                '操作区分
                '変更区分が新規の場合
                If WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.NEWDATA Then
                    P_OPERATEKBN.Value = CInt(LNM0002WRKINC.OPERATEKBN.NEWDATA).ToString
                Else
                    '削除データの場合
                    If LNM0002tbl.Rows(0)("DELFLG") = "0" And LNM0002row("DELFLG") = "1" Then
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
            WW_ErrSW = C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

#End Region

    ' ******************************************************************************
    ' ***  詳細表示関連操作                                                      ***
    ' ******************************************************************************

    ''' <summary>
    ''' 詳細画面-更新ボタン押下、確認ダイアログOKボタン押下時処理
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub WF_UPDATE_ConfirmOkClick()

        '○ エラーレポート準備
        rightview.SetErrorReport("")

        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0002INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        '○ 項目チェック
        INPTableCheck(WW_ErrSW)

        '○ 入力値のテーブル反映
        If isNormal(WW_ErrSW) Then
            LNM0002tbl_UPD()
            ' 入力レコードに変更がない場合は、メッセージダイアログを表示して処理打ち切り
            If C_MESSAGE_NO.NO_CHANGE_UPDATE.Equals(WW_ErrCode) Then
                Master.Output(C_MESSAGE_NO.NO_CHANGE_UPDATE, C_MESSAGE_TYPE.WAR, needsPopUp:=True)
                Exit Sub
            End If
        End If

        '○ 画面表示データ保存
        Master.SaveTable(LNM0002tbl, work.WF_SEL_INPTBL.Text)

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
                Master.Output(WW_ErrSW, C_MESSAGE_TYPE.ERR, "コンテナ", needsPopUp:=True)
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
    Protected Sub DetailBoxToLNM0002INPtbl(ByRef O_RTN As String)

        O_RTN = C_MESSAGE_NO.NORMAL

        '○ 画面(Repeaterヘッダー情報)の使用禁止文字排除
        Master.EraseCharToIgnore(TxtDelFlg.Text)                '削除フラグ
        Master.EraseCharToIgnore(TxtCTNType.Text)               'コンテナ記号
        Master.EraseCharToIgnore(TxtCTNNo.Text)                 'コンテナ番号
        Master.EraseCharToIgnore(TxtJurisdictionCD.Text)        '所管部コード
        Master.EraseCharToIgnore(TxtAccountingAsSetCD.Text)     '経理資産コード
        Master.EraseCharToIgnore(TxtAccountingAsSetKbn.Text)    '経理資産区分
        Master.EraseCharToIgnore(TxtDummyKbn.Text)              'ダミー区分
        Master.EraseCharToIgnore(TxtSpotKbn.Text)               'スポット区分
        Master.EraseCharToIgnore(TxtSpotStYMD.Text)             'スポット区分　開始年月日
        Master.EraseCharToIgnore(TxtSpotEndYMD.Text)            'スポット区分　終了年月日
        Master.EraseCharToIgnore(TxtBigCTNCD.Text)              '大分類コード
        Master.EraseCharToIgnore(TxtMiddleCTNCD.Text)           '中分類コード
        Master.EraseCharToIgnore(TxtSmallCTNCD.Text)            '小分類コード
        Master.EraseCharToIgnore(TxtConstructionYM.Text)        '建造年月
        Master.EraseCharToIgnore(TxtCTNMaker.Text)              'コンテナメーカー
        Master.EraseCharToIgnore(TxtFrozenMaker.Text)           '冷凍機メーカー
        Master.EraseCharToIgnore(TxtGrossWeight.Text)           '総重量
        Master.EraseCharToIgnore(TxtCargoWeight.Text)           '荷重
        Master.EraseCharToIgnore(TxtMyWeight.Text)              '自重
        Master.EraseCharToIgnore(TxtBookValue.Text)             '簿価商品価格
        Master.EraseCharToIgnore(TxtOutHeight.Text)             '外寸・高さ
        Master.EraseCharToIgnore(TxtOutWidth.Text)              '外寸・幅
        Master.EraseCharToIgnore(TxtOutLength.Text)             '外寸・長さ
        Master.EraseCharToIgnore(TxtInHeight.Text)              '内寸・高さ
        Master.EraseCharToIgnore(TxtInWidth.Text)               '内寸・幅
        Master.EraseCharToIgnore(TxtInLength.Text)              '内寸・長さ
        Master.EraseCharToIgnore(TxtWifeHeight.Text)            '妻入口・高さ
        Master.EraseCharToIgnore(TxtWifeWidth.Text)             '妻入口・幅
        Master.EraseCharToIgnore(TxtSideHeight.Text)            '側入口・高さ
        Master.EraseCharToIgnore(TxtSideWidth.Text)             '側入口・幅
        Master.EraseCharToIgnore(TxtFloorArea.Text)             '床面積
        Master.EraseCharToIgnore(TxtInVolumeMarking.Text)       '内容積・標記
        Master.EraseCharToIgnore(TxtInVolumeActua.Text)         '内容積・実寸
        Master.EraseCharToIgnore(TxtTrainsCycleDays.Text)       '交番検査・ｻｲｸﾙ日数
        Master.EraseCharToIgnore(TxtTrainsBeforeRunYMD.Text)    '交番検査・前回実施日
        Master.EraseCharToIgnore(TxtTrainsNextRunYMD.Text)      '交番検査・次回実施日
        Master.EraseCharToIgnore(TxtReginsCycleDays.Text)       '定期検査・ｻｲｸﾙ月数
        Master.EraseCharToIgnore(TxtReginsCycleHourMeter.Text)  '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
        Master.EraseCharToIgnore(TxtReginsBeforeRunYMD.Text)    '定期検査・前回実施日
        Master.EraseCharToIgnore(TxtReginsNextRunYMD.Text)      '定期検査・次回実施日
        Master.EraseCharToIgnore(TxtReginsHourMeterYMD.Text)    '定期検査・ｱﾜﾒｰﾀ記載日
        Master.EraseCharToIgnore(TxtReginsHourMeterTime.Text)   '定期検査・ｱﾜﾒｰﾀ時間
        Master.EraseCharToIgnore(TxtReginsHourMeterDSP.Text)    '定期検査・ｱﾜﾒｰﾀ表示桁
        Master.EraseCharToIgnore(TxtOperationStYMD.Text)        '運用開始年月日
        Master.EraseCharToIgnore(TxtOperationEndYMD.Text)       '運用除外年月日
        Master.EraseCharToIgnore(TxtRetirmentYMD.Text)          '除却年月日
        Master.EraseCharToIgnore(TxtCompKanKbn.Text)            '複合一貫区分
        Master.EraseCharToIgnore(TxtSupplyFLG.Text)             '調達フラグ
        Master.EraseCharToIgnore(TxtAddItem1.Text)              '付帯項目１
        Master.EraseCharToIgnore(TxtAddItem2.Text)              '付帯項目２
        Master.EraseCharToIgnore(TxtAddItem3.Text)              '付帯項目３
        Master.EraseCharToIgnore(TxtAddItem4.Text)              '付帯項目４
        Master.EraseCharToIgnore(TxtAddItem5.Text)              '付帯項目５
        Master.EraseCharToIgnore(TxtAddItem6.Text)              '付帯項目６
        Master.EraseCharToIgnore(TxtAddItem7.Text)              '付帯項目７
        Master.EraseCharToIgnore(TxtAddItem8.Text)              '付帯項目８
        Master.EraseCharToIgnore(TxtAddItem9.Text)              '付帯項目９
        Master.EraseCharToIgnore(TxtAddItem10.Text)             '付帯項目１０
        Master.EraseCharToIgnore(TxtAddItem11.Text)             '付帯項目１１
        Master.EraseCharToIgnore(TxtAddItem12.Text)             '付帯項目１２
        Master.EraseCharToIgnore(TxtAddItem13.Text)             '付帯項目１３
        Master.EraseCharToIgnore(TxtAddItem14.Text)             '付帯項目１４
        Master.EraseCharToIgnore(TxtAddItem15.Text)             '付帯項目１５
        Master.EraseCharToIgnore(TxtAddItem16.Text)             '付帯項目１６
        Master.EraseCharToIgnore(TxtAddItem17.Text)             '付帯項目１７
        Master.EraseCharToIgnore(TxtAddItem18.Text)             '付帯項目１８
        Master.EraseCharToIgnore(TxtAddItem19.Text)             '付帯項目１９
        Master.EraseCharToIgnore(TxtAddItem20.Text)             '付帯項目２０
        Master.EraseCharToIgnore(TxtAddItem21.Text)             '付帯項目２１
        Master.EraseCharToIgnore(TxtAddItem22.Text)             '付帯項目２２
        Master.EraseCharToIgnore(TxtAddItem23.Text)             '付帯項目２３
        Master.EraseCharToIgnore(TxtAddItem24.Text)             '付帯項目２４
        Master.EraseCharToIgnore(TxtAddItem25.Text)             '付帯項目２５
        Master.EraseCharToIgnore(TxtAddItem26.Text)             '付帯項目２６
        Master.EraseCharToIgnore(TxtAddItem27.Text)             '付帯項目２７
        Master.EraseCharToIgnore(TxtAddItem28.Text)             '付帯項目２８
        Master.EraseCharToIgnore(TxtAddItem29.Text)             '付帯項目２９
        Master.EraseCharToIgnore(TxtAddItem30.Text)             '付帯項目３０
        Master.EraseCharToIgnore(TxtAddItem31.Text)             '付帯項目３１
        Master.EraseCharToIgnore(TxtAddItem32.Text)             '付帯項目３２
        Master.EraseCharToIgnore(TxtAddItem33.Text)             '付帯項目３３
        Master.EraseCharToIgnore(TxtAddItem34.Text)             '付帯項目３４
        Master.EraseCharToIgnore(TxtAddItem35.Text)             '付帯項目３５
        Master.EraseCharToIgnore(TxtAddItem36.Text)             '付帯項目３６
        Master.EraseCharToIgnore(TxtAddItem37.Text)             '付帯項目３７
        Master.EraseCharToIgnore(TxtAddItem38.Text)             '付帯項目３８
        Master.EraseCharToIgnore(TxtAddItem39.Text)             '付帯項目３９
        Master.EraseCharToIgnore(TxtAddItem40.Text)             '付帯項目４０
        Master.EraseCharToIgnore(TxtAddItem41.Text)             '付帯項目４１
        Master.EraseCharToIgnore(TxtAddItem42.Text)             '付帯項目４２
        Master.EraseCharToIgnore(TxtAddItem43.Text)             '付帯項目４３
        Master.EraseCharToIgnore(TxtAddItem44.Text)             '付帯項目４４
        Master.EraseCharToIgnore(TxtAddItem45.Text)             '付帯項目４５
        Master.EraseCharToIgnore(TxtAddItem46.Text)             '付帯項目４６
        Master.EraseCharToIgnore(TxtAddItem47.Text)             '付帯項目４７
        Master.EraseCharToIgnore(TxtAddItem48.Text)             '付帯項目４８
        Master.EraseCharToIgnore(TxtAddItem49.Text)             '付帯項目４９
        Master.EraseCharToIgnore(TxtAddItem50.Text)             '付帯項目５０
        Master.EraseCharToIgnore(TxtFloorMaterial.Text)         '床材質コード

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

        Master.CreateEmptyTable(LNM0002INPtbl, work.WF_SEL_INPTBL.Text)
        Dim LNM0002INProw As DataRow = LNM0002INPtbl.NewRow

        'LINECNT
        If String.IsNullOrEmpty(LblSelLineCNT.Text) Then
            LNM0002INProw("LINECNT") = 0
        Else
            Try
                Integer.TryParse(LblSelLineCNT.Text, LNM0002INProw("LINECNT"))
            Catch ex As Exception
                LNM0002INProw("LINECNT") = 0
            End Try
        End If

        LNM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
        'LNM0002INProw("UPDTIMSTP") = 0
        LNM0002INProw("SELECT") = 1
        LNM0002INProw("HIDDEN") = 0

        LNM0002INProw("DELFLG") = TxtDelFlg.Text                              '削除フラグ
        LNM0002INProw("CTNTYPE") = TxtCTNType.Text                            'コンテナ記号
        LNM0002INProw("CTNNO") = TxtCTNNo.Text                                'コンテナ番号
        LNM0002INProw("JURISDICTIONCD") = TxtJurisdictionCD.Text              '所管部コード
        LNM0002INProw("ACCOUNTINGASSETSCD") = TxtAccountingAsSetCD.Text       '経理資産コード
        LNM0002INProw("ACCOUNTINGASSETSKBN") = TxtAccountingAsSetKbn.Text     '経理資産区分
        LNM0002INProw("DUMMYKBN") = TxtDummyKbn.Text                          'ダミー区分
        LNM0002INProw("SPOTKBN") = TxtSpotKbn.Text                            'スポット区分
        LNM0002INProw("SPOTSTYMD") = TxtSpotStYMD.Text                        'スポット区分　開始年月日
        LNM0002INProw("SPOTENDYMD") = TxtSpotEndYMD.Text                      'スポット区分　終了年月日
        LNM0002INProw("BIGCTNCD") = TxtBigCTNCD.Text                          '大分類コード
        LNM0002INProw("MIDDLECTNCD") = TxtMiddleCTNCD.Text                    '中分類コード
        LNM0002INProw("SMALLCTNCD") = TxtSmallCTNCD.Text                      '小分類コード
        LNM0002INProw("CONSTRUCTIONYM") = TxtConstructionYM.Text              '建造年月
        LNM0002INProw("CTNMAKER") = TxtCTNMaker.Text                          'コンテナメーカー
        LNM0002INProw("FROZENMAKER") = TxtFrozenMaker.Text                    '冷凍機メーカー
        LNM0002INProw("GROSSWEIGHT") = TxtGrossWeight.Text                    '総重量
        LNM0002INProw("CARGOWEIGHT") = TxtCargoWeight.Text                    '荷重
        LNM0002INProw("MYWEIGHT") = TxtMyWeight.Text                          '自重
        LNM0002INProw("BOOKVALUE") = Val(TxtBookValue.Text)                   '簿価商品価格
        LNM0002INProw("OUTHEIGHT") = TxtOutHeight.Text                        '外寸・高さ
        LNM0002INProw("OUTWIDTH") = TxtOutWidth.Text                          '外寸・幅
        LNM0002INProw("OUTLENGTH") = TxtOutLength.Text                        '外寸・長さ
        LNM0002INProw("INHEIGHT") = TxtInHeight.Text                          '内寸・高さ
        LNM0002INProw("INWIDTH") = TxtInWidth.Text                            '内寸・幅
        LNM0002INProw("INLENGTH") = TxtInLength.Text                          '内寸・長さ
        LNM0002INProw("WIFEHEIGHT") = TxtWifeHeight.Text                      '妻入口・高さ
        LNM0002INProw("WIFEWIDTH") = TxtWifeWidth.Text                        '妻入口・幅
        LNM0002INProw("SIDEHEIGHT") = TxtSideHeight.Text                      '側入口・高さ
        LNM0002INProw("SIDEWIDTH") = TxtSideWidth.Text                        '側入口・幅
        LNM0002INProw("FLOORAREA") = TxtFloorArea.Text                        '床面積
        LNM0002INProw("INVOLUMEMARKING") = TxtInVolumeMarking.Text            '内容積・標記
        LNM0002INProw("INVOLUMEACTUA") = TxtInVolumeActua.Text                '内容積・実寸
        LNM0002INProw("TRAINSCYCLEDAYS") = TxtTrainsCycleDays.Text            '交番検査・ｻｲｸﾙ日数
        LNM0002INProw("TRAINSBEFORERUNYMD") = TxtTrainsBeforeRunYMD.Text      '交番検査・前回実施日
        LNM0002INProw("TRAINSNEXTRUNYMD") = TxtTrainsNextRunYMD.Text          '交番検査・次回実施日
        LNM0002INProw("REGINSCYCLEDAYS") = TxtReginsCycleDays.Text            '定期検査・ｻｲｸﾙ月数
        LNM0002INProw("REGINSCYCLEHOURMETER") = TxtReginsCycleHourMeter.Text  '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
        LNM0002INProw("REGINSBEFORERUNYMD") = TxtReginsBeforeRunYMD.Text      '定期検査・前回実施日
        LNM0002INProw("REGINSNEXTRUNYMD") = TxtReginsNextRunYMD.Text          '定期検査・次回実施日
        LNM0002INProw("REGINSHOURMETERYMD") = TxtReginsHourMeterYMD.Text      '定期検査・ｱﾜﾒｰﾀ記載日
        LNM0002INProw("REGINSHOURMETERTIME") = TxtReginsHourMeterTime.Text    '定期検査・ｱﾜﾒｰﾀ時間
        LNM0002INProw("REGINSHOURMETERDSP") = TxtReginsHourMeterDSP.Text      '定期検査・ｱﾜﾒｰﾀ表示桁
        LNM0002INProw("OPERATIONSTYMD") = TxtOperationStYMD.Text              '運用開始年月日
        LNM0002INProw("OPERATIONENDYMD") = TxtOperationEndYMD.Text            '運用除外年月日
        LNM0002INProw("RETIRMENTYMD") = TxtRetirmentYMD.Text                  '除却年月日
        LNM0002INProw("COMPKANKBN") = TxtCompKanKbn.Text                      '複合一貫区分
        LNM0002INProw("SUPPLYFLG") = TxtSupplyFLG.Text                        '調達フラグ
        LNM0002INProw("ADDITEM1") = TxtAddItem1.Text                          '付帯項目１
        LNM0002INProw("ADDITEM2") = TxtAddItem2.Text                          '付帯項目２
        LNM0002INProw("ADDITEM3") = TxtAddItem3.Text                          '付帯項目３
        LNM0002INProw("ADDITEM4") = TxtAddItem4.Text                          '付帯項目４
        LNM0002INProw("ADDITEM5") = TxtAddItem5.Text                          '付帯項目５
        LNM0002INProw("ADDITEM6") = TxtAddItem6.Text                          '付帯項目６
        LNM0002INProw("ADDITEM7") = TxtAddItem7.Text                          '付帯項目７
        LNM0002INProw("ADDITEM8") = TxtAddItem8.Text                          '付帯項目８
        LNM0002INProw("ADDITEM9") = TxtAddItem9.Text                          '付帯項目９
        LNM0002INProw("ADDITEM10") = TxtAddItem10.Text                        '付帯項目１０
        LNM0002INProw("ADDITEM11") = TxtAddItem11.Text                        '付帯項目１１
        LNM0002INProw("ADDITEM12") = TxtAddItem12.Text                        '付帯項目１２
        LNM0002INProw("ADDITEM13") = TxtAddItem13.Text                        '付帯項目１３
        LNM0002INProw("ADDITEM14") = TxtAddItem14.Text                        '付帯項目１４
        LNM0002INProw("ADDITEM15") = TxtAddItem15.Text                        '付帯項目１５
        LNM0002INProw("ADDITEM16") = TxtAddItem16.Text                        '付帯項目１６
        LNM0002INProw("ADDITEM17") = TxtAddItem17.Text                        '付帯項目１７
        LNM0002INProw("ADDITEM18") = TxtAddItem18.Text                        '付帯項目１８
        LNM0002INProw("ADDITEM19") = TxtAddItem19.Text                        '付帯項目１９
        LNM0002INProw("ADDITEM20") = TxtAddItem20.Text                        '付帯項目２０
        LNM0002INProw("ADDITEM21") = TxtAddItem21.Text                        '付帯項目２１
        LNM0002INProw("ADDITEM22") = TxtAddItem22.Text                        '付帯項目２２
        LNM0002INProw("ADDITEM23") = TxtAddItem23.Text                        '付帯項目２３
        LNM0002INProw("ADDITEM24") = TxtAddItem24.Text                        '付帯項目２４
        LNM0002INProw("ADDITEM25") = TxtAddItem25.Text                        '付帯項目２５
        LNM0002INProw("ADDITEM26") = TxtAddItem26.Text                        '付帯項目２６
        LNM0002INProw("ADDITEM27") = TxtAddItem27.Text                        '付帯項目２７
        LNM0002INProw("ADDITEM28") = TxtAddItem28.Text                        '付帯項目２８
        LNM0002INProw("ADDITEM29") = TxtAddItem29.Text                        '付帯項目２９
        LNM0002INProw("ADDITEM30") = TxtAddItem30.Text                        '付帯項目３０
        LNM0002INProw("ADDITEM31") = TxtAddItem31.Text                        '付帯項目３１
        LNM0002INProw("ADDITEM32") = TxtAddItem32.Text                        '付帯項目３２
        LNM0002INProw("ADDITEM33") = TxtAddItem33.Text                        '付帯項目３３
        LNM0002INProw("ADDITEM34") = TxtAddItem34.Text                        '付帯項目３４
        LNM0002INProw("ADDITEM35") = TxtAddItem35.Text                        '付帯項目３５
        LNM0002INProw("ADDITEM36") = TxtAddItem36.Text                        '付帯項目３６
        LNM0002INProw("ADDITEM37") = TxtAddItem37.Text                        '付帯項目３７
        LNM0002INProw("ADDITEM38") = TxtAddItem38.Text                        '付帯項目３８
        LNM0002INProw("ADDITEM39") = TxtAddItem39.Text                        '付帯項目３９
        LNM0002INProw("ADDITEM40") = TxtAddItem40.Text                        '付帯項目４０
        LNM0002INProw("ADDITEM41") = TxtAddItem41.Text                        '付帯項目４１
        LNM0002INProw("ADDITEM42") = TxtAddItem42.Text                        '付帯項目４２
        LNM0002INProw("ADDITEM43") = TxtAddItem43.Text                        '付帯項目４３
        LNM0002INProw("ADDITEM44") = TxtAddItem44.Text                        '付帯項目４４
        LNM0002INProw("ADDITEM45") = TxtAddItem45.Text                        '付帯項目４５
        LNM0002INProw("ADDITEM46") = TxtAddItem46.Text                        '付帯項目４６
        LNM0002INProw("ADDITEM47") = TxtAddItem47.Text                        '付帯項目４７
        LNM0002INProw("ADDITEM48") = TxtAddItem48.Text                        '付帯項目４８
        LNM0002INProw("ADDITEM49") = TxtAddItem49.Text                        '付帯項目４９
        LNM0002INProw("ADDITEM50") = TxtAddItem50.Text                        '付帯項目５０
        LNM0002INProw("FLOORMATERIAL") = TxtFloorMaterial.Text                '床材質コード

        '○ チェック用テーブルに登録する
        LNM0002INPtbl.Rows.Add(LNM0002INProw)

    End Sub
    ''' <summary>
    ''' 詳細画面-更新ボタン押下時処理
    ''' </summary>
    Protected Sub WF_UPDATE_Click()

        WW_ErrSW = Messages.C_MESSAGE_NO.NORMAL

        If Not String.IsNullOrEmpty(TxtTrainsCycleDays.Text) Then        '交番検査・ｻｲｸﾙ日数
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()
                ' 交番検査・ｻｲｸﾙ日数 自動計算値同値チェック
                KobanCheck(SQLcon, WW_ErrSW)
            End Using
        End If
        If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
            ' エラーの場合は、確認ダイアログを表示し警告を表示
            Master.Output(C_MESSAGE_NO.CTN_KOBANCYCLE_ERR, C_MESSAGE_TYPE.WAR, I_PARA02:="W",
                    needsPopUp:=True, messageBoxTitle:="警告", IsConfirm:=True, YesButtonId:="btnUpdateConfirmOK")
        Else
            ' エラーではない場合は、確認ダイアログを表示せずに更新処理を実行
            WF_UPDATE_ConfirmOkClick()
        End If
    End Sub

    ''' <summary>
    ''' 詳細画面-戻るボタン押下時処理
    ''' </summary>
    Protected Sub WF_CLEAR_Click()
        '○ DetailBoxをINPtblへ退避
        DetailBoxToLNM0002INPtbl(WW_ErrSW)
        If Not isNormal(WW_ErrSW) Then
            Exit Sub
        End If

        Dim WW_InputChangeFlg As Boolean = True
        Dim LNM0002INProw As DataRow = LNM0002INPtbl.Rows(0)

        ' 既存レコードとの比較
        For Each LNM0002row As DataRow In LNM0002tbl.Rows
            ' KEY項目が等しい時
            If LNM0002row("CTNTYPE") = LNM0002INProw("CTNTYPE") AndAlso
                LNM0002row("CTNNO") = LNM0002INProw("CTNNO") Then
                ' KEY項目以外の項目の差異をチェック
                If LNM0002row("DELFLG") = LNM0002INProw("DELFLG") AndAlso
                    LNM0002row("JURISDICTIONCD") = LNM0002INProw("JURISDICTIONCD") AndAlso
                    LNM0002row("ACCOUNTINGASSETSCD") = LNM0002INProw("ACCOUNTINGASSETSCD") AndAlso
                    LNM0002row("ACCOUNTINGASSETSKBN") = LNM0002INProw("ACCOUNTINGASSETSKBN") AndAlso
                    LNM0002row("DUMMYKBN") = LNM0002INProw("DUMMYKBN") AndAlso
                    LNM0002row("SPOTKBN") = LNM0002INProw("SPOTKBN") AndAlso
                    LNM0002row("SPOTSTYMD") = LNM0002INProw("SPOTSTYMD") AndAlso
                    LNM0002row("SPOTENDYMD") = LNM0002INProw("SPOTENDYMD") AndAlso
                    LNM0002row("BIGCTNCD") = LNM0002INProw("BIGCTNCD") AndAlso
                    LNM0002row("MIDDLECTNCD") = LNM0002INProw("MIDDLECTNCD") AndAlso
                    LNM0002row("SMALLCTNCD") = LNM0002INProw("SMALLCTNCD") AndAlso
                    LNM0002row("CONSTRUCTIONYM") = LNM0002INProw("CONSTRUCTIONYM") AndAlso
                    LNM0002row("CTNMAKER") = LNM0002INProw("CTNMAKER") AndAlso
                    LNM0002row("FROZENMAKER") = LNM0002INProw("FROZENMAKER") AndAlso
                    LNM0002row("GROSSWEIGHT") = LNM0002INProw("GROSSWEIGHT") AndAlso
                    LNM0002row("CARGOWEIGHT") = LNM0002INProw("CARGOWEIGHT") AndAlso
                    LNM0002row("MYWEIGHT") = LNM0002INProw("MYWEIGHT") AndAlso
                    LNM0002row("BOOKVALUE") = LNM0002INProw("BOOKVALUE") AndAlso
                    LNM0002row("OUTHEIGHT") = LNM0002INProw("OUTHEIGHT") AndAlso
                    LNM0002row("OUTWIDTH") = LNM0002INProw("OUTWIDTH") AndAlso
                    LNM0002row("OUTLENGTH") = LNM0002INProw("OUTLENGTH") AndAlso
                    LNM0002row("INHEIGHT") = LNM0002INProw("INHEIGHT") AndAlso
                    LNM0002row("INWIDTH") = LNM0002INProw("INWIDTH") AndAlso
                    LNM0002row("INLENGTH") = LNM0002INProw("INLENGTH") AndAlso
                    LNM0002row("WIFEHEIGHT") = LNM0002INProw("WIFEHEIGHT") AndAlso
                    LNM0002row("WIFEWIDTH") = LNM0002INProw("WIFEWIDTH") AndAlso
                    LNM0002row("SIDEHEIGHT") = LNM0002INProw("SIDEHEIGHT") AndAlso
                    LNM0002row("SIDEWIDTH") = LNM0002INProw("SIDEWIDTH") AndAlso
                    LNM0002row("FLOORAREA") = LNM0002INProw("FLOORAREA") AndAlso
                    LNM0002row("INVOLUMEMARKING") = LNM0002INProw("INVOLUMEMARKING") AndAlso
                    LNM0002row("INVOLUMEACTUA") = LNM0002INProw("INVOLUMEACTUA") AndAlso
                    LNM0002row("TRAINSCYCLEDAYS") = LNM0002INProw("TRAINSCYCLEDAYS") AndAlso
                    LNM0002row("TRAINSBEFORERUNYMD") = LNM0002INProw("TRAINSBEFORERUNYMD") AndAlso
                    LNM0002row("TRAINSNEXTRUNYMD") = LNM0002INProw("TRAINSNEXTRUNYMD") AndAlso
                    LNM0002row("REGINSCYCLEDAYS") = LNM0002INProw("REGINSCYCLEDAYS") AndAlso
                    LNM0002row("REGINSCYCLEHOURMETER") = LNM0002INProw("REGINSCYCLEHOURMETER") AndAlso
                    LNM0002row("REGINSBEFORERUNYMD") = LNM0002INProw("REGINSBEFORERUNYMD") AndAlso
                    LNM0002row("REGINSNEXTRUNYMD") = LNM0002INProw("REGINSNEXTRUNYMD") AndAlso
                    LNM0002row("REGINSHOURMETERYMD") = LNM0002INProw("REGINSHOURMETERYMD") AndAlso
                    LNM0002row("REGINSHOURMETERTIME") = LNM0002INProw("REGINSHOURMETERTIME") AndAlso
                    LNM0002row("REGINSHOURMETERDSP") = LNM0002INProw("REGINSHOURMETERDSP") AndAlso
                    LNM0002row("OPERATIONSTYMD") = LNM0002INProw("OPERATIONSTYMD") AndAlso
                    LNM0002row("OPERATIONENDYMD") = LNM0002INProw("OPERATIONENDYMD") AndAlso
                    LNM0002row("RETIRMENTYMD") = LNM0002INProw("RETIRMENTYMD") AndAlso
                    LNM0002row("COMPKANKBN") = LNM0002INProw("COMPKANKBN") AndAlso
                    LNM0002row("SUPPLYFLG") = LNM0002INProw("SUPPLYFLG") AndAlso
                    LNM0002row("ADDITEM1") = LNM0002INProw("ADDITEM1") AndAlso
                    LNM0002row("ADDITEM2") = LNM0002INProw("ADDITEM2") AndAlso
                    LNM0002row("ADDITEM3") = LNM0002INProw("ADDITEM3") AndAlso
                    LNM0002row("ADDITEM4") = LNM0002INProw("ADDITEM4") AndAlso
                    LNM0002row("ADDITEM5") = LNM0002INProw("ADDITEM5") AndAlso
                    LNM0002row("ADDITEM6") = LNM0002INProw("ADDITEM6") AndAlso
                    LNM0002row("ADDITEM7") = LNM0002INProw("ADDITEM7") AndAlso
                    LNM0002row("ADDITEM8") = LNM0002INProw("ADDITEM8") AndAlso
                    LNM0002row("ADDITEM9") = LNM0002INProw("ADDITEM9") AndAlso
                    LNM0002row("ADDITEM10") = LNM0002INProw("ADDITEM10") AndAlso
                    LNM0002row("ADDITEM11") = LNM0002INProw("ADDITEM11") AndAlso
                    LNM0002row("ADDITEM12") = LNM0002INProw("ADDITEM12") AndAlso
                    LNM0002row("ADDITEM13") = LNM0002INProw("ADDITEM13") AndAlso
                    LNM0002row("ADDITEM14") = LNM0002INProw("ADDITEM14") AndAlso
                    LNM0002row("ADDITEM15") = LNM0002INProw("ADDITEM15") AndAlso
                    LNM0002row("ADDITEM16") = LNM0002INProw("ADDITEM16") AndAlso
                    LNM0002row("ADDITEM17") = LNM0002INProw("ADDITEM17") AndAlso
                    LNM0002row("ADDITEM18") = LNM0002INProw("ADDITEM18") AndAlso
                    LNM0002row("ADDITEM19") = LNM0002INProw("ADDITEM19") AndAlso
                    LNM0002row("ADDITEM20") = LNM0002INProw("ADDITEM20") AndAlso
                    LNM0002row("ADDITEM21") = LNM0002INProw("ADDITEM21") AndAlso
                    LNM0002row("ADDITEM22") = LNM0002INProw("ADDITEM22") AndAlso
                    LNM0002row("ADDITEM23") = LNM0002INProw("ADDITEM23") AndAlso
                    LNM0002row("ADDITEM24") = LNM0002INProw("ADDITEM24") AndAlso
                    LNM0002row("ADDITEM25") = LNM0002INProw("ADDITEM25") AndAlso
                    LNM0002row("ADDITEM26") = LNM0002INProw("ADDITEM26") AndAlso
                    LNM0002row("ADDITEM27") = LNM0002INProw("ADDITEM27") AndAlso
                    LNM0002row("ADDITEM28") = LNM0002INProw("ADDITEM28") AndAlso
                    LNM0002row("ADDITEM29") = LNM0002INProw("ADDITEM29") AndAlso
                    LNM0002row("ADDITEM30") = LNM0002INProw("ADDITEM30") AndAlso
                    LNM0002row("ADDITEM31") = LNM0002INProw("ADDITEM31") AndAlso
                    LNM0002row("ADDITEM32") = LNM0002INProw("ADDITEM32") AndAlso
                    LNM0002row("ADDITEM33") = LNM0002INProw("ADDITEM33") AndAlso
                    LNM0002row("ADDITEM34") = LNM0002INProw("ADDITEM34") AndAlso
                    LNM0002row("ADDITEM35") = LNM0002INProw("ADDITEM35") AndAlso
                    LNM0002row("ADDITEM36") = LNM0002INProw("ADDITEM36") AndAlso
                    LNM0002row("ADDITEM37") = LNM0002INProw("ADDITEM37") AndAlso
                    LNM0002row("ADDITEM38") = LNM0002INProw("ADDITEM38") AndAlso
                    LNM0002row("ADDITEM39") = LNM0002INProw("ADDITEM39") AndAlso
                    LNM0002row("ADDITEM40") = LNM0002INProw("ADDITEM40") AndAlso
                    LNM0002row("ADDITEM41") = LNM0002INProw("ADDITEM41") AndAlso
                    LNM0002row("ADDITEM42") = LNM0002INProw("ADDITEM42") AndAlso
                    LNM0002row("ADDITEM43") = LNM0002INProw("ADDITEM43") AndAlso
                    LNM0002row("ADDITEM44") = LNM0002INProw("ADDITEM44") AndAlso
                    LNM0002row("ADDITEM45") = LNM0002INProw("ADDITEM45") AndAlso
                    LNM0002row("ADDITEM46") = LNM0002INProw("ADDITEM46") AndAlso
                    LNM0002row("ADDITEM47") = LNM0002INProw("ADDITEM47") AndAlso
                    LNM0002row("ADDITEM48") = LNM0002INProw("ADDITEM48") AndAlso
                    LNM0002row("ADDITEM49") = LNM0002INProw("ADDITEM49") AndAlso
                    LNM0002row("ADDITEM50") = LNM0002INProw("ADDITEM50") AndAlso
                    LNM0002row("FLOORMATERIAL") = LNM0002INProw("FLOORMATERIAL") Then
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
        For Each LNM0002row As DataRow In LNM0002tbl.Rows
            Select Case LNM0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    WW_ErrSW = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.SELECTED
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    WW_ErrSW = C_MESSAGE_NO.NORMAL
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                    WW_ErrSW = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End Select
        Next

        '○ 画面表示データ保存
        Master.SaveTable(LNM0002tbl, work.WF_SEL_INPTBL.Text)

        LblSelLineCNT.Text = ""               'LINECNT
        TxtMapId.Text = "M00001"              '画面ＩＤ
        TxtDelFlg.Text = ""                   '削除フラグ
        TxtCTNType.Text = ""                  'コンテナ記号
        TxtCTNNo.Text = ""                    'コンテナ番号
        TxtJurisdictionCD.Text = ""           '所管部コード
        TxtAccountingAsSetCD.Text = ""        '経理資産コード
        TxtAccountingAsSetKbn.Text = ""       '経理資産区分
        TxtDummyKbn.Text = ""                 'ダミー区分
        TxtSpotKbn.Text = ""                  'スポット区分
        TxtSpotStYMD.Text = ""                'スポット区分　開始年月
        TxtSpotEndYMD.Text = ""               'スポット区分　終了年月
        TxtBigCTNCD.Text = ""                 '大分類コード
        TxtMiddleCTNCD.Text = ""              '中分類コード
        TxtSmallCTNCD.Text = ""               '小分類コード
        TxtConstructionYM.Text = ""           '建造年月
        TxtCTNMaker.Text = ""                 'コンテナメーカー
        TxtFrozenMaker.Text = ""              '冷凍機メーカー
        TxtGrossWeight.Text = ""              '総重量
        TxtCargoWeight.Text = ""              '荷重
        TxtMyWeight.Text = ""                 '自重
        TxtBookValue.Text = ""                '簿価商品価格
        TxtOutHeight.Text = ""                '外寸・高さ
        TxtOutWidth.Text = ""                 '外寸・幅
        TxtOutLength.Text = ""                '外寸・長さ
        TxtInHeight.Text = ""                 '内寸・高さ
        TxtInWidth.Text = ""                  '内寸・幅
        TxtInLength.Text = ""                 '内寸・長さ
        TxtWifeHeight.Text = ""               '妻入口・高さ
        TxtWifeWidth.Text = ""                '妻入口・幅
        TxtSideHeight.Text = ""               '側入口・高さ
        TxtSideWidth.Text = ""                '側入口・幅
        TxtFloorArea.Text = ""                '床面積
        TxtInVolumeMarking.Text = ""          '内容積・標記
        TxtInVolumeActua.Text = ""            '内容積・実寸
        TxtTrainsCycleDays.Text = ""          '交番検査・ｻｲｸﾙ日数
        TxtTrainsBeforeRunYMD.Text = ""       '交番検査・前回実施日
        TxtTrainsNextRunYMD.Text = ""         '交番検査・次回実施日
        TxtReginsCycleDays.Text = ""          '定期検査・ｻｲｸﾙ月数
        TxtReginsCycleHourMeter.Text = ""     '定期検査・ｻｲｸﾙｱﾜﾒｰﾀ
        TxtReginsBeforeRunYMD.Text = ""       '定期検査・前回実施日
        TxtReginsNextRunYMD.Text = ""         '定期検査・次回実施日
        TxtReginsHourMeterYMD.Text = ""       '定期検査・ｱﾜﾒｰﾀ記載日
        TxtReginsHourMeterTime.Text = ""      '定期検査・ｱﾜﾒｰﾀ時間
        TxtReginsHourMeterDSP.Text = ""       '定期検査・ｱﾜﾒｰﾀ表示桁
        TxtOperationStYMD.Text = ""           '運用開始年月日
        TxtOperationEndYMD.Text = ""          '運用除外年月日
        TxtRetirmentYMD.Text = ""             '除却年月日
        TxtCompKanKbn.Text = ""               '複合一貫区分
        TxtSupplyFLG.Text = ""                '調達フラグ
        TxtAddItem1.Text = ""                 '付帯項目１
        TxtAddItem2.Text = ""                 '付帯項目２
        TxtAddItem3.Text = ""                 '付帯項目３
        TxtAddItem4.Text = ""                 '付帯項目４
        TxtAddItem5.Text = ""                 '付帯項目５
        TxtAddItem6.Text = ""                 '付帯項目６
        TxtAddItem7.Text = ""                 '付帯項目７
        TxtAddItem8.Text = ""                 '付帯項目８
        TxtAddItem9.Text = ""                 '付帯項目９
        TxtAddItem10.Text = ""                '付帯項目１０
        TxtAddItem11.Text = ""                '付帯項目１１
        TxtAddItem12.Text = ""                '付帯項目１２
        TxtAddItem13.Text = ""                '付帯項目１３
        TxtAddItem14.Text = ""                '付帯項目１４
        TxtAddItem15.Text = ""                '付帯項目１５
        TxtAddItem16.Text = ""                '付帯項目１６
        TxtAddItem17.Text = ""                '付帯項目１７
        TxtAddItem18.Text = ""                '付帯項目１８
        TxtAddItem19.Text = ""                '付帯項目１９
        TxtAddItem20.Text = ""                '付帯項目２０
        TxtAddItem21.Text = ""                '付帯項目２１
        TxtAddItem22.Text = ""                '付帯項目２２
        TxtAddItem23.Text = ""                '付帯項目２３
        TxtAddItem24.Text = ""                '付帯項目２４
        TxtAddItem25.Text = ""                '付帯項目２５
        TxtAddItem26.Text = ""                '付帯項目２６
        TxtAddItem27.Text = ""                '付帯項目２７
        TxtAddItem28.Text = ""                '付帯項目２８
        TxtAddItem29.Text = ""                '付帯項目２９
        TxtAddItem30.Text = ""                '付帯項目３０
        TxtAddItem31.Text = ""                '付帯項目３１
        TxtAddItem32.Text = ""                '付帯項目３２
        TxtAddItem33.Text = ""                '付帯項目３３
        TxtAddItem34.Text = ""                '付帯項目３４
        TxtAddItem35.Text = ""                '付帯項目３５
        TxtAddItem36.Text = ""                '付帯項目３６
        TxtAddItem37.Text = ""                '付帯項目３７
        TxtAddItem38.Text = ""                '付帯項目３８
        TxtAddItem39.Text = ""                '付帯項目３９
        TxtAddItem40.Text = ""                '付帯項目４０
        TxtAddItem41.Text = ""                '付帯項目４１
        TxtAddItem42.Text = ""                '付帯項目４２
        TxtAddItem43.Text = ""                '付帯項目４３
        TxtAddItem44.Text = ""                '付帯項目４４
        TxtAddItem45.Text = ""                '付帯項目４５
        TxtAddItem46.Text = ""                '付帯項目４６
        TxtAddItem47.Text = ""                '付帯項目４７
        TxtAddItem48.Text = ""                '付帯項目４８
        TxtAddItem49.Text = ""                '付帯項目４９
        TxtAddItem50.Text = ""                '付帯項目５０
        TxtFloorMaterial.Text = ""            '床材質コード

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
                Select Case WF_LeftMViewChange.Value
                    Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                        ' 日付の場合、入力日付のカレンダーが表示されるように入力値をカレンダーに渡す
                        Select Case WF_FIELD.Value
                            Case "TxtSpotStYMD"           'スポット区分　開始年月日
                                .WF_Calendar.Text = TxtSpotStYMD.Text
                            Case "TxtSpotEndYMD"          'スポット区分　終了年月日
                                .WF_Calendar.Text = TxtSpotEndYMD.Text
                            Case "TxtReginsHourMeterYMD"  '定期検査・ｱﾜﾒｰﾀ記載日
                                .WF_Calendar.Text = TxtReginsHourMeterYMD.Text
                            Case "TxtOperationStYMD"      '運用開始年月日
                                .WF_Calendar.Text = TxtOperationStYMD.Text
                            Case "TxtOperationEndYMD"     '運用除外年月日
                                .WF_Calendar.Text = TxtOperationEndYMD.Text
                            Case "TxtRetirmentYMD"        '除却年月日
                                .WF_Calendar.Text = TxtRetirmentYMD.Text
                        End Select
                        .ActiveCalendar()

                    Case Else
                        ' フィールドによってパラメータを変える
                        Select Case WF_FIELD.Value
                            Case "TxtCTNType"             'コンテナ記号
                                WW_PrmData = work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_TYPE)
                            Case "TxtCTNNo"               'コンテナ番号
                                WW_PrmData = work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, TxtCTNType.Text)
                            Case "TxtJurisdictionCD"      '所管部コード
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "JURISDICTION")
                            Case "TxtAccountingAsSetCD"   '経理資産コード
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTINGASSETSCD")
                            Case "TxtAccountingAsSetKbn"  '経理資産区分
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ACCOUNTINGASSETSKBN")
                            Case "TxtDummyKbn"            'ダミー区分
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "DUMMYKBN")
                            Case "TxtSpotKbn"             'スポット区分
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "SPOTKBN")
                            Case "TxtBigCTNCD"            '大分類コード
                                WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS)
                            Case "TxtMiddleCTNCD"         '中分類コード
                                WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, TxtBigCTNCD.Text)
                            Case "TxtSmallCTNCD"          '小分類コード
                                WW_PrmData = work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.SMALL_CLASS, TxtBigCTNCD.Text, TxtMiddleCTNCD.Text)
                            Case "TxtCTNMaker"            'コンテナメーカー
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "CTNMAKER")
                            Case "TxtFrozenMaker"         '冷凍機メーカー
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "FROZENMAKER")
                            Case "TxtCompKanKbn"          '複合一貫区分
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "COMPKANKBN")
                            Case "TxtSupplyFLG"           '調達フラグ
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "SUPPLYFLG")
                            Case "TxtAddItem1"            '付帯項目１
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM1")
                            Case "TxtAddItem2"            '付帯項目２
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM2")
                            Case "TxtAddItem3"            '付帯項目３
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM3")
                            Case "TxtAddItem4"            '付帯項目４
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM4")
                            Case "TxtAddItem5"            '付帯項目５
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM5")
                            Case "TxtAddItem6"            '付帯項目６
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM6")
                            Case "TxtAddItem7"            '付帯項目７
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM7")
                            Case "TxtAddItem8"            '付帯項目８
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM8")
                            Case "TxtAddItem9"            '付帯項目９
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM9")
                            Case "TxtAddItem10"           '付帯項目１０
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM10")
                            Case "TxtAddItem11"           '付帯項目１１
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM11")
                            Case "TxtAddItem12"           '付帯項目１２
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM12")
                            Case "TxtAddItem13"           '付帯項目１３
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM13")
                            Case "TxtAddItem14"           '付帯項目１４
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM14")
                            Case "TxtAddItem15"           '付帯項目１５
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM15")
                            Case "TxtAddItem16"           '付帯項目１６
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM16")
                            Case "TxtAddItem17"           '付帯項目１７
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM17")
                            Case "TxtAddItem18"           '付帯項目１８
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM18")
                            Case "TxtAddItem19"           '付帯項目１９
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM19")
                            Case "TxtAddItem20"           '付帯項目２０
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM20")
                            Case "TxtAddItem21"           '付帯項目２１
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM21")
                            Case "TxtAddItem22"           '付帯項目２２
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM22")
                            Case "TxtAddItem23"           '付帯項目２３
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM23")
                            Case "TxtAddItem24"           '付帯項目２４
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM24")
                            Case "TxtAddItem25"           '付帯項目２５
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM25")
                            Case "TxtAddItem26"           '付帯項目２６
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM26")
                            Case "TxtAddItem27"           '付帯項目２７
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM27")
                            Case "TxtAddItem28"           '付帯項目２８
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM28")
                            Case "TxtAddItem29"           '付帯項目２９
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM29")
                            Case "TxtAddItem30"           '付帯項目３０
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM30")
                            Case "TxtAddItem31"           '付帯項目３１
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM31")
                            Case "TxtAddItem32"           '付帯項目３２
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM32")
                            Case "TxtAddItem33"           '付帯項目３３
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM33")
                            Case "TxtAddItem34"           '付帯項目３４
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM34")
                            Case "TxtAddItem35"           '付帯項目３５
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM35")
                            Case "TxtAddItem36"           '付帯項目３６
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM36")
                            Case "TxtAddItem37"           '付帯項目３７
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM37")
                            Case "TxtAddItem38"           '付帯項目３８
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM38")
                            Case "TxtAddItem39"           '付帯項目３９
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM39")
                            Case "TxtAddItem40"           '付帯項目４０
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM40")
                            Case "TxtAddItem41"           '付帯項目４１
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM41")
                            Case "TxtAddItem42"           '付帯項目４２
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM42")
                            Case "TxtAddItem43"           '付帯項目４３
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM43")
                            Case "TxtAddItem44"           '付帯項目４４
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM44")
                            Case "TxtAddItem45"           '付帯項目４５
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM45")
                            Case "TxtAddItem46"           '付帯項目４６
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM46")
                            Case "TxtAddItem47"           '付帯項目４７
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM47")
                            Case "TxtAddItem48"           '付帯項目４８
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM48")
                            Case "TxtAddItem49"           '付帯項目４９
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM49")
                            Case "TxtAddItem50"           '付帯項目５０
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "ADDITEM50")
                            Case "TxtFloorMaterial"       '床材質コード
                                WW_PrmData = work.CreateFIXParam(Master.USERCAMP, "FLOORMATERIAL")
                            Case "TxtDelFlg"              '削除フラグ
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
            Case "TxtCTNType"             'コンテナ記号
                CODENAME_get("CTNTYPE", TxtCTNType.Text, LblCTNTypeName.Text, WW_Dummy)
                TxtCTNType.Focus()
            Case "TxtJurisdictionCD"      '所管部コード
                CODENAME_get("JURISDICTION", TxtJurisdictionCD.Text, LblJurisdictionCDName.Text, WW_Dummy)
                TxtJurisdictionCD.Focus()
            Case "TxtAccountingAsSetCD"   '経理資産コード
                CODENAME_get("ACCOUNTINGASSETSCD", TxtAccountingAsSetCD.Text, LblAccountingAsSetCDName.Text, WW_Dummy)
                TxtAccountingAsSetCD.Focus()
            Case "TxtAccountingAsSetKbn"  '経理資産区分
                CODENAME_get("ACCOUNTINGASSETSKBN", TxtAccountingAsSetKbn.Text, LblAccountingAsSetKbnName.Text, WW_Dummy)
                TxtAccountingAsSetKbn.Focus()
            Case "TxtDummyKbn"            'ダミー区分
                CODENAME_get("DUMMYKBN", TxtDummyKbn.Text, LblDummyKbnName.Text, WW_Dummy)
                TxtDummyKbn.Focus()
            Case "TxtSpotKbn"             'スポット区分
                CODENAME_get("SPOTKBN", TxtSpotKbn.Text, LblSpotKbnName.Text, WW_Dummy)
                TxtSpotKbn.Focus()
            Case "TxtBigCTNCD"            '大分類コード
                CODENAME_get("BIGCTNCD", TxtBigCTNCD.Text, LblBigCTNCDName.Text, WW_Dummy)
                ReSetClassCd("1")
                TxtMiddleCTNCD.Focus()
            Case "TxtMiddleCTNCD"         '中分類コード
                CODENAME_get("MIDDLECTNCD", TxtMiddleCTNCD.Text, LblMiddleCTNCDName.Text, WW_Dummy)
                ReSetClassCd("")
                TxtSmallCTNCD.Focus()
            Case "TxtSmallCTNCD"          '小分類コード
                CODENAME_get("SMALLCTNCD", TxtSmallCTNCD.Text, LblSmallCTNCDName.Text, WW_Dummy)
                TxtSmallCTNCD.Focus()
            Case "TxtCTNMaker"            'コンテナメーカー
                CODENAME_get("CTNMAKER", TxtCTNMaker.Text, LblCTNMakerName.Text, WW_Dummy)
                TxtCTNMaker.Focus()
            Case "TxtFrozenMaker"         '冷凍機メーカー
                CODENAME_get("FROZENMAKER", TxtFrozenMaker.Text, LblFrozenMakerName.Text, WW_Dummy)
                TxtFrozenMaker.Focus()
            Case "TxtCompKanKbn"          '複合一貫区分
                CODENAME_get("COMPKANKBN", TxtCompKanKbn.Text, LblCompKanKbnName.Text, WW_Dummy)
                TxtCompKanKbn.Focus()
            Case "TxtSupplyFLG"           '調達フラグ
                CODENAME_get("SUPPLYFLG", TxtSupplyFLG.Text, LblSupplyFLGName.Text, WW_Dummy)
                TxtSupplyFLG.Focus()
            Case "TxtAddItem1"            '付帯項目１
                CODENAME_get("ADDITEM1", TxtAddItem1.Text, LblAddItem1Name.Text, WW_Dummy)
                TxtAddItem1.Focus()
            Case "TxtAddItem2"            '付帯項目２
                CODENAME_get("ADDITEM2", TxtAddItem2.Text, LblAddItem2Name.Text, WW_Dummy)
                TxtAddItem2.Focus()
            Case "TxtAddItem3"            '付帯項目３
                CODENAME_get("ADDITEM3", TxtAddItem3.Text, LblAddItem3Name.Text, WW_Dummy)
                TxtAddItem3.Focus()
            Case "TxtAddItem4"            '付帯項目４
                CODENAME_get("ADDITEM4", TxtAddItem4.Text, LblAddItem4Name.Text, WW_Dummy)
                TxtAddItem4.Focus()
            Case "TxtAddItem5"            '付帯項目５
                CODENAME_get("ADDITEM5", TxtAddItem5.Text, LblAddItem5Name.Text, WW_Dummy)
                TxtAddItem5.Focus()
            Case "TxtAddItem6"            '付帯項目６
                CODENAME_get("ADDITEM6", TxtAddItem6.Text, LblAddItem6Name.Text, WW_Dummy)
                TxtAddItem6.Focus()
            Case "TxtAddItem7"            '付帯項目７
                CODENAME_get("ADDITEM7", TxtAddItem7.Text, LblAddItem7Name.Text, WW_Dummy)
                TxtAddItem7.Focus()
            Case "TxtAddItem8"            '付帯項目８
                CODENAME_get("ADDITEM8", TxtAddItem8.Text, LblAddItem8Name.Text, WW_Dummy)
                TxtAddItem8.Focus()
            Case "TxtAddItem9"            '付帯項目９
                CODENAME_get("ADDITEM9", TxtAddItem9.Text, LblAddItem9Name.Text, WW_Dummy)
                TxtAddItem9.Focus()
            Case "TxtAddItem10"           '付帯項目１０
                CODENAME_get("ADDITEM10", TxtAddItem10.Text, LblAddItem10Name.Text, WW_Dummy)
                TxtAddItem10.Focus()
            Case "TxtAddItem11"           '付帯項目１１
                CODENAME_get("ADDITEM11", TxtAddItem11.Text, LblAddItem11Name.Text, WW_Dummy)
                TxtAddItem11.Focus()
            Case "TxtAddItem12"           '付帯項目１２
                CODENAME_get("ADDITEM12", TxtAddItem12.Text, LblAddItem12Name.Text, WW_Dummy)
                TxtAddItem12.Focus()
            Case "TxtAddItem13"           '付帯項目１３
                CODENAME_get("ADDITEM13", TxtAddItem13.Text, LblAddItem13Name.Text, WW_Dummy)
                TxtAddItem13.Focus()
            Case "TxtAddItem14"           '付帯項目１４
                CODENAME_get("ADDITEM14", TxtAddItem14.Text, LblAddItem14Name.Text, WW_Dummy)
                TxtAddItem14.Focus()
            Case "TxtAddItem15"           '付帯項目１５
                CODENAME_get("ADDITEM15", TxtAddItem15.Text, LblAddItem15Name.Text, WW_Dummy)
                TxtAddItem15.Focus()
            Case "TxtAddItem16"           '付帯項目１６
                CODENAME_get("ADDITEM16", TxtAddItem16.Text, LblAddItem16Name.Text, WW_Dummy)
                TxtAddItem16.Focus()
            Case "TxtAddItem17"           '付帯項目１７
                CODENAME_get("ADDITEM17", TxtAddItem17.Text, LblAddItem17Name.Text, WW_Dummy)
                TxtAddItem17.Focus()
            Case "TxtAddItem18"           '付帯項目１８
                CODENAME_get("ADDITEM18", TxtAddItem18.Text, LblAddItem18Name.Text, WW_Dummy)
                TxtAddItem18.Focus()
            Case "TxtAddItem19"           '付帯項目１９
                CODENAME_get("ADDITEM19", TxtAddItem19.Text, LblAddItem19Name.Text, WW_Dummy)
                TxtAddItem19.Focus()
            Case "TxtAddItem20"           '付帯項目２０
                CODENAME_get("ADDITEM20", TxtAddItem20.Text, LblAddItem20Name.Text, WW_Dummy)
                TxtAddItem20.Focus()
            Case "TxtAddItem21"           '付帯項目２１
                CODENAME_get("ADDITEM21", TxtAddItem21.Text, LblAddItem21Name.Text, WW_Dummy)
                TxtAddItem21.Focus()
            Case "TxtAddItem22"           '付帯項目２２
                CODENAME_get("ADDITEM22", TxtAddItem22.Text, LblAddItem22Name.Text, WW_Dummy)
                TxtAddItem22.Focus()
            Case "TxtAddItem23"           '付帯項目２３
                CODENAME_get("ADDITEM23", TxtAddItem23.Text, LblAddItem23Name.Text, WW_Dummy)
                TxtAddItem23.Focus()
            Case "TxtAddItem24"           '付帯項目２４
                CODENAME_get("ADDITEM24", TxtAddItem24.Text, LblAddItem24Name.Text, WW_Dummy)
                TxtAddItem24.Focus()
            Case "TxtAddItem25"           '付帯項目２５
                CODENAME_get("ADDITEM25", TxtAddItem25.Text, LblAddItem25Name.Text, WW_Dummy)
                TxtAddItem25.Focus()
            Case "TxtAddItem26"           '付帯項目２６
                CODENAME_get("ADDITEM26", TxtAddItem26.Text, LblAddItem26Name.Text, WW_Dummy)
                TxtAddItem26.Focus()
            Case "TxtAddItem27"           '付帯項目２７
                CODENAME_get("ADDITEM27", TxtAddItem27.Text, LblAddItem27Name.Text, WW_Dummy)
                TxtAddItem27.Focus()
            Case "TxtAddItem28"           '付帯項目２８
                CODENAME_get("ADDITEM28", TxtAddItem28.Text, LblAddItem28Name.Text, WW_Dummy)
                TxtAddItem28.Focus()
            Case "TxtAddItem29"           '付帯項目２９
                CODENAME_get("ADDITEM29", TxtAddItem29.Text, LblAddItem29Name.Text, WW_Dummy)
                TxtAddItem29.Focus()
            Case "TxtAddItem30"           '付帯項目３０
                CODENAME_get("ADDITEM30", TxtAddItem30.Text, LblAddItem30Name.Text, WW_Dummy)
                TxtAddItem30.Focus()
            Case "TxtAddItem31"           '付帯項目３１
                CODENAME_get("ADDITEM31", TxtAddItem31.Text, LblAddItem31Name.Text, WW_Dummy)
                TxtAddItem31.Focus()
            Case "TxtAddItem32"           '付帯項目３２
                CODENAME_get("ADDITEM32", TxtAddItem32.Text, LblAddItem32Name.Text, WW_Dummy)
                TxtAddItem32.Focus()
            Case "TxtAddItem33"           '付帯項目３３
                CODENAME_get("ADDITEM33", TxtAddItem33.Text, LblAddItem33Name.Text, WW_Dummy)
                TxtAddItem33.Focus()
            Case "TxtAddItem34"           '付帯項目３４
                CODENAME_get("ADDITEM34", TxtAddItem34.Text, LblAddItem34Name.Text, WW_Dummy)
                TxtAddItem34.Focus()
            Case "TxtAddItem35"           '付帯項目３５
                CODENAME_get("ADDITEM35", TxtAddItem35.Text, LblAddItem35Name.Text, WW_Dummy)
                TxtAddItem35.Focus()
            Case "TxtAddItem36"           '付帯項目３６
                CODENAME_get("ADDITEM36", TxtAddItem36.Text, LblAddItem36Name.Text, WW_Dummy)
                TxtAddItem36.Focus()
            Case "TxtAddItem37"           '付帯項目３７
                CODENAME_get("ADDITEM37", TxtAddItem37.Text, LblAddItem37Name.Text, WW_Dummy)
                TxtAddItem37.Focus()
            Case "TxtAddItem38"           '付帯項目３８
                CODENAME_get("ADDITEM38", TxtAddItem38.Text, LblAddItem38Name.Text, WW_Dummy)
                TxtAddItem38.Focus()
            Case "TxtAddItem39"           '付帯項目３９
                CODENAME_get("ADDITEM39", TxtAddItem39.Text, LblAddItem39Name.Text, WW_Dummy)
                TxtAddItem39.Focus()
            Case "TxtAddItem40"           '付帯項目４０
                CODENAME_get("ADDITEM40", TxtAddItem40.Text, LblAddItem40Name.Text, WW_Dummy)
                TxtAddItem40.Focus()
            Case "TxtAddItem41"           '付帯項目４１
                CODENAME_get("ADDITEM41", TxtAddItem41.Text, LblAddItem41Name.Text, WW_Dummy)
                TxtAddItem41.Focus()
            Case "TxtAddItem42"           '付帯項目４２
                CODENAME_get("ADDITEM42", TxtAddItem42.Text, LblAddItem42Name.Text, WW_Dummy)
                TxtAddItem42.Focus()
            Case "TxtAddItem43"           '付帯項目４３
                CODENAME_get("ADDITEM43", TxtAddItem43.Text, LblAddItem43Name.Text, WW_Dummy)
                TxtAddItem43.Focus()
            Case "TxtAddItem44"           '付帯項目４４
                CODENAME_get("ADDITEM44", TxtAddItem44.Text, LblAddItem44Name.Text, WW_Dummy)
                TxtAddItem44.Focus()
            Case "TxtAddItem45"           '付帯項目４５
                CODENAME_get("ADDITEM45", TxtAddItem45.Text, LblAddItem45Name.Text, WW_Dummy)
                TxtAddItem45.Focus()
            Case "TxtAddItem46"           '付帯項目４６
                CODENAME_get("ADDITEM46", TxtAddItem46.Text, LblAddItem46Name.Text, WW_Dummy)
                TxtAddItem46.Focus()
            Case "TxtAddItem47"           '付帯項目４７
                CODENAME_get("ADDITEM47", TxtAddItem47.Text, LblAddItem47Name.Text, WW_Dummy)
                TxtAddItem47.Focus()
            Case "TxtAddItem48"           '付帯項目４８
                CODENAME_get("ADDITEM48", TxtAddItem48.Text, LblAddItem48Name.Text, WW_Dummy)
                TxtAddItem48.Focus()
            Case "TxtAddItem49"           '付帯項目４９
                CODENAME_get("ADDITEM49", TxtAddItem49.Text, LblAddItem49Name.Text, WW_Dummy)
                TxtAddItem49.Focus()
            Case "TxtAddItem50"           '付帯項目５０
                CODENAME_get("ADDITEM50", TxtAddItem50.Text, LblAddItem50Name.Text, WW_Dummy)
                TxtAddItem50.Focus()
            Case "TxtFloorMaterial"       '床材質コード
                CODENAME_get("FLOORMATERIAL", TxtFloorMaterial.Text, LblFloorMaterialName.Text, WW_Dummy)
                TxtFloorMaterial.Focus()
            Case "TxtDelFlg"              '削除フラグ
                CODENAME_get("DELFLG", TxtDelFlg.Text, LblDelFlgName.Text, WW_Dummy)
                TxtDelFlg.Focus()

        End Select

        '○ メッセージ表示
        If Not isNormal(WW_RtnSW) Then
            Master.Output(WW_RtnSW, C_MESSAGE_TYPE.ERR)
        End If

    End Sub

    ''' <summary>
    ''' 中小分類リセット
    ''' </summary>
    ''' <param name="ClassFlg"></param>
    Protected Sub ReSetClassCd(ByVal ClassFlg As String)
        If ClassFlg = "1" Then
            TxtMiddleCTNCD.Text = ""
            LblMiddleCTNCDName.Text = ""
        End If
        TxtSmallCTNCD.Text = ""
        LblSmallCTNCDName.Text = ""

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
                Case "TxtDelFlg"              '削除フラグ
                    TxtDelFlg.Text = WW_SelectValue
                    LblDelFlgName.Text = WW_SelectText
                    TxtDelFlg.Focus()
                Case "TxtSpotStYMD"           'スポット区分　開始年月日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtSpotStYMD.Text = ""
                        Else
                            TxtSpotStYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtSpotStYMD.Focus()
                Case "TxtSpotEndYMD"          'スポット区分　終了年月日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtSpotEndYMD.Text = ""
                        Else
                            TxtSpotEndYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtSpotEndYMD.Focus()
                Case "TxtReginsHourMeterYMD"  '定期検査・ｱﾜﾒｰﾀ記載日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtReginsHourMeterYMD.Text = ""
                        Else
                            TxtReginsHourMeterYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtReginsHourMeterYMD.Focus()
                Case "TxtOperationStYMD"      '運用開始年月日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtOperationStYMD.Text = ""
                        Else
                            TxtOperationStYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtOperationStYMD.Focus()
                Case "TxtOperationEndYMD"     '運用除外年月日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtOperationEndYMD.Text = ""
                        Else
                            TxtOperationEndYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtOperationEndYMD.Focus()
                Case "TxtRetirmentYMD"        '除却年月日
                    Try
                        Date.TryParse(leftview.WF_Calendar.Text, WW_Date)
                        If WW_Date < C_DEFAULT_YMD Then
                            TxtRetirmentYMD.Text = ""
                        Else
                            TxtRetirmentYMD.Text = CDate(leftview.WF_Calendar.Text).ToString("yyyy/MM/dd")
                        End If
                    Catch ex As Exception
                    End Try
                    TxtRetirmentYMD.Focus()
                Case "TxtCTNType"             'コンテナ記号
                    TxtCTNType.Text = WW_SelectValue
                    LblCTNTypeName.Text = WW_SelectText
                    TxtCTNType.Focus()
                Case "TxtJurisdictionCD"      '所管部コード
                    TxtJurisdictionCD.Text = WW_SelectValue
                    LblJurisdictionCDName.Text = WW_SelectText
                    TxtJurisdictionCD.Focus()
                Case "TxtAccountingAsSetCD"   '経理資産コード
                    TxtAccountingAsSetCD.Text = WW_SelectValue
                    LblAccountingAsSetCDName.Text = WW_SelectText
                    TxtAccountingAsSetCD.Focus()
                Case "TxtAccountingAsSetKbn"  '経理資産区分
                    TxtAccountingAsSetKbn.Text = WW_SelectValue
                    LblAccountingAsSetKbnName.Text = WW_SelectText
                    TxtAccountingAsSetKbn.Focus()
                Case "TxtDummyKbn"            'ダミー区分
                    TxtDummyKbn.Text = WW_SelectValue
                    LblDummyKbnName.Text = WW_SelectText
                    TxtDummyKbn.Focus()
                Case "TxtSpotKbn"             'スポット区分
                    TxtSpotKbn.Text = WW_SelectValue
                    LblSpotKbnName.Text = WW_SelectText
                    TxtSpotKbn.Focus()
                Case "TxtBigCTNCD"            '大分類コード
                    TxtBigCTNCD.Text = WW_SelectValue
                    LblBigCTNCDName.Text = WW_SelectText
                    ReSetClassCd("1")
                    TxtBigCTNCD.Focus()
                Case "TxtMiddleCTNCD"         '中分類コード
                    TxtMiddleCTNCD.Text = WW_SelectValue
                    LblMiddleCTNCDName.Text = WW_SelectText
                    ReSetClassCd("")
                    TxtMiddleCTNCD.Focus()
                Case "TxtSmallCTNCD"          '小分類コード
                    TxtSmallCTNCD.Text = WW_SelectValue
                    LblSmallCTNCDName.Text = WW_SelectText
                    TxtSmallCTNCD.Focus()
                Case "TxtCTNMaker"            'コンテナメーカー
                    TxtCTNMaker.Text = WW_SelectValue
                    LblCTNMakerName.Text = WW_SelectText
                    TxtCTNMaker.Focus()
                Case "TxtFrozenMaker"         '冷凍機メーカー
                    TxtFrozenMaker.Text = WW_SelectValue
                    LblFrozenMakerName.Text = WW_SelectText
                    TxtFrozenMaker.Focus()
                Case "TxtCompKanKbn"          '複合一貫区分
                    TxtCompKanKbn.Text = WW_SelectValue
                    LblCompKanKbnName.Text = WW_SelectText
                    TxtCompKanKbn.Focus()
                Case "TxtSupplyFLG"          '調達フラグ
                    TxtSupplyFLG.Text = WW_SelectValue
                    LblSupplyFLGName.Text = WW_SelectText
                    TxtSupplyFLG.Focus()
                Case "TxtAddItem1"            '付帯項目１
                    TxtAddItem1.Text = WW_SelectValue
                    LblAddItem1Name.Text = WW_SelectText
                    TxtAddItem1.Focus()
                Case "TxtAddItem2"            '付帯項目２
                    TxtAddItem2.Text = WW_SelectValue
                    LblAddItem2Name.Text = WW_SelectText
                    TxtAddItem2.Focus()
                Case "TxtAddItem3"            '付帯項目３
                    TxtAddItem3.Text = WW_SelectValue
                    LblAddItem3Name.Text = WW_SelectText
                    TxtAddItem3.Focus()
                Case "TxtAddItem4"            '付帯項目４
                    TxtAddItem4.Text = WW_SelectValue
                    LblAddItem4Name.Text = WW_SelectText
                    TxtAddItem4.Focus()
                Case "TxtAddItem5"            '付帯項目５
                    TxtAddItem5.Text = WW_SelectValue
                    LblAddItem5Name.Text = WW_SelectText
                    TxtAddItem5.Focus()
                Case "TxtAddItem6"            '付帯項目６
                    TxtAddItem6.Text = WW_SelectValue
                    LblAddItem6Name.Text = WW_SelectText
                    TxtAddItem6.Focus()
                Case "TxtAddItem7"            '付帯項目７
                    TxtAddItem7.Text = WW_SelectValue
                    LblAddItem7Name.Text = WW_SelectText
                    TxtAddItem7.Focus()
                Case "TxtAddItem8"            '付帯項目８
                    TxtAddItem8.Text = WW_SelectValue
                    LblAddItem8Name.Text = WW_SelectText
                    TxtAddItem8.Focus()
                Case "TxtAddItem9"            '付帯項目９
                    TxtAddItem9.Text = WW_SelectValue
                    LblAddItem9Name.Text = WW_SelectText
                    TxtAddItem9.Focus()
                Case "TxtAddItem10"           '付帯項目１０
                    TxtAddItem10.Text = WW_SelectValue
                    LblAddItem10Name.Text = WW_SelectText
                    TxtAddItem10.Focus()
                Case "TxtAddItem11"           '付帯項目１１
                    TxtAddItem11.Text = WW_SelectValue
                    LblAddItem11Name.Text = WW_SelectText
                    TxtAddItem11.Focus()
                Case "TxtAddItem12"           '付帯項目１２
                    TxtAddItem12.Text = WW_SelectValue
                    LblAddItem12Name.Text = WW_SelectText
                    TxtAddItem12.Focus()
                Case "TxtAddItem13"           '付帯項目１３
                    TxtAddItem13.Text = WW_SelectValue
                    LblAddItem13Name.Text = WW_SelectText
                    TxtAddItem13.Focus()
                Case "TxtAddItem14"           '付帯項目１４
                    TxtAddItem14.Text = WW_SelectValue
                    LblAddItem14Name.Text = WW_SelectText
                    TxtAddItem14.Focus()
                Case "TxtAddItem15"           '付帯項目１５
                    TxtAddItem15.Text = WW_SelectValue
                    LblAddItem15Name.Text = WW_SelectText
                    TxtAddItem15.Focus()
                Case "TxtAddItem16"           '付帯項目１６
                    TxtAddItem16.Text = WW_SelectValue
                    LblAddItem16Name.Text = WW_SelectText
                    TxtAddItem16.Focus()
                Case "TxtAddItem17"           '付帯項目１７
                    TxtAddItem17.Text = WW_SelectValue
                    LblAddItem17Name.Text = WW_SelectText
                    TxtAddItem17.Focus()
                Case "TxtAddItem18"           '付帯項目１８
                    TxtAddItem18.Text = WW_SelectValue
                    LblAddItem18Name.Text = WW_SelectText
                    TxtAddItem18.Focus()
                Case "TxtAddItem19"           '付帯項目１９
                    TxtAddItem19.Text = WW_SelectValue
                    LblAddItem19Name.Text = WW_SelectText
                    TxtAddItem19.Focus()
                Case "TxtAddItem20"           '付帯項目２０
                    TxtAddItem20.Text = WW_SelectValue
                    LblAddItem20Name.Text = WW_SelectText
                    TxtAddItem20.Focus()
                Case "TxtAddItem21"           '付帯項目２１
                    TxtAddItem21.Text = WW_SelectValue
                    LblAddItem21Name.Text = WW_SelectText
                    TxtAddItem21.Focus()
                Case "TxtAddItem22"           '付帯項目２２
                    TxtAddItem22.Text = WW_SelectValue
                    LblAddItem22Name.Text = WW_SelectText
                    TxtAddItem22.Focus()
                Case "TxtAddItem23"           '付帯項目２３
                    TxtAddItem23.Text = WW_SelectValue
                    LblAddItem23Name.Text = WW_SelectText
                    TxtAddItem23.Focus()
                Case "TxtAddItem24"           '付帯項目２４
                    TxtAddItem24.Text = WW_SelectValue
                    LblAddItem24Name.Text = WW_SelectText
                    TxtAddItem24.Focus()
                Case "TxtAddItem25"           '付帯項目２５
                    TxtAddItem25.Text = WW_SelectValue
                    LblAddItem25Name.Text = WW_SelectText
                    TxtAddItem25.Focus()
                Case "TxtAddItem26"           '付帯項目２６
                    TxtAddItem26.Text = WW_SelectValue
                    LblAddItem26Name.Text = WW_SelectText
                    TxtAddItem26.Focus()
                Case "TxtAddItem27"           '付帯項目２７
                    TxtAddItem27.Text = WW_SelectValue
                    LblAddItem27Name.Text = WW_SelectText
                    TxtAddItem27.Focus()
                Case "TxtAddItem28"           '付帯項目２８
                    TxtAddItem28.Text = WW_SelectValue
                    LblAddItem28Name.Text = WW_SelectText
                    TxtAddItem28.Focus()
                Case "TxtAddItem29"           '付帯項目２９
                    TxtAddItem29.Text = WW_SelectValue
                    LblAddItem29Name.Text = WW_SelectText
                    TxtAddItem29.Focus()
                Case "TxtAddItem30"           '付帯項目３０
                    TxtAddItem30.Text = WW_SelectValue
                    LblAddItem30Name.Text = WW_SelectText
                    TxtAddItem30.Focus()
                Case "TxtAddItem31"           '付帯項目３１
                    TxtAddItem31.Text = WW_SelectValue
                    LblAddItem31Name.Text = WW_SelectText
                    TxtAddItem31.Focus()
                Case "TxtAddItem32"           '付帯項目３２
                    TxtAddItem32.Text = WW_SelectValue
                    LblAddItem32Name.Text = WW_SelectText
                    TxtAddItem32.Focus()
                Case "TxtAddItem33"           '付帯項目３３
                    TxtAddItem33.Text = WW_SelectValue
                    LblAddItem33Name.Text = WW_SelectText
                    TxtAddItem33.Focus()
                Case "TxtAddItem34"           '付帯項目３４
                    TxtAddItem34.Text = WW_SelectValue
                    LblAddItem34Name.Text = WW_SelectText
                    TxtAddItem34.Focus()
                Case "TxtAddItem35"           '付帯項目３５
                    TxtAddItem35.Text = WW_SelectValue
                    LblAddItem35Name.Text = WW_SelectText
                    TxtAddItem35.Focus()
                Case "TxtAddItem36"           '付帯項目３６
                    TxtAddItem36.Text = WW_SelectValue
                    LblAddItem36Name.Text = WW_SelectText
                    TxtAddItem36.Focus()
                Case "TxtAddItem37"           '付帯項目３７
                    TxtAddItem37.Text = WW_SelectValue
                    LblAddItem37Name.Text = WW_SelectText
                    TxtAddItem37.Focus()
                Case "TxtAddItem38"           '付帯項目３８
                    TxtAddItem38.Text = WW_SelectValue
                    LblAddItem38Name.Text = WW_SelectText
                    TxtAddItem38.Focus()
                Case "TxtAddItem39"           '付帯項目３９
                    TxtAddItem39.Text = WW_SelectValue
                    LblAddItem39Name.Text = WW_SelectText
                    TxtAddItem39.Focus()
                Case "TxtAddItem40"           '付帯項目４０
                    TxtAddItem40.Text = WW_SelectValue
                    LblAddItem40Name.Text = WW_SelectText
                    TxtAddItem40.Focus()
                Case "TxtAddItem41"           '付帯項目４１
                    TxtAddItem41.Text = WW_SelectValue
                    LblAddItem41Name.Text = WW_SelectText
                    TxtAddItem41.Focus()
                Case "TxtAddItem42"           '付帯項目４２
                    TxtAddItem42.Text = WW_SelectValue
                    LblAddItem42Name.Text = WW_SelectText
                    TxtAddItem42.Focus()
                Case "TxtAddItem43"           '付帯項目４３
                    TxtAddItem43.Text = WW_SelectValue
                    LblAddItem43Name.Text = WW_SelectText
                    TxtAddItem43.Focus()
                Case "TxtAddItem44"           '付帯項目４４
                    TxtAddItem44.Text = WW_SelectValue
                    LblAddItem44Name.Text = WW_SelectText
                    TxtAddItem44.Focus()
                Case "TxtAddItem45"           '付帯項目４５
                    TxtAddItem45.Text = WW_SelectValue
                    LblAddItem45Name.Text = WW_SelectText
                    TxtAddItem45.Focus()
                Case "TxtAddItem46"           '付帯項目４６
                    TxtAddItem46.Text = WW_SelectValue
                    LblAddItem46Name.Text = WW_SelectText
                    TxtAddItem46.Focus()
                Case "TxtAddItem47"           '付帯項目４７
                    TxtAddItem47.Text = WW_SelectValue
                    LblAddItem47Name.Text = WW_SelectText
                    TxtAddItem47.Focus()
                Case "TxtAddItem48"           '付帯項目４８
                    TxtAddItem48.Text = WW_SelectValue
                    LblAddItem48Name.Text = WW_SelectText
                    TxtAddItem48.Focus()
                Case "TxtAddItem49"           '付帯項目４９
                    TxtAddItem49.Text = WW_SelectValue
                    LblAddItem49Name.Text = WW_SelectText
                    TxtAddItem49.Focus()
                Case "TxtAddItem50"           '付帯項目５０
                    TxtAddItem50.Text = WW_SelectValue
                    LblAddItem50Name.Text = WW_SelectText
                    TxtAddItem50.Focus()
                Case "TxtFloorMaterial"       '床材質コード
                    TxtFloorMaterial.Text = WW_SelectValue
                    LblFloorMaterialName.Text = WW_SelectText
                    TxtFloorMaterial.Focus()
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
                Case "TxtDelFlg"              '削除フラグ
                    TxtDelFlg.Focus()
                Case "TxtCTNType"             'コンテナ記号
                    TxtCTNType.Focus()
                Case "TxtJurisdictionCD"      '所管部コード
                    TxtJurisdictionCD.Focus()
                Case "TxtAccountingAsSetCD"   '経理資産コード
                    TxtAccountingAsSetCD.Focus()
                Case "TxtAccountingAsSetKbn"  '経理資産区分
                    TxtAccountingAsSetKbn.Focus()
                Case "TxtDummyKbn"            'ダミー区分
                    TxtDummyKbn.Focus()
                Case "TxtSpotKbn"             'スポット区分
                    TxtSpotKbn.Focus()
                Case "TxtSpotStYMD"           'スポット区分　開始年月日
                    TxtSpotStYMD.Focus()
                Case "TxtSpotEndYMD"          'スポット区分　終了年月日
                    TxtSpotEndYMD.Focus()
                Case "TxtBigCTNCD"            '大分類コード
                    TxtBigCTNCD.Focus()
                Case "TxtMiddleCTNCD"         '中分類コード
                    TxtMiddleCTNCD.Focus()
                Case "TxtSmallCTNCD"          '小分類コード
                    TxtSmallCTNCD.Focus()
                Case "TxtCTNMaker"            'コンテナメーカー
                    TxtCTNMaker.Focus()
                Case "TxtFrozenMaker"         '冷凍機メーカー
                    TxtFrozenMaker.Focus()
                Case "TxtReginsHourMeterYMD"  '定期検査・ｱﾜﾒｰﾀ記載日
                    TxtReginsHourMeterYMD.Focus()
                Case "TxtOperationStYMD"      '運用開始年月日
                    TxtOperationStYMD.Focus()
                Case "TxtOperationEndYMD"     '運用除外年月日
                    TxtOperationEndYMD.Focus()
                Case "TxtRetirmentYMD"        '除却年月日
                    TxtRetirmentYMD.Focus()
                Case "TxtCompKanKbn"          '複合一貫区分
                    TxtCompKanKbn.Focus()
                Case "TxtSupplyFLG"           '調達フラグ
                    TxtSupplyFLG.Focus()
                Case "TxtAddItem1"            '付帯項目１
                    TxtAddItem1.Focus()
                Case "TxtAddItem2"            '付帯項目２
                    TxtAddItem2.Focus()
                Case "TxtAddItem3"            '付帯項目３
                    TxtAddItem3.Focus()
                Case "TxtAddItem4"            '付帯項目４
                    TxtAddItem4.Focus()
                Case "TxtAddItem5"            '付帯項目５
                    TxtAddItem5.Focus()
                Case "TxtAddItem6"            '付帯項目６
                    TxtAddItem6.Focus()
                Case "TxtAddItem7"            '付帯項目７
                    TxtAddItem7.Focus()
                Case "TxtAddItem8"            '付帯項目８
                    TxtAddItem8.Focus()
                Case "TxtAddItem9"            '付帯項目９
                    TxtAddItem9.Focus()
                Case "TxtAddItem10"           '付帯項目１０
                    TxtAddItem10.Focus()
                Case "TxtAddItem11"           '付帯項目１１
                    TxtAddItem11.Focus()
                Case "TxtAddItem12"           '付帯項目１２
                    TxtAddItem12.Focus()
                Case "TxtAddItem13"           '付帯項目１３
                    TxtAddItem13.Focus()
                Case "TxtAddItem14"           '付帯項目１４
                    TxtAddItem14.Focus()
                Case "TxtAddItem15"           '付帯項目１５
                    TxtAddItem15.Focus()
                Case "TxtAddItem16"           '付帯項目１６
                    TxtAddItem16.Focus()
                Case "TxtAddItem17"           '付帯項目１７
                    TxtAddItem17.Focus()
                Case "TxtAddItem18"           '付帯項目１８
                    TxtAddItem18.Focus()
                Case "TxtAddItem19"           '付帯項目１９
                    TxtAddItem19.Focus()
                Case "TxtAddItem20"           '付帯項目２０
                    TxtAddItem20.Focus()
                Case "TxtAddItem21"           '付帯項目２１
                    TxtAddItem21.Focus()
                Case "TxtAddItem22"           '付帯項目２２
                    TxtAddItem22.Focus()
                Case "TxtAddItem23"           '付帯項目２３
                    TxtAddItem23.Focus()
                Case "TxtAddItem24"           '付帯項目２４
                    TxtAddItem24.Focus()
                Case "TxtAddItem25"           '付帯項目２５
                    TxtAddItem25.Focus()
                Case "TxtAddItem26"           '付帯項目２６
                    TxtAddItem26.Focus()
                Case "TxtAddItem27"           '付帯項目２７
                    TxtAddItem27.Focus()
                Case "TxtAddItem28"           '付帯項目２８
                    TxtAddItem28.Focus()
                Case "TxtAddItem29"           '付帯項目２９
                    TxtAddItem29.Focus()
                Case "TxtAddItem30"           '付帯項目３０
                    TxtAddItem30.Focus()
                Case "TxtAddItem31"           '付帯項目３１
                    TxtAddItem31.Focus()
                Case "TxtAddItem32"           '付帯項目３２
                    TxtAddItem32.Focus()
                Case "TxtAddItem33"           '付帯項目３３
                    TxtAddItem33.Focus()
                Case "TxtAddItem34"           '付帯項目３４
                    TxtAddItem34.Focus()
                Case "TxtAddItem35"           '付帯項目３５
                    TxtAddItem35.Focus()
                Case "TxtAddItem36"           '付帯項目３６
                    TxtAddItem36.Focus()
                Case "TxtAddItem37"           '付帯項目３７
                    TxtAddItem37.Focus()
                Case "TxtAddItem38"           '付帯項目３８
                    TxtAddItem38.Focus()
                Case "TxtAddItem39"           '付帯項目３９
                    TxtAddItem39.Focus()
                Case "TxtAddItem40"           '付帯項目４０
                    TxtAddItem40.Focus()
                Case "TxtAddItem41"           '付帯項目４１
                    TxtAddItem41.Focus()
                Case "TxtAddItem42"           '付帯項目４２
                    TxtAddItem42.Focus()
                Case "TxtAddItem43"           '付帯項目４３
                    TxtAddItem43.Focus()
                Case "TxtAddItem44"           '付帯項目４４
                    TxtAddItem44.Focus()
                Case "TxtAddItem45"           '付帯項目４５
                    TxtAddItem45.Focus()
                Case "TxtAddItem46"           '付帯項目４６
                    TxtAddItem46.Focus()
                Case "TxtAddItem47"           '付帯項目４７
                    TxtAddItem47.Focus()
                Case "TxtAddItem48"           '付帯項目４８
                    TxtAddItem48.Focus()
                Case "TxtAddItem49"           '付帯項目４９
                    TxtAddItem49.Focus()
                Case "TxtAddItem50"           '付帯項目５０
                    TxtAddItem50.Focus()
                Case "TxtFloorMaterial"       '床材質コード
                    TxtFloorMaterial.Focus()
            End Select
        End If

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
        Dim WW_CS0024FCheckerr As String = ""
        Dim WW_CS0024FCheckReport As String = ""
        Dim WW_DBDataCheck As String = ""
        Dim WW_ConstructionYMD As String = ""

        '○ 画面操作権限チェック
        ' 権限チェック(操作者に更新権限があるかチェック
        CS0025AUTHORget.USERID = CS0050SESSION.USERID
        CS0025AUTHORget.OBJCODE = C_ROLE_VARIANT.USER_PERTMIT
        CS0025AUTHORget.CODE = Master.MAPID
        CS0025AUTHORget.STYMD = Date.Now
        CS0025AUTHORget.ENDYMD = Date.Now
        CS0025AUTHORget.CS0025AUTHORget()
        If Not isNormal(CS0025AUTHORget.ERR) OrElse CS0025AUTHORget.PERMITCODE <> C_PERMISSION.UPDATE Then
            WW_CheckMES1 = "・コンテナマスタ更新権限なし"
            WW_CheckMES2 = ""
            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
            WW_LineErr = "ERR"
            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Exit Sub
        End If

        '○ 単項目チェック
        For Each LNM0002INProw As DataRow In LNM0002INPtbl.Rows

            WW_LineErr = ""

            ' 削除フラグ(バリデーションチェック）
            Master.CheckField(Master.USERCAMP, "DELFLG", LNM0002INProw("DELFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("DELFLG", LNM0002INProw("DELFLG"), WW_Dummy, WW_RtnSW)
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
            ' コンテナ記号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CTNTYPE", LNM0002INProw("CTNTYPE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・コンテナ記号エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' コンテナ番号(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CTNNO", LNM0002INProw("CTNNO"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・コンテナ番号エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 所管部コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "JURISDICTIONCD", LNM0002INProw("JURISDICTIONCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("JURISDICTION", LNM0002INProw("JURISDICTIONCD"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・所管部コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・所管部コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 経理資産コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ACCOUNTINGASSETSCD", LNM0002INProw("ACCOUNTINGASSETSCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ACCOUNTINGASSETSCD")) Then
                    ' 名称存在チェック
                    CODENAME_get("ACCOUNTINGASSETSCD", LNM0002INProw("ACCOUNTINGASSETSCD"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・経理資産コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・経理資産コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 経理資産区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ACCOUNTINGASSETSKBN", LNM0002INProw("ACCOUNTINGASSETSKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("ACCOUNTINGASSETSKBN", LNM0002INProw("ACCOUNTINGASSETSKBN"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・経理資産区分エラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・経理資産区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' ダミー区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "DUMMYKBN", LNM0002INProw("DUMMYKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("DUMMYKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("DUMMYKBN", LNM0002INProw("DUMMYKBN"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・ダミー区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・ダミー区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' スポット区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPOTKBN", LNM0002INProw("SPOTKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("SPOTKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("SPOTKBN", LNM0002INProw("SPOTKBN"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・スポット区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・スポット区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 入力値チェック(経理資産区分・スポット区分)
            If LNM0002INProw("ACCOUNTINGASSETSKBN") = WW_Kbn01 AndAlso LNM0002INProw("SPOTKBN") = WW_Kbn02 OrElse
                 LNM0002INProw("ACCOUNTINGASSETSKBN") = WW_Kbn02 AndAlso LNM0002INProw("SPOTKBN") = WW_Kbn01 Then
                WW_CheckMES1 = "・経理資産区分＆スポット区分エラー"
                WW_CheckMES2 = "同じ区分は入力出来ません。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            Dim blnSpotStart As Boolean = False
            Dim blnSpotEnd As Boolean = False
            ' スポット区分　開始年月日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPOTSTYMD", LNM0002INProw("SPOTSTYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' スポット区分入力時、必須チェック
                If Not String.IsNullOrEmpty(LNM0002INProw("SPOTKBN")) AndAlso LNM0002INProw("SPOTKBN") <> "00" Then
                    If String.IsNullOrEmpty(LNM0002INProw("SPOTSTYMD")) Then
                        WW_CheckMES1 = "・スポット区分　開始年月日エラー"
                        WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        blnSpotStart = True
                    End If
                Else
                    If Not String.IsNullOrEmpty(LNM0002INProw("SPOTSTYMD")) Then
                        LNM0002INProw("SPOTSTYMD") = CDate(LNM0002INProw("SPOTSTYMD")).ToString("yyyy/MM/dd")
                    End If
                    blnSpotStart = True
                End If
            Else
                WW_CheckMES1 = "・スポット区分　開始年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' スポット区分　終了年月日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SPOTENDYMD", LNM0002INProw("SPOTENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' スポット区分入力時、必須チェック
                If Not String.IsNullOrEmpty(LNM0002INProw("SPOTKBN")) AndAlso LNM0002INProw("SPOTKBN") <> "00" Then
                    If String.IsNullOrEmpty(LNM0002INProw("SPOTENDYMD")) Then
                        WW_CheckMES1 = "・スポット区分　終了年月日エラー"
                        WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    Else
                        blnSpotEnd = True
                    End If
                Else
                    If Not String.IsNullOrEmpty(LNM0002INProw("SPOTENDYMD")) Then
                        ' 過去日入力チェック
                        If Date.Now > CDate(LNM0002INProw("SPOTENDYMD")) And LNM0002INProw("SPOTENDYMD") <> work.WF_SEL_SPOTENDYMD.Text Then
                            WW_CheckMES1 = "・スポット区分　終了年月日エラー"
                            WW_CheckMES2 = "過去日入力エラー"
                            WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                            WW_LineErr = "ERR"
                            O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                        Else
                            LNM0002INProw("SPOTENDYMD") = CDate(LNM0002INProw("SPOTENDYMD")).ToString("yyyy/MM/dd")
                            blnSpotEnd = True
                        End If
                    End If
                End If
            Else
                WW_CheckMES1 = "・スポット区分　終了年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            If blnSpotStart = True AndAlso blnSpotEnd = True Then
                ' 日付大小チェック(スポット区分　開始年月日・スポット区分　終了年月日)
                If Not String.IsNullOrEmpty(LNM0002INProw("SPOTSTYMD")) AndAlso Not String.IsNullOrEmpty(LNM0002INProw("SPOTENDYMD")) Then
                    If CDate(LNM0002INProw("SPOTSTYMD")) > CDate(LNM0002INProw("SPOTENDYMD")) Then
                        WW_CheckMES1 = "・スポット区分　開始年月日＆スポット区分　終了年月日エラー"
                        WW_CheckMES2 = "日付大小入力エラー"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            End If

            ' 大分類コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BIGCTNCD", LNM0002INProw("BIGCTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("BIGCTNCD", LNM0002INProw("BIGCTNCD"), WW_Dummy, WW_RtnSW)
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
            Master.CheckField(Master.USERCAMP, "MIDDLECTNCD", LNM0002INProw("MIDDLECTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("MIDDLECTNCD", LNM0002INProw("MIDDLECTNCD"), WW_Dummy, WW_RtnSW)
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
            ' 小分類コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SMALLCTNCD", LNM0002INProw("SMALLCTNCD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("SMALLCTNCD", LNM0002INProw("SMALLCTNCD"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・小分類コードエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・小分類コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            Dim blnCONSTRUCTIONYM As Boolean = False
            ' 建造年月(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CONSTRUCTIONYM", LNM0002INProw("CONSTRUCTIONYM").ToString, WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 建造年月を年月日(YYYY/MM/DD)に変更(月初指定)
                If LNM0002INProw("CONSTRUCTIONYM").ToString.Length = 6 Then
                    Dim strDateYmd As String = LNM0002INProw("CONSTRUCTIONYM").ToString
                    strDateYmd = Left(strDateYmd, 4) & "/" & Right(strDateYmd, 2) & "/01"
                    Dim dt As DateTime
                    If DateTime.TryParse(strDateYmd, dt) Then
                        '変換出来たら、OK
                        WW_ConstructionYMD = strDateYmd
                        blnCONSTRUCTIONYM = True
                    Else
                        WW_CheckMES1 = "・建造年月エラーです。"
                        WW_CheckMES2 = "入力値が不正です。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                Else
                    WW_CheckMES1 = "・建造年月エラーです。"
                    WW_CheckMES2 = "入力値が不正です。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・建造年月エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' コンテナメーカー(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "CTNMAKER", LNM0002INProw("CTNMAKER"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                ' 名称存在チェック
                CODENAME_get("CTNMAKER", LNM0002INProw("CTNMAKER"), WW_Dummy, WW_RtnSW)
                If Not isNormal(WW_RtnSW) Then
                    WW_CheckMES1 = "・コンテナメーカーエラーです。"
                    WW_CheckMES2 = "マスタに存在しません。"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                End If
            Else
                WW_CheckMES1 = "・コンテナメーカーエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 冷凍機メーカー(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "FROZENMAKER", LNM0002INProw("FROZENMAKER"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("FROZENMAKER")) Then
                    ' 名称存在チェック
                    CODENAME_get("FROZENMAKER", LNM0002INProw("FROZENMAKER"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・冷凍機メーカーエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・冷凍機メーカーエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 総重量(バリデーションチェック)
            Dim blnGROSSWEIGHTErr As Boolean = False
            Master.CheckField(Master.USERCAMP, "GROSSWEIGHT", LNM0002INProw("GROSSWEIGHT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・総重量エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                blnGROSSWEIGHTErr = True
            ElseIf String.IsNullOrEmpty(LNM0002INProw("GROSSWEIGHT")) OrElse CDbl(LNM0002INProw("GROSSWEIGHT")) = 0 Then
                WW_CheckMES1 = "・総重量エラー"
                WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT & "(" & LNM0002INProw("GROSSWEIGHT") & ")"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                blnGROSSWEIGHTErr = True
            End If
            ' 荷重(バリデーションチェック)
            Dim blnCARGOWEIGHTErr As Boolean = False
            Master.CheckField(Master.USERCAMP, "CARGOWEIGHT", LNM0002INProw("CARGOWEIGHT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・荷重エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                blnCARGOWEIGHTErr = True
            ElseIf String.IsNullOrEmpty(LNM0002INProw("CARGOWEIGHT")) OrElse CDbl(LNM0002INProw("CARGOWEIGHT")) = 0 Then
                WW_CheckMES1 = "・荷重エラー"
                WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT & "(" & LNM0002INProw("CARGOWEIGHT") & ")"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                blnCARGOWEIGHTErr = True
            End If
            ' 自重(バリデーションチェック)
            Dim blnMYWEIGHTErr As Boolean = False
            Master.CheckField(Master.USERCAMP, "MYWEIGHT", LNM0002INProw("MYWEIGHT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・自重エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                blnMYWEIGHTErr = True
            ElseIf String.IsNullOrEmpty(LNM0002INProw("MYWEIGHT")) OrElse CDbl(LNM0002INProw("MYWEIGHT")) = 0 Then
                WW_CheckMES1 = "・自重エラー"
                WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT & "(" & LNM0002INProw("MYWEIGHT") & ")"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                blnMYWEIGHTErr = True
            End If
            ' 簿価商品価格(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "BOOKVALUE", LNM0002INProw("BOOKVALUE"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・簿価商品価格エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                'ElseIf String.IsNullOrEmpty(LNM0002INProw("BOOKVALUE")) OrElse CInt(LNM0002INProw("BOOKVALUE")) = 0 Then
                '    WW_CheckMES1 = "・簿価商品価格エラー"
                '    WW_CheckMES2 = C_MESSAGE_TEXT.PREREQUISITE_ERROR_TEXT & "(" & LNM0002INProw("BOOKVALUE") & ")"
                '    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                '    WW_LineErr = "ERR"
                '    O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 重量大小チェック(荷重・自重)
            If blnCARGOWEIGHTErr = False AndAlso blnMYWEIGHTErr = False Then
                If Not String.IsNullOrEmpty(LNM0002INProw("CARGOWEIGHT")) AndAlso Not String.IsNullOrEmpty(LNM0002INProw("MYWEIGHT")) Then
                    If CDbl(LNM0002INProw("CARGOWEIGHT")) < CDbl(LNM0002INProw("MYWEIGHT")) Then
                        WW_CheckMES1 = "・荷重＆自重エラー"
                        WW_CheckMES2 = "重量大小入力エラー"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            End If
            ' 重量大小チェック(総重量・荷重・自重)
            If blnGROSSWEIGHTErr = False AndAlso blnCARGOWEIGHTErr = False AndAlso blnMYWEIGHTErr = False Then
                If Not String.IsNullOrEmpty(LNM0002INProw("GROSSWEIGHT")) AndAlso
                    Not String.IsNullOrEmpty(LNM0002INProw("CARGOWEIGHT")) AndAlso
                    Not String.IsNullOrEmpty(LNM0002INProw("MYWEIGHT")) Then
                    If CDbl(LNM0002INProw("GROSSWEIGHT")) < (CDbl(LNM0002INProw("CARGOWEIGHT")) + CDbl(LNM0002INProw("MYWEIGHT"))) Then
                        WW_CheckMES1 = "・総重量＆荷重＋自重エラー"
                        WW_CheckMES2 = "重量大小入力エラー"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            End If
            ' 外寸・高さ(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "OUTHEIGHT", LNM0002INProw("OUTHEIGHT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・外寸・高さエラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 外寸・幅(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "OUTWIDTH", LNM0002INProw("OUTWIDTH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・外寸・幅エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 外寸・長さ(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "OUTLENGTH", LNM0002INProw("OUTLENGTH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・外寸・長さエラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 内寸・高さ(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "INHEIGHT", LNM0002INProw("INHEIGHT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・内寸・高さエラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 内寸・幅(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "INWIDTH", LNM0002INProw("INWIDTH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・内寸・幅エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 内寸・長さ(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "INLENGTH", LNM0002INProw("INLENGTH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・内寸・長さエラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 妻入口・高さ(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "WIFEHEIGHT", LNM0002INProw("WIFEHEIGHT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・妻入口・高さエラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 妻入口・幅(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "WIFEWIDTH", LNM0002INProw("WIFEWIDTH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・妻入口・幅エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 側入口・高さ(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SIDEHEIGHT", LNM0002INProw("SIDEHEIGHT"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・側入口・高さエラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 側入口・幅(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SIDEWIDTH", LNM0002INProw("SIDEWIDTH"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・側入口・幅エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 床面積(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "FLOORAREA", LNM0002INProw("FLOORAREA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・床面積エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 内容積・標記(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "INVOLUMEMARKING", LNM0002INProw("INVOLUMEMARKING"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・内容積・標記エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 内容積・実寸(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "INVOLUMEACTUA", LNM0002INProw("INVOLUMEACTUA"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・内容積・実寸エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 交番検査・ｻｲｸﾙ日数(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "TRAINSCYCLEDAYS", LNM0002INProw("TRAINSCYCLEDAYS"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・交番検査・ｻｲｸﾙ日数エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 定期検査・ｻｲｸﾙ月数(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "REGINSCYCLEDAYS", LNM0002INProw("REGINSCYCLEDAYS"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・定期検査・ｻｲｸﾙ月数エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 定期検査・ｻｲｸﾙｱﾜﾒｰﾀ(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "REGINSCYCLEHOURMETER", LNM0002INProw("REGINSCYCLEHOURMETER"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・定期検査・ｻｲｸﾙｱﾜﾒｰﾀエラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 大分類コード＝"15"(冷蔵)時、入力値チェック(定期検査・ｻｲｸﾙ月数・定期検査・ｻｲｸﾙｱﾜﾒｰﾀ)
            If LNM0002INProw("REGINSCYCLEHOURMETER") = "15" AndAlso
                    String.IsNullOrEmpty(LNM0002INProw("REGINSCYCLEDAYS")) AndAlso
                    String.IsNullOrEmpty(LNM0002INProw("REGINSCYCLEHOURMETER")) Then
                WW_CheckMES1 = "・定期検査・ｻｲｸﾙ月数＆定期検査・ｻｲｸﾙｱﾜﾒｰﾀエラー"
                WW_CheckMES2 = "どちらかを入力してください。"
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 定期検査・ｱﾜﾒｰﾀ記載日(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "REGINSHOURMETERYMD", LNM0002INProw("REGINSHOURMETERYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・定期検査・ｱﾜﾒｰﾀ記載日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 定期検査・ｱﾜﾒｰﾀ時間(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "REGINSHOURMETERTIME", LNM0002INProw("REGINSHOURMETERTIME"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・定期検査・ｱﾜﾒｰﾀ時間エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 定期検査・ｱﾜﾒｰﾀ表示桁(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "REGINSHOURMETERDSP", LNM0002INProw("REGINSHOURMETERDSP"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If Not isNormal(WW_CS0024FCheckerr) Then
                WW_CheckMES1 = "・定期検査・ｱﾜﾒｰﾀ表示桁エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            Else
                ' 入力値チェック
                If LNM0002INProw("REGINSHOURMETERDSP") = "0" OrElse
                    String.IsNullOrEmpty(LNM0002INProw("REGINSHOURMETERDSP")) Then
                    LNM0002INProw("REGINSHOURMETERDSP") = WW_DefaultReginsHourMeterDsp
                End If
            End If
            ' 運用開始年月日(バリデーションチェック)
            Dim blnOperationStart As Boolean = False
            Master.CheckField(Master.USERCAMP, "OPERATIONSTYMD", LNM0002INProw("OPERATIONSTYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("OPERATIONSTYMD")) Then
                    LNM0002INProw("OPERATIONSTYMD") = CDate(LNM0002INProw("OPERATIONSTYMD")).ToString("yyyy/MM/dd")
                End If
                blnOperationStart = True
            Else
                WW_CheckMES1 = "・運用開始年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 日付大小チェック(建造年月(01日)・運用開始年月日)
            If Not String.IsNullOrEmpty(LNM0002INProw("CONSTRUCTIONYM")) AndAlso Not String.IsNullOrEmpty(LNM0002INProw("OPERATIONSTYMD")) Then
                If blnCONSTRUCTIONYM = True AndAlso blnOperationStart = True Then
                    If CDate(WW_ConstructionYMD) > CDate(LNM0002INProw("OPERATIONSTYMD")) Then
                        WW_CheckMES1 = "・建造年月＆運用開始年月日エラー"
                        WW_CheckMES2 = "日付大小入力エラー"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            End If
            ' 運用除外年月日(バリデーションチェック)
            Dim blnOperationEnd As Boolean = False
            Master.CheckField(Master.USERCAMP, "OPERATIONENDYMD", LNM0002INProw("OPERATIONENDYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("OPERATIONENDYMD")) Then
                    LNM0002INProw("OPERATIONENDYMD") = CDate(LNM0002INProw("OPERATIONENDYMD")).ToString("yyyy/MM/dd")
                End If
                blnOperationEnd = True
            Else
                WW_CheckMES1 = "・運用除外年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 日付大小チェック(運用開始年月日・運用除外年月日)
            If Not String.IsNullOrEmpty(LNM0002INProw("OPERATIONSTYMD")) AndAlso Not String.IsNullOrEmpty(LNM0002INProw("OPERATIONENDYMD")) Then
                If blnOperationStart = True AndAlso blnOperationEnd = True Then
                    If CDate(LNM0002INProw("OPERATIONSTYMD")) > CDate(LNM0002INProw("OPERATIONENDYMD")) Then
                        WW_CheckMES1 = "・運用開始年月日＆運用除外年月日エラー"
                        WW_CheckMES2 = "日付大小入力エラー"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            End If
            ' 除却年月日(バリデーションチェック)
            Dim blnRetirment As Boolean = False
            Master.CheckField(Master.USERCAMP, "RETIRMENTYMD", LNM0002INProw("RETIRMENTYMD"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("RETIRMENTYMD")) Then
                    LNM0002INProw("RETIRMENTYMD") = CDate(LNM0002INProw("RETIRMENTYMD")).ToString("yyyy/MM/dd")
                End If
                blnRetirment = True
            Else
                WW_CheckMES1 = "・除却年月日エラー"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 日付大小チェック(運用除外年月日・除却年月日)
            If Not String.IsNullOrEmpty(LNM0002INProw("OPERATIONENDYMD")) AndAlso Not String.IsNullOrEmpty(LNM0002INProw("RETIRMENTYMD")) Then
                If blnOperationEnd = True AndAlso blnRetirment = True Then
                    If CDate(LNM0002INProw("OPERATIONENDYMD")) > CDate(LNM0002INProw("RETIRMENTYMD")) Then
                        WW_CheckMES1 = "・運用除外年月日＆除却年月日エラー"
                        WW_CheckMES2 = "日付大小入力エラー"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            End If
            ' 複合一貫区分(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "COMPKANKBN", LNM0002INProw("COMPKANKBN"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("COMPKANKBN")) Then
                    ' 名称存在チェック
                    CODENAME_get("COMPKANKBN", LNM0002INProw("COMPKANKBN"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・複合一貫区分エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・複合一貫区分エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 調達フラグ(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "SUPPLYFLG", LNM0002INProw("SUPPLYFLG"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("SUPPLYFLG")) Then
                    ' 名称存在チェック
                    CODENAME_get("SUPPLYFLG", LNM0002INProw("SUPPLYFLG"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・調達フラグエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・調達フラグエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目１(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM1", LNM0002INProw("ADDITEM1"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM1")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM1", LNM0002INProw("ADDITEM1"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目１(使用禁止)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目１(使用禁止)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目２(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM2", LNM0002INProw("ADDITEM2"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM2")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM2", LNM0002INProw("ADDITEM2"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目２(優先臨時表示)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目２(優先臨時表示)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目３(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM3", LNM0002INProw("ADDITEM3"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM3")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM3", LNM0002INProw("ADDITEM3"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目３(エンジンオーバーホー)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目３(エンジンオーバーホー)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目４(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM4", LNM0002INProw("ADDITEM4"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM4")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM4", LNM0002INProw("ADDITEM4"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目４(重点整備対象)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目４(重点整備対象)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目５(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM5", LNM0002INProw("ADDITEM5"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM5")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM5", LNM0002INProw("ADDITEM5"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目５(青函アンテナ交換対象)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目５(青函アンテナ交換対象)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目６(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM6", LNM0002INProw("ADDITEM6"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM6")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM6", LNM0002INProw("ADDITEM6"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目６(管外回送禁止)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目６(管外回送禁止)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目７(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM7", LNM0002INProw("ADDITEM7"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM7")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM7", LNM0002INProw("ADDITEM7"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目７(再塗装未実施)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目７(再塗装未実施)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目８(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM8", LNM0002INProw("ADDITEM8"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM8")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM8", LNM0002INProw("ADDITEM8"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目８(濡損防止対策未実施)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目８(濡損防止対策未実施)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目９(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM9", LNM0002INProw("ADDITEM9"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM9")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM9", LNM0002INProw("ADDITEM9"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目９エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目９エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目１０(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM10", LNM0002INProw("ADDITEM10"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM10")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM10", LNM0002INProw("ADDITEM10"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目１０(基本表示)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目１０(基本表示)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目１１(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM11", LNM0002INProw("ADDITEM11"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM11")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM11", LNM0002INProw("ADDITEM11"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目１１(色・標記)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目１１(色・標記)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目１２(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM12", LNM0002INProw("ADDITEM12"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM12")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM12", LNM0002INProw("ADDITEM12"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目１２(扉配置)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目１２(扉配置)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目１３(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM13", LNM0002INProw("ADDITEM13"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM13")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM13", LNM0002INProw("ADDITEM13"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目１３(フォークポケット)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目１３(フォークポケット)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目１４(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM14", LNM0002INProw("ADDITEM14"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM14")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM14", LNM0002INProw("ADDITEM14"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目１４(隅金具)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目１４(隅金具)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目１５(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM15", LNM0002INProw("ADDITEM15"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM15")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM15", LNM0002INProw("ADDITEM15"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目１５(ラッシングリング)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目１５(ラッシングリング)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目１６(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM16", LNM0002INProw("ADDITEM16"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM16")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM16", LNM0002INProw("ADDITEM16"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目１６(ジョロダーレール)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目１６(ジョロダーレール)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目１７(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM17", LNM0002INProw("ADDITEM17"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM17")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM17", LNM0002INProw("ADDITEM17"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目１７(側扉ヒンジ)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目１７(側扉ヒンジ)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目１８(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM18", LNM0002INProw("ADDITEM18"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM18")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM18", LNM0002INProw("ADDITEM18"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目１８エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目１８エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目１９(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM19", LNM0002INProw("ADDITEM19"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM19")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM19", LNM0002INProw("ADDITEM19"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目１９エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目１９エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目２０(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM20", LNM0002INProw("ADDITEM20"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM20")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM20", LNM0002INProw("ADDITEM20"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目２０エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目２０エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目２１(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM21", LNM0002INProw("ADDITEM21"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM21")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM21", LNM0002INProw("ADDITEM21"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目２１(通風装置)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目２１(通風装置)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目２２(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM22", LNM0002INProw("ADDITEM22"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM22")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM22", LNM0002INProw("ADDITEM22"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目２２(パレット積載)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目２２(パレット積載)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目２３(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM23", LNM0002INProw("ADDITEM23"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM23")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM23", LNM0002INProw("ADDITEM23"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目２３(水抜き穴)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目２３(水抜き穴)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目２４(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM24", LNM0002INProw("ADDITEM24"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM24")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM24", LNM0002INProw("ADDITEM24"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目２４(エアリブ)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目２４(エアリブ)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目２５(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM25", LNM0002INProw("ADDITEM25"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM25")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM25", LNM0002INProw("ADDITEM25"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目２５エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目２５エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目２６(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM26", LNM0002INProw("ADDITEM26"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM26")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM26", LNM0002INProw("ADDITEM26"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目２６(鮮魚専用コンテナ)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目２６(鮮魚専用コンテナ)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目２７(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM27", LNM0002INProw("ADDITEM27"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM27")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM27", LNM0002INProw("ADDITEM27"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目２７(キャノン専用コンテナ)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目２７(キャノン専用コンテナ)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目２８(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM28", LNM0002INProw("ADDITEM28"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM28")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM28", LNM0002INProw("ADDITEM28"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目２８(特別留置)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目２８(特別留置)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目２９(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM29", LNM0002INProw("ADDITEM29"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM29")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM29", LNM0002INProw("ADDITEM29"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目２９エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目２９エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目３０(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM30", LNM0002INProw("ADDITEM30"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM30")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM30", LNM0002INProw("ADDITEM30"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目３０エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目３０エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目３１(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM31", LNM0002INProw("ADDITEM31"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM31")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM31", LNM0002INProw("ADDITEM31"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目３１(冷凍機温度帯)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目３１(冷凍機温度帯)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目３２(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM32", LNM0002INProw("ADDITEM32"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM32")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM32", LNM0002INProw("ADDITEM32"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目３２(遠隔監視制御装置)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目３２(遠隔監視制御装置)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目３３(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM33", LNM0002INProw("ADDITEM33"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM33")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM33", LNM0002INProw("ADDITEM33"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目３３(エンジン形式)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目３３(エンジン形式)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目３４(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM34", LNM0002INProw("ADDITEM34"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM34")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM34", LNM0002INProw("ADDITEM34"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目３４(燃料タンク容量（リットル）)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目３４(燃料タンク容量（リットル）)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目３５(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM35", LNM0002INProw("ADDITEM35"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM35")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM35", LNM0002INProw("ADDITEM35"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目３５(モータ駆動)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目３５(モータ駆動)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目３６(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM36", LNM0002INProw("ADDITEM36"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM36")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM36", LNM0002INProw("ADDITEM36"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目３６(青函トンネル通過装置)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目３６(青函トンネル通過装置)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目３７(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM37", LNM0002INProw("ADDITEM37"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM37")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM37", LNM0002INProw("ADDITEM37"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目３７エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目３７エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目３８(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM38", LNM0002INProw("ADDITEM38"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM38")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM38", LNM0002INProw("ADDITEM38"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目３８(北海道限定運用コンテナ)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目３８(北海道限定運用コンテナ)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目３９(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM39", LNM0002INProw("ADDITEM39"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM39")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM39", LNM0002INProw("ADDITEM39"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目３９(ヤマト専用コンテナ)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目３９(ヤマト専用コンテナ)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目４０(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM40", LNM0002INProw("ADDITEM40"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM40")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM40", LNM0002INProw("ADDITEM40"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目４０エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目４０エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目４１(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM41", LNM0002INProw("ADDITEM41"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM41")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM41", LNM0002INProw("ADDITEM41"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目４１エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目４１エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目４２(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM42", LNM0002INProw("ADDITEM42"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM42")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM42", LNM0002INProw("ADDITEM42"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目４２エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目４２エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目４３(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM43", LNM0002INProw("ADDITEM43"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM43")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM43", LNM0002INProw("ADDITEM43"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目４３エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目４３エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目４４(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM44", LNM0002INProw("ADDITEM44"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM44")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM44", LNM0002INProw("ADDITEM44"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目４４エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目４４エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目４５(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM45", LNM0002INProw("ADDITEM45"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM45")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM45", LNM0002INProw("ADDITEM45"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目４５(発送通知)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目４５(発送通知)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目４６(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM46", LNM0002INProw("ADDITEM46"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM46")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM46", LNM0002INProw("ADDITEM46"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目４６(他者所有コンテナ)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目４６(他者所有コンテナ)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目４７(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM47", LNM0002INProw("ADDITEM47"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM47")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM47", LNM0002INProw("ADDITEM47"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目４７(リース取得コンテナ)エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目４７(リース取得コンテナ)エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目４８(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM48", LNM0002INProw("ADDITEM48"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM48")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM48", LNM0002INProw("ADDITEM48"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目４８エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目４８エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目４９(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM49", LNM0002INProw("ADDITEM49"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM49")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM49", LNM0002INProw("ADDITEM49"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目４９エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目４９エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 付帯項目５０(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "ADDITEM50", LNM0002INProw("ADDITEM50"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("ADDITEM50")) Then
                    ' 名称存在チェック
                    CODENAME_get("ADDITEM50", LNM0002INProw("ADDITEM50"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・付帯項目５０エラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・付帯項目５０エラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If
            ' 床材質コード(バリデーションチェック)
            Master.CheckField(Master.USERCAMP, "FLOORMATERIAL", LNM0002INProw("FLOORMATERIAL"), WW_CS0024FCheckerr, WW_CS0024FCheckReport)
            If isNormal(WW_CS0024FCheckerr) Then
                If Not String.IsNullOrEmpty(LNM0002INProw("FLOORMATERIAL")) Then
                    ' 名称存在チェック
                    CODENAME_get("FLOORMATERIAL", LNM0002INProw("FLOORMATERIAL"), WW_Dummy, WW_RtnSW)
                    If Not isNormal(WW_RtnSW) Then
                        WW_CheckMES1 = "・床材質コードエラーです。"
                        WW_CheckMES2 = "マスタに存在しません。"
                        WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                        WW_LineErr = "ERR"
                        O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
                    End If
                End If
            Else
                WW_CheckMES1 = "・床材質コードエラーです。"
                WW_CheckMES2 = WW_CS0024FCheckReport
                WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                WW_LineErr = "ERR"
                O_RTN = C_MESSAGE_NO.INVALID_REGIST_RECORD_ERROR
            End If

            ' 排他チェック
            If Not String.IsNullOrEmpty(work.WF_SEL_CTNTYPE2.Text) Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 排他チェック
                    work.HaitaCheck(SQLcon, WW_DBDataCheck, work.WF_SEL_CTNTYPE2.Text, work.WF_SEL_CTNNO2.Text, work.WF_SEL_TIMESTAMP.Text)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・排他エラー（コンテナ記号 & コンテナ番号）"
                    WW_CheckMES2 = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR &
                                           "([" & LNM0002INProw("CTNTYPE") & "]" &
                                           " [" & LNM0002INProw("CTNNO") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                End If
            End If
            ' 一意制約チェック
            If Not LNM0002INProw("CTNTYPE") = work.WF_SEL_CTNTYPE2.Text OrElse
                    Not LNM0002INProw("CTNNO") = work.WF_SEL_CTNNO2.Text Then
                Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                    ' DataBase接続
                    SQLcon.Open()
                    ' 一意制約チェック
                    UniqueKeyCheck(SQLcon, WW_DBDataCheck)
                End Using

                If Not isNormal(WW_DBDataCheck) Then
                    WW_CheckMES1 = "・一意制約違反（コンテナ記号 & コンテナ番号）"
                    WW_CheckMES2 = C_MESSAGE_NO.OVERLAP_DATA_ERROR &
                                       "([" & LNM0002INProw("CTNTYPE") & "]" &
                                       " [" & LNM0002INProw("CTNNO") & "])"
                    WW_CheckERR(WW_CheckMES1, WW_CheckMES2)
                    WW_LineErr = "ERR"
                    O_RTN = C_MESSAGE_NO.CTN_PRIMARYKEY_REPEAT_ERROR
                End If
            End If

            If String.IsNullOrEmpty(WW_LineErr) Then
                If LNM0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.ERRORED Then
                    LNM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                End If
            Else
                If WW_LineErr = CONST_PATTERNERR Then
                    ' 関連チェックエラーをセット
                    LNM0002INProw.Item("OPERATION") = CONST_PATTERNERR
                Else
                    ' 単項目チェックエラーをセット
                    LNM0002INProw.Item("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' パスワード有効期限チェック
    ''' </summary>
    ''' <param name="PassEndDate"></param>
    ''' <param name="NowDate"></param>
    ''' <param name="WW_StyDateFlag"></param>
    ''' <param name="WW_NewPassEndDate"></param>
    ''' <remarks></remarks>
    Protected Sub WW_CheckPassEndDate(ByVal PassEndDate As DateTime, ByVal NowDate As DateTime, ByRef WW_StyDateFlag As String, ByRef WW_NewPassEndDate As String)

        WW_StyDateFlag = "1"

        NowDate = NowDate.AddDays(ADDDATE)

        WW_NewPassEndDate = NowDate

        If Not String.IsNullOrEmpty(PassEndDate.ToString) Then
            If NowDate <= PassEndDate Then
                WW_StyDateFlag = "0"
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
    ''' LNM0002tbl更新
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub LNM0002tbl_UPD()
        ' 発見フラグ
        Dim WW_IsFound As Boolean = False

        '○ 画面状態設定
        For Each LNM0002row As DataRow In LNM0002tbl.Rows
            Select Case LNM0002row("OPERATION")
                Case C_LIST_OPERATION_CODE.NODATA
                    ' データなし
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.NODISP
                    ' 表示なし
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED
                    ' 行選択
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.UPDATING
                    ' 行選択 & 更新対象
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                Case C_LIST_OPERATION_CODE.SELECTED & C_LIST_OPERATION_CODE.ERRORED
                    ' 行選択 & エラー行対象
                    LNM0002row("OPERATION") = C_LIST_OPERATION_CODE.ERRORED
            End Select
        Next

        '○ 追加変更判定
        For Each LNM0002INProw As DataRow In LNM0002INPtbl.Rows
            ' エラーレコード読み飛ばし
            If LNM0002INProw("OPERATION") <> C_LIST_OPERATION_CODE.UPDATING Then
                Continue For
            End If

            LNM0002INProw.Item("OPERATION") = CONST_INSERT

            ' 既存レコードとの比較
            For Each LNM0002row As DataRow In LNM0002tbl.Rows
                ' KEY項目が等しい時
                If LNM0002row("CTNTYPE") = LNM0002INProw("CTNTYPE") AndAlso
                    LNM0002row("CTNNO") = LNM0002INProw("CTNNO") Then
                    ' KEY項目以外の項目の差異をチェック
                    If LNM0002row("DELFLG") = LNM0002INProw("DELFLG") AndAlso
                        LNM0002row("JURISDICTIONCD") = LNM0002INProw("JURISDICTIONCD") AndAlso
                        LNM0002row("ACCOUNTINGASSETSCD") = LNM0002INProw("ACCOUNTINGASSETSCD") AndAlso
                        LNM0002row("ACCOUNTINGASSETSKBN") = LNM0002INProw("ACCOUNTINGASSETSKBN") AndAlso
                        LNM0002row("DUMMYKBN") = LNM0002INProw("DUMMYKBN") AndAlso
                        LNM0002row("SPOTKBN") = LNM0002INProw("SPOTKBN") AndAlso
                        LNM0002row("SPOTSTYMD") = LNM0002INProw("SPOTSTYMD") AndAlso
                        LNM0002row("SPOTENDYMD") = LNM0002INProw("SPOTENDYMD") AndAlso
                        LNM0002row("BIGCTNCD") = LNM0002INProw("BIGCTNCD") AndAlso
                        LNM0002row("MIDDLECTNCD") = LNM0002INProw("MIDDLECTNCD") AndAlso
                        LNM0002row("SMALLCTNCD") = LNM0002INProw("SMALLCTNCD") AndAlso
                        LNM0002row("CONSTRUCTIONYM") = LNM0002INProw("CONSTRUCTIONYM") AndAlso
                        LNM0002row("CTNMAKER") = LNM0002INProw("CTNMAKER") AndAlso
                        LNM0002row("FROZENMAKER") = LNM0002INProw("FROZENMAKER") AndAlso
                        LNM0002row("GROSSWEIGHT") = LNM0002INProw("GROSSWEIGHT") AndAlso
                        LNM0002row("CARGOWEIGHT") = LNM0002INProw("CARGOWEIGHT") AndAlso
                        LNM0002row("MYWEIGHT") = LNM0002INProw("MYWEIGHT") AndAlso
                        LNM0002row("BOOKVALUE") = LNM0002INProw("BOOKVALUE") AndAlso
                        LNM0002row("OUTHEIGHT") = LNM0002INProw("OUTHEIGHT") AndAlso
                        LNM0002row("OUTWIDTH") = LNM0002INProw("OUTWIDTH") AndAlso
                        LNM0002row("OUTLENGTH") = LNM0002INProw("OUTLENGTH") AndAlso
                        LNM0002row("INHEIGHT") = LNM0002INProw("INHEIGHT") AndAlso
                        LNM0002row("INWIDTH") = LNM0002INProw("INWIDTH") AndAlso
                        LNM0002row("INLENGTH") = LNM0002INProw("INLENGTH") AndAlso
                        LNM0002row("WIFEHEIGHT") = LNM0002INProw("WIFEHEIGHT") AndAlso
                        LNM0002row("WIFEWIDTH") = LNM0002INProw("WIFEWIDTH") AndAlso
                        LNM0002row("SIDEHEIGHT") = LNM0002INProw("SIDEHEIGHT") AndAlso
                        LNM0002row("SIDEWIDTH") = LNM0002INProw("SIDEWIDTH") AndAlso
                        LNM0002row("FLOORAREA") = LNM0002INProw("FLOORAREA") AndAlso
                        LNM0002row("INVOLUMEMARKING") = LNM0002INProw("INVOLUMEMARKING") AndAlso
                        LNM0002row("INVOLUMEACTUA") = LNM0002INProw("INVOLUMEACTUA") AndAlso
                        LNM0002row("TRAINSCYCLEDAYS") = LNM0002INProw("TRAINSCYCLEDAYS") AndAlso
                        LNM0002row("TRAINSBEFORERUNYMD") = LNM0002INProw("TRAINSBEFORERUNYMD") AndAlso
                        LNM0002row("TRAINSNEXTRUNYMD") = LNM0002INProw("TRAINSNEXTRUNYMD") AndAlso
                        LNM0002row("REGINSCYCLEDAYS") = LNM0002INProw("REGINSCYCLEDAYS") AndAlso
                        LNM0002row("REGINSCYCLEHOURMETER") = LNM0002INProw("REGINSCYCLEHOURMETER") AndAlso
                        LNM0002row("REGINSBEFORERUNYMD") = LNM0002INProw("REGINSBEFORERUNYMD") AndAlso
                        LNM0002row("REGINSNEXTRUNYMD") = LNM0002INProw("REGINSNEXTRUNYMD") AndAlso
                        LNM0002row("REGINSHOURMETERYMD") = LNM0002INProw("REGINSHOURMETERYMD") AndAlso
                        LNM0002row("REGINSHOURMETERTIME") = LNM0002INProw("REGINSHOURMETERTIME") AndAlso
                        LNM0002row("REGINSHOURMETERDSP") = LNM0002INProw("REGINSHOURMETERDSP") AndAlso
                        LNM0002row("OPERATIONSTYMD") = LNM0002INProw("OPERATIONSTYMD") AndAlso
                        LNM0002row("OPERATIONENDYMD") = LNM0002INProw("OPERATIONENDYMD") AndAlso
                        LNM0002row("RETIRMENTYMD") = LNM0002INProw("RETIRMENTYMD") AndAlso
                        LNM0002row("COMPKANKBN") = LNM0002INProw("COMPKANKBN") AndAlso
                        LNM0002row("SUPPLYFLG") = LNM0002INProw("SUPPLYFLG") AndAlso
                        LNM0002row("ADDITEM1") = LNM0002INProw("ADDITEM1") AndAlso
                        LNM0002row("ADDITEM2") = LNM0002INProw("ADDITEM2") AndAlso
                        LNM0002row("ADDITEM3") = LNM0002INProw("ADDITEM3") AndAlso
                        LNM0002row("ADDITEM4") = LNM0002INProw("ADDITEM4") AndAlso
                        LNM0002row("ADDITEM5") = LNM0002INProw("ADDITEM5") AndAlso
                        LNM0002row("ADDITEM6") = LNM0002INProw("ADDITEM6") AndAlso
                        LNM0002row("ADDITEM7") = LNM0002INProw("ADDITEM7") AndAlso
                        LNM0002row("ADDITEM8") = LNM0002INProw("ADDITEM8") AndAlso
                        LNM0002row("ADDITEM9") = LNM0002INProw("ADDITEM9") AndAlso
                        LNM0002row("ADDITEM10") = LNM0002INProw("ADDITEM10") AndAlso
                        LNM0002row("ADDITEM11") = LNM0002INProw("ADDITEM11") AndAlso
                        LNM0002row("ADDITEM12") = LNM0002INProw("ADDITEM12") AndAlso
                        LNM0002row("ADDITEM13") = LNM0002INProw("ADDITEM13") AndAlso
                        LNM0002row("ADDITEM14") = LNM0002INProw("ADDITEM14") AndAlso
                        LNM0002row("ADDITEM15") = LNM0002INProw("ADDITEM15") AndAlso
                        LNM0002row("ADDITEM16") = LNM0002INProw("ADDITEM16") AndAlso
                        LNM0002row("ADDITEM17") = LNM0002INProw("ADDITEM17") AndAlso
                        LNM0002row("ADDITEM18") = LNM0002INProw("ADDITEM18") AndAlso
                        LNM0002row("ADDITEM19") = LNM0002INProw("ADDITEM19") AndAlso
                        LNM0002row("ADDITEM20") = LNM0002INProw("ADDITEM20") AndAlso
                        LNM0002row("ADDITEM21") = LNM0002INProw("ADDITEM21") AndAlso
                        LNM0002row("ADDITEM22") = LNM0002INProw("ADDITEM22") AndAlso
                        LNM0002row("ADDITEM23") = LNM0002INProw("ADDITEM23") AndAlso
                        LNM0002row("ADDITEM24") = LNM0002INProw("ADDITEM24") AndAlso
                        LNM0002row("ADDITEM25") = LNM0002INProw("ADDITEM25") AndAlso
                        LNM0002row("ADDITEM26") = LNM0002INProw("ADDITEM26") AndAlso
                        LNM0002row("ADDITEM27") = LNM0002INProw("ADDITEM27") AndAlso
                        LNM0002row("ADDITEM28") = LNM0002INProw("ADDITEM28") AndAlso
                        LNM0002row("ADDITEM29") = LNM0002INProw("ADDITEM29") AndAlso
                        LNM0002row("ADDITEM30") = LNM0002INProw("ADDITEM30") AndAlso
                        LNM0002row("ADDITEM31") = LNM0002INProw("ADDITEM31") AndAlso
                        LNM0002row("ADDITEM32") = LNM0002INProw("ADDITEM32") AndAlso
                        LNM0002row("ADDITEM33") = LNM0002INProw("ADDITEM33") AndAlso
                        LNM0002row("ADDITEM34") = LNM0002INProw("ADDITEM34") AndAlso
                        LNM0002row("ADDITEM35") = LNM0002INProw("ADDITEM35") AndAlso
                        LNM0002row("ADDITEM36") = LNM0002INProw("ADDITEM36") AndAlso
                        LNM0002row("ADDITEM37") = LNM0002INProw("ADDITEM37") AndAlso
                        LNM0002row("ADDITEM38") = LNM0002INProw("ADDITEM38") AndAlso
                        LNM0002row("ADDITEM39") = LNM0002INProw("ADDITEM39") AndAlso
                        LNM0002row("ADDITEM40") = LNM0002INProw("ADDITEM40") AndAlso
                        LNM0002row("ADDITEM41") = LNM0002INProw("ADDITEM41") AndAlso
                        LNM0002row("ADDITEM42") = LNM0002INProw("ADDITEM42") AndAlso
                        LNM0002row("ADDITEM43") = LNM0002INProw("ADDITEM43") AndAlso
                        LNM0002row("ADDITEM44") = LNM0002INProw("ADDITEM44") AndAlso
                        LNM0002row("ADDITEM45") = LNM0002INProw("ADDITEM45") AndAlso
                        LNM0002row("ADDITEM46") = LNM0002INProw("ADDITEM46") AndAlso
                        LNM0002row("ADDITEM47") = LNM0002INProw("ADDITEM47") AndAlso
                        LNM0002row("ADDITEM48") = LNM0002INProw("ADDITEM48") AndAlso
                        LNM0002row("ADDITEM49") = LNM0002INProw("ADDITEM49") AndAlso
                        LNM0002row("ADDITEM50") = LNM0002INProw("ADDITEM50") AndAlso
                        LNM0002row("FLOORMATERIAL") = LNM0002INProw("FLOORMATERIAL") AndAlso
                        Not C_LIST_OPERATION_CODE.UPDATING.Equals(LNM0002row("OPERATION")) Then
                        ' 変更がない時は「操作」の項目は空白にする
                        LNM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.NODATA
                    Else
                        ' 変更がある時は「操作」の項目を「更新」に設定する
                        LNM0002INProw("OPERATION") = CONST_UPDATE
                    End If

                    Exit For
                End If
            Next
        Next

        ' 更新チェック
        If C_LIST_OPERATION_CODE.NODATA.Equals(LNM0002INPtbl.Rows(0)("OPERATION")) Then
            ' 更新なしの場合、エラーコードに変更なしエラーをセットして処理打ち切り
            WW_ErrCode = C_MESSAGE_NO.NO_CHANGE_UPDATE
            Exit Sub

        ElseIf CONST_UPDATE.Equals(LNM0002INPtbl.Rows(0)("OPERATION")) OrElse
            CONST_INSERT.Equals(LNM0002INPtbl.Rows(0)("OPERATION")) Then
            ' 追加/更新の場合、DB更新処理
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                ' DataBase接続
                SQLcon.Open()

                Dim WW_MODIFYKBN As String = ""
                Dim WW_DATE As Date = Date.Now

                '変更チェック
                RECONMEXISTS(SQLcon, WW_MODIFYKBN)
                If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                    Exit Sub
                End If

                '変更がある場合履歴テーブルに変更前データを登録
                If WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.BEFDATA Then
                    '履歴登録(変更前)
                    InsertHist(SQLcon, WW_MODIFYKBN, WW_DATE)
                    If Not WW_ErrSW.Equals(C_MESSAGE_NO.NORMAL) Then
                        Exit Sub
                    End If
                    '登録後変更区分を変更後にする
                    WW_MODIFYKBN = LNM0002WRKINC.MODIFYKBN.AFTDATA
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

                Dim LNM0002row As DataRow = LNM0002INPtbl.Rows(0)
                Dim zaikokbn As String = LNM0002WRKINC.GetZaikoUpdateHantei(LNM0002row)
                '在庫更新判定処理
                If zaikokbn <> "" Then
                    ' 在庫更新 
                    UpdateZaiko(SQLcon, LNM0002row, zaikokbn)
                End If

                work.WF_SEL_DETAIL_UPDATE_MESSAGE.Text = "Update Success!!"
            End Using
        End If

        '○ 変更有無判定 & 入力値反映
        For Each LNM0002INProw As DataRow In LNM0002INPtbl.Rows
            ' 発見フラグ
            WW_IsFound = False

            For Each LNM0002row As DataRow In LNM0002tbl.Rows
                ' 同一レコードか判定
                If LNM0002INProw("CTNTYPE") = LNM0002row("CTNTYPE") AndAlso
                    LNM0002INProw("CTNNO") = LNM0002row("CTNNO") Then
                    ' 画面入力テーブル項目設定
                    LNM0002INProw("LINECNT") = LNM0002row("LINECNT")
                    LNM0002INProw("OPERATION") = C_LIST_OPERATION_CODE.UPDATING
                    LNM0002INProw("UPDTIMSTP") = LNM0002row("UPDTIMSTP")
                    LNM0002INProw("SELECT") = 0
                    LNM0002INProw("HIDDEN") = 0
                    ' 項目テーブル項目設定
                    LNM0002row.ItemArray = LNM0002INProw.ItemArray
                    ' 発見フラグON
                    WW_IsFound = True
                    Exit For
                End If
            Next

            ' 同一レコードが発見できない場合は、追加する
            If Not WW_IsFound Then
                Dim WW_NRow = LNM0002tbl.NewRow
                WW_NRow.ItemArray = LNM0002INProw.ItemArray
                ' 画面入力テーブル項目設定
                WW_NRow("LINECNT") = LNM0002tbl.Rows.Count + 1
                WW_NRow("OPERATION") = C_LIST_OPERATION_CODE.INSERTING
                'WW_NRow("UPDTIMSTP") = "0"
                WW_NRow("SELECT") = 0
                WW_NRow("HIDDEN") = 0
                LNM0002tbl.Rows.Add(WW_NRow)
            End If
        Next

    End Sub

    ''' <summary>
    ''' コンテナマスタ登録更新
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <remarks></remarks>
    Private Function UpdateZaiko(ByVal SQLcon As MySqlConnection, ByVal LNM0002row As DataRow, ByVal zaikokbn As String) As String

        UpdateZaiko = Messages.C_MESSAGE_NO.NORMAL

        Dim WW_DATENOW As DateTime = Date.Now
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
            Master.Output(C_MESSAGE_NO.DB_ERROR, C_MESSAGE_TYPE.ABORT, "LNM0002C UPDATE_ZAIKO")

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0002C UPDATE_ZAIKO"
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
                Case "CTNTYPE"                    'コンテナ記号
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_TYPE))
                Case "CTNNO"                      'コンテナ番号
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_RECONM, I_VALUE, O_TEXT, O_RTN, work.CreateContenaParam(GL0020ContenaList.LS_CONTENA_WITH.CTN_NO, TxtCTNType.Text))
                Case "JURISDICTION"               '所管部コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ACCOUNTINGASSETSCD"         '経理資産コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ACCOUNTINGASSETSKBN"        '経理資産区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "DUMMYKBN"                   'ダミー区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "SPOTKBN"                    'スポット区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "BIGCTNCD"                   '大分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.BIG_CLASS))
                Case "MIDDLECTNCD"                '中分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.MIDDLE_CLASS, TxtBigCTNCD.Text))
                Case "SMALLCTNCD"                 '小分類コード
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_CLASS, I_VALUE, O_TEXT, O_RTN, work.CreateClassParam(GL0016ClassList.LS_CLASS_WITH.SMALL_CLASS, TxtBigCTNCD.Text, TxtMiddleCTNCD.Text))
                Case "CTNMAKER"                 　'コンテナメーカー
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "FROZENMAKER"                '冷凍機メーカー
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "COMPKANKBN"                 '複合一貫区分
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "SUPPLYFLG"                  '調達フラグ
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM1"                   '付帯項目１
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM2"                   '付帯項目２
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM3"                   '付帯項目３
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM4"                   '付帯項目４
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM5"                   '付帯項目５
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM6"                   '付帯項目６
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM7"                   '付帯項目７
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM8"                   '付帯項目８
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM9"                   '付帯項目９
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM10"                  '付帯項目１０
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM11"                  '付帯項目１１
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM12"                  '付帯項目１２
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM13"                  '付帯項目１３
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM14"                  '付帯項目１４
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM15"                  '付帯項目１５
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM16"                  '付帯項目１６
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM17"                  '付帯項目１７
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM18"                  '付帯項目１８
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM19"                  '付帯項目１９
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM20"                  '付帯項目２０
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM21"                  '付帯項目２１
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM22"                  '付帯項目２２
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM23"                  '付帯項目２３
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM24"                  '付帯項目２４
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM25"                  '付帯項目２５
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM26"                  '付帯項目２６
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM27"                  '付帯項目２７
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM28"                  '付帯項目２８
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM29"                  '付帯項目２９
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM30"                  '付帯項目３０
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM31"                  '付帯項目３１
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM32"                  '付帯項目３２
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM33"                  '付帯項目３３
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM34"                  '付帯項目３４
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM35"                  '付帯項目３５
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM36"                  '付帯項目３６
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM37"                  '付帯項目３７
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM38"                  '付帯項目３８
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM39"                  '付帯項目３９
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM40"                  '付帯項目４０
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM41"                  '付帯項目４１
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM42"                  '付帯項目４２
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM43"                  '付帯項目４３
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM44"                  '付帯項目４４
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM45"                  '付帯項目４５
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM46"                  '付帯項目４６
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM47"                  '付帯項目４７
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM48"                  '付帯項目４８
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM49"                  '付帯項目４９
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "ADDITEM50"                  '付帯項目５０
                    leftview.CodeToName(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE, I_VALUE, O_TEXT, O_RTN, work.CreateFIXParam(Master.USERCAMP, I_FIELD))
                Case "FLOORMATERIAL"              '床材質コード
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

End Class
