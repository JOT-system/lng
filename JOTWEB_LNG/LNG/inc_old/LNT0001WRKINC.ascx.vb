Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0001WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPIDS As String = "LNT0001S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNT0001L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNT0001D"       'MAPID(明細)
    'タイトル区分
    Public Const TITLEKBNS As String = "9"   'タイトル区分

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
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = GL0002OrgList.LS_AUTHORITY_WITH.NO_AUTHORITY
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

End Class