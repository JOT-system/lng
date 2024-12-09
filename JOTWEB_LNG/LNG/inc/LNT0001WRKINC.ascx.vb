Imports MySql.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0001WRKINC
    Inherits UserControl

    Public Const MAPIDL As String = "LNT0001L"      'MAPID
    Public Const MAPIDD As String = "LNT0001D"      'MAPID
    Public Const MAPIDI As String = "LNT0001I"      'MAPID

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

End Class