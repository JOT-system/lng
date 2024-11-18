Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0005WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPIDL As String = "LNT0005L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNT0005D"       'MAPID(明細)
    Public Const MAPIDM As String = "LNT0005DMAIN"   'MAPID(登録)
    'モード
    Public Const MODE_INIT As String = "0"           '初期処理モード
    Public Const MODE_NEW As String = "1"            '新規モード
    Public Const MODE_UPDATE As String = "2"         '更新モード
    Public Const MODE_DELETE As String = "3"         '削除
    Public Const MODE_ROW_DELETE As String = "4"     '行削除
    'タイトル区分
    Public Const TITLEKBNS As String = "6"   'タイトル区分

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
    ''' 固定値マスタから一覧の取得
    ''' </summary>
    ''' <param name="COMPCODE">会社コード</param>
    ''' <param name="FIXCODE">クラス名</param>
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