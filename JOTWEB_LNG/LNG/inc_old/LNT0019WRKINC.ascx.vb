Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0019WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPIDL As String = "LNT0019S"       'MAPID(一覧)
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
    Function CreateInvKesaiKbnParam(ByVal INVOICETYPE_FLG As Integer, Optional ByVal I_TORICODE As String = "", Optional ByVal I_INVFILINGDEPT As String = "") As Hashtable

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

End Class