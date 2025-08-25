Option Strict On
Imports System.Web
Imports MySql.Data.MySqlClient
Imports System.Web.SessionState
Imports System.Configuration

''' <summary>
''' セッション情報操作処理
''' </summary>
''' <remarks></remarks>
Public Class CS0050SESSION : Implements IDisposable

    ''' <summary>
    ''' セッション情報
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SESSION As HttpSessionState
    ''' <summary>
    ''' 名前空間名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property NAMESPACE_VALUE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.NAMESPACE_VALUE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.NAMESPACE_VALUE) = value
        End Set
    End Property
    ''' <summary>
    ''' クラス名称
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property CLASS_NAME As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.CLASS_NAME))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.CLASS_NAME) = value
        End Set
    End Property
    ''' <summary>
    ''' DB接続文字列
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DBCon As String
        Get
            Dim GetStr As String
            GetStr = ConfigurationManager.AppSettings(C_SESSION_KEY.DB_CONNECT)

            If GetStr = Nothing Then
                SESSION = If(SESSION, HttpContext.Current.Session)
                Return Convert.ToString(SESSION(C_SESSION_KEY.DB_CONNECT))
            Else
                Return GetStr
            End If
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.DB_CONNECT) = value
        End Set
    End Property
    ''' <summary>
    ''' ユーザID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property USERID As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.USER_ID))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.USER_ID) = value
        End Set
    End Property
    ''' <summary>
    ''' ユーザ端末ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMID As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.USER_TERM_ID))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.USER_TERM_ID) = value
        End Set
    End Property
    ''' <summary>
    ''' ユーザ端末IPアドレス
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERMIPADDRESS As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION("address"))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION("address") = value
        End Set
    End Property
    ''' <summary>
    ''' ユーザ端末保持会社
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERM_COMPANY As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.TERM_COMPANY))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.TERM_COMPANY) = value
        End Set
    End Property
    ''' <summary>
    ''' ユーザ端末保持部署
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERM_ORG As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.TERM_ORGANIZATION))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.TERM_ORGANIZATION) = value
        End Set
    End Property
    ''' <summary>
    ''' ユーザ端末管理部署
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property TERM_M_ORG As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.TERM_MANAGMENT_ORGANIZATION))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.TERM_MANAGMENT_ORGANIZATION) = value
        End Set
    End Property
    ''' <summary>
    ''' 選択別会社
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SELECTED_COMPANY As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.SELECTED_ANOTHER_COMPANY))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.SELECTED_ANOTHER_COMPANY) = value
        End Set
    End Property
    ''' <summary>
    ''' TERM_DRIVERS
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property DRIVERS As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.TERM_DRIVERS))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.TERM_DRIVERS) = value
        End Set
    End Property
    ''' <summary>
    ''' ログ格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LOG_PATH As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.LOGGING_PATH))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.LOGGING_PATH) = value
        End Set
    End Property
    ''' <summary>
    ''' 情報退避XML格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property PDF_PATH As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.PDF_PRINT_PATH))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.PDF_PRINT_PATH) = value
        End Set
    End Property
    ''' <summary>
    ''' アップロードFILE格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property UPLOAD_PATH As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.UPLOADED_PATH))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.UPLOADED_PATH) = value
        End Set
    End Property
    ''' <summary>
    ''' 更新ジャーナル格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property JORNAL_PATH As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.UPDATE_JORNALING_PATH))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.UPDATE_JORNALING_PATH) = value
        End Set
    End Property
    ''' <summary>
    ''' システム格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SYSTEM_PATH As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.SYSTEM_PATH))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.SYSTEM_PATH) = value
        End Set
    End Property
    '### 20200828 START OT発送日報送信用追加 #########################################
    ''' <summary>
    ''' OT発送日報送信FILE格納ディレクトリ
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property OTFILESEND_PATH As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.OTFILESEND_PATH))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.OTFILESEND_PATH) = value
        End Set
    End Property
    '### 20200828 END   OT発送日報送信用追加 #########################################
    ''' <summary>
    ''' 印刷先URLのルート名
    ''' </summary>
    ''' <returns></returns>
    Public Property PRINT_ROOT_URL_NAME As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Dim retVal As String = Convert.ToString(SESSION(C_SESSION_KEY.PRINT_ROOT_URL_NAME))
            If retVal.Trim = "" Then
                retVal = "PRINT"
            End If
            Return retVal
        End Get
        Set(value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.PRINT_ROOT_URL_NAME) = value
        End Set
    End Property
    ''' <summary>
    ''' HelpURLのルート名
    ''' </summary>
    ''' <returns></returns>
    Public Property PRINT_ROOT_HELP_NAME As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Dim retVal As String = Convert.ToString(SESSION(C_SESSION_KEY.PRINT_ROOT_HELP_NAME))
            If retVal.Trim = "" Then
                retVal = "HELP"
            End If
            Return retVal
        End Get
        Set(value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.PRINT_ROOT_HELP_NAME) = value
        End Set
    End Property
    ''' <summary>
    ''' APサーバ端末ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property APSV_ID As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.APSV_TERM_ID))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.APSV_TERM_ID) = value
        End Set
    End Property
    ''' <summary>
    ''' APサーバ端末保持会社
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property APSV_COMPANY As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.APSV_FOUNDIION_COMPAY))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.APSV_FOUNDIION_COMPAY) = value
        End Set
    End Property
    ''' <summary>
    ''' APサーバ端末保持部署
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property APSV_ORG As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.APSV_FOUNDIION_ORGANIZATION))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.APSV_FOUNDIION_ORGANIZATION) = value
        End Set
    End Property
    ''' <summary>
    ''' APサーバ端末管理部署
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property APSV_M_ORG As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.APSV_MANAGMENT_ORGANIZATION))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.APSV_MANAGMENT_ORGANIZATION) = value
        End Set
    End Property
    ''' <summary>
    ''' MAPID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_MAPID As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_DISPLAY_MAP_ID))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_DISPLAY_MAP_ID) = value
        End Set
    End Property
    ''' <summary>
    ''' MENU_MODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_MENU_MODE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_USER_MENU_MODE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_USER_MENU_MODE) = value
        End Set
    End Property
    ''' <summary>
    ''' MAP_MODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_MAP_MODE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_USER_MAP_MODE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_USER_MAP_MODE) = value
        End Set
    End Property
    ''' <summary>
    ''' VIEWPROF_MODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_VIEWPROF_MODE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_USER_VIEWPROF_MODE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_USER_VIEWPROF_MODE) = value
        End Set
    End Property
    ''' <summary>
    ''' RPRTPROF_MODE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_RPRTPROF_MODE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_USER_RPRTPROF_MODE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_USER_RPRTPROF_MODE) = value
        End Set
    End Property
    '''' <summary>
    '''' APPROVALID
    '''' </summary>
    '''' <value></value>
    '''' <returns></returns>
    '''' <remarks></remarks>
    'Public Property VIEW_APPROVALID As String
    '    Get
    '        SESSION = If(SESSION, HttpContext.Current.Session)
    '        Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_USER_APPROVALID))
    '    End Get
    '    Set(ByVal value As String)
    '        SESSION = If(SESSION, HttpContext.Current.Session)
    '        SESSION(C_SESSION_KEY.MAPPING_USER_APPROVALID) = value
    '    End Set
    'End Property
    ''' <summary>
    ''' MAPVARIANT
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_MAP_VARIANT As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_USER_MAP_VARIANT))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_USER_MAP_VARIANT) = value
        End Set
    End Property
    ''' <summary>
    ''' PERTMISSION
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property VIEW_PERMIT As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_PERMISSION_MODE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_PERMISSION_MODE) = value
        End Set
    End Property
    ''' <summary>
    ''' ETC
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property MAP_ETC As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_ETC_VALUE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_ETC_VALUE) = value
        End Set
    End Property
    ''' <summary>
    ''' ヘルプ表示画面ID
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property HELP_ID As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_HELP_MAP_ID))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_HELP_MAP_ID) = value
        End Set
    End Property
    ''' <summary>
    ''' ヘルプ表示会社コード
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property HELP_COMP As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.MAPPING_HELP_COMP_CD))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.MAPPING_HELP_COMP_CD) = value
        End Set
    End Property
    ''' <summary>
    ''' LOGONDATE
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LOGONDATE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.LOGON_LOGIN_DATE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.LOGON_LOGIN_DATE) = value
        End Set
    End Property
    ''' <summary>
    ''' 開始年月日（特殊）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SELECTED_START_DATE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.SELECTED_START_DATE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.SELECTED_START_DATE) = value
        End Set
    End Property
    ''' <summary>
    ''' 終了年月日（特殊）
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property SELECTED_END_DATE As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.SELECTED_END_DATE))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.SELECTED_END_DATE) = value
        End Set
    End Property
    ''' <summary>
    ''' メニューリスト表示リスト
    ''' </summary>
    ''' <returns></returns>
    Public Property UserMenuCostomList As List(Of UserMenuCostomItem)
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return DirectCast(SESSION(C_SESSION_KEY.USERMENU_COSTOM_LIST), List(Of UserMenuCostomItem))
        End Get

        Set(value As List(Of UserMenuCostomItem))
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.USERMENU_COSTOM_LIST) = value
        End Set
    End Property
    ''' <summary>
    ''' DBの接続情報を作成する
    ''' </summary>
    ''' <param name="connect"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function getConnection(Optional ByRef connect As MySqlConnection = Nothing) As MySqlConnection
        'DataBase接続文字
        Dim SQLcon As New MySqlConnection(DBCon)
        If Not IsNothing(connect) Then
            connect = SQLcon
        End If
        getConnection = SQLcon
    End Function


    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Protected Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        'GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' 解放処理
    ''' </summary>
    Protected Sub Dispose(ByVal isDispose As Boolean)
        If isDispose Then

        End If
    End Sub

#Region "共通処理"
    ''' <summary>
    ''' ストアド実行処理
    ''' </summary>
    ''' <param name="dtData">データテーブル</param>
    ''' <remarks>備考</remarks>
    Public Sub executeStoredSQL(ByVal SQLcon As MySqlConnection, ByVal strPGName As String, ByVal param As Dictionary(Of String, String),
                        ByRef dtData As DataTable, Optional ByRef p_tran As MySqlTransaction = Nothing)

        Dim query As New StringBuilder

        'ストアド名設定
        With query
            .AppendLine("EXECUTE " & strPGName)
            .AppendLine(GetStoredParamQuery(param))
        End With
        '結果取得
        GetDataTable(SQLcon, query.ToString, DToH(param), dtData, p_tran)

    End Sub

    ''' <summary>
    ''' パラメータHashTableからストアド実行時のパラメータクエリを返却する。
    ''' </summary>
    ''' <param name="param"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function GetStoredParamQuery(ByVal param As Dictionary(Of String, String)) As String
        Dim ret As New StringBuilder
        For Each key As String In param.Keys
            ret.Append(key & ",")
        Next key
        ret.Remove(ret.Length - 1, 1)
        Return ret.ToString
    End Function

    ''' <summary>
    ''' DictionaryからHashtable変換
    ''' </summary>
    ''' <param name="dic"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function DToH(ByVal dic As Dictionary(Of String, String)) As Hashtable
        Dim h As New Hashtable
        For Each p As KeyValuePair(Of String, String) In dic
            h.Add(p.Key, p.Value)
        Next p
        Return h
    End Function

    ''' <summary>
    ''' データテーブル取得
    ''' </summary>
    ''' <param name="sql">実行SELECT文</param>
    ''' <param name="param">パラメータ</param>
    ''' <remarks></remarks>
    Public Overloads Sub GetDataTable(ByVal SQLcon As MySqlConnection, ByVal sql As String, ByVal param As Hashtable, ByRef dt As DataTable, Optional ByRef p_tran As MySqlTransaction = Nothing)
        dt = GetDataSet1(SQLcon, p_tran, sql, param).Tables(0)
    End Sub
    ''' <summary>
    ''' データテーブル取得
    ''' </summary>
    ''' <param name="sql">実行SELECT文</param>
    ''' <param name="param">パラメータ</param>
    ''' <remarks></remarks>
    Public Overloads Sub GetDataSet(ByVal SQLcon As MySqlConnection, ByVal sql As String, ByVal param As Hashtable, ByRef dt As DataSet, Optional ByRef p_tran As MySqlTransaction = Nothing)
        dt = GetDataSet1(SQLcon, p_tran, sql, param)
    End Sub
    ''' <summary>
    ''' データテーブル取得
    ''' </summary>
    ''' <param name="sql">実行SELECT文</param>
    ''' <remarks></remarks>
    Public Overloads Sub GetDataTable(ByVal SQLcon As MySqlConnection, ByVal sql As String, ByRef dt As DataTable)
        GetDataTable(SQLcon, sql, New Hashtable, dt)
    End Sub
#End Region

    ''' <summary>
    ''' SQLを実行してデータセットを取得する。SQL文実行用
    ''' </summary>
    ''' <params name="sqlstr">SQL文字列</params>
    ''' <params name="htblBind">バインド変数テーブル</params>
    ''' <returns>SQL実行結果</returns>
    Public Function GetDataSet1(ByVal SQLcon As MySqlConnection, ByVal p_tran As MySqlTransaction, ByVal sqlstr As String, ByVal htblBind As Hashtable) As DataSet


        Return GetDataSet2(SQLcon, p_tran, sqlstr, htblBind, CommandType.Text)

    End Function

    ''' <summary>
    ''' SQLを実行してデータセットを取得する。
    ''' </summary>
    ''' <params name="sqlstr">SQL文字列</params>
    ''' <params name="htblBind">バインド変数テーブル</params>
    ''' <param name="cmdType">コマンドタイプ</param>
    ''' <returns>SQL実行結果</returns>
    Public Function GetDataSet2(ByVal p_conn As MySqlConnection, ByVal p_tran As MySqlTransaction, ByVal sqlStr As String, ByVal htblBind As Hashtable, ByVal cmdType As CommandType) As DataSet
        Dim ret As DataSet = Nothing
        Dim colBind As System.Collections.DictionaryEntry

        Try
            Dim dataAdpt = New MySqlDataAdapter()
            Dim cmd As New MySqlCommand(sqlStr, p_conn)

            ' コマンドオブジェクト設定
            cmd.Connection = p_conn
            cmd.CommandText = sqlStr
            cmd.CommandType = cmdType
            cmd.Transaction = p_tran
            cmd.CommandTimeout = 0
            For Each colBind In htblBind
                cmd.Parameters.AddWithValue(colBind.Key.ToString(), colBind.Value)
            Next

            ' アダプタに設定
            dataAdpt.SelectCommand = cmd

            ' データセットに読み込み
            ret = New DataSet()
            dataAdpt.Fill(ret)

        Catch ex As Exception
            Throw ex
        End Try
        Return ret
    End Function

    ''' <summary>
    ''' ユーザーメニューのカスタマイズ
    ''' </summary>
    Public Class UserMenuCostomItem
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New(outputId As String, onOff As String, sortNo As Integer)
            Me.OutputId = outputId
            If onOff = "1" Then
                Me.OnOff = True
            Else
                Me.OnOff = False
            End If

            Me.SortNo = sortNo
        End Sub

        ''' <summary>
        ''' 表示ID
        ''' </summary>
        ''' <returns></returns>
        Public Property OutputId As String
        ''' <summary>
        ''' 表示非表示(True:表示,False:非表示)
        ''' </summary>
        ''' <returns></returns>
        Public Property OnOff As Boolean
        ''' <summary>
        ''' 並び順
        ''' </summary>
        ''' <returns></returns>
        Public Property SortNo As Integer
    End Class

#Region "楽々WebApi関連"
    ''' <summary>
    ''' 楽々精算WebAPIURL
    ''' </summary>
    ''' <returns></returns>
    Public Property WEBAPIURL As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.WEBAPI_URL))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.WEBAPI_URL) = value
        End Set
    End Property
    ''' <summary>
    ''' 楽々精算WebAPI アカウント
    ''' </summary>
    ''' <returns></returns>
    Public Property WEBAPIACCOUNT As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.WEBAPI_ACCOUNT))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.WEBAPI_ACCOUNT) = value
        End Set
    End Property
    ''' <summary>
    ''' 楽々精算WebAPI システム部用トークン
    ''' </summary>
    ''' <returns></returns>
    Public Property WEBAPITOKENSYSTEM As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.WEBAPI_TOKENSYSTEM))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.WEBAPI_TOKENSYSTEM) = value
        End Set
    End Property
    ''' <summary>
    ''' 楽々精算WebAP連携実行FLG
    ''' </summary>
    ''' <returns></returns>
    Public Property WEBAPIFLG As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.WEBAPI_FLG))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.WEBAPI_FLG) = value
        End Set
    End Property
    ''' <summary>
    ''' WebAPI用トークン取得ユーザーに応じて使用可能なAPIトークン
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property WEBAPITOKEN As String
        Get
            Dim userRole As String = Me.VIEW_MENU_MODE
            Select Case userRole
                Case "jot_sys_1"
                    Return Me.WEBAPITOKENSYSTEM
                Case Else
                    Return ""
            End Select
        End Get
    End Property

    ''' <summary>
    ''' WebAPI連動可能なユーザーか(True:連動可能,False:連動不可)あくまでユーザーレベル（〆状態等は別途考慮すること）
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property WEBAPI_CAN_RELATION_RAKURAKU As Boolean
        Get
            'システム部
            If {"jot_sys_1"}.Contains(Me.VIEW_MENU_MODE) Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property
    ''' <summary>
    ''' WebAPI全件連動可能なユーザーか(True:連動可能,False:連動不可)システムと石油部は問答無用で全件連動する
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property WEBAPI_CAN_ALL_RELATION_RAKURAKU As Boolean
        Get
            'システム部は全件問答無用
            If {"jot_sys_1"}.Contains(Me.VIEW_MENU_MODE) Then
                Return True
            Else
                Return False
            End If
        End Get
    End Property

#End Region
    ''' <summary>
    ''' ライセンス取得用
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property LICENSE_GET As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.LICENSE_GET))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.LICENSE_GET) = value
        End Set
    End Property

    ''' <summary>
    ''' 環境判定用
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property ENVIRONMENTFLG As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.ENVIRONMENT_FLG))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.ENVIRONMENT_FLG) = value
        End Set
    End Property

    ''' <summary>
    ''' Hypertext Transfer Protocol（Secure）取得用   2025/04/16 ADD
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Property HTTPS_GET As String
        Get
            SESSION = If(SESSION, HttpContext.Current.Session)
            Return Convert.ToString(SESSION(C_SESSION_KEY.HTTPS_GET))
        End Get
        Set(ByVal value As String)
            SESSION = If(SESSION, HttpContext.Current.Session)
            SESSION(C_SESSION_KEY.HTTPS_GET) = value
        End Set
    End Property
End Class


