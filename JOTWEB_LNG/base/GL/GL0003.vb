Option Strict On
Imports MySQL.Data.MySqlClient
Imports System.Web.UI.WebControls

''' <summary>
''' 取引先情報取得
''' </summary>
''' <remarks></remarks>
Public Class GL0003CustomerList
    Inherits GL0000
    ''' <summary>
    ''' 取得条件
    ''' </summary>
    Public Enum LC_CUSTOMER_TYPE
        ''' <summary>
        ''' 全取得
        ''' </summary>
        ALL
        ''' <summary>
        ''' 全取得
        ''' </summary>
        WITHTERM
        ''' <summary>
        ''' 荷主
        ''' </summary> 
        OWNER
        ''' <summary>
        ''' 端末権限参照の荷主
        ''' </summary>
        OWNER_WITHTERM
        ''' <summary>
        ''' 庸車
        ''' </summary> 
        CARRIDE
        ''' <summary>
        ''' 端末権限参照の庸車
        ''' </summary>
        CARRIDE_WITHTERM
    End Enum
    ''' <summary>
    ''' 取引先タイプ一覧
    ''' </summary>
    ''' <remarks></remarks>
    Protected Class C_TORITYPE
        Public Const TYPE_01_GROUP As String = "02"
        ''' <summary>
        ''' 荷主　（TYPECODE2）
        ''' </summary>
        Public Const TYPE_02_OWNER As String = "NI"
        ''' <summary>
        ''' 庸車　（TYPECODE3)
        ''' </summary>
        Public Const TYPE_03_RIDECAR As String = "YO"

    End Class
    ''' <summary>
    '''　取得区分
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TYPE() As LC_CUSTOMER_TYPE
    ''' <summary>
    ''' 会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CAMPCODE() As String
    ''' <summary>
    ''' 部署コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ORGCODE() As String
    ''' <summary>
    ''' ROLECODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ROLECODE() As String
    ''' <summary>
    ''' 権限フラグ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PERMISSION() As String
    ''' <summary>
    ''' メソッド名
    ''' </summary>
    ''' <remarks></remarks>
    Protected Const METHOD_NAME As String = "getList"


    ''' <summary>
    ''' 情報の取得
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub getList()

        '<< エラー説明 >>
        'O_ERR = OK:00000,ERR:00002(環境エラー),ERR:00003(DBerr)
        '●初期処理


        'PARAM 01: TYPE
        If checkParam(METHOD_NAME, TYPE) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If
        'PARAM 01: CAMPCODE
        If checkParam(METHOD_NAME, CAMPCODE) <> C_MESSAGE_NO.NORMAL Then
            Exit Sub
        End If
        'PARAM EXTRA01: ORGCODE
        If IsNothing(ORGCODE) Then
            ORGCODE = String.Empty
        End If
        'PARAM EXTRA02: STYMD
        If STYMD < CDate(C_DEFAULT_YMD) Then
            STYMD = Date.Now
        End If
        'PARAM EXTRA03: ENDYMD
        If ENDYMD < CDate(C_DEFAULT_YMD) Then
            ENDYMD = Date.Now
        End If

        Try
            If IsNothing(LIST) Then
                LIST = New ListBox
            Else
                LIST.Items.Clear()
            End If
        Catch ex As Exception
        End Try
        'DataBase接続文字
        Using SQLcon = sm.getConnection
            SQLcon.Open() 'DataBase接続(Open)
            MySqlConnection.ClearPool(SQLcon)
            'Select Case TYPE
            '    'Case LC_CUSTOMER_TYPE.OWNER
            '    '    getOwnerList(SQLcon)
            '    'Case LC_CUSTOMER_TYPE.OWNER_WITHTERM
            '    '    getOwnerTermList(SQLcon)
            '    'Case LC_CUSTOMER_TYPE.CARRIDE
            '    '    getRideCarList(SQLcon)
            '    'Case LC_CUSTOMER_TYPE.CARRIDE_WITHTERM
            '    '    getRideCarTermList(SQLcon)
            '    'Case LC_CUSTOMER_TYPE.WITHTERM
            '    '    getCustomerTermList(SQLcon)
            '    'Case Else
            '    '    getCustomerList(SQLcon)
            'End Select

        End Using

    End Sub

End Class

