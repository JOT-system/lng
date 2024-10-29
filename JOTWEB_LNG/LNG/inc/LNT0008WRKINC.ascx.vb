Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0008WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPIDL As String = "LNT0008L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNT0008D"       'MAPID(明細)
    Public Const MAPIDM As String = "LNT0008DMAIN"   'MAPID(登録)
    'モード
    Public Const MODE_NEW As String = "1"            '新規モード
    Public Const MODE_UPDATE As String = "2"         '更新モード
    'タイトル区分
    Public Const TITLEKBNS As String = "3"   'タイトル区分

    ''' <summary>
    ''' 複数選択可否(初期値Multiple(複数選択可能)
    ''' </summary>
    ''' <returns></returns>
    Public Property SelectionMode As ListSelectionMode = ListSelectionMode.Multiple

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 運用部署パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function CreateUORGParam(ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.BRANCH_ONLY
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}

        CreateUORGParam = prmData

    End Function

    ''' <summary>
    ''' 所管部の取得
    ''' </summary>
    ''' <param name="I_JURISDICTIONT"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateJURISDICTIONParam(ByVal I_COMPCODE As String, ByVal I_JURISDICTIONT As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_SALESOFFICE) = I_JURISDICTIONT
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateJURISDICTIONParam = prmData
    End Function

    ''' <summary>
    ''' コンテナ番号の取得
    ''' </summary>
    ''' <param name="I_CTNNO"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateCTNNOParam(ByVal I_COMPCODE As String, ByVal I_CTNNO As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_CTNNO) = I_CTNNO
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateCTNNOParam = prmData
    End Function

    ''' <summary>
    ''' 発受託人の取得
    ''' </summary>
    ''' <param name="I_DEPTRUSTEECD"></param>
    ''' <returns></returns>
    ''' <remarks>全て</remarks>
    Function CreateDEPTRUSTEEParam(ByVal I_COMPCODE As String, ByVal I_DEPTRUSTEECD As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_DEPTRUSTEECD) = I_DEPTRUSTEECD
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0003CustomerList.LC_CUSTOMER_TYPE.ALL
        CreateDEPTRUSTEEParam = prmData
    End Function

    ''' <summary>
    ''' 受託人コードの取得
    ''' </summary>
    ''' <param name="CODETYPE_FLG"></param>
    ''' <param name="I_STATION"></param>
    ''' <param name="I_TRUSTEECD"></param>
    ''' <returns></returns>
    Function CreateDepTrusteeCdParam(ByVal CODETYPE_FLG As Integer, ByVal I_STATION As String, Optional ByVal I_TRUSTEECD As String = "") As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = CODETYPE_FLG
        prmData.Item(C_PARAMETERS.LP_STATION) = I_STATION
        prmData.Item(C_PARAMETERS.LP_TRUSTEECD) = I_TRUSTEECD

        CreateDepTrusteeCdParam = prmData

    End Function

    ''' <summary>
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE"></param>
    ''' <param name="FIXCODE"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Function CreateFIXParam(ByVal COMPCODE As String, Optional ByVal FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = FIXCODE
        CreateFIXParam = prmData
    End Function

    ''' <summary>
    ''' 組織コード取得のパラメータ設定
    ''' </summary>
    ''' <param name="AUTHORITYALL_FLG"></param>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateORGParam(ByVal AUTHORITYALL_FLG As Integer, ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = AUTHORITYALL_FLG
        prmData.Item(C_PARAMETERS.LP_PERMISSION) = C_PERMISSION.INVALID
        prmData.Item(C_PARAMETERS.LP_ORG_CATEGORYS) = New String() {
            GL0002OrgList.C_CATEGORY_LIST.CARAGE}

        CreateORGParam = prmData

    End Function

    ''' <summary>
    ''' コンテナ記号・番号取得パラメーター設定
    ''' </summary>
    ''' <param name="CNTENATYPE_FLG"></param>
    ''' <param name="I_CTNTYPE"></param>
    ''' <returns></returns>
    Public Function CreateContenaParam(ByVal CNTENATYPE_FLG As Integer, Optional ByVal I_CTNTYPE As String = "") As Hashtable

        Dim WW_PrmData As New Hashtable
        WW_PrmData.Item(C_PARAMETERS.LP_CTNTYPE) = I_CTNTYPE
        WW_PrmData.Item(C_PARAMETERS.LP_TYPEMODE) = CNTENATYPE_FLG

        CreateContenaParam = WW_PrmData

    End Function

    ''' <summary>
    ''' 駅マスタから一覧の取得
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_ORGCODE"></param>
    ''' <returns></returns>
    Function CreateStationParam(ByVal I_COMPCODE As String, Optional ByVal I_ORGCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_ORG) = I_ORGCODE
        CreateStationParam = prmData
    End Function

    ''' <summary>
    ''' 営業収入決済条件マスタ項目取得
    ''' </summary>
    ''' <param name="INVOICETYPE_FLG">取得項目選択区分（0：取引先コード・1：取引先サブコード・2：請求書決済区分）</param>
    ''' <param name="I_INVOICECAMPCD">取引先コード</param>
    ''' <param name="I_INVFILINGDEPT">請求書提出部店</param>
    ''' <returns></returns>
    Function CreateInvKesaiKbnParam(ByVal INVOICETYPE_FLG As Integer, Optional ByVal I_INVOICECAMPCD As String = "", Optional ByVal I_INVFILINGDEPT As String = "") As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = INVOICETYPE_FLG
        prmData.Item(C_PARAMETERS.LP_TORICODE) = I_INVOICECAMPCD
        prmData.Item(C_PARAMETERS.LP_INVFILINGDEPT) = I_INVFILINGDEPT

        CreateInvKesaiKbnParam = prmData

    End Function

End Class