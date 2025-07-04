Imports MySql.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0001WRKINC
    Inherits UserControl

    Public Const MAPIDL As String = "LNT0001L"      'MAPID
    Public Const MAPIDD As String = "LNT0001D"      'MAPID
    Public Const MAPIDI As String = "LNT0002L"      'MAPID
    Public Const MAPIDZ As String = "LNT0001Z"      'MAPID
    Public Const MAPIDAJ As String = "LNT0001AJ"    'MAPID

    ''' <summary>
    ''' アボカド接続情報
    ''' </summary>
    Public Class AVOCADOINFO
        ''' <summary>
        ''' 部署
        ''' </summary>
        Public Property Org As String
        ''' <summary>
        ''' アプリID
        ''' </summary>
        Public Property AppId As String
        ''' <summary>
        ''' トークン
        ''' </summary>
        Public Property Token As String
        ''' <summary>
        ''' 取引先
        ''' </summary>
        Public Property Tori As String
        ''' <summary>
        ''' コンストラクタ
        ''' </summary>
        Public Sub New(Org As String, AppId As String, Token As String, Tori As String)
            Me.Org = Org
            Me.AppId = AppId
            Me.Token = Token
            Me.Tori = Tori
        End Sub
    End Class

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 固定値マスタから一覧の取得設定
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_FIXCODE"></param>
    ''' <returns></returns>
    Function CreateFIXParam(ByVal I_COMPCODE As String, Optional ByVal I_FIXCODE As String = "") As Hashtable
        Dim WW_PrmData As New Hashtable
        WW_PrmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        WW_PrmData.Item(C_PARAMETERS.LP_FIX_CLASS) = I_FIXCODE
        CreateFIXParam = WW_PrmData
    End Function

    ''' <summary>
    ''' 排他チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="SHIPPERCD">荷主コード</param>
    ''' <param name="TIMESTAMP">タイムスタンプ</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String,
                          ByRef SHIPPERCD As String, ByRef TIMESTAMP As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr As String =
              " SELECT                                      " _
            & "     SHIPPERCD                               " _
            & "   , UPDTIMSTP                               " _
            & " FROM                                        " _
            & "     LNG.LNM0023_SHIPPER                     " _
            & " WHERE                                       " _
            & "         SHIPPERCD  = @P1                    "

        Try
            Using SQLcmd As New MySqlCommand(SQLStr, SQLcon)
                Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 6) '荷主コード

                PARA1.Value = SHIPPERCD

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0023Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0023Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0023Chk.Load(SQLdr)

                    If LNM0023Chk.Rows.Count > 0 Then
                        Dim LNM0023row As DataRow
                        LNM0023row = LNM0023Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0023row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0023row("UPDTIMSTP").ToString <> TIMESTAMP Then
                                ' 排他エラー
                                O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                            End If
                        End If
                    Else
                        ' 排他エラー
                        O_MESSAGENO = Messages.C_MESSAGE_NO.CTN_HAITA_DATA_ERROR
                    End If
                End Using
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNM0023C HAITA"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub
    ''' <summary>
    ''' 遷移先URLの取得
    ''' </summary>
    ''' <param name="I_MAPID"></param>
    ''' <param name="O_URL"></param>
    ''' <remarks></remarks>
    Public Sub GetURL(ByVal I_MAPID As String, ByRef O_URL As String)

        '○共通宣言
        '*共通関数宣言(APPLDLL)
        Dim CS0011LOGWRITE As New CS0011LOGWrite            'LogOutput DirString Get
        Dim CS0050SESSION As New CS0050SESSION          'セッション情報操作処理

        Dim WW_URL As String = ""
        Try
            'DataBase接続文字
            Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
                SQLcon.Open() 'DataBase接続(Open)

                'LNS0007_URL検索SQL文
                Dim SQL_Str As String =
                     "SELECT rtrim(URL) as URL " _
                   & " FROM  COM.LNS0007_URL " _
                   & " Where MAPID    = @P1 " _
                   & "   and STYMD   <= @P2 " _
                   & "   and ENDYMD  >= @P3 " _
                   & "   and DELFLG  <> @P4 "
                Using SQLcmd As New MySqlCommand(SQL_Str, SQLcon)
                    Dim PARA1 As MySqlParameter = SQLcmd.Parameters.Add("@P1", MySqlDbType.VarChar, 50)
                    Dim PARA2 As MySqlParameter = SQLcmd.Parameters.Add("@P2", MySqlDbType.Date)
                    Dim PARA3 As MySqlParameter = SQLcmd.Parameters.Add("@P3", MySqlDbType.Date)
                    Dim PARA4 As MySqlParameter = SQLcmd.Parameters.Add("@P4", MySqlDbType.VarChar, 1)
                    PARA1.Value = I_MAPID

                    PARA2.Value = Date.Now
                    PARA3.Value = Date.Now
                    PARA4.Value = C_DELETE_FLG.DELETE
                    Dim SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    If SQLdr.Read Then
                        O_URL = Convert.ToString(SQLdr("URL"))
                    End If

                    'Close
                    SQLdr.Close() 'Reader(Close)
                    SQLdr = Nothing

                End Using
                'SQL コネクションクローズ
                SQLcon.Close()
                SQLcon.Dispose()

            End Using

        Catch ex As Exception
            CS0011LOGWRITE.INFSUBCLASS = "GetURL"                         'SUBクラス名
            CS0011LOGWRITE.INFPOSI = "LNS0007_URL SELECT"
            CS0011LOGWRITE.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWRITE.TEXT = ex.ToString()
            CS0011LOGWRITE.MESSAGENO = C_MESSAGE_NO.DB_ERROR 'DBエラー。
            CS0011LOGWRITE.CS0011LOGWrite()                             'ログ出力
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' アボカド情報取得（組織、取引先、アプリID、トークン）
    ''' </summary>
    Public Function GetAvocadoInfo(ByVal iComp As String, ByVal iOrg As String, ByVal iTori As String) As List(Of AVOCADOINFO)

        Dim CS0007CheckAuthority As New CS0007CheckAuthority        '更新権限チェック
        Dim GS0007FIXVALUElst As New GS0007FIXVALUElst              '固定値マスタ
        Dim CS0050SESSION As New CS0050SESSION                      'セッション情報操作処理

        Dim ApiInfo As New List(Of AVOCADOINFO)
        Dim toriList As String() = iTori.Split(",")

        '------------------------------------------------------------
        '指定された荷主に該当するアボカド接続情報（営業所毎）取得
        '------------------------------------------------------------
        Dim apiList1 As New ListBox
        Dim apiList2 As New ListBox
        Dim apiList3 As New ListBox
        Dim apiList4 As New ListBox
        Dim apiList5 As New ListBox
        GS0007FIXVALUElst.CAMPCODE = iComp
        GS0007FIXVALUElst.CLAS = "AVOCADOINFO"
        GS0007FIXVALUElst.LISTBOX1 = apiList1
        GS0007FIXVALUElst.LISTBOX2 = apiList2
        GS0007FIXVALUElst.LISTBOX3 = apiList3
        GS0007FIXVALUElst.LISTBOX4 = apiList4
        GS0007FIXVALUElst.LISTBOX5 = apiList5
        GS0007FIXVALUElst.ADDITIONAL_SORT_ORDER = ""
        GS0007FIXVALUElst.GS0007FIXVALUElst()
        If Not isNormal(GS0007FIXVALUElst.ERR) Then
            Throw New Exception("固定値取得エラー: " & GS0007FIXVALUElst.ERR)
        End If


        Using SQLcon As MySqlConnection = CS0050SESSION.getConnection
            SQLcon.Open()  ' DataBase接続

            For i As Integer = 0 To apiList1.Items.Count - 1
                '■参考
                'apiList1.Items(i).Value:対象部署
                'apiList1.Items(i).Text:対象アプリID
                'apiList2.Items(i).Text:対象トークン

                '操作可能な組織コードかチェック
                If CS0007CheckAuthority.checkUserPermission(SQLcon, iOrg, C_ROLE_VARIANT.USER_ORG, apiList1.Items(i).Value) = "2" Then
                    'リスト３～５（VALUE3～5）に取引先コードが設定されている
                    Dim toriCode As String = ""
                    If iTori.Count = 0 Then
                        '画面指定なし（初期表示の場合）
                        If apiList3.Items(i).Text <> "" Then
                            If toriCode.Length > 0 Then toriCode += ","
                            toriCode += apiList3.Items(i).Text
                        End If
                        If apiList4.Items(i).Text <> "" Then
                            If toriCode.Length > 0 Then toriCode += ","
                            toriCode += apiList4.Items(i).Text
                        End If
                        If apiList5.Items(i).Text <> "" Then
                            If toriCode.Length > 0 Then toriCode += ","
                            toriCode += apiList5.Items(i).Text
                        End If
                        ApiInfo.Add(New AVOCADOINFO(apiList1.Items(i).Value, apiList1.Items(i).Text, apiList2.Items(i).Text, toriCode))
                    Else
                        For Each toriFor In toriList
                            If apiList3.Items(i).Text = toriFor Then
                                If toriCode.Length > 0 Then toriCode += ","
                                toriCode += apiList3.Items(i).Text
                            End If
                            If apiList4.Items(i).Text = toriFor Then
                                If toriCode.Length > 0 Then toriCode += ","
                                toriCode += apiList4.Items(i).Text
                            End If
                            If apiList5.Items(i).Text = toriFor Then
                                If toriCode.Length > 0 Then toriCode += ","
                                toriCode += apiList5.Items(i).Text
                            End If
                        Next
                        If toriCode.Length > 0 Then
                            ApiInfo.Add(New AVOCADOINFO(apiList1.Items(i).Value, apiList1.Items(i).Text, apiList2.Items(i).Text, toriCode))
                        End If
                    End If
                End If
            Next
        End Using

        Return ApiInfo

    End Function

End Class