'Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0014WRKINC
    Inherits UserControl

    Public Const MAPID As String = "LNT0014S"      'MAPID

    'タイトル区分
    Public Const TITLEKBNS As String = "4"          'タイトル区分

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub

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
    ''' 運用部署パラメーター
    ''' </summary>
    ''' <param name="I_COMPCODE">会社コード</param>
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
    ''' 支払先コード取得
    ''' </summary>
    ''' <param name="INVOICETYPE_FLG">取得項目選択区分（0：取引先コード・1：取引先サブコード・2：請求書決済区分）</param>
    ''' <param name="I_INVOICECAMPCD">取引先コード</param>
    ''' <returns></returns>
    Function CreateSiharaisakiParam(ByVal INVOICETYPE_FLG As Integer, Optional ByVal I_INVOICECAMPCD As String = "") As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = INVOICETYPE_FLG
        prmData.Item(C_PARAMETERS.LP_TORICODE) = I_INVOICECAMPCD

        CreateSiharaisakiParam = prmData

    End Function
End Class