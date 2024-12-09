Option Strict On
''' <summary>
''' 左ボックス共通ユーザーコントロールクラス
''' </summary>
Public Class GRIS0005LeftBox
    Inherits UserControl
    ''' <summary>
    ''' ソート機能
    ''' </summary>
    Public Property LF_SORTING_CODE As String
    ''' <summary>
    ''' フィルターの有無
    ''' </summary>
    Public Property LF_FILTER_CODE As String
    ''' <summary>
    ''' 再検索時の主要パラメータ（１つ）
    ''' </summary>
    Public Property LF_PARAM_DATA As String

    ''' <summary>
    ''' ソート機能の条件一覧
    ''' </summary>
    ''' <remarks></remarks>
    Public Class C_SORTING_CODE
        ''' <summary>
        ''' ソート機能：なし
        ''' </summary>
        Public Const HIDE As String = "0"
        ''' <summary>
        ''' ソート機能：名称
        ''' </summary>
        Public Const NAME As String = "1"
        ''' <summary>
        ''' ソート機能：コード
        ''' </summary>
        Public Const CODE As String = "2"
        ''' <summary>
        ''' ソート機能：名称・コード
        ''' </summary>
        Public Const BOTH As String = "3"
    End Class
    ''' <summary>
    ''' フィルター機能の条件一覧
    ''' </summary>
    ''' <remarks></remarks>
    Public Class C_FILTER_CODE
        ''' <summary>
        ''' フィルター機能：なし
        ''' </summary>
        Public Const DISABLE As String = "0"
        ''' <summary>
        ''' フィルター機能：あり
        ''' </summary>
        Public Const ENABLE As String = "1"
        ''' <summary>
        ''' フィルター機能：再検索
        ''' </summary>
        Public Const RESEACH As String = "2"
    End Class
    ''' <summary>
    ''' 左リストの作成情報一覧
    ''' </summary>
    ''' <list type="number">
    ''' <item><description>LC_COMPANY       : 会社のリストを作成</description></item>
    ''' <item><description>LC_CUSTOMER      : 顧客のリストを作成</description></item>
    ''' <item><description>LC_DISTINATION   : 届先のリストを作成</description></item>
    ''' <item><description>LC_ORG           : 部署のリストを作成</description></item>
    ''' <item><description>LC_STAFFCODE     : 社員のリストを作成</description></item>
    ''' <item><description>LC_GOODS         : 油種・品名のリストを作成</description></item>
    ''' <item><description>LC_CARCODE       : 統一車番のリストを作成</description></item>
    ''' <item><description>LC_WORKLORRY     : 業務車番のリストを作成(品名または固定値のFast)</description></item>
    ''' <item><description>LC_URIKBN 　　　 : 売上区分のリストを作成(固定値のFast)</description></item>
    ''' <item><description>LC_STAFFKBN      : 社員区分のリストを作成(固定値のFast)</description></item>
    ''' <item><description>LC_DELFLG        : 削除区分のリストを作成(固定値のFast)</description></item>
    ''' <item><description>LC_TERM          : 端末一覧のリストを作成</description></item>
    ''' <item><description>LC_ROLE          : 権限のリストを作成</description></item>
    ''' <item><description>LC_URL           : URLのリストを作成</description></item>
    ''' <item><description>LC_MODELPT       : モデル距離パターンのリストを作成(固定値のFast)</description></item>
    ''' <item><description>LC_EXTRA_LIST    : 指定されたリストを使用する</description></item>
    ''' <item><description>LC_CALENDAR      : カレンダー表示を行う</description></item>
    ''' <item><description>LC_FIX_VALUE     : 固定値区分のリストを作成</description></item>
    ''' <item><description>LC_STATIONCODE   : 貨物駅パターンのリストを作成</description></item>
    ''' <item><description>LC_BRANCH        : 管轄支店のリストを作成（タンク所在向け）</description></item>
    ''' <item><description>LC_BELONGTOOFFICE: 所属営業所（支店含む）のリストを作成（タンク所在向け）</description></item>
    ''' <item><description>LC_BRANCHOFFICESTATION: 管轄所属・駅関連付けのリストを作成（タンク所在向け）</description></item>
    ''' <item><description>LC_TANKSTATUS: タンク車状態のリストを作成</description></item>
    ''' <item><description>LC_LOADINGKBN: 積車状態リストを作成</description></item>
    ''' <item><description>LC_TANKSITUATION: タンク車状況リストを作成</description></item>
    ''' </list>
    Public Enum LIST_BOX_CLASSIFICATION
        LC_COMPANY             '会社コード
        LC_ORG                 '運用部署
        LC_CALENDAR            'カレンダー
        LC_DELFLG              '削除フラグ
        LC_ROLE                '権限
        LC_JURISDICTION        '所管部
        LC_ORDERSTATUS         '受注状態
        LC_CTNTYPE             'コンテナ記号
        LC_CTNNO               'コンテナ番号
        '受注登録画面の明細データ
        LC_ITEMCD              '品目コード
        LC_ITEMNM              '品目名
        LC_DEPSTATION          '発駅コード
        LC_DEPSTATIONNM        '発駅名
        LC_ARRSTATION          '着駅コード
        LC_ARRSTATIONNM        '着駅名
        LC_RAILDEPSTATION      '鉄道発駅コード
        LC_RAILDEPSTATIONNM    '鉄道発駅名
        LC_RAILARRSTATION      '鉄道着駅コード
        LC_RAILARRSTATIONNM    '鉄道着駅名
        LC_RAWDEPSTATION       '原発駅コード
        LC_RAWDEPSTATIONNM     '原発駅名
        LC_RAWARRSTATION       '原着駅コード
        LC_RAWARRSTATIONNM     '原着駅名
        LC_DEPTRUSTEECD        '発受託人コード
        LC_DEPTRUSTEENM        '発受託人       
        LC_DEPPICKDELTRADERCD  '発受託人サブコード 
        LC_DEPPICKDELTRADERNM  '発受託人サブ
        LC_ARRTRUSTEECD        '着受託人コード
        LC_ARRTRUSTEENM        '着受託人
        LC_ARRPICKDELTRADERCD  '着受託人サブコード
        LC_ARRPICKDELTRADERNM  '着受託人サブ
        LC_DEPTRAINNO          '発列車番号
        LC_ARRTRAINNO          '着列車番号
        LC_PLANARRYMD          '到着予定日
        LC_RESULTARRYMD        '到着実績日
        LC_STACKFREEKBNCD      '積空区分コード
        LC_STACKFREEKBNNM      '積空区分名
        LC_SHIPPERCD           '荷送人コード
        LC_SHIPPERNM           '荷送人
        LC_SLCPICKUPTEL        '集荷先電話番号
        LC_OTHERFEE            'その他料金
        '受注検索画面で使用
        LC_JOTDEPBRANCH        'JOT発店所
        LC_RECONM              'コンテナマスタ
        LC_REKEJM              'コンテナ取引先マスタ
        LC_ITEM                '品目マスタ
        LC_CLASS               '大中小分類マスタ
        LC_SHIPPER             '荷主マスタ
        LC_KEKKJM              '営業収入決済条件マスタ
        LC_KEKSBM              '請求摘要マスタ
        LC_STATION             '駅マスタ
        LC_FINANCE_ITEN        'ファイナンスリース項目
        LC_USERMST              'ユーザーマスタ
        LC_FIX_VALUE           '固定値マスタ
        '帳票出力画面で使用
        LC_REPORT              '帳票マスタ
        LC_MODE                '処理種別
        LC_ACCOUNTINGASSETSKBN '経理資産区分
        LC_SEARCH              '検索種別
        LC_INVCYCL             '締め日
        LC_SORT                '並び順
        LC_KEIJOBASE           '計上ベース
        LC_TRUSTEEKBN          '受託人指定
        LC_BILLINGKBN          '請求先指定
        LC_ADDSUBKBN           '加減額表示指定
        LC_BRANCHBASE          '処理種別
        LC_DEPARRBASE          '発着ベース
        LC_STACKFREE           '積空区分(帳票用)
        LC_REPORTSETTING       '帳票出力設定
        LC_REPLACE             '入れ替え
        '収入管理で使用
        LC_ACCOUNTCODE         '科目コード
        LC_SEGMENTCODE         'セグメント
    End Enum

    ''' <summary>
    ''' パラメタ群
    ''' </summary>
    ''' <remarks>
    ''' <list type="number">
    ''' <item><description>LP_COMPANY       : 検索条件に会社コードを指定</description></item>
    ''' <item><description>LP_TYPEMODE      : 検索条件に各検索の条件区分値を指定</description></item>
    ''' <item><description>LP_PERMISSION    : 検索条件に権限を指定</description></item>
    ''' <item><description>LP_CUSTOMER      : 検索条件に取引先コードを指定</description></item>
    ''' <item><description>LP_CLASSCODE     : 検索条件に区分値を指定</description></item>
    ''' <item><description>LP_STAFF_KBN_LIST: 検索条件に社員区分一覧を指定</description></item>
    ''' <item><description>LP_ORG_COMP      : 検索条件に部署における会社コードを指定</description></item>
    ''' <item><description>LP_ORG           : 検索条件に部署コードを指定</description></item>
    ''' <item><description>LP_ORG_CATEGORYS : 検索条件に部署の区分け条件を指定</description></item>
    ''' <item><description>LP_OILTYPE       : 検索条件に油種コードを指定</description></item>
    ''' <item><description>LP_PRODCODE1     : 検索条件に品名１コードを指定</description></item>
    ''' <item><description>LP_FIX_CLASS     : 検索条件に固定値区分コードを指定</description></item>
    ''' <item><description>LP_LIST          : 画面表示させたい一覧を指定</description></item>
    ''' </list>
    ''' </remarks>
    Public Enum C_PARAMETERS
        LP_COMPANY            '会社コード
        LP_CTNNO              'コンテナ番号
        LP_DEPTRUSTEECD       '発受託人コード
        LP_STYMD
        LP_ENDYMD
        LP_TYPEMODE
        LP_PERMISSION
        LP_CUSTOMER
        LP_CLASSCODE
        LP_STAFF_KBN_LIST
        LP_ORG_COMP
        LP_ORG
        LP_ORG_CATEGORYS
        LP_OILTYPE
        LP_PRODCODE1
        LP_FIX_CLASS
        LP_LIST
        LP_MODELPT
        LP_DEFAULT_SORT
        LP_DISPLAY_FORMAT
        LP_ROLE
        LP_SELECTED_CODE
        LP_STATIONCODE
        LP_TANKNUMBER
        LP_TANKMODEL
        LP_SALESOFFICE
        LP_TRAINNUMBER
        LP_PRODUCTLIST
        LP_ORDERSTATUS
        LP_ORDERINFO
        LP_USEPROPRIETY
        LP_BIGOILCODE
        LP_MIDDLEOILCODE
        LP_TRAINCLASS
        LP_SPEEDCLASS
        LP_ORIGINOWNER
        LP_OWNER
        LP_LEASE
        LP_LEASECLASS
        LP_THIRDUSER
        LP_DEDICATETYPE
        LP_EXTRADINARYTYPE
        LP_BASE
        LP_COLOR
        LP_OBTAINED
        LP_SHIPPERSLIST
        LP_CONSIGNEELIST
        LP_STATION
        LP_ORDERTYPE
        LP_PRODULNSEGLIST
        LP_ADDITINALCONDITION
        LP_ADDITINALSORTORDER
        LP_RINKAITRAIN_INLIST
        LP_RINKAITRAIN_OUTLIST
        LP_RINKAITRAIN_LINELIST
        LP_DEPARRSTATIONLIST
        LP_STATIONCODE_FOCUSON
        LP_APPROVALFLG1
        LP_APPROVALFLG2
        LP_USERID
        ' 大分類中分類取得パラメータ
        LP_BIGCTNCD
        LP_MIDDLECTNCD
        ' 発受託人コード取得(コンテナ取引先マスタ)パラメータ
        LP_TRUSTEECD
        ' 請求項目請求書決済区分・請求項目請求書細分コード取得パラメータ
        LP_TORICODE
        LP_INVFILINGDEPT
        LP_INVKESAIKBN
        ' コンテナ記号取得パラメータ
        LP_CTNTYPE
        ' 計上年月
        LP_KEIJYOYM

    End Enum
    Public Const LEFT_TABLE_SELECTED_KEY As String = "LEFT_TABLE_SELECTED_KEY"
    ''' <summary>
    ''' 作成一覧情報の保持
    ''' </summary>
    Protected LbMap As New Hashtable

    Protected C_TABLE_SPLIT As String = "|"

    Public ReadOnly Property ActiveViewIdx As Integer
        Get
            Return Me.WF_LEFTMView.ActiveViewIndex
        End Get
    End Property

    ''' <summary>
    ''' サーバ処理の遷移先
    ''' </summary>
    ''' <param name="sender">起動オブジェクト</param>
    ''' <param name="e">イベント発生時パラメータ</param>
    ''' <remarks></remarks>
    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        Dim O_RTN As String = C_MESSAGE_NO.NORMAL


        If IsPostBack Then
            Select Case DirectCast(Page.Master.FindControl("contents1").FindControl("WF_ButtonClick"), HtmlInputText).Value
                Case "WF_Field_DBClick", "WF_LeftBoxSubmit", "WF_SPREAD_BtnClick"            'フィールドダブルクリック
                    ViewState("LF_FILTER_CODE") = LF_FILTER_CODE
                    ViewState("LF_SORTING_CODE") = LF_SORTING_CODE
                    ViewState("LF_PARAM_DATA") = LF_PARAM_DATA

                    '親フォームのワークより、カレンダアイコンの押下位置を取得
                    Dim strTOP As String = Request.Form("ctl00$WF_saveTop")
                    Dim strLEFT As String = Request.Form("ctl00$WF_saveLeft")

                    If Not IsNothing(strTOP) And Not IsNothing(strLEFT) Then
                        LF_LEFTBOX.Style.Value = "TOP:" & strTOP & "px;" & "LEFT:" & strLEFT & "px;"
                    End If

                Case "WF_ListboxDBclick", "WF_ButtonCan", "WF_ButtonSel"
                    '〇初期化
                    ViewState("LF_FILTER_CODE") = Nothing
                    ViewState("LF_SORTING_CODE") = Nothing
                    ViewState("LF_PARAM_DATA") = Nothing
                    ViewState("LF_LIST_SELECT") = Nothing
                    ViewState("LF_PARAMS") = Nothing
                Case Else
                    Restore(O_RTN)
                    '〇取得
                    LF_FILTER_CODE = If(ViewState("LF_FILTER_CODE") Is Nothing, "0", Convert.ToString(ViewState("LF_FILTER_CODE")))
                    LF_SORTING_CODE = If(ViewState("LF_SORTING_CODE") Is Nothing, "0", Convert.ToString(ViewState("LF_SORTING_CODE")))
                    LF_PARAM_DATA = If(ViewState("LF_PARAM_DATA") Is Nothing, "0", Convert.ToString(ViewState("LF_PARAM_DATA")))
            End Select

        End If
    End Sub

    ''' <summary>
    ''' 左リストボックス設定処理
    ''' </summary>
    ''' <param name="ListCode">一覧を作成したい種別</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="Params"><para>一覧作成に必要なパラメータ群</para>
    ''' <para>会社    ：TYPEMODE, ROLE </para>
    ''' <para>取引先  ：TYPEMODE, COMPANY, ORGCODE, ROLE, PERMISSION</para>
    ''' <para>届先    ：TYPEMODE, COMPANY, ORGCODE, TORICODE, CLASSCODE, ROLE, PERMISSION</para>
    ''' <para>部署    ：TYPEMODE, COMPANY, CATEGORYS, ROLE, PERMISSION </para>
    ''' <para>社員    ：TYPEMODE, COMPANY, ORGCODE, STAFFKBN, ROLE, PERMISSION</para>
    ''' <para>統一車番：TYPEMODE, COMPANY, ORGCODE,  ROLE, PERMISSION</para>
    ''' <para>業務車番：COMPANY, ORGCODE, OILTYPE </para>
    ''' <para>品名    ：TYPEMODE, COMPANY, ORG_COMPANY, ORGCODE, OILTYPE, PRODUCT1, ROLE, PERMISSION</para>
    ''' <para>端末　　： </para>
    ''' <para>権限　　：TYPEMODE, COMPANY</para>
    ''' <para>ＵＲＬ　：TYPEMODE</para>
    ''' <para>拡張型　：LISTBOX</para>
    ''' <para>固定一覧：COMPANY, FIXVALUENAME</para>
    ''' </param>
    ''' <remarks>
    ''' <para>左リストボックスを作成する</para>
    ''' <para>ソート・フィルタの設定は一覧作成後に行う</para>
    ''' </remarks>
    Public Sub SetListBox(ByVal ListCode As LIST_BOX_CLASSIFICATION, ByRef O_RTN As String, Optional ByVal Params As Hashtable = Nothing)
        LF_LEFTBOX.Style.Clear()
        LF_SORTING_CODE = C_SORTING_CODE.BOTH
        LF_FILTER_CODE = C_FILTER_CODE.ENABLE
        ListToView(CreateListData(ListCode, O_RTN, Params))
        Backup(ListCode, Params)
    End Sub

    ''' <summary>
    ''' 左リストボックス設定処理
    ''' </summary>
    ''' <param name="ListCode">一覧を作成したい種別</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="Params"><para>一覧作成に必要なパラメータ群</para>
    ''' <para>社員    ：TYPEMODE, COMPANYCODE, ORGCODE, STAFFKBN, ROLE, PERMISSION</para>
    ''' <para>統一車番：TYPEMODE, COMPANYCODE, ORGCODE</para>
    ''' </param>
    ''' <remarks>
    ''' <para>左リストボックスを作成する</para>
    ''' <para>ソート・フィルタの設定は一覧作成後に行う</para>
    ''' </remarks>
    Public Sub SetTableList(ByVal ListCode As LIST_BOX_CLASSIFICATION, ByRef O_RTN As String, Optional ByVal Params As Hashtable = Nothing)
        LF_LEFTBOX.Style.Add(HtmlTextWriterStyle.PaddingBottom, "0")
        LF_LEFTBOX.Style.Add(HtmlTextWriterStyle.PaddingRight, "0")
        LF_LEFTBOX.Style.Add(HtmlTextWriterStyle.Width, "50%")
        LF_LEFTBOX.Style.Add("min-width", "200px")
        LF_LEFTBOX.Style.Add("overflow-y", "hidden")
        'リサイズ用のCSS(Chromeのみワーク、2020年4月のEdgeでもChromiumなので対応できる想定)
        LF_LEFTBOX.Style.Add("overflow-x", "auto")
        LF_LEFTBOX.Style.Add("resize", "horizontal")
        LF_LEFTBOX.Style.Add("max-width", "calc(100vw - 20px)")
        LF_SORTING_CODE = C_SORTING_CODE.HIDE
        LF_FILTER_CODE = C_FILTER_CODE.DISABLE

        Backup(ListCode, Params)

    End Sub

    ''' <summary>
    ''' LeftBoxより名称取得＆チェック
    ''' </summary>
    ''' <param name="ListCode">名称を取得したい種別</param>
    ''' <param name="I_VALUE">名称を取得したいコード値</param>
    ''' <param name="O_TEXT">取得した名称</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="Params"><para>一覧作成に必要なパラメータ群</para>
    ''' <para>会社    ：TYPEMODE, ROLE </para>
    ''' <para>取引先  ：TYPEMODE, COMPANY, ORGCODE, ROLE, PERMISSION</para>
    ''' <para>届先    ：TYPEMODE, COMPANY, ORGCODE, TORICODE, CLASSCODE, ROLE, PERMISSION</para>
    ''' <para>部署    ：TYPEMODE, COMPANY, CATEGORYS, ROLE, PERMISSION </para>
    ''' <para>社員    ：TYPEMODE, COMPANY, ORGCODE, STAFFKBN, ROLE, PERMISSION</para>
    ''' <para>統一車番：TYPEMODE, COMPANY, ORGCODE,  ROLE, PERMISSION</para>
    ''' <para>業務車番：COMPANY, ORGCODE, OILTYPE </para>
    ''' <para>品名    ：TYPEMODE, COMPANY, ORG_COMPANY, ORGCODE, OILTYPE, PRODUCT1, ROLE, PERMISSION</para>
    ''' <para>端末　　： </para>
    ''' <para>権限　　：TYPEMODE, COMPANY</para>
    ''' <para>ＵＲＬ　：TYPEMODE</para>
    ''' <para>拡張型　：LISTBOX</para>
    ''' <para>固定一覧：COMPANY, FIXVALUENAME</para>
    ''' </param>
    ''' <remarks></remarks>
    Public Sub CodeToName(ByVal ListCode As LIST_BOX_CLASSIFICATION, ByRef I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, Optional ByVal Params As Hashtable = Nothing)

        O_TEXT = GetListText(CreateListData(ListCode, O_RTN, Params), I_VALUE, O_RTN)
    End Sub

    ''' <summary>
    ''' 固定値マスタよりサブコードを取得する
    ''' </summary>
    ''' <param name="I_VALUE">名称を取得したいコード値</param>
    ''' <param name="O_TEXT">取得した名称</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="Params">一覧作成に必要なパラメータ群</param>
    ''' <param name="I_SUBCODE" >取得したいサブコード番号</param>
    ''' <remarks></remarks>
    Public Sub CodeToName(ByVal I_VALUE As String, ByRef O_TEXT As String, ByRef O_RTN As String, ByVal Params As Hashtable, Optional ByVal I_SUBCODE As Integer = 2)

        O_TEXT = GetListText(CreateSubCodeList(Params, O_RTN, I_SUBCODE), I_VALUE, O_RTN)
    End Sub
    ''' <summary>
    ''' テーブル表示時
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ActiveTable()
        WF_LEFTMView.ActiveViewIndex = 2
    End Sub
    ''' <summary>
    ''' カレンダー表示時
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ActiveCalendar()
        WF_LEFTMView.ActiveViewIndex = 1
        WF_Calendar.Focus()
    End Sub
    ''' <summary>
    ''' 一覧表示
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ActiveListBox()
        WF_LEFTMView.ActiveViewIndex = 0
        WF_LeftListBox.Focus()
    End Sub
    ''' <summary>
    ''' 左ボックスで選択した情報を取得
    ''' </summary>
    ''' <returns></returns>
    Public Function GetLeftTableValue() As Dictionary(Of String, String)
        If WF_LEFTMView.ActiveViewIndex <> 2 Then
            Return Nothing
        End If
        Dim retVal As New Dictionary(Of String, String)
        retVal.Add(LEFT_TABLE_SELECTED_KEY, Me.hdnLeftTableSelectedKey.Value)
        Dim retArr As New List(Of String)
        retArr.AddRange(WF_TBL_SELECT.Text.Split(C_TABLE_SPLIT.ToCharArray))
        For Each itm In retArr
            Dim fieldValuePair = itm.Split("=".ToCharArray, 2)
            Dim fieldName As String = fieldValuePair(0)
            Dim value As String
            If fieldValuePair.Count > 2 Then
                value = fieldValuePair(1)
            Else
                value = ""
            End If
            retVal.Add(fieldName, value)
        Next
        Return retVal
    End Function

    ''' <summary>
    ''' 左ボックスで指定した値を取得する
    ''' </summary>
    ''' <returns>
    ''' <para>LISTBOX：選択値、選択名称</para>
    ''' <para>CALENAR：選択日付(変換有)、選択日付(無変換)</para>
    ''' <para>TABLE  ：選択値群（選択項目＝選択値）</para>
    ''' </returns>
    ''' <remarks></remarks>
    Public Function GetActiveValue() As String()
        Select Case WF_LEFTMView.ActiveViewIndex
            Case 2
                Dim retArr As New List(Of String)
                retArr.Add(Me.hdnLeftTableSelectedKey.Value)
                retArr.AddRange(WF_TBL_SELECT.Text.Split(C_TABLE_SPLIT.ToCharArray))
                Return retArr.ToArray
            Case 1
                Dim Value As String() = {"", ""}
                Value(0) = WF_Calendar.Text
                Value(1) = WF_Calendar.Text
                If (Value(0) < C_DEFAULT_YMD) Then
                    Value(0) = C_DEFAULT_YMD
                End If
                Return Value
            Case 0
                Dim Value As String() = {"", ""}
                If WF_LeftListBox.SelectedIndex >= 0 Then
                    Value(0) = WF_LeftListBox.SelectedItem.Value
                    Value(1) = WF_LeftListBox.SelectedItem.Text
                End If
                Return Value
        End Select
        Return Nothing
    End Function
    ''' <summary>
    ''' 一覧情報を作成する
    ''' </summary>
    ''' <param name="ListCode">作成する一覧の内容</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="Params"><para>一覧作成に必要なパラメータ群</para>
    ''' <para>会社    ：TYPEMODE, ROLE </para>
    ''' <para>取引先  ：TYPEMODE, COMPANY, ORGCODE, ROLE, PERMISSION</para>
    ''' <para>届先    ：TYPEMODE, COMPANY, ORGCODE, TORICODE, CLASSCODE, ROLE, PERMISSION</para>
    ''' <para>部署    ：TYPEMODE, COMPANY, CATEGORYS, ROLE, PERMISSION </para>
    ''' <para>社員    ：TYPEMODE, COMPANY, ORGCODE, STAFFKBN, ROLE, PERMISSION</para>
    ''' <para>統一車番：TYPEMODE, COMPANY, ORGCODE,  ROLE, PERMISSION</para>
    ''' <para>業務車番：COMPANY, ORGCODE, OILTYPE </para>
    ''' <para>品名    ：TYPEMODE, COMPANY, ORG_COMPANY, ORGCODE, OILTYPE, PRODUCT1, ROLE, PERMISSION</para>
    ''' <para>端末　　： </para>
    ''' <para>権限　　：TYPEMODE, COMPANY</para>
    ''' <para>ＵＲＬ　：TYPEMODE</para>
    ''' <para>拡張型　：LISTBOX</para>
    ''' <para>固定一覧：COMPANY, FIXVALUENAME</para>
    ''' </param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateListData(ByVal ListCode As LIST_BOX_CLASSIFICATION, ByRef O_RTN As String, Optional ByVal Params As Hashtable = Nothing) As ListBox
        If IsNothing(Params) Then
            Params = New Hashtable
        End If
        Dim lbox As ListBox
        Select Case ListCode
            Case LIST_BOX_CLASSIFICATION.LC_COMPANY
                '会社一覧設定
                lbox = CreateCompList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_ORG
                '部署
                lbox = CreateOrg(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_DELFLG
                '削除区分
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "DELFLG"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_ROLE
                '権限コード
                lbox = CreateRoleList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_JURISDICTION
                '所管部
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "JURISDICTION"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_ORDERSTATUS
                '受注進行ステータス
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "ORDERSTATUS"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_CTNTYPE
                'コンテナ記号
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "CTNTYPE"
                lbox = CreateContena(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_CTNNO
                'コンテナ番号
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "CTNNO"
                lbox = CreateContena(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_ITEMCD
                '品目
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "ITEM"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_DEPSTATION,
                 LIST_BOX_CLASSIFICATION.LC_ARRSTATION
                '駅名
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "STATION"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_DEPTRUSTEECD,
                 LIST_BOX_CLASSIFICATION.LC_ARRTRUSTEECD
                '受託人コード(発・着)
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "DEPTRUSTEE"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_STACKFREEKBNCD
                '積空区分
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "STACKFREEKBN"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_SHIPPERCD
                '荷送人
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "SHIPPER"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_JOTDEPBRANCH
                'JOT発店所
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "JOTDEPBRANCH"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_RECONM
                'コンテナマスタ
                lbox = CreateContena(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_REKEJM
                'コンテナ取引先マスタ
                lbox = CreateCtnCustomer(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_ITEM
                ' 品目マスタ
                lbox = CreateItem(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_CLASS
                ' 大中小分類マスタ
                lbox = CreateClass(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_SHIPPER
                ' 荷主マスタ
                lbox = CreateShipper(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_KEKKJM
                ' 営業収入決済条件マスタ
                lbox = CreateInvKesaiKbn(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_KEKSBM
                ' 請求摘要マスタ
                lbox = CreateInvSubCd(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_FIX_VALUE
                ' 固定値マスタ
                lbox = CreateFixParam(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_STATION
                ' 駅マスタ
                lbox = CreateStation(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_FINANCE_ITEN
                ' ファイナンスリース項目
                lbox = CreateFinanceItem(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_USERMST
                ' ユーザーマスタ
                lbox = CreateUser(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_CALENDAR
                'カレンダー
                lbox = Nothing

            Case LIST_BOX_CLASSIFICATION.LC_REPORT
                '帳票マスタ
                lbox = CreateReportList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_MODE
                '処理種別
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "MODE"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_ACCOUNTINGASSETSKBN
                '所管部
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "ACCOUNTINGASSETSKBN"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_SEARCH
                '検索種別
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "SEARCH"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_INVCYCL
                '締め日
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "INVCYCL"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_SORT
                '並び順
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "SORT"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_KEIJOBASE
                '計上ベース
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "KEIJOBASE"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_TRUSTEEKBN
                '受託人指定
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "TRUSTEEKBN"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_BILLINGKBN
                '請求先指定
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "BILLINGKBN"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_ADDSUBKBN
                '加減額表示指定
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "ADDSUBKBN"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_BRANCHBASE
                '処理種別
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "BRANCHBASE"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_DEPARRBASE
                '発着ベース
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "DEPARRBASE"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_STACKFREE
                '積空区分
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "STACKFREE"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_REPORTSETTING
                '出力設定
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "REPORTSETTING"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_REPLACE
                '入れ替え
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "REPLACE"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_ACCOUNTCODE
                '科目コード
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "ACCOUNTCODE"
                lbox = CreateFixValueList(Params, O_RTN)

            Case LIST_BOX_CLASSIFICATION.LC_SEGMENTCODE
                'セグメント
                Params.Item(C_PARAMETERS.LP_FIX_CLASS) = "SEGMENTCODE"
                lbox = CreateFixValueList(Params, O_RTN)

            Case Else
                lbox = CreateFixValueList(Params, O_RTN)
        End Select
        Return lbox
    End Function

    ''' <summary>
    ''' 会社コード一覧を作成する
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateCompList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        Dim typeMode As String = ""
        If Params.Item(C_PARAMETERS.LP_TYPEMODE) Is Nothing Then
            typeMode = Convert.ToString(GL0001CompList.LC_COMPANY_TYPE.ROLE)
        Else
            typeMode = CInt(Params.Item(C_PARAMETERS.LP_TYPEMODE)).ToString
        End If
        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Dim viewFormat = DirectCast([Enum].ToObject(GetType(GL0000.C_VIEW_FORMAT_PATTERN), CInt(dispFormat)), GL0000.C_VIEW_FORMAT_PATTERN)
        Dim listClassComp As String = CInt(LIST_BOX_CLASSIFICATION.LC_COMPANY).ToString

        Dim key As String = ""
        key = typeMode & dispFormat & listClassComp

        If Not LbMap.ContainsKey(key) Then
            Dim paramStYmd As Date = Date.Now
            If Params.Item(C_PARAMETERS.LP_STYMD) IsNot Nothing Then
                paramStYmd = CDate(Params.Item(C_PARAMETERS.LP_STYMD))
            End If
            Dim paramEndYmd As Date = Date.Now
            If Params.Item(C_PARAMETERS.LP_ENDYMD) IsNot Nothing Then
                paramEndYmd = CDate(Params.Item(C_PARAMETERS.LP_ENDYMD))
            End If
            Dim roleCode As String = DirectCast(Parent.Page.Master, LNGMasterPage).ROLE_MAP
            If Params.Item(C_PARAMETERS.LP_ROLE) IsNot Nothing Then
                roleCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_ROLE))
            End If
            Dim defaultSort As String = String.Empty
            If Params.Item(C_PARAMETERS.LP_DEFAULT_SORT) IsNot Nothing Then
                defaultSort = Convert.ToString(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT))
            End If
            '○会社コードListBox設定
            Using CL0001CompList As New GL0001CompList With {
                   .TYPEMODE = typeMode _
                 , .STYMD = paramStYmd _
                 , .ENDYMD = paramEndYmd _
                 , .ROLECODE = roleCode _
                 , .DEFAULT_SORT = defaultSort _
                 , .VIEW_FORMAT = viewFormat
            }
                CL0001CompList.getList()
                Dim lsbx As ListBox = CL0001CompList.LIST
                O_RTN = CL0001CompList.ERR
                LbMap.Add(key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(key), ListBox)
    End Function

    ''' <summary>
    ''' 部署(管理・配属)
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateOrg(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○部署ListBox設定
        Dim Categorys As String() = TryCast(Params.Item(C_PARAMETERS.LP_ORG_CATEGORYS), String())
        Dim Key As String = "-"
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            Key = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        For Each category As String In Categorys
            Key = Key & category
        Next
        ' リスト表示フォーマットの設定(名称・コード・併記)
        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Key = Key & dispFormat & CInt(LIST_BOX_CLASSIFICATION.LC_ORG).ToString

        If Not LbMap.ContainsKey(Key) Then
            Dim defaultSort As String = String.Empty
            If Params.Item(C_PARAMETERS.LP_DEFAULT_SORT) IsNot Nothing Then
                defaultSort = Convert.ToString(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT))
            End If
            Dim paramStYmd As Date = Date.Now
            If Params.Item(C_PARAMETERS.LP_STYMD) IsNot Nothing Then
                paramStYmd = CDate(Params.Item(C_PARAMETERS.LP_STYMD))
            End If
            Dim paramEndYmd As Date = Date.Now
            If Params.Item(C_PARAMETERS.LP_ENDYMD) IsNot Nothing Then
                paramEndYmd = CDate(Params.Item(C_PARAMETERS.LP_ENDYMD))
            End If
            Dim viewFormat = DirectCast([Enum].ToObject(GetType(GL0000.C_VIEW_FORMAT_PATTERN), CInt(dispFormat)), GL0000.C_VIEW_FORMAT_PATTERN)
            ' 会社コード
            Dim campCode As String = ""
            If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
                campCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
            End If
            Dim authWith = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
            If Params.Item(C_PARAMETERS.LP_TYPEMODE) IsNot Nothing Then
                Dim authWithNum As Integer = CInt(Params.Item(C_PARAMETERS.LP_TYPEMODE))
                authWith = DirectCast([Enum].ToObject(GetType(GL0002OrgList.LS_AUTHORITY_WITH), CInt(authWithNum)), GL0002OrgList.LS_AUTHORITY_WITH)
            End If
            Dim roleCode As String = DirectCast(Parent.Page.Master, LNGMasterPage).ROLE_MAP
            If Params.Item(C_PARAMETERS.LP_ROLE) IsNot Nothing Then
                roleCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_ROLE))
            End If
            Dim permission As String = C_PERMISSION.REFERLANCE
            If Params.Item(C_PARAMETERS.LP_PERMISSION) IsNot Nothing Then
                permission = Convert.ToString(Params.Item(C_PARAMETERS.LP_PERMISSION))
            End If
            ' 組織コード
            Dim orgCode As String = DirectCast(Parent.Page.Master, LNGMasterPage).USER_ORG
            If Params.Item(C_PARAMETERS.LP_ORG) IsNot Nothing Then
                orgCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_ORG))
            End If
            Using CL0002OrgList As New GL0002OrgList With {
                  .DEFAULT_SORT = defaultSort _
                , .STYMD = paramStYmd _
                , .ENDYMD = paramEndYmd _
                , .VIEW_FORMAT = viewFormat _
                , .CAMPCODE = campCode _
                , .AUTHWITH = authWith _
                , .Categorys = Categorys _
                , .ROLECODE = roleCode _
                , .PERMISSION = permission _
                , .ORGCODE = orgCode
             }
                CL0002OrgList.getList()
                O_RTN = CL0002OrgList.ERR
                Dim lsbx As ListBox = CL0002OrgList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
    End Function

    ''' <summary>
    ''' コンテナマスタ一覧取得
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateContena(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○ListBox設定
        Dim Key As String = "-"
        If Params.Item(C_PARAMETERS.LP_CTNTYPE) IsNot Nothing Then
            Key = Convert.ToString(Params.Item(C_PARAMETERS.LP_CTNTYPE))
        End If
        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Key = Key & dispFormat & CInt(LIST_BOX_CLASSIFICATION.LC_RECONM).ToString

        If Not LbMap.ContainsKey(Key) Then
            ' コンテナ記号
            Dim CTNType As String = ""
            If Params.Item(C_PARAMETERS.LP_CTNTYPE) IsNot Nothing Then
                CTNType = Convert.ToString(Params.Item(C_PARAMETERS.LP_CTNTYPE))
            End If
            ' 取得対象分類設定(コンテナ記号・番号)
            Dim contenaWith = GL0020ContenaList.LS_CONTENA_WITH.CTN_TYPE
            If Params.Item(C_PARAMETERS.LP_TYPEMODE) IsNot Nothing Then
                Dim contenaWithNum As Integer = CInt(Params.Item(C_PARAMETERS.LP_TYPEMODE))
                contenaWith = DirectCast([Enum].ToObject(GetType(GL0020ContenaList.LS_CONTENA_WITH), CInt(contenaWithNum)), GL0020ContenaList.LS_CONTENA_WITH)
            End If
            Using GL0020ContenaList As New GL0020ContenaList With {
                  .CTNTYPE = CTNType _
                , .CONTENAWITH = contenaWith
             }
                GL0020ContenaList.getList()
                O_RTN = GL0020ContenaList.ERR
                Dim lsbx As ListBox = GL0020ContenaList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
    End Function

    ''' <summary>
    ''' コンテナ取引先マスタ一覧取得
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateCtnCustomer(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○発受託人コードListBox設定
        Dim Key As String = "-"
        If Params.Item(C_PARAMETERS.LP_STATION) IsNot Nothing Then
            Key = Convert.ToString(Params.Item(C_PARAMETERS.LP_STATION))
        End If
        If Params.Item(C_PARAMETERS.LP_TRUSTEECD) IsNot Nothing Then
            Key &= Convert.ToString(Params.Item(C_PARAMETERS.LP_TRUSTEECD))
        End If
        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Key = Key & dispFormat & CInt(LIST_BOX_CLASSIFICATION.LC_REKEJM).ToString

        If Not LbMap.ContainsKey(Key) Then
            ' 発駅コード
            Dim station As String = ""
            If Params.Item(C_PARAMETERS.LP_STATION) IsNot Nothing Then
                station = Convert.ToString(Params.Item(C_PARAMETERS.LP_STATION))
            End If
            ' 受託人コード
            Dim trusteeCd As String = ""
            If Params.Item(C_PARAMETERS.LP_TRUSTEECD) IsNot Nothing Then
                trusteeCd = Convert.ToString(Params.Item(C_PARAMETERS.LP_TRUSTEECD))
            End If
            ' 取得対象分類設定(コード・サブコード)
            Dim customerWith = GL0017CtnCustomerList.LS_CUSTOMER_WITH.TRUSTEE_CD
            If Params.Item(C_PARAMETERS.LP_TYPEMODE) IsNot Nothing Then
                Dim customerWithNum As Integer = CInt(Params.Item(C_PARAMETERS.LP_TYPEMODE))
                customerWith = DirectCast([Enum].ToObject(GetType(GL0017CtnCustomerList.LS_CUSTOMER_WITH), CInt(customerWithNum)), GL0017CtnCustomerList.LS_CUSTOMER_WITH)
            End If
            Using GL0017CtnCustomerList As New GL0017CtnCustomerList With {
                  .STATION = station _
                , .TRUSTEECD = trusteeCd _
                , .CUSTOMERWITH = customerWith
             }
                GL0017CtnCustomerList.getList()
                O_RTN = GL0017CtnCustomerList.ERR
                Dim lsbx As ListBox = GL0017CtnCustomerList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
    End Function

    ''' <summary>
    ''' 品目マスタ一覧取得
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateItem(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○ListBox設定
        Dim Key As String = "-"
        Key = "ITEM"
        Key = Key & CInt(LIST_BOX_CLASSIFICATION.LC_ITEM).ToString

        If Not LbMap.ContainsKey(Key) Then
            Using GL0023ItemList As New GL0023ItemList
                GL0023ItemList.getList()
                O_RTN = GL0023ItemList.ERR
                Dim lsbx As ListBox = GL0023ItemList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
    End Function

    ''' <summary>
    ''' 大中小分類マスタ一覧取得
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateClass(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○大中小分類ListBox設定
        Dim Key As String = "-"
        If Params.Item(C_PARAMETERS.LP_BIGCTNCD) IsNot Nothing Then
            Key = Convert.ToString(Params.Item(C_PARAMETERS.LP_BIGCTNCD))
        End If
        If Params.Item(C_PARAMETERS.LP_MIDDLECTNCD) IsNot Nothing Then
            Key &= Convert.ToString(Params.Item(C_PARAMETERS.LP_MIDDLECTNCD))
        End If
        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Key = Key & dispFormat & CInt(LIST_BOX_CLASSIFICATION.LC_CLASS).ToString

        If Not LbMap.ContainsKey(Key) Then
            ' 大分類
            Dim bigCTNCode As String = ""
            If Params.Item(C_PARAMETERS.LP_BIGCTNCD) IsNot Nothing Then
                bigCTNCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_BIGCTNCD))
            End If
            ' 中分類
            Dim middleCTNCode As String = ""
            If Params.Item(C_PARAMETERS.LP_MIDDLECTNCD) IsNot Nothing Then
                middleCTNCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_MIDDLECTNCD))
            End If
            ' 取得対象分類設定(大・中・小)
            Dim classWith = GL0016ClassList.LS_CLASS_WITH.BIG_CLASS
            If Params.Item(C_PARAMETERS.LP_TYPEMODE) IsNot Nothing Then
                Dim classWithNum As Integer = CInt(Params.Item(C_PARAMETERS.LP_TYPEMODE))
                classWith = DirectCast([Enum].ToObject(GetType(GL0016ClassList.LS_CLASS_WITH), CInt(classWithNum)), GL0016ClassList.LS_CLASS_WITH)
            End If
            Using GL0016ClassList As New GL0016ClassList With {
                  .BIGCTNCD = bigCTNCode _
                , .MIDDLECTNCD = middleCTNCode _
                , .CLASSWITH = classWith
             }
                GL0016ClassList.getList()
                O_RTN = GL0016ClassList.ERR
                Dim lsbx As ListBox = GL0016ClassList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
    End Function

    ''' <summary>
    ''' 荷主マスタ一覧取得
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateShipper(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○ListBox設定
        Dim Key As String = "-"
        Key = "SHIPPER"
        Key = Key & CInt(LIST_BOX_CLASSIFICATION.LC_SHIPPER).ToString

        If Not LbMap.ContainsKey(Key) Then
            Using GL0024ShipperList As New GL0024ShipperList
                GL0024ShipperList.getList()
                O_RTN = GL0024ShipperList.ERR
                Dim lsbx As ListBox = GL0024ShipperList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
    End Function

    ''' <summary>
    ''' 営業収入決済条件マスタ一覧取得
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateInvKesaiKbn(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○請求項目請求書決済区分ListBox設定
        Dim Key As String = "-"
        If Params.Item(C_PARAMETERS.LP_TORICODE) IsNot Nothing Then
            Key = Convert.ToString(Params.Item(C_PARAMETERS.LP_TORICODE))
        End If
        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Key = Key & dispFormat & CInt(LIST_BOX_CLASSIFICATION.LC_KEKKJM).ToString

        If Not LbMap.ContainsKey(Key) Then
            Dim defaultSort As String = String.Empty
            If Params.Item(C_PARAMETERS.LP_DEFAULT_SORT) IsNot Nothing Then
                defaultSort = Convert.ToString(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT))
            End If
            Dim viewFormat = DirectCast([Enum].ToObject(GetType(GL0000.C_VIEW_FORMAT_PATTERN), CInt(dispFormat)), GL0000.C_VIEW_FORMAT_PATTERN)
            ' 取引先コード
            Dim ToriCode As String = ""
            If Params.Item(C_PARAMETERS.LP_TORICODE) IsNot Nothing Then
                ToriCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_TORICODE))
            End If
            ' 請求書提出部店
            Dim InvFilingDept As String = ""
            If Params.Item(C_PARAMETERS.LP_INVFILINGDEPT) IsNot Nothing Then
                InvFilingDept = Convert.ToString(Params.Item(C_PARAMETERS.LP_INVFILINGDEPT))
            End If
            ' 取得対象分類設定(取引先コード・取引先サブコード・営業収入決済区分)
            Dim invoiceWith = GL0018InvKesaiKbnList.LS_INVOICE_WITH.TORICODE
            If Params.Item(C_PARAMETERS.LP_TYPEMODE) IsNot Nothing Then
                Dim invoiceWithNum As Integer = CInt(Params.Item(C_PARAMETERS.LP_TYPEMODE))
                invoiceWith = DirectCast([Enum].ToObject(GetType(GL0018InvKesaiKbnList.LS_INVOICE_WITH), CInt(invoiceWithNum)), GL0018InvKesaiKbnList.LS_INVOICE_WITH)
            End If

            Using GL0018InvKesaiKbnList As New GL0018InvKesaiKbnList With {
                  .DEFAULT_SORT = defaultSort _
                , .VIEW_FORMAT = viewFormat _
                , .TORICODE = ToriCode _
                , .INVFILINGDEPT = InvFilingDept _
                , .INVOICEWITH = invoiceWith
             }
                GL0018InvKesaiKbnList.getList()
                O_RTN = GL0018InvKesaiKbnList.ERR
                Dim lsbx As ListBox = GL0018InvKesaiKbnList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
    End Function

    ''' <summary>
    ''' 請求摘要マスタ一覧取得
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateInvSubCd(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○請求項目請求書決済区分ListBox設定
        Dim Key As String = "-"
        If Params.Item(C_PARAMETERS.LP_TORICODE) IsNot Nothing Then
            Key = Convert.ToString(Params.Item(C_PARAMETERS.LP_TORICODE))
        End If
        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Key = Key & dispFormat & CInt(LIST_BOX_CLASSIFICATION.LC_KEKSBM).ToString

        If Not LbMap.ContainsKey(Key) Then
            Dim defaultSort As String = String.Empty
            If Params.Item(C_PARAMETERS.LP_DEFAULT_SORT) IsNot Nothing Then
                defaultSort = Convert.ToString(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT))
            End If
            Dim viewFormat = DirectCast([Enum].ToObject(GetType(GL0000.C_VIEW_FORMAT_PATTERN), CInt(dispFormat)), GL0000.C_VIEW_FORMAT_PATTERN)
            ' 取引先コード
            Dim ToriCode As String = ""
            If Params.Item(C_PARAMETERS.LP_TORICODE) IsNot Nothing Then
                ToriCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_TORICODE))
            End If

            ' 請求書提出部店
            Dim InvFilingDept As String = ""
            If Params.Item(C_PARAMETERS.LP_INVFILINGDEPT) IsNot Nothing Then
                InvFilingDept = Convert.ToString(Params.Item(C_PARAMETERS.LP_INVFILINGDEPT))
            End If
            ' 請求書決済区分
            Dim InvKesaiKbn As String = ""
            If Params.Item(C_PARAMETERS.LP_INVKESAIKBN) IsNot Nothing Then
                InvKesaiKbn = Convert.ToString(Params.Item(C_PARAMETERS.LP_INVKESAIKBN))
            End If

            Using GL0019InvSubCdList As New GL0019InvSubCdList With {
                  .DEFAULT_SORT = defaultSort _
                , .VIEW_FORMAT = viewFormat _
                , .TORICODE = ToriCode _
                , .INVFILINGDEPT = InvFilingDept _
                , .INVKESAIKBN = InvKesaiKbn
             }
                GL0019InvSubCdList.getList()
                O_RTN = GL0019InvSubCdList.ERR
                Dim lsbx As ListBox = GL0019InvSubCdList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
    End Function

    ''' <summary>
    ''' 権限コード一覧を作成する
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateRoleList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '---20191120追加---_OIS0001USERに利用するため修正
        Dim I_COMP As String = C_DEFAULT_DATAKEY
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            I_COMP = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        Dim I_CLASS As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_CLASSCODE))
        Dim I_STYMD As Date = Date.Now
        If Params.Item(C_PARAMETERS.LP_STYMD) IsNot Nothing Then
            I_STYMD = CDate(Params.Item(C_PARAMETERS.LP_STYMD))
        End If
        Dim I_ENDYMD As Date = Date.Now
        If Params.Item(C_PARAMETERS.LP_ENDYMD) IsNot Nothing Then
            I_ENDYMD = CDate(Params.Item(C_PARAMETERS.LP_ENDYMD))
        End If
        Dim key As String = I_COMP & If(I_CLASS = String.Empty, "ALLVALUE", I_CLASS)
        If Not LbMap.ContainsKey(key) Then
            Dim lsbx As New ListBox

            Using GL0012RoleList As New GL0012RoleList With {
                   .CAMPCODE = I_COMP _
                 , .OBJCODE = I_CLASS _
                 , .STYMD = I_STYMD _
                 , .ENDYMD = I_ENDYMD _
                 , .LIST = lsbx
                }
                GL0012RoleList.getList()
                O_RTN = GL0012RoleList.ERR
                lsbx = GL0012RoleList.LIST
                Dim cnt As Long = lsbx.Rows
                LbMap.Add(key, lsbx)
            End Using
        End If

        Return DirectCast(LbMap.Item(key), ListBox)
    End Function

    ''' <summary>
    ''' 基地コード一覧を作成する
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateBaseList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        Dim I_COMP = If(Params.Item(C_PARAMETERS.LP_COMPANY), C_DEFAULT_DATAKEY)

        Dim key As String = Convert.ToString(I_COMP)
        If Not LbMap.ContainsKey(key) Then
            Dim lsbx As New ListBox

            Using GL0014PLANTList As New GL0014PLANTList With {
                   .CAMPCODE = key _
                 , .LIST = lsbx
                }
                GL0014PLANTList.getList()
                O_RTN = GL0014PLANTList.ERR
                lsbx = GL0014PLANTList.LIST
                Dim cnt As Long = lsbx.Rows
                LbMap.Add(key, lsbx)
            End Using
        End If

        Return DirectCast(LbMap.Item(key), ListBox)
    End Function

    ''' <summary>
    ''' 貨物駅一覧を作成する
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateStationList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        Dim I_COMP As String = C_DEFAULT_DATAKEY
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            I_COMP = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        Dim I_CLASS As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_FIX_CLASS))
        Dim I_DEPARRFLG As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_STATION))
        Dim key As String = I_COMP & If(I_CLASS = String.Empty, "ALLVALUE", I_CLASS)
        If Not LbMap.ContainsKey(key) Then
            Dim lsbx As New ListBox

            Using GL0015StationList As New GL0015StationList With {
                   .CAMPCODE = I_COMP _
                 , .CLAS = I_CLASS _
                 , .DEPARRSTATIONFLG = I_DEPARRFLG _
                 , .LIST = lsbx
                }
                GL0015StationList.getList()
                O_RTN = GL0015StationList.ERR
                lsbx = GL0015StationList.LIST
                LbMap.Add(key, lsbx)
            End Using
        End If

        Return DirectCast(LbMap.Item(key), ListBox)
    End Function

    ''' <summary>
    ''' 固定マスタ一覧を作成する
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateFixParam(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○固定値マスタListBox設定
        Dim Key As String = "-"
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            Key = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        If Params.Item(C_PARAMETERS.LP_FIX_CLASS) IsNot Nothing Then
            Key &= Convert.ToString(Params.Item(C_PARAMETERS.LP_FIX_CLASS))
        End If
        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Key = Key & dispFormat & CInt(LIST_BOX_CLASSIFICATION.LC_FIX_VALUE).ToString

        If Not LbMap.ContainsKey(Key) Then
            ' 会社コード
            Dim CampCode As String = C_DEFAULT_DATAKEY
            If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
                CampCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
            End If
            ' クラス(取得したい項目名)
            Dim ObjCode As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_FIX_CLASS))

            Using GL0021FixParamList As New GL0021FixParamList With {
                   .CAMPCODE = CampCode _
                 , .OBJCODE = ObjCode
                 }
                GL0021FixParamList.getList()
                O_RTN = GL0021FixParamList.ERR
                Dim lsbx As ListBox = GL0021FixParamList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If

        Return DirectCast(LbMap.Item(key), ListBox)
    End Function

    ''' <summary>
    ''' 駅マスタ一覧取得
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateStation(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○駅マスタListBox設定
        Dim Key As String = "-"
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            Key = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        If Params.Item(C_PARAMETERS.LP_ORG) IsNot Nothing Then
            Key &= Convert.ToString(Params.Item(C_PARAMETERS.LP_ORG))
        End If
        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Key = Key & dispFormat & CInt(LIST_BOX_CLASSIFICATION.LC_KEKKJM).ToString

        If Not LbMap.ContainsKey(Key) Then
            ' 会社コード
            Dim CampCode As String = C_DEFAULT_DATAKEY
            If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
                CampCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
            End If
            ' 組織コード
            Dim OrgCode As String = ""
            If Params.Item(C_PARAMETERS.LP_ORG) IsNot Nothing Then
                OrgCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_ORG))
            End If

            Using GL0022StationList As New GL0022StationList With {
                  .CAMPCODE = CampCode _
                , .ORGCODE = OrgCode
             }
                GL0022StationList.getList()
                O_RTN = GL0022StationList.ERR
                Dim lsbx As ListBox = GL0022StationList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
    End Function

    ''' <summary>
    ''' ファイナンスリース項目一覧取得
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateFinanceItem(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○ ファイナンスリース項目ListBox設定
        Dim Key As String = "-"
        If Params.Item(C_PARAMETERS.LP_TYPEMODE) IsNot Nothing Then
            Key = Convert.ToString(Params.Item(C_PARAMETERS.LP_TYPEMODE))
        End If
        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Key = Key & dispFormat & CInt(LIST_BOX_CLASSIFICATION.LC_FINANCE_ITEN).ToString

        If Not LbMap.ContainsKey(Key) Then
            Dim defaultSort As String = String.Empty
            If Params.Item(C_PARAMETERS.LP_DEFAULT_SORT) IsNot Nothing Then
                defaultSort = Convert.ToString(Params.Item(C_PARAMETERS.LP_DEFAULT_SORT))
            End If
            Dim viewFormat = DirectCast([Enum].ToObject(GetType(GL0000.C_VIEW_FORMAT_PATTERN), CInt(dispFormat)), GL0000.C_VIEW_FORMAT_PATTERN)
            ' 計上年月
            Dim KeijyoYM As String = ""
            If Params.Item(C_PARAMETERS.LP_KEIJYOYM) IsNot Nothing Then
                KeijyoYM = Convert.ToString(Params.Item(C_PARAMETERS.LP_KEIJYOYM))
            End If
            ' 取得対象分類設定(計上支店)
            Dim FinanceItemWith = GL0025FinanceItemList.LS_FINANCEITEM_WITH.ORG_CD
            If Params.Item(C_PARAMETERS.LP_TYPEMODE) IsNot Nothing Then
                Dim FinanceItemWithNum As Integer = CInt(Params.Item(C_PARAMETERS.LP_TYPEMODE))
                FinanceItemWith = DirectCast([Enum].ToObject(GetType(GL0025FinanceItemList.LS_FINANCEITEM_WITH), CInt(FinanceItemWithNum)), GL0025FinanceItemList.LS_FINANCEITEM_WITH)
            End If

            Using GL0025FinanceItemList As New GL0025FinanceItemList With {
                  .DEFAULT_SORT = defaultSort _
                , .VIEW_FORMAT = viewFormat _
                , .KEIJYOYM = KeijyoYM _
                , .FINANCEITEMWITH = FinanceItemWith
             }
                GL0025FinanceItemList.getList()
                O_RTN = GL0025FinanceItemList.ERR
                Dim lsbx As ListBox = GL0025FinanceItemList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
    End Function

    ''' <summary>
    ''' ユーザーマスタ一覧取得
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateUser(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○ユーザーマスタListBox設定
        Dim Key As String = "-"
        If Params.Item(C_PARAMETERS.LP_STYMD) IsNot Nothing Then
            Key &= Convert.ToString(Params.Item(C_PARAMETERS.LP_STYMD))
        End If
        If Params.Item(C_PARAMETERS.LP_APPROVALFLG1) IsNot Nothing Then
            Key &= Convert.ToString(Params.Item(C_PARAMETERS.LP_APPROVALFLG1))
        End If
        If Params.Item(C_PARAMETERS.LP_APPROVALFLG2) IsNot Nothing Then
            Key &= Convert.ToString(Params.Item(C_PARAMETERS.LP_APPROVALFLG2))
        End If
        If Params.Item(C_PARAMETERS.LP_USERID) IsNot Nothing Then
            Key &= Convert.ToString(Params.Item(C_PARAMETERS.LP_USERID))
        End If
        If Params.Item(C_PARAMETERS.LP_ORG) IsNot Nothing Then
            Key &= Convert.ToString(Params.Item(C_PARAMETERS.LP_ORG))
        End If

        Dim dispFormat As String = ""
        If Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT) Is Nothing Then
            dispFormat = CInt(GL0001CompList.C_VIEW_FORMAT_PATTERN.NAMES).ToString
        Else
            dispFormat = Convert.ToString(Params.Item(C_PARAMETERS.LP_DISPLAY_FORMAT))
        End If
        Key = Key & dispFormat & CInt(LIST_BOX_CLASSIFICATION.LC_KEKKJM).ToString

        If Not LbMap.ContainsKey(Key) Then
            ' 開始年月日
            Dim StYmd As Date = Convert.ToDateTime(C_DEFAULT_YMD)
            If Params.Item(C_PARAMETERS.LP_STYMD) IsNot Nothing Then
                StYmd = Convert.ToDateTime(Params.Item(C_PARAMETERS.LP_STYMD))
            End If
            '' 承認権限ロール(第一承認者)
            'Dim ApprovalId_1 As String = ""
            'If Params.Item(C_PARAMETERS.LP_APPROVALFLG1) IsNot Nothing Then
            '    If Convert.ToString(Params.Item(C_PARAMETERS.LP_APPROVALFLG1)) = "1" Then
            '        ApprovalId_1 = GL0026UserList.C_APPROVALID.ROLE_1
            '    End If
            'End If
            '' 承認権限ロール(最終承認者)
            'Dim ApprovalId_2 As String = ""
            'If Params.Item(C_PARAMETERS.LP_APPROVALFLG2) IsNot Nothing Then
            '    If Convert.ToString(Params.Item(C_PARAMETERS.LP_APPROVALFLG2)) = "1" Then
            '        ApprovalId_2 = GL0026UserList.C_APPROVALID.ROLE_2
            '    End If
            'End If
            ' ユーザーID
            Dim userId As String = ""
            If Params.Item(C_PARAMETERS.LP_USERID) IsNot Nothing Then
                userId = Convert.ToString(Params.Item(C_PARAMETERS.LP_USERID))
            End If
            ' 組織コード
            Dim orgCode As String = ""
            If Params.Item(C_PARAMETERS.LP_ORG) IsNot Nothing Then
                orgCode = Convert.ToString(Params.Item(C_PARAMETERS.LP_ORG))
            End If

            Using GL0026UserList As New GL0026UserList With {
                  .STYMD = StYmd _
                , .USERID = userId _
                , .ORGCODE = orgCode
             }
                GL0026UserList.getList()
                O_RTN = GL0026UserList.ERR
                Dim lsbx As ListBox = GL0026UserList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
    End Function

    ''' <summary>
    ''' 帳票マスタ一覧取得
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateReportList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        '○ListBox設定
        Dim Key As String = "-"
        Key = "REPORT"
        Key = Key & CInt(LIST_BOX_CLASSIFICATION.LC_REPORT).ToString

        If Not LbMap.ContainsKey(Key) Then
            Using GL0027ReportList As New GL0027ReportList
                GL0027ReportList.getList()
                O_RTN = GL0027ReportList.ERR
                Dim lsbx As ListBox = GL0027ReportList.LIST
                LbMap.Add(Key, lsbx)
            End Using
        End If
        Return DirectCast(LbMap.Item(Key), ListBox)
    End Function

    ''' <summary>
    ''' ListBox設定共通サブ
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks>固定値一覧情報からリストボックスに表示する固定値を取得する</remarks>
    Protected Function CreateFixValueList(ByVal Params As Hashtable, ByRef O_RTN As String) As ListBox
        Dim I_COMP As String = C_DEFAULT_DATAKEY
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            I_COMP = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        Dim I_CLASS As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_FIX_CLASS))
        Dim key As String = I_COMP & If(I_CLASS = String.Empty, "ALLVALUE", I_CLASS)
        If Not LbMap.ContainsKey(key) Then
            Dim lsbx As New ListBox

            Using GS0007FIXVALUElst As New GS0007FIXVALUElst With {
                   .CAMPCODE = I_COMP _
                 , .CLAS = I_CLASS _
                 , .LISTBOX1 = lsbx
                }
                'FixValue抽出用の追加条件付与
                If Params.ContainsKey(C_PARAMETERS.LP_ADDITINALCONDITION) AndAlso
                   Convert.ToString(Params.Item(C_PARAMETERS.LP_ADDITINALCONDITION)) <> "" Then
                    GS0007FIXVALUElst.ADDITIONAL_CONDITION = Convert.ToString(Params.Item(C_PARAMETERS.LP_ADDITINALCONDITION))
                End If

                'FixValue抽出用のソート条件付与
                If Params.ContainsKey(C_PARAMETERS.LP_ADDITINALSORTORDER) AndAlso
                   Convert.ToString(Params.Item(C_PARAMETERS.LP_ADDITINALSORTORDER)) <> "" Then
                    GS0007FIXVALUElst.ADDITIONAL_SORT_ORDER = Convert.ToString(Params.Item(C_PARAMETERS.LP_ADDITINALSORTORDER))
                End If
                GS0007FIXVALUElst.GS0007FIXVALUElst()
                O_RTN = GS0007FIXVALUElst.ERR
                lsbx = GS0007FIXVALUElst.LISTBOX1
                LbMap.Add(key, lsbx)
            End Using
        End If

        Return DirectCast(LbMap.Item(key), ListBox)
    End Function

    ''' <summary>
    ''' Datatable設定共通サブ
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks>固定値一覧情報からリストボックスに表示する固定値を取得する</remarks>
    Protected Function CreateFixValueTable(ByVal Params As Hashtable, ByRef O_RTN As String) As DataTable
        Dim I_COMP As String = C_DEFAULT_DATAKEY
        Dim retDt As DataTable = Nothing
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            I_COMP = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        Dim I_CLASS As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_FIX_CLASS))
        Dim key As String = I_COMP & If(I_CLASS = String.Empty, "ALLVALUE", I_CLASS)
        If Not LbMap.ContainsKey(key) Then
            Dim lsbx As New ListBox

            Using GS0007FIXVALUElst As New GS0007FIXVALUElst With {
                   .CAMPCODE = I_COMP _
                 , .CLAS = I_CLASS _
                 , .LISTBOX1 = lsbx
                }
                'FixValue抽出用の追加条件付与
                If Params.ContainsKey(C_PARAMETERS.LP_ADDITINALCONDITION) AndAlso
                   Convert.ToString(Params.Item(C_PARAMETERS.LP_ADDITINALCONDITION)) <> "" Then
                    GS0007FIXVALUElst.ADDITIONAL_CONDITION = Convert.ToString(Params.Item(C_PARAMETERS.LP_ADDITINALCONDITION))
                End If
                'FixValue抽出用のソート条件付与
                If Params.ContainsKey(C_PARAMETERS.LP_ADDITINALSORTORDER) AndAlso
                   Convert.ToString(Params.Item(C_PARAMETERS.LP_ADDITINALSORTORDER)) <> "" Then
                    GS0007FIXVALUElst.ADDITIONAL_SORT_ORDER = Convert.ToString(Params.Item(C_PARAMETERS.LP_ADDITINALSORTORDER))
                End If
                retDt = GS0007FIXVALUElst.GS0007FIXVALUETbl()
                O_RTN = GS0007FIXVALUElst.ERR
            End Using
        End If

        Return retDt
    End Function

    ''' <summary>
    ''' コードからサブコードを取得する
    ''' </summary>
    ''' <param name="Params">取得用パラメータ</param>
    ''' <param name="O_RTN">成功可否</param>
    ''' <param name="I_SUBCODE">サブコード番号</param>
    ''' <returns>作成した一覧情報</returns>
    ''' <remarks></remarks>
    Protected Function CreateSubCodeList(ByVal Params As Hashtable, ByRef O_RTN As String, ByVal I_SUBCODE As Integer) As ListBox
        Dim I_CLASS As String = Convert.ToString(Params.Item(C_PARAMETERS.LP_FIX_CLASS))
        Dim I_COMP As String = C_DEFAULT_DATAKEY
        If Params.Item(C_PARAMETERS.LP_COMPANY) IsNot Nothing Then
            I_COMP = Convert.ToString(Params.Item(C_PARAMETERS.LP_COMPANY))
        End If
        Dim I_KEY As String = I_CLASS & I_SUBCODE
        If Not LbMap.ContainsKey(I_KEY) Then
            Using GS0007FIXVALUElst As New GS0007FIXVALUElst
                Dim lsbx As New ListBox
                GS0007FIXVALUElst.CAMPCODE = I_COMP
                GS0007FIXVALUElst.CLAS = I_CLASS
                Select Case I_SUBCODE
                    Case 3
                        GS0007FIXVALUElst.LISTBOX3 = lsbx
                    Case 4
                        GS0007FIXVALUElst.LISTBOX4 = lsbx
                    Case 5
                        GS0007FIXVALUElst.LISTBOX5 = lsbx
                    Case Else
                        GS0007FIXVALUElst.LISTBOX2 = lsbx
                End Select
                GS0007FIXVALUElst.GS0007FIXVALUElst()
                O_RTN = GS0007FIXVALUElst.ERR
                Select Case I_SUBCODE
                    Case 3
                        lsbx = GS0007FIXVALUElst.LISTBOX3
                    Case 4
                        lsbx = GS0007FIXVALUElst.LISTBOX4
                    Case 5
                        lsbx = GS0007FIXVALUElst.LISTBOX5
                    Case Else
                        lsbx = GS0007FIXVALUElst.LISTBOX2
                End Select
                LbMap.Add(I_KEY, lsbx)
            End Using
        End If

        Return DirectCast(LbMap.Item(I_KEY), ListBox)
    End Function

    ''' <summary>
    ''' リスト検索
    ''' </summary>
    ''' <param name="I_LISTBOX">検索するリストボックス</param>
    ''' <param name="I_VALUE">検索するKEY</param>
    ''' <param name="O_RTN">成否判定　00000：成功　それ以外：失敗</param>
    ''' <returns >検索結果の値</returns>
    ''' <remarks></remarks>
    Protected Function GetListText(ByVal I_LISTBOX As ListBox, ByVal I_VALUE As String, ByRef O_RTN As String) As String
        O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
        '空なら探さない
        If IsNothing(I_LISTBOX) Then
            O_RTN = C_MESSAGE_NO.NORMAL
            Return String.Empty
        End If
        For Each item As ListItem In I_LISTBOX.Items
            If item.Value = I_VALUE Then
                O_RTN = C_MESSAGE_NO.NORMAL
                Return item.Text
                Exit For
            End If
        Next
        Return String.Empty

    End Function
    ''' <summary>
    ''' 表示用一覧に追加する
    ''' </summary>
    ''' <param name="box">設定情報</param>
    ''' <remarks></remarks>
    Protected Friend Sub ListToView(ByVal box As ListBox)
        WF_LeftListBox.Items.Clear()
        '空なら設定しない
        If IsNothing(box) Then
            Exit Sub
        End If
        '設定項目があるなら設定する
        For Each item As ListItem In box.Items
            WF_LeftListBox.Items.Add(item)
        Next
    End Sub
    ''' <summary>
    ''' 選択情報の保持
    ''' </summary>
    ''' <param name="SELECT_VALUE"></param>
    ''' <param name="PARAMS"></param>
    ''' <remarks></remarks>
    Protected Sub Backup(ByVal SELECT_VALUE As LIST_BOX_CLASSIFICATION, ByVal PARAMS As Hashtable)
        '〇EXTRA＿LISTはTABLE化する
        If Not IsNothing(PARAMS(C_PARAMETERS.LP_LIST)) Then
            Dim list As ListBox = DirectCast(PARAMS(C_PARAMETERS.LP_LIST), ListBox)
            Dim htbl As New Hashtable
            For Each item As ListItem In list.Items
                htbl.Add(item.Value, item.Text)
            Next
            PARAMS(C_PARAMETERS.LP_LIST) = htbl
        End If

        ViewState.Add("LF_PARAMS", PARAMS)
        ViewState.Add("LF_LIST_SELECT", CInt(SELECT_VALUE).ToString)
    End Sub
    ''' <summary>
    ''' 保持した情報の反映
    ''' </summary>
    ''' <param name="O_RTN"></param>
    ''' <remarks></remarks>
    Protected Sub Restore(ByRef O_RTN As String)

        If Not IsNothing(ViewState("LF_LIST_SELECT")) Then
            Dim listClass = DirectCast([Enum].ToObject(GetType(LIST_BOX_CLASSIFICATION), CInt(ViewState("LF_LIST_SELECT"))), LIST_BOX_CLASSIFICATION)
            If WF_LEFTMView.ActiveViewIndex = 2 Then
                SetTableList(listClass, O_RTN, DirectCast(ViewState("LF_PARAMS"), Hashtable))
            ElseIf WF_LEFTMView.ActiveViewIndex = 0 Then
                Dim params As Hashtable = DirectCast(ViewState("LF_PARAMS"), Hashtable)
                '〇EXTRA＿LISTはLISTBOX化する
                If Not IsNothing(params(C_PARAMETERS.LP_LIST)) Then
                    Dim list As New ListBox
                    Dim htbl As Hashtable = DirectCast(params(C_PARAMETERS.LP_LIST), Hashtable)
                    For Each key As String In htbl.Keys
                        list.Items.Add(New ListItem(Convert.ToString(htbl.Item(key)), key))
                    Next
                    params(C_PARAMETERS.LP_LIST) = list
                End If
                SetListBox(listClass, O_RTN, DirectCast(ViewState("LF_PARAMS"), Hashtable))
            End If
        End If
    End Sub
#Region "左ボックスのテーブル表処理関連"
    ''' <summary>
    ''' テーブルオブジェクト展開
    ''' </summary>
    ''' <param name="leftTableDefs">カラム定義</param>
    ''' <param name="outArea">出力先(Panel)コントロール</param>
    Private Sub MakeTableObject(ByVal leftTableDefs As List(Of LeftTableDefItem), ByVal srcTbl As DataTable, outArea As Panel)

        '●項目定義取得
        Dim outTHCell = New TableHeaderCell With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim lenghtFix As Integer = 0
        Dim leftFixAll As Integer = 32
        Dim rightLengthFixAll As Integer = 0

        'ソートキー領域作成
        Dim sortItemId As String = "hdnListSortValue" & outArea.Page.Form.ClientID & outArea.ID
        Dim sortValue As String = ""
        Dim sortItems As New HiddenField With {.ID = sortItemId, .ViewStateMode = UI.ViewStateMode.Disabled}
        If outArea.Page.Request.Form.GetValues(sortItemId) IsNot Nothing Then
            sortValue = outArea.Page.Request.Form.GetValues(sortItemId)(0)
        End If
        sortItems.Value = sortValue
        outArea.Controls.Add(sortItems)
        'テーブル全体のタグ
        Dim tableObj As New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}
        tableObj.Attributes.Add("class", "leftTable")
        ' ヘッダー作成
        Dim wholeHeaderWrapper As New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}
        wholeHeaderWrapper.Attributes.Add("class", "leftTableHeaderWrapper")
        Dim wholeHeader As New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}
        Dim keyFieldName As String = ""
        wholeHeader.Attributes.Add("class", "leftTableHeader")
        For Each leftTableDef In leftTableDefs
            'データテーブルに対象カラムが含まれていない場合はスキップ
            If srcTbl IsNot Nothing AndAlso srcTbl.Columns.Contains(leftTableDef.FieldName) = False Then
                leftTableDef.HasDtColumn = False
                Continue For
            End If

            If keyFieldName = "" AndAlso leftTableDef.KeyField Then
                keyFieldName = leftTableDef.FieldName
            End If

            Dim headerCell As New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}
            Dim headerCellValue As New HtmlGenericControl("span") With {.ViewStateMode = UI.ViewStateMode.Disabled}
            headerCellValue.Attributes.Add("data-fieldname", leftTableDef.FieldName)
            headerCellValue.InnerHtml = leftTableDef.DispFieldName
            lenghtFix = leftTableDef.Length * 16
            If leftTableDef.IsNumericField Then
                headerCellValue.Attributes.Add("data-isnumfield", "1")
            End If

            If lenghtFix = 0 Then
                headerCell.Style.Add("display", "none")
            Else
                headerCell.Style.Add("width", lenghtFix.ToString & "px")
                headerCell.Style.Add("min-width", lenghtFix.ToString & "px")
            End If

            headerCell.Controls.Add(headerCellValue)
            wholeHeader.Controls.Add(headerCell)
        Next leftTableDef
        'キーフィールド設定が無い場合は最左のフィールドをキーとする
        If keyFieldName = "" Then
            keyFieldName = (From val In leftTableDefs Where val.HasDtColumn).FirstOrDefault.FieldName
        End If

        wholeHeaderWrapper.Controls.Add(wholeHeader)
        tableObj.Controls.Add(wholeHeaderWrapper)
        ' データ
        Dim scrDr As DataRow = Nothing
        Dim wholeDataRowWrapper As New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}
        wholeDataRowWrapper.Attributes.Add("class", "leftTableDataWrapper")
        Dim wholeDataRow As HtmlGenericControl
        Dim dataCell As HtmlGenericControl
        Dim dataCellValue As HtmlGenericControl
        Dim keyValue As String = ""
        'Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Dim dicFieldValues As Dictionary(Of String, String)
        'Dim base64Str As String = ""
        'Dim noConpressionByte As Byte()

        For i As Integer = 0 To srcTbl.Rows.Count - 1
            scrDr = srcTbl(i)
            dicFieldValues = New Dictionary(Of String, String)
            wholeDataRow = New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}

            For Each leftTableDef In leftTableDefs
                If leftTableDef.HasDtColumn = False Then
                    Continue For
                End If
                dataCell = New HtmlGenericControl("div") With {.ViewStateMode = UI.ViewStateMode.Disabled}
                dataCellValue = New HtmlGenericControl("span") With {.ViewStateMode = UI.ViewStateMode.Disabled}

                Dim fieldName As String = leftTableDef.FieldName
                Dim fieldValue As String = Convert.ToString(scrDr(fieldName))
                If leftTableDef.FontSize <> "" Then
                    dataCellValue.Style.Add(HtmlTextWriterStyle.FontSize, leftTableDef.FontSize)
                    dataCellValue.Style.Add(HtmlTextWriterStyle.Height, "20px")
                    dataCellValue.Style.Add(HtmlTextWriterStyle.OverflowY, "hidden")
                    If leftTableDef.MarginTop <> "" Then
                        dataCellValue.Style.Add(HtmlTextWriterStyle.MarginTop, leftTableDef.MarginTop)
                    End If
                End If
                dataCellValue.InnerHtml = fieldValue
                dicFieldValues.Add(fieldName, fieldValue)
                'テーブルセルのサイズ
                If leftTableDef.Length * 16 = 0 Then
                    dataCell.Style.Add("display", "none")
                Else
                    Dim cellWidth As String = (leftTableDef.Length * 16).ToString
                    dataCell.Style.Add("width", cellWidth & "px")
                    dataCell.Style.Add("min-width", cellWidth & "px")
                    If leftTableDef.Align <> "" Then
                        Dim alignSetting = ""
                        If leftTableDef.Align.ToUpper = "RIGHT" Then
                            alignSetting = "flex-end"
                        End If
                        If leftTableDef.Align.ToUpper = "CENTER" Then
                            alignSetting = "center"
                        End If
                        If alignSetting <> "" Then
                            dataCell.Style.Add("justify-content", alignSetting)
                        End If
                    End If
                End If

                dataCell.Attributes.Add("data-fieldname", leftTableDef.FieldName)
                dataCell.Controls.Add(dataCellValue)
                wholeDataRow.Controls.Add(dataCell)
            Next leftTableDef

            keyValue = Convert.ToString(scrDr(keyFieldName))
            ''クラスをシリアライズ
            'Using ms As New IO.MemoryStream()
            '    formatter.Serialize(ms, dicFieldValues)
            '    noConpressionByte = ms.ToArray
            'End Using
            ''圧縮シリアライズしたByteデータを圧縮し圧縮したByteデータをBase64に変換
            'Using ms As New IO.MemoryStream(),
            '  ds As New IO.Compression.DeflateStream(ms, IO.Compression.CompressionMode.Compress, True)
            '    ds.Write(noConpressionByte, 0, noConpressionByte.Length)
            '    ds.Close()
            '    Dim byteDat = ms.ToArray
            '    base64Str = Convert.ToBase64String(byteDat, 0, byteDat.Length, Base64FormattingOptions.None)
            'End Using
            Dim fieldValuesStr = String.Join(C_TABLE_SPLIT, (From x In dicFieldValues Select String.Format("{0}={1}", x.Key, x.Value)))
            wholeDataRow.Style.Add("order", (i + 1).ToString)
            wholeDataRow.Attributes.Add("data-initorder", (i + 1).ToString)
            wholeDataRow.Attributes.Add("data-key", keyValue)
            wholeDataRow.Attributes.Add("data-values", fieldValuesStr)
            wholeDataRow.Attributes.Add("onclick", "WF_TableF_DbClick(this);")
            If srcTbl.Rows.Count - 1 = i Then
                wholeDataRow.Attributes.Add("class", "leftTableDataRow lastRow")
            Else
                wholeDataRow.Attributes.Add("class", "leftTableDataRow")
            End If
            wholeDataRowWrapper.Controls.Add(wholeDataRow)
        Next i

        tableObj.Controls.Add(wholeDataRowWrapper)
        outArea.Controls.Add(tableObj)
        Dim style As New HtmlGenericControl("style") With {.ViewStateMode = UI.ViewStateMode.Disabled}
        'Edgeで背面のdivContensbox横スクロールBox効くので抑止(leftBox表示中は)
        style.InnerHtml = "#divContensbox {overflow:hidden;}"
        outArea.Controls.Add(style)
    End Sub
    ''' <summary>
    ''' 左ボックス用テーブルの出力フィールド定義(1カラム分)
    ''' </summary>
    Public Class LeftTableDefItem
        ''' <summary>
        ''' フィールド名
        ''' </summary>
        ''' <returns></returns>
        Public Property FieldName As String
        ''' <summary>
        ''' 画面表示フィールド名
        ''' </summary>
        ''' <returns></returns>
        Public Property DispFieldName As String
        ''' <summary>
        ''' 表示幅
        ''' </summary>
        ''' <returns></returns>
        Public Property Length As Integer
        ''' <summary>
        ''' 参照テーブルに対象のフィールド名を保持しているか
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>描画処理で使用するので設定の使う側は設定の意識不要</remarks>
        Public Property HasDtColumn As Boolean = True
        Public Property TextAlign As StyleCollection
        ''' <summary>
        ''' キーフィールド設定、選択したキーとなるフィールド（True：キー、False：非キー）
        ''' 未設定の場合、表示上最左（先頭列がキーとなる）複数ある場合は１つのみ
        ''' </summary>
        ''' <returns></returns>
        Public Property KeyField As Boolean = False
        ''' <summary>
        ''' テキストの表示位置設定（"left","right","center"等を設定）
        ''' </summary>
        ''' <returns></returns>
        Public Property Align As String = ""
        ''' <summary>
        ''' 数字フィールド（True:数字フィールド,False:通常フィールド）
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>一旦未使用</remarks>
        Public Property IsNumericField As Boolean = False
        ''' <summary>
        ''' 個別フォントサイズ（未指定時は設定しない）
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>これを指定した場合、縦枠は広がらない</remarks>
        Public Property FontSize As String = ""
        ''' <summary>
        ''' 個別フォントサイズを指定時に上位置微調整の為使用(マイナスしていすると上にずれます)
        ''' </summary>
        ''' <returns></returns>
        Public Property MarginTop As String = ""
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispFieldName">画面表示フィールド名</param>
        ''' <param name="length">幅</param>
        ''' <param name="align">テキスト表示位置</param>
        ''' <param name="keyField">キーフィールド</param>
        Public Sub New(fieldName As String, dispFieldName As String, length As Integer, align As String, keyField As Boolean)
            Me.FieldName = fieldName
            Me.DispFieldName = dispFieldName
            Me.Length = length
            Me.Align = align
            Me.KeyField = keyField
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispFieldName">画面表示フィールド名</param>
        ''' <param name="length">幅</param>
        ''' <param name="align">テキスト表示位置</param>
        Public Sub New(fieldName As String, dispFieldName As String, length As Integer, align As String)
            Me.New(fieldName, dispFieldName, length, align, False)
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="DispFieldName">画面表示カラム</param>
        ''' <param name="Length">サイズ</param>
        ''' <param name = "keyField" > キーフィールド</param>
        Public Sub New(fieldName As String, dispFieldName As String, length As Integer, keyField As Boolean)
            Me.New(fieldName, dispFieldName, length, "", keyField)
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="DispFieldName">画面表示カラム</param>
        ''' <param name="Length">サイズ</param>
        Public Sub New(fieldName As String, dispFieldName As String, length As Integer)
            Me.New(fieldName, dispFieldName, length, "")
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispFieldName">画面表示カラム</param>
        ''' <param name="keyField">キーフィールド</param>
        Public Sub New(fieldName As String, dispFieldName As String, keyField As Boolean)
            Me.New(fieldName, dispFieldName, 6, "", keyField)
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispFieldName">画面表示カラム</param>
        Public Sub New(fieldName As String, dispFieldName As String)
            Me.New(fieldName, dispFieldName, 6)
        End Sub
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="param"></param>
        Public Sub New(ParamArray param() As String)
            Me.FieldName = param(0)
            Me.DispFieldName = param(1)
            If param.Length = 3 Then
                Me.Length = CInt(param(3))
            End If
        End Sub
    End Class

#End Region

End Class