Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0007WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPIDL As String = "LNT0007L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNT0007D"       'MAPID(明細)
    'タイトル区分
    Public Const TITLEKBNS As String = "9"   'タイトル区分

    ''' <summary>
    ''' 複数選択可否(初期値Multiple(複数選択可能)
    ''' </summary>
    ''' <returns></returns>
    Public Property SelectionMode As ListSelectionMode = ListSelectionMode.Multiple

    Public Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 組織コード取得のパラメータ設定
    ''' </summary>
    ''' <param name="AUTHORITYALL_FLG">取得範囲</param>
    ''' <param name="I_COMPCODE">会社コード</param>
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
    ''' 営業収入決済条件マスタ項目取得
    ''' </summary>
    ''' <param name="INVOICETYPE_FLG"></param>
    ''' <param name="I_TORICODE"></param>
    ''' <param name="I_INVFILINGDEPT"></param>
    ''' <returns></returns>
    Function CreateInvKesaiKbnParam(ByVal INVOICETYPE_FLG As Integer, Optional ByVal I_TORICODE As String = "", Optional ByVal I_TORINAME As String = "", Optional ByVal I_INVFILINGDEPT As String = "") As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = INVOICETYPE_FLG
        prmData.Item(C_PARAMETERS.LP_TORICODE) = I_TORICODE
        prmData.Item(C_PARAMETERS.LP_INVFILINGDEPT) = I_INVFILINGDEPT

        CreateInvKesaiKbnParam = prmData

    End Function

    ''' <summary>
    ''' ユーザーマスタ項目取得
    ''' </summary>
    ''' <param name="I_STYMD"></param>
    ''' <param name="I_APPROVALFLG1">("0":指定なし、"1":第一承認者・上長)</param>
    ''' <param name="I_APPROVALFLG2">("0":指定なし、"1":最終承認者)</param>
    ''' <param name="I_USERID"></param>
    ''' <param name="I_ORGCODE"></param>
    ''' <returns></returns>
    Function CreateUserParam(ByVal I_STYMD As String, Optional ByVal I_APPROVALFLG1 As String = "0", Optional ByVal I_APPROVALFLG2 As String = "0", Optional ByVal I_USERID As String = "", Optional ByVal I_ORGCODE As String = "") As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_STYMD) = I_STYMD
        prmData.Item(C_PARAMETERS.LP_APPROVALFLG1) = I_APPROVALFLG1
        prmData.Item(C_PARAMETERS.LP_APPROVALFLG2) = I_APPROVALFLG2
        prmData.Item(C_PARAMETERS.LP_USERID) = I_USERID
        prmData.Item(C_PARAMETERS.LP_ORG) = I_ORGCODE

        CreateUserParam = prmData

    End Function

    ''' <summary>
    ''' コンテナ記号・番号取得パラメーター設定
    ''' </summary>
    ''' <param name="CNTENATYPE_FLG">0:コンテナ記号、1：コンテナ番号</param>
    ''' <param name="I_CTNTYPE">コンテナ記号</param>
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
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_FIXCODE"></param>
    ''' <returns></returns>
    Function CreateFIXParam(ByVal I_COMPCODE As String, Optional ByVal I_FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = I_FIXCODE
        CreateFIXParam = prmData
    End Function

    ''' <summary>
    ''' 受託人コードの取得
    ''' </summary>
    ''' <param name="CODETYPE_FLG"></param>
    ''' <param name="I_STATION"></param>
    ''' <param name="I_TRUSTEECD"></param>
    ''' <returns></returns>
    Function CreateDepTrusteeCdParam(ByVal CODETYPE_FLG As Integer, ByVal Optional I_STATION As String = "", Optional ByVal I_TRUSTEECD As String = "") As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = CODETYPE_FLG
        prmData.Item(C_PARAMETERS.LP_STATION) = I_STATION
        prmData.Item(C_PARAMETERS.LP_TRUSTEECD) = I_TRUSTEECD

        CreateDepTrusteeCdParam = prmData

    End Function
End Class