'Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0012WRKINC
    Inherits UserControl

    Public Const MAPID As String = "LNT0012S"      'MAPID

    'タイトル区分
    Public Const TITLEKBNS As String = "7"          'タイトル区分

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
    ''' 帳票マスタから一覧の取得
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_FIXCODE"></param>
    ''' <returns></returns>
    Function CreateREPORTParam(ByVal I_COMPCODE As String, Optional ByVal I_FIXCODE As String = "") As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_FIX_CLASS) = I_FIXCODE

        CreateREPORTParam = prmData

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

    ''' <summary>
    ''' 大中小分類コード取得のパラメータ設定
    ''' </summary>
    ''' <param name="I_BIGCTNCD"></param>
    ''' <param name="I_MIDDLECTNCD"></param>
    ''' <param name="CLASSTYPE_FLG"></param>
    ''' <returns></returns>
    Public Function CreateClassParam(ByVal CLASSTYPE_FLG As Integer, Optional ByVal I_BIGCTNCD As String = "", Optional ByVal I_MIDDLECTNCD As String = "") As Hashtable

        Dim WW_PrmData As New Hashtable
        WW_PrmData.Item(C_PARAMETERS.LP_TYPEMODE) = CLASSTYPE_FLG
        WW_PrmData.Item(C_PARAMETERS.LP_BIGCTNCD) = I_BIGCTNCD
        WW_PrmData.Item(C_PARAMETERS.LP_MIDDLECTNCD) = I_MIDDLECTNCD

        CreateClassParam = WW_PrmData

    End Function
End Class