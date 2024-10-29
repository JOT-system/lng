Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0009WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPIDL As String = "LNT0009L"       'MAPID(一覧画面)
    Public Const MAPIDD As String = "LNT0009D"       'MAPID(照会画面)

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' ファイナンスリース登録済みアイテム一覧を取得
    ''' </summary>
    ''' <param name="FINANCEITEM_FLG"></param>
    ''' <returns></returns>
    Function CreateFinanceItemParam(ByVal FINANCEITEM_FLG As Integer, Optional ByVal I_KEIJYOYM As String = "") As Hashtable
        Dim WW_PrmData As New Hashtable
        WW_PrmData.Item(C_PARAMETERS.LP_KEIJYOYM) = I_KEIJYOYM
        WW_PrmData.Item(C_PARAMETERS.LP_TYPEMODE) = FINANCEITEM_FLG
        CreateFinanceItemParam = WW_PrmData
    End Function

End Class