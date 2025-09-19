Imports MySQL.Data.MySqlClient
Imports JOTWEB_LNG.GRIS0005LeftBox

Public Class LNT0030WRKINC
    Inherits UserControl

    Public Const MAPIDL As String = "LNT0030L"       'MAPID(一覧)
    'タイトル区分
    Public Const TITLEKBNS As String = "C"   'タイトル区分

    Public Const MAX_ENDYMD As String = "2099/12/31"

    '必須桁数
    Public Const REQUIREDDIGITS_TORICODE As Integer = 10 '取引先コード
    Public Const REQUIREDDIGITS_AVOCADOSHUKABASHO As Integer = 6 '実績出荷場所コード
    Public Const REQUIREDDIGITS_AVOCADOTODOKECODE As Integer = 6 '実績届先コード

    ''' <summary>
    ''' ファイルタイプ
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum FILETYPE
        EXCEL
        PDF
    End Enum

    ''' <summary>
    ''' 入出力項目位置
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum INOUTEXCELCOL
        DELFLG   '削除フラグ
        SEIKYUYM   '請求年月
        SEIKYUBRANCH   '請求年月枝番
        SEIKYUDATEFROM   '請求対象期間From
        SEIKYUDATETO   '請求対象期間To
        TORICODE   '取引先コード
        TORINAME   '取引先名
        ORGCODE   '部門コード
        ORGNAME   '部門名
        KASANORGCODE   '加算先部門コード
        KASANORGNAME   '加算先部門名
        PATTERNCODE   'パターンコード
        AVOCADOSHUKABASHO   '出荷場所コード
        AVOCADOSHUKANAME   '出荷場所名
        AVOCADOTODOKECODE   '届先コード
        AVOCADOTODOKENAME   '届先名
        SHAGATA   '車型
        SHABARA   '車腹
        SHABAN   '車番
        DIESELPRICESTANDARD   '基準単価
        DIESELPRICECURRENT   '実勢単価
        CALCMETHOD   '距離計算方式
        DISTANCE   '距離
        SHIPPINGCOUNT   '輸送回数
        NENPI   '燃費
        FUELBASE   '基準燃料使用量
        FUELRESULT   '燃料使用量
        ADJUSTMENT   '精算調整幅
        MEMO   '計算式メモ
    End Enum

    ''' <summary>
    ''' 変更履歴出力項目位置
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum HISTORYEXCELCOL
        DELFLG   '削除フラグ
    End Enum

    '操作区分
    Public Enum OPERATEKBN
        NEWDATA = 1 '新規
        UPDDATA = 2 '更新
        DELDATA = 3 '削除
    End Enum

    '変更区分
    Public Enum MODIFYKBN
        NEWDATA = 1 '新規
        BEFDATA = 2 '変更前
        AFTDATA = 3　'変更後
    End Enum

    ''' <summary>
    ''' ワークデータ初期化処理
    ''' </summary>
    Public Sub Initialize()
    End Sub

    ''' <summary>
    ''' 管理権限のある組織コードか確認する
    ''' </summary>
    ''' <param name="I_ORG">対象組織コード</param>
    ''' <remarks></remarks>
    Public Shared Function AdminCheck(ByVal I_ORG As Object) As Boolean
        Dim WW_HT As New Hashtable
        WW_HT.Add("011308", "情報システム部")
        WW_HT.Add("011310", "高圧ガス１部")

        Return WW_HT.ContainsKey(I_ORG)
    End Function

    ''' <summary>
    ''' 組織取得
    ''' </summary>
    ''' <param name="I_ORGCODE">組織コード</param>
    ''' <param name="O_NAME">リターンコード</param>
    ''' <param name="O_RTN">リターンコード</param>
    Public Shared Sub getOrgName(ByVal I_ORGCODE As String, ByRef O_NAME As String, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       ORGNAME AS NAME                                                                               ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.VIW0006_TORI                                                                                ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     ORGCODE = @ORGCODE                                                                              ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ORGCODE", MySqlDbType.VarChar).Value = I_ORGCODE
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If WW_Tbl.Rows.Count > 0 Then
                        O_NAME = WW_Tbl.Rows(0)("NAME")
                    End If
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

    End Sub
    ''' <summary>
    ''' 加算先部門取得
    ''' </summary>
    ''' <param name="I_ORGCODE">部署コード</param>
    ''' <param name="O_CODE">コード</param>
    ''' <param name="O_NAME">名称</param>
    ''' <param name="O_RTN">リターンコード</param>
    Public Shared Sub getKasanOrgName(ByVal I_ORGCODE As String, ByRef O_CODE As String, ByRef O_NAME As String, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       KASANORGCODE AS KASANORGCODE                                                                  ")
        SQLStr.AppendLine("      ,KASANORGNAME AS KASANORGNAME                                                                  ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.VIW0006_TORI                                                                                ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     ORGCODE = @ORGCODE                                                                              ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@ORGCODE", MySqlDbType.VarChar).Value = I_ORGCODE
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If WW_Tbl.Rows.Count > 0 Then
                        O_CODE = WW_Tbl.Rows(0)("KASANORGCODE")
                        O_NAME = WW_Tbl.Rows(0)("KASANORGNAME")
                    End If
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' 取引先取得
    ''' </summary>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="O_NAME">取引先名</param>
    ''' <param name="O_RTN">リターンコード</param>
    Public Shared Sub getToriName(ByVal I_TORICODE As String, ByRef O_NAME As String, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       TORINAME AS TORINAME                                                                          ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.VIW0006_TORI                                                                                ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     TORICODE = @TORICODE                                                                            ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@TORICODE", MySqlDbType.VarChar).Value = I_TORICODE
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If WW_Tbl.Rows.Count > 0 Then
                        O_NAME = WW_Tbl.Rows(0)("TORINAME")
                    End If
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

    End Sub
    ''' <summary>
    ''' 出荷場所コード取得
    ''' </summary>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部署コード</param>
    ''' <param name="I_SHUKANAME">出荷場所名</param>
    ''' <param name="O_CODE">リターンコード</param>
    ''' <param name="O_RTN">リターンコード</param>
    Public Shared Sub getShukaCode(ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_SHUKANAME As String, ByRef O_CODE As String, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       SHUKABASHO AS SHUKABASHO                                                                      ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.VIW0006_TORI                                                                                ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     TORICODE     = @TORICODE                                                                        ")
        SQLStr.AppendLine(" AND ORGCODE      = @ORGCODE                                                                         ")
        SQLStr.AppendLine(" AND SHUKANAME    = @SHUKANAME                                                                       ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@TORICODE", MySqlDbType.VarChar).Value = I_TORICODE
                    .Add("@ORGCODE", MySqlDbType.VarChar).Value = I_ORGCODE
                    .Add("@SHUKANAME", MySqlDbType.VarChar).Value = I_SHUKANAME
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If WW_Tbl.Rows.Count > 0 Then
                        O_CODE = WW_Tbl.Rows(0)("SHUKABASHO")
                    End If
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' 出荷場所名取得
    ''' </summary>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部署コード</param>
    ''' <param name="I_SHUKABASHO">出荷場所コード</param>
    ''' <param name="O_NAME">リターンコード</param>
    ''' <param name="O_RTN">リターンコード</param>
    Public Shared Sub getShukaName(ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_SHUKABASHO As String, ByRef O_NAME As String, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       SHUKANAME AS SHUKANAME                                                                        ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.VIW0006_TODOKE                                                                              ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     TORICODE     = @TORICODE                                                                        ")
        SQLStr.AppendLine(" AND ORDERORGCODE = @ORDERORGCODE                                                                    ")
        SQLStr.AppendLine(" AND SHUKABASHO   = @SHUKABASHO                                                                      ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@TORICODE", MySqlDbType.VarChar).Value = I_TORICODE
                    .Add("@ORDERORGCODE", MySqlDbType.VarChar).Value = I_ORGCODE
                    .Add("@SHUKABASHO", MySqlDbType.VarChar).Value = I_SHUKABASHO
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If WW_Tbl.Rows.Count > 0 Then
                        O_NAME = WW_Tbl.Rows(0)("SHUKANAME")
                    End If
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

    End Sub
    ''' <summary>
    ''' 届先コード取得
    ''' </summary>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部署コード</param>
    ''' <param name="I_TODOKENAME">届先名</param>
    ''' <param name="O_CODE">リターンコード</param>
    ''' <param name="O_RTN">リターンコード</param>
    Public Shared Sub getTodokeCode(ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TODOKENAME As String, ByRef O_CODE As String, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       TODOKECODE AS TODOKECODE                                                                      ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.VIW0006_TODOKE                                                                              ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     TORICODE     = @TORICODE                                                                        ")
        SQLStr.AppendLine(" AND ORDERORGCODE = @ORDERORGCODE                                                                    ")
        SQLStr.AppendLine(" AND TODOKENAME   = @TODOKENAME                                                                      ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@TORICODE", MySqlDbType.VarChar).Value = I_TORICODE
                    .Add("@ORDERORGCODE", MySqlDbType.VarChar).Value = I_ORGCODE
                    .Add("@TODOKENAME", MySqlDbType.VarChar).Value = I_TODOKENAME
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If WW_Tbl.Rows.Count > 0 Then
                        O_CODE = WW_Tbl.Rows(0)("TODOKECODE")
                    End If
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

    End Sub

    ''' <summary>
    ''' 届先名取得
    ''' </summary>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部署コード</param>
    ''' <param name="I_TODOKECODE">届先コード</param>
    ''' <param name="O_NAME">リターンコード</param>
    ''' <param name="O_RTN">リターンコード</param>
    Public Shared Sub getTodokeName(ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_TODOKECODE As String, ByRef O_NAME As String, ByRef O_RTN As String)
        O_RTN = C_MESSAGE_NO.NORMAL

        Dim CS0050Session As New CS0050SESSION
        Dim SQLStr As New StringBuilder

        SQLStr.AppendLine("SELECT DISTINCT                                                                                      ")
        SQLStr.AppendLine("       TODOKENAME AS TODOKENAME                                                                      ")
        SQLStr.AppendLine(" FROM                                                                                                ")
        SQLStr.AppendLine("     LNG.VIW0006_TODOKE                                                                              ")
        SQLStr.AppendLine(" WHERE                                                                                               ")
        SQLStr.AppendLine("     TORICODE     = @TORICODE                                                                        ")
        SQLStr.AppendLine(" AND ORDERORGCODE = @ORDERORGCODE                                                                    ")
        SQLStr.AppendLine(" AND TODOKECODE   = @TODOKECODE                                                                      ")

        Try
            Using sqlCon As New MySqlConnection(CS0050Session.DBCon),
              sqlCmd As New MySqlCommand(SQLStr.ToString, sqlCon)
                sqlCon.Open()
                MySqlConnection.ClearPool(sqlCon)
                With sqlCmd.Parameters
                    .Add("@TORICODE", MySqlDbType.VarChar).Value = I_TORICODE
                    .Add("@ORDERORGCODE", MySqlDbType.VarChar).Value = I_ORGCODE
                    .Add("@TODOKECODE", MySqlDbType.VarChar).Value = I_TODOKECODE
                End With
                Using sqlDr As MySqlDataReader = sqlCmd.ExecuteReader()
                    If sqlDr.HasRows = False Then
                        O_RTN = C_MESSAGE_NO.NO_DATA_EXISTS_ERROR
                    End If
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To sqlDr.FieldCount - 1
                        WW_Tbl.Columns.Add(sqlDr.GetName(index), sqlDr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(sqlDr)
                    If WW_Tbl.Rows.Count > 0 Then
                        O_NAME = WW_Tbl.Rows(0)("TODOKENAME")
                    End If
                End Using
            End Using
        Catch ex As Exception
            Throw ex '呼び出し元の例外にスロー
        End Try

    End Sub
    ''' <summary>
    ''' 請求枝番取得
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="O_SEQ">ID</param>
    Public Sub GetMaxSEIKYUBRANCH(ByVal SQLcon As MySqlConnection, ByVal I_ROW As DataRow, ByRef O_MESSAGENO As String, ByRef O_SEQ As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL
        O_SEQ = "00000001"

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                             ")
        SQLStr.AppendLine("    COALESCE(MAX(CAST(SEIKYUBRANCH AS UNSIGNED)),0) + 1 AS SEQ  ")
        SQLStr.AppendLine(" FROM LNG.LNT0030_SURCHARGEFEE                      ")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(SEIKYUYM, '')            = @SEIKYUYM ")
        SQLStr.AppendLine("    AND  COALESCE(SEIKYUDATEFROM, '')      = @SEIKYUDATEFROM ")
        SQLStr.AppendLine("    AND  COALESCE(SEIKYUDATETO, '')        = @SEIKYUDATETO ")
        SQLStr.AppendLine("    AND  COALESCE(TORICODE, '')            = @TORICODE ")
        SQLStr.AppendLine("    AND  COALESCE(ORGCODE, '')             = @ORGCODE ")
        SQLStr.AppendLine("    AND  COALESCE(PATTERNCODE, '')         = @PATTERNCODE ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOSHUKABASHO, '')   = @AVOCADOSHUKABASHO ")
        SQLStr.AppendLine("    AND  COALESCE(AVOCADOTODOKECODE, '')   = @AVOCADOTODOKECODE ")
        SQLStr.AppendLine("    AND  COALESCE(SHAGATA, '')             = @SHAGATA ")
        SQLStr.AppendLine("    AND  SHABARA                           = @SHABARA ")
        SQLStr.AppendLine("    AND  COALESCE(SHABAN, '')              = @SHABAN ")


        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_SEIKYUYM As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUYM", MySqlDbType.VarChar)                          '請求年月
                Dim P_SEIKYUDATEFROM As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUDATEFROM", MySqlDbType.Date)                 '請求対象期間From
                Dim P_SEIKYUDATETO As MySqlParameter = SQLcmd.Parameters.Add("@SEIKYUDATETO", MySqlDbType.Date)                     '請求対象期間To
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar)                          '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar)                            '部門コード
                Dim P_PATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@PATTERNCODE", MySqlDbType.VarChar)                    'パターンコード
                Dim P_AVOCADOSHUKABASHO As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOSHUKABASHO", MySqlDbType.VarChar)        '出荷場所コード
                Dim P_AVOCADOTODOKECODE As MySqlParameter = SQLcmd.Parameters.Add("@AVOCADOTODOKECODE", MySqlDbType.VarChar)        '届先コード
                Dim P_SHAGATA As MySqlParameter = SQLcmd.Parameters.Add("@SHAGATA", MySqlDbType.VarChar)                            '車型
                Dim P_SHABARA As MySqlParameter = SQLcmd.Parameters.Add("@SHABARA", MySqlDbType.Decimal)                            '車腹
                Dim P_SHABAN As MySqlParameter = SQLcmd.Parameters.Add("@SHABAN", MySqlDbType.VarChar)                              '車番

                P_SEIKYUYM.Value = I_ROW("SEIKYUYM")                           '請求年月
                P_SEIKYUDATEFROM.Value = I_ROW("SEIKYUDATEFROM")               '請求対象期間From
                P_SEIKYUDATETO.Value = I_ROW("SEIKYUDATETO")                   '請求対象期間To
                P_TORICODE.Value = I_ROW("TORICODE")                           '取引先コード
                P_ORGCODE.Value = I_ROW("ORGCODE")                             '部門コード
                P_PATTERNCODE.Value = I_ROW("PATTERNCODE")                     'パターンコード
                P_AVOCADOSHUKABASHO.Value = I_ROW("AVOCADOSHUKABASHO")         '出荷場所コード
                P_AVOCADOTODOKECODE.Value = I_ROW("AVOCADOTODOKECODE")         '届先コード
                P_SHAGATA.Value = I_ROW("SHAGATA")                             '車型
                P_SHABARA.Value = I_ROW("SHABARA")                             '車腹
                P_SHABAN.Value = I_ROW("SHABAN")                               '車番

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0020Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0020Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0020Chk.Load(SQLdr)
                    O_SEQ = CInt(LNM0020Chk.Rows(0)("SEQ")).ToString("00000000")
                End Using
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                   'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0030_SURCHARGEFEE select"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            O_MESSAGENO = Messages.C_MESSAGE_NO.DB_ERROR
            Exit Sub
        End Try

    End Sub

    ''' <summary>
    ''' 日付がシリアル値になっている場合正しい日付に変換する
    ''' </summary>
    ''' <param name="I_VALUE">対象文字列</param>
    ''' <remarks></remarks>
    Public Shared Function DateConvert(ByVal I_VALUE As Object) As String
        Dim dt As DateTime
        Dim i As Integer
        '日付に変換できる場合
        If DateTime.TryParse(I_VALUE, dt) Then
            DateConvert = dt
        Else
            '数値に変換できる場合
            If Integer.TryParse(I_VALUE, i) Then
                DateConvert = DateTime.FromOADate(i)
            Else
                DateConvert = ""
            End If
        End If
    End Function

    ''' <summary>
    ''' ロールマスタから一覧の取得
    ''' </summary>
    ''' <param name="I_COMPCODE"></param>
    ''' <param name="I_OBJCODE"></param>
    ''' <returns></returns>
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

    ''' <summary>
    ''' データ変換(データ型チェック)
    ''' </summary>
    ''' <param name="I_FIELDNAME"></param>
    ''' <param name="I_DATATYPE"></param>
    ''' <param name="I_VALUE"></param>
    ''' <param name="O_RESULT"></param>
    Public Shared Function DataConvert(ByVal I_FIELDNAME As String,
                               ByVal I_VALUE As String,
                               ByVal I_DATATYPE As String,
                               ByRef O_RESULT As Boolean,
                               ByRef O_MESSAGE1 As String,
                               ByRef O_MESSAGE2 As String) As Object
        O_RESULT = True
        Dim WW_VALUE As String
        Dim WWInt As Integer
        Dim WWDecimal As Decimal
        Dim WWdt As DateTime

        DataConvert = I_VALUE
        Select Case I_DATATYPE
            Case "String" '文字型は変換の必要がないので何もしない
            Case "Int32" '数値型(小数点含まない)
                '""の場合"0"をセット
                If I_VALUE = "" Then
                    DataConvert = "0"
                Else
                    '数値に変換できる場合
                    If Integer.TryParse(I_VALUE, WWInt) Then
                        DataConvert = WWInt
                        '数値に変換できない場合
                    Else
                        O_RESULT = False
                        DataConvert = "0"
                        O_MESSAGE1 = "・[" + I_FIELDNAME + "]のデータ変換に失敗しました。"
                        O_MESSAGE2 = "数値形式で入力してください。(小数点不可)"
                    End If
                End If
            Case "Decimal" '数値型(小数点含む)
                '""の場合"0"をセット
                If I_VALUE = "" Then
                    DataConvert = "0"
                Else
                    '数値に変換できる場合
                    If Decimal.TryParse(I_VALUE, WWDecimal) Then
                        DataConvert = WWDecimal
                        '数値に変換できない場合
                    Else
                        O_RESULT = False
                        DataConvert = "0"
                        O_MESSAGE1 = "・[" + I_FIELDNAME + "]のデータ変換に失敗しました。"
                        O_MESSAGE2 = "数値形式で入力してください。(小数点可)"
                    End If
                End If
            Case "DateTime" '日付型
                '""の場合最小値の日付をセット
                If I_VALUE = "" Then
                    DataConvert = Date.MinValue
                Else
                    'シリアル値の場合日付型に変換
                    WW_VALUE = DateConvert(I_VALUE)
                    '日付に変換できる場合
                    If DateTime.TryParse(WW_VALUE, WWdt) Then
                        DataConvert = WWdt
                        '日付に変換できない場合
                    Else
                        O_RESULT = False
                        DataConvert = Date.MinValue
                        O_MESSAGE1 = "・[" + I_FIELDNAME + "]のデータ変換に失敗しました。"
                        O_MESSAGE2 = "日付形式(yyyy/MM/dd)で入力してください。"
                    End If
                End If
        End Select
    End Function

    ''' <summary>
    ''' 操作権限のある組織コード取得
    ''' </summary>
    Public Sub GetPermitOrg(ByVal SQLcon As MySqlConnection,
                                   ByVal I_CAMPCODE As String,
                                   ByVal I_ROLEORG As String,
                                   ByRef O_ORGHT As Hashtable)

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" Select ")
        SQLStr.AppendLine("       CODE As CODE")
        SQLStr.AppendLine(" FROM")
        SQLStr.AppendLine("     COM.LNS0005_ROLE")
        SQLStr.AppendLine(" WHERE")
        SQLStr.AppendLine("        CAMPCODE  = @CAMPCODE                 ")
        SQLStr.AppendLine("   AND  OBJECT  = 'ORG'                       ")
        SQLStr.AppendLine("   AND  ROLE  = @ROLE                         ")
        SQLStr.AppendLine("   AND DATE_FORMAT(CURDATE(),'%Y/%m/%d') BETWEEN STYMD AND ENDYMD")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_CAMPCODE As MySqlParameter = SQLcmd.Parameters.Add("@CAMPCODE", MySqlDbType.VarChar, 20) '会社コード
                Dim P_ROLE As MySqlParameter = SQLcmd.Parameters.Add("@ROLE", MySqlDbType.VarChar, 20) 'ロール

                P_CAMPCODE.Value = I_CAMPCODE '会社コード
                P_ROLE.Value = I_ROLEORG 'ロール

                Dim WW_Tbl = New DataTable
                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next
                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)
                End Using
                'ハッシュテーブルにコードと名称を格納
                For Each WW_Row As DataRow In WW_Tbl.Rows
                    '組織コード格納
                    If Not O_ORGHT.ContainsKey(WW_Row("CODE")) Then
                        O_ORGHT.Add(WW_Row("CODE"), WW_Row("CODE"))
                    End If
                Next
            End Using
        Catch ex As Exception
        End Try

    End Sub

    ''' <summary>
    ''' 会社コード取得のパラメータ設定
    ''' </summary>
    ''' <param name="COMPANY_FLG"></param>
    ''' <param name="I_COMPCODE"></param>
    ''' <returns></returns>
    Public Function CreateCOMPANYParam(ByVal COMPANY_FLG As Integer, ByVal I_COMPCODE As String) As Hashtable

        Dim prmData As New Hashtable
        prmData.Item(C_PARAMETERS.LP_COMPANY) = I_COMPCODE
        prmData.Item(C_PARAMETERS.LP_TYPEMODE) = COMPANY_FLG

        CreateCOMPANYParam = prmData

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
    ''' 排他チェック
    ''' </summary>
    ''' <param name="SQLcon"></param>
    ''' <param name="O_MESSAGENO">メッセージ</param>
    ''' <param name="I_TIMESTAMP">タイムスタンプ</param>
    ''' <param name="I_TORICODE">取引先コード</param>
    ''' <param name="I_ORGCODE">部門コード</param>
    ''' <param name="I_SURCHARGEPATTERNCODE">サーチャージパターンコード</param>
    ''' <param name="I_BILLINGCYCLE">請求サイクル</param>
    ''' <param name="I_STYMD">有効開始日</param>
    Public Sub HaitaCheck(ByVal SQLcon As MySqlConnection, ByRef O_MESSAGENO As String, ByVal I_TIMESTAMP As String,
                          ByVal I_TORICODE As String, ByVal I_ORGCODE As String, ByVal I_SURCHARGEPATTERNCODE As String,
                          ByVal I_BILLINGCYCLE As String, ByVal I_STYMD As String)

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力
        O_MESSAGENO = Messages.C_MESSAGE_NO.NORMAL

        '○ 対象データ取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine(" SELECT                                                ")
        SQLStr.AppendLine("    UPDTIMSTP                                          ")
        SQLStr.AppendLine(" FROM                                                  ")
        SQLStr.AppendLine("     LNG.LNM0019_SURCHARGEPATTERN                      ")
        SQLStr.AppendLine(" WHERE                                                 ")
        SQLStr.AppendLine("       TORICODE  = @TORICODE                           ")
        SQLStr.AppendLine("   AND ORGCODE  = @ORGCODE                             ")
        SQLStr.AppendLine("   AND SURCHARGEPATTERNCODE  = @SURCHARGEPATTERNCODE   ")
        SQLStr.AppendLine("   AND BILLINGCYCLE  = @BILLINGCYCLE                   ")
        SQLStr.AppendLine("   AND STYMD  = @STYMD                                 ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_TORICODE As MySqlParameter = SQLcmd.Parameters.Add("@TORICODE", MySqlDbType.VarChar, 10) '取引先コード
                Dim P_ORGCODE As MySqlParameter = SQLcmd.Parameters.Add("@ORGCODE", MySqlDbType.VarChar, 6) '部門コード
                Dim P_SURCHARGEPATTERNCODE As MySqlParameter = SQLcmd.Parameters.Add("@SURCHARGEPATTERNCODE", MySqlDbType.VarChar, 2)   'サーチャージパターンコード
                Dim P_BILLINGCYCLE As MySqlParameter = SQLcmd.Parameters.Add("@BILLINGCYCLE", MySqlDbType.VarChar, 1)                   '請求サイクル
                Dim P_STYMD As MySqlParameter = SQLcmd.Parameters.Add("@STYMD", MySqlDbType.Date)     '有効開始日

                P_TORICODE.Value = I_TORICODE                           '取引先コード
                P_ORGCODE.Value = I_ORGCODE                             '部門コード
                P_SURCHARGEPATTERNCODE.Value = I_SURCHARGEPATTERNCODE   'サーチャージパターンコード
                P_BILLINGCYCLE.Value = I_BILLINGCYCLE                   '請求サイクル
                P_STYMD.Value = I_STYMD                                 '有効開始日

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()

                    Dim LNM0019Chk = New DataTable

                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        LNM0019Chk.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    LNM0019Chk.Load(SQLdr)

                    If LNM0019Chk.Rows.Count > 0 Then
                        Dim LNM0019row As DataRow
                        LNM0019row = LNM0019Chk.Rows(0)
                        If Not String.IsNullOrEmpty(LNM0019row("UPDTIMSTP").ToString) Then          'タイムスタンプ
                            If LNM0019row("UPDTIMSTP").ToString <> I_TIMESTAMP Then
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
            CS0011LOGWrite.INFPOSI = "DB:LNM0019_SURCHARGEPATTERN UPDATE_INSERT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                       'ログ出力
            Exit Sub
        End Try

    End Sub

    '' <summary>
    '' サーチャージ定義マスタ存在チェック
    '' </summary>
    Public Function AddDataChk(ByVal SQLcon As MySqlConnection, ByVal WW_ROW As DataRow) As Boolean
        AddDataChk = True

        Dim CS0011LOGWrite As New CS0011LOGWrite                    'ログ出力

        '更新前の削除フラグを取得
        Dim SQLStr = New StringBuilder
        SQLStr.AppendLine("    SELECT")
        SQLStr.AppendLine("        DIESELPRICESITEID")
        SQLStr.AppendLine("    FROM")
        SQLStr.AppendLine("        LNG.LNT0030_DISELPRICEHIST")
        SQLStr.AppendLine("    WHERE")
        SQLStr.AppendLine("         COALESCE(DIESELPRICESITEID, '')      = @DIESELPRICESITEID ")
        SQLStr.AppendLine("    AND  COALESCE(DIESELPRICESITEBRANCH, '')  = @DIESELPRICESITEBRANCH ")
        SQLStr.AppendLine("    AND  COALESCE(TARGETYEAR, '')             = @TARGETYEAR ")

        Try
            Using SQLcmd As New MySqlCommand(SQLStr.ToString, SQLcon)
                Dim P_DIESELPRICESITEID As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEID", MySqlDbType.VarChar, 10)                '実勢軽油価格参照先ID
                Dim P_DIESELPRICESITEBRANCH As MySqlParameter = SQLcmd.Parameters.Add("@DIESELPRICESITEBRANCH", MySqlDbType.VarChar, 10)        '実勢軽油価格参照先ID枝番
                Dim P_TARGETYEAR As MySqlParameter = SQLcmd.Parameters.Add("@TARGETYEAR", MySqlDbType.VarChar, 4)                               '対象年

                P_DIESELPRICESITEID.Value = WW_ROW("DIESELPRICESITEID")             '実勢軽油価格参照先ID
                P_DIESELPRICESITEBRANCH.Value = WW_ROW("DIESELPRICESITEBRANCH")     '実勢軽油価格参照先ID枝番
                P_TARGETYEAR.Value = WW_ROW("TARGETYEAR")                           '対象年

                Using SQLdr As MySqlDataReader = SQLcmd.ExecuteReader()
                    Dim WW_Tbl = New DataTable
                    '○ フィールド名とフィールドの型を取得
                    For index As Integer = 0 To SQLdr.FieldCount - 1
                        WW_Tbl.Columns.Add(SQLdr.GetName(index), SQLdr.GetFieldType(index))
                    Next

                    '○ テーブル検索結果をテーブル格納
                    WW_Tbl.Load(SQLdr)

                    'データが存在した場合
                    If WW_Tbl.Rows.Count > 0 Then
                        AddDataChk = False
                        Exit Function
                    End If
                End Using
            End Using
        Catch ex As Exception

            CS0011LOGWrite.INFSUBCLASS = "MAIN"                         'SUBクラス名
            CS0011LOGWrite.INFPOSI = "DB:LNT0030_DISELPRICEHIST SELECT"
            CS0011LOGWrite.NIWEA = C_MESSAGE_TYPE.ABORT
            CS0011LOGWrite.TEXT = ex.ToString()
            CS0011LOGWrite.MESSAGENO = C_MESSAGE_NO.DB_ERROR
            CS0011LOGWrite.CS0011LOGWrite()                             'ログ出力

            Exit Function
        End Try
    End Function

End Class