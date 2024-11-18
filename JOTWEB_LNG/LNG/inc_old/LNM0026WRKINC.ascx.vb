'Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNM0026WRKINC
    Inherits UserControl

    Public Const MAPIDS As String = "LNM0026S"       'MAPID(検索)
    Public Const MAPIDL As String = "LNM0026L"       'MAPID(一覧)
    Public Const MAPIDD As String = "LNM0026D"       'MAPID(更新)
    'タイトル区分
    Public Const TITLEKBNS As String = "6"   'タイトル区分

    '' <summary>
    '' ワークデータ初期化処理
    '' </summary>
    '' <remarks></remarks>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 営業収入決済条件マスタ項目取得
    ''' </summary>
    ''' <param name="KEKKJMTYPE_FLG"></param>
    ''' <param name="I_TORICODE"></param>
    ''' <param name="I_INVFILINGDEPT"></param>
    ''' <returns></returns>
    Function CreateKekkjmParam(ByVal KEKKJMTYPE_FLG As Integer, Optional ByVal I_TORICODE As String = "", Optional ByVal I_INVFILINGDEPT As String = "") As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = KEKKJMTYPE_FLG
        prmData.Item(C_PARAMETERS.LP_TORICODE) = I_TORICODE
        prmData.Item(C_PARAMETERS.LP_INVFILINGDEPT) = I_INVFILINGDEPT

        CreateKekkjmParam = prmData

    End Function

    ''' <summary>
    ''' 駅コード取得のパラメータ設定
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


    '' <summary>
    '' ロールマスタから一覧の取得
    '' </summary>
    '' <param name="COMPCODE"></param>
    '' <param name="FIXCODE"></param>
    '' <returns></returns>
    '' <remarks></remarks>
    Function CreateRoleList(ByVal I_COMPCODE As String, ByVal I_OBJCODE As String) As Hashtable
        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_CLASSCODE) = I_OBJCODE
        CreateRoleList = prmData
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

End Class