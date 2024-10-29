Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0006WRKINC
    Inherits System.Web.UI.UserControl

    Public Const MAPIDS As String = "LNT0006O"       'MAPID(検索)
    'タイトル区分
    Public Const TITLEKBNS As String = "5"   'タイトル区分

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
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
    ''' 対象フラグの初期値取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetNewReportList() As List(Of DisplayFlag)
        Dim retVal As New List(Of DisplayFlag)

        retVal.Add(New DisplayFlag("発送日報", "SHIPPINGDAILY", 0, "0"))
        retVal.Add(New DisplayFlag("他駅発送明細", "OTHERSTATIONS", 1, "1"))
        Return retVal
    End Function

    ''' <summary>
    ''' 対象フラグの初期値取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetNewReportTypeList() As List(Of DisplayFlag)
        Dim retVal As New List(Of DisplayFlag)

        retVal.Add(New DisplayFlag("A", "REPORTA", 0, "0"))
        retVal.Add(New DisplayFlag("B", "REPORTB", 1, "1"))
        Return retVal
    End Function

    ''' <summary>
    ''' 対象フラグの初期値取得
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetNewOutputPatternList() As List(Of DisplayFlag)
        Dim retVal As New List(Of DisplayFlag)

        retVal.Add(New DisplayFlag("絞り込み無し", "ALL", 0, "0"))
        retVal.Add(New DisplayFlag("冷凍のみ", "URONLY", 1, "1"))
        retVal.Add(New DisplayFlag("空回送のみ", "AIRFORWARDINGONLY", 2, "2"))
        Return retVal
    End Function

    ''' <summary>
    ''' 掲載フラグ関連クラス
    ''' </summary>
    <Serializable>
    Public Class DisplayFlag
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        ''' <param name="dispName">画面表示名</param>
        ''' <param name="fieldName">フィールド名</param>
        ''' <param name="dispOrder">並び順</param>
        Public Sub New(dispName As String, fieldName As String, dispOrder As Integer, officeCode As String)
            Me.DispName = dispName
            Me.FieldName = fieldName
            Me.DispOrder = dispOrder
            Me.OfficeCode = officeCode
        End Sub
        ''' <summary>
        ''' 表示名
        ''' </summary>
        ''' <returns></returns>
        Public Property DispName As String
        ''' <summary>
        ''' 対象フィールド
        ''' </summary>
        ''' <returns></returns>
        Public Property FieldName As String
        ''' <summary>
        ''' 表示順
        ''' </summary>
        ''' <returns></returns>
        Public Property DispOrder As Integer
        ''' <summary>
        ''' 表示グループ（仮）
        ''' </summary>
        ''' <returns></returns>
        Public Property Group As String = ""
        ''' <summary>
        ''' 選択フラグ
        ''' </summary>
        ''' <returns></returns>
        Public Property Checked As Boolean = False
        ''' <summary>
        ''' オフィスコード
        ''' </summary>
        ''' <returns></returns>
        Public Property OfficeCode As String
    End Class

End Class